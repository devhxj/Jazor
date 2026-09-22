// Provides the host-rendered document shell for the generated RazorVue application.
// 提供 RazorVue 生成应用的宿主文档壳；库资源由 JazorDebug 物化到本地，不保留 CDN 映射。
namespace JazorAdmin;

/// <summary>Renders the host document with the standard JavaScript project entry.</summary>
internal static class Shell
{
    private const string Document = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>JazorAdmin</title>
          {0}
        </head>
        <body>
          <div id="app"></div>
          <script type="module" src="{1}"></script>
        </body>
        </html>
        """;

    public static Task WriteAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        if (HttpMethods.IsHead(context.Request.Method))
            return Task.CompletedTask;

        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var viteClient = environment.IsDevelopment() ? "<script type=\"module\" src=\"/jazor/@vite/client\"></script>" : string.Empty;
        var entry = environment.IsDevelopment() ? "/jazor/entry.js" : "/jazor/dist/bundle.js";
        return context.Response.WriteAsync(string.Format(Document, viteClient, entry), cancellationToken);
    }

    public static string GetHeadLinks(IWebHostEnvironment environment)
        => "<link rel=\"icon\" href=\"/brand/jazor-mark.svg\" type=\"image/svg+xml\">" +
           Environment.NewLine +
           "<link rel=\"alternate icon\" href=\"/favicon.ico\" sizes=\"any\">";
}
