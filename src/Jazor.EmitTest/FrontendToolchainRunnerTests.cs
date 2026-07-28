using System.Text;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class FrontendToolchainRunnerTests
{
    [TestMethod]
    public void TryParse_BuildCommand_CreatesExplicitDenoProductionRequest()
    {
        var parsed = FrontendToolchainCommand.TryParse(
            [
                "build",
                "--toolchain", "Deno",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out var command,
            out var error);

        Assert.IsTrue(parsed, error);
        Assert.IsNotNull(command);
        Assert.AreEqual(FrontendBuildMode.Production, command.Mode);
        Assert.AreEqual(FrontendToolchainKind.Deno, command.Request.Toolchain);
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.ProductionBuild));
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.SourceMaps));
        Assert.IsFalse(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.Hmr));
    }

    [TestMethod]
    public void TryParse_ServeCommand_CreatesDevelopmentRequestWithHmrCapability()
    {
        var parsed = FrontendToolchainCommand.TryParse(
            [
                "serve",
                "--toolchain", "Netpack",
                "--manifest", "manifest.json",
                "--artifacts", "artifacts",
                "--source-root", "src",
                "--out-root", "dist"
            ],
            out var command,
            out var error);

        Assert.IsTrue(parsed, error);
        Assert.IsNotNull(command);
        Assert.AreEqual(FrontendBuildMode.Development, command.Mode);
        Assert.AreEqual(FrontendToolchainKind.Netpack, command.Request.Toolchain);
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.DevelopmentServer));
        Assert.IsTrue(command.Request.RequiredCapabilities.Contains(FrontendToolchainCapability.Hmr));
    }

    [TestMethod]
    public void TryParse_RejectsMissingExplicitArtifactRoot()
    {
        var parsed = FrontendToolchainCommand.TryParse(
            [
                "build",
                "--toolchain", "Deno",
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
    public void Create_NormalizesExplicitToolchainContractPaths()
    {
        using var workspace = new TestWorkspace();

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        Assert.AreEqual(Path.GetFullPath(workspace.ManifestPath), request.ManifestPath);
        Assert.AreEqual(Path.GetFullPath(workspace.ArtifactRoot), request.ArtifactRoot);
        Assert.AreEqual(Path.GetFullPath(workspace.SourceRoot), request.SourceRoot);
        Assert.AreEqual(Path.GetFullPath(workspace.OutputRoot), request.OutputRoot);
        Assert.AreEqual(Path.Combine(request.OutputRoot, "bundle.js"), request.BundleOutputPath);
        CollectionAssert.AreEquivalent(
            new[]
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            },
            request.RequiredCapabilities.ToArray());
    }

    [TestMethod]
    public async Task BuildAsync_DenoProduction_ConsumesUnifiedRequestAndWritesBundle()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            export function Boot() {
              return "ready";
            }
            """);
        WriteManifest(workspace, "host/app.mjs");

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.AreEqual(FrontendToolchainKind.Deno, result.Toolchain);
        Assert.AreEqual(request.BundleOutputPath, result.OutputPath);
        Assert.AreEqual(1, result.ModuleCount);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("function Boot()", script);
        Assert.Contains("export {", script);
    }

    [TestMethod]
    public async Task BuildAsync_DenoProduction_UsesExplicitSourceRootForRegisteredVueSfcAsset()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.ArtifactRoot, "host/app.mjs",
            """
            import LocalCard from "./LocalCard.vue";

            export const ComponentName = LocalCard.name;
            export default LocalCard;
            """);
        WriteModule(workspace.SourceRoot, "components/LocalCard.vue",
            """
            <template>
              <section>Toolchain SFC</section>
            </template>

            <script>
            export default {
              name: "ToolchainLocalCard"
            };
            </script>
            """);
        WriteManifest(
            workspace,
            "host/app.mjs",
            [
                new ManifestAssetEntry(
                    "components/LocalCard.vue",
                    "host/LocalCard.vue",
                    ManifestAssetEntry.KindVueSfc,
                    "hash-asset-1")
            ]);

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("ToolchainLocalCard", script);
        Assert.Contains("Toolchain SFC", script);
        Assert.DoesNotContain("./LocalCard.vue", script);
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

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Netpack,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.AreEqual(FrontendToolchainKind.Netpack, result.Toolchain);
        Assert.AreEqual(request.BundleOutputPath, result.OutputPath);
        Assert.AreEqual(1, result.ModuleCount);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));
        Assert.IsTrue(File.Exists(request.BundleOutputPath + ".map"));

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("netpack-ready", script);
        Assert.Contains("Boot", script);
        Assert.Contains("sourceMappingURL=bundle.js.map", script);
    }

    [TestMethod]
    public async Task BuildAsync_NetpackProduction_UsesManifestVueSfcAssetFromGeneratedImport()
    {
        using var workspace = new TestWorkspace();
        if (!TryWriteVueBrowserPackage(workspace.SourceRoot, out var vuePackageError))
        {
            Assert.Inconclusive(vuePackageError);
            return;
        }

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
                new ManifestAssetEntry(
                    "components/LocalCard.vue",
                    "host/LocalCard.vue",
                    ManifestAssetEntry.KindVueSfc,
                    "hash-asset-1")
            ]);

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Netpack,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsTrue(result.IsSuccess, result.Diagnostic?.Message ?? string.Empty);
        Assert.AreEqual(FrontendToolchainKind.Netpack, result.Toolchain);
        Assert.IsTrue(File.Exists(request.BundleOutputPath));
        Assert.IsTrue(File.Exists(request.BundleOutputPath + ".map"));

        var script = await File.ReadAllTextAsync(request.BundleOutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("NetpackLocalCard", script);
        Assert.Contains("Netpack SFC", script);
        Assert.DoesNotContain("./LocalCard.vue.mjs", script);
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
                new ManifestAssetEntry(
                    "assets/logo.svg",
                    "assets/logo.svg",
                    ManifestAssetEntry.KindStatic,
                    "hash-asset-1")
            ]);

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Netpack,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

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
        if (!TryWriteVueBrowserPackage(workspace.SourceRoot, out var vuePackageError))
        {
            Assert.Inconclusive(vuePackageError);
            return;
        }

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
                new ManifestAssetEntry(
                    "components/LocalCard.vue",
                    "host/LocalCard.vue",
                    ManifestAssetEntry.KindVueSfc,
                    "hash-asset-1")
            ]);

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Netpack,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

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
        if (!TryWriteVueBrowserPackage(workspace.SourceRoot, out var vuePackageError))
        {
            Assert.Inconclusive(vuePackageError);
            return;
        }

        if (!TryCopyRepositoryPackage("vuetify", workspace.SourceRoot, out var vuetifyPackageError))
        {
            Assert.Inconclusive(vuetifyPackageError);
            return;
        }

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
        WriteManifest(workspace, "host/app.mjs");

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Netpack,
            workspace.ManifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot,
            requiredCapabilities: new HashSet<FrontendToolchainCapability>
            {
                FrontendToolchainCapability.ProductionBuild,
                FrontendToolchainCapability.SourceMaps
            });

        var result = await new FrontendToolchainRunner().BuildAsync(request);

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
        Assert.IsTrue(smoke.GetProperty("styleCount").GetInt32() > 0, smoke.GetRawText());
    }

    [TestMethod]
    public async Task BuildAsync_MissingArtifactRoot_ReturnsTypedContractDiagnostic()
    {
        using var workspace = new TestWorkspace();
        WriteManifest(workspace, "host/app.mjs");
        var manifestPath = Path.Combine(workspace.RootPath, "jazor-manifest.json");
        File.Copy(workspace.ManifestPath, manifestPath);
        Directory.Delete(workspace.ArtifactRoot, recursive: true);

        var request = FrontendToolchainRequest.Create(
            FrontendToolchainKind.Deno,
            manifestPath,
            workspace.ArtifactRoot,
            workspace.SourceRoot,
            workspace.OutputRoot);

        var result = await new FrontendToolchainRunner().BuildAsync(request);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(FrontendToolchainKind.Deno, result.Toolchain);
        Assert.AreEqual("JAZOR_TOOLCHAIN_ARTIFACT_ROOT_NOT_FOUND", result.Diagnostic?.Code);
        Assert.Contains(workspace.ArtifactRoot, result.Diagnostic?.Message ?? string.Empty);
    }

    private static void WriteManifest(
        TestWorkspace workspace,
        string relativePath,
        IReadOnlyList<ManifestAssetEntry>? assets = null)
    {
        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", relativePath, "hash-1")
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
                      styleCount: document.head.querySelectorAll("style").length
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

    private static bool TryWriteVueBrowserPackage(string sourceRoot, out string error)
    {
        var vueBrowserModule = TryFindRepositoryFile("node_modules/vue/dist/vue.esm-browser.prod.js");
        if (vueBrowserModule is null)
        {
            error =
                "Real browser Netpack SFC smoke requires node_modules/vue/dist/vue.esm-browser.prod.js. " +
                "Install repository frontend dependencies before running Browser tests.";
            return false;
        }

        var packageRoot = Path.Combine(sourceRoot, "node_modules", "vue");
        var distRoot = Path.Combine(packageRoot, "dist");
        Directory.CreateDirectory(distRoot);
        File.Copy(vueBrowserModule, Path.Combine(distRoot, "vue.esm-browser.prod.js"), overwrite: true);
        WriteModule(packageRoot, "package.json",
            """
            {
              "name": "vue",
              "type": "module",
              "module": "./dist/vue.esm-browser.prod.js",
              "exports": {
                ".": "./dist/vue.esm-browser.prod.js"
              }
            }
            """);

        error = string.Empty;
        return true;
    }

    private static bool TryCopyRepositoryPackage(string packageName, string sourceRoot, out string error)
    {
        var packageJson = TryFindRepositoryFile($"node_modules/{packageName}/package.json");
        if (packageJson is null)
        {
            error =
                $"Netpack package smoke requires node_modules/{packageName}/package.json. " +
                "Install repository frontend dependencies before running Browser tests.";
            return false;
        }

        var sourcePackageRoot = Directory.GetParent(packageJson)!.FullName;
        var targetPackageRoot = Path.Combine(sourceRoot, "node_modules", packageName);
        CopyDirectory(sourcePackageRoot, targetPackageRoot);

        error = string.Empty;
        return true;
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var sourceFile in Directory.EnumerateFiles(sourceDirectory))
        {
            var targetFile = Path.Combine(targetDirectory, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, targetFile, overwrite: true);
        }

        foreach (var sourceChildDirectory in Directory.EnumerateDirectories(sourceDirectory))
        {
            var targetChildDirectory = Path.Combine(targetDirectory, Path.GetFileName(sourceChildDirectory));
            CopyDirectory(sourceChildDirectory, targetChildDirectory);
        }
    }

    private static string? TryFindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return null;
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
