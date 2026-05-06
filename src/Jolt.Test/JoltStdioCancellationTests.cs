using Jazor.RazorVue.Protocol;
using Jolt.Analysis;
using Jolt.Rpc;

namespace Jolt.Test;

[TestClass]
public sealed class JoltStdioCancellationTests
{
    [TestMethod]
    public async Task StdioJoltRpcServer_RunAsync_CancellationDuringRead_ExitsCleanly()
    {
        var server = new StdioJoltRpcServer(new NoOpJoltRpcProcessor());
        using var cancellationSource = new CancellationTokenSource();

        var runTask = server.RunAsync(new BlockingTextReader(), new StringWriter(), cancellationSource.Token);
        cancellationSource.CancelAfter(100);

        await runTask;
    }

    [TestMethod]
    public async Task StdioVueAnalysisRpcServer_RunAsync_CancellationDuringRead_ExitsCleanly()
    {
        var server = new StdioVueAnalysisRpcServer(new NoOpVueAnalysisRpcProcessor());
        using var cancellationSource = new CancellationTokenSource();

        var runTask = server.RunAsync(new BlockingTextReader(), new StringWriter(), cancellationSource.Token);
        cancellationSource.CancelAfter(100);

        await runTask;
    }

    private sealed class BlockingTextReader : TextReader
    {
        public override Task<string?> ReadLineAsync()
            => Task.FromException<string?>(new InvalidOperationException("Expected cancellable ReadLineAsync overload."));

        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return null;
        }
    }

    private sealed class NoOpJoltRpcProcessor : IJoltRpcProcessor
    {
        public Task<string> ProcessAsync(string requestJson, CancellationToken cancellationToken)
            => Task.FromException<string>(new InvalidOperationException("Processor should not be invoked."));
    }

    private sealed class NoOpVueAnalysisRpcProcessor : IVueAnalysisRpcProcessor
    {
        public ValueTask<string> ProcessAsync(string requestJson, CancellationToken cancellationToken)
            => ValueTask.FromException<string>(new InvalidOperationException("Processor should not be invoked."));
    }
}

