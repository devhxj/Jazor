using Jazor.Common.VueContracts.Protocol;
using SharedVueAnalysisRpcMethodNames = Jazor.Common.VueContracts.Protocol.VueAnalysisRpcMethodNames;

namespace Jazor.Vue;

public sealed class VueAnalysisRpcProcessor : IVueAnalysisRpcProcessor
{
    private readonly IVueAnalysisRpcService _service;

    public VueAnalysisRpcProcessor(IVueAnalysisRpcService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestLine))
        {
            throw new ArgumentException("Request line must be provided.", nameof(requestLine));
        }

        RpcRequestEnvelope? request = null;
        RpcResponseEnvelope response;
        try
        {
            request = VueAnalysisRpcSerializer.Deserialize<RpcRequestEnvelope>(requestLine)
                ?? throw new VueAnalysisRpcException("invalid_request", "RPC request payload could not be deserialized.");

            var result = request.Method switch
            {
                SharedVueAnalysisRpcMethodNames.AnalyzeJazor => await _service.AnalyzeJazorAsync(
                    DeserializeRequired<AnalyzeJazorRequest>(request.PayloadJson),
                    cancellationToken),
                _ => throw new VueAnalysisRpcException("unknown_method", $"Unknown Vue analysis RPC method '{request.Method}'.")
            };

            response = new RpcResponseEnvelope(
                id: request.Id,
                success: true,
                payloadJson: VueAnalysisRpcSerializer.Serialize(result),
                error: null);
        }
        catch (OperationCanceledException exception)
        {
            response = CreateErrorResponse(request?.Id, "cancelled", exception);
        }
        catch (VueAnalysisRpcException exception)
        {
            response = CreateErrorResponse(request?.Id, exception.Code, exception);
        }
        catch (Exception exception)
        {
            response = CreateErrorResponse(request?.Id, "internal_error", exception);
        }

        return VueAnalysisRpcSerializer.Serialize(response);
    }

    private static T DeserializeRequired<T>(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new VueAnalysisRpcException(
                "invalid_payload",
                string.Format("Expected RPC payload for '{0}', but received <null>.", typeof(T).FullName));
        }

        var value = VueAnalysisRpcSerializer.Deserialize<T>(payloadJson!);
        if (value is null)
        {
            throw new VueAnalysisRpcException(
                "invalid_payload",
                string.Format("Failed to deserialize RPC payload as '{0}'.", typeof(T).FullName));
        }

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
