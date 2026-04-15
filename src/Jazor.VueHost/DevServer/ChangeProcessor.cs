using System.Text.Json.Serialization;
using Jazor.Emit;

namespace Jazor.VueHost.DevServer;

internal sealed class ChangeProcessor
{
    private readonly OnDemandCompiler _compiler;
    private readonly ModuleResolver _moduleResolver;
    private readonly DependencyGraph _dependencyGraph;

    public ChangeProcessor(
        OnDemandCompiler compiler,
        ModuleResolver moduleResolver,
        DependencyGraph dependencyGraph)
    {
        _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        _moduleResolver = moduleResolver ?? throw new ArgumentNullException(nameof(moduleResolver));
        _dependencyGraph = dependencyGraph ?? throw new ArgumentNullException(nameof(dependencyGraph));
    }

    public async ValueTask<ChangeProcessingResult> ProcessChangesAsync(
        IReadOnlyList<string> changedPaths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(changedPaths);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedChangedPaths = changedPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (normalizedChangedPaths.Length == 0)
        {
            return new ChangeProcessingResult
            {
                UpdateKind = ChangeUpdateKind.FullReload,
                FullReloadReason = "empty-change-set",
                ChangedPaths = [],
                AffectedPaths = []
            };
        }

        var classifiedReload = TryCreateClassifiedReload(normalizedChangedPaths);
        if (classifiedReload is not null)
        {
            return classifiedReload;
        }

        var sfcHotUpdate = await TryCreateSfcHotUpdateAsync(normalizedChangedPaths, cancellationToken);
        if (sfcHotUpdate is not null)
        {
            return sfcHotUpdate;
        }

        var styleUpdate = await TryCreateStyleUpdateAsync(normalizedChangedPaths, cancellationToken);
        if (styleUpdate is not null)
        {
            return styleUpdate;
        }

        var affectedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var changedPath in normalizedChangedPaths)
        {
            affectedModules.Add(changedPath);
            foreach (var affectedModule in _dependencyGraph.GetAllAffectedModules(changedPath))
            {
                affectedModules.Add(affectedModule);
            }
        }

        foreach (var affectedModule in affectedModules)
        {
            _compiler.Invalidate(affectedModule);
        }

        var orderedAffectedModules = affectedModules.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        return new ChangeProcessingResult
        {
            UpdateKind = ChangeUpdateKind.FullReload,
            FullReloadReason = affectedModules.SetEquals(normalizedChangedPaths)
                ? "frontend-change"
                : "frontend-change-with-dependents",
            AffectedPaths = orderedAffectedModules,
            ChangedPaths = normalizedChangedPaths,
        };
    }

    private static ChangeProcessingResult? TryCreateClassifiedReload(IReadOnlyList<string> changedPaths)
    {
        string? reason = null;
        foreach (var changedPath in changedPaths)
        {
            var fileName = Path.GetFileName(changedPath);
            if (string.Equals(fileName, "index.html", StringComparison.OrdinalIgnoreCase))
            {
                reason = "index-html-change";
                break;
            }

            if (string.Equals(fileName, "jazor.config.json", StringComparison.OrdinalIgnoreCase))
            {
                reason = "config-change";
                break;
            }

            if (!File.Exists(changedPath))
            {
                reason = "missing-file-change";
                break;
            }
        }

        return reason is null
            ? null
            : new ChangeProcessingResult
            {
                UpdateKind = ChangeUpdateKind.FullReload,
                FullReloadReason = reason,
                ChangedPaths = changedPaths,
                AffectedPaths = changedPaths
            };
    }

    private async ValueTask<ChangeProcessingResult?> TryCreateStyleUpdateAsync(
        IReadOnlyList<string> changedPaths,
        CancellationToken cancellationToken)
    {
        if (changedPaths.Count == 0)
        {
            return null;
        }

        if (changedPaths.All(static path => path.EndsWith(".css", StringComparison.OrdinalIgnoreCase)))
        {
            var cssAffectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var changedPath in changedPaths)
            {
                cssAffectedPaths.Add(changedPath);
                foreach (var affectedPath in _dependencyGraph.GetAllAffectedModules(changedPath))
                {
                    cssAffectedPaths.Add(affectedPath);
                }
            }

            var changedSet = changedPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (cssAffectedPaths.Any(path => !changedSet.Contains(path)))
            {
                return null;
            }

            foreach (var affectedPath in cssAffectedPaths)
            {
                _compiler.Invalidate(affectedPath);
            }

            return new ChangeProcessingResult
            {
                UpdateKind = ChangeUpdateKind.StyleUpdate,
                ChangedPaths = changedPaths,
                AffectedPaths = cssAffectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                ChangedCssUrls = changedPaths
                    .Select(_moduleResolver.GetResolvedUrlForAbsolutePath)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            };
        }

        if (changedPaths.Any(static path =>
                !path.EndsWith(".vue", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var inlineStyleUpdates = new List<InlineStyleUpdate>();
        foreach (var changedPath in changedPaths)
        {
            var affectedPaths = _dependencyGraph.GetAllAffectedModules(changedPath);
            if (affectedPaths.Count > 0)
            {
                return null;
            }

            if (!_compiler.TryGetCachedResult(changedPath, out var previousResult) || previousResult is null)
            {
                return null;
            }

            var nextResult = await _compiler.RecompileAsync(changedPath, cancellationToken);
            if (nextResult.IsError
                || previousResult.IsError
                || string.IsNullOrWhiteSpace(previousResult.ModuleSignature)
                || string.IsNullOrWhiteSpace(nextResult.ModuleSignature)
                || !string.Equals(previousResult.ModuleSignature, nextResult.ModuleSignature, StringComparison.Ordinal)
                || string.Equals(previousResult.StyleContent, nextResult.StyleContent, StringComparison.Ordinal))
            {
                return null;
            }

            inlineStyleUpdates.Add(
                new InlineStyleUpdate
                {
                    TargetId = _moduleResolver.GetStyleTargetIdForAbsolutePath(changedPath),
                    Content = nextResult.StyleContent ?? string.Empty
                });
        }

        return new ChangeProcessingResult
        {
            UpdateKind = ChangeUpdateKind.StyleUpdate,
            ChangedPaths = changedPaths,
            AffectedPaths = changedPaths,
            InlineStyleUpdates = inlineStyleUpdates
        };
    }

    private async ValueTask<ChangeProcessingResult?> TryCreateSfcHotUpdateAsync(
        IReadOnlyList<string> changedPaths,
        CancellationToken cancellationToken)
    {
        if (changedPaths.Count == 0
            || changedPaths.Any(static path =>
                !path.EndsWith(".vue", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var affectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var inlineStyleUpdates = new List<InlineStyleUpdate>(changedPaths.Count);
        var jsUpdates = new List<JavaScriptHotUpdate>(changedPaths.Count);

        foreach (var changedPath in changedPaths)
        {
            if (!_compiler.TryGetCachedResult(changedPath, out var previousResult) || previousResult is null)
            {
                return null;
            }

            var nextResult = await _compiler.RecompileAsync(changedPath, cancellationToken);
            if (nextResult.IsError || previousResult.IsError)
            {
                return null;
            }

            affectedPaths.Add(changedPath);
            foreach (var dependent in _dependencyGraph.GetAllAffectedModules(changedPath))
            {
                affectedPaths.Add(dependent);
            }

            if (string.Equals(previousResult.ModuleSignature, nextResult.ModuleSignature, StringComparison.Ordinal))
            {
                if (!string.Equals(previousResult.StyleContent, nextResult.StyleContent, StringComparison.Ordinal))
                {
                    inlineStyleUpdates.Add(
                        new InlineStyleUpdate
                        {
                            TargetId = _moduleResolver.GetStyleTargetIdForAbsolutePath(changedPath),
                            Content = nextResult.StyleContent ?? string.Empty
                        });
                }

                continue;
            }

            if (!nextResult.SupportsHmr)
            {
                return null;
            }

            if (changedPath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
            {
                var manifestDiff = TryDiffJazorHotReload(previousResult, nextResult);
                if (manifestDiff is null)
                {
                    return null;
                }

                if (manifestDiff.Action == RazorVueHotUpdateAction.FullReload)
                {
                    return new ChangeProcessingResult
                    {
                        UpdateKind = ChangeUpdateKind.FullReload,
                        FullReloadReason = manifestDiff.Reason,
                        ChangedPaths = changedPaths,
                        AffectedPaths = affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray()
                    };
                }
            }

            var resolvedUrl = _moduleResolver.GetResolvedUrlForAbsolutePath(changedPath);
            jsUpdates.Add(
                new JavaScriptHotUpdate
                {
                    Path = resolvedUrl,
                    AcceptedPath = resolvedUrl
                });
        }

        if (jsUpdates.Count == 0)
        {
            return inlineStyleUpdates.Count == 0
                ? null
                : new ChangeProcessingResult
                {
                    UpdateKind = ChangeUpdateKind.StyleUpdate,
                    ChangedPaths = changedPaths,
                    AffectedPaths = affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                    InlineStyleUpdates = inlineStyleUpdates
                };
        }

        return new ChangeProcessingResult
        {
            UpdateKind = ChangeUpdateKind.JavaScriptUpdate,
            ChangedPaths = changedPaths,
            AffectedPaths = affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            JavaScriptUpdates = jsUpdates
        };
    }

    private static RazorVueManifestDiffResult? TryDiffJazorHotReload(
        CompilationResult previousResult,
        CompilationResult nextResult)
    {
        if (previousResult.HotReloadManifestEntry is null || nextResult.HotReloadManifestEntry is null)
        {
            return null;
        }

        return RazorVueManifestDiffer.Diff(
            CreateSingleModuleManifest(previousResult.HotReloadManifestEntry),
            CreateSingleModuleManifest(nextResult.HotReloadManifestEntry));
    }

    private static RazorVueManifestModel CreateSingleModuleManifest(RazorVueManifestEntry module)
    {
        return new RazorVueManifestModel(
            module.AssemblyName,
            DateTime.UnixEpoch,
            [module],
            [.. module.Styles],
            [.. module.PluginRequirements]);
    }
}

internal enum ChangeUpdateKind
{
    FullReload,
    StyleUpdate,
    JavaScriptUpdate
}

internal sealed class ChangeProcessingResult
{
    public required ChangeUpdateKind UpdateKind { get; init; }

    public string? FullReloadReason { get; init; }

    public required IReadOnlyList<string> ChangedPaths { get; init; }

    public required IReadOnlyList<string> AffectedPaths { get; init; }

    public IReadOnlyList<string> ChangedCssUrls { get; init; } = [];

    public IReadOnlyList<InlineStyleUpdate> InlineStyleUpdates { get; init; } = [];

    public IReadOnlyList<JavaScriptHotUpdate> JavaScriptUpdates { get; init; } = [];
}

internal sealed class InlineStyleUpdate
{
    [JsonPropertyName("path")]
    public required string TargetId { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}

internal sealed class JavaScriptHotUpdate
{
    [JsonPropertyName("type")]
    public string Type => "js-update";

    [JsonPropertyName("path")]
    public required string Path { get; init; }

    [JsonPropertyName("acceptedPath")]
    public required string AcceptedPath { get; init; }
}
