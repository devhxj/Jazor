using Jolt.Volar.Deno.Hosting;

namespace Jolt.Test;

[TestClass]
public sealed class JoltDenoFrontendHostOptionsTests
{
    private const string DefaultAllowEnvArgument =
        "--allow-env=__MINIMATCH_TESTING_PLATFORM__,BABEL_TYPES_8_BREAKING,DENO_DIR,JOLT_*,LANG,NODE_DEBUG,NODE_ENV,NODE_INSPECTOR_IPC,NO_COLOR,TSC_*,VSCODE_INSPECTOR_OPTIONS,VSCODE_NLS_CONFIG,XDG_RUNTIME_DIR";

    [TestMethod]
    public void Jolt_DenoFrontendHostOptionsParser_Parse_UsesBundledRuntimeAndWorkerDefaults()
    {
        var baseDirectory = CreateTemporaryDirectory();

        try
        {
            var expectedCommand = WriteBundledRuntime(baseDirectory);
            var expectedWorkerPath = WriteWorkerScript(baseDirectory);
            var expectedWorkspaceRoot = Path.Combine(baseDirectory, "workspace");
            Directory.CreateDirectory(expectedWorkspaceRoot);

            var options = DenoVolarHostOptionsParser.Parse(
                [$"--dev-root={expectedWorkspaceRoot}"],
                baseDirectory);

            Assert.IsTrue(options.Enabled);
            Assert.AreEqual(expectedCommand, options.ExecutablePath);
            Assert.IsFalse(options.HasExplicitExecutableOverride);
            Assert.AreEqual(expectedWorkerPath, options.WorkerScriptPath);
            Assert.AreEqual("run", options.Arguments[0]);
            Assert.AreEqual("--quiet", options.Arguments[1]);
            Assert.AreEqual(DefaultAllowEnvArgument, options.Arguments[2]);
            Assert.AreEqual(expectedWorkerPath, options.Arguments[^1]);

            var allowReadArgument = options.Arguments.Single(argument => argument.StartsWith("--allow-read=", StringComparison.Ordinal));
            var allowedReadPaths = allowReadArgument["--allow-read=".Length..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            CollectionAssert.AreEquivalent(
                new[]
                {
                    Path.GetDirectoryName(expectedWorkerPath)!,
                    options.CacheDirectory,
                    expectedWorkspaceRoot
                },
                allowedReadPaths);
            Assert.AreEqual(Path.GetDirectoryName(expectedWorkerPath), options.WorkingDirectory);
            Assert.IsTrue(options.IgnoreStartupFailure);
        }
        finally
        {
            Directory.Delete(baseDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void Jolt_DenoFrontendHostOptionsParser_Parse_PreservesExplicitCommandArgumentsAndWorkingDirectory()
    {
        var baseDirectory = CreateTemporaryDirectory();

        try
        {
            var options = DenoVolarHostOptionsParser.Parse(
                [
                    "--deno-worker",
                    "--deno-command=C:\\tools\\deno.exe",
                    "--deno-working-directory=C:\\workspace\\jolt",
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
            Assert.AreEqual(@"C:\workspace\jolt", options.WorkingDirectory);
        }
        finally
        {
            Directory.Delete(baseDirectory, recursive: true);
        }
    }

    [TestMethod]
    public async Task Jolt_DenoWorkerProcess_StartAsync_ThrowsHelpfulErrorWhenBundledRuntimeIsMissing()
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
                    Arguments =
                    [
                        "run",
                        "--quiet",
                        "--cached-only",
                        DefaultAllowEnvArgument,
                        $"--allow-read={Path.GetDirectoryName(workerPath)},{Path.Combine(baseDirectory, "Volar", "Deno", "Cache")},{Path.GetDirectoryName(workerPath)}",
                        workerPath
                    ],
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

    [TestMethod]
    public void Jolt_DenoWorkerProcess_HardenInheritedWorkerEnvironment_RemovesHostControlledAllowEnvKnobs()
    {
        var environment = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["PATH"] = "keep",
            ["DENO_DIR"] = "host-deno-cache",
            ["JOLT_SECRET"] = "host-secret",
            ["TSC_WATCHFILE"] = "dynamicPriorityPolling",
            ["NODE_ENV"] = "production",
            ["NODE_DEBUG"] = "module",
            ["VSCODE_NLS_CONFIG"] = "{}",
            ["__MINIMATCH_TESTING_PLATFORM__"] = "win32",
            ["BABEL_TYPES_8_BREAKING"] = "1",
            ["XDG_RUNTIME_DIR"] = "/run/user/1000"
        };

        DenoWorkerProcess.HardenInheritedWorkerEnvironment(environment);

        Assert.AreEqual("keep", environment["PATH"]);
        Assert.AreEqual("1", environment["NO_COLOR"]);
        Assert.IsFalse(environment.ContainsKey("DENO_DIR"));
        Assert.IsFalse(environment.ContainsKey("JOLT_SECRET"));
        Assert.IsFalse(environment.ContainsKey("TSC_WATCHFILE"));
        Assert.IsFalse(environment.ContainsKey("NODE_ENV"));
        Assert.IsFalse(environment.ContainsKey("NODE_DEBUG"));
        Assert.IsFalse(environment.ContainsKey("VSCODE_NLS_CONFIG"));
        Assert.IsFalse(environment.ContainsKey("__MINIMATCH_TESTING_PLATFORM__"));
        Assert.IsFalse(environment.ContainsKey("BABEL_TYPES_8_BREAKING"));
        Assert.IsFalse(environment.ContainsKey("XDG_RUNTIME_DIR"));
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JoltDenoOptionsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string WriteWorkerScript(string baseDirectory)
    {
        var workerPath = Path.Combine(baseDirectory, "Volar", "Deno", "Worker", "volar-worker.ts");
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
