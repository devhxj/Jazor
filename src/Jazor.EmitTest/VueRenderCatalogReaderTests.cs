using System.Reflection;
using System.Text;
using Jazor.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.EmitTest;

[TestClass]
public sealed class VueRenderCatalogReaderTests
{
    [TestMethod]
    public void CatalogReader_TryRead_ReadsVueRenderCatalog()
    {
        var assembly = CompileCatalogAssembly(
            "VueRenderCatalog.Reader.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class VueRenderCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const int RuntimeProtocolVersion = 1;

                    internal static System.Collections.IEnumerable GetModules()
                    {
                        return _modules;
                    }

                    private static readonly GeneratedVueRenderModule[] _modules = new[]
                    {
                        new GeneratedVueRenderModule(
                            componentId: "Demo.Pages.Counter",
                            relativePath: "components/counter.mjs",
                            moduleText: "export default {};",
                            contentHash: "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")
                    };

                    private sealed class GeneratedVueRenderModule
                    {
                        public GeneratedVueRenderModule(string componentId, string relativePath, string moduleText, string contentHash)
                        {
                            ComponentId = componentId;
                            RelativePath = relativePath;
                            ModuleText = moduleText;
                            ContentHash = contentHash;
                        }

                        public string ComponentId { get; }
                        public string RelativePath { get; }
                        public string ModuleText { get; }
                        public string ContentHash { get; }
                    }
                }
            }
            """);

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        Assert.AreEqual(1, modules.Count);
        var module = modules[0];
        Assert.AreEqual("VueRenderCatalog.Reader.Tests", module.AssemblyName);
        Assert.AreEqual("Demo.Pages.Counter", module.TypeName);
        Assert.AreEqual("Demo.Pages.Counter", module.Id);
        Assert.AreEqual("components/counter.mjs", module.RelativePath);
        Assert.AreEqual("export default {};", module.Content);
        Assert.AreEqual("sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", module.Hash);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ReadsVueRenderCatalogSourceMapPayload()
    {
        var assembly = CompileCatalogAssembly(
            "VueRenderCatalog.SourceMap.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class VueRenderCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const int RuntimeProtocolVersion = 1;

                    internal static System.Collections.IEnumerable GetModules()
                    {
                        return _modules;
                    }

                    private static readonly GeneratedVueRenderModule[] _modules = new[]
                    {
                        new GeneratedVueRenderModule(
                            componentId: "Demo.Pages.Counter",
                            relativePath: "components/counter.mjs",
                            moduleText: "export default {};",
                            contentHash: "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                            sourceMapRelativePath: "components/counter.mjs.map",
                            sourceMapContent: "{\"version\":3,\"file\":\"components/counter.mjs\",\"sources\":[\"Counter.razor\"],\"names\":[],\"mappings\":\"AAAA\"}",
                            mapHash: "sha256:abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789")
                    };

                    private sealed class GeneratedVueRenderModule
                    {
                        public GeneratedVueRenderModule(
                            string componentId,
                            string relativePath,
                            string moduleText,
                            string contentHash,
                            string sourceMapRelativePath,
                            string sourceMapContent,
                            string mapHash)
                        {
                            ComponentId = componentId;
                            RelativePath = relativePath;
                            ModuleText = moduleText;
                            ContentHash = contentHash;
                            SourceMapRelativePath = sourceMapRelativePath;
                            SourceMapContent = sourceMapContent;
                            MapHash = mapHash;
                        }

                        public string ComponentId { get; }
                        public string RelativePath { get; }
                        public string ModuleText { get; }
                        public string ContentHash { get; }
                        public string SourceMapRelativePath { get; }
                        public string SourceMapContent { get; }
                        public string MapHash { get; }
                    }
                }
            }
            """);

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        var module = modules.Single(static item => item.Id == "Demo.Pages.Counter");
        Assert.AreEqual("components/counter.mjs.map", module.SourceMapRelativePath);
        StringAssert.Contains(module.SourceMapContent, "\"sources\":[\"Counter.razor\"]");
        Assert.AreEqual("sha256:abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789", module.MapHash);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ReadsVueRenderCatalogFrontendAssets()
    {
        var assembly = CompileCatalogAssembly(
            "VueRenderCatalog.Assets.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class VueRenderCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const int RuntimeProtocolVersion = 1;

                    internal static System.Collections.IEnumerable GetModules()
                    {
                        return _modules;
                    }

                    internal static System.Collections.IEnumerable GetAssets()
                    {
                        return _assets;
                    }

                    private static readonly GeneratedVueRenderModule[] _modules = new[]
                    {
                        new GeneratedVueRenderModule(
                            componentId: "Demo.Pages.Counter",
                            relativePath: "components/counter.mjs",
                            moduleText: "import LocalCard from \"./LocalCard.vue.mjs\";\nexport default {};",
                            contentHash: "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")
                    };

                    private static readonly GeneratedVueRenderAsset[] _assets = new[]
                    {
                        new GeneratedVueRenderAsset(
                            sourcePath: "components/LocalCard.vue",
                            artifactPath: "components/LocalCard.vue",
                            kind: "vue-sfc",
                            contentHash: "")
                    };

                    private sealed class GeneratedVueRenderModule
                    {
                        public GeneratedVueRenderModule(string componentId, string relativePath, string moduleText, string contentHash)
                        {
                            ComponentId = componentId;
                            RelativePath = relativePath;
                            ModuleText = moduleText;
                            ContentHash = contentHash;
                        }

                        public string ComponentId { get; }
                        public string RelativePath { get; }
                        public string ModuleText { get; }
                        public string ContentHash { get; }
                    }

                    private sealed class GeneratedVueRenderAsset
                    {
                        public GeneratedVueRenderAsset(string sourcePath, string artifactPath, string kind, string contentHash)
                        {
                            SourcePath = sourcePath;
                            ArtifactPath = artifactPath;
                            Kind = kind;
                            ContentHash = contentHash;
                        }

                        public string SourcePath { get; }
                        public string ArtifactPath { get; }
                        public string Kind { get; }
                        public string ContentHash { get; }
                    }
                }
            }
            """);

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        var asset = modules.Single().FrontendAssets?.Single();
        Assert.IsNotNull(asset);
        Assert.AreEqual("components/LocalCard.vue", asset.SourcePath);
        Assert.AreEqual("components/LocalCard.vue", asset.ArtifactPath);
        Assert.AreEqual(ManifestAssetEntry.KindVueSfc, asset.Kind);
        Assert.AreEqual(string.Empty, asset.Hash);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ReadsRazorVueRuntimeEmbeddedResources()
    {
        var assembly = CompileCatalogAssembly(
            "Jazor.RazorVue.Runtime.Resource.Tests",
            """
            namespace Jazor.RazorVue
            {
                internal static class Marker
                {
                }
            }
            """,
            new ResourceDescription(
                "Jazor.RazorVue.Runtime.render-context.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export function createRenderContext() {}\n")),
                isPublic: true),
            new ResourceDescription(
                "Jazor.RazorVue.Runtime.render-context-core.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export const RENDER_CONTEXT_PROTOCOL_VERSION = 1;\n")),
                isPublic: true));

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        Assert.HasCount(2, modules);

        var runtimeModule = modules.Single(static item => item.RelativePath == "@jazor/vue-runtime/render-context.mjs");
        Assert.AreEqual("Jazor.RazorVue.Runtime.Resource.Tests", runtimeModule.AssemblyName);
        Assert.AreEqual("Jazor.RazorVue.Runtime.render-context", runtimeModule.TypeName);
        Assert.AreEqual("Jazor.RazorVue.Runtime.render-context", runtimeModule.Id);
        StringAssert.Contains(runtimeModule.Content, "createRenderContext");
        StringAssert.StartsWith(runtimeModule.Hash, "sha256:");

        var coreModule = modules.Single(static item => item.RelativePath == "@jazor/vue-runtime/render-context-core.mjs");
        StringAssert.Contains(coreModule.Content, "RENDER_CONTEXT_PROTOCOL_VERSION");
        StringAssert.StartsWith(coreModule.Hash, "sha256:");
    }

    [TestMethod]
    public void ModuleWriter_Write_MaterializesVueRenderCatalogWithSourceMapAndRuntimeAssets()
    {
        var catalogAssembly = CompileCatalogAssembly(
            "VueRenderCatalog.Materialize.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class VueRenderCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const int RuntimeProtocolVersion = 1;

                    internal static System.Collections.IEnumerable GetModules()
                    {
                        return _modules;
                    }

                    private static readonly GeneratedVueRenderModule[] _modules = new[]
                    {
                        new GeneratedVueRenderModule(
                            componentId: "Demo.Pages.Counter",
                            relativePath: "components/counter.mjs",
                            moduleText: "import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";\nexport default {};",
                            contentHash: "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                            sourceMapRelativePath: "components/counter.mjs.map",
                            sourceMapContent: "{\"version\":3,\"file\":\"components/counter.mjs\",\"sources\":[\"Pages/Counter.razor\"],\"names\":[],\"mappings\":\"AAAA\"}",
                            mapHash: "sha256:abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789")
                    };

                    private sealed class GeneratedVueRenderModule
                    {
                        public GeneratedVueRenderModule(
                            string componentId,
                            string relativePath,
                            string moduleText,
                            string contentHash,
                            string sourceMapRelativePath,
                            string sourceMapContent,
                            string mapHash)
                        {
                            ComponentId = componentId;
                            RelativePath = relativePath;
                            ModuleText = moduleText;
                            ContentHash = contentHash;
                            SourceMapRelativePath = sourceMapRelativePath;
                            SourceMapContent = sourceMapContent;
                            MapHash = mapHash;
                        }

                        public string ComponentId { get; }
                        public string RelativePath { get; }
                        public string ModuleText { get; }
                        public string ContentHash { get; }
                        public string SourceMapRelativePath { get; }
                        public string SourceMapContent { get; }
                        public string MapHash { get; }
                    }
                }
            }
            """);
        var runtimeAssembly = CompileCatalogAssembly(
            "Jazor.RazorVue.Runtime.Materialize.Tests",
            """
            namespace Jazor.RazorVue
            {
                internal static class Marker
                {
                }
            }
            """,
            new ResourceDescription(
                "Jazor.RazorVue.Runtime.render-context.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export function createRenderContext() {}\n")),
                isPublic: true),
            new ResourceDescription(
                "Jazor.RazorVue.Runtime.render-context-core.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export const RENDER_CONTEXT_PROTOCOL_VERSION = 1;\n")),
                isPublic: true));
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "VueRenderCatalog.Materialize.Tests.dll");

        try
        {
            var modules = CatalogReader.TryRead(catalogAssembly)!
                .Concat(CatalogReader.TryRead(runtimeAssembly)!)
                .ToArray();
            var result = new ModuleWriter().Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                modules,
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter.mjs")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter.mjs.map")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "@jazor", "vue-runtime", "render-context.mjs")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "@jazor", "vue-runtime", "render-context-core.mjs")));

            var moduleText = File.ReadAllText(Path.Combine(outputDirectory, "components", "counter.mjs"));
            StringAssert.Contains(moduleText, "@jazor/vue-runtime/render-context.mjs");
            StringAssert.Contains(moduleText, "sourceMappingURL=counter.mjs.map");

            var mapText = File.ReadAllText(Path.Combine(outputDirectory, "components", "counter.mjs.map"));
            StringAssert.Contains(mapText, "Pages/Counter.razor");

            var manifestText = File.ReadAllText(manifestPath).ReplaceLineEndings("\n");
            StringAssert.Contains(manifestText, "\"schemaVersion\": 1");
            StringAssert.Contains(manifestText, "\"path\": \"components/counter.mjs\"");
            StringAssert.Contains(manifestText, "\"sourceMap\": \"components/counter.mjs.map\"");
            StringAssert.Contains(manifestText, "\"path\": \"@jazor/vue-runtime/render-context.mjs\"");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_VueRenderCatalogRepeatWriteIsByteForByteStable()
    {
        var catalogAssembly = CompileCatalogAssembly(
            "VueRenderCatalog.RepeatStable.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class VueRenderCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const int RuntimeProtocolVersion = 1;

                    internal static System.Collections.IEnumerable GetModules()
                    {
                        return _modules;
                    }

                    private static readonly GeneratedVueRenderModule[] _modules = new[]
                    {
                        new GeneratedVueRenderModule(
                            componentId: "Demo.Pages.Counter",
                            relativePath: "components/counter.mjs",
                            moduleText: "import { createRenderContext } from \"@jazor/vue-runtime/render-context.mjs\";\nexport default {};",
                            contentHash: "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                            sourceMapRelativePath: "components/counter.mjs.map",
                            sourceMapContent: "{\"version\":3,\"file\":\"components/counter.mjs\",\"sources\":[\"Pages/Counter.razor\"],\"names\":[],\"mappings\":\"AAAA\"}",
                            mapHash: "sha256:abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789")
                    };

                    private sealed class GeneratedVueRenderModule
                    {
                        public GeneratedVueRenderModule(
                            string componentId,
                            string relativePath,
                            string moduleText,
                            string contentHash,
                            string sourceMapRelativePath,
                            string sourceMapContent,
                            string mapHash)
                        {
                            ComponentId = componentId;
                            RelativePath = relativePath;
                            ModuleText = moduleText;
                            ContentHash = contentHash;
                            SourceMapRelativePath = sourceMapRelativePath;
                            SourceMapContent = sourceMapContent;
                            MapHash = mapHash;
                        }

                        public string ComponentId { get; }
                        public string RelativePath { get; }
                        public string ModuleText { get; }
                        public string ContentHash { get; }
                        public string SourceMapRelativePath { get; }
                        public string SourceMapContent { get; }
                        public string MapHash { get; }
                    }
                }
            }
            """);
        var runtimeAssembly = CompileCatalogAssembly(
            "Jazor.RazorVue.Runtime.RepeatStable.Tests",
            """
            namespace Jazor.RazorVue
            {
                internal static class Marker
                {
                }
            }
            """,
            new ResourceDescription(
                "Jazor.RazorVue.Runtime.render-context.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export function createRenderContext() {}\n")),
                isPublic: true),
            new ResourceDescription(
                "Jazor.RazorVue.Runtime.render-context-core.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export const RENDER_CONTEXT_PROTOCOL_VERSION = 1;\n")),
                isPublic: true));
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "VueRenderCatalog.RepeatStable.Tests.dll");
        var componentPath = Path.Combine(outputDirectory, "components", "counter.mjs");
        var componentMapPath = Path.Combine(outputDirectory, "components", "counter.mjs.map");
        var runtimePath = Path.Combine(outputDirectory, "@jazor", "vue-runtime", "render-context.mjs");
        var runtimeCorePath = Path.Combine(outputDirectory, "@jazor", "vue-runtime", "render-context-core.mjs");

        try
        {
            var modules = CatalogReader.TryRead(catalogAssembly)!
                .Concat(CatalogReader.TryRead(runtimeAssembly)!)
                .ToArray();
            var writer = new ModuleWriter();

            var first = writer.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                modules,
                clean: true);

            Assert.IsTrue(first.IsSuccess, first.Error ?? string.Empty);
            Assert.AreEqual(3, first.Written);
            Assert.AreEqual(0, first.Skipped);

            var firstComponent = File.ReadAllBytes(componentPath);
            var firstMap = File.ReadAllBytes(componentMapPath);
            var firstRuntime = File.ReadAllBytes(runtimePath);
            var firstRuntimeCore = File.ReadAllBytes(runtimeCorePath);
            var firstManifest = File.ReadAllBytes(manifestPath);

            var second = writer.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                modules,
                clean: true);

            Assert.IsTrue(second.IsSuccess, second.Error ?? string.Empty);
            Assert.AreEqual(0, second.Written);
            Assert.AreEqual(3, second.Skipped);
            Assert.AreEqual(0, second.Deleted);
            CollectionAssert.AreEqual(firstComponent, File.ReadAllBytes(componentPath), "Component module changed between identical writes.");
            CollectionAssert.AreEqual(firstMap, File.ReadAllBytes(componentMapPath), "Component source map changed between identical writes.");
            CollectionAssert.AreEqual(firstRuntime, File.ReadAllBytes(runtimePath), "Runtime module changed between identical writes.");
            CollectionAssert.AreEqual(firstRuntimeCore, File.ReadAllBytes(runtimeCorePath), "Runtime core module changed between identical writes.");
            CollectionAssert.AreEqual(firstManifest, File.ReadAllBytes(manifestPath), "Manifest changed between identical writes.");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_MaterializesVueRenderCatalogFrontendAssetsIntoManifest()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "VueRenderCatalog.AssetManifest.Tests.dll");
        var module = new EmitModuleRecord(
            SourceAssemblyPath: rootAssemblyPath,
            AssemblyName: "VueRenderCatalog.AssetManifest.Tests",
            TypeName: "Demo.Pages.Counter",
            Id: "Demo.Pages.Counter",
            RelativePath: "components/counter.mjs",
            Content: "import LocalCard from \"./LocalCard.vue.mjs\";\nexport default {};\n",
            Hash: "sha256:counter",
            FrontendAssets:
            [
                new ManifestAssetEntry(
                    "components/LocalCard.vue",
                    "components/LocalCard.vue",
                    ManifestAssetEntry.KindVueSfc,
                    string.Empty)
            ]);

        try
        {
            var result = new ModuleWriter().Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                [module],
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Assets);
            Assert.AreEqual("components/LocalCard.vue", manifest.Assets[0].SourcePath);
            Assert.AreEqual("components/LocalCard.vue", manifest.Assets[0].ArtifactPath);
            Assert.AreEqual(ManifestAssetEntry.KindVueSfc, manifest.Assets[0].Kind);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_WhenVueRenderComponentRemoved_DeletesStaleComponentAndSourceMap()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "VueRenderCatalog.Stale.Tests.dll");
        var counterPath = Path.Combine(outputDirectory, "components", "counter.mjs");
        var counterMapPath = Path.Combine(outputDirectory, "components", "counter.mjs.map");
        var todoPath = Path.Combine(outputDirectory, "components", "todo.mjs");
        var todoMapPath = Path.Combine(outputDirectory, "components", "todo.mjs.map");

        var counter = new EmitModuleRecord(
            SourceAssemblyPath: rootAssemblyPath,
            AssemblyName: "VueRenderCatalog.Stale.Tests",
            TypeName: "Demo.Pages.Counter",
            Id: "Demo.Pages.Counter",
            RelativePath: "components/counter.mjs",
            Content: "export default { name: \"Counter\" };\n",
            Hash: "sha256:counter",
            SourceMapRelativePath: "components/counter.mjs.map",
            SourceMapContent: "{\"version\":3,\"file\":\"components/counter.mjs\",\"sources\":[\"Pages/Counter.razor\"],\"names\":[],\"mappings\":\"AAAA\"}",
            MapHash: "sha256:counter-map");
        var todo = new EmitModuleRecord(
            SourceAssemblyPath: rootAssemblyPath,
            AssemblyName: "VueRenderCatalog.Stale.Tests",
            TypeName: "Demo.Pages.Todo",
            Id: "Demo.Pages.Todo",
            RelativePath: "components/todo.mjs",
            Content: "export default { name: \"Todo\" };\n",
            Hash: "sha256:todo",
            SourceMapRelativePath: "components/todo.mjs.map",
            SourceMapContent: "{\"version\":3,\"file\":\"components/todo.mjs\",\"sources\":[\"Pages/Todo.razor\"],\"names\":[],\"mappings\":\"AAAA\"}",
            MapHash: "sha256:todo-map");

        try
        {
            var writer = new ModuleWriter();
            var first = writer.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                [counter, todo],
                clean: true);

            Assert.IsTrue(first.IsSuccess, first.Error ?? string.Empty);
            Assert.IsTrue(File.Exists(counterPath));
            Assert.IsTrue(File.Exists(counterMapPath));
            Assert.IsTrue(File.Exists(todoPath));
            Assert.IsTrue(File.Exists(todoMapPath));

            var second = writer.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                [todo],
                clean: true);

            Assert.IsTrue(second.IsSuccess, second.Error ?? string.Empty);
            Assert.AreEqual(2, second.Deleted);
            Assert.IsFalse(File.Exists(counterPath), "Removed Vue render component module should be deleted.");
            Assert.IsFalse(File.Exists(counterMapPath), "Removed Vue render component source map should be deleted.");
            Assert.IsTrue(File.Exists(todoPath));
            Assert.IsTrue(File.Exists(todoMapPath));

            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("components/todo.mjs", manifest.Modules[0].RelativePath);
            Assert.AreEqual("components/todo.mjs.map", manifest.Modules[0].SourceMapPath);
            Assert.IsFalse(
                File.ReadAllText(manifestPath).Contains("components/counter.mjs", StringComparison.Ordinal),
                "Manifest must stop declaring removed Vue render components.");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void CatalogReader_TryRead_RejectsUnsupportedVueRenderCatalogSchema()
    {
        var assembly = CompileCatalogAssembly(
            "VueRenderCatalog.BadSchema.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class VueRenderCatalog
                {
                    internal const int SchemaVersion = 99;
                    internal const int RuntimeProtocolVersion = 1;

                    internal static System.Collections.IEnumerable GetModules()
                    {
                        return System.Array.Empty<object>();
                    }
                }
            }
            """);

        var exception = Assert.Throws<InvalidOperationException>(() => CatalogReader.TryRead(assembly));
        StringAssert.Contains(exception.Message, "VueRenderCatalog schema version '99'");
    }

    [TestMethod]
    public void CatalogReader_TryRead_RejectsUnsupportedVueRenderRuntimeProtocol()
    {
        var assembly = CompileCatalogAssembly(
            "VueRenderCatalog.BadRuntime.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class VueRenderCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const int RuntimeProtocolVersion = 99;

                    internal static System.Collections.IEnumerable GetModules()
                    {
                        return System.Array.Empty<object>();
                    }
                }
            }
            """);

        var exception = Assert.Throws<InvalidOperationException>(() => CatalogReader.TryRead(assembly));
        StringAssert.Contains(exception.Message, "VueRenderCatalog runtime protocol version '99'");
    }

    private static Assembly CompileCatalogAssembly(
        string assemblyName,
        string source,
        params ResourceDescription[] resources)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: assemblyName + ".g.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: [syntaxTree],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Collections.IEnumerable).Assembly.Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        var result = compilation.Emit(stream, manifestResources: resources);
        Assert.IsTrue(
            result.Success,
            string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.ToString())));

        stream.Position = 0;
        return Assembly.Load(stream.ToArray());
    }
}
