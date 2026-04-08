using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Rpc;

public interface IVueHostRpcService
{
    Task<PingResponse> PingAsync(CancellationToken cancellationToken);

    Task<GetHostInfoResponse> GetHostInfoAsync(CancellationToken cancellationToken);

    Task OpenDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken);

    Task UpdateDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken);

    Task CloseDocumentAsync(string documentPath, CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(CancellationToken cancellationToken);

    Task<GetFrontendContextResponse> GetFrontendContextAsync(
        GetFrontendContextRequest request,
        CancellationToken cancellationToken);

    Task<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);
}
