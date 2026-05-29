using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Jazor.Emit;
using Jazor.RazorVue.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class SdkIntegrationTests
{
    private static readonly Lazy<Task<LocalPackageFixture>> LocalPackage = new(CreateLocalPackageAsync);
    private static readonly SemaphoreSlim SourceReferencedRazorVueBuildGate = new(1, 1);

    [TestMethod]
    public async Task CreateLocalPackage_IncludesRazorVueAuthoringAssets()
    {
        var package = await LocalPackage.Value;

        using var archive = ZipFile.OpenRead(package.PackagePath);
        var entryNames = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[]
            {
                "lib/net11.0/ECMAScript.dll",
                "lib/net11.0/ECMAScript.pdb",
                "lib/net11.0/ECMAScript.Contract.dll",
                "lib/net11.0/ECMAScript.Contract.pdb",
                "lib/net11.0/ECMAScript.VueContract.dll",
                "lib/net11.0/ECMAScript.VueContract.pdb",
                "lib/net11.0/ECMAScript.Vue3.dll",
                "lib/net11.0/ECMAScript.Vue3.pdb",
                "lib/net11.0/Jazor.AspNetCore.dll",
                "lib/net11.0/Jazor.AspNetCore.pdb",
                "lib/net11.0/Jazor.AspNetCore.Dev.dll",
                "lib/net11.0/Jazor.AspNetCore.Dev.pdb",
                "lib/net11.0/Jazor.Compiler.dll",
                "lib/net11.0/Jazor.Compiler.pdb",
                "lib/net11.0/Jazor.Common.dll",
                "lib/net11.0/Jazor.Common.pdb",
                "lib/net11.0/Jazor.RazorVue.dll",
                "lib/net11.0/Jazor.RazorVue.pdb"
            },
            entryNames.Where(static entry => entry.StartsWith("lib/net11.0/", StringComparison.Ordinal)).ToArray());
        CollectionAssert.AreEquivalent(
            new[]
            {
                "analyzers/dotnet/cs/Acornima.Extras.dll",
                "analyzers/dotnet/cs/Acornima.dll",
                "analyzers/dotnet/cs/Jazor.Analyzer.dll",
                "analyzers/dotnet/cs/Jazor.Analyzer.pdb",
                "analyzers/dotnet/cs/ECMAScript.dll",
                "analyzers/dotnet/cs/ECMAScript.pdb",
                "analyzers/dotnet/cs/ECMAScript.Contract.dll",
                "analyzers/dotnet/cs/ECMAScript.Contract.pdb",
                "analyzers/dotnet/cs/ECMAScript.Vue3.dll",
                "analyzers/dotnet/cs/ECMAScript.Vue3.pdb",
                "analyzers/dotnet/cs/ECMAScript.VueContract.dll",
                "analyzers/dotnet/cs/ECMAScript.VueContract.pdb",
                "analyzers/dotnet/cs/Jazor.Compiler.dll",
                "analyzers/dotnet/cs/Jazor.Compiler.pdb",
                "analyzers/dotnet/cs/Jazor.Common.dll",
                "analyzers/dotnet/cs/Jazor.Common.pdb",
                "analyzers/dotnet/cs/Jazor.RazorVue.dll",
                "analyzers/dotnet/cs/Jazor.RazorVue.pdb"
            },
            entryNames.Where(static entry => entry.StartsWith("analyzers/dotnet/cs/", StringComparison.Ordinal)).ToArray());
        Assert.IsFalse(
            entryNames.Any(static entry =>
                entry.Contains("Razor.Compiler", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("Razor.Utilities.Shared", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("Microsoft.CodeAnalysis.Razor", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("Microsoft.AspNetCore.Razor.Language", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("Harmony", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("MonoMod", StringComparison.OrdinalIgnoreCase) ||
                entry.Contains("Detour", StringComparison.OrdinalIgnoreCase)),
            "Jazor package must not carry Razor Compiler, Razor Utilities, Harmony, MonoMod, or Detour payloads.");
        CollectionAssert.IsSubsetOf(
            new[]
            {
                "buildTransitive/Jazor.props",
                "buildTransitive/Jazor.targets"
            },
            entryNames);
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
    public async Task Build_LocalJazorPackage_MultiProjectSample_EmitsModulesAndBundle()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");

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
                "-p:JazorBundle=true"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var manifestPath = Path.Combine(hostRoot, "wwwroot", "jazor", "jazor-manifest.json");
        var bundlePath = Path.Combine(hostRoot, "wwwroot", "app.bundle.js");
        var sharedModulePath = Path.Combine(hostRoot, "wwwroot", "jazor", "shared", "greetings.mjs");
        var featureModulePath = Path.Combine(hostRoot, "wwwroot", "jazor", "features", "greeter.mjs");
        var hostModulePath = Path.Combine(hostRoot, "wwwroot", "jazor", "host", "app.mjs");

        Assert.IsTrue(
            File.Exists(manifestPath),
            """
            Manifest was not generated.
            Expected:
            """ + manifestPath + """

            Build:
            """ + build + """

            Files under host root:
            """ + string.Join(
                Environment.NewLine,
                Directory.Exists(hostRoot)
                    ? Directory.EnumerateFiles(hostRoot, "*", SearchOption.AllDirectories)
                        .Select(path => Path.GetRelativePath(hostRoot, path))
                    : []) + Environment.NewLine);
        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(sharedModulePath), $"Shared module was not generated: {sharedModulePath}");
        Assert.IsTrue(File.Exists(featureModulePath), $"Feature module was not generated: {featureModulePath}");
        Assert.IsTrue(File.Exists(hostModulePath), $"Host module was not generated: {hostModulePath}");

        var emittedManifest = LoadManifest(manifestPath);
        var modulePaths = emittedManifest.Modules
            .Select(static module => module.RelativePath)
            .ToArray();

        CollectionAssert.IsSubsetOf(
            new[]
            {
                "shared/greetings.mjs",
                "features/greeter.mjs",
                "host/app.mjs"
            },
            modulePaths);

        var sharedModule = await File.ReadAllTextAsync(sharedModulePath);
        var featureModule = await File.ReadAllTextAsync(featureModulePath);
        var hostModule = await File.ReadAllTextAsync(hostModulePath);
        var bundle = await File.ReadAllTextAsync(bundlePath);

        StringAssert.Contains(sharedModule, "export function prefix()");
        StringAssert.Contains(sharedModule, "export function compose(name)");
        StringAssert.Contains(featureModule, "import { compose } from \"shared/greetings.mjs\";");
        StringAssert.Contains(featureModule, "export function greet(name)");
        StringAssert.Contains(hostModule, "import { greet } from \"features/greeter.mjs\";");
        StringAssert.Contains(hostModule, "export function boot()");
        StringAssert.Contains(bundle, "function prefix()");
        StringAssert.Contains(bundle, "function greet(name)");
        StringAssert.Contains(bundle, "function boot()");
        StringAssert.Contains(bundle, "export {");
        StringAssert.Contains(bundle, "boot");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_SingleProjectWrapperApis_EmitsMinimalRuntimeImports()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sourceSampleRoot = Path.Combine(package.RepoRoot, "samples", "Jazor.MultiProject");
        CopyDirectory(sourceSampleRoot, workspace.SampleRoot);

        var hostRoot = Path.Combine(workspace.SampleRoot, "Sample.Host");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");

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
    public async Task Build_LocalJazorPackage_StaticHost_UsesProjectRootJazorByDefault()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "StaticHostDefaultBuildSample");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
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

        var devJazorRoot = Path.Combine(projectRoot, "jazor");
        var publishJazorRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        Assert.IsTrue(File.Exists(Path.Combine(devJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(devJazorRoot, "host", "app.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(devJazorRoot, "host", "app.mjs.map")));
        var devModule = await File.ReadAllTextAsync(Path.Combine(devJazorRoot, "host", "app.mjs"));
        StringAssert.Contains(devModule, "sourceMappingURL=app.mjs.map");
        Assert.IsFalse(Directory.Exists(publishJazorRoot), $"Build should not materialize publish assets under '{publishJazorRoot}'.");
    }

    [TestMethod]
    public async Task Publish_LocalJazorPackage_StaticHost_UsesWwwrootJazorByDefault()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "StaticHostDefaultPublishSample");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
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
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
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
    public async Task Build_LocalJazorPackage_WebSdkHost_WithColocatedConsumer_UsesSdkConsumerBuildForDevelopmentBrowserAssets()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "WebSdkConsumerBuildSample");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var projectPath = CreateWebHostWithColocatedConsumerProject(projectRoot);

        var build = await RunDotNetWithEnvironmentAsync(
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
            ],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["JAZOR_DENO_EXE"] = package.DenoExePath
            });

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var sourceDevJazorRoot = Path.Combine(projectRoot, "jazor");
        var sourceBrowserBundleRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var consumerDistRoot = Path.Combine(projectRoot, "consumer", "dist");

        Assert.IsTrue(File.Exists(Path.Combine(sourceDevJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceDevJazorRoot, "components", "catalog-page.vue")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceDevJazorRoot, "components", "detail-page.vue")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceDevJazorRoot, "__jazor", "razorvue-host.mjs")));

        Assert.IsTrue(File.Exists(Path.Combine(sourceBrowserBundleRoot, "client-entry.js")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceBrowserBundleRoot, "client-entry.css")));
        Assert.IsFalse(
            File.Exists(Path.Combine(sourceBrowserBundleRoot, "jazor-manifest.json")),
            "Build-time consumer browser assets must not materialize compiler-owned RazorVue manifest into wwwroot/jazor.");
        Assert.IsFalse(
            Directory.Exists(Path.Combine(sourceBrowserBundleRoot, "components")),
            "Build-time consumer browser assets must not materialize compiler-owned RazorVue SFCs into wwwroot/jazor.");

        var distHtmlPath = Path.Combine(consumerDistRoot, "index.html");
        Assert.IsTrue(File.Exists(distHtmlPath), $"Expected colocated consumer dist HTML was not generated: {distHtmlPath}");
        var distHtml = (await File.ReadAllTextAsync(distHtmlPath)).ReplaceLineEndings("\n");
        StringAssert.Contains(distHtml, "<script type=\"module\" src=\"./jazor/client-entry.js\"></script>");
        StringAssert.Contains(distHtml, "<link rel=\"stylesheet\" href=\"./jazor/client-entry.css\" />");

        await AssertSdkConsumerBrowserEntryAsync(
            Path.Combine(sourceBrowserBundleRoot, "client-entry.js"),
            "Build-time colocated consumer browser entry should not retain unresolved .vue imports.");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_WebSdkHost_WithColocatedConsumer_MissingRunnerFailsFast()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "WebSdkConsumerMissingRunnerSample");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var projectPath = CreateWebHostWithColocatedConsumerProject(projectRoot);
        var expectedRunnerPath = Path.Combine(projectRoot, "consumer", "scripts", "run-deno.cs");
        File.Delete(expectedRunnerPath);

        var build = await RunDotNetWithEnvironmentAsync(
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
            ],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["JAZOR_DENO_EXE"] = package.DenoExePath
            });

        Assert.AreNotEqual(0, build.ExitCode, build.ToString());
        var output = (build.StandardOutput + build.StandardError).ReplaceLineEndings("\n");
        StringAssert.Contains(output, "Jazor consumer runner was not found:");
        StringAssert.Contains(output, expectedRunnerPath);

        var sourceDevJazorRoot = Path.Combine(projectRoot, "jazor");
        var sourceBrowserBundleRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        Assert.IsTrue(
            File.Exists(Path.Combine(sourceDevJazorRoot, "jazor-manifest.json")),
            "JazorEmit should complete before the colocated consumer handoff fails.");
        Assert.IsFalse(
            File.Exists(Path.Combine(sourceBrowserBundleRoot, "client-entry.js")),
            "Missing consumer runner must fail before any browser JS bundle is produced.");
        Assert.IsFalse(
            File.Exists(Path.Combine(sourceBrowserBundleRoot, "client-entry.css")),
            "Missing consumer runner must fail before any browser CSS bundle is produced.");
    }

    [TestMethod]
    public async Task Publish_LocalJazorPackage_WebSdkHost_WithColocatedConsumer_UsesSdkConsumerBuildAndUnifiedJazorPublishRoot()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "WebSdkConsumerPublishSample");
        var publishOutputRoot = Path.Combine(workspace.RootPath, "publish-output");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var projectPath = CreateWebHostWithColocatedConsumerProject(projectRoot);

        var publish = await RunDotNetWithEnvironmentAsync(
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
            ],
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["JAZOR_DENO_EXE"] = package.DenoExePath
            });

        Assert.AreEqual(0, publish.ExitCode, publish.ToString());

        var sourceDevJazorRoot = Path.Combine(projectRoot, "jazor");
        var sourceBrowserBundleRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var publishedJazorRoot = Path.Combine(publishOutputRoot, "wwwroot", "jazor");
        var publishedShadowJazorRoot = Path.Combine(publishOutputRoot, "jazor");
        var publishedLegacyAssetsRoot = Path.Combine(publishOutputRoot, "wwwroot", "assets");

        Assert.IsTrue(File.Exists(Path.Combine(sourceDevJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceDevJazorRoot, "components", "catalog-page.vue")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceDevJazorRoot, "__jazor", "razorvue-host.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceBrowserBundleRoot, "client-entry.js")));
        Assert.IsTrue(File.Exists(Path.Combine(sourceBrowserBundleRoot, "client-entry.css")));

        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "components", "catalog-page.vue")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "__jazor", "razorvue-host.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "client-entry.js")));
        Assert.IsTrue(File.Exists(Path.Combine(publishedJazorRoot, "client-entry.css")));

        var manifest = LoadManifest(Path.Combine(publishedJazorRoot, "jazor-manifest.json"));
        var modulePaths = manifest.Modules
            .Select(static module => module.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();
        CollectionAssert.Contains(modulePaths, "components/catalog-page.vue");
        CollectionAssert.Contains(modulePaths, "components/detail-page.vue");

        await AssertSdkConsumerBrowserEntryAsync(
            Path.Combine(publishedJazorRoot, "client-entry.js"),
            "Published colocated consumer browser entry should not retain unresolved .vue imports.");

        Assert.IsFalse(
            Directory.Exists(publishedShadowJazorRoot),
            $"Publish output must not leak a shadow root jazor directory at '{publishedShadowJazorRoot}'.");
        Assert.IsFalse(
            Directory.Exists(publishedLegacyAssetsRoot),
            $"Publish output must not leak legacy browser assets directory at '{publishedLegacyAssetsRoot}'.");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_SourceReferencedRazorVue_UsesProjectRootJazorByDefault()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var hostRoot = Path.Combine(workspace.RootPath, "RazorVueDefaultBuild.Host");
        var projectPath = CreateDefaultOutputRazorVueSampleProject(hostRoot, package);

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

        var devJazorRoot = Path.Combine(hostRoot, "jazor");
        var publishJazorRoot = Path.Combine(hostRoot, "wwwroot", "jazor");
        Assert.IsTrue(File.Exists(Path.Combine(devJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(devJazorRoot, "components", "profile-form.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(devJazorRoot, "__jazor", "razorvue-host.mjs")));
        Assert.IsFalse(Directory.Exists(publishJazorRoot), $"Build should not materialize RazorVue publish assets under '{publishJazorRoot}'.");
    }

    [TestMethod]
    public async Task Publish_LocalJazorPackage_SourceReferencedRazorVue_UsesWwwrootJazorByDefault()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var hostRoot = Path.Combine(workspace.RootPath, "RazorVueDefaultPublish.Host");
        var projectPath = CreateDefaultOutputRazorVueSampleProject(hostRoot, package);

        var publish = await RunSourceReferencedRazorVueBuildAsync(
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

        var devJazorRoot = Path.Combine(hostRoot, "jazor");
        var publishJazorRoot = Path.Combine(hostRoot, "wwwroot", "jazor");
        Assert.IsTrue(File.Exists(Path.Combine(publishJazorRoot, "jazor-manifest.json")));
        Assert.IsTrue(File.Exists(Path.Combine(publishJazorRoot, "components", "profile-form.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(publishJazorRoot, "__jazor", "razorvue-host.mjs")));
        Assert.IsFalse(Directory.Exists(devJazorRoot), $"Publish should not fall back to the RazorVue development output root '{devJazorRoot}'.");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_WithVueRouteAuthoring_EmitsVueRouterImportsAndRouteObjects()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "VueRouteSdkSample");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");

        WriteFile(
            Path.Combine(projectRoot, "VueRouteSdkSample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
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
            "import { createRouter, createWebHistory, useRouter } from \"npm:vue-router@4\";",
            GetImportLine(module, "npm:vue-router@4"));
        StringAssert.Contains(module, "export function createAppRouter()");
        StringAssert.Contains(module, "history: createWebHistory()");
        StringAssert.Contains(module, "redirect: \"/home\"");
        StringAssert.Contains(module, "path: \"/users\"");
        StringAssert.Contains(module, "props: true");
        StringAssert.Contains(module, "return createRouter(");
        StringAssert.Contains(module, "export function currentPath()");
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
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");

        WriteFile(
            Path.Combine(projectRoot, "VueRouteReactiveSdkSample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
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
            "import { computed, inject, provide, shallowRef, toRef, triggerRef } from \"npm:vue@3\";",
            GetImportLine(module, "npm:vue@3"));
        var vueRouterImport = GetImportLine(module, "npm:vue-router@4");
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
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");

        WriteFile(
            Path.Combine(projectRoot, "VueRouteReactiveBundleSdkSample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>true</JazorBundle>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
                <JazorBundleOut>$(MSBuildProjectDirectory)\wwwroot\app.bundle.js</JazorBundleOut>
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

        var outputRoot = Path.Combine(projectRoot, "wwwroot");
        var moduleRoot = Path.Combine(outputRoot, "jazor");
        var manifestPath = Path.Combine(moduleRoot, "jazor-manifest.json");
        var modulePath = Path.Combine(moduleRoot, "host", "app.mjs");
        var bundlePath = Path.Combine(outputRoot, "app.bundle.js");
        var bundleSourceMapPath = Path.Combine(outputRoot, "app.bundle.js.map");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(modulePath), $"Module was not generated: {modulePath}");
        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(bundleSourceMapPath), $"Bundle source map was not generated: {bundleSourceMapPath}");

        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");
        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        var bundleSourceMap = (await File.ReadAllTextAsync(bundleSourceMapPath)).ReplaceLineEndings("\n");

        Assert.AreEqual(
            "import { computed, inject, provide, shallowRef, toRef, triggerRef } from \"npm:vue@3\";",
            GetImportLine(module, "npm:vue@3"));
        var vueRouterImport = GetImportLine(module, "npm:vue-router@4");
        StringAssert.Contains(vueRouterImport, "createRouter");
        StringAssert.Contains(vueRouterImport, "createWebHistory");
        StringAssert.Contains(vueRouterImport, "loadRouteLocation");
        StringAssert.Contains(vueRouterImport, "matchedRouteKey");
        StringAssert.Contains(vueRouterImport, "routeLocationKey");
        StringAssert.Contains(vueRouterImport, "routerKey");
        StringAssert.Contains(vueRouterImport, "routerViewLocationKey");
        StringAssert.Contains(vueRouterImport, "useLink");
        StringAssert.Contains(vueRouterImport, "useRoute");
        StringAssert.Contains(vueRouterImport, "viewDepthKey");

        StringAssert.Contains(bundle, "/bundle-base");
        StringAssert.Contains(bundle, "/bundle-home");
        StringAssert.Contains(bundle, "/bundle-users");
        StringAssert.Contains(bundle, "bundle-user");
        StringAssert.Contains(bundle, "sourceMappingURL=app.bundle.js.map");
        Assert.IsFalse(
            bundle.Contains("from \"npm:vue-router@4\"", StringComparison.Ordinal),
            "Bundle should not keep unresolved vue-router imports.");
        Assert.IsFalse(
            bundle.Contains("from \"npm:vue@3\"", StringComparison.Ordinal),
            "Bundle should not keep unresolved vue imports.");

        StringAssert.Contains(bundleSourceMap, "\"sources\"");
        Assert.IsTrue(
            bundleSourceMap.Contains("host/app.mjs", StringComparison.Ordinal)
            || bundleSourceMap.Contains("AppModule.cs", StringComparison.Ordinal),
            "Bundle source map should preserve authored module provenance.");

        var emittedRelativePaths = LoadManifest(manifestPath).Modules
            .Select(static moduleEntry => moduleEntry.RelativePath)
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray();

        CollectionAssert.Contains(emittedRelativePaths, "host/app.mjs");
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_WithSourceReferencedRazorVueSample_EmitsRazorVueOutputs()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var hostRoot = Path.Combine(workspace.RootPath, "RazorVueSample.Host");
        var projectPath = CreateRazorVueSampleProject(hostRoot, package);

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
                "-p:JazorBundle=true"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(hostRoot, "wwwroot");
        var moduleRoot = Path.Combine(outputRoot, "jazor");
        var manifestPath = Path.Combine(moduleRoot, "jazor-manifest.json");
        var componentModulePath = Path.Combine(moduleRoot, "components", "profile-form.mjs");
        var hostRequirementsModulePath = Path.Combine(moduleRoot, "__jazor", "razorvue-host.mjs");
        var bundlePath = Path.Combine(outputRoot, "app.bundle.js");
        var cssPath = Path.Combine(outputRoot, "app.bundle.razorvue.css");
        var hostContractPath = Path.Combine(outputRoot, "app.bundle.razorvue.host.json");
        var updatePlanPath = Path.Combine(outputRoot, "app.bundle.razorvue.update-plan.json");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(hostRequirementsModulePath), $"RazorVue host requirements module was not generated: {hostRequirementsModulePath}");
        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(cssPath), $"RazorVue CSS sidecar was not generated: {cssPath}");
        Assert.IsTrue(File.Exists(hostContractPath), $"RazorVue host contract sidecar was not generated: {hostContractPath}");
        Assert.IsTrue(File.Exists(updatePlanPath), $"RazorVue update plan sidecar was not generated: {updatePlanPath}");

        var componentModule = (await File.ReadAllTextAsync(componentModulePath)).ReplaceLineEndings("\n");
        var hostRequirementsModule = (await File.ReadAllTextAsync(hostRequirementsModulePath)).ReplaceLineEndings("\n");
        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        var css = (await File.ReadAllTextAsync(cssPath)).ReplaceLineEndings("\n");

        StringAssert.Contains(componentModule, "vuetify/components");
        StringAssert.Contains(componentModule, "\"modelValue\": props.name");
        StringAssert.Contains(componentModule, "\"onUpdate:modelValue\": (__value) => emit(\"update:name\", __value)");
        StringAssert.Contains(hostRequirementsModule, "razorVueHostRequirements");
        StringAssert.Contains(hostRequirementsModule, "\"componentName\":\"ProfileForm\"");
        StringAssert.Contains(hostRequirementsModule, "\"componentId\":\"RazorVueSample.Host.ProfileForm\"");
        StringAssert.Contains(hostRequirementsModule, "\"moduleId\":\"components/profile-form.mjs\"");
        StringAssert.Contains(hostRequirementsModule, "\"relativeModulePath\":\"components/profile-form.mjs\"");
        StringAssert.Contains(hostRequirementsModule, "\"sourceMapPath\":\"components/profile-form.mjs.map\"");
        StringAssert.Contains(hostRequirementsModule, "\"originMapPath\":\"components/profile-form.mjs.origins.json\"");
        StringAssert.Contains(hostRequirementsModule, "\"descriptorHash\":");
        StringAssert.Contains(hostRequirementsModule, "\"hmrBoundaryKind\":");
        StringAssert.Contains(hostRequirementsModule, "\"vuetify/styles\"");
        StringAssert.Contains(hostRequirementsModule, "\"vuetify\"");
        StringAssert.Contains(bundle, "razorVueHostRequirements");
        Assert.AreEqual("@import \"vuetify/styles\";\n", css);

        var razorVueManifest = LoadRazorVueManifestProjection(manifestPath);
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            RequireManifestStringList(razorVueManifest.Styles, nameof(RazorVueManifestModel.Styles), manifestPath).ToArray());
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            RequireManifestStringList(razorVueManifest.PluginRequirements, nameof(RazorVueManifestModel.PluginRequirements), manifestPath).ToArray());
        var sourceManifestModule = razorVueManifest.Modules[0];
        Assert.AreEqual(
            "components/profile-form.mjs",
            sourceManifestModule.RelativeModulePath);
        var expectedSourceHmrBoundary = (int)sourceManifestModule.HmrBoundaryKind;
        var expectedSourceRequiresHydration = sourceManifestModule.RequiresHydration;
        var expectedSourceSupportsSsr = sourceManifestModule.SupportsSsr;

        using var hostContract = JsonDocument.Parse(await File.ReadAllTextAsync(hostContractPath));
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            GetStringArrayProperty(hostContract.RootElement, "Styles"));
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            GetStringArrayProperty(hostContract.RootElement, "PluginRequirements"));
        Assert.AreEqual("app.bundle.js.map", hostContract.RootElement.GetProperty("BundleSourceMapFile").GetString());
        Assert.AreEqual(
            "RazorVueSample.Host.ProfileForm",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ModuleId").GetString());
        Assert.AreEqual(
            "ProfileForm",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentName").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("RelativeModulePath").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs.map",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("SourceMapPath").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs.origins.json",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("OriginMapPath").GetString());
        Assert.IsTrue(hostContract.RootElement.GetProperty("Modules")[0].TryGetProperty("DescriptorHash", out var sourceDescriptorHash));
        Assert.AreNotEqual(string.Empty, sourceDescriptorHash.GetString());
        Assert.AreEqual(expectedSourceHmrBoundary, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("HmrBoundaryKind").GetInt32());
        Assert.AreEqual(expectedSourceRequiresHydration, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("RequiresHydration").GetBoolean());
        Assert.AreEqual(expectedSourceSupportsSsr, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("SupportsSsr").GetBoolean());

        using var updatePlan = JsonDocument.Parse(await File.ReadAllTextAsync(updatePlanPath));
        Assert.AreEqual("FullReload", updatePlan.RootElement.GetProperty("Action").GetString());
        Assert.AreEqual("Previous Jazor manifest component projection is missing.", updatePlan.RootElement.GetProperty("Reason").GetString());
        Assert.AreEqual(0, updatePlan.RootElement.GetProperty("Modules").GetArrayLength());

    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_WithSourceReferencedRazorVueSample_SecondBuildWritesUpdatePlan()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var hostRoot = Path.Combine(workspace.RootPath, "RazorVueSample.Host");
        var projectPath = CreateRazorVueSampleProject(hostRoot, package);
        var profileFormPath = Path.Combine(hostRoot, "ProfileForm.cs");
        var updatePlanPath = Path.Combine(hostRoot, "wwwroot", "app.bundle.razorvue.update-plan.json");

        var firstBuild = await RunSourceReferencedRazorVueBuildAsync(
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
                "-p:JazorBundle=true"
            ]);

        Assert.AreEqual(0, firstBuild.ExitCode, firstBuild.ToString());
        Assert.IsTrue(File.Exists(updatePlanPath), $"Initial build should emit a bootstrap update plan: {updatePlanPath}");

        WriteFile(
            profileFormPath,
            """
            using ECMAScript;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RazorVueSample.Host;

            [ECMAScriptModule("./components/profile-form")]
            public sealed class ProfileForm : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Name { get; set; }

                [Parameter]
                public EventCallback<string?> NameChanged { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.Label), "Display Name");
                    builder.AddAttribute(2, nameof(VTextField.ModelValue), Name);
                    builder.AddAttribute(3, nameof(VTextField.ModelValueChanged), NameChanged);
                    builder.CloseComponent();
                }
            }
            """);

        var secondBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            [
                "build",
                projectPath,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                $"-p:JazorPackageVersion={package.PackageVersion}",
                "-p:JazorBundle=true"
            ]);

        Assert.AreEqual(0, secondBuild.ExitCode, secondBuild.ToString());
        Assert.IsTrue(File.Exists(updatePlanPath), $"Update plan was not generated: {updatePlanPath}");

        using var updatePlan = JsonDocument.Parse(await File.ReadAllTextAsync(updatePlanPath));
        Assert.AreEqual("TemplatePatch", updatePlan.RootElement.GetProperty("Action").GetString());
        Assert.AreEqual(
            "RazorVueSample.Host.ProfileForm",
            updatePlan.RootElement.GetProperty("Modules")[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual("TemplatePatch", updatePlan.RootElement.GetProperty("Modules")[0].GetProperty("Action").GetString());
    }

    [TestMethod]
    public async Task Build_LocalJazorPackage_RazorVueAuthoring_EmitsManifestAndHostRequirementsModule()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "RazorVueSdkSample");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");

        WriteFile(
            Path.Combine(projectRoot, "RazorVueSdkSample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorRazorVueOutputMode>legacy</JazorRazorVueOutputMode>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
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
            Console.WriteLine("RazorVue SDK sample");
            """);
        WriteFile(
            Path.Combine(projectRoot, "DemoButton.cs"),
            """
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace Demo.Sample;

            [VueLibraryComponent("demo/components", "DemoButton")]
            [VueLibraryStyle("demo/button.css")]
            [VueLibraryPluginRequirement("demo-host")]
            public sealed class DemoButton : ComponentBase, IVueLibraryComponent
            {
                [Parameter]
                public string? Label { get; set; }

                [Parameter]
                public bool Disabled { get; set; }

                [Parameter]
                public RenderFragment? ChildContent { get; set; }
            }
            """);
        WriteFile(
            Path.Combine(projectRoot, "CounterCard.cs"),
            """
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Sample;

            [ECMAScript.ECMAScriptModule("./components/counter-card")]
            public sealed class CounterCard : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Label { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<DemoButton>(0);
                    builder.AddAttribute(1, nameof(DemoButton.Label), Label);
                    builder.AddAttribute(2, nameof(DemoButton.Disabled), false);
                    builder.CloseComponent();
                }
            }
            """);

        var projectPath = Path.Combine(projectRoot, "RazorVueSdkSample.csproj");
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
        var modulePath = Path.Combine(outputRoot, "components", "counter-card.mjs");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var hostRequirementsModulePath = Path.Combine(outputRoot, "__jazor", "razorvue-host.mjs");

        Assert.IsTrue(File.Exists(modulePath), $"RazorVue module was not generated: {modulePath}");
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(hostRequirementsModulePath), $"RazorVue host requirements module was not generated: {hostRequirementsModulePath}");

        var module = (await File.ReadAllTextAsync(modulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(module, "import { DemoButton as DemoButtonComponent } from \"demo/components\";");
        StringAssert.Contains(module, "\"label\": props.label");
        StringAssert.Contains(module, "\"disabled\": false");

        var hostRequirements = (await File.ReadAllTextAsync(hostRequirementsModulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(hostRequirements, "export const razorVueStyles = Object.freeze([\"demo/button.css\"]);");
        StringAssert.Contains(hostRequirements, "export const razorVuePluginRequirements = Object.freeze([\"demo-host\"]);");
        StringAssert.Contains(hostRequirements, "\"componentName\":\"CounterCard\"");
        StringAssert.Contains(hostRequirements, "\"componentId\":\"Demo.Sample.CounterCard\"");
        StringAssert.Contains(hostRequirements, "\"moduleId\":\"components/counter-card.mjs\"");
        StringAssert.Contains(hostRequirements, "\"relativeModulePath\":\"components/counter-card.mjs\"");
        StringAssert.Contains(hostRequirements, "\"sourceMapPath\":\"components/counter-card.mjs.map\"");
        StringAssert.Contains(hostRequirements, "\"originMapPath\":\"components/counter-card.mjs.origins.json\"");
        StringAssert.Contains(hostRequirements, "\"descriptorHash\":");
        StringAssert.Contains(hostRequirements, "\"hmrBoundaryKind\":");

        var manifest = LoadRazorVueManifestProjection(manifestPath);
        CollectionAssert.AreEqual(
            new[] { "demo/button.css" },
            RequireManifestStringList(manifest.Styles, nameof(RazorVueManifestModel.Styles), manifestPath).ToArray());
        CollectionAssert.AreEqual(
            new[] { "demo-host" },
            RequireManifestStringList(manifest.PluginRequirements, nameof(RazorVueManifestModel.PluginRequirements), manifestPath).ToArray());
        Assert.AreEqual("CounterCard", manifest.Modules[0].ComponentName);
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithPackagedCustomRazorVueLibrary_EmitsRazorVueBundleAndSidecars()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "PackagedCustomRazorVueSample");
        var authoringRoot = Path.Combine(workspace.RootPath, "Demo.Authoring");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        const string authoringPackageVersion = "1.0.0";

        var authoringProjectPath = CreatePackagedCustomAuthoringLibraryProject(authoringRoot, package.PackageVersion, authoringPackageVersion);
        var authoringPack = await RunDotNetAsync(
            package.RepoRoot,
            [
                "pack",
                authoringProjectPath,
                "-c",
                "Debug",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:RestoreSources={package.PackageOutputDirectory}",
                "-p:RestoreAdditionalProjectSources=https://api.nuget.org/v3/index.json",
                $"-p:RestorePackagesPath={restorePackagesPath}",
                "-o",
                package.PackageOutputDirectory
            ]);

        Assert.AreEqual(0, authoringPack.ExitCode, authoringPack.ToString());

        var projectPath = CreatePackagedCustomRazorVueHostProject(projectRoot, package.PackageVersion, authoringPackageVersion);
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
                $"-p:RestorePackagesPath={restorePackagesPath}"
            ]);

        Assert.AreEqual(0, build.ExitCode, build.ToString());

        var outputRoot = Path.Combine(projectRoot, "wwwroot");
        var moduleRoot = Path.Combine(outputRoot, "jazor");
        var componentModulePath = Path.Combine(moduleRoot, "components", "counter-card.mjs");
        var manifestPath = Path.Combine(moduleRoot, "jazor-manifest.json");
        var hostRequirementsModulePath = Path.Combine(moduleRoot, "__jazor", "razorvue-host.mjs");
        var bundlePath = Path.Combine(outputRoot, "app.bundle.js");
        var cssPath = Path.Combine(outputRoot, "app.bundle.razorvue.css");
        var hostContractPath = Path.Combine(outputRoot, "app.bundle.razorvue.host.json");

        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(hostRequirementsModulePath), $"RazorVue host requirements module was not generated: {hostRequirementsModulePath}");
        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(cssPath), $"RazorVue CSS sidecar was not generated: {cssPath}");
        Assert.IsTrue(File.Exists(hostContractPath), $"RazorVue host contract sidecar was not generated: {hostContractPath}");

        var componentModule = (await File.ReadAllTextAsync(componentModulePath)).ReplaceLineEndings("\n");
        var hostRequirementsModule = (await File.ReadAllTextAsync(hostRequirementsModulePath)).ReplaceLineEndings("\n");
        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        var css = (await File.ReadAllTextAsync(cssPath)).ReplaceLineEndings("\n");

        StringAssert.Contains(componentModule, "import { DemoButton as DemoButtonComponent } from \"demo/components\";");
        StringAssert.Contains(componentModule, "\"text\": props.label");
        StringAssert.Contains(componentModule, "\"disabled\": props.disabled");
        StringAssert.Contains(componentModule, "\"modelValue\": props.value");
        StringAssert.Contains(componentModule, "\"onUpdate:modelValue\": (__value) => emit(\"update:value\", __value)");
        StringAssert.Contains(componentModule, "header: (slotProps) => slots.header ? slots.header(slotProps) : null");
        StringAssert.Contains(hostRequirementsModule, "export const razorVueStyles = Object.freeze([\"demo/button.css\"]);");
        StringAssert.Contains(hostRequirementsModule, "export const razorVuePluginRequirements = Object.freeze([\"demo-host\"]);");
        StringAssert.Contains(hostRequirementsModule, "\"componentName\":\"CounterCard\"");
        StringAssert.Contains(hostRequirementsModule, "\"componentId\":\"PackagedCustomRazorVueSample.CounterCard\"");
        StringAssert.Contains(hostRequirementsModule, "\"moduleId\":\"components/counter-card.mjs\"");
        StringAssert.Contains(hostRequirementsModule, "\"relativeModulePath\":\"components/counter-card.mjs\"");
        StringAssert.Contains(hostRequirementsModule, "\"sourceMapPath\":\"components/counter-card.mjs.map\"");
        StringAssert.Contains(hostRequirementsModule, "\"originMapPath\":\"components/counter-card.mjs.origins.json\"");
        StringAssert.Contains(hostRequirementsModule, "\"descriptorHash\":");
        StringAssert.Contains(hostRequirementsModule, "\"hmrBoundaryKind\":");
        StringAssert.Contains(bundle, "razorVueHostRequirements");
        Assert.AreEqual("@import \"demo/button.css\";\n", css);

        var manifest = LoadRazorVueManifestProjection(manifestPath);
        CollectionAssert.AreEqual(
            new[] { "demo/button.css" },
            RequireManifestStringList(manifest.Styles, nameof(RazorVueManifestModel.Styles), manifestPath).ToArray());
        CollectionAssert.AreEqual(
            new[] { "demo-host" },
            RequireManifestStringList(manifest.PluginRequirements, nameof(RazorVueManifestModel.PluginRequirements), manifestPath).ToArray());
        var expectedHmrBoundary = (int)manifest.Modules[0].HmrBoundaryKind;
        var expectedRequiresHydration = manifest.Modules[0].RequiresHydration;
        var expectedSupportsSsr = manifest.Modules[0].SupportsSsr;

        using var hostContract = JsonDocument.Parse(await File.ReadAllTextAsync(hostContractPath));
        CollectionAssert.AreEqual(
            new[] { "demo/button.css" },
            GetStringArrayProperty(hostContract.RootElement, "Styles"));
        CollectionAssert.AreEqual(
            new[] { "demo-host" },
            GetStringArrayProperty(hostContract.RootElement, "PluginRequirements"));
        Assert.AreEqual("app.bundle.js.map", hostContract.RootElement.GetProperty("BundleSourceMapFile").GetString());
        Assert.AreEqual(
            "PackagedCustomRazorVueSample.CounterCard",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual(
            "components/counter-card.mjs",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ModuleId").GetString());
        Assert.AreEqual(
            "CounterCard",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentName").GetString());
        Assert.AreEqual(
            "components/counter-card.mjs",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("RelativeModulePath").GetString());
        Assert.AreEqual(
            "components/counter-card.mjs.map",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("SourceMapPath").GetString());
        Assert.AreEqual(
            "components/counter-card.mjs.origins.json",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("OriginMapPath").GetString());
        Assert.IsTrue(hostContract.RootElement.GetProperty("Modules")[0].TryGetProperty("DescriptorHash", out var descriptorHash));
        Assert.AreNotEqual(string.Empty, descriptorHash.GetString());
        Assert.AreEqual(expectedHmrBoundary, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("HmrBoundaryKind").GetInt32());
        Assert.AreEqual(expectedRequiresHydration, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("RequiresHydration").GetBoolean());
        Assert.AreEqual(expectedSupportsSsr, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("SupportsSsr").GetBoolean());
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithPackagedRazorVueVuetify_EmitsRazorVueBundleAndSidecars()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "PackagedRazorVueVuetifySample");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");

        WriteFile(
            Path.Combine(projectRoot, "PackagedRazorVueVuetifySample.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>true</JazorBundle>
                <JazorRazorVueOutputMode>legacy</JazorRazorVueOutputMode>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
                <JazorBundleOut>$(MSBuildProjectDirectory)\wwwroot\app.bundle.js</JazorBundleOut>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="ECMAScript.Vuetify" Version="$(JazorPackageVersion)" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """);
        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            namespace PackagedRazorVueVuetifySample;

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);
        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace PackagedRazorVueVuetifySample;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "ready";
            }
            """);
        WriteFile(
            Path.Combine(projectRoot, "ProfileForm.cs"),
            """
            using ECMAScript;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace PackagedRazorVueVuetifySample;

            [ECMAScriptModule("./components/profile-form")]
            public sealed class ProfileForm : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Name { get; set; }

                [Parameter]
                public EventCallback<string?> NameChanged { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.Label), "Name");
                    builder.AddAttribute(2, nameof(VTextField.ModelValue), Name);
                    builder.AddAttribute(3, nameof(VTextField.ModelValueChanged), NameChanged);
                    builder.CloseComponent();
                }
            }
            """);

        var projectPath = Path.Combine(projectRoot, "PackagedRazorVueVuetifySample.csproj");
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

        var outputRoot = Path.Combine(projectRoot, "wwwroot");
        var moduleRoot = Path.Combine(outputRoot, "jazor");
        var componentModulePath = Path.Combine(moduleRoot, "components", "profile-form.mjs");
        var manifestPath = Path.Combine(moduleRoot, "jazor-manifest.json");
        var hostRequirementsModulePath = Path.Combine(moduleRoot, "__jazor", "razorvue-host.mjs");
        var bundlePath = Path.Combine(outputRoot, "app.bundle.js");
        var cssPath = Path.Combine(outputRoot, "app.bundle.razorvue.css");
        var hostContractPath = Path.Combine(outputRoot, "app.bundle.razorvue.host.json");

        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(hostRequirementsModulePath), $"RazorVue host requirements module was not generated: {hostRequirementsModulePath}");
        Assert.IsTrue(File.Exists(bundlePath), $"Bundle was not generated: {bundlePath}");
        Assert.IsTrue(File.Exists(cssPath), $"RazorVue CSS sidecar was not generated: {cssPath}");
        Assert.IsTrue(File.Exists(hostContractPath), $"RazorVue host contract sidecar was not generated: {hostContractPath}");

        var componentModule = (await File.ReadAllTextAsync(componentModulePath)).ReplaceLineEndings("\n");
        var bundle = (await File.ReadAllTextAsync(bundlePath)).ReplaceLineEndings("\n");
        var css = (await File.ReadAllTextAsync(cssPath)).ReplaceLineEndings("\n");

        StringAssert.Contains(componentModule, "vuetify/components");
        StringAssert.Contains(componentModule, "\"modelValue\": props.name");
        StringAssert.Contains(componentModule, "\"onUpdate:modelValue\": (__value) => emit(\"update:name\", __value)");
        StringAssert.Contains(bundle, "razorVueHostRequirements");
        Assert.AreEqual("@import \"vuetify/styles\";\n", css);

        var manifest = LoadRazorVueManifestProjection(manifestPath);
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            RequireManifestStringList(manifest.Styles, nameof(RazorVueManifestModel.Styles), manifestPath).ToArray());
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            RequireManifestStringList(manifest.PluginRequirements, nameof(RazorVueManifestModel.PluginRequirements), manifestPath).ToArray());
        var expectedPackagedHmrBoundary = (int)manifest.Modules[0].HmrBoundaryKind;
        var expectedPackagedRequiresHydration = manifest.Modules[0].RequiresHydration;
        var expectedPackagedSupportsSsr = manifest.Modules[0].SupportsSsr;

        using var hostContract = JsonDocument.Parse(await File.ReadAllTextAsync(hostContractPath));
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            GetStringArrayProperty(hostContract.RootElement, "Styles"));
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            GetStringArrayProperty(hostContract.RootElement, "PluginRequirements"));
        Assert.AreEqual("app.bundle.js.map", hostContract.RootElement.GetProperty("BundleSourceMapFile").GetString());
        Assert.AreEqual(
            "PackagedRazorVueVuetifySample.ProfileForm",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ModuleId").GetString());
        Assert.AreEqual(
            "ProfileForm",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentName").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("RelativeModulePath").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs.map",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("SourceMapPath").GetString());
        Assert.AreEqual(
            "components/profile-form.mjs.origins.json",
            hostContract.RootElement.GetProperty("Modules")[0].GetProperty("OriginMapPath").GetString());
        Assert.IsTrue(hostContract.RootElement.GetProperty("Modules")[0].TryGetProperty("DescriptorHash", out var packagedDescriptorHash));
        Assert.AreNotEqual(string.Empty, packagedDescriptorHash.GetString());
        Assert.AreEqual(expectedPackagedHmrBoundary, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("HmrBoundaryKind").GetInt32());
        Assert.AreEqual(expectedPackagedRequiresHydration, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("RequiresHydration").GetBoolean());
        Assert.AreEqual(expectedPackagedSupportsSsr, hostContract.RootElement.GetProperty("Modules")[0].GetProperty("SupportsSsr").GetBoolean());
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgSfcConsumer_EmitsVueSfcArtifacts()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorVueSfcConsumer");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var projectPath = CreateExternalRazorVueSfcConsumerProject(projectRoot);

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
        var sfcPath = Path.Combine(outputRoot, "components", "external-dashboard.vue");
        var legacyModulePath = Path.Combine(outputRoot, "components", "external-dashboard.mjs");
        var hostRequirementsModulePath = Path.Combine(outputRoot, "__jazor", "razorvue-host.mjs");
        var hostModulePath = Path.Combine(outputRoot, "host", "app.mjs");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(sfcPath), $"RazorVue SFC was not generated: {sfcPath}");
        Assert.IsTrue(File.Exists(sfcPath + ".map"), $"RazorVue SFC source map was not generated: {sfcPath}.map");
        Assert.IsTrue(File.Exists(Path.ChangeExtension(sfcPath, ".vue.origins.json")), $"RazorVue SFC origins were not generated for: {sfcPath}");
        Assert.IsFalse(File.Exists(legacyModulePath), $"SFC output mode must not also emit legacy module: {legacyModulePath}");
        Assert.IsTrue(File.Exists(hostRequirementsModulePath), $"RazorVue host requirements module was not generated: {hostRequirementsModulePath}");
        Assert.IsTrue(File.Exists(hostModulePath), $"Host module was not generated: {hostModulePath}");

        var sfc = (await File.ReadAllTextAsync(sfcPath)).ReplaceLineEndings("\n");
        StringAssert.Contains(sfc, "<VApp>");
        StringAssert.Contains(sfc, "<VMain>");
        StringAssert.Contains(sfc, "<VContainer");
        StringAssert.Contains(sfc, "<VCardTitle>");
        StringAssert.Contains(sfc, "External RazorVue Consumer");
        StringAssert.Contains(sfc, "<VList");
        StringAssert.Contains(sfc, "item.title");
        StringAssert.Contains(sfc, "item.category");
        StringAssert.Contains(sfc, "item.isPinned");
        StringAssert.Contains(sfc, "from \"vuetify/components\"");
        StringAssert.Contains(sfc, "defineProps<{ items?: any }>()");
        Assert.IsFalse(sfc.Contains("text=\"External RazorVue Consumer\"", StringComparison.Ordinal), sfc);

        var hostRequirementsModule = (await File.ReadAllTextAsync(hostRequirementsModulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(hostRequirementsModule, "export const razorVueStyles = Object.freeze([\"vuetify/styles\"]);");
        StringAssert.Contains(hostRequirementsModule, "export const razorVuePluginRequirements = Object.freeze([\"vuetify\"]);");
        StringAssert.Contains(hostRequirementsModule, "\"componentId\":\"ExternalRazorVueSfcConsumer.ExternalDashboard\"");
        StringAssert.Contains(hostRequirementsModule, "\"relativeModulePath\":\"components/external-dashboard.vue\"");

        var razorVueManifest = LoadRazorVueManifestProjection(manifestPath);
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            RequireManifestStringList(razorVueManifest.Styles, nameof(RazorVueManifestModel.Styles), manifestPath).ToArray());
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            RequireManifestStringList(razorVueManifest.PluginRequirements, nameof(RazorVueManifestModel.PluginRequirements), manifestPath).ToArray());

        var module = razorVueManifest.Modules[0];
        Assert.AreEqual("ExternalRazorVueSfcConsumer.ExternalDashboard", module.ComponentId);
        Assert.AreEqual("ExternalDashboard", module.ComponentName);
        Assert.AreEqual("components/external-dashboard.vue", module.RelativeModulePath);
        Assert.AreEqual("components/external-dashboard.vue.map", module.SourceMapPath);
        Assert.AreEqual("components/external-dashboard.vue.origins.json", module.OriginMapPath);
    }

    [TestMethod]
    public async Task Build_LocalPackages_WithExternalRazorSgSfcConsumer_PureDenoPipeline_PassesInIsolatedWorkspace()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorVueSfcConsumer");
        var restorePackagesPath = Path.Combine(workspace.RootPath, "packages");
        var projectPath = CreateExternalRazorVueSfcConsumerProject(projectRoot);

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

        var hostJazorRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var consumerRoot = CreateExternalRazorVuePureDenoConsumer(Path.Combine(workspace.RootPath, "ExternalRazorVuePureDenoConsumer"));
        var consumerBuildRoot = Path.Combine(consumerRoot, ".deno-build-test");
        var consumerDistRoot = Path.Combine(consumerRoot, "dist-test");

        var denoEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["RAZORVUE_HOST_JAZOR_ROOT"] = hostJazorRoot,
            ["RAZORVUE_BUILD_ROOT"] = consumerBuildRoot,
            ["RAZORVUE_DIST_ROOT"] = consumerDistRoot,
            ["RAZORVUE_ROOT_COMPONENT_SELECTOR"] = "id:ExternalRazorVueSfcConsumer.ExternalDashboard",
            ["RAZORVUE_BROWSER_EXPECTED_TEXTS_JSON"] = JsonSerializer.Serialize(new[]
            {
                "External RazorVue Consumer",
                "Preview external pure Deno consumer",
                "Automation | Open",
                "Validate host requirements",
                "Runtime | Done",
                "Pinned"
            }),
            ["RAZORVUE_BROWSER_CLICK_BUTTON_TEXT"] = string.Empty,
            ["RAZORVUE_BROWSER_AFTER_CLICK_EXPECTED_TEXTS_JSON"] = "[]",
            ["JAZOR_EMIT_TOOL_PATH"] = Path.Combine(package.RepoRoot, "src", "Jazor.Emit", "bin", "Debug", "net11.0", "Jazor.Emit.dll")
        };

        var pipeline = await RunDotNetWithEnvironmentAsync(
            consumerRoot,
            [
                "run",
                "--file",
                Path.Combine(consumerRoot, "scripts", "run-deno.cs"),
                "--",
                "task",
                "test"
            ],
            denoEnvironment);

        Assert.AreEqual(0, pipeline.ExitCode, pipeline.ToString());
        var output = (pipeline.StandardOutput + pipeline.StandardError).ReplaceLineEndings("\n");

        var distHtmlPath = Path.Combine(consumerDistRoot, "index.html");
        var distJsPath = Path.Combine(consumerDistRoot, "jazor", "client-entry.js");
        var distCssPath = Path.Combine(consumerDistRoot, "jazor", "client-entry.css");
        Assert.IsTrue(File.Exists(distHtmlPath), $"Expected Deno dist HTML was not generated: {distHtmlPath}");
        Assert.IsTrue(File.Exists(distJsPath), $"Expected Deno dist JS was not generated: {distJsPath}");
        Assert.IsTrue(File.Exists(distCssPath), $"Expected Deno dist CSS was not generated: {distCssPath}");
        Assert.IsTrue(File.Exists(distJsPath + ".map"), $"Expected Deno dist JS source map was not generated: {distJsPath}.map");
        Assert.IsTrue(File.Exists(distCssPath + ".map"), $"Expected Deno dist CSS source map was not generated: {distCssPath}.map");

        var distHtml = (await File.ReadAllTextAsync(distHtmlPath)).ReplaceLineEndings("\n");
        var distJs = (await File.ReadAllTextAsync(distJsPath)).ReplaceLineEndings("\n");
        StringAssert.Contains(distHtml, "<script type=\"module\" src=\"./jazor/client-entry.js\"></script>");
        StringAssert.Contains(distHtml, "<link rel=\"stylesheet\" href=\"./jazor/client-entry.css\" />");
        StringAssert.Contains(distJs, "globalThis.__VUE_OPTIONS_API__ = true;");
        StringAssert.Contains(distJs, "globalThis.__VUE_PROD_DEVTOOLS__ = false;");
        StringAssert.Contains(distJs, "globalThis.__VUE_PROD_HYDRATION_MISMATCH_DETAILS__ = false;");
        Assert.IsFalse(
            ContainsVueModuleSpecifier(distJs),
            "Bundled browser entry should not retain unresolved .vue imports.");
    }

    [TestMethod]
    public async Task Build_LocalPackages_RazorVueTodoListSample_PureDenoPipeline_PassesInIsolatedWorkspace()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var sampleSourceRoot = Path.Combine(package.RepoRoot, "samples", "RazorVue.TodoList");
        var sampleRoot = Path.Combine(workspace.RootPath, "RazorVue.TodoList");
        var hostJazorRoot = Path.Combine(sampleRoot, "Todo.Host", "jazor");
        var hostBrowserAssetRoot = Path.Combine(sampleRoot, "Todo.Host", "wwwroot", "jazor");
        var consumerRoot = Path.Combine(sampleRoot, "Todo.Host", "consumer");
        var consumerBuildRoot = Path.Combine(consumerRoot, ".deno-build-test");
        var consumerDistRoot = Path.Combine(consumerRoot, "dist-test");

        CopyDirectory(sampleSourceRoot, sampleRoot);

        var sampleBuildEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["JAZOR_SAMPLE_REPO_ROOT"] = package.RepoRoot,
            ["JAZOR_SAMPLE_PACKAGE_OUTPUT"] = Path.Combine(workspace.RootPath, "sample-packages"),
            ["JAZOR_SAMPLE_RESTORE_PACKAGES_ROOT"] = Path.Combine(workspace.RootPath, "sample-restore-packages")
        };

        var buildLocal = await RunDotNetWithEnvironmentAsync(
            package.RepoRoot,
            [
                "run",
                "--file",
                Path.Combine(sampleRoot, "build-local.cs"),
                "--",
                "--configuration",
                "Debug",
                "--base-output-path",
                Path.Combine(workspace.RootPath, "sample-out"),
                "--base-intermediate-output-path",
                Path.Combine(workspace.RootPath, "sample-obj")
            ],
            sampleBuildEnvironment);

        Assert.AreEqual(0, buildLocal.ExitCode, buildLocal.ToString());

        var todoAppSfcPath = Path.Combine(hostJazorRoot, "components", "todo-app.vue");
        var todoSummaryCardSfcPath = Path.Combine(hostJazorRoot, "components", "todo-summary-card.vue");
        var manifestPath = Path.Combine(hostJazorRoot, "jazor-manifest.json");
        var hostRequirementsModulePath = Path.Combine(hostJazorRoot, "__jazor", "razorvue-host.mjs");

        Assert.IsTrue(File.Exists(todoAppSfcPath), $"Expected TodoApp SFC was not generated: {todoAppSfcPath}");
        Assert.IsTrue(File.Exists(todoSummaryCardSfcPath), $"Expected TodoSummaryCard SFC was not generated: {todoSummaryCardSfcPath}");
        Assert.IsTrue(File.Exists(manifestPath), $"Expected manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(hostRequirementsModulePath), $"Expected RazorVue host requirements module was not generated: {hostRequirementsModulePath}");

        var todoAppSfc = (await File.ReadAllTextAsync(todoAppSfcPath)).ReplaceLineEndings("\n");
        StringAssert.Contains(todoAppSfc, "<VApp>");
        StringAssert.Contains(todoAppSfc, "<VMain>");
        StringAssert.Contains(todoAppSfc, "<VCardTitle>");
        StringAssert.Contains(todoAppSfc, "RazorVue Todo Workspace");
        StringAssert.Contains(todoAppSfc, ":fluid=\"true\"");
        StringAssert.Contains(todoAppSfc, ":cols=\"12\"");
        StringAssert.Contains(todoAppSfc, "from \"vuetify/components\"");
        Assert.IsFalse(todoAppSfc.Contains("text=\"RazorVue Todo Workspace\"", StringComparison.Ordinal), todoAppSfc);

        var razorVueManifest = LoadRazorVueManifestProjection(manifestPath);
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            RequireManifestStringList(razorVueManifest.Styles, nameof(RazorVueManifestModel.Styles), manifestPath).ToArray());
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            RequireManifestStringList(razorVueManifest.PluginRequirements, nameof(RazorVueManifestModel.PluginRequirements), manifestPath).ToArray());

        var denoEnvironment = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["RAZORVUE_HOST_JAZOR_ROOT"] = hostJazorRoot,
            ["RAZORVUE_HOST_WWWROOT_ROOT"] = Path.Combine(sampleRoot, "Todo.Host", "wwwroot"),
            ["RAZORVUE_BUILD_ROOT"] = consumerBuildRoot,
            ["RAZORVUE_DIST_ROOT"] = consumerDistRoot,
            ["JAZOR_EMIT_TOOL_PATH"] = Path.Combine(package.RepoRoot, "src", "Jazor.Emit", "bin", "Debug", "net11.0", "Jazor.Emit.dll")
        };

        var ssrSmoke = await RunDotNetWithEnvironmentAsync(
            consumerRoot,
            [
                "run",
                "--file",
                Path.Combine(consumerRoot, "scripts", "run-deno.cs"),
                "--",
                "task",
                "smoke:ssr"
            ],
            denoEnvironment);
        Assert.AreEqual(0, ssrSmoke.ExitCode, ssrSmoke.ToString());

        var bundleApiSmoke = await RunDotNetWithEnvironmentAsync(
            consumerRoot,
            [
                "run",
                "--file",
                Path.Combine(consumerRoot, "scripts", "run-deno.cs"),
                "--",
                "task",
                "smoke:bundle-api"
            ],
            denoEnvironment);
        Assert.AreEqual(0, bundleApiSmoke.ExitCode, bundleApiSmoke.ToString());

        var browserBuild = await RunDotNetWithEnvironmentAsync(
            consumerRoot,
            [
                "run",
                "--file",
                Path.Combine(consumerRoot, "scripts", "run-deno.cs"),
                "--",
                "task",
                "build"
            ],
            denoEnvironment);
        Assert.AreEqual(0, browserBuild.ExitCode, browserBuild.ToString());

        var browserSmokeEnvironment = new Dictionary<string, string>(denoEnvironment, StringComparer.OrdinalIgnoreCase)
        {
            ["RAZORVUE_BROWSER_SKIP_BUILD"] = "1"
        };
        var browserSmoke = await RunDotNetWithEnvironmentAsync(
            consumerRoot,
            [
                "run",
                "--file",
                Path.Combine(consumerRoot, "scripts", "run-deno.cs"),
                "--",
                "task",
                "smoke:browser"
            ],
            browserSmokeEnvironment);
        Assert.AreEqual(0, browserSmoke.ExitCode, browserSmoke.ToString());

        var distHtmlPath = Path.Combine(consumerDistRoot, "index.html");
        var distJsPath = Path.Combine(consumerDistRoot, "jazor", "client-entry.js");
        var distCssPath = Path.Combine(consumerDistRoot, "jazor", "client-entry.css");
        Assert.IsTrue(File.Exists(distHtmlPath), $"Expected Deno dist HTML was not generated: {distHtmlPath}");
        Assert.IsTrue(File.Exists(distJsPath), $"Expected Deno dist JS was not generated: {distJsPath}");
        Assert.IsTrue(File.Exists(distCssPath), $"Expected Deno dist CSS was not generated: {distCssPath}");
        Assert.IsTrue(File.Exists(distJsPath + ".map"), $"Expected Deno dist JS source map was not generated: {distJsPath}.map");
        Assert.IsTrue(File.Exists(distCssPath + ".map"), $"Expected Deno dist CSS source map was not generated: {distCssPath}.map");

        var distHtml = (await File.ReadAllTextAsync(distHtmlPath)).ReplaceLineEndings("\n");
        var distJs = (await File.ReadAllTextAsync(distJsPath)).ReplaceLineEndings("\n");
        StringAssert.Contains(distHtml, "<script type=\"module\" src=\"./jazor/client-entry.js\"></script>");
        StringAssert.Contains(distHtml, "<link rel=\"stylesheet\" href=\"./jazor/client-entry.css\" />");
        StringAssert.Contains(distJs, "globalThis.__VUE_OPTIONS_API__ = true;");
        StringAssert.Contains(distJs, "globalThis.__VUE_PROD_DEVTOOLS__ = false;");
        StringAssert.Contains(distJs, "globalThis.__VUE_PROD_HYDRATION_MISMATCH_DETAILS__ = false;");
        Assert.IsFalse(
            ContainsVueModuleSpecifier(distJs),
            "Bundled browser entry should not retain unresolved .vue imports.");
        Assert.IsTrue(File.Exists(Path.Combine(hostBrowserAssetRoot, "client-entry.js")), "Expected host browser JS bundle was not copied into wwwroot/jazor.");
        Assert.IsTrue(File.Exists(Path.Combine(hostBrowserAssetRoot, "client-entry.css")), "Expected host browser CSS bundle was not copied into wwwroot/jazor.");
    }

    private static async Task<LocalPackageFixture> CreateLocalPackageAsync()
    {
        var repoRoot = FindRepoRoot();
        var packageOutputDirectory = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg", Guid.NewGuid().ToString("N"));
        var packageBuildOutputRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "package-out", Guid.NewGuid().ToString("N"));
        var packageBuildIntermediateRoot = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "package-obj", Guid.NewGuid().ToString("N"));
        var ecmascriptOutput = Path.Combine(packageBuildOutputRoot, "ECMAScript", "bin", "Debug", "net11.0", "ECMAScript.dll");
        var contractOutput = Path.Combine(packageBuildOutputRoot, "ECMAScript.Contract", "bin", "Debug", "netstandard2.0", "ECMAScript.Contract.dll");
        var vuetifyOutput = Path.Combine(packageBuildOutputRoot, "ECMAScript.Vuetify", "bin", "Debug", "net11.0", "ECMAScript.Vuetify.dll");
        var analyzerOutput = Path.Combine(packageBuildOutputRoot, "Jazor.Analyzer", "bin", "Debug", "netstandard2.0", "Jazor.Analyzer.dll");
        var emitPublishDirectory = Path.Combine(packageBuildOutputRoot, "Jazor.Emit", "bin", "Debug", "net11.0", "publish");
        var emitPublishOutput = Path.Combine(emitPublishDirectory, "Jazor.Emit.dll");

        if (Directory.Exists(packageOutputDirectory))
            Directory.Delete(packageOutputDirectory, recursive: true);

        Directory.CreateDirectory(packageOutputDirectory);

        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript", "ECMAScript.csproj"),
            ecmascriptOutput,
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Contract", "ECMAScript.Contract.csproj"),
            contractOutput,
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Vuetify", "ECMAScript.Vuetify.csproj"),
            vuetifyOutput,
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "ECMAScript.VueRoute.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.VueRoute", "bin", "Debug", "net11.0", "ECMAScript.VueRoute.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "ECMAScript.Pinia.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.Pinia", "bin", "Debug", "net11.0", "ECMAScript.Pinia.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing", "ECMAScript.Pinia.Testing.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.Pinia.Testing", "bin", "Debug", "net11.0", "ECMAScript.Pinia.Testing.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.TDesign", "ECMAScript.TDesign.csproj"),
            Path.Combine(packageBuildOutputRoot, "ECMAScript.TDesign", "bin", "Debug", "net11.0", "ECMAScript.TDesign.dll"),
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "Jazor.Analyzer", "Jazor.Analyzer.csproj"),
            analyzerOutput,
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await EnsureProjectPublishedAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "Jazor.Emit", "Jazor.Emit.csproj"),
            emitPublishOutput,
            emitPublishDirectory,
            packageBuildOutputRoot,
            packageBuildIntermediateRoot);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "restore",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        var jazorPack = await RunDotNetAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory,
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        Assert.AreEqual(0, jazorPack.ExitCode, jazorPack.ToString());
        Assert.IsFalse(
            jazorPack.ToString().Contains("NU5118", StringComparison.OrdinalIgnoreCase),
            "Jazor package emitted duplicate pack warnings." + Environment.NewLine + jazorPack);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "ECMAScript.Vuetify", "ECMAScript.Vuetify.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory,
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "ECMAScript.VueRoute.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory,
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "ECMAScript.Pinia.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory,
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing", "ECMAScript.Pinia.Testing.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory,
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "ECMAScript.TDesign", "ECMAScript.TDesign.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory,
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);

        var packageVersion = DiscoverPackageVersion(packageOutputDirectory, "Jazor");

        return new LocalPackageFixture(
            repoRoot,
            packageVersion,
            packageOutputDirectory,
            GetPackagePath(packageOutputDirectory, packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Vuetify", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.VueRoute", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia.Testing", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.TDesign", packageVersion),
            GetBundledDenoPath(emitPublishDirectory));
    }

    private static async Task RunDotNetAndAssertAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var result = await RunDotNetAsync(workingDirectory, arguments);
        Assert.AreEqual(0, result.ExitCode, result.ToString());
    }

    private static async Task EnsureProjectBuiltAsync(
        string repoRoot,
        string projectPath,
        string expectedOutputPath,
        string packageBuildOutputRoot,
        string packageBuildIntermediateRoot)
    {
        // 生产打包验证不能只看 DLL 是否存在；Directory.Build.props 或依赖版本变更时旧产物会污染 nupkg。
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "build",
                projectPath,
                "-c",
                "Debug",
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);

        Assert.IsTrue(File.Exists(expectedOutputPath), $"Expected build output was not produced: {expectedOutputPath}");
    }

    private static async Task EnsureProjectPublishedAsync(
        string repoRoot,
        string projectPath,
        string expectedOutputPath,
        string publishDirectory,
        string packageBuildOutputRoot,
        string packageBuildIntermediateRoot)
    {
        // publish 输出同样走 MSBuild 增量判断，避免复用旧工具导致包内行为和源码不一致。
        await RunDotNetAndAssertAsync(
            repoRoot,
            [
                "publish",
                projectPath,
                "-c",
                "Debug",
                "-o",
                publishDirectory,
                "/m:1",
                "/p:BuildInParallel=false",
                $"-p:JazorIsolatedBaseOutputRoot={EnsureTrailingDirectorySeparator(packageBuildOutputRoot)}",
                $"-p:JazorIsolatedBaseIntermediateOutputRoot={EnsureTrailingDirectorySeparator(packageBuildIntermediateRoot)}",
                "/nr:false",
                "-p:UseSharedCompilation=false"
            ]);

        Assert.IsTrue(File.Exists(expectedOutputPath), $"Expected publish output was not produced: {expectedOutputPath}");
    }

    private static async Task AssertSdkConsumerBrowserEntryAsync(string browserEntryPath, string unresolvedVueImportMessage)
    {
        var browserEntry = (await File.ReadAllTextAsync(browserEntryPath)).ReplaceLineEndings("\n");
        StringAssert.Contains(browserEntry, "mountSdkConsumer");
        StringAssert.Contains(browserEntry, "CatalogPage");
        StringAssert.Contains(browserEntry, "DetailPage");
        StringAssert.Contains(browserEntry, "razorVueHostRequirements");
        StringAssert.Contains(browserEntry, "razorVueConsumerRoutes");
        StringAssert.Contains(browserEntry, "Array.isArray(routesOrSelector)");
        Assert.IsFalse(
            ContainsVueModuleSpecifier(browserEntry),
            unresolvedVueImportMessage);
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

    private static async Task<ProcessResult> RunDotNetWithEnvironmentAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string>? environmentVariables = null)
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

        if (environmentVariables is not null)
        {
            foreach (var pair in environmentVariables)
                startInfo.Environment[pair.Key] = pair.Value;
        }

        startInfo.Environment["DOTNET_CLI_HOME"] = Path.Combine(FindRepoRoot(), ".dotnet");
        startInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        startInfo.Environment["MSBUILDDISABLENODEREUSE"] = "1";
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

    private static bool ContainsVueModuleSpecifier(string script)
        => Regex.IsMatch(
            script,
            """\b(?:import|export)\s+(?:[^"'`]*?\s+from\s*)?["'](?<specifier>[^"'`]+\.vue)["']|\bimport\s*\(\s*["'](?<dynamic>[^"'`]+\.vue)["']\s*\)""",
            RegexOptions.CultureInvariant);

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

    private static string ReadPackageEntryText(string packagePath, string entryName)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var entry = archive.GetEntry(entryName)
            ?? throw new FileNotFoundException($"Package entry '{entryName}' was not found in '{packagePath}'.", entryName);

        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);

        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, directory);
            if (ShouldSkip(relativePath))
                continue;

            Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            if (ShouldSkip(relativePath))
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

    private static string CreatePackagedCustomAuthoringLibraryProject(
        string projectRoot,
        string jazorPackageVersion,
        string authoringPackageVersion)
    {
        Directory.CreateDirectory(projectRoot);
        var projectPath = Path.Combine(projectRoot, "Demo.Authoring.csproj");

        // This simulates a third-party authoring package that ships independently from the host app.
        WriteFile(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <PackageId>Demo.Authoring</PackageId>
                <Version>{{authoringPackageVersion}}</Version>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="{{jazorPackageVersion}}" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(projectRoot, "DemoButton.cs"),
            """
            using ECMAScript.VueContract;
            using ECMAScript.VueContract.Descriptor;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace Demo.Authoring;

            [VueLibraryComponent("demo/components", "DemoButton")]
            [VueLibraryStyle("demo/button.css")]
            [VueLibraryPluginRequirement("demo-host")]
            [VueLibraryComponentFlags(VueComponentFlags.SupportsModelValue | VueComponentFlags.RequiresExplicitChildren)]
            [VueProp(nameof(Label), Name = "text")]
            [VueProp(nameof(Value), Name = "modelValue", AcceptsBinding = true, Required = true)]
            [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
            [VueSlot(nameof(Header), Name = "header", ContextTypeName = "string", ContextParameterName = "slotProps")]
            public sealed class DemoButton : ComponentBase, IVueLibraryComponent
            {
                [Parameter]
                public string? Label { get; set; }

                [Parameter]
                public bool Disabled { get; set; }

                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public RenderFragment<string>? Header { get; set; }
            }
            """);

        return projectPath;
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
                <JazorEmit>true</JazorEmit>
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

    private static string CreateDefaultOutputRazorVueSampleProject(string hostRoot, LocalPackageFixture package)
    {
        Directory.CreateDirectory(hostRoot);

        var contractProjectPath = Path.Combine(package.RepoRoot, "src", "ECMAScript.Contract", "ECMAScript.Contract.csproj");
        var vuetifyProjectPath = Path.Combine(package.RepoRoot, "src", "ECMAScript.Vuetify", "ECMAScript.Vuetify.csproj");
        var projectPath = Path.Combine(hostRoot, "RazorVueDefaultOutput.Host.csproj");

        WriteFile(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorRazorVueOutputMode>legacy</JazorRazorVueOutputMode>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>

              <ItemGroup>
                <ProjectReference Include="{{contractProjectPath}}" />
                <ProjectReference Include="{{vuetifyProjectPath}}" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(hostRoot, "Program.cs"),
            """
            namespace RazorVueDefaultOutput.Host;

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);

        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace RazorVueDefaultOutput.Host;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "ready";
            }
            """);

        WriteFile(
            Path.Combine(hostRoot, "ProfileForm.cs"),
            """
            using ECMAScript;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RazorVueDefaultOutput.Host;

            [ECMAScriptModule("./components/profile-form")]
            public sealed class ProfileForm : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Name { get; set; }

                [Parameter]
                public EventCallback<string?> NameChanged { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.Label), "Name");
                    builder.AddAttribute(2, nameof(VTextField.ModelValue), Name);
                    builder.AddAttribute(3, nameof(VTextField.ModelValueChanged), NameChanged);
                    builder.CloseComponent();
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
                <JazorEmit>true</JazorEmit>
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

    private static string CreateWebHostWithColocatedConsumerProject(string projectRoot)
    {
        Directory.CreateDirectory(projectRoot);
        var consumerRoot = Path.Combine(projectRoot, "consumer");
        var projectPath = Path.Combine(projectRoot, "WebHostConsumerOutput.csproj");

        WriteFile(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk.Web">
              <PropertyGroup>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <RazorLangVersion>11.0</RazorLangVersion>
                <UseRazorSourceGenerator>true</UseRazorSourceGenerator>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorRazorVueOutputMode>sfc</JazorRazorVueOutputMode>
                <JazorRazorVueEnableRazorSgIntegration>true</JazorRazorVueEnableRazorSgIntegration>
                <JazorOutDir>$(MSBuildProjectDirectory)\jazor\</JazorOutDir>
                <JazorPublishMaterializeEnabled>true</JazorPublishMaterializeEnabled>
                <JazorConsumerRoot>$(MSBuildProjectDirectory)\consumer</JazorConsumerRoot>
              </PropertyGroup>

              <ItemGroup>
                <Compile Remove="consumer\**" />
                <Content Remove="consumer\**" />
                <EmbeddedResource Remove="consumer\**" />
                <None Remove="consumer\**" />
                <Compile Remove="jazor\**" />
                <Content Remove="jazor\**" />
                <EmbeddedResource Remove="jazor\**" />
                <None Remove="jazor\**" />
                <Compile Remove="wwwroot\jazor\**" />
                <Content Remove="wwwroot\jazor\**" />
                <EmbeddedResource Remove="wwwroot\jazor\**" />
                <None Remove="wwwroot\jazor\**" />
              </ItemGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="ECMAScript.Vuetify" Version="$(JazorPackageVersion)" />
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
            Path.Combine(projectRoot, "_Imports.razor"),
            """
            @using Microsoft.AspNetCore.Components
            @using ECMAScript.Vuetify
            """);

        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace WebHostConsumerOutput;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "ready";
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "CatalogPage.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace WebHostConsumerOutput;

            [ECMAScriptModule("./components/catalog-page")]
            public partial class CatalogPage : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "CatalogPage.razor"),
            """
            <VContainer Fluid="true">
                <VRow>
                    <VCol Cols="12">
                        <VCard class="catalog-card">
                            <VCardTitle>Catalog</VCardTitle>
                            <VCardText>Catalog body</VCardText>
                        </VCard>
                    </VCol>
                </VRow>
            </VContainer>
            """);

        WriteFile(
            Path.Combine(projectRoot, "DetailPage.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace WebHostConsumerOutput;

            [ECMAScriptModule("./components/detail-page")]
            public partial class DetailPage : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "DetailPage.razor"),
            """
            <VContainer Fluid="true">
                <VCard class="detail-card">
                    <VCardTitle>Detail</VCardTitle>
                    <VCardText>Detail body</VCardText>
                </VCard>
            </VContainer>
            """);

        Directory.CreateDirectory(consumerRoot);

        WriteFile(
            Path.Combine(consumerRoot, "deno.json"),
            """
            {
              "nodeModulesDir": "auto",
              "imports": {
                "vue": "npm:vue@3.5.21",
                "vue/server-renderer": "npm:@vue/server-renderer@3.5.21",
                "vuetify": "npm:vuetify@3.10.8",
                "vuetify/components": "npm:vuetify@3.10.8/components",
                "vuetify/directives": "npm:vuetify@3.10.8/directives",
                "vuetify/styles": "npm:vuetify@3.10.8/styles"
              },
              "tasks": {
                "build": "deno run -A scripts/build.ts"
              }
            }
            """);

        WriteFile(
            Path.Combine(consumerRoot, "index.html"),
            """
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="utf-8" />
                <title>SDK Consumer Host</title>
                <!-- razorvue:styles -->
              </head>
              <body>
                <div id="app"></div>
                <!-- razorvue:script -->
              </body>
            </html>
            """);

        WriteFile(
            Path.Combine(consumerRoot, "scripts", "run-deno.cs"),
            """
            #!/usr/bin/env dotnet run
            #:package DenoHost.Core@2.7.14
            #:package DenoHost.Runtime.win-x64@2.7.14

            using DenoHost.Core;

            var consumerRoot = Path.GetDirectoryName(Path.GetDirectoryName(GetScriptPath()))
                ?? throw new InvalidOperationException("Cannot determine SDK consumer root.");

            await Deno.Execute(
                new DenoExecuteBaseOptions
                {
                    WorkingDirectory = consumerRoot
                },
                args);

            static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = "")
                => path;
            """);

        WriteFile(
            Path.Combine(consumerRoot, "scripts", "build.ts"),
            """
            import { dirname, join, relative, resolve } from "node:path";
            import { fileURLToPath } from "node:url";

            const consumerRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
            const hostJazorRoot = resolveRequiredEnvironmentPath("RAZORVUE_HOST_JAZOR_ROOT");
            const hostWwwrootRoot = resolveRequiredEnvironmentPath("RAZORVUE_HOST_WWWROOT_ROOT");
            const distRoot = resolve(consumerRoot, "dist");
            const browserBundleDirectory = resolve(distRoot, "jazor");
            const hostBrowserBundleDirectory = resolve(hostWwwrootRoot, "jazor");
            const buildRoot = resolve(consumerRoot, ".deno-build", `pid-${Deno.pid}`);
            const browserGeneratedRoot = resolve(buildRoot, "generated-browser");
            const ssrGeneratedRoot = resolve(buildRoot, "generated-ssr");
            const clientEntryPath = resolve(buildRoot, "client-entry.mjs");
            const ssrEntryPath = resolve(buildRoot, "ssr-entry.mjs");
            const vueFeatureFlagsPath = resolve(buildRoot, "vue-feature-flags.mjs");
            const resultPath = resolve(buildRoot, "razorvue-consumer-entry.json");
            const emitToolPath = readRequiredText("JAZOR_EMIT_TOOL_PATH");

            await emptyDirectory(buildRoot);
            await emptyDirectory(distRoot);
            await Deno.mkdir(browserBundleDirectory, { recursive: true });

            const entryOutput = await new Deno.Command("dotnet", {
              cwd: resolve(consumerRoot, "..", "..", ".."),
              args: [
                emitToolPath,
                "razorvue-consumer-entry",
                "--host-root", hostJazorRoot,
                "--out", buildRoot,
                "--client-runtime", resolve(consumerRoot, "src", "runtime-client.js"),
                "--ssr-runtime", resolve(consumerRoot, "src", "runtime-ssr.js"),
                "--client-runtime-export", "mountSdkConsumer",
                "--ssr-runtime-export", "runSdkConsumerSsr",
                "--ssr-execute-export", "executeSdkConsumerSsr",
                "--component", "CatalogPage=id:WebHostConsumerOutput.CatalogPage",
                "--component", "DetailPage=id:WebHostConsumerOutput.DetailPage",
                "--mode", "both",
                "--production", "true",
                "--clean", "true",
                "--write-result", resultPath
              ],
              stdin: "null",
              stdout: "piped",
              stderr: "piped"
            }).output();

            if (!entryOutput.success) {
              throw new Error(new TextDecoder().decode(entryOutput.stderr).trim() || "SDK consumer entry generation failed.");
            }

            const bundleOutput = await new Deno.Command(Deno.execPath(), {
              cwd: consumerRoot,
              args: [
                "bundle",
                "--platform", "browser",
                "--format", "esm",
                "--packages=bundle",
                "--sourcemap=linked",
                "--outdir", browserBundleDirectory,
                clientEntryPath
              ],
              stdin: "null",
              stdout: "piped",
              stderr: "piped"
            }).output();

            if (!bundleOutput.success) {
              throw new Error(new TextDecoder().decode(bundleOutput.stderr).trim() || "SDK consumer browser bundle failed.");
            }

            const entryFilePath = join(browserBundleDirectory, "client-entry.js");
            if (!(await fileExists(entryFilePath))) {
              throw new Error(`SDK consumer bundle did not produce '${entryFilePath}'.`);
            }

            const cssFiles = (await collectFiles(browserBundleDirectory))
              .filter((file) => file.endsWith(".css"))
              .map((file) => `./${relative(distRoot, file).replaceAll("\\", "/")}`)
              .sort((left, right) => left.localeCompare(right, "en"));

            const template = await Deno.readTextFile(resolve(consumerRoot, "index.html"));
            const cssMarkup = cssFiles.map((file) => `    <link rel="stylesheet" href="${file}" />`).join("\\n");
            const outputHtml = template
              .replace("    <!-- razorvue:styles -->", cssMarkup.length === 0 ? "    <!-- razorvue:styles -->" : cssMarkup)
              .replace(
                "    <!-- razorvue:script -->",
                `    <script type="module" src="./${relative(distRoot, entryFilePath).replaceAll("\\", "/")}"></script>`
              );

            await Deno.writeTextFile(resolve(distRoot, "index.html"), outputHtml);
            await copyDirectory(browserBundleDirectory, hostBrowserBundleDirectory);
            console.log(`SDK Deno build emitted /jazor/client-entry.js and ${cssFiles.length} CSS asset(s).`);

            function readRequiredText(name: string): string {
              const value = Deno.env.get(name)?.trim();
              if (value === undefined || value.length === 0) {
                throw new Error(`Missing required environment variable '${name}'.`);
              }

              return value;
            }

            function resolveRequiredEnvironmentPath(name: string): string {
              return resolve(readRequiredText(name));
            }

            async function emptyDirectory(path: string): Promise<void> {
              await Deno.remove(path, { recursive: true }).catch((error: unknown) => {
                if (!(error instanceof Deno.errors.NotFound)) {
                  throw error;
                }
              });
              await Deno.mkdir(path, { recursive: true });
            }

            async function fileExists(path: string): Promise<boolean> {
              try {
                await Deno.stat(path);
                return true;
              } catch (error) {
                if (error instanceof Deno.errors.NotFound) {
                  return false;
                }

                throw error;
              }
            }

            async function collectFiles(path: string): Promise<string[]> {
              const files: string[] = [];
              await collectFilesCore(path, files);
              return files.sort((left, right) => left.localeCompare(right, "en"));
            }

            async function collectFilesCore(path: string, files: string[]): Promise<void> {
              for await (const entry of Deno.readDir(path)) {
                const entryPath = join(path, entry.name);
                if (entry.isDirectory) {
                  await collectFilesCore(entryPath, files);
                  continue;
                }

                if (entry.isFile) {
                  files.push(entryPath);
                }
              }
            }

            async function copyDirectory(source: string, destination: string): Promise<void> {
              await emptyDirectory(destination);
              for await (const entry of Deno.readDir(source)) {
                const sourcePath = join(source, entry.name);
                const destinationPath = join(destination, entry.name);
                if (entry.isDirectory) {
                  await copyDirectory(sourcePath, destinationPath);
                  continue;
                }

                if (entry.isFile) {
                  await Deno.mkdir(dirname(destinationPath), { recursive: true });
                  await Deno.copyFile(sourcePath, destinationPath);
                }
              }
            }
            """);

        WriteFile(
            Path.Combine(consumerRoot, "src", "runtime-client.js"),
            """
            import { createApp, h } from "vue";
            import "vuetify/styles";
            import "./style.css";

            export function mountSdkConsumer(components, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
              if (hostRequirements === null || typeof hostRequirements !== "object") {
                throw new Error("Missing RazorVue host requirements.");
              }

              const CatalogPage = components?.CatalogPage;
              if (typeof CatalogPage !== "object" && typeof CatalogPage !== "function") {
                throw new Error("SDK consumer expected a CatalogPage component export.");
              }

              const DetailPage = components?.DetailPage;
              if (typeof DetailPage !== "object" && typeof DetailPage !== "function") {
                throw new Error("SDK consumer expected a DetailPage component export.");
              }

              const hasExplicitRoutes = Array.isArray(routesOrSelector);
              const selector = hasExplicitRoutes ? maybeSelector : routesOrSelector;
              const app = createApp({
                render() {
                  return h("main", { class: "sdk-consumer-shell" }, [
                    h(CatalogPage, { title: "Catalog from SDK consumer" }),
                    h(DetailPage, { title: "Detail from SDK consumer" })
                  ]);
                }
              });

              app.mount(selector);
              return app;
            }
            """);

        WriteFile(
            Path.Combine(consumerRoot, "src", "runtime-ssr.js"),
            """
            export async function runSdkConsumerSsr(components, hostRequirements, razorVueConsumerRoutes) {
              void components;
              void hostRequirements;
              void razorVueConsumerRoutes;
              return "<div>sdk-consumer-ssr</div>";
            }
            """);

        WriteFile(
            Path.Combine(consumerRoot, "src", "style.css"),
            """
            .sdk-consumer-shell {
              display: grid;
              gap: 1rem;
            }
            """);

        return projectPath;
    }

    private static string CreatePackagedCustomRazorVueHostProject(
        string hostRoot,
        string jazorPackageVersion,
        string authoringPackageVersion)
    {
        Directory.CreateDirectory(hostRoot);
        var projectPath = Path.Combine(hostRoot, "PackagedCustomRazorVueSample.csproj");

        // The host consumes only packaged assets here so RazorVue discovery crosses pack/restore boundaries.
        WriteFile(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>true</JazorBundle>
                <JazorRazorVueOutputMode>legacy</JazorRazorVueOutputMode>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
                <JazorBundleOut>$(MSBuildProjectDirectory)\wwwroot\app.bundle.js</JazorBundleOut>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="{{jazorPackageVersion}}" />
                <PackageReference Include="Demo.Authoring" Version="{{authoringPackageVersion}}" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(hostRoot, "Program.cs"),
            """
            namespace PackagedCustomRazorVueSample;

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);

        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace PackagedCustomRazorVueSample;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "ready";
            }
            """);

        WriteFile(
            Path.Combine(hostRoot, "CounterCard.cs"),
            """
            using Demo.Authoring;
            using ECMAScript;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace PackagedCustomRazorVueSample;

            [ECMAScriptModule("./components/counter-card")]
            public sealed class CounterCard : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Label { get; set; }

                [Parameter]
                public bool Disabled { get; set; }

                [Parameter]
                public int Value { get; set; }

                [Parameter]
                public EventCallback<int> ValueChanged { get; set; }

                [Parameter]
                public RenderFragment<string>? Header { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<DemoButton>(0);
                    builder.AddAttribute(1, nameof(DemoButton.Label), Label);
                    builder.AddAttribute(2, nameof(DemoButton.Disabled), Disabled);
                    builder.AddAttribute(3, nameof(DemoButton.Value), Value);
                    builder.AddAttribute(4, nameof(DemoButton.ValueChanged), ValueChanged);
                    builder.AddAttribute(5, nameof(DemoButton.Header), Header);
                    builder.CloseComponent();
                }
            }
            """);

        return projectPath;
    }

    private static string CreateExternalRazorVueSfcConsumerProject(string projectRoot)
    {
        Directory.CreateDirectory(projectRoot);

        var projectPath = Path.Combine(projectRoot, "ExternalRazorVueSfcConsumer.csproj");

        WriteFile(
            projectPath,
            """
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <RazorLangVersion>11.0</RazorLangVersion>
                <UseRazorSourceGenerator>true</UseRazorSourceGenerator>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorRazorVueOutputMode>sfc</JazorRazorVueOutputMode>
                <JazorRazorVueEnableRazorSgIntegration>true</JazorRazorVueEnableRazorSgIntegration>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="ECMAScript.Vuetify" Version="$(JazorPackageVersion)" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(projectRoot, "Program.cs"),
            """
            namespace ExternalRazorVueSfcConsumer;

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace ExternalRazorVueSfcConsumer;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "external RazorVue SFC consumer ready";
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "_Imports.razor"),
            """
            @using ExternalRazorVueSfcConsumer
            @using ECMAScript.Vuetify
            @using Microsoft.AspNetCore.Components
            """);

        WriteFile(
            Path.Combine(projectRoot, "TaskItem.cs"),
            """
            namespace ExternalRazorVueSfcConsumer;

            public sealed record TaskItem(
                string Title,
                string Category,
                bool IsDone,
                bool IsPinned);
            """);

        WriteFile(
            Path.Combine(projectRoot, "ExternalDashboard.razor.cs"),
            """
            using System.Collections.Generic;
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace ExternalRazorVueSfcConsumer;

            [ECMAScriptModule("./components/external-dashboard")]
            public partial class ExternalDashboard : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyList<TaskItem> Items { get; set; } = [];
            }
            """);

        WriteFile(
            Path.Combine(projectRoot, "ExternalDashboard.razor"),
            """
            <VApp>
                <VMain>
                    <VContainer Fluid="true">
                        <VCard>
                            <VCardTitle>External RazorVue Consumer</VCardTitle>
                            <VCardText>
                                <VList>
                                    @foreach (var item in Items)
                                    {
                                        <VListItem Title="@item.Title"
                                                   Subtitle="@(item.Category + " | " + (item.IsDone ? "Done" : "Open"))">
                                            @if (item.IsPinned)
                                            {
                                                <VChip Text='@("Pinned")' Color="primary" />
                                            }
                                        </VListItem>
                                    }
                                </VList>
                            </VCardText>
                        </VCard>
                    </VContainer>
                </VMain>
            </VApp>
            """);

        return projectPath;
    }

    private static string CreateExternalRazorVuePureDenoConsumer(string consumerRoot)
    {
        Directory.CreateDirectory(consumerRoot);
        var templateRoot = Path.Combine(FindRepoRoot(), "samples", "RazorVue.TodoList", "Todo.Host", "consumer");

        WriteFile(
            Path.Combine(consumerRoot, "deno.json"),
            """
            {
              "nodeModulesDir": "auto",
              "imports": {
                "vue": "npm:vue@3.5.13",
                "vue/server-renderer": "npm:@vue/server-renderer@3.5.13",
                "vuetify": "npm:vuetify@3.8.0",
                "vuetify/components": "npm:vuetify@3.8.0/components",
                "vuetify/styles": "npm:vuetify@3.8.0/styles"
              },
              "tasks": {
                "build": "deno run -A scripts/build.ts",
                "smoke:ssr": "deno run -A scripts/smoke-ssr.ts",
                "smoke:browser": "deno run -A scripts/smoke-browser.ts",
                "smoke:bundle-api": "deno run -A --unstable-bundle scripts/smoke-bundle-api.ts",
                "test": "deno run -A --unstable-bundle scripts/test.ts"
              }
            }
            """);

        WriteFile(
            Path.Combine(consumerRoot, "index.html"),
            """
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>External RazorVue Consumer</title>
                <!-- razorvue:styles -->
              </head>
              <body>
                <div id="app"></div>
                <!-- razorvue:script -->
              </body>
            </html>
            """);

        File.Copy(Path.Combine(templateRoot, "deno.lock"), Path.Combine(consumerRoot, "deno.lock"), overwrite: true);
        CopyDirectory(Path.Combine(templateRoot, "scripts"), Path.Combine(consumerRoot, "scripts"));

        WriteFile(
            Path.Combine(consumerRoot, "src", "runtime-common.js"),
            """
            import { h } from "vue";

            export function assertHostRequirements(hostRequirements) {
              if (hostRequirements === null || typeof hostRequirements !== "object") {
                throw new Error("RazorVue host requirements were not provided to the external Deno consumer.");
              }

              if (!Array.isArray(hostRequirements.pluginRequirements)) {
                throw new Error("RazorVue host requirements must expose a pluginRequirements array.");
              }

              if (!Array.isArray(hostRequirements.styles)) {
                throw new Error("RazorVue host requirements must expose a styles array.");
              }

              if (!hostRequirements.pluginRequirements.includes("vuetify")) {
                throw new Error("RazorVue host requirements must declare the Vuetify plugin.");
              }

              if (!hostRequirements.styles.includes("vuetify/styles")) {
                throw new Error("RazorVue host requirements must declare Vuetify styles.");
              }
            }

            export function createDashboardItems() {
              return [
                { title: "Preview external pure Deno consumer", category: "Automation", isDone: false, isPinned: true },
                { title: "Validate host requirements", category: "Runtime", isDone: true, isPinned: false }
              ];
            }

            export function createExternalDashboardRootComponent(Dashboard, items = createDashboardItems()) {
              return {
                render() {
                  return h(Dashboard, { items });
                }
              };
            }
            """);

        WriteFile(
            Path.Combine(consumerRoot, "src", "runtime-client.js"),
            """
            import { createApp } from "vue";
            import { createVuetify } from "vuetify";
            import "vuetify/styles";
            import { assertHostRequirements, createExternalDashboardRootComponent } from "./runtime-common.js";

            export function mountTodoConsumer(components, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
              assertHostRequirements(hostRequirements);
              const Dashboard = components?.TodoApp;
              if (typeof Dashboard !== "object" && typeof Dashboard !== "function") {
                throw new Error("External Deno consumer expected a TodoApp component export.");
              }

              const hasExplicitRoutes = Array.isArray(routesOrSelector);
              const selector = hasExplicitRoutes ? maybeSelector : routesOrSelector;
              const app = createApp(createExternalDashboardRootComponent(Dashboard));
              app.use(createVuetify());
              app.mount(selector);
              return app;
            }

            export function mountRootComponent(rootComponent, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
              return mountTodoConsumer({ TodoApp: rootComponent }, hostRequirements, routesOrSelector, maybeSelector);
            }
            """);

        WriteFile(
            Path.Combine(consumerRoot, "src", "runtime-ssr.js"),
            """
            import { createSSRApp, h } from "vue";
            import { renderToString } from "vue/server-renderer";
            import { createVuetify } from "vuetify";
            import { assertHostRequirements, createDashboardItems } from "./runtime-common.js";

            const expectedTexts = [
              "External RazorVue Consumer",
              "Preview external pure Deno consumer",
              "Automation | Open",
              "Validate host requirements",
              "Runtime | Done",
              "Pinned"
            ];

            export async function runTodoConsumerSsr(components, hostRequirements, razorVueConsumerRoutes) {
              assertHostRequirements(hostRequirements);
              void razorVueConsumerRoutes;
              const Dashboard = components?.TodoApp;
              if (typeof Dashboard !== "object" && typeof Dashboard !== "function") {
                throw new Error("External Deno consumer expected a TodoApp component export.");
              }

              const app = createSSRApp({
                render() {
                  return h(Dashboard, {
                    items: createDashboardItems()
                  });
                }
              });

              app.use(createVuetify());

              const html = await renderToString(app);
              for (const expectedText of expectedTexts) {
                if (!html.includes(expectedText)) {
                  throw new Error(`SSR smoke output did not contain expected text: ${expectedText}`);
                }
              }

              return html;
            }

            export async function runSsrSmoke(Dashboard, hostRequirements, razorVueConsumerRoutes) {
              return await runTodoConsumerSsr({ TodoApp: Dashboard }, hostRequirements, razorVueConsumerRoutes);
            }
            """);

        return consumerRoot;
    }

    private static string CreateRazorVueSampleProject(string hostRoot, LocalPackageFixture package)
    {
        Directory.CreateDirectory(hostRoot);

        var contractProjectPath = Path.Combine(package.RepoRoot, "src", "ECMAScript.Contract", "ECMAScript.Contract.csproj");
        var vuetifyProjectPath = Path.Combine(package.RepoRoot, "src", "ECMAScript.Vuetify", "ECMAScript.Vuetify.csproj");
        var projectPath = Path.Combine(hostRoot, "RazorVueSample.Host.csproj");

        WriteFile(
            projectPath,
            $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net11.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>true</JazorBundle>
                <JazorRazorVueOutputMode>legacy</JazorRazorVueOutputMode>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
                <JazorBundleOut>$(MSBuildProjectDirectory)\wwwroot\app.bundle.js</JazorBundleOut>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
              </ItemGroup>

              <ItemGroup>
                <ProjectReference Include="{{contractProjectPath}}" />
                <ProjectReference Include="{{vuetifyProjectPath}}" />
              </ItemGroup>
            </Project>
            """);

        WriteFile(
            Path.Combine(hostRoot, "Program.cs"),
            """
            namespace RazorVueSample.Host;

            internal static class Program
            {
                private static void Main()
                {
                }
            }
            """);

        WriteFile(
            Path.Combine(hostRoot, "AppModule.cs"),
            """
            using ECMAScript;

            namespace RazorVueSample.Host;

            [ECMAScriptModule("host/app.mjs")]
            public static class AppModule
            {
                public static string Boot() => "ready";
            }
            """);

        WriteFile(
            Path.Combine(hostRoot, "ProfileForm.cs"),
            """
            using ECMAScript;
            using ECMAScript.Vuetify;
            using ECMAScript.VueContract;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RazorVueSample.Host;

            [ECMAScriptModule("./components/profile-form")]
            public sealed class ProfileForm : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Name { get; set; }

                [Parameter]
                public EventCallback<string?> NameChanged { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<VTextField>(0);
                    builder.AddAttribute(1, nameof(VTextField.Label), "Name");
                    builder.AddAttribute(2, nameof(VTextField.ModelValue), Name);
                    builder.AddAttribute(3, nameof(VTextField.ModelValueChanged), NameChanged);
                    builder.CloseComponent();
                }
            }
            """);

        return projectPath;
    }

    private static string[] GetStringArrayProperty(JsonElement element, string propertyName)
        => element
            .GetProperty(propertyName)
            .EnumerateArray()
            .Select(static item => item.GetString())
            .OfType<string>()
            .ToArray();

    private static IReadOnlyList<string> RequireManifestStringList(
        List<string>? values,
        string propertyName,
        string manifestPath)
        => values ?? throw new InvalidOperationException(
            $"Manifest property '{propertyName}' must be materialized for '{manifestPath}'.");

    private static RazorVueManifestModel LoadRazorVueManifestProjection(string manifestPath)
    {
        var manifest = ManifestModel.TryLoad(manifestPath)
            ?? throw new FileNotFoundException("Manifest was not found: " + manifestPath, manifestPath);

        var razorVueManifest = manifest.ToRazorVueManifest();
        if (razorVueManifest.Modules.Count == 0)
            throw new InvalidOperationException("Manifest does not contain RazorVue component metadata: " + manifestPath);

        return razorVueManifest;
    }

    private static ManifestModel LoadManifest(string manifestPath)
        => ManifestModel.TryLoad(manifestPath)
            ?? throw new FileNotFoundException("Manifest was not found: " + manifestPath, manifestPath);

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

    private sealed record LocalPackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        string PackagePath,
        string VuetifyPackagePath,
        string VueRoutePackagePath,
        string PiniaPackagePath,
        string PiniaTestingPackagePath,
        string TDesignPackagePath,
        string DenoExePath);

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace(string repoRoot)
        {
            RootPath = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            SampleRoot = Path.Combine(RootPath, "Jazor.MultiProject");
            Directory.CreateDirectory(SampleRoot);
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
