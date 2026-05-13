using Jazor.AspNetCore;
using Playground;

var builder = JazorWebApplication.CreateBuilder(args);
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

app.UseJazorWebAssets(options =>
{
    options.OnPrepareResponse = PlaygroundHostPage.ApplyStaticAssetHeaders;
});

app.UseJazorSpaFallback(PlaygroundHostPage.WriteHtmlAsync);

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
