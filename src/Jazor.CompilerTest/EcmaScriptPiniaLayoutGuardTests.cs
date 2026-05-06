using System.Text.RegularExpressions;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class EcmaScriptPiniaLayoutGuardTests
{
    [TestMethod]
    public void Pinia_ModuleLayout_UsesApiAndTypesSubdirectories()
    {
        var repoRoot = ResolveRepositoryRoot();
        var piniaRoot = Path.Combine(repoRoot, "src", "ECMAScript.Pinia");
        var apiDir = Path.Combine(piniaRoot, "Api");
        var typesDir = Path.Combine(piniaRoot, "Types");

        Assert.IsTrue(Directory.Exists(piniaRoot), $"Pinia module directory not found: {piniaRoot}");
        Assert.IsTrue(Directory.Exists(apiDir), $"Pinia API directory not found: {apiDir}");
        Assert.IsTrue(Directory.Exists(typesDir), $"Pinia Types directory not found: {typesDir}");

        var apiFiles = Directory.GetFiles(apiDir, "Pinia.Api*.cs", SearchOption.TopDirectoryOnly);
        var typeFiles = Directory.GetFiles(typesDir, "Pinia.Types.*.cs", SearchOption.TopDirectoryOnly);
        var rootApiFiles = Directory.GetFiles(piniaRoot, "Pinia.Api*.cs", SearchOption.TopDirectoryOnly);
        var rootTypeFiles = Directory.GetFiles(piniaRoot, "Pinia.Types.*.cs", SearchOption.TopDirectoryOnly);

        Assert.IsTrue(apiFiles.Length >= 1, $"Expected Pinia API partial files under {apiDir}, actual: {apiFiles.Length}");
        Assert.IsTrue(typeFiles.Length >= 2, $"Expected Pinia type partial files under {typesDir}, actual: {typeFiles.Length}");
        Assert.AreEqual(0, rootApiFiles.Length, $"Pinia API partial files should not stay in module root: {string.Join(", ", rootApiFiles.Select(Path.GetFileName))}");
        Assert.AreEqual(0, rootTypeFiles.Length, $"Pinia type partial files should not stay in module root: {string.Join(", ", rootTypeFiles.Select(Path.GetFileName))}");

        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Pinia.Api.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(typesDir, "Pinia.Types.Core.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(typesDir, "Pinia.Types.Store.cs")));
    }

    [TestMethod]
    public void Pinia_ShellFile_RemainsHostAttributeEntryPointOnly()
    {
        var repoRoot = ResolveRepositoryRoot();
        var shellPath = Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "Pinia.cs");
        var source = File.ReadAllText(shellPath);

        StringAssert.Contains(source, "[ECMAScript(\"pinia\")]");
        StringAssert.Contains(source, "[Description(\"@#\")]");
        StringAssert.Contains(source, "public static partial class Pinia");
        Assert.IsFalse(source.Contains("public extern static", StringComparison.Ordinal), "Pinia shell file should not contain static API members.");

        var match = Regex.Match(
            source,
            @"public\s+static\s+partial\s+class\s+Pinia\s*\{(?<body>[\s\S]*)\}\s*$",
            RegexOptions.Compiled);

        Assert.IsTrue(match.Success, "Cannot locate Pinia shell class body.");

        var body = match.Groups["body"].Value;
        var nonCommentLines = body
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static line => line.Trim())
            .Where(static line => !line.StartsWith("//", StringComparison.Ordinal))
            .ToArray();

        Assert.AreEqual(
            0,
            nonCommentLines.Length,
            $"Pinia shell class should only keep attribute entrypoint semantics. Unexpected content: {string.Join(" | ", nonCommentLines)}");
    }

    [TestMethod]
    public void Pinia_ProjectFile_UsesExternalLibraryMetadataAndPlatformNamespace()
    {
        var repoRoot = ResolveRepositoryRoot();
        var projectPath = Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "ECMAScript.Pinia.csproj");
        var source = File.ReadAllText(projectPath);

        StringAssert.Contains(source, "<PackageId>ECMAScript.Pinia</PackageId>");
        StringAssert.Contains(source, "<RootNamespace>ECMAScript</RootNamespace>");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript\\ECMAScript.csproj\" />");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript.Vue3\\ECMAScript.Vue3.csproj\" />");
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
                return current.FullName;
            current = current.Parent;
        }

        throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
    }
}
