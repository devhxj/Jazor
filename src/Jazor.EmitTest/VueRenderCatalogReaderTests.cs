using System.Reflection;
using System.Text;
using System.Text.Json;
using Jazor.Emit;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ArtifactCatalogReaderTests
{
    [TestMethod]
    public void CatalogReader_TryRead_ReadsNeutralArtifactCatalog()
    {
        var assembly = CompileCatalogAssembly(
            "ArtifactCatalog.Reader.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ArtifactCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const string ProducerId = "adapter.test";

                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly ArtifactModule[] _modules =
                    [
                        new ArtifactModule(
                            "Demo.Pages.Counter",
                            "Demo.Pages.Counter",
                            "components/counter.mjs",
                            "export default {};",
                            "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")
                    ];

                    private sealed class ArtifactModule(string id, string typeName, string relativePath, string content, string hash)
                    {
                        public string Id { get; } = id;
                        public string TypeName { get; } = typeName;
                        public string RelativePath { get; } = relativePath;
                        public string Content { get; } = content;
                        public string Hash { get; } = hash;
                    }
                }
            }
            """);

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        Assert.AreEqual(1, modules.Count);
        var module = modules[0];
        Assert.AreEqual("ArtifactCatalog.Reader.Tests", module.AssemblyName);
        Assert.AreEqual("Demo.Pages.Counter", module.TypeName);
        Assert.AreEqual("Demo.Pages.Counter", module.Id);
        Assert.AreEqual("components/counter.mjs", module.RelativePath);
        Assert.AreEqual("export default {};", module.Content);
        Assert.AreEqual("sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", module.Hash);
    }

    [TestMethod]
    public void CatalogReader_TryRead_PreservesOpaqueProviderHmrPayload()
    {
        var assembly = CompileCatalogAssembly(
            "ArtifactCatalog.Hmr.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ArtifactCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const string ProducerId = "jazor.vue";

                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly ArtifactModule[] _modules =
                    [
                        new ArtifactModule(
                            "Demo.Pages.Counter",
                            "Demo.Pages.Counter",
                            "components/counter.mjs",
                            "export default {};",
                            "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                            "jazor.vue",
                            "ArtifactCatalog.Hmr.Tests:components/counter.mjs",
                            "{\"componentId\":\"Demo.Pages.Counter\",\"boundaryKind\":\"template-only\"}")
                    ];

                    private sealed class ArtifactModule(
                        string id,
                        string typeName,
                        string relativePath,
                        string content,
                        string hash,
                        string hmrProviderId,
                        string hmrModuleId,
                        string hmrPayload)
                    {
                        public string Id { get; } = id;
                        public string TypeName { get; } = typeName;
                        public string RelativePath { get; } = relativePath;
                        public string Content { get; } = content;
                        public string Hash { get; } = hash;
                        public string HmrProviderId { get; } = hmrProviderId;
                        public string HmrModuleId { get; } = hmrModuleId;
                        public string HmrPayload { get; } = hmrPayload;
                    }
                }
            }
            """);

        var module = CatalogReader.TryRead(assembly)!.Single();

        Assert.IsNotNull(module.Hmr);
        Assert.AreEqual("jazor.vue", module.Hmr.ProviderId);
        Assert.AreEqual("ArtifactCatalog.Hmr.Tests:components/counter.mjs", module.Hmr.ModuleId);
        StringAssert.Contains(module.Hmr.Payload, "\"componentId\":\"Demo.Pages.Counter\"");
        StringAssert.Contains(module.Hmr.Payload, "\"boundaryKind\":\"template-only\"");
    }

    [TestMethod]
    public void CatalogReader_TryRead_ReadsArtifactCatalogSourceMapPayload()
    {
        var assembly = CompileCatalogAssembly(
            "ArtifactCatalog.SourceMap.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ArtifactCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const string ProducerId = "adapter.test";

                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly ArtifactModule[] _modules =
                    [
                        new ArtifactModule(
                            "Demo.Pages.Counter",
                            "Demo.Pages.Counter",
                            "components/counter.mjs",
                            "export default {};",
                            "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
                            "components/counter.mjs.map",
                            "{\"version\":3,\"file\":\"components/counter.mjs\",\"sources\":[\"Counter.razor\"],\"names\":[],\"mappings\":\"AAAA\"}",
                            "sha256:abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789")
                    ];

                    private sealed class ArtifactModule(
                        string id,
                        string typeName,
                        string relativePath,
                        string content,
                        string hash,
                        string sourceMapRelativePath,
                        string sourceMapContent,
                        string mapHash)
                    {
                        public string Id { get; } = id;
                        public string TypeName { get; } = typeName;
                        public string RelativePath { get; } = relativePath;
                        public string Content { get; } = content;
                        public string Hash { get; } = hash;
                        public string SourceMapRelativePath { get; } = sourceMapRelativePath;
                        public string SourceMapContent { get; } = sourceMapContent;
                        public string MapHash { get; } = mapHash;
                    }
                }
            }
            """);

        var module = CatalogReader.TryRead(assembly)!.Single();

        Assert.AreEqual("components/counter.mjs.map", module.SourceMapRelativePath);
        StringAssert.Contains(module.SourceMapContent, "\"sources\":[\"Counter.razor\"]");
        Assert.AreEqual("sha256:abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789", module.MapHash);
    }

    [TestMethod]
    public void CatalogReader_TryRead_ReadsModuleSourceAssets()
    {
        var assembly = CompileCatalogAssembly(
            "ArtifactCatalog.Assets.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ArtifactCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const string ProducerId = "adapter.test";

                    internal static System.Collections.IEnumerable GetModules() => _modules;
                    internal static System.Collections.IEnumerable GetAssets() => _assets;

                    private static readonly ArtifactModule[] _modules =
                    [
                        new ArtifactModule("Demo.Pages.Counter", "Demo.Pages.Counter", "components/counter.mjs", "export default {};", "sha256:test")
                    ];
                    private static readonly ArtifactAsset[] _assets =
                    [
                        new ArtifactAsset("components/LocalCard.vue", "components/LocalCard.vue", "module-source", "components/LocalCard.vue.mjs", "")
                    ];

                    private sealed class ArtifactModule(string id, string typeName, string relativePath, string content, string hash)
                    {
                        public string Id { get; } = id;
                        public string TypeName { get; } = typeName;
                        public string RelativePath { get; } = relativePath;
                        public string Content { get; } = content;
                        public string Hash { get; } = hash;
                    }

                    private sealed class ArtifactAsset(string sourcePath, string artifactPath, string kind, string importPath, string contentHash)
                    {
                        public string SourcePath { get; } = sourcePath;
                        public string ArtifactPath { get; } = artifactPath;
                        public string Kind { get; } = kind;
                        public string ImportPath { get; } = importPath;
                        public string ContentHash { get; } = contentHash;
                    }
                }
            }
            """);

        var asset = CatalogReader.TryRead(assembly)!.Single().Assets!.Single();

        Assert.AreEqual("components/LocalCard.vue", asset.SourcePath);
        Assert.AreEqual("components/LocalCard.vue", asset.ArtifactPath);
        Assert.AreEqual(AssetEntry.KindModuleSource, asset.Kind);
        Assert.AreEqual("components/LocalCard.vue.mjs", asset.ImportPath);
        Assert.AreEqual(string.Empty, asset.Hash);
    }

    [TestMethod]
    public void CatalogReader_TryReadCatalogs_ReadsRuntimeProviderResourcesAndImportMapContributions()
    {
        var assembly = CompileCatalogAssembly(
            "RuntimeProvider.Reader.Tests",
            """
            namespace Jazor.Artifacts
            {
                internal static class RuntimeProviderCatalog
                {
                    internal const int SchemaVersion = 1;
                    internal const string ProviderId = "adapter.test";

                    internal static System.Collections.IEnumerable GetModules() => _modules;
                    internal static System.Collections.IEnumerable GetImportMapEntries() => _importMapEntries;

                    private static readonly RuntimeModule[] _modules =
                    [
                        new RuntimeModule("Runtime.context.mjs", "adapter.context", "@adapter/runtime/context.mjs", ["@adapter/runtime/core.mjs"]),
                        new RuntimeModule("Runtime.core.mjs", "adapter.core", "@adapter/runtime/core.mjs", [])
                    ];
                    private static readonly ImportMapEntry[] _importMapEntries =
                    [
                        new ImportMapEntry("@adapter/runtime/", "@adapter/runtime/")
                    ];

                    private sealed class RuntimeModule(string resourceName, string id, string relativePath, string[] dependencies)
                    {
                        public string ResourceName { get; } = resourceName;
                        public string Id { get; } = id;
                        public string RelativePath { get; } = relativePath;
                        public string[] Dependencies { get; } = dependencies;
                    }

                    private sealed class ImportMapEntry(string specifier, string artifactPath)
                    {
                        public string Specifier { get; } = specifier;
                        public string ArtifactPath { get; } = artifactPath;
                    }
                }
            }
            """,
            new ResourceDescription(
                "Runtime.context.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export { createContext } from './core.mjs';\n")),
                isPublic: true),
            new ResourceDescription(
                "Runtime.core.mjs",
                static () => new MemoryStream(Encoding.UTF8.GetBytes("export function createContext() {}\n")),
                isPublic: true));

        var result = CatalogReader.TryReadCatalogs(assembly);

        Assert.HasCount(2, result.Modules);
        Assert.AreEqual("adapter.test", result.Modules[0].RuntimeProviderId);
        CollectionAssert.AreEquivalent(
            new[] { "@adapter/runtime/core.mjs" },
            result.Modules.Single(static module => module.RelativePath == "@adapter/runtime/context.mjs").RuntimeDependencies!.ToArray());
        Assert.HasCount(1, result.ImportMapEntries);
        Assert.AreEqual("@adapter/runtime/", result.ImportMapEntries[0].Specifier);
        Assert.AreEqual("@adapter/runtime/", result.ImportMapEntries[0].ArtifactPath);
    }

    [TestMethod]
    public void ModuleCollector_RetainsTransitiveRuntimeProviderImports()
    {
        var component = CreateModule(
            "components/context-usage.mjs",
            "import { createContext } from \"@adapter/runtime/context.mjs\";\ncreateContext();\n");
        var context = CreateModule(
            "@adapter/runtime/context.mjs",
            "export { createContext } from \"./core.mjs\";\n",
            runtimeProviderId: "adapter.test",
            runtimeDependencies: ["@adapter/runtime/core.mjs"]);
        var core = CreateModule(
            "@adapter/runtime/core.mjs",
            "export function createContext() {}\n",
            runtimeProviderId: "adapter.test");

        var retained = ModuleCollector.RetainReferencedRuntimeProviderModules([component, context, core]);
        var paths = retained.Select(static module => module.RelativePath).ToArray();

        CollectionAssert.Contains(paths, "components/context-usage.mjs");
        CollectionAssert.Contains(paths, "@adapter/runtime/context.mjs");
        CollectionAssert.Contains(paths, "@adapter/runtime/core.mjs");
    }

    [TestMethod]
    public void RazorVueRuntimeModules_AreRetainedAndMaterializedOnlyWhenReferenced()
    {
        var catalog = CatalogReader.TryReadCatalogs(typeof(VueRawMarkup).Assembly);

        Assert.HasCount(3, catalog.Modules);
        var rawMarkupRuntime = catalog.Modules.Single(static module =>
            module.RelativePath == "@jazor/vue-runtime/raw-markup.mjs");
        var cascadingRuntime = catalog.Modules.Single(static module =>
            module.RelativePath == "@jazor/vue-runtime/cascading.mjs");
        var routingRuntime = catalog.Modules.Single(static module =>
            module.RelativePath == "@jazor/vue-runtime/blazor-routing.mjs");
        Assert.AreEqual("jazor.vue", rawMarkupRuntime.RuntimeProviderId);
        Assert.AreEqual("jazor.vue", cascadingRuntime.RuntimeProviderId);
        Assert.AreEqual("jazor.vue", routingRuntime.RuntimeProviderId);
        StringAssert.Contains(rawMarkupRuntime.Content, "export function createRawMarkup", StringComparison.Ordinal);
        StringAssert.Contains(cascadingRuntime.Content, "export const CascadingValue", StringComparison.Ordinal);
        StringAssert.Contains(routingRuntime.Content, "createNavigationHost", StringComparison.Ordinal);
        Assert.IsFalse(
            routingRuntime.Content.Contains("export const Router", StringComparison.Ordinal) ||
            routingRuntime.Content.Contains("export const RouteView", StringComparison.Ordinal) ||
            routingRuntime.Content.Contains("export const LayoutView", StringComparison.Ordinal) ||
            routingRuntime.Content.Contains("export const NavLink", StringComparison.Ordinal),
            "The routing host must not expose standard Blazor UI component adapters.");
        Assert.HasCount(1, catalog.ImportMapEntries);
        Assert.AreEqual("@jazor/vue-runtime/", catalog.ImportMapEntries[0].Specifier);

        var inactiveComponent = CreateModule("components/plain.mjs", "export default {};\n");
        var inactive = ModuleCollector.RetainReferencedRuntimeProviderModules(
            [inactiveComponent, rawMarkupRuntime, cascadingRuntime, routingRuntime]);
        Assert.HasCount(1, inactive);
        Assert.AreEqual("components/plain.mjs", inactive[0].RelativePath);

        var activeComponent = CreateModule(
            "components/raw-markup.mjs",
            "import { createRawMarkup } from \"@jazor/vue-runtime/raw-markup.mjs\";\nexport default createRawMarkup;\n");
        var active = ModuleCollector.RetainReferencedRuntimeProviderModules(
            [activeComponent, rawMarkupRuntime, cascadingRuntime, routingRuntime]);
        CollectionAssert.AreEquivalent(
            new[] { "components/raw-markup.mjs", "@jazor/vue-runtime/raw-markup.mjs" },
            active.Select(static module => module.RelativePath).ToArray());

        var cascadingComponent = CreateModule(
            "components/cascading-value.mjs",
            "import { CascadingValue } from \"@jazor/vue-runtime/cascading.mjs\";\nexport default CascadingValue;\n");
        var cascading = ModuleCollector.RetainReferencedRuntimeProviderModules(
            [cascadingComponent, rawMarkupRuntime, cascadingRuntime, routingRuntime]);
        CollectionAssert.AreEquivalent(
            new[] { "components/cascading-value.mjs", "@jazor/vue-runtime/cascading.mjs" },
            cascading.Select(static module => module.RelativePath).ToArray());

        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        try
        {
            var inactiveOutput = Path.Combine(root, "inactive");
            var inactiveWrite = ModuleWriter.Write(
                "RawMarkup.Provider.Tests.dll",
                inactiveOutput,
                Path.Combine(inactiveOutput, "jazor-manifest.json"),
                inactive,
                clean: true);
            Assert.IsTrue(inactiveWrite.IsSuccess, inactiveWrite.Error);
            Assert.IsFalse(File.Exists(Path.Combine(inactiveOutput, "@jazor", "vue-runtime", "raw-markup.mjs")));

            var activeOutput = Path.Combine(root, "active");
            var activeWrite = ModuleWriter.Write(
                "RawMarkup.Provider.Tests.dll",
                activeOutput,
                Path.Combine(activeOutput, "jazor-manifest.json"),
                active,
                clean: true,
                importMapEntries: catalog.ImportMapEntries);
            Assert.IsTrue(activeWrite.IsSuccess, activeWrite.Error);
            Assert.IsTrue(File.Exists(Path.Combine(activeOutput, "@jazor", "vue-runtime", "raw-markup.mjs")));
            Assert.IsFalse(File.Exists(Path.Combine(activeOutput, "@jazor", "vue-runtime", "cascading.mjs")));

            var cascadingOutput = Path.Combine(root, "cascading");
            var cascadingWrite = ModuleWriter.Write(
                "Cascading.Provider.Tests.dll",
                cascadingOutput,
                Path.Combine(cascadingOutput, "jazor-manifest.json"),
                cascading,
                clean: true,
                importMapEntries: catalog.ImportMapEntries);
            Assert.IsTrue(cascadingWrite.IsSuccess, cascadingWrite.Error);
            Assert.IsTrue(File.Exists(Path.Combine(cascadingOutput, "@jazor", "vue-runtime", "cascading.mjs")));
            Assert.IsFalse(File.Exists(Path.Combine(cascadingOutput, "@jazor", "vue-runtime", "raw-markup.mjs")));

            var manifest = ManifestModel.TryLoad(Path.Combine(activeOutput, "jazor-manifest.json"));
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.ImportMapEntries);
            Assert.AreEqual("@jazor/vue-runtime/", manifest.ImportMapEntries[0].Specifier);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_MaterializesNeutralHmrAndRuntimeProviderData()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "out");
        var manifestPath = Path.Combine(root, "jazor-manifest.json");
        try
        {
            var module = new ModuleRecord(
                SourceAssemblyPath: "test.dll",
                AssemblyName: "ArtifactCatalog.Materialize.Tests",
                TypeName: "Demo.Pages.Counter",
                Id: "Demo.Pages.Counter",
                RelativePath: "components/counter.mjs",
                Content: "export default {};",
                Hash: "sha256:test",
                Hmr: HmrMetadata.Create(
                    "adapter.test",
                    "ArtifactCatalog.Materialize.Tests:components/counter.mjs",
                    "{\"change\":\"safe\"}"));
            var runtime = CreateModule(
                "@adapter/runtime/context.mjs",
                "export function createContext() {}\n",
                runtimeProviderId: "adapter.test");
            var mapEntry = new ImportMapEntry("adapter.test", "@adapter/runtime/", "@adapter/runtime/");

            var result = ModuleWriter.Write(
                "ArtifactCatalog.Materialize.Tests.dll",
                outputDirectory,
                manifestPath,
                [module, runtime],
                clean: true,
                importMapEntries: [mapEntry]);

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "components", "counter.mjs")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "@adapter", "runtime", "context.mjs")));

            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(2, manifest.Modules);
            Assert.HasCount(1, manifest.ImportMapEntries);
            Assert.AreEqual("@adapter/runtime/", manifest.ImportMapEntries[0].Specifier);
            var hmr = manifest.Modules.Single(static entry => entry.RelativePath == "components/counter.mjs").Hmr;
            Assert.IsNotNull(hmr);
            Assert.AreEqual("adapter.test", hmr.ProviderId);
            using var hmrPayload = JsonDocument.Parse(hmr.Payload);
            Assert.AreEqual("safe", hmrPayload.RootElement.GetProperty("change").GetString());

            var manifestText = File.ReadAllText(manifestPath);
            StringAssert.Contains(manifestText, "\"providerId\": \"adapter.test\"");
            StringAssert.Contains(manifestText, "\"data\": {");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void CatalogReader_TryRead_RejectsUnsupportedArtifactCatalogSchema()
    {
        var assembly = CompileCatalogAssembly(
            "ArtifactCatalog.BadSchema.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ArtifactCatalog
                {
                    internal const int SchemaVersion = 99;
                    internal const string ProducerId = "adapter.test";
                    internal static System.Collections.IEnumerable GetModules() => System.Array.Empty<object>();
                }
            }
            """);

        var exception = Assert.Throws<InvalidOperationException>(() => CatalogReader.TryRead(assembly));
        StringAssert.Contains(exception.Message, "artifact catalog schema version '99'");
    }

    [TestMethod]
    public void CatalogReader_TryReadCatalogs_RejectsUnsupportedRuntimeProviderSchema()
    {
        var assembly = CompileCatalogAssembly(
            "RuntimeProvider.BadSchema.Tests",
            """
            namespace Jazor.Artifacts
            {
                internal static class RuntimeProviderCatalog
                {
                    internal const int SchemaVersion = 99;
                    internal const string ProviderId = "adapter.test";
                    internal static System.Collections.IEnumerable GetModules() => System.Array.Empty<object>();
                }
            }
            """);

        var exception = Assert.Throws<InvalidOperationException>(() => CatalogReader.TryReadCatalogs(assembly));
        StringAssert.Contains(exception.Message, "runtime provider catalog schema version '99'");
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

    private static ModuleRecord CreateModule(
        string relativePath,
        string content,
        string? runtimeProviderId = null,
        IReadOnlyList<string>? runtimeDependencies = null)
        => new(
            SourceAssemblyPath: "test.dll",
            AssemblyName: "Test",
            TypeName: relativePath,
            Id: relativePath,
            RelativePath: relativePath,
            Content: content,
            Hash: "sha256:test",
            RuntimeProviderId: runtimeProviderId,
            RuntimeDependencies: runtimeDependencies);
}
