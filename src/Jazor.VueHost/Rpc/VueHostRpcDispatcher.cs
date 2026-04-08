using Jazor.VueContracts.Protocol;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

namespace Jazor.VueHost.Rpc;

public sealed class VueHostRpcDispatcher : IVueHostRpcDispatcher
{
    private readonly IVueHostRpcService _rpcService;

    public VueHostRpcDispatcher(IVueHostRpcService rpcService)
    {
        _rpcService = rpcService ?? throw new ArgumentNullException(nameof(rpcService));
    }

    public async Task<object?> DispatchAsync(
        string methodName,
        object? payload,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(methodName);

        return methodName switch
        {
            SharedVueHostRpcMethodNames.Ping => await _rpcService.PingAsync(cancellationToken),
            SharedVueHostRpcMethodNames.GetHostInfo => await _rpcService.GetHostInfoAsync(cancellationToken),
            SharedVueHostRpcMethodNames.OpenDocument => await DispatchOpenDocumentAsync(payload, cancellationToken),
            SharedVueHostRpcMethodNames.UpdateDocument => await DispatchUpdateDocumentAsync(payload, cancellationToken),
            SharedVueHostRpcMethodNames.CloseDocument => await DispatchCloseDocumentAsync(payload, cancellationToken),
            SharedVueHostRpcMethodNames.GetOpenDocuments => await _rpcService.GetOpenDocumentsAsync(cancellationToken),
            SharedVueHostRpcMethodNames.GetFrontendContext => await DispatchGetFrontendContextAsync(payload, cancellationToken),
            SharedVueHostRpcMethodNames.AnalyzeJazor => await DispatchAnalyzeJazorAsync(payload, cancellationToken),
            _ => throw new VueHostRpcException("unknown_method", $"Unknown Jazor.VueHost RPC method '{methodName}'.")
        };
    }

    private async Task<object?> DispatchOpenDocumentAsync(object? payload, CancellationToken cancellationToken)
    {
        await _rpcService.OpenDocumentAsync(RequirePayload<DocumentSnapshot>(payload), cancellationToken);
        return null;
    }

    private async Task<object?> DispatchUpdateDocumentAsync(object? payload, CancellationToken cancellationToken)
    {
        await _rpcService.UpdateDocumentAsync(RequirePayload<DocumentSnapshot>(payload), cancellationToken);
        return null;
    }

    private async Task<object?> DispatchCloseDocumentAsync(object? payload, CancellationToken cancellationToken)
    {
        await _rpcService.CloseDocumentAsync(RequirePayload<string>(payload), cancellationToken);
        return null;
    }

    private Task<GetFrontendContextResponse> DispatchGetFrontendContextAsync(
        object? payload,
        CancellationToken cancellationToken)
        => _rpcService.GetFrontendContextAsync(
            RequirePayload<GetFrontendContextRequest>(payload),
            cancellationToken);

    private Task<AnalyzeJazorResponse> DispatchAnalyzeJazorAsync(
        object? payload,
        CancellationToken cancellationToken)
        => _rpcService.AnalyzeJazorAsync(
            RequirePayload<AnalyzeJazorRequest>(payload),
            cancellationToken);

    private static T RequirePayload<T>(object? payload)
    {
        if (payload is T typedPayload)
            return typedPayload;

        throw new VueHostRpcException(
            "invalid_payload",
            $"Expected RPC payload of type '{typeof(T).FullName}', but received '{payload?.GetType().FullName ?? "<null>"}'.");
    }
}
