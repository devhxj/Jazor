using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Analysis;

public interface IAnalysisRpcTransport
{
    ValueTask<RpcResponseEnvelope> SendAsync(
        RpcRequestEnvelope request,
        CancellationToken cancellationToken);
}
