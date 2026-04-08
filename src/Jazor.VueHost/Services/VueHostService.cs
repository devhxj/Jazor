using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Frontend;
using Jazor.VueHost.Frontend.Deno.Hosting;
using Jazor.VueHost.Hosting;
using Jazor.VueHost.Rpc;
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
    private readonly IDenoFrontendHost _denoFrontendHost;
    private readonly FallbackJazorAnalysisService _fallbackAnalysisService = new();
    private int _started;

    public VueHostService(
        IVueHostWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient)
        : this(workspaceStore, analysisClient, denoFrontendHost: null)
    {
    }

    internal VueHostService(
        IVueHostWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient,
        IDenoFrontendHost? denoFrontendHost)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _analysisClient = analysisClient ?? throw new ArgumentNullException(nameof(analysisClient));
        _denoFrontendHost = denoFrontendHost ?? new DenoFrontendHost(new DenoFrontendHostOptions());
    }

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _denoFrontendHost.StartAsync(cancellationToken);
        Interlocked.Exchange(ref _started, 1);
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Exchange(ref _started, 0);
        await _denoFrontendHost.StopAsync(cancellationToken);
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
    }

    public Task UpdateDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken)
        => OpenDocumentAsync(documentSnapshot, cancellationToken);

    public async Task CloseDocumentAsync(string documentPath, CancellationToken cancellationToken)
    {
        EnsureStarted();
        await _workspaceStore.RemoveDocumentAsync(documentPath, cancellationToken);
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

        var semanticContext = new SemanticContext(
            contextKind: "frontend",
            relatedDocuments: relatedDocuments,
            properties: CreateContextProperties(
                request.DocumentPath,
                relatedDocuments,
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
                    }).ToArray()
                }),
                contentHash: null)
        };
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
                    .Distinct(StringComparer.Ordinal))
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
        var candidatePaths = new LinkedHashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var explicitPath in explicitPaths)
        {
            if (!string.IsNullOrWhiteSpace(explicitPath))
                candidatePaths.Add(explicitPath);
        }

        var importAnalysis = await _fallbackAnalysisService.AnalyzeJazorAsync(
            new AnalyzeJazorRequest(
                jazorDocument,
                relatedDocuments: Array.Empty<DocumentSnapshot>(),
                frontendContext: null),
            cancellationToken);

        foreach (var importDescriptor in importAnalysis.Imports)
        {
            foreach (var candidate in GetImportPathCandidates(jazorDocument.DocumentPath, importDescriptor.Source))
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
        foreach (var probePath in ExpandPathCandidates(candidatePath))
        {
            var normalizedProbePath = NormalizeComparablePath(probePath);
            var trackedDocument = openDocuments.FirstOrDefault(document =>
                string.Equals(
                    NormalizeComparablePath(document.DocumentPath),
                    normalizedProbePath,
                    StringComparison.OrdinalIgnoreCase));
            if (trackedDocument is not null)
                return trackedDocument;
        }

        foreach (var probePath in ExpandPathCandidates(candidatePath))
        {
            if (!File.Exists(probePath))
                continue;

            var documentKind = GetFrontendDocumentKind(probePath);
            if (documentKind is null)
                return null;

            var text = await File.ReadAllTextAsync(probePath, cancellationToken);
            return new DocumentSnapshot(
                probePath,
                documentKind.Value,
                text,
                version: null);
        }

        return null;
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

    private static bool IsFrontendImport(string importSource)
        => GetFrontendDocumentKind(importSource) is not null
            || importSource.StartsWith("./", StringComparison.Ordinal)
            || importSource.StartsWith("../", StringComparison.Ordinal)
            || importSource.StartsWith(".\\", StringComparison.Ordinal)
            || importSource.StartsWith("..\\", StringComparison.Ordinal);

    private static DocumentKind? GetFrontendDocumentKind(string documentPath)
        => Path.GetExtension(documentPath).ToLowerInvariant() switch
        {
            ".vue" => DocumentKind.Vue,
            ".js" => DocumentKind.JavaScript,
            ".ts" => DocumentKind.TypeScript,
            _ => null
        };

    private static string NormalizeComparablePath(string documentPath)
    {
        var slashNormalized = documentPath.Replace('\\', '/');
        var prefix = string.Empty;
        var workingPath = slashNormalized;

        if (workingPath.Length >= 2 && workingPath[1] == ':')
        {
            prefix = workingPath[..2];
            workingPath = workingPath[2..];
        }
        else if (workingPath.StartsWith("/", StringComparison.Ordinal))
        {
            prefix = "/";
            workingPath = workingPath.TrimStart('/');
        }

        var segments = new Stack<string>();
        foreach (var segment in workingPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (segments.Count > 0 && !string.Equals(segments.Peek(), "..", StringComparison.Ordinal))
                {
                    segments.Pop();
                }
                else if (prefix.Length == 0)
                {
                    segments.Push(segment);
                }

                continue;
            }

            segments.Push(segment);
        }

        var normalized = string.Join("/", segments.Reverse());
        if (prefix.Length == 0)
            return normalized;

        if (normalized.Length == 0)
            return prefix;

        if (prefix == "/")
            return prefix + normalized;

        return prefix + "/" + normalized;
    }

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
}
