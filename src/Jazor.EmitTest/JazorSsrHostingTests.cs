using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Net;
using Jazor.AspNetCore;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorSSRHostingTests
{
    [TestMethod]
    public async Task UseJazorSSR_RendersLocallyHydratesWithSamePropsAndPreservesEndpoints()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await using var host = await CreateHostAsync(
            workspace.RootPath,
            artifactRoot,
            app =>
            {
                app.UsePathBase("/docs");
                app.UseStaticFiles();
                app.UseJazorSSR(new JazorSSRRequest(
                    "components/counter.mjs",
                    new { Title = "SSR <title>" }));
                app.MapGet("/api/status", () => Results.Ok(new { status = "ok" }));
            });

        var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/docs/features/ssr");
        request.Headers.Accept.ParseAdd("text/html");
        var response = await client.SendAsync(request);
        var html = await response.Content.ReadAsStringAsync();

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("text/html", response.Content.Headers.ContentType?.MediaType);
        StringAssert.Contains(html, "<main id=\"ssr-output\">SSR &lt;title&gt;|prefetched</main>");
        StringAssert.Contains(html, "<link rel=\"stylesheet\" href=\"/docs/jazor/vendor/test.css\">");
        StringAssert.Contains(html, "import { createSSRApp } from \"vue\";");
        StringAssert.Contains(html, "await import(\"/docs/jazor/components/counter.mjs\")");
        StringAssert.Contains(html, "\"@vue/server-renderer\"");
        StringAssert.Contains(html, "\"Title\":\"SSR \\u003Ctitle\\u003E\"");
        Assert.IsFalse(html.Contains("node_modules", StringComparison.Ordinal));
        Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "@jazor", "ssr-runner.mjs")));

        var styleResponse = await client.GetAsync("/docs/jazor/vendor/test.css");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, styleResponse.StatusCode);
        Assert.AreEqual("main{display:block;}", await styleResponse.Content.ReadAsStringAsync());

        var endpointResponse = await client.GetAsync("/docs/api/status");
        Assert.AreEqual(System.Net.HttpStatusCode.OK, endpointResponse.StatusCode);
        Assert.AreEqual("application/json", endpointResponse.Content.Headers.ContentType?.MediaType);
        StringAssert.Contains(await endpointResponse.Content.ReadAsStringAsync(), "\"status\":\"ok\"");
    }

    [TestMethod]
    public async Task UseJazorSSR_HeadRequestDoesNotExecuteTheRenderer()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await using var host = await CreateHostAsync(
            workspace.RootPath,
            artifactRoot,
            app => app.UseJazorSSR(new JazorSSRRequest("components/counter.mjs")));

        var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, "/features/ssr");
        request.Headers.Accept.ParseAdd("text/html");
        var response = await client.SendAsync(request);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.IsFalse(File.Exists(Path.Combine(artifactRoot, "@jazor", "ssr-runner.mjs")));
    }

    [TestMethod]
    public async Task JazorSSRRenderer_UsesPackagedDenoHostRuntime()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = workspace.RootPath,
            WebRootPath = Path.Combine(workspace.RootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.Services.AddJazorSSR(options => options.ArtifactRootPath = artifactRoot);

        await using var app = builder.Build();
        var renderer = app.Services.GetRequiredService<IJazorSSRRenderer>();

        var result = await renderer.RenderAsync(new JazorSSRRequest(
            "components/counter.mjs",
            new { Title = "DenoHost" }));

        Assert.AreEqual("components/counter.mjs", result.ModulePath);
        Assert.AreEqual("<main id=\"ssr-output\">DenoHost|prefetched</main>", result.Html);
    }

    [TestMethod]
    [TestCategory("Browser")]
    public async Task UseJazorSSR_HydratesServerHtmlInRealBrowser()
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        if (browserPath is null)
        {
            Assert.Inconclusive(
                "Real browser SSR hydration smoke requires Microsoft Edge, Chrome, or Chromium. " +
                "Set RAZORVUE_BROWSER_EXE to the browser executable path.");
            return;
        }

        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await using var host = await CreateNetworkHostAsync(
            workspace.RootPath,
            artifactRoot,
            app =>
            {
                app.UsePathBase("/docs");
                app.UseStaticFiles();
                app.UseJazorSSR(new JazorSSRRequest(
                    "components/hydration.mjs",
                    new { Title = "SSR hydration" }));
            });

        var address = new Uri(host.Urls.Single());
        var browser = await BrowserSmokeTestHelper.RunBrowserDumpDomAsync(
            browserPath,
            new Uri(address, "/docs/features/hydration"),
            workspace.RootPath,
            virtualTimeBudgetMilliseconds: 10000);

        Assert.AreEqual(0, browser.ExitCode, browser.ToString());
        StringAssert.Contains(browser.StandardOutput, "<main id=\"hydration-output\">SSR hydration</main>");
        StringAssert.Contains(browser.StandardOutput, "data-jazor-ssr-hydrated=\"true\"");
        Assert.IsFalse(
            browser.StandardError.Contains("Hydration", StringComparison.OrdinalIgnoreCase),
            browser.ToString());
    }

    private static async Task<WebApplication> CreateHostAsync(
        string contentRootPath,
        string artifactRoot,
        Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = contentRootPath,
            WebRootPath = Path.Combine(contentRootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddJazorSSR(options =>
        {
            options.ArtifactRootPath = artifactRoot;
            options.AssetPath = "/jazor";
        });

        var app = builder.Build();
        configure(app);
        await app.StartAsync();
        return app;
    }

    private static async Task<WebApplication> CreateNetworkHostAsync(
        string contentRootPath,
        string artifactRoot,
        Action<WebApplication> configure)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = contentRootPath,
            WebRootPath = Path.Combine(contentRootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, port: 0));
        builder.Services.AddJazorSSR(options =>
        {
            options.ArtifactRootPath = artifactRoot;
            options.AssetPath = "/jazor";
        });

        var app = builder.Build();
        configure(app);
        await app.StartAsync();
        return app;
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate the Jazor repository root.");
    }

    private sealed class SsrHostWorkspace : IDisposable
    {
        public SsrHostWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", "ssr", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(RootPath);
        }

        public string RootPath { get; }

        public async Task<string> CreateArtifactRootAsync()
        {
            var artifactRoot = Path.Combine(RootPath, "wwwroot", "jazor");
            var manifestPath = Path.Combine(FindRepositoryRoot(), "src", "ECMAScript.Vue", "manifest.json");
            var materialization = new LibraryMaterializer().Materialize(
                [manifestPath],
                artifactRoot,
                BuildMode.Production);
            await ImportMapWriter.WriteAsync(artifactRoot, materialization);
            await File.WriteAllTextAsync(Path.Combine(artifactRoot, "jazor-manifest.json"), "{}\n");
            await File.WriteAllTextAsync(
                Path.Combine(artifactRoot, "manifest.json"),
                """{"styles":["/jazor/vendor/test.css"]}""");
            var stylePath = Path.Combine(artifactRoot, "vendor", "test.css");
            Directory.CreateDirectory(Path.GetDirectoryName(stylePath)!);
            await File.WriteAllTextAsync(stylePath, "main{display:block;}");

            var componentPath = Path.Combine(artifactRoot, "components", "counter.mjs");
            Directory.CreateDirectory(Path.GetDirectoryName(componentPath)!);
            await File.WriteAllTextAsync(
                componentPath,
                """
                import { defineComponent, h, onServerPrefetch, ref } from "vue";

                export default defineComponent({
                  props: ["Title"],
                  setup(props) {
                    const phase = ref("before");
                    onServerPrefetch(async () => {
                      phase.value = "prefetched";
                    });
                    return () => h("main", { id: "ssr-output" }, `${props.Title}|${phase.value}`);
                  }
                });
                """);

            var hydrationComponentPath = Path.Combine(artifactRoot, "components", "hydration.mjs");
            await File.WriteAllTextAsync(
                hydrationComponentPath,
                """
                import { defineComponent, h, onMounted } from "vue";

                export default defineComponent({
                  props: ["Title"],
                  setup(props) {
                    onMounted(() => {
                      document.documentElement.setAttribute("data-jazor-ssr-hydrated", "true");
                    });
                    return () => h("main", { id: "hydration-output" }, props.Title);
                  }
                });
                """);
            return artifactRoot;
        }

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
    }
}
