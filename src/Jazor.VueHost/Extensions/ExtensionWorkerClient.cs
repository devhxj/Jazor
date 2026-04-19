using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Jazor.VueHost.Lsp;

namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionWorkerClient : IAsyncDisposable
{
    private readonly Process _process;
    private readonly LspMessageReader _reader;
    private readonly LspMessageWriter _writer;
    private readonly SemaphoreSlim _requestGate = new(1, 1);
    private readonly CancellationTokenSource _stderrLifetime = new();
    private readonly StringBuilder _stderrBuffer = new();
    private readonly Task _stderrPumpTask;

    private int _nextRequestId;
    private int _shutdownRequested;
    private bool _disposed;

    private ExtensionWorkerClient(Process process)
    {
        _process = process;
        _reader = new LspMessageReader(process.StandardOutput.BaseStream);
        _writer = new LspMessageWriter(process.StandardInput.BaseStream);
        _stderrPumpTask = Task.Run(PumpStandardErrorAsync);
    }

    public static ValueTask<ExtensionWorkerClient> StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var startInfo = CreateProcessStartInfo();
        var process = new Process
        {
            StartInfo = startInfo
        };

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("failed to start extension worker process.");
            }
        }
        catch (ObjectDisposedException)
        {
            process.Dispose();
            throw;
        }
        catch (InvalidOperationException)
        {
            process.Dispose();
            throw;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            process.Dispose();
            throw;
        }

        return ValueTask.FromResult(new ExtensionWorkerClient(process));
    }

    public async ValueTask<ExtensionWorkerBootstrapResponse> BootstrapAsync(
        ExtensionWorkerBootstrapRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var payload = await SendRequestAsync(
            ExtensionWorkerMethodNames.Bootstrap,
            request,
            cancellationToken);
        return DeserializeRequired<ExtensionWorkerBootstrapResponse>(
            payload,
            $"{ExtensionWorkerMethodNames.Bootstrap} response");
    }

    public async ValueTask<TResult?> InvokeAsync<TResult>(
        string capability,
        object context,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(capability))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(capability));
        }

        var payload = await SendRequestAsync(
            ExtensionWorkerMethodNames.Invoke,
            new ExtensionWorkerInvokeRequest(capability, context),
            cancellationToken);
        return DeserializeOptional<TResult>(payload);
    }

    public async ValueTask ShutdownAsync()
    {
        if (Interlocked.Exchange(ref _shutdownRequested, 1) != 0)
        {
            return;
        }

        try
        {
            using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await SendRequestAsync(
                ExtensionWorkerMethodNames.Shutdown,
                parameters: null,
                shutdownTimeout.Token);
        }
        catch (OperationCanceledException)
        {
            // Best-effort shutdown. We'll terminate forcefully below.
        }
        catch (ExtensionWorkerConnectionException)
        {
            // Best-effort shutdown. We'll terminate forcefully below.
        }
        catch (ObjectDisposedException)
        {
            // Best-effort shutdown. We'll terminate forcefully below.
        }
        catch (InvalidOperationException)
        {
            // Best-effort shutdown. We'll terminate forcefully below.
        }
        catch (IOException)
        {
            // Best-effort shutdown. We'll terminate forcefully below.
        }
        finally
        {
            await TerminateWorkerAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await ShutdownAsync();
        _disposed = true;
        _requestGate.Dispose();
        _stderrLifetime.Cancel();
        try
        {
            await _stderrPumpTask;
        }
        catch (OperationCanceledException)
        {
            // Ignore stderr read termination errors.
        }
        catch (ObjectDisposedException)
        {
            // Ignore stderr read termination errors.
        }
        catch (IOException)
        {
            // Ignore stderr read termination errors.
        }
        catch (InvalidOperationException)
        {
            // Ignore stderr read termination errors.
        }

        _stderrLifetime.Dispose();
        _process.Dispose();
    }

    private async ValueTask<object?> SendRequestAsync(
        string method,
        object? parameters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ThrowIfProcessExited();

        await _requestGate.WaitAsync(cancellationToken);
        try
        {
            ThrowIfDisposed();
            ThrowIfProcessExited();

            var requestId = Interlocked.Increment(ref _nextRequestId);
            var request = new ExtensionWorkerRequestEnvelope(
                Id: requestId,
                Method: method,
                Params: parameters);
            ExtensionWorkerResponseEnvelope response;
            try
            {
                await _writer.WriteMessageAsync(
                    LspJsonSerializer.Serialize(request),
                    CancellationToken.None);

                response = await ReadResponseAsync(requestId, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                await TerminateWorkerAsync();
                throw;
            }
            catch (ExtensionWorkerConnectionException)
            {
                throw;
            }
            catch (ObjectDisposedException exception)
            {
                await TerminateWorkerAsync();
                throw new ExtensionWorkerConnectionException(
                    $"extension worker connection was disposed while invoking '{method}': {exception.Message}",
                    exception);
            }
            catch (IOException exception)
            {
                await TerminateWorkerAsync();
                throw new ExtensionWorkerConnectionException(
                    $"failed to communicate with extension worker while invoking '{method}': {exception.Message}",
                    exception);
            }
            catch (InvalidOperationException exception) when (_process.HasExited)
            {
                await TerminateWorkerAsync();
                ThrowIfProcessExited();
                throw new ExtensionWorkerConnectionException(
                    $"extension worker process exited while invoking '{method}': {exception.Message}",
                    exception);
            }

            if (response.Error is not null)
            {
                throw new InvalidOperationException(
                    $"extension worker method '{method}' failed with {response.Error.Code}: {response.Error.Message}");
            }

            return response.Result;
        }
        finally
        {
            _requestGate.Release();
        }
    }

    private async ValueTask<ExtensionWorkerResponseEnvelope> ReadResponseAsync(
        int expectedRequestId,
        CancellationToken cancellationToken)
    {
        Task<string?> readTask;
        try
        {
            readTask = _reader.ReadMessageAsync(CancellationToken.None).AsTask();
        }
        catch (ObjectDisposedException exception)
        {
            await TerminateWorkerAsync();
            throw new ExtensionWorkerConnectionException(
                $"failed to read extension worker response: {exception.Message}",
                exception);
        }
        catch (IOException exception)
        {
            await TerminateWorkerAsync();
            throw new ExtensionWorkerConnectionException(
                $"failed to read extension worker response: {exception.Message}",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            await TerminateWorkerAsync();
            throw new ExtensionWorkerConnectionException(
                $"failed to read extension worker response: {exception.Message}",
                exception);
        }

        string? responseJson;
        try
        {
            responseJson = await readTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await TerminateWorkerAsync();
            throw;
        }
        catch (ObjectDisposedException exception)
        {
            await TerminateWorkerAsync();
            throw new ExtensionWorkerConnectionException(
                $"failed to receive extension worker response: {exception.Message}",
                exception);
        }
        catch (IOException exception)
        {
            await TerminateWorkerAsync();
            throw new ExtensionWorkerConnectionException(
                $"failed to receive extension worker response: {exception.Message}",
                exception);
        }
        catch (InvalidOperationException exception)
        {
            await TerminateWorkerAsync();
            throw new ExtensionWorkerConnectionException(
                $"failed to receive extension worker response: {exception.Message}",
                exception);
        }

        if (responseJson is null)
        {
            await TerminateWorkerAsync();
            ThrowIfProcessExited();
            throw new ExtensionWorkerConnectionException("extension worker connection closed unexpectedly.");
        }

        var response = LspJsonSerializer.Deserialize<ExtensionWorkerResponseEnvelope>(responseJson)
            ?? throw new ExtensionWorkerConnectionException("extension worker response payload is invalid.");
        if (response.Id != expectedRequestId)
        {
            await TerminateWorkerAsync();
            throw new ExtensionWorkerConnectionException(
                $"extension worker response id mismatch: expected {expectedRequestId}, actual {response.Id}.");
        }

        return response;
    }

    private async Task PumpStandardErrorAsync()
    {
        try
        {
            while (!_stderrLifetime.Token.IsCancellationRequested)
            {
                var line = await _process.StandardError.ReadLineAsync(_stderrLifetime.Token);
                if (line is null)
                {
                    break;
                }

                lock (_stderrBuffer)
                {
                    if (_stderrBuffer.Length > 8_192)
                    {
                        _stderrBuffer.Remove(0, _stderrBuffer.Length - 8_192);
                    }

                    _stderrBuffer.AppendLine(line);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
        catch (ObjectDisposedException)
        {
            // Ignore stderr collection failures.
        }
        catch (IOException)
        {
            // Ignore stderr collection failures.
        }
        catch (InvalidOperationException)
        {
            // Ignore stderr collection failures.
        }
    }

    private async ValueTask TerminateWorkerAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }
        }
        catch (ObjectDisposedException)
        {
            // Ignore kill failures if the process already exited.
        }
        catch (InvalidOperationException)
        {
            // Ignore kill failures if the process already exited.
        }
        catch (PlatformNotSupportedException)
        {
            // Ignore kill failures if the process already exited.
        }
        catch (NotSupportedException)
        {
            // Ignore kill failures if the process already exited.
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Ignore kill failures if the process already exited.
        }

        try
        {
            await _process.WaitForExitAsync(CancellationToken.None);
        }
        catch (ObjectDisposedException)
        {
            // Ignore wait failures during teardown.
        }
        catch (InvalidOperationException)
        {
            // Ignore wait failures during teardown.
        }
    }

    private static ProcessStartInfo CreateProcessStartInfo()
    {
        var environmentProcessPath = Environment.ProcessPath;
        var workerRuntimeDirectory = AppContext.BaseDirectory;
        if (!string.IsNullOrWhiteSpace(environmentProcessPath)
            && string.Equals(
                Path.GetFileNameWithoutExtension(environmentProcessPath),
                "Jazor.VueHost",
                StringComparison.OrdinalIgnoreCase))
        {
            var directHostStartInfo = new ProcessStartInfo
            {
                FileName = environmentProcessPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workerRuntimeDirectory
            };
            directHostStartInfo.ArgumentList.Add("--extension-worker");
            ApplyHardenedWorkerEnvironment(directHostStartInfo.Environment);
            return directHostStartInfo;
        }

        var hostDllPath = ResolveManagedHostDllPath();
        if (hostDllPath is not null)
        {
            var dotnetHostPath = ResolveDotnetHostPath(environmentProcessPath);
            var dotnetStartInfo = new ProcessStartInfo
            {
                FileName = dotnetHostPath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(hostDllPath)
            };
            dotnetStartInfo.ArgumentList.Add(hostDllPath);
            dotnetStartInfo.ArgumentList.Add("--extension-worker");
            ApplyHardenedWorkerEnvironment(dotnetStartInfo.Environment);
            return dotnetStartInfo;
        }

        throw new InvalidOperationException(
            $"Unable to locate Jazor.VueHost worker host executable. Environment.ProcessPath='{environmentProcessPath ?? "<null>"}', AppContext.BaseDirectory='{AppContext.BaseDirectory}'.");
    }

    private static string ResolveDotnetHostPath(string? environmentProcessPath)
    {
        if (!string.IsNullOrWhiteSpace(environmentProcessPath)
            && string.Equals(
                Path.GetFileNameWithoutExtension(environmentProcessPath),
                "dotnet",
                StringComparison.OrdinalIgnoreCase))
        {
            return environmentProcessPath;
        }

        return "dotnet";
    }

    private static void ApplyHardenedWorkerEnvironment(
        IDictionary<string, string?> environment)
    {
        environment["JAZOR_EXTENSION_WORKER"] = "1";
        environment["DOTNET_EnableDiagnostics"] = "0";
        environment["COMPlus_EnableDiagnostics"] = "0";
    }

    private static string? ResolveManagedHostDllPath()
    {
        var candidates = new List<string>();
        var localAssemblyPath = typeof(ExtensionWorkerClient).Assembly.Location;
        if (!string.IsNullOrWhiteSpace(localAssemblyPath))
        {
            candidates.Add(localAssemblyPath);
        }

        var appContextCandidate = Path.Combine(AppContext.BaseDirectory, "Jazor.VueHost.dll");
        candidates.Add(appContextCandidate);

        var repositoryRoot = FindRepositoryRoot();
        if (!string.IsNullOrWhiteSpace(repositoryRoot)
            && !string.IsNullOrWhiteSpace(localAssemblyPath))
        {
            var (configuration, targetFramework) = InferBuildLayout(localAssemblyPath);
            if (!string.IsNullOrWhiteSpace(configuration)
                && !string.IsNullOrWhiteSpace(targetFramework))
            {
                candidates.Insert(
                    0,
                    Path.Combine(
                        repositoryRoot,
                        "src",
                        "Jazor.VueHost",
                        "bin",
                        configuration,
                        targetFramework,
                        "Jazor.VueHost.dll"));
            }
        }

        foreach (var candidate in candidates
                     .Where(static path => !string.IsNullOrWhiteSpace(path))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate))
            {
                continue;
            }

            if (string.Equals(
                    Path.GetFileName(candidate),
                    "Jazor.VueHost.dll",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
    }

    private static (string? Configuration, string? TargetFramework) InferBuildLayout(string assemblyPath)
    {
        var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
        if (string.IsNullOrWhiteSpace(assemblyDirectory))
        {
            return (null, null);
        }

        var tfmDirectory = new DirectoryInfo(assemblyDirectory);
        var configurationDirectory = tfmDirectory.Parent;
        var binDirectory = configurationDirectory?.Parent;
        if (configurationDirectory is null
            || binDirectory is null
            || !string.Equals(binDirectory.Name, "bin", StringComparison.OrdinalIgnoreCase))
        {
            return (null, null);
        }

        return (configurationDirectory.Name, tfmDirectory.Name);
    }

    private static string? FindRepositoryRoot()
    {
        var currentDirectoryRoot = FindRepositoryRootFrom(Directory.GetCurrentDirectory());
        if (!string.IsNullOrWhiteSpace(currentDirectoryRoot))
        {
            return currentDirectoryRoot;
        }

        return FindRepositoryRootFrom(AppContext.BaseDirectory);
    }

    private static string? FindRepositoryRootFrom(string startPath)
    {
        if (string.IsNullOrWhiteSpace(startPath))
        {
            return null;
        }

        var directory = new DirectoryInfo(Path.GetFullPath(startPath));
        while (directory is not null)
        {
            var solutionPath = Path.Combine(directory.FullName, "Jazor.slnx");
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static TPayload DeserializeRequired<TPayload>(object? payload, string name)
    {
        var typed = DeserializeOptional<TPayload>(payload);
        if (typed is null)
        {
            throw new InvalidOperationException($"{name} payload is invalid.");
        }

        return typed;
    }

    private static TPayload? DeserializeOptional<TPayload>(object? payload)
    {
        if (payload is null)
        {
            return default;
        }

        if (payload is JsonElement element)
        {
            return LspJsonSerializer.Deserialize<TPayload>(element.GetRawText());
        }

        if (payload is TPayload typed)
        {
            return typed;
        }

        return LspJsonSerializer.Deserialize<TPayload>(LspJsonSerializer.Serialize(payload));
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ExtensionWorkerClient));
        }
    }

    private void ThrowIfProcessExited()
    {
        if (!_process.HasExited)
        {
            return;
        }

        var message = new StringBuilder();
        message.Append("extension worker process exited");
        message.Append(" (exitCode: ");
        message.Append(_process.ExitCode);
        message.Append(')');

        var stderr = GetCapturedStderr();
        if (!string.IsNullOrWhiteSpace(stderr))
        {
            message.Append(". stderr: ");
            message.Append(stderr.Trim());
        }

        throw new ExtensionWorkerConnectionException(message.ToString());
    }

    private string GetCapturedStderr()
    {
        lock (_stderrBuffer)
        {
            return _stderrBuffer.ToString();
        }
    }
}

internal sealed class ExtensionWorkerConnectionException : InvalidOperationException
{
    public ExtensionWorkerConnectionException(string message)
        : base(message)
    {
    }

    public ExtensionWorkerConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
