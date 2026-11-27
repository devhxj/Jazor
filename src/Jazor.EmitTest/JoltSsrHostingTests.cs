using Jazor.AspNetCore;
using Microsoft.JavaScript.NodeApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace Jazor.EmitTest;

[TestClass]
public sealed class JoltSsrHostingTests
{
    private const string Template = """
        <div id="app"><!--jolt-ssr-outlet--></div>
        <div id="overlay"><!--jolt-ssr-teleport:#overlay--></div>
        <!--jolt-ssr-state-->
        <script type="module" src="/docs/assets/client.mjs"></script>
        """;

    [TestMethod]
    public async Task UseJoltSsr_ComposesDeploymentAndServesAssetsUnderPathBase()
    {
        using var deployment = new Deployment(Template);
        await using var app = deployment.Build();
        app.UsePathBase("/docs");
        app.UseJoltSsr(static (context, _) =>
            Task.FromResult(new JoltSsrRequest(new { Name = context.Request.Query["name"].ToString() })));
        await app.StartAsync();
        using var client = app.GetTestClient();

        var names = new[] { "Alice", "Bob" };
        var pages = await Task.WhenAll(names.Select(name => client.GetStringAsync($"/docs/editor?name={name}")));
        for (var index = 0; index < pages.Length; index++)
        {
            var name = names[index];
            var page = pages[index];
            StringAssert.Contains(page, $"<main>{name} @ /editor?name={name}</main>");
            StringAssert.Contains(page, $"<div id=\"overlay\"><!--teleport start anchor--><span>{name}</span><!--teleport anchor--></div>");
            StringAssert.Contains(page, """<script id="__jazor_ssr_state" type="application/json">""");
            StringAssert.Contains(page, """\u003C/script\u003E""");
            StringAssert.Contains(page, """src="/docs/assets/client.mjs"></script>""");
            Assert.IsFalse(page.Contains(names[1 - index], StringComparison.Ordinal));
        }

        using var asset = await client.GetAsync("/docs/assets/client.mjs");
        Assert.AreEqual(HttpStatusCode.OK, asset.StatusCode);
        Assert.AreEqual("export {};", await asset.Content.ReadAsStringAsync());
        StringAssert.Contains(await client.GetStringAsync("/docs?name=Alice"), "<main>Alice @ /?name=Alice</main>");
        using var template = await client.GetAsync("/docs/index.html");
        Assert.AreEqual(HttpStatusCode.NotFound, template.StatusCode);
    }

    [TestMethod]
    public async Task UseJoltSsr_RejectsMissingAndDuplicateTemplateMarkers()
    {
        foreach (var marker in new[] { "<!--jolt-ssr-outlet-->", "<!--jolt-ssr-state-->" })
        {
            foreach (var replacement in new[] { "", marker + marker })
            {
                using var deployment = new Deployment(Template.Replace(marker, replacement, StringComparison.Ordinal));
                await using var app = deployment.Build();
                var error = Assert.ThrowsExactly<InvalidOperationException>(() =>
                    app.UseJoltSsr(static (_, _) => Task.FromResult(new JoltSsrRequest())));
                StringAssert.Contains(error.Message, marker);
            }
        }
    }

    [TestMethod]
    public async Task BuildJoltSsr_AllowsBuiltinsAndRejectsAppConfiguredNpmExternals()
    {
        var source = RepositoryTemp.CreateSubdirectory("jolt-ssr-externals-");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        try
        {
            File.WriteAllText(Path.Combine(source.FullName, "package.json"),
                """{"private":true,"type":"module","dependencies":{"vue":"3.5.42","vite":"8.2.0"}}""");
            File.WriteAllText(Path.Combine(source.FullName, "index.html"), "<div id=\"app\"></div>");
            var entry = Path.Combine(source.FullName, "entry-server.mjs");
            File.WriteAllText(entry, """
                import { isBuiltin } from 'node:module';
                export async function render() {
                    const { version } = await import('./vue-version.mjs');
                    return { html: version + isBuiltin('node:module') };
                }
                """);
            File.WriteAllText(Path.Combine(source.FullName, "vue-version.mjs"), "export { version } from 'vue';");
            var store = Path.Combine(RepositoryTemp.Root, "nodeapi-store");
            Task<bool> Build() => Jolt.Vite.JoltViteToolchain.BuildSsrAsync(source.FullName, entry,
                _ => Task.FromResult(true), store, timeout.Token);
            Assert.IsTrue(await Build());
            var distribution = Path.Combine(source.FullName, ".jazor", "dist");
            Assert.IsTrue(File.Exists(Path.Combine(distribution, "client", "index.html")));
            var serverOutput = Path.Combine(distribution, "server", "entry-server.mjs");
            var previousServer = await File.ReadAllTextAsync(serverOutput, timeout.Token);

            File.WriteAllText(Path.Combine(source.FullName, "vite.config.mjs"),
                """export default { build: { rolldownOptions: { external: ['vue'] } } };""");
            var error = await Assert.ThrowsExactlyAsync<JSException>(() => Build());
            StringAssert.Contains(error.Message, "imports 'vue' outside the server bundle");
            StringAssert.Contains(error.Message, "Vite's SSR or Rolldown externals");
            Assert.AreEqual(previousServer, await File.ReadAllTextAsync(serverOutput, timeout.Token));
        }
        finally
        {
            source.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task JoltSsrRenderer_CancelledRenderReleasesWorkerAndClearsItsModuleState()
    {
        using var deployment = new Deployment(Template);
        await using var app = deployment.Build(workerCount: 1);
        var renderer = app.Services.GetRequiredService<IJoltSsrRenderer>();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(deadline.Token);
        var request = new JoltSsrRequest(new { Name = "Alice" });

        var before = await renderer.RenderAsync("/before", request, deadline.Token);
        StringAssert.Contains(before.Html, "<!--render:1-->");
        var delayed = renderer.RenderAsync("/delay", request, cancellation.Token);
        while (!File.Exists(deployment.DelayStartedPath))
            await Task.Delay(10, deadline.Token);
        await cancellation.CancelAsync();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => delayed);

        var after = await renderer.RenderAsync("/after", request, deadline.Token);
        StringAssert.Contains(after.Html, "<!--render:1-->");
    }

    private sealed class Deployment : IDisposable
    {
        private readonly DirectoryInfo _root = RepositoryTemp.CreateSubdirectory("jolt-ssr-host-");

        public Deployment(string index)
        {
            var client = Directory.CreateDirectory(Path.Combine(_root.FullName, "dist", "client"));
            var assets = client.CreateSubdirectory("assets");
            var server = Directory.CreateDirectory(Path.Combine(_root.FullName, "dist", "server"));
            File.WriteAllText(Path.Combine(client.FullName, "index.html"), index);
            File.WriteAllText(Path.Combine(assets.FullName, "client.mjs"), "export {};");
            File.WriteAllText(Path.Combine(server.FullName, "entry-server.mjs"), """
                import { writeFileSync } from 'node:fs';
                import { renderCount } from './chunk.mjs';
                export async function render({ url, state }) {
                    const count = renderCount();
                    if (url === '/delay') {
                        writeFileSync(new URL('../../delay-started', import.meta.url), '');
                        await new Promise(resolve => setTimeout(resolve, 30_000));
                    }
                    state.props.Snapshot = '</script>';
                    const name = state.props.Name;
                    return {
                        html: `<main>${name} @ ${url}</main><!--render:${count}-->`,
                        teleports: { '#overlay': `<!--teleport start anchor--><span>${name}</span><!--teleport anchor-->` },
                    };
                }
                """);
            File.WriteAllText(Path.Combine(server.FullName, "chunk.mjs"), "let count = 0; export const renderCount = () => ++count;");
        }

        public string DelayStartedPath => Path.Combine(_root.FullName, "delay-started");

        public WebApplication Build(int workerCount = 2)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = _root.FullName,
                EnvironmentName = Environments.Production
            });
            builder.WebHost.UseTestServer();
            builder.Services.AddJoltSsr("dist", workerCount);
            return builder.Build();
        }

        public void Dispose() => _root.Delete(recursive: true);
    }
}
