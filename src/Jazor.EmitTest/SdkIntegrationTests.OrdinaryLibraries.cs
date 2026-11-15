using System.Text.Json;
using Jazor.NodeHost;

namespace Jazor.EmitTest;

public sealed partial class SdkIntegrationTests
{
    [TestMethod]
    [DataRow("Debug", false)]
    [DataRow("Release", false)]
    [DataRow("Debug", true)]
    [DataRow("Release", true)]
    [TestCategory("Consumer")]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_OrdinaryLibraryImports_UsesImplementationAssets(string configuration, bool useRazor)
    {
        var core = await LocalCorePackage.Value;
        var feed = core.PackageOutputDirectory;
        if (configuration == "Release") feed = (await LocalReleasePackage.Value).PackageOutputDirectory;
        using var workspace = new TestWorkspace(core.RepoRoot);
        var host = Path.Combine(workspace.SampleRoot, "Host");
        var library = Path.Combine(workspace.SampleRoot, "Plain");
        WriteFile(Path.Combine(library, "Plain.csproj"), """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net11.0</TargetFramework><ProduceReferenceAssembly>true</ProduceReferenceAssembly></PropertyGroup>
            </Project>
            """);
        WriteFile(Path.Combine(library, "State.cs"), """
            namespace Plain;
            public sealed class Counter
            {
                private int value;
                public Counter(int initial) { value = initial; }
                public int Add(int step) { value += step; return value; }
                public bool BackendOnly() => System.IO.File.Exists("server");
            }
            public static class State
            {
                private static int value;
                public static int Next() => ++value;
                public static bool BackendOnly() => System.IO.File.Exists("server");
            }
            """);
        WriteFile(Path.Combine(host, "Host.csproj"), $$"""
            <Project Sdk="{{(useRazor ? "Microsoft.NET.Sdk.Razor" : "Microsoft.NET.Sdk")}}">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework><OutputType>Exe</OutputType>
                <JazorMode>debug</JazorMode><JazorPublishSourceMaps>true</JazorPublishSourceMaps><JazorSSR>{{useRazor.ToString().ToLowerInvariant()}}</JazorSSR>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Jazor" Version="{{core.PackageVersion}}" />
                {{(useRazor ? "<PackageReference Include=\"Jazor.Vue\" Version=\"" + core.PackageVersion + "\" />" : "")}}
                <PackageReference Include="Fastenshtein" Version="1.1.1" />
                <ProjectReference Include="../Plain/Plain.csproj" />
              </ItemGroup>
            </Project>
            """);
        if (useRazor)
            WriteFile(Path.Combine(host, "OrdinaryComponent.razor"), """
                @using ECMAScript
                @using static ECMAScript.Vue
                @using Microsoft.AspNetCore.Components
                @using Microsoft.AspNetCore.Components.Web
                @using Plain
                @implements IVueComponent
                @attribute [ECMAScriptModule("components/ordinary.js")]
                <p>@counter.Add(0)</p>
                <button @onclick="Increment">+</button>
                @code {
                    private readonly Counter counter = new(4);
                    private void Increment() => counter.Add(1);
                }
                """);
        WriteFile(Path.Combine(host, "Program.cs"), """
            using ECMAScript;
            public static class Program { public static void Main() { } }
            [ECMAScriptModule("first.mjs")]
            public static class First
            {
                public static int Run() => Plain.State.Next();
                public static int Distance(string left, string right) => Fastenshtein.Levenshtein.Distance(left, right);
                public static int Add(int value) => new Plain.Counter(value).Add(3);
            }
            [ECMAScriptModule("second.mjs")]
            public static class Second { public static int Run() => Plain.State.Next(); }
            """);
        var project = Path.Combine(host, "Host.csproj");
        var arguments = new[] { "build", project, "-c", configuration, "/m:1", "/p:BuildInParallel=false",
            "-p:RestoreSources=" + feed, "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            "-p:RestorePackagesPath=" + workspace.RestorePackagesPath };
        var build = await RunDotNetAsync(core.RepoRoot, arguments);
        Assert.AreEqual(0, build.ExitCode, build.ToString());
        var output = Path.Combine(host, "jazor");
        var modules = Directory.GetFiles(Path.Combine(output, "_jazor", "libraries"), "*.mjs");
        Assert.HasCount(2, modules);
        Assert.IsTrue(modules.All(path => !File.ReadAllText(path).Contains("BackendOnly", StringComparison.Ordinal)));
        foreach (var path in modules)
        {
            using var map = JsonDocument.Parse(await File.ReadAllTextAsync(path + ".map"));
            Assert.IsTrue(map.RootElement.GetProperty("sources").EnumerateArray()
                .Any(source => source.GetString()!.Contains("jazor-recovered", StringComparison.Ordinal)));
        }
        var test = Path.Combine(output, "ordinary-libraries.test.mjs");
        WriteFile(test, """
            import { Run as first, Distance, Add } from './first.mjs';
            import { Run as second } from './second.mjs';
            globalThis.__jazorTest('ordinary project and NuGet library package consumer', () => {
                if (JSON.stringify([first(), second(), first()]) !== '[1,2,3]') throw new Error('Split library state');
                if (Add(4) !== 7) throw new Error('Imported instance state');
                for (const [left, right, expected] of [['kitten','sitting',3],['','',0],['a','',1],['中文','中',1],['😀','😀',0]]) {
                    if (Distance(left, right) !== expected) throw new Error(`Distance mismatch: ${left}, ${right}`);
                }
            });
            """);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        await NodeModuleRunner.RunAsync(output, test, cancellationToken: timeout.Token);
        if (useRazor)
        {
            var component = Path.Combine(output, "components", "ordinary.js");
            Assert.IsTrue(File.Exists(component), "Official Razor component module was not materialized.");
            var componentSpecifier = "./" + Path.GetRelativePath(output, component).Replace('\\', '/');
            var ssrTest = Path.Combine(output, "ordinary-razor-ssr.test.mjs");
            WriteFile(ssrTest, $$"""
                import { createSSRApp } from 'vue';
                import { renderToString } from '@vue/server-renderer';
                import component from {{JsonSerializer.Serialize(componentSpecifier)}};
                globalThis.__jazorTest('packaged official Razor component imports an ordinary class during SSR', async () => {
                    const html = await renderToString(createSSRApp(component));
                    if (!html.includes('<p>4</p>')) throw new Error(`Unexpected SSR output: ${html}`);
                });
                """);
            await NodeModuleRunner.RunAsync(output, ssrTest, cancellationToken: timeout.Token);
            if (configuration == "Release")
                await AssertOrdinaryLibraryBrowserAsync(output, workspace.RootPath);
        }

        var before = modules.ToDictionary(path => path, File.ReadAllText, StringComparer.Ordinal);
        var incremental = await RunDotNetAsync(core.RepoRoot, arguments);
        Assert.AreEqual(0, incremental.ExitCode, incremental.ToString());
        foreach (var entry in before) Assert.AreEqual(entry.Value, await File.ReadAllTextAsync(entry.Key));

        // Same assembly/version and public API, different implementation: MSBuild must refresh
        // the manifest and generated imports even when the consumer source does not change.
        var implementationSource = Path.Combine(library, "State.cs");
        await File.WriteAllTextAsync(implementationSource,
            (await File.ReadAllTextAsync(implementationSource)).Replace("value += step;", "value += step + 1;", StringComparison.Ordinal));
        var rebuilt = await RunDotNetAsync(core.RepoRoot, arguments);
        Assert.AreEqual(0, rebuilt.ExitCode, rebuilt.ToString());
        var updated = Directory.GetFiles(Path.Combine(output, "_jazor", "libraries"), "Plain-*.mjs")
            .Single(path => !before.ContainsKey(path));
        Assert.AreNotEqual(before.Single(entry => Path.GetFileName(entry.Key).StartsWith("Plain-", StringComparison.Ordinal)).Value,
            await File.ReadAllTextAsync(updated));
        var invalidationTest = Path.Combine(output, "ordinary-libraries-updated.test.mjs");
        WriteFile(invalidationTest, """
            import { Add } from './first.mjs?updated';
            globalThis.__jazorTest('implementation-only rebuild invalidates recovered modules', () => {
                if (Add(4) !== 8) throw new Error('Stale ordinary library implementation');
            });
            """);
        using var rebuildTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(40));
        await NodeModuleRunner.RunAsync(output, invalidationTest, cancellationToken: rebuildTimeout.Token);

        var libraryBuild = await RunDotNetAsync(core.RepoRoot, arguments.Concat(
            ["-p:OutputType=Library", "-p:CopyLocalLockFileAssemblies=false"]).ToArray());
        Assert.AreEqual(0, libraryBuild.ExitCode, libraryBuild.ToString());
        var preparedInputs = Directory.GetFiles(Path.Combine(host, "obj"), "*.jazorimports", SearchOption.AllDirectories).Single();
        StringAssert.Contains(await File.ReadAllTextAsync(preparedInputs), "asset|Fastenshtein|");
    }

    private static async Task AssertOrdinaryLibraryBrowserAsync(string output, string workspaceRoot)
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        Assert.IsNotNull(browserPath, "Ordinary library browser verification requires Chromium.");
        var harness = Path.Combine(workspaceRoot, "ordinary-browser");
        CopyDirectory(output, harness, includeGeneratedAssets: true);
        WriteFile(Path.Combine(harness, "ordinary-browser.mjs"), """
            import { createApp, nextTick } from 'vue';
            import component from './components/ordinary.js';
            import { Run as first, Distance } from './first.mjs';
            import { Run as second } from './second.mjs';
            try {
                createApp(component).mount('#app');
                if (document.querySelector('p').textContent !== '4') throw new Error('Initial imported state');
                document.querySelector('button').click();
                await nextTick();
                if (document.querySelector('p').textContent !== '5') throw new Error('Imported state through Vue proxy: ' + document.querySelector('p').textContent);
                if (JSON.stringify([first(), second(), first()]) !== '[1,2,3]') throw new Error('Split browser static state');
                if (Distance('kitten', 'sitting') !== 3) throw new Error('NuGet algorithm in browser');
                document.body.dataset.jazorSmoke = btoa(JSON.stringify({ok:true}));
            } catch (error) {
                document.body.dataset.jazorSmoke = btoa(JSON.stringify({ok:false,error:String(error)}));
            }
            """);
        WriteFile(Path.Combine(harness, "vite.config.js"), """
            import { defineConfig } from 'vite';
            export default defineConfig({ base: './', define: {
              __VUE_OPTIONS_API__: false, __VUE_PROD_DEVTOOLS__: false,
              __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: false
            }, build: { target:'esnext', rolldownOptions: {
              input:'ordinary-browser.mjs', output:{entryFileNames:'ordinary-browser.js'}
            } } });
            """);
        var bundle = await new Jazor.Emit.JavaScriptProjectBuilder().BuildAsync(harness);
        Assert.IsTrue(bundle.IsSuccess, bundle.Diagnostic?.Message);
        var index = Path.Combine(harness, "index.html");
        WriteFile(index, "<html><body><div id=\"app\"></div><script type=\"module\" src=\"./dist/ordinary-browser.js\"></script></body></html>");
        var browser = await BrowserSmokeTestHelper.RunBrowserDumpDomFromHttpAsync(browserPath!, index);
        Assert.AreEqual(0, browser.ExitCode, browser.ToString());
        using var payload = BrowserSmokeTestHelper.ReadBrowserSmokePayload(browser, "ordinary library");
        Assert.IsTrue(payload.RootElement.GetProperty("ok").GetBoolean(), payload.RootElement.ToString() +
            Environment.NewLine + await File.ReadAllTextAsync(Path.Combine(output, "components", "ordinary.js")));
    }
}
