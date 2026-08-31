using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Jazor.Emit;

namespace Jazor.EmitTest;

[TestClass]
public sealed class ModuleCatalogReaderTests
{
    [TestMethod]
    public void CatalogReader_TryRead_ReadsModuleAndSourceMapFromOneCatalog()
    {
        const string moduleContent = "export default {};";
        const string mapContent = "{\"version\":3,\"file\":\"components/counter.mjs\",\"sources\":[],\"names\":[],\"mappings\":\"\"}";
        var assembly = CompileCatalogAssembly(
            "ModuleCatalog.Reader.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ModuleCatalog
                {
                    internal const int SchemaVersion = 2;
                    internal const string AssemblyName = "ModuleCatalog.Reader.Tests";
                    internal static System.Collections.IEnumerable GetModules() => _modules;
                    internal static System.Collections.IEnumerable GetAssets() => _assets;

                    private static readonly GeneratedModule[] _modules =
                    [
                        new GeneratedModule(
                            "Demo.Pages.Counter",
                            "Demo.Pages.Counter",
                            "components/counter.mjs",
                            {{moduleContent}},
                            {{moduleHash}},
                            "components/counter.mjs.map",
                            {{mapContent}},
                            {{mapHash}})
                    ];

                    private static readonly GeneratedAsset[] _assets =
                    [
                        new GeneratedAsset("components/counter.mjs", "components/LocalCard.vue", "components/LocalCard.vue", "module-source", "components/LocalCard.vue.mjs", "")
                    ];

                    private sealed class GeneratedModule(
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

                    private sealed class GeneratedAsset(string ownerModulePath, string sourcePath, string artifactPath, string kind, string importPath, string contentHash)
                    {
                        public string OwnerModulePath { get; } = ownerModulePath;
                        public string SourcePath { get; } = sourcePath;
                        public string ArtifactPath { get; } = artifactPath;
                        public string Kind { get; } = kind;
                        public string ImportPath { get; } = importPath;
                        public string ContentHash { get; } = contentHash;
                    }
                }
            }
            """.Replace("{{moduleContent}}", EscapeCSharp(moduleContent))
                .Replace("{{moduleHash}}", EscapeCSharp(Sha256(moduleContent)))
                .Replace("{{mapContent}}", EscapeCSharp(mapContent))
                .Replace("{{mapHash}}", EscapeCSharp(Sha256(mapContent))));

        var result = CatalogReader.TryReadCatalogs(assembly);

        Assert.HasCount(1, result.Modules);
        Assert.HasCount(1, result.Assets);
        var module = result.Modules.Single();
        Assert.AreEqual("ModuleCatalog.Reader.Tests", module.AssemblyName);
        Assert.AreEqual("Demo.Pages.Counter", module.Id);
        Assert.AreEqual(moduleContent, module.Content);
        Assert.AreEqual(Sha256(moduleContent), module.Hash);
        Assert.AreEqual("components/counter.mjs.map", module.SourceMapRelativePath);
        Assert.AreEqual(mapContent, module.SourceMapContent);
        Assert.AreEqual(Sha256(mapContent), module.MapHash);

        var asset = result.Assets.Single();
        Assert.AreEqual(AssetEntry.KindModuleSource, asset.Kind);
        Assert.AreEqual("components/LocalCard.vue.mjs", asset.ImportPath);
    }

    [TestMethod]
    public void CatalogReader_TryRead_PreservesHmrMetadataInModuleCatalog()
    {
        const string content = "export default {};";
        var assembly = CompileCatalogAssembly(
            "ModuleCatalog.Hmr.Tests",
            """
            namespace Jazor.Generated
            {
                internal static partial class ModuleCatalog
                {
                    internal const int SchemaVersion = 2;
                    internal static System.Collections.IEnumerable GetModules() => _modules;
                    private static readonly GeneratedModule[] _modules =
                    [
                        new GeneratedModule(
                            "Demo.Pages.Counter",
                            "Demo.Pages.Counter",
                            "components/counter.mjs",
                            {{content}},
                            {{hash}},
                            "jazor.vue",
                            "Demo:components/counter.mjs",
                            {{payload}})
                    ];

                    private sealed class GeneratedModule(
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
            """.Replace("{{content}}", EscapeCSharp(content))
                .Replace("{{hash}}", EscapeCSharp(Sha256(content)))
                .Replace("{{payload}}", EscapeCSharp("{\"componentId\":\"Demo.Pages.Counter\"}")));

        var module = CatalogReader.TryRead(assembly)!.Single();

        Assert.IsNotNull(module.Hmr);
        Assert.AreEqual("jazor.vue", module.Hmr.ProviderId);
        Assert.AreEqual("Demo:components/counter.mjs", module.Hmr.ModuleId);
        using var payload = JsonDocument.Parse(module.Hmr.Payload);
        Assert.AreEqual("Demo.Pages.Counter", payload.RootElement.GetProperty("componentId").GetString());
    }

    [TestMethod]
    public void CatalogReader_TryRead_IgnoresUnrelatedCatalogType()
    {
        var assembly = CompileCatalogAssembly(
            "UnrelatedCatalog.Tests",
            """
            namespace Jazor.Generated
            {
                internal static class UnrelatedCatalog
                {
                    internal const int SchemaVersion = 99;
                    internal static System.Collections.IEnumerable GetModules() => System.Array.Empty<object>();
                }
            }
            """);

        var result = CatalogReader.TryReadCatalogs(assembly);

        Assert.IsEmpty(result.Modules);
        Assert.IsEmpty(result.Assets);
    }

    [TestMethod]
    public void CatalogReader_TryRead_RejectsUnsupportedModuleCatalogSchema()
    {
        var assembly = CompileCatalogAssembly(
            "ModuleCatalog.BadSchema.Tests",
            """
            namespace Jazor.Generated
            {
                internal static class ModuleCatalog
                {
                    internal const int SchemaVersion = 99;
                    internal static System.Collections.IEnumerable GetModules() => System.Array.Empty<object>();
                }
            }
            """);

        var exception = Assert.Throws<InvalidOperationException>(() => CatalogReader.TryRead(assembly));

        StringAssert.Contains(exception.Message, "schema version '99'");
    }

    [TestMethod]
    public void ModuleCollector_Collect_SelectsOnlyConsoleRootModuleClosureAndOwnedAssets()
    {
        const string hostContent = "export { bridge } from '../b/bridge.mjs';";
        const string bridgeContent = "export { used } from '../a/used.mjs';";
        const string usedContent = "export const used = true;";
        const string unusedContent = "export const unused = true;";
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));

        try
        {
            var consoleAssemblyPath = CompileCatalogAssemblyToPath(
                root,
                "Console.Host",
                """
                namespace Jazor.Generated
                {
                    internal static class ModuleCatalog
                    {
                        internal const int SchemaVersion = 2;
                        internal const string AssemblyName = "Console.Host";
                        internal static System.Collections.IEnumerable GetModules() => new object[]
                        {
                            new Module("Console.App", "host/app.mjs", {{content}}, {{hash}}, new string[] { "b/bridge.mjs" })
                        };

                        private sealed class Module
                        {
                            public Module(string id, string relativePath, string content, string hash, string[] dependencies)
                            { Id = id; RelativePath = relativePath; Content = content; Hash = hash; Dependencies = dependencies; }
                            public string Id { get; } public string TypeName => Id; public string RelativePath { get; }
                            public string Content { get; } public string Hash { get; } public string[] Dependencies { get; }
                        }
                    }
                }
                """.Replace("{{content}}", EscapeCSharp(hostContent))
                    .Replace("{{hash}}", EscapeCSharp(Sha256(hostContent))));
            var bridgeAssemblyPath = CompileCatalogAssemblyToPath(
                root,
                "Bridge.Library",
                """
                namespace Jazor.Generated
                {
                    internal static class ModuleCatalog
                    {
                        internal const int SchemaVersion = 2;
                        internal const string AssemblyName = "Bridge.Library";
                        internal static System.Collections.IEnumerable GetModules() => new object[]
                        {
                            new Module("Bridge.Module", "b/bridge.mjs", {{content}}, {{hash}}, new string[] { "a/used.mjs" })
                        };

                        private sealed class Module
                        {
                            public Module(string id, string relativePath, string content, string hash, string[] dependencies)
                            { Id = id; RelativePath = relativePath; Content = content; Hash = hash; Dependencies = dependencies; }
                            public string Id { get; } public string TypeName => Id; public string RelativePath { get; }
                            public string Content { get; } public string Hash { get; } public string[] Dependencies { get; }
                        }
                    }
                }
                """.Replace("{{content}}", EscapeCSharp(bridgeContent))
                    .Replace("{{hash}}", EscapeCSharp(Sha256(bridgeContent))));
            var libraryAssemblyPath = CompileCatalogAssemblyToPath(
                root,
                "A.Library",
                """
                namespace Jazor.Generated
                {
                    internal static class ModuleCatalog
                    {
                        internal const int SchemaVersion = 2;
                        internal const string AssemblyName = "A.Library";
                        internal static System.Collections.IEnumerable GetModules() => new object[]
                        {
                            new Module("A.Used", "a/used.mjs", {{usedContent}}, {{usedHash}}, new string[0]),
                            new Module("A.Unused", "a/unused.mjs", {{unusedContent}}, {{unusedHash}}, new string[0])
                        };
                        internal static System.Collections.IEnumerable GetAssets() => new object[]
                        {
                            new Asset("a/used.mjs", "assets/used.txt", "assets/used.txt", "static", null, ""),
                            new Asset("a/unused.mjs", "assets/unused.txt", "assets/unused.txt", "static", null, "")
                        };

                        private sealed class Module
                        {
                            public Module(string id, string relativePath, string content, string hash, string[] dependencies)
                            { Id = id; RelativePath = relativePath; Content = content; Hash = hash; Dependencies = dependencies; }
                            public string Id { get; } public string TypeName => Id; public string RelativePath { get; }
                            public string Content { get; } public string Hash { get; } public string[] Dependencies { get; }
                        }

                        private sealed class Asset
                        {
                            public Asset(string ownerModulePath, string sourcePath, string artifactPath, string kind, string importPath, string contentHash)
                            { OwnerModulePath = ownerModulePath; SourcePath = sourcePath; ArtifactPath = artifactPath; Kind = kind; ImportPath = importPath; ContentHash = contentHash; }
                            public string OwnerModulePath { get; } public string SourcePath { get; } public string ArtifactPath { get; }
                            public string Kind { get; } public string ImportPath { get; } public string ContentHash { get; }
                        }
                    }
                }
                """.Replace("{{usedContent}}", EscapeCSharp(usedContent))
                    .Replace("{{usedHash}}", EscapeCSharp(Sha256(usedContent)))
                    .Replace("{{unusedContent}}", EscapeCSharp(unusedContent))
                    .Replace("{{unusedHash}}", EscapeCSharp(Sha256(unusedContent))));

            var collection = CollectCatalogClosure(
                consoleAssemblyPath,
                bridgeAssemblyPath,
                libraryAssemblyPath);
            WaitForUnload(collection.LoadContext);
            var result = collection.Result;

            Assert.IsTrue(result.IsSuccess, result.Error ?? string.Empty);
            CollectionAssert.AreEquivalent(
                new[] { "host/app.mjs", "b/bridge.mjs", "a/used.mjs" },
                result.Modules.Select(static module => module.RelativePath).ToArray());
            CollectionAssert.AreEquivalent(
                new[] { "assets/used.txt" },
                result.Assets.Select(static asset => asset.ArtifactPath).ToArray());
        }
        finally
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleCollector_Collect_UsesReferencedCatalogsWhenRootHasNoModule()
    {
        const string upstreamContent = "export const upstream = true;";
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));

        try
        {
            var consoleAssemblyPath = CompileCatalogAssemblyToPath(
                root,
                "Consumer.Host",
                "public static class Host { } ");
            var libraryAssemblyPath = CompileCatalogAssemblyToPath(
                root,
                "Upstream.Library",
                """
                namespace Jazor.Generated
                {
                    internal static class ModuleCatalog
                    {
                        internal const int SchemaVersion = 2;
                        internal const string AssemblyName = "Upstream.Library";
                        internal static System.Collections.IEnumerable GetModules() => new object[]
                        {
                            new Module("Upstream.Module", "upstream/module.mjs", {{content}}, {{hash}}, new string[0])
                        };

                        private sealed class Module
                        {
                            public Module(string id, string relativePath, string content, string hash, string[] dependencies)
                            { Id = id; RelativePath = relativePath; Content = content; Hash = hash; Dependencies = dependencies; }
                            public string Id { get; } public string TypeName => Id; public string RelativePath { get; }
                            public string Content { get; } public string Hash { get; } public string[] Dependencies { get; }
                        }
                    }
                }
                """.Replace("{{content}}", EscapeCSharp(upstreamContent))
                    .Replace("{{hash}}", EscapeCSharp(Sha256(upstreamContent))));

            var collection = CollectCatalogClosure(consoleAssemblyPath, libraryAssemblyPath);
            WaitForUnload(collection.LoadContext);

            Assert.IsTrue(collection.Result.IsSuccess, collection.Result.Error ?? string.Empty);
            CollectionAssert.AreEquivalent(
                new[] { "upstream/module.mjs" },
                collection.Result.Modules.Select(static module => module.RelativePath).ToArray());
        }
        finally
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public void ModuleWriter_Write_MaterializesHmrAndModuleCatalogMetadata()
    {
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var output = Path.Combine(root, "out");
        var manifestPath = Path.Combine(output, "jazor-manifest.json");
        const string content = "export default {};";
        try
        {
            var result = ModuleWriter.Write(
                "ModuleCatalog.Materialize.Tests.dll",
                output,
                manifestPath,
                [new ModuleRecord(
                    "ModuleCatalog.Materialize.Tests.dll",
                    "ModuleCatalog.Materialize.Tests",
                    "Demo.Pages.Counter",
                    "Demo.Pages.Counter",
                    "components/counter.mjs",
                    content,
                    Sha256(content),
                    Hmr: HmrMetadata.Create("jazor.vue", "Demo:components/counter.mjs", "{\"change\":\"safe\"}"))],
                clean: true);

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.IsTrue(File.Exists(Path.Combine(output, "components", "counter.mjs")));
            var manifest = ManifestModel.TryLoad(manifestPath);
            Assert.IsNotNull(manifest);
            Assert.HasCount(1, manifest.Modules);
            Assert.IsNotNull(manifest.Modules[0].Hmr);
            Assert.AreEqual("jazor.vue", manifest.Modules[0].Hmr!.ProviderId);
            StringAssert.Contains(File.ReadAllText(manifestPath), "\"providerId\": \"jazor.vue\"");
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static Assembly CompileCatalogAssembly(string assemblyName, string source)
    {
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
            source,
            new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview),
            path: assemblyName + ".g.cs");
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: [syntaxTree],
            references:
            [
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(System.Collections.IEnumerable).Assembly.Location)
            ],
            options: new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        Assert.IsTrue(
            result.Success,
            string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        return Assembly.Load(stream.ToArray());
    }

    private static string CompileCatalogAssemblyToPath(string directory, string assemblyName, string source)
    {
        Directory.CreateDirectory(directory);
        var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(
            source,
            new Microsoft.CodeAnalysis.CSharp.CSharpParseOptions(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview),
            path: assemblyName + ".g.cs");
        var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: [syntaxTree],
            references:
            [
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(System.Collections.IEnumerable).Assembly.Location)
            ],
            options: new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(
                Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary));
        var assemblyPath = Path.Combine(directory, assemblyName + ".dll");
        using var stream = File.Create(assemblyPath);
        var result = compilation.Emit(stream);
        Assert.IsTrue(
            result.Success,
            string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        return assemblyPath;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (CollectResult Result, WeakReference LoadContext) CollectCatalogClosure(
        string rootAssemblyPath,
        params string[] assemblyPaths)
    {
        var loadContext = new EmitLoadContext(rootAssemblyPath);
        var weakReference = new WeakReference(loadContext);
        try
        {
            var collector = new ModuleCollector(loadContext);
            collector.AddAssembly(rootAssemblyPath);
            foreach (var assemblyPath in assemblyPaths)
                collector.AddAssembly(assemblyPath);
            return (collector.Collect(rootAssemblyPath), weakReference);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    private static void WaitForUnload(WeakReference loadContext)
    {
        for (var attempt = 0; attempt < 10 && loadContext.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string EscapeCSharp(string value)
        => Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(value, quote: true);
}
