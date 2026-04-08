using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.Analysis;

public sealed class DelegateAnalysisRpcTransport : IAnalysisRpcTransport
{
    private readonly Func<RpcRequestEnvelope, CancellationToken, ValueTask<RpcResponseEnvelope>> _handler;

    public DelegateAnalysisRpcTransport(
        Func<RpcRequestEnvelope, CancellationToken, ValueTask<RpcResponseEnvelope>> handler)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    }

    public ValueTask<RpcResponseEnvelope> SendAsync(
        RpcRequestEnvelope request,
        CancellationToken cancellationToken)
        => _handler(request, cancellationToken);
}
