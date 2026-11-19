using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Jazor.Emit;
using Jazor.NodeHost;

namespace Jazor.EmitTest;

[TestClass]
[TestCategory("Consumer")]
public sealed partial class SdkIntegrationTests
{
    private static readonly Lazy<Task<LocalCorePackageFixture>> LocalCorePackage = new(CreateLocalCorePackageAsync);
    private static readonly Lazy<Task<LocalPackageFixture>> LocalPackage = new(CreateLocalPackageAsync);
    private static readonly Lazy<Task<LocalReleasePackageFixture>> LocalReleasePackage = new(CreateLocalReleasePackageAsync);
    private static readonly Lazy<Task<LocalStylePackageFixture>> LocalStylePackage = new(CreateLocalStylePackageAsync);
    private static readonly SemaphoreSlim DotNetProcessGate = new(4, 4);

    [TestMethod]
    [TestCategory("Consumer.Packages")]
    public async Task CreateLocalPackage_SeparatesDirectToolingFromTransitiveResourceLocators()
    {
        var package = await LocalPackage.Value;

        using var jazorArchive = ZipFile.OpenRead(package.PackagePath);
        var jazorEntryNames = jazorArchive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        CollectionAssert.Contains(jazorEntryNames, "build/Jazor.props");
        CollectionAssert.Contains(jazorEntryNames, "build/Jazor.targets");
        CollectionAssert.Contains(jazorEntryNames, "buildTransitive/Jazor.Resources.targets");
        var jazorTransitiveEntries = jazorEntryNames
            .Where(static path => path.StartsWith("buildTransitive/", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        CollectionAssert.AreEqual(
            new[] { "buildTransitive/Jazor.Resources.targets" },
            jazorTransitiveEntries,
            "Only a JS-resource manifest locator may propagate from the Jazor package.");
        var jazorAnalyzerEntries = jazorEntryNames
            .Where(static path => path.StartsWith("tools/net11.0/analyzers/", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        CollectionAssert.Contains(jazorAnalyzerEntries, "tools/net11.0/analyzers/Jazor.Analyzer.dll");
        CollectionAssert.Contains(jazorAnalyzerEntries, "tools/net11.0/analyzers/Jazor.Compiler.dll");
        Assert.IsFalse(
            jazorEntryNames.Any(static path => path.StartsWith("analyzers/dotnet/cs/", StringComparison.OrdinalIgnoreCase)),
            "NuGet automatic analyzer assets would incorrectly activate tooling for transitive package consumers.");
        using var vueArchive = ZipFile.OpenRead(package.VuePackagePath);
        var vueEntryNames = vueArchive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        AssertPackageDoesNotContain(
            package.VuePackagePath,
            "lib/net11.0/ECMAScript.Blazor.dll",
            "lib/net11.0/ECMAScript.Blazor.pdb");
        CollectionAssert.Contains(vueEntryNames, "lib/net11.0/ECMAScript.Vue.dll");
        CollectionAssert.DoesNotContain(vueEntryNames, "lib/net11.0/ECMAScript.VueContract.dll");
        CollectionAssert.Contains(vueEntryNames, "buildTransitive/ECMAScript.Vue.targets");
        Assert.IsFalse(vueEntryNames.Any(static path =>
            path.StartsWith("tools/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("analyzers/", StringComparison.OrdinalIgnoreCase)));
        var directJazorTarget = ReadPackageEntryText(package.PackagePath, "build/Jazor.targets");
        var transitiveJazorTarget = ReadPackageEntryText(package.PackagePath, "buildTransitive/Jazor.Resources.targets");
        StringAssert.Contains(directJazorTarget, "analyzers\\*.dll", StringComparison.Ordinal);
        StringAssert.Contains(directJazorTarget, "JazorDebug", StringComparison.Ordinal);
        Assert.AreEqual(
            2,
            directJazorTarget.Split("ConsoleToMSBuild=\"true\"", StringSplitOptions.None).Length - 1,
            directJazorTarget);
        StringAssert.Contains(directJazorTarget, "--assembly-list", StringComparison.Ordinal);
        StringAssert.Contains(directJazorTarget, "--library-manifest-list", StringComparison.Ordinal);
        StringAssert.Contains(transitiveJazorTarget, "JazorLibraryManifest", StringComparison.Ordinal);
        Assert.IsFalse(transitiveJazorTarget.Contains("JazorDebug", StringComparison.Ordinal), transitiveJazorTarget);
        Assert.IsFalse(transitiveJazorTarget.Contains("<Analyzer", StringComparison.Ordinal), transitiveJazorTarget);

        var transitiveVueTarget = ReadPackageEntryText(package.VuePackagePath, "buildTransitive/ECMAScript.Vue.targets");
        StringAssert.Contains(transitiveVueTarget, "JazorLibraryManifest");
        Assert.IsFalse(transitiveVueTarget.Contains("<Analyzer ", StringComparison.Ordinal));
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
    public async Task CreateLocalPackage_ExcludesJazorToolAssetsFromTransitiveComponentDependencies()
    {
        var package = await LocalPackage.Value;
        var packageNuspecs = new[]
        {
            (package.VueRoutePackagePath, "ECMAScript.VueRoute.nuspec"),
            (package.PiniaPackagePath, "ECMAScript.Pinia.nuspec"),
            (package.PiniaTestingPackagePath, "ECMAScript.Pinia.Testing.nuspec"),
        };

        foreach (var (packagePath, entryName) in packageNuspecs)
        {
            var nuspec = ReadPackageEntryText(packagePath, entryName);
            foreach (var dependencyLine in nuspec
                         .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
                         .Where(static line => line.Contains("<dependency id=\"Jazor\"", StringComparison.Ordinal)))
            {
                Assert.IsTrue(
                    dependencyLine.Contains("exclude=\"Build,Analyzers\"", StringComparison.Ordinal),
                    $"Jazor tooling must be directly selected by the consumer: {entryName}");
            }
        }
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
    public async Task CreateLocalPackage_CarriesNodeApiRuntimeAndFrontendHost()
    {
        var package = await LocalCorePackage.Value;
        var nuspec = ReadPackageEntryText(package.PackagePath, "Jazor.nuspec");
        var props = ReadPackageEntryText(package.PackagePath, "build/Jazor.props");
        using var archive = ZipFile.OpenRead(package.PackagePath);
        var entryNames = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();

        StringAssert.Contains(nuspec, "id=\"Microsoft.JavaScript.NodeApi\"", StringComparison.Ordinal);
        var nativeDependencies = XDocument.Parse(nuspec).Descendants()
            .Where(element => element.Name.LocalName == "dependency")
            .Select(element => (string?)element.Attribute("id"))
            .Where(id => id?.Contains("LibNode", StringComparison.OrdinalIgnoreCase) == true)
            .ToArray();
        CollectionAssert.AreEquivalent(new[] {
            "Microsoft.JavaScript.LibNode.win-x64", "Microsoft.JavaScript.LibNode.win-arm64",
            "Microsoft.JavaScript.LibNode.linux-x64", "Microsoft.JavaScript.LibNode.linux-arm64"
        }, nativeDependencies);
        Assert.IsFalse(nuspec.Contains("DenoHost", StringComparison.Ordinal));
        Assert.IsFalse(entryNames.Any(path => path.Contains("DenoHost", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith("/deno.exe", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/deno", StringComparison.OrdinalIgnoreCase)));
        Assert.IsFalse(
            nuspec.Contains("Microsoft.AspNetCore.SpaProxy", StringComparison.OrdinalIgnoreCase),
            "The unified frontend host must not restore the ASP.NET Core SpaProxy package.");
        foreach (var asset in new[]
                 {
                     "lib/net11.0/Jazor.AspNetCore.Dev.dll",
                     "lib/net11.0/Jazor.AspNetCore.Dev.xml"
                 })
        {
            Assert.IsTrue(
                entryNames.Contains(asset, StringComparer.OrdinalIgnoreCase),
                    $"The core Jazor package must carry the frontend host asset '{asset}'. Actual lib entries: " +
                    string.Join(", ", entryNames.Where(static name => name.StartsWith("lib/", StringComparison.Ordinal))));
        }
        Assert.IsFalse(
            entryNames.Any(static path => path.EndsWith("spa.proxy.json", StringComparison.OrdinalIgnoreCase)),
            "The package must not carry SpaProxy hosting-startup configuration.");
        // The build host selects libnode independently of the consumer target
        // RID. All tool-native assets must survive packing on a single platform.
        var nativePaths = new[]
        {
            "tools/net11.0/runtimes/win-x64/native/libnode.dll",
            "tools/net11.0/runtimes/win-arm64/native/libnode.dll",
            "tools/net11.0/runtimes/linux-x64/native/libnode.so",
            "tools/net11.0/runtimes/linux-arm64/native/libnode.so"
        };
        CollectionAssert.AreEquivalent(nativePaths, entryNames
            .Where(path => path.StartsWith("tools/net11.0/runtimes/", StringComparison.Ordinal)).ToArray());
        foreach (var file in new[] { "LICENSE", "NOTICE.txt" })
        {
            Assert.IsNotNull(archive.GetEntry($"licenses/libnode/{file}"), file);
        }

        StringAssert.Contains(
            props,
            "<JsonSerializerIsReflectionEnabledByDefault>true</JsonSerializerIsReflectionEnabledByDefault>",
            StringComparison.Ordinal);
        Assert.IsTrue(
            entryNames.Contains("tools/net11.0/Jazor.NodeHost.dll", StringComparer.OrdinalIgnoreCase));
        Assert.IsTrue(entryNames.Contains("lib/net11.0/Jazor.NodeHost.dll", StringComparer.OrdinalIgnoreCase));
        foreach (var rid in new[] { "win-x64", "win-arm64" })
            Assert.IsTrue(entryNames.Contains($"tools/net11.0/Jazor.Emit.{rid}.exe", StringComparer.OrdinalIgnoreCase));
        StringAssert.Contains(props, "<CETCompat>false</CETCompat>");
    }

    [TestMethod]
    [TestCategory("Consumer.Release")]
    public async Task Build_LocalReleasePackage_CoreConsumer_ExcludesBlazorAndVuePackages()
    {
        var package = await LocalReleasePackage.Value;
        using var workspace = new TestWorkspace(package.RepoRoot);
        var coreRoot = Path.Combine(workspace.RootPath, "ReleaseCorePackageConsumer");
        var coreProject = CreateReleaseCorePackageConsumerProject(coreRoot);
        var coreBuild = await RunDotNetAsync(
            package.RepoRoot,
            ["build", coreProject, .. GetReleaseConsumerBuildArguments(package, workspace.RestorePackagesPath)]);
        Assert.AreEqual(0, coreBuild.ExitCode, coreBuild.ToString());

        var coreAssets = ReadProjectAssetsText(coreRoot);
        Assert.IsFalse(coreAssets.Contains("ECMAScript.Blazor.dll", StringComparison.OrdinalIgnoreCase), coreAssets);
        Assert.IsFalse(coreAssets.Contains("ECMAScript.Vue", StringComparison.OrdinalIgnoreCase), coreAssets);
        Assert.IsFalse(coreAssets.Contains("Microsoft.AspNetCore.App", StringComparison.Ordinal), coreAssets);
        var coreOutput = Path.Combine(coreRoot, "bin", "Release", "net11.0");
        Assert.IsFalse(
            Directory.Exists(coreOutput) &&
            Directory.EnumerateFiles(coreOutput, "ECMAScript.Blazor.dll", SearchOption.AllDirectories).Any(),
            "The core Jazor package consumer must not copy ECMAScript.Blazor.");
        var assemblyPath = Path.Combine(coreOutput, "ReleaseCorePackageConsumer.dll");
        var loadContext = new EmitLoadContext(assemblyPath);
        try
        {
            var consumerAssembly = loadContext.LoadFromAssemblyPath(assemblyPath);
            var sourceModule = CatalogReader.TryRead(consumerAssembly)!.Single();
            StringAssert.Contains(sourceModule.Content, "class Counter", StringComparison.Ordinal);
            StringAssert.Contains(sourceModule.Content, "new Counter(1)", StringComparison.Ordinal);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
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
        StringAssert.Contains(nuspec, "<dependency id=\"ECMAScript.Vue\"");
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
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
        CollectionAssert.Contains(piniaEntryNames, "jazor/pinia/manifest.json");
        Assert.IsFalse(
            piniaEntryNames.Any(static path => path.StartsWith("jazor/pinia/dist/", StringComparison.OrdinalIgnoreCase)),
            "Pinia runtime modules belong to the restored npm package, not the binding archive.");
        CollectionAssert.Contains(piniaEntryNames, "jazor/pinia/licenses/NOSTICS-LICENSE");
        StringAssert.Contains(piniaNuspec, "<dependency id=\"Jazor\"");
        StringAssert.Contains(piniaNuspec, "<dependency id=\"ECMAScript.Vue\"");

        using var piniaTestingArchive = ZipFile.OpenRead(package.PiniaTestingPackagePath);
        var piniaTestingEntryNames = piniaTestingArchive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();
        var piniaTestingNuspec = ReadPackageEntryText(package.PiniaTestingPackagePath, "ECMAScript.Pinia.Testing.nuspec");
        CollectionAssert.Contains(piniaTestingEntryNames, "lib/net11.0/ECMAScript.Pinia.Testing.dll");
        CollectionAssert.Contains(piniaTestingEntryNames, "ECMAScript.Pinia.Testing.nuspec");
        CollectionAssert.Contains(piniaTestingEntryNames, "buildTransitive/ECMAScript.Pinia.Testing.targets");
        CollectionAssert.Contains(piniaTestingEntryNames, "jazor/pinia-testing/manifest.json");
        Assert.IsFalse(
            piniaTestingEntryNames.Any(static path => path.StartsWith("jazor/pinia-testing/dist/", StringComparison.OrdinalIgnoreCase)),
            "Pinia Testing runtime modules belong to the restored npm package, not the binding archive.");
        CollectionAssert.Contains(piniaTestingEntryNames, "jazor/pinia-testing/licenses/LICENSE");
        StringAssert.Contains(piniaTestingNuspec, "<dependency id=\"ECMAScript.Pinia\"");
        StringAssert.Contains(piniaTestingNuspec, "<dependency id=\"ECMAScript.Vue\"");
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
    public async Task CreateLocalPackage_IncludesSelfContainedBrowserAssets()
    {
        var package = await LocalPackage.Value;

        AssertPackageDoesNotContain(
            package.PackagePath,
            "lib/net11.0/ECMAScript.Vue.dll",
            "lib/net11.0/ECMAScript.VueContract.dll",
            "lib/net11.0/ECMAScript.Blazor.dll",
            "jazor/vue3/manifest.json",
            "tools/net11.0/tooling/vue/compiler-sfc.esm-browser.js");
        AssertPackageEntries(
            package.VuePackagePath,
            "lib/net11.0/ECMAScript.Vue.dll",
            "jazor/vue3/manifest.json",
            "jazor/vue3/licenses/LICENSE",
            "jazor/vue3/licenses/VUE-DEVTOOLS-API-LICENSE",
            "jazor/vue3/licenses/PERFECT-DEBOUNCE-LICENSE",
            "jazor/vue3/licenses/VUE-SERVER-RENDERER-LICENSE");
        AssertPackageEntries(
            package.PackagePath,
            "jazor/ecmascript/manifest.json",
            "jazor/ecmascript/clr/System/RuntimeModule.js",
            "jazor/ecmascript/clr/System/StringModule.js");
        AssertPackageEntries(
            package.VueRoutePackagePath,
            "lib/net11.0/ECMAScript.VueRoute.dll",
            "buildTransitive/ECMAScript.VueRoute.targets",
            "jazor/vue-router/manifest.json",
            "jazor/vue-router/licenses/NOSTICS-LICENSE",
            "jazor/vue-router/licenses/LICENSE");
        AssertPackageEntries(
            package.PiniaPackagePath,
            "lib/net11.0/ECMAScript.Pinia.dll",
            "buildTransitive/ECMAScript.Pinia.targets",
            "jazor/pinia/manifest.json",
            "jazor/pinia/licenses/NOSTICS-LICENSE",
            "jazor/pinia/licenses/LICENSE");
        AssertPackageEntries(
            package.PiniaTestingPackagePath,
            "lib/net11.0/ECMAScript.Pinia.Testing.dll",
            "buildTransitive/ECMAScript.Pinia.Testing.targets",
            "jazor/pinia-testing/manifest.json",
            "jazor/pinia-testing/licenses/LICENSE");

        foreach (var (packagePath, libraryId) in new[]
                 {
                     (package.VuePackagePath, "vue3"),
                     (package.VueRoutePackagePath, "vue-router"),
                     (package.PiniaPackagePath, "pinia"),
                     (package.PiniaTestingPackagePath, "pinia-testing"),
                 })
        {
            using var bindingArchive = ZipFile.OpenRead(packagePath);
            Assert.IsFalse(
                bindingArchive.Entries.Any(entry =>
                    entry.FullName.Replace('\\', '/').StartsWith($"jazor/{libraryId}/dist/", StringComparison.OrdinalIgnoreCase)),
                $"Binding package '{libraryId}' must not carry a historical dist snapshot.");
        }
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
    public async Task CreateLocalPackage_VueDevtoolsApi_DeclaresVueRouterAndPiniaDevelopmentImports()
    {
        var package = await LocalPackage.Value;
        using var manifest = JsonDocument.Parse(ReadPackageEntryText(package.VuePackagePath, "jazor/vue3/manifest.json"));

        var devtools = manifest.RootElement.GetProperty("imports").GetProperty("@vue/devtools-api");
        Assert.AreEqual("@vue/devtools-api", devtools.GetProperty("path").GetString());
        Assert.AreEqual("@vue/devtools-api", devtools.GetProperty("path").GetString());
        CollectionAssert.AreEquivalent(
            new[] { "perfect-debounce" },
            devtools.GetProperty("dependencies").EnumerateArray().Select(static value => value.GetString()).ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "perfect-debounce" },
            devtools.GetProperty("dependencies").EnumerateArray().Select(static value => value.GetString()).ToArray());
        var serverRenderer = manifest.RootElement.GetProperty("imports").GetProperty("@vue/server-renderer");
        Assert.AreEqual("@vue/server-renderer", serverRenderer.GetProperty("path").GetString());
        Assert.AreEqual("@vue/server-renderer", serverRenderer.GetProperty("path").GetString());
        using (var vueArchive = ZipFile.OpenRead(package.VuePackagePath))
        {
            Assert.IsFalse(
                vueArchive.Entries.Any(entry =>
                    entry.FullName.Replace('\\', '/').StartsWith("jazor/vue3/dist/", StringComparison.OrdinalIgnoreCase)));
        }

        using var routerManifest = JsonDocument.Parse(ReadPackageEntryText(package.VueRoutePackagePath, "jazor/vue-router/manifest.json"));
        var routerEntry = routerManifest.RootElement.GetProperty("imports").GetProperty("vue-router");
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            routerEntry.GetProperty("dependencies").EnumerateArray().Select(static value => value.GetString()).ToArray());
        using var piniaManifest = JsonDocument.Parse(ReadPackageEntryText(package.PiniaPackagePath, "jazor/pinia/manifest.json"));
        var piniaEntry = piniaManifest.RootElement.GetProperty("imports").GetProperty("pinia");
        CollectionAssert.AreEquivalent(
            new[] { "vue" },
            piniaEntry.GetProperty("dependencies").EnumerateArray().Select(static value => value.GetString()).ToArray());

        using (var piniaArchive = ZipFile.OpenRead(package.PiniaPackagePath))
        {
            Assert.IsFalse(
                piniaArchive.Entries.Any(entry =>
                    entry.FullName.Replace('\\', '/').StartsWith("jazor/pinia/dist/", StringComparison.OrdinalIgnoreCase)));
        }
    }

    [TestMethod]
    [TestCategory("Consumer.Style")]
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
        StringAssert.Contains(
            nuspec,
            "<dependency id=\"Jazor\" version=\"[" + package.PackageVersion + "]\" exclude=\"Build,Analyzers\" />");
        Assert.IsFalse(
            entryNames.Any(static path => path.StartsWith("build", StringComparison.OrdinalIgnoreCase)),
            "ECMAScript.Style must rely on Jazor's existing build integration and must not install CSS-specific targets.");
    }

    [TestMethod]
    [TestCategory("Consumer.Style")]
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
            $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
            $"-p:JazorPackageVersion={package.PackageVersion}"
        };

        var debugBuild = await RunDotNetAsync(
            package.RepoRoot,
            ["build", projectPath, .. commonArguments, "-p:JazorMode=debug"]);
        Assert.AreEqual(0, debugBuild.ExitCode, debugBuild.ToString());

        var outputRoot = Path.Combine(projectRoot, "jazor");
        var runtimePath = Path.Combine(outputRoot, "style.js");
        var runtimeMapPath = runtimePath + ".map";
        var appPath = Path.Combine(outputRoot, "app.mjs");
        var manifestPath = FindBuildState(outputRoot);

        Assert.IsTrue(File.Exists(runtimePath), $"ECMAScript.Style runtime was not materialized: {runtimePath}");
        Assert.IsTrue(File.Exists(runtimeMapPath), $"ECMAScript.Style source map was not materialized: {runtimeMapPath}");
        Assert.IsTrue(File.Exists(appPath), $"Consumer module was not materialized: {appPath}");
        Assert.IsTrue(File.Exists(manifestPath), $"Debug manifest was not generated: {manifestPath}");
        var appModule = await File.ReadAllTextAsync(appPath);
        StringAssert.Contains(appModule, "from \"./style.js\"");
        StringAssert.Contains(appModule, "\"background-color\": hex(\"1769aa\")");
        StringAssert.Contains(appModule, "context({");
        Assert.IsFalse(appModule.Contains("context as ", StringComparison.Ordinal), appModule);
        StringAssert.Contains(appModule, "styleIn");
        StringAssert.Contains(appModule, "atRuleIn");
        StringAssert.Contains(appModule, "snapshotFrom");

        var manifest = LoadManifest(manifestPath);
        var runtimeEntry = manifest.Modules.Single(static entry => entry.RelativePath == "style.js");
        Assert.AreEqual("style.js.map", runtimeEntry.SourceMapPath);
        Assert.HasCount(64, runtimeEntry.Hash);
        Assert.HasCount(64, runtimeEntry.MapHash!);

        var releaseBuild = await RunDotNetAsync(
            package.RepoRoot,
            ["build", projectPath, .. commonArguments, "-p:JazorMode=release"]);
        Assert.AreEqual(0, releaseBuild.ExitCode, releaseBuild.ToString());

        var bundlePath = Path.Combine(outputRoot, "dist", "bundle.js");
        var bundleMapPath = Path.Combine(outputRoot, "dist", "bundle.js.map");
        Assert.IsTrue(File.Exists(bundlePath), $"ECMAScript.Style bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(bundleMapPath), $"ECMAScript.Style bundle source map was not generated: {bundleMapPath}");
        Assert.IsTrue(File.Exists(runtimePath), "Release builds consume the same project source tree.");
        Assert.IsTrue(File.Exists(manifestPath), "Emit build state remains under obj.");

        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(bundle, "ecmascript-style:v1");
        StringAssert.Contains(bundle, "ecs-");
        StringAssert.Contains(bundle, "font-face");
        StringAssert.Contains(bundle, "server-css");
        StringAssert.Contains(bundle, "sourceMappingURL=bundle.js.map");
        Assert.IsFalse(bundle.Contains("from \"./style.js\"", StringComparison.Ordinal), bundle);
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_MultiProjectSample_ReleasePreservesNamedExports()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);
        var restorePackagesPath = workspace.RestorePackagesPath;

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
        var debugManifestPath = Path.Combine(hostRoot, "jazor", "jazor-manifest.json");
        var bundlePath = Path.Combine(hostRoot, "jazor", "dist", "bundle.js");

        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsFalse(
            File.Exists(debugManifestPath),
            $"Bundle must not materialize debug artifacts: {debugManifestPath}");

        var bundle = await File.ReadAllTextAsync(bundlePath);

        // Minification may rename internals; the declared public export remains callable.
        StringAssert.Contains(bundle, "Hello");
        StringAssert.Contains(bundle, "Jazor");
        var testFile = Path.Combine(hostRoot, "jazor", "bundle-test.mjs");
        await File.WriteAllTextAsync(testFile, """
            import { Boot } from './dist/bundle.js';
            globalThis.__jazorTest('project bundle preserves named exports', () => {
              if (Boot() !== 'Hello, Jazor') throw new Error(Boot());
            });
            """);
        await RunNodeTestAsync( testFile, Path.Combine(hostRoot, "jazor"));
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_ReleaseWithSsrEnabled_MaterializesRawModuleGraph()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "SsrReleaseSdkSample");
        var projectPath = CreateDefaultOutputStaticHostProject(projectRoot, includeVue: true);
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
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=release",
                "-p:JazorSSR=true"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var browserRoot = Path.Combine(projectRoot, "jazor");
        var ssrRoot = browserRoot;
        Assert.IsTrue(File.Exists(Path.Combine(browserRoot, "dist", "bundle.js")), "Release browser bundle was not generated.");
        Assert.IsFalse(File.Exists(Path.Combine(ssrRoot, "jazor-manifest.json")), "SSR application manifest was not generated.");
        Assert.IsTrue(File.Exists(Path.Combine(ssrRoot, "host", "app.mjs")), "SSR raw module graph was not generated.");
        Assert.IsFalse(File.Exists(Path.Combine(ssrRoot, "importmap.json")), "SSR browser import map was not generated.");
        Assert.IsFalse(File.Exists(Path.Combine(ssrRoot, "ssr-importmap.json")), "SSR local import map was not generated.");
        Assert.IsFalse(File.Exists(Path.Combine(ssrRoot, "manifest.json")), "SSR asset manifest was not generated.");
        Assert.IsTrue(
            File.Exists(Path.Combine(browserRoot, "node_modules", "@vue", "server-renderer", "package.json")),
            "SSR server renderer must be restored in the shared node_modules graph.");
        Assert.IsTrue(
            File.Exists(Path.Combine(browserRoot, "node_modules", "vue", "package.json")),
            "Vue must be restored in the shared node_modules graph.");
        Assert.IsFalse(
            Directory.Exists(Path.Combine(browserRoot, "vendor")),
            "External npm packages must not be copied into a binding-owned vendor snapshot.");

        Assert.IsTrue(File.Exists(Path.Combine(ssrRoot, "ssr-entry.js")));
        Assert.IsTrue(File.Exists(Path.Combine(browserRoot, "package.json")), "The shared package project was not emitted.");
        Assert.IsTrue(Directory.Exists(Path.Combine(browserRoot, "node_modules")), "The shared pnpm node_modules graph was not restored.");
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Publish_LocalJazorPackage_WebSdkHost_ReleaseWithSsrEnabled_CopiesRuntimeBundles()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "SsrPublishSdkSample");
        var publishOutputRoot = Path.Combine(workspace.RootPath, "publish-output");
        var projectPath = CreateDefaultOutputWebHostProject(projectRoot, includeVue: true);
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
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=release",
                "-p:JazorSSR=true"
            ]);

        Assert.AreEqual(0, publish.ExitCode, publish.ToString());

        var publishedSsrRoot = Path.Combine(publishOutputRoot, "jazor");
        Assert.IsFalse(File.Exists(Path.Combine(publishedSsrRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedSsrRoot, "dist", "bundle.js")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedSsrRoot, "ssr", "ssr-entry.js")));
        Assert.IsFalse(Directory.Exists(Path.Combine(publishedSsrRoot, "host")));
        Assert.IsFalse(File.Exists(Path.Combine(publishedSsrRoot, "importmap.json")));
        Assert.IsFalse(File.Exists(Path.Combine(publishedSsrRoot, "ssr-importmap.json")));
        Assert.IsTrue(
            File.Exists(Path.Combine(publishedSsrRoot, "ssr", "package.json")),
            "Publish output must carry the bundled SSR task package.");
        Assert.IsFalse(File.Exists(Path.Combine(publishedSsrRoot, "package.json")));
        Assert.IsFalse(File.Exists(Path.Combine(publishedSsrRoot, "pnpm-lock.yaml")));
        Assert.IsFalse(Directory.Exists(Path.Combine(publishedSsrRoot, "node_modules")));
        Assert.IsFalse(Directory.EnumerateFiles(publishedSsrRoot, "*.map", SearchOption.AllDirectories).Any());
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_SingleProjectWrapperApis_EmitsMinimalRuntimeImports()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var restorePackagesPath = workspace.RestorePackagesPath;

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

        var outputRoot = Path.Combine(hostRoot, "jazor");
        var manifestPath = FindBuildState(outputRoot);
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
            "import { _e2640560d207afce } from \"../clr/System/DateOnlyModule.js\";",
            GetImportLine(module, "../clr/System/DateOnlyModule.js"));
        Assert.AreEqual(
            "import { _25187a24d190d864, _e856edbfd7db0646 } from \"../clr/System/DateTimeOffsetModule.js\";",
            GetImportLine(module, "../clr/System/DateTimeOffsetModule.js"));
        var decimalImport = GetImportLine(module, "../clr/System/DecimalModule.js");
        StringAssert.Contains(decimalImport, "_01be2a34fe2cda4e");
        StringAssert.Contains(decimalImport, "_b1e6a06111674f0c");

        var cultureInfoImport = GetImportLine(module, "../clr/System/Globalization/CultureInfoModule.js");
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

        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/DecimalModule.js");
        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/Globalization/CultureInfoModule.js");
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_StoredIndexAndRange_ExecutesMaterializedRuntimeOnNodeApi()
    {
        var package = await LocalCorePackage.Value;

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
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(hostRoot, "jazor");
        var manifestPath = FindBuildState(outputRoot);
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");

        var modulePath = Path.Combine(outputRoot, "host", "app.mjs");
        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");
        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");

        var indexImport = GetImportLine(module, "../clr/System/IndexModule.js");
        StringAssert.Contains(indexImport, "_ce8b9229a41c8545");
        StringAssert.Contains(indexImport, "_9b817e75f3f8f58f");

        var rangeImport = GetImportLine(module, "../clr/System/RangeModule.js");
        StringAssert.Contains(rangeImport, "_fc3dfc5dbaa397eb");
        StringAssert.Contains(rangeImport, "_1c7a1e658ed790ff");

        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/IndexModule.js");
        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/RangeModule.js");
        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/RuntimeModule.js");

        AssertStandardNodeProject(outputRoot);
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

            globalThis.__jazorTest("materialized stored Index and Range preserve array offsets and mutation", () => {
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

        await RunNodeTestAsync( testFile, outputRoot);
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_QuerySyntaxWithCapturedLambda_ExecutesOnNodeApi()
    {
        var package = await LocalCorePackage.Value;

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
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(hostRoot, "jazor");
        var manifestPath = FindBuildState(outputRoot);
        var modulePath = Path.Combine(outputRoot, "host", "app.mjs");
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");

        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(module, "return __src.filter(__callback);");
        StringAssert.Contains(module, "return Array.from(__src).map(__callback);");
        StringAssert.Contains(module, "clr/System/Linq/EnumerableModule.js");
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

        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/Linq/EnumerableModule.js");
        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/Collections/Generic/ComparerT1Module.js");
        await AssertResourceImportIsMaterializedAsync(outputRoot, "clr/System/Collections/Generic/EqualityComparerT1Module.js");

        AssertStandardNodeProject(outputRoot);

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

            globalThis.__jazorTest("materialized query syntax preserves lambda capture and source values", () => {
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

        await RunNodeTestAsync( testFile, outputRoot);
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_StaticHost_UsesProjectJazorByDefault()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "StaticHostDefaultBuildSample");
        var restorePackagesPath = workspace.RestorePackagesPath;
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

        var projectJazorRoot = Path.Combine(projectRoot, "jazor");
        var webRootJazor = Path.Combine(projectRoot, "wwwroot", "jazor");
        Assert.IsFalse(File.Exists(Path.Combine(projectJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(projectJazorRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(projectJazorRoot, "host", "app.mjs.map")));
        var module = await File.ReadAllTextAsync(Path.Combine(projectJazorRoot, "host", "app.mjs"));
        StringAssert.Contains(module, "sourceMappingURL=app.mjs.map");
        Assert.IsFalse(Directory.Exists(webRootJazor), $"Build must not materialize artifacts under '{webRootJazor}'.");
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_ClassLibraryWithGlobalEmitMode_OnlyCarriesModuleCatalog()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "CatalogOnlyClassLibrary");
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "CatalogOnlyClassLibrary.csproj");
        WriteFile(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(projectRoot, "LibraryModule.cs"),
            """
            using ECMAScript;

            namespace CatalogOnlyClassLibrary;

            [ECMAScriptModule("library/module.mjs")]
            public static class LibraryModule
            {
                public static int Value() => 1;
            }
            """);

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
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=debug"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());
        var generatedSource = ReadSingleGeneratedSource(
            Path.Combine(projectRoot, "obj"),
            "Jazor.Generated.ModuleCatalog.g.cs");
        StringAssert.Contains(generatedSource, "internal static partial class ModuleCatalog", StringComparison.Ordinal);
        Assert.IsFalse(
            Directory.Exists(Path.Combine(projectRoot, "jazor")),
            "A class library must stop at its assembly-contained ModuleCatalog even when JazorMode is globally inherited.");
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_IndirectNuGetReferenceDoesNotActivateTooling()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var feedRoot = Path.Combine(workspace.RootPath, "feed");
        var libraryRoot = Path.Combine(workspace.RootPath, "DirectToolLibrary");
        var hostRoot = Path.Combine(workspace.RootPath, "IndirectToolHost");
        Directory.CreateDirectory(feedRoot);
        Directory.CreateDirectory(libraryRoot);
        Directory.CreateDirectory(hostRoot);
        File.Copy(package.PackagePath, Path.Combine(feedRoot, Path.GetFileName(package.PackagePath)));

        var libraryProject = Path.Combine(libraryRoot, "DirectToolLibrary.csproj");
        WriteFile(
            libraryProject,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <PackageId>DirectToolLibrary</PackageId>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(libraryRoot, "LibraryModule.cs"),
            """
            using ECMAScript;

            namespace DirectToolLibrary;

            [ECMAScriptModule("libraries/direct-tool.mjs")]
            public static class LibraryModule
            {
                public static int Value() => 41;
            }
            """);

        var pack = await RunDotNetAsync(
            package.RepoRoot,
            [
                "pack",
                libraryProject,
                "-c",
                "Debug",
                "-o",
                feedRoot,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={feedRoot}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:PackageVersion=1.0.0"
            ]);
        Assert.AreEqual(0, pack.ExitCode, pack.ToString());
        var libraryPackagePath = Path.Combine(feedRoot, "DirectToolLibrary.1.0.0.nupkg");
        Assert.IsTrue(File.Exists(libraryPackagePath), libraryPackagePath);
        StringAssert.Contains(
            ReadPackageEntryText(libraryPackagePath, "DirectToolLibrary.nuspec"),
            "<dependency id=\"Jazor\"",
            StringComparison.Ordinal);

        var hostProject = Path.Combine(hostRoot, "IndirectToolHost.csproj");
        WriteFile(
            hostProject,
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
                <PackageReference Include="DirectToolLibrary" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(hostRoot, "Program.cs"),
            """
            using DirectToolLibrary;

            Console.WriteLine(LibraryModule.Value());
            """);

        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                hostProject,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={feedRoot}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());
        StringAssert.Contains(
            ReadSingleGeneratedSource(Path.Combine(libraryRoot, "obj"), "Jazor.Generated.ModuleCatalog.g.cs"),
            "relativePath: \"libraries/direct-tool.mjs\"",
            StringComparison.Ordinal);
        Assert.IsFalse(
            Directory.Exists(Path.Combine(hostRoot, "jazor")),
            "An indirect Jazor dependency must not grant Emit to the host.");
        Assert.IsFalse(
            Directory.EnumerateFiles(hostRoot, "Jazor.Generated.ModuleCatalog.g.cs", SearchOption.AllDirectories).Any(),
            "An indirect Jazor dependency must not activate the compiler generator in the host.");

        var generatedTargetsPath = Path.Combine(hostRoot, "obj", "IndirectToolHost.csproj.nuget.g.targets");
        Assert.IsTrue(File.Exists(generatedTargetsPath), generatedTargetsPath);
        var generatedTargets = File.ReadAllText(generatedTargetsPath).Replace('\\', '/');
        Assert.IsFalse(
            generatedTargets.Contains("build/Jazor.targets", StringComparison.Ordinal),
            "The direct-only target must not be imported by a project that only consumes a library using Jazor.");
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_ProjectReferenceChain_OnlyFinalHostMaterializesTransitiveModuleCatalog()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var providerRoot = Path.Combine(workspace.RootPath, "ProviderLibrary");
        var intermediaryRoot = Path.Combine(workspace.RootPath, "IntermediaryLibrary");
        var hostRoot = Path.Combine(workspace.RootPath, "FinalHost");
        Directory.CreateDirectory(providerRoot);
        Directory.CreateDirectory(intermediaryRoot);
        Directory.CreateDirectory(hostRoot);

        var providerProject = Path.Combine(providerRoot, "ProviderLibrary.csproj");
        WriteFile(
            providerProject,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" PrivateAssets="all" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(providerRoot, "ProviderModule.cs"),
            """
            using ECMAScript;

            namespace ProviderLibrary;

            [ECMAScriptModule("libraries/provider.mjs")]
            public static class ProviderModule
            {
                public static int Value() => 41;
            }
            """);

        var intermediaryProject = Path.Combine(intermediaryRoot, "IntermediaryLibrary.csproj");
        WriteFile(
            intermediaryProject,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>

              <ItemGroup>
                <ProjectReference Include="..\ProviderLibrary\ProviderLibrary.csproj" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(intermediaryRoot, "ReferenceAnchor.cs"),
            """
            using ProviderLibrary;

            namespace IntermediaryLibrary;

            public static class ReferenceAnchor
            {
                public static int Validate() => ProviderModule.Value();
            }
            """);

        var hostProject = Path.Combine(hostRoot, "FinalHost.csproj");
        WriteFile(
            hostProject,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <ProjectReference Include="..\IntermediaryLibrary\IntermediaryLibrary.csproj" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(hostRoot, "Program.cs"),
            """
            using IntermediaryLibrary;

            Console.WriteLine(ReferenceAnchor.Validate());
            """);
        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using ECMAScript;
            using ProviderLibrary;

            namespace FinalHost;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static int Boot() => ProviderModule.Value() + 1;
            }
            """);

        var build = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                hostProject,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=debug"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());
        StringAssert.Contains(
            ReadSingleGeneratedSource(Path.Combine(providerRoot, "obj"), "Jazor.Generated.ModuleCatalog.g.cs"),
            "relativePath: \"libraries/provider.mjs\"",
            StringComparison.Ordinal);
        var hostCatalog = ReadSingleGeneratedSource(
            Path.Combine(hostRoot, "obj"),
            "Jazor.Generated.ModuleCatalog.g.cs");
        StringAssert.Contains(
            hostCatalog,
            "dependencies: new string[] { \"libraries/provider.mjs\" }",
            StringComparison.Ordinal);

        Assert.IsFalse(Directory.Exists(Path.Combine(providerRoot, "jazor")));
        Assert.IsFalse(Directory.Exists(Path.Combine(intermediaryRoot, "jazor")));

        var outputRoot = Path.Combine(hostRoot, "jazor");
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "libraries", "provider.mjs")));
        var manifest = LoadManifest(FindBuildState(outputRoot));
        CollectionAssert.AreEquivalent(
            new[] { "host/app.mjs", "libraries/provider.mjs" },
            manifest.Modules.Select(static module => module.RelativePath).ToArray());
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Build_LocalJazorPackage_NuGetLibraryChain_OnlyFinalHostMaterializesTransitiveModuleCatalog()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var feedRoot = Path.Combine(workspace.RootPath, "feed");
        var providerRoot = Path.Combine(workspace.RootPath, "CarrierProvider");
        var intermediaryRoot = Path.Combine(workspace.RootPath, "CarrierIntermediary");
        var hostRoot = Path.Combine(workspace.RootPath, "CarrierHost");
        Directory.CreateDirectory(feedRoot);
        Directory.CreateDirectory(providerRoot);
        Directory.CreateDirectory(intermediaryRoot);
        Directory.CreateDirectory(hostRoot);
        File.Copy(package.PackagePath, Path.Combine(feedRoot, Path.GetFileName(package.PackagePath)));

        var providerProject = Path.Combine(providerRoot, "CarrierProvider.csproj");
        WriteFile(
            providerProject,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <PackageId>CarrierProvider</PackageId>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" PrivateAssets="all" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(providerRoot, "ProviderModule.cs"),
            """
            using ECMAScript;

            namespace CarrierProvider;

            [ECMAScriptModule("libraries/nuget-provider.mjs")]
            public static class ProviderModule
            {
                public static int Value() => 41;
            }
            """);

        var packProvider = await RunDotNetAsync(
            package.RepoRoot,
            [
                "pack",
                providerProject,
                "-c",
                "Debug",
                "-o",
                feedRoot,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={feedRoot}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:PackageVersion=1.0.0"
            ]);
        Assert.AreEqual(0, packProvider.ExitCode, packProvider.ToString());

        var providerPackagePath = Path.Combine(feedRoot, "CarrierProvider.1.0.0.nupkg");
        Assert.IsTrue(File.Exists(providerPackagePath));
        var providerNuspec = ReadPackageEntryText(providerPackagePath, "CarrierProvider.nuspec");
        Assert.IsFalse(
            providerNuspec.Contains("<dependency id=\"Jazor\"", StringComparison.Ordinal),
            "A library's direct Jazor tooling reference must not become a consumer tool dependency.");

        var intermediaryProject = Path.Combine(intermediaryRoot, "CarrierIntermediary.csproj");
        WriteFile(
            intermediaryProject,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <PackageId>CarrierIntermediary</PackageId>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="CarrierProvider" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(intermediaryRoot, "ReferenceAnchor.cs"),
            """
            using CarrierProvider;

            namespace CarrierIntermediary;

            public static class ReferenceAnchor
            {
                public static int Validate() => ProviderModule.Value();
            }
            """);

        var packIntermediary = await RunDotNetAsync(
            package.RepoRoot,
            [
                "pack",
                intermediaryProject,
                "-c",
                "Debug",
                "-o",
                feedRoot,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={feedRoot}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                "-p:PackageVersion=1.0.0"
            ]);
        Assert.AreEqual(0, packIntermediary.ExitCode, packIntermediary.ToString());

        var intermediaryPackagePath = Path.Combine(feedRoot, "CarrierIntermediary.1.0.0.nupkg");
        Assert.IsTrue(File.Exists(intermediaryPackagePath));
        var intermediaryNuspec = ReadPackageEntryText(intermediaryPackagePath, "CarrierIntermediary.nuspec");
        StringAssert.Contains(
            intermediaryNuspec,
            "<dependency id=\"CarrierProvider\" version=\"1.0.0\" exclude=\"Build,Analyzers\"",
            StringComparison.Ordinal);
        Assert.IsFalse(intermediaryNuspec.Contains("id=\"Jazor\"", StringComparison.Ordinal));

        var hostProject = Path.Combine(hostRoot, "CarrierHost.csproj");
        WriteFile(
            hostProject,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="CarrierIntermediary" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(hostRoot, "Program.cs"),
            """
            using CarrierIntermediary;

            Console.WriteLine(ReferenceAnchor.Validate());
            """);
        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using CarrierProvider;
            using ECMAScript;

            namespace CarrierHost;

            [ECMAScriptModule("host/nuget-app.mjs")]
            public static class AppModule
            {
                public static int Boot() => ProviderModule.Value() + 1;
            }
            """);

        var buildHost = await RunDotNetAsync(
            package.RepoRoot,
            [
                "build",
                hostProject,
                "-t:Rebuild",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={feedRoot}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={workspace.RestorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorMode=debug"
            ]);

        Assert.AreEqual(0, buildHost.ExitCode, buildHost.ToString());
        Assert.IsFalse(Directory.Exists(Path.Combine(providerRoot, "jazor")));
        Assert.IsFalse(Directory.Exists(Path.Combine(intermediaryRoot, "jazor")));

        var hostCatalog = ReadSingleGeneratedSource(
            Path.Combine(hostRoot, "obj"),
            "Jazor.Generated.ModuleCatalog.g.cs");
        StringAssert.Contains(
            hostCatalog,
            "dependencies: new string[] { \"libraries/nuget-provider.mjs\" }",
            StringComparison.Ordinal);
        var outputRoot = Path.Combine(hostRoot, "jazor");
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "host", "nuget-app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "libraries", "nuget-provider.mjs")));
        var manifest = LoadManifest(FindBuildState(outputRoot));
        CollectionAssert.AreEquivalent(
            new[] { "host/nuget-app.mjs", "libraries/nuget-provider.mjs" },
            manifest.Modules.Select(static module => module.RelativePath).ToArray());
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Publish_LocalJazorPackage_StaticHost_UsesProjectAndPublishJazorByDefault()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "StaticHostDefaultPublishSample");
        var restorePackagesPath = workspace.RestorePackagesPath;
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

        var projectJazorRoot = Path.Combine(projectRoot, "jazor");
        var projectWebRootJazor = Path.Combine(projectRoot, "wwwroot", "jazor");
        var publishOutputRoot = Path.Combine(projectRoot, "bin", "Debug", "net11.0", "publish");
        var publishJazorRoot = Path.Combine(publishOutputRoot, "jazor");
        var publishWebRootJazor = Path.Combine(publishOutputRoot, "wwwroot", "jazor");
        Assert.IsFalse(File.Exists(Path.Combine(projectJazorRoot, "jazor-manifest.json")));
        Assert.IsFalse(File.Exists(Path.Combine(publishJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishJazorRoot, "dist", "bundle.js")));
        Assert.IsFalse(Directory.Exists(Path.Combine(publishJazorRoot, "host")));
        Assert.IsFalse(Directory.Exists(Path.Combine(publishJazorRoot, "node_modules")));
        Assert.IsFalse(Directory.EnumerateFiles(publishJazorRoot, "*.map", SearchOption.AllDirectories).Any());
        Assert.IsFalse(Directory.Exists(projectWebRootJazor), $"Build must not materialize artifacts under '{projectWebRootJazor}'.");
        Assert.IsFalse(Directory.Exists(publishWebRootJazor), $"Publish must not materialize artifacts under '{publishWebRootJazor}'.");
    }

    [TestMethod]
    [TestCategory("Consumer.Core")]
    public async Task Publish_LocalJazorPackage_WebSdkHost_MaterializesJazorAssetsIntoPublishOutput()
    {
        var package = await LocalCorePackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "WebSdkPublishSample");
        var publishOutputRoot = Path.Combine(workspace.RootPath, "publish-output");
        var restorePackagesPath = workspace.RestorePackagesPath;
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

        var sourceJazorRoot = Path.Combine(projectRoot, "jazor");
        var sourceWebRootJazor = Path.Combine(projectRoot, "wwwroot", "jazor");
        var publishedJazorRoot = Path.Combine(publishOutputRoot, "jazor");
        var publishedWebRootJazor = Path.Combine(publishOutputRoot, "wwwroot", "jazor");

        Assert.IsFalse(File.Exists(Path.Combine(sourceJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceJazorRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceJazorRoot, "host", "app.mjs.map")));
        Assert.IsFalse(File.Exists(Path.Combine(publishedJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "dist", "bundle.js")));
        Assert.IsFalse(Directory.Exists(Path.Combine(publishedJazorRoot, "host")));
        Assert.IsFalse(Directory.Exists(Path.Combine(publishedJazorRoot, "node_modules")));
        Assert.IsFalse(Directory.EnumerateFiles(publishedJazorRoot, "*.map", SearchOption.AllDirectories).Any());
        Assert.IsFalse(
            Directory.Exists(sourceWebRootJazor),
            $"Build must not materialize artifacts under '{sourceWebRootJazor}'.");
        Assert.IsFalse(
            Directory.Exists(publishedWebRootJazor),
            $"Publish output must not materialize artifacts under '{publishedWebRootJazor}'.");
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
    public async Task Build_LocalJazorPackage_WithVueRouteAuthoring_EmitsVueRouterImportsAndRouteObjects()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "VueRouteSdkSample");
        var restorePackagesPath = workspace.RestorePackagesPath;

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

        var outputRoot = Path.Combine(projectRoot, "jazor");
        var manifestPath = FindBuildState(outputRoot);
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
    [TestCategory("Consumer.Packages")]
    public async Task Build_LocalJazorPackage_WithVueRouteInjectionAndReactiveAuthoring_EmitsTypedVueRouterContracts()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "VueRouteReactiveSdkSample");
        var restorePackagesPath = workspace.RestorePackagesPath;

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
            using static ECMAScript.Vue;
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

        var outputRoot = Path.Combine(projectRoot, "jazor");
        var manifestPath = FindBuildState(outputRoot);
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
        StringAssert.Contains(module, "(typeof injectedDepth === \"number\" ? injectedDepth : null).toString()");
        StringAssert.Contains(module, "link.href.value");
        StringAssert.Contains(module, "link.route.value.href");

        var emittedRelativePaths = LoadManifest(manifestPath).Modules
            .Select(static moduleEntry => moduleEntry.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        CollectionAssert.Contains(emittedRelativePaths, "host/app.mjs");
    }

    [TestMethod]
    [TestCategory("Consumer.Packages")]
    public async Task Build_LocalJazorPackage_WithVueRouteReactiveAuthoring_BundlesThroughProjectScript_AndResolvesVuePackages()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "VueRouteReactiveBundleSdkSample");
        var restorePackagesPath = workspace.RestorePackagesPath;

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
            using static ECMAScript.Vue;
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

        var moduleRoot = Path.Combine(projectRoot, "jazor");
        var manifestPath = FindBuildState(moduleRoot);
        var modulePath = Path.Combine(moduleRoot, "host", "app.mjs");
        var bundlePath = Path.Combine(moduleRoot, "dist", "bundle.js");
        var bundleSourceMapPath = Path.Combine(moduleRoot, "dist", "bundle.js.map");

        Assert.IsTrue(File.Exists(manifestPath), $"Build state was not written under obj: {manifestPath}");
        Assert.IsTrue(File.Exists(modulePath), $"Release project source is missing: {modulePath}");
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

    private static async Task<LocalStylePackageFixture> CreateLocalStylePackageAsync()
    {
        var core = await LocalCorePackage.Value;
        var packageOutputDirectory = CreateFixtureDirectory(core.RepoRoot, "debug-style-nupkg");
        CopyPackageFeed(core.PackageOutputDirectory, packageOutputDirectory);

        var result = await core.BuildWorkspace.PackAsync("ECMAScript.Style", packageOutputDirectory);
        Assert.AreEqual(0, result.ExitCode, result.ToString());
        Assert.IsTrue(
            File.Exists(Path.Combine(
                core.BuildWorkspace.OutputRoot,
                "ECMAScript.Style",
                "bin",
                "Debug",
                "net11.0",
                "ECMAScript.Style.dll")),
            "ECMAScript.Style package preparation did not build ECMAScript.Style.");

        return new LocalStylePackageFixture(
            core.RepoRoot,
            core.PackageVersion,
            packageOutputDirectory,
            GetPackagePath(packageOutputDirectory, "ECMAScript.Style", core.PackageVersion));
    }

    private static string CreateFixtureDirectory(string repoRoot, string purpose)
    {
        var path = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", purpose, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static async Task<string> ResolvePackageVersionAsync(string repoRoot)
    {
        var result = await RunDotNetAsync(
            repoRoot,
            [
                "msbuild",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                "-restore",
                "-t:MinVer",
                "-getProperty:PackageVersion",
                "-nologo",
                "/nr:false"
            ]);
        Assert.AreEqual(0, result.ExitCode, result.ToString());

        var output = result.StandardOutput.Trim();
        string? packageVersion;
        if (output.StartsWith('{'))
        {
            using var document = JsonDocument.Parse(output);
            packageVersion = document.RootElement
                .GetProperty("Properties")
                .GetProperty("PackageVersion")
                .GetString();
        }
        else
        {
            packageVersion = output;
        }

        return !string.IsNullOrWhiteSpace(packageVersion)
            ? packageVersion
            : throw new InvalidOperationException("MinVer did not return PackageVersion.");
    }

    private static void CopyPackageFeed(string sourceDirectory, string destinationDirectory)
    {
        foreach (var sourcePath in Directory.EnumerateFiles(sourceDirectory, "*.nupkg", SearchOption.TopDirectoryOnly))
        {
            File.Copy(sourcePath, Path.Combine(destinationDirectory, Path.GetFileName(sourcePath)));
        }
    }

    private static void AssertReleaseCorePackages(string jazorPackagePath, string vuePackagePath)
    {
        AssertPackageEntries(
            jazorPackagePath,
            "lib/net11.0/ECMAScript.dll",
            "tools/net11.0/Jazor.Emit.dll");
        AssertPackageEntries(
            vuePackagePath,
            "lib/net11.0/ECMAScript.Vue.dll");
        AssertPackageDoesNotContain(
            vuePackagePath,
            "lib/net11.0/ECMAScript.VueContract.dll",
            "lib/net11.0/ECMAScript.Blazor.dll",
            "lib/net11.0/ECMAScript.Blazor.pdb");
    }

    private static async Task<LocalReleasePackageFixture> CreateLocalReleasePackageAsync()
    {
        var repoRoot = FindRepoRoot();
        var packageOutputDirectory = CreateFixtureDirectory(repoRoot, "release-core-nupkg");
        var packageVersion = await ResolvePackageVersionAsync(repoRoot);
        var buildWorkspace = new PackageBuildWorkspace(repoRoot, "Release", packageVersion, "release-packages");

        foreach (var projectName in new[] { "Jazor", "ECMAScript.Vue" })
        {
            var result = await buildWorkspace.PackAsync(projectName, packageOutputDirectory);
            Assert.AreEqual(0, result.ExitCode, result.ToString());
        }

        Assert.AreEqual(packageVersion, DiscoverPackageVersion(packageOutputDirectory, "Jazor"));
        Assert.AreEqual(packageVersion, DiscoverPackageVersion(packageOutputDirectory, "ECMAScript.Vue"));
        var jazorPackagePath = GetPackagePath(packageOutputDirectory, packageVersion);
        var vuePackagePath = GetPackagePath(packageOutputDirectory, "ECMAScript.Vue", packageVersion);
        AssertReleaseCorePackages(jazorPackagePath, vuePackagePath);

        return new LocalReleasePackageFixture(
            repoRoot,
            packageVersion,
            packageOutputDirectory,
            buildWorkspace);
    }

    private static async Task<LocalCorePackageFixture> CreateLocalCorePackageAsync()
    {
        var repoRoot = FindRepoRoot();
        var packageOutputDirectory = CreateFixtureDirectory(repoRoot, "debug-core-nupkg");
        var packageVersion = await ResolvePackageVersionAsync(repoRoot);
        var buildWorkspace = new PackageBuildWorkspace(repoRoot, "Debug", packageVersion, "debug-packages");

        var jazorPack = await buildWorkspace.PackAsync("Jazor", packageOutputDirectory);
        Assert.AreEqual(0, jazorPack.ExitCode, jazorPack.ToString());
        Assert.IsFalse(
            jazorPack.ToString().Contains("NU5118", StringComparison.OrdinalIgnoreCase),
            "Jazor package emitted duplicate pack warnings." + Environment.NewLine + jazorPack);
        var vuePack = await buildWorkspace.PackAsync("ECMAScript.Vue", packageOutputDirectory);
        Assert.AreEqual(0, vuePack.ExitCode, vuePack.ToString());
        AssertPackageArtifactOutputs(
            buildWorkspace.OutputRoot,
            Path.Combine(buildWorkspace.OutputRoot, "Jazor.Emit", "bin", "Debug", "net11.0", "publish"));

        Assert.AreEqual(packageVersion, DiscoverPackageVersion(packageOutputDirectory, "Jazor"));
        Assert.AreEqual(packageVersion, DiscoverPackageVersion(packageOutputDirectory, "ECMAScript.Vue"));

        return new LocalCorePackageFixture(
            repoRoot,
            packageVersion,
            packageOutputDirectory,
            GetPackagePath(packageOutputDirectory, packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Vue", packageVersion),
            buildWorkspace);
    }

    private static async Task<LocalPackageFixture> CreateLocalPackageAsync()
    {
        var core = await LocalCorePackage.Value;
        var packageOutputDirectory = CreateFixtureDirectory(core.RepoRoot, "debug-bindings-nupkg");
        CopyPackageFeed(core.PackageOutputDirectory, packageOutputDirectory);

        var packages = new[]
        {
            new DebugPackageProject("ECMAScript.VueRoute", "ECMAScript.VueRoute", "net11.0"),
            new DebugPackageProject("ECMAScript.Pinia", "ECMAScript.Pinia", "net11.0"),
            new DebugPackageProject("ECMAScript.Pinia.Testing", "ECMAScript.Pinia.Testing", "net11.0"),
        };

        foreach (var package in packages)
        {
            var result = await core.BuildWorkspace.PackAsync(package.ProjectName, packageOutputDirectory);
            Assert.AreEqual(0, result.ExitCode, result.ToString());
            Assert.IsTrue(
                File.Exists(Path.Combine(
                    core.BuildWorkspace.OutputRoot,
                    package.OutputProjectName,
                    "bin",
                    "Debug",
                    package.TargetFramework,
                    package.OutputProjectName + ".dll")),
                $"{package.ProjectName} package preparation did not build {package.OutputProjectName}.");
        }

        return new LocalPackageFixture(
            core.RepoRoot,
            core.PackageVersion,
            packageOutputDirectory,
            core.PackagePath,
            core.VuePackagePath,
            GetPackagePath(packageOutputDirectory, "ECMAScript.VueRoute", core.PackageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia", core.PackageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia.Testing", core.PackageVersion));
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

    private static void AssertPackageDoesNotContain(string packagePath, params string[] forbiddenPaths)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entries = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var forbiddenPath in forbiddenPaths)
            Assert.IsFalse(entries.Contains(forbiddenPath), $"Package '{packagePath}' unexpectedly contains '{forbiddenPath}'.");
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

    private static string EnsureTrailingDirectorySeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

    private static Task<ProcessResult> RunIsolatedConsumerBuildAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments)
        => RunDotNetAsync(workingDirectory, arguments);

    private static async Task<ProcessResult> RunDotNetAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        await DotNetProcessGate.WaitAsync();
        try
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
        finally
        {
            DotNetProcessGate.Release();
        }
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

        if (arguments.Count > 0 && arguments[0] is "build" or "publish")
            startInfo.ArgumentList.Add("-p:JazorPnpmStoreDirectory=" + Path.Combine(RepositoryTemp.Root, "nodeapi-store"));

        RepositoryTemp.ApplyProcessEnvironment(startInfo);
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        // Avoid compiler-server file locks when repeatedly packing the same projects.
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

    private static Task<string> RunNodeTestAsync(string testFile, string workingDirectory)
        => NodeModuleRunner.RunAsync(workingDirectory, testFile,
            imports: File.Exists(Path.Combine(workingDirectory, "vue-test-runtime.mjs"))
                ? new Dictionary<string, string> { ["vue"] = "./vue-test-runtime.mjs" }
                : null);
    private static string FindBuildState(string outputRoot)
    {
        var projectRoot = Directory.GetParent(outputRoot)!.FullName;
        return Directory.EnumerateFiles(Path.Combine(projectRoot, "obj"), "jazor-manifest.json", SearchOption.AllDirectories).Single();
    }

    private static void AssertStandardNodeProject(string outputRoot)
    {
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "package.json")));
        Assert.IsFalse(File.Exists(Path.Combine(outputRoot, "ssr-importmap.json")));
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
        var package = Directory.EnumerateFiles(packageOutputDirectory, "*.nupkg", SearchOption.TopDirectoryOnly)
            .Where(static path => !path.EndsWith(".snupkg", StringComparison.OrdinalIgnoreCase))
            .Select(static path => (Path: path, Identity: ReadPackageIdentity(path)))
            .Where(candidate => string.Equals(candidate.Identity.Id, packageId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static candidate => File.GetLastWriteTimeUtc(candidate.Path))
            .FirstOrDefault();

        if (package.Path is null)
            throw new FileNotFoundException($"No '{packageId}' nupkg found in '{packageOutputDirectory}'.");

        return package.Identity.Version;
    }

    private static (string Id, string Version) ReadPackageIdentity(string packagePath)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var nuspec = archive.Entries.SingleOrDefault(static entry =>
            entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidDataException($"Package '{packagePath}' does not contain a nuspec.");
        using var stream = nuspec.Open();
        var document = XDocument.Load(stream);
        var packageNamespace = document.Root?.Name.Namespace ?? XNamespace.None;
        var metadata = document.Root?.Element(packageNamespace + "metadata")
            ?? throw new InvalidDataException($"Package '{packagePath}' does not contain nuspec metadata.");
        var id = metadata.Element(packageNamespace + "id")?.Value;
        var version = metadata.Element(packageNamespace + "version")?.Value;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(version))
            throw new InvalidDataException($"Package '{packagePath}' does not declare an id and version.");

        return (id, version);
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

    private static string CreateDefaultOutputStaticHostProject(string projectRoot, bool includeVue = false)
    {
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "StaticHostDefaultOutput.csproj");
        var vuePackageReference = includeVue
            ? "    <PackageReference Include=\"ECMAScript.Vue\" Version=\"$(JazorPackageVersion)\" PrivateAssets=\"all\" />"
            : "";

        WriteFile(
            projectPath,
            $$"""
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
            {{vuePackageReference}}
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
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
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
                    color = color("white"),
                    background_color = hex("1769aa"),
                    children =
                    [
                        new(ChildKind.Container, "toolbar (width > 30rem)", new CssRule
                        {
                            display = grid
                        })
                    ]
                });

                public static CssSnapshot Snapshot()
                {
                    css.at_rule(Context, new CssAtRule(
                        "font-face",
                        new CssDeclarations
                        {
                            font_family = str("Example Sans"),
                            ["src"] = raw("url(example.woff2)")
                        }));
                    return css.snapshot(Context);
                }
            }
            """);

        return projectPath;
    }

    private static string CreateDefaultOutputWebHostProject(string projectRoot, bool includeVue = false)
    {
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "WebHostDefaultOutput.csproj");
        var vuePackageReference = includeVue
            ? "    <PackageReference Include=\"ECMAScript.Vue\" Version=\"$(JazorPackageVersion)\" PrivateAssets=\"all\" />"
            : "";

        WriteFile(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorMode>debug</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
            {{vuePackageReference}}
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

    private static string CreateReleaseCorePackageConsumerProject(string projectRoot)
    {
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "ReleaseCorePackageConsumer.csproj");
        WriteFile(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorMode>none</JazorMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            using ECMAScript;

            namespace ReleaseCorePackageConsumer;

            [ECMAScriptModule("./core")]
            public static class CoreModule
            {
                public static int Value() => new Counter(1).Value;
            }

            internal sealed class Counter
            {
                public Counter(int value) { Value = value; }
                public int Value { get; }
            }

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);
        return projectPath;
    }

    private static string[] GetReleaseConsumerBuildArguments(
        LocalReleasePackageFixture package,
        string restorePackagesPath)
        =>
        [
            "-c",
            "Release",
            "-t:Rebuild",
            "/m:1",
            "/p:BuildInParallel=false",
            $"-p:RestoreSources={package.PackageOutputDirectory}",
            "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
            $"-p:RestorePackagesPath={restorePackagesPath}",
            $"-p:JazorPackageVersion={package.PackageVersion}"
        ];

    private static string ReadProjectAssetsText(string projectRoot)
    {
        var assetsPath = Path.Combine(projectRoot, "obj", "project.assets.json");
        Assert.IsTrue(File.Exists(assetsPath), $"NuGet assets file was not generated: {assetsPath}");
        return File.ReadAllText(assetsPath, Encoding.UTF8);
    }

    private static ManifestModel LoadManifest(string manifestPath)
        => ManifestModel.TryLoad(manifestPath)
            ?? throw new FileNotFoundException("Manifest was not found: " + manifestPath, manifestPath);

    private static void AssertManifestFile(JsonElement entries, string expectedType, string expectedPath)
    {
        var entry = entries.EnumerateArray().Single(candidate =>
            string.Equals(candidate.GetProperty("type").GetString(), expectedType, StringComparison.Ordinal) &&
            string.Equals(candidate.GetProperty("path").GetString(), expectedPath, StringComparison.Ordinal));
        var hash = entry.GetProperty("hash").GetString();
        Assert.IsNotNull(hash, $"Manifest file '{expectedPath}' must declare a SHA-256 hash.");
        Assert.HasCount(64, hash);
        Assert.IsTrue(hash.All(static value => value is >= '0' and <= '9' or >= 'a' and <= 'f'));
    }

    private static Task AssertResourceImportIsMaterializedAsync(string outputRoot, string specifier)
    {
        var materializedPath = Path.Combine(outputRoot, specifier.Replace('/', Path.DirectorySeparatorChar));
        Assert.IsTrue(File.Exists(materializedPath), $"Resource import '{specifier}' was not materialized: {materializedPath}");
        return Task.CompletedTask;
    }

    private static ArtifactHash[] ReadArtifactHashes(string outputRoot)
    {
        Assert.IsTrue(Directory.Exists(outputRoot), $"Artifact output root was not generated: {outputRoot}");

        return Directory
            .EnumerateFiles(outputRoot, "*", SearchOption.AllDirectories)
            // Compare emitted source, maps, config and lock, not pnpm's mutable
            // installation metadata or the environment's materialized tool cache.
            .Where(filePath => !IsNodeToolCachePath(outputRoot, filePath))
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
            if (IsNodeToolCachePath(outputRoot, filePath)) continue;
            var text = File.ReadAllText(filePath);
            var relativePath = Path.GetRelativePath(outputRoot, filePath).Replace('\\', '/');
            Assert.IsFalse(
                text.Contains(unexpectedText, StringComparison.OrdinalIgnoreCase),
                $"Artifact '{relativePath}' must not persist the external consumer's absolute project path.");
        }
    }

    private static bool IsNodeToolCachePath(string root, string path)
    {
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        return relative.StartsWith("node_modules/", StringComparison.Ordinal) ||
            relative.StartsWith(".jazor/", StringComparison.Ordinal);
    }

    private static bool ShouldSkip(string relativePath)
    {
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(static segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("jazor", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("TestResults", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("node_modules", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("dist", StringComparison.OrdinalIgnoreCase) ||
            segment.StartsWith("dist-", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("wwwroot", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals(".deno-build", StringComparison.OrdinalIgnoreCase) ||
            segment.StartsWith(".deno-build", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ArtifactHash(string RelativePath, string ContentHash);

    private sealed record DebugPackageProject(
        string ProjectName,
        string OutputProjectName,
        string TargetFramework);

    private sealed record LocalCorePackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        string PackagePath,
        string VuePackagePath,
        PackageBuildWorkspace BuildWorkspace);

    private sealed record LocalPackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        string PackagePath,
        string VuePackagePath,
        string VueRoutePackagePath,
        string PiniaPackagePath,
        string PiniaTestingPackagePath);

    private sealed record LocalReleasePackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        PackageBuildWorkspace BuildWorkspace);

    private sealed record LocalStylePackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        string StylePackagePath);

    private sealed class PackageBuildWorkspace
    {
        private readonly SemaphoreSlim _packGate = new(1, 1);

        public PackageBuildWorkspace(
            string repoRoot,
            string configuration,
            string packageVersion,
            string purpose)
        {
            RepoRoot = repoRoot;
            Configuration = configuration;
            PackageVersion = packageVersion;
            RestorePackagesPath = CreateFixtureDirectory(repoRoot, purpose + "-restore-packages");
            OutputRoot = CreateFixtureDirectory(repoRoot, purpose + "-package-out");
            IntermediateRoot = CreateFixtureDirectory(repoRoot, purpose + "-package-obj");
        }

        public string RepoRoot { get; }

        public string Configuration { get; }

        public string PackageVersion { get; }

        public string RestorePackagesPath { get; }

        public string OutputRoot { get; }

        public string IntermediateRoot { get; }

        public async Task<ProcessResult> PackAsync(string projectName, string packageOutputDirectory)
        {
            await _packGate.WaitAsync();
            try
            {
                // 同一配置的 package fixture 共享 restore/bin/obj。串行增量 pack 比多个隔离构建
                // 同时重复编译整张 ProjectReference 图更快，也避免跨进程争抢同一输出文件。
                return await RunDotNetAsync(
                    RepoRoot,
                    [
                        "pack",
                        Path.Combine(RepoRoot, "src", projectName, projectName + ".csproj"),
                        "-c",
                        Configuration,
                        "-o",
                        packageOutputDirectory,
                        "/m:1",
                        "/p:BuildInParallel=false",
                        $"-p:PackageVersion={PackageVersion}",
                        $"-p:JazorPackageVersion={PackageVersion}",
                        $"-p:RestorePackagesPath={RestorePackagesPath}",
                        $"-p:NuGetPackageRoot={EnsureTrailingDirectorySeparator(RestorePackagesPath)}",
                        $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(OutputRoot)}",
                        $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(IntermediateRoot)}",
                        "/nr:false",
                        "-p:UseSharedCompilation=false"
                    ]);
            }
            finally
            {
                _packGate.Release();
            }
        }
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace(string repoRoot)
        {
            RootPath = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            SampleRoot = Path.Combine(RootPath, "Jazor.MultiProject");
            RestorePackagesPath = Path.Combine(RootPath, "restore-packages");
            Directory.CreateDirectory(SampleRoot);
            Directory.CreateDirectory(RestorePackagesPath);

            // The copied sample imports ../Directory.Build.props; preserve that layout
            // so every parallel test owns a complete, isolated MSBuild tree.
            File.Copy(
                Path.Combine(repoRoot, "Directory.Build.props"),
                Path.Combine(RootPath, "Directory.Build.props"));
        }

        public string RootPath { get; }

        public string SampleRoot { get; }

        public string RestorePackagesPath { get; }

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
