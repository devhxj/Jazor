using Microsoft.AspNetCore.Hosting;

namespace RazorVue.Authoring;

internal static class AuthoringHostShell
{
    private const string Document = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>RazorVue Authoring</title>
          <base href="{2}/">
          <link rel="icon" href="{2}/favicon.svg" type="image/svg+xml">
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

        var pathBase = context.Request.PathBase.Value ?? string.Empty;
        var environment = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var normalizedBase = pathBase.TrimEnd('/');
        var viteClient = environment.IsDevelopment()
            ? $"<script type=\"module\" src=\"{normalizedBase}/jazor/@vite/client\"></script>"
            : string.Empty;
        var entryUrl = normalizedBase + (environment.IsDevelopment() ? "/jazor/entry.js" : "/jazor/dist/bundle.js");
        return context.Response.WriteAsync(
            string.Format(Document, viteClient, entryUrl, normalizedBase),
            cancellationToken);
    }

}
