namespace Jolt.Rpc;

public interface IJoltRpcProcessor
{
    Task<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken);
}
