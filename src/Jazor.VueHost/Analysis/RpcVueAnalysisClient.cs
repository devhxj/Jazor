using Jazor.VueContracts.Protocol;
using Jazor.VueHost.Rpc;

namespace Jazor.VueHost.Analysis;

public sealed class RpcVueAnalysisClient : IVueAnalysisClient
{
    private readonly IAnalysisRpcTransport _transport;

    public RpcVueAnalysisClient(IAnalysisRpcTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    public async ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var rpcRequest = new RpcRequestEnvelope(
            id: Guid.NewGuid().ToString("N"),
            method: Jazor.VueContracts.Protocol.VueAnalysisRpcMethodNames.AnalyzeJazor,
            payloadJson: VueHostRpcSerializer.Serialize(request));

        var rpcResponse = await _transport.SendAsync(rpcRequest, cancellationToken);
        if (!rpcResponse.Success)
        {
            var errorCode = rpcResponse.Error?.Code;
            var errorMessage = rpcResponse.Error?.Message ?? "VueAnalysis RPC call failed without an error payload.";
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(errorCode)
                    ? errorMessage
                    : errorCode + ": " + errorMessage);
        }

        if (string.IsNullOrWhiteSpace(rpcResponse.PayloadJson))
            throw new InvalidOperationException("VueAnalysis RPC call returned an empty payload.");

        var response = VueHostRpcSerializer.Deserialize<AnalyzeJazorResponse>(rpcResponse.PayloadJson);
        if (response is null)
            throw new InvalidOperationException("VueAnalysis RPC response could not be deserialized.");

        return response;
    }
}
