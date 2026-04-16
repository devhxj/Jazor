using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Jazor.Emit;
using Jazor.Emit.SourceMaps;
using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Roslyn.InProc;
using Jazor.VueHost.SourceMap;

namespace Jazor.VueHost.DevServer;

internal sealed class OnDemandCompiler
{
    private static readonly Regex TrailingSourceMapCommentPattern = new(
        @"(?:\r?\n)?//# sourceMappingURL=.*\s*$",
        RegexOptions.Compiled);

    private readonly JazorVueParser _parser;
    private readonly JazorVueCompiler _compiler;
    private readonly IFrontendModuleCompiler _frontendCompiler;
    private readonly CompilationCache _cache;
    private readonly DependencyGraph? _dependencyGraph;
    private readonly ModuleResolver? _moduleResolver;
    private readonly JazorHotReloadMetadataProvider _hotReloadMetadataProvider;
    private readonly ISourceMapService? _sourceMapService;

    public DependencyGraph? DependencyGraph => _dependencyGraph;

    public OnDemandCompiler(
        JazorVueParser parser,
        JazorVueCompiler compiler,
        IFrontendModuleCompiler? frontendCompiler,
        CompilationCache cache,
        DependencyGraph? dependencyGraph = null,
        ModuleResolver? moduleResolver = null,
        JazorHotReloadMetadataProvider? hotReloadMetadataProvider = null,
        ISourceMapService? sourceMapService = null)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _frontendCompiler = frontendCompiler ?? new NullFrontendModuleCompiler();
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _dependencyGraph = dependencyGraph;
        _moduleResolver = moduleResolver;
        _hotReloadMetadataProvider = hotReloadMetadataProvider ?? new JazorHotReloadMetadataProvider();
        _sourceMapService = sourceMapService;
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

        var result = await CompileCoreAsync(absolutePath, text, companionDocuments: null, cancellationToken);
        SynchronizeSourceMapRegistration(absolutePath, result);
        _dependencyGraph?.Record(absolutePath, result.Dependencies);
        _cache.Set(absolutePath, contentHash, result);
        return result;
    }

    public async ValueTask<CompilationResult> CompileAsync(
        string absolutePath,
        string text,
        CancellationToken cancellationToken)
        => await CompileAsync(absolutePath, text, companionDocuments: null, cancellationToken);

    public async ValueTask<CompilationResult> CompileAsync(
        string absolutePath,
        string text,
        IReadOnlyList<DocumentSnapshot>? companionDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(text);
        cancellationToken.ThrowIfCancellationRequested();

        var contentHash = ComputeContentHash(text);
        if (_cache.TryGet(absolutePath, contentHash, out var cached))
        {
            return cached;
        }

        var result = await CompileCoreAsync(absolutePath, text, companionDocuments, cancellationToken);
        SynchronizeSourceMapRegistration(absolutePath, result);
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
        return await RecompileAsync(absolutePath, text, companionDocuments: null, cancellationToken);
    }

    public async ValueTask<CompilationResult> RecompileAsync(
        string absolutePath,
        IReadOnlyList<DocumentSnapshot>? companionDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        cancellationToken.ThrowIfCancellationRequested();

        var text = await File.ReadAllTextAsync(absolutePath, cancellationToken);
        return await RecompileAsync(absolutePath, text, companionDocuments, cancellationToken);
    }

    public async ValueTask<CompilationResult> RecompileAsync(
        string absolutePath,
        string text,
        CancellationToken cancellationToken)
        => await RecompileAsync(absolutePath, text, companionDocuments: null, cancellationToken);

    public async ValueTask<CompilationResult> RecompileAsync(
        string absolutePath,
        string text,
        IReadOnlyList<DocumentSnapshot>? companionDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(text);
        cancellationToken.ThrowIfCancellationRequested();

        var contentHash = ComputeContentHash(text);
        var result = await CompileCoreAsync(absolutePath, text, companionDocuments, cancellationToken);
        SynchronizeSourceMapRegistration(absolutePath, result);
        _dependencyGraph?.Record(absolutePath, result.Dependencies);
        _cache.Set(absolutePath, contentHash, result);
        return result;
    }

    public void Invalidate(string absolutePath)
    {
        _cache.Invalidate(absolutePath);
        _dependencyGraph?.Remove(absolutePath);
        UnregisterSourceMap(absolutePath);
    }

    public void InvalidateAll()
    {
        foreach (var absolutePath in _cache.GetPaths())
        {
            UnregisterSourceMap(absolutePath);
        }

        _cache.InvalidateAll();
        _dependencyGraph?.Clear();
    }

    private async ValueTask<CompilationResult> CompileCoreAsync(
        string absolutePath,
        string text,
        IReadOnlyList<DocumentSnapshot>? companionDocuments,
        CancellationToken cancellationToken)
    {
        return Path.GetExtension(absolutePath).ToLowerInvariant() switch
        {
            ".jazor" => await CompileJazorAsync(absolutePath, text, companionDocuments, cancellationToken),
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

    private void SynchronizeSourceMapRegistration(string absolutePath, CompilationResult result)
    {
        if (_sourceMapService is null)
        {
            return;
        }

        foreach (var key in GetSourceMapKeys(absolutePath))
        {
            if (string.IsNullOrWhiteSpace(result.SourceMap))
            {
                _sourceMapService.Unregister(key);
            }
            else
            {
                _sourceMapService.Register(key, result.SourceMap);
            }
        }
    }

    private void UnregisterSourceMap(string absolutePath)
    {
        if (_sourceMapService is null)
        {
            return;
        }

        foreach (var key in GetSourceMapKeys(absolutePath))
        {
            _sourceMapService.Unregister(key);
        }
    }

    private IReadOnlyList<string> GetSourceMapKeys(string absolutePath)
    {
        if (_moduleResolver is not null)
        {
            try
            {
                return [_moduleResolver.GetResolvedUrlForAbsolutePath(absolutePath)];
            }
            catch (InvalidOperationException)
            {
            }
        }

        return [absolutePath];
    }

    private async ValueTask<CompilationResult> CompileJazorAsync(
        string absolutePath,
        string text,
        IReadOnlyList<DocumentSnapshot>? companionDocuments,
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

        var hotReloadManifestEntry = CreateJazorHotReloadManifestEntry(absolutePath, document, compilation: sfc, module, companionDocuments);
        var moduleSignature = ComputeJazorModuleSignature(module.JavaScript, hotReloadManifestEntry);
        var servedModule = CreateServedModule(absolutePath, module.JavaScript, module.StyleContent);
        var chainedSourceMap = ChainJazorSourceMap(module.SourceMap, sfc);
        var servedSourceMap = OffsetSourceMapGeneratedLines(chainedSourceMap, servedModule.JavaScriptLineOffset);

        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = AttachInlineSourceMap(servedModule.Content, servedSourceMap),
            ModuleSignature = moduleSignature,
            HotReloadManifestEntry = hotReloadManifestEntry,
            SourceMap = servedSourceMap,
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
        if (module is null)
        {
            return CreateFrontendUnavailableResult("Vue SFC compilation is not available because the frontend compiler is unavailable.");
        }

        var servedModule = CreateServedModule(absolutePath, module.JavaScript, module.StyleContent);
        var servedSourceMap = OffsetSourceMapGeneratedLines(module.SourceMap, servedModule.JavaScriptLineOffset);
        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = AttachInlineSourceMap(servedModule.Content, servedSourceMap),
            ModuleSignature = ComputeContentHash(module.JavaScript),
            SourceMap = servedSourceMap,
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
                Content = AttachInlineSourceMap(module.JavaScript, module.SourceMap),
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

    private ServedModuleContent CreateServedModule(
        string documentPath,
        string javaScript,
        string? styleContent)
    {
        if (string.IsNullOrWhiteSpace(styleContent))
        {
            return new ServedModuleContent(javaScript, JavaScriptLineOffset: 0);
        }

        var styleTargetId = GetStyleTargetId(documentPath);
        var prefix = $$"""
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
            """;

        return new ServedModuleContent(
            string.Concat(prefix, "\n", javaScript),
            CountLines(prefix));
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

    private static string? OffsetSourceMapGeneratedLines(string? sourceMap, int generatedLineOffset)
    {
        if (string.IsNullOrWhiteSpace(sourceMap) || generatedLineOffset <= 0)
        {
            return sourceMap;
        }

        try
        {
            if (JsonNode.Parse(sourceMap) is not JsonObject sourceMapObject)
            {
                return sourceMap;
            }

            var mappings = sourceMapObject["mappings"]?.GetValue<string>() ?? string.Empty;
            sourceMapObject["mappings"] = string.Concat(new string(';', generatedLineOffset), mappings);
            return sourceMapObject.ToJsonString();
        }
        catch
        {
            return sourceMap;
        }
    }

    private static string? ChainJazorSourceMap(
        string? javaScriptSourceMap,
        JazorVueCompilationResult compilation)
    {
        if (string.IsNullOrWhiteSpace(javaScriptSourceMap))
        {
            return javaScriptSourceMap;
        }

        try
        {
            var generatedVueFileName = Path.GetFileName(compilation.Document.FilePath);
            var generatedVueSourceMap = compilation.GeneratedVueSourceMap;
            if (string.IsNullOrWhiteSpace(generatedVueSourceMap))
            {
                return javaScriptSourceMap;
            }

            var chainedMap = new SourceMapChainBuilder().Chain(
                javaScriptSourceMap,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [generatedVueFileName] = generatedVueSourceMap
                });
            return new SourceMapWriter().Write(chainedMap);
        }
        catch
        {
            return javaScriptSourceMap;
        }
    }

    private static string AttachInlineSourceMap(string content, string? sourceMap)
    {
        if (string.IsNullOrWhiteSpace(sourceMap))
        {
            return content;
        }

        var normalizedContent = TrailingSourceMapCommentPattern.Replace(content, string.Empty).TrimEnd();
        var dataUri = "data:application/json;base64," + Convert.ToBase64String(Encoding.UTF8.GetBytes(sourceMap));
        return string.Concat(normalizedContent, "\n//# sourceMappingURL=", dataUri);
    }

    private static int CountLines(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0;
        }

        var count = 1;
        foreach (var character in text)
        {
            if (character == '\n')
            {
                count++;
            }
        }

        return count;
    }

    private static string ComputeJazorModuleSignature(
        string javaScript,
        RazorVueManifestEntry? hotReloadManifestEntry)
    {
        if (hotReloadManifestEntry is null)
        {
            return ComputeContentHash(javaScript);
        }

        return ComputeContentHash(string.Create(
            javaScript.Length
            + hotReloadManifestEntry.DescriptorHash.Length
            + hotReloadManifestEntry.TemplateHash.Length
            + hotReloadManifestEntry.LogicHash.Length
            + 3,
            (javaScript, hotReloadManifestEntry),
            static (buffer, state) =>
            {
                var offset = 0;
                state.javaScript.AsSpan().CopyTo(buffer[offset..]);
                offset += state.javaScript.Length;
                buffer[offset++] = '\n';
                state.hotReloadManifestEntry.DescriptorHash.AsSpan().CopyTo(buffer[offset..]);
                offset += state.hotReloadManifestEntry.DescriptorHash.Length;
                buffer[offset++] = '\n';
                state.hotReloadManifestEntry.TemplateHash.AsSpan().CopyTo(buffer[offset..]);
                offset += state.hotReloadManifestEntry.TemplateHash.Length;
                buffer[offset++] = '\n';
                state.hotReloadManifestEntry.LogicHash.AsSpan().CopyTo(buffer[offset..]);
            }));
    }

    private RazorVueManifestEntry? CreateJazorHotReloadManifestEntry(
        string documentPath,
        JazorVueDocument document,
        JazorVueCompilationResult compilation,
        FrontendModuleCompilation module,
        IReadOnlyList<DocumentSnapshot>? companionDocuments)
    {
        var hotReloadMetadata = _hotReloadMetadataProvider.CreateMetadata(document, compilation.Diagnostics, companionDocuments);

        var resolvedModulePath = GetManifestModulePath(documentPath);
        var imports = document.Imports
            .Select(static import => import.Source)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static import => import, StringComparer.Ordinal)
            .ToList();
        var contentHash = ComputeJazorModuleSignature(module.JavaScript, new RazorVueManifestEntry(
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
            DescriptorHash: hotReloadMetadata.DescriptorSignature,
            TemplateHash: hotReloadMetadata.TemplateSignature,
            LogicHash: hotReloadMetadata.LogicSignature,
            ContentHash: string.Empty,
            HmrBoundaryKind: hotReloadMetadata.HmrBoundaryKind,
            RequiresHydration: false,
            SupportsSsr: true));

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
            DescriptorHash: hotReloadMetadata.DescriptorSignature,
            TemplateHash: hotReloadMetadata.TemplateSignature,
            LogicHash: hotReloadMetadata.LogicSignature,
            ContentHash: contentHash,
            HmrBoundaryKind: hotReloadMetadata.HmrBoundaryKind,
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

    private readonly record struct ServedModuleContent(
        string Content,
        int JavaScriptLineOffset);
}
