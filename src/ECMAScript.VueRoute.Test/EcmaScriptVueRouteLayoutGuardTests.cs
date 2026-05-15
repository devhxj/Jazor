using System.Text.RegularExpressions;

namespace ECMAScriptVueRouteTest;

[TestClass]
public sealed class EcmaScriptVueRouteLayoutGuardTests
{
    [TestMethod]
    public void VueRoute_ModuleLayout_UsesApiAndTypesSubdirectories()
    {
        var repoRoot = ResolveRepositoryRoot();
        var moduleRoot = System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute");
        var apiDir = System.IO.Path.Combine(moduleRoot, "Api");
        var typesDir = System.IO.Path.Combine(moduleRoot, "Types");

        Assert.IsTrue(System.IO.Directory.Exists(moduleRoot), $"VueRoute module directory not found: {moduleRoot}");
        Assert.IsTrue(System.IO.Directory.Exists(apiDir), $"VueRoute API directory not found: {apiDir}");
        Assert.IsTrue(System.IO.Directory.Exists(typesDir), $"VueRoute Types directory not found: {typesDir}");

        var apiFiles = System.IO.Directory.GetFiles(apiDir, "VueRoute.Api*.cs", SearchOption.TopDirectoryOnly);
        var typeFiles = System.IO.Directory.GetFiles(typesDir, "VueRoute.Types*.cs", SearchOption.TopDirectoryOnly);
        var rootApiFiles = System.IO.Directory.GetFiles(moduleRoot, "VueRoute.Api*.cs", SearchOption.TopDirectoryOnly);
        var rootTypeFiles = System.IO.Directory.GetFiles(moduleRoot, "VueRoute.Types*.cs", SearchOption.TopDirectoryOnly);

        Assert.IsTrue(apiFiles.Length >= 1, $"Expected VueRoute API partial files under {apiDir}, actual: {apiFiles.Length}");
        Assert.IsTrue(typeFiles.Length >= 2, $"Expected VueRoute type partial files under {typesDir}, actual: {typeFiles.Length}");
        Assert.AreEqual(0, rootApiFiles.Length, $"VueRoute API partial files should not stay in module root: {string.Join(", ", rootApiFiles.Select(Path.GetFileName))}");
        Assert.AreEqual(0, rootTypeFiles.Length, $"VueRoute type partial files should not stay in module root: {string.Join(", ", rootTypeFiles.Select(Path.GetFileName))}");
    }

    [TestMethod]
    public void VueRoute_ShellFile_RemainsHostAttributeEntryPointOnly()
    {
        var repoRoot = ResolveRepositoryRoot();
        var shellPath = System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "VueRoute.cs");
        var source = System.IO.File.ReadAllText(shellPath);

        StringAssert.Contains(source, "[ECMAScript(\"npm:vue-router@4\")]");
        StringAssert.Contains(source, "[Description(\"@#\")]");
        StringAssert.Contains(source, "public static partial class VueRoute");
        Assert.IsFalse(source.Contains("public extern static", StringComparison.Ordinal), "VueRoute shell file should not contain static API members.");

        var match = Regex.Match(
            source,
            @"public\s+static\s+partial\s+class\s+VueRoute\s*\{(?<body>[\s\S]*)\}\s*$",
            RegexOptions.Compiled);

        Assert.IsTrue(match.Success, "Cannot locate VueRoute shell class body.");

        var body = match.Groups["body"].Value;
        var nonCommentLines = body
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(static line => line.Trim())
            .Where(static line => !line.StartsWith("//", StringComparison.Ordinal))
            .ToArray();

        Assert.AreEqual(
            0,
            nonCommentLines.Length,
            $"VueRoute shell class should only keep attribute entrypoint semantics. Unexpected content: {string.Join(" | ", nonCommentLines)}");
    }

    [TestMethod]
    public void VueRoute_ProjectFile_UsesExternalLibraryMetadataAndPlatformNamespace()
    {
        var repoRoot = ResolveRepositoryRoot();
        var projectPath = System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "ECMAScript.VueRoute.csproj");
        var source = System.IO.File.ReadAllText(projectPath);

        StringAssert.Contains(source, "<PackageId>ECMAScript.VueRoute</PackageId>");
        StringAssert.Contains(source, "<RootNamespace>ECMAScript</RootNamespace>");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript\\ECMAScript.csproj\" />");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript.Vue3\\ECMAScript.Vue3.csproj\" />");
    }

    [TestMethod]
    public void VueRoute_StandardTestScript_IncludesDedicatedTargetAndDefaultLane()
    {
        var repoRoot = ResolveRepositoryRoot();
        var scriptPath = System.IO.Path.Combine(repoRoot, "scripts", "csharp", "test-dotnet.cs");
        var source = System.IO.File.ReadAllText(scriptPath);

        StringAssert.Contains(source, "\"vueroute\"");
        StringAssert.Contains(source, "var vueRouteTestProject = Path.Combine(repoRoot, \"src\", \"ECMAScript.VueRoute.Test\", \"ECMAScript.VueRoute.Test.csproj\");");
        StringAssert.Contains(source, "\"vueroute\" => new[] { vueRouteTestProject }");
        StringAssert.Contains(source, "vueRouteTestProject,");
    }

    [TestMethod]
    public void VueRoute_ProjectFile_StaysIndependentlyPackable_WithoutJazorBundling()
    {
        var repoRoot = ResolveRepositoryRoot();
        var projectPath = System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "ECMAScript.VueRoute.csproj");
        var source = System.IO.File.ReadAllText(projectPath);
        var nuspecPath = System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "ECMAScript.VueRoute.nuspec");
        var nuspec = System.IO.File.ReadAllText(nuspecPath);

        StringAssert.Contains(source, "<IsPackable>true</IsPackable>");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript\\ECMAScript.csproj\" />");
        StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript.Vue3\\ECMAScript.Vue3.csproj\" />");
        StringAssert.Contains(source, "<NuspecFile>ECMAScript.VueRoute.nuspec</NuspecFile>");
        StringAssert.Contains(nuspec, "<dependency id=\"Jazor\" version=\"$dependencyVersion$\" />");

        var jazorPackageProject = System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj"));
        Assert.IsFalse(
            jazorPackageProject.Contains("ECMAScript.VueRoute\\bin\\$(Configuration)\\net11.0\\ECMAScript.VueRoute.dll", StringComparison.Ordinal),
            "Jazor package should not bundle ECMAScript.VueRoute anymore.");
        Assert.IsFalse(
            jazorPackageProject.Contains("..\\ECMAScript.VueRoute\\ECMAScript.VueRoute.csproj", StringComparison.Ordinal),
            "Jazor package artifact preparation should not build ECMAScript.VueRoute as a bundled payload anymore.");
    }

    [TestMethod]
    public void VueRoute_TestProject_HasReadmeAndCoverageSettings()
    {
        var repoRoot = ResolveRepositoryRoot();
        var testProjectRoot = System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute.Test");
        var readmePath = System.IO.Path.Combine(testProjectRoot, "README.md");
        var coverletPath = System.IO.Path.Combine(testProjectRoot, "coverlet.runsettings");

        Assert.IsTrue(System.IO.File.Exists(readmePath), $"VueRoute test project README not found: {readmePath}");
        Assert.IsTrue(System.IO.File.Exists(coverletPath), $"VueRoute test project coverlet settings not found: {coverletPath}");

        var readme = System.IO.File.ReadAllText(readmePath);
        var coverlet = System.IO.File.ReadAllText(coverletPath);

        StringAssert.Contains(readme, "# ECMAScript.VueRoute.Test");
        StringAssert.Contains(readme, "dotnet run --file ./scripts/csharp/test-dotnet.cs -- --project vueroute");
        StringAssert.Contains(coverlet, "<LineMinimum>85</LineMinimum>");
        StringAssert.Contains(coverlet, "<BranchMinimum>80</BranchMinimum>");
    }

    [TestMethod]
    public void VueRoute_SampleSmokeScript_UsesDenoConsumerPath_AndGuardsNoLegacyConsumerResidue()
    {
        var repoRoot = ResolveRepositoryRoot();
        var consumerRoot = Path.Combine(repoRoot, "samples", "ECMAScript.VueRoute.MemorySmoke", "vueroute-consumer");
        var denoConfigPath = Path.Combine(consumerRoot, "deno.json");
        var scriptPath = Path.Combine(repoRoot, "samples", "ECMAScript.VueRoute.MemorySmoke", "verify-smoke.cs");
        var legacyPackageJsonPath = Path.Combine(consumerRoot, "package.json");
        var legacyConfigPath = Path.Combine(consumerRoot, "vite.config.js");
        var denoConfig = File.ReadAllText(denoConfigPath);
        var source = File.ReadAllText(scriptPath);

        StringAssert.Contains(denoConfig, "\"build\": \"deno run -A scripts/build.ts\"");
        StringAssert.Contains(denoConfig, "\"test\": \"deno run -A scripts/test.ts\"");
        StringAssert.Contains(source, "ResolveDenoExecutable(repoRoot, options)");
        StringAssert.Contains(source, "RunProcessAsync(denoExePath, [\"task\", \"build\"]");
        StringAssert.Contains(source, "src/vueroute.generated.test.js");
        StringAssert.Contains(source, "src/vueroute.runtime.test.js");
        StringAssert.Contains(source, "src/vueroute.generated.dom.test.js");
        StringAssert.Contains(source, "ECMAScript.VueRoute sample smoke verification passed.");
        Assert.IsFalse(source.Contains("vite", StringComparison.OrdinalIgnoreCase), "VueRoute smoke verification script should not depend on legacy frontend bundler residue anymore.");
        Assert.IsFalse(source.Contains("vitest", StringComparison.OrdinalIgnoreCase), "VueRoute smoke verification script should not depend on legacy test runner residue anymore.");
        Assert.IsFalse(File.Exists(legacyPackageJsonPath), $"Legacy package manifest should not remain in the Deno consumer: {legacyPackageJsonPath}");
        Assert.IsFalse(File.Exists(legacyConfigPath), $"Legacy config should not remain in the Deno consumer: {legacyConfigPath}");
    }

    [TestMethod]
    public void VueRoute_SampleDirectoryBuildProps_ImportsRepoRootBuildProps()
    {
        var repoRoot = ResolveRepositoryRoot();
        var propsPath = System.IO.Path.Combine(repoRoot, "samples", "ECMAScript.VueRoute.MemorySmoke", "Directory.Build.props");
        var source = System.IO.File.ReadAllText(propsPath);

        StringAssert.Contains(source, "<Import Project=\"..\\..\\Directory.Build.props\" />");
    }

    [TestMethod]
    public void VueRoute_SafeErasedValueUnions_UseNativeUnionKeyword()
    {
        var repoRoot = ResolveRepositoryRoot();
        var unionSource = System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "Types", "VueRoute.Types.Unions.cs"));
        var typesSource = System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "src", "ECMAScript.VueRoute", "Types", "VueRoute.Types.cs"));

        AssertUsesNativeUnion(unionSource, "RouteRecordName");
        AssertUsesNativeUnion(unionSource, "RouteRecordAlias");
        AssertUsesNativeUnion(unionSource, "RouteLocationRaw");
        AssertUsesNativeUnion(unionSource, "RouterViewDepthValue");
        AssertUsesNativeUnion(unionSource, "HistoryStateValue");
        AssertUsesNativeUnion(unionSource, "RouteRecordRaw");
        AssertUsesNativeUnion(unionSource, "MatcherLocationRaw");
        AssertUsesNativeUnion(unionSource, "RouteParam");
        AssertUsesNativeUnion(unionSource, "RouteParamRaw");
        AssertUsesNativeUnion(unionSource, "LocationQueryValue");
        AssertUsesNativeUnion(unionSource, "LocationQueryValueRaw");
        AssertUsesNativeUnion(typesSource, "ScrollPositionTarget");
    }

    private static string ResolveRepositoryRoot()
    {
        var current = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(current.FullName, "Jazor.slnx")))
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
