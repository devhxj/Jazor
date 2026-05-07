using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

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
                "lib/net10.0/ECMAScript.dll",
                "lib/net10.0/ECMAScript.pdb",
                "lib/net10.0/ECMAScript.Pinia.dll",
                "lib/net10.0/ECMAScript.Pinia.pdb",
                "lib/net10.0/ECMAScript.VueRoute.dll",
                "lib/net10.0/ECMAScript.VueRoute.pdb",
                "lib/net10.0/ECMAScript.Contract.dll",
                "lib/net10.0/ECMAScript.Contract.pdb",
                "lib/net10.0/ECMAScript.VueContract.dll",
                "lib/net10.0/ECMAScript.VueContract.pdb",
                "lib/net10.0/ECMAScript.Vue3.dll",
                "lib/net10.0/ECMAScript.Vue3.pdb",
                "lib/net10.0/Jazor.Compiler.dll",
                "lib/net10.0/Jazor.Compiler.pdb",
                "lib/net10.0/Jazor.Common.dll",
                "lib/net10.0/Jazor.Common.pdb",
                "lib/net10.0/Jazor.RazorVue.dll",
                "lib/net10.0/Jazor.RazorVue.pdb",
                "lib/net10.0/ECMAScript.Vuetify.dll",
                "lib/net10.0/ECMAScript.Vuetify.pdb"
            },
            entryNames.Where(static entry => entry.StartsWith("lib/net10.0/", StringComparison.Ordinal)).ToArray());
        CollectionAssert.AreEquivalent(
            new[]
            {
                "analyzers/dotnet/cs/Acornima.Extras.dll",
                "analyzers/dotnet/cs/Acornima.dll",
                "analyzers/dotnet/cs/Jazor.Analyzer.dll",
                "analyzers/dotnet/cs/Jazor.Analyzer.pdb",
                "analyzers/dotnet/cs/ECMAScript.Contract.dll",
                "analyzers/dotnet/cs/ECMAScript.Contract.pdb",
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
    }

    [TestMethod]
    public async Task CreateLocalPackage_IncludesVuetifyAuthoringPackage()
    {
        var package = await LocalPackage.Value;

        using var archive = ZipFile.OpenRead(package.VuetifyPackagePath);
        var entryNames = archive.Entries
            .Select(static entry => entry.FullName.Replace('\\', '/'))
            .ToArray();

        CollectionAssert.Contains(entryNames, "lib/net10.0/ECMAScript.Vuetify.dll");
        CollectionAssert.Contains(entryNames, "ECMAScript.Vuetify.nuspec");
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

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var modulePaths = manifest.RootElement
            .GetProperty("Modules")
            .EnumerateArray()
            .Select(static module => module.GetProperty("RelativePath").GetString())
            .OfType<string>()
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
            using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
            var relativePath = manifest.RootElement
                .GetProperty("Modules")
                .EnumerateArray()
                .Select(static module => module.GetProperty("RelativePath").GetString())
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

        using var emittedManifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var emittedRelativePaths = emittedManifest.RootElement
            .GetProperty("Modules")
            .EnumerateArray()
            .Select(static module => module.GetProperty("RelativePath").GetString())
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray()!;
        CollectionAssert.Contains(emittedRelativePaths, "System/DecimalModule.js");
        CollectionAssert.Contains(emittedRelativePaths, "System/Globalization/CultureInfoModule.js");
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
                <TargetFramework>net10.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
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

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var emittedRelativePaths = manifest.RootElement
            .GetProperty("Modules")
            .EnumerateArray()
            .Select(static moduleEntry => moduleEntry.GetProperty("RelativePath").GetString())
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray()!;

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
                <TargetFramework>net10.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>false</JazorBundle>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
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
                    VueComputedRef<RouteRecordNormalized?> matched = Computed(() => normalized.Matched[0]);

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

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var emittedRelativePaths = manifest.RootElement
            .GetProperty("Modules")
            .EnumerateArray()
            .Select(static moduleEntry => moduleEntry.GetProperty("RelativePath").GetString())
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray()!;

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
                <TargetFramework>net10.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <JazorEmit>true</JazorEmit>
                <JazorBundle>true</JazorBundle>
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
                <JazorBundleOut>$(MSBuildProjectDirectory)\wwwroot\app.bundle.js</JazorBundleOut>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
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
                    VueComputedRef<RouteRecordNormalized?> matched = Computed(() => normalized.Matched[0]);

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

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath));
        var emittedRelativePaths = manifest.RootElement
            .GetProperty("Modules")
            .EnumerateArray()
            .Select(static moduleEntry => moduleEntry.GetProperty("RelativePath").GetString())
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .ToArray()!;

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
        var razorVueManifestPath = Path.Combine(moduleRoot, "jazor-manifest-razorvue.json");
        var componentModulePath = Path.Combine(moduleRoot, "components", "profile-form.mjs");
        var hostRequirementsModulePath = Path.Combine(moduleRoot, "__jazor", "razorvue-host.mjs");
        var bundlePath = Path.Combine(outputRoot, "app.bundle.js");
        var cssPath = Path.Combine(outputRoot, "app.bundle.razorvue.css");
        var hostContractPath = Path.Combine(outputRoot, "app.bundle.razorvue.host.json");
        var updatePlanPath = Path.Combine(outputRoot, "app.bundle.razorvue.update-plan.json");

        Assert.IsTrue(File.Exists(manifestPath), $"Manifest was not generated: {manifestPath}");
        Assert.IsTrue(File.Exists(razorVueManifestPath), $"RazorVue manifest was not generated: {razorVueManifestPath}");
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

        using var razorVueManifest = JsonDocument.Parse(await File.ReadAllTextAsync(razorVueManifestPath));
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            GetStringArrayProperty(razorVueManifest.RootElement, "Styles"));
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            GetStringArrayProperty(razorVueManifest.RootElement, "PluginRequirements"));
        Assert.AreEqual(
            "components/profile-form.mjs",
            razorVueManifest.RootElement.GetProperty("Modules")[0].GetProperty("RelativeModulePath").GetString());
        var expectedSourceHmrBoundary = razorVueManifest.RootElement.GetProperty("Modules")[0].GetProperty("HmrBoundaryKind").GetInt32();
        var expectedSourceRequiresHydration = razorVueManifest.RootElement.GetProperty("Modules")[0].GetProperty("RequiresHydration").GetBoolean();
        var expectedSourceSupportsSsr = razorVueManifest.RootElement.GetProperty("Modules")[0].GetProperty("SupportsSsr").GetBoolean();

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
        Assert.AreEqual("Previous RazorVue manifest is missing.", updatePlan.RootElement.GetProperty("Reason").GetString());
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
                <TargetFramework>net10.0</TargetFramework>
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
        var razorVueManifestPath = Path.Combine(outputRoot, "jazor-manifest-razorvue.json");
        var hostRequirementsModulePath = Path.Combine(outputRoot, "__jazor", "razorvue-host.mjs");

        Assert.IsTrue(File.Exists(modulePath), $"RazorVue module was not generated: {modulePath}");
        Assert.IsTrue(File.Exists(razorVueManifestPath), $"RazorVue manifest was not generated: {razorVueManifestPath}");
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

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(razorVueManifestPath));
        CollectionAssert.AreEqual(
            new[] { "demo/button.css" },
            manifest.RootElement.GetProperty("Styles").EnumerateArray().Select(static item => item.GetString()).OfType<string>().ToArray());
        CollectionAssert.AreEqual(
            new[] { "demo-host" },
            manifest.RootElement.GetProperty("PluginRequirements").EnumerateArray().Select(static item => item.GetString()).OfType<string>().ToArray());
        Assert.AreEqual("CounterCard", manifest.RootElement.GetProperty("Modules")[0].GetProperty("ComponentName").GetString());
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
        var razorVueManifestPath = Path.Combine(moduleRoot, "jazor-manifest-razorvue.json");
        var hostRequirementsModulePath = Path.Combine(moduleRoot, "__jazor", "razorvue-host.mjs");
        var bundlePath = Path.Combine(outputRoot, "app.bundle.js");
        var cssPath = Path.Combine(outputRoot, "app.bundle.razorvue.css");
        var hostContractPath = Path.Combine(outputRoot, "app.bundle.razorvue.host.json");

        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(razorVueManifestPath), $"RazorVue manifest was not generated: {razorVueManifestPath}");
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
        StringAssert.Contains(componentModule, "header: (slotProps) => props.header(slotProps)");
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

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(razorVueManifestPath));
        CollectionAssert.AreEqual(
            new[] { "demo/button.css" },
            GetStringArrayProperty(manifest.RootElement, "Styles"));
        CollectionAssert.AreEqual(
            new[] { "demo-host" },
            GetStringArrayProperty(manifest.RootElement, "PluginRequirements"));
        var expectedHmrBoundary = manifest.RootElement.GetProperty("Modules")[0].GetProperty("HmrBoundaryKind").GetInt32();
        var expectedRequiresHydration = manifest.RootElement.GetProperty("Modules")[0].GetProperty("RequiresHydration").GetBoolean();
        var expectedSupportsSsr = manifest.RootElement.GetProperty("Modules")[0].GetProperty("SupportsSsr").GetBoolean();

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
                <TargetFramework>net10.0</TargetFramework>
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
        var razorVueManifestPath = Path.Combine(moduleRoot, "jazor-manifest-razorvue.json");
        var hostRequirementsModulePath = Path.Combine(moduleRoot, "__jazor", "razorvue-host.mjs");
        var bundlePath = Path.Combine(outputRoot, "app.bundle.js");
        var cssPath = Path.Combine(outputRoot, "app.bundle.razorvue.css");
        var hostContractPath = Path.Combine(outputRoot, "app.bundle.razorvue.host.json");

        Assert.IsTrue(File.Exists(componentModulePath), $"RazorVue module was not generated: {componentModulePath}");
        Assert.IsTrue(File.Exists(razorVueManifestPath), $"RazorVue manifest was not generated: {razorVueManifestPath}");
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

        using var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(razorVueManifestPath));
        CollectionAssert.AreEqual(
            new[] { "vuetify/styles" },
            GetStringArrayProperty(manifest.RootElement, "Styles"));
        CollectionAssert.AreEqual(
            new[] { "vuetify" },
            GetStringArrayProperty(manifest.RootElement, "PluginRequirements"));
        var expectedPackagedHmrBoundary = manifest.RootElement.GetProperty("Modules")[0].GetProperty("HmrBoundaryKind").GetInt32();
        var expectedPackagedRequiresHydration = manifest.RootElement.GetProperty("Modules")[0].GetProperty("RequiresHydration").GetBoolean();
        var expectedPackagedSupportsSsr = manifest.RootElement.GetProperty("Modules")[0].GetProperty("SupportsSsr").GetBoolean();

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

    private static async Task<LocalPackageFixture> CreateLocalPackageAsync()
    {
        var repoRoot = FindRepoRoot();
        var packageVersion = ReadPackageVersion(Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"));
        var packageOutputDirectory = Path.Combine(repoRoot, ".tmp", "Jazor.EmitTest", "nupkg", Guid.NewGuid().ToString("N"));
        var ecmascriptOutput = Path.Combine(repoRoot, "src", "ECMAScript", "bin", "Debug", "net10.0", "ECMAScript.dll");
        var contractOutput = Path.Combine(repoRoot, "src", "ECMAScript.Contract", "bin", "Debug", "netstandard2.0", "ECMAScript.Contract.dll");
        var vuetifyOutput = Path.Combine(repoRoot, "src", "ECMAScript.Vuetify", "bin", "Debug", "net10.0", "ECMAScript.Vuetify.dll");
        var analyzerOutput = Path.Combine(repoRoot, "src", "Jazor.Analyzer", "bin", "Debug", "netstandard2.0", "Jazor.Analyzer.dll");
        var emitPublishOutput = Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Debug", "net10.0", "publish", "Jazor.Emit.dll");

        if (Directory.Exists(packageOutputDirectory))
            Directory.Delete(packageOutputDirectory, recursive: true);

        Directory.CreateDirectory(packageOutputDirectory);

        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript", "ECMAScript.csproj"),
            ecmascriptOutput);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Contract", "ECMAScript.Contract.csproj"),
            contractOutput);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "ECMAScript.Vuetify", "ECMAScript.Vuetify.csproj"),
            vuetifyOutput);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "Jazor.Analyzer", "Jazor.Analyzer.csproj"),
            analyzerOutput);
        await EnsureProjectPublishedAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "Jazor.Emit", "Jazor.Emit.csproj"),
            emitPublishOutput,
            Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Debug", "net10.0", "publish"));
        var jazorPack = await RunDotNetAsync(
            repoRoot,
            [
                "pack",
                Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"),
                "-c",
                "Debug",
                "--no-build",
                "-o",
                packageOutputDirectory
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
                packageOutputDirectory
            ]);

        return new LocalPackageFixture(
            repoRoot,
            packageVersion,
            packageOutputDirectory,
            GetPackagePath(packageOutputDirectory, packageVersion),
            GetPackagePath(packageOutputDirectory, "ECMAScript.Vuetify", packageVersion));
    }

    private static async Task RunDotNetAndAssertAsync(string workingDirectory, IReadOnlyList<string> arguments)
    {
        var result = await RunDotNetAsync(workingDirectory, arguments);
        Assert.AreEqual(0, result.ExitCode, result.ToString());
    }

    private static async Task EnsureProjectBuiltAsync(
        string repoRoot,
        string projectPath,
        string expectedOutputPath)
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
                "/p:BuildInParallel=false"
            ]);

        Assert.IsTrue(File.Exists(expectedOutputPath), $"Expected build output was not produced: {expectedOutputPath}");
    }

    private static async Task EnsureProjectPublishedAsync(
        string repoRoot,
        string projectPath,
        string expectedOutputPath,
        string publishDirectory)
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
                "/p:BuildInParallel=false"
            ]);

        Assert.IsTrue(File.Exists(expectedOutputPath), $"Expected publish output was not produced: {expectedOutputPath}");
    }

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

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static string ReadPackageVersion(string projectPath)
    {
        var document = XDocument.Load(projectPath);
        var version = document.Root?
            .Elements("PropertyGroup")
            .Elements("Version")
            .Select(static element => element.Value.Trim())
            .FirstOrDefault(static value => !string.IsNullOrWhiteSpace(value));

        if (string.IsNullOrWhiteSpace(version))
            throw new InvalidOperationException($"Could not read package version from '{projectPath}'.");

        return version;
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
                <TargetFramework>net10.0</TargetFramework>
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
            using Microsoft.AspNetCore.Components;

            namespace Demo.Authoring;

            [VueLibraryComponent("demo/components", "DemoButton")]
            [VueLibraryStyle("demo/button.css")]
            [VueLibraryPluginRequirement("demo-host")]
            [VueLibraryComponentFlags(VueComponentFlags.SupportsModelValue | VueComponentFlags.RequiresExplicitChildren)]
            [VueLibraryProp(nameof(Label), Name = "text")]
            [VueLibraryProp(nameof(Value), Name = "modelValue", AcceptsBinding = true, Required = true)]
            [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
            [VueLibrarySlot(nameof(Header), Name = "header", ContextTypeName = "string", ContextParameterName = "slotProps")]
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
                <TargetFramework>net10.0</TargetFramework>
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
                <TargetFramework>net10.0</TargetFramework>
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

    private static bool ShouldSkip(string relativePath)
    {
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(static segment =>
            segment.Equals("bin", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
            segment.Equals("TestResults", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record LocalPackageFixture(
        string RepoRoot,
        string PackageVersion,
        string PackageOutputDirectory,
        string PackagePath,
        string VuetifyPackagePath);

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
