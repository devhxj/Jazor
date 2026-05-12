using System.Text;

namespace Playground;

internal static class PlaygroundHostPage
{
    private const string HtmlShell = """
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>Jazor Playground</title>
  <meta name="description" content="Production-style RazorVue + Vuetify + Pinia + VueRoute + ASP.NET Core playground." />
</head>
<body>
  <div id="app"></div>
  <script type="module" src="/assets/client-entry.js"></script>
</body>
</html>
""";

    public static bool IsHtmlShellPath(PathString path)
    {
        if (path == "/" || path == string.Empty)
        {
            return true;
        }

        return !Path.HasExtension(path.Value);
    }

    public static Task WriteHtmlAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.Headers.CacheControl = "no-store, no-cache";
        return context.Response.WriteAsync(HtmlShell, Encoding.UTF8);
    }
}
