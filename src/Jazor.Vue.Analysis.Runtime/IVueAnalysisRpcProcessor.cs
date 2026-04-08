namespace Jazor.Vue.Analysis.Runtime;

public interface IVueAnalysisRpcProcessor
{
    ValueTask<string> ProcessAsync(
        string requestLine,
        CancellationToken cancellationToken);
}
