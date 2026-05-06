using Jazor.RazorVue.Protocol;
using Jolt.Lsp;
using Jolt.Volar.Deno.Hosting;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDenoVolarHostTests
{
    [TestMethod]
    public async Task Jolt_DenoVolarHost_WhenWorkerRequestStalls_TimesOutAndResetsWorker()
    {
        var workerProcess = new HangingDenoWorkerProcess();
        await using var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                RequestTimeout = TimeSpan.FromMilliseconds(50),
                IgnoreStartupFailure = false
            },
            workerProcess);
        var document = new DocumentSnapshot(
            "/test/Counter.jazor",
            DocumentKind.Jazor,
            "<template><div /></template>",
            version: "1");

        var exception = await Assert.ThrowsAsync<TimeoutException>(
            async () => await host.GetTemplateHoverAsync(
                document,
                new LspPosition { Line = 0, Character = 0 },
                context: null,
                CancellationToken.None));

        StringAssert.Contains(exception.Message, "timed out", StringComparison.OrdinalIgnoreCase);
        Assert.IsTrue(workerProcess.StartCallCount >= 1);
        Assert.IsTrue(workerProcess.SendCallCount >= 1);
        Assert.IsTrue(workerProcess.StopCallCount >= 1, "Timed out requests should reset the worker before retrying.");
    }

    private sealed class HangingDenoWorkerProcess : IDenoWorkerProcess
    {
        public bool IsRunning { get; private set; }

        public int StartCallCount { get; private set; }

        public int SendCallCount { get; private set; }

        public int StopCallCount { get; private set; }

        public ValueTask StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartCallCount++;
            IsRunning = true;
            return ValueTask.CompletedTask;
        }

        public async ValueTask<TResult?> SendRequestAsync<TResult>(
            string method,
            object payload,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SendCallCount++;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return default;
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StopCallCount++;
            IsRunning = false;
            return ValueTask.CompletedTask;
        }
    }
}

