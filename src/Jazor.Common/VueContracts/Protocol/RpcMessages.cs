namespace Jazor.Common.VueContracts.Protocol;

public sealed class RpcRequestEnvelope
{
    public RpcRequestEnvelope(
        string id,
        string method,
        string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("RPC request id cannot be null or whitespace.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(method))
        {
            throw new ArgumentException("RPC method cannot be null or whitespace.", nameof(method));
        }

        Id = id;
        Method = method;
        PayloadJson = payloadJson;
    }

    public string Id { get; }

    public string Method { get; }

    public string? PayloadJson { get; }
}

public sealed class RpcResponseEnvelope
{
    public RpcResponseEnvelope(
        string? id,
        bool success,
        string? payloadJson,
        RpcErrorRecord? error)
    {
        Id = id;
        Success = success;
        PayloadJson = payloadJson;
        Error = error;
    }

    public string? Id { get; }

    public bool Success { get; }

    public string? PayloadJson { get; }

    public RpcErrorRecord? Error { get; }
}

public sealed class RpcErrorRecord
{
    public RpcErrorRecord(
        string code,
        string message,
        string? details)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("RPC error code cannot be null or whitespace.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("RPC error message cannot be null or whitespace.", nameof(message));
        }

        Code = code;
        Message = message;
        Details = details;
    }

    public string Code { get; }

    public string Message { get; }

    public string? Details { get; }
}
