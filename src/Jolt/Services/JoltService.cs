using Jolt.Analysis;
using Jolt.Volar;
using Jolt.Volar.Deno.Hosting;
using Jolt.Hosting;
using Jazor.Common.VueContracts.Protocol;
using Jolt.Roslyn.InProc;
using Jolt.Rpc;
using Jolt.VirtualDocuments.Mapping;
using Jolt.Workspace;
using System.Text.RegularExpressions;
using SharedJoltRpcMethodNames = Jazor.Common.VueContracts.Protocol.JoltRpcMethodNames;
using Jazor.Vue;

namespace Jolt.Services;

public sealed class JoltService : IJoltService, IJoltRpcService, IVolarContextProvider
{
    private static readonly IReadOnlyList<HostCapabilityDescriptor> HostCapabilities =
    [
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.Ping, "Lightweight liveness probe."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.GetHostInfo, "Returns protocol version and advertised capabilities."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.OpenDocument, "Opens or tracks a document in the workspace."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.UpdateDocument, "Updates an already tracked document."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.CloseDocument, "Stops tracking a document in the workspace."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.GetOpenDocuments, "Returns currently tracked workspace documents."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.GetVolarContext, "Returns frontend semantic context for a .jazor document."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.AnalyzeJazor, "Delegates .jazor analysis to the analysis client."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.GetVirtualArtifact, "Compiles a .jazor document and returns one requested virtual artifact."),
        new HostCapabilityDescriptor(SharedJoltRpcMethodNames.GetHotUpdatePlan, "Returns host-driven hot update impact for a changed workspace document.")
    ];

    private readonly IJoltWorkspaceStore _workspaceStore;
    private readonly IVueAnalysisClient _analysisClient;
    private readonly IDenoVolarHost _denoVolarHost;
    private readonly JazorRelatedDocumentResolver _relatedDocumentResolver;
    private readonly InProcRoslynCodeService _roslynCodeService = new();
    private readonly FallbackJazorAnalysisService _fallbackAnalysisService = new();
    private readonly JazorVueParser _parser = new();
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private int _started;

    public JoltService(
        IJoltWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient)
        : this(workspaceStore, analysisClient, denoVolarHost: null)
    {
    }

    internal JoltService(
        IJoltWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient,
        IDenoVolarHost? denoVolarHost)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _analysisClient = analysisClient ?? throw new ArgumentNullException(nameof(analysisClient));
        _denoVolarHost = denoVolarHost ?? new DenoVolarHost(new DenoVolarHostOptions());
        _relatedDocumentResolver = new JazorRelatedDocumentResolver(_workspaceStore);
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        await _lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Volatile.Read(ref _started) == 1)
            {
                return;
            }

            await _denoVolarHost.StartAsync(cancellationToken);
            Volatile.Write(ref _started, 1);
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        await _lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (Volatile.Read(ref _started) == 0)
            {
                return;
            }

            try
            {
                await _denoVolarHost.StopAsync(cancellationToken);
            }
            finally
            {
                Volatile.Write(ref _started, 0);
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
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
            hostName: "Jolt",
            protocolVersion: "0.1",
            capabilities: HostCapabilities));
    }

    public async Task OpenDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken)
    {
        EnsureStarted();
        await _workspaceStore.UpsertDocumentAsync(documentSnapshot, cancellationToken);
        JoltWorkspaceResolver.InvalidatePath(documentSnapshot.DocumentPath);
    }

    public Task UpdateDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken)
        => OpenDocumentAsync(documentSnapshot, cancellationToken);

    public async Task CloseDocumentAsync(string documentPath, CancellationToken cancellationToken)
    {
        EnsureStarted();
        await _workspaceStore.RemoveDocumentAsync(documentPath, cancellationToken);
        JoltWorkspaceResolver.InvalidatePath(documentPath);
    }

    public async Task<GetVolarContextResponse> GetVolarContextAsync(
        GetVolarContextRequest request,
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
        artifacts.AddRange(CreateVolarSummaryArtifacts(relatedDocuments));

        return new GetVolarContextResponse(
            semanticContext,
            artifacts);
    }

    ValueTask<GetVolarContextResponse> IVolarContextProvider.GetVolarContextAsync(
        GetVolarContextRequest request,
        CancellationToken cancellationToken)
        => new(GetVolarContextAsync(request, cancellationToken));

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
            volarContext: null,
            cancellationToken);
        var analysisResponse = await _analysisClient.AnalyzeWithFallbackAsync(
            _fallbackAnalysisService,
            analysisRequest,
            cancellationToken,
            acceptResponse: response => response.FindArtifact(request.ArtifactKind) is not null);
        var artifact = analysisResponse.FindArtifact(request.ArtifactKind);

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

    private async Task<AnalyzeJazorResponse> AnalyzeJazorCoreAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        var hydratedRequest = await BuildAnalyzeJazorRequestAsync(
            request.JazorDocument,
            request.RelatedDocuments,
            request.VolarContext,
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

        if (request.DocumentKind is not (DocumentKind.Vue or DocumentKind.TypeScript or DocumentKind.JavaScript or DocumentKind.Css))
        {
            return new GetHotUpdatePlanResponse(
                requiresFullReload: true,
                affectedDocumentPaths: Array.Empty<string>(),
                reason: "unsupported-document-kind");
        }

        var normalizedChangedPath = NormalizeComparablePath(request.DocumentPath);
        if (Path.IsPathRooted(request.DocumentPath))
        {
            _ = JoltWorkspaceResolver.GetRequiredOwningProjectRoot(request.DocumentPath);
        }

        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var affectedJazorDocuments = new List<string>();

        // Hot update 影响面必须按 owning project 过滤；否则一个项目的前端文件变化
        // 会把同 solution 下其他项目的 Jazor 页面也误判成受影响。
        foreach (var document in openDocuments.Where(document =>
                     document.DocumentKind == DocumentKind.Jazor
                     && JoltWorkspaceResolver.IsInSameProjectScope(request.DocumentPath, document.DocumentPath)))
        {
            var referencesChangedPath = await _relatedDocumentResolver.ReferencesPathAsync(
                document,
                normalizedChangedPath,
                Array.Empty<string>(),
                openDocuments,
                cancellationToken);
            if (referencesChangedPath)
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
        VolarRoslynProjectionContext? roslynProjection = null,
        int explicitDocumentCount = 0,
        int derivedDocumentCount = 0)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["documentPath"] = documentPath,
            ["relatedDocumentCount"] = relatedDocuments.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["explicitDocumentCount"] = explicitDocumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["derivedDocumentCount"] = derivedDocumentCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["provider"] = "Jolt",
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
            throw new InvalidOperationException("Jolt must be started before handling RPC requests.");
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
        SemanticContext? volarContext,
        CancellationToken cancellationToken)
    {
        var resolvedDocuments = await ResolveRelatedDocumentsAsync(
            jazorDocument,
            relatedDocuments.Select(static document => document.DocumentPath).ToArray(),
            cancellationToken);
        var mergedDocuments = MergeRelatedDocuments(relatedDocuments, resolvedDocuments);
        var derivedDocumentCount = Math.Max(0, mergedDocuments.Count - relatedDocuments.Count);
        var effectiveVolarContext = volarContext
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
            effectiveVolarContext);
    }

    private async Task<IReadOnlyList<DocumentSnapshot>> ResolveRelatedDocumentsAsync(
        DocumentSnapshot jazorDocument,
        IReadOnlyList<string> explicitPaths,
        CancellationToken cancellationToken)
        => await _relatedDocumentResolver.ResolveAsync(jazorDocument, explicitPaths, cancellationToken);

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

    private static IReadOnlyList<ArtifactRecord> CreateVolarSummaryArtifacts(
        IReadOnlyList<DocumentSnapshot> relatedDocuments)
        => relatedDocuments
            .Select(CreateVolarSummaryArtifact)
            .ToArray();

    private static IReadOnlyList<ArtifactRecord> CreateRoslynProjectionArtifacts(
        string documentPath,
        VolarRoslynProjectionContext? roslynProjection)
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

    private static ArtifactRecord CreateVolarSummaryArtifact(DocumentSnapshot document)
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

    private async Task<VolarRoslynProjectionContext?> CreateRoslynProjectionContextAsync(
        DocumentSnapshot jazorDocument,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (jazorDocument.DocumentKind != DocumentKind.Jazor
            || string.IsNullOrWhiteSpace(jazorDocument.Text))
        {
            return null;
        }

        var parsed = JazorVueParser.Parse(jazorDocument.DocumentPath, jazorDocument.Text);
        var projection = _roslynCodeService.CreateProjection(jazorDocument, parsed);
        var openDocuments = await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
        var workspaceSourceDocuments = await _roslynCodeService.GetSourceDocumentsAsync(jazorDocument, openDocuments, cancellationToken);
        var componentSourceDocumentPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            NormalizeComparablePath(jazorDocument.DocumentPath)
        };
        foreach (var codeBehindPath in JoltWorkspaceResolver.GetCoLocatedCodeBehindPaths(jazorDocument.DocumentPath))
        {
            componentSourceDocumentPaths.Add(NormalizeComparablePath(codeBehindPath));
        }

        var sourceDocuments = workspaceSourceDocuments
            .Where(document => componentSourceDocumentPaths.Contains(NormalizeComparablePath(document.DocumentPath)))
            .OrderBy(static document => document.DocumentKind == DocumentKind.Jazor ? 0 : 1)
            .ThenBy(static document => document.DocumentPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var codeBehindDocumentCount = sourceDocuments.Count(static document => document.DocumentKind == DocumentKind.CSharp);

        return new VolarRoslynProjectionContext(
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
        => JoltWorkspaceResolver.NormalizePath(documentPath);

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

    private sealed record VolarRoslynProjectionContext(
        string ProjectionKind,
        string ProjectedDocumentPath,
        string SourceText,
        ProjectionMap ProjectionMap,
        IReadOnlyList<DocumentSnapshot> SourceDocuments,
        int WorkspaceSourceDocumentCount,
        int CodeBehindDocumentCount);
}
