using System.Text.RegularExpressions;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class EcmaScriptVue3LayoutGuardTests
{
    [TestMethod]
    public void Vue3_ModuleLayout_UsesApiAndTypesSubdirectories()
    {
        var repoRoot = ResolveRepositoryRoot();
        var vue3Root = Path.Combine(repoRoot, "src", "ECMAScript.Vue3");
        var apiDir = Path.Combine(vue3Root, "Api");
        var typesDir = Path.Combine(vue3Root, "Types");

        Assert.IsTrue(Directory.Exists(vue3Root), $"Vue3 module directory not found: {vue3Root}");
        Assert.IsTrue(Directory.Exists(apiDir), $"Vue3 API directory not found: {apiDir}");
        Assert.IsTrue(Directory.Exists(typesDir), $"Vue3 Types directory not found: {typesDir}");

        var apiFiles = Directory.GetFiles(apiDir, "Vue3.Api*.cs", SearchOption.TopDirectoryOnly);
        var typeFiles = Directory.GetFiles(typesDir, "Vue3.Types.*.cs", SearchOption.TopDirectoryOnly);
        var rootApiFiles = Directory.GetFiles(vue3Root, "Vue3.Api*.cs", SearchOption.TopDirectoryOnly);
        var rootTypeFiles = Directory.GetFiles(vue3Root, "Vue3.Types.*.cs", SearchOption.TopDirectoryOnly);

        Assert.IsTrue(apiFiles.Length >= 5, $"Expected at least 5 Vue3 API partial files under {apiDir}, actual: {apiFiles.Length}");
        Assert.IsTrue(typeFiles.Length >= 7, $"Expected at least 7 Vue3 Types partial files under {typesDir}, actual: {typeFiles.Length}");
        Assert.AreEqual(0, rootApiFiles.Length, $"Vue3 API partial files should not stay in module root: {string.Join(", ", rootApiFiles.Select(Path.GetFileName))}");
        Assert.AreEqual(0, rootTypeFiles.Length, $"Vue3 Types partial files should not stay in module root: {string.Join(", ", rootTypeFiles.Select(Path.GetFileName))}");

        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue3.Api.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue3.Api.Render.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue3.Api.Reactivity.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue3.Api.Composition.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue3.Api.Lifecycle.cs")));
    }

    [TestMethod]
    public void Vue3_ShellFile_RemainsHostAttributeEntryPointOnly()
    {
        var repoRoot = ResolveRepositoryRoot();
        var shellPath = Path.Combine(repoRoot, "src", "ECMAScript.Vue3", "Vue3.cs");
        var source = File.ReadAllText(shellPath);

        StringAssert.Contains(source, "[ECMAScript(\"npm:vue@3\")]");
        StringAssert.Contains(source, "[Description(\"@#\")]");
        StringAssert.Contains(source, "public static partial class Vue3");
        Assert.IsFalse(source.Contains("public extern static", StringComparison.Ordinal), "Vue3 shell file should not contain static API members.");
        Assert.IsFalse(source.Contains("public interface IVueComponent", StringComparison.Ordinal), "Vue3 nested type contracts should stay in Types/ partial files.");
        Assert.IsFalse(source.Contains("public abstract class VueApp", StringComparison.Ordinal), "VueApp runtime-shape type should stay in Types/ partial files.");

        var match = Regex.Match(
            source,
            @"public\s+static\s+partial\s+class\s+Vue3\s*\{(?<body>[\s\S]*)\}\s*$",
            RegexOptions.Compiled);

        Assert.IsTrue(match.Success, "Cannot locate Vue3 shell class body.");

        var body = match.Groups["body"].Value;
        var nonCommentLines = body
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static line => line.Trim())
            .Where(static line => !line.StartsWith("//", StringComparison.Ordinal))
            .ToArray();

        Assert.AreEqual(
            0,
            nonCommentLines.Length,
            $"Vue3 shell class should only keep attribute entrypoint semantics. Unexpected content: {string.Join(" | ", nonCommentLines)}");
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

