using System.Net;
using Jazor.AspNetCore.Dev;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Jero.Web;
using Xunit;

namespace Jero.Test.Web;

public class SpaHostingTests
{
    [Theory]
    [InlineData("", "")]
    [InlineData("/docs", "")]
    [InlineData("/docs", "/site")]
    public async Task ReleasePipeline_ServesPublishedEntryChunksAndShell_AndPreservesApi404(string pathBase, string mount)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var root = TestWorkspace.Create("spa");
        var assets = Path.Combine(root, "wwwroot", "assets");
        Directory.CreateDirectory(assets);
        await File.WriteAllTextAsync(Path.Combine(assets, "main-a1B2c3D4.js"), "export const entry = true;", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(root, "appsettings.json"), """{"sample":"server only"}""", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(assets, "lazy.js"), "export const lazy = true;", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(assets, "site.css"), "body { color: black; }", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(root, "wwwroot", "index.html"),
            """<!doctype html><html><head><link rel="stylesheet" href="./assets/site.css"></head><body><script type="module" src="./assets/main-a1B2c3D4.js"></script></body></html>""", cancellationToken);
        try
        {
            var site = pathBase + mount;
            await using var app = CreateHost(root, pathBase, mount);
            app.MapGet("/api/not-found", () => Results.NotFound());
            await app.StartAsync(cancellationToken);
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

            var shell = await client.GetStringAsync(site + "/system/user", cancellationToken);
            Assert.Contains($"<base href=\"{site}/\">", shell);
            Assert.Equal(shell, await client.GetStringAsync(site + "/index.html", cancellationToken));
            Assert.Contains("src=\"./assets/main-a1B2c3D4.js\"", shell);
            Assert.Contains("href=\"./assets/site.css\"", shell);
            Assert.Equal("export const entry = true;", await client.GetStringAsync(site + "/assets/main-a1B2c3D4.js", cancellationToken));
            Assert.Equal("export const lazy = true;", await client.GetStringAsync(site + "/assets/lazy.js", cancellationToken));
            Assert.Equal("body { color: black; }", await client.GetStringAsync(site + "/assets/site.css", cancellationToken));
            foreach (var asset in new[] { "/assets/main-a1B2c3D4.js", "/assets/lazy.js", "/assets/site.css" })
            {
                using var assetHead = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, site + asset), cancellationToken);
                Assert.Equal(HttpStatusCode.OK, assetHead.StatusCode);
                Assert.Equal(asset.EndsWith("a1B2c3D4.js"), assetHead.Headers.CacheControl!.ToString().Contains("immutable"));
                Assert.Empty(await assetHead.Content.ReadAsByteArrayAsync(cancellationToken));
            }

            using var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, site + "/user"), cancellationToken);
            Assert.Equal(HttpStatusCode.OK, head.StatusCode);
            Assert.Empty(await head.Content.ReadAsByteArrayAsync(cancellationToken));
            using var api = await client.GetAsync(pathBase + "/api/not-found", cancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, api.StatusCode);
            Assert.DoesNotContain("<html", await api.Content.ReadAsStringAsync(cancellationToken));
            foreach (var missing in new[] { "/missing.js", "/appsettings.json", "/api/missing", "/hubs/missing" })
            {
                using var response = await client.GetAsync(site + missing, cancellationToken);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            using var outside = await client.GetAsync(pathBase + "/__jolt/missing", cancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, outside.StatusCode);
            using var jsonRequest = new HttpRequestMessage(HttpMethod.Get, site + "/system/user");
            jsonRequest.Headers.Accept.ParseAdd("application/json");
            using var json = await client.SendAsync(jsonRequest, cancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, json.StatusCode);
            await app.StopAsync(cancellationToken);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static WebApplication CreateHost(string root, string pathBase, string mount)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(Startup).Assembly.GetName().Name,
            ContentRootPath = root,
            EnvironmentName = Environments.Staging
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Services.AddJazorSPA(options =>
        {
            options.PathBase = pathBase;
            if (mount.Length > 0)
                options.RequestPath = mount;
            options.Fallback.ExcludedPathPrefixes.Add("/hubs");
        });
        var app = builder.Build();
        // Furion supplies IApplicationBuilder: verify this exact consumer integration surface.
        IApplicationBuilder pipeline = app;
        pipeline.UseJazorPathBase();
        pipeline.UseJazorSPA();
        return app;
    }
}
