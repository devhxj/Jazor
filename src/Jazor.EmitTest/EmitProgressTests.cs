using Jazor.Emit;
using Jazor.NodeHost;

namespace Jazor.EmitTest;

[TestClass]
public sealed class EmitProgressTests
{
    [TestMethod]
    [DataRow("build")]
    [DataRow("check")]
    public async Task ToolchainCancellation_StopsPendingVitePluginBeforeReturning(string operation)
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.ToolchainCancellation", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "package.json"),
                """{"private":true,"type":"module","devDependencies":{"vite":"8.3.0"}}""");
            var entry = Path.Combine(root, "app.js");
            await File.WriteAllTextAsync(entry, "export const value = 1;");
            await File.WriteAllTextAsync(Path.Combine(root, "vite.config.js"), """
                import { writeFileSync } from 'node:fs';
                export default {
                    plugins: [{ name: 'pending-plugin', load(id) {
                        if (!id.endsWith('/app.js')) return;
                        let writes = 0;
                        const marker = new URL('./writes.txt', import.meta.url);
                        setInterval(() => writeFileSync(marker, String(++writes)), 10);
                        return new Promise(() => {});
                    } }],
                    build: { rolldownOptions: { input: 'app.js' } }
                };
                """);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await NodeToolchain.RestoreAsync(root, Path.Combine(RepositoryTemp.Root, "nodeapi-store"), timeout.Token);
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
            var running = operation == "build"
                ? NodeToolchain.BuildAsync(root, false, cancellation.Token)
                : NodeToolchain.CheckAsync(root, [entry], cancellation.Token);
            var marker = Path.Combine(root, "writes.txt");
            while (!File.Exists(marker)) await Task.Delay(10, timeout.Token);
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await running.WaitAsync(timeout.Token));
            var writes = await File.ReadAllTextAsync(marker);
            await Task.Delay(100, timeout.Token);
            Assert.AreEqual(writes, await File.ReadAllTextAsync(marker), "Vite plugin continued after cancellation returned.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task NodeApiProgress_IsVisibleBeforeCompletion_AndCancellationStopsTheEnvironment()
    {
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitProgressTest", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
        using var progress = new ReadyWriter();
        Task<string>? running = null;
        using var environment = new NodeEnvironment(root, progress);
        try
        {
            // A live timer keeps Node alive until the test cancels it; no timing-based sleep
            // can accidentally turn buffered output into a successful streaming assertion.
            var module = Path.Combine(root, "progress.cjs");
            await File.WriteAllTextAsync(module, """
                exports.run = async () => {
                    console.log('progress-ready');
                    setInterval(() => {}, 1000);
                    await new Promise(() => {});
                };
                """);
            running = environment.InvokeAsync(module, "run", cancellation.Token);
            await progress.Ready.Task.WaitAsync(timeout.Token);
            Assert.IsFalse(running.IsCompleted, "Progress must be visible while JS is still running.");
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await running);
            StringAssert.Contains(progress.ToString(), "progress-ready");
        }
        finally
        {
            cancellation.Cancel();
            if (running is not null)
                try { await running; } catch (OperationCanceledException) { }
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task PipelineCancellation_NamesTheActualStageWithoutClaimingRollback()
    {
        using var progress = new StringWriter();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await new EmitPipeline(progress).ExecuteAsync(
            new EmitOptions(typeof(EmitProgressTests).Assembly.Location, [], "output", "manifest.json", BuildMode.Development,
                SourceRoot: null, LibraryManifests: [], EnableSsr: false), cancellation.Token);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(6, result.ExitCode);
        StringAssert.Contains(result.Error, "cancelled during 'validate options'");
        StringAssert.Contains(progress.ToString(), "validate options: cancelled");
    }

    private sealed class ReadyWriter : StringWriter
    {
        public TaskCompletionSource Ready { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override void WriteLine(string? value)
        {
            base.WriteLine(value);
            if (value?.Contains("progress-ready", StringComparison.Ordinal) == true)
                Ready.TrySetResult();
        }
    }
}
