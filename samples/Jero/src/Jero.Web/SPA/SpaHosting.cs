using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Jero.Web;

public static class SpaHosting
{
    private static readonly PathString[] ServerPrefixes = ["/api", "/assets", "/health", "/hubs", "/swagger", "/schedule", "/rezero", "/jazor"];
    private const string DistDirectory = "App/dist";

    public static void UseSpaAssets(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var pathBase = configuration["Frontend:PathBase"];
        if (!string.IsNullOrEmpty(pathBase))
            app.UsePathBase(new PathString(pathBase));

        app.Use(async (context, next) =>
        {
            // Swagger and ReZero are embedded in same-origin frames.
            context.Response.Headers.XFrameOptions = "SAMEORIGIN";
            await next();
        });

        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var assets = Path.Combine(environment.ContentRootPath, DistDirectory, "assets");
        if (Directory.Exists(assets))
        {
            var files = new PhysicalFileProvider(assets);
            app.ApplicationServices.GetRequiredService<Microsoft.Extensions.Hosting.IHostApplicationLifetime>()
                .ApplicationStopped.Register(files.Dispose);
            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "/assets",
                FileProvider = files,
                OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable"
            });
        }
    }

    public static void UseSpaFallback(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        var index = Path.Combine(environment.ContentRootPath, DistDirectory, "index.html");
        app.Use(async (context, next) =>
        {
            await next();
            var request = context.Request;
            if (context.Response.HasStarted || context.Response.StatusCode != StatusCodes.Status404NotFound ||
                context.GetEndpoint() is not null ||
                (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method)) ||
                Path.HasExtension(request.Path.Value) ||
                ServerPrefixes.Any(prefix => request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase)))
                return;

            var accept = request.GetTypedHeaders().Accept;
            if (accept?.Count > 0 && !accept.Any(media => (media.Quality ?? 1) > 0 &&
                (media.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase) ||
                 media.MediaType.Equals("application/xhtml+xml", StringComparison.OrdinalIgnoreCase))))
                return;

            if (!System.IO.File.Exists(index))
                return;

            // A base element anchors Vite's relative assets and Vue Router on deep links.
            var baseHref = HtmlEncoder.Default.Encode(request.PathBase.Value?.TrimEnd('/') + "/");
            var html = await System.IO.File.ReadAllTextAsync(index, context.RequestAborted);
            html = html.Replace("<head>", $"<head><base href=\"{baseHref}\">", StringComparison.Ordinal);
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            if (!HttpMethods.IsHead(request.Method))
                await context.Response.WriteAsync(html, context.RequestAborted);
        });
    }
}
