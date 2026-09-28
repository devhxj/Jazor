using System.Diagnostics;
using System.Globalization;
using System.Text;
using DenoHost.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Starts and stops the generated project's Vite task through DenoHost.</summary>
internal sealed class JazorViteDevelopmentServer(
    IHostEnvironment environment,
    IHttpClientFactory httpClientFactory,
    IOptions<JazorFrontendOptions> options,
    ILogger<JazorViteDevelopmentServer> logger) : IHostedService, IDisposable
{
    internal const string HttpClientName = "JazorFrontend";
    private readonly IHostEnvironment _environment = environment;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly JazorFrontendOptions _options = options.Value;
    private readonly ILogger<JazorViteDevelopmentServer> _logger = logger;
    private readonly object _errorGate = new();
    private readonly StringBuilder _standardError = new();
    private DenoProcess? _process;
    private bool _ownsProcess;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return;

        var readyUri = _options.GetReadyUri();
        if (await IsReadyAsync(readyUri, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("Using the existing Vite development server at {ServerOrigin}.", _options.Vite.ServerOrigin);
            return;
        }

        if (!_options.Vite.LaunchServer)
        {
            throw new InvalidOperationException(
                $"The configured Vite development server is not ready at '{readyUri}', and LaunchServer is disabled.");
        }

        var projectRoot = _options.ResolveProjectRoot(_environment.ContentRootPath);
        if (!File.Exists(Path.Combine(projectRoot, "package.json")))
        {
            throw new InvalidOperationException(
                $"Jazor frontend project '{projectRoot}' does not contain package.json. Build the Jazor host before starting it.");
        }

        var developmentEntry = Path.Combine(projectRoot, _options.DevelopmentEntryRelativePath);
        if (!File.Exists(developmentEntry))
        {
            throw new InvalidOperationException(
                $"Jazor frontend development entry '{developmentEntry}' was not found. Build the Jazor host before starting it.");
        }

        var arguments = new[]
        {
            "task",
            _options.Vite.TaskName,
            "--host",
            _options.Vite.ServerOrigin.Host,
            "--port",
            _options.Vite.ServerOrigin.Port.ToString(CultureInfo.InvariantCulture),
            "--strictPort",
            "--base",
            _options.GetPublicBasePath()
        };

        // DenoHost process-internal logging must not inherit host providers such as Windows EventLog:
        // a provider failure occurs on DenoHost's reader thread and can terminate the process. The
        // explicit output events below are the single logging boundary owned by this integration.
        _process = new DenoProcess(arguments, projectRoot);
        _process.OutputDataReceived += HandleOutput;
        _process.ErrorDataReceived += HandleError;
        _process.ProcessExited += HandleExit;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            // DenoHost owns runtime discovery, signed-runtime verification, and the child process tree.
            // Cancellation begins only after StartAsync completes so its internal process bookkeeping is never interrupted.
            await _process.StartAsync(CancellationToken.None).ConfigureAwait(false);
            _ownsProcess = true;
            await WaitUntilReadyAsync(readyUri, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation(
                "Vite development server is ready at {ServerOrigin} (Deno PID {ProcessId}).",
                _options.Vite.ServerOrigin,
                _process.ProcessId);
        }
        catch
        {
            await StopOwnedProcessAsync(TimeSpan.Zero).ConfigureAwait(false);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => StopOwnedProcessAsync(_options.Vite.ShutdownTimeout);

    public void Dispose()
    {
        if (_process is not null)
        {
            _process.OutputDataReceived -= HandleOutput;
            _process.ErrorDataReceived -= HandleError;
            _process.ProcessExited -= HandleExit;
            _process.Dispose();
            _process = null;
        }
    }

    private async Task WaitUntilReadyAsync(Uri readyUri, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Vite.StartupTimeout);

        try
        {
            while (true)
            {
                if (await IsReadyAsync(readyUri, timeout.Token).ConfigureAwait(false))
                    return;

                if (_process is not { IsRunning: true })
                {
                    throw CreateProcessFailure(
                        $"The DenoHost Vite task '{_options.Vite.TaskName}' exited with code {_process?.ExitCode} " +
                        $"before '{readyUri}' became ready.");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100), timeout.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw CreateProcessFailure(
                $"The DenoHost Vite task '{_options.Vite.TaskName}' did not make '{readyUri}' ready " +
                $"within {_options.Vite.StartupTimeout}.", timeout: true);
        }
    }

    private async Task<bool> IsReadyAsync(Uri readyUri, CancellationToken cancellationToken)
    {
        using var probeTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        probeTimeout.CancelAfter(TimeSpan.FromSeconds(1));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, readyUri);
            using var response = await _httpClientFactory.CreateClient(HttpClientName)
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, probeTimeout.Token)
                .ConfigureAwait(false);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private async Task StopOwnedProcessAsync(TimeSpan timeout)
    {
        var process = _process;
        if (!_ownsProcess || process is null)
            return;

        _ownsProcess = false;
        try
        {
            if (process.IsRunning)
                await process.StopAsync(timeout, CancellationToken.None).ConfigureAwait(false);
        }
        catch (InvalidOperationException)
        {
            // The task may exit between IsRunning and StopAsync during host shutdown.
        }
    }

    private void HandleOutput(object? sender, DataReceivedEventArgs eventArgs)
    {
        if (!string.IsNullOrWhiteSpace(eventArgs.Data))
            _logger.LogInformation("Vite: {Output}", eventArgs.Data);
    }

    private void HandleError(object? sender, DataReceivedEventArgs eventArgs)
    {
        if (string.IsNullOrWhiteSpace(eventArgs.Data))
            return;

        lock (_errorGate)
            _standardError.AppendLine(eventArgs.Data);
        _logger.LogWarning("Vite: {Output}", eventArgs.Data);
    }

    private void HandleExit(object? sender, ProcessExitedEventArgs eventArgs)
    {
        if (_ownsProcess)
            _logger.LogInformation("DenoHost Vite process exited with code {ExitCode}.", eventArgs.ExitCode);
    }

    private Exception CreateProcessFailure(string message, bool timeout = false)
    {
        lock (_errorGate)
        {
            var detail = _standardError.Length == 0
                ? message
                : message + Environment.NewLine + _standardError;
            return timeout ? new TimeoutException(detail) : new InvalidOperationException(detail);
        }
    }
}
