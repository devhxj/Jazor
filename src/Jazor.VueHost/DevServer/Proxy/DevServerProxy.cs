using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Jazor.VueHost.DevServer;

internal sealed class DevServerProxy : IDisposable
{
    private readonly IReadOnlyList<KeyValuePair<string, ProxyTarget>> _proxyRules;
    private readonly HttpClient _httpClient;

    public DevServerProxy(
        IReadOnlyDictionary<string, ProxyTarget> proxyRules,
        HttpMessageHandler? messageHandler = null)
    {
        ArgumentNullException.ThrowIfNull(proxyRules);

        _proxyRules = proxyRules
            .OrderByDescending(static rule => rule.Key.Length)
            .ToArray();
        _httpClient = messageHandler is null
            ? new HttpClient()
            : new HttpClient(messageHandler, disposeHandler: false);
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

            await ForwardAsync(context, prefix, target);
            return true;
        }

        return false;
    }

    private async Task ForwardAsync(
        HttpContext context,
        string prefix,
        ProxyTarget target)
    {
        using var requestMessage = new HttpRequestMessage(
            new HttpMethod(context.Request.Method),
            BuildTargetUri(context, prefix, target));

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

    private static Uri BuildTargetUri(
        HttpContext context,
        string prefix,
        ProxyTarget target)
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
        return builder.Uri;
    }

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
