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

        StringAssert.Contains(source, "[ECMAScript(\"vue\")]");
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

    [TestMethod]
    public void Vue3_ProjectFile_UsesExternalLibraryMetadataAndPlatformNamespace()
    {
        var repoRoot = ResolveRepositoryRoot();
        var projectPath = Path.Combine(repoRoot, "src", "ECMAScript.Vue3", "ECMAScript.Vue3.csproj");
        var source = File.ReadAllText(projectPath);

        StringAssert.Contains(source, "<PackageId>ECMAScript.Vue3</PackageId>");
        StringAssert.Contains(source, "<RootNamespace>ECMAScript</RootNamespace>");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript\\ECMAScript.csproj\" />");
    }

    [TestMethod]
    public void Vue3_Documentation_IsSplitOutOfPlatformCoreDirectories()
    {
        var repoRoot = ResolveRepositoryRoot();
        var goalRoot = Path.Combine(repoRoot, "docs", "01-目标");
        var planRoot = Path.Combine(repoRoot, "docs", "02-计划");
        var statusRoot = Path.Combine(repoRoot, "docs", "03-完成");

        var vueGoalRoot = Path.Combine(goalRoot, "ecmascript.vue3");
        var vuePlanRoot = Path.Combine(planRoot, "ecmascript.vue3");
        var vueStatusRoot = Path.Combine(statusRoot, "ecmascript.vue3");
        var coreGoalRoot = Path.Combine(goalRoot, "ecmascript");
        var corePlanRoot = Path.Combine(planRoot, "ecmascript");

        Assert.IsTrue(Directory.Exists(vueGoalRoot), $"Vue3 goal docs directory not found: {vueGoalRoot}");
        Assert.IsTrue(Directory.Exists(vuePlanRoot), $"Vue3 plan docs directory not found: {vuePlanRoot}");
        Assert.IsTrue(Directory.Exists(vueStatusRoot), $"Vue3 status docs directory not found: {vueStatusRoot}");
        Assert.IsTrue(File.Exists(Path.Combine(vueGoalRoot, "README.md")), $"Vue3 goal README missing under {vueGoalRoot}");
        Assert.IsTrue(File.Exists(Path.Combine(vuePlanRoot, "README.md")), $"Vue3 plan README missing under {vuePlanRoot}");
        Assert.IsTrue(File.Exists(Path.Combine(vueStatusRoot, "status.md")), $"Vue3 status file missing under {vueStatusRoot}");

        var coreGoalReadme = File.ReadAllText(Path.Combine(coreGoalRoot, "README.md"));
        var corePlanReadme = File.ReadAllText(Path.Combine(corePlanRoot, "README.md"));
        StringAssert.Contains(coreGoalReadme, "ECMAScript.Vue3");
        StringAssert.Contains(coreGoalReadme, "ecmascript.vue3");
        StringAssert.Contains(corePlanReadme, "ECMAScript.Vue3");
        StringAssert.Contains(corePlanReadme, "ecmascript.vue3");

        var misplacedGoalFiles = Directory.GetFiles(coreGoalRoot, "*vue*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .ToArray();
        var misplacedPlanFiles = Directory.GetFiles(corePlanRoot, "*vue*", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .ToArray();

        CollectionAssert.AreEqual(Array.Empty<string>(), misplacedGoalFiles, $"Vue docs should not remain under {coreGoalRoot}");
        CollectionAssert.AreEqual(Array.Empty<string>(), misplacedPlanFiles, $"Vue docs should not remain under {corePlanRoot}");
    }

    [TestMethod]
    public void Vue3_Source_DoesNotFlowBackIntoPlatformCoreModule()
    {
        var repoRoot = ResolveRepositoryRoot();
        var ecmascriptRoot = Path.Combine(repoRoot, "src", "ECMAScript");
        var misplacedVueFiles = Directory.GetFiles(ecmascriptRoot, "*Vue3*.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(ecmascriptRoot, path))
            .ToArray();

        CollectionAssert.AreEqual(Array.Empty<string>(), misplacedVueFiles, $"Vue3 source files should not flow back into {ecmascriptRoot}");
    }

    [TestMethod]
    public void Vue3_SafeErasedValueUnions_UseNativeUnionKeyword()
    {
        var repoRoot = ResolveRepositoryRoot();
        var propsSource = File.ReadAllText(Path.Combine(repoRoot, "src", "ECMAScript.Vue3", "Types", "Vue3.Types.Props.cs"));
        var unionSource = File.ReadAllText(Path.Combine(repoRoot, "src", "ECMAScript.Vue3", "Types", "Vue3.Types.Unions.cs"));

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
