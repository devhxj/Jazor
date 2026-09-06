using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Jazor.AspNetCore;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JazorSsrHostingTests
{
    [TestMethod]
    public async Task UseJazorSsr_RendersLocallyHydratesWithSamePropsAndPreservesEndpoints()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await using var host = await CreateHostAsync(
            workspace.RootPath,
            artifactRoot,
            app =>
            {
                app.UsePathBase("/docs");
                app.UseJazorArtifacts();
                app.UseJazorSsr(new JazorSsrRequest(
                    "components/counter.mjs",
                    new { Title = "SSR <title>" },
                    [new JazorSsrProvider("app:feature", new { Enabled = true })]));
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
        StringAssert.Contains(html, "<script id=\"__jazor_ssr_state\" type=\"application/json\">{\"schema\":\"jazor-ssr-state\",\"version\":1,\"props\":{\"Title\":\"SSR \\u003Ctitle\\u003E\"},\"providers\":[{\"key\":\"app:feature\",\"value\":{\"Enabled\":true}}],\"authentication\":null}</script>");
        StringAssert.Contains(html, "state.providers.some(provider => !provider || typeof provider.key !== \"string\" || provider.key.length === 0)");
        StringAssert.Contains(html, "for (const provider of providers) app.provide(provider.key, provider.value);");
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
    public async Task UseJazorSsr_HeadRequestDoesNotExecuteTheRenderer()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await using var host = await CreateHostAsync(
            workspace.RootPath,
            artifactRoot,
            app => app.UseJazorSsr(new JazorSsrRequest("components/counter.mjs")));

        var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, "/features/ssr");
        request.Headers.Accept.ParseAdd("text/html");
        var response = await client.SendAsync(request);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.IsFalse(File.Exists(Path.Combine(artifactRoot, "@jazor", "ssr-runner.mjs")));
    }

    [TestMethod]
    public async Task JazorSsrRenderer_UsesPackagedDenoHostRuntime()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = workspace.RootPath,
            WebRootPath = Path.Combine(workspace.RootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.Services.AddJazorSsr(options => options.ArtifactRootPath = artifactRoot);

        await using var app = builder.Build();
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var result = await renderer.RenderAsync(new JazorSsrRequest(
            "components/counter.mjs",
            new { Title = "DenoHost" }));

        Assert.AreEqual("components/counter.mjs", result.ModulePath);
        Assert.AreEqual("<main id=\"ssr-output\">DenoHost|prefetched</main>", result.Html);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_AppliesRequestProvidersToServerComponent()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteInjectedComponentAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = workspace.RootPath,
            WebRootPath = Path.Combine(workspace.RootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.Services.AddJazorSsr(options => options.ArtifactRootPath = artifactRoot);

        await using var app = builder.Build();
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var result = await renderer.RenderAsync(new JazorSsrRequest(
            "components/injected.mjs",
            Providers:
            [
                new JazorSsrProvider(
                    "jazor:service:Jazor.EmitTest.SsrBrowserProbe",
                    new { Label = "server-provider" })
            ]));

        Assert.AreEqual("<main id=\"ssr-service\">server-provider</main>", result.Html);
        Assert.AreEqual(
            "[{\"key\":\"jazor:service:Jazor.EmitTest.SsrBrowserProbe\",\"value\":{\"Label\":\"server-provider\"}}]",
            result.SerializedProviders);
        StringAssert.Contains(result.SerializedState, "\"schema\":\"jazor-ssr-state\"");
        StringAssert.Contains(result.SerializedState, "\"version\":1");
    }

    [TestMethod]
    public async Task JazorSsrRenderer_AddsTypedAuthenticationAsReservedProvider()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = workspace.RootPath,
            WebRootPath = Path.Combine(workspace.RootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.Services.AddJazorSsr(options => options.ArtifactRootPath = artifactRoot);

        await using var app = builder.Build();
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();
        var authentication = new JazorAuthenticationState(
            JazorAuthenticationStatus.Authenticated,
            "user-42",
            new Dictionary<string, string[]> { ["role"] = ["admin"] });

        var result = await renderer.RenderAsync(new JazorSsrRequest(
            "components/counter.mjs",
            Authentication: authentication));

        StringAssert.Contains(result.SerializedState, "\"authentication\":{\"status\":\"Authenticated\"");
        StringAssert.Contains(result.SerializedState, "\"key\":\"jazor:auth-state\"");
        StringAssert.Contains(result.SerializedProviders, "\"jazor:auth-state\"");
    }

    [TestMethod]
    public async Task JazorSsrRenderer_RejectsProviderWithoutKeyBeforeStartingWorker()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = workspace.RootPath,
            WebRootPath = Path.Combine(workspace.RootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.Services.AddJazorSsr(options => options.ArtifactRootPath = artifactRoot);

        await using var app = builder.Build();
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var error = await Assert.ThrowsExactlyAsync<ArgumentException>(() => renderer.RenderAsync(
            new JazorSsrRequest(
                "components/counter.mjs",
                Providers: [new JazorSsrProvider("", new { Enabled = true })])));

        StringAssert.Contains(error.Message, "non-empty keys");
        Assert.IsFalse(File.Exists(Path.Combine(artifactRoot, "@jazor", "ssr-runner.mjs")));
    }

    [TestMethod]
    public void JazorAuthenticationState_FromPrincipalProducesClosedTypedSnapshot()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-42"),
            new Claim("role", "admin"),
            new Claim("role", "operator")
        ], authenticationType: "test"));

        var state = JazorAuthenticationState.FromPrincipal(principal);

        Assert.AreEqual(JazorAuthenticationStatus.Authenticated, state.Status);
        Assert.AreEqual("user-42", state.Subject);
        CollectionAssert.AreEqual(new[] { "admin", "operator" }, state.Claims!["role"]);

        var envelope = JazorSsrStateEnvelope.Create(new JazorSsrRequest(
            "components/counter.mjs",
            Authentication: state));
        Assert.AreEqual(JazorSsrStateEnvelope.CurrentSchema, envelope.Schema);
        Assert.AreEqual(JazorSsrStateEnvelope.CurrentVersion, envelope.Version);
        Assert.AreEqual(JazorAuthenticationState.ProviderKey, envelope.Providers.Single().Key);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_RenderHookErrorFailsExplicitlyInsteadOfEmptyHtml()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteRenderErrorComponentAsync();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = workspace.RootPath,
            WebRootPath = Path.Combine(workspace.RootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.Services.AddJazorSsr(options => options.ArtifactRootPath = artifactRoot);

        await using var app = builder.Build();
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        // renderToString swallows render-hook errors into "<!---->" placeholders; the runner's
        // errorHandler capture must turn them into an explicit failure with the original stack.
        // 渲染期错误必须显式失败并携带原始栈，而不是静默输出空占位 HTML。
        var error = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => renderer.RenderAsync(new JazorSsrRequest("components/render-error.mjs")));

        StringAssert.Contains(error.Message, "render-boom");
        StringAssert.Contains(error.Message, "components/render-error.mjs");
    }

    [TestMethod]
    public async Task JazorSsrRenderer_ReusesWarmWorkerForSameGeneration()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("warm");
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var first = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));
        var second = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));

        Assert.AreEqual(first.ProcessId, second.ProcessId);
        Assert.AreEqual(1, first.RenderCount);
        Assert.AreEqual(2, second.RenderCount);
        Assert.AreEqual("warm", second.Version);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_ManifestGenerationChangeReplacesWorkersAndModuleCache()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("before");
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var before = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));
        await workspace.WriteWorkerProbeAsync("after");
        await workspace.PublishGenerationAsync("generation-after-module-rewrite");
        var after = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));

        Assert.AreNotEqual(before.ProcessId, after.ProcessId);
        Assert.AreEqual(1, after.RenderCount);
        Assert.AreEqual("after", after.Version);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_CrashedWorkerIsDiscardedAndReplaced()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("recovery");
        await workspace.WriteCrashComponentAsync();
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var before = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => renderer.RenderAsync(
            new JazorSsrRequest("components/crash.mjs")));
        var after = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));

        Assert.AreNotEqual(before.ProcessId, after.ProcessId);
        Assert.AreEqual(1, after.RenderCount);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_CancellationTerminatesLeasedWorkerAndRestoresCapacity()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("cancellation");
        await workspace.WriteDelayedComponentAsync();
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var before = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => renderer.RenderAsync(
            new JazorSsrRequest("components/delayed.mjs", new { Delay = 30_000 }),
            cancellation.Token));
        var after = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));

        Assert.AreNotEqual(before.ProcessId, after.ProcessId);
        Assert.AreEqual(1, after.RenderCount);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_BoundsConcurrentDenoWorkers()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteDelayedComponentAsync();
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 2);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var renders = Enumerable.Range(0, 6)
            .Select(_ => renderer.RenderAsync(
                new JazorSsrRequest("components/delayed.mjs", new { Delay = 300 })))
            .ToArray();
        var results = await Task.WhenAll(renders);
        var processIds = results
            .Select(ParseDelayedProcessId)
            .Distinct()
            .ToArray();

        Assert.HasCount(2, processIds);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_ApplicationDisposalStopsPersistentWorkers()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("dispose");
        var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();
        var probe = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.mjs")));

        await app.DisposeAsync();

        Assert.IsTrue(
            await WaitForProcessExitAsync(probe.ProcessId, TimeSpan.FromSeconds(5)),
            "The persistent Deno worker remained alive after the application service provider was disposed.");
    }

    [TestMethod]
    [TestCategory("Browser")]
    public async Task UseJazorSsr_HydratesServerHtmlInRealBrowser()
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
                app.UseJazorArtifacts();
                app.UseJazorSsr(new JazorSsrRequest(
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
        builder.Services.AddJazorSsr(options =>
        {
            options.ArtifactRootPath = artifactRoot;
            options.RequestPath = "/jazor";
        });

        var app = builder.Build();
        configure(app);
        await app.StartAsync();
        return app;
    }

    private static WebApplication CreateRendererApplication(
        string contentRootPath,
        string artifactRoot,
        int workerCount)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ContentRootPath = contentRootPath,
            WebRootPath = Path.Combine(contentRootPath, "wwwroot"),
            EnvironmentName = Environments.Development
        });
        builder.Services.AddJazorSsr(options =>
        {
            options.ArtifactRootPath = artifactRoot;
            options.WorkerCount = workerCount;
        });
        return builder.Build();
    }

    private static WorkerProbe ParseWorkerProbe(JazorSsrRenderResult result)
    {
        const string prefix = "<main id=\"worker-probe\">";
        const string suffix = "</main>";
        Assert.IsTrue(result.Html.StartsWith(prefix, StringComparison.Ordinal));
        Assert.IsTrue(result.Html.EndsWith(suffix, StringComparison.Ordinal));
        var parts = result.Html[prefix.Length..^suffix.Length].Split('|');
        Assert.HasCount(3, parts);
        return new WorkerProbe(int.Parse(parts[0]), int.Parse(parts[1]), parts[2]);
    }

    private static int ParseDelayedProcessId(JazorSsrRenderResult result)
    {
        const string prefix = "<main id=\"delayed\">";
        const string suffix = "</main>";
        Assert.IsTrue(result.Html.StartsWith(prefix, StringComparison.Ordinal));
        Assert.IsTrue(result.Html.EndsWith(suffix, StringComparison.Ordinal));
        return int.Parse(result.Html[prefix.Length..^suffix.Length]);
    }

    private static async Task<bool> WaitForProcessExitAsync(int processId, TimeSpan timeout)
    {
        var deadline = Stopwatch.GetTimestamp() + (long)(timeout.TotalSeconds * Stopwatch.Frequency);
        while (Stopwatch.GetTimestamp() < deadline)
        {
            try
            {
                using var process = Process.GetProcessById(processId);
                if (process.HasExited)
                    return true;
            }
            catch (ArgumentException)
            {
                return true;
            }

            await Task.Delay(50);
        }

        return false;
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
        builder.Services.AddJazorSsr(options =>
        {
            options.ArtifactRootPath = artifactRoot;
            options.RequestPath = "/jazor";
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
            var artifactRoot = Path.Combine(RootPath, "jazor");
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

        public Task PublishGenerationAsync(string generation)
            => File.WriteAllTextAsync(
                Path.Combine(RootPath, "jazor", "jazor-manifest.json"),
                "{\"generation\":" + System.Text.Json.JsonSerializer.Serialize(generation) + "}\n");

        public Task WriteWorkerProbeAsync(string version)
            => File.WriteAllTextAsync(
                Path.Combine(RootPath, "jazor", "components", "worker-probe.mjs"),
                $$"""
                import { defineComponent, h } from "vue";

                let renderCount = 0;
                export default defineComponent({
                  setup() {
                    const currentRender = ++renderCount;
                    return () => h("main", { id: "worker-probe" }, `${Deno.pid}|${currentRender}|{{version}}`);
                  }
                });
                """);

        public Task WriteCrashComponentAsync()
            => File.WriteAllTextAsync(
                Path.Combine(RootPath, "jazor", "components", "crash.mjs"),
                """
                import { defineComponent } from "vue";

                export default defineComponent({
                  setup() {
                    Deno.exit(73);
                  }
                });
                """);

        public Task WriteRenderErrorComponentAsync()
            => File.WriteAllTextAsync(
                Path.Combine(RootPath, "jazor", "components", "render-error.mjs"),
                """
                import { defineComponent, h } from "vue";

                export default defineComponent({
                  setup() {
                    return () => {
                      throw new Error("render-boom");
                    };
                  }
                });
                """);

        public Task WriteInjectedComponentAsync()
            => File.WriteAllTextAsync(
                Path.Combine(RootPath, "jazor", "components", "injected.mjs"),
                """
                import { defineComponent, h, inject } from "vue";

                export default defineComponent({
                  setup() {
                    const probe = inject("jazor:service:Jazor.EmitTest.SsrBrowserProbe");
                    return () => h("main", { id: "ssr-service" }, probe?.Label ?? "missing");
                  }
                });
                """);

        public Task WriteDelayedComponentAsync()
            => File.WriteAllTextAsync(
                Path.Combine(RootPath, "jazor", "components", "delayed.mjs"),
                """
                import { defineComponent, h, onServerPrefetch } from "vue";

                export default defineComponent({
                  props: ["Delay"],
                  setup(props) {
                    onServerPrefetch(() => new Promise((resolve) => setTimeout(resolve, props.Delay)));
                    return () => h("main", { id: "delayed" }, String(Deno.pid));
                  }
                });
                """);

        public void Dispose()
        {
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
    }

    private sealed record WorkerProbe(int ProcessId, int RenderCount, string Version);
}
