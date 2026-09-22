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

}
