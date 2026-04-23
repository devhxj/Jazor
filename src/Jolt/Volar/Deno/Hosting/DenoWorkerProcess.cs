using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Jolt.Volar.Deno.Protocol;

namespace Jolt.Volar.Deno.Hosting;

internal sealed class DenoWorkerProcess : IDenoWorkerProcess
{
    private const int MaxCapturedStandardErrorLines = 32;
    private static int _launchWorkspaceSequence;
    private static readonly HashSet<string> InheritedWorkerEnvironmentVariablesToRemove = new(StringComparer.OrdinalIgnoreCase)
    {
        "__MINIMATCH_TESTING_PLATFORM__",
        "BABEL_TYPES_8_BREAKING",
        "DENO_DIR",
        "LANG",
        "NODE_DEBUG",
        "NODE_ENV",
        "NODE_INSPECTOR_IPC",
        "VSCODE_INSPECTOR_OPTIONS",
        "VSCODE_NLS_CONFIG",
        "XDG_RUNTIME_DIR"
    };
    private static readonly Lock LaunchWorkspaceCleanupGate = new();
    private static readonly HashSet<string> LaunchWorkspaces = new(StringComparer.OrdinalIgnoreCase);
    private static bool _launchWorkspaceCleanupHookRegistered;

    private readonly DenoVolarHostOptions _options;
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    private readonly Lock _standardErrorGate = new();
    private readonly Queue<string> _standardErrorLines = [];
    private readonly ConcurrentDictionary<string, TaskCompletionSource<DenoVolarResponseEnvelope>> _pendingResponses =
        new(StringComparer.Ordinal);
    private Process? _process;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private Task? _standardOutputPumpTask;
    private CancellationTokenSource? _standardOutputPumpCancellationSource;
    private Task? _standardErrorPumpTask;
    private CancellationTokenSource? _standardErrorPumpCancellationSource;
    private string? _launchWorkingDirectory;
    private int _droppedStandardErrorLineCount;

    public DenoWorkerProcess(DenoVolarHostOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public bool IsRunning => _process is { HasExited: false };

    public async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            if (IsRunning)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.ExecutablePath))
            {
                throw new InvalidOperationException("No Deno runtime path was configured for the Jolt Volar worker.");
            }

            if (!_options.HasExplicitExecutableOverride &&
                Path.IsPathRooted(_options.ExecutablePath) &&
                !File.Exists(_options.ExecutablePath))
            {
                throw new InvalidOperationException(
                    DenoRuntimeAssetResolver.CreateMissingRuntimeMessage(_options.ExecutablePath));
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _options.ExecutablePath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardErrorEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8
            };

            foreach (var argument in _options.Arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            HardenInheritedWorkerEnvironment(startInfo.Environment);

            var workingDirectory = ResolveLaunchWorkingDirectory();
            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
                startInfo.WorkingDirectory = workingDirectory;
            }

            if (!string.IsNullOrWhiteSpace(_options.CacheDirectory))
            {
                Directory.CreateDirectory(_options.CacheDirectory);
                startInfo.Environment["DENO_DIR"] = _options.CacheDirectory;
            }

            _process = new Process
            {
                StartInfo = startInfo
            };
            ResetStandardErrorBuffer();

            try
            {
                if (!_process.Start())
                {
                    throw new InvalidOperationException($"Failed to start Deno Volar worker '{_options.ExecutablePath}'.");
                }
            }
            catch (Win32Exception ex) when (!_options.HasExplicitExecutableOverride)
            {
                CleanupLaunchWorkingDirectory();
                throw new InvalidOperationException(
                    DenoRuntimeAssetResolver.CreateMissingRuntimeMessage(_options.ExecutablePath),
                    ex);
            }
            catch
            {
                CleanupLaunchWorkingDirectory();
                throw;
            }

            _writer = new StreamWriter(_process.StandardInput.BaseStream, new UTF8Encoding(false))
            {
                AutoFlush = true,
                NewLine = "\n"
            };
            _reader = new StreamReader(_process.StandardOutput.BaseStream, Encoding.UTF8);
            _standardOutputPumpCancellationSource = new CancellationTokenSource();
            _standardOutputPumpTask = PumpStandardOutputAsync(
                _reader,
                _standardOutputPumpCancellationSource.Token);
            _standardErrorPumpCancellationSource = new CancellationTokenSource();
            _standardErrorPumpTask = PumpStandardErrorAsync(
                _process.StandardError,
                _standardErrorPumpCancellationSource.Token);
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    internal static void HardenInheritedWorkerEnvironment(IDictionary<string, string?> environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        foreach (var key in environment.Keys.ToArray())
        {
            if (ShouldRemoveInheritedEnvironmentVariable(key))
            {
                environment.Remove(key);
            }
        }

        environment["NO_COLOR"] = "1";
    }

    private static bool ShouldRemoveInheritedEnvironmentVariable(string key)
        => InheritedWorkerEnvironmentVariablesToRemove.Contains(key)
            || key.StartsWith("JOLT_", StringComparison.OrdinalIgnoreCase)
            || key.StartsWith("TSC_", StringComparison.OrdinalIgnoreCase);

    public async ValueTask<TResult?> SendRequestAsync<TResult>(
        string method,
        object payload,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentNullException.ThrowIfNull(payload);
        cancellationToken.ThrowIfCancellationRequested();

        ThrowIfWorkerUnavailable();

        var requestId = Guid.NewGuid().ToString("N");
        var responseSource = new TaskCompletionSource<DenoVolarResponseEnvelope>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (!_pendingResponses.TryAdd(requestId, responseSource))
        {
            throw new InvalidOperationException($"Failed to track Deno frontend worker request '{requestId}'.");
        }

        try
        {
            var request = new DenoVolarRequestEnvelope
            {
                Id = requestId,
                Method = method,
                Payload = payload
            };

            await _writeGate.WaitAsync(cancellationToken);
            try
            {
                ThrowIfWorkerUnavailable();
                await _writer.WriteLineAsync(JsonSerializer.Serialize(request, _jsonOptions));
            }
            catch
            {
                _pendingResponses.TryRemove(requestId, out _);
                throw;
            }
            finally
            {
                _writeGate.Release();
            }

            DenoVolarResponseEnvelope response;
            try
            {
                response = await responseSource.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _pendingResponses.TryRemove(requestId, out _);
                throw;
            }

            if (!response.Success)
            {
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(response.Error)
                        ? $"Deno frontend worker request '{method}' failed.{CreateStandardErrorSummarySuffix()}"
                        : $"Deno frontend worker request '{method}' failed: {response.Error}{CreateStandardErrorSummarySuffix()}");
            }

            var responseResult = response.Result;
            if (responseResult is null
                || responseResult.Value.ValueKind == JsonValueKind.Null)
            {
                return default;
            }

            return responseResult.Value.Deserialize<TResult>(_jsonOptions);
        }
        finally
        {
            _pendingResponses.TryRemove(requestId, out _);
        }
    }

    [MemberNotNull(nameof(_process), nameof(_writer), nameof(_reader))]
    private void ThrowIfWorkerUnavailable()
    {
        if (_process is { HasExited: false }
            && _writer is not null
            && _reader is not null)
        {
            return;
        }

        if (_process is { HasExited: true } exitedProcess)
        {
            throw new InvalidOperationException(
                $"Deno frontend worker exited unexpectedly with code {exitedProcess.ExitCode}.{CreateStandardErrorSummarySuffix()}");
        }

        throw new InvalidOperationException($"Deno frontend worker is not running.{CreateStandardErrorSummarySuffix()}");
    }

    public async ValueTask StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _lifecycleGate.WaitAsync(cancellationToken);
        try
        {
            FailPendingResponses(CreateWorkerUnavailableException());
            if (_process is null)
            {
                return;
            }

            try
            {
                await TerminateProcessIfRunningAsync(_process, cancellationToken);
            }
            finally
            {
                await StopStandardOutputPumpAsync();
                await StopStandardErrorPumpAsync();
                _writer?.Dispose();
                _writer = null;
                _reader?.Dispose();
                _reader = null;
                _process.Dispose();
                _process = null;
                CleanupLaunchWorkingDirectory();
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    private static async Task TerminateProcessIfRunningAsync(
        Process process,
        CancellationToken cancellationToken)
    {
        if (HasExitedOrDetached(process))
        {
            return;
        }

        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException) when (HasExitedOrDetached(process))
        {
            return;
        }
        catch (Win32Exception) when (HasExitedOrDetached(process))
        {
            return;
        }

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (InvalidOperationException) when (HasExitedOrDetached(process))
        {
        }
    }

    private static bool HasExitedOrDetached(Process process)
    {
        try
        {
            return process.HasExited;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private async Task PumpStandardOutputAsync(
        StreamReader standardOutputReader,
        CancellationToken cancellationToken)
    {
        Exception? terminalFailure = null;
        try
        {
            while (true)
            {
                string? line;
                try
                {
                    line = await standardOutputReader.ReadLineAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (line is null)
                {
                    terminalFailure = CreateWorkerUnavailableException();
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                DenoVolarResponseEnvelope? response;
                try
                {
                    response = JsonSerializer.Deserialize<DenoVolarResponseEnvelope>(line, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    terminalFailure = new InvalidOperationException(
                        $"Deno frontend worker returned an invalid response.{CreateStandardErrorSummarySuffix()}",
                        ex);
                    break;
                }

                if (response is null || string.IsNullOrWhiteSpace(response.Id))
                {
                    terminalFailure = new InvalidOperationException(
                        $"Deno frontend worker returned an invalid response.{CreateStandardErrorSummarySuffix()}");
                    break;
                }

                if (_pendingResponses.TryRemove(response.Id, out var pendingResponse))
                {
                    pendingResponse.TrySetResult(response);
                }
            }
        }
        catch (IOException ex)
        {
            terminalFailure = new InvalidOperationException(
                $"Deno frontend worker output stream failed.{CreateStandardErrorSummarySuffix()}",
                ex);
        }
        catch (ObjectDisposedException ex)
        {
            terminalFailure = new InvalidOperationException(
                $"Deno frontend worker output stream was disposed unexpectedly.{CreateStandardErrorSummarySuffix()}",
                ex);
        }

        if (terminalFailure is not null)
        {
            FailPendingResponses(terminalFailure);
        }
    }

    private async Task PumpStandardErrorAsync(
        StreamReader standardErrorReader,
        CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                string? line;
                try
                {
                    line = await standardErrorReader.ReadLineAsync(cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (line is null)
                {
                    return;
                }

                CaptureStandardErrorLine(line);
            }
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private async ValueTask StopStandardErrorPumpAsync()
    {
        var pumpCancellationSource = _standardErrorPumpCancellationSource;
        var pumpTask = _standardErrorPumpTask;
        _standardErrorPumpCancellationSource = null;
        _standardErrorPumpTask = null;

        if (pumpCancellationSource is not null)
        {
            try
            {
                pumpCancellationSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        if (pumpTask is not null)
        {
            try
            {
                await pumpTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        pumpCancellationSource?.Dispose();
    }

    private async ValueTask StopStandardOutputPumpAsync()
    {
        var pumpCancellationSource = _standardOutputPumpCancellationSource;
        var pumpTask = _standardOutputPumpTask;
        _standardOutputPumpCancellationSource = null;
        _standardOutputPumpTask = null;

        if (pumpCancellationSource is not null)
        {
            try
            {
                pumpCancellationSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        if (pumpTask is not null)
        {
            try
            {
                await pumpTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        pumpCancellationSource?.Dispose();
    }

    private void CaptureStandardErrorLine(string line)
    {
        lock (_standardErrorGate)
        {
            while (_standardErrorLines.Count >= MaxCapturedStandardErrorLines)
            {
                _standardErrorLines.Dequeue();
                _droppedStandardErrorLineCount++;
            }

            _standardErrorLines.Enqueue(line);
        }
    }

    private void ResetStandardErrorBuffer()
    {
        lock (_standardErrorGate)
        {
            _standardErrorLines.Clear();
            _droppedStandardErrorLineCount = 0;
        }
    }

    private string CreateStandardErrorSummarySuffix()
    {
        lock (_standardErrorGate)
        {
            if (_standardErrorLines.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(" stderr: ");
            if (_droppedStandardErrorLineCount > 0)
            {
                builder.Append('(');
                builder.Append(_droppedStandardErrorLineCount);
                builder.Append(" earlier stderr lines omitted)");
                if (_standardErrorLines.Count > 0)
                {
                    builder.Append(Environment.NewLine);
                }
            }

            var isFirstLine = true;
            foreach (var standardErrorLine in _standardErrorLines)
            {
                if (!isFirstLine)
                {
                    builder.AppendLine();
                }

                builder.Append(standardErrorLine);
                isFirstLine = false;
            }
            return builder.ToString();
        }
    }

    private InvalidOperationException CreateWorkerUnavailableException()
    {
        if (_process is { HasExited: true } exitedProcess)
        {
            return new InvalidOperationException(
                $"Deno frontend worker exited unexpectedly with code {exitedProcess.ExitCode}.{CreateStandardErrorSummarySuffix()}");
        }

        return new InvalidOperationException($"Deno frontend worker is not running.{CreateStandardErrorSummarySuffix()}");
    }

    private void FailPendingResponses(Exception exception)
    {
        foreach (var pendingResponse in _pendingResponses)
        {
            if (_pendingResponses.TryRemove(pendingResponse.Key, out var responseSource))
            {
                responseSource.TrySetException(exception);
            }
        }
    }

    private string? ResolveLaunchWorkingDirectory()
    {
        if (string.IsNullOrWhiteSpace(_options.WorkingDirectory))
        {
            return null;
        }

        var configuredWorkingDirectory = Path.GetFullPath(_options.WorkingDirectory);
        var workerDirectory = Path.GetDirectoryName(_options.WorkerScriptPath);
        if (string.IsNullOrWhiteSpace(workerDirectory))
        {
            return configuredWorkingDirectory;
        }

        var normalizedWorkerDirectory = Path.GetFullPath(workerDirectory);
        if (!string.Equals(configuredWorkingDirectory, normalizedWorkerDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return configuredWorkingDirectory;
        }

        if (!File.Exists(Path.Combine(normalizedWorkerDirectory, "deno.json")))
        {
            return configuredWorkingDirectory;
        }

        if (HasReadyWorkerNodeModules(normalizedWorkerDirectory))
        {
            // Bundled workers can carry a ready node_modules tree. Keep that
            // directory as the config root so Deno resolves npm specifiers
            // against the packaged dependencies instead of an empty temp root.
            return configuredWorkingDirectory;
        }

        if (!string.IsNullOrWhiteSpace(_launchWorkingDirectory))
        {
            return _launchWorkingDirectory;
        }

        var launchWorkspaceRoot = string.IsNullOrWhiteSpace(_options.CacheDirectory)
            ? Path.Combine(Path.GetTempPath(), "Jolt", "Deno", "Workspaces")
            : Path.Combine(_options.CacheDirectory, "workspaces");
        var launchWorkspaceDirectory = Path.Combine(
            launchWorkspaceRoot,
            $"{Environment.ProcessId:D6}-{Interlocked.Increment(ref _launchWorkspaceSequence):D4}");
        Directory.CreateDirectory(launchWorkspaceDirectory);
        CopyWorkerConfigurationFiles(normalizedWorkerDirectory, launchWorkspaceDirectory);
        RegisterLaunchWorkspaceForCleanup(launchWorkspaceDirectory);
        _launchWorkingDirectory = launchWorkspaceDirectory;
        return launchWorkspaceDirectory;
    }

    private static bool HasReadyWorkerNodeModules(string workerDirectory)
    {
        var nodeModulesDirectory = Path.Combine(workerDirectory, "node_modules");
        return Directory.Exists(Path.Combine(nodeModulesDirectory, "@volar"))
            && Directory.Exists(Path.Combine(nodeModulesDirectory, "@vue"));
    }

    private static void CopyWorkerConfigurationFiles(
        string sourceDirectory,
        string destinationDirectory)
    {
        foreach (var fileName in new[] { "deno.json", "deno.lock", "package.json", "package-lock.json", "npm-shrinkwrap.json" })
        {
            var sourcePath = Path.Combine(sourceDirectory, fileName);
            if (!File.Exists(sourcePath))
            {
                continue;
            }

            File.Copy(sourcePath, Path.Combine(destinationDirectory, fileName), overwrite: true);
        }
    }

    private static void RegisterLaunchWorkspaceForCleanup(string launchWorkspaceDirectory)
    {
        lock (LaunchWorkspaceCleanupGate)
        {
            LaunchWorkspaces.Add(launchWorkspaceDirectory);
            if (_launchWorkspaceCleanupHookRegistered)
            {
                return;
            }

            AppDomain.CurrentDomain.ProcessExit += static (_, _) => CleanupLaunchWorkspaces();
            _launchWorkspaceCleanupHookRegistered = true;
        }
    }

    private static void CleanupLaunchWorkspaces()
    {
        string[] launchWorkspaceDirectories;
        lock (LaunchWorkspaceCleanupGate)
        {
            launchWorkspaceDirectories = LaunchWorkspaces.ToArray();
            LaunchWorkspaces.Clear();
        }

        foreach (var launchWorkspaceDirectory in launchWorkspaceDirectories)
        {
            try
            {
                if (Directory.Exists(launchWorkspaceDirectory))
                {
                    Directory.Delete(launchWorkspaceDirectory, recursive: true);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private void CleanupLaunchWorkingDirectory()
    {
        string? launchWorkingDirectory;
        lock (LaunchWorkspaceCleanupGate)
        {
            launchWorkingDirectory = _launchWorkingDirectory;
            _launchWorkingDirectory = null;
            if (string.IsNullOrWhiteSpace(launchWorkingDirectory))
            {
                return;
            }

            LaunchWorkspaces.Remove(launchWorkingDirectory);
        }

        TryDeleteLaunchWorkspace(launchWorkingDirectory);
    }

    private static void TryDeleteLaunchWorkspace(string launchWorkspaceDirectory)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (!Directory.Exists(launchWorkspaceDirectory))
                {
                    return;
                }

                Directory.Delete(launchWorkspaceDirectory, recursive: true);
                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(50 * attempt);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(50 * attempt);
            }
        }
    }
}
