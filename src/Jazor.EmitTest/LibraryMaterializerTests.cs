using System.Text.Json;
using System.Text.Json.Nodes;
using System.Security.Cryptography;
using System.Text;
using Acornima;
using Acornima.Ast;
using Jazor.Common;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class LibraryMaterializerTests
{
    [TestMethod]
    public void Load_AllRepositoryResourceManifests_AreValidAndComplete()
    {
        var manifests = Directory
            .EnumerateFiles(Path.Combine(FindRepositoryRoot(), "src"), "manifest.json", SearchOption.AllDirectories)
            .Where(static path => !path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase))
            .Where(static path => !path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.IsNotEmpty(manifests, "The repository must contain at least one JS resource manifest.");
        foreach (var manifestPath in manifests)
        {
            var packageRoot = Path.GetDirectoryName(manifestPath)!;
            Assert.IsTrue(
                Directory.Exists(Path.Combine(packageRoot, "dist")),
                $"JS resource package '{manifestPath}' must contain a dist directory.");

            // Load validates schema, typed records, explicit module edges, file paths and every
            // declared SHA-256 before the manifest can reach Emit's materialization path.
            _ = LibraryManifest.Load(manifestPath);
        }
    }

    [TestMethod]
    public void Materialize_CopiesPackageAssetsAndSelectsRequestedMode()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/dev.mjs", "export const mode = 'development';");
        workspace.WriteFile("dist/prod.mjs", "export const mode = 'production';");
        workspace.WriteFile("dist/main.css", ".app { color: green; }");
        var manifestPath = workspace.WriteManifest(
            "vue3",
            "3.5.13",
            "vue",
            "dist/dev.mjs",
            "dist/prod.mjs",
            styles: ["dist/main.css"]);

        var outputRoot = Path.Combine(workspace.Root, "out");
        var result = new LibraryMaterializer().Materialize([manifestPath], outputRoot, BuildMode.Production);

        Assert.AreEqual("vendor/vue3/3.5.13/dist/prod.mjs", result.ImportPaths["vue"]);
        Assert.AreEqual("vendor/vue3/3.5.13/dist/main.css", result.StylePaths.Single());
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "vendor", "vue3", "3.5.13", "dist", "prod.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "vendor", "vue3", "3.5.13", "dist", "main.css")));
        Assert.IsFalse(File.Exists(Path.Combine(outputRoot, "vendor", "vue3", "3.5.13", "dist", "dev.mjs")));
    }

    [TestMethod]
    public void Load_ValidatesProductionEntryBeforeMaterialization()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/dev.mjs", "export const mode = 'development';");
        workspace.WriteFile("dist/prod.mjs", "export const mode = 'production';");
        var manifestPath = workspace.WriteManifest(
            "profile-library",
            "1.0.0",
            "profile-library",
            "dist/dev.mjs",
            "dist/prod.mjs");

        File.Delete(Path.Combine(workspace.Root, "dist", "prod.mjs"));

        var exception = Assert.Throws<LibraryException>(() => LibraryManifest.Load(manifestPath));

        Assert.AreEqual("JAZOR_LIBRARY_FILE_MISSING", exception.Code);
        StringAssert.Contains(exception.Message, "dist/prod.mjs", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Load_ValidatesProductionHashBeforeMaterialization()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/dev.mjs", "export const mode = 'development';");
        workspace.WriteFile("dist/prod.mjs", "export const mode = 'production';");
        var manifestPath = workspace.WriteManifest(
            "profile-library",
            "1.0.0",
            "profile-library",
            "dist/dev.mjs",
            "dist/prod.mjs");

        workspace.WriteFile("dist/prod.mjs", "export const mode = 'tampered';");

        var exception = Assert.Throws<LibraryException>(() => LibraryManifest.Load(manifestPath));

        Assert.AreEqual("JAZOR_LIBRARY_FILE_HASH_MISMATCH", exception.Code);
        StringAssert.Contains(exception.Message, "dist/prod.mjs", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ManifestModel_RejectsConflictingDuplicateModulePath()
    {
        var firstHash = ArtifactHash.ComputeSha256("export const first = true;");
        var secondHash = ArtifactHash.ComputeSha256("export const second = true;");

        var exception = Assert.Throws<InvalidOperationException>(() => new ManifestModel(
            "Sample.Host.dll",
            [
                new ModuleEntry("Sample.Host", "First", "first", "components/app.mjs", firstHash),
                new ModuleEntry("Sample.Host", "Second", "second", "components/app.mjs", secondHash)
            ]));

        StringAssert.Contains(exception.Message, "conflicting modules", StringComparison.Ordinal);
    }

    [TestMethod]
    public void ManifestModel_RejectsConflictingDuplicateAssetPath()
    {
        var firstHash = ArtifactHash.ComputeSha256("first");
        var secondHash = ArtifactHash.ComputeSha256("second");
        var manifest = new ManifestModel(
            "Sample.Host.dll",
            [new ModuleEntry(
                "Sample.Host",
                "App",
                "app",
                "components/app.mjs",
                ArtifactHash.ComputeSha256("export const app = true;"))]);

        manifest.Assets.Add(new AssetEntry("assets/first.txt", "assets/shared.txt", AssetEntry.KindStatic, firstHash));

        var outputRoot = Path.Combine(Path.GetTempPath(), "jazor-invalid-manifest", Guid.NewGuid().ToString("N"));
        try
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                manifest.Assets.Add(new AssetEntry("assets/second.txt", "assets/shared.txt", AssetEntry.KindStatic, secondHash));
                manifest.Save(Path.Combine(outputRoot, "manifest.json"));
            });

            StringAssert.Contains(exception.Message, "conflicting assets", StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(outputRoot))
                Directory.Delete(outputRoot, recursive: true);
        }
    }

    [TestMethod]
    public void Materialize_FollowsRelativeEsmClosure_WithoutCopyingUnreferencedChunks()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/components/widget.mjs", """
            import { helper } from "../shared/helper.mjs";
            export { value } from "../shared/reexport.mjs";
            export const result = helper;
            export async function load() { return import("../async/lazy.mjs"); }
            """);
        workspace.WriteFile("dist/shared/helper.mjs", """
            import { cycle } from "../cycle.mjs";
            export const helper = cycle;
            """);
        workspace.WriteFile("dist/cycle.mjs", """
            import { helper } from "./shared/helper.mjs";
            export const cycle = helper ? 1 : 0;
            """);
        workspace.WriteFile("dist/shared/reexport.mjs", "export const value = 'ready';");
        workspace.WriteFile("dist/async/lazy.mjs", "export const lazy = true;");
        workspace.WriteFile("dist/unused.mjs", "export const unused = true;");
        var manifestPath = workspace.WriteManifest(
            "component-library",
            "1.0.0",
            "component-library/widget",
            "dist/components/widget.mjs",
            "dist/components/widget.mjs",
            moduleDependencies:
            [
                "dist/shared/helper.mjs",
                "dist/shared/reexport.mjs",
                "dist/cycle.mjs",
                "dist/async/lazy.mjs"
            ]);

        var outputRoot = Path.Combine(workspace.Root, "out");
        _ = new LibraryMaterializer().Materialize(
            [manifestPath],
            outputRoot,
            BuildMode.Production,
            ["component-library/widget"]);

        var materializedRoot = Path.Combine(outputRoot, "vendor", "component-library", "1.0.0", "dist");
        Assert.IsTrue(File.Exists(Path.Combine(materializedRoot, "components", "widget.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(materializedRoot, "shared", "helper.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(materializedRoot, "shared", "reexport.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(materializedRoot, "cycle.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(materializedRoot, "async", "lazy.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(materializedRoot, "unused.mjs")));
    }

    [TestMethod]
    public void Materialize_ResolvesModuleDependencyOnlyWithinDeclaringManifest()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/app.mjs", "export const app = true;");
        workspace.WriteFile("dist/shared.mjs", "export const owner = true;");
        var ownerManifest = workspace.WriteManifest(
            "owner",
            "1.0.0",
            "owner",
            "dist/app.mjs",
            "dist/app.mjs",
            moduleDependencies: ["dist/shared.mjs"]);
        UpdateEntryFile(ownerManifest, "owner", "dist/shared.mjs", file => file["moduleId"] = "shared");
        UpdateEntryModuleDependencies(ownerManifest, "owner", "shared");

        // An unrelated manifest deliberately exposes the same logical name. A module edge must
        // still resolve to the declaring manifest's module record, never to this provider.
        var unrelatedManifest = workspace.WriteLibrary(
            "unrelated",
            "unrelated",
            "1.0.0",
            "shared");

        var outputRoot = Path.Combine(workspace.Root, "out");
        var result = new LibraryMaterializer().Materialize(
            [ownerManifest, unrelatedManifest],
            outputRoot,
            BuildMode.Production,
            ["owner"]);

        Assert.AreEqual("vendor/owner/1.0.0/dist/app.mjs", result.ImportPaths["owner"]);
        Assert.IsFalse(result.ImportPaths.ContainsKey("shared"));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "owner",
            "1.0.0",
            "dist",
            "shared.mjs")));
        Assert.IsFalse(Directory.Exists(Path.Combine(outputRoot, "vendor", "unrelated")));
    }

    [TestMethod]
    public async Task Materialize_RecursivelyMapsNamedModuleDependencies()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        // The CLR resource manifest has a real nested ESM chain:
        // IndexModule -> RuntimeModule -> StringModule. Every named edge must be exposed in
        // the import map, while unrelated modules remain unmaterialized.
        var result = new LibraryMaterializer().Materialize(
            [FindLibraryManifest("ECMAScript")],
            outputRoot,
            BuildMode.Production,
            ["System/IndexModule.js"]);

        CollectionAssert.AreEquivalent(
            new[]
            {
                "System/IndexModule.js",
                "System/RuntimeModule.js",
                "System/StringModule.js",
                "System/Collections/Generic/EqualityComparerT1Module.js",
                "System/Collections/Generic/HashSetT1Module.js",
                "System/Collections/Generic/IEqualityComparerT1Module.js"
            },
            result.ImportPaths.Keys.ToArray());
        Assert.IsFalse(result.ImportPaths.ContainsKey("System/ArrayModule.js"));

        await ImportMapWriter.WriteAsync(outputRoot, result);
        using var importMap = JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(outputRoot, ImportMapWriter.BrowserImportMapFileName)));
        var imports = importMap.RootElement.GetProperty("imports");
        foreach (var specifier in result.ImportPaths.Keys)
        {
            var target = imports.GetProperty(specifier).GetString();
            Assert.IsTrue(target?.StartsWith("/jazor/vendor/ecmascript/", StringComparison.Ordinal));
        }
    }

    [TestMethod]
    public void Materialize_RejectsMissingLocalModuleDependencyEvenWhenAnotherManifestProvidesItsName()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/app.mjs", "export const app = true;");
        var ownerManifest = workspace.WriteManifest(
            "owner",
            "1.0.0",
            "owner",
            "dist/app.mjs",
            "dist/app.mjs");
        UpdateEntryModuleDependencies(ownerManifest, "owner", "shared");
        var unrelatedManifest = workspace.WriteLibrary(
            "unrelated",
            "unrelated",
            "1.0.0",
            "shared");

        var exception = Assert.Throws<LibraryException>(() =>
            new LibraryMaterializer().Materialize(
                [ownerManifest, unrelatedManifest],
                Path.Combine(workspace.Root, "out"),
                BuildMode.Production,
                ["owner"]));

        Assert.AreEqual("JAZOR_LIBRARY_MODULE_DEPENDENCY_MISSING", exception.Code);
        StringAssert.Contains(exception.Message, "shared", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Materialize_RejectsStaticFileAsModuleDependency()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/index.mjs", "import './dependency.mjs';");
        workspace.WriteFile("dist/dependency.mjs", "export const value = 1;");
        var manifestPath = workspace.WriteManifest(
            "invalid-module-library",
            "1.0.0",
            "invalid-module-library",
            "dist/index.mjs",
            "dist/index.mjs",
            moduleDependencies: ["dist/dependency.mjs"]);
        UpdateEntryFile(manifestPath, "invalid-module-library", "dist/dependency.mjs", file =>
        {
            file["type"] = "static";
            file.Remove("moduleId");
        });

        var exception = Assert.Throws<LibraryException>(() =>
            new LibraryMaterializer().Materialize(
                [manifestPath],
                Path.Combine(workspace.Root, "out"),
                BuildMode.Production));

        Assert.AreEqual("JAZOR_LIBRARY_MODULE_DEPENDENCY_MISSING", exception.Code);
        StringAssert.Contains(exception.Message, "dist/dependency.mjs", StringComparison.Ordinal);
    }

    [TestMethod]
    [DataRow("module")]
    [DataRow("source-map")]
    public void Materialize_RejectsModuleAssociatedFileWithoutModuleId(string type)
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/index.mjs", "import './dependency.mjs';");
        workspace.WriteFile("dist/dependency.mjs", "export const value = 1;");
        var manifestPath = workspace.WriteManifest(
            "missing-module-id-library",
            "1.0.0",
            "missing-module-id-library",
            "dist/index.mjs",
            "dist/index.mjs",
            moduleDependencies: ["dist/dependency.mjs"]);
        UpdateEntryFile(manifestPath, "missing-module-id-library", "dist/dependency.mjs", file =>
        {
            file["type"] = type;
            file.Remove("moduleId");
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new LibraryMaterializer().Materialize(
                [manifestPath],
                Path.Combine(workspace.Root, "out"),
                BuildMode.Production));

        StringAssert.Contains(exception.Message, "must declare moduleId", StringComparison.Ordinal);
    }

    [TestMethod]
    public void Materialize_RejectsSourceMapForUnknownModule()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/index.mjs", "export const value = 1;");
        workspace.WriteFile("dist/index.mjs.map", "{}");
        var manifestPath = workspace.WriteManifest(
            "orphan-map-library",
            "1.0.0",
            "orphan-map-library",
            "dist/index.mjs",
            "dist/index.mjs",
            moduleDependencies: ["dist/index.mjs.map"]);
        UpdateEntryFile(manifestPath, "orphan-map-library", "dist/index.mjs.map", file =>
        {
            file["type"] = "source-map";
            file["moduleId"] = "missing/module";
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new LibraryMaterializer().Materialize(
                [manifestPath],
                Path.Combine(workspace.Root, "out"),
                BuildMode.Production));

        StringAssert.Contains(exception.Message, "references missing module id 'missing/module'", StringComparison.Ordinal);
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
            materialization);

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
        Assert.IsFalse(
            (await File.ReadAllTextAsync(Path.Combine(outputRoot, ImportMapWriter.SsrImportMapFileName)))
                .Contains("node_modules", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Materialize_RejectsDifferentProvidersForSameLogicalImport()
    {
        using var workspace = new LibraryWorkspace();
        workspace.WriteFile("dist/dev.mjs", "export const first = true;");
        workspace.WriteFile("dist/prod.mjs", "export const first = true;");
        var firstManifest = workspace.WriteManifest("first", "1.0.0", "vue", "dist/dev.mjs", "dist/prod.mjs");

        var secondRoot = Path.Combine(workspace.Root, "second");
        Directory.CreateDirectory(Path.Combine(secondRoot, "dist"));
        File.WriteAllText(Path.Combine(secondRoot, "dist", "dev.mjs"), "export const second = true;");
        File.WriteAllText(Path.Combine(secondRoot, "dist", "prod.mjs"), "export const second = true;");
        var secondManifest = Path.Combine(secondRoot, "manifest.json");
        File.WriteAllText(secondManifest, """
            {
              "schemaVersion": 2,
              "libraryId": "second",
              "version": "1.0.0",
              "imports": { "vue": {
                "type": "module",
                "development": "dist/dev.mjs", "production": "dist/prod.mjs",
                "developmentHash": "8fae6a0c6a49529f37de5867ce360d0a8af6f46b7673fbe5fd810fdaebc9f020",
                "productionHash": "8fae6a0c6a49529f37de5867ce360d0a8af6f46b7673fbe5fd810fdaebc9f020",
                "developmentDependencies": [], "productionDependencies": [],
                "developmentModuleDependencies": [], "productionModuleDependencies": [], "files": []
              } },
              "requires": {}, "styles": [], "files": []
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
        workspace.WriteFile("dist/vue.mjs", "export const version = '3.4.0';");
        var vueManifest = workspace.WriteManifest("vue3", "3.4.0", "vue", "dist/vue.mjs", "dist/vue.mjs");

        var componentRoot = Path.Combine(workspace.Root, "component");
        Directory.CreateDirectory(Path.Combine(componentRoot, "dist"));
        File.WriteAllText(Path.Combine(componentRoot, "dist", "component.mjs"), "export const ready = true;");
        var componentManifest = Path.Combine(componentRoot, "manifest.json");
        File.WriteAllText(componentManifest, """
            {
              "schemaVersion": 2,
              "libraryId": "component",
              "version": "1.0.0",
              "imports": { "component": {
                "type": "module",
                "development": "dist/component.mjs", "production": "dist/component.mjs",
                "developmentHash": "6e022b4c49ef4368c407c653dda43e5b44565cfa549b47150e3f8f4427199ac7",
                "productionHash": "6e022b4c49ef4368c407c653dda43e5b44565cfa549b47150e3f8f4427199ac7",
                "developmentDependencies": [], "productionDependencies": [],
                "developmentModuleDependencies": [], "productionModuleDependencies": [], "files": []
              } },
              "requires": { "vue3": "^3.5.0" }, "styles": [], "files": []
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
        workspace.WriteFile("dist/vue.mjs", "export const version = '3.5.13';");
        var manifestPath = workspace.WriteManifest("vue3", "3.5.13", "vue", "dist/vue.mjs", "dist/vue.mjs");

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
        workspace.WriteFile("dist/pinia.mjs", "export const version = '3.0.4';");
        var manifestPath = workspace.WriteManifest("pinia", "3.0.4", "pinia", "dist/pinia.mjs", "dist/pinia.mjs");

        var result = new LibraryMaterializer().Materialize(
            [manifestPath],
            Path.Combine(workspace.Root, "out"),
            BuildMode.Production,
            ["pinia", "host/app.mjs", "stores/counter-store.mjs"],
            ["host/app.mjs", "stores/counter-store.mjs"]);

        Assert.AreEqual("vendor/pinia/3.0.4/dist/pinia.mjs", result.ImportPaths["pinia"]);
    }

    [TestMethod]
    public void Materialize_IgnoresInvalidManifestOutsideSelectedClosure()
    {
        using var workspace = new LibraryWorkspace();
        var selectedManifest = workspace.WriteLibrary(
            "selected",
            "selected-library",
            "1.0.0",
            "selected");
        var unrelatedManifest = workspace.WriteLibrary(
            "unrelated",
            "unrelated-library",
            "1.0.0",
            "unrelated");

        // The unrelated package is present in the transitive locator set, but its bytes and
        // provider graph are intentionally broken. Since no selected root reaches it, this must
        // not prevent the selected package from being materialized.
        File.Delete(Path.Combine(workspace.Root, "unrelated", "dist", "index.mjs"));
        var unrelatedRoot = JsonNode.Parse(File.ReadAllText(unrelatedManifest))?.AsObject()
            ?? throw new InvalidOperationException("Unrelated manifest is not an object.");
        unrelatedRoot["requires"] = new JsonObject
        {
            ["missing-provider"] = "1.0.0"
        };
        File.WriteAllText(
            unrelatedManifest,
            unrelatedRoot.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        var outputRoot = Path.Combine(workspace.Root, "out");
        var result = new LibraryMaterializer().Materialize(
            [selectedManifest, unrelatedManifest],
            outputRoot,
            BuildMode.Production,
            ["selected"]);

        Assert.AreEqual("vendor/selected-library/1.0.0/dist/index.mjs", result.ImportPaths["selected"]);
        Assert.HasCount(1, result.ManifestPaths);
        Assert.AreEqual(
            Path.GetFullPath(selectedManifest),
            result.ManifestPaths.Single(),
            ignoreCase: true);
        Assert.IsFalse(Directory.Exists(Path.Combine(outputRoot, "vendor", "unrelated-library")));
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
            "vue-devtools-api.esm-browser.js")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "perfect-debounce.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "licenses",
            "LICENSE")));
    }

    [TestMethod]
    public void Materialize_DevtoolsApiEntry_UsesVueOwnedRuntimeClosure()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [FindLibraryManifest("ECMAScript.Vue")],
            outputRoot,
            BuildMode.Production,
            ["@vue/devtools-api"]);

        CollectionAssert.AreEquivalent(
            new[] { "@vue/devtools-api", "perfect-debounce" },
            result.ImportPaths.Keys.ToArray());
        Assert.AreEqual(
            "vendor/vue3/3.5.13/dist/devtools-api/vue-devtools-api.esm-browser.js",
            result.ImportPaths["@vue/devtools-api"]);
        Assert.AreEqual(
            "vendor/vue3/3.5.13/dist/devtools-api/perfect-debounce.mjs",
            result.ImportPaths["perfect-debounce"]);
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "vue-devtools-api.esm-browser.js")));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "perfect-debounce.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "ECMAScript.Vue.Devtools",
            "manifest.json")),
            "The optional C# binding must reuse the Vue-owned @vue/devtools-api provider instead of registering a duplicate runtime manifest.");
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
            new[] { "pinia", "vue" },
            result.ImportPaths.Keys.ToArray());
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "pinia",
            "4.0.3",
            "dist",
            "pinia.esm-browser.prod.js")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "pinia",
            "4.0.3",
            "dist",
            "nostics",
            "index.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "vue-devtools-api.esm-browser.js")));
    }

    [TestMethod]
    public void Materialize_DevelopmentPiniaEntry_FollowsDeclaredDiagnosticsAndDevtoolsClosure()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.Pinia"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Development,
            ["pinia"]);

        CollectionAssert.AreEquivalent(
            new[] { "pinia", "vue", "nostics", "@vue/devtools-api", "perfect-debounce" },
            result.ImportPaths.Keys.ToArray());
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "pinia",
            "4.0.3",
            "dist",
            "pinia.esm-browser.js")));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "pinia",
            "4.0.3",
            "dist",
            "nostics",
            "index.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "vue-devtools-api.esm-browser.js")));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "vue3",
            "3.5.13",
            "dist",
            "devtools-api",
            "perfect-debounce.mjs")));
    }

    [TestMethod]
    public void Materialize_ProductionPiniaTestingEntry_FollowsDeclaredRuntimeClosure()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.Pinia.Testing"),
                FindLibraryManifest("ECMAScript.Pinia"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["@pinia/testing"]);

        CollectionAssert.AreEquivalent(
            new[] { "@pinia/testing", "pinia", "vue", "nostics" },
            result.ImportPaths.Keys.ToArray());
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "pinia-testing",
            "2.0.1",
            "dist",
            "index.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(
            outputRoot,
            "vendor",
            "pinia",
            "4.0.3",
            "dist",
            "nostics",
            "index.mjs")));
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

    [TestMethod]
    public void Materialize_ProductionVueDataUiDonut_CopiesSelectedChartClosureAndPdfImportOnly()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.VueDataUi"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["vue-data-ui/vue-ui-donut"]);

        CollectionAssert.AreEquivalent(
            new[] { "vue", "jspdf", "vue-data-ui/vue-ui-donut" },
            result.ImportPaths.Keys.ToArray());
        CollectionAssert.Contains(
            result.StylePaths.ToArray(),
            "vendor/vue-data-ui/3.23.4/dist/style.css");

        var dataUiRoot = Path.Combine(outputRoot, "vendor", "vue-data-ui", "3.23.4", "dist");
        Assert.IsTrue(File.Exists(Path.Combine(dataUiRoot, "components", "vue-ui-donut.js")));
        Assert.IsTrue(Directory.GetFiles(dataUiRoot, "vue-ui-donut-*.js", SearchOption.TopDirectoryOnly).Length > 0);
        Assert.IsTrue(File.Exists(Path.Combine(dataUiRoot, "jspdf.browser.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(dataUiRoot, "components", "vue-ui-xy.js")));
    }

    [TestMethod]
    public async Task Materialize_ProductionVueDataUiTable_ProvidesImportMapForCompleteBrowserClosure()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var materialization = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.VueDataUi"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["vue-data-ui/vue-ui-table"]);

        CollectionAssert.AreEquivalent(
            new[] { "vue", "jspdf", "vue-data-ui/vue-ui-table" },
            materialization.ImportPaths.Keys.ToArray());
        await ImportMapWriter.WriteAsync(outputRoot, materialization);

        using var browserMap = JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(outputRoot, ImportMapWriter.BrowserImportMapFileName)));
        var providers = browserMap.RootElement
            .GetProperty("imports")
            .EnumerateObject()
            .Select(static entry => entry.Name)
            .ToArray();
        var dataUiRoot = Path.Combine(outputRoot, "vendor", "vue-data-ui", "3.23.4", "dist");
        var unresolved = Directory.EnumerateFiles(dataUiRoot, "*", SearchOption.AllDirectories)
            .Where(IsJavaScriptModule)
            .SelectMany(GetBareModuleSpecifiers)
            .Where(specifier => !providers.Any(provider => ProvidesImport(provider, specifier)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static specifier => specifier, StringComparer.Ordinal)
            .ToArray();

        Assert.IsEmpty(unresolved, "Browser import map does not provide: " + string.Join(", ", unresolved));
        Assert.IsTrue(File.Exists(Path.Combine(dataUiRoot, "jspdf.browser.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(dataUiRoot, "jspdf.browser.mjs.LEGAL.txt")));

        var noticesRoot = Path.Combine(outputRoot, "vendor", "vue-data-ui", "3.23.4", "licenses");
        var expectedNotices = new[]
        {
            "JSPDF-LICENSE",
            "jspdf-browser-bundle/README.md",
            "jspdf-browser-bundle/babel-runtime-LICENSE",
            "jspdf-browser-bundle/canvg-LICENSE",
            "jspdf-browser-bundle/core-js-LICENSE",
            "jspdf-browser-bundle/dompurify-LICENSE",
            "jspdf-browser-bundle/fast-png-LICENSE",
            "jspdf-browser-bundle/fflate-LICENSE",
            "jspdf-browser-bundle/html2canvas-LICENSE",
            "jspdf-browser-bundle/iobuffer-LICENSE",
            "jspdf-browser-bundle/pako-LICENSE",
            "jspdf-browser-bundle/performance-now-license.txt",
            "jspdf-browser-bundle/raf-LICENSE",
            "jspdf-browser-bundle/rgbcolor-LICENSE.md",
            "jspdf-browser-bundle/rgbcolor-FEEL-FREE.md",
            "jspdf-browser-bundle/stackblur-canvas-LICENSE-MIT.txt",
            "jspdf-browser-bundle/svg-pathdata-LICENSE"
        };
        foreach (var notice in expectedNotices)
            Assert.IsTrue(File.Exists(Path.Combine(noticesRoot, notice)), $"Missing bundled dependency notice: {notice}");
    }

    [TestMethod]
    public void Materialize_ProductionVueDataUiFlow_CopiesNewCatalogEntryAndPdfImportOnly()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.VueDataUi"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["vue-data-ui/vue-ui-flow"]);

        CollectionAssert.AreEquivalent(
            new[] { "vue", "jspdf", "vue-data-ui/vue-ui-flow" },
            result.ImportPaths.Keys.ToArray());

        var dataUiRoot = Path.Combine(outputRoot, "vendor", "vue-data-ui", "3.23.4", "dist");
        Assert.IsTrue(File.Exists(Path.Combine(dataUiRoot, "components", "vue-ui-flow.js")));
        Assert.IsTrue(Directory.GetFiles(dataUiRoot, "vue-ui-flow-*.js", SearchOption.TopDirectoryOnly).Length > 0);
        Assert.IsTrue(File.Exists(Path.Combine(dataUiRoot, "jspdf.browser.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(dataUiRoot, "components", "vue-ui-donut.js")));
    }

    [TestMethod]
    public void Materialize_ProductionVuIconsStaticEntry_CopiesOnlyTheSelectedIconClosure()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.VuIcons"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["vu-icons/VuUser"]);

        CollectionAssert.AreEquivalent(
            new[] { "vue", "vu-icons/VuUser" },
            result.ImportPaths.Keys.ToArray());
        CollectionAssert.Contains(result.StylePaths.ToArray(), "vendor/vu-icons/1.5.4/dist/jazor-vu-icon.css");

        var iconsRoot = Path.Combine(outputRoot, "vendor", "vu-icons", "1.5.4", "dist");
        Assert.IsTrue(File.Exists(Path.Combine(iconsRoot, "components", "VuUser.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(iconsRoot, "jazor-vu-icon-runtime.mjs")));
        Assert.IsFalse(File.Exists(Path.Combine(iconsRoot, "icons-data.js")));
        Assert.IsFalse(File.Exists(Path.Combine(iconsRoot, "components", "VuSearch.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(outputRoot, "vendor", "vu-icons", "1.5.4", "licenses", "VU-ICONS-LICENSE")));
    }

    [TestMethod]
    public void Materialize_ProductionVuIconsDynamicEntry_CopiesTheRuntimeIconCatalog()
    {
        using var workspace = new LibraryWorkspace();
        var outputRoot = Path.Combine(workspace.Root, "out");

        var result = new LibraryMaterializer().Materialize(
            [
                FindLibraryManifest("ECMAScript.VuIcons"),
                FindLibraryManifest("ECMAScript.Vue")
            ],
            outputRoot,
            BuildMode.Production,
            ["vu-icons"]);

        CollectionAssert.AreEquivalent(
            new[] { "vue", "vu-icons" },
            result.ImportPaths.Keys.ToArray());

        var iconsRoot = Path.Combine(outputRoot, "vendor", "vu-icons", "1.5.4", "dist");
        Assert.IsTrue(File.Exists(Path.Combine(iconsRoot, "jazor-vu-icon.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(iconsRoot, "jazor-vu-icon-runtime.mjs")));
        Assert.IsTrue(File.Exists(Path.Combine(iconsRoot, "icons-data.js")));
        Assert.IsFalse(Directory.Exists(Path.Combine(iconsRoot, "components")));
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Jazor.slnx")))
                return directory.FullName;
        }

        throw new FileNotFoundException("Could not locate the Jazor repository root.");
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

    private static IEnumerable<string> GetBareModuleSpecifiers(string modulePath)
    {
        var module = new Parser().ParseModule(File.ReadAllText(modulePath));
        var collector = new BareModuleSpecifierCollector();
        collector.Visit(module);
        return collector.Specifiers;
    }

    private static bool IsJavaScriptModule(string path)
        => Path.GetExtension(path) is ".js" or ".mjs" or ".cjs" or ".jsx";

    private static bool ProvidesImport(string provider, string specifier)
        => string.Equals(provider, specifier, StringComparison.Ordinal) ||
           (provider.EndsWith('/', StringComparison.Ordinal) &&
            specifier.StartsWith(provider, StringComparison.Ordinal));

    private sealed class BareModuleSpecifierCollector : AstVisitor
    {
        public HashSet<string> Specifiers { get; } = new(StringComparer.Ordinal);

        protected override object VisitImportDeclaration(ImportDeclaration node)
        {
            Add(node.Source);
            base.VisitImportDeclaration(node);
            return node;
        }

        protected override object VisitExportNamedDeclaration(ExportNamedDeclaration node)
        {
            if (node.Source is not null)
                Add(node.Source);
            base.VisitExportNamedDeclaration(node);
            return node;
        }

        protected override object VisitExportAllDeclaration(ExportAllDeclaration node)
        {
            Add(node.Source);
            base.VisitExportAllDeclaration(node);
            return node;
        }

        protected override object VisitImportExpression(ImportExpression node)
        {
            if (node.Source is StringLiteral source)
                Add(source);
            base.VisitImportExpression(node);
            return node;
        }

        private void Add(StringLiteral source)
        {
            var specifier = source.Value;
            if (specifier.StartsWith('.', StringComparison.Ordinal) ||
                specifier.StartsWith('/', StringComparison.Ordinal) ||
                specifier.StartsWith('#', StringComparison.Ordinal) ||
                Uri.TryCreate(specifier, UriKind.Absolute, out _))
            {
                return;
            }

            Specifiers.Add(specifier);
        }
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
            IReadOnlyList<string>? styles = null,
            IReadOnlyList<string>? moduleDependencies = null)
        {
            var manifestPath = Path.Combine(Root, "manifest.json");
            var moduleFiles = (moduleDependencies ?? [])
                .Select(path => (object)new
                {
                    type = "module",
                    path,
                    hash = HashFile(path),
                    moduleId = path
                })
                .ToArray();
            var styleFiles = (styles ?? [])
                .Select(path => (object)new
                {
                    type = "style",
                    path,
                    hash = HashFile(path)
                })
                .ToArray();
            var manifest = new
            {
                schemaVersion = 2,
                libraryId,
                version,
                imports = new Dictionary<string, object>
                {
                    [import] = new
                    {
                        type = "module",
                        development,
                        production,
                        developmentHash = HashFile(development),
                        productionHash = HashFile(production),
                        developmentDependencies = Array.Empty<string>(),
                        productionDependencies = Array.Empty<string>(),
                        developmentModuleDependencies = moduleDependencies ?? [],
                        productionModuleDependencies = moduleDependencies ?? [],
                        files = moduleFiles
                    }
                },
                requires = new Dictionary<string, string>(),
                styles = styleFiles,
                files = Array.Empty<object>()
            };
            File.WriteAllText(
                manifestPath,
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
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
                    schemaVersion = 2,
                    libraryId,
                    version,
                    imports = new Dictionary<string, object>
                    {
                        [import] = new
                        {
                            type = "module",
                            development = "dist/index.mjs",
                            production = "dist/index.mjs",
                            developmentHash = HashFile(Path.Combine(root, "dist", "index.mjs")),
                            productionHash = HashFile(Path.Combine(root, "dist", "index.mjs")),
                            developmentDependencies = Array.Empty<string>(),
                            productionDependencies = Array.Empty<string>(),
                            developmentModuleDependencies = Array.Empty<string>(),
                            productionModuleDependencies = Array.Empty<string>(),
                            files = Array.Empty<object>()
                        }
                    },
                    requires = requires ?? new Dictionary<string, string>(),
                    styles = style is null
                        ? Array.Empty<object>()
                        : new object[]
                        {
                            new
                            {
                                type = "style",
                                path = style,
                                hash = HashFile(Path.Combine(root, style.Replace('/', Path.DirectorySeparatorChar)))
                            }
                        },
                    files = Array.Empty<object>()
                }));
            return manifestPath;
        }

        private string HashFile(string relativePath)
        {
            var path = Path.IsPathRooted(relativePath)
                ? relativePath
                : Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
                throw new FileNotFoundException($"Test fixture file '{relativePath}' was not found.", path);
            return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }

    private static void UpdateEntryFile(
        string manifestPath,
        string importSpecifier,
        string path,
        Action<JsonObject> update)
    {
        var root = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject()
            ?? throw new InvalidOperationException("Test manifest is not a JSON object.");
        var files = root["imports"]?[importSpecifier]?["files"]?.AsArray()
            ?? throw new InvalidOperationException("Test manifest entry does not contain files.");
        var file = files
            .Select(static item => item?.AsObject())
            .Single(item => string.Equals(item?["path"]?.GetValue<string>(), path, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Test manifest file '{path}' was not found.");
        update(file);
        File.WriteAllText(
            manifestPath,
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void UpdateEntryModuleDependencies(
        string manifestPath,
        string importSpecifier,
        params string[] dependencies)
    {
        var root = JsonNode.Parse(File.ReadAllText(manifestPath))?.AsObject()
            ?? throw new InvalidOperationException("Test manifest is not a JSON object.");
        var entry = root["imports"]?[importSpecifier]?.AsObject()
            ?? throw new InvalidOperationException("Test manifest entry does not exist.");
        var values = new JsonArray(dependencies.Select(static value => JsonValue.Create(value)).ToArray());
        entry["developmentModuleDependencies"] = values.DeepClone();
        entry["productionModuleDependencies"] = values;
        File.WriteAllText(
            manifestPath,
            root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
}
