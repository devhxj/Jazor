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
    public async Task ExecuteAsync_ValidatesAssetSourceHashBeforeReusingExistingOutput()
    {
        const string moduleContent = "export const app = true;";
        const string expectedAssetContent = "expected";
        const string tamperedAssetContent = "tampered";
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
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

            var result = await ExecuteAsync(
                assemblyPath,
                sourceRoot,
                outputRoot,
                clean: false);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Error, "JAZOR_MODULE_ASSET_HASH_MISMATCH", StringComparison.Ordinal);
            Assert.AreEqual(expectedAssetContent, await File.ReadAllTextAsync(existingAsset));
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
    public async Task ExecuteAsync_RejectsAssetPathThatConflictsWithGeneratedModule()
    {
        const string moduleContent = "export const app = true;";
        const string assetContent = "asset";
        var root = Path.Combine(Path.GetTempPath(), "Jazor.EmitTest", Guid.NewGuid().ToString("N"));
        var sourceRoot = Path.Combine(root, "source");
        var outputRoot = Path.Combine(root, "out");
        var assetSource = Path.Combine(sourceRoot, "assets", "source.txt");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(assetSource)!);
            await File.WriteAllTextAsync(assetSource, assetContent);

            var assemblyPath = CompileCatalogAssemblyToPath(
                root,
                "Asset.Path.Host",
                CreateCatalogSource(
                    moduleContent,
                    sourcePath: "assets/source.txt",
                    artifactPath: "components/app.mjs",
                    assetHash: Sha256(assetContent)));

            var result = await ExecuteAsync(
                assemblyPath,
                sourceRoot,
                outputRoot,
                clean: true);

            Assert.IsFalse(result.IsSuccess);
            StringAssert.Contains(result.Error, "conflicts with a generated or Emit-owned output file", StringComparison.Ordinal);
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
        bool clean)
        => new EmitPipeline().ExecuteAsync(
            new EmitOptions(
                assemblyPath,
                [],
                outputRoot,
                Path.Combine(outputRoot, "jazor-manifest.json"),
                clean,
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
