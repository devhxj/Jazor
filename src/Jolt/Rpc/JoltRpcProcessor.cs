using Jazor.VueContracts.Protocol;
using SharedJoltRpcMethodNames = Jazor.VueContracts.Protocol.JoltRpcMethodNames;

namespace Jolt.Rpc;

public sealed class JoltRpcProcessor : IJoltRpcProcessor
{
    private readonly IJoltRpcDispatcher _dispatcher;

    public JoltRpcProcessor(IJoltRpcDispatcher dispatcher)
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
            request = JoltRpcSerializer.Deserialize<RpcRequestEnvelope>(requestLine)
                ?? throw new JoltRpcException("invalid_request", "RPC request payload could not be deserialized.");

            var payload = DeserializePayload(request.Method, request.PayloadJson);
            var result = await _dispatcher.DispatchAsync(request.Method, payload, cancellationToken);

            response = new RpcResponseEnvelope(
                id: request.Id,
                success: true,
                payloadJson: result is null ? null : JoltRpcSerializer.Serialize(result),
                error: null);
        }
        catch (OperationCanceledException exception)
        {
            response = CreateErrorResponse(request?.Id, "cancelled", exception);
        }
        catch (JoltRpcException exception)
        {
            response = CreateErrorResponse(request?.Id, exception.Code, exception);
        }
        catch (Exception exception)
        {
            response = CreateErrorResponse(request?.Id, "internal_error", exception);
        }

        return JoltRpcSerializer.Serialize(response);
    }

    private static object? DeserializePayload(string methodName, string? payloadJson)
    {
        return methodName switch
        {
            SharedJoltRpcMethodNames.Ping => null,
            SharedJoltRpcMethodNames.GetHostInfo => null,
            SharedJoltRpcMethodNames.OpenDocument => DeserializeRequired<DocumentSnapshot>(payloadJson),
            SharedJoltRpcMethodNames.UpdateDocument => DeserializeRequired<DocumentSnapshot>(payloadJson),
            SharedJoltRpcMethodNames.CloseDocument => DeserializeRequired<string>(payloadJson),
            SharedJoltRpcMethodNames.GetOpenDocuments => null,
            SharedJoltRpcMethodNames.GetFrontendContext => DeserializeRequired<GetFrontendContextRequest>(payloadJson),
            SharedJoltRpcMethodNames.AnalyzeJazor => DeserializeRequired<AnalyzeJazorRequest>(payloadJson),
            SharedJoltRpcMethodNames.GetVirtualArtifact => DeserializeRequired<GetVirtualArtifactRequest>(payloadJson),
            SharedJoltRpcMethodNames.GetHotUpdatePlan => DeserializeRequired<GetHotUpdatePlanRequest>(payloadJson),
            _ => throw new JoltRpcException("unknown_method", $"Unknown Jolt RPC method '{methodName}'.")
        };
    }

    private static T DeserializeRequired<T>(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new JoltRpcException(
                "invalid_payload",
                $"Expected RPC payload for '{typeof(T).FullName}', but received <null>.");

        var value = JoltRpcSerializer.Deserialize<T>(payloadJson);
        if (value is null)
            throw new JoltRpcException(
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
