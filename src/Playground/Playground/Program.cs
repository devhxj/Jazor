using Playground;
using Jazor.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PlaygroundExampleRepository>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    await next();
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseJazorStaticFiles();

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
