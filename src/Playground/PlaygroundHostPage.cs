using System.Text;

namespace Playground;

internal static class PlaygroundHostPage
{
    private const string HtmlCacheControl = "no-store, no-cache";

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
