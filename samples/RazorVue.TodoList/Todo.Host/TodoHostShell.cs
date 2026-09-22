namespace Todo.Host;

/// <summary>Writes the document shell for the standard JavaScript project's web server.</summary>
internal static class TodoHostShell
{
    private const string Document = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>Jazor TODOList</title>
        </head>
        <body>
          <div id="app"></div>
          {1}
          <script type="module" src="{0}/jazor/{2}"></script>
        </body>
        </html>
        """;

    public static Task WriteAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        if (HttpMethods.IsHead(context.Request.Method))
            return Task.CompletedTask;

        var pathBase = context.Request.PathBase.Value ?? string.Empty;
        var development = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
        var client = development ? $"<script type=\"module\" src=\"{pathBase}/jazor/@vite/client\"></script>" : string.Empty;
        return context.Response.WriteAsync(
            string.Format(Document, pathBase, client, development ? "entry.js" : "dist/bundle.js"), cancellationToken);
    }
}
