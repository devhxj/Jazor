using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Analysis;
using Jazor.VueHost.Frontend;
using Jazor.VueHost.Hosting;
using Jazor.VueHost.Rpc;
using Jazor.VueHost.Workspace;
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
        new HostCapabilityDescriptor(SharedVueHostRpcMethodNames.AnalyzeJazor, "Delegates .jazor analysis to the analysis client.")
    ];

    private readonly IVueHostWorkspaceStore _workspaceStore;
    private readonly IVueAnalysisClient _analysisClient;
    private int _started;

    public VueHostService(
        IVueHostWorkspaceStore workspaceStore,
        IVueAnalysisClient analysisClient)
    {
        _workspaceStore = workspaceStore ?? throw new ArgumentNullException(nameof(workspaceStore));
        _analysisClient = analysisClient ?? throw new ArgumentNullException(nameof(analysisClient));
    }

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Exchange(ref _started, 1);
        return ValueTask.CompletedTask;
    }

    public ValueTask StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Exchange(ref _started, 0);
        return ValueTask.CompletedTask;
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

        var relatedDocuments = await _workspaceStore.GetDocumentsAsync(
            request.RelatedDocumentPaths,
            cancellationToken);

        var semanticContext = new SemanticContext(
            contextKind: "frontend",
            relatedDocuments: relatedDocuments,
            properties: CreateContextProperties(request.DocumentPath, relatedDocuments));

        return new GetFrontendContextResponse(
            semanticContext,
            artifacts: Array.Empty<ArtifactRecord>());
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
        return _analysisClient.AnalyzeJazorAsync(request, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(
        CancellationToken cancellationToken)
    {
        EnsureStarted();
        return await _workspaceStore.GetOpenDocumentsAsync(cancellationToken);
    }

    private static IReadOnlyDictionary<string, string> CreateContextProperties(
        string documentPath,
        IReadOnlyList<DocumentSnapshot> relatedDocuments)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["documentPath"] = documentPath,
            ["relatedDocumentCount"] = relatedDocuments.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["provider"] = "Jazor.VueHost"
        };
    }

    private void EnsureStarted()
    {
        if (Volatile.Read(ref _started) == 0)
            throw new InvalidOperationException("Jazor.VueHost must be started before handling RPC requests.");
    }
}
