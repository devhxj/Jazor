using System.IO;
using System.Linq;
using System.Reflection;
using Basic.Reference.Assemblies;
using Jazor.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.EmitTest;

[TestClass]
public sealed class StaticModuleSourceMapTests
{
    [TestMethod]
    public void CatalogReader_TryRead_MergesModuleSourceMapCatalogById()
    {
        var assembly = CompileCatalogAssembly(
            "StaticModule.SourceMap.Reader.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ModuleCatalog
                {
                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly GeneratedModule[] _modules = new GeneratedModule[]
                    {
                        new GeneratedModule(
                            assemblyName: "Sample.Host",
                            typeName: "Demo.Modules.Counter",
                            id: "Demo.Modules.Counter",
                            relativePath: "modules/counter.mjs",
                            content: "export const counter = 1;",
                            hash: "hash-js"),
                        new GeneratedModule(
                            assemblyName: "Sample.Host",
                            typeName: "Demo.Modules.Plain",
                            id: "Demo.Modules.Plain",
                            relativePath: "modules/plain.mjs",
                            content: "export const plain = 2;",
                            hash: "hash-plain")
                    };

                    private sealed class GeneratedModule
                    {
                        public GeneratedModule(string assemblyName, string typeName, string id, string relativePath, string content, string hash)
                        {
                            AssemblyName = assemblyName;
                            TypeName = typeName;
                            Id = id;
                            RelativePath = relativePath;
                            Content = content;
                            Hash = hash;
                        }

                        public string AssemblyName { get; }
                        public string TypeName { get; }
                        public string Id { get; }
                        public string RelativePath { get; }
                        public string Content { get; }
                        public string Hash { get; }
                    }
                }

                internal static partial class ModuleSourceMapCatalog
                {
                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly GeneratedModuleSourceMap[] _modules = new GeneratedModuleSourceMap[]
                    {
                        new GeneratedModuleSourceMap(
                            assemblyName: "Sample.Host",
                            typeName: "Demo.Modules.Counter",
                            id: "Demo.Modules.Counter",
                            relativePath: "modules/counter.mjs",
                            sourceMapRelativePath: "modules/counter.mjs.map",
                            sourceMapContent: "{\"version\":3,\"file\":\"modules/counter.mjs\",\"sources\":[],\"names\":[],\"mappings\":\"\"}",
                            mapHash: "hash-map")
                    };

                    private sealed class GeneratedModuleSourceMap
                    {
                        public GeneratedModuleSourceMap(
                            string assemblyName,
                            string typeName,
                            string id,
                            string relativePath,
                            string sourceMapRelativePath,
                            string sourceMapContent,
                            string mapHash)
                        {
                            AssemblyName = assemblyName;
                            TypeName = typeName;
                            Id = id;
                            RelativePath = relativePath;
                            SourceMapRelativePath = sourceMapRelativePath;
                            SourceMapContent = sourceMapContent;
                            MapHash = mapHash;
                        }

                        public string AssemblyName { get; }
                        public string TypeName { get; }
                        public string Id { get; }
                        public string RelativePath { get; }
                        public string SourceMapRelativePath { get; }
                        public string SourceMapContent { get; }
                        public string MapHash { get; }
                    }
                }
            }
            """);

        var modules = CatalogReader.TryRead(assembly);

        Assert.IsNotNull(modules);
        Assert.HasCount(2, modules);

        var counter = modules.Single(static module => module.Id == "Demo.Modules.Counter");
        Assert.AreEqual("modules/counter.mjs.map", counter.SourceMapRelativePath);
        Assert.AreEqual("{\"version\":3,\"file\":\"modules/counter.mjs\",\"sources\":[],\"names\":[],\"mappings\":\"\"}", counter.SourceMapContent);
        Assert.AreEqual("hash-map", counter.MapHash);

        var plain = modules.Single(static module => module.Id == "Demo.Modules.Plain");
        Assert.IsNull(plain.SourceMapRelativePath);
        Assert.IsNull(plain.SourceMapContent);
        Assert.IsNull(plain.MapHash);
    }

    [TestMethod]
    public void CatalogReader_TryRead_DoesNotReadRetiredEcmascriptCatalogType()
    {
        var assembly = CompileCatalogAssembly(
            "ECMAScript.Runtime.Reader.Tests",
            """
            namespace ECMAScript
            {
                internal static partial class Catalog
                {
                    internal static System.Collections.IEnumerable GetModules() => _modules;

                    private static readonly GeneratedModule[] _modules =
                    [
                        new GeneratedModule(
                            assemblyName: "ECMAScript",
                            typeName: "Jazor.CLR.RuntimeModule",
                            id: "Jazor.CLR.RuntimeModule",
                            relativePath: "System/RuntimeModule.js",
                            content: "export const RuntimeModule = {};",
                            hash: "hash-runtime")
                    ];

                    private sealed class GeneratedModule
                    {
                        public GeneratedModule(string assemblyName, string typeName, string id, string relativePath, string content, string hash)
                        {
                            AssemblyName = assemblyName;
                            TypeName = typeName;
                            Id = id;
                            RelativePath = relativePath;
                            Content = content;
                            Hash = hash;
                        }

                        public string AssemblyName { get; }
                        public string TypeName { get; }
                        public string Id { get; }
                        public string RelativePath { get; }
                        public string Content { get; }
                        public string Hash { get; }
                    }
                }
            }
            """);

        var result = CatalogReader.TryReadCatalogs(assembly);

        Assert.HasCount(0, result.Modules);
        Assert.HasCount(0, result.ImportMapEntries);
    }

    [TestMethod]
    public void ModuleWriter_Write_WithSourceMap_WritesMapAndManifestFields()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "Sample.Host.dll");
        var modulePath = Path.Combine(outputDirectory, "modules", "counter.mjs");
        var mapPath = modulePath + ".map";
        var mapContent = "{\"version\":3,\"file\":\"modules/counter.mjs\",\"sources\":[],\"names\":[],\"mappings\":\"\"}";

        try
        {
            var writer = new ModuleWriter();
            var result = ModuleWriter.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                [
                    new ModuleRecord(
                        SourceAssemblyPath: rootAssemblyPath,
                        AssemblyName: "Sample.Host",
                        TypeName: "Demo.Modules.Counter",
                        Id: "Demo.Modules.Counter",
                        RelativePath: "modules/counter.mjs",
                        Content: "export const counter = 1;\n",
                        Hash: "hash-js",
                        SourceMapRelativePath: "modules/counter.mjs.map",
                        SourceMapContent: mapContent,
                        MapHash: "hash-map")
                ],
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            Assert.AreEqual(1, result.Written);
            Assert.IsTrue(File.Exists(modulePath));
            Assert.IsTrue(File.Exists(mapPath));

            var moduleCode = File.ReadAllText(modulePath);
            StringAssert.Contains(moduleCode, "export const counter = 1;");
            StringAssert.Contains(moduleCode, "//# sourceMappingURL=counter.mjs.map");
            Assert.AreEqual(1, moduleCode.Split("sourceMappingURL=").Length - 1);

            Assert.AreEqual(mapContent, File.ReadAllText(mapPath));

            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("modules/counter.mjs.map", manifest.Modules[0].SourceMapPath);
            Assert.AreEqual("hash-map", manifest.Modules[0].MapHash);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_ProducesCanonicalSchemaV1ManifestWithoutTimeOrAbsoluteRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "bin", "Debug", "net11.0", "Sample.Host.dll");
        var modules = new[]
        {
            new ModuleRecord(
                SourceAssemblyPath: rootAssemblyPath,
                AssemblyName: "Sample.Host",
                TypeName: "Demo.Pages.Counter",
                Id: "Demo.Pages.Counter",
                RelativePath: "components/counter.mjs",
                Content: "export default {};\n",
                Hash: "sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef")
        };

        try
        {
            var writer = new ModuleWriter();
            var first = ModuleWriter.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                modules,
                clean: true);

            Assert.IsTrue(first.IsSuccess, first.Error ?? string.Empty);
            var firstManifest = File.ReadAllText(manifestPath).ReplaceLineEndings("\n");

            var second = ModuleWriter.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                modules,
                clean: true);

            Assert.IsTrue(second.IsSuccess, second.Error ?? string.Empty);
            var secondManifest = File.ReadAllText(manifestPath).ReplaceLineEndings("\n");

            Assert.AreEqual(firstManifest, secondManifest);
            StringAssert.Contains(firstManifest, "\"schemaVersion\": 1");
            StringAssert.Contains(firstManifest, "\"runtimeProtocolVersion\": 1");
            StringAssert.Contains(firstManifest, "\"rootAssemblyName\": \"Sample.Host\"");
            StringAssert.Contains(firstManifest, "\"entries\": [");
            StringAssert.Contains(firstManifest, "\"components/counter.mjs\"");
            StringAssert.Contains(firstManifest, "\"path\": \"components/counter.mjs\"");
            StringAssert.Contains(firstManifest, "\"contentHash\": \"sha256:0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef\"");

            Assert.IsFalse(firstManifest.Contains("generatedAtUtc", StringComparison.OrdinalIgnoreCase), firstManifest);
            Assert.IsFalse(firstManifest.Contains("rootAssemblyPath", StringComparison.OrdinalIgnoreCase), firstManifest);
            Assert.IsFalse(firstManifest.Contains(root.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase), firstManifest);
            Assert.IsFalse(firstManifest.Contains(root.Replace("\\", "\\\\"), StringComparison.OrdinalIgnoreCase), firstManifest);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_WhenSourceMapRemoved_DeletesStaleMapAndClearsManifestFields()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "Sample.Host.dll");
        var modulePath = Path.Combine(outputDirectory, "modules", "counter.mjs");
        var mapPath = modulePath + ".map";

        try
        {
            var writer = new ModuleWriter();
            var first = ModuleWriter.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                [
                    new ModuleRecord(
                        SourceAssemblyPath: rootAssemblyPath,
                        AssemblyName: "Sample.Host",
                        TypeName: "Demo.Modules.Counter",
                        Id: "Demo.Modules.Counter",
                        RelativePath: "modules/counter.mjs",
                        Content: "export const counter = 1;\n",
                        Hash: "hash-js",
                        SourceMapRelativePath: "modules/counter.mjs.map",
                        SourceMapContent: "{\"version\":3,\"file\":\"modules/counter.mjs\",\"sources\":[],\"names\":[],\"mappings\":\"\"}",
                        MapHash: "hash-map")
                ],
                clean: true);

            Assert.IsTrue(first.IsSuccess, first.Error ?? string.Empty);
            Assert.IsTrue(File.Exists(mapPath));

            var second = ModuleWriter.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                [
                    new ModuleRecord(
                        SourceAssemblyPath: rootAssemblyPath,
                        AssemblyName: "Sample.Host",
                        TypeName: "Demo.Modules.Counter",
                        Id: "Demo.Modules.Counter",
                        RelativePath: "modules/counter.mjs",
                        Content: "export const counter = 1;\n",
                        Hash: "hash-js")
                ],
                clean: true);

            Assert.IsTrue(second.IsSuccess, second.Error ?? string.Empty);
            Assert.AreEqual(1, second.Written);
            Assert.IsFalse(File.Exists(mapPath));

            var moduleCode = File.ReadAllText(modulePath);
            Assert.AreEqual(0, moduleCode.Split("sourceMappingURL=").Length - 1);

            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.IsNull(manifest.Modules[0].SourceMapPath);
            Assert.IsNull(manifest.Modules[0].MapHash);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_WhenLegacyManifestUsesDotSegments_PreservesCurrentModule()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var outputDirectory = Path.Combine(root, "wwwroot", "jazor");
        var manifestPath = Path.Combine(outputDirectory, "jazor-manifest.json");
        var rootAssemblyPath = Path.Combine(root, "Sample.Host.dll");
        var modulePath = Path.Combine(outputDirectory, "components", "wiki-home.mjs");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(modulePath)!);
            File.WriteAllText(modulePath, "stale");
            File.WriteAllText(
                manifestPath,
                """
                {
                  "RootAssemblyPath": "legacy.dll",
                  "GeneratedAtUtc": "2026-04-29T00:00:00Z",
                  "Modules": [
                    {
                      "AssemblyName": "Sample.Host",
                      "TypeName": "Demo.Modules.WikiHome",
                      "Id": "Demo.Modules.WikiHome",
                      "RelativePath": "./components/wiki-home.mjs",
                      "Hash": "legacy-hash"
                    }
                  ]
                }
                """);

            var writer = new ModuleWriter();
            var result = ModuleWriter.Write(
                rootAssemblyPath,
                outputDirectory,
                manifestPath,
                [
                    new ModuleRecord(
                        SourceAssemblyPath: rootAssemblyPath,
                        AssemblyName: "Sample.Host",
                        TypeName: "Demo.Modules.WikiHome",
                        Id: "Demo.Modules.WikiHome",
                        RelativePath: "components/wiki-home.mjs",
                        Content: "export default 1;\n",
                        Hash: "current-hash")
                ],
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            Assert.IsTrue(File.Exists(modulePath));
            Assert.AreEqual("export default 1;\n", File.ReadAllText(modulePath));

            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.AreEqual("components/wiki-home.mjs", manifest.Modules[0].RelativePath);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }


    private static Assembly CompileCatalogAssembly(string assemblyName, string source)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(source, path: $"{assemblyName}.g.cs")
            ],
            references: Net110.References.All.Cast<MetadataReference>(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        var emitResult = compilation.Emit(stream);
        Assert.IsTrue(emitResult.Success, string.Join("\n", emitResult.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        stream.Position = 0;
        return Assembly.Load(stream.ToArray());
    }
}
