using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using HostFile = System.IO.File;

// Provides the host-rendered document shell for the generated RazorVue application.
// 提供 RazorVue 生成应用的宿主文档壳；库资源由 JazorDebug 物化到本地，不保留 CDN 映射。
namespace JazorAdmin;

/// <summary>Renders the host document with only local import-map, stylesheet, and application resources.</summary>
internal static class JazorAdminShell
{
    private const string Document = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>JazorAdmin</title>
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
        var root = Path.Combine(environment.WebRootPath, "jazor");
        var importMap = ReadJson(Path.Combine(root, "importmap.json"), "{\"imports\":{}}");
        var styles = GetStyleLinks(environment);
        return context.Response.WriteAsync(string.Format(Document, importMap, styles), cancellationToken);
    }

    public static string GetStyleLinks(IWebHostEnvironment environment)
        => ReadStyles(Path.Combine(environment.WebRootPath, "jazor", "manifest.json"));

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
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Select(static path => $"<link rel=\"stylesheet\" href=\"{path}\">"));
    }
}
