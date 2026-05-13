using Jazor.AspNetCore;
using Microsoft.Extensions.FileProviders;
using Playground;

var contentRoot = ResolveContentRoot();
var webRoot = Path.Combine(contentRoot, "wwwroot");
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = webRoot
});
builder.Services.AddSingleton<PlaygroundExampleRepository>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(static state =>
    {
        var response = (HttpResponse)state;
        PlaygroundHostPage.ApplySecurityHeaders(response.Headers);
        return Task.CompletedTask;
    }, context.Response);

    await next();
});

var publishedJazorRoot = Path.Combine(webRoot, "jazor");
if (Directory.Exists(publishedJazorRoot))
{
    app.UseJazorStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(publishedJazorRoot),
        RequestPath = "/jazor",
        OnPrepareResponse = PlaygroundHostPage.ApplyStaticAssetHeaders
    });
}

app.UseJazorDevelopmentAssets(options =>
{
    options.EntryModuleRelativePath = "jazor-manifest-razorvue.json";
    options.OnPrepareResponse = PlaygroundHostPage.ApplyStaticAssetHeaders;
});

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(webRoot),
    OnPrepareResponse = PlaygroundHostPage.ApplyStaticAssetHeaders
});

app.Use(async (context, next) =>
{
    if (await PlaygroundHostPage.TryHandleHtmlRequestAsync(context, context.RequestAborted))
        return;

    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "playground-host" }));

app.MapGet("/api/playground/examples", (PlaygroundExampleRepository repository) =>
{
    return Results.Ok(repository.GetCatalog());
});

app.MapGet("/api/playground/examples/{id}", (string id, PlaygroundExampleRepository repository) =>
{
    var detail = repository.GetDetail(id);
    return detail is null ? Results.NotFound() : Results.Ok(detail);
});

app.Run();

static string ResolveContentRoot([System.Runtime.CompilerServices.CallerFilePath] string programFilePath = "")
{
    var appBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);
    var publishedWebRoot = Path.Combine(appBaseDirectory, "wwwroot");
    if (Directory.Exists(publishedWebRoot))
    {
        return appBaseDirectory;
    }

    return Path.GetDirectoryName(programFilePath)
        ?? throw new InvalidOperationException("Cannot determine Playground content root.");
}
