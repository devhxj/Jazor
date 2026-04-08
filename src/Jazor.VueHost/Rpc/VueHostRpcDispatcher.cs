using Jazor.VueContracts.Protocol;

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
            VueHostRpcMethodNames.Ping => await _rpcService.PingAsync(cancellationToken),
            VueHostRpcMethodNames.GetHostInfo => await _rpcService.GetHostInfoAsync(cancellationToken),
            VueHostRpcMethodNames.OpenDocument => await DispatchOpenDocumentAsync(payload, cancellationToken),
            VueHostRpcMethodNames.UpdateDocument => await DispatchUpdateDocumentAsync(payload, cancellationToken),
            VueHostRpcMethodNames.CloseDocument => await DispatchCloseDocumentAsync(payload, cancellationToken),
            VueHostRpcMethodNames.GetOpenDocuments => await _rpcService.GetOpenDocumentsAsync(cancellationToken),
            VueHostRpcMethodNames.GetFrontendContext => await DispatchGetFrontendContextAsync(payload, cancellationToken),
            VueHostRpcMethodNames.AnalyzeJazor => await DispatchAnalyzeJazorAsync(payload, cancellationToken),
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
