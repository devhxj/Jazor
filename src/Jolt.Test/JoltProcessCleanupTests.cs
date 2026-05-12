using System.Diagnostics;
using Jazor.RazorVue.Protocol;
using Jolt.Analysis;

namespace Jolt.Test;

[TestClass]
public sealed class JoltProcessCleanupTests
{
    [TestMethod]
    public async Task ProcessAnalysisRpcTransport_SendAsync_Cancellation_TerminatesChildProcess()
    {
        var tempDirectory = CreateTemporaryDirectory();
        int? childProcessId = null;
        try
        {
            var pidFilePath = Path.Combine(tempDirectory, "analysis-worker.pid");
            var scriptPath = Path.Combine(tempDirectory, "sleeping-analysis-worker.cs");
            await File.WriteAllTextAsync(
                scriptPath,
                """
                using System;
                using System.IO;
                using System.Threading;

                var pidPath = args[0];
                File.WriteAllText(pidPath, Environment.ProcessId.ToString());
                Thread.Sleep(TimeSpan.FromSeconds(30));
                Console.WriteLine("{\"id\":\"analysis-cancelled\",\"success\":true,\"payloadJson\":null}");
                """);
            ConfigureDotNetChildScriptEnvironment();

            var transport = new ProcessAnalysisRpcTransport(
                "dotnet",
                BuildDotNetFileArguments(scriptPath, pidFilePath));
            using var cancellationSource = new CancellationTokenSource();
            var sendTask = transport.SendAsync(
                new RpcRequestEnvelope("analysis-cancelled", "analysis/test", payloadJson: "{}"),
                cancellationSource.Token).AsTask();

            childProcessId = await WaitForProcessIdAsync(pidFilePath, sendTask);
            cancellationSource.Cancel();
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await sendTask);
            Assert.IsNotNull(exception);

            await WaitForProcessExitAsync(childProcessId.Value);
        }
        finally
        {
            if (childProcessId is { } processId)
            {
                TryTerminateProcess(processId);
            }

            DeleteDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public async Task ProcessAnalysisRpcTransport_SendAsync_LargeStandardError_DoesNotBlockResponse()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var scriptPath = Path.Combine(tempDirectory, "chatty-analysis-worker.cs");
            await File.WriteAllTextAsync(
                scriptPath,
                """
                using System;

                for (var i = 0; i < 5000; i++) {
                    Console.Error.WriteLine($"stderr-{i:D4} {new string('x', 120)}");
                }

                Console.WriteLine("{\"id\":\"analysis-stdio\",\"success\":true,\"payloadJson\":\"{\\\"value\\\":42}\",\"error\":null}");
                """);
            ConfigureDotNetChildScriptEnvironment();

            var transport = new ProcessAnalysisRpcTransport(
                "dotnet",
                BuildDotNetFileArguments(scriptPath));
            using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            var response = await transport.SendAsync(
                new RpcRequestEnvelope("analysis-stdio", "analysis/test", payloadJson: "{}"),
                cancellationSource.Token);

            Assert.IsTrue(response.Success);
            Assert.AreEqual("analysis-stdio", response.Id);
            Assert.AreEqual("{\"value\":42}", response.PayloadJson);
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    [TestMethod]
    public async Task ProcessAnalysisRpcTransport_SendAsync_TooManyNonJsonLines_ThrowsBoundedFailure()
    {
        var tempDirectory = CreateTemporaryDirectory();
        try
        {
            var scriptPath = Path.Combine(tempDirectory, "noisy-stdout-analysis-worker.cs");
            await File.WriteAllTextAsync(
                scriptPath,
                """
                using System;

                for (var i = 0; i < 1005; i++) {
                    Console.WriteLine($"noise-{i}");
                }

                Console.WriteLine("{\"id\":\"analysis-noise\",\"success\":true,\"payloadJson\":null,\"error\":null}");
                """);
            ConfigureDotNetChildScriptEnvironment();

            var transport = new ProcessAnalysisRpcTransport(
                "dotnet",
                BuildDotNetFileArguments(scriptPath));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await transport.SendAsync(
                    new RpcRequestEnvelope("analysis-noise", "analysis/test", payloadJson: "{}"),
                    CancellationToken.None));

            StringAssert.Contains(exception.Message, "more than 1000 non-JSON stdout lines");
        }
        finally
        {
            DeleteDirectory(tempDirectory);
        }
    }

    private static string BuildDotNetFileArguments(string scriptPath, params string[] scriptArguments)
    {
        var arguments = new List<string>
        {
            "run",
            "--file",
            QuoteArgument(scriptPath)
        };

        if (scriptArguments.Length > 0)
        {
            arguments.Add("--");
            arguments.AddRange(scriptArguments.Select(QuoteArgument));
        }

        return string.Join(" ", arguments);
    }

    private static void ConfigureDotNetChildScriptEnvironment()
    {
        Environment.SetEnvironmentVariable("DOTNET_SKIP_FIRST_TIME_EXPERIENCE", "1");
        Environment.SetEnvironmentVariable("DOTNET_NOLOGO", "1");
    }

    private static string QuoteArgument(string value)
    {
        return "\"" + value.Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";
    }

    private static async Task<int> WaitForProcessIdAsync(string pidFilePath, Task? operationTask = null)
    {
        for (var attempt = 0; attempt < 300; attempt++)
        {
            if (File.Exists(pidFilePath))
            {
                var text = await File.ReadAllTextAsync(pidFilePath);
                if (int.TryParse(text.Trim(), out var processId))
                {
                    return processId;
                }
            }

            if (operationTask is { IsFaulted: true })
            {
                Assert.Fail(
                    $"Operation faulted before pid file '{pidFilePath}' was written: {operationTask.Exception?.GetBaseException().Message}");
            }

            if (operationTask is { IsCanceled: true })
            {
                Assert.Fail($"Operation canceled before pid file '{pidFilePath}' was written.");
            }

            await Task.Delay(50);
        }

        Assert.Fail($"Expected pid file '{pidFilePath}' to be written.");
        return 0;
    }

    private static async Task WaitForProcessExitAsync(int processId)
    {
        for (var attempt = 0; attempt < 60; attempt++)
        {
            if (!IsProcessRunning(processId))
            {
                return;
            }

            await Task.Delay(50);
        }

        Assert.Fail($"Expected child process {processId} to exit after cancellation.");
    }

    private static bool IsProcessRunning(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static void TryTerminateProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(1000);
            }
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "JoltProcessCleanupTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100 * attempt);
            }
        }
    }
}

