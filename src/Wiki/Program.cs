using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddJazorDevelopmentReload(options =>
{
    options.WatchRootPaths.Clear();
    options.WatchRootPaths.Add("jazor");
    options.WatchRootPaths.Add("wwwroot");
});
Wiki.WikiCatalogGuard.ValidateOrThrow();
var app = builder.Build();
var configuredPathBase = builder.Configuration["Wiki:PathBase"];

if (!string.IsNullOrWhiteSpace(configuredPathBase))
{
    if (!configuredPathBase.StartsWith("/", StringComparison.Ordinal))
        throw new InvalidOperationException("Wiki:PathBase must start with '/'.");

    if (configuredPathBase.Length > 1 && configuredPathBase.EndsWith("/", StringComparison.Ordinal))
        configuredPathBase = configuredPathBase[..^1];

    app.UsePathBase(configuredPathBase);
}

app.UseJazorDevelopmentAssets(options =>
{
    options.OnPrepareResponse = Wiki.WikiHostShell.ApplyStaticAssetHeaders;
});
app.UseJazorDevelopmentReload();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        Wiki.WikiHostShell.ApplySecurityHeaders(context.Response.Headers);
        return Task.CompletedTask;
    });

    if (await Wiki.WikiHostShell.TryHandleHtmlRequestAsync(context, context.RequestAborted))
        return;

    await next();
});

app.UseJazorStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = Wiki.WikiHostShell.ApplyStaticAssetHeaders
});
app.MapGet("/health", (HttpContext context) =>
{
    context.Response.Headers["Cache-Control"] = "no-store";
    return Results.Ok("ok");
});
app.MapMethods("/robots.txt", ["GET", "HEAD"], async (HttpContext context) =>
{
    await Wiki.WikiHostDiscoveryDocuments.TryHandleAsync(context, context.RequestAborted);
});
app.MapMethods("/sitemap.xml", ["GET", "HEAD"], async (HttpContext context) =>
{
    await Wiki.WikiHostDiscoveryDocuments.TryHandleAsync(context, context.RequestAborted);
});

app.Run();
