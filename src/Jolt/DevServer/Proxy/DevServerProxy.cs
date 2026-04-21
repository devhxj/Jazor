using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Jolt.DevServer;

internal sealed class DevServerProxy : IDisposable
{
    private static readonly TimeSpan DefaultHttpTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan WebSocketRelayShutdownGracePeriod = TimeSpan.FromMilliseconds(500);

    private readonly IReadOnlyList<KeyValuePair<string, ProxyTarget>> _proxyRules;
    private readonly HashSet<string> _insecureAuthorities;
    private readonly HttpClient _httpClient;

    public DevServerProxy(
        IReadOnlyDictionary<string, ProxyTarget> proxyRules,
        HttpMessageHandler? messageHandler = null)
    {
        ArgumentNullException.ThrowIfNull(proxyRules);

        _proxyRules = proxyRules
            .OrderByDescending(static rule => rule.Key.Length)
            .ToArray();
        _insecureAuthorities = _proxyRules
            .Select(static rule => rule.Value)
            .Where(static target => !target.Secure)
            .Select(static target => Uri.TryCreate(target.Target, UriKind.Absolute, out var targetUri)
                ? targetUri
                : null)
            .Where(static targetUri => targetUri is not null
                && string.Equals(targetUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            .Select(static targetUri => targetUri!.Authority)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _httpClient = messageHandler is null
            ? CreateHttpClient()
            : new HttpClient(messageHandler, disposeHandler: false)
            {
                Timeout = DefaultHttpTimeout
            };
    }

    public async Task<bool> TryProxyAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var requestPath = context.Request.Path.Value;
        if (string.IsNullOrWhiteSpace(requestPath))
        {
            return false;
        }

        foreach (var (prefix, target) in _proxyRules)
        {
            if (!PathMatchesPrefix(requestPath, prefix))
            {
                continue;
            }

            if (IsWebSocketHandshakeRequest(context.Request))
            {
                if (!target.WebSocket)
                {
                    return false;
                }

                await ForwardWebSocketAsync(context, prefix, target);
                return true;
            }

            await ForwardHttpAsync(context, prefix, target);
            return true;
        }

        return false;
    }

    private async Task ForwardHttpAsync(
        HttpContext context,
        string prefix,
        ProxyTarget target)
    {
        using var requestMessage = new HttpRequestMessage(
            new HttpMethod(context.Request.Method),
            BuildTargetUri(context, prefix, target, useWebSocket: false));

        var hasBody = (context.Request.ContentLength ?? 0) > 0
            || context.Request.Headers.ContainsKey(HeaderNames.TransferEncoding);
        if (hasBody)
        {
            requestMessage.Content = new StreamContent(context.Request.Body);
        }

        CopyRequestHeaders(context, requestMessage);

        using var responseMessage = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted);

        context.Response.StatusCode = (int)responseMessage.StatusCode;
        CopyResponseHeaders(context.Response, responseMessage);
        await responseMessage.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
    }

    private async Task ForwardWebSocketAsync(
        HttpContext context,
        string prefix,
        ProxyTarget target)
    {
        var targetUri = BuildTargetUri(context, prefix, target, useWebSocket: true);
        using var upstreamSocket = new ClientWebSocket();
        if (!target.Secure
            && string.Equals(targetUri.Scheme, "wss", StringComparison.OrdinalIgnoreCase))
        {
            upstreamSocket.Options.RemoteCertificateValidationCallback = AcceptInsecureServerCertificate;
        }

        foreach (var protocol in context.WebSockets.WebSocketRequestedProtocols)
        {
            upstreamSocket.Options.AddSubProtocol(protocol);
        }

        foreach (var header in context.Request.Headers)
        {
            if (ShouldSkipWebSocketRequestHeader(header.Key))
            {
                continue;
            }

            try
            {
                upstreamSocket.Options.SetRequestHeader(header.Key, header.Value.ToString());
            }
            catch (ArgumentException)
            {
            }
        }

        await upstreamSocket.ConnectAsync(targetUri, context.RequestAborted);
        using var downstreamSocket = await context.WebSockets.AcceptWebSocketAsync(upstreamSocket.SubProtocol);

        using var relayCancellation = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
        var relayToken = relayCancellation.Token;
        var upstreamRelay = RelayWebSocketAsync(downstreamSocket, upstreamSocket, relayToken);
        var downstreamRelay = RelayWebSocketAsync(upstreamSocket, downstreamSocket, relayToken);
        var relayTasks = Task.WhenAll(upstreamRelay, downstreamRelay);

        try
        {
            await Task.WhenAny(upstreamRelay, downstreamRelay);
            var completedWithinGrace = await Task.WhenAny(
                relayTasks,
                Task.Delay(WebSocketRelayShutdownGracePeriod, context.RequestAborted));
            if (completedWithinGrace != relayTasks)
            {
                relayCancellation.Cancel();
            }

            await relayTasks;
        }
        catch (OperationCanceledException) when (relayToken.IsCancellationRequested || context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (WebSocketException) when (relayToken.IsCancellationRequested || context.RequestAborted.IsCancellationRequested)
        {
        }
        finally
        {
            relayCancellation.Cancel();
            await CloseWebSocketPairAsync(downstreamSocket, upstreamSocket, CancellationToken.None);
        }
    }

    private static Uri BuildTargetUri(
        HttpContext context,
        string prefix,
        ProxyTarget target,
        bool useWebSocket)
    {
        var targetUri = new Uri(target.Target, UriKind.Absolute);
        var requestPath = context.Request.Path.Value ?? "/";
        var pathRemainder = requestPath.Length == prefix.Length
            ? string.Empty
            : requestPath[prefix.Length..];
        var effectivePath = string.IsNullOrWhiteSpace(target.RewritePath)
            ? pathRemainder
            : target.RewritePath;
        var builder = new UriBuilder(targetUri)
        {
            Path = CombinePaths(targetUri.AbsolutePath, effectivePath),
            Query = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value![1..]
                : string.Empty
        };
        if (useWebSocket)
        {
            builder.Scheme = targetUri.Scheme switch
            {
                "https" => "wss",
                "http" => "ws",
                _ => targetUri.Scheme
            };
            builder.Port = targetUri.IsDefaultPort ? -1 : targetUri.Port;
        }

        return builder.Uri;
    }

    private HttpClient CreateHttpClient()
    {
        if (_insecureAuthorities.Count == 0)
        {
            return new HttpClient
            {
                Timeout = DefaultHttpTimeout
            };
        }

        return new HttpClient(
            new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = ValidateServerCertificate
            })
        {
            Timeout = DefaultHttpTimeout
        };
    }

    private bool ValidateServerCertificate(
        HttpRequestMessage requestMessage,
        X509Certificate2? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        return requestMessage.RequestUri is not null
            && _insecureAuthorities.Contains(requestMessage.RequestUri.Authority);
    }

    private static bool AcceptInsecureServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
        => true;

    private static void CopyRequestHeaders(
        HttpContext context,
        HttpRequestMessage requestMessage)
    {
        foreach (var header in context.Request.Headers)
        {
            if (string.Equals(header.Key, HeaderNames.Host, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                requestMessage.Content ??= new ByteArrayContent([]);
                requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }

    private static bool ShouldSkipWebSocketRequestHeader(string headerName)
        => string.Equals(headerName, HeaderNames.Host, StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerName, HeaderNames.Connection, StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerName, HeaderNames.Upgrade, StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerName, HeaderNames.SecWebSocketAccept, StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerName, HeaderNames.SecWebSocketKey, StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerName, HeaderNames.SecWebSocketVersion, StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerName, HeaderNames.SecWebSocketProtocol, StringComparison.OrdinalIgnoreCase)
            || string.Equals(headerName, HeaderNames.SecWebSocketExtensions, StringComparison.OrdinalIgnoreCase);

    private static bool IsWebSocketHandshakeRequest(HttpRequest request)
    {
        if (!HttpMethods.IsGet(request.Method))
        {
            return false;
        }

        var connection = request.Headers.Connection.ToString();
        var upgrade = request.Headers.Upgrade.ToString();
        return connection.Contains("Upgrade", StringComparison.OrdinalIgnoreCase)
            && string.Equals(upgrade, "websocket", StringComparison.OrdinalIgnoreCase);
    }

    private static void CopyResponseHeaders(
        HttpResponse response,
        HttpResponseMessage responseMessage)
    {
        foreach (var header in responseMessage.Headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            response.Headers[header.Key] = header.Value.ToArray();
        }

        response.Headers.Remove(HeaderNames.TransferEncoding);
    }

    private static bool PathMatchesPrefix(string requestPath, string prefix)
    {
        if (!requestPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return requestPath.Length == prefix.Length
            || prefix.EndsWith("/", StringComparison.Ordinal)
            || requestPath[prefix.Length] == '/';
    }

    private static string CombinePaths(string left, string right)
    {
        var normalizedLeft = string.IsNullOrWhiteSpace(left) ? "/" : left;
        var normalizedRight = string.IsNullOrWhiteSpace(right) ? string.Empty : right;
        if (!normalizedLeft.StartsWith("/", StringComparison.Ordinal))
        {
            normalizedLeft = "/" + normalizedLeft;
        }

        if (normalizedLeft.Length > 1)
        {
            normalizedLeft = normalizedLeft.TrimEnd('/');
        }

        normalizedRight = normalizedRight.Trim();
        if (normalizedRight.Length == 0)
        {
            return normalizedLeft;
        }

        if (!normalizedRight.StartsWith("/", StringComparison.Ordinal))
        {
            normalizedRight = "/" + normalizedRight;
        }

        return normalizedLeft == "/" ? normalizedRight : normalizedLeft + normalizedRight;
    }

    private static async Task RelayWebSocketAsync(
        WebSocket source,
        WebSocket destination,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[16 * 1024];
        while (!cancellationToken.IsCancellationRequested)
        {
            var result = await source.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ForwardCloseAsync(source, destination, cancellationToken);
                break;
            }

            await destination.SendAsync(
                new ArraySegment<byte>(buffer, 0, result.Count),
                result.MessageType,
                result.EndOfMessage,
                cancellationToken);
        }
    }

    private static async Task ForwardCloseAsync(
        WebSocket source,
        WebSocket destination,
        CancellationToken cancellationToken)
    {
        try
        {
            if (destination.State is not (WebSocketState.Open or WebSocketState.CloseReceived))
            {
                return;
            }

            var closeStatus = source.CloseStatus ?? WebSocketCloseStatus.NormalClosure;
            var closeDescription = source.CloseStatusDescription ?? "Proxy close";
            if (destination.State == WebSocketState.Open)
            {
                await destination.CloseOutputAsync(closeStatus, closeDescription, cancellationToken);
                return;
            }

            await destination.CloseAsync(closeStatus, closeDescription, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (WebSocketException)
        {
        }
    }

    private static async Task CloseWebSocketPairAsync(
        WebSocket downstreamSocket,
        ClientWebSocket upstreamSocket,
        CancellationToken cancellationToken)
    {
        try
        {
            if (downstreamSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await downstreamSocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Proxy shutdown",
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }

        try
        {
            if (upstreamSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
            {
                await upstreamSocket.CloseOutputAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Proxy shutdown",
                    cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (WebSocketException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public void Dispose()
        => _httpClient.Dispose();
}

internal sealed class ProxyTarget
{
    public required string Target { get; init; }

    public bool Secure { get; init; }

    public bool WebSocket { get; init; } = true;

    public string? RewritePath { get; init; }
}
