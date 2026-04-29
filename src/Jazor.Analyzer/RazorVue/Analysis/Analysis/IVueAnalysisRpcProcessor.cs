namespace Jazor.Vue;

public interface IVueAnalysisRpcProcessor
{
    ValueTask<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken);
}

