using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Jolt.Build;

internal sealed class BundlerModuleProxyServer : IAsyncDisposable
{
    private static readonly TimeSpan DefaultProxyTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultPooledConnectionLifetime = TimeSpan.FromMinutes(2);
    private static readonly string[] AuthoredModuleExtensions =
    [
        ".jazor",
        ".vue"
    ];

    private readonly Uri _originBaseUri;
    private readonly string _requestPrefix;
    private readonly HttpClient _httpClient;
    private Uri? _listeningUri;
    private WebApplication? _application;

    private BundlerModuleProxyServer(Uri originEntryUri)
    {
        _originBaseUri = new Uri(originEntryUri.GetLeftPart(UriPartial.Authority));
        _requestPrefix = "/__jazor_bundle/" + Guid.NewGuid().ToString("N") + "/";
        _httpClient = new HttpClient(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            UseProxy = false,
            PooledConnectionLifetime = DefaultPooledConnectionLifetime
        })
        {
            Timeout = DefaultProxyTimeout
        };
    }

    public Uri ListeningUri
        => _listeningUri
            ?? throw new InvalidOperationException("Bundler proxy is not started.");

    public static async Task<BundlerModuleProxyServer> StartAsync(
        Uri originEntryUri,
        CancellationToken cancellationToken)
    {
        var server = new BundlerModuleProxyServer(originEntryUri);
        await server.StartCoreAsync(cancellationToken);
        return server;
    }

    public Uri CreateBundlerEntryUri(Uri originEntryUri)
        => new(ListeningUri, ToBundlerRequestPath(RewriteSpecifierForBundler(originEntryUri.PathAndQuery, _requestPrefix)));

    public async ValueTask DisposeAsync()
    {
        _httpClient.Dispose();
        if (_application is null)
        {
            return;
        }

        await _application.DisposeAsync();
        _application = null;
    }

    private async Task StartCoreAsync(CancellationToken cancellationToken)
    {
        if (_application is not null)
        {
            return;
        }

        var port = GetAvailablePort();
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls($"http://127.0.0.1:{port}");

        var application = builder.Build();
        application.Map(
            "/{**requestPath}",
            async context =>
            {
                if (!HttpMethods.IsGet(context.Request.Method)
                    && !HttpMethods.IsHead(context.Request.Method))
                {
                    context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                    return;
                }

                await ProxyAsync(context);
            });

        await application.StartAsync(cancellationToken);
        _application = application;
        _listeningUri = ResolveListeningUri(application)
            ?? throw new InvalidOperationException("Failed to resolve bundler proxy listening URI.");
    }

    private async Task ProxyAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.HasValue
            ? context.Request.Path.Value!
            : "/";
        var originRequestPath = MapBundlerRequestPathToOriginPath(requestPath);
        if (context.Request.QueryString.HasValue)
        {
            originRequestPath += context.Request.QueryString.Value;
        }

        var originUri = new Uri(_originBaseUri, originRequestPath);
        using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), originUri);
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

        context.Response.StatusCode = (int)response.StatusCode;
        if (!response.IsSuccessStatusCode)
        {
            await WriteSanitizedErrorResponseAsync(context, response.StatusCode, context.RequestAborted);
            return;
        }

        if (response.Content.Headers.ContentType is MediaTypeHeaderValue contentType)
        {
            context.Response.ContentType = contentType.ToString();
        }

        if (HttpMethods.IsHead(context.Request.Method))
        {
            return;
        }

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (IsJavaScriptMediaType(mediaType))
        {
            var content = await response.Content.ReadAsStringAsync(context.RequestAborted);
            var rewrittenContent = RewriteJavaScriptSpecifiers(content);
            await context.Response.WriteAsync(rewrittenContent, Encoding.UTF8, context.RequestAborted);
            return;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(context.RequestAborted);
        await context.Response.Body.WriteAsync(bytes, context.RequestAborted);
    }

    private static async Task WriteSanitizedErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        CancellationToken cancellationToken)
    {
        if (HttpMethods.IsHead(context.Request.Method))
        {
            return;
        }

        context.Response.ContentType = "text/plain; charset=utf-8";
        await context.Response.WriteAsync(
            $"Bundler upstream request failed with status {(int)statusCode} ({statusCode}).",
            Encoding.UTF8,
            cancellationToken);
    }

    private static bool IsJavaScriptMediaType(string? mediaType)
        => string.Equals(mediaType, "text/javascript", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mediaType, "application/javascript", StringComparison.OrdinalIgnoreCase);

    private string RewriteJavaScriptSpecifiers(string content)
        => JavaScriptModuleSpecifierScanner.RewriteSpecifiers(
            content,
            specifier =>
            {
                var rewrittenSpecifier = RewriteSpecifierForBundler(specifier.Value, _requestPrefix);
                return string.Equals(rewrittenSpecifier, specifier.Value, StringComparison.Ordinal)
                    ? null
                    : rewrittenSpecifier;
            });

    private static string RewriteSpecifierForBundler(string specifier, string requestPrefix)
    {
        if (string.IsNullOrWhiteSpace(specifier))
        {
            return specifier;
        }

        if (Uri.TryCreate(specifier, UriKind.Absolute, out var absoluteUri))
        {
            if (!IsAuthoredModulePath(absoluteUri.AbsolutePath))
            {
                return specifier;
            }

            var builder = new UriBuilder(absoluteUri)
            {
                Path = absoluteUri.AbsolutePath + ".js"
            };
            return builder.Uri.AbsoluteUri;
        }

        var suffixIndex = specifier.IndexOfAny(['?', '#']);
        var path = suffixIndex >= 0
            ? specifier[..suffixIndex]
            : specifier;

        if (path.StartsWith("/", StringComparison.Ordinal)
            && !path.StartsWith(requestPrefix, StringComparison.OrdinalIgnoreCase))
        {
            path = requestPrefix + path[1..];
        }

        var rewrittenPath = IsAuthoredModulePath(path)
            ? path + ".js"
            : path;

        if (string.Equals(rewrittenPath, path, StringComparison.Ordinal)
            && suffixIndex < 0)
        {
            return specifier;
        }

        return suffixIndex >= 0
            ? string.Concat(rewrittenPath, specifier.AsSpan(suffixIndex))
            : rewrittenPath;
    }

    private string MapBundlerRequestPathToOriginPath(string requestPath)
    {
        requestPath = StripRequestPrefix(requestPath);

        foreach (var extension in AuthoredModuleExtensions)
        {
            var aliasSuffix = extension + ".js";
            if (requestPath.EndsWith(aliasSuffix + ".map", StringComparison.OrdinalIgnoreCase))
            {
                return requestPath[..^3];
            }

            if (requestPath.EndsWith(aliasSuffix, StringComparison.OrdinalIgnoreCase))
            {
                return requestPath[..^3];
            }
        }

        return requestPath;
    }

    private static bool IsAuthoredModulePath(string path)
        => AuthoredModuleExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));

    private string ToBundlerRequestPath(string pathOrPathAndQuery)
    {
        if (string.IsNullOrWhiteSpace(pathOrPathAndQuery))
        {
            return _requestPrefix;
        }

        if (pathOrPathAndQuery.StartsWith(_requestPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return pathOrPathAndQuery;
        }

        var suffixIndex = pathOrPathAndQuery.IndexOfAny(['?', '#']);
        var path = suffixIndex >= 0
            ? pathOrPathAndQuery[..suffixIndex]
            : pathOrPathAndQuery;
        var suffix = suffixIndex >= 0
            ? pathOrPathAndQuery[suffixIndex..]
            : string.Empty;

        if (path.StartsWith(_requestPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return path + suffix;
        }

        var trimmedPath = path.StartsWith("/", StringComparison.Ordinal)
            ? path[1..]
            : path;

        return _requestPrefix + trimmedPath + suffix;
    }

    private string StripRequestPrefix(string path)
    {
        if (path.StartsWith(_requestPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var trimmed = path[_requestPrefix.Length..];
            return "/" + trimmed;
        }

        return path;
    }

    private static Uri? ResolveListeningUri(WebApplication application)
    {
        foreach (var address in application.Urls)
        {
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                return uri;
            }
        }

        return null;
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
