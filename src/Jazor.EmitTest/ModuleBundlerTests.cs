using System.Text;
using System.Text.Json;
using Jazor.Emit;
using Jazor.Common.SourceMaps;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ModuleBundlerTests
{
    [TestMethod]
    public async Task BundleAsync_SingleRootModule_PreservesExports()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "SdkSmoke/SampleModule.mjs",
            """
            export let Value = 42;
            export function Add(left, right) {
              return left + right;
            }
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "SdkSmoke.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("SdkSmoke", "SampleModule", "SampleModule", "SdkSmoke/SampleModule.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();

        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            null,
            null));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.OutputPath));

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.AreNotEqual(string.Empty, script);
        Assert.Contains("function Add(left, right)", script);
        Assert.Contains("var Value = 42;", script);
        Assert.Contains("export {", script);
        Assert.Contains("Add", script);
        Assert.Contains("Value", script);
    }

    [TestMethod]
    public async Task BundleAsync_MultiProjectHostBundle_ReExportsRootAssemblyMembers()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "shared/greetings.mjs",
            """
            export function Prefix() {
              return "Hello";
            }

            export function Compose(name) {
              return `${Prefix()}, ${name}`;
            }
            """);
        WriteModule(workspace.InputDirectory, "features/greeter.mjs",
            """
            import { Compose } from "shared/greetings.mjs";
            export function Greet(name) {
              return Compose(name);
            }
            """);
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            import { Greet } from "features/greeter.mjs";
            export function Boot() {
              return Greet("Jazor");
            }
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Contracts", "Sample.Contracts.GreetingSharedModule", "Sample.Contracts.GreetingSharedModule", "shared/greetings.mjs", "hash-1"),
                new ManifestModuleEntry("Sample.Features", "Sample.Features.GreeterModule", "Sample.Features.GreeterModule", "features/greeter.mjs", "hash-2"),
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-3")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();

        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            null,
            null));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.OutputPath));

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.AreNotEqual(string.Empty, script);
        Assert.Contains("function Prefix()", script);
        Assert.Contains("function Compose(name)", script);
        Assert.Contains("function Greet(name)", script);
        Assert.Contains("function Boot()", script);
        Assert.Contains("export {", script);
        Assert.Contains("Boot", script);
    }

    [TestMethod]
    public async Task BundleAsync_WithRazorVueHostRequirements_ReExportsHostContract()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            export function Boot() {
              return "ready";
            }
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        SaveUnifiedManifest(
            workspace.ManifestPath,
            manifest,
            CreateRazorVueManifest(
                componentId: "Sample.Host.CounterCard",
                moduleId: "components/counter-card.mjs",
                componentName: "CounterCard",
                relativeModulePath: "components/counter-card.mjs",
                imports: ["vue", "vuetify/components"],
                styles: ["vuetify/styles"],
                pluginRequirements: ["feature-flags", "vuetify"],
                descriptorHash: "descriptor-hash",
                templateHash: "template-hash",
                logicHash: "logic-hash",
                contentHash: "content-hash",
                boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));
        WriteModule(
            workspace.InputDirectory,
            "__jazor/razorvue-host.mjs",
            """
            export const razorVueHostAssemblyName = "Sample.Host";
            export const razorVueHostGeneratedAtUtc = "2026-04-08T00:00:00.0000000Z";
            export const razorVueStyles = Object.freeze(["vuetify/styles"]);
            export const razorVuePluginRequirements = Object.freeze(["feature-flags", "vuetify"]);
            export const razorVueHostModules = Object.freeze([{"assemblyName":"Sample.Host","componentId":"Sample.Host.CounterCard","moduleId":"components/counter-card.mjs","componentName":"CounterCard","relativeModulePath":"components/counter-card.mjs","sourceMapPath":"components/counter-card.mjs.map","originMapPath":"components/counter-card.mjs.origins.json","styles":["vuetify/styles"],"pluginRequirements":["feature-flags","vuetify"],"descriptorHash":"descriptor-hash","templateHash":"template-hash","logicHash":"logic-hash","contentHash":"content-hash","hmrBoundaryKind":2,"requiresHydration":false,"supportsSsr":true}]);
            export const razorVueHostRequirements = Object.freeze({
              assemblyName: razorVueHostAssemblyName,
              generatedAtUtc: razorVueHostGeneratedAtUtc,
              styles: razorVueStyles,
              pluginRequirements: razorVuePluginRequirements,
              modules: razorVueHostModules
            });
            """);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            null,
            null));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("function Boot()", script);
        Assert.Contains("razorVueHostAssemblyName", script);
        Assert.Contains("razorVueHostModules", script);
        Assert.Contains("razorVueHostRequirements", script);
        Assert.Contains("CounterCard", script);
        Assert.Contains("descriptor-hash", script);
        Assert.Contains("components/counter-card.mjs.map", script);
        Assert.Contains("components/counter-card.mjs.origins.json", script);
        Assert.Contains("feature-flags", script);
        Assert.Contains("vuetify/styles", script);
    }

    [TestMethod]
    public async Task BundleAsync_WritesExternalSourceMapAndSourceMappingUrl()
    {
        using var workspace = new TestWorkspace();
        WriteMappedModule(
            workspace.InputDirectory,
            "host/app.mjs",
            """
            export function Boot() {
              return "ready";
            }
            """,
            new SourceMapDocument(
                "host/app.mjs",
                [new SourceMapSource("Pages/App.razor", "<App />")],
                [
                    new SourceMapSegment(0, 0, 0, 0, 0),
                    new SourceMapSegment(1, 0, 0, 1, 0)
                ]));

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            null,
            null));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.OutputPath));
        Assert.IsTrue(File.Exists(workspace.OutputMapPath));

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("//# sourceMappingURL=bundle.js.map", script);

        using var map = await ReadJsonAsync(workspace.OutputMapPath);
        Assert.AreEqual("bundle.js", map.RootElement.GetProperty("file").GetString(), map.RootElement.ToString());
        Assert.IsTrue(map.RootElement.TryGetProperty("mappings", out var mappingsProperty));
        Assert.AreNotEqual(string.Empty, mappingsProperty.GetString());
    }

    [TestMethod]
    public async Task BundleAsync_ChainsBundleMapBackToOriginalSources()
    {
        using var workspace = new TestWorkspace();
        WriteMappedModule(
            workspace.InputDirectory,
            "host/app.mjs",
            """
            export const Value = 42;
            export function Boot() {
              return Value;
            }
            """,
            new SourceMapDocument(
                "host/app.mjs",
                [new SourceMapSource("Pages/App.razor", "<div>@Value</div>")],
                [
                    new SourceMapSegment(0, 0, 0, 4, 2),
                    new SourceMapSegment(1, 0, 0, 5, 0),
                    new SourceMapSegment(2, 0, 0, 6, 0)
                ]));

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            null,
            null));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        using var map = await ReadJsonAsync(workspace.OutputMapPath);
        var sources = map.RootElement.GetProperty("sources")
            .EnumerateArray()
            .Select(static item => item.GetString())
            .Where(static item => item is not null)
            .Cast<string>()
            .ToArray();
        var sourcesContent = map.RootElement.GetProperty("sourcesContent")
            .EnumerateArray()
            .Select(static item => item.GetString())
            .ToArray();

        CollectionAssert.AreEquivalent(new[] { "Pages/App.razor" }, sources, map.RootElement.ToString());
        CollectionAssert.DoesNotContain(sources, "host/app.mjs");
        Assert.IsTrue(sources.All(static source => !source.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase)));
        CollectionAssert.Contains(sourcesContent, "<div>@Value</div>");
    }

    [TestMethod]
    public async Task BundleAsync_WithRazorVueManifest_WritesHostSidecars()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            export function Boot() {
              return "ready";
            }
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        SaveUnifiedManifest(
            workspace.ManifestPath,
            manifest,
            CreateRazorVueManifest(
                componentId: "Sample.Host.ProfileForm",
                moduleId: "components/profile-form.mjs",
                componentName: "ProfileForm",
                relativeModulePath: "components/profile-form.mjs",
                imports: ["vue", "vuetify/components"],
                styles: ["feature/flags.css", "vuetify/styles"],
                pluginRequirements: ["feature-flags", "vuetify"],
                descriptorHash: "descriptor-hash",
                templateHash: "template-hash",
                logicHash: "logic-hash",
                contentHash: "content-hash",
                boundaryKind: RazorVueHmrBoundaryKind.LogicSafe));

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            null,
            null));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.RazorVueCssPath));
        Assert.IsTrue(File.Exists(workspace.RazorVueHostContractPath));

        var css = await File.ReadAllTextAsync(workspace.RazorVueCssPath, CancellationToken.None);
        Assert.AreEqual(
            "@import \"feature/flags.css\";\n@import \"vuetify/styles\";\n",
            css.ReplaceLineEndings("\n"));

        using var contract = await ReadJsonAsync(workspace.RazorVueHostContractPath);
        Assert.AreEqual("bundle.js.map", contract.RootElement.GetProperty("BundleSourceMapFile").GetString());
        CollectionAssert.AreEqual(
            new[] { "feature/flags.css", "vuetify/styles" },
            contract.RootElement.GetProperty("Styles").EnumerateArray().Select(static item => item.GetString()).OfType<string>().ToArray());
        CollectionAssert.AreEqual(
            new[] { "feature-flags", "vuetify" },
            contract.RootElement.GetProperty("PluginRequirements").EnumerateArray().Select(static item => item.GetString()).OfType<string>().ToArray());
        Assert.AreEqual("Sample.Host", contract.RootElement.GetProperty("Modules")[0].GetProperty("AssemblyName").GetString());
        Assert.AreEqual("Sample.Host.ProfileForm", contract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual("components/profile-form.mjs", contract.RootElement.GetProperty("Modules")[0].GetProperty("ModuleId").GetString());
        Assert.AreEqual("ProfileForm", contract.RootElement.GetProperty("Modules")[0].GetProperty("ComponentName").GetString());
        Assert.AreEqual("components/profile-form.mjs", contract.RootElement.GetProperty("Modules")[0].GetProperty("RelativeModulePath").GetString());
        Assert.AreEqual("components/profile-form.mjs.map", contract.RootElement.GetProperty("Modules")[0].GetProperty("SourceMapPath").GetString());
        Assert.AreEqual("components/profile-form.mjs.origins.json", contract.RootElement.GetProperty("Modules")[0].GetProperty("OriginMapPath").GetString());
        Assert.AreEqual("descriptor-hash", contract.RootElement.GetProperty("Modules")[0].GetProperty("DescriptorHash").GetString());
        Assert.AreEqual("template-hash", contract.RootElement.GetProperty("Modules")[0].GetProperty("TemplateHash").GetString());
        Assert.AreEqual("logic-hash", contract.RootElement.GetProperty("Modules")[0].GetProperty("LogicHash").GetString());
        Assert.AreEqual("content-hash", contract.RootElement.GetProperty("Modules")[0].GetProperty("ContentHash").GetString());
        Assert.AreEqual((int)RazorVueHmrBoundaryKind.LogicSafe, contract.RootElement.GetProperty("Modules")[0].GetProperty("HmrBoundaryKind").GetInt32());
        Assert.IsFalse(contract.RootElement.GetProperty("Modules")[0].GetProperty("RequiresHydration").GetBoolean());
        Assert.IsTrue(contract.RootElement.GetProperty("Modules")[0].GetProperty("SupportsSsr").GetBoolean());
    }

    [TestMethod]
    public async Task BundleAsync_WithPreviousManifest_WritesRazorVueUpdatePlanSidecar()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            export function Boot() {
              return "ready";
            }
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Save(workspace.ManifestPath);

        SaveUnifiedManifest(
            workspace.PreviousManifestPath,
            manifest,
            CreateRazorVueManifest(
                "template-a",
                "logic-a",
                "content-a"));
        SaveUnifiedManifest(
            workspace.ManifestPath,
            manifest,
            CreateRazorVueManifest(
                "template-b",
                "logic-a",
                "content-b"));

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            workspace.PreviousManifestPath,
            workspace.RazorVueUpdatePlanPath));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.RazorVueUpdatePlanPath));

        using var plan = await ReadJsonAsync(workspace.RazorVueUpdatePlanPath);
        Assert.AreEqual("TemplatePatch", plan.RootElement.GetProperty("Action").GetString());
        Assert.AreEqual("Sample.Host.ProfileForm", plan.RootElement.GetProperty("Modules")[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual("TemplatePatch", plan.RootElement.GetProperty("Modules")[0].GetProperty("Action").GetString());
        Assert.AreEqual("Template hash changed while descriptor and logic stayed stable.", plan.RootElement.GetProperty("Modules")[0].GetProperty("Reason").GetString());
    }

    private static RazorVueManifestModel CreateRazorVueManifest(string templateHash, string logicHash, string contentHash)
        => CreateRazorVueManifest(
            componentId: "Sample.Host.ProfileForm",
            moduleId: "components/profile-form.mjs",
            componentName: "ProfileForm",
            relativeModulePath: "components/profile-form.mjs",
            imports: ["vue"],
            styles: ["vuetify/styles"],
            pluginRequirements: ["vuetify"],
            descriptorHash: "descriptor-hash",
            templateHash,
            logicHash,
            contentHash,
            RazorVueHmrBoundaryKind.TemplateOnly);

    private static RazorVueManifestModel CreateRazorVueManifest(
        string componentId,
        string moduleId,
        string componentName,
        string relativeModulePath,
        IReadOnlyList<string> imports,
        IReadOnlyList<string> styles,
        IReadOnlyList<string> pluginRequirements,
        string descriptorHash,
        string templateHash,
        string logicHash,
        string contentHash,
        RazorVueHmrBoundaryKind boundaryKind)
        => new(
            "Sample.Host",
            new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
            [
                new RazorVueManifestEntry(
                    "Sample.Host",
                    componentId,
                    moduleId,
                    componentName,
                    ["/", "/counter"],
                    relativeModulePath,
                    relativeModulePath + ".map",
                    relativeModulePath + ".origins.json",
                    imports.ToList(),
                    styles.ToList(),
                    pluginRequirements.ToList(),
                    descriptorHash,
                    templateHash,
                    logicHash,
                    contentHash,
                    boundaryKind,
                    false,
                    true)
            ],
            RazorVueManifestFactory.NormalizeHostRequirementList(styles),
            RazorVueManifestFactory.NormalizeHostRequirementList(pluginRequirements));

    private static void SaveUnifiedManifest(
        string manifestPath,
        ManifestModel baseManifest,
        RazorVueManifestModel razorVueManifest)
        => baseManifest
            .WithRazorVueManifest(razorVueManifest, ManifestComponentModel.H)
            .Save(manifestPath);

    private static void WriteModule(string rootDirectory, string relativePath, string content)
    {
        var fullPath = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content.ReplaceLineEndings("\n"), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static void WriteMappedModule(string rootDirectory, string relativePath, string content, SourceMapDocument map)
    {
        var writer = new SourceMapWriter();
        var fullPath = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var mapPath = fullPath + ".map";
        var script = writer.AppendSourceMappingUrl(content.ReplaceLineEndings("\n"), Path.GetFileName(mapPath));
        WriteModule(rootDirectory, relativePath, script);
        File.WriteAllText(mapPath, writer.Write(map), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static async Task<JsonDocument> ReadJsonAsync(string path)
        => JsonDocument.Parse(await File.ReadAllTextAsync(path, CancellationToken.None));

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            InputDirectory = Path.Combine(RootPath, "modules");
            ManifestPath = Path.Combine(InputDirectory, "jazor-manifest.json");
            OutputPath = Path.Combine(RootPath, "bundle.js");
            OutputMapPath = Path.Combine(RootPath, "bundle.js.map");
            RazorVueCssPath = Path.Combine(RootPath, "bundle.razorvue.css");
            RazorVueHostContractPath = Path.Combine(RootPath, "bundle.razorvue.host.json");
            PreviousManifestPath = Path.Combine(RootPath, "previous-jazor-manifest.json");
            RazorVueUpdatePlanPath = Path.Combine(RootPath, "bundle.razorvue.update-plan.json");
            Directory.CreateDirectory(InputDirectory);
        }

        public string RootPath { get; }

        public string InputDirectory { get; }

        public string ManifestPath { get; }

        public string OutputPath { get; }

        public string OutputMapPath { get; }

        public string RazorVueCssPath { get; }

        public string RazorVueHostContractPath { get; }

        public string PreviousManifestPath { get; }

        public string RazorVueUpdatePlanPath { get; }

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

    public TestContext TestContext { get; set; }
}
