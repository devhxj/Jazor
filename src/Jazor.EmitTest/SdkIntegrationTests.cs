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
    private static readonly SemaphoreSlim SourceReferencedRazorVueBuildGate = new(1, 1);

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
    public async Task Build_LocalJazorPackage_StaticHost_UsesProjectRootJazorByDefault()
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
    public async Task Build_LocalPackages_WithExternalRazorSgG0Consumer_ReconcilesFinalDocumentsAcrossIncrementalBuilds()
    {
        var package = await LocalPackage.Value;

        using var workspace = new TestWorkspace(package.RepoRoot);
        var projectRoot = Path.Combine(workspace.RootPath, "ExternalRazorSgG0Consumer");
        var projectPath = CreateExternalRazorSgG0ConsumerProject(projectRoot);
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
        var firstGenerated = ReadRazorSgG0GeneratedSources(generatedRoot);
        var firstDocumentHash = ReadGeneratedStringConstant(firstGenerated.Evidence, "GeneratedDocumentContentHash");
        var firstOperationInventory = ReadGeneratedStringConstant(firstGenerated.Evidence, "BuildRenderTreeOperationInventory");
        Assert.IsFalse(
            firstGenerated.Evidence.Contains(projectRoot, StringComparison.OrdinalIgnoreCase),
            "Canonical G0 evidence must not contain the external consumer's absolute path.");

        var incrementalBuild = await RunSourceReferencedRazorVueBuildAsync(
            package.RepoRoot,
            ["build", projectPath, "--no-restore", .. commonArguments]);
        Assert.AreEqual(0, incrementalBuild.ExitCode, incrementalBuild.ToString());

        var incrementalGenerated = ReadRazorSgG0GeneratedSources(generatedRoot);
        Assert.AreEqual(firstDocumentHash, ReadGeneratedStringConstant(incrementalGenerated.Evidence, "GeneratedDocumentContentHash"));
        Assert.AreEqual(firstOperationInventory, ReadGeneratedStringConstant(incrementalGenerated.Evidence, "BuildRenderTreeOperationInventory"));

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

        var changedGenerated = ReadRazorSgG0GeneratedSources(generatedRoot);
        Assert.AreNotEqual(firstDocumentHash, ReadGeneratedStringConstant(changedGenerated.Evidence, "GeneratedDocumentContentHash"));
        Assert.AreNotEqual(firstOperationInventory, ReadGeneratedStringConstant(changedGenerated.Evidence, "BuildRenderTreeOperationInventory"));
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
        _ = ReadRazorSgG0GeneratedSources(generatedRoot);
        var catalogSource = ReadSingleGeneratedSource(generatedRoot, "Jazor.Generated.VueRenderCatalog.g.cs");
        StringAssert.Contains(catalogSource, "internal const int SchemaVersion = 1;");
        StringAssert.Contains(catalogSource, "components/counter.mjs");
        StringAssert.Contains(catalogSource, "components/counter.mjs.map");
        Assert.IsFalse(
            catalogSource.Contains(projectRoot, StringComparison.OrdinalIgnoreCase),
            "VueRenderCatalog must not persist the external consumer's absolute project path.");

        var outputRoot = Path.Combine(projectRoot, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputRoot, "jazor-manifest.json");
        var componentModulePath = Path.Combine(outputRoot, "components", "counter.mjs");
        var componentMapPath = Path.Combine(outputRoot, "components", "counter.mjs.map");
        var runtimeModulePath = Path.Combine(outputRoot, "@jazor", "vue-runtime", "render-context.mjs");
        var runtimeCoreModulePath = Path.Combine(outputRoot, "@jazor", "vue-runtime", "render-context-core.mjs");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue component module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(componentMapPath), $"RazorVue component source map was not generated: {componentMapPath}");
        Assert.IsTrue(File.Exists(runtimeModulePath), $"RazorVue runtime module was not generated: {runtimeModulePath}");
        Assert.IsTrue(File.Exists(runtimeCoreModulePath), $"RazorVue runtime core module was not generated: {runtimeCoreModulePath}");

        var componentModule = (await File.ReadAllTextAsync(componentModulePath)).ReplaceLineEndings("\n");
        StringAssert.Contains(componentModule, "import { defineComponent, h, reactive } from \"vue\";");
        StringAssert.Contains(componentModule, "import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";");
        StringAssert.Contains(componentModule, "export default defineComponent({");
        StringAssert.Contains(componentModule, "sourceMappingURL=counter.mjs.map");

        var componentMap = await File.ReadAllTextAsync(componentMapPath);
        StringAssert.Contains(componentMap, "\"file\": \"components/counter.mjs\"");
        StringAssert.Contains(componentMap, "Counter.razor");
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
        CollectionAssert.Contains(emittedRelativePaths, "@jazor/vue-runtime/render-context.mjs");
        CollectionAssert.Contains(emittedRelativePaths, "@jazor/vue-runtime/render-context-core.mjs");

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
        CollectionAssert.Contains(
            firstArtifacts.Select(static artifact => artifact.RelativePath).ToArray(),
            "@jazor/vue-runtime/render-context.mjs");
        CollectionAssert.Contains(
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
        var browserPath = ResolveBrowserExecutable();
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
        Assert.IsTrue(File.Exists(runtimeModulePath), $"RazorVue runtime module was not generated: {runtimeModulePath}");
        Assert.IsTrue(File.Exists(runtimeCoreModulePath), $"RazorVue runtime core module was not generated: {runtimeCoreModulePath}");

        var harnessRoot = Path.Combine(workspace.RootPath, "browser-harness");
        var harnessJazorRoot = Path.Combine(harnessRoot, "jazor");
        CopyDirectory(outputRoot, harnessJazorRoot);
        CreateCounterBrowserHarness(harnessRoot);

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
        var browser = await RunBrowserDumpDomAsync(browserPath, indexPath);
        Assert.AreEqual(0, browser.ExitCode, browser.ToString());

        using var smokePayload = ReadBrowserSmokePayload(browser);
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

        return new LocalPackageFixture(
            repoRoot,
            packageVersion,
            packageOutputDirectory,
            restorePackagesPath,
            GetPackagePath(packageOutputDirectory, packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Vuetify", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.VueRoute", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Pinia.Testing", packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.TDesign", packageVersion),
            GetBundledDenoPath(emitPublishDirectory));
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

    private static void CreateCounterBrowserHarness(string harnessRoot)
    {
        WriteFile(
            Path.Combine(harnessRoot, "deno.json"),
            """
            {
              "imports": {
                "vue": "npm:vue@3.5.13/dist/vue.runtime.esm-browser.prod.js",
                "@jazor/vue-runtime/": "./jazor/@jazor/vue-runtime/"
              }
            }
            """);

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

    private static async Task<ProcessResult> RunBrowserDumpDomAsync(string browserPath, string indexPath)
    {
        var harnessRoot = Path.GetDirectoryName(indexPath)!;
        var userDataRoot = Path.Combine(harnessRoot, ".browser-profile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(userDataRoot);

        try
        {
            return await RunProcessAsync(
                browserPath,
                harnessRoot,
                [
                    "--headless=new",
                    "--disable-gpu",
                    "--disable-dev-shm-usage",
                    "--no-first-run",
                    "--no-default-browser-check",
                    "--no-sandbox",
                    "--allow-file-access-from-files",
                    "--run-all-compositor-stages-before-draw",
                    "--virtual-time-budget=5000",
                    "--dump-dom",
                    $"--user-data-dir={userDataRoot}",
                    new Uri(Path.GetFullPath(indexPath)).AbsoluteUri
                ],
                TimeSpan.FromSeconds(45),
                environment: null);
        }
        finally
        {
            try
            {
                if (Directory.Exists(userDataRoot))
                    Directory.Delete(userDataRoot, recursive: true);
            }
            catch
            {
            }
        }
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

    private static string? ResolveBrowserExecutable()
    {
        var explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_EXE")?.Trim();
        if (string.IsNullOrWhiteSpace(explicitPath))
            explicitPath = Environment.GetEnvironmentVariable("RAZORVUE_BROWSER_PATH")?.Trim();

        if (!string.IsNullOrWhiteSpace(explicitPath))
            return File.Exists(explicitPath) ? explicitPath : null;

        var candidates = OperatingSystem.IsWindows()
            ? new[]
            {
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                "msedge.exe",
                "chrome.exe"
            }
            : OperatingSystem.IsMacOS()
                ? new[]
                {
                    "/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge",
                    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                    "microsoft-edge",
                    "google-chrome",
                    "chromium"
                }
                : new[]
                {
                    "microsoft-edge",
                    "microsoft-edge-stable",
                    "google-chrome",
                    "google-chrome-stable",
                    "chromium",
                    "chromium-browser"
                };

        foreach (var candidate in candidates)
        {
            var resolved = TryResolveBrowserExecutable(candidate);
            if (resolved is not null)
                return resolved;
        }

        return null;
    }

    private static string? TryResolveBrowserExecutable(string candidate)
    {
        if (candidate.Contains(Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            candidate.Contains(Path.AltDirectorySeparatorChar, StringComparison.Ordinal) ||
            candidate.Contains(':', StringComparison.Ordinal))
        {
            return File.Exists(candidate) ? candidate : null;
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? "";
        var extensions = OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.CMD;.BAT")
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
            : [""];

        foreach (var directory in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(directory))
                continue;

            foreach (var extension in extensions)
            {
                var fileName = candidate.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                    ? candidate
                    : candidate + extension;
                var fullPath = Path.Combine(directory, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
        }

        return null;
    }

    private static JsonDocument ReadBrowserSmokePayload(ProcessResult browser)
    {
        var match = Regex.Match(
            browser.StandardOutput,
            "data-jazor-smoke=\"(?<payload>[A-Za-z0-9+/=]+)\"",
            RegexOptions.CultureInvariant);
        Assert.IsTrue(
            match.Success,
            "Browser DOM did not contain the RazorVue smoke result marker." + Environment.NewLine + browser);

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups["payload"].Value));
        return JsonDocument.Parse(json);
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

    private static (string BootstrapTrace, string TailTrace, string Evidence) ReadRazorSgG0GeneratedSources(string generatedRoot)
    {
        Assert.IsTrue(Directory.Exists(generatedRoot), $"Compiler generated source root was not created: {generatedRoot}");

        var razorGeneratedSources = Directory
            .EnumerateFiles(generatedRoot, "*_razor.g.cs", SearchOption.AllDirectories)
            .Where(static path => !Path.GetFileName(path).Equals("_Imports_razor.g.cs", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.AreEqual(
            1,
            razorGeneratedSources.Length,
            "The external G0 consumer must expose exactly one official Razor generated component document." + Environment.NewLine +
            string.Join(Environment.NewLine, razorGeneratedSources));

        var bootstrapTrace = ReadSingleGeneratedSource(generatedRoot, "Jazor.RazorVue.RazorSgBootstrapTrace.g.cs");
        var tailTrace = ReadSingleGeneratedSource(generatedRoot, "Jazor.RazorVue.RazorSgTailTrace.g.cs");
        var evidence = ReadSingleGeneratedSource(generatedRoot, "Jazor.Generated.RazorSgFinalDocumentEvidence.g.cs");

        StringAssert.Contains(bootstrapTrace, "internal const bool ImplementationSourceOutputHookInstalled = true;");
        StringAssert.Contains(bootstrapTrace, "internal const bool TailOutputRegisteredForCurrentContext = true;");
        StringAssert.Contains(tailTrace, "internal const string State = \"bound\";");
        StringAssert.Contains(tailTrace, "internal const int ReusedGeneratedTreeCount = 0;");
        StringAssert.Contains(tailTrace, "internal const int GeneratorDocumentCount = 2;");
        StringAssert.Contains(tailTrace, "internal const int DerivedGeneratedTreeCount = 2;");
        StringAssert.Contains(tailTrace, "internal const string BindingMode = \"DerivedHookCompilation\";");

        StringAssert.Contains(evidence, "internal const int SchemaVersion = 2;");
        StringAssert.Contains(evidence, "internal const string InputContract = \"OfficialRazorSgFinalDocument\";");
        StringAssert.Contains(evidence, "internal const bool ConsumesRazorIntermediateRepresentation = false;");
        StringAssert.Contains(evidence, "internal const bool RecreatedCompilation = false;");
        StringAssert.Contains(evidence, "internal const bool NestedRazorSourceGeneratorRun = false;");
        StringAssert.Contains(evidence, "internal const int GeneratorDocumentCount = 2;");
        StringAssert.Contains(evidence, "internal const int CurrentGeneratedTreeCount = 2;");
        StringAssert.Contains(evidence, "internal const int ComponentCount = 1;");
        StringAssert.Contains(evidence, "internal const string BindingMode = \"DerivedHookCompilation\";");

        AssertNoGeneratedSource(generatedRoot, "Jazor.Generated.RazorVueCatalog.g.cs");
        AssertNoGeneratedSource(generatedRoot, "Jazor.Generated.RazorVue.Artifact_*.g.cs");

        return (bootstrapTrace, tailTrace, evidence);
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

    private static string ReadGeneratedStringConstant(string source, string constantName)
    {
        var match = Regex.Match(
            source,
            "internal const string " + Regex.Escape(constantName) + " = \\\"(?<value>[^\\\"]*)\\\";",
            RegexOptions.CultureInvariant);
        Assert.IsTrue(match.Success, "Generated source did not define string constant '" + constantName + "'." + Environment.NewLine + source);
        return match.Groups["value"].Value;
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
                <JazorEmit>{{enableEmit.ToString().ToLowerInvariant()}}</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
                <JazorRazorVueEnableRazorSgIntegration>true</JazorRazorVueEnableRazorSgIntegration>
                <JazorRazorVueTestHook>true</JazorRazorVueTestHook>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
              </ItemGroup>

              <ItemGroup>
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
                <CompilerVisibleProperty Include="JazorRazorVueTestHook" />
                <CompilerVisibleProperty Include="JazorRazorVueEnableRazorSgIntegration" />
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
