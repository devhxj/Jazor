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
                "lib/net10.0/Jazor.Common.dll",
                "lib/net10.0/Jazor.Common.pdb",
                "lib/net10.0/Jazor.Compiler.dll",
                "lib/net10.0/Jazor.Compiler.pdb",
                "lib/net10.0/Jazor.Name.dll",
                "lib/net10.0/Jazor.Name.pdb",
                "lib/net10.0/Jazor.Razor.dll",
                "lib/net10.0/Jazor.Razor.pdb",
                "lib/net10.0/Jazor.RazorVue.dll",
                "lib/net10.0/Jazor.RazorVue.pdb"
            },
            entryNames.Where(static entry => entry.StartsWith("lib/net10.0/", StringComparison.Ordinal)).ToArray());
        CollectionAssert.AreEquivalent(
            new[]
            {
                "analyzers/dotnet/cs/Acornima.Extras.dll",
                "analyzers/dotnet/cs/Acornima.dll",
                "analyzers/dotnet/cs/Jazor.Analyzer.dll",
                "analyzers/dotnet/cs/Jazor.Analyzer.pdb",
                "analyzers/dotnet/cs/Jazor.Common.dll",
                "analyzers/dotnet/cs/Jazor.Common.pdb",
                "analyzers/dotnet/cs/Jazor.Compiler.dll",
                "analyzers/dotnet/cs/Jazor.Compiler.pdb",
                "analyzers/dotnet/cs/Jazor.Name.dll",
                "analyzers/dotnet/cs/Jazor.Name.pdb",
                "analyzers/dotnet/cs/Jazor.RazorVue.Analysis.dll",
                "analyzers/dotnet/cs/Jazor.RazorVue.Analysis.pdb",
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

        CollectionAssert.Contains(entryNames, "lib/net10.0/Jazor.RazorVue.Vuetify.dll");
        CollectionAssert.Contains(entryNames, "Jazor.RazorVue.Vuetify.nuspec");
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

        CollectionAssert.AreEquivalent(
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

        StringAssert.Contains(sharedModule, "export function Prefix()");
        StringAssert.Contains(sharedModule, "export function Compose(name)");
        StringAssert.Contains(featureModule, "import { Compose } from \"shared/greetings.mjs\";");
        StringAssert.Contains(featureModule, "export function Greet(name)");
        StringAssert.Contains(hostModule, "import { Greet } from \"features/greeter.mjs\";");
        StringAssert.Contains(hostModule, "export function Boot()");
        StringAssert.Contains(bundle, "function Prefix()");
        StringAssert.Contains(bundle, "function Greet(name)");
        StringAssert.Contains(bundle, "function Boot()");
        StringAssert.Contains(bundle, "export {");
        StringAssert.Contains(bundle, "Boot");
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
        Assert.AreEqual(
            "import { _559b27327f84f1af, _b7486264ae338f27 } from \"System/Globalization/CultureInfoModule.js\";",
            GetImportLine(module, "System/Globalization/CultureInfoModule.js"));

        StringAssert.Contains(module, "return _e2640560d207afce(\"2024-01-02\").toString();");
        StringAssert.Contains(module, "return _e856edbfd7db0646(_25187a24d190d864(\"2024-01-02T03:04:05+08:00\"), \"O\", null);");
        StringAssert.Contains(module, "let culture = _b7486264ae338f27(\"en-US\");");
        StringAssert.Contains(module, "return culture + \"|\" + _559b27327f84f1af(culture);");
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
        StringAssert.Contains(componentModule, "\"onUpdate:modelValue\": props.nameChanged");
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
            using ECMAScript.UI.Vue.Vuetify;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RazorVueSample.Host;

            [ECMAScriptModule("./components/profile-form")]
            public sealed class ProfileForm : VueComponent
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
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;

            namespace Demo.Sample;

            [VueLibraryComponent("demo/components", "DemoButton")]
            [VueLibraryStyle("demo/button.css")]
            [VueLibraryPluginRequirement("demo-host")]
            public sealed class DemoButton : VueLibraryComponent
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
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Sample;

            [ECMAScript.ECMAScriptModule("./components/counter-card")]
            public sealed class CounterCard : VueComponent
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
        StringAssert.Contains(componentModule, "\"onUpdate:modelValue\": props.valueChanged");
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
                <JazorOutDir>$(MSBuildProjectDirectory)\wwwroot\jazor\</JazorOutDir>
                <JazorBundleOut>$(MSBuildProjectDirectory)\wwwroot\app.bundle.js</JazorBundleOut>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="Jazor" Version="$(JazorPackageVersion)" />
                <PackageReference Include="Jazor.RazorVue.Vuetify" Version="$(JazorPackageVersion)" />
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
            using ECMAScript.UI.Vue.Vuetify;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace PackagedRazorVueVuetifySample;

            [ECMAScriptModule("./components/profile-form")]
            public sealed class ProfileForm : VueComponent
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
        StringAssert.Contains(componentModule, "\"onUpdate:modelValue\": props.nameChanged");
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
        var razorVueOutput = Path.Combine(repoRoot, "src", "Jazor.RazorVue", "bin", "Debug", "net10.0", "Jazor.RazorVue.dll");
        var vuetifyOutput = Path.Combine(repoRoot, "src", "Jazor.RazorVue.Vuetify", "bin", "Debug", "net10.0", "Jazor.RazorVue.Vuetify.dll");
        var analyzerOutput = Path.Combine(repoRoot, "src", "Jazor.Analyzer", "bin", "Debug", "netstandard2.0", "Jazor.Analyzer.dll");
        var emitPublishOutput = Path.Combine(repoRoot, "src", "Jazor.Emit", "bin", "Debug", "net10.0", "publish", "Jazor.Emit.dll");

        if (Directory.Exists(packageOutputDirectory))
            Directory.Delete(packageOutputDirectory, recursive: true);

        Directory.CreateDirectory(packageOutputDirectory);

        if (!File.Exists(ecmascriptOutput))
        {
            await RunDotNetAndAssertAsync(
                repoRoot,
                [
                    "build",
                    Path.Combine(repoRoot, "src", "ECMAScript", "ECMAScript.csproj"),
                    "-c",
                    "Debug",
                    "/m:1",
                    "/p:BuildInParallel=false"
                ]);
        }
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "Jazor.RazorVue", "Jazor.RazorVue.csproj"),
            razorVueOutput);
        await EnsureProjectBuiltAsync(
            repoRoot,
            Path.Combine(repoRoot, "src", "Jazor.RazorVue.Vuetify", "Jazor.RazorVue.Vuetify.csproj"),
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
                Path.Combine(repoRoot, "src", "Jazor.RazorVue.Vuetify", "Jazor.RazorVue.Vuetify.csproj"),
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
            GetPackagePath(packageOutputDirectory, "Jazor.RazorVue.Vuetify", packageVersion));
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
        if (File.Exists(expectedOutputPath))
        {
            return;
        }

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
    }

    private static async Task EnsureProjectPublishedAsync(
        string repoRoot,
        string projectPath,
        string expectedOutputPath,
        string publishDirectory)
    {
        if (File.Exists(expectedOutputPath))
        {
            return;
        }

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
            using Jazor.RazorVue;
            using Jazor.RazorVue.Descriptor;
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
            public sealed class DemoButton : VueLibraryComponent
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
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace PackagedCustomRazorVueSample;

            [ECMAScriptModule("./components/counter-card")]
            public sealed class CounterCard : VueComponent
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

        var razorVueProjectPath = Path.Combine(package.RepoRoot, "src", "Jazor.RazorVue", "Jazor.RazorVue.csproj");
        var vuetifyProjectPath = Path.Combine(package.RepoRoot, "src", "Jazor.RazorVue.Vuetify", "Jazor.RazorVue.Vuetify.csproj");
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
                <ProjectReference Include="{{razorVueProjectPath}}" />
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
            using ECMAScript.UI.Vue.Vuetify;
            using Jazor.RazorVue;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace RazorVueSample.Host;

            [ECMAScriptModule("./components/profile-form")]
            public sealed class ProfileForm : VueComponent
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
