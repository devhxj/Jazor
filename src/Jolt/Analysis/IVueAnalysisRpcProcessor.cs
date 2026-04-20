namespace Jolt.Analysis;

public interface IVueAnalysisRpcProcessor
{
    ValueTask<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken);
}
