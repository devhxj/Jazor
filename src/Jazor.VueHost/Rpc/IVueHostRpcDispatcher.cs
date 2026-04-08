namespace Jazor.VueHost.Rpc;

public interface IVueHostRpcDispatcher
{
    Task<object?> DispatchAsync(
        string methodName,
        object? payload,
        CancellationToken cancellationToken);
}
