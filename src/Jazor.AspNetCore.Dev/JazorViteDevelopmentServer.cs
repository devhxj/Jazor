using Jazor.NodeHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Owns an embedded Vite server and its NodeApi environment.</summary>
internal sealed class JazorViteDevelopmentServer(
    IHostEnvironment environment,
    IHttpClientFactory httpClientFactory,
    IOptions<JazorSPAOptions> options,
    ILogger<JazorViteDevelopmentServer> logger) : IHostedService, IAsyncDisposable
{
    internal const string HttpClientName = "JazorSPA";
    private NodeViteServer? _server;
    private JoltDevelopmentSession? _jolt;
    private readonly JazorSPAOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
            return;

        var readyUri = _options.GetReadyUri();
        var tool = _options.JoltHost is null && _options.JoltTool is null
            ? null : new JoltDevelopmentTool(_options.JoltHost, _options.JoltTool);
        Uri? ViteClient() => tool is null ? null : new Uri(_options.GetServerOrigin(), _options.GetPublicBasePath() + "@vite/client");
        // A ready listener only identifies an external session when the host was configured to attach.
        if ((tool is null || !_options.Vite.LaunchServer) &&
            await IsReadyAsync(readyUri, ViteClient(), cancellationToken).ConfigureAwait(false))
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
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(_options.Vite.StartupTimeout);
            // Cancellation closes and joins the environment, including an
            // unfinished startup; it cannot leave a detached listener behind.
            if (tool is null)
            {
                _server = new NodeViteServer(projectRoot);
                await _server.StartAsync(projectRoot, _options.Vite.ServerOrigin.Host,
                    _options.Vite.ServerOrigin.Port, _options.GetPublicBasePath(), timeout.Token).ConfigureAwait(false);
            }
            else
            {
                var origin = _options.Vite.ServerOrigin;
                var port = origin == JazorViteServerOptions.DefaultServerOrigin ? 0 : origin.Port;
                _jolt = JoltDevelopmentSession.Start(tool, environment.ContentRootPath, projectRoot, _options, port, timeout.Token);
                _options.OwnedServerOrigin = await _jolt.WaitForOriginAsync(timeout.Token).ConfigureAwait(false);
                readyUri = _options.GetReadyUri();
            }
            cancellationToken.ThrowIfCancellationRequested();
            while (!await IsReadyAsync(readyUri, ViteClient(), timeout.Token).ConfigureAwait(false))
            {
                _jolt?.RequireRunning();
                await Task.Delay(100, timeout.Token).ConfigureAwait(false);
            }
            _jolt?.RequireRunning();
            logger.LogInformation("{DevelopmentServer} is ready at {ServerOrigin}.",
                tool is null ? "Embedded Vite" : "Jolt", _options.GetServerOrigin());
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
        var jolt = Interlocked.Exchange(ref _jolt, null);
        if (jolt is not null)
        {
            _options.OwnedServerOrigin = null;
            await jolt.DisposeAsync().ConfigureAwait(false);
        }
        var server = Interlocked.Exchange(ref _server, null);
        if (server is not null)
        {
            using var timeout = new CancellationTokenSource(_options.Vite.ShutdownTimeout);
            await server.StopAsync(timeout.Token).ConfigureAwait(false);
        }
    }

    private async Task<bool> IsReadyAsync(Uri readyUri, Uri? viteClient, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(1));
        try
        {
            using var client = httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(readyUri, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode || viteClient is null)
                return response.IsSuccessStatusCode;
            if (response.Content.Headers.ContentType?.MediaType != "text/html")
                return false;
            using var script = await client.GetAsync(viteClient, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            return script.IsSuccessStatusCode && script.Content.Headers.ContentType?.MediaType is "text/javascript" or "application/javascript";
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
