using System.Security.Cryptography;
using System.Text;
using Jazor.Emit;
using Jazor.Vue;

namespace Jazor.VueHost.DevServer;

internal sealed class OnDemandCompiler
{
    private readonly JazorVueParser _parser;
    private readonly JazorVueCompiler _compiler;
    private readonly IFrontendModuleCompiler _frontendCompiler;
    private readonly CompilationCache _cache;
    private readonly DependencyGraph? _dependencyGraph;
    private readonly ModuleResolver? _moduleResolver;

    public DependencyGraph? DependencyGraph => _dependencyGraph;

    public OnDemandCompiler(
        JazorVueParser parser,
        JazorVueCompiler compiler,
        IFrontendModuleCompiler? frontendCompiler,
        CompilationCache cache,
        DependencyGraph? dependencyGraph = null,
        ModuleResolver? moduleResolver = null)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _frontendCompiler = frontendCompiler ?? new NullFrontendModuleCompiler();
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _dependencyGraph = dependencyGraph;
        _moduleResolver = moduleResolver;
    }

    public async ValueTask<CompilationResult> CompileAsync(
        string absolutePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        cancellationToken.ThrowIfCancellationRequested();

        var text = await File.ReadAllTextAsync(absolutePath, cancellationToken);
        var contentHash = ComputeContentHash(text);
        if (_cache.TryGet(absolutePath, contentHash, out var cached))
        {
            return cached;
        }

        var result = await CompileCoreAsync(absolutePath, text, cancellationToken);
        _dependencyGraph?.Record(absolutePath, result.Dependencies);
        _cache.Set(absolutePath, contentHash, result);
        return result;
    }

    public bool TryGetCachedResult(string absolutePath, out CompilationResult? result)
        => _cache.TryPeek(absolutePath, out result);

    public async ValueTask<CompilationResult> RecompileAsync(
        string absolutePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        cancellationToken.ThrowIfCancellationRequested();

        var text = await File.ReadAllTextAsync(absolutePath, cancellationToken);
        var contentHash = ComputeContentHash(text);
        var result = await CompileCoreAsync(absolutePath, text, cancellationToken);
        _dependencyGraph?.Record(absolutePath, result.Dependencies);
        _cache.Set(absolutePath, contentHash, result);
        return result;
    }

    public void Invalidate(string absolutePath)
    {
        _cache.Invalidate(absolutePath);
        _dependencyGraph?.Remove(absolutePath);
    }

    public void InvalidateAll()
    {
        _cache.InvalidateAll();
        _dependencyGraph?.Clear();
    }

    private async ValueTask<CompilationResult> CompileCoreAsync(
        string absolutePath,
        string text,
        CancellationToken cancellationToken)
    {
        return Path.GetExtension(absolutePath).ToLowerInvariant() switch
        {
            ".jazor" => await CompileJazorAsync(absolutePath, text, cancellationToken),
            ".vue" => await CompileVueAsync(absolutePath, text, cancellationToken),
            ".ts" => await CompileTypeScriptAsync(absolutePath, text, cancellationToken),
            ".js" => CreatePassThrough("text/javascript", text),
            ".css" => CreateStylePassThrough(text),
            ".html" => CreatePassThrough("text/html", text),
            _ => new CompilationResult
            {
                ContentType = "text/plain",
                Content = string.Empty,
                IsError = true,
                ErrorMessage = $"Unsupported document '{absolutePath}'.",
                Diagnostics = [$"Unsupported document '{absolutePath}'."]
            }
        };
    }

    private async ValueTask<CompilationResult> CompileJazorAsync(
        string absolutePath,
        string text,
        CancellationToken cancellationToken)
    {
        var document = _parser.Parse(absolutePath, text);
        var sfc = _compiler.Compile(document);
        var module = await _frontendCompiler.CompileSfcAsync(absolutePath, sfc.GeneratedVueText, cancellationToken);
        if (module is null)
        {
            return CreateFrontendUnavailableResult(
                "Vue SFC compilation is not available because the frontend compiler is unavailable.",
                sfc.Diagnostics);
        }

        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = CreateServedModule(absolutePath, module.JavaScript, module.StyleContent),
            ModuleSignature = ComputeContentHash(module.JavaScript),
            HotReloadManifestEntry = CreateJazorHotReloadManifestEntry(absolutePath, document, sfc, module),
            SourceMap = module.SourceMap,
            StyleContent = module.StyleContent,
            Dependencies = module.Dependencies,
            Diagnostics = sfc.Diagnostics,
            IsError = false,
            SupportsHmr = module.SupportsHmr
        };
    }

    private async ValueTask<CompilationResult> CompileVueAsync(
        string absolutePath,
        string text,
        CancellationToken cancellationToken)
    {
        var module = await _frontendCompiler.CompileSfcAsync(absolutePath, text, cancellationToken);
        return module is null
            ? CreateFrontendUnavailableResult("Vue SFC compilation is not available because the frontend compiler is unavailable.")
            : new CompilationResult
            {
                ContentType = "text/javascript",
                Content = CreateServedModule(absolutePath, module.JavaScript, module.StyleContent),
                ModuleSignature = ComputeContentHash(module.JavaScript),
                SourceMap = module.SourceMap,
                StyleContent = module.StyleContent,
                Dependencies = module.Dependencies,
                SupportsHmr = module.SupportsHmr
            };
    }

    private async ValueTask<CompilationResult> CompileTypeScriptAsync(
        string absolutePath,
        string text,
        CancellationToken cancellationToken)
    {
        var module = await _frontendCompiler.CompileTypeScriptAsync(absolutePath, text, cancellationToken);
        return module is null
            ? CreateFrontendUnavailableResult("TypeScript transpilation is not available because the frontend compiler is unavailable.")
            : new CompilationResult
            {
                ContentType = "text/javascript",
                Content = module.JavaScript,
                ModuleSignature = ComputeContentHash(module.JavaScript),
                SourceMap = module.SourceMap,
                Dependencies = module.Dependencies
            };
    }

    private static CompilationResult CreatePassThrough(string contentType, string content)
        => new()
        {
            ContentType = contentType,
            Content = content
        };

    private static CompilationResult CreateStylePassThrough(string content)
        => new()
        {
            ContentType = "text/css",
            Content = content,
            StyleContent = content
        };

    private static CompilationResult CreateFrontendUnavailableResult(
        string message,
        IReadOnlyList<string>? diagnostics = null)
        => new()
        {
            ContentType = "text/javascript",
            Content = $$"""
                throw new Error({{System.Text.Json.JsonSerializer.Serialize(message)}});
                """,
            Diagnostics = diagnostics ?? [message],
            IsError = true,
            ErrorMessage = message
        };

    private string CreateServedModule(
        string documentPath,
        string javaScript,
        string? styleContent)
    {
        if (string.IsNullOrWhiteSpace(styleContent))
        {
            return javaScript;
        }

        var styleTargetId = GetStyleTargetId(documentPath);

        return $$"""
            const __jazorStyleId = {{System.Text.Json.JsonSerializer.Serialize(styleTargetId)}};
            const __jazorStyle = {{System.Text.Json.JsonSerializer.Serialize(styleContent)}};
            if (typeof document !== "undefined" && __jazorStyle) {
              let style = document.querySelector(`style[data-jazor-vuehost="${__jazorStyleId}"]`);
              if (!style) {
                style = document.createElement("style");
                style.setAttribute("data-jazor-vuehost", __jazorStyleId);
                document.head.appendChild(style);
              }
              style.textContent = __jazorStyle;
            }
            {{javaScript}}
            """;
    }

    private string GetStyleTargetId(string documentPath)
    {
        if (_moduleResolver is null)
        {
            return documentPath;
        }

        try
        {
            return _moduleResolver.GetStyleTargetIdForAbsolutePath(documentPath);
        }
        catch (InvalidOperationException)
        {
            return documentPath;
        }
    }

    private static string ComputeContentHash(string text)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash);
    }

    private RazorVueManifestEntry? CreateJazorHotReloadManifestEntry(
        string documentPath,
        JazorVueDocument document,
        JazorVueCompilationResult compilation,
        FrontendModuleCompilation module)
    {
        if (compilation.HotReload is null)
        {
            return null;
        }

        var resolvedModulePath = GetManifestModulePath(documentPath);
        var imports = document.Imports
            .Select(static import => import.Source)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static import => import, StringComparer.Ordinal)
            .ToList();

        return new RazorVueManifestEntry(
            AssemblyName: "Jazor.VueHost",
            ComponentId: resolvedModulePath,
            ModuleId: resolvedModulePath,
            ComponentName: Path.GetFileNameWithoutExtension(documentPath),
            RelativeModulePath: resolvedModulePath,
            SourceMapPath: resolvedModulePath + ".map",
            OriginMapPath: resolvedModulePath + ".origins.json",
            Imports: imports,
            Styles: [],
            PluginRequirements: [],
            DescriptorHash: compilation.HotReload.DescriptorSignature,
            TemplateHash: compilation.HotReload.TemplateSignature,
            LogicHash: compilation.HotReload.LogicSignature,
            ContentHash: ComputeContentHash(module.JavaScript),
            HmrBoundaryKind: compilation.HotReload.HmrBoundaryKind,
            RequiresHydration: false,
            SupportsSsr: true);
    }

    private string GetManifestModulePath(string documentPath)
    {
        if (_moduleResolver is null)
        {
            return Path.GetFileName(documentPath);
        }

        try
        {
            return _moduleResolver.GetResolvedUrlForAbsolutePath(documentPath);
        }
        catch (InvalidOperationException)
        {
            return Path.GetFileName(documentPath);
        }
    }
}
