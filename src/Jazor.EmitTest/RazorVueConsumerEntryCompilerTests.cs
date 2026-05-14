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
        Assert.Contains("export const razorVueConsumerRoutes = Object.freeze([", clientEntry);
        Assert.Contains("routeTemplate: \"/catalog\"", clientEntry);
        Assert.Contains("path: \"/examples/:id\"", clientEntry);
        Assert.Contains("CatalogPage,", clientEntry);
        Assert.Contains("DetailPage,", clientEntry);
        Assert.Contains("mountPlaygroundConsumer(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);", clientEntry);

        var ssrEntry = await File.ReadAllTextAsync(workspace.SsrEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("import { CatalogPage } from \"./generated-ssr/pages/catalog-page.mjs\";", ssrEntry);
        Assert.Contains("import { DetailPage } from \"./generated-ssr/pages/detail-page.mjs\";", ssrEntry);
        Assert.Contains("import { runPlaygroundConsumerSsr } from \"../../src/runtime-ssr.js\";", ssrEntry);
        Assert.Contains("export async function executeSsrSmoke() {", ssrEntry);
        Assert.Contains("return await runPlaygroundConsumerSsr(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);", ssrEntry);

        using var resultDocument = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.ResultPath, TestContext.CancellationTokenSource.Token));
        var components = resultDocument.RootElement.GetProperty("Components").EnumerateArray().ToArray();
        Assert.HasCount(2, components);
        Assert.AreEqual("CatalogPage", components[0].GetProperty("Alias").GetString());
        Assert.AreEqual("Demo.Pages.CatalogPage", components[0].GetProperty("ComponentId").GetString());
        Assert.AreEqual("pages/catalog-page.mjs", components[0].GetProperty("BrowserRelativeOutputPath").GetString());
        Assert.AreEqual("DetailPage", components[1].GetProperty("Alias").GetString());
        Assert.AreEqual("Demo.Pages.DetailPage", components[1].GetProperty("ComponentId").GetString());

        var routes = resultDocument.RootElement.GetProperty("Routes").EnumerateArray().ToArray();
        Assert.HasCount(3, routes);
        Assert.AreEqual("CatalogPage", routes[0].GetProperty("Alias").GetString());
        Assert.AreEqual("/", routes[0].GetProperty("Path").GetString());
        Assert.AreEqual("/catalog", routes[1].GetProperty("RouteTemplate").GetString());
        Assert.AreEqual("/examples/:id", routes[2].GetProperty("Path").GetString());
        CollectionAssert.AreEqual(
            new[] { "id" },
            routes[2].GetProperty("ParameterNames").EnumerateArray().Select(static item => item.GetString()).OfType<string>().ToArray());
    }

    [TestMethod]
    public async Task GenerateAsync_EmitsRouteArgumentCallers_ForRuntimeExports()
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
                new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")
            ],
            Mode: RazorVueConsumerEntryMode.Both,
            Production: true,
            Clean: true,
            WriteResultPath: null));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var clientEntry = await File.ReadAllTextAsync(workspace.ClientEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains(
            "mountPlaygroundConsumer(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);",
            clientEntry);

        var ssrEntry = await File.ReadAllTextAsync(workspace.SsrEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains(
            "return await runPlaygroundConsumerSsr(razorVueConsumerComponents, razorVueHostRequirements, razorVueConsumerRoutes);",
            ssrEntry);
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
        StringAssert.Contains(result.Error, "matched multiple RazorVue component entries in the Jazor manifest");
        StringAssert.Contains(result.Error, "Use 'id:', 'name:', or 'path:'");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public async Task GenerateAsync_WhenManifestContainsNoRazorVueComponents_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WritePlainManifest();

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("CatalogPage", "id:Demo.Pages.CatalogPage")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(11, result.ExitCode);
        StringAssert.Contains(result.Error, "did not contain any RazorVue component entries");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public async Task GenerateAsync_WithMixedHAndSfcComponents_WritesDirectHImportsAndSfcBridgeImports()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.CatalogPage", "CatalogPage", "pages/catalog-page.mjs", ManifestComponentModel.H),
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue", ManifestComponentModel.Sfc));
        workspace.WriteHostModule(
            "pages/catalog-page.mjs",
            """
            export default {
              name: "CatalogPage"
            };
            """);
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
                new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")
            ],
            Mode: RazorVueConsumerEntryMode.Both,
            Production: true,
            Clean: true,
            WriteResultPath: workspace.ResultPath));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var clientEntry = await File.ReadAllTextAsync(workspace.ClientEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("import CatalogPage from \"../../../jazor/pages/catalog-page.mjs\";", clientEntry);
        Assert.Contains("import { DetailPage } from \"./generated-browser/pages/detail-page.mjs\";", clientEntry);

        var ssrEntry = await File.ReadAllTextAsync(workspace.SsrEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("import CatalogPage from \"../../../jazor/pages/catalog-page.mjs\";", ssrEntry);
        Assert.Contains("import { DetailPage } from \"./generated-ssr/pages/detail-page.mjs\";", ssrEntry);

        using var resultDocument = JsonDocument.Parse(await File.ReadAllTextAsync(workspace.ResultPath, TestContext.CancellationTokenSource.Token));
        var components = resultDocument.RootElement.GetProperty("Components").EnumerateArray().ToArray();
        Assert.HasCount(2, components);
        Assert.AreEqual("h", components[0].GetProperty("ComponentModel").GetString());
        Assert.AreEqual("default", components[0].GetProperty("ExportName").GetString());
        Assert.AreEqual("pages/catalog-page.mjs", components[0].GetProperty("BrowserRelativeOutputPath").GetString());
        Assert.AreEqual("sfc", components[1].GetProperty("ComponentModel").GetString());
        Assert.AreEqual("DetailPage", components[1].GetProperty("ExportName").GetString());
        Assert.AreEqual("pages/detail-page.mjs", components[1].GetProperty("BrowserRelativeOutputPath").GetString());

        var routes = resultDocument.RootElement.GetProperty("Routes").EnumerateArray().ToArray();
        Assert.HasCount(3, routes);
        Assert.AreEqual("CatalogPage", routes[0].GetProperty("Alias").GetString());
        Assert.AreEqual("DetailPage", routes[2].GetProperty("Alias").GetString());
    }

    [TestMethod]
    public async Task GenerateAsync_WhenRouteTemplateUsesConstraint_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue") with
            {
                RouteTemplates = ["/examples/{id:int}"]
            });
        workspace.WriteVue("pages/detail-page.vue", "<template><section>Detail</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(15, result.ExitCode);
        StringAssert.Contains(result.Error, "route constraints are not supported yet");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public async Task GenerateAsync_WhenRouteTemplateUsesOptionalParameter_EmitsOptionalVueRouteSegment()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue") with
            {
                RouteTemplates = ["/examples/{id?}"]
            });
        workspace.WriteVue("pages/detail-page.vue", "<template><section>Detail</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var clientEntry = await File.ReadAllTextAsync(workspace.ClientEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("path: \"/examples/:id?\"", clientEntry);
        Assert.Contains("parameterNames: Object.freeze([\"id\"])", clientEntry);
    }

    [TestMethod]
    public async Task GenerateAsync_WhenRouteTemplateUsesCatchAll_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue") with
            {
                RouteTemplates = ["/examples/{*path}"]
            });
        workspace.WriteVue("pages/detail-page.vue", "<template><section>Detail</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(15, result.ExitCode);
        StringAssert.Contains(result.Error, "catch-all route parameters are not supported yet");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public async Task GenerateAsync_WhenRouteTemplateUsesDefaultValue_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue") with
            {
                RouteTemplates = ["/examples/{id=42}"]
            });
        workspace.WriteVue("pages/detail-page.vue", "<template><section>Detail</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(15, result.ExitCode);
        StringAssert.Contains(result.Error, "route default values are not supported yet");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public async Task GenerateAsync_WhenRouteTemplateUsesCompositeSegment_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue") with
            {
                RouteTemplates = ["/examples/post-{id}"]
            });
        workspace.WriteVue("pages/detail-page.vue", "<template><section>Detail</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(15, result.ExitCode);
        StringAssert.Contains(result.Error, "composite route segments are not supported");
        Assert.IsFalse(File.Exists(workspace.ClientEntryPath));
    }

    [TestMethod]
    public async Task GenerateAsync_WhenOnlyHComponentsAreSelected_SkipsUnselectedBrokenSfcArtifacts()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.CatalogPage", "CatalogPage", "pages/catalog-page.mjs", ManifestComponentModel.H),
            ManifestEntry("Demo.Pages.BrokenDetailPage", "BrokenDetailPage", "pages/broken-detail.vue", ManifestComponentModel.Sfc));
        workspace.WriteHostModule(
            "pages/catalog-page.mjs",
            """
            export default {
              name: "CatalogPage"
            };
            """);

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("CatalogPage", "id:Demo.Pages.CatalogPage")));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.ClientEntryPath));
        Assert.IsFalse(Directory.Exists(workspace.BrowserGeneratedRoot));
        Assert.IsFalse(Directory.Exists(workspace.SsrGeneratedRoot));

        var clientEntry = await File.ReadAllTextAsync(workspace.ClientEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("import CatalogPage from \"../../../jazor/pages/catalog-page.mjs\";", clientEntry);
        Assert.DoesNotContain(clientEntry, "broken-detail.mjs");
    }

    [TestMethod]
    public async Task GenerateAsync_WhenOnlySelectedSfcComponentIsHealthy_SkipsUnselectedBrokenSfcArtifacts()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.DetailPage", "DetailPage", "pages/detail-page.vue", ManifestComponentModel.Sfc),
            ManifestEntry("Demo.Pages.BrokenPage", "BrokenPage", "pages/broken-page.vue", ManifestComponentModel.Sfc));
        workspace.WriteVue("pages/detail-page.vue", "<template><section>Detail</section></template>");

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("DetailPage", "id:Demo.Pages.DetailPage")));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.ClientEntryPath));
        Assert.IsTrue(File.Exists(Path.Combine(workspace.BrowserGeneratedRoot, "pages", "detail-page.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(workspace.BrowserGeneratedRoot, "pages", "broken-page.mjs")));

        var clientEntry = await File.ReadAllTextAsync(workspace.ClientEntryPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("import { DetailPage } from \"./generated-browser/pages/detail-page.mjs\";", clientEntry);
        Assert.DoesNotContain(clientEntry, "broken-page.mjs");
    }

    [TestMethod]
    public async Task GenerateAsync_WhenSelectedHComponentModuleIsMissing_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            ManifestEntry("Demo.Pages.CatalogPage", "CatalogPage", "pages/catalog-page.mjs", ManifestComponentModel.H));

        var compiler = new RazorVueConsumerEntryCompiler();
        var result = await compiler.GenerateAsync(workspace.CreateDefaultOptions(
            new RazorVueConsumerComponentSelection("CatalogPage", "id:Demo.Pages.CatalogPage")));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(14, result.ExitCode);
        StringAssert.Contains(result.Error, "RazorVue H component host module was not found");
        StringAssert.Contains(result.Error, "pages\\catalog-page.mjs");
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
        string relativeModulePath,
        string componentModel = ManifestComponentModel.Sfc)
        => new(
            AssemblyName: "Demo",
            ComponentId: componentId,
            ModuleId: componentId,
            ComponentName: componentName,
            RouteTemplates: componentName switch
            {
                "CatalogPage" => ["/", "/catalog"],
                "DetailPage" => ["/examples/{id}"],
                _ => []
            },
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
            SupportsSsr: true,
            ComponentModel: componentModel);

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
                Modules: modules.Select(ToManifestModuleEntry).ToList())
                .Save(ManifestPath);

        public void WritePlainManifest()
            => new ManifestModel(
                RootAssemblyPath: Path.Combine(RootPath, "Demo.dll"),
                GeneratedAtUtc: DateTime.UtcNow,
                Modules:
                [
                    new ManifestModuleEntry(
                        "Demo",
                        "Demo.AppModule",
                        "Demo.AppModule",
                        "host/app.mjs",
                        "content-hash",
                        "host/app.mjs.map",
                        MapHash: null,
                        Kind: ManifestModuleKind.Mjs,
                        Component: null)
                ])
                .Save(ManifestPath);

        public void WriteVue(string relativePath, string content)
            => WriteText(Path.Combine(HostJazorRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)), content);

        public void WriteHostModule(string relativePath, string content)
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

        private static ManifestModuleEntry ToManifestModuleEntry(RazorVueManifestEntry module)
            => new(
                module.AssemblyName,
                module.ComponentName,
                module.ComponentId,
                module.RelativeModulePath,
                module.ContentHash,
                module.SourceMapPath,
                MapHash: null,
                Kind: string.Equals(module.ComponentModel, ManifestComponentModel.Sfc, StringComparison.Ordinal)
                    ? ManifestModuleKind.Vue
                    : ManifestModuleKind.Mjs,
                Component: new ManifestComponentMetadata(
                    module.ComponentModel,
                    module.ComponentId,
                    module.ModuleId,
                    module.ComponentName,
                    module.RouteTemplates,
                    module.OriginMapPath,
                    module.Imports,
                    module.Styles,
                    module.PluginRequirements,
                    module.DescriptorHash,
                    module.TemplateHash,
                    module.LogicHash,
                    module.ContentHash,
                    module.HmrBoundaryKind,
                    module.RequiresHydration,
                    module.SupportsSsr,
                    module.StyleHash));
    }

    public TestContext TestContext { get; set; }
}
