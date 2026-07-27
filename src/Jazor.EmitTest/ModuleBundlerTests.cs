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
            workspace.OutputPath));

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
            workspace.OutputPath));

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
            workspace.OutputPath));

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
            workspace.OutputPath));

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
    public async Task BundleAsync_ExplicitVueSfcAsset_CompilesAndRewritesRegisteredImport()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            import LocalCard from "./LocalCard.vue";

            export const ComponentName = LocalCard.name;
            export default LocalCard;
            """);
        WriteModule(workspace.SourceRoot, "components/LocalCard.vue",
            """
            <template>
              <section class="local-card">Hello from SFC</section>
            </template>

            <script>
            export default {
              name: "LocalCard"
            };
            </script>

            <style>
            .local-card {
              color: rgb(10, 20, 30);
            }
            </style>
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Assets.Add(new ManifestAssetEntry(
            "components/LocalCard.vue",
            "host/LocalCard.vue",
            ManifestAssetEntry.KindVueSfc,
            "hash-asset-1"));
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            workspace.SourceRoot));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var script = await File.ReadAllTextAsync(workspace.OutputPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("LocalCard", script);
        Assert.Contains("Hello from SFC", script);
        Assert.DoesNotContain("./LocalCard.vue", script);

        Assert.IsTrue(File.Exists(workspace.OutputCssPath), $"Expected SFC style bundle: {workspace.OutputCssPath}");
        var css = await File.ReadAllTextAsync(workspace.OutputCssPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains(".local-card", css);
        Assert.Contains("rgb(10, 20, 30)", css);

        Assert.IsTrue(File.Exists(workspace.OutputMapPath), $"Expected SFC bundle source map: {workspace.OutputMapPath}");
        using var map = await ReadJsonAsync(workspace.OutputMapPath);
        var sources = map.RootElement.GetProperty("sources")
            .EnumerateArray()
            .Select(static item => item.GetString())
            .Where(static item => item is not null)
            .Cast<string>()
            .ToArray();
        Assert.IsTrue(
            sources.Any(static source => source.Contains("LocalCard.vue.mjs", StringComparison.Ordinal) ||
                                         source.Contains("LocalCard.vue", StringComparison.Ordinal)),
            map.RootElement.ToString());
    }

    [TestMethod]
    public async Task BundleAsync_RegisteredAssetWithoutSourceRoot_ReturnsFailure()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            export const Ready = true;
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Assets.Add(new ManifestAssetEntry(
            "components/LocalCard.vue",
            "host/LocalCard.vue",
            ManifestAssetEntry.KindVueSfc,
            "hash-asset-1"));
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(6, result.ExitCode);
        StringAssert.Contains(result.Error, "explicit source root", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ManifestModel_SaveAndLoad_PreservesExplicitFrontendAssets()
    {
        using var workspace = new TestWorkspace();
        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Assets.Add(new ManifestAssetEntry(
            "components\\LocalCard.vue",
            "host/LocalCard.vue",
            ManifestAssetEntry.KindVueSfc,
            "hash-asset-1"));

        manifest.Save(workspace.ManifestPath);

        var loaded = ManifestModel.TryLoad(workspace.ManifestPath);

        Assert.IsNotNull(loaded);
        Assert.HasCount(1, loaded.Assets);
        Assert.AreEqual("components/LocalCard.vue", loaded.Assets[0].SourcePath);
        Assert.AreEqual("host/LocalCard.vue", loaded.Assets[0].ArtifactPath);
        Assert.AreEqual(ManifestAssetEntry.KindVueSfc, loaded.Assets[0].Kind);
        Assert.AreEqual("hash-asset-1", loaded.Assets[0].Hash);
    }

    [TestMethod]
    public async Task BundleAsync_StaticManifestAsset_CopiesToOutputRoot()
    {
        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            export const Ready = true;
            """);
        WriteModule(workspace.SourceRoot, "assets/logo.svg",
            """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1 1"></svg>
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Assets.Add(new ManifestAssetEntry(
            "assets/logo.svg",
            "assets/logo.svg",
            ManifestAssetEntry.KindStatic,
            "hash-asset-1"));
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            workspace.SourceRoot));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var outputAssetPath = Path.Combine(workspace.RootPath, "assets", "logo.svg");
        Assert.IsTrue(File.Exists(outputAssetPath), $"Expected static asset output: {outputAssetPath}");
        var asset = await File.ReadAllTextAsync(outputAssetPath, TestContext.CancellationTokenSource.Token);
        Assert.Contains("<svg", asset);
    }

    [TestMethod]
    [TestCategory("Browser")]
    public async Task BundleAsync_ExplicitVueSfcAsset_RunsRegisteredSfcInRealBrowser()
    {
        var browserPath = BrowserSmokeTestHelper.ResolveBrowserExecutable();
        if (browserPath is null)
        {
            Assert.Inconclusive(
                "Real browser SFC smoke requires Microsoft Edge, Chrome, or Chromium. " +
                "Set RAZORVUE_BROWSER_EXE to the browser executable path.");
            return;
        }

        using var workspace = new TestWorkspace();
        WriteModule(workspace.InputDirectory, "host/app.mjs",
            """
            import { createApp, nextTick } from "npm:vue@3";
            import LocalCard from "./LocalCard.vue";

            export async function mount(selector) {
              createApp(LocalCard, { title: "Browser SFC" }).mount(selector);
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
              name: "LocalCard",
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
              color: rgb(10, 20, 30);
            }
            </style>
            """);

        var manifest = new ManifestModel(
            RootAssemblyPath: Path.Combine(workspace.RootPath, "Sample.Host.dll"),
            GeneratedAtUtc: DateTime.UtcNow,
            Modules:
            [
                new ManifestModuleEntry("Sample.Host", "Sample.Host.AppModule", "Sample.Host.AppModule", "host/app.mjs", "hash-1")
            ]);
        manifest.Assets.Add(new ManifestAssetEntry(
            "components/LocalCard.vue",
            "host/LocalCard.vue",
            ManifestAssetEntry.KindVueSfc,
            "hash-asset-1"));
        manifest.Save(workspace.ManifestPath);

        var bundler = new ModuleBundler();
        var result = await bundler.BundleAsync(new BundleOptions(
            workspace.InputDirectory,
            workspace.ManifestPath,
            workspace.OutputPath,
            workspace.SourceRoot));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(workspace.OutputPath), $"Expected browser bundle: {workspace.OutputPath}");
        Assert.IsTrue(File.Exists(workspace.OutputCssPath), $"Expected browser CSS bundle: {workspace.OutputCssPath}");

        WriteBrowserSmokeHarness(workspace.RootPath);
        var browser = await BrowserSmokeTestHelper.RunBrowserDumpDomAsync(browserPath, Path.Combine(workspace.RootPath, "index.html"));
        Assert.AreEqual(0, browser.ExitCode, browser.ToString());

        using var smokePayload = BrowserSmokeTestHelper.ReadBrowserSmokePayload(browser, "SFC");
        var smoke = smokePayload.RootElement;
        Assert.IsTrue(
            smoke.GetProperty("ok").GetBoolean(),
            "Browser SFC smoke failed." + Environment.NewLine + smoke.GetRawText() + Environment.NewLine + browser);
        Assert.AreEqual("Browser SFC", smoke.GetProperty("text").GetString(), smoke.GetRawText());
        Assert.AreEqual("rgb(10, 20, 30)", smoke.GetProperty("color").GetString(), smoke.GetRawText());
    }

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

    private static void WriteBrowserSmokeHarness(string rootPath)
    {
        WriteModule(rootPath, "index.html",
            """
            <!doctype html>
            <html lang="en">
              <head>
                <meta charset="utf-8">
                <title>Jazor Explicit SFC Browser Smoke</title>
                <link rel="stylesheet" href="./bundle.css">
              </head>
              <body>
                <div id="app"></div>
                <script type="module">
                  import { mount } from "./bundle.js";

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
                    await mount("#app");
                    const card = document.querySelector(".local-card");
                    if (!(card instanceof HTMLElement)) {
                      throw new Error("Registered SFC did not render .local-card.");
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

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace()
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            InputDirectory = Path.Combine(RootPath, "modules");
            SourceRoot = Path.Combine(RootPath, "src");
            ManifestPath = Path.Combine(InputDirectory, "jazor-manifest.json");
            OutputPath = Path.Combine(RootPath, "bundle.js");
            OutputMapPath = Path.Combine(RootPath, "bundle.js.map");
            OutputCssPath = Path.Combine(RootPath, "bundle.css");
            Directory.CreateDirectory(InputDirectory);
            Directory.CreateDirectory(SourceRoot);
        }

        public string RootPath { get; }

        public string InputDirectory { get; }

        public string SourceRoot { get; }

        public string ManifestPath { get; }

        public string OutputPath { get; }

        public string OutputMapPath { get; }

        public string OutputCssPath { get; }

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
