using ECMAScript.Internal.VueContracts.Protocol;

namespace Jolt.Analysis;

public sealed class DelegateAnalysisRpcTransport(
	Func<RpcRequestEnvelope, CancellationToken, ValueTask<RpcResponseEnvelope>> handler) : IAnalysisRpcTransport
{
    private readonly Func<RpcRequestEnvelope, CancellationToken, ValueTask<RpcResponseEnvelope>> _handler = handler ?? throw new ArgumentNullException(nameof(handler));

	public ValueTask<RpcResponseEnvelope> SendAsync(
        RpcRequestEnvelope request,
        CancellationToken cancellationToken)
        => _handler(request, cancellationToken);
}
