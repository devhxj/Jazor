namespace Todo.Host;

/// <summary>Writes the minimal document shell; the development reload middleware adds its transport client.</summary>
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
          <script type="module" src="{0}/jazor/app.mjs"></script>
        </body>
        </html>
        """;

    public static Task WriteAsync(HttpContext context, CancellationToken cancellationToken)
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        if (HttpMethods.IsHead(context.Request.Method))
            return Task.CompletedTask;

        var pathBase = context.Request.PathBase.Value ?? string.Empty;
        return context.Response.WriteAsync(string.Format(Document, pathBase), cancellationToken);
    }
}
