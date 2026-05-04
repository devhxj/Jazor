using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
Wiki.WikiCatalogGuard.ValidateOrThrow();
var app = builder.Build();

var projectJazorRoot = Path.Combine(app.Environment.ContentRootPath, "jazor");
var localJazorEntryPath = Path.Combine(projectJazorRoot, "main.mjs");

app.UseDefaultFiles();

if (File.Exists(localJazorEntryPath))
{
    // Keep /jazor bound to the live emit directory during local development.
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(projectJazorRoot),
        RequestPath = "/jazor"
    });

    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/jazor"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await next();
    });
}

app.UseStaticFiles();
app.MapGet("/health", () => Results.Ok("ok"));
app.MapFallbackToFile("index.html");

app.Run();
