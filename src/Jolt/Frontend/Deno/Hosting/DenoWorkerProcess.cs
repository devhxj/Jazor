using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Jolt.Frontend.Deno.Protocol;

namespace Jolt.Frontend.Deno.Hosting;

internal sealed class DenoWorkerProcess : IDenoWorkerProcess
{
    private const int MaxCapturedStandardErrorLines = 32;
    private static int _launchWorkspaceSequence;
    private static readonly Lock LaunchWorkspaceCleanupGate = new();
    private static readonly HashSet<string> LaunchWorkspaces = new(StringComparer.OrdinalIgnoreCase);
    private static bool _launchWorkspaceCleanupHookRegistered;

    private readonly DenoVolarHostOptions _options;
    private readonly SemaphoreSlim _requestGate = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
    private readonly Lock _standardErrorGate = new();
    private readonly Queue<string> _standardErrorLines = [];
    private Process? _process;
    private StreamWriter? _writer;
    private StreamReader? _reader;
    private Task? _standardErrorPumpTask;
    private CancellationTokenSource? _standardErrorPumpCancellationSource;
    private string? _launchWorkingDirectory;
    private int _droppedStandardErrorLineCount;

    public DenoWorkerProcess(DenoVolarHostOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public bool IsRunning => _process is { HasExited: false };

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IsRunning)
        {
            return ValueTask.CompletedTask;
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
        _standardErrorPumpCancellationSource = new CancellationTokenSource();
        _standardErrorPumpTask = PumpStandardErrorAsync(
            _process.StandardError,
            _standardErrorPumpCancellationSource.Token);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<TResult?> SendRequestAsync<TResult>(
        string method,
        object payload,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(method);
        ArgumentNullException.ThrowIfNull(payload);
        cancellationToken.ThrowIfCancellationRequested();

        ThrowIfWorkerUnavailable();

        await _requestGate.WaitAsync(cancellationToken);
        try
        {
            ThrowIfWorkerUnavailable();

            var request = new DenoFrontendRequestEnvelope
            {
                Id = Guid.NewGuid().ToString("N"),
                Method = method,
                Payload = payload
            };

            await _writer.WriteLineAsync(JsonSerializer.Serialize(request, _jsonOptions));
            var responseLine = await _reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(responseLine))
            {
                throw new InvalidOperationException(
                    $"Deno frontend worker returned no response for request '{method}'.{CreateStandardErrorSummarySuffix()}");
            }

            var response = JsonSerializer.Deserialize<DenoFrontendResponseEnvelope>(responseLine, _jsonOptions);
            if (response is null)
            {
                throw new InvalidOperationException(
                    $"Deno frontend worker returned an invalid response for request '{method}'.{CreateStandardErrorSummarySuffix()}");
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
            _requestGate.Release();
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
        if (_process is null)
        {
            return;
        }

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync(cancellationToken);
            }
        }
        finally
        {
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

            builder.Append(string.Join(Environment.NewLine, _standardErrorLines));
            return builder.ToString();
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
