using Jazor.NodeHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Owns an embedded Vite server and its NodeApi environment.</summary>
internal sealed class JazorViteDevelopmentServer(
    IHostEnvironment environment,
    IHttpClientFactory httpClientFactory,
    IOptions<JazorFrontendOptions> options,
    ILogger<JazorViteDevelopmentServer> logger) : IHostedService, IAsyncDisposable
{
    internal const string HttpClientName = "JazorFrontend";
    private NodeViteServer? _server;
    private readonly JazorFrontendOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return;

        var readyUri = _options.GetReadyUri();
        if (await IsReadyAsync(readyUri, cancellationToken).ConfigureAwait(false))
        {
            logger.LogInformation("Using the existing Vite development server at {ServerOrigin}.", _options.Vite.ServerOrigin);
            return;
        }
        if (!_options.Vite.LaunchServer)
            throw new InvalidOperationException($"The configured Vite development server is not ready at '{readyUri}', and LaunchServer is disabled.");

        var projectRoot = _options.ResolveProjectRoot(environment.ContentRootPath);
        if (!File.Exists(Path.Combine(projectRoot, "package.json")) ||
            !File.Exists(Path.Combine(projectRoot, _options.DevelopmentEntryRelativePath)))
            throw new InvalidOperationException($"Jazor frontend project '{projectRoot}' requires package.json and '{_options.DevelopmentEntryRelativePath}'. Build the Jazor host before starting it.");

        cancellationToken.ThrowIfCancellationRequested();
        _server = new NodeViteServer(projectRoot);
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_options.Vite.StartupTimeout);
            // Cancellation closes and joins the environment, including an
            // unfinished startup; it cannot leave a detached listener behind.
            await _server.StartAsync(projectRoot, _options.Vite.ServerOrigin.Host,
                _options.Vite.ServerOrigin.Port, _options.GetPublicBasePath(), timeout.Token).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            while (!await IsReadyAsync(readyUri, timeout.Token).ConfigureAwait(false))
                await Task.Delay(100, timeout.Token).ConfigureAwait(false);
            logger.LogInformation("Embedded Vite is ready at {ServerOrigin} (.NET PID {ProcessId}).",
                _options.Vite.ServerOrigin, Environment.ProcessId);
        }
        catch
        {
            await DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => DisposeAsync().AsTask();

    public async ValueTask DisposeAsync()
    {
        var server = Interlocked.Exchange(ref _server, null);
        if (server is not null)
        {
            using var timeout = new CancellationTokenSource(_options.Vite.ShutdownTimeout);
            await server.StopAsync(timeout.Token).ConfigureAwait(false);
        }
    }

    private async Task<bool> IsReadyAsync(Uri readyUri, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(1));
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, readyUri);
            using var response = await httpClientFactory.CreateClient(HttpClientName)
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
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
}
