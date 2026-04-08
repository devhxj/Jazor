namespace Jazor.VueHost.Rpc;

public interface IVueHostRpcProcessor
{
    Task<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken);
}
