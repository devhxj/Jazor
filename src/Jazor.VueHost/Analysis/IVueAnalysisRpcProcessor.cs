namespace Jazor.VueHost.Analysis;

public interface IVueAnalysisRpcProcessor
{
    ValueTask<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken);
}
