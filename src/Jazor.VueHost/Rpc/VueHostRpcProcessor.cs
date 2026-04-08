using Jazor.VueContracts.Protocol;
using SharedVueHostRpcMethodNames = Jazor.VueContracts.Protocol.VueHostRpcMethodNames;

namespace Jazor.VueHost.Rpc;

public sealed class VueHostRpcProcessor : IVueHostRpcProcessor
{
    private readonly IVueHostRpcDispatcher _dispatcher;

    public VueHostRpcProcessor(IVueHostRpcDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestLine);

        RpcRequestEnvelope? request = null;
        RpcResponseEnvelope response;
        try
        {
            request = VueHostRpcSerializer.Deserialize<RpcRequestEnvelope>(requestLine)
                ?? throw new VueHostRpcException("invalid_request", "RPC request payload could not be deserialized.");

            var payload = DeserializePayload(request.Method, request.PayloadJson);
            var result = await _dispatcher.DispatchAsync(request.Method, payload, cancellationToken);

            response = new RpcResponseEnvelope(
                id: request.Id,
                success: true,
                payloadJson: result is null ? null : VueHostRpcSerializer.Serialize(result),
                error: null);
        }
        catch (OperationCanceledException exception)
        {
            response = CreateErrorResponse(request?.Id, "cancelled", exception);
        }
        catch (VueHostRpcException exception)
        {
            response = CreateErrorResponse(request?.Id, exception.Code, exception);
        }
        catch (Exception exception)
        {
            response = CreateErrorResponse(request?.Id, "internal_error", exception);
        }

        return VueHostRpcSerializer.Serialize(response);
    }

    private static object? DeserializePayload(string methodName, string? payloadJson)
    {
        return methodName switch
        {
            SharedVueHostRpcMethodNames.Ping => null,
            SharedVueHostRpcMethodNames.GetHostInfo => null,
            SharedVueHostRpcMethodNames.OpenDocument => DeserializeRequired<DocumentSnapshot>(payloadJson),
            SharedVueHostRpcMethodNames.UpdateDocument => DeserializeRequired<DocumentSnapshot>(payloadJson),
            SharedVueHostRpcMethodNames.CloseDocument => DeserializeRequired<string>(payloadJson),
            SharedVueHostRpcMethodNames.GetOpenDocuments => null,
            SharedVueHostRpcMethodNames.GetFrontendContext => DeserializeRequired<GetFrontendContextRequest>(payloadJson),
            SharedVueHostRpcMethodNames.AnalyzeJazor => DeserializeRequired<AnalyzeJazorRequest>(payloadJson),
            SharedVueHostRpcMethodNames.GetVirtualArtifact => DeserializeRequired<GetVirtualArtifactRequest>(payloadJson),
            SharedVueHostRpcMethodNames.GetHotUpdatePlan => DeserializeRequired<GetHotUpdatePlanRequest>(payloadJson),
            _ => throw new VueHostRpcException("unknown_method", $"Unknown Jazor.VueHost RPC method '{methodName}'.")
        };
    }

    private static T DeserializeRequired<T>(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new VueHostRpcException(
                "invalid_payload",
                $"Expected RPC payload for '{typeof(T).FullName}', but received <null>.");

        var value = VueHostRpcSerializer.Deserialize<T>(payloadJson);
        if (value is null)
            throw new VueHostRpcException(
                "invalid_payload",
                $"Failed to deserialize RPC payload as '{typeof(T).FullName}'.");

        return value;
    }

    private static RpcResponseEnvelope CreateErrorResponse(
        string? requestId,
        string errorCode,
        Exception exception)
    {
        return new RpcResponseEnvelope(
            id: requestId,
            success: false,
            payloadJson: null,
            error: new RpcErrorRecord(
                code: errorCode,
                message: exception.Message,
                details: exception.GetType().FullName));
    }
}
