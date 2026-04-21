namespace Jazor.VueContracts.Protocol;

public sealed class RpcRequestEnvelope
{
    public RpcRequestEnvelope(
        string id,
        string method,
        string? payloadJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
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
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
        Details = details;
    }

    public string Code { get; }

    public string Message { get; }

    public string? Details { get; }
}
