using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Rpc;

public interface IJoltRpcService
{
    Task<PingResponse> PingAsync(CancellationToken cancellationToken);

    Task<GetHostInfoResponse> GetHostInfoAsync(CancellationToken cancellationToken);

    Task OpenDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken);

    Task UpdateDocumentAsync(DocumentSnapshot documentSnapshot, CancellationToken cancellationToken);

    Task CloseDocumentAsync(string documentPath, CancellationToken cancellationToken);

    Task<IReadOnlyList<DocumentSnapshot>> GetOpenDocumentsAsync(CancellationToken cancellationToken);

    Task<GetVolarContextResponse> GetVolarContextAsync(
        GetVolarContextRequest request,
        CancellationToken cancellationToken);

    Task<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken);

    Task<GetVirtualArtifactResponse> GetVirtualArtifactAsync(
        GetVirtualArtifactRequest request,
        CancellationToken cancellationToken);

    Task<GetHotUpdatePlanResponse> GetHotUpdatePlanAsync(
        GetHotUpdatePlanRequest request,
        CancellationToken cancellationToken);
}
