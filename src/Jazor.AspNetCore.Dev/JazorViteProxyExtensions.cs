using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore.Dev;

/// <summary>Forwards development browser requests and Vite HMR sockets to the standard project server.</summary>
public static class JazorViteProxyExtensions
{
    private const string RegisteredKey = "__JazorViteProxyRegistered";
    private static readonly HashSet<string> HopByHopHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Connection",
        "Keep-Alive",
        "Proxy-Authenticate",
        "Proxy-Authorization",
        "TE",
        "Trailer",
        "Transfer-Encoding",
        "Upgrade"
    };

    /// <summary>Registers the Vite proxy with default options.</summary>
    public static IServiceCollection AddJazorViteProxy(this IServiceCollection services)
        => services.AddJazorViteProxy(configure: null);

    /// <summary>Registers the Vite proxy with explicit options.</summary>
    public static IServiceCollection AddJazorViteProxy(
        this IServiceCollection services,
        Action<JazorViteOptions>? configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        var options = services.AddOptions<JazorViteOptions>();
        if (configure is not null)
            options.Configure(configure);
        services.AddHttpClient("JazorViteProxy");
        return services;
    }

    /// <summary>Maps the configured project prefix to Vite HTTP and WebSocket endpoints.</summary>
    /// <remarks>Vite/Deno owns file serving and HMR. ASP.NET Core only forwards the connection.</remarks>
    public static IApplicationBuilder UseJazorViteProxy(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (app.Properties.ContainsKey(RegisteredKey))
            return app;
        app.Properties[RegisteredKey] = true;

        var options = app.ApplicationServices.GetRequiredService<IOptions<JazorViteOptions>>().Value;
        if (!options.RequestPath.HasValue || options.RequestPath == "/")
            throw new InvalidOperationException("Jazor Vite proxy RequestPath must be a non-root path.");

        app.UseWebSockets();
        app.Map(options.RequestPath.Value!, branch => branch.Run(ProxyAsync));
        return app;

        async Task ProxyAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                await ProxyWebSocketAsync(context, options).ConfigureAwait(false);
                return;
            }

            using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), options.BuildTarget(context.Request));
            if (context.Request.ContentLength is not null || context.Request.Headers.ContainsKey("Transfer-Encoding"))
                request.Content = new StreamContent(context.Request.Body);
            CopyRequestHeaders(context, request);

            var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
            using var response = await clientFactory.CreateClient("JazorViteProxy")
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted)
                .ConfigureAwait(false);
            context.Response.StatusCode = (int)response.StatusCode;
            CopyResponseHeaders(response, context.Response);
            await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted).ConfigureAwait(false);
        }
    }

    private static void CopyRequestHeaders(HttpContext context, HttpRequestMessage request)
    {
        foreach (var header in context.Request.Headers)
        {
            if (string.Equals(header.Key, "Host", StringComparison.OrdinalIgnoreCase) ||
                HopByHopHeaders.Contains(header.Key))
            {
                continue;
            }
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    private static void CopyResponseHeaders(HttpResponseMessage response, HttpResponse target)
    {
        foreach (var header in response.Headers)
        {
            if (!HopByHopHeaders.Contains(header.Key))
                target.Headers[header.Key] = header.Value.ToArray();
        }
        foreach (var header in response.Content.Headers)
            target.Headers[header.Key] = header.Value.ToArray();
    }

    private static async Task ProxyWebSocketAsync(HttpContext context, JazorViteOptions options)
    {
        using var upstream = new ClientWebSocket();
        foreach (var header in context.Request.Headers)
        {
            if (string.Equals(header.Key, "Host", StringComparison.OrdinalIgnoreCase) ||
                HopByHopHeaders.Contains(header.Key) ||
                header.Key.StartsWith("Sec-WebSocket-", StringComparison.OrdinalIgnoreCase))
                continue;
            upstream.Options.SetRequestHeader(header.Key, header.Value.ToString());
        }
        foreach (var protocol in context.WebSockets.WebSocketRequestedProtocols)
            upstream.Options.AddSubProtocol(protocol);

        var target = options.BuildTarget(context.Request);
        var builder = new UriBuilder(target) { Scheme = target.Scheme == "https" ? "wss" : "ws" };
        await upstream.ConnectAsync(builder.Uri, context.RequestAborted).ConfigureAwait(false);
        var acceptedProtocol = upstream.SubProtocol;
        using var downstream = await context.WebSockets.AcceptWebSocketAsync(acceptedProtocol).ConfigureAwait(false);

        using var relayCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        var clientToServer = RelayAsync(downstream, upstream, relayCancellation.Token);
        var serverToClient = RelayAsync(upstream, downstream, relayCancellation.Token);
        try
        {
            var completed = await Task.WhenAny(clientToServer, serverToClient).ConfigureAwait(false);
            await completed.ConfigureAwait(false);
            // A normal close is forwarded in each direction by the relay that owns its send.
            // Let the peer acknowledge it before cancellation tears down the receive operation.
            await Task.WhenAll(clientToServer, serverToClient).ConfigureAwait(false);
        }
        finally
        {
            // CloseAsync would start a second receive while the other relay still owns it.
            // Cancel and observe both relays before disposing their sockets instead.
            await relayCancellation.CancelAsync().ConfigureAwait(false);
            try
            {
                await Task.WhenAll(clientToServer, serverToClient).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (relayCancellation.IsCancellationRequested) { }
        }
    }

    private static async Task RelayAsync(WebSocket source, WebSocket destination, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        while (!cancellationToken.IsCancellationRequested && source.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            var result = await source.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await destination.CloseOutputAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                    result.CloseStatusDescription, cancellationToken).ConfigureAwait(false);
                return;
            }
            await destination.SendAsync(buffer.AsMemory(0, result.Count), result.MessageType, result.EndOfMessage, cancellationToken).ConfigureAwait(false);
        }
    }
}
