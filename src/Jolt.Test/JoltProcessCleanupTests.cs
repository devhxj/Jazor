using System.Diagnostics;
using ECMAScript.Contract.VueContracts.Protocol;
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
            var scriptPath = Path.Combine(tempDirectory, "sleeping-analysis-worker.ps1");
            await File.WriteAllTextAsync(
                scriptPath,
                """
                param(
                    [string]$PidPath
                )

                Set-Content -LiteralPath $PidPath -Value $PID -Encoding utf8
                Start-Sleep -Seconds 30
                '{"id":"analysis-cancelled","success":true,"payloadJson":null}'
                """);

            var transport = new ProcessAnalysisRpcTransport(
                ResolvePowerShellPath(),
                $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" \"{pidFilePath}\"");
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
            var scriptPath = Path.Combine(tempDirectory, "chatty-analysis-worker.ps1");
            await File.WriteAllTextAsync(
                scriptPath,
                """
                for ($i = 0; $i -lt 5000; $i++) {
                    [Console]::Error.WriteLine(("stderr-" + $i.ToString("D4") + " " + ("x" * 120)))
                }

                '{"id":"analysis-stdio","success":true,"payloadJson":"{\"value\":42}","error":null}'
                """);

            var transport = new ProcessAnalysisRpcTransport(
                ResolvePowerShellPath(),
                $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"");
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
            var scriptPath = Path.Combine(tempDirectory, "noisy-stdout-analysis-worker.ps1");
            await File.WriteAllTextAsync(
                scriptPath,
                """
                for ($i = 0; $i -lt 1005; $i++) {
                    "noise-$i"
                }

                '{"id":"analysis-noise","success":true,"payloadJson":null,"error":null}'
                """);

            var transport = new ProcessAnalysisRpcTransport(
                ResolvePowerShellPath(),
                $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"");

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

    private static string ResolvePowerShellPath()
    {
        var systemPowerShell = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");
        return File.Exists(systemPowerShell)
            ? systemPowerShell
            : "powershell.exe";
    }

    private static async Task<int> WaitForProcessIdAsync(string pidFilePath, Task? operationTask = null)
    {
        for (var attempt = 0; attempt < 100; attempt++)
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
