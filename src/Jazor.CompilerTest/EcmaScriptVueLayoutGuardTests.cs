using System.Text.RegularExpressions;

namespace Jazor.ComplierTest;

[TestClass]
public sealed class EcmaScriptVueLayoutGuardTests
{
    [TestMethod]
    public void Vue_ModuleLayout_UsesApiAndTypesSubdirectories()
    {
        var repoRoot = ResolveRepositoryRoot();
        var vueRoot = Path.Combine(repoRoot, "src", "ECMAScript.Vue");
        var apiDir = Path.Combine(vueRoot, "Api");
        var typesDir = Path.Combine(vueRoot, "Types");

        Assert.IsTrue(Directory.Exists(vueRoot), $"Vue module directory not found: {vueRoot}");
        Assert.IsTrue(Directory.Exists(apiDir), $"Vue API directory not found: {apiDir}");
        Assert.IsTrue(Directory.Exists(typesDir), $"Vue Types directory not found: {typesDir}");

        var apiFiles = Directory.GetFiles(apiDir, "Vue.Api*.cs", SearchOption.TopDirectoryOnly);
        var typeFiles = Directory.GetFiles(typesDir, "Vue.Types.*.cs", SearchOption.TopDirectoryOnly);
        var rootApiFiles = Directory.GetFiles(vueRoot, "Vue.Api*.cs", SearchOption.TopDirectoryOnly);
        var rootTypeFiles = Directory.GetFiles(vueRoot, "Vue.Types.*.cs", SearchOption.TopDirectoryOnly);

        Assert.IsTrue(apiFiles.Length >= 5, $"Expected at least 5 Vue API partial files under {apiDir}, actual: {apiFiles.Length}");
        Assert.IsTrue(typeFiles.Length >= 7, $"Expected at least 7 Vue Types partial files under {typesDir}, actual: {typeFiles.Length}");
        Assert.AreEqual(0, rootApiFiles.Length, $"Vue API partial files should not stay in module root: {string.Join(", ", rootApiFiles.Select(Path.GetFileName))}");
        Assert.AreEqual(0, rootTypeFiles.Length, $"Vue Types partial files should not stay in module root: {string.Join(", ", rootTypeFiles.Select(Path.GetFileName))}");

        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue.Api.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue.Api.Render.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue.Api.Reactivity.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue.Api.Composition.cs")));
        Assert.IsTrue(File.Exists(Path.Combine(apiDir, "Vue.Api.Lifecycle.cs")));
    }

    [TestMethod]
    public void Vue_ShellFile_RemainsHostAttributeEntryPointOnly()
    {
        var repoRoot = ResolveRepositoryRoot();
        var shellPath = Path.Combine(repoRoot, "src", "ECMAScript.Vue", "Vue.cs");
        var source = File.ReadAllText(shellPath);

        StringAssert.Contains(source, "[ECMAScript(\"vue\")]");
        StringAssert.Contains(source, "[Description(\"@#\")]");
        StringAssert.Contains(source, "public static partial class Vue");
        Assert.IsFalse(source.Contains("public extern static", StringComparison.Ordinal), "Vue shell file should not contain static API members.");
        Assert.IsFalse(source.Contains("public interface IVueComponent", StringComparison.Ordinal), "Vue nested type contracts should stay in Types/ partial files.");
        Assert.IsFalse(source.Contains("public abstract class VueApp", StringComparison.Ordinal), "VueApp runtime-shape type should stay in Types/ partial files.");

        var match = Regex.Match(
            source,
            @"public\s+static\s+partial\s+class\s+Vue\s*\{(?<body>[\s\S]*)\}\s*$",
            RegexOptions.Compiled);

        Assert.IsTrue(match.Success, "Cannot locate Vue shell class body.");

        var body = match.Groups["body"].Value;
        var nonCommentLines = body
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static line => line.Trim())
            .Where(static line => !line.StartsWith("//", StringComparison.Ordinal))
            .ToArray();

        Assert.AreEqual(
            0,
            nonCommentLines.Length,
            $"Vue shell class should only keep attribute entrypoint semantics. Unexpected content: {string.Join(" | ", nonCommentLines)}");
    }

    [TestMethod]
    public void Vue_ProjectFile_UsesExternalLibraryMetadataAndPlatformNamespace()
    {
        var repoRoot = ResolveRepositoryRoot();
        var projectPath = Path.Combine(repoRoot, "src", "ECMAScript.Vue", "ECMAScript.Vue.csproj");
        var source = File.ReadAllText(projectPath);

        StringAssert.Contains(source, "<PackageId>ECMAScript.Vue</PackageId>");
        StringAssert.Contains(source, "<RootNamespace>ECMAScript</RootNamespace>");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript\\ECMAScript.csproj\" />");
    }

    [TestMethod]
    public void Vue_Documentation_UsesCurrentPlatformAndBindingsGuide()
    {
        var repoRoot = ResolveRepositoryRoot();
        var guidePath = Path.Combine(repoRoot, "docs", "02-architecture", "platform-and-bindings.md");

        Assert.IsTrue(File.Exists(guidePath), $"Platform and bindings guide not found: {guidePath}");
        var guide = File.ReadAllText(guidePath);
        StringAssert.Contains(guide, "ECMAScript.Vue");
        StringAssert.Contains(guide, "ECMAScript.VueRoute");
        StringAssert.Contains(guide, "ECMAScript.Pinia");

        foreach (var legacyDirectory in new[] { "01-目标", "02-计划", "03-完成" })
        {
            Assert.IsFalse(
                Directory.Exists(Path.Combine(repoRoot, "docs", legacyDirectory)),
                $"Legacy documentation directory should not exist: {legacyDirectory}");
        }
    }

    [TestMethod]
    public void Vue_Source_DoesNotFlowBackIntoPlatformCoreModule()
    {
        var repoRoot = ResolveRepositoryRoot();
        var ecmascriptRoot = Path.Combine(repoRoot, "src", "ECMAScript");
        var misplacedVueFiles = Directory.GetFiles(ecmascriptRoot, "*Vue*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(ecmascriptRoot, path))
            .ToArray();

        CollectionAssert.AreEqual(Array.Empty<string>(), misplacedVueFiles, $"Vue source files should not flow back into {ecmascriptRoot}");
    }

    [TestMethod]
    public void Vue_SafeErasedValueUnions_UseNativeUnionKeyword()
    {
        var repoRoot = ResolveRepositoryRoot();
        var propsSource = File.ReadAllText(Path.Combine(repoRoot, "src", "ECMAScript.Vue", "Types", "Vue.Types.Props.cs"));
        var unionSource = File.ReadAllText(Path.Combine(repoRoot, "src", "ECMAScript.Vue", "Types", "Vue.Types.Unions.cs"));

        AssertUsesNativeUnion(propsSource, "VueNamesOrOptions");
        AssertUsesNativeUnion(unionSource, "VueInjectFrom");
        AssertUsesNativeUnion(unionSource, "VuePropDeclaration");
        AssertUsesNativeUnion(unionSource, "VueClassValue");
        AssertUsesNativeUnion(unionSource, "VueBooleanStringValue");
        AssertUsesNativeUnion(unionSource, "VueStringComponentValue");
        AssertUsesNativeUnion(unionSource, "VueStringNumberValue");
        AssertUsesNativeUnion(unionSource, "VueWatchDeep");
        AssertUsesNativeUnion(unionSource, "VueTransitionDurationValue");
        AssertUsesNativeUnion(unionSource, "VueKeepAliveMatch");
        AssertUsesNativeUnion(unionSource, "VueIntStringValue");
        AssertUsesNativeUnion(unionSource, "VueStyleValue");
        AssertUsesNativeUnion(unionSource, "VueStyleValues");
        AssertUsesNativeUnion(unionSource, "VueTeleportTarget");
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

    private static void AssertUsesNativeUnion(string source, string typeName)
    {
        Assert.IsTrue(
            System.Text.RegularExpressions.Regex.IsMatch(source, $@"\bpublic\s+readonly\s+union\s+{System.Text.RegularExpressions.Regex.Escape(typeName)}\b"),
            $"{typeName} must be declared with the C# native union keyword.");
        Assert.IsFalse(
            System.Text.RegularExpressions.Regex.IsMatch(source, $@"\bpublic\s+readonly\s+struct\s+{System.Text.RegularExpressions.Regex.Escape(typeName)}\b"),
            $"{typeName} must stay on the C# native union keyword path.");
    }
}
