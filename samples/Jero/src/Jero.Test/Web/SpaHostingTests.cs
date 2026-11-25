using System.Net;
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
    [InlineData("")]
    [InlineData("/docs")]
    public async Task ReleasePipeline_ServesPublishedEntryChunksAndShell_AndPreservesApi404(string pathBase)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var root = Path.Combine(Path.GetTempPath(), "jero-jolt-tests", Guid.NewGuid().ToString("N"));
        var assets = Path.Combine(root, "App", "dist", "assets");
        Directory.CreateDirectory(assets);
        await File.WriteAllTextAsync(Path.Combine(assets, "main-TEST.js"), "export const entry = true;", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(assets, "lazy.js"), "export const lazy = true;", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(assets, "site.css"), "body { color: black; }", cancellationToken);
        await File.WriteAllTextAsync(Path.Combine(root, "App", "dist", "index.html"),
            """<!doctype html><html><head><link rel="stylesheet" href="./assets/site.css"></head><body><script type="module" src="./assets/main-TEST.js"></script></body></html>""", cancellationToken);
        try
        {
            await using var app = CreateHost(root, Environments.Staging, pathBase: pathBase);
            app.MapGet("/api/not-found", () => Results.NotFound());
            await app.StartAsync(cancellationToken);
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

            var shell = await client.GetStringAsync(pathBase + "/user", cancellationToken);
            Assert.Contains($"<base href=\"{pathBase}/\">", shell);
            Assert.Contains("src=\"./assets/main-TEST.js\"", shell);
            Assert.Contains("href=\"./assets/site.css\"", shell);
            Assert.Equal("export const entry = true;", await client.GetStringAsync(pathBase + "/assets/main-TEST.js", cancellationToken));
            Assert.Equal("export const lazy = true;", await client.GetStringAsync(pathBase + "/assets/lazy.js", cancellationToken));
            Assert.Equal("body { color: black; }", await client.GetStringAsync(pathBase + "/assets/site.css", cancellationToken));
            foreach (var asset in new[] { "/assets/main-TEST.js", "/assets/lazy.js", "/assets/site.css" })
            {
                using var assetHead = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, pathBase + asset), cancellationToken);
                Assert.Equal(HttpStatusCode.OK, assetHead.StatusCode);
                Assert.Empty(await assetHead.Content.ReadAsByteArrayAsync(cancellationToken));
            }

            using var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, pathBase + "/user"), cancellationToken);
            Assert.Equal(HttpStatusCode.OK, head.StatusCode);
            Assert.Empty(await head.Content.ReadAsByteArrayAsync(cancellationToken));
            using var api = await client.GetAsync(pathBase + "/api/not-found", cancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, api.StatusCode);
            Assert.DoesNotContain("<html", await api.Content.ReadAsStringAsync(cancellationToken));
            using var missingAsset = await client.GetAsync(pathBase + "/missing.js", cancellationToken);
            Assert.Equal(HttpStatusCode.NotFound, missingAsset.StatusCode);
            await app.StopAsync(cancellationToken);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static WebApplication CreateHost(string root, string environment, string pathBase = "")
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = root,
            EnvironmentName = environment
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Configuration["Frontend:PathBase"] = pathBase;
        var app = builder.Build();
        // Furion supplies IApplicationBuilder: verify this exact consumer integration surface.
        IApplicationBuilder pipeline = app;
        pipeline.UseSpaAssets();
        pipeline.UseSpaFallback();
        return app;
    }
}
