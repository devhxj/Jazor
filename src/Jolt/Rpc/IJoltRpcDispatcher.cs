namespace Jolt.Rpc;

public interface IJoltRpcDispatcher
{
    Task<object?> DispatchAsync(
        string methodName,
        object? payload,
        CancellationToken cancellationToken);
}
