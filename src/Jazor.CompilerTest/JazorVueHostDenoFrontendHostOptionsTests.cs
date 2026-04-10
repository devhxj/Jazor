using Jazor.VueHost.Frontend.Deno.Hosting;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JazorVueHostDenoFrontendHostOptionsTests
{
    [TestMethod]
    public void JazorVueHost_DenoFrontendHostOptionsParser_Parse_UsesBundledRuntimeAndWorkerDefaults()
    {
        var baseDirectory = CreateTemporaryDirectory();

        try
        {
            var expectedCommand = WriteBundledRuntime(baseDirectory);
            var expectedWorkerPath = WriteWorkerScript(baseDirectory);

            var options = DenoVolarHostOptionsParser.Parse([], baseDirectory);

            Assert.IsTrue(options.Enabled);
            Assert.AreEqual(expectedCommand, options.ExecutablePath);
            Assert.IsFalse(options.HasExplicitExecutableOverride);
            Assert.AreEqual(expectedWorkerPath, options.WorkerScriptPath);
            CollectionAssert.AreEqual(
                new[]
                {
                    "run",
                    "--quiet",
                    "--cached-only",
                    "--allow-env",
                    "--allow-read",
                    expectedWorkerPath
                },
                options.Arguments);
            Assert.AreEqual(Path.GetDirectoryName(expectedWorkerPath), options.WorkingDirectory);
            Assert.IsTrue(options.IgnoreStartupFailure);
        }
        finally
        {
            Directory.Delete(baseDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void JazorVueHost_DenoFrontendHostOptionsParser_Parse_PreservesExplicitCommandArgumentsAndWorkingDirectory()
    {
        var baseDirectory = CreateTemporaryDirectory();

        try
        {
            var options = DenoVolarHostOptionsParser.Parse(
                [
                    "--deno-worker",
                    "--deno-command=C:\\tools\\deno.exe",
                    "--deno-working-directory=C:\\workspace\\vuehost",
                    "--deno-arg=run",
                    "--deno-arg=custom-worker.ts",
                    "--deno-arg=--flag"
                ],
                baseDirectory);

            Assert.IsTrue(options.Enabled);
            Assert.AreEqual(@"C:\tools\deno.exe", options.ExecutablePath);
            Assert.IsTrue(options.HasExplicitExecutableOverride);
            CollectionAssert.AreEqual(
                new[]
                {
                    "run",
                    "custom-worker.ts",
                    "--flag"
                },
                options.Arguments);
            Assert.AreEqual(@"C:\workspace\vuehost", options.WorkingDirectory);
        }
        finally
        {
            Directory.Delete(baseDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task JazorVueHost_DenoWorkerProcess_StartAsync_ThrowsHelpfulErrorWhenBundledRuntimeIsMissing()
    {
        var baseDirectory = CreateTemporaryDirectory();

        try
        {
            var workerPath = WriteWorkerScript(baseDirectory);
            var executablePath = DenoRuntimeAssetResolver.GetExpectedBundledExecutablePath(baseDirectory);
            var process = new DenoWorkerProcess(
                new DenoVolarHostOptions
                {
                    Enabled = true,
                    ExecutablePath = executablePath,
                    HasExplicitExecutableOverride = false,
                    WorkerScriptPath = workerPath,
                    Arguments = ["run", "--quiet", "--cached-only", "--allow-env", "--allow-read", workerPath],
                    WorkingDirectory = Path.GetDirectoryName(workerPath),
                    IgnoreStartupFailure = false
                });

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await process.StartAsync(CancellationToken.None));

            StringAssert.Contains(exception.Message, "DenoHost runtime assets");
        }
        finally
        {
            Directory.Delete(baseDirectory, recursive: true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorVueHostDenoOptionsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string WriteWorkerScript(string baseDirectory)
    {
        var workerPath = Path.Combine(baseDirectory, "Frontend", "Deno", "Worker", "frontend-worker.ts");
        Directory.CreateDirectory(Path.GetDirectoryName(workerPath)!);
        File.WriteAllText(workerPath, "console.log('ok');");
        return workerPath;
    }

    private static string WriteBundledRuntime(string baseDirectory)
    {
        var executablePath = DenoRuntimeAssetResolver.GetExpectedBundledExecutablePath(baseDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(executablePath)!);
        File.WriteAllText(executablePath, "stub");
        return executablePath;
    }
}
