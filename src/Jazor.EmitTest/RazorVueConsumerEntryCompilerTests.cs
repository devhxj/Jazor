using System.Text;
using System.Text.Json;
using Jazor.Emit;
using Jazor.RazorVue;
using Jazor.RazorVue.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class RazorVueConsumerEntryCompilerTests
{
    [TestMethod]
    public async Task GenerateAsync_WritesBrowserAndSsrEntryModulesWithSelectedComponents()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.CatalogPage", "CatalogPage", "pages/catalog-page.vue"),
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue"));
        workspace.WriteVue("pages/catalog-page.vue", "<template><section>Catalog</section></template>");
        workspace.WriteVue("pages/detail-page.vue", "<template><section>Detail</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(new RazorVueConsumerEntryOptions(
            workspace.HostJazorRoot,
            workspace.BuildRoot,
            ManifestPath: workspace.ManifestPath,
            HostRequirementsModulePath: workspace.HostRequirementsModulePath,
            BrowserGeneratedRoot: workspace.BrowserGeneratedRoot,
            SsrGeneratedRoot: workspace.SsrGeneratedRoot,
            ClientEntryPath: workspace.ClientEntryPath,
            SsrEntryPath: workspace.SsrEntryPath,
            VueFeatureFlagsPath: workspace.VueFeatureFlagsPath,
            ClientRuntimeModulePath: workspace.ClientRuntimeModulePath,
            SsrRuntimeModulePath: workspace.SsrRuntimeModulePath,
            ClientRuntimeExportName: "mountPlaygroundConsumer",
            SsrRuntimeExportName: "runPlaygroundConsumerSsr",
            SsrExecuteExportName: "executeSsrSmoke",
            Components:
            [
                new RazorVueConsumerComponentSelection("CatalogPage", "id:Demo.Pages.CatalogPage"),
                new RazorVueConsumerComponentSelection("DetailPage", "name:DetailPage")
            ],
            Mode: RazorVueConsumerEntryMode.Both,
            Production: true,
            Clean: true,
            WriteResultPath: workspace.ResultPath));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.ClientEntryPath));
        Assert.IsTrue(File.Exists(workspace.SsrEntryPath));
        Assert.IsTrue(File.Exists(workspace.VueFeatureFlagsPath));
        Assert.IsTrue(File.Exists(workspace.ResultPath));

        var clientEntry = await File.ReadAllTextAsync(workspace.ClientEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("import \"./vue-feature-flags.mjs\";", clientEntry);
        Assert.Contains("import { CatalogPage } from \"./generated-browser/pages/catalog-page.mjs\";", clientEntry);
        Assert.Contains("import { DetailPage } from \"./generated-browser/pages/detail-page.mjs\";", clientEntry);
        Assert.Contains("import { razorVueHostRequirements } from \"../../../jazor/__jazor/razorvue-host.mjs\";", clientEntry);
        Assert.Contains("import { mountPlaygroundConsumer } from \"../../src/runtime-client.js\";", clientEntry);
        Assert.Contains("export const razorVueConsumerComponents = Object.freeze({", clientEntry);
        Assert.Contains("CatalogPage,", clientEntry);
        Assert.Contains("DetailPage,", clientEntry);
        Assert.Contains("mountPlaygroundConsumer(razorVueConsumerComponents, razorVueHostRequirements);", clientEntry);

        var ssrEntry = await File.ReadAllTextAsync(workspace.SsrEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("import { CatalogPage } from \"./generated-ssr/pages/catalog-page.mjs\";", ssrEntry);
        Assert.Contains("import { DetailPage } from \"./generated-ssr/pages/detail-page.mjs\";", ssrEntry);
        Assert.Contains("import { runPlaygroundConsumerSsr } from \"../../src/runtime-ssr.js\";", ssrEntry);
        Assert.Contains("export async function executeSsrSmoke() {", ssrEntry);
        Assert.Contains("return await runPlaygroundConsumerSsr(razorVueConsumerComponents, razorVueHostRequirements);", ssrEntry);

        using var resultDocument = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.ResultPath, TestContext.CancellationTokenSource.Token));
        var components = resultDocument.RootElement.GetProperty("Components").EnumerateArray().ToArray();
        Assert.HasCount(2, components);
        Assert.AreEqual("CatalogPage", components[0].GetProperty("Alias").GetString());
        Assert.AreEqual("Demo.Pages.CatalogPage", components[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual("pages/catalog-page.mjs", components[0].GetProperty("BrowserRelativeOutputPath").GetString());
        Assert.AreEqual("DetailPage", components[1].GetProperty("Alias").GetString());
        Assert.AreEqual("Demo.Pages.DetailPage", components[1].GetProperty("ComponentId").GetString());
    }

    [TestMethod]
    public async Task GenerateAsync_WhenComponentSelectorIsAmbiguous_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.One.SharedCard", "SharedCard", "one/shared-card.vue"),
            ManifestEntry("Demo.Two.SharedCard", "SharedCard", "two/shared-card.vue"));
        workspace.WriteVue("one/shared-card.vue", "<template><section>One</section></template>");
        workspace.WriteVue("two/shared-card.vue", "<template><section>Two</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("SharedCard", "SharedCard")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(13, result.ExitCode);
        StringAssert.Contains(result.Error, "matched multiple RazorVue components");
        StringAssert.Contains(result.Error, "Use 'id:', 'name:', or 'path:'");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public async Task GenerateAsync_CleanModeRejectsOutputRootThatWouldDeleteConsumerRuntime()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(ManifestEntry("Demo.Pages.CatalogPage", "CatalogPage", "pages/catalog-page.vue"));
        workspace.WriteVue("pages/catalog-page.vue", "<template><section>Catalog</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("CatalogPage", "Demo.Pages.CatalogPage"),
            outputRoot: workspace.ConsumerRoot));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(16, result.ExitCode);
        StringAssert.Contains(result.Error, "cannot be the same as or an ancestor of the client runtime module");
        Assert.IsTrue(File.Exists(workspace.ClientRuntimeModulePath));
    }

    [TestMethod]
    public async Task GenerateAsync_CleanModeRejectsOutputPathEqualToConsumerRuntimeFile()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(ManifestEntry("Demo.Pages.CatalogPage", "CatalogPage", "pages/catalog-page.vue"));
        workspace.WriteVue("pages/catalog-page.vue", "<template><section>Catalog</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("CatalogPage", "Demo.Pages.CatalogPage"),
            outputRoot: workspace.ClientRuntimeModulePath));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(16, result.ExitCode);
        StringAssert.Contains(result.Error, "cannot be the same as or an ancestor of the client runtime module");
        Assert.IsTrue(File.Exists(workspace.ClientRuntimeModulePath));
    }

    [TestMethod]
    public async Task GenerateAsync_ReservedComponentAlias_FailsBeforeWritingEntries()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(ManifestEntry("Demo.Pages.CatalogPage", "CatalogPage", "pages/catalog-page.vue"));
        workspace.WriteVue("pages/catalog-page.vue", "<template><section>Catalog</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("default", "Demo.Pages.CatalogPage")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(13, result.ExitCode);
        StringAssert.Contains(result.Error, "reserved JavaScript identifier");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public void TryParse_AcceptsCliContractAndResolvesDefaults()
    {
        using var workspace = new TestWorkspace();

        var parsed = RazorVueConsumerEntryOptions.TryParse(
            [
                "--host-root",
                workspace.HostJazorRoot,
                "--out",
                workspace.BuildRoot,
                "--client-runtime",
                workspace.ClientRuntimeModulePath,
                "--ssr-runtime",
                workspace.SsrRuntimeModulePath,
                "--client-runtime-export",
                "mountPlaygroundConsumer",
                "--ssr-runtime-export",
                "runPlaygroundConsumerSsr",
                "--component",
                "CatalogPage=id:Demo.Pages.CatalogPage",
                "--component",
                "DetailPage=name:DetailPage",
                "--mode",
                "both",
                "--production",
                "false",
                "--clean",
                "true"
            ],
            out var options,
            out var error);

        Assert.IsTrue(parsed, error);
        Assert.IsNotNull(options);
        Assert.AreEqual(Path.Combine(workspace.HostJazorRoot, "jazor-manifest.json"), options.ManifestPath);
        Assert.AreEqual(Path.Combine(workspace.HostJazorRoot, "__jazor", "razorvue-host.mjs"), options.HostRequirementsModulePath);
        Assert.AreEqual(Path.Combine(workspace.BuildRoot, "generated-browser"), options.BrowserGeneratedRoot);
        Assert.AreEqual(Path.Combine(workspace.BuildRoot, "generated-ssr"), options.SsrGeneratedRoot);
        Assert.AreEqual(Path.Combine(workspace.BuildRoot, "client-entry.mjs"), options.ClientEntryPath);
        Assert.AreEqual(Path.Combine(workspace.BuildRoot, "ssr-entry.mjs"), options.SsrEntryPath);
        Assert.AreEqual("mountPlaygroundConsumer", options.ClientRuntimeExportName);
        Assert.AreEqual("runPlaygroundConsumerSsr", options.SsrRuntimeExportName);
        Assert.AreEqual(RazorVueConsumerEntryMode.Both, options.Mode);
        Assert.IsFalse(options.Production);
        Assert.IsTrue(options.Clean);
        Assert.HasCount(2, options.Components);
    }

    private static RazorVueManifestEntry ManifestEntry(
        string componentId,
        string componentName,
        string relativeModulePath)
        => new(
            AssemblyName: "Demo",
            ComponentId: componentId,
            ModuleId: componentId,
            ComponentName: componentName,
            RelativeModulePath: relativeModulePath,
            SourceMapPath: relativeModulePath + ".map",
            OriginMapPath: relativeModulePath + ".origins.json",
            Imports: [],
            Styles: [],
            PluginRequirements: [],
            DescriptorHash: "descriptor",
            TemplateHash: "template",
            LogicHash: "logic",
            ContentHash: "content",
            HmrBoundaryKind: RazorVueHmrBoundaryKind.TemplateOnly,
            RequiresHydration: false,
            SupportsSsr: true);

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            HostJazorRoot = Path.Combine(RootPath, "jazor");
            BuildRoot = Path.Combine(RootPath, "consumer", ".deno-build", "test");
            ConsumerRoot = Path.Combine(RootPath, "consumer");
            BrowserGeneratedRoot = Path.Combine(BuildRoot, "generated-browser");
            SsrGeneratedRoot = Path.Combine(BuildRoot, "generated-ssr");
            ClientEntryPath = Path.Combine(BuildRoot, "client-entry.mjs");
            SsrEntryPath = Path.Combine(BuildRoot, "ssr-entry.mjs");
            VueFeatureFlagsPath = Path.Combine(BuildRoot, "vue-feature-flags.mjs");
            ClientRuntimeModulePath = Path.Combine(ConsumerRoot, "src", "runtime-client.js");
            SsrRuntimeModulePath = Path.Combine(ConsumerRoot, "src", "runtime-ssr.js");
            ManifestPath = Path.Combine(HostJazorRoot, "jazor-manifest.json");
            HostRequirementsModulePath = Path.Combine(HostJazorRoot, "__jazor", "razorvue-host.mjs");
            ResultPath = Path.Combine(BuildRoot, "razorvue-consumer-entry.json");

            WriteText(ClientRuntimeModulePath, "export function mountPlaygroundConsumer() {}\n");
            WriteText(SsrRuntimeModulePath, "export async function runPlaygroundConsumerSsr() {}\n");
            WriteText(HostRequirementsModulePath, "export const razorVueHostRequirements = Object.freeze({});\n");
        }

        public string RootPath { get; }

        public string HostJazorRoot { get; }

        public string BuildRoot { get; }

        public string ConsumerRoot { get; }

        public string BrowserGeneratedRoot { get; }

        public string SsrGeneratedRoot { get; }

        public string ClientEntryPath { get; }

        public string SsrEntryPath { get; }

        public string VueFeatureFlagsPath { get; }

        public string ClientRuntimeModulePath { get; }

        public string SsrRuntimeModulePath { get; }

        public string ManifestPath { get; }

        public string HostRequirementsModulePath { get; }

        public string ResultPath { get; }

        public void WriteManifest(params RazorVueManifestEntry[] modules)
            => new ManifestModel(
                RootAssemblyPath: Path.Combine(RootPath, "Demo.dll"),
                GeneratedAtUtc: DateTime.UtcNow,
                Modules: [])
                .WithRazorVueManifest(
                    new RazorVueManifestModel(
                        "Demo",
                        DateTime.UtcNow,
                        modules.ToList(),
                        Styles: [],
                        PluginRequirements: []),
                    ManifestComponentModel.Sfc)
                .Save(ManifestPath);

        public void WriteVue(string relativePath, string content)
            => WriteText(Path.Combine(HostJazorRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)), content);

        public RazorVueConsumerEntryOptions CreateDefaultOptions(
            RazorVueConsumerComponentSelection component,
            string? outputRoot = null)
            => new(
                HostJazorRoot,
                outputRoot ?? BuildRoot,
                ManifestPath,
                HostRequirementsModulePath,
                BrowserGeneratedRoot,
                SsrGeneratedRoot,
                ClientEntryPath,
                SsrEntryPath,
                VueFeatureFlagsPath,
                ClientRuntimeModulePath,
                SsrRuntimeModulePath,
                "mountPlaygroundConsumer",
                "runPlaygroundConsumerSsr",
                "executeSsrSmoke",
                [component],
                RazorVueConsumerEntryMode.Both,
                Production: true,
                Clean: true,
                ResultPath);

        private static void WriteText(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, content.ReplaceLineEndings("\n"), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

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
