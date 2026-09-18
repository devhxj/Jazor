using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace BindingsShowcase.Host;

internal static class ShowcaseHostShell
{
    private const string Document = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>ECMAScript bindings showcase</title>
          <base href="{2}/">
          <script type="importmap">{0}</script>
        </head>
        <body>
          <div id="app"></div>
          <script type="module" src="{2}/jazor/{1}"></script>
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
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
        var artifactRoot = ResolveArtifactRoot(environment.ContentRootPath, configuration["Showcase:JazorRoot"]);
        var importMap = ReadImportMap(Path.Combine(artifactRoot, "importmap.json"), pathBase);
        var entryModule = File.Exists(Path.Combine(artifactRoot, "bundle.js")) ? "bundle.js" : "app.mjs";
        var normalizedBase = pathBase.TrimEnd('/');
        return context.Response.WriteAsync(
            string.Format(Document, importMap, entryModule, normalizedBase),
            cancellationToken);
    }

    private static string ReadImportMap(string path, string pathBase)
    {
        if (!File.Exists(path))
            return "{\"imports\":{}}";

        var node = JsonNode.Parse(File.ReadAllText(path))?.AsObject() ?? new JsonObject();
        if (!string.IsNullOrEmpty(pathBase) && node["imports"] is JsonObject imports)
        {
            foreach (var entry in imports.ToArray())
            {
                if (entry.Value is JsonValue value && value.TryGetValue<string>(out var target) &&
                    target.StartsWith("/", StringComparison.Ordinal))
                    imports[entry.Key] = pathBase + target;
            }
        }

        return node.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    private static string ResolveArtifactRoot(string contentRoot, string? configuredRoot)
        => string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(contentRoot, "jazor")
            : Path.GetFullPath(Path.IsPathRooted(configuredRoot)
                ? configuredRoot
                : Path.Combine(contentRoot, configuredRoot));
}
