using System.Collections.Concurrent;
using Jolt.Frontend.Deno.Hosting;
using Jolt.Frontend.Deno.Protocol;

namespace Jolt.Test;

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
        var requestMethods = workerProcess.RequestMethods;
        Assert.AreEqual(2, requestMethods.Count);
        CollectionAssert.AreEqual(
            new[] { "compile/sfc", "compile/sfc" },
            requestMethods.ToArray());
    }

    private sealed class DelayedStartWorkerProcess : IDenoWorkerProcess
    {
        private readonly TaskCompletionSource<bool> _startObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> _startRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ConcurrentDictionary<string, object?> _results = new(StringComparer.Ordinal);
        private readonly List<string> _requestMethods = [];
        private readonly Lock _requestMethodsGate = new();
        private int _isRunning;
        private int _startCallCount;

        public bool IsRunning => Volatile.Read(ref _isRunning) == 1;

        public int StartCallCount => Volatile.Read(ref _startCallCount);

        public IReadOnlyList<string> RequestMethods
        {
            get
            {
                lock (_requestMethodsGate)
                {
                    return _requestMethods.ToArray();
                }
            }
        }

        public void SetResult(string method, object? result)
        {
            _results[method] = result;
        }

        public async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _startCallCount);
            _startObserved.TrySetResult(true);
            await _startRelease.Task.WaitAsync(cancellationToken);
            Volatile.Write(ref _isRunning, 1);
        }

        public ValueTask<TResult?> SendRequestAsync<TResult>(
            string method,
            object payload,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_requestMethodsGate)
            {
                _requestMethods.Add(method);
            }

            return ValueTask.FromResult(
                _results.TryGetValue(method, out var result)
                    ? (TResult?)result
                    : default);
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Volatile.Write(ref _isRunning, 0);
            return ValueTask.CompletedTask;
        }

        public async Task WaitForFirstStartAsync()
            => await _startObserved.Task.WaitAsync(TimeSpan.FromSeconds(1));

        public void ReleaseStart()
            => _startRelease.TrySetResult(true);
    }
}
