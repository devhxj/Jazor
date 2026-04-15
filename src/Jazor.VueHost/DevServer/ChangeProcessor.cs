using System.Text.Json.Serialization;
using Jazor.Emit;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Workspace;

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
        => await ProcessChangesCoreAsync(changedPaths, documentOverrides: null, cancellationToken);

    public async ValueTask<ChangeProcessingResult> ProcessWorkspaceDocumentChangeAsync(
        DocumentSnapshot document,
        CancellationToken cancellationToken)
        => await ProcessWorkspaceDocumentChangeAsync(document, [document], cancellationToken);

    public async ValueTask<ChangeProcessingResult> ProcessWorkspaceDocumentChangeAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(openDocuments);
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedPath = Path.GetFullPath(document.DocumentPath);
        var documentOverrides = await BuildDocumentOverridesAsync(
            new DocumentSnapshot(normalizedPath, document.DocumentKind, document.Text, document.Version),
            openDocuments,
            cancellationToken);
        return await ProcessChangesCoreAsync(
            [normalizedPath],
            documentOverrides,
            cancellationToken);
    }

    private async ValueTask<IReadOnlyDictionary<string, DocumentSnapshot>> BuildDocumentOverridesAsync(
        DocumentSnapshot document,
        IReadOnlyList<DocumentSnapshot> openDocuments,
        CancellationToken cancellationToken)
    {
        var overrides = new Dictionary<string, DocumentSnapshot>(StringComparer.OrdinalIgnoreCase)
        {
            [Path.GetFullPath(document.DocumentPath)] = document
        };

        var effectivePath = VueHostWorkspaceResolver.TryResolveOwningJazorPath(document.DocumentPath, out var owningJazorPath)
            ? Path.GetFullPath(owningJazorPath)
            : Path.GetFullPath(document.DocumentPath);

        if (!effectivePath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase))
        {
            return overrides;
        }

        foreach (var candidatePath in EnumerateWorkspaceSourcePaths(effectivePath))
        {
            var normalizedCandidatePath = Path.GetFullPath(candidatePath);
            if (overrides.ContainsKey(normalizedCandidatePath))
            {
                continue;
            }

            var openDocument = openDocuments.FirstOrDefault(openDocument =>
                string.Equals(
                    Path.GetFullPath(openDocument.DocumentPath),
                    normalizedCandidatePath,
                    StringComparison.OrdinalIgnoreCase));
            if (openDocument is not null)
            {
                overrides[normalizedCandidatePath] = new DocumentSnapshot(
                    normalizedCandidatePath,
                    openDocument.DocumentKind,
                    openDocument.Text,
                    openDocument.Version);
                continue;
            }

            var resolvedDocument = await VueHostWorkspaceResolver.ResolveDocumentAsync(
                normalizedCandidatePath,
                openDocuments,
                cancellationToken);
            if (resolvedDocument is null)
            {
                continue;
            }

            overrides[normalizedCandidatePath] = new DocumentSnapshot(
                Path.GetFullPath(resolvedDocument.DocumentPath),
                resolvedDocument.DocumentKind,
                resolvedDocument.Text,
                resolvedDocument.Version);
        }

        return overrides;
    }

    private static IEnumerable<string> EnumerateWorkspaceSourcePaths(string effectivePath)
    {
        yield return effectivePath;
        foreach (var companionPath in VueHostWorkspaceResolver.GetCoLocatedCodeBehindPaths(effectivePath))
        {
            yield return companionPath;
        }
    }

    private async ValueTask<ChangeProcessingResult> ProcessChangesCoreAsync(
        IReadOnlyList<string> changedPaths,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
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
        var routedChanges = normalizedChangedPaths
            .Select(static path => ChangeRoute.Create(path))
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

        var classifiedReload = TryCreateClassifiedReload(routedChanges, normalizedChangedPaths, documentOverrides);
        if (classifiedReload is not null)
        {
            return classifiedReload;
        }

        var sfcHotUpdate = await TryCreateSfcHotUpdateAsync(routedChanges, normalizedChangedPaths, documentOverrides, cancellationToken);
        if (sfcHotUpdate is not null)
        {
            return sfcHotUpdate;
        }

        var scriptHotUpdate = await TryCreateScriptHotUpdateAsync(normalizedChangedPaths, documentOverrides, cancellationToken);
        if (scriptHotUpdate is not null)
        {
            return scriptHotUpdate;
        }

        var styleUpdate = await TryCreateStyleUpdateAsync(normalizedChangedPaths, documentOverrides, cancellationToken);
        if (styleUpdate is not null)
        {
            return styleUpdate;
        }

        var affectedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var routedChange in routedChanges)
        {
            affectedModules.Add(routedChange.OriginalPath);
            affectedModules.Add(routedChange.EffectivePath);
            foreach (var affectedModule in _dependencyGraph.GetAllAffectedModules(routedChange.EffectivePath))
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

    private static ChangeProcessingResult? TryCreateClassifiedReload(
        IReadOnlyList<ChangeRoute> routedChanges,
        IReadOnlyList<string> changedPaths,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides)
    {
        string? reason = null;
        foreach (var routedChange in routedChanges)
        {
            var fileName = Path.GetFileName(routedChange.OriginalPath);
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

            if (HasDocumentOverride(routedChange.OriginalPath, documentOverrides))
            {
                continue;
            }

            if (!File.Exists(routedChange.OriginalPath)
                && !string.Equals(routedChange.OriginalPath, routedChange.EffectivePath, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!File.Exists(routedChange.OriginalPath))
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
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
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

            var nextResult = await RecompileAsync(changedPath, documentOverrides, cancellationToken);
            if (nextResult.IsError)
            {
                return CreateErrorResult(changedPaths, changedPaths, nextResult.ErrorMessage);
            }

            if (previousResult.IsError
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
        IReadOnlyList<ChangeRoute> routedChanges,
        IReadOnlyList<string> changedPaths,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
        CancellationToken cancellationToken)
    {
        if (routedChanges.Count == 0
            || routedChanges.Any(static route =>
                !route.EffectivePath.EndsWith(".vue", StringComparison.OrdinalIgnoreCase)
                && !route.EffectivePath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var affectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var routedChange in routedChanges)
            affectedPaths.Add(routedChange.OriginalPath);

        var inlineStyleUpdates = new List<InlineStyleUpdate>(routedChanges.Count);
        var jsUpdates = new List<JavaScriptHotUpdate>(routedChanges.Count);

        foreach (var routeGroup in routedChanges
                     .GroupBy(static route => route.EffectivePath, StringComparer.OrdinalIgnoreCase)
                     .OrderBy(static group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            var changedPath = routeGroup.Key;
            if (!_compiler.TryGetCachedResult(changedPath, out var previousResult) || previousResult is null)
            {
                return null;
            }

            var nextResult = await RecompileAsync(changedPath, documentOverrides, cancellationToken);
            if (nextResult.IsError)
            {
                return CreateErrorResult(changedPaths, affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(), nextResult.ErrorMessage);
            }

            if (previousResult.IsError)
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

    private async ValueTask<ChangeProcessingResult?> TryCreateScriptHotUpdateAsync(
        IReadOnlyList<string> changedPaths,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
        CancellationToken cancellationToken)
    {
        if (changedPaths.Count == 0
            || changedPaths.Any(static path =>
                !path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)
                && !path.EndsWith(".js", StringComparison.OrdinalIgnoreCase)))
        {
            return null;
        }

        var affectedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var jsUpdates = new List<JavaScriptHotUpdate>(changedPaths.Count);

        foreach (var changedPath in changedPaths.Order(StringComparer.OrdinalIgnoreCase))
        {
            if (!_compiler.TryGetCachedResult(changedPath, out var previousResult) || previousResult is null)
            {
                return null;
            }

            affectedPaths.Add(changedPath);
            foreach (var dependent in _dependencyGraph.GetAllAffectedModules(changedPath))
            {
                affectedPaths.Add(dependent);
            }

            var nextResult = await RecompileAsync(changedPath, documentOverrides, cancellationToken);
            if (nextResult.IsError)
            {
                return CreateErrorResult(
                    changedPaths,
                    affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
                    nextResult.ErrorMessage);
            }

            if (previousResult.IsError)
            {
                return null;
            }

            var resolvedUrl = _moduleResolver.GetResolvedUrlForAbsolutePath(changedPath);
            jsUpdates.Add(
                new JavaScriptHotUpdate
                {
                    Path = resolvedUrl,
                    AcceptedPath = resolvedUrl
                });
        }

        return new ChangeProcessingResult
        {
            UpdateKind = ChangeUpdateKind.JavaScriptUpdate,
            ChangedPaths = changedPaths,
            AffectedPaths = affectedPaths.Order(StringComparer.OrdinalIgnoreCase).ToArray(),
            JavaScriptUpdates = jsUpdates
        };
    }

    private async ValueTask<CompilationResult> RecompileAsync(
        string changedPath,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
        CancellationToken cancellationToken)
    {
        var companionDocuments = GetCompanionDocumentOverrides(changedPath, documentOverrides);
        if (TryGetDocumentOverride(changedPath, documentOverrides, out var documentOverride)
            && documentOverride.DocumentKind is not DocumentKind.CSharp)
        {
            return await _compiler.RecompileAsync(changedPath, documentOverride.Text, companionDocuments, cancellationToken);
        }

        return companionDocuments.Count > 0
            ? await _compiler.RecompileAsync(changedPath, companionDocuments, cancellationToken)
            : await _compiler.RecompileAsync(changedPath, cancellationToken);
    }

    private static bool HasDocumentOverride(
        string path,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides)
        => TryGetDocumentOverride(path, documentOverrides, out _);

    private static bool TryGetDocumentOverride(
        string path,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides,
        out DocumentSnapshot documentOverride)
    {
        documentOverride = null!;
        if (documentOverrides is null)
        {
            return false;
        }

        return documentOverrides.TryGetValue(Path.GetFullPath(path), out documentOverride!);
    }

    private static IReadOnlyList<DocumentSnapshot> GetCompanionDocumentOverrides(
        string changedPath,
        IReadOnlyDictionary<string, DocumentSnapshot>? documentOverrides)
    {
        if (!changedPath.EndsWith(".jazor", StringComparison.OrdinalIgnoreCase)
            || documentOverrides is null)
        {
            return Array.Empty<DocumentSnapshot>();
        }

        var companionDocuments = new List<DocumentSnapshot>();
        foreach (var companionPath in VueHostWorkspaceResolver.GetCoLocatedCodeBehindPaths(changedPath))
        {
            if (!documentOverrides.TryGetValue(Path.GetFullPath(companionPath), out var companionDocument)
                || companionDocument.DocumentKind != DocumentKind.CSharp)
            {
                continue;
            }

            companionDocuments.Add(new DocumentSnapshot(
                Path.GetFullPath(companionDocument.DocumentPath),
                companionDocument.DocumentKind,
                companionDocument.Text,
                companionDocument.Version));
        }

        return companionDocuments;
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

    private readonly record struct ChangeRoute(
        string OriginalPath,
        string EffectivePath)
    {
        public static ChangeRoute Create(string originalPath)
        {
            if (VueHostWorkspaceResolver.TryResolveOwningJazorPath(originalPath, out var jazorDocumentPath))
            {
                return new ChangeRoute(originalPath, jazorDocumentPath);
            }

            return new ChangeRoute(originalPath, originalPath);
        }
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

    private static ChangeProcessingResult CreateErrorResult(
        IReadOnlyList<string> changedPaths,
        IReadOnlyList<string> affectedPaths,
        string? message)
    {
        return new ChangeProcessingResult
        {
            UpdateKind = ChangeUpdateKind.Error,
            ErrorMessage = string.IsNullOrWhiteSpace(message) ? "Hot update failed." : message,
            ChangedPaths = changedPaths,
            AffectedPaths = affectedPaths
        };
    }
}

internal enum ChangeUpdateKind
{
    FullReload,
    StyleUpdate,
    JavaScriptUpdate,
    Error
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

    public string? ErrorMessage { get; init; }
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
