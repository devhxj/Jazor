using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jazor.AspNetCore.Dev;

internal static class JazorViteProxy
{
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

    public static async Task ForwardAsync(HttpContext context, JazorFrontendOptions options)
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            await ProxyWebSocketAsync(context, options).ConfigureAwait(false);
            return;
        }

        using var request = new HttpRequestMessage(
            new HttpMethod(context.Request.Method),
            options.BuildProxyTarget(context.Request));
        if (context.Request.ContentLength is not null || context.Request.Headers.ContainsKey("Transfer-Encoding"))
            request.Content = new StreamContent(context.Request.Body);
        CopyRequestHeaders(context, request);

        var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
        using var response = await clientFactory.CreateClient(JazorViteDevelopmentServer.HttpClientName)
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted)
            .ConfigureAwait(false);
        context.Response.StatusCode = (int)response.StatusCode;
        CopyResponseHeaders(response, context.Response);
        await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted).ConfigureAwait(false);
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
        {
            if (!HopByHopHeaders.Contains(header.Key))
                target.Headers[header.Key] = header.Value.ToArray();
        }
    }

    private static async Task ProxyWebSocketAsync(HttpContext context, JazorFrontendOptions options)
    {
        using var upstream = new ClientWebSocket();
        foreach (var header in context.Request.Headers)
        {
            if (string.Equals(header.Key, "Host", StringComparison.OrdinalIgnoreCase) ||
                HopByHopHeaders.Contains(header.Key) ||
                header.Key.StartsWith("Sec-WebSocket-", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            upstream.Options.SetRequestHeader(header.Key, header.Value.ToString());
        }

        foreach (var protocol in context.WebSockets.WebSocketRequestedProtocols)
            upstream.Options.AddSubProtocol(protocol);

        var target = options.BuildProxyTarget(context.Request);
        var builder = new UriBuilder(target) { Scheme = target.Scheme == "https" ? "wss" : "ws" };
        await upstream.ConnectAsync(builder.Uri, context.RequestAborted).ConfigureAwait(false);
        using var downstream = await context.WebSockets.AcceptWebSocketAsync(upstream.SubProtocol).ConfigureAwait(false);

        using var relayCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        var clientToServer = RelayAsync(downstream, upstream, relayCancellation.Token);
        var serverToClient = RelayAsync(upstream, downstream, relayCancellation.Token);
        try
        {
            var completed = await Task.WhenAny(clientToServer, serverToClient).ConfigureAwait(false);
            await completed.ConfigureAwait(false);
            // The relay that receives a close owns forwarding CloseOutput. Let its peer acknowledge
            // before cancellation tears down the other receive operation.
            await Task.WhenAll(clientToServer, serverToClient).ConfigureAwait(false);
        }
        finally
        {
            await relayCancellation.CancelAsync().ConfigureAwait(false);
            try
            {
                await Task.WhenAll(clientToServer, serverToClient).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (relayCancellation.IsCancellationRequested)
            {
            }
        }
    }

    private static async Task RelayAsync(WebSocket source, WebSocket destination, CancellationToken cancellationToken)
    {
        var buffer = new byte[64 * 1024];
        while (!cancellationToken.IsCancellationRequested &&
               source.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            var result = await source.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await destination.CloseOutputAsync(
                    result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                    result.CloseStatusDescription,
                    cancellationToken).ConfigureAwait(false);
                return;
            }

            await destination.SendAsync(
                buffer.AsMemory(0, result.Count),
                result.MessageType,
                result.EndOfMessage,
                cancellationToken).ConfigureAwait(false);
        }
    }
}
