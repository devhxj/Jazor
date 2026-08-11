using System.Text;
using System.Text.Json;
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
                    AssetEntry.KindVueSfc,
                    "hash-asset-1")
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
            libraryManifests: [FindLibraryManifest("ECMAScript.Vue3")]);

        var result = await new Toolchain().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));
        Assert.IsTrue(File.Exists(request.BundleOutputPath + ".map"));

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("NetpackLocalCard", script);
        Assert.Contains("Netpack SFC", script);
        Assert.DoesNotContain("./LocalCard.vue.mjs", script);
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
                    AssetEntry.KindStatic,
                    "hash-asset-1")
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
    [TestCategory("Browser")]
    public async Task BuildAsync_NetpackProduction_RunsManifestVueSfcAssetInRealBrowser()
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        if (browserPath is null)
        {
            Assert.Inconclusive(
                "Real browser Netpack SFC smoke requires Microsoft Edge, Chrome, or Chromium. " +
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
                    AssetEntry.KindVueSfc,
                    "hash-asset-1")
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
            libraryManifests: [FindLibraryManifest("ECMAScript.Vue3")]);

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
                "Real browser Netpack Vuetify smoke requires Microsoft Edge, Chrome, or Chromium. " +
                "Set RAZORVUE_BROWSER_EXE to the browser executable path.");
            return;
        }

        using var workspace = new TestWorkspace();

        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import { VBtn } from "vuetify/components";

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
        WriteManifest(workspace, "host/app.mjs", packageImports: ["vuetify/components"]);

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
                FindLibraryManifest("ECMAScript.Vue3"),
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
                    "hash-1",
                    PackageImports: packageImports)
            ]);
        if (assets is not null)
            manifest.Assets.AddRange(assets);

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
