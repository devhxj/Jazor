using System.Text;
using System.Text.Json;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class RazorVueSfcBridgeCompilerTests
{
    [TestMethod]
    public async Task CompileAsync_WritesNamedExportBridgeModules_AndRewritesRelativeVueDefaultImports()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            new ManifestModule("Demo.Components.ParentCard", "ParentCard", "components/parent-card.vue"),
            new ManifestModule("Demo.Components.ChildCard", "ChildCard", "components/child-card.vue"));
        workspace.WriteVue(
            "components/child-card.vue",
            """
            <template><span class="child-card">Child</span></template>
            <script setup lang="ts">
            const title: string = "Child";
            </script>
            """);
        workspace.WriteVue(
            "components/parent-card.vue",
            """
            <template><ChildCard /></template>
            <script setup lang="ts">
            import ChildCard from "./child-card.vue";
            </script>
            <style scoped>
            .parent-card { color: red; }
            </style>
            """);

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(result.ResultPath));

        var parentModule = await File.ReadAllTextAsync(
            Path.Combine(workspace.BrowserOutputRoot, "components", "parent-card.mjs"),
            TestContext.CancellationTokenSource.Token);
        var childModule = await File.ReadAllTextAsync(
            Path.Combine(workspace.BrowserOutputRoot, "components", "child-card.mjs"),
            TestContext.CancellationTokenSource.Token);

        Assert.Contains("import \"./parent-card.css\";", parentModule);
        Assert.Contains("import { ChildCard } from \"./child-card.mjs\";", parentModule);
        Assert.Contains("export { _sfc_main as ParentCard };", parentModule);
        Assert.Contains("export { _sfc_main as ChildCard };", childModule);
        Assert.IsFalse(parentModule.Contains("export default", StringComparison.Ordinal), parentModule);
        Assert.IsFalse(parentModule.Contains(".vue", StringComparison.Ordinal), parentModule);
        Assert.IsTrue(File.Exists(Path.Combine(workspace.BrowserOutputRoot, "components", "parent-card.css")));

        using var resultDocument = JsonDocument.Parse(await File.ReadAllTextAsync(result.ResultPath, TestContext.CancellationTokenSource.Token));
        var modules = resultDocument.RootElement.GetProperty("Modules").EnumerateArray().ToArray();
        Assert.HasCount(2, modules);
        Assert.AreEqual("ChildCard", modules[0].GetProperty("ExportName").GetString());
        Assert.AreEqual("components/child-card.mjs", modules[0].GetProperty("RelativeOutputPath").GetString());
        Assert.AreEqual("ParentCard", modules[1].GetProperty("ExportName").GetString());
    }

    [TestMethod]
    public async Task CompileAsync_InSsrMode_WritesCssWithoutRuntimeCssImport()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(new ManifestModule("Demo.Components.ParentCard", "ParentCard", "components/parent-card.vue"));
        workspace.WriteVue(
            "components/parent-card.vue",
            """
            <template><section class="parent-card">Parent</section></template>
            <style scoped>
            .parent-card { color: red; }
            </style>
            """);

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.SsrOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Ssr,
            Production: true,
            Clean: true));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var module = await File.ReadAllTextAsync(
            Path.Combine(workspace.SsrOutputRoot, "components", "parent-card.mjs"),
            TestContext.CancellationTokenSource.Token);
        Assert.IsFalse(module.Contains("import \"./parent-card.css\";", StringComparison.Ordinal), module);
        Assert.Contains("export { _sfc_main as ParentCard };", module);
        Assert.IsTrue(File.Exists(Path.Combine(workspace.SsrOutputRoot, "components", "parent-card.css")));
    }

    [TestMethod]
    public async Task CompileAsync_WithSelectedEntryModulePaths_BridgesOnlySelectedClosure()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            new ManifestModule("Demo.Components.ParentCard", "ParentCard", "components/parent-card.vue"),
            new ManifestModule("Demo.Components.ChildCard", "ChildCard", "components/child-card.vue"),
            new ManifestModule("Demo.Components.BrokenCard", "BrokenCard", "components/broken-card.vue"));
        workspace.WriteVue(
            "components/child-card.vue",
            """
            <template><span class="child-card">Child</span></template>
            """);
        workspace.WriteVue(
            "components/parent-card.vue",
            """
            <template><ChildCard /></template>
            <script setup lang="ts">
            import ChildCard from "./child-card.vue";
            </script>
            """);

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true,
            EntryModulePaths: ["components/parent-card.vue"]));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
        Assert.IsTrue(File.Exists(Path.Combine(workspace.BrowserOutputRoot, "components", "parent-card.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(workspace.BrowserOutputRoot, "components", "child-card.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(workspace.BrowserOutputRoot, "components", "broken-card.mjs")));

        using var resultDocument = JsonDocument.Parse(await File.ReadAllTextAsync(result.ResultPath!, TestContext.CancellationTokenSource.Token));
        var modules = resultDocument.RootElement.GetProperty("Modules").EnumerateArray().ToArray();
        Assert.HasCount(2, modules);
        CollectionAssert.AreEquivalent(
            new[] { "components/child-card.vue", "components/parent-card.vue" },
            modules.Select(static module => module.GetProperty("RelativeModulePath").GetString()).OfType<string>().ToArray());
    }

    [TestMethod]
    public async Task CompileAsync_RewritesRelativeVueDefaultImportsWithoutDroppingNamedImports()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            new ManifestModule("Demo.Components.ParentCard", "ParentCard", "components/parent-card.vue"),
            new ManifestModule("Demo.Components.ChildCard", "ChildCard", "components/child-card.vue"));
        workspace.WriteVue(
            "components/child-card.vue",
            """
            <script lang="ts">
            export const childFlag = true;
            export default {
              name: "ChildCard"
            };
            </script>
            """);
        workspace.WriteVue(
            "components/parent-card.vue",
            """
            <script lang="ts">
            import ChildCard, { childFlag as importedChildFlag } from "./child-card.vue";
            export default {
              components: { ChildCard },
              setup() {
                return { importedChildFlag };
              }
            };
            </script>
            """);

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var parentModule = await File.ReadAllTextAsync(
            Path.Combine(workspace.BrowserOutputRoot, "components", "parent-card.mjs"),
            TestContext.CancellationTokenSource.Token);
        Assert.Contains("import { ChildCard, childFlag as importedChildFlag } from \"./child-card.mjs\";", parentModule);
        Assert.IsFalse(parentModule.Contains("import ChildCard", StringComparison.Ordinal), parentModule);
        Assert.IsFalse(parentModule.Contains(".vue", StringComparison.Ordinal), parentModule);
    }

    [TestMethod]
    public async Task CompileAsync_RewritesRelativeVueDefaultReExportsToNamedBridgeExports()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(
            new ManifestModule("Demo.Components.ParentCard", "ParentCard", "components/parent-card.vue"),
            new ManifestModule("Demo.Components.ChildCard", "ChildCard", "components/child-card.vue"));
        workspace.WriteVue(
            "components/child-card.vue",
            """
            <template><span class="child-card">Child</span></template>
            """);
        workspace.WriteVue(
            "components/parent-card.vue",
            """
            <script lang="ts">
            export { default as ForwardedChildCard } from "./child-card.vue";
            export default {
              name: "ParentCard"
            };
            </script>
            """);

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var parentModule = await File.ReadAllTextAsync(
            Path.Combine(workspace.BrowserOutputRoot, "components", "parent-card.mjs"),
            TestContext.CancellationTokenSource.Token);
        Assert.Contains("export { ChildCard as ForwardedChildCard } from \"./child-card.mjs\";", parentModule);
        Assert.IsFalse(parentModule.Contains("default as ForwardedChildCard", StringComparison.Ordinal), parentModule);
        Assert.IsFalse(parentModule.Contains(".vue", StringComparison.Ordinal), parentModule);
    }

    [TestMethod]
    public async Task CompileAsync_WithSelectedEntryModulePaths_FailsWhenRelativeVueDependencyIsNotInManifest()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(new ManifestModule("Demo.Components.ParentCard", "ParentCard", "components/parent-card.vue"));
        workspace.WriteVue(
            "components/child-card.vue",
            """
            <template><span class="child-card">Child</span></template>
            """);
        workspace.WriteVue(
            "components/parent-card.vue",
            """
            <script setup lang="ts">
            import ChildCard from "./child-card.vue";
            </script>
            """);

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true,
            EntryModulePaths: ["components/parent-card.vue"]));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(9, result.ExitCode);
        StringAssert.Contains(result.Error, "missing relative .vue dependency module(s)");
        StringAssert.Contains(result.Error, "components/child-card.vue");
        Assert.IsFalse(File.Exists(Path.Combine(workspace.BrowserOutputRoot, "components", "parent-card.mjs")));
    }

    [TestMethod]
    public async Task CompileAsync_InvalidComponentName_FailsBeforeWritingBridgeModule()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(new ManifestModule("Demo.Components.Invalid", "bad-name", "components/bad-name.vue"));
        workspace.WriteVue("components/bad-name.vue", "<template><span>Bad</span></template>");

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(9, result.ExitCode);
        StringAssert.Contains(result.Error, "cannot be used as a JavaScript named export");
        Assert.IsFalse(File.Exists(Path.Combine(workspace.BrowserOutputRoot, "components", "bad-name.mjs")));
    }

    [TestMethod]
    public async Task CompileAsync_MissingManifest_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace(writeManifest: false);
        var compiler = new RazorVueSfcBridgeCompiler();

        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(7, result.ExitCode);
        StringAssert.Contains(result.Error, "Jazor manifest was not found");
    }

    [TestMethod]
    public async Task CompileAsync_ManifestWithoutSfcComponents_FailsWithActionableError()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteNonComponentManifest();
        var compiler = new RazorVueSfcBridgeCompiler();

        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(7, result.ExitCode);
        StringAssert.Contains(result.Error, "did not contain any RazorVue SFC component entries");
    }

    [TestMethod]
    public async Task CompileAsync_CleanModeRejectsOutputDirectoryThatWouldDeleteHostSfcSources()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(new ManifestModule("Demo.Components.ParentCard", "ParentCard", "components/parent-card.vue"));
        workspace.WriteVue("components/parent-card.vue", "<template><span>Parent</span></template>");
        var compiler = new RazorVueSfcBridgeCompiler();

        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.HostJazorRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(11, result.ExitCode);
        StringAssert.Contains(result.Error, "cannot be the host output root");
        Assert.IsTrue(File.Exists(Path.Combine(workspace.HostJazorRoot, "components", "parent-card.vue")));
    }

    [TestMethod]
    public async Task CompileAsync_RenderFunctionOnlyVueSfc_WritesNamedExportBridgeModule()
    {
        using var workspace = new TestWorkspace();
        workspace.WriteManifest(new ManifestModule("Demo.Components.CounterCard", "CounterCard", "components/counter-card.vue"));
        workspace.WriteVue(
            "components/counter-card.vue",
            """
            <script lang="ts">
            import { defineComponent, h } from "vue";

            export default defineComponent({
              name: "CounterCard",
              setup() {
                return () => h("section", { class: "counter-card" }, "Counter");
              }
            });
            </script>
            """);

        var compiler = new RazorVueSfcBridgeCompiler();
        var result = await compiler.CompileAsync(new RazorVueSfcBridgeOptions(
            workspace.HostJazorRoot,
            workspace.BrowserOutputRoot,
            workspace.ManifestPath,
            RazorVueSfcBridgeMode.Browser,
            Production: true,
            Clean: true));

        Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);

        var module = await File.ReadAllTextAsync(
            Path.Combine(workspace.BrowserOutputRoot, "components", "counter-card.mjs"),
            TestContext.CancellationTokenSource.Token);
        Assert.Contains("const _sfc_main =", module);
        Assert.Contains("export { _sfc_main as CounterCard };", module);
        Assert.DoesNotContain(module, "render = render", StringComparison.Ordinal);
    }

    private sealed class TestWorkspace : IDisposable
    {
        public TestWorkspace(bool writeManifest = true)
        {
            RootPath = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
            HostJazorRoot = Path.Combine(RootPath, "jazor");
            BrowserOutputRoot = Path.Combine(RootPath, "generated-browser");
            SsrOutputRoot = Path.Combine(RootPath, "generated-ssr");
            ManifestPath = Path.Combine(HostJazorRoot, "jazor-manifest.json");
            Directory.CreateDirectory(HostJazorRoot);

            if (writeManifest)
                WriteManifest();
        }

        public string RootPath { get; }

        public string HostJazorRoot { get; }

        public string BrowserOutputRoot { get; }

        public string SsrOutputRoot { get; }

        public string ManifestPath { get; }

        public void WriteManifest(params ManifestModule[] modules)
        {
            var manifest = new
            {
                rootAssemblyPath = Path.Combine(RootPath, "Demo.Components.dll"),
                generatedAtUtc = DateTime.UtcNow,
                modules = modules.Select(static module => new
                {
                    assemblyName = "Demo.Components",
                    typeName = module.ComponentName,
                    id = module.ComponentId,
                    relativePath = module.RelativeModulePath,
                    hash = "content-hash",
                    sourceMapPath = module.RelativeModulePath + ".map",
                    kind = "vue",
                    component = new
                    {
                        model = "sfc",
                        componentId = module.ComponentId,
                        moduleId = module.RelativeModulePath,
                        componentName = module.ComponentName,
                        originMapPath = module.RelativeModulePath + ".origins.json",
                        imports = Array.Empty<string>(),
                        styles = Array.Empty<string>(),
                        pluginRequirements = Array.Empty<string>(),
                        descriptorHash = "descriptor-hash",
                        templateHash = "template-hash",
                        logicHash = "logic-hash",
                        contentHash = "content-hash",
                        hmrBoundaryKind = RazorVueHmrBoundaryKind.TemplateOnly,
                        requiresHydration = false,
                        supportsSsr = true,
                        styleHash = ""
                    }
                }).ToArray()
            };
            WriteText(ManifestPath, JsonSerializer.Serialize(manifest));
        }

        public void WriteNonComponentManifest()
        {
            var manifest = new
            {
                rootAssemblyPath = Path.Combine(RootPath, "Demo.Components.dll"),
                generatedAtUtc = DateTime.UtcNow,
                modules = new[]
                {
                    new
                    {
                        assemblyName = "Demo.Components",
                        typeName = "Demo.Components.AppModule",
                        id = "Demo.Components.AppModule",
                        relativePath = "host/app.mjs",
                        hash = "content-hash",
                        sourceMapPath = "host/app.mjs.map",
                        kind = "mjs",
                        component = (object?)null
                    }
                }
            };

            WriteText(ManifestPath, JsonSerializer.Serialize(manifest));
        }

        public void WriteVue(string relativePath, string content)
            => WriteText(Path.Combine(HostJazorRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)), content);

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

    private sealed record ManifestModule(
        string ComponentId,
        string ComponentName,
        string RelativeModulePath);

    public TestContext TestContext { get; set; }
}
