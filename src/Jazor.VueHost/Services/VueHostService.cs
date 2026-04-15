using Jazor.Vue;
using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Frontend;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Hosting;
using Jazor.VueHost.Roslyn.InProc;
using Jazor.VueHost.Rpc;
using Jazor.VueHost.VirtualDocuments.Mapping;
using Jazor.VueHost.Workspace;
using System.Text.RegularExpressions;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

namespace Jazor.VueHost.Services;

public sealed class VueHostService : IVueHostService, IVueHostRpcService, IFrontendContextProvider
{
    private static readonly IReadOnlyList<HostCapabilityDescriptor> HostCapabilities =
    [
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.Ping, "Lightweight liveness probe."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.GetHostInfo, "Returns protocol version and advertised capabilities."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.OpenDocument, "Opens or tracks a document in the workspace."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.UpdateDocument, "Updates an already tracked document."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.CloseDocument, "Stops tracking a document in the workspace."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.GetOpenDocuments, "Returns currently tracked workspace documents."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.GetFrontendContext, "Returns frontend semantic context for a .jazor document."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.AnalyzeJazor, "Delegates .jazor analysis to the analysis client."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.GetVirtualArtifact, "Compiles a .jazor document and returns one requested virtual artifact."),
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.GetHotUpdatePlan, "Returns host-driven hot update impact for a changed workspace document.")
    ];

    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly IVueAnalysisClient _analysisClient;
    private readonly IDenoVolarHost _denoVolarHost;
    private readonly InProcRoslynCodeService _roslynCodeService = new();
    private readonly FallbackJazorAnalysisService _fallbackAnalysisService = new();
    private readonly JazorVueParser _parser = new();
    private int _started;

    public VueHostService(
        IVueHostWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient)
        : this(workspaceStore, analysisClient, denoVolarHost: null)
    {
    }

    internal VueHostService(
        IVueHostWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient,
        IDenoVolarHost? denoVolarHost)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _analysisClient = analysisClient ?? throw new ArgumentNullException(nameof(analysisClient));
        _denoVolarHost = denoVolarHost ?? new DenoVolarHost(new DenoVolarHostOptions());
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _denoVolarHost.StartAsync(cancellationToken);
        Interlocked.Exchange(ref _started, 1);
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Exchange(ref _started, 0);
        await _denoVolarHost.StopAsync(cancellationToken);
    }

    public Task<PingResponse> PingAsync(CancellationToken cancellationToken)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new PingResponse(
            message: "pong",
            protocolVersion: "0.1"));
    }

    public Task<GetHostInfoResponse> GetHostInfoAsync(CancellationToken cancellationToken)
    {
        EnsureStarted();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new GetHostInfoResponse(
            hostName: "Jazor.VueHost",
            protocolVersion: "0.1",
            capabilities: HostCapabilities));
    }

    public async Task OpenDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken)
    {
        EnsureStarted();
        await _workspaceStore.UpsertDocumentAsync(documentSnapshot, cancellationToken);
        VueHostWorkspaceResolver.InvalidatePath(documentSnapshot.DocumentPath);
    }

    public Task UpdateDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken)
        => OpenDocumentAsync(documentSnapshot, cancellationToken);

    public async Task CloseDocumentAsync(string documentPath, CancellationToken cancellationToken)
    {
        EnsureStarted();
        await _workspaceStore.RemoveDocumentAsync(documentPath, cancellationToken);
        VueHostWorkspaceResolver.InvalidatePath(documentPath);
    }

    public async Task<GetFrontendContextResponse> GetFrontendContextAsync(
        GetFrontendContextRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStarted();

        var jazorDocument = await ResolveJazorDocumentSnapshotAsync(request.DocumentPath, cancellationToken);
        var relatedDocuments = await ResolveRelatedDocumentsAsync(
            jazorDocument,
            request.RelatedDocumentPaths,
            cancellationToken);
        var roslynProjection = await CreateRoslynProjectionContextAsync(jazorDocument, cancellationToken);

        var semanticContext = new SemanticContext(
            contextKind: "frontend",
            relatedDocuments: relatedDocuments,
            properties: CreateContextProperties(
                request.DocumentPath,
                relatedDocuments,
                roslynProjection,
                explicitDocumentCount: request.RelatedDocumentPaths.Count,
                derivedDocumentCount: Math.Max(0, relatedDocuments.Count - request.RelatedDocumentPaths.Count)));

        var artifacts = new List<ArtifactRecord>
        {
            new(
                artifactName: "virtual:" + request.DocumentPath + ".frontend-context.json",
                artifactKind: "frontend-context",
                content: ProtocolJsonSerializer.Serialize(new
                {
                    documentPath = request.DocumentPath,
                    relatedDocuments = relatedDocuments.Select(static document => new
                    {
                        document.DocumentPath,
                        kind = document.DocumentKind.ToString()
                    }).ToArray(),
                    roslynProjection = roslynProjection is null
                        ? null
                        : new
                        {
                            roslynProjection.ProjectionKind,
                            roslynProjection.ProjectedDocumentPath,
                            projectionSegmentCount = roslynProjection.ProjectionMap.Segments.Count,
                            sourceDocumentCount = roslynProjection.SourceDocuments.Count,
                            codeBehindDocumentCount = roslynProjection.CodeBehindDocumentCount
                        }
                }),
                contentHash: null)
        };
        artifacts.AddRange(CreateRoslynProjectionArtifacts(request.DocumentPath, roslynProjection));
        artifacts.AddRange(CreateFrontendSummaryArtifacts(relatedDocuments));

        return new GetFrontendContextResponse(
            semanticContext,
            artifacts);
    }

    ValueTask<GetFrontendContextResponse> IFrontendContextProvider.GetFrontendContextAsync(
        GetFrontendContextRequest request,
        CancellationToken cancellationToken)
        => new(GetFrontendContextAsync(request, cancellationToken));

    public Task<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStarted();
        return AnalyzeJazorCoreAsync(request, cancellationToken);
    }

    public async Task<GetVirtualArtifactResponse> GetVirtualArtifactAsync(
        GetVirtualArtifactRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStarted();

        var jazorDocument = await ResolveJazorDocumentAsync(request, cancellationToken);
        var analysisRequest = await BuildAnalyzeJazorRequestAsync(
            jazorDocument,
            relatedDocuments: Array.Empty<DocumentSnapshot>(),
            frontendContext: null,
            cancellationToken);
        var analysisResponse = await _analysisClient.AnalyzeJazorAsync(
            analysisRequest,
            cancellationToken);
        var artifact = TryGetArtifact(analysisResponse, request.ArtifactKind);

        if (artifact is null)
        {
            analysisResponse = await _fallbackAnalysisService.AnalyzeJazorAsync(
                analysisRequest,
                cancellationToken);
            artifact = TryGetArtifact(analysisResponse, request.ArtifactKind);
        }

        if (artifact is null)
        {
            var availableArtifactKinds = string.Join(
                ", ",
                analysisResponse.Artifacts.Select(static candidate => candidate.ArtifactKind));
            throw new InvalidOperationException(
                $"Virtual artifact '{request.ArtifactKind}' was not produced for '{request.DocumentPath}'. Available artifacts: [{availableArtifactKinds}].");
        }

        return new GetVirtualArtifactResponse(
            artifact,
            analysisResponse.Diagnostics,
            analysisResponse.SourceMaps);
    }

    private static ArtifactRecord? TryGetArtifact(AnalyzeJazorResponse analysisResponse, string artifactKind)
        => analysisResponse.Artifacts.FirstOrDefault(candidate =>
            string.Equals(candidate.ArtifactKind, artifactKind, StringComparison.OrdinalIgnoreCase));

    private async Task<AnalyzeJazorResponse> AnalyzeJazorCoreAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        var hydratedRequest = await BuildAnalyzeJazorRequestAsync(
            request.JazorDocument,
            request.RelatedDocuments,
            request.FrontendContext,
            cancellationToken);
        return await _analysisClient.AnalyzeJazorAsync(hydratedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(
        CancellationToken cancellationToken)
    {
        EnsureStarted();
        return await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
    }

    public async Task<GetHotUpdatePlanResponse> GetHotUpdatePlanAsync(
        GetHotUpdatePlanRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureStarted();

        if (request.DocumentKind == DocumentKind.Jazor)
        {
            return new GetHotUpdatePlanResponse(
                requiresFullReload: false,
                affectedDocumentPaths: [NormalizeComparablePath(request.DocumentPath)],
                reason: "jazor-change");
        }

        if (request.DocumentKind is not (DocumentKind.Vue or DocumentKind.TypeScript or DocumentKind.JavaScript))
        {
            return new GetHotUpdatePlanResponse(
                requiresFullReload: true,
                affectedDocumentPaths: Array.Empty<string>(),
                reason: "unsupported-document-kind");
        }

        var normalizedChangedPath = NormalizeComparablePath(request.DocumentPath);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var affectedJazorDocuments = new List<string>();

        foreach (var document in openDocuments.Where(static document => document.DocumentKind == DocumentKind.Jazor))
        {
            var relatedDocuments = await ResolveRelatedDocumentsAsync(
                document,
                Array.Empty<string>(),
                cancellationToken);
            if (relatedDocuments.Any(candidate =>
                string.Equals(
                    NormalizeComparablePath(candidate.DocumentPath),
                    normalizedChangedPath,
                    StringComparison.OrdinalIgnoreCase)))
            {
                affectedJazorDocuments.Add(NormalizeComparablePath(document.DocumentPath));
            }
        }

        return new GetHotUpdatePlanResponse(
            requiresFullReload: false,
            affectedDocumentPaths: affectedJazorDocuments,
            reason: affectedJazorDocuments.Count == 0
                ? "frontend-change-no-jazor-dependents"
                : "frontend-change");
    }

    private static IReadOnlyDictionary<string, string> CreateContextProperties(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> relatedDocuments,
        FrontendRoslynProjectionContext? roslynProjection = null,
        int explicitDocumentCount = 0,
        int derivedDocumentCount = 0)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["documentPath"] = documentPath,
            ["relatedDocumentCount"] = relatedDocuments.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["explicitDocumentCount"] = explicitDocumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["derivedDocumentCount"] = derivedDocumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["provider"] = "Jazor.VueHost",
            ["relatedDocumentKinds"] = string.Join(
                ",",
                relatedDocuments
                    .Select(static document => document.DocumentKind.ToString())
                    .Distinct(StringComparer.Ordinal)),
            ["projectionKind"] = roslynProjection?.ProjectionKind ?? "none",
            ["projectionDocumentPath"] = roslynProjection?.ProjectedDocumentPath ?? string.Empty,
            ["projectionSegmentCount"] = (roslynProjection?.ProjectionMap.Segments.Count ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["roslynSourceDocumentCount"] = (roslynProjection?.SourceDocuments.Count ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["roslynWorkspaceSourceDocumentCount"] = (roslynProjection?.WorkspaceSourceDocumentCount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["codeBehindDocumentCount"] = (roslynProjection?.CodeBehindDocumentCount ?? 0).ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }

    private void EnsureStarted()
    {
        if (Volatile.Read(ref _started) == 0)
            throw new InvalidOperationException("Jazor.VueHost must be started before handling RPC requests.");
    }

    private async Task<DocumentSnapshot> ResolveJazorDocumentAsync(
        GetVirtualArtifactRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            return new DocumentSnapshot(
                request.DocumentPath,
                DocumentKind.Jazor,
                request.Text,
                request.Version);
        }

        var trackedDocument = await _workspaceStore.GetDocumentAsync(request.DocumentPath, cancellationToken);
        if (trackedDocument is not null)
            return trackedDocument;

        if (!File.Exists(request.DocumentPath))
        {
            throw new FileNotFoundException(
                $"Jazor document '{request.DocumentPath}' was not found in the workspace or on disk.",
                request.DocumentPath);
        }

        var text = await File.ReadAllTextAsync(request.DocumentPath, cancellationToken);
        return new DocumentSnapshot(
            request.DocumentPath,
            DocumentKind.Jazor,
            text,
            request.Version);
    }

    private async Task<DocumentSnapshot> ResolveJazorDocumentSnapshotAsync(
        string documentPath,
        CancellationToken cancellationToken)
    {
        var trackedDocument = await _workspaceStore.GetDocumentAsync(documentPath, cancellationToken);
        if (trackedDocument is not null)
            return trackedDocument;

        if (!File.Exists(documentPath))
        {
            return new DocumentSnapshot(
                documentPath,
                DocumentKind.Jazor,
                string.Empty,
                version: null);
        }

        var text = await File.ReadAllTextAsync(documentPath, cancellationToken);
        return new DocumentSnapshot(
            documentPath,
            DocumentKind.Jazor,
            text,
            version: null);
    }

    private async Task<AnalyzeJazorRequest> BuildAnalyzeJazorRequestAsync(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<DocumentSnapshot> relatedDocuments,
        SemanticContext? frontendContext,
        CancellationToken cancellationToken)
    {
        var resolvedDocuments = await ResolveRelatedDocumentsAsync(
            jazorDocument,
            relatedDocuments.Select(static document => document.DocumentPath).ToArray(),
            cancellationToken);
        var mergedDocuments = MergeRelatedDocuments(relatedDocuments, resolvedDocuments);
        var derivedDocumentCount = Math.Max(0, mergedDocuments.Count - relatedDocuments.Count);
        var effectiveFrontendContext = frontendContext
            ?? new SemanticContext(
                contextKind: "frontend",
                relatedDocuments: mergedDocuments,
                properties: CreateContextProperties(
                    jazorDocument.DocumentPath,
                    mergedDocuments,
                    explicitDocumentCount: relatedDocuments.Count,
                    derivedDocumentCount: derivedDocumentCount));

        return new AnalyzeJazorRequest(
            jazorDocument,
            mergedDocuments,
            effectiveFrontendContext);
    }

    private async Task<IReadOnlyList<DocumentSnapshot>> ResolveRelatedDocumentsAsync(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<string> explicitPaths,
        CancellationToken cancellationToken)
    {
        var parsed = _parser.Parse(jazorDocument.DocumentPath, jazorDocument.Text);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var candidatePaths = new LinkedHashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var explicitPath in explicitPaths)
        {
            if (!string.IsNullOrWhiteSpace(explicitPath))
                candidatePaths.Add(explicitPath);
        }

        foreach (var importDescriptor in parsed.Imports)
        {
            foreach (var candidate in VueHostWorkspaceResolver.GetImportPathCandidates(jazorDocument.DocumentPath, importDescriptor.Source))
                candidatePaths.Add(candidate);
        }

        foreach (var componentName in GetReferencedVueComponents(jazorDocument.Text))
        {
            if (VueHostWorkspaceResolver.TryResolveTrackedNearbyVueComponent(jazorDocument.DocumentPath, componentName, openDocuments, out var trackedNearby))
            {
                candidatePaths.Add(trackedNearby.AbsolutePath);
                continue;
            }

            if (VueHostWorkspaceResolver.TryResolveNearbyVueComponent(jazorDocument.DocumentPath, componentName, out var nearbyComponentPath, out _))
            {
                candidatePaths.Add(nearbyComponentPath);
                continue;
            }

            if (VueHostWorkspaceResolver.TryResolveTrackedVueComponent(jazorDocument.DocumentPath, componentName, openDocuments, out var tracked))
            {
                candidatePaths.Add(tracked.AbsolutePath);
                continue;
            }

            if (VueHostWorkspaceResolver.ResolveWorkspaceVueComponent(jazorDocument.DocumentPath, componentName, openDocuments, cancellationToken) is { } workspaceResolved)
            {
                candidatePaths.Add(workspaceResolved.AbsolutePath);
            }
        }

        foreach (var candidate in VueHostWorkspaceResolver.GetCoLocatedAssetPaths(jazorDocument.DocumentPath))
        {
            candidatePaths.Add(candidate);
        }

        var documents = new List<DocumentSnapshot>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var candidatePath in candidatePaths)
        {
            var document = await ResolveFrontendDocumentAsync(candidatePath, cancellationToken);
            if (document is null || !seen.Add(NormalizeComparablePath(document.DocumentPath)))
                continue;

            documents.Add(document);
        }

        return documents;
    }

    private async Task<DocumentSnapshot?> ResolveFrontendDocumentAsync(
        string candidatePath,
        CancellationToken cancellationToken)
    {
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var document = await VueHostWorkspaceResolver.ResolveDocumentAsync(candidatePath, openDocuments, cancellationToken);
        return document is { DocumentKind: DocumentKind.Vue or DocumentKind.JavaScript or DocumentKind.TypeScript }
            ? document
            : null;
    }

    private static IReadOnlyList<DocumentSnapshot> MergeRelatedDocuments(
        IReadOnlyList<DocumentSnapshot> explicitDocuments,
        IReadOnlyList<DocumentSnapshot> derivedDocuments)
    {
        var merged = new List<DocumentSnapshot>(explicitDocuments.Count + derivedDocuments.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var document in explicitDocuments.Concat(derivedDocuments))
        {
            if (!seen.Add(document.DocumentPath))
                continue;

            merged.Add(document);
        }

        return merged;
    }

    private static IReadOnlyList<ArtifactRecord> CreateFrontendSummaryArtifacts(
        IReadOnlyList<DocumentSnapshot> relatedDocuments)
        => relatedDocuments
            .Select(CreateFrontendSummaryArtifact)
            .ToArray();

    private static IReadOnlyList<ArtifactRecord> CreateRoslynProjectionArtifacts(
        string documentPath,
        FrontendRoslynProjectionContext? roslynProjection)
    {
        if (roslynProjection is null)
        {
            return Array.Empty<ArtifactRecord>();
        }

        return
        [
            new ArtifactRecord(
                artifactName: "virtual:" + documentPath + ".razor-projection.json",
                artifactKind: "razor-projection",
                content: ProtocolJsonSerializer.Serialize(new
                {
                    documentPath,
                    roslynProjection.ProjectionKind,
                    roslynProjection.ProjectedDocumentPath,
                    workspaceSourceDocumentCount = roslynProjection.WorkspaceSourceDocumentCount,
                    projectionSegments = roslynProjection.ProjectionMap.Segments.Select(static segment => new
                    {
                        segment.OriginalStart,
                        segment.OriginalLength,
                        segment.ProjectedStart,
                        segment.ProjectedLength,
                        segment.IsBidirectional
                    }).ToArray(),
                    sourceDocuments = roslynProjection.SourceDocuments.Select(static document => new
                    {
                        document.DocumentPath,
                        kind = document.DocumentKind.ToString(),
                        document.Version
                    }).ToArray()
                }),
                contentHash: null),
            new ArtifactRecord(
                artifactName: "virtual:" + documentPath + ".razor.g.cs",
                artifactKind: "razor-projected-csharp",
                content: roslynProjection.SourceText,
                contentHash: null)
        ];
    }

    private static ArtifactRecord CreateFrontendSummaryArtifact(DocumentSnapshot document)
    {
        var importedSources = GetImportedSources(document.Text);
        var exportedSymbols = GetExportedSymbols(document.Text);
        var referencedComponents = document.DocumentKind == DocumentKind.Vue
            ? GetReferencedVueComponents(document.Text)
            : Array.Empty<string>();

        return new ArtifactRecord(
            artifactName: "virtual:" + document.DocumentPath + ".frontend-summary.json",
            artifactKind: "frontend-summary",
            content: ProtocolJsonSerializer.Serialize(new
            {
                documentPath = document.DocumentPath,
                documentKind = document.DocumentKind.ToString(),
                lineCount = document.Text.Split('\n').Length,
                importCount = importedSources.Length,
                importedSources,
                exportedSymbols,
                referencedComponents,
                hasScriptSetup = document.DocumentKind == DocumentKind.Vue &&
                    document.Text.Contains("<script setup", StringComparison.OrdinalIgnoreCase)
            }),
            contentHash: null);
    }

    private static IEnumerable<string> GetImportPathCandidates(
        string jazorDocumentPath,
        string importSource)
    {
        if (!IsFrontendImport(importSource))
            yield break;

        if (Path.IsPathRooted(importSource))
        {
            foreach (var candidate in ExpandPathCandidates(importSource))
                yield return candidate;

            yield break;
        }

        var jazorDirectory = Path.GetDirectoryName(jazorDocumentPath);
        if (!string.IsNullOrWhiteSpace(jazorDirectory))
        {
            foreach (var candidate in ExpandPathCandidates(Path.Combine(jazorDirectory, importSource)))
                yield return candidate;
        }

        foreach (var candidate in ExpandPathCandidates(importSource))
            yield return candidate;
    }

    private static IEnumerable<string> ExpandPathCandidates(string documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
            yield break;

        yield return documentPath;

        if (string.IsNullOrWhiteSpace(Path.GetExtension(documentPath)))
        {
            yield return documentPath + ".vue";
            yield return documentPath + ".ts";
            yield return documentPath + ".js";
        }

        var slashNormalized = documentPath.Replace('\\', '/');
        if (!string.Equals(documentPath, slashNormalized, StringComparison.Ordinal))
            yield return slashNormalized;

        if (Path.IsPathRooted(documentPath))
        {
            var fullPath = Path.GetFullPath(documentPath);
            if (!string.Equals(documentPath, fullPath, StringComparison.OrdinalIgnoreCase))
                yield return fullPath;

            var fullSlashNormalized = fullPath.Replace('\\', '/');
            if (!string.Equals(fullPath, fullSlashNormalized, StringComparison.Ordinal))
                yield return fullSlashNormalized;
        }
    }

    private static IEnumerable<string> GetCoLocatedAssetPathCandidates(string jazorDocumentPath)
    {
        var documentDirectory = Path.GetDirectoryName(jazorDocumentPath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(jazorDocumentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(fileNameWithoutExtension))
        {
            yield break;
        }

        foreach (var extension in new[] { ".css", ".js", ".ts" })
        {
            yield return Path.Combine(documentDirectory, fileNameWithoutExtension + extension);
        }
    }

    private static IEnumerable<string> GetNearbyVueComponentPathCandidates(
        string jazorDocumentPath,
        string componentName)
    {
        var documentDirectory = Path.GetDirectoryName(jazorDocumentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(componentName))
        {
            yield break;
        }

        foreach (var directory in GetNearbyVueSearchDirectories(documentDirectory))
        {
            yield return Path.Combine(directory, componentName + ".vue");
        }
    }

    private async Task<IReadOnlyList<string>> GetNearbyTrackedVueComponentPathCandidatesAsync(
        string jazorDocumentPath,
        string componentName,
        CancellationToken cancellationToken)
    {
        var documentDirectory = Path.GetDirectoryName(jazorDocumentPath);
        if (string.IsNullOrWhiteSpace(documentDirectory) || string.IsNullOrWhiteSpace(componentName))
        {
            return Array.Empty<string>();
        }

        var nearbyDirectories = GetNearbyVueSearchDirectories(documentDirectory)
            .Select(NormalizeComparablePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        return openDocuments
            .Where(static document => document.DocumentKind == DocumentKind.Vue)
            .Where(document => string.Equals(
                Path.GetFileNameWithoutExtension(document.DocumentPath),
                componentName,
                StringComparison.Ordinal))
            .Where(document => nearbyDirectories.Contains(
                NormalizeComparablePath(Path.GetDirectoryName(document.DocumentPath) ?? string.Empty)))
            .Select(static document => document.DocumentPath)
            .ToArray();
    }

    private static IEnumerable<string> GetNearbyVueSearchDirectories(string documentDirectory)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var parentDirectory = GetParentDirectoryPath(documentDirectory);
        foreach (var directory in new[]
                 {
                     documentDirectory,
                     Path.Combine(documentDirectory, "Components"),
                     Path.Combine(documentDirectory, "components"),
                     parentDirectory,
                     parentDirectory is null ? null : Path.Combine(parentDirectory, "Components"),
                     parentDirectory is null ? null : Path.Combine(parentDirectory, "components")
                 })
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            var comparablePath = NormalizeComparablePath(directory);
            if (seen.Add(comparablePath))
            {
                yield return directory;
            }
        }
    }

    private static string? GetParentDirectoryPath(string documentDirectory)
    {
        if (Path.IsPathRooted(documentDirectory))
        {
            return Directory.GetParent(documentDirectory)?.FullName;
        }

        var normalized = documentDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (normalized.Length == 0)
        {
            return null;
        }

        return Path.GetDirectoryName(normalized);
    }

    private static bool IsFrontendImport(string importSource)
        => GetFrontendDocumentKind(importSource) is not null
            || importSource.StartsWith("./", StringComparison.Ordinal)
            || importSource.StartsWith("../", StringComparison.Ordinal)
            || importSource.StartsWith(".\\", StringComparison.Ordinal)
            || importSource.StartsWith("..\\", StringComparison.Ordinal);

    private static DocumentKind? GetFrontendDocumentKind(string documentPath)
        => VueHostWorkspaceResolver.GetFrontendDocumentKind(documentPath);

    private async Task<FrontendRoslynProjectionContext?> CreateRoslynProjectionContextAsync(
        DocumentSnapshot jazorDocument,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (jazorDocument.DocumentKind != DocumentKind.Jazor
            || string.IsNullOrWhiteSpace(jazorDocument.Text))
        {
            return null;
        }

        var parsed = _parser.Parse(jazorDocument.DocumentPath, jazorDocument.Text);
        var projection = _roslynCodeService.CreateProjection(jazorDocument, parsed);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var workspaceSourceDocuments = await _roslynCodeService.GetSourceDocumentsAsync(jazorDocument, openDocuments, cancellationToken);
        var componentSourceDocumentPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            NormalizeComparablePath(jazorDocument.DocumentPath)
        };
        foreach (var codeBehindPath in VueHostWorkspaceResolver.GetCoLocatedCodeBehindPaths(jazorDocument.DocumentPath))
        {
            componentSourceDocumentPaths.Add(NormalizeComparablePath(codeBehindPath));
        }

        var sourceDocuments = workspaceSourceDocuments
            .Where(document => componentSourceDocumentPaths.Contains(NormalizeComparablePath(document.DocumentPath)))
            .OrderBy(static document => document.DocumentKind == DocumentKind.Jazor ? 0 : 1)
            .ThenBy(static document => document.DocumentPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var codeBehindDocumentCount = sourceDocuments.Count(static document => document.DocumentKind == DocumentKind.CSharp);

        return new FrontendRoslynProjectionContext(
            ProjectionKind: GetProjectionKind(projection.ProjectedDocumentPath),
            ProjectedDocumentPath: projection.ProjectedDocumentPath,
            SourceText: projection.SourceText,
            ProjectionMap: projection.ProjectionMap,
            SourceDocuments: sourceDocuments,
            WorkspaceSourceDocumentCount: workspaceSourceDocuments.Count,
            CodeBehindDocumentCount: codeBehindDocumentCount);
    }

    private static string GetProjectionKind(string projectedDocumentPath)
    {
        if (projectedDocumentPath.EndsWith(".razor.g.cs", StringComparison.OrdinalIgnoreCase))
        {
            return "razor-design-time";
        }

        if (projectedDocumentPath.EndsWith(".inproc.g.cs", StringComparison.OrdinalIgnoreCase))
        {
            return "fallback";
        }

        return "unknown";
    }

    private static string NormalizeComparablePath(string documentPath)
        => VueHostWorkspaceResolver.NormalizePath(documentPath);

    private static string[] GetImportedSources(string text)
        => Regex.Matches(text, "(?:from\\s+[\"'](?<source>[^\"']+)[\"']|import\\s+[\"'](?<sourceOnly>[^\"']+)[\"'])", RegexOptions.Multiline)
            .Cast<Match>()
            .Select(static match => match.Groups["source"].Success
                ? match.Groups["source"].Value
                : match.Groups["sourceOnly"].Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string[] GetExportedSymbols(string text)
        => Regex.Matches(text, "export\\s+(?:const|let|var|function|class|interface|type)\\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Multiline)
            .Cast<Match>()
            .Select(static match => match.Groups["name"].Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string[] GetReferencedVueComponents(string text)
        => Regex.Matches(text, "<(?<name>[A-Z][A-Za-z0-9_]*)\\b", RegexOptions.Multiline)
            .Cast<Match>()
            .Select(static match => match.Groups["name"].Value)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private sealed class LinkedHashSet<T> where T : notnull
    {
        private readonly HashSet<T> _set;
        private readonly List<T> _items = [];

        public LinkedHashSet(IEqualityComparer<T> comparer)
        {
            _set = new HashSet<T>(comparer);
        }

        public void Add(T value)
        {
            if (_set.Add(value))
                _items.Add(value);
        }

        public IEnumerator<T> GetEnumerator()
            => _items.GetEnumerator();
    }

    private sealed record FrontendRoslynProjectionContext(
        string ProjectionKind,
        string ProjectedDocumentPath,
        string SourceText,
        ProjectionMap ProjectionMap,
        IReadOnlyList<DocumentSnapshot> SourceDocuments,
        int WorkspaceSourceDocumentCount,
        int CodeBehindDocumentCount);
}
