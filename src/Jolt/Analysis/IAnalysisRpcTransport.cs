using Jazor.RazorVue.Protocol;

namespace Jolt.Analysis;

public interface IAnalysisRpcTransport
{
    ValueTask<RpcResponseEnvelope> SendAsync(
        RpcRequestEnvelope request,
        CancellationToken cancellationToken);
}

