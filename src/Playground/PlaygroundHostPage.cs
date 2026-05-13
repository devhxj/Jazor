using System.Text;
using Microsoft.AspNetCore.StaticFiles;

namespace Playground;

internal static class PlaygroundHostPage
{
    private const string HtmlCacheControl = "no-store, no-cache";
    private const string MutableAssetCacheControl = "no-cache, must-revalidate";

    private const string HtmlShell = """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Jazor Playground</title>
  <meta name="description" content="Production-style RazorVue + Vuetify + Pinia + VueRoute + ASP.NET Core playground." />
  <link rel="stylesheet" href="/jazor/client-entry.css" />
</head>
<body>
  <div id="app"></div>
  <script type="module" src="/jazor/client-entry.js"></script>
</body>
</html>
""";

    public static void ApplySecurityHeaders(IHeaderDictionary headers)
    {
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["X-Frame-Options"] = "DENY";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";
        headers["Permissions-Policy"] =
            "accelerometer=(), autoplay=(), camera=(), display-capture=(), geolocation=(), gyroscope=(), " +
            "hid=(), microphone=(), payment=(), usb=()";
    }

    public static void ApplyStaticAssetHeaders(StaticFileResponseContext context)
    {
        ApplySecurityHeaders(context.Context.Response.Headers);
        context.Context.Response.Headers.CacheControl = MutableAssetCacheControl;
    }

    public static Task WriteHtmlAsync(HttpContext context)
        => WriteHtmlAsync(context, CancellationToken.None);

    public static async Task WriteHtmlAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = HtmlCacheControl;

        if (HttpMethods.IsHead(context.Request.Method))
        {
            return;
        }

        await context.Response.WriteAsync(HtmlShell, Encoding.UTF8, cancellationToken);
    }
}
