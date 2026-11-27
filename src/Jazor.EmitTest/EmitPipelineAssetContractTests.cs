using System.Security.Cryptography;
using System.Text;
using Basic.Reference.Assemblies;
using Jazor.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.EmitTest;

[TestClass]
public sealed class EmitPipelineAssetContractTests
{
    [TestMethod]
    public async Task ExecuteAsync_BundlerHandoffWritesModulesAndAssetsWithoutStandaloneProject()
    {
        const string javascript = """import locale from "@example/widgets/locale/en"; export const load = () => import("/Pages/View.jazor");""";
        const string asset = "asset";
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var outputRoot = Path.Combine(root, "out");
        var manifestPath = Path.Combine(root, "handoff-manifest.json");
        try
        {
            Directory.CreateDirectory(sourceRoot);
            await File.WriteAllTextAsync(Path.Combine(sourceRoot, "logo.txt"), asset);
            await File.WriteAllTextAsync(Path.Combine(sourceRoot, "package.json"),
                """{"dependencies":{"@example/widgets":"1.2.3"}}""");
            var assembly = CompileCatalogAssemblyToPath(root, "Handoff.Host",
                CreateCatalogSource(javascript, "logo.txt", "assets/logo.txt", Sha256(asset))
                    .Replace("public string[] Dependencies { get; }", """
                        public string[] Dependencies { get; }
                        public string[] PackageImports => new[] { "@example/widgets/locale/en" };
                        """, StringComparison.Ordinal));
            var result = await new EmitPipeline().ExecuteAsync(
                new EmitOptions(assembly, [], outputRoot, manifestPath, BuildMode.Development, sourceRoot, [],
                    EnableSsr: false, BundlerHandoff: true));

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.AreEqual(javascript, await File.ReadAllTextAsync(Path.Combine(outputRoot, "components", "app.mjs")));
            Assert.AreEqual(asset, await File.ReadAllTextAsync(Path.Combine(outputRoot, "assets", "logo.txt")));
            Assert.IsTrue(File.Exists(manifestPath));
            Assert.IsFalse(File.Exists(Path.Combine(outputRoot, "package.json")));
            Assert.IsFalse(File.Exists(Path.Combine(outputRoot, "entry.js")));
            Assert.IsFalse(Directory.Exists(Path.Combine(outputRoot, "node_modules")));
            var standalone = await ExecuteAsync(assembly, sourceRoot, Path.Combine(root, "standalone"));
            Assert.IsFalse(standalone.IsSuccess);
            StringAssert.Contains(standalone.Error, "JAZOR_LIBRARY_IMPORT_MISSING");
        }
        finally
        {
            WaitForAssemblyUnload();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ExecuteAsync_SharedRootRejectsJoltStyleCollisionBeforeWritingModules()
    {
        var root = RepositoryTemp.CreateDirectory("handoff-style-");
        var source = Directory.CreateDirectory(Path.Combine(root, "source")).FullName;
        var output = Directory.CreateDirectory(Path.Combine(root, "out")).FullName;
        var style = Path.Combine(output, "Page.jolt.css");
        var inventory = Path.Combine(output, "jolt.modules.json");
        try
        {
            const string content = "body { color: blue; }";
            await File.WriteAllTextAsync(Path.Combine(source, "theme.css"), content);
            var assembly = CompileCatalogAssemblyToPath(root, "Style.Host",
                CreateCatalogSource("export const app = true;", "theme.css", "Page.jolt.css", Sha256(content)));
            var options = new EmitOptions(assembly, [], output, Path.Combine(root, "manifest.json"),
                BuildMode.Development, source, [], EnableSsr: false, BundlerHandoff: true);
            await File.WriteAllTextAsync(style, ".page { color: green; }");
            await File.WriteAllTextAsync(inventory, """["Page.jolt.css"]""");

            var collision = await new EmitPipeline().ExecuteAsync(options);
            Assert.IsFalse(collision.IsSuccess);
            StringAssert.Contains(collision.Error, "Jolt and Jazor both generate 'Page.jolt.css'");
            Assert.AreEqual(".page { color: green; }", await File.ReadAllTextAsync(style));
            Assert.IsFalse(File.Exists(Path.Combine(output, "components", "app.mjs")));

            await File.WriteAllTextAsync(inventory, """["OtherPage.mjs"]""");
            var retry = await new EmitPipeline().ExecuteAsync(options);
            Assert.IsTrue(retry.IsSuccess, retry.Error);
            Assert.AreEqual(content, await File.ReadAllTextAsync(style));
        }
        finally
        {
            WaitForAssemblyUnload();
            Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    public async Task ExecuteAsync_ValidatesAssetSourceHashBeforeReusingExistingOutput()
    {
        const string moduleContent = "export const app = true;";
        const string expectedAssetContent = "expected";
        const string tamperedAssetContent = "tampered";
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var outputRoot = Path.Combine(root, "out");
        var assetSource = Path.Combine(sourceRoot, "assets", "logo.txt");
        var existingAsset = Path.Combine(outputRoot, "assets", "logo.txt");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(assetSource)!);
            await File.WriteAllTextAsync(assetSource, tamperedAssetContent);
            Directory.CreateDirectory(Path.GetDirectoryName(existingAsset)!);
            await File.WriteAllTextAsync(existingAsset, expectedAssetContent);

            var assemblyPath = CompileCatalogAssemblyToPath(
                root,
                "Asset.Hash.Host",
                CreateCatalogSource(
                    moduleContent,
                    sourcePath: "assets/logo.txt",
                    artifactPath: "assets/logo.txt",
                    assetHash: Sha256(expectedAssetContent)));

            using var progress = new StringWriter();
            var result = await ExecuteAsync(
                assemblyPath,
                sourceRoot,
                outputRoot,
                progress);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Error, "JAZOR_MODULE_ASSET_HASH_MISMATCH", StringComparison.Ordinal);
            StringAssert.Contains(result.Error, "failed during 'collect modules'", StringComparison.Ordinal);
            StringAssert.Contains(progress.ToString(), "collect modules: failed", StringComparison.Ordinal);
            Assert.IsFalse(progress.ToString().Contains("write project: started", StringComparison.Ordinal));
            Assert.AreEqual(expectedAssetContent, await File.ReadAllTextAsync(existingAsset));
            // 写入前校验阶段拒绝：不产生 manifest，既有输出保持原样。
            Assert.IsFalse(File.Exists(Path.Combine(outputRoot, "jazor-manifest.json")));
        }
        finally
        {
            WaitForAssemblyUnload();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [TestMethod]
    [DataRow("components/app.mjs", false)]
    [DataRow("entry.js", false)]
    [DataRow("ssr-entry.js", false)]
    [DataRow("package.json", false)]
    [DataRow("package-lock.json", false)]
    [DataRow("pnpm-lock.yaml", false)]
    [DataRow("entry.js", true)]
    [DataRow("ssr-entry.js", true)]
    public async Task ExecuteAsync_RejectsModuleOrAssetPathThatConflictsWithProjectOutput(string artifactPath, bool moduleCollision)
    {
        const string moduleContent = "export const app = true;";
        const string assetContent = "asset";
        var root = Path.Combine(RepositoryTemp.Root, "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var outputRoot = Path.Combine(root, "out");
        var assetSource = Path.Combine(sourceRoot, "assets", "source.txt");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(assetSource)!);
            await File.WriteAllTextAsync(assetSource, assetContent);

            var source = CreateCatalogSource(
                    moduleContent,
                    sourcePath: "assets/source.txt",
                    artifactPath: moduleCollision ? "assets/output.txt" : artifactPath,
                    assetHash: Sha256(assetContent));
            if (moduleCollision)
                source = source.Replace("components/app.mjs", artifactPath, StringComparison.Ordinal);
            var assemblyPath = CompileCatalogAssemblyToPath(root, "Asset.Path.Host", source);

            var result = await ExecuteAsync(
                assemblyPath,
                sourceRoot,
                outputRoot);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Error, "conflicts with a generated or Emit-owned output file", StringComparison.Ordinal);
            // 冲突在写入前被拒绝，因此输出根未被创建。
            Assert.IsFalse(Directory.Exists(outputRoot));
        }
        finally
        {
            WaitForAssemblyUnload();
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private static Task<EmitPipelineResult> ExecuteAsync(
        string assemblyPath,
        string sourceRoot,
        string outputRoot,
        TextWriter? progress = null)
        => new EmitPipeline(progress).ExecuteAsync(
            new EmitOptions(
                assemblyPath,
                [],
                outputRoot,
                Path.Combine(outputRoot, "jazor-manifest.json"),
                BuildMode.Development,
                sourceRoot,
                [],
                EnableSsr: false));

    private static string CreateCatalogSource(
        string moduleContent,
        string sourcePath,
        string artifactPath,
        string assetHash)
        => """
           namespace Jazor.Generated
           {
               internal static class ModuleCatalog
               {
                   internal const int SchemaVersion = 2;
                   internal const string AssemblyName = "Asset.Host";
                   internal static System.Collections.IEnumerable GetModules() => new object[]
                   {
                       new Module("Asset.App", "Asset.App", "components/app.mjs", {{moduleContent}}, {{moduleHash}}, new string[0])
                   };
                   internal static System.Collections.IEnumerable GetAssets() => new object[]
                   {
                       new Asset("components/app.mjs", {{sourcePath}}, {{artifactPath}}, "static", null, {{assetHash}})
                   };

                   private sealed class Module
                   {
                       public Module(string id, string typeName, string relativePath, string content, string hash, string[] dependencies)
                       { Id = id; TypeName = typeName; RelativePath = relativePath; Content = content; Hash = hash; Dependencies = dependencies; }
                       public string Id { get; } public string TypeName { get; } public string RelativePath { get; }
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
           """
            .Replace("{{moduleContent}}", EscapeCSharp(moduleContent), StringComparison.Ordinal)
            .Replace("{{moduleHash}}", EscapeCSharp(Sha256(moduleContent)), StringComparison.Ordinal)
            .Replace("{{sourcePath}}", EscapeCSharp(sourcePath), StringComparison.Ordinal)
            .Replace("{{artifactPath}}", EscapeCSharp(artifactPath), StringComparison.Ordinal)
            .Replace("{{assetHash}}", EscapeCSharp(assetHash), StringComparison.Ordinal);

    private static string CompileCatalogAssemblyToPath(string directory, string assemblyName, string source)
    {
        Directory.CreateDirectory(directory);
        var syntaxTree = CSharpSyntaxTree.ParseText(
            source,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: assemblyName + ".g.cs");
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
            Net110.References.All.Cast<MetadataReference>(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var assemblyPath = Path.Combine(directory, assemblyName + ".dll");
        using var stream = File.Create(assemblyPath);
        var result = compilation.Emit(stream);
        Assert.IsTrue(
            result.Success,
            string.Join(Environment.NewLine, result.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        return assemblyPath;
    }

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string EscapeCSharp(string value)
        => Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(value, quote: true);

    private static void WaitForAssemblyUnload()
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
