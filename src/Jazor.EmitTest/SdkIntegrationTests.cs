using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SdkIntegrationTests
{
    private static readonly Lazy<Task<LocalPackageFixture>> LocalPackage = new(CreateLocalPackageAsync);
    private static readonly Lazy<Task<LocalStylePackageFixture>> LocalStylePackage = new(CreateLocalStylePackageAsync);
    private static readonly SemaphoreSlim SourceReferencedRazorVueBuildGate = new(1, 1);

    [TestMethod]
    public async Task CreateLocalPackage_SeparatesSharedAndRazorVueAnalyzers()
    {
        var package = await LocalPackage.Value;

        using var jazorArchive = ZipFile.OpenRead(package.PackagePath);
        var jazorAnalyzerEntries = jazorArchive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .Where(static path => path.StartsWith("analyzers/dotnet/cs/", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        CollectionAssert.Contains(jazorAnalyzerEntries, "analyzers/dotnet/cs/Jazor.Analyzer.dll");
        Assert.IsFalse(
            jazorAnalyzerEntries.Any(static path =>
                path.EndsWith("/Jazor.RazorVue.dll", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith("/Jazor.RazorVue.Generator.dll", StringComparison.OrdinalIgnoreCase)),
            "Jazor must not install the opt-in RazorVue generator assembly.");

        using var vueArchive = ZipFile.OpenRead(package.VuePackagePath);
        var vueAnalyzerEntries = vueArchive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .Where(static path => path.StartsWith("analyzers/dotnet/cs/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        CollectionAssert.AreEqual(
            new[]
            {
                "analyzers/dotnet/cs/Jazor.RazorVue.dll",
                "analyzers/dotnet/cs/Jazor.RazorVue.pdb"
            },
            vueAnalyzerEntries,
            "Jazor.Vue must install only the merged RazorVue analyzer and rely on Jazor for shared dependencies.");
    }

    [TestMethod]
    public async Task CreateLocalPackage_IncludesVuetifyAuthoringPackage()
    {
        var package = await LocalPackage.Value;

        using var archive = ZipFile.OpenRead(package.VuetifyPackagePath);
        var entryNames = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        var nuspec = ReadPackageEntryText(package.VuetifyPackagePath, "ECMAScript.Vuetify.nuspec");

        CollectionAssert.Contains(entryNames, "lib/net11.0/ECMAScript.Vuetify.dll");
        CollectionAssert.Contains(entryNames, "ECMAScript.Vuetify.nuspec");
        StringAssert.Contains(nuspec, "<dependency id=\"Jazor\"");
    }

    [TestMethod]
    public async Task CreateLocalPackage_IncludesVueRouteAuthoringPackage()
    {
        var package = await LocalPackage.Value;

        using var archive = ZipFile.OpenRead(package.VueRoutePackagePath);
        var entryNames = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        var nuspec = ReadPackageEntryText(package.VueRoutePackagePath, "ECMAScript.VueRoute.nuspec");

        CollectionAssert.Contains(entryNames, "lib/net11.0/ECMAScript.VueRoute.dll");
        CollectionAssert.Contains(entryNames, "ECMAScript.VueRoute.nuspec");
        StringAssert.Contains(nuspec, "<dependency id=\"Jazor\"");
    }

    [TestMethod]
    public async Task CreateLocalPackage_IncludesPiniaPackages()
    {
        var package = await LocalPackage.Value;

        using var piniaArchive = ZipFile.OpenRead(package.PiniaPackagePath);
        var piniaEntryNames = piniaArchive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        var piniaNuspec = ReadPackageEntryText(package.PiniaPackagePath, "ECMAScript.Pinia.nuspec");
        CollectionAssert.Contains(piniaEntryNames, "lib/net11.0/ECMAScript.Pinia.dll");
        CollectionAssert.Contains(piniaEntryNames, "ECMAScript.Pinia.nuspec");
        StringAssert.Contains(piniaNuspec, "<dependency id=\"Jazor\"");

        using var piniaTestingArchive = ZipFile.OpenRead(package.PiniaTestingPackagePath);
        var piniaTestingEntryNames = piniaTestingArchive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        var piniaTestingNuspec = ReadPackageEntryText(package.PiniaTestingPackagePath, "ECMAScript.Pinia.Testing.nuspec");
        CollectionAssert.Contains(piniaTestingEntryNames, "lib/net11.0/ECMAScript.Pinia.Testing.dll");
        CollectionAssert.Contains(piniaTestingEntryNames, "ECMAScript.Pinia.Testing.nuspec");
        CollectionAssert.Contains(piniaTestingEntryNames, "buildTransitive/ECMAScript.Pinia.Testing.targets");
        CollectionAssert.Contains(piniaTestingEntryNames, "jazor/pinia-testing/manifest.json");
        CollectionAssert.Contains(piniaTestingEntryNames, "jazor/pinia-testing/dist/index.mjs");
        CollectionAssert.Contains(piniaTestingEntryNames, "jazor/pinia-testing/licenses/LICENSE");
        StringAssert.Contains(piniaTestingNuspec, "<dependency id=\"ECMAScript.Pinia\"");
    }

    [TestMethod]
    public async Task CreateLocalPackage_IncludesTDesignAuthoringPackage()
    {
        var package = await LocalPackage.Value;

        using var archive = ZipFile.OpenRead(package.TDesignPackagePath);
        var entryNames = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        var nuspec = ReadPackageEntryText(package.TDesignPackagePath, "ECMAScript.TDesign.nuspec");

        CollectionAssert.Contains(entryNames, "lib/net11.0/ECMAScript.TDesign.dll");
        CollectionAssert.Contains(entryNames, "ECMAScript.TDesign.nuspec");
        StringAssert.Contains(nuspec, "<dependency id=\"Jazor\"");
        StringAssert.Contains(nuspec, "<frameworkReference name=\"Microsoft.AspNetCore.App\" />");
    }

    [TestMethod]
    public async Task CreateLocalPackage_IncludesSelfContainedBrowserAssets()
    {
        var package = await LocalPackage.Value;

        AssertPackageEntries(
            package.PackagePath,
            "lib/net11.0/ECMAScript.Vue3.dll",
            "jazor/vue3/manifest.json",
            "jazor/vue3/dist/vue.runtime.esm-browser.js",
            "jazor/vue3/dist/vue.runtime.esm-browser.prod.js",
            "jazor/vue3/dist/server-renderer.esm-browser.js",
            "jazor/vue3/dist/server-renderer.esm-browser.prod.js",
            "jazor/vue3/dist/devtools-api/index.js",
            "jazor/vue3/dist/devtools-api/api/index.js",
            "jazor/vue3/licenses/LICENSE",
            "jazor/vue3/licenses/VUE-DEVTOOLS-API-LICENSE",
            "jazor/vue3/licenses/VUE-SERVER-RENDERER-LICENSE",
            "tools/net11.0/tooling/vue/compiler-sfc.esm-browser.js",
            "tools/net11.0/tooling/vue/licenses/LICENSE");
        AssertPackageEntries(
            package.VuetifyPackagePath,
            "lib/net11.0/ECMAScript.Vuetify.dll",
            "buildTransitive/ECMAScript.Vuetify.targets",
            "jazor/vuetify/manifest.json",
            "jazor/vuetify/dist/vuetify.esm.js",
            "jazor/vuetify/dist/components.mjs",
            "jazor/vuetify/dist/directives.mjs",
            "jazor/vuetify/dist/vuetify.min.css",
            "jazor/vuetify/licenses/LICENSE.md");
        AssertPackageEntries(
            package.VueRoutePackagePath,
            "lib/net11.0/ECMAScript.VueRoute.dll",
            "buildTransitive/ECMAScript.VueRoute.targets",
            "jazor/vue-router/manifest.json",
            "jazor/vue-router/dist/vue-router.esm-browser.prod.js",
            "jazor/vue-router/licenses/LICENSE");
        AssertPackageEntries(
            package.PiniaPackagePath,
            "lib/net11.0/ECMAScript.Pinia.dll",
            "buildTransitive/ECMAScript.Pinia.targets",
            "jazor/pinia/manifest.json",
            "jazor/pinia/dist/pinia.mjs",
            "jazor/pinia/licenses/LICENSE");
        AssertPackageEntries(
            package.PiniaTestingPackagePath,
            "lib/net11.0/ECMAScript.Pinia.Testing.dll",
            "buildTransitive/ECMAScript.Pinia.Testing.targets",
            "jazor/pinia-testing/manifest.json",
            "jazor/pinia-testing/dist/index.mjs",
            "jazor/pinia-testing/licenses/LICENSE");
        AssertPackageEntries(
            package.TDesignPackagePath,
            "lib/net11.0/ECMAScript.TDesign.dll",
            "buildTransitive/ECMAScript.TDesign.targets",
            "jazor/tdesign-vue-next/manifest.json",
            "jazor/tdesign-vue-next/dist/tdesign.mjs",
            "jazor/tdesign-vue-next/dist/tdesign.css",
            "jazor/tdesign-vue-next/licenses/LICENSE");
        AssertPackageEntries(
            package.ElementPlusPackagePath,
            "lib/net11.0/ECMAScript.ElementPlus.dll",
            "buildTransitive/ECMAScript.ElementPlus.targets",
            "jazor/element-plus/manifest.json",
            "jazor/element-plus/dist/index.full.min.mjs",
            "jazor/element-plus/dist/index.css",
            "jazor/element-plus/licenses/LICENSE");
    }

    [TestMethod]
    public async Task CreateLocalPackage_Vue3DevtoolsApi_SatisfiesVueRouterAndPiniaDevelopmentImports()
    {
        var package = await LocalPackage.Value;
        using var manifest = JsonDocument.Parse(ReadPackageEntryText(package.PackagePath, "jazor/vue3/manifest.json"));

        var devtools = manifest.RootElement.GetProperty("imports").GetProperty("@vue/devtools-api");
        Assert.AreEqual("dist/devtools-api/index.js", devtools.GetProperty("development").GetString());
        Assert.AreEqual("dist/devtools-api/index.js", devtools.GetProperty("production").GetString());
        var serverRenderer = manifest.RootElement.GetProperty("imports").GetProperty("@vue/server-renderer");
        Assert.AreEqual("dist/server-renderer.esm-browser.js", serverRenderer.GetProperty("development").GetString());
        Assert.AreEqual("dist/server-renderer.esm-browser.prod.js", serverRenderer.GetProperty("production").GetString());
        var files = manifest.RootElement.GetProperty("files")
            .EnumerateArray()
            .Select(static value => value.GetString())
            .ToArray();
        CollectionAssert.Contains(files, "dist/devtools-api/api/index.js");
        CollectionAssert.Contains(files, "licenses/VUE-DEVTOOLS-API-LICENSE");
        CollectionAssert.Contains(files, "licenses/VUE-SERVER-RENDERER-LICENSE");

        var devtoolsApi = ReadPackageEntryText(package.PackagePath, "jazor/vue3/dist/devtools-api/index.js");
        Assert.IsFalse(
            Regex.IsMatch(devtoolsApi, "\\b(?:from|import)\\s+[\\\"'](?!\\.)", RegexOptions.CultureInvariant),
            "The bundled devtools API may only use local relative module imports.");

        var router = ReadPackageEntryText(package.VueRoutePackagePath, "jazor/vue-router/dist/vue-router.esm-browser.js");
        var pinia = ReadPackageEntryText(package.PiniaPackagePath, "jazor/pinia/dist/pinia.mjs");
        StringAssert.Contains(router, "from \"@vue/devtools-api\"", StringComparison.Ordinal);
        StringAssert.Contains(pinia, "from '@vue/devtools-api'", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task CreateLocalPackage_TDesignManifest_DeclaresOnlyPackagedAssetsAndVue3()
    {
        var package = await LocalPackage.Value;
        using var manifest = JsonDocument.Parse(ReadPackageEntryText(
            package.TDesignPackagePath,
            "jazor/tdesign-vue-next/manifest.json"));

        var root = manifest.RootElement;
        Assert.AreEqual("tdesign-vue-next", root.GetProperty("libraryId").GetString());
        Assert.AreEqual("1.20.5", root.GetProperty("version").GetString());
        var entry = root.GetProperty("imports").GetProperty("tdesign-vue-next");
        Assert.AreEqual("dist/tdesign.mjs", entry.GetProperty("development").GetString());
        Assert.AreEqual("dist/tdesign.mjs", entry.GetProperty("production").GetString());
        Assert.AreEqual("^3.5.0", root.GetProperty("requires").GetProperty("vue3").GetString());
        CollectionAssert.AreEqual(
            new[] { "dist/tdesign.css" },
            root.GetProperty("styles").EnumerateArray().Select(static value => value.GetString()).ToArray());
        CollectionAssert.AreEqual(
            new[] { "licenses/LICENSE" },
            root.GetProperty("files").EnumerateArray().Select(static value => value.GetString()).ToArray());

        var esm = ReadPackageEntryText(package.TDesignPackagePath, "jazor/tdesign-vue-next/dist/tdesign.mjs");
        var imports = Regex.Matches(esm, "\\b(?:from|import)\\s+[\\\"'](?<specifier>[^\\\"']+)[\\\"']", RegexOptions.CultureInvariant)
            .Select(static match => match.Groups["specifier"].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static specifier => specifier, StringComparer.Ordinal)
            .ToArray();
        CollectionAssert.AreEqual(new[] { "vue" }, imports);

        var css = ReadPackageEntryText(package.TDesignPackagePath, "jazor/tdesign-vue-next/dist/tdesign.css");
        Assert.IsFalse(Regex.IsMatch(css, "https?://", RegexOptions.CultureInvariant), "TDesign CSS must not fetch remote assets.");
    }

    [TestMethod]
    public async Task CreateLocalPackage_IncludesEcmaScriptStyleAsIndependentOptInPackage()
    {
        var package = await LocalStylePackage.Value;

        using var archive = ZipFile.OpenRead(package.StylePackagePath);
        var entryNames = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        var nuspec = ReadPackageEntryText(package.StylePackagePath, "ECMAScript.Style.nuspec");

        CollectionAssert.Contains(entryNames, "lib/net11.0/ECMAScript.Style.dll");
        CollectionAssert.Contains(entryNames, "README.md");
        StringAssert.Contains(nuspec, "<dependency id=\"Jazor\" version=\"[" + package.PackageVersion + "]\" />");
        Assert.IsFalse(
            entryNames.Any(static path => path.StartsWith("build", StringComparison.OrdinalIgnoreCase)),
            "ECMAScript.Style must rely on Jazor's existing build integration and must not install CSS-specific targets.");
    }

    [TestMethod]
    public async Task Build_LocalEcmaScriptStylePackage_DebugMaterializesAndReleaseBundlesRuntime()
    {
        var package = await LocalStylePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "EcmaScriptStylePackageConsumer");
        var projectPath = CreateEcmaScriptStylePackageConsumerProject(projectRoot);
        var commonArguments = new[]
        {
            "-t:Rebuild",
            "/m:1",
            "/p:BuildInParallel=false",
            $"-p:RestoreSources={package.PackageOutputDirectory}",
            "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            $"-p:RestorePackagesPath={package.RestorePackagesPath}",
            $"-p:JazorPackageVersion={package.PackageVersion}"
        };

        var debugBuild = await RunDotNetAsync(
            package.RepoRoot,
            ["build", projectPath, .. commonArguments, "-p:JazorMode=debug"]);
        Assert.AreEqual(0, debugBuild.ExitCode, debugBuild.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var runtimePath = Path.Combine(outputRoot, "style.mjs");
        var runtimeMapPath = runtimePath + ".map";
        var appPath = Path.Combine(outputRoot, "app.mjs");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");

        Assert.IsTrue(File.Exists(runtimePath), $"ECMAScript.Style runtime was not materialized: {runtimePath}");
        Assert.IsTrue(File.Exists(runtimeMapPath), $"ECMAScript.Style source map was not materialized: {runtimeMapPath}");
        Assert.IsTrue(File.Exists(appPath), $"Consumer module was not materialized: {appPath}");
        Assert.IsTrue(File.Exists(manifestPath), $"Debug manifest was not generated: {manifestPath}");
        var appModule = await File.ReadAllTextAsync(appPath);
        StringAssert.Contains(appModule, "from \"style.mjs\"");
        StringAssert.Contains(appModule, "\"background-color\": hex(\"1769aa\")");
        StringAssert.Contains(appModule, "context({");
        Assert.IsFalse(appModule.Contains("context as ", StringComparison.Ordinal), appModule);
        StringAssert.Contains(appModule, "styleIn");
        StringAssert.Contains(appModule, "atRuleIn");
        StringAssert.Contains(appModule, "snapshotFrom");

        var manifest = LoadManifest(manifestPath);
        var runtimeEntry = manifest.Modules.Single(static entry => entry.RelativePath == "style.mjs");
        Assert.AreEqual("style.mjs.map", runtimeEntry.SourceMapPath);
        Assert.HasCount(64, runtimeEntry.Hash);
        Assert.HasCount(64, runtimeEntry.MapHash!);

        var releaseBuild = await RunDotNetAsync(
            package.RepoRoot,
            ["build", projectPath, .. commonArguments, "-p:JazorMode=release"]);
        Assert.AreEqual(0, releaseBuild.ExitCode, releaseBuild.ToString());

        var bundlePath = Path.Combine(outputRoot, "bundle.js");
        var bundleMapPath = Path.Combine(outputRoot, "bundle.js.map");
        Assert.IsTrue(File.Exists(bundlePath), $"ECMAScript.Style bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(bundleMapPath), $"ECMAScript.Style bundle source map was not generated: {bundleMapPath}");
        Assert.IsFalse(File.Exists(runtimePath), "Release must not retain the debug runtime module.");
        Assert.IsFalse(File.Exists(manifestPath), "Release must not retain the debug manifest.");

        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(bundle, "ecmascript-style:v1");
        StringAssert.Contains(bundle, "ecs-");
        StringAssert.Contains(bundle, "font-face");
        StringAssert.Contains(bundle, "server-css");
        StringAssert.Contains(bundle, "sourceMappingURL=bundle.js.map");
        Assert.IsFalse(bundle.Contains("from \"style.mjs\"", StringComparison.Ordinal), bundle);
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_MultiProjectSample_ReleaseWritesOnlyBundle()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);
        var restorePackagesPath = package.RestorePackagesPath;

        var hostProjectPath = Path.Combine(workspace.SampleRoot, "Sample.Host", "Sample.Host.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                hostProjectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=release"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var debugManifestPath = Path.Combine(hostRoot, "wwwroot", "jazor", "jazor-manifest.json");
        var bundlePath = Path.Combine(hostRoot, "wwwroot", "jazor", "bundle.js");

        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsFalse(
            File.Exists(debugManifestPath),
            $"Bundle must not materialize debug artifacts: {debugManifestPath}");

        var bundle = await File.ReadAllTextAsync(bundlePath);

        StringAssert.Contains(bundle, "function Prefix()");
        StringAssert.Contains(bundle, "function Greet(name)");
        StringAssert.Contains(bundle, "function Boot()");
        StringAssert.Contains(bundle, "export {");
        StringAssert.Contains(bundle, "Boot");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_ReleaseWithSsrEnabled_MaterializesRawModuleGraph()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "SsrReleaseSdkSample");
        var projectPath = CreateDefaultOutputStaticHostProject(projectRoot);
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={package.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=release",
                "-p:JazorSsrEnabled=true"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var browserRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var ssrRoot = Path.Combine(browserRoot, "ssr");
        Assert.IsTrue(File.Exists(Path.Combine(browserRoot, "bundle.js")), "Release browser bundle was not generated.");
        Assert.IsTrue(File.Exists(Path.Combine(ssrRoot, "jazor-manifest.json")), "SSR application manifest was not generated.");
        Assert.IsTrue(File.Exists(Path.Combine(ssrRoot, "host", "app.mjs")), "SSR raw module graph was not generated.");
        Assert.IsTrue(File.Exists(Path.Combine(ssrRoot, "importmap.json")), "SSR browser import map was not generated.");
        Assert.IsTrue(File.Exists(Path.Combine(ssrRoot, "ssr-importmap.json")), "SSR local import map was not generated.");
        Assert.IsTrue(File.Exists(Path.Combine(ssrRoot, "manifest.json")), "SSR asset manifest was not generated.");
        Assert.IsTrue(
            File.Exists(Path.Combine(ssrRoot, "vendor", "vue3", "3.5.13", "dist", "server-renderer.esm-browser.prod.js")),
            "SSR server renderer was not materialized.");
        Assert.IsTrue(
            File.Exists(Path.Combine(ssrRoot, "vendor", "vue3", "3.5.13", "licenses", "VUE-SERVER-RENDERER-LICENSE")),
            "SSR server renderer license was not materialized.");

        var ssrImportMap = await File.ReadAllTextAsync(Path.Combine(ssrRoot, "ssr-importmap.json"));
        StringAssert.Contains(ssrImportMap, "\"@vue/server-renderer\"");
        Assert.IsFalse(ssrImportMap.Contains("node_modules", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Publish_LocalJazorPackage_WebSdkHost_ReleaseWithSsrEnabled_CopiesRawModuleGraph()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "SsrPublishSdkSample");
        var publishOutputRoot = Path.Combine(workspace.RootPath, "publish-output");
        var projectPath = CreateDefaultOutputWebHostProject(projectRoot);
        var publish = await RunDotNetAsync(
            package.RepoRoot,
            [
                "publish",
                projectPath,
                "-c",
                "Debug",
                "-o",
                publishOutputRoot,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={package.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=release",
                "-p:JazorSsrEnabled=true"
            ]);

        Assert.AreEqual(0, publish.ExitCode, publish.ToString());

        var publishedSsrRoot = Path.Combine(publishOutputRoot, "wwwroot", "jazor", "ssr");
        Assert.IsTrue(File.Exists(Path.Combine(publishedSsrRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedSsrRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedSsrRoot, "importmap.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedSsrRoot, "ssr-importmap.json")));
        Assert.IsTrue(
            File.Exists(Path.Combine(publishedSsrRoot, "vendor", "vue3", "3.5.13", "dist", "server-renderer.esm-browser.prod.js")),
            "Publish output must carry the SSR renderer dependency.");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_SingleProjectWrapperApis_EmitsMinimalRuntimeImports()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var restorePackagesPath = package.RestorePackagesPath;

        var wwwroot = Path.Combine(hostRoot, "wwwroot");
        if (Directory.Exists(wwwroot))
            Directory.Delete(wwwroot, recursive: true);

        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using ECMAScript;
            using System;
            using System.Globalization;
            using Sample.Features;

            namespace Sample.Host;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => GreeterModule.Greet("Jazor");

                public static string DateOnlyText() => DateOnly.Parse("2024-01-02").ToString();

                public static string OffsetText() => DateTimeOffset.Parse("2024-01-02T03:04:05+08:00").ToString("O", null);

                public static string DecimalText()
                    => decimal.Parse("1234.50", null).ToString("N2", CultureInfo.GetCultureInfo("en-US"));

                public static string CultureText()
                {
                    var culture = new CultureInfo("en-US");
                    return culture.Name + "|" + culture.ToString();
                }
            }
            """);

        var projectPath = Path.Combine(hostRoot, "Sample.Host.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(hostRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        if (!File.Exists(manifestPath))
        {
            manifestPath = Directory
                .EnumerateFiles(hostRoot, "jazor-manifest.json", SearchOption.AllDirectories)
                .FirstOrDefault() ?? manifestPath;

            if (File.Exists(manifestPath))
                outputRoot = Path.GetDirectoryName(manifestPath)!;
        }

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");

        var modulePath = Path.Combine(outputRoot, "host", "app.mjs");
        if (!File.Exists(modulePath))
        {
            var emittedManifest = LoadManifest(manifestPath);
            var relativePath = emittedManifest.Modules
                .Select(static module => module.RelativePath)
                .FirstOrDefault(static path => !string.IsNullOrWhiteSpace(path));

            if (!string.IsNullOrWhiteSpace(relativePath))
                modulePath = Path.Combine(outputRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(modulePath))
            {
                modulePath = Directory
                    .EnumerateFiles(outputRoot, "*.mjs", SearchOption.AllDirectories)
                    .FirstOrDefault() ?? modulePath;
            }
        }

        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");

        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");

        Assert.AreEqual(
            "import { _e2640560d207afce } from \"System/DateOnlyModule.js\";",
            GetImportLine(module, "System/DateOnlyModule.js"));
        Assert.AreEqual(
            "import { _25187a24d190d864, _e856edbfd7db0646 } from \"System/DateTimeOffsetModule.js\";",
            GetImportLine(module, "System/DateTimeOffsetModule.js"));
        var decimalImport = GetImportLine(module, "System/DecimalModule.js");
        StringAssert.Contains(decimalImport, "_01be2a34fe2cda4e");
        StringAssert.Contains(decimalImport, "_b1e6a06111674f0c");

        var cultureInfoImport = GetImportLine(module, "System/Globalization/CultureInfoModule.js");
        StringAssert.Contains(cultureInfoImport, "_559b27327f84f1af");
        StringAssert.Contains(cultureInfoImport, "_b7486264ae338f27");
        StringAssert.Contains(cultureInfoImport, "_a536c354b66082b9");

        StringAssert.Contains(module, "return _e2640560d207afce(\"2024-01-02\").toString();");
        StringAssert.Contains(module, "return _e856edbfd7db0646(_25187a24d190d864(\"2024-01-02T03:04:05+08:00\"), \"O\", null);");
        StringAssert.Contains(module, "_01be2a34fe2cda4e(\"1234.50\", null)");
        StringAssert.Contains(module, "_a536c354b66082b9(\"en-US\")");
        StringAssert.Contains(module, "_b1e6a06111674f0c(");
        StringAssert.Contains(module, "let culture = _b7486264ae338f27(\"en-US\");");
        StringAssert.Contains(module, "return culture + \"|\" + _559b27327f84f1af(culture);");

        var manifestModules = LoadManifest(manifestPath).Modules;
        var emittedRelativePaths = manifestModules
            .Select(static module => module.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();
        CollectionAssert.Contains(emittedRelativePaths, "System/DecimalModule.js");
        CollectionAssert.Contains(emittedRelativePaths, "System/Globalization/CultureInfoModule.js");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_StoredIndexAndRange_ExecutesMaterializedRuntimeOnDenoHost()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var wwwroot = Path.Combine(hostRoot, "wwwroot");
        if (Directory.Exists(wwwroot))
            Directory.Delete(wwwroot, recursive: true);

        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using ECMAScript;
            using Index = System.Index;
            using Range = System.Range;

            namespace Sample.Host;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public sealed class SliceBuffer
                {
                    private int[] values = [3, 5, 7, 11, 13, 17];
                    private int sliceCalls;
                    private int reads;
                    private int writes;

                    public int Length => values.Length;
                    public int SliceCalls => sliceCalls;
                    public int ReadCount => reads;
                    public int WriteCount => writes;
                    public int LastValue => values[values.Length - 1];

                    public int this[int index]
                    {
                        get
                        {
                            reads++;
                            return values[index];
                        }
                        set
                        {
                            writes++;
                            values[index] = value;
                        }
                    }

                    public int[] Slice(int start, int length)
                    {
                        sliceCalls++;
                        var result = new int[length];
                        for (var index = 0; index < length; index++)
                            result[index] = values[start + index];
                        return result;
                    }
                }

                public static string Boot() => "index-range-ready";

                public static int[] SliceInterior(int[] values)
                {
                    Index start = ^4;
                    Range range = start..^1;
                    return values[range];
                }

                public static int IncreaseLast(int[] values, int amount)
                {
                    Index last = ^1;
                    values[last] += amount;
                    return values[last];
                }

                public static int[] SliceStoredRangeWithProtocol()
                {
                    var buffer = new SliceBuffer();
                    Range range = ^4..^1;
                    var slice = buffer[range];
                    return [buffer.SliceCalls, slice[0], slice[1], slice[2]];
                }

                public static int TriggerInvalidStoredRange()
                {
                    var buffer = new SliceBuffer();
                    Range range = ^1..1;
                    return buffer[range].Length;
                }

                public static int[] IncreaseStoredIndexWithProtocol()
                {
                    var buffer = new SliceBuffer();
                    Index last = ^1;
                    buffer[last] += 4;
                    return [buffer.ReadCount, buffer.WriteCount, buffer.LastValue];
                }
            }
            """);

        var projectPath = Path.Combine(hostRoot, "Sample.Host.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={package.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(hostRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");

        var modulePath = Path.Combine(outputRoot, "host", "app.mjs");
        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");
        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");

        var indexImport = GetImportLine(module, "System/IndexModule.js");
        StringAssert.Contains(indexImport, "_ce8b9229a41c8545");
        StringAssert.Contains(indexImport, "_9b817e75f3f8f58f");

        var rangeImport = GetImportLine(module, "System/RangeModule.js");
        StringAssert.Contains(rangeImport, "_fc3dfc5dbaa397eb");
        StringAssert.Contains(rangeImport, "_1c7a1e658ed790ff");

        var emittedRelativePaths = LoadManifest(manifestPath).Modules
            .Select(static module => module.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();
        CollectionAssert.Contains(emittedRelativePaths, "System/IndexModule.js");
        CollectionAssert.Contains(emittedRelativePaths, "System/RangeModule.js");
        CollectionAssert.Contains(emittedRelativePaths, "System/RuntimeModule.js");

        // Generated runtime imports use the deployment-time System/ module namespace. Resolve it
        // through Deno's standard import map so the test executes the artifact without rewriting it.
        WriteFile(
            Path.Combine(outputRoot, "deno.json"),
            """
            {
              "imports": {
                "System/": "./System/"
              }
            }
            """);
        var testFile = Path.Combine(outputRoot, "materialized-index-range.test.mjs");
        WriteFile(
            testFile,
            """
            import {
                SliceInterior,
                IncreaseLast,
                SliceStoredRangeWithProtocol,
                TriggerInvalidStoredRange,
                IncreaseStoredIndexWithProtocol
            } from "./host/app.mjs";

            function assertEqual(actual, expected, message) {
                if (!Object.is(actual, expected))
                    throw new Error(`${message}: expected ${String(expected)}, got ${String(actual)}`);
            }

            function assertArrayEqual(actual, expected, message) {
                assertEqual(actual.length, expected.length, `${message} length`);
                for (let index = 0; index < expected.length; index++)
                    assertEqual(actual[index], expected[index], `${message} at ${index}`);
            }

            Deno.test("materialized stored Index and Range preserve array offsets and mutation", () => {
                const source = [3, 5, 7, 11, 13, 17];
                assertArrayEqual(SliceInterior(source), [7, 11, 13], "stored range slice");
                assertArrayEqual(source, [3, 5, 7, 11, 13, 17], "slice source remains unchanged");

                const mutable = [2, 4, 6];
                assertEqual(IncreaseLast(mutable, 5), 11, "stored index return value");
                assertArrayEqual(mutable, [2, 4, 11], "stored index compound assignment");

                assertArrayEqual(
                    SliceStoredRangeWithProtocol(),
                    [1, 7, 11, 13],
                    "stored range custom Slice protocol and single invocation");

                assertArrayEqual(
                    IncreaseStoredIndexWithProtocol(),
                    [1, 1, 21],
                    "stored Index custom indexer compound assignment");

                let invalidRangeError = null;
                try {
                    TriggerInvalidStoredRange();
                } catch (error) {
                    invalidRangeError = error;
                }

                if (!(invalidRangeError instanceof Error) ||
                    invalidRangeError.message !== "ArgumentOutOfRangeException: Range is outside the bounds of the collection.") {
                    throw new Error(`expected the materialized Range carrier bounds error, got ${String(invalidRangeError)}`);
                }
            });
            """);

        await RunDenoTestAsync(package.DenoExePath, testFile, outputRoot);
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_QuerySyntaxWithCapturedLambda_ExecutesOnDenoHost()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var wwwroot = Path.Combine(hostRoot, "wwwroot");
        if (Directory.Exists(wwwroot))
            Directory.Delete(wwwroot, recursive: true);

        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using System.Linq;
            using ECMAScript;

            namespace Sample.Host;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "query-ready";

                public static int[] SelectEvenProducts(int[] values, int factor)
                {
                    return (from value in values
                            where value % 2 == 0
                            select value * factor).ToArray();
                }

                public static int[] SortByParity(int[] values)
                {
                    return (from value in values
                            orderby value % 2
                            select value).ToArray();
                }

                public static int[] SortByParityDescending(int[] values)
                {
                    return (from value in values
                            orderby value % 2 descending
                            select value).ToArray();
                }

                public static int[] SortByParityThenDescendingValue(int[] values)
                {
                    return (from value in values
                            orderby value % 2, value descending
                            select value).ToArray();
                }

                public static int CountOrderKeyInvocations(int[] values)
                {
                    var invocations = 0;
                    var ordered = values.OrderBy(value =>
                    {
                        invocations++;
                        return value % 2;
                    }).ToArray();
                    return invocations + ordered.Length * 0;
                }

                public static int[] PageAfterThreshold(int[] values, int threshold, int skip, int take)
                {
                    return values.Where(value => value > threshold).Skip(skip).Take(take).ToArray();
                }

                public static int CountAnyChecksUntilMatch(int[] values)
                {
                    var checks = 0;
                    var found = values.Any(value =>
                    {
                        checks++;
                        return value % 2 == 0;
                    });
                    return found ? checks : -checks;
                }

                public static int CountAllChecksUntilFailure(int[] values)
                {
                    var checks = 0;
                    var allPositive = values.All(value =>
                    {
                        checks++;
                        return value > 0;
                    });
                    return allPositive ? checks : -checks;
                }

                public static int[] JoinAllowedReleaseIds(int[] releaseIds, int[] allowedIds)
                {
                    return (from releaseId in releaseIds
                            join allowedId in allowedIds on releaseId equals allowedId
                            select releaseId).ToArray();
                }

                public static int[] GroupJoinAllowedReleaseCounts(int[] releaseIds, int[] allowedIds)
                {
                    return (from releaseId in releaseIds
                            join allowedId in allowedIds on releaseId equals allowedId into matches
                            select releaseId * 10 + matches.Count()).ToArray();
                }

                public static int[] ExpandPositiveValues(int[] values, int threshold, int offset)
                {
                    return (from outer in values
                            where outer > threshold
                            from inner in new[] { outer + offset, outer * 10 + offset }
                            select outer + inner).ToArray();
                }

                public static int[] ReverseVisibleReleaseIds(int[] releaseIds)
                {
                    return releaseIds.Reverse().ToArray();
                }

                public static bool HasMatchingReleaseSequence(int[] expectedReleaseIds, int[] actualReleaseIds)
                {
                    return expectedReleaseIds.SequenceEqual(actualReleaseIds);
                }

                public static int[] ConcatenateReleaseIds(int[] firstReleaseIds, int[] secondReleaseIds)
                {
                    return firstReleaseIds.Concat(secondReleaseIds).ToArray();
                }

                public static int[] FrameReleaseIds(int[] releaseIds, int firstReleaseId, int lastReleaseId)
                {
                    return releaseIds.Prepend(firstReleaseId).Append(lastReleaseId).ToArray();
                }

                public static int SelectReleaseAt(int[] releaseIds, int index)
                {
                    return releaseIds.ElementAt(index);
                }

                public static int[] DistinctReleaseIdsByParity(int[] releaseIds)
                {
                    return releaseIds.DistinctBy(releaseId => releaseId % 2).ToArray();
                }

                public static int[] OrderReleaseIds(int[] releaseIds)
                {
                    return releaseIds.Order().ToArray();
                }

                public static int[] OrderReleaseIdsDescending(int[] releaseIds)
                {
                    return releaseIds.OrderDescending().ToArray();
                }

                public static int FindReleaseWithMinimumLastDigit(int[] releaseIds)
                {
                    return releaseIds.MinBy(releaseId => releaseId % 10);
                }

                public static int FindReleaseWithMaximumLastDigit(int[] releaseIds)
                {
                    return releaseIds.MaxBy(releaseId => releaseId % 10);
                }

                public static int[][] ChunkReleaseIds(int[] releaseIds, int size)
                {
                    return releaseIds.Chunk(size).ToArray();
                }

                public static int TerminalReleaseScore(int[] releaseIds, int threshold)
                {
                    return releaseIds.First() +
                        releaseIds.First(releaseId => releaseId > threshold) +
                        releaseIds.Last() +
                        releaseIds.Last(releaseId => releaseId > threshold);
                }

                public static int TerminalSingleScore(int[] onlyReleaseId, int[] releaseIds, int threshold)
                {
                    return onlyReleaseId.Single() + releaseIds.Single(releaseId => releaseId > threshold);
                }

                public static int AggregateReleaseScore(int[] releaseIds, int seed)
                {
                    return releaseIds.Aggregate((total, releaseId) => total + releaseId) +
                        releaseIds.Aggregate(seed, (total, releaseId) => total + releaseId) +
                        releaseIds.Aggregate(seed, (total, releaseId) => total + releaseId, total => total * 2);
                }
            }
            """);

        var projectPath = Path.Combine(hostRoot, "Sample.Host.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={package.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(hostRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var modulePath = Path.Combine(outputRoot, "host", "app.mjs");
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");

        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(module, "return __src.filter(__callback);");
        StringAssert.Contains(module, "return Array.from(__src).map(__callback);");
        StringAssert.Contains(module, "System/Linq/EnumerableModule.js");
        StringAssert.Contains(module, "sequenceEqual");
        StringAssert.Contains(module, "concat");
        StringAssert.Contains(module, "append");
        StringAssert.Contains(module, "prepend");
        StringAssert.Contains(module, "elementAt");
        StringAssert.Contains(module, "distinctBy");
        StringAssert.Contains(module, "order");
        StringAssert.Contains(module, "orderDescending");
        StringAssert.Contains(module, "minBy");
        StringAssert.Contains(module, "maxBy");
        StringAssert.Contains(module, "chunk");

        var emittedRelativePaths = LoadManifest(manifestPath).Modules
            .Select(static module => module.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();
        CollectionAssert.Contains(emittedRelativePaths, "System/Linq/EnumerableModule.js");
        CollectionAssert.Contains(emittedRelativePaths, "System/Collections/Generic/ComparerT1Module.js");
        CollectionAssert.Contains(emittedRelativePaths, "System/Collections/Generic/EqualityComparerT1Module.js");

        // OrderBy introduces CLR runtime imports. Keep the deployment module namespace resolvable
        // under Deno without rewriting generated source text.
        WriteFile(
            Path.Combine(outputRoot, "deno.json"),
            """
            {
              "imports": {
                "System/": "./System/"
              }
            }
            """);

        var testFile = Path.Combine(outputRoot, "materialized-query.test.mjs");
        WriteFile(
            testFile,
            """
            import { AggregateReleaseScore, ChunkReleaseIds, ConcatenateReleaseIds, CountAllChecksUntilFailure, CountAnyChecksUntilMatch, CountOrderKeyInvocations, DistinctReleaseIdsByParity, ExpandPositiveValues, FindReleaseWithMaximumLastDigit, FindReleaseWithMinimumLastDigit, FrameReleaseIds, GroupJoinAllowedReleaseCounts, HasMatchingReleaseSequence, JoinAllowedReleaseIds, OrderReleaseIds, OrderReleaseIdsDescending, PageAfterThreshold, ReverseVisibleReleaseIds, SelectEvenProducts, SelectReleaseAt, SortByParity, SortByParityDescending, SortByParityThenDescendingValue, TerminalReleaseScore, TerminalSingleScore } from "./host/app.mjs";

            function assertEqual(actual, expected, message) {
                if (!Object.is(actual, expected))
                    throw new Error(`${message}: expected ${String(expected)}, got ${String(actual)}`);
            }

            function assertArrayEqual(actual, expected, message) {
                assertEqual(actual.length, expected.length, `${message} length`);
                for (let index = 0; index < expected.length; index++)
                    assertEqual(actual[index], expected[index], `${message} at ${index}`);
            }

            Deno.test("materialized query syntax preserves lambda capture and source values", () => {
                const source = [1, 2, 3, 4, 5];
                assertArrayEqual(SelectEvenProducts(source, 7), [14, 28], "query result");
                assertArrayEqual(source, [1, 2, 3, 4, 5], "query source remains unchanged");

                const sortable = [2, 3, 4, 1];
                assertArrayEqual(SortByParity(sortable), [2, 4, 3, 1], "ascending stable query order");
                assertArrayEqual(SortByParityDescending(sortable), [3, 1, 2, 4], "descending stable query order");
                assertArrayEqual(sortable, [2, 3, 4, 1], "order query source remains unchanged");

                const chained = [2, 1, 4, 3];
                assertArrayEqual(SortByParityThenDescendingValue(chained), [4, 2, 3, 1], "primary and secondary query order");
                assertArrayEqual(chained, [2, 1, 4, 3], "then-by query source remains unchanged");

                assertEqual(CountOrderKeyInvocations([2, 3, 4, 1]), 4, "order key selector invocation count");

                const pageSource = [1, 2, 3, 4, 5, 6];
                assertArrayEqual(PageAfterThreshold(pageSource, 2, 1, 2), [4, 5], "filtered page");
                assertArrayEqual(PageAfterThreshold(pageSource, 2, -1, 0), [], "empty page");
                assertArrayEqual(pageSource, [1, 2, 3, 4, 5, 6], "page source remains unchanged");

                assertEqual(CountAnyChecksUntilMatch([1, 3, 4, 6]), 3, "any predicate short-circuits at match");
                assertEqual(CountAllChecksUntilFailure([4, 2, -1, 8]), -3, "all predicate short-circuits at failure");

                const releaseIds = [7, 2, 7, 3];
                const allowedIds = [2, 7, 7];
                assertArrayEqual(
                    JoinAllowedReleaseIds(releaseIds, allowedIds),
                    [7, 7, 2, 7, 7],
                    "join preserves outer order and duplicate inner match order");
                assertArrayEqual(
                    GroupJoinAllowedReleaseCounts(releaseIds, allowedIds),
                    [72, 21, 72, 30],
                    "group join preserves unmatched outer values as empty groups");
                assertArrayEqual(releaseIds, [7, 2, 7, 3], "join outer source remains unchanged");
                assertArrayEqual(allowedIds, [2, 7, 7], "join inner source remains unchanged");

                const expansionSource = [2, -1, 3];
                assertArrayEqual(
                    ExpandPositiveValues(expansionSource, 0, 5),
                    [9, 27, 11, 38],
                    "multiple from clauses preserve capture and outer/inner expansion order");
                assertArrayEqual(expansionSource, [2, -1, 3], "multiple from source remains unchanged");

                const reverseSource = [2, 7, 2, 9];
                assertArrayEqual(
                    ReverseVisibleReleaseIds(reverseSource),
                    [9, 2, 7, 2],
                    "reverse materializes descending source order");
                assertArrayEqual(reverseSource, [2, 7, 2, 9], "reverse source remains unchanged");

                const expectedSequence = [Number.NaN, -0, 7];
                const actualSequence = [Number.NaN, 0, 7];
                assertEqual(
                    HasMatchingReleaseSequence(expectedSequence, actualSequence),
                    true,
                    "sequence equality uses the CLR default equality contract");
                assertEqual(
                    HasMatchingReleaseSequence(expectedSequence, [Number.NaN, 0, 8]),
                    false,
                    "sequence equality rejects the first unequal release");
                assertArrayEqual(expectedSequence, [Number.NaN, -0, 7], "sequence equality expected input remains unchanged");
                assertArrayEqual(actualSequence, [Number.NaN, 0, 7], "sequence equality actual input remains unchanged");

                const firstReleaseIds = [2, 7];
                const secondReleaseIds = [3, 9];
                assertArrayEqual(
                    ConcatenateReleaseIds(firstReleaseIds, secondReleaseIds),
                    [2, 7, 3, 9],
                    "concat preserves first then second source order");
                assertArrayEqual(firstReleaseIds, [2, 7], "concat first input remains unchanged");
                assertArrayEqual(secondReleaseIds, [3, 9], "concat second input remains unchanged");

                const frameSource = [2, 7];
                assertArrayEqual(
                    FrameReleaseIds(frameSource, 1, 9),
                    [1, 2, 7, 9],
                    "prepend and append frame source in bound order");
                assertArrayEqual(frameSource, [2, 7], "prepend and append input remains unchanged");

                const elementAtSource = [2, 7, 9];
                assertEqual(SelectReleaseAt(elementAtSource, 1), 7, "element at bound index");
                assertArrayEqual(elementAtSource, [2, 7, 9], "element at input remains unchanged");

                const distinctBySource = [2, 7, 4, 9, 3];
                assertArrayEqual(
                    DistinctReleaseIdsByParity(distinctBySource),
                    [2, 7],
                    "distinct by preserves the first release for each bound key");
                assertArrayEqual(distinctBySource, [2, 7, 4, 9, 3], "distinct by input remains unchanged");

                const orderSource = [2, 7, 4, 1];
                assertArrayEqual(OrderReleaseIds(orderSource), [1, 2, 4, 7], "order uses the bound default comparer");
                assertArrayEqual(OrderReleaseIdsDescending(orderSource), [7, 4, 2, 1], "order descending uses the bound default comparer");
                assertArrayEqual(orderSource, [2, 7, 4, 1], "order input remains unchanged");

                const extremumSource = [22, 15, 35, 12];
                assertEqual(FindReleaseWithMinimumLastDigit(extremumSource), 22, "min by preserves the first tied key");
                assertEqual(FindReleaseWithMaximumLastDigit(extremumSource), 15, "max by preserves the first tied key");
                assertArrayEqual(extremumSource, [22, 15, 35, 12], "min and max by input remains unchanged");

                const chunkSource = [2, 7, 3, 9, 4];
                assertEqual(
                    ChunkReleaseIds(chunkSource, 2).map(chunk => chunk.join(",")).join("|"),
                    "2,7|3,9|4",
                    "chunk preserves source order and final partial chunk");
                assertArrayEqual(chunkSource, [2, 7, 3, 9, 4], "chunk input remains unchanged");

                const terminalSource = [2, 7, 3, 9];
                assertEqual(
                    TerminalReleaseScore(terminalSource, 3),
                    27,
                    "terminal query operators preserve bound first and last values");
                assertArrayEqual(terminalSource, [2, 7, 3, 9], "terminal query source remains unchanged");

                const singleSource = [2, 7, 3];
                assertEqual(
                    TerminalSingleScore([7], singleSource, 3),
                    14,
                    "single query operators preserve the unique bound values");
                assertArrayEqual(singleSource, [2, 7, 3], "single query source remains unchanged");

                const aggregateSource = [2, 3];
                assertEqual(
                    AggregateReleaseScore(aggregateSource, 10),
                    50,
                    "aggregate query operators preserve accumulator and result selector values");
                assertArrayEqual(aggregateSource, [2, 3], "aggregate query source remains unchanged");
            });
            """);

        await RunDenoTestAsync(package.DenoExePath, testFile, outputRoot);
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_StaticHost_UsesWwwrootJazorByDefault()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "StaticHostDefaultBuildSample");
        var restorePackagesPath = package.RestorePackagesPath;
        var projectPath = CreateDefaultOutputStaticHostProject(projectRoot);

        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var projectRootJazor = Path.Combine(projectRoot, "jazor");
        var wwwrootJazor = Path.Combine(projectRoot, "wwwroot", "jazor");
        Assert.IsTrue(File.Exists(Path.Combine(wwwrootJazor, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(wwwrootJazor, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(wwwrootJazor, "host", "app.mjs.map")));
        var module = await File.ReadAllTextAsync(Path.Combine(wwwrootJazor, "host", "app.mjs"));
        StringAssert.Contains(module, "sourceMappingURL=app.mjs.map");
        Assert.IsFalse(Directory.Exists(projectRootJazor), $"Build must not materialize assets under '{projectRootJazor}'.");
    }

    [TestMethod]
    public async Task Publish_LocalJazorPackage_StaticHost_UsesWwwrootJazorByDefault()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "StaticHostDefaultPublishSample");
        var restorePackagesPath = package.RestorePackagesPath;
        var projectPath = CreateDefaultOutputStaticHostProject(projectRoot);

        var publish = await RunDotNetAsync(
            package.RepoRoot,
            [
                "publish",
                projectPath,
                "-c",
                "Debug",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, publish.ExitCode, publish.ToString());

        var devJazorRoot = Path.Combine(projectRoot, "jazor");
        var publishJazorRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        Assert.IsTrue(File.Exists(Path.Combine(publishJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishJazorRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(publishJazorRoot, "host", "app.mjs.map")));
        var publishedStaticHostModule = await File.ReadAllTextAsync(Path.Combine(publishJazorRoot, "host", "app.mjs"));
        StringAssert.Contains(publishedStaticHostModule, "sourceMappingURL=app.mjs.map");
        Assert.IsFalse(Directory.Exists(devJazorRoot), $"Publish should not fall back to the development output root '{devJazorRoot}'.");
    }

    [TestMethod]
    public async Task Publish_LocalJazorPackage_WebSdkHost_MaterializesJazorAssetsIntoPublishOutput()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "WebSdkPublishSample");
        var publishOutputRoot = Path.Combine(workspace.RootPath, "publish-output");
        var restorePackagesPath = package.RestorePackagesPath;
        var projectPath = CreateDefaultOutputWebHostProject(projectRoot);

        var publish = await RunDotNetAsync(
            package.RepoRoot,
            [
                "publish",
                projectPath,
                "-c",
                "Debug",
                "-o",
                publishOutputRoot,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, publish.ExitCode, publish.ToString());

        var sourcePublishJazorRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var publishedJazorRoot = Path.Combine(publishOutputRoot, "wwwroot", "jazor");
        var publishedShadowJazorRoot = Path.Combine(publishOutputRoot, "jazor");

        Assert.IsTrue(File.Exists(Path.Combine(sourcePublishJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(sourcePublishJazorRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(sourcePublishJazorRoot, "host", "app.mjs.map")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "host", "app.mjs.map")));
        var publishedWebHostModule = await File.ReadAllTextAsync(Path.Combine(publishedJazorRoot, "host", "app.mjs"));
        StringAssert.Contains(publishedWebHostModule, "sourceMappingURL=app.mjs.map");
        Assert.IsFalse(
            Directory.Exists(publishedShadowJazorRoot),
            $"Publish output must not leak a shadow root jazor directory at '{publishedShadowJazorRoot}'.");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_WithVueRouteAuthoring_EmitsVueRouterImportsAndRouteObjects()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "VueRouteSdkSample");
        var restorePackagesPath = package.RestorePackagesPath;

        WriteFile(
            Path.Combine(projectRoot, "VueRouteSdkSample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorMode>debug</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="ECMAScript.VueRoute" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            Console.WriteLine("VueRoute SDK sample");
            """);
        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;
            using static ECMAScript.VueRoute;

            namespace VueRouteSdkSample;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static Router CreateAppRouter()
                {
                    return VueRoute.CreateRouter(new RouterOptions
                    {
                        History = VueRoute.CreateWebHistory(),
                        Routes =
                        [
                            new RouteRecordRedirect
                            {
                                Path = "/",
                                Redirect = "/home"
                            },
                            new RouteRecordSingleView
                            {
                                Path = "/users",
                                Props = true
                            }
                        ]
                    });
                }

                public static string CurrentPath()
                {
                    return VueRoute.UseRouter().CurrentRoute.Value.Path;
                }
            }
            """);

        var projectPath = Path.Combine(projectRoot, "VueRouteSdkSample.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var modulePath = Path.Combine(outputRoot, "host", "app.mjs");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");

        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");

        Assert.AreEqual(
            "import { createRouter, createWebHistory, useRouter } from \"vue-router\";",
            GetImportLine(module, "vue-router"));
        StringAssert.Contains(module, "export function CreateAppRouter()");
        StringAssert.Contains(module, "history: createWebHistory()");
        StringAssert.Contains(module, "redirect: \"/home\"");
        StringAssert.Contains(module, "path: \"/users\"");
        StringAssert.Contains(module, "props: true");
        StringAssert.Contains(module, "return createRouter(");
        StringAssert.Contains(module, "export function CurrentPath()");
        StringAssert.Contains(module, "return useRouter().currentRoute.value.path;");

        var emittedRelativePaths = LoadManifest(manifestPath).Modules
            .Select(static moduleEntry => moduleEntry.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        CollectionAssert.Contains(emittedRelativePaths, "host/app.mjs");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_WithVueRouteInjectionAndReactiveAuthoring_EmitsTypedVueRouterContracts()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "VueRouteReactiveSdkSample");
        var restorePackagesPath = package.RestorePackagesPath;

        WriteFile(
            Path.Combine(projectRoot, "VueRouteReactiveSdkSample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorMode>debug</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="ECMAScript.VueRoute" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            Console.WriteLine("VueRoute reactive SDK sample");
            """);
        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using static ECMAScript.VueRoute;

            namespace VueRouteReactiveSdkSample;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Build(Router router, RouteLocation location, RouteLocationNormalized normalized)
                {
                    var routeRef = ShallowRef(UseRoute());
                    VueComputedRef<RouteRecordNormalized?> matched = Computed(() => (RouteRecordNormalized?)normalized.Matched[0]);

                    Provide(VueRoute.RouterKey, router);
                    Provide(VueRoute.RouteLocationKey, UseRoute());
                    Provide(VueRoute.RouterViewLocationKey, routeRef);
                    Provide(VueRoute.MatchedRouteKey, matched);
                    Provide(VueRoute.ViewDepthKey, 2);

                    var injectedRouter = Inject(VueRoute.RouterKey)!;
                    var injectedRoute = Inject(VueRoute.RouteLocationKey)!;
                    var injectedRouteRef = Inject(VueRoute.RouterViewLocationKey)!;
                    var injectedMatched = Inject(VueRoute.MatchedRouteKey)!;
                    var injectedDepth = Inject(VueRoute.ViewDepthKey)!;
                    var loadedFromLocation = LoadRouteLocation(location);
                    var loadedFromNormalized = LoadRouteLocation(normalized);
                    var link = UseLink(new UseLinkOptions
                    {
                        To = ToRef(() => new RouteLocationAsRelative
                        {
                            Name = injectedRoute.Name!
                        }),
                        Replace = Computed(() => true)
                    });

                    TriggerRef(routeRef);
                    return injectedRouter.CurrentRoute.Value.Path
                        + injectedRoute.Path
                        + injectedRouteRef.Value.Path
                        + injectedMatched.Value!.Path
                        + injectedDepth.AsNumber!.ToString()
                        + link.Href.Value
                        + link.Route.Value.Href
                        + loadedFromLocation.ToString()
                        + loadedFromNormalized.ToString();
                }
            }
            """);

        var projectPath = Path.Combine(projectRoot, "VueRouteReactiveSdkSample.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var modulePath = Path.Combine(outputRoot, "host", "app.mjs");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");

        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");

        Assert.AreEqual(
            "import { computed, inject, provide, shallowRef, toRef, triggerRef } from \"vue\";",
            GetImportLine(module, "vue"));
        var vueRouterImport = GetImportLine(module, "vue-router");
        StringAssert.Contains(vueRouterImport, "loadRouteLocation");
        StringAssert.Contains(vueRouterImport, "matchedRouteKey");
        StringAssert.Contains(vueRouterImport, "routeLocationKey");
        StringAssert.Contains(vueRouterImport, "routerKey");
        StringAssert.Contains(vueRouterImport, "routerViewLocationKey");
        StringAssert.Contains(vueRouterImport, "useLink");
        StringAssert.Contains(vueRouterImport, "useRoute");
        StringAssert.Contains(vueRouterImport, "viewDepthKey");
        StringAssert.Contains(module, "let routeRef = shallowRef(useRoute());");
        StringAssert.Contains(module, "let matched = computed(() => {");
        StringAssert.Contains(module, "provide(routerKey, router);");
        StringAssert.Contains(module, "provide(routeLocationKey, useRoute());");
        StringAssert.Contains(module, "provide(routerViewLocationKey, routeRef);");
        StringAssert.Contains(module, "provide(matchedRouteKey, matched);");
        StringAssert.Contains(module, "provide(viewDepthKey, 2);");
        StringAssert.Contains(module, "let injectedRouter = inject(routerKey);");
        StringAssert.Contains(module, "let injectedRoute = inject(routeLocationKey);");
        StringAssert.Contains(module, "let injectedRouteRef = inject(routerViewLocationKey);");
        StringAssert.Contains(module, "let injectedMatched = inject(matchedRouteKey);");
        StringAssert.Contains(module, "let injectedDepth = inject(viewDepthKey);");
        StringAssert.Contains(module, "let loadedFromLocation = loadRouteLocation(location);");
        StringAssert.Contains(module, "let loadedFromNormalized = loadRouteLocation(normalized);");
        StringAssert.Contains(module, "let link = useLink({");
        StringAssert.Contains(module, "triggerRef(routeRef);");
        StringAssert.Contains(module, "injectedRouter.currentRoute.value.path");
        StringAssert.Contains(module, "injectedRoute.path");
        StringAssert.Contains(module, "injectedRouteRef.value.path");
        StringAssert.Contains(module, "injectedMatched.value.path");
        StringAssert.Contains(module, "injectedDepth.toString()");
        StringAssert.Contains(module, "link.href.value");
        StringAssert.Contains(module, "link.route.value.href");

        var emittedRelativePaths = LoadManifest(manifestPath).Modules
            .Select(static moduleEntry => moduleEntry.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        CollectionAssert.Contains(emittedRelativePaths, "host/app.mjs");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_WithVueRouteReactiveAuthoring_BundlesThroughBundledDeno_AndResolvesVuePackages()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "VueRouteReactiveBundleSdkSample");
        var restorePackagesPath = package.RestorePackagesPath;

        WriteFile(
            Path.Combine(projectRoot, "VueRouteReactiveBundleSdkSample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorMode>release</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="ECMAScript.VueRoute" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            Console.WriteLine("VueRoute reactive bundle SDK sample");
            """);
        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using static ECMAScript.VueRoute;

            namespace VueRouteReactiveBundleSdkSample;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static Router CreateAppRouter()
                {
                    return VueRoute.CreateRouter(new RouterOptions
                    {
                        History = VueRoute.CreateWebHistory("/bundle-base"),
                        Routes =
                        [
                            new RouteRecordRedirect
                            {
                                Path = "/bundle-home",
                                Redirect = "/bundle-users"
                            },
                            new RouteRecordSingleView
                            {
                                Path = "/bundle-users",
                                Name = "bundle-user",
                                Props = true
                            }
                        ]
                    });
                }

                public static string Build(Router router, RouteLocation location, RouteLocationNormalized normalized)
                {
                    var routeRef = ShallowRef(UseRoute());
                    VueComputedRef<RouteRecordNormalized?> matched = Computed(() => (RouteRecordNormalized?)normalized.Matched[0]);

                    Provide(VueRoute.RouterKey, router);
                    Provide(VueRoute.RouteLocationKey, UseRoute());
                    Provide(VueRoute.RouterViewLocationKey, routeRef);
                    Provide(VueRoute.MatchedRouteKey, matched);
                    Provide(VueRoute.ViewDepthKey, 3);

                    var injectedRouter = Inject(VueRoute.RouterKey)!;
                    var injectedRoute = Inject(VueRoute.RouteLocationKey)!;
                    var injectedRouteRef = Inject(VueRoute.RouterViewLocationKey)!;
                    var injectedMatched = Inject(VueRoute.MatchedRouteKey)!;
                    var injectedDepth = Inject(VueRoute.ViewDepthKey)!;
                    var loadedFromLocation = LoadRouteLocation(location);
                    var loadedFromNormalized = LoadRouteLocation(normalized);
                    var link = UseLink(new UseLinkOptions
                    {
                        To = ToRef(() => new RouteLocationAsRelative
                        {
                            Name = injectedRoute.Name!
                        }),
                        Replace = Computed(() => true)
                    });

                    TriggerRef(routeRef);
                    return injectedRouter.CurrentRoute.Value.Path
                        + injectedRoute.Path
                        + injectedRouteRef.Value.Path
                        + injectedMatched.Value!.Path
                        + injectedDepth.AsNumber!.ToString()
                        + link.Href.Value
                        + link.Route.Value.Href
                        + loadedFromLocation.ToString()
                        + loadedFromNormalized.ToString();
                }
            }
            """);

        var projectPath = Path.Combine(projectRoot, "VueRouteReactiveBundleSdkSample.csproj");
        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var moduleRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(moduleRoot, "jazor-manifest.json");
        var modulePath = Path.Combine(moduleRoot, "host", "app.mjs");
        var bundlePath = Path.Combine(moduleRoot, "bundle.js");
        var bundleSourceMapPath = Path.Combine(moduleRoot, "bundle.js.map");

        Assert.IsFalse(File.Exists(manifestPath), $"Release must not materialize a manifest: {manifestPath}");
        Assert.IsFalse(File.Exists(modulePath), $"Release must not materialize modules: {modulePath}");
        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(bundleSourceMapPath), $"Bundle source map was not generated: {bundleSourceMapPath}");

        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        var bundleSourceMap = (await File.ReadAllTextAsync(bundleSourceMapPath)).ReplaceLineEndings("\n");

        StringAssert.Contains(bundle, "/bundle-base");
        StringAssert.Contains(bundle, "/bundle-home");
        StringAssert.Contains(bundle, "/bundle-users");
        StringAssert.Contains(bundle, "bundle-user");
        StringAssert.Contains(bundle, "sourceMappingURL=bundle.js.map");
        Assert.IsFalse(
            bundle.Contains("from \"vue-router\"", StringComparison.Ordinal),
            "Bundle should not keep unresolved vue-router imports.");
        Assert.IsFalse(
            bundle.Contains("from \"vue\"", StringComparison.Ordinal),
            "Bundle should not keep unresolved vue imports.");

        StringAssert.Contains(bundleSourceMap, "\"sources\"");
        Assert.IsTrue(
            bundleSourceMap.Contains("host/app.mjs", StringComparison.Ordinal)
            || bundleSourceMap.Contains("AppModule.cs", StringComparison.Ordinal),
            "Bundle source map should preserve authored module provenance.");
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgG0Consumer_ReconcilesFinalDocumentsAcrossIncrementalBuilds()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgG0Consumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot, enableEmit: true);
        var restorePackagesPath = package.RestorePackagesPath;
        var commonArguments = new[]
        {
            "/m:1",
            "/p:BuildInParallel=false",
            $"-p:RestoreSources={package.PackageOutputDirectory}",
            "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            $"-p:RestorePackagesPath={restorePackagesPath}",
            $"-p:JazorPackageVersion={package.PackageVersion}"
        };

        var restore = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["restore", projectPath, .. commonArguments]);
        Assert.AreEqual(0, restore.ExitCode, restore.ToString());

        var firstBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "--no-restore", .. commonArguments]);
        Assert.AreEqual(0, firstBuild.ExitCode, firstBuild.ToString());

        var generatedRoot = Path.Combine(projectRoot, "obj", "Generated");
        var firstCounterSource = ReadCounterRazorSgGeneratedSource(generatedRoot);
        var counterModulePath = Path.Combine(projectRoot, "wwwroot", "jazor", "components", "counter.mjs");
        var firstCounterModule = await File.ReadAllTextAsync(counterModulePath);
        Assert.IsFalse(
            firstCounterModule.Contains(projectRoot, StringComparison.OrdinalIgnoreCase),
            "The generated Vue module must not contain the external consumer's absolute path.");

        var incrementalBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "--no-restore", .. commonArguments]);
        Assert.AreEqual(0, incrementalBuild.ExitCode, incrementalBuild.ToString());

        var incrementalCounterSource = ReadCounterRazorSgGeneratedSource(generatedRoot);
        var incrementalCounterModule = await File.ReadAllTextAsync(counterModulePath);
        Assert.AreEqual(firstCounterSource, incrementalCounterSource);
        Assert.AreEqual(firstCounterModule, incrementalCounterModule);

        await File.WriteAllTextAsync(
            Path.Combine(projectRoot, "Counter.razor"),
            """
            <button @onclick="Increment">Clicks: @count</button>
            @if (count > 0)
            {
                <span>Counter changed</span>
            }
            """);

        var changedBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "--no-restore", .. commonArguments]);
        Assert.AreEqual(0, changedBuild.ExitCode, changedBuild.ToString());

        var changedCounterSource = ReadCounterRazorSgGeneratedSource(generatedRoot);
        var changedCounterModule = await File.ReadAllTextAsync(counterModulePath);
        Assert.AreNotEqual(firstCounterSource, changedCounterSource);
        Assert.AreNotEqual(firstCounterModule, changedCounterModule);
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgConsumer_EmitsVueRenderArtifactsAndManifest()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgEmitConsumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot, enableEmit: true);
        var restorePackagesPath = package.RestorePackagesPath;
        var build = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var generatedRoot = Path.Combine(projectRoot, "obj", "Generated");
        _ = ReadCounterRazorSgGeneratedSource(generatedRoot);

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var componentModulePath = Path.Combine(outputRoot, "components", "counter.mjs");
        var componentMapPath = Path.Combine(outputRoot, "components", "counter.mjs.map");
        var runtimeModulePath = Path.Combine(outputRoot, "@jazor", "vue-runtime", "render-context.mjs");
        var runtimeCoreModulePath = Path.Combine(outputRoot, "@jazor", "vue-runtime", "render-context-core.mjs");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue component module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(componentMapPath), $"RazorVue component source map was not generated: {componentMapPath}");
        Assert.IsFalse(File.Exists(runtimeModulePath), $"Direct render must not emit an unused runtime bridge: {runtimeModulePath}");
        Assert.IsFalse(File.Exists(runtimeCoreModulePath), $"Direct render must not emit an unused runtime core bridge: {runtimeCoreModulePath}");

        var componentModule = (await File.ReadAllTextAsync(componentModulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(componentModule, "import { defineComponent, h, reactive } from \"vue\";");
        Assert.IsFalse(componentModule.Contains("watch", StringComparison.Ordinal), componentModule);
        Assert.IsFalse(componentModule.Contains("onMounted", StringComparison.Ordinal), componentModule);
        Assert.IsFalse(componentModule.Contains("onUpdated", StringComparison.Ordinal), componentModule);
        Assert.IsFalse(componentModule.Contains("onUnmounted", StringComparison.Ordinal), componentModule);
        Assert.IsFalse(componentModule.Contains("createRenderContext", StringComparison.Ordinal), componentModule);
        StringAssert.Contains(componentModule, "const __jazorComponent = defineComponent({");
        StringAssert.Contains(componentModule, "export default __jazorComponent;");
        StringAssert.Contains(componentModule, "sourceMappingURL=counter.mjs.map");

        var componentMap = await File.ReadAllTextAsync(componentMapPath);
        StringAssert.Contains(componentMap, "\"file\": \"components/counter.mjs\"");
        StringAssert.Contains(componentMap, "Counter.razor");
        var sourceMap = new SourceMapReader().Read(componentMap);
        var razorSource = sourceMap.Sources.Single(static source => source.Path == "Counter.razor");
        Assert.IsNotNull(razorSource.Content);
        StringAssert.Contains(razorSource.Content!, "Clicks: @count", StringComparison.Ordinal);
        Assert.IsFalse(
            componentMap.Contains(projectRoot, StringComparison.OrdinalIgnoreCase),
            "RazorVue component source map must not persist the external consumer's absolute project path.");

        var manifest = LoadManifest(manifestPath);
        Assert.AreEqual(1, manifest.SchemaVersion);
        Assert.AreEqual(1, manifest.RuntimeProtocolVersion);
        Assert.AreEqual("ExternalRazorSgG0Consumer", manifest.RootAssemblyName);

        var emittedRelativePaths = manifest.Modules
            .Select(static moduleEntry => moduleEntry.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();
        CollectionAssert.Contains(emittedRelativePaths, "components/counter.mjs");
        CollectionAssert.DoesNotContain(emittedRelativePaths, "@jazor/vue-runtime/render-context.mjs");
        CollectionAssert.DoesNotContain(emittedRelativePaths, "@jazor/vue-runtime/render-context-core.mjs");

        var counterEntry = manifest.Modules.Single(static moduleEntry => moduleEntry.RelativePath == "components/counter.mjs");
        Assert.AreEqual("components/counter.mjs.map", counterEntry.SourceMapPath);
        StringAssert.StartsWith(counterEntry.Hash, "sha256:");
        StringAssert.StartsWith(counterEntry.MapHash, "sha256:");

        var manifestText = (await File.ReadAllTextAsync(manifestPath)).ReplaceLineEndings("\n");
        Assert.IsFalse(manifestText.Contains("generatedAtUtc", StringComparison.OrdinalIgnoreCase), manifestText);
        Assert.IsFalse(manifestText.Contains("rootAssemblyPath", StringComparison.OrdinalIgnoreCase), manifestText);
        Assert.IsFalse(
            manifestText.Contains(projectRoot, StringComparison.OrdinalIgnoreCase),
            "Canonical manifest must not persist the external consumer's absolute project path.");
    }

    [TestMethod]
    public async Task Build_LocalPackages_RazorAuthoringError_DoesNotAddSecondaryRazorVueDiagnostic()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgInvalidBindingConsumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot);
        WriteFile(
            Path.Combine(projectRoot, "Counter.razor"),
            "<input @bind:get=\"Note\" @bind:set=\"SetNoteAsync\" @bind:after=\"PersistNoteAsync\" />");
        WriteFile(
            Path.Combine(projectRoot, "Counter.razor.cs"),
            """
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgG0Consumer;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private string Note { get; set; } = string.Empty;

                private Task SetNoteAsync(string value)
                {
                    Note = value;
                    return Task.CompletedTask;
                }

                private Task PersistNoteAsync()
                    => Task.CompletedTask;
            }
            """);

        var build = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={package.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreNotEqual(0, build.ExitCode, build.ToString());
        StringAssert.Contains(build.ToString(), "RZ10019", StringComparison.Ordinal);
        Assert.IsFalse(build.ToString().Contains("JAZORVGA020", StringComparison.Ordinal), build.ToString());
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgConsumer_ExecutesMaterializedComponentBindingAndCounterOnDenoHost()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgDenoConsumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot, enableEmit: true);
        WriteFile(
            Path.Combine(projectRoot, "Counter.razor"),
            """
            <input @bind:get="Note" @bind:set="SetNoteAsync" @bind:event="oninput" data-saved="@SavedNote" data-persisted="@PersistedNote" />
            <ReleaseEditor @bind-Value:get="Note" @bind-Value:set="SetNoteAsync" />
            <button @onclick="IncrementAsync">Clicks: @count</button>
            @if (count > 0)
            {
                <span>Counter changed</span>
            }
            """);
        WriteFile(
            Path.Combine(projectRoot, "Counter.razor.cs"),
            """
            using System.Threading.Tasks;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgG0Consumer;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private int count;

                private string Note { get; set; } = "Draft note";

                private string SavedNote { get; set; } = "none";

                private string PersistedNote { get; set; } = "none";

                private void SetNote(string value)
                {
                    Note = value.Trim();
                    SavedNote = "saved:" + Note;
                }

                // Razor forbids combining @bind:set with @bind:after. The setter owns
                // the ordered state update and follow-up task so both bind targets share it.
                private Task SetNoteAsync(string value)
                {
                    SetNote(value);
                    return PersistNoteAsync();
                }

                private Task PersistNoteAsync()
                {
                    PersistedNote = "persisted:" + Note;
                    return Task.CompletedTask;
                }

                private async Task IncrementAsync()
                {
                    await InvokeAsync(IncrementCoreAsync);
                }

                private async Task IncrementCoreAsync()
                {
                    count++;
                    StateHasChanged();
                }
            }
            """);
        WriteFile(
            Path.Combine(projectRoot, "ReleaseEditor.razor"),
            """
            <span>Release editor</span>
            """);
        WriteFile(
            Path.Combine(projectRoot, "ReleaseEditor.razor.cs"),
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgG0Consumer;

            [ECMAScriptModule("./components/release-editor")]
            public partial class ReleaseEditor : ComponentBase, IVueComponent
            {
                [Parameter]
                [ECMAScriptName("modelValue")]
                public string Value { get; set; } = "";

                [Parameter]
                [ECMAScriptName("onUpdate:modelValue")]
                public EventCallback<string> ValueChanged { get; set; }
            }
            """);
        var build = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={package.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var componentModulePath = Path.Combine(outputRoot, "components", "counter.mjs");
        var releaseEditorModulePath = Path.Combine(outputRoot, "components", "release-editor.mjs");
        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue Counter module was not materialized: {componentModulePath}");
        Assert.IsTrue(File.Exists(releaseEditorModulePath), $"RazorVue ReleaseEditor module was not materialized: {releaseEditorModulePath}");
        var componentModule = (await File.ReadAllTextAsync(componentModulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(componentModule, "invokeAsync", StringComparison.Ordinal);
        StringAssert.Contains(componentModule, "stateHasChanged", StringComparison.Ordinal);
        StringAssert.Contains(componentModule, "SetNoteAsync(__value)", StringComparison.Ordinal);
        StringAssert.Contains(componentModule, "from \"./release-editor.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(componentModule, "modelValue: state.Note", StringComparison.Ordinal);
        StringAssert.Contains(componentModule, "onUpdate:modelValue", StringComparison.Ordinal);
        Assert.IsFalse(componentModule.Contains("this.", StringComparison.Ordinal), componentModule);
        var generatedModules = Directory
            .EnumerateFiles(outputRoot, "*.mjs", SearchOption.AllDirectories)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.IsNotEmpty(generatedModules, "External Razor SG consumer did not materialize any ECMAScript modules.");
        foreach (var modulePath in generatedModules)
        {
            var moduleText = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");
            Assert.IsFalse(moduleText.Contains("createRenderContext", StringComparison.Ordinal), modulePath);
            Assert.IsFalse(moduleText.Contains("scope.buildRenderTree(builder)", StringComparison.Ordinal), modulePath);
            Assert.IsFalse(moduleText.Contains("builder.finish()", StringComparison.Ordinal), modulePath);
            Assert.IsFalse(moduleText.Contains(".vue", StringComparison.OrdinalIgnoreCase), modulePath);
        }

        // Resolve the generated bare Vue import through Deno's local import map. The fixture is
        // intentionally adjacent to the artifact so this consumer path never needs node_modules.
        WriteFile(
            Path.Combine(outputRoot, "deno.json"),
            """
            {
              "imports": {
                "vue": "./vue-test-runtime.mjs"
              }
            }
            """);
        WriteFile(
            Path.Combine(outputRoot, "vue-test-runtime.mjs"),
            """
            export const Fragment = Symbol("Fragment");

            export function createStaticVNode(html, count) {
                return { kind: "static", html, count };
            }

            export function defineComponent(options) {
                return options;
            }

            export function reactive(value) {
                return value;
            }

            export function h(name, props, children) {
                return { name, props, children };
            }
            """);

        var testFile = Path.Combine(outputRoot, "materialized-counter.test.mjs");
        WriteFile(
            testFile,
            """
            import component from "./components/counter.mjs";
            import releaseEditor from "./components/release-editor.mjs";

            function assertEqual(actual, expected, message) {
                if (!Object.is(actual, expected))
                    throw new Error(`${message}: expected ${String(expected)}, got ${String(actual)}`);
            }

            function renderedText(vnode) {
                if (vnode == null)
                    return "";
                if (Array.isArray(vnode))
                    return vnode.map(renderedText).join("");
                if (typeof vnode === "object")
                    return renderedText(vnode.children);
                return String(vnode);
            }

            function findElement(vnode, name) {
                if (Array.isArray(vnode))
                    return vnode.map(value => findElement(value, name)).find(Boolean);
                if (vnode != null && typeof vnode === "object")
                    return vnode.name === name
                        ? vnode
                        : findElement(vnode.children, name);
                return undefined;
            }

            function findComponent(vnode, component) {
                if (Array.isArray(vnode))
                    return vnode.map(value => findComponent(value, component)).find(Boolean);
                if (vnode != null && typeof vnode === "object")
                    return vnode.name === component
                        ? vnode
                        : findComponent(vnode.children, component);
                return undefined;
            }

            function hasStaticHtml(vnode, expectedHtml) {
                if (Array.isArray(vnode))
                    return vnode.some(value => hasStaticHtml(value, expectedHtml));
                if (vnode != null && typeof vnode === "object")
                    return vnode.kind === "static"
                        ? vnode.html === expectedHtml
                        : hasStaticHtml(vnode.children, expectedHtml);
                return false;
            }

            Deno.test("materialized RazorVue consumer preserves bind:set and async event state", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const initialInput = findElement(initial, "input");
                const initialEditor = findComponent(initial, releaseEditor);
                const initialButton = findElement(initial, "button");
                assertEqual(initialInput?.name, "input", "initial input element");
                assertEqual(initialEditor?.name, releaseEditor, "initial release editor component");
                assertEqual(initialButton?.name, "button", "initial vnode element");
                assertEqual(initialInput?.props.value, "Draft note", "initial bound input value");
                assertEqual(initialInput?.props["data-saved"], "none", "initial saved note");
                assertEqual(typeof initialInput?.props.onInput, "function", "input handler");
                assertEqual(initialEditor?.props.modelValue, "Draft note", "initial component model value");
                assertEqual(typeof initialEditor?.props["onUpdate:modelValue"], "function", "component model update handler");
                assertEqual(renderedText(initial), "Clicks: 0", "initial rendered text");
                assertEqual(hasStaticHtml(initial, "<span>Counter changed</span>"), false, "initial conditional content");
                assertEqual(typeof initialButton?.props.onClick, "function", "click handler");

                await Promise.resolve(initialInput.props.onInput({ target: { value: "  package consumer  " } }));

                const bound = render();
                const boundInput = findElement(bound, "input");
                const boundEditor = findComponent(bound, releaseEditor);
                assertEqual(boundInput?.props.value, "package consumer", "bound input value");
                assertEqual(boundInput?.props["data-saved"], "saved:package consumer", "bound saved note");
                assertEqual(boundInput?.props["data-persisted"], "persisted:package consumer", "DOM async bind setter persists the normalized value");
                assertEqual(boundEditor?.props.modelValue, "package consumer", "bound component model value");

                await Promise.resolve(boundEditor.props["onUpdate:modelValue"]("  component update  "));

                const componentBound = render();
                const componentBoundInput = findElement(componentBound, "input");
                const componentBoundEditor = findComponent(componentBound, releaseEditor);
                assertEqual(componentBoundInput?.props.value, "component update", "component-updated input value");
                assertEqual(componentBoundInput?.props["data-saved"], "saved:component update", "component update uses the explicit setter");
                assertEqual(componentBoundInput?.props["data-persisted"], "persisted:component update", "component async bind setter persists the normalized value");
                assertEqual(componentBoundEditor?.props.modelValue, "component update", "component-updated model value");

                const boundButton = findElement(componentBound, "button");
                await Promise.resolve(boundButton.props.onClick());

                const updated = render();
                assertEqual(renderedText(updated), "Clicks: 1", "updated rendered text");
                assertEqual(hasStaticHtml(updated, "<span>Counter changed</span>"), true, "updated conditional content");
            });
            """);

        await RunDenoTestAsync(package.DenoExePath, testFile, outputRoot);
        Assert.IsFalse(
            Directory.Exists(Path.Combine(outputRoot, "node_modules")),
            "The materialized RazorVue consumer test must resolve Vue without frontend node_modules.");
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgConsumer_BundlesThroughNetpackToolchain()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgNetpackBundleConsumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot, enableEmit: true);

        var restorePackagesPath = package.RestorePackagesPath;
        var bundleRoot = Path.Combine(projectRoot, "wwwroot", "netpack");
        var build = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=release",
                "-p:JazorTool=Netpack",
                $"-p:JazorDir={bundleRoot}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var counterModulePath = Path.Combine(outputRoot, "components", "counter.mjs");
        var bundlePath = Path.Combine(bundleRoot, "bundle.js");
        var bundleMapPath = Path.Combine(bundleRoot, "bundle.js.map");

        Assert.IsFalse(File.Exists(manifestPath), $"Release must not materialize a manifest: {manifestPath}");
        Assert.IsFalse(File.Exists(counterModulePath), $"Release must not materialize modules: {counterModulePath}");
        Assert.IsTrue(File.Exists(bundlePath), $"Netpack bundle was not generated by package consumer: {bundlePath}");
        Assert.IsTrue(File.Exists(bundleMapPath), $"Netpack bundle source map was not generated: {bundleMapPath}");

        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(bundle, "Clicks:");
        Assert.IsFalse(bundle.Contains("createRenderContext", StringComparison.Ordinal), bundle);
        StringAssert.Contains(bundle, "sourceMappingURL=bundle.js.map");
        Assert.IsFalse(
            bundle.Contains("deno", StringComparison.OrdinalIgnoreCase),
            "Netpack package consumer bundle should not show Deno fallback output.");
        Assert.IsFalse(
            Directory.Exists(Path.Combine(projectRoot, "node_modules")),
            "Netpack package consumer must use NuGet-carried library assets instead of frontend node_modules.");
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgConsumer_CleanBuildsVueRenderArtifactsByteForByte()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgDeterministicConsumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot, enableEmit: true);
        var restorePackagesPath = package.RestorePackagesPath;
        var commonArguments = new[]
        {
            "/m:1",
            "/p:BuildInParallel=false",
            $"-p:RestoreSources={package.PackageOutputDirectory}",
            "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            $"-p:RestorePackagesPath={restorePackagesPath}",
            $"-p:JazorPackageVersion={package.PackageVersion}"
        };

        var firstBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "-t:Rebuild", .. commonArguments]);
        Assert.AreEqual(0, firstBuild.ExitCode, firstBuild.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var firstArtifacts = ReadArtifactHashes(outputRoot);
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "jazor-manifest.json");
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "components/counter.mjs");
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "components/counter.mjs.map");
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "components/plain-text.mjs");
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "components/plain-text.mjs.map");
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "components/keyed-list-100.mjs");
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "components/keyed-list-100.mjs.map");
        CollectionAssert.DoesNotContain(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "@jazor/vue-runtime/render-context.mjs");
        CollectionAssert.DoesNotContain(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "@jazor/vue-runtime/render-context-core.mjs");

        var firstManifestText = await File.ReadAllTextAsync(Path.Combine(outputRoot, "jazor-manifest.json"));
        Assert.IsFalse(firstManifestText.Contains("generatedAtUtc", StringComparison.OrdinalIgnoreCase), firstManifestText);
        Assert.IsFalse(firstManifestText.Contains("rootAssemblyPath", StringComparison.OrdinalIgnoreCase), firstManifestText);
        Assert.IsFalse(
            firstManifestText.Contains(projectRoot, StringComparison.OrdinalIgnoreCase),
            "Canonical manifest must not persist the external consumer's absolute project path.");
        AssertArtifactsDoNotContain(outputRoot, projectRoot);

        Directory.Delete(outputRoot, recursive: true);

        var secondBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "-t:Rebuild", .. commonArguments]);
        Assert.AreEqual(0, secondBuild.ExitCode, secondBuild.ToString());

        var secondArtifacts = ReadArtifactHashes(outputRoot);
        CollectionAssert.AreEqual(
            firstArtifacts.Select(static artifact => artifact.ToString()).ToArray(),
            secondArtifacts.Select(static artifact => artifact.ToString()).ToArray(),
            "Repeated clean builds of the same external Razor SG multi-fixture consumer must produce the same relative-path/SHA256 Vue render artifact manifest.");
        AssertArtifactsDoNotContain(outputRoot, projectRoot);
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgConsumer_CleanEmitRemovesDeletedComponentArtifacts()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgDeletedComponentConsumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot, enableEmit: true);
        var restorePackagesPath = package.RestorePackagesPath;
        var commonArguments = new[]
        {
            "/m:1",
            "/p:BuildInParallel=false",
            $"-p:RestoreSources={package.PackageOutputDirectory}",
            "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            $"-p:RestorePackagesPath={restorePackagesPath}",
            $"-p:JazorPackageVersion={package.PackageVersion}"
        };

        var firstBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "-t:Rebuild", .. commonArguments]);
        Assert.AreEqual(0, firstBuild.ExitCode, firstBuild.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var componentModulePath = Path.Combine(outputRoot, "components", "counter.mjs");
        var componentMapPath = Path.Combine(outputRoot, "components", "counter.mjs.map");

        Assert.IsTrue(File.Exists(componentModulePath), $"Initial component module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(componentMapPath), $"Initial component source map was not generated: {componentMapPath}");
        CollectionAssert.Contains(
            LoadManifest(manifestPath).Modules.Select(static module => module.RelativePath).ToArray(),
            "components/counter.mjs");

        File.Delete(Path.Combine(projectRoot, "Counter.razor"));
        File.Delete(Path.Combine(projectRoot, "Counter.razor.cs"));

        var secondBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "--no-restore", .. commonArguments]);
        Assert.AreEqual(0, secondBuild.ExitCode, secondBuild.ToString());

        Assert.IsFalse(File.Exists(componentModulePath), "Clean emit must delete the removed RazorVue component module.");
        Assert.IsFalse(File.Exists(componentMapPath), "Clean emit must delete the removed RazorVue component source map.");

        var currentManifest = LoadManifest(manifestPath);
        var currentPaths = currentManifest.Modules
            .Select(static module => module.RelativePath)
            .ToArray();
        CollectionAssert.DoesNotContain(currentPaths, "components/counter.mjs");
        CollectionAssert.DoesNotContain(
            currentManifest.Modules
                .Select(static module => module.SourceMapPath)
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Cast<string>()
                .ToArray(),
            "components/counter.mjs.map");
    }

    [TestMethod]
    [TestCategory("Browser")]
    public async Task Build_LocalPackages_WithExternalRazorSgConsumer_RunsCounterInRealBrowser()
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        if (browserPath is null)
        {
            Assert.Inconclusive(
                "Real browser Counter smoke requires Microsoft Edge, Chrome, or Chromium. " +
                "Set RAZORVUE_BROWSER_EXE to the browser executable path.");
            return;
        }

        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgBrowserCounterConsumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot, enableEmit: true);
        var restorePackagesPath = package.RestorePackagesPath;
        var build = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var componentModulePath = Path.Combine(outputRoot, "components", "counter.mjs");
        var runtimeModulePath = Path.Combine(outputRoot, "@jazor", "vue-runtime", "render-context.mjs");
        var runtimeCoreModulePath = Path.Combine(outputRoot, "@jazor", "vue-runtime", "render-context-core.mjs");
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue component module was not generated: {componentModulePath}");
        Assert.IsFalse(File.Exists(runtimeModulePath), $"Direct render must not emit an unused runtime bridge: {runtimeModulePath}");
        Assert.IsFalse(File.Exists(runtimeCoreModulePath), $"Direct render must not emit an unused runtime core bridge: {runtimeCoreModulePath}");

        var harnessRoot = Path.Combine(workspace.RootPath, "browser-harness");
        var harnessJazorRoot = Path.Combine(harnessRoot, "jazor");
        CopyDirectory(outputRoot, harnessJazorRoot, includeGeneratedAssets: true);
        var harnessImportMapPath = Path.Combine(harnessJazorRoot, "importmap.json");
        Assert.IsTrue(File.Exists(harnessImportMapPath), $"Import map was not materialized: {harnessImportMapPath}");
        CreateCounterBrowserHarness(harnessRoot, harnessImportMapPath);

        var distRoot = Path.Combine(harnessRoot, "dist");
        Directory.CreateDirectory(distRoot);
        var bundle = await RunDenoAsync(
            package,
            harnessRoot,
            [
                "bundle",
                "--config",
                "deno.json",
                "--platform",
                "browser",
                "--format",
                "esm",
                "--packages=bundle",
                "--sourcemap=linked",
                "-o",
                "dist/client-entry.js",
                "client-entry.mjs"
            ],
            TimeSpan.FromMinutes(5));
        Assert.AreEqual(0, bundle.ExitCode, bundle.ToString());

        var indexPath = Path.Combine(harnessRoot, "index.html");
        var browser = await BrowserSmokeTestHelper.RunBrowserDumpDomAsync(browserPath, indexPath);
        Assert.AreEqual(0, browser.ExitCode, browser.ToString());

        using var smokePayload = BrowserSmokeTestHelper.ReadBrowserSmokePayload(browser, "RazorVue");
        var smoke = smokePayload.RootElement;
        Assert.IsTrue(
            smoke.GetProperty("ok").GetBoolean(),
            "Browser Counter smoke failed." + Environment.NewLine + smoke.GetRawText() + Environment.NewLine + browser);
        AssertJsonTextContains(smoke, "initialText", "Clicks: 0");
        AssertJsonTextContains(smoke, "firstClickText", "Clicks: 1");
        AssertJsonTextContains(smoke, "thirdClickText", "Clicks: 3");

        var failures = smoke.GetProperty("failures").EnumerateArray()
            .Select(static failure => failure.GetString() ?? "")
            .Where(static failure => !string.IsNullOrWhiteSpace(failure))
            .ToArray();
        Assert.HasCount(0, failures, "Browser console/runtime failures were observed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
    }

    private static async Task<LocalStylePackageFixture> CreateLocalStylePackageAsync()
    {
        var repoRoot = FindRepoRoot();
        var packageOutputDirectory = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "css-nupkg", Guid.NewGuid().ToString("N"));
        var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "css-restore-packages", Guid.NewGuid().ToString("N"));
        var packageBuildOutputRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "css-package-out", Guid.NewGuid().ToString("N"));
        var packageBuildIntermediateRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "css-package-obj", Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(packageOutputDirectory);
        Directory.CreateDirectory(restorePackagesPath);

        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                "-c",
                "Debug",
                "-o",
                packageOutputDirectory,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:NuGetPackageRoot={EnsureTrailingDirectorySeparator(restorePackagesPath)}",
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        var packageVersion = DiscoverPackageVersion(packageOutputDirectory, "Jazor");

        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Style", "ECMAScript.Style.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.Style", "bin", "Debug", "net11.0", "ECMAScript.Style.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);

        return new LocalStylePackageFixture(
            repoRoot,
            packageVersion,
            packageOutputDirectory,
            restorePackagesPath,
            GetPackagePath(packageOutputDirectory, "ECMAScript.Style", packageVersion));
    }

    private static async Task<LocalPackageFixture> CreateLocalPackageAsync()
    {
        var repoRoot = FindRepoRoot();
        var packageOutputDirectory = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg", Guid.NewGuid().ToString("N"));
        var restorePackagesPath = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "restore-packages", Guid.NewGuid().ToString("N"));
        var packageBuildOutputRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "package-out", Guid.NewGuid().ToString("N"));
        var packageBuildIntermediateRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "package-obj", Guid.NewGuid().ToString("N"));
        var emitPublishDirectory = Path.Combine(packageBuildOutputRoot, "Jazor.Emit", "bin", "Debug", "net11.0", "publish");

        if (Directory.Exists(packageOutputDirectory))
            Directory.Delete(packageOutputDirectory, recursive: true);

        Directory.CreateDirectory(packageOutputDirectory);
        Directory.CreateDirectory(restorePackagesPath);

        var jazorPack = await RunDotNetAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                "-c",
                "Debug",
                "-o",
                packageOutputDirectory,
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:NuGetPackageRoot={EnsureTrailingDirectorySeparator(restorePackagesPath)}",
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        Assert.AreEqual(0, jazorPack.ExitCode, jazorPack.ToString());
        Assert.IsFalse(
            jazorPack.ToString().Contains("NU5118", StringComparison.OrdinalIgnoreCase),
            "Jazor package emitted duplicate pack warnings." + Environment.NewLine + jazorPack);
        AssertPackageArtifactOutputs(packageBuildOutputRoot, emitPublishDirectory);
        var packageVersion = DiscoverPackageVersion(packageOutputDirectory, "Jazor");

        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "Jazor.Vue", "Jazor.Vue.csproj"),
            Path.Combine(packageBuildOutputRoot, "Jazor.RazorVue", "bin", "Debug", "netstandard2.0", "Jazor.RazorVue.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);
        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Vuetify", "ECMAScript.Vuetify.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.Vuetify", "bin", "Debug", "net11.0", "ECMAScript.Vuetify.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);
        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "ECMAScript.VueRoute.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.VueRoute", "bin", "Debug", "net11.0", "ECMAScript.VueRoute.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);
        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "ECMAScript.Pinia.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.Pinia", "bin", "Debug", "net11.0", "ECMAScript.Pinia.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);
        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing", "ECMAScript.Pinia.Testing.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.Pinia.Testing", "bin", "Debug", "net11.0", "ECMAScript.Pinia.Testing.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);
        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.TDesign", "ECMAScript.TDesign.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.TDesign", "bin", "Debug", "net11.0", "ECMAScript.TDesign.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);
        await PackProjectAndAssertOutputAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.ElementPlus", "ECMAScript.ElementPlus.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.ElementPlus", "bin", "Debug", "net11.0", "ECMAScript.ElementPlus.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot,
            packageOutputDirectory,
            packageVersion);

        return new LocalPackageFixture(
            repoRoot,
            packageVersion,
            packageOutputDirectory,
            restorePackagesPath,
            GetPackagePath(packageOutputDirectory, packageVersion),
            GetPackagePath(packageOutputDirectory, "Jazor.Vue", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Vuetify", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.VueRoute", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia.Testing", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.TDesign", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.ElementPlus", packageVersion),
            GetBundledDenoPath(emitPublishDirectory));
    }

    private static void AssertPackageEntries(string packagePath, params string[] expectedPaths)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entries = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var expectedPath in expectedPaths)
            Assert.IsTrue(entries.Contains(expectedPath), $"Package '{packagePath}' is missing '{expectedPath}'.");
    }

    private static void AssertPackageArtifactOutputs(
        string packageBuildOutputRoot,
        string emitPublishDirectory)
    {
        Assert.IsTrue(
            File.Exists(Path.Combine(packageBuildOutputRoot, "ECMAScript", "bin", "Debug", "net11.0", "ECMAScript.dll")),
            "Jazor package preparation did not build ECMAScript.");
        Assert.IsTrue(
            File.Exists(Path.Combine(packageBuildOutputRoot, "ECMAScript.Contract", "bin", "Debug", "netstandard2.0", "ECMAScript.Contract.dll")),
            "Jazor package preparation did not build ECMAScript.Contract.");
        Assert.IsTrue(
            File.Exists(Path.Combine(packageBuildOutputRoot, "Jazor.Analyzer", "bin", "Debug", "netstandard2.0", "Jazor.Analyzer.dll")),
            "Jazor package preparation did not build Jazor.Analyzer.");
        Assert.IsTrue(
            File.Exists(Path.Combine(emitPublishDirectory, "Jazor.Emit.dll")),
            "Jazor package preparation did not publish Jazor.Emit.");
    }

    private static async Task RunDotNetAndAssertAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var result = await RunDotNetAsync(workingDirectory, arguments);
        Assert.AreEqual(0, result.ExitCode, result.ToString());
    }

    private static async Task PackProjectAndAssertOutputAsync(
        string repoRoot,
        string projectPath,
        string expectedOutputPath,
        string packageBuildOutputRoot,
        string packageBuildIntermediateRoot,
        string packageOutputDirectory,
        string packageVersion)
    {
        // 生产打包验证不能只看 DLL 是否存在；Directory.Build.props 或依赖版本变更时旧产物会污染 nupkg。
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                projectPath,
                "-c",
                "Debug",
                "-o",
                packageOutputDirectory,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:PackageVersion={packageVersion}",
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);

        Assert.IsTrue(File.Exists(expectedOutputPath), $"Expected build output was not produced: {expectedOutputPath}");
    }

    private static string EnsureTrailingDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static async Task<ProcessResult> RunSourceReferencedRazorVueBuildAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments)
    {
        await SourceReferencedRazorVueBuildGate.WaitAsync();
        try
        {
            return await RunDotNetAsync(workingDirectory, arguments);
        }
        finally
        {
            SourceReferencedRazorVueBuildGate.Release();
        }
    }

    private static async Task<ProcessResult> RunDotNetAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        ProcessResult? result = null;

        for (var attempt = 0; attempt < 3; attempt++)
        {
            result = await RunDotNetOnceAsync(workingDirectory, arguments);
            if (result.ExitCode == 0 || !IsTransientBuildRetryCandidate(result) || attempt == 2)
                return result;

            await Task.Delay(250);
        }

        return result ?? throw new InvalidOperationException("dotnet process did not produce a result.");
    }

    private static async Task<ProcessResult> RunDotNetOnceAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(FindRepoRoot(), ".dotnet");
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        // SDK integration tests build and pack the same projects repeatedly; disabling
        // the compiler server avoids transient file locks in cross-targeted RazorVue builds.
        startInfo.Environment["UseSharedCompilation"] = "false";

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessResult(
            process.ExitCode,
            await standardOutput,
            await standardError);
    }

    private static async Task RunDenoTestAsync(string denoExecutablePath, string testFile, string workingDirectory)
    {
        Assert.IsTrue(File.Exists(denoExecutablePath), $"Bundled DenoHost runtime was not found: {denoExecutablePath}");

        var startInfo = new ProcessStartInfo(denoExecutablePath)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("test");
        startInfo.ArgumentList.Add("--quiet");
        startInfo.ArgumentList.Add("--allow-all");
        startInfo.ArgumentList.Add(testFile);
        // A test-local cache keeps Deno's module resolution independent from a developer cache.
        startInfo.Environment["DENO_DIR"] = Path.Combine(workingDirectory, ".deno-cache");

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        Assert.AreEqual(
            0,
            process.ExitCode,
            "Bundled DenoHost runtime test failed." + Environment.NewLine +
            await standardOutput + Environment.NewLine +
            await standardError);
    }

    private static string FindRepoRoot([CallerFilePath] string sourceFilePath = "")
    {
        foreach (var startPath in GetRepoRootSearchStartPaths(sourceFilePath))
        {
            var root = TryFindRepoRootFrom(startPath);
            if (root is not null)
                return root;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static IEnumerable<string> GetRepoRootSearchStartPaths(string sourceFilePath)
    {
        if (!string.IsNullOrWhiteSpace(sourceFilePath))
            yield return Path.GetDirectoryName(sourceFilePath)!;

        yield return Environment.CurrentDirectory;
        yield return AppContext.BaseDirectory;
    }

    private static string? TryFindRepoRootFrom(string? startPath)
    {
        if (string.IsNullOrWhiteSpace(startPath))
            return null;

        var directory = new DirectoryInfo(startPath);
        if (!directory.Exists && directory.Parent is not null)
            directory = directory.Parent;

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return null;
    }

    private static string DiscoverPackageVersion(string packageOutputDirectory, string packageId)
    {
        var prefix = packageId + ".";
        var nupkg = Directory.GetFiles(packageOutputDirectory, $"{packageId}.*.nupkg")
            .Where(static f => !f.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static f => File.GetLastWriteTimeUtc(f))
            .FirstOrDefault();

        if (nupkg is null)
            throw new FileNotFoundException($"No '{packageId}' nupkg found in '{packageOutputDirectory}'.");

        var fileName = Path.GetFileNameWithoutExtension(nupkg);
        return fileName[prefix.Length..];
    }

    private static string GetPackagePath(string packageOutputDirectory, string packageVersion)
        => GetPackagePath(packageOutputDirectory, "Jazor", packageVersion);

    private static string GetPackagePath(string packageOutputDirectory, string packageId, string packageVersion)
    {
        var packagePath = Path.Combine(packageOutputDirectory, $"{packageId}.{packageVersion}.nupkg");
        if (!File.Exists(packagePath))
            throw new FileNotFoundException($"Could not locate packed Jazor package '{packagePath}'.", packagePath);

        return packagePath;
    }

    private static string GetBundledDenoPath(string emitPublishDirectory)
    {
        var denoPath = Path.Combine(emitPublishDirectory, "runtimes", "win-x64", "native", "deno.exe");
        if (!File.Exists(denoPath))
            throw new FileNotFoundException($"Bundled Deno runtime was not found under '{emitPublishDirectory}'.", denoPath);

        return denoPath;
    }

    private static void CreateCounterBrowserHarness(string harnessRoot, string importMapPath)
    {
        using var importMap = JsonDocument.Parse(File.ReadAllText(importMapPath));
        var vuePath = importMap.RootElement
            .GetProperty("imports")
            .GetProperty("vue")
            .GetString();
        if (string.IsNullOrWhiteSpace(vuePath) || !vuePath.StartsWith("/jazor/", StringComparison.Ordinal))
            throw new InvalidOperationException($"Materialized import map does not provide a local Vue path: {vuePath}");

        // Debug and release select different Vue files. Reuse the generated import map
        // instead of duplicating a versioned vendor path in the browser harness.
        var denoConfig = new
        {
            imports = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["vue"] = "." + vuePath,
                ["@jazor/vue-runtime/"] = "./jazor/@jazor/vue-runtime/"
            }
        };
        WriteFile(
            Path.Combine(harnessRoot, "deno.json"),
            JsonSerializer.Serialize(denoConfig, new JsonSerializerOptions { WriteIndented = true }));

        WriteFile(
            Path.Combine(harnessRoot, "index.html"),
            """
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="utf-8">
                <title>Jazor RazorVue Counter Browser Smoke</title>
                <script>
                  window.__jazorSmokeFailures = [];
                  (function () {
                    function formatArg(value) {
                      if (value instanceof Error) {
                        return value.stack || value.message;
                      }

                      if (typeof value === "string") {
                        return value;
                      }

                      try {
                        return JSON.stringify(value);
                      } catch {
                        return String(value);
                      }
                    }

                    function record(kind, values) {
                      window.__jazorSmokeFailures.push(kind + ": " + Array.from(values).map(formatArg).join(" "));
                    }

                    const originalError = console.error.bind(console);
                    const originalWarn = console.warn.bind(console);
                    console.error = function (...args) {
                      record("console.error", args);
                      originalError(...args);
                    };
                    console.warn = function (...args) {
                      record("console.warn", args);
                      originalWarn(...args);
                    };
                    window.addEventListener("error", function (event) {
                      record("error", [event.message || "unknown"]);
                    });
                    window.addEventListener("unhandledrejection", function (event) {
                      record("unhandledrejection", [event.reason || "unknown"]);
                    });
                  })();
                </script>
              </head>
              <body>
                <div id="app"></div>
                <script type="module" src="./dist/client-entry.js"></script>
              </body>
            </html>
            """);

        WriteFile(
            Path.Combine(harnessRoot, "client-entry.mjs"),
            """
            import { createApp, nextTick } from "vue";
            import Counter from "./jazor/components/counter.mjs";

            function bodyText() {
              return document.body ? (document.body.textContent || "") : "";
            }

            function smokeFailures() {
              return Array.isArray(window.__jazorSmokeFailures)
                ? [...window.__jazorSmokeFailures]
                : [];
            }

            function encodeUtf8Base64(value) {
              const bytes = new TextEncoder().encode(value);
              let binary = "";
              for (const byte of bytes) {
                binary += String.fromCharCode(byte);
              }

              return btoa(binary);
            }

            function finish(payload) {
              document.documentElement.setAttribute(
                "data-jazor-smoke",
                encodeUtf8Base64(JSON.stringify(payload)));
            }

            function assertBodyContains(text, expected) {
              if (!text.includes(expected)) {
                throw new Error(`Expected browser body to contain '${expected}', but saw '${text}'.`);
              }
            }

            function clickCounterButton() {
              const button = document.querySelector("button");
              if (!(button instanceof HTMLButtonElement)) {
                throw new Error("Counter button was not rendered.");
              }

              button.click();
            }

            try {
              createApp(Counter).mount("#app");
              await nextTick();

              const initialText = bodyText();
              assertBodyContains(initialText, "Clicks: 0");

              clickCounterButton();
              await nextTick();
              const firstClickText = bodyText();
              assertBodyContains(firstClickText, "Clicks: 1");

              clickCounterButton();
              await nextTick();
              assertBodyContains(bodyText(), "Clicks: 2");

              clickCounterButton();
              await nextTick();
              const thirdClickText = bodyText();
              assertBodyContains(thirdClickText, "Clicks: 3");

              finish({
                ok: true,
                initialText,
                firstClickText,
                thirdClickText,
                failures: smokeFailures()
              });
            } catch (error) {
              finish({
                ok: false,
                error: error instanceof Error ? (error.stack || error.message) : String(error),
                bodyText: bodyText(),
                failures: smokeFailures()
              });
            }
            """);
    }

    private static async Task<ProcessResult> RunDenoAsync(
        LocalPackageFixture package,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        TimeSpan timeout)
    {
        var denoCacheRoot = Path.Combine(workingDirectory, ".deno-cache");
        Directory.CreateDirectory(denoCacheRoot);

        return await RunProcessAsync(
            package.DenoExePath,
            workingDirectory,
            arguments,
            timeout,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["DENO_DIR"] = denoCacheRoot
            });
    }

    private static async Task<ProcessResult> RunProcessAsync(
        string fileName,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        IReadOnlyDictionary<string, string>? environment)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
            startInfo.ArgumentList.Add(argument);

        if (environment is not null)
        {
            foreach (var (key, value) in environment)
                startInfo.Environment[key] = value;
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();

        var timedOut = false;
        using var timeoutSource = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token);
        }
        catch (OperationCanceledException)
        {
            timedOut = true;
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
            }

            await process.WaitForExitAsync();
        }

        var output = await standardOutput;
        var error = await standardError;
        return timedOut
            ? new ProcessResult(-1, output, $"Process timed out after {timeout}." + Environment.NewLine + error)
            : new ProcessResult(process.ExitCode, output, error);
    }

    private static void AssertJsonTextContains(JsonElement element, string propertyName, string expected)
    {
        var actual = element.GetProperty(propertyName).GetString() ?? "";
        StringAssert.Contains(actual, expected, $"Browser smoke payload property '{propertyName}' did not contain expected text.");
    }

    private static string ReadPackageEntryText(string packagePath, string entryName)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.GetEntry(entryName)
            ?? throw new FileNotFoundException($"Package entry '{entryName}' was not found in '{packagePath}'.", entryName);

        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool includeGeneratedAssets = false)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, directory);
            if (!includeGeneratedAssets && ShouldSkip(relativePath))
                continue;

            Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            if (!includeGeneratedAssets && ShouldSkip(relativePath))
                continue;

            var destinationPath = Path.Combine(destinationDirectory, relativePath);
            var destinationParent = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationParent))
                Directory.CreateDirectory(destinationParent);

            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private static void WriteFile(string path, string content)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, content.ReplaceLineEndings("\n"), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string GetImportLine(string script, string modulePath)
    {
        var suffix = $"from \"{modulePath}\";";
        var line = script
            .Split('\n')
            .Select(static x => x.Trim())
            .FirstOrDefault(x => x.EndsWith(suffix, StringComparison.Ordinal));

        Assert.IsFalse(string.IsNullOrWhiteSpace(line), $"Import line not found for module '{modulePath}'.");
        return line!;
    }

    private static bool IsTransientBuildRetryCandidate(ProcessResult result)
    {
        var output = result.ToString();
        return output.Contains("CS0006", StringComparison.Ordinal) ||
               output.Contains("MSB3030", StringComparison.Ordinal) ||
               output.Contains("being used by another process", StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateDefaultOutputStaticHostProject(string projectRoot)
    {
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "StaticHostDefaultOutput.csproj");

        WriteFile(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorMode>debug</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            Console.WriteLine("Static host default output sample");
            """);

        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace StaticHostDefaultOutput;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "ready";
            }
            """);

        return projectPath;
    }

    private static string CreateEcmaScriptStylePackageConsumerProject(string projectRoot)
    {
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "EcmaScriptStylePackageConsumer.csproj");

        WriteFile(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <LangVersion>preview</LangVersion>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="ECMAScript.Style" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            Console.WriteLine("ECMAScript.Style package consumer");
            """);

        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;
            using ECMAScript.Style;
            using static ECMAScript.Style.css;

            namespace EcmaScriptStylePackageConsumer;

            [ECMAScriptModule("app.mjs")]
            public static class AppModule
            {
                private static readonly CssContext Context = css.context(new CssOptions
                {
                    Detached = true,
                    StyleId = "server-css"
                });

                public static string ButtonClass() => css.style(Context, new CssRule
                {
                    Color = color("white"),
                    BackgroundColor = hex("1769aa"),
                    Children =
                    [
                        new(CssChildKind.Container, "toolbar (width > 30rem)", new CssRule
                        {
                            Display = grid
                        })
                    ]
                });

                public static CssSnapshot Snapshot()
                {
                    css.atRule(Context, new CssAtRule(
                        "font-face",
                        new CssDeclarations
                        {
                            FontFamily = str("Example Sans"),
                            ["src"] = raw("url(example.woff2)")
                        }));
                    return css.snapshot(Context);
                }
            }
            """);

        return projectPath;
    }

    private static string CreateDefaultOutputWebHostProject(string projectRoot)
    {
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "WebHostDefaultOutput.csproj");

        WriteFile(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorMode>debug</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            app.MapGet("/", () => "ready");
            app.Run();
            """);

        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace WebHostDefaultOutput;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "ready";
            }
            """);

        return projectPath;
    }

    private static string ReadCounterRazorSgGeneratedSource(string generatedRoot)
    {
        Assert.IsTrue(Directory.Exists(generatedRoot), $"Compiler generated source root was not created: {generatedRoot}");

        var razorGeneratedSources = Directory
            .EnumerateFiles(generatedRoot, "*_razor.g.cs", SearchOption.AllDirectories)
            .Where(static path => !Path.GetFileName(path).Equals("_Imports_razor.g.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.AreEqual(
            3,
            razorGeneratedSources.Length,
            "The external G0 consumer must expose exactly the Counter, KeyedList100 and PlainText official Razor generated component documents." + Environment.NewLine +
            string.Join(Environment.NewLine, razorGeneratedSources));

        var counterPath = razorGeneratedSources.Single(static path =>
            Path.GetFileName(path).Equals("Counter_razor.g.cs", StringComparison.OrdinalIgnoreCase));
        AssertNoGeneratedSource(generatedRoot, "Jazor.RazorVue.RazorSgBootstrapTrace.g.cs");
        AssertNoGeneratedSource(generatedRoot, "Jazor.RazorVue.RazorSgTailTrace.g.cs");
        AssertNoGeneratedSource(generatedRoot, "Jazor.Generated.RazorSgFinalDocumentEvidence.g.cs");
        AssertNoGeneratedSource(generatedRoot, "Jazor.Generated.RazorVueCatalog.g.cs");
        AssertNoGeneratedSource(generatedRoot, "Jazor.Generated.RazorVue.Artifact_*.g.cs");

        return File.ReadAllText(counterPath);
    }

    private static string ReadSingleGeneratedSource(string generatedRoot, string searchPattern)
    {
        var generatedPaths = Directory
            .EnumerateFiles(generatedRoot, searchPattern, SearchOption.AllDirectories)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.AreEqual(
            1,
            generatedPaths.Length,
            "Expected exactly one generated source matching '" + searchPattern + "'." + Environment.NewLine +
            string.Join(Environment.NewLine, generatedPaths));
        return File.ReadAllText(generatedPaths[0]);
    }

    private static void AssertNoGeneratedSource(string generatedRoot, string searchPattern)
    {
        var generatedPath = Directory
            .EnumerateFiles(generatedRoot, searchPattern, SearchOption.AllDirectories)
            .FirstOrDefault();
        Assert.IsTrue(
            string.IsNullOrWhiteSpace(generatedPath),
            "Generated source '" + searchPattern + "' was not expected: " + generatedPath);
    }

    private static string CreateExternalRazorSgG0ConsumerProject(
        string projectRoot,
        bool enableEmit = false)
    {
        Directory.CreateDirectory(projectRoot);

        var projectPath = Path.Combine(projectRoot, "ExternalRazorSgG0Consumer.csproj");
        WriteFile(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <RazorLangVersion>11.0</RazorLangVersion>
                <UseRazorSourceGenerator>true</UseRazorSourceGenerator>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
                <JazorMode>{{(enableEmit ? "debug" : "none")}}</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="Jazor.Vue" Version="$(JazorPackageVersion)" PrivateAssets="all" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            namespace ExternalRazorSgG0Consumer;

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "Counter.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgG0Consumer;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private int count;

                private void Increment()
                {
                    count++;
                }
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "_Imports.razor"),
            """
            @using Microsoft.AspNetCore.Components.Web
            """);

        WriteFile(
            Path.Combine(projectRoot, "Counter.razor"),
            """
            <button @onclick="Increment">Clicks: @count</button>
            """);

        WriteFile(
            Path.Combine(projectRoot, "PlainText.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgG0Consumer;

            [ECMAScriptModule("./components/plain-text")]
            public partial class PlainText : ComponentBase, IVueComponent
            {
                private string text = "Hello RazorVue";
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "PlainText.razor"),
            """
            @text
            """);

        WriteFile(
            Path.Combine(projectRoot, "KeyedList100.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorSgG0Consumer;

            [ECMAScriptModule("./components/keyed-list-100")]
            public partial class KeyedList100 : ComponentBase, IVueComponent
            {
                private string[] items = new[]
                {
            """ +
            string.Join(
                "," + Environment.NewLine,
                Enumerable
                    .Range(0, 100)
                    .Select(static index => "        \"Item " + index.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\"")) +
            Environment.NewLine +
            """
                };
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "KeyedList100.razor"),
            """
            <ul>
            @foreach (var item in items)
            {
                <li key="@item">@item</li>
            }
            </ul>
            """);

        return projectPath;
    }

    private static ManifestModel LoadManifest(string manifestPath)
        => ManifestModel.TryLoad(manifestPath)
            ?? throw new FileNotFoundException("Manifest was not found: " + manifestPath, manifestPath);

    private static ArtifactHash[] ReadArtifactHashes(string outputRoot)
    {
        Assert.IsTrue(Directory.Exists(outputRoot), $"Artifact output root was not generated: {outputRoot}");

        return Directory
            .EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories)
            .Select(filePath =>
            {
                var relativePath = Path.GetRelativePath(outputRoot, filePath).Replace('\\', '/');
                var contentHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(filePath))).ToLowerInvariant();
                return new ArtifactHash(relativePath, contentHash);
            })
            .OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal)
            .ToArray();
    }

    private static void AssertArtifactsDoNotContain(string outputRoot, string unexpectedText)
    {
        foreach (var filePath in Directory.EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(filePath);
            var relativePath = Path.GetRelativePath(outputRoot, filePath).Replace('\\', '/');
            Assert.IsFalse(
                text.Contains(unexpectedText, StringComparison.OrdinalIgnoreCase),
                $"Artifact '{relativePath}' must not persist the external consumer's absolute project path.");
        }
    }

    private static bool ShouldSkip(string relativePath)
    {
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(static segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("TestResults", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("node_modules", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("dist", StringComparison.OrdinalIgnoreCase) ||
            segment.StartsWith("dist-", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("wwwroot", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals(".deno-build", StringComparison.OrdinalIgnoreCase) ||
            segment.StartsWith(".deno-build", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ArtifactHash(string RelativePath, string ContentHash);

    private sealed record LocalPackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        string RestorePackagesPath,
        string PackagePath,
        string VuePackagePath,
        string VuetifyPackagePath,
        string VueRoutePackagePath,
        string PiniaPackagePath,
        string PiniaTestingPackagePath,
        string TDesignPackagePath,
        string ElementPlusPackagePath,
        string DenoExePath);

    private sealed record LocalStylePackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        string RestorePackagesPath,
        string StylePackagePath);

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace(string repoRoot)
        {
            RootPath = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            SampleRoot = Path.Combine(RootPath, "Jazor.MultiProject");
            Directory.CreateDirectory(SampleRoot);

            // The copied sample imports ../Directory.Build.props; preserve that layout
            // so every parallel test owns a complete, isolated MSBuild tree.
            File.Copy(
                Path.Combine(repoRoot, "Directory.Build.props"),
                Path.Combine(RootPath, "Directory.Build.props"));
        }

        public string RootPath { get; }

        public string SampleRoot { get; }

        public void Dispose()
        {
            try
            {
                if (Environment.GetEnvironmentVariable("JAZOR_EMITTEST_KEEP_WORKSPACE") == "1")
                    return;

                if (Directory.Exists(RootPath))
                    Directory.Delete(RootPath, recursive: true);
            }
            catch
            {
            }
        }
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
    {
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"ExitCode: {ExitCode}");

            if (!string.IsNullOrWhiteSpace(StandardOutput))
            {
                builder.AppendLine("STDOUT:");
                builder.AppendLine(StandardOutput);
            }

            if (!string.IsNullOrWhiteSpace(StandardError))
            {
                builder.AppendLine("STDERR:");
                builder.AppendLine(StandardError);
            }

            return builder.ToString();
        }
    }
}
