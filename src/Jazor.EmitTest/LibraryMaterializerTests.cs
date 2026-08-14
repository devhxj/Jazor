using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class LibraryMaterializerTests
{
    [TestMethod]
    public void Materialize_CopiesPackageAssetsAndSelectsRequestedMode()
    {
        using var workspace = new LibraryWorkspace();
        var manifestPath = workspace.WriteManifest(
            "vue3",
            "3.5.13",
            "vue",
            "dist/dev.mjs",
            "dist/prod.mjs",
            styles: ["dist/main.css"]);
        workspace.WriteFile("dist/dev.mjs", "export const mode = 'development';");
        workspace.WriteFile("dist/prod.mjs", "export const mode = 'production';");
        workspace.WriteFile("dist/main.css", ".app { color: green; }");

        var outputRoot = Path.Combine(workspace.Root, "out");
        var result = new LibraryMaterializer().Materialize([manifestPath], outputRoot, BuildMode.Production);

        Assert.AreEqual("vendor/vue3/3.5.13/dist/prod.mjs", result.ImportPaths["vue"]);
        Assert.AreEqual("vendor/vue3/3.5.13/dist/main.css", result.StylePaths.Single());
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "vendor", "vue3", "3.5.13", "dist", "prod.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "vendor", "vue3", "3.5.13", "dist", "main.css")));
        Assert.IsFalse(File.Exists(Path.Combine(outputRoot, "vendor", "vue3", "3.5.13", "dist", "dev.mjs")));
    }

    [TestMethod]
    public async Task ImportMapWriter_WritesLocalSsrImportsAlongsideBrowserImports()
    {
        using var workspace = new LibraryWorkspace();
        var vueManifest = workspace.WriteLibrary("vue", "vue3", "3.5.13", "vue");
        var rendererManifest = workspace.WriteLibrary(
            "renderer",
            "vue-server-renderer",
            "3.5.13",
            "@vue/server-renderer",
            new Dictionary<string, string> { ["vue3"] = "3.5.13" });
        var outputRoot = Path.Combine(workspace.Root, "out");
        var materialization = new LibraryMaterializer().Materialize(
            [vueManifest, rendererManifest],
            outputRoot,
            BuildMode.Production);

        await ImportMapWriter.WriteAsync(
            outputRoot,
            materialization,
            [new ImportMapEntry("adapter.test", "@adapter/runtime/", "@adapter/runtime/")]);

        using var browserMap = System.Text.Json.JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(outputRoot, ImportMapWriter.BrowserImportMapFileName)));
        using var ssrMap = System.Text.Json.JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(outputRoot, ImportMapWriter.SsrImportMapFileName)));
        Assert.AreEqual(
            "/jazor/vendor/vue3/3.5.13/dist/index.mjs",
            browserMap.RootElement.GetProperty("imports").GetProperty("vue").GetString());
        Assert.AreEqual(
            "./vendor/vue3/3.5.13/dist/index.mjs",
            ssrMap.RootElement.GetProperty("imports").GetProperty("vue").GetString());
        Assert.AreEqual(
            "./vendor/vue-server-renderer/3.5.13/dist/index.mjs",
            ssrMap.RootElement.GetProperty("imports").GetProperty("@vue/server-renderer").GetString());
        Assert.AreEqual(
            "/jazor/@adapter/runtime/",
            browserMap.RootElement.GetProperty("imports").GetProperty("@adapter/runtime/").GetString());
        Assert.AreEqual(
            "./@adapter/runtime/",
            ssrMap.RootElement.GetProperty("imports").GetProperty("@adapter/runtime/").GetString());
        Assert.IsFalse(
            (await File.ReadAllTextAsync(Path.Combine(outputRoot, ImportMapWriter.SsrImportMapFileName)))
                .Contains("node_modules", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Materialize_RejectsDifferentProvidersForSameLogicalImport()
    {
        using var workspace = new LibraryWorkspace();
        var firstManifest = workspace.WriteManifest("first", "1.0.0", "vue", "dist/dev.mjs", "dist/prod.mjs");
        workspace.WriteFile("dist/dev.mjs", "export const first = true;");
        workspace.WriteFile("dist/prod.mjs", "export const first = true;");

        var secondRoot = Path.Combine(workspace.Root, "second");
        Directory.CreateDirectory(Path.Combine(secondRoot, "dist"));
        File.WriteAllText(Path.Combine(secondRoot, "dist", "dev.mjs"), "export const second = true;");
        File.WriteAllText(Path.Combine(secondRoot, "dist", "prod.mjs"), "export const second = true;");
        var secondManifest = Path.Combine(secondRoot, "manifest.json");
        File.WriteAllText(secondManifest, """
            {
              "schemaVersion": 1,
              "libraryId": "second",
              "version": "1.0.0",
              "imports": { "vue": { "development": "dist/dev.mjs", "production": "dist/prod.mjs" } }
            }
            """);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new LibraryMaterializer().Materialize([firstManifest, secondManifest], Path.Combine(workspace.Root, "out"), BuildMode.Production));

        StringAssert.Contains(exception.Message, "Library import 'vue'", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Materialize_RejectsUnsatisfiedProviderVersion()
    {
        using var workspace = new LibraryWorkspace();
        var vueManifest = workspace.WriteManifest("vue3", "3.4.0", "vue", "dist/vue.mjs", "dist/vue.mjs");
        workspace.WriteFile("dist/vue.mjs", "export const version = '3.4.0';");

        var componentRoot = Path.Combine(workspace.Root, "component");
        Directory.CreateDirectory(Path.Combine(componentRoot, "dist"));
        File.WriteAllText(Path.Combine(componentRoot, "dist", "component.mjs"), "export const ready = true;");
        var componentManifest = Path.Combine(componentRoot, "manifest.json");
        File.WriteAllText(componentManifest, """
            {
              "schemaVersion": 1,
              "libraryId": "component",
              "version": "1.0.0",
              "imports": { "component": { "development": "dist/component.mjs", "production": "dist/component.mjs" } },
              "requires": { "vue3": "^3.5.0" }
            }
            """);

        var exception = Assert.Throws<LibraryException>(() =>
            new LibraryMaterializer().Materialize(
                [vueManifest, componentManifest],
                Path.Combine(workspace.Root, "out"),
                BuildMode.Production));

        Assert.AreEqual("JAZOR_LIBRARY_VERSION_MISMATCH", exception.Code);
    }

    [TestMethod]
    public void Materialize_OrdersStylesByDependencyThenManifestOrder()
    {
        using var workspace = new LibraryWorkspace();
        var consumer = workspace.WriteLibrary(
            "consumer",
            "a-ui",
            "1.0.0",
            "a-ui",
            new Dictionary<string, string> { ["z-core"] = "1.0.0" },
            "dist/ui.css");
        var provider = workspace.WriteLibrary(
            "provider",
            "z-core",
            "1.0.0",
            "z-core",
            style: "dist/core.css");

        var result = new LibraryMaterializer().Materialize(
            [consumer, provider],
            Path.Combine(workspace.Root, "out"),
            BuildMode.Production);

        CollectionAssert.AreEqual(
            new[]
            {
                "vendor/z-core/1.0.0/dist/core.css",
                "vendor/a-ui/1.0.0/dist/ui.css"
            },
            result.StylePaths.ToArray());
    }

    [TestMethod]
    public void Materialize_RejectsDuplicateLibraryProvider()
    {
        using var workspace = new LibraryWorkspace();
        var first = workspace.WriteLibrary("first", "vue3", "3.5.13", "vue");
        var second = workspace.WriteLibrary("second", "vue3", "3.5.13", "vue");

        var exception = Assert.Throws<LibraryException>(() =>
            new LibraryMaterializer().Materialize(
                [first, second],
                Path.Combine(workspace.Root, "out"),
                BuildMode.Production));

        Assert.AreEqual("JAZOR_LIBRARY_PROVIDER_DUPLICATE", exception.Code);
    }

    [TestMethod]
    public void Materialize_RejectsMissingRequiredImport()
    {
        using var workspace = new LibraryWorkspace();
        var manifestPath = workspace.WriteManifest("vue3", "3.5.13", "vue", "dist/vue.mjs", "dist/vue.mjs");
        workspace.WriteFile("dist/vue.mjs", "export const version = '3.5.13';");

        var exception = Assert.Throws<LibraryException>(() =>
            new LibraryMaterializer().Materialize(
                [manifestPath],
                Path.Combine(workspace.Root, "out"),
                BuildMode.Production,
                ["vue", "vuetify/components"]));

        Assert.AreEqual("JAZOR_LIBRARY_IMPORT_MISSING", exception.Code);
        StringAssert.Contains(exception.Message, "vuetify/components", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Materialize_IgnoresImportsProvidedByApplicationManifest()
    {
        using var workspace = new LibraryWorkspace();
        var manifestPath = workspace.WriteManifest("pinia", "3.0.4", "pinia", "dist/pinia.mjs", "dist/pinia.mjs");
        workspace.WriteFile("dist/pinia.mjs", "export const version = '3.0.4';");

        var result = new LibraryMaterializer().Materialize(
            [manifestPath],
            Path.Combine(workspace.Root, "out"),
            BuildMode.Production,
            ["pinia", "host/app.mjs", "stores/counter-store.mjs"],
            ["host/app.mjs", "stores/counter-store.mjs"]);

        Assert.AreEqual("vendor/pinia/3.0.4/dist/pinia.mjs", result.ImportPaths["pinia"]);
    }

    [TestMethod]
    public void Materialize_ProductionVueEntry_OmitsUnusedSsrAndDevtoolsAssets()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [FindLibraryManifest("ECMAScript.Vue")],
            outputRoot,
            BuildMode.Production,
            ["vue"]);

        Assert.HasCount(1, result.ImportPaths);
        Assert.IsTrue(result.ImportPaths.ContainsKey("vue"));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "vue.runtime.esm-browser.prod.js")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "server-renderer.esm-browser.prod.js")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "index.js")));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "licenses",
            "LICENSE")));
    }

    [TestMethod]
    public void Materialize_ProductionPiniaEntry_FollowsDeclaredRuntimeClosure()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.Pinia"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["pinia"]);

        CollectionAssert.AreEquivalent(
            new[] { "pinia", "vue", "@vue/devtools-api" },
            result.ImportPaths.Keys.ToArray());
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "index.js")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "server-renderer.esm-browser.prod.js")));
    }

    [TestMethod]
    public void Materialize_ProductionVuetifyLabsEntry_CopiesRelativeModuleClosureOnly()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.Vuetify"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["vuetify/labs/components"]);

        CollectionAssert.AreEquivalent(
            new[] { "vue", "vuetify", "vuetify/labs/components" },
            result.ImportPaths.Keys.ToArray());
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vuetify",
            "4.1.8",
            "dist",
            "vuetify-labs.esm.js")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vuetify",
            "4.1.8",
            "dist",
            "components.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vuetify",
            "4.1.8",
            "dist",
            "directives.mjs")));
    }

    private static string FindLibraryManifest(string projectName)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "src", projectName, "manifest.json");
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Could not locate the {projectName} library manifest.");
    }

    private sealed class LibraryWorkspace : IDisposable
    {
        public LibraryWorkspace()
        {
            Root = Path.Combine(Path.GetTempPath(), "jazor-library-assets", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public string WriteManifest(
            string libraryId,
            string version,
            string import,
            string development,
            string production,
            IReadOnlyList<string>? styles = null)
        {
            var stylesJson = styles is null
                ? "[]"
                : "[" + string.Join(",", styles.Select(static item => $"\"{item}\"")) + "]";
            var manifestPath = Path.Combine(Root, "manifest.json");
            File.WriteAllText(manifestPath, $$"""
                {
                  "schemaVersion": 1,
                  "libraryId": "{{libraryId}}",
                  "version": "{{version}}",
                  "imports": {
                    "{{import}}": {
                      "development": "{{development}}",
                      "production": "{{production}}"
                    }
                  },
                  "styles": {{stylesJson}}
                }
                """);
            return manifestPath;
        }

        public void WriteFile(string relativePath, string content)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public string WriteLibrary(
            string folder,
            string libraryId,
            string version,
            string import,
            IReadOnlyDictionary<string, string>? requires = null,
            string? style = null)
        {
            var root = Path.Combine(Root, folder);
            var modulePath = Path.Combine(root, "dist", "index.mjs");
            Directory.CreateDirectory(Path.GetDirectoryName(modulePath)!);
            File.WriteAllText(modulePath, $"export const id = '{libraryId}';");
            if (style is not null)
            {
                var stylePath = Path.Combine(root, style.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(stylePath)!);
                File.WriteAllText(stylePath, $".{libraryId} {{ display: block; }}");
            }

            var manifestPath = Path.Combine(root, "manifest.json");
            File.WriteAllText(
                manifestPath,
                System.Text.Json.JsonSerializer.Serialize(new
                {
                    schemaVersion = 1,
                    libraryId,
                    version,
                    imports = new Dictionary<string, object>
                    {
                        [import] = new { development = "dist/index.mjs", production = "dist/index.mjs" }
                    },
                    requires = requires ?? new Dictionary<string, string>(),
                    styles = style is null ? Array.Empty<string>() : new[] { style }
                }));
            return manifestPath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
