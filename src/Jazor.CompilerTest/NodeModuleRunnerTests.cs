using Jazor.NodeHost;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class NodeModuleRunnerTests
{
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
                globalThis.__jazorTest('async assertion must reach MSTest', async () => {
                    await new Promise(resolve => setTimeout(resolve, 10));
                    assert.equal(41, 42, 'native host assertion evidence');
                });
                """);
            var failure = await Assert.ThrowsAsync<Exception>(
                async () => await NodeModuleRunner.RunAsync(root, module));
            StringAssert.Contains(failure.ToString(), "async assertion must reach MSTest");
            StringAssert.Contains(failure.ToString(), "native host assertion evidence");

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
