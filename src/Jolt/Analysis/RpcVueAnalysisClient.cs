using System.Text.Json;
using Jazor.VueContracts.Protocol;
using Jolt.Rpc;
using SharedVueAnalysisRpcMethodNames = Jazor.VueContracts.Protocol.VueAnalysisRpcMethodNames;

namespace Jolt.Analysis;

public sealed class RpcVueAnalysisClient : IVueAnalysisClient
{
    private readonly IAnalysisRpcTransport _transport;

    public RpcVueAnalysisClient(IAnalysisRpcTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    public async ValueTask<AnalyzeJazorResponse> AnalyzeJazorAsync(
        AnalyzeJazorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var rpcRequest = new RpcRequestEnvelope(
            id: Guid.NewGuid().ToString("N"),
            method: SharedVueAnalysisRpcMethodNames.AnalyzeJazor,
            payloadJson: JoltRpcSerializer.Serialize(request));

        RpcResponseEnvelope rpcResponse;
        try
        {
            rpcResponse = await _transport.SendAsync(rpcRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            WriteFailureLog(rpcRequest.Id, rpcRequest.Method, "transport_failure", exception.Message, exception.GetType().FullName);
            throw;
        }

        if (!rpcResponse.Success)
        {
            var errorCode = rpcResponse.Error?.Code;
            var errorMessage = rpcResponse.Error?.Message ?? "VueAnalysis RPC call failed without an error payload.";
            WriteFailureLog(
                rpcRequest.Id,
                rpcRequest.Method,
                errorCode ?? "rpc_failure",
                errorMessage,
                rpcResponse.Error?.Details);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(errorCode)
                    ? errorMessage
                    : errorCode + ": " + errorMessage);
        }

        if (string.IsNullOrWhiteSpace(rpcResponse.PayloadJson))
        {
            WriteFailureLog(
                rpcRequest.Id,
                rpcRequest.Method,
                "empty_payload",
                "VueAnalysis RPC call returned an empty payload.",
                details: null);
            throw new InvalidOperationException("VueAnalysis RPC call returned an empty payload.");
        }

        AnalyzeJazorResponse? response;
        try
        {
            response = JoltRpcSerializer.Deserialize<AnalyzeJazorResponse>(rpcResponse.PayloadJson);
        }
        catch (Exception exception)
        {
            WriteFailureLog(rpcRequest.Id, rpcRequest.Method, "invalid_payload", exception.Message, exception.GetType().FullName);
            throw;
        }

        if (response is null)
        {
            WriteFailureLog(
                rpcRequest.Id,
                rpcRequest.Method,
                "invalid_payload",
                "VueAnalysis RPC response could not be deserialized.",
                details: null);
            throw new InvalidOperationException("VueAnalysis RPC response could not be deserialized.");
        }

        return response;
    }

    private static void WriteFailureLog(
        string requestId,
        string method,
        string errorCode,
        string errorMessage,
        string? details)
    {
        var payload = new
        {
            eventType = "analysisRpcFailure",
            timestamp = DateTimeOffset.UtcNow,
            requestId,
            method,
            errorCode,
            errorMessage,
            details
        };

        Console.Error.WriteLine(JsonSerializer.Serialize(payload));
    }
}
