using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Jolt.Roslyn.InProc;
using Jazor.Common.SourceMaps;
using Jolt.SourceMap;
using Jazor.Vue;
using Jazor.RazorVue.Protocol;

namespace Jolt.DevServer;

internal sealed class OnDemandCompiler
{
    private static readonly Regex TrailingSourceMapCommentPattern = new(
        @"(?:\r?\n)?//# sourceMappingURL=.*\s*$",
        RegexOptions.Compiled);
    private static readonly Regex StaticCssImportPattern = new(
        @"^[ \t]*import\s*(?<quote>[""'])(?<specifier>[^""']+)\k<quote>\s*;?[ \t\r]*$",
        RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex StaticCssModuleDefaultImportPattern = new(
        @"^[ \t]*import\s+(?<binding>[$_\p{L}][$_\p{L}\p{Nd}]*)\s+from\s*(?<quote>[""'])(?<specifier>[^""']+)\k<quote>\s*;?[ \t\r]*$",
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);
    private static readonly Regex StaticCssModuleNamespaceImportPattern = new(
        @"^[ \t]*import\s+\*\s+as\s+(?<binding>[$_\p{L}][$_\p{L}\p{Nd}]*)\s+from\s*(?<quote>[""'])(?<specifier>[^""']+)\k<quote>\s*;?[ \t\r]*$",
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);
    private static readonly Regex StaticCssModuleNamedDefaultImportPattern = new(
        @"^[ \t]*import\s*\{\s*default\s+as\s+(?<binding>[$_\p{L}][$_\p{L}\p{Nd}]*)\s*\}\s+from\s*(?<quote>[""'])(?<specifier>[^""']+)\k<quote>\s*;?[ \t\r]*$",
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);

    private readonly JazorVueParser _parser;
    private readonly JazorVueCompiler _compiler;
    private readonly IVolarModuleCompiler _frontendCompiler;
    private readonly CompilationCache _cache;
    private readonly DependencyGraph? _dependencyGraph;
    private readonly ModuleResolver? _moduleResolver;
    private readonly JazorHotReloadMetadataProvider _hotReloadMetadataProvider;
    private readonly ISourceMapService? _sourceMapService;
    private readonly bool _buildMode;
    private readonly Lock _stateGate = new();

    public DependencyGraph? DependencyGraph => _dependencyGraph;

    public OnDemandCompiler(
        JazorVueParser parser,
        JazorVueCompiler compiler,
        IVolarModuleCompiler? frontendCompiler,
        CompilationCache cache,
        DependencyGraph? dependencyGraph = null,
        ModuleResolver? moduleResolver = null,
        JazorHotReloadMetadataProvider? hotReloadMetadataProvider = null,
        ISourceMapService? sourceMapService = null,
        bool buildMode = false)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _frontendCompiler = frontendCompiler ?? new NullVolarModuleCompiler();
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _dependencyGraph = dependencyGraph;
        _moduleResolver = moduleResolver;
        _hotReloadMetadataProvider = hotReloadMetadataProvider ?? new JazorHotReloadMetadataProvider();
        _sourceMapService = sourceMapService;
        _buildMode = buildMode;
    }

    public IReadOnlyList<KeyValuePair<string, CompilationResult>> GetCachedResults()
    {
        lock (_stateGate)
        {
            return _cache.GetEntries();
        }
    }

    public async ValueTask<CompilationResult> CompileAsync(
        string absolutePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        cancellationToken.ThrowIfCancellationRequested();

        var text = await File.ReadAllTextAsync(absolutePath, cancellationToken);
        var contentHash = ComputeCacheHash(text, companionDocuments: null);
        if (TryGetCachedResultCore(absolutePath, contentHash, out var cached))
        {
            return cached;
        }

        var result = await CompileCoreAsync(absolutePath, text, companionDocuments: null, cancellationToken);
        PublishCompilationResult(absolutePath, contentHash, result);
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

        var contentHash = ComputeCacheHash(text, companionDocuments);
        if (TryGetCachedResultCore(absolutePath, contentHash, out var cached))
        {
            return cached;
        }

        var result = await CompileCoreAsync(absolutePath, text, companionDocuments, cancellationToken);
        PublishCompilationResult(absolutePath, contentHash, result);
        return result;
    }

    public bool TryGetCachedResult(string absolutePath, out CompilationResult? result)
    {
        lock (_stateGate)
        {
            return _cache.TryPeek(absolutePath, out result);
        }
    }

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

        var contentHash = ComputeCacheHash(text, companionDocuments);
        var result = await CompileCoreAsync(absolutePath, text, companionDocuments, cancellationToken);
        PublishCompilationResult(absolutePath, contentHash, result);
        return result;
    }

    public void Invalidate(string absolutePath)
    {
        lock (_stateGate)
        {
            if (_cache.Invalidate(absolutePath))
            {
                _dependencyGraph?.Remove(absolutePath);
                UnregisterSourceMap(absolutePath);
            }
        }
    }

    public void InvalidateAll()
    {
        lock (_stateGate)
        {
            foreach (var absolutePath in _cache.InvalidateAll())
            {
                UnregisterSourceMap(absolutePath);
            }

            _dependencyGraph?.Clear();
        }
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
            ".js" => await CompileJavaScriptAsync(absolutePath, text, cancellationToken),
            ".css" => await CompileStyleAsync(absolutePath, text, cancellationToken),
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
        var document = JazorVueParser.Parse(absolutePath, text);
        var sfc = _compiler.Compile(document);
        var module = await _frontendCompiler.CompileSfcAsync(absolutePath, sfc.GeneratedVueText, cancellationToken);
        if (module is null)
        {
            return CreateVolarUnavailableResult(
                "Vue SFC compilation is not available because the frontend compiler is unavailable.",
                sfc.Diagnostics);
        }

        var hotReloadManifestEntry = CreateJazorHotReloadManifestEntry(absolutePath, document, compilation: sfc, module, companionDocuments);
        var moduleSignature = ComputeJazorModuleSignature(module.JavaScript, hotReloadManifestEntry);
        var chainedSourceMap = ChainJazorSourceMap(module.SourceMap, sfc);
        var preparedJavaScript = await PrepareJavaScriptForCurrentModeAsync(absolutePath, module.JavaScript, module.StyleContent, cancellationToken);
        var servedSourceMap = OffsetSourceMapGeneratedLines(chainedSourceMap, preparedJavaScript.GeneratedLineOffset);

        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = AttachInlineSourceMap(preparedJavaScript.Content, servedSourceMap),
            ModuleSignature = moduleSignature,
            HotReloadManifestEntry = hotReloadManifestEntry,
            SourceMap = servedSourceMap,
            StyleContent = module.StyleContent,
            StyleFragments = module.StyleFragments,
            Dependencies = module.Dependencies,
            EmbeddedStyleDependencies = module.EmbeddedStyleDependencies,
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
            return CreateVolarUnavailableResult("Vue SFC compilation is not available because the frontend compiler is unavailable.");
        }

        var preparedJavaScript = await PrepareJavaScriptForCurrentModeAsync(absolutePath, module.JavaScript, module.StyleContent, cancellationToken);
        var servedSourceMap = OffsetSourceMapGeneratedLines(module.SourceMap, preparedJavaScript.GeneratedLineOffset);
        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = AttachInlineSourceMap(preparedJavaScript.Content, servedSourceMap),
            ModuleSignature = ComputeContentHash(module.JavaScript),
            SourceMap = servedSourceMap,
            StyleContent = module.StyleContent,
            StyleFragments = module.StyleFragments,
            Dependencies = module.Dependencies,
            EmbeddedStyleDependencies = module.EmbeddedStyleDependencies,
            SupportsHmr = module.SupportsHmr
        };
    }

    private async ValueTask<CompilationResult> CompileTypeScriptAsync(
        string absolutePath,
        string text,
        CancellationToken cancellationToken)
    {
        var module = await _frontendCompiler.CompileTypeScriptAsync(absolutePath, text, cancellationToken);
        var preparedJavaScript = module is null
            ? default
            : await PrepareJavaScriptForCurrentModeAsync(absolutePath, module.JavaScript, styleContent: null, cancellationToken);
        return module is null
            ? CreateVolarUnavailableResult("TypeScript transpilation is not available because the frontend compiler is unavailable.")
            : new CompilationResult
            {
                ContentType = "text/javascript",
                Content = AttachInlineSourceMap(preparedJavaScript.Content, module.SourceMap),
                ModuleSignature = ComputeContentHash(module.JavaScript),
                SourceMap = module.SourceMap,
                Dependencies = module.Dependencies
            };
    }

    private async ValueTask<CompilationResult> CompileJavaScriptAsync(
        string absolutePath,
        string content,
        CancellationToken cancellationToken)
        => !_buildMode
            ? new CompilationResult
            {
                ContentType = "text/javascript",
                Content = content,
                Dependencies = DenoVolarModuleCompiler.ExtractJavaScriptDependencies(content)
            }
            : new CompilationResult
            {
                ContentType = "text/javascript",
                Content = await TransformBuildJavaScriptAsync(absolutePath, content, cancellationToken),
                Dependencies = DenoVolarModuleCompiler.ExtractJavaScriptDependencies(content)
            };

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

    private async ValueTask<CompilationResult> CompileStyleAsync(
        string absolutePath,
        string content,
        CancellationToken cancellationToken)
    {
        if (!LooksLikeCssModulePath(absolutePath))
        {
            return CreateStylePassThrough(content);
        }

        var module = await _frontendCompiler.CompileCssModuleAsync(absolutePath, content, cancellationToken);
        if (module is null)
        {
            return CreateVolarUnavailableResult("CSS Modules compilation is not available because the frontend compiler is unavailable.");
        }

        if (_buildMode)
        {
            return new CompilationResult
            {
                ContentType = "text/css",
                Content = module.CssContent,
                StyleContent = module.CssContent,
                CssModuleMappings = module.Mappings,
                Diagnostics = module.Diagnostics
            };
        }

        var cssModuleJavaScript = CreateCssModuleJavaScript(module.Mappings);
        var servedModule = CreateServedModule(absolutePath, cssModuleJavaScript, module.CssContent);
        return new CompilationResult
        {
            ContentType = "text/javascript",
            Content = servedModule.Content,
            ModuleSignature = ComputeContentHash(servedModule.Content),
            StyleContent = module.CssContent,
            CssModuleMappings = module.Mappings,
            Diagnostics = module.Diagnostics,
            SupportsHmr = true
        };
    }

    private static CompilationResult CreateVolarUnavailableResult(
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
              let style = document.querySelector(`style[data-jolt="${__jazorStyleId}"]`);
              if (!style) {
                style = document.createElement("style");
                style.setAttribute("data-jolt", __jazorStyleId);
                document.head.appendChild(style);
              }
              style.textContent = __jazorStyle;
            }
            """;

        return new ServedModuleContent(
            string.Concat(prefix, "\n", javaScript),
            CountLines(prefix));
    }

    private async ValueTask<PreparedJavaScriptContent> PrepareJavaScriptForCurrentModeAsync(
        string documentPath,
        string javaScript,
        string? styleContent,
        CancellationToken cancellationToken)
    {
        if (_buildMode)
        {
            return new PreparedJavaScriptContent(
                await TransformBuildJavaScriptAsync(documentPath, javaScript, cancellationToken),
                GeneratedLineOffset: 0);
        }

        var servedModule = CreateServedModule(documentPath, javaScript, styleContent);
        return new PreparedJavaScriptContent(servedModule.Content, servedModule.JavaScriptLineOffset);
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

    private static string ComputeCacheHash(
        string text,
        IReadOnlyList<DocumentSnapshot>? companionDocuments)
    {
        if (companionDocuments is null || companionDocuments.Count == 0)
        {
            return ComputeContentHash(text);
        }

        var builder = new StringBuilder(text.Length + (companionDocuments.Count * 64));
        builder.Append(text);
        foreach (var companion in companionDocuments
                     .Where(static document => document.DocumentKind == DocumentKind.CSharp)
                     .OrderBy(static document => document.DocumentPath, StringComparer.OrdinalIgnoreCase))
        {
            builder
                .Append("\n// companion:")
                .Append(Path.GetFullPath(companion.DocumentPath))
                .Append('|')
                .Append(companion.Version)
                .Append('\n')
                .Append(companion.Text);
        }

        return ComputeContentHash(builder.ToString());
    }

    private bool TryGetCachedResultCore(
        string absolutePath,
        string contentHash,
        [NotNullWhen(true)] out CompilationResult? result)
    {
        lock (_stateGate)
        {
            return _cache.TryGet(absolutePath, contentHash, out result);
        }
    }

    private void PublishCompilationResult(string absolutePath, string contentHash, CompilationResult result)
    {
        lock (_stateGate)
        {
            SynchronizeSourceMapRegistration(absolutePath, result);
            _dependencyGraph?.Record(absolutePath, result.Dependencies);
            var evictedPaths = _cache.Set(absolutePath, contentHash, result);
            foreach (var evictedPath in evictedPaths)
            {
                _dependencyGraph?.Remove(evictedPath);
                UnregisterSourceMap(evictedPath);
            }
        }
    }

    private static string StripBuildCssImports(string javaScript)
        => StaticCssImportPattern.Replace(
            javaScript,
            static match =>
            {
                var specifier = match.Groups["specifier"].Value;
                return LooksLikeCssSpecifier(specifier)
                    ? PreserveLineStructure(match.Value)
                    : match.Value;
            });

    private static bool LooksLikeCssSpecifier(string specifier)
    {
        var normalized = StripQueryAndHash(specifier).Trim();
        return normalized.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeCssModuleSpecifier(string specifier)
    {
        var normalized = StripQueryAndHash(specifier).Trim();
        return normalized.EndsWith(".module.css", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeCssModulePath(string path)
        => path.EndsWith(".module.css", StringComparison.OrdinalIgnoreCase);

    private static string StripQueryAndHash(string value)
    {
        var index = value.IndexOfAny(['?', '#']);
        return index >= 0
            ? value[..index]
            : value;
    }

    private static string PreserveLineStructure(string value)
    {
        var buffer = value.ToCharArray();
        for (var index = 0; index < buffer.Length; index++)
        {
            if (buffer[index] is not ('\r' or '\n'))
            {
                buffer[index] = ' ';
            }
        }

        return new string(buffer);
    }

    private async ValueTask<string> TransformBuildJavaScriptAsync(
        string documentPath,
        string javaScript,
        CancellationToken cancellationToken)
    {
        var strippedJavaScript = StripBuildCssImports(javaScript);
        var withDefaultCssModules = await RewriteBuildCssModuleImportsAsync(
            documentPath,
            strippedJavaScript,
            StaticCssModuleDefaultImportPattern,
            cancellationToken);
        var withNamespaceCssModules = await RewriteBuildCssModuleImportsAsync(
            documentPath,
            withDefaultCssModules,
            StaticCssModuleNamespaceImportPattern,
            cancellationToken);
        return await RewriteBuildCssModuleImportsAsync(
            documentPath,
            withNamespaceCssModules,
            StaticCssModuleNamedDefaultImportPattern,
            cancellationToken);
    }

    private async ValueTask<string> RewriteBuildCssModuleImportsAsync(
        string documentPath,
        string javaScript,
        Regex importPattern,
        CancellationToken cancellationToken)
    {
        return await ReplaceMatchesAsync(
            importPattern,
            javaScript,
            async match =>
            {
                var specifier = match.Groups["specifier"].Value;
                if (!LooksLikeCssModuleSpecifier(specifier))
                {
                    return match.Value;
                }

                var mappings = await ResolveCssModuleMappingsAsync(documentPath, specifier, cancellationToken);
                var binding = match.Groups["binding"].Value;
                return CreateCssModuleImportReplacement(match.Value, binding, mappings);
            });
    }

    private async ValueTask<IReadOnlyDictionary<string, string>> ResolveCssModuleMappingsAsync(
        string importerPath,
        string specifier,
        CancellationToken cancellationToken)
    {
        if (_moduleResolver is null)
        {
            throw new InvalidOperationException("CSS Modules resolution requires a module resolver in build mode.");
        }

        var resolved = _moduleResolver.Resolve(specifier, importerPath);
        if (!resolved.Found || resolved.IsVirtual)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(resolved.Error)
                    ? $"Unable to resolve CSS module '{specifier}' from '{importerPath}'."
                    : resolved.Error);
        }

        if (!LooksLikeCssModulePath(resolved.AbsolutePath))
        {
            throw new InvalidOperationException(
                $"Resolved CSS module '{specifier}' from '{importerPath}' to unsupported path '{resolved.AbsolutePath}'.");
        }

        var cssModuleResult = await CompileAsync(resolved.AbsolutePath, cancellationToken);
        if (cssModuleResult.IsError)
        {
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(cssModuleResult.ErrorMessage)
                    ? $"Failed to compile CSS module '{specifier}'."
                    : cssModuleResult.ErrorMessage);
        }

        return cssModuleResult.CssModuleMappings;
    }

    private static string CreateCssModuleImportReplacement(
        string originalImport,
        string binding,
        IReadOnlyDictionary<string, string> mappings)
    {
        var serializedMappings = System.Text.Json.JsonSerializer.Serialize(
            mappings
                .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
                .ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal));
        var indentation = GetLeadingIndentation(originalImport);
        return $"{indentation}const {binding} = {serializedMappings};";
    }

    private static string CreateCssModuleJavaScript(IReadOnlyDictionary<string, string> mappings)
    {
        var serializedMappings = System.Text.Json.JsonSerializer.Serialize(
            mappings
                .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
                .ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal));
        return $$"""
            const __jazorHotContext = import.meta.hot ?? globalThis.__JAZOR_HMR__?.createHotContext(import.meta.url);
            if (__jazorHotContext) {
              import.meta.hot = __jazorHotContext;
            }

            const __jazorCssModules = {{serializedMappings}};
            function __jazorSyncCssModules(target, source) {
              for (const key of Object.keys(target)) {
                if (!(key in source)) {
                  delete target[key];
                }
              }

              Object.assign(target, source);
            }

            export default __jazorCssModules;

            if (import.meta.hot) {
              import.meta.hot.accept((updatedModule) => {
                const updatedMappings = updatedModule?.default;
                if (!updatedMappings || typeof updatedMappings !== "object") {
                  import.meta.hot.invalidate?.("CSS modules update payload was unavailable.");
                  return;
                }

                __jazorSyncCssModules(__jazorCssModules, updatedMappings);
              });
            }
            """;
    }

    private static string GetLeadingIndentation(string value)
    {
        var length = 0;
        while (length < value.Length && (value[length] == ' ' || value[length] == '\t'))
        {
            length++;
        }

        return value[..length];
    }

    private static async ValueTask<string> ReplaceMatchesAsync(
        Regex pattern,
        string input,
        Func<Match, ValueTask<string>> replacementFactory)
    {
        var matches = pattern.Matches(input);
        if (matches.Count == 0)
        {
            return input;
        }

        var builder = new StringBuilder(input.Length);
        var currentIndex = 0;
        foreach (Match match in matches)
        {
            builder.Append(input, currentIndex, match.Index - currentIndex);
            builder.Append(await replacementFactory(match));
            currentIndex = match.Index + match.Length;
        }

        builder.Append(input, currentIndex, input.Length - currentIndex);
        return builder.ToString();
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
        catch (System.Text.Json.JsonException)
        {
            return sourceMap;
        }
        catch (FormatException)
        {
            return sourceMap;
        }
        catch (InvalidOperationException)
        {
            return sourceMap;
        }
        catch (NotSupportedException)
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
        catch (System.Text.Json.JsonException)
        {
            return javaScriptSourceMap;
        }
        catch (FormatException)
        {
            return javaScriptSourceMap;
        }
        catch (ArgumentException)
        {
            return javaScriptSourceMap;
        }
        catch (InvalidOperationException)
        {
            return javaScriptSourceMap;
        }
        catch (NotSupportedException)
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
        VolarModuleCompilation module,
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
            AssemblyName: "Jolt",
            ComponentId: resolvedModulePath,
            ModuleId: resolvedModulePath,
            ComponentName: Path.GetFileNameWithoutExtension(documentPath),
            RouteTemplates: [],
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
            AssemblyName: "Jolt",
            ComponentId: resolvedModulePath,
            ModuleId: resolvedModulePath,
            ComponentName: Path.GetFileNameWithoutExtension(documentPath),
            RouteTemplates: [],
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

    private readonly record struct PreparedJavaScriptContent(
        string Content,
        int GeneratedLineOffset);
}

