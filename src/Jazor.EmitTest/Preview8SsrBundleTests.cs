using System.Runtime.InteropServices;
using Jazor.AspNetCore;
using Jazor.Emit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Jazor.EmitTest;

[TestClass]
public sealed class Preview8SsrBundleTests
{
    [TestMethod]
    [DataRow("Development", false)]
    [DataRow("Production", true)]
    public async Task SsrLocator_SourceAndPreviousReleaseCoexist_SelectsCurrentEnvironment(string environment, bool bundled)
    {
        var root = RepositoryTemp.CreateDirectory("preview8-ssr-environment-");
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "ssr"));
            File.WriteAllText(Path.Combine(root, "ssr-entry.js"), "// current development source");
            File.WriteAllText(Path.Combine(root, "package.json"), "{}");
            File.WriteAllText(Path.Combine(root, "deno.lock"), "{}");
            File.WriteAllText(Path.Combine(root, "ssr", "ssr-entry.js"), "// previous release bundle");
            File.WriteAllText(Path.Combine(root, "ssr", "package.json"), "{}");
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = root,
                EnvironmentName = environment
            });
            await using var host = builder.Build();
            var locator = new SsrArtifactLocator(builder.Environment,
                Options.Create(new JazorSsrOptions { ArtifactRootPath = root }));

            var artifacts = locator.Resolve();

            Assert.AreEqual(bundled, artifacts.IsBundled);
            Assert.AreEqual(bundled ? Path.Combine(root, "ssr") : root, artifacts.RootPath);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ProductionSsrBundle_RendersFromCopiedRuntimeWithoutSourcesOrNodeModules()
    {
        var root = Path.Combine(RepositoryTemp.Root, "preview8-ssr-bundle", Guid.NewGuid().ToString("N"));
        var source = Path.Combine(root, "source");
        var published = Path.Combine(root, "published");
        var deno = Path.Combine(AppContext.BaseDirectory, "runtimes", RuntimeInformation.RuntimeIdentifier,
            "native", OperatingSystem.IsWindows() ? "deno.exe" : "deno");
        Assert.IsTrue(File.Exists(deno), "Use the test's current-output Deno runtime.");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        try
        {
            var manifest = Path.Combine(FindRepositoryRoot(), "src", "ECMAScript.Vue", "manifest.json");
            var libraries = new LibraryMaterializer().Materialize([manifest], source, BuildMode.Production,
                ["vue", "@vue/server-renderer"]);
            Directory.CreateDirectory(Path.Combine(source, "components"));
            await File.WriteAllTextAsync(Path.Combine(source, "components", "counter.js"), """
                import { defineComponent, h, onServerPrefetch, ref } from 'vue';
                export default defineComponent({
                  props: ['Title'],
                  setup(props) {
                    const phase = ref('before');
                    onServerPrefetch(async () => { phase.value = 'prefetched'; });
                    return () => h('main', { id: 'published-ssr' }, props.Title + '|' + phase.value);
                  }
                });
                """, timeout.Token);
            var entries = ProjectEntryWriter.Write(source, ["components/counter.js"], enableSsr: true);
            LibraryPackageWriter.WritePackageProject(source, libraries, hasSsrEntry: true);
            await DenoPackageRestorer.RestoreAndCheckAsync(source, deno, entries, libraries, timeout.Token);
            var built = await new JavaScriptProjectBuilder().BuildAsync(source, deno, timeout.Token, "build:ssr");
            Assert.IsTrue(built.IsSuccess, built.Diagnostic?.Message);
            ProjectEntryWriter.WriteSsrRuntimePackage(source);

            var bundle = Path.Combine(source, "ssr");
            foreach (var file in Directory.GetFiles(bundle, "*", SearchOption.AllDirectories))
            {
                var destination = Path.Combine(published, "ssr", Path.GetRelativePath(bundle, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination);
            }
            // Removing the complete original project proves no emitted component path or
            // npm import leaks back into the independently deployed runtime closure.
            Directory.Delete(source, recursive: true);
            Assert.IsFalse(Directory.Exists(Path.Combine(published, "node_modules")));
            Assert.IsFalse(File.Exists(Path.Combine(published, "package.json")));
            Assert.IsFalse(File.Exists(Path.Combine(published, "deno.lock")));
            Assert.IsFalse(File.Exists(Path.Combine(published, "ssr", "deno.lock")));

            // Exercise the actual ASP.NET locator and DenoHost task launcher. Starting the
            // entry directly would miss task working-directory and runtime-package defects.
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = published,
                EnvironmentName = Environments.Production
            });
            builder.Services.AddJazorSsr(options =>
            {
                options.ArtifactRootPath = published;
                options.WorkerCount = 1;
            });
            await using var host = builder.Build();
            var renderer = host.Services.GetRequiredService<IJazorSsrRenderer>();
            var rendered = await renderer.RenderAsync(new JazorSsrRequest(
                "components/counter.js", new { Title = "Published <title>" }), timeout.Token);
            Assert.AreEqual("<main id=\"published-ssr\">Published &lt;title&gt;|prefetched</main>", rendered.Html);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }
        throw new InvalidOperationException("Repository root not found.");
    }
}
