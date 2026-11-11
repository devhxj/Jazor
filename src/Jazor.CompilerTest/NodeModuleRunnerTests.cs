using Jazor.NodeHost;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class NodeModuleRunnerTests
{
    [TestMethod]
    public async Task RunAsync_CancellationStopsTimerAndFileWritesBeforeReturning()
    {
        var root = Path.Combine(RepositoryTemp.Root, "nodeapi-cancellation", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var module = Path.Combine(root, "timer.mjs");
            var counter = Path.Combine(root, "counter.txt");
            await File.WriteAllTextAsync(module, """
                import { writeFileSync } from 'node:fs';
                const counter = new URL('./counter.txt', import.meta.url);
                let writes = 0;
                setInterval(() => writeFileSync(counter, String(++writes)), 10);
                globalThis.__jazorTest('pending timer work', () => new Promise(() => {}));
                """);
            using var cancellation = new CancellationTokenSource();
            var running = NodeModuleRunner.RunAsync(root, module, cancellationToken: cancellation.Token);
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (!File.Exists(counter))
                await Task.Delay(10, deadline.Token);
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await running.WaitAsync(deadline.Token));
            var writes = await File.ReadAllTextAsync(counter);
            await Task.Delay(100, deadline.Token);
            Assert.AreEqual(writes, await File.ReadAllTextAsync(counter), "JS continued to write after cancellation returned.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task RunAsync_AwaitsRegisteredTestsAndPropagatesAsyncAssertionFailure()
    {
        var root = Path.Combine(RepositoryTemp.Root, "nodeapi-runner", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var module = Path.Combine(root, "failure.mjs");
            await File.WriteAllTextAsync(module, """
                import assert from 'node:assert/strict';
                import { writeFile } from 'node:fs/promises';
                globalThis.__jazorAfterAll(async () => {
                    await writeFile(new URL('./cleanup.txt', import.meta.url), 'released');
                });
                globalThis.__jazorTest('async assertion must reach MSTest', async () => {
                    await new Promise(resolve => setTimeout(resolve, 10));
                    assert.equal(41, 42, 'native host assertion evidence');
                });
                """);
            var failure = await Assert.ThrowsAsync<Exception>(
                async () => await NodeModuleRunner.RunAsync(root, module));
            StringAssert.Contains(failure.ToString(), "async assertion must reach MSTest");
            StringAssert.Contains(failure.ToString(), "native host assertion evidence");
            Assert.AreEqual("released", await File.ReadAllTextAsync(Path.Combine(root, "cleanup.txt")),
                "Fixture cleanup must finish even when an async assertion fails.");

            // A failed environment must not poison the next environment or
            // replay the previous module's registered assertions.
            var nextModule = Path.Combine(root, "success.mjs");
            await File.WriteAllTextAsync(nextModule, """
                globalThis.__jazorTest('fresh environment', async () => {
                    await new Promise(resolve => setTimeout(resolve, 10));
                    console.log('completed');
                });
                """);
            Assert.AreEqual("completed", await NodeModuleRunner.RunAsync(root, nextModule));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
