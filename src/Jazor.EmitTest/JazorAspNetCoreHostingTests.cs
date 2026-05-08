using Jazor.AspNetCore;
using Jazor.Common.SourceMaps;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorAspNetCoreHostingTests
{
    [TestMethod]
    public async Task UseJazorDevelopmentAssets_ServesMountedAssetsAndReturns404ForMissingMountedFile()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var jazorRoot = Path.Combine(workspace.RootPath, "jazor");
        Directory.CreateDirectory(jazorRoot);
        await File.WriteAllTextAsync(
            Path.Combine(jazorRoot, "main.mjs"),
            "export const ready = true;\n//# sourceMappingURL=main.mjs.map\n");
        await File.WriteAllTextAsync(
            Path.Combine(jazorRoot, "main.mjs.map"),
            new SourceMapWriter().Write(
                new SourceMapDocument(
                    "main.mjs",
                    [new SourceMapSource("AppModule.cs", "public static string Boot() => \"ready\";")],
                    [new SourceMapSegment(0, 0, 0, 0, 0)])));
        await File.WriteAllTextAsync(Path.Combine(jazorRoot, "feature.mjs"), "export const value = 1;\n");

        using var host = await CreateHostAsync(workspace.RootPath, app =>
        {
            app.UseJazorDevelopmentAssets();
            app.MapGet("/", () => "ready");
        });

        var client = host.GetTestClient();

        var mountedAssetResponse = await client.GetAsync("/jazor/feature.mjs");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, mountedAssetResponse.StatusCode);
        Assert.AreEqual("export const value = 1;\n", await mountedAssetResponse.Content.ReadAsStringAsync());

        var sourceMapResponse = await client.GetAsync("/jazor/main.mjs.map");
        var sourceMapPayload = await sourceMapResponse.Content.ReadAsStringAsync();
        Assert.AreEqual(System.Net.HttpStatusCode.OK, sourceMapResponse.StatusCode);
        StringAssert.Contains(sourceMapPayload, "\"version\": 3");
        StringAssert.Contains(sourceMapPayload, "\"file\": \"main.mjs\"");
        StringAssert.Contains(sourceMapPayload, "\"AppModule.cs\"");
        Assert.AreEqual("application/json", sourceMapResponse.Content.Headers.ContentType?.MediaType);

        var missingMountedAssetResponse = await client.GetAsync("/jazor/missing.mjs");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, missingMountedAssetResponse.StatusCode);

        var rootResponse = await client.GetAsync("/");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, rootResponse.StatusCode);
    }

    [TestMethod]
    public async Task UseJazorDevelopmentAssets_DoesNothingWhenEntryModuleIsMissing()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var jazorRoot = Path.Combine(workspace.RootPath, "jazor");
        Directory.CreateDirectory(jazorRoot);
        await File.WriteAllTextAsync(Path.Combine(jazorRoot, "feature.mjs"), "export const value = 1;\n");

        using var host = await CreateHostAsync(workspace.RootPath, app =>
        {
            app.UseJazorDevelopmentAssets();
            app.MapGet("/", () => "ready");
        });

        var client = host.GetTestClient();

        var mountedAssetResponse = await client.GetAsync("/jazor/feature.mjs");
        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, mountedAssetResponse.StatusCode);

        var rootResponse = await client.GetAsync("/");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, rootResponse.StatusCode);
    }

    [TestMethod]
    public async Task UseJazorStaticFiles_ServesSourceMapsAsApplicationJson()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "app.mjs.map"),
            "{\"version\":3,\"file\":\"app.mjs\",\"sources\":[\"AppModule.cs\"],\"sourcesContent\":[\"public static class AppModule {}\"],\"names\":[],\"mappings\":\"\"}");

        using var host = await CreateHostAsync(workspace.RootPath, app =>
        {
            app.UseJazorStaticFiles();
            app.MapGet("/", () => "ready");
        });

        var client = host.GetTestClient();

        var response = await client.GetAsync("/app.mjs.map");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "\"file\":\"app.mjs\"");
    }

    [TestMethod]
    public async Task UseJazorStaticFiles_PreservesCallerResponseHooksAndOverridesCustomProviderForSourceMaps()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "app.mjs.map"),
            "{\"version\":3,\"file\":\"app.mjs\",\"sources\":[\"AppModule.cs\"],\"sourcesContent\":[\"public static class AppModule {}\"],\"names\":[],\"mappings\":\"\"}");
        await File.WriteAllTextAsync(Path.Combine(webRoot, "app.bin"), "binary");

        using var host = await CreateHostAsync(workspace.RootPath, app =>
        {
            app.UseJazorStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = new FixedContentTypeProvider("application/octet-stream"),
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers["X-Jazor-Test"] = "prepared";
                }
            });
            app.MapGet("/", () => "ready");
        });

        var client = host.GetTestClient();

        var sourceMapResponse = await client.GetAsync("/app.mjs.map");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, sourceMapResponse.StatusCode);
        Assert.AreEqual("application/json", sourceMapResponse.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual("prepared", GetSingleHeaderValue(sourceMapResponse, "X-Jazor-Test"));

        var binaryResponse = await client.GetAsync("/app.bin");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, binaryResponse.StatusCode);
        Assert.AreEqual("application/octet-stream", binaryResponse.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual("prepared", GetSingleHeaderValue(binaryResponse, "X-Jazor-Test"));
    }

    [TestMethod]
    public async Task UseJazorStaticFiles_WhenApplicationUsesPathBase_ServesSourceMapsFromPathBase()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "app.mjs.map"),
            "{\"version\":3,\"file\":\"app.mjs\",\"sources\":[\"AppModule.cs\"],\"sourcesContent\":[\"public static class AppModule {}\"],\"names\":[],\"mappings\":\"\"}");

        using var host = await CreateHostAsync(workspace.RootPath, app =>
        {
            app.UsePathBase("/docs");
            app.UseJazorStaticFiles();
            app.MapGet("/", () => "ready");
        });

        var client = host.GetTestClient();

        var response = await client.GetAsync("/docs/app.mjs.map");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json", response.Content.Headers.ContentType?.MediaType);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "\"file\":\"app.mjs\"");
    }

    [TestMethod]
    public async Task WikiHostShell_WhenApplicationUsesPathBase_RendersPathBaseAwareAssetUrlsAndCanonicalMetadata()
    {
        using var workspace = new AspNetCoreHostTestWorkspace();
        var webRoot = Path.Combine(workspace.RootPath, "wwwroot");
        Directory.CreateDirectory(webRoot);
        await File.WriteAllTextAsync(
            Path.Combine(webRoot, "index.html"),
            """
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <title>__WIKI_DOCUMENT_TITLE__</title>
              <meta name="description" content="__WIKI_DOCUMENT_DESCRIPTION__" />
              <meta name="robots" content="__WIKI_DOCUMENT_ROBOTS__" />
              <link rel="canonical" href="__WIKI_DOCUMENT_CANONICAL_URL__" />
              <meta property="og:title" content="__WIKI_OPEN_GRAPH_TITLE__" />
              <meta property="og:description" content="__WIKI_OPEN_GRAPH_DESCRIPTION__" />
              <meta property="og:url" content="__WIKI_OPEN_GRAPH_URL__" />
              <meta name="twitter:title" content="__WIKI_TWITTER_TITLE__" />
              <meta name="twitter:description" content="__WIKI_TWITTER_DESCRIPTION__" />
              <link rel="icon" href="__WIKI_FAVICON_URL__" type="image/svg+xml" />
              <link rel="stylesheet" href="__WIKI_SITE_CSS_URL__" />
            </head>
            <body data-wiki-path-base="__WIKI_PATH_BASE__">
              <div id="app"></div>
              <script type="importmap" nonce="__WIKI_SCRIPT_NONCE__">
                {
                  "imports": {
                    "System/": "__WIKI_SYSTEM_IMPORT_BASE__",
                    "vue": "__WIKI_VENDOR_VUE_URL__",
                    "npm:vue@3": "__WIKI_VENDOR_VUE_URL__"
                  }
                }
              </script>
              <script type="module" src="__WIKI_MAIN_MODULE_URL__"></script>
            </body>
            </html>
            """);

        using var host = await CreateHostAsync(workspace.RootPath, app =>
        {
            app.UsePathBase("/docs");
            app.Use(async (context, next) =>
            {
                if (await Wiki.WikiHostShell.TryHandleHtmlRequestAsync(context, context.RequestAborted))
                    return;

                await next();
            });
            app.MapGet("/health", () => "ok");
        });

        var client = host.GetTestClient();
        client.DefaultRequestHeaders.Host = "wiki.example.test";

        var response = await client.GetAsync("/docs/guides/getting-started");
        var html = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(html, "href=\"/docs/favicon.svg\"");
        StringAssert.Contains(html, "href=\"/docs/site.css\"");
        StringAssert.Contains(html, "\"System/\": \"/docs/jazor/System/\"");
        StringAssert.Contains(html, "\"vue\": \"/docs/vendor/vue@3.5.16.mjs\"");
        StringAssert.Contains(html, "src=\"/docs/jazor/main.mjs\"");
        StringAssert.Contains(html, "data-wiki-path-base=\"/docs\"");
        StringAssert.Contains(html, "href=\"http://wiki.example.test/docs/guides/getting-started\"");
        StringAssert.Contains(html, "content=\"http://wiki.example.test/docs/guides/getting-started\"");
    }

    private static async Task<IHost> CreateHostAsync(string contentRootPath, Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = contentRootPath
        });
        builder.WebHost.UseTestServer();

        var app = builder.Build();
        configure(app);
        await app.StartAsync();
        return app;
    }

    private static string? GetSingleHeaderValue(HttpResponseMessage response, string headerName)
    {
        return response.Headers.TryGetValues(headerName, out var values)
            ? values.SingleOrDefault()
            : null;
    }

    private sealed class AspNetCoreHostTestWorkspace : IDisposable
    {
        public AspNetCoreHostTestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);
        }

        public string RootPath { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(RootPath))
                    Directory.Delete(RootPath, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed class FixedContentTypeProvider(string contentType) : IContentTypeProvider
    {
        public bool TryGetContentType(string subpath, out string resolvedContentType)
        {
            resolvedContentType = contentType;
            return true;
        }
    }
}
