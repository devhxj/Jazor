using Jazor.AspNetCore;
using Playground;

var contentRoot = ResolveContentRoot();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = Path.Combine(contentRoot, "wwwroot")
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

app.UseJazorDevelopmentAssets(options =>
{
    options.EntryModuleRelativePath = "jazor-manifest-razorvue.json";
    options.OnPrepareResponse = PlaygroundHostPage.ApplyStaticAssetHeaders;
});

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = PlaygroundHostPage.ApplyStaticAssetHeaders
});
app.UseJazorStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = PlaygroundHostPage.ApplyStaticAssetHeaders
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

app.MapMethods("/{**path}", ["GET", "HEAD"], async context =>
{
    if (!PlaygroundHostPage.IsHtmlShellPath(context.Request.Path))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    await PlaygroundHostPage.WriteHtmlAsync(context);
});

app.Run();

static string ResolveContentRoot([System.Runtime.CompilerServices.CallerFilePath] string programFilePath = "")
{
    return Path.GetDirectoryName(programFilePath)
        ?? throw new InvalidOperationException("Cannot determine Playground content root.");
}
