using Jolt.Frontend.Deno.Hosting;
using Jolt.Frontend.Deno.Protocol;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltDenoFrontendHostLifecycleTests
{
    [TestMethod]
    public async Task DenoFrontendHost_ConcurrentFirstRequests_StartWorkerOnce()
    {
        var workerProcess = new DelayedStartWorkerProcess();
        workerProcess.SetResult(
            "compile/sfc",
            new DenoSfcCompileResult
            {
                JsContent = "export default {};",
                JsSourceMap = """{"version":3}""",
                CssContent = ".app{}",
                Diagnostics = [],
                SupportsHmr = true
            });
        var host = new DenoVolarHost(
            new DenoVolarHostOptions
            {
                Enabled = true,
                IgnoreStartupFailure = false
            },
            workerProcess);

        var firstRequest = host.CompileSfcAsync(
            @"D:\temp\App.jazor",
            "<template><div /></template>",
            "App.jazor",
            CancellationToken.None).AsTask();
        var secondRequest = host.CompileSfcAsync(
            @"D:\temp\App.jazor",
            "<template><div /></template>",
            "App.jazor",
            CancellationToken.None).AsTask();

        await workerProcess.WaitForFirstStartAsync();
        workerProcess.ReleaseStart();
        await Task.WhenAll(firstRequest, secondRequest);

        Assert.AreEqual(1, workerProcess.StartCallCount);
        Assert.AreEqual(2, workerProcess.RequestMethods.Count);
        CollectionAssert.AreEqual(
            new[] { "compile/sfc", "compile/sfc" },
            workerProcess.RequestMethods.ToArray());
    }

    private sealed class DelayedStartWorkerProcess : IDenoWorkerProcess
    {
        private readonly TaskCompletionSource<bool> _startObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _startRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Dictionary<string, object?> _results = new(StringComparer.Ordinal);

        public bool IsRunning { get; private set; }

        public int StartCallCount { get; private set; }

        public List<string> RequestMethods { get; } = [];

        public void SetResult(string method, object? result)
        {
            _results[method] = result;
        }

        public async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StartCallCount++;
            _startObserved.TrySetResult(true);
            await _startRelease.Task.WaitAsync(cancellationToken);
            IsRunning = true;
        }

        public ValueTask<TResult?> SendRequestAsync<TResult>(
            string method,
            object payload,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequestMethods.Add(method);
            return ValueTask.FromResult(
                _results.TryGetValue(method, out var result)
                    ? (TResult?)result
                    : default);
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IsRunning = false;
            return ValueTask.CompletedTask;
        }

        public async Task WaitForFirstStartAsync()
            => await _startObserved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        public void ReleaseStart()
            => _startRelease.TrySetResult(true);
    }
}
