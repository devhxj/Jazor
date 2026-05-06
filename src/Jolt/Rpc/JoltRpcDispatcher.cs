using Jazor.RazorVue.Protocol;
using SharedJoltRpcMethodNames = Jazor.RazorVue.Protocol.JoltRpcMethodNames;

namespace Jolt.Rpc;

public sealed class JoltRpcDispatcher : IJoltRpcDispatcher
{
    private readonly IJoltRpcService _rpcService;

    public JoltRpcDispatcher(IJoltRpcService rpcService)
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
            SharedJoltRpcMethodNames.Ping => await _rpcService.PingAsync(cancellationToken),
            SharedJoltRpcMethodNames.GetHostInfo => await _rpcService.GetHostInfoAsync(cancellationToken),
            SharedJoltRpcMethodNames.OpenDocument => await DispatchOpenDocumentAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.UpdateDocument => await DispatchUpdateDocumentAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.CloseDocument => await DispatchCloseDocumentAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.GetOpenDocuments => await _rpcService.GetOpenDocumentsAsync(cancellationToken),
            SharedJoltRpcMethodNames.GetVolarContext => await DispatchGetVolarContextAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.AnalyzeJazor => await DispatchAnalyzeJazorAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.GetVirtualArtifact => await DispatchGetVirtualArtifactAsync(payload, cancellationToken),
            SharedJoltRpcMethodNames.GetHotUpdatePlan => await DispatchGetHotUpdatePlanAsync(payload, cancellationToken),
            _ => throw new JoltRpcException("unknown_method", $"Unknown Jolt RPC method '{methodName}'.")
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

    private Task<GetVolarContextResponse> DispatchGetVolarContextAsync(
        object? payload,
        CancellationToken cancellationToken)
        => _rpcService.GetVolarContextAsync(
            RequirePayload<GetVolarContextRequest>(payload),
            cancellationToken);

    private Task<AnalyzeJazorResponse> DispatchAnalyzeJazorAsync(
        object? payload,
        CancellationToken cancellationToken)
        => _rpcService.AnalyzeJazorAsync(
            RequirePayload<AnalyzeJazorRequest>(payload),
            cancellationToken);

    private Task<GetVirtualArtifactResponse> DispatchGetVirtualArtifactAsync(
        object? payload,
        CancellationToken cancellationToken)
        => _rpcService.GetVirtualArtifactAsync(
            RequirePayload<GetVirtualArtifactRequest>(payload),
            cancellationToken);

    private Task<GetHotUpdatePlanResponse> DispatchGetHotUpdatePlanAsync(
        object? payload,
        CancellationToken cancellationToken)
        => _rpcService.GetHotUpdatePlanAsync(
            RequirePayload<GetHotUpdatePlanRequest>(payload),
            cancellationToken);

    private static T RequirePayload<T>(object? payload)
    {
        if (payload is T typedPayload)
            return typedPayload;

        throw new JoltRpcException(
            "invalid_payload",
            $"Expected RPC payload of type '{typeof(T).FullName}', but received '{payload?.GetType().FullName ?? "<null>"}'.");
    }
}

