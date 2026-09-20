using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Jazor.Common;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ToolchainTests
{
    [TestMethod]
    public void TryParse_BuildCommand_CreatesNetpackProductionRequest()
    {
        var parsed = ToolchainCommand.TryParse(
            [
                "build",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out var command,
            out var error);

        Assert.IsTrue(parsed, error);
        Assert.IsNotNull(command);
        Assert.AreEqual(BuildMode.Production, command.Mode);
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(ToolchainCapability.ProductionBuild));
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(ToolchainCapability.SourceMaps));
        Assert.IsFalse(command.Request.RequiredCapabilities.Contains(ToolchainCapability.Hmr));
    }

    [TestMethod]
    public void TryParse_ServeCommand_CreatesDevelopmentRequestWithHmrCapability()
    {
        var parsed = ToolchainCommand.TryParse(
            [
                "serve",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out var command,
            out var error);

        Assert.IsTrue(parsed, error);
        Assert.IsNotNull(command);
        Assert.AreEqual(BuildMode.Development, command.Mode);
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(ToolchainCapability.DevelopmentServer));
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(ToolchainCapability.Hmr));
    }

    [TestMethod]
    public void TryParse_RejectsRemovedToolchainSelector()
    {
        var parsed = ToolchainCommand.TryParse(
            [
                "build",
                "--toolchain", "Deno",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out _,
            out var error);

        Assert.IsFalse(parsed);
        Assert.AreEqual("Unknown argument '--toolchain'.", error);
    }

    [TestMethod]
    public void TryParse_RejectsMissingExplicitArtifactRoot()
    {
        var parsed = ToolchainCommand.TryParse(
            [
                "build",
                "--manifest", "manifest.json",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out _,
            out var error);

        Assert.IsFalse(parsed);
        Assert.AreEqual("Missing required argument --artifacts.", error);
    }

    [TestMethod]
    public void Create_NormalizesNetpackBuildContractPaths()
    {
        using var workspace = new TestWorkspace();

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps
            });

        Assert.AreEqual(Path.GetFullPath(workspace.ManifestPath), request.ManifestPath);
        Assert.AreEqual(Path.GetFullPath(workspace.ArtifactRoot), request.ArtifactRoot);
        Assert.AreEqual(Path.GetFullPath(workspace.SourceRoot), request.SourceRoot);
        Assert.AreEqual(Path.GetFullPath(workspace.OutputRoot), request.OutputRoot);
        Assert.AreEqual(Path.Combine(request.OutputRoot, "bundle.js"), request.BundleOutputPath);
        CollectionAssert.AreEquivalent(
            new[]
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps
            },
            request.RequiredCapabilities.ToArray());
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_ConsumesUnifiedRequestAndWritesBundle()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            export function Boot() {
              return "netpack-ready";
            }
            """);
        WriteManifest(workspace, "host/app.mjs");

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild
            });

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.AreEqual(request.BundleOutputPath, result.OutputPath);
        Assert.AreEqual(1, result.ModuleCount);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));
        Assert.IsTrue(File.Exists(request.BundleOutputPath + ".map"));

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("netpack-ready", script);
        Assert.Contains("Boot", script);
        Assert.Contains("sourceMappingURL=bundle.js.map", script);

        using var sourceMap = JsonDocument.Parse(
            await File.ReadAllTextAsync(request.BundleOutputPath + ".map", TestContext.CancellationTokenSource.Token));
        Assert.AreEqual("bundle.js", sourceMap.RootElement.GetProperty("file").GetString());
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_BundlesGeneratedRouteCatalogThroughBlazorRoutingRuntime()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import { createNavigationHost } from "@jazor/vue-runtime/blazor-routing.mjs";

            export const hasRouteHost = typeof createNavigationHost === "function";
            """);
        WriteModule(workspace.ArtifactRoot, "@jazor/vue-runtime/routes.mjs",
            """
            export const routes = [{
              template: "/tasks",
              component: "Sample.Host.Tasks",
              layout: null,
              parameters: [],
              queries: []
            }];
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ModuleEntry(
                    "Sample.Host",
                    "Sample.Host.AppModule",
                    "Sample.Host.AppModule",
                    "host/app.mjs",
                    ArtifactHash.ComputeSha256(File.ReadAllBytes(Path.Combine(workspace.ArtifactRoot, "host", "app.mjs"))),
                    PackageImports: ["@jazor/vue-runtime/blazor-routing.mjs"]),
                new ModuleEntry(
                    "Sample.Host",
                    "Jazor.Generated.RazorVue.RouteCatalog",
                    "Jazor.Generated.RazorVue.RouteCatalog",
                    "@jazor/vue-runtime/routes.mjs",
                    ArtifactHash.ComputeSha256(File.ReadAllBytes(Path.Combine(workspace.ArtifactRoot, "@jazor", "vue-runtime", "routes.mjs"))))
            ]);
        manifest.Save(workspace.ManifestPath);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps
            },
            libraryManifests:
            [
                FindLibraryManifest("ECMAScript"),
                FindLibraryManifest("ECMAScript.Vue"),
                FindLibraryManifest("Jazor.Vue")
            ]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.AreEqual(2, result.ModuleCount);

        var bundle = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.DoesNotContain("blazor-routing.mjs", bundle, StringComparison.Ordinal);
        Assert.DoesNotContain("@jazor/vue-runtime/routes.mjs", bundle, StringComparison.Ordinal);
        Assert.Contains("createNavigationHost", bundle, StringComparison.Ordinal);

        using var sourceMap = JsonDocument.Parse(
            await File.ReadAllTextAsync(request.BundleOutputPath + ".map", TestContext.CancellationTokenSource.Token));
        var sources = sourceMap.RootElement.GetProperty("sources")
            .EnumerateArray()
            .Select(static source => source.GetString() ?? string.Empty)
            .ToArray();
        CollectionAssert.Contains(sources, "host/app.mjs");
        CollectionAssert.Contains(sources, "@jazor/vue-runtime/routes.mjs");
        CollectionAssert.Contains(sources, "__jazor_runtime/blazor-routing.mjs");
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_UsesManifestVueSfcAssetFromGeneratedImport()
    {
        using var workspace = new TestWorkspace();

        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import LocalCard from "./LocalCard.vue.mjs";

            export const ComponentName = LocalCard.name;
            export default LocalCard;
            """);
        WriteModule(workspace.SourceRoot, "components/LocalCard.vue",
            """
            <template>
              <section>Netpack SFC</section>
            </template>

            <script>
            export default {
              name: "NetpackLocalCard"
            };
            </script>
            """);
        WriteManifest(
            workspace,
            "host/app.mjs",
            [
                new AssetEntry(
                    "components/LocalCard.vue",
                    "host/LocalCard.vue",
                    AssetEntry.KindModuleSource,
                    ImportPath: "host/LocalCard.vue.mjs")
            ],
            ["vue"]);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps
            },
            libraryManifests: [FindLibraryManifest("ECMAScript.Vue")]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));
        Assert.IsTrue(File.Exists(request.BundleOutputPath + ".map"));

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("NetpackLocalCard", script);
        Assert.Contains("Netpack SFC", script);
        Assert.DoesNotContain("./LocalCard.vue.mjs", script);
        Assert.DoesNotContain("server-renderer.esm-browser.prod.js", script);
        Assert.DoesNotContain("vue-devtools-api.esm-browser.js", script);
        Assert.IsFalse(Directory.Exists(Path.Combine(workspace.SourceRoot, "node_modules")));
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_CopiesManifestStaticAssetToOutputRoot()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            export const Ready = true;
            """);
        WriteModule(workspace.SourceRoot, "assets/logo.svg",
            """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1 1"></svg>
            """);
        WriteManifest(
            workspace,
            "host/app.mjs",
            [
                new AssetEntry(
                    "assets/logo.svg",
                    "assets/logo.svg",
                    AssetEntry.KindStatic)
            ]);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild
            });

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);

        var outputAssetPath = Path.Combine(workspace.OutputRoot, "assets", "logo.svg");
        Assert.IsTrue(File.Exists(outputAssetPath), $"Expected static asset output: {outputAssetPath}");
        var asset = await File.ReadAllTextAsync(outputAssetPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("<svg", asset);
    }

    [TestMethod]
    public async Task BuildAsync_MinifiedLibraryGraph_ShakesUnusedExportsAndPreservesReachableResources()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import { used } from "tree-lib/used";

            export const result = used();
            """);
        WriteManifest(workspace, "host/app.mjs", packageImports: ["tree-lib/used"]);

        var libraryRoot = Path.Combine(workspace.RootPath, "tree-lib");
        WriteModule(libraryRoot, "dist/used.mjs",
            """
            import { dependency } from "./dependency.mjs";

            export function used() { return "USED_SENTINEL:" + dependency; }
            export function dead() { return "DEAD_SENTINEL"; }
            """);
        WriteModule(libraryRoot, "dist/dependency.mjs",
            """
            export const dependency = "INTERNAL_DEPENDENCY_SENTINEL";
            """);
        WriteModule(libraryRoot, "dist/unreachable.mjs",
            """
            export const unreachable = "UNREACHABLE_SENTINEL";
            """);
        WriteModule(libraryRoot, "dist/unused.mjs",
            """
            export const unusedEntry = "UNUSED_ENTRY_SENTINEL";
            """);
        WriteModule(libraryRoot, "dist/editor.worker.mjs",
            """
            globalThis.onmessage = () => "WORKER_SENTINEL";
            """);
        WriteModule(libraryRoot, "dist/used.css", ".used-style { color: green; }");
        WriteModule(libraryRoot, "dist/unused.css", ".unused-style { color: red; }");

        var libraryManifest = WriteSyntheticTreeShakingManifest(libraryRoot);
        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            minify: true,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps,
                ToolchainCapability.Minify
            },
            libraryManifests: [libraryManifest]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        var bundle = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("USED_SENTINEL", bundle, StringComparison.Ordinal);
        Assert.Contains("INTERNAL_DEPENDENCY_SENTINEL", bundle, StringComparison.Ordinal);
        Assert.DoesNotContain("DEAD_SENTINEL", bundle, StringComparison.Ordinal);
        Assert.DoesNotContain("UNREACHABLE_SENTINEL", bundle, StringComparison.Ordinal);
        Assert.DoesNotContain("UNUSED_ENTRY_SENTINEL", bundle, StringComparison.Ordinal);
        Assert.IsFalse(Directory.EnumerateFiles(workspace.OutputRoot, "*.mjs", SearchOption.AllDirectories)
            .Any(path => path.EndsWith("used.mjs", StringComparison.OrdinalIgnoreCase) ||
                         path.EndsWith("dependency.mjs", StringComparison.OrdinalIgnoreCase) ||
                         path.EndsWith("unreachable.mjs", StringComparison.OrdinalIgnoreCase)));

        var css = await File.ReadAllTextAsync(
            Path.ChangeExtension(request.BundleOutputPath, ".css"),
            TestContext.CancellationTokenSource.Token);
        Assert.Contains(".used-style", css, StringComparison.Ordinal);
        Assert.DoesNotContain(".unused-style", css, StringComparison.Ordinal);

        // 自有源码 carrier 按声明路径写入项目源码树，不再套 packages/<name>/ 合成包根。
        var workerPath = Path.Combine(
            workspace.OutputRoot,
            "dist",
            "editor.worker.mjs");
        Assert.IsTrue(File.Exists(workerPath), $"Expected selected worker output: {workerPath}");
        Assert.Contains("WORKER_SENTINEL", await File.ReadAllTextAsync(workerPath), StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_BundlesSelectedVueDataUiStylesheetClosure()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import { VueUiDonut } from "vue-data-ui/vue-ui-donut";

            export const selectedChart = VueUiDonut;
            """);
        WriteManifest(workspace, "host/app.mjs", packageImports: ["vue-data-ui/vue-ui-donut"]);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            minify: true,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps,
                ToolchainCapability.Minify
            },
            libraryManifests:
            [
                FindLibraryManifest("ECMAScript.Vue"),
                FindLibraryManifest("ECMAScript.VueDataUi")
            ]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));
        var css = await File.ReadAllTextAsync(
            Path.ChangeExtension(request.BundleOutputPath, ".css"),
            TestContext.CancellationTokenSource.Token);
        Assert.Contains(".vue-ui-donut", css, StringComparison.Ordinal);
        // vue-data-ui publishes one global stylesheet. The package graph can select that
        // stylesheet file as a whole, while selector-level pruning would change upstream CSS
        // semantics and is outside the ESM/package exports contract.
        Assert.Contains(".vue-ui-xy[", css, StringComparison.Ordinal);
    }

    [TestMethod]
    [DataRow("ECMAScript.VueUse", "@vueuse/core", "useToggle", "dragover")]
    [DataRow("ECMAScript.VueI18n", "vue-i18n", "useI18n", "globalInjection")]
    [DataRow("ECMAScript.VeeValidate", "vee-validate", "configure", "No vee-validate <Form /> or `useForm` was detected in the component tree")]
    public async Task BuildAsync_NetpackProduction_ShakesUnusedAggregateBindingExports(
        string manifestName,
        string specifier,
        string selectedExport,
        string excludedMarker)
    {
        using var workspace = new TestWorkspace();
        WriteModule(
            workspace.ArtifactRoot,
            "host/app.mjs",
            $"import {{ {selectedExport} }} from \"{specifier}\";\n\n" +
            $"export const selectedBinding = {selectedExport};\n");
        WriteManifest(workspace, "host/app.mjs", packageImports: [specifier]);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            minify: true,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps,
                ToolchainCapability.Minify
            },
            libraryManifests:
            [
                FindLibraryManifest("ECMAScript.Vue"),
                FindLibraryManifest(manifestName)
            ]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        var bundle = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("selectedBinding", bundle, StringComparison.Ordinal);
        Assert.DoesNotContain(excludedMarker, bundle, StringComparison.Ordinal);

        var bareImports = Regex.Matches(
                bundle,
                """\b(?:from\s*|import\s*\(\s*)['"](?<specifier>[^'"]+)['"]""")
            .Select(static match => match.Groups["specifier"].Value)
            .Where(static value => !value.StartsWith('.', StringComparison.Ordinal) &&
                                   !value.StartsWith('/', StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        Assert.IsEmpty(bareImports, "Final NetPack bundle retains bare imports: " + string.Join(", ", bareImports));
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_PreservesCoupledVueDraggableRuntime()
    {
        using var workspace = new TestWorkspace();
        WriteModule(
            workspace.ArtifactRoot,
            "host/app.mjs",
            "import { useDraggable } from \"vue-draggable-plus\";\n\n" +
            "export const selectedBinding = useDraggable;\n");
        WriteManifest(workspace, "host/app.mjs", packageImports: ["vue-draggable-plus"]);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            minify: true,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps,
                ToolchainCapability.Minify
            },
            libraryManifests:
            [
                FindLibraryManifest("ECMAScript.Vue"),
                FindLibraryManifest("ECMAScript.VueDraggable")
            ]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        var bundle = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("selectedBinding", bundle, StringComparison.Ordinal);
        // vue-draggable-plus ships one coupled runtime: the component, directive, composable,
        // and Sortable lifecycle share state, so the aggregate entry is the supported closure.
        Assert.Contains("VueDraggable", bundle, StringComparison.Ordinal);
        var bareImports = Regex.Matches(
                bundle,
                "\\b(?:from\\s*|import\\s*\\(\\s*)['\"](?<specifier>[^'\"]+)['\"]")
            .Select(static match => match.Groups["specifier"].Value)
            .Where(static value => !value.StartsWith('.', StringComparison.Ordinal) &&
                                   !value.StartsWith('/', StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        Assert.IsEmpty(bareImports, "Coupled VueDraggable bundle retains bare imports: " + string.Join(", ", bareImports));
    }

    [TestMethod]
    [TestCategory("Browser")]
    public async Task BuildAsync_NetpackProduction_RunsManifestVueSfcAssetInRealBrowser()
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        if (browserPath is null)
        {
            Assert.Inconclusive(
                "Real browser Netpack SFC smoke requires Google Chrome or Chromium. " +
                "Set RAZORVUE_BROWSER_EXE to the browser executable path.");
            return;
        }

        using var workspace = new TestWorkspace();

        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import { createApp, nextTick } from "vue";
            import LocalCard from "./LocalCard.vue.mjs";

            export async function mount(selector) {
              createApp(LocalCard, { title: "Netpack Browser SFC" }).mount(selector);
              await nextTick();
            }
            """);
        WriteModule(workspace.SourceRoot, "components/LocalCard.vue",
            """
            <template>
              <section class="local-card">{{ title }}</section>
            </template>

            <script>
            export default {
              name: "NetpackLocalCard",
              props: {
                title: {
                  type: String,
                  default: "Fallback"
                }
              }
            };
            </script>

            <style>
            .local-card {
              color: rgb(40, 50, 60);
            }
            </style>
            """);
        WriteManifest(
            workspace,
            "host/app.mjs",
            [
                new AssetEntry(
                    "components/LocalCard.vue",
                    "host/LocalCard.vue",
                    AssetEntry.KindModuleSource,
                    ImportPath: "host/LocalCard.vue.mjs")
            ],
            ["vue"]);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps
            },
            libraryManifests: [FindLibraryManifest("ECMAScript.Vue")]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.IsTrue(File.Exists(request.BundleOutputPath), $"Expected browser bundle: {request.BundleOutputPath}");

        WriteBrowserSmokeHarness(workspace.OutputRoot);
        var browser = await BrowserSmokeTestHelper.RunBrowserDumpDomAsync(
            browserPath,
            Path.Combine(workspace.OutputRoot, "index.html"),
            virtualTimeBudgetMilliseconds: 20000);
        Assert.AreEqual(0, browser.ExitCode, browser.ToString());

        using var smokePayload = BrowserSmokeTestHelper.ReadBrowserSmokePayload(browser, "Netpack SFC");
        var smoke = smokePayload.RootElement;
        Assert.IsTrue(
            smoke.GetProperty("ok").GetBoolean(),
            "Browser Netpack SFC smoke failed." + Environment.NewLine + smoke.GetRawText() + Environment.NewLine + browser);
        Assert.AreEqual("Netpack Browser SFC", smoke.GetProperty("text").GetString(), smoke.GetRawText());
    }

    [TestMethod]
    [TestCategory("Browser")]
    public async Task BuildAsync_NetpackProduction_BundlesVuetifyImportInRealBrowser()
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        if (browserPath is null)
        {
            Assert.Inconclusive(
                "Real browser Netpack Vuetify smoke requires Google Chrome or Chromium. " +
                "Set RAZORVUE_BROWSER_EXE to the browser executable path.");
            return;
        }

        using var workspace = new TestWorkspace();

        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import { VBtn } from "vuetify/components/VBtn";

            export async function mount(selector) {
              const target = document.querySelector(selector);
              if (!(target instanceof HTMLElement)) {
                throw new Error("Mount target was not found.");
              }

              target.innerHTML = "";
              const button = document.createElement("button");
              button.className = "vuetify-smoke";
              button.textContent = VBtn.name;
              target.append(button);
            }
            """);
        WriteManifest(workspace, "host/app.mjs", packageImports: ["vuetify/components/VBtn"]);

        var request = ToolchainRequest.Create(
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<ToolchainCapability>
            {
                ToolchainCapability.ProductionBuild,
                ToolchainCapability.SourceMaps,
                ToolchainCapability.Minify
            },
            libraryManifests:
            [
                FindLibraryManifest("ECMAScript.Vue"),
                FindLibraryManifest("ECMAScript.Vuetify")
            ]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.IsTrue(File.Exists(request.BundleOutputPath), $"Expected browser bundle: {request.BundleOutputPath}");

        WriteVuetifyBrowserSmokeHarness(workspace.OutputRoot);
        var browser = await BrowserSmokeTestHelper.RunBrowserDumpDomAsync(
            browserPath,
            Path.Combine(workspace.OutputRoot, "index.html"),
            virtualTimeBudgetMilliseconds: 20000);
        Assert.AreEqual(0, browser.ExitCode, browser.ToString());

        using var smokePayload = BrowserSmokeTestHelper.ReadBrowserSmokePayload(browser, "Netpack Vuetify");
        var smoke = smokePayload.RootElement;
        Assert.IsTrue(
            smoke.GetProperty("ok").GetBoolean(),
            "Browser Netpack Vuetify smoke failed." + Environment.NewLine + smoke.GetRawText() + Environment.NewLine + browser);
        Assert.AreEqual("VBtn", smoke.GetProperty("text").GetString(), smoke.GetRawText());
        Assert.IsTrue(smoke.GetProperty("styleSheetCount").GetInt32() > 0, smoke.GetRawText());

        var css = File.ReadAllText(Path.Combine(workspace.OutputRoot, "bundle.css"));
        StringAssert.Contains(css, ".v-btn");
        Assert.IsFalse(css.Contains(".v-alert", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task BuildAsync_MissingArtifactRoot_ReturnsTypedContractDiagnostic()
    {
        using var workspace = new TestWorkspace();
        WriteManifest(workspace, "host/app.mjs");
        var manifestPath = Path.Combine(workspace.RootPath, "jazor-manifest.json");
        File.Copy(workspace.ManifestPath, manifestPath);
        Directory.Delete(workspace.ArtifactRoot, recursive: true);

        var request = ToolchainRequest.Create(
            manifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("JAZOR_TOOLCHAIN_ARTIFACT_ROOT_NOT_FOUND", result.Diagnostic?.Code);
        Assert.Contains(workspace.ArtifactRoot, result.Diagnostic?.Message ?? string.Empty);
    }

    private static void WriteManifest(
        TestWorkspace workspace,
        string relativePath,
        IReadOnlyList<AssetEntry>? assets = null,
        IReadOnlyList<string>? packageImports = null)
    {
        var modulePath = Path.Combine(
            workspace.ArtifactRoot,
            relativePath.Replace('/', Path.DirectorySeparatorChar));
        var moduleHash = ArtifactHash.ComputeSha256(
            File.Exists(modulePath) ? File.ReadAllBytes(modulePath) : []);
        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ModuleEntry(
                    "Sample.Host",
                    "Sample.Host.AppModule",
                    "Sample.Host.AppModule",
                    relativePath,
                    moduleHash,
                    PackageImports: packageImports)
            ]);
        if (assets is not null)
        {
            manifest.Assets.AddRange(assets.Select(asset =>
            {
                var sourcePath = Path.Combine(
                    workspace.SourceRoot,
                    asset.SourcePath.Replace('/', Path.DirectorySeparatorChar));
                return asset with { Hash = ArtifactHash.ComputeSha256(File.ReadAllBytes(sourcePath)) };
            }));
        }

        manifest.Save(workspace.ManifestPath);
    }

    private static void WriteModule(string rootDirectory, string relativePath, string content)
    {
        var fullPath = Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(fullPath, content.ReplaceLineEndings("\n"), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string WriteSyntheticTreeShakingManifest(string libraryRoot)
    {
        JsonObject FileRecord(string type, string relativePath, bool module = false)
        {
            var record = new JsonObject
            {
                ["type"] = type,
                ["path"] = relativePath,
                ["hash"] = ArtifactHash.ComputeSha256(File.ReadAllBytes(Path.Combine(libraryRoot, relativePath)))
            };
            if (module)
                record["moduleId"] = relativePath;
            return record;
        }

        JsonObject Entry(string modulePath, string? stylePath = null, JsonArray? files = null)
        {
            var hash = ArtifactHash.ComputeSha256(File.ReadAllBytes(Path.Combine(libraryRoot, modulePath)));
            var styles = stylePath is null
                ? new JsonArray()
                : new JsonArray(FileRecord("style", stylePath));
            return new JsonObject
            {
                ["type"] = "module",
                ["development"] = modulePath,
                ["production"] = modulePath,
                ["developmentHash"] = hash,
                ["productionHash"] = hash,
                ["developmentDependencies"] = new JsonArray(),
                ["productionDependencies"] = new JsonArray(),
                ["developmentModuleDependencies"] = new JsonArray(),
                ["productionModuleDependencies"] = new JsonArray(),
                ["developmentStyles"] = styles.DeepClone(),
                ["productionStyles"] = styles.DeepClone(),
                ["files"] = files ?? new JsonArray()
            };
        }

        var usedFiles = new JsonArray(
            FileRecord("module", "dist/dependency.mjs", module: true),
            FileRecord("module", "dist/unreachable.mjs", module: true),
            FileRecord("worker", "dist/editor.worker.mjs"));
        var manifest = new JsonObject
        {
            ["schemaVersion"] = 2,
            ["libraryId"] = "tree-lib",
            ["version"] = "1.0.0",
            ["source"] = "embedded-mjs",
            ["imports"] = new JsonObject
            {
                ["tree-lib/used"] = Entry("dist/used.mjs", "dist/used.css", usedFiles),
                ["tree-lib/unused"] = Entry("dist/unused.mjs", "dist/unused.css")
            },
            ["requires"] = new JsonObject(),
            ["styles"] = new JsonArray(),
            ["files"] = new JsonArray()
        };
        var path = Path.Combine(libraryRoot, "manifest.json");
        File.WriteAllText(path, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + "\n");
        return path;
    }

    private static void WriteBrowserSmokeHarness(string rootPath)
    {
        WriteModule(rootPath, "index.html",
            """
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="utf-8">
                <title>Jazor Netpack SFC Browser Smoke</title>
              </head>
              <body>
                <div id="app"></div>
                <script type="module">
                  import * as bundle from "./bundle.js";

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

                  try {
                    const mount = bundle.mount || (bundle.default && bundle.default.mount);
                    if (typeof mount !== "function") {
                      throw new Error("Netpack bundle did not expose mount.");
                    }

                    await mount("#app");
                    const card = document.querySelector(".local-card");
                    if (!(card instanceof HTMLElement)) {
                      throw new Error("Manifest SFC did not render .local-card.");
                    }

                    finish({
                      ok: true,
                      text: card.textContent || "",
                      color: getComputedStyle(card).color
                    });
                  } catch (error) {
                    finish({
                      ok: false,
                      error: error instanceof Error ? (error.stack || error.message) : String(error),
                      bodyText: document.body ? (document.body.textContent || "") : ""
                    });
                  }
                </script>
              </body>
            </html>
            """);
    }

    private static void WriteVuetifyBrowserSmokeHarness(string rootPath)
    {
        WriteModule(rootPath, "index.html",
            """
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="utf-8">
                <title>Jazor Netpack Vuetify Browser Smoke</title>
                <link rel="stylesheet" href="./bundle.css">
              </head>
              <body>
                <div id="app"></div>
                <script type="module">
                  import * as bundle from "./bundle.js";

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

                  try {
                    const mount = bundle.mount || (bundle.default && bundle.default.mount);
                    if (typeof mount !== "function") {
                      throw new Error("Netpack bundle did not expose mount.");
                    }

                    await mount("#app");
                    const button = document.querySelector(".vuetify-smoke");
                    if (!(button instanceof HTMLElement)) {
                      throw new Error("Vuetify import smoke did not render .vuetify-smoke.");
                    }

                    finish({
                      ok: true,
                      text: button.textContent || "",
                      styleSheetCount: document.styleSheets.length
                    });
                  } catch (error) {
                    finish({
                      ok: false,
                      error: error instanceof Error ? (error.stack || error.message) : String(error),
                      bodyText: document.body ? (document.body.textContent || "") : ""
                    });
                  }
                </script>
              </body>
            </html>
            """);
    }

    private static string FindLibraryManifest(string projectName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", projectName, "manifest.json");
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate the {projectName} library manifest.");
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            ArtifactRoot = Path.Combine(RootPath, "artifacts");
            SourceRoot = Path.Combine(RootPath, "src");
            OutputRoot = Path.Combine(RootPath, "dist");
            ManifestPath = Path.Combine(ArtifactRoot, "jazor-manifest.json");
            Directory.CreateDirectory(ArtifactRoot);
            Directory.CreateDirectory(SourceRoot);
        }

        public string RootPath { get; }

        public string ArtifactRoot { get; }

        public string SourceRoot { get; }

        public string OutputRoot { get; }

        public string ManifestPath { get; }

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
