using System.Text.Json;
using HostFile = System.IO.File;

namespace JazorAdmin.DemoClient;

internal static class DemoShell
{
    private const string Document = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>JazorAdmin Operations Demo</title>
          <script type="importmap">{0}</script>
          {1}
        </head>
        <body>
          <div id="app"></div>
          <script type="module" src="/jazor/app.mjs"></script>
        </body>
        </html>
        """;

    public static Task WriteAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        if (HttpMethods.IsHead(context.Request.Method))
            return Task.CompletedTask;

        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var root = Path.Combine(environment.ContentRootPath, "jazor");
        var importMap = ReadJson(Path.Combine(root, "importmap.json"), "{\"imports\":{}}");
        var styles = ReadStyles(Path.Combine(root, "manifest.json"));
        return context.Response.WriteAsync(string.Format(Document, importMap, styles), cancellationToken);
    }

    private static string ReadJson(string path, string fallback)
        => HostFile.Exists(path) ? HostFile.ReadAllText(path) : fallback;

    private static string ReadStyles(string path)
    {
        if (!HostFile.Exists(path))
            return string.Empty;

        using var document = JsonDocument.Parse(HostFile.ReadAllText(path));
        if (!document.RootElement.TryGetProperty("styles", out var styles) || styles.ValueKind != JsonValueKind.Array)
            return string.Empty;

        return string.Join(
            Environment.NewLine,
            styles.EnumerateArray()
                .Select(static item => item.GetString())
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Select(static value => "<link rel=\"stylesheet\" href=\"" + value + "\">"));
    }
}
