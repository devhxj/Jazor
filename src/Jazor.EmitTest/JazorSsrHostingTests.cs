using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using Jazor.AspNetCore;
using Jazor.AspNetCore.Dev;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed partial class JazorSsrHostingTests
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
                    "components/counter.js",
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
        StringAssert.Contains(html, "import { hydrate } from \"/docs/jazor/hydration.js\";");
        StringAssert.Contains(html, "await hydrate(\"components/counter.js\", \"app\");");
        StringAssert.Contains(html, "<script id=\"__jazor_ssr_state\" type=\"application/json\">{\"schema\":\"jazor-ssr-state\",\"version\":1,\"props\":{\"Title\":\"SSR \\u003Ctitle\\u003E\"},\"providers\":[{\"key\":\"app:feature\",\"value\":{\"Enabled\":true}}],\"authentication\":null}</script>");
        var hydration = await File.ReadAllTextAsync(Path.Combine(artifactRoot, "hydration.js"));
        StringAssert.Contains(hydration, "import { createSSRApp } from \"vue\";");
        StringAssert.Contains(hydration, "() => import(\"./components/counter.js\")");
        Assert.IsFalse(html.Contains("from \"vue\"", StringComparison.Ordinal));
        StringAssert.Contains(html, "\"Title\":\"SSR \\u003Ctitle\\u003E\"");
        Assert.IsFalse(html.Contains("type=\"importmap\"", StringComparison.Ordinal));
        Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "ssr-entry.js")));

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
            app => app.UseJazorSsr(new JazorSsrRequest("components/counter.js")));

        var client = host.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, "/features/ssr");
        request.Headers.Accept.ParseAdd("text/html");
        var response = await client.SendAsync(request);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "ssr-entry.js")));
    }

    [TestMethod]
    public async Task JazorSsrRenderer_UsesPackagedDenoHostRuntime()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        File.Delete(Path.Combine(artifactRoot, "jazor-manifest.json"));
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
            "components/counter.js",
            new { Title = "DenoHost" }));

        Assert.AreEqual("components/counter.js", result.ModulePath);
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
            "components/injected.js",
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
            "components/counter.js",
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
                "components/counter.js",
                Providers: [new JazorSsrProvider("", new { Enabled = true })])));

        StringAssert.Contains(error.Message, "non-empty keys");
        Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "ssr-entry.js")));
    }

    [TestMethod]
    public async Task JazorSsrRenderer_RejectsWhitespaceProviderKeyBeforeStartingWorker()
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
                "components/counter.js",
                Providers: [new JazorSsrProvider(" \t\r\n", new { Enabled = true })])));

        StringAssert.Contains(error.Message, "non-empty keys", StringComparison.Ordinal);
        Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "ssr-entry.js")));
    }

    [TestMethod]
    public async Task JazorSsrRenderer_RejectsDuplicateProviderKeysBeforeStartingWorker()
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
                "components/counter.js",
                Providers:
                [
                    new JazorSsrProvider("app:feature", new { Enabled = true }),
                    new JazorSsrProvider("app:feature", new { Enabled = false })
                ])));

        StringAssert.Contains(error.Message, "unique keys");
        Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "ssr-entry.js")));
    }

    [TestMethod]
    public async Task JazorSsrRenderer_RejectsAuthenticationProviderKeyCollisionBeforeStartingWorker()
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
                "components/counter.js",
                Providers: [new JazorSsrProvider(JazorAuthenticationState.ProviderKey, new { Spoofed = true })],
                Authentication: new JazorAuthenticationState(JazorAuthenticationStatus.Anonymous))));

        StringAssert.Contains(error.Message, "reserved", StringComparison.Ordinal);
        Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "ssr-entry.js")));
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
            "components/counter.js",
            Authentication: state));
        Assert.AreEqual(JazorSsrStateEnvelope.CurrentSchema, envelope.Schema);
        Assert.AreEqual(JazorSsrStateEnvelope.CurrentVersion, envelope.Version);
        Assert.AreEqual(JazorAuthenticationState.ProviderKey, envelope.Providers.Single().Key);
    }

    [TestMethod]
    public void JazorAuthenticationEnvelope_CreateProducesVersionedEndpointContract()
    {
        var state = new JazorAuthenticationState(JazorAuthenticationStatus.Forbidden, "user-42");
        var envelope = JazorAuthenticationEnvelope.Create(state);
        Assert.AreEqual(JazorAuthenticationState.EnvelopeSchema, envelope.Schema);
        Assert.AreEqual(JazorAuthenticationState.EnvelopeVersion, envelope.Version);
        Assert.AreSame(state, envelope.State);
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
            () => renderer.RenderAsync(new JazorSsrRequest("components/render-error.js")));

        StringAssert.Contains(error.Message, "render-boom");
        StringAssert.Contains(error.Message, "components/render-error.js");
    }

    [TestMethod]
    public async Task JazorSsrRenderer_DoesNotExecuteBrowserEntryDuringServerRendering()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await File.WriteAllTextAsync(
            Path.Combine(artifactRoot, "browser.js"),
            "document.querySelector('#app').textContent = 'browser startup';\n");
        ProjectEntryWriter.Write(artifactRoot, ["browser.js"], enableSsr: true);
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var result = await renderer.RenderAsync(new JazorSsrRequest(
            "components/counter.js", new { Title = "Server only" }));

        Assert.AreEqual("<main id=\"ssr-output\">Server only|prefetched</main>", result.Html);
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
            new JazorSsrRequest("components/worker-probe.js")));
        var second = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.js")));

        Assert.AreEqual(first.ProcessId, second.ProcessId);
        Assert.AreEqual(1, first.RenderCount);
        Assert.AreEqual(2, second.RenderCount);
        Assert.AreEqual("warm", second.Version);
    }

    [TestMethod]
    public async Task JazorSsrRenderer_ProjectGenerationChangeReplacesWorkersAndModuleCache()
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("before");
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();

        var before = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.js")));
        await workspace.WriteWorkerProbeAsync("after");
        await workspace.PublishGenerationAsync("generation-after-module-rewrite");
        var after = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.js")));

        Assert.AreNotEqual(before.ProcessId, after.ProcessId);
        Assert.AreEqual(1, after.RenderCount);
        Assert.AreEqual("after", after.Version);
    }

    [TestMethod]
    [DataRow("package.json")]
    [DataRow("deno.lock")]
    [DataRow("ssr-entry.js")]
    public async Task JazorSsrRenderer_ProjectInputChangeReplacesWorkersButTimestampChangeDoesNot(string fileName)
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("project-input");
        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();
        var request = new JazorSsrRequest("components/worker-probe.js");
        var before = ParseWorkerProbe(await renderer.RenderAsync(request));

        var inputPath = Path.Combine(artifactRoot, fileName);
        File.SetLastWriteTimeUtc(inputPath, File.GetLastWriteTimeUtc(inputPath).AddSeconds(-10));
        var touched = ParseWorkerProbe(await renderer.RenderAsync(request));
        Assert.AreEqual(before.ProcessId, touched.ProcessId);
        Assert.AreEqual(2, touched.RenderCount);

        // A valid content change in each project input must invalidate the warm module cache.
        await File.AppendAllTextAsync(inputPath, "\n");
        var changed = ParseWorkerProbe(await renderer.RenderAsync(request));
        Assert.AreNotEqual(before.ProcessId, changed.ProcessId);
        Assert.AreEqual(1, changed.RenderCount);
    }

    [TestMethod]
    [DataRow(".js")]
    [DataRow(".mjs")]
    public async Task JazorSsrRenderer_DenoWatchReloadsTransitiveModuleWithoutRewritingEntry(string extension)
    {
        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        await workspace.WriteWorkerProbeAsync("before");
        var leafPath = Path.Combine(artifactRoot, "components", "leaf" + extension);
        var barrelPath = Path.Combine(artifactRoot, "components", "barrel" + extension);
        await File.WriteAllTextAsync(leafPath, "export const version = 'before';");
        await File.WriteAllTextAsync(barrelPath, $"export {{ version }} from './leaf{extension}';");
        var componentPath = Path.Combine(artifactRoot, "components", "worker-probe.js");
        var component = await File.ReadAllTextAsync(componentPath);
        await File.WriteAllTextAsync(componentPath,
            $"import {{ version }} from './barrel{extension}';\n" + component.Replace("|before", "|${version}"));
        var projectInputs = new[] { "ssr-entry.js", "package.json", "deno.lock", "components/worker-probe.js" }
            .ToDictionary(name => name, name => File.ReadAllText(Path.Combine(artifactRoot, name)));

        await using var app = CreateRendererApplication(workspace.RootPath, artifactRoot, workerCount: 1, taskName: "ssr:dev");
        var renderer = app.Services.GetRequiredService<IJazorSsrRenderer>();
        var request = new JazorSsrRequest("components/worker-probe.js");
        Assert.AreEqual("before", ParseWorkerProbe(await renderer.RenderAsync(request)).Version);

        foreach (var version in new[] { "after", "second" })
        {
            await File.WriteAllTextAsync(leafPath, $"export const version = '{version}';");
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            WorkerProbe updated;
            while (true)
            {
                try
                {
                    updated = ParseWorkerProbe(await renderer.RenderAsync(request, timeout.Token));
                    if (updated.Version == version)
                        break;
                }
                catch (InvalidOperationException) when (!timeout.IsCancellationRequested)
                {
                    // A request overlapping Deno's restart may fail; it must not hang or be
                    // silently replayed by the host. The next request uses the new listener.
                }
                await Task.Delay(50, timeout.Token);
            }

            var warm = ParseWorkerProbe(await renderer.RenderAsync(request));
            Assert.AreEqual(version, warm.Version);
            Assert.AreEqual(updated.ProcessId, warm.ProcessId);
            Assert.AreEqual(updated.RenderCount + 1, warm.RenderCount);
        }

        foreach (var input in projectInputs)
            Assert.AreEqual(input.Value, await File.ReadAllTextAsync(Path.Combine(artifactRoot, input.Key)), input.Key);
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
            new JazorSsrRequest("components/worker-probe.js")));
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => renderer.RenderAsync(
            new JazorSsrRequest("components/crash.js")));
        var after = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.js")));

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
            new JazorSsrRequest("components/worker-probe.js")));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => renderer.RenderAsync(
            new JazorSsrRequest("components/delayed.js", new { Delay = 30_000 }),
            cancellation.Token));
        var after = ParseWorkerProbe(await renderer.RenderAsync(
            new JazorSsrRequest("components/worker-probe.js")));

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
                new JazorSsrRequest("components/delayed.js", new { Delay = 300 })))
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
            new JazorSsrRequest("components/worker-probe.js")));

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
                "Real browser SSR hydration smoke requires Google Chrome or Chromium. " +
                "Set RAZORVUE_BROWSER_EXE to the browser executable path.");
            return;
        }

        using var workspace = new SsrHostWorkspace();
        var artifactRoot = await workspace.CreateArtifactRootAsync();
        var serverOrigin = await workspace.StartDevServerAsync("components/hydration.js");
        await using var host = await CreateNetworkHostAsync(
            workspace.RootPath,
            artifactRoot,
            app =>
            {
                app.UsePathBase("/docs");
                app.UseJazorViteProxy();
                app.UseJazorSsr(new JazorSsrRequest(
                    "components/hydration.js",
                    new { Title = "SSR hydration" }));
            }, serverOrigin);

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
        int workerCount,
        string taskName = "ssr")
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
            options.TaskName = taskName;
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
        Action<WebApplication> configure,
        Uri serverOrigin)
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
            options.HydrationEntryPath = "hydration.js";
        });
        builder.Services.AddJazorViteProxy(options => options.ServerOrigin = serverOrigin);

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
        private Process? _devServer;
        private Task<string>? _devServerErrors;
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
                BuildMode.Production,
                ["vue", "@vue/server-renderer"]);
            await File.WriteAllTextAsync(Path.Combine(artifactRoot, "jazor-manifest.json"), "{}\n");
            var stylePath = Path.Combine(artifactRoot, "vendor", "test.css");
            Directory.CreateDirectory(Path.GetDirectoryName(stylePath)!);
            await File.WriteAllTextAsync(stylePath, "main{display:block;}");

            var componentPath = Path.Combine(artifactRoot, "components", "counter.js");
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

            var hydrationComponentPath = Path.Combine(artifactRoot, "components", "hydration.js");
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

            // SSR fixtures use the same standard package project as MSBuild Emit. Restore once
            // at the artifact root so DenoHost resolves Vue through node_modules; the SSR host
            // remains a consumer of this already-prepared graph.
            ProjectEntryWriter.Write(
                artifactRoot,
                ["components/counter.js", "components/hydration.js"],
                enableSsr: true);
            LibraryPackageWriter.WritePackageProject(artifactRoot, materialization, hasSsrEntry: true);
            await DenoPackageRestorer.RestoreAndCheckAsync(
                artifactRoot,
                ResolveTestDenoExecutable(),
                [Path.Combine(artifactRoot, "entry.js"), Path.Combine(artifactRoot, "ssr-entry.js")],
                materialization,
                CancellationToken.None);
            await File.WriteAllTextAsync(
                Path.Combine(artifactRoot, "jazor-manifest.json"),
                "{\"generation\":\"fixture\"}\n");
            Assert.IsFalse(File.Exists(Path.Combine(artifactRoot, "manifest.json")));
            Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "ssr-entry.js")));
            Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "package.json")));
            Assert.IsTrue(File.Exists(Path.Combine(artifactRoot, "deno.lock")));
            return artifactRoot;
        }

        private static string ResolveTestDenoExecutable()
        {
            var runtimeName = OperatingSystem.IsWindows() ? "deno.exe" : "deno";
            var rid = OperatingSystem.IsWindows()
                ? (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? "win-arm64" : "win-x64")
                : OperatingSystem.IsMacOS()
                    ? (System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? "osx-arm64" : "osx-x64")
                    : "linux-x64";
            var path = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", runtimeName);
            return File.Exists(path) ? path : runtimeName;
        }

        public async Task<Uri> StartDevServerAsync(params string[] components)
        {
            var root = Path.Combine(RootPath, "jazor");
            ProjectEntryWriter.Write(root, components, enableSsr: true);
            await File.WriteAllTextAsync(Path.Combine(root, "test-server.js"), """
                import { createServer } from 'vite';
                const server = await createServer({ configFile: false, base: '/docs/jazor/',
                  logLevel: 'silent', server: { host: '127.0.0.1', port: 0 } });
                await server.listen();
                console.log(server.resolvedUrls.local[0]);
                """);
            var start = new ProcessStartInfo(ResolveTestDenoExecutable())
            {
                WorkingDirectory = root, UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            };
            foreach (var argument in new[] { "run", "-A", "test-server.js" })
                start.ArgumentList.Add(argument);
            _devServer = Process.Start(start)!;
            _devServerErrors = _devServer.StandardError.ReadToEndAsync();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var address = await _devServer.StandardOutput.ReadLineAsync(timeout.Token);
            Assert.IsNotNull(address, _devServer.HasExited ? await _devServerErrors : "Vite did not publish its URL.");
            return new Uri(new Uri(address), "/");
        }

        public async Task PublishGenerationAsync(string generation)
        {
            var entryPath = Path.Combine(RootPath, "jazor", "ssr-entry.js");
            var source = await File.ReadAllTextAsync(entryPath);
            await File.WriteAllTextAsync(
                entryPath,
                "// generation " + System.Text.Json.JsonSerializer.Serialize(generation) + "\n" + source);
        }

        public Task WriteWorkerProbeAsync(string version)
            => File.WriteAllTextAsync(
                Path.Combine(RootPath, "jazor", "components", "worker-probe.js"),
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
                Path.Combine(RootPath, "jazor", "components", "crash.js"),
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
                Path.Combine(RootPath, "jazor", "components", "render-error.js"),
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
                Path.Combine(RootPath, "jazor", "components", "injected.js"),
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
                Path.Combine(RootPath, "jazor", "components", "delayed.js"),
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
            if (_devServer is not null)
            {
                if (!_devServer.HasExited)
                    _devServer.Kill(entireProcessTree: true);
                _devServer.WaitForExit();
                _ = _devServerErrors?.GetAwaiter().GetResult();
                _devServer.Dispose();
            }
            if (Directory.Exists(RootPath))
                Directory.Delete(RootPath, recursive: true);
        }
    }

    private sealed record WorkerProbe(int ProcessId, int RenderCount, string Version);
}
