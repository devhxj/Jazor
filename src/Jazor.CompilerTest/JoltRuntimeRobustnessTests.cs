using System.Diagnostics;
using Jazor.VueContracts.Protocol;
using Jolt.Analysis;
using Jolt.Frontend.Deno.Hosting;
using Jolt.Frontend.Deno.Protocol;
using Jolt.Lsp;
using Jolt.Rpc;
using Jolt.Services;
using Jolt.Workspace;

namespace Jazor.CompilerTest;

[TestClass]
public sealed class JoltRuntimeRobustnessTests
{
    [TestMethod]
    public async Task ProcessAnalysisRpcTransport_SendAsync_FailsFastAfterTooManyNonJsonStdoutLines()
    {
        var tempDirectory = CreateTemporaryDirectory();
        int? childProcessId = null;
        try
        {
            var pidFilePath = Path.Combine(tempDirectory, "analysis-worker.pid");
            var scriptPath = Path.Combine(tempDirectory, "chatty-analysis-worker.ps1");
            await File.WriteAllTextAsync(
                scriptPath,
                """
                param(
                    [string]$PidPath
                )

                Set-Content -LiteralPath $PidPath -Value $PID -Encoding utf8
                1..1100 | ForEach-Object { "noise-$($_)" }
                Start-Sleep -Seconds 30
                """);

            var transport = new ProcessAnalysisRpcTransport(
                ResolvePowerShellPath(),
                $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" \"{pidFilePath}\"");
            var sendTask = transport.SendAsync(
                new RpcRequestEnvelope("analysis-noise", "analysis/test", payloadJson: "{}"),
                CancellationToken.None).AsTask();

            childProcessId = await WaitForProcessIdAsync(pidFilePath, sendTask);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await sendTask);

            StringAssert.Contains(exception.Message, "non-JSON stdout lines");
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
    public async Task StdioJoltRpcServer_RunAsync_CancellationDuringRead_ShutsDownCleanly()
    {
        var server = new StdioJoltRpcServer(
            new JoltRpcProcessor(new JoltRpcDispatcher(CreateStartedHost())));
        using var cancellationSource = new CancellationTokenSource();
        var input = new BlockingTextReader();
        using var output = new StringWriter();

        var runTask = server.RunAsync(input, output, cancellationSource.Token);
        cancellationSource.Cancel();

        await runTask;
    }

    [TestMethod]
    public async Task StdioVueAnalysisRpcServer_RunAsync_CancellationDuringRead_ShutsDownCleanly()
    {
        var server = new StdioVueAnalysisRpcServer(new VueAnalysisRpcProcessor(new JazorVueAnalysisService()));
        using var cancellationSource = new CancellationTokenSource();
        var input = new BlockingTextReader();
        using var output = new StringWriter();

        var runTask = server.RunAsync(input, output, cancellationSource.Token);
        cancellationSource.Cancel();

        await runTask;
    }

    [TestMethod]
    public async Task JoltService_StopAsync_WaitsForStartToFinishBeforeStopping()
    {
        var denoHost = new ControlledDenoFrontendHost();
        var service = new JoltService(
            new InMemoryWorkspaceStore(),
            VueAnalysisClientFactory.CreateDefault(),
            denoHost);
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var startTask = service.StartAsync(cancellationSource.Token).AsTask();
        await denoHost.StartEntered.Task.WaitAsync(cancellationSource.Token);

        var stopTask = service.StopAsync(cancellationSource.Token).AsTask();
        Assert.IsFalse(stopTask.IsCompleted, "Stop should wait until the in-flight start finishes.");

        denoHost.AllowStartCompletion.SetResult(true);
        await Task.WhenAll(startTask, stopTask);

        Assert.AreEqual(1, denoHost.StartCallCount);
        Assert.AreEqual(1, denoHost.StopCallCount);
    }

    private static JoltService CreateStartedHost()
    {
        var host = new JoltService(
            new InMemoryWorkspaceStore(),
            VueAnalysisClientFactory.CreateDefault(),
            new ImmediateDenoFrontendHost());
        host.StartAsync(CancellationToken.None).AsTask().GetAwaiter().GetResult();
        return host;
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
                try
                {
                    var text = await File.ReadAllTextAsync(pidFilePath);
                    if (int.TryParse(text.Trim(), out var processId))
                    {
                        return processId;
                    }
                }
                catch (IOException)
                {
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

        Assert.Fail($"Expected child process {processId} to exit.");
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
            "JoltRuntimeRobustnessTests",
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

    private sealed class BlockingTextReader : TextReader
    {
        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var registration = cancellationToken.Register(static state =>
            {
                ((TaskCompletionSource<bool>)state!).TrySetCanceled();
            }, completion);

            await completion.Task;
            return null;
        }
    }

    private sealed class ImmediateDenoFrontendHost : ControlledDenoFrontendHost
    {
        public override async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            StartCallCount++;
            StartEntered.TrySetResult(true);
            IsRunning = true;
            await ValueTask.CompletedTask;
        }
    }

    private class ControlledDenoFrontendHost : IDenoVolarHost
    {
        public TaskCompletionSource<bool> StartEntered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<bool> AllowStartCompletion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsEnabled => true;

        public bool IsRunning { get; protected set; }

        public int StartCallCount { get; protected set; }

        public int StopCallCount { get; protected set; }

        public virtual async ValueTask StartAsync(CancellationToken cancellationToken)
        {
            StartCallCount++;
            StartEntered.TrySetResult(true);
            await AllowStartCompletion.Task.WaitAsync(cancellationToken);
            IsRunning = true;
        }

        public ValueTask StopAsync(CancellationToken cancellationToken)
        {
            StopCallCount++;
            IsRunning = false;
            return ValueTask.CompletedTask;
        }

        public ValueTask<DenoSfcCompileResult?> CompileSfcAsync(
            string documentPath,
            string sfcText,
            string filename,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<DenoSfcCompileResult?>(default);

        public ValueTask<IReadOnlyList<LspDiagnostic>> GetTemplateDiagnosticsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDiagnostic>>([]);

        public ValueTask<IReadOnlyList<LspCompletionItem>> GetTemplateCompletionItemsAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspCompletionItem>>([]);

        public ValueTask<IReadOnlyList<LspDocumentSymbol>> GetTemplateDocumentSymbolsAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspDocumentSymbol>>([]);

        public ValueTask<IReadOnlyList<LspSemanticToken>> GetTemplateSemanticTokensAsync(
            DocumentSnapshot document,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspSemanticToken>>([]);

        public ValueTask<LspHoverResult?> GetTemplateHoverAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspHoverResult?>(default);

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateDefinitionAsync(
            DocumentSnapshot document,
            LspPosition position,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>([]);

        public ValueTask<IReadOnlyList<LspLocation>> GetTemplateReferencesAsync(
            DocumentSnapshot document,
            LspPosition position,
            bool includeDeclaration,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<IReadOnlyList<LspLocation>>([]);

        public ValueTask<LspWorkspaceEdit?> GetTemplateRenameAsync(
            DocumentSnapshot document,
            LspPosition position,
            string newName,
            DenoVolarIntelliSenseContext? context,
            CancellationToken cancellationToken)
            => ValueTask.FromResult<LspWorkspaceEdit?>(default);

        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }
}
