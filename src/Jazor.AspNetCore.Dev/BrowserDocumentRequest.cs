using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore.Dev;

/// <summary>Identifies navigation requests whose HTML response can safely receive the reload script.</summary>
internal static class BrowserDocumentRequest
{
    private static readonly HashSet<string> HtmlExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".htm",
        ".html",
        ".xhtml"
    };

    private static readonly HashSet<string> DocumentFetchDestinations = new(StringComparer.OrdinalIgnoreCase)
    {
        "document",
        "frame",
        "iframe"
    };

    /// <summary>Returns whether the request can produce an HTML document suitable for injection.</summary>
    public static bool ShouldInspect(
        HttpContext context,
        PathString clientScriptPath,
        PathString webSocketPath,
        bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!isEnabled)
            return false;

        var request = context.Request;
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            return false;

        var requestPath = request.Path;
        // Do not buffer Jazor's own transport endpoints as HTML documents.
        if (requestPath.StartsWithSegments(clientScriptPath)
            || requestPath.StartsWithSegments(webSocketPath))
        {
            return false;
        }

        if (HasNonHtmlFileExtension(requestPath))
            return false;

        if (IsXmlHttpRequest(request))
            return false;

        if (TryGetSingleHeaderValue(request, "Sec-Fetch-Dest", out var fetchDestination))
            return DocumentFetchDestinations.Contains(fetchDestination);

        if (TryGetSingleHeaderValue(request, "Sec-Fetch-Mode", out var fetchMode)
            && string.Equals(fetchMode, "navigate", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (AcceptsHtmlDocument(request))
            return true;

        return HasHtmlExtension(requestPath) || IsDirectoryLikePath(requestPath);
    }

    private static bool AcceptsHtmlDocument(HttpRequest request)
    {
        var acceptHeaders = request.GetTypedHeaders().Accept;
        if (acceptHeaders is null || acceptHeaders.Count == 0)
            return false;

        foreach (var mediaType in acceptHeaders)
        {
            var value = mediaType.MediaType.Value;
            if (string.IsNullOrWhiteSpace(value))
                continue;

            if (string.Equals(value, "text/html", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "application/xhtml+xml", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasNonHtmlFileExtension(PathString requestPath)
    {
        var extension = Path.GetExtension(requestPath.Value);
        return !string.IsNullOrEmpty(extension) && !HtmlExtensions.Contains(extension);
    }

    private static bool HasHtmlExtension(PathString requestPath)
    {
        var extension = Path.GetExtension(requestPath.Value);
        return !string.IsNullOrEmpty(extension) && HtmlExtensions.Contains(extension);
    }

    private static bool IsDirectoryLikePath(PathString requestPath)
    {
        var value = requestPath.Value;
        return string.IsNullOrEmpty(value)
            || string.Equals(value, "/", StringComparison.Ordinal)
            || value.EndsWith("/", StringComparison.Ordinal);
    }

    private static bool IsXmlHttpRequest(HttpRequest request)
        => TryGetSingleHeaderValue(request, "X-Requested-With", out var requestedWith)
            && string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private static bool TryGetSingleHeaderValue(HttpRequest request, string headerName, out string value)
    {
        value = string.Empty;
        if (!request.Headers.TryGetValue(headerName, out var values))
            return false;

        value = values.ToString().Trim();
        return value.Length > 0;
    }
}
