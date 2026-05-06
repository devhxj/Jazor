using System.Text.RegularExpressions;

namespace ECMAScript.PiniaTestingTests;

[TestClass]
public sealed class EcmaScriptPiniaTestingLayoutGuardTests
{
	[TestMethod]
	public void PiniaTesting_ModuleLayout_UsesApiAndTypesSubdirectories()
	{
		var repoRoot = ResolveRepositoryRoot();
		var moduleRoot = Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing");
		var apiDir = Path.Combine(moduleRoot, "Api");
		var typesDir = Path.Combine(moduleRoot, "Types");

		Assert.IsTrue(Directory.Exists(moduleRoot), $"PiniaTesting module directory not found: {moduleRoot}");
		Assert.IsTrue(Directory.Exists(apiDir), $"PiniaTesting API directory not found: {apiDir}");
		Assert.IsTrue(Directory.Exists(typesDir), $"PiniaTesting Types directory not found: {typesDir}");

		var apiFiles = Directory.GetFiles(apiDir, "PiniaTesting.Api*.cs", SearchOption.TopDirectoryOnly);
		var typeFiles = Directory.GetFiles(typesDir, "PiniaTesting.Types*.cs", SearchOption.TopDirectoryOnly);
		var rootApiFiles = Directory.GetFiles(moduleRoot, "PiniaTesting.Api*.cs", SearchOption.TopDirectoryOnly);
		var rootTypeFiles = Directory.GetFiles(moduleRoot, "PiniaTesting.Types*.cs", SearchOption.TopDirectoryOnly);

		Assert.IsTrue(apiFiles.Length >= 1, $"Expected PiniaTesting API partial files under {apiDir}, actual: {apiFiles.Length}");
		Assert.IsTrue(typeFiles.Length >= 1, $"Expected PiniaTesting type partial files under {typesDir}, actual: {typeFiles.Length}");
		Assert.AreEqual(0, rootApiFiles.Length, $"PiniaTesting API partial files should not stay in module root: {string.Join(", ", rootApiFiles.Select(Path.GetFileName))}");
		Assert.AreEqual(0, rootTypeFiles.Length, $"PiniaTesting type partial files should not stay in module root: {string.Join(", ", rootTypeFiles.Select(Path.GetFileName))}");
	}

	[TestMethod]
	public void PiniaTesting_ShellFile_RemainsHostAttributeEntryPointOnly()
	{
		var repoRoot = ResolveRepositoryRoot();
		var shellPath = Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing", "PiniaTesting.cs");
		var source = System.IO.File.ReadAllText(shellPath);

		StringAssert.Contains(source, "[ECMAScript(\"@pinia/testing\")]");
		StringAssert.Contains(source, "[Description(\"@#\")]");
		StringAssert.Contains(source, "public static partial class PiniaTesting");
		Assert.IsFalse(source.Contains("public extern static", StringComparison.Ordinal), "PiniaTesting shell file should not contain static API members.");

		var match = Regex.Match(
			source,
			@"public\s+static\s+partial\s+class\s+PiniaTesting\s*\{(?<body>[\s\S]*)\}\s*$",
			RegexOptions.Compiled);

		Assert.IsTrue(match.Success, "Cannot locate PiniaTesting shell class body.");

		var body = match.Groups["body"].Value;
		var nonCommentLines = body
			.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
			.Select(static line => line.Trim())
			.Where(static line => !line.StartsWith("//", StringComparison.Ordinal))
			.ToArray();

		Assert.AreEqual(
			0,
			nonCommentLines.Length,
			$"PiniaTesting shell class should only keep attribute entrypoint semantics. Unexpected content: {string.Join(" | ", nonCommentLines)}");
	}

	[TestMethod]
	public void PiniaTesting_ProjectFile_UsesExternalLibraryMetadataAndPlatformNamespace()
	{
		var repoRoot = ResolveRepositoryRoot();
		var projectPath = Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing", "ECMAScript.Pinia.Testing.csproj");
		var source = System.IO.File.ReadAllText(projectPath);

		StringAssert.Contains(source, "<PackageId>ECMAScript.Pinia.Testing</PackageId>");
		StringAssert.Contains(source, "<RootNamespace>ECMAScript</RootNamespace>");
		StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript\\ECMAScript.csproj\" />");
		StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript.Vue3\\ECMAScript.Vue3.csproj\" />");
		StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript.Pinia\\ECMAScript.Pinia.csproj\" />");
	}

	[TestMethod]
	public void PiniaTesting_StandardTestScript_IncludesDedicatedTargetAndDefaultLane()
	{
		var repoRoot = ResolveRepositoryRoot();
		var scriptPath = Path.Combine(repoRoot, "scripts", "test-dotnet.ps1");
		var source = System.IO.File.ReadAllText(scriptPath);

		StringAssert.Contains(source, "\"pinia-testing\"");
		StringAssert.Contains(source, "src\\ECMAScript.Pinia.Testing.Test\\ECMAScript.Pinia.Testing.Test.csproj");
		StringAssert.Contains(source, "\"pinia-testing\" { $piniaTestingTestProject }");
		StringAssert.Contains(source, "default { $compilerTestProject, $clrTestProject, $piniaTestProject, $piniaTestingTestProject, $vueRouteTestProject, $razorVueTestProject, $joltTestProject, $emitTestProject }");
	}

	[TestMethod]
	public void PiniaTesting_JazorPackageProject_IncludesLibraryArtifactsAndBuildTarget()
	{
		var repoRoot = ResolveRepositoryRoot();
		var projectPath = Path.Combine(repoRoot, "src", "Jazor", "Jazor.csproj");
		var source = System.IO.File.ReadAllText(projectPath);

		StringAssert.Contains(source, "..\\ECMAScript.Pinia.Testing\\bin\\$(Configuration)\\net10.0\\ECMAScript.Pinia.Testing.dll");
		StringAssert.Contains(source, "..\\ECMAScript.Pinia.Testing\\bin\\$(Configuration)\\net10.0\\ECMAScript.Pinia.Testing.pdb");
		StringAssert.Contains(source, "..\\ECMAScript.Pinia.Testing\\ECMAScript.Pinia.Testing.csproj");
	}

	[TestMethod]
	public void PiniaTesting_TestProject_HasReadmeAndCoverageSettings()
	{
		var repoRoot = ResolveRepositoryRoot();
		var testProjectRoot = Path.Combine(repoRoot, "src", "ECMAScript.Pinia.Testing.Test");
		var readmePath = Path.Combine(testProjectRoot, "README.md");
		var coverletPath = Path.Combine(testProjectRoot, "coverlet.runsettings");

		Assert.IsTrue(System.IO.File.Exists(readmePath), $"PiniaTesting test project README not found: {readmePath}");
		Assert.IsTrue(System.IO.File.Exists(coverletPath), $"PiniaTesting test project coverlet settings not found: {coverletPath}");

		var readme = System.IO.File.ReadAllText(readmePath);
		var coverlet = System.IO.File.ReadAllText(coverletPath);

		StringAssert.Contains(readme, "# ECMAScript.Pinia.Testing.Test");
		StringAssert.Contains(readme, "pwsh ./scripts/test-dotnet.ps1 -Project pinia-testing");
		StringAssert.Contains(coverlet, "<LineMinimum>85</LineMinimum>");
		StringAssert.Contains(coverlet, "<BranchMinimum>80</BranchMinimum>");
	}

	private static string ResolveRepositoryRoot()
	{
		var current = new DirectoryInfo(AppContext.BaseDirectory);
		while (current is not null)
		{
			if (System.IO.File.Exists(Path.Combine(current.FullName, "Jazor.slnx")))
				return current.FullName;
			current = current.Parent;
		}

		throw new InvalidOperationException("Cannot locate repository root (Jazor.slnx).");
	}
}
