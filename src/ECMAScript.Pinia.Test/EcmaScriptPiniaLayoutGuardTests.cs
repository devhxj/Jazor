using System.Text.RegularExpressions;

namespace ECMAScript.PiniaTests;

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
		Assert.IsTrue(typeFiles.Length >= 3, $"Expected Pinia type partial files under {typesDir}, actual: {typeFiles.Length}");
		Assert.AreEqual(0, rootApiFiles.Length, $"Pinia API partial files should not stay in module root: {string.Join(", ", rootApiFiles.Select(Path.GetFileName))}");
		Assert.AreEqual(0, rootTypeFiles.Length, $"Pinia type partial files should not stay in module root: {string.Join(", ", rootTypeFiles.Select(Path.GetFileName))}");

		Assert.IsTrue(System.IO.File.Exists(Path.Combine(apiDir, "Pinia.Api.cs")));
		Assert.IsTrue(System.IO.File.Exists(Path.Combine(typesDir, "Pinia.Types.Core.cs")));
		Assert.IsTrue(System.IO.File.Exists(Path.Combine(typesDir, "Pinia.Types.Mapping.cs")));
		Assert.IsTrue(System.IO.File.Exists(Path.Combine(typesDir, "Pinia.Types.Store.cs")));
	}

	[TestMethod]
	public void Pinia_ShellFile_RemainsHostAttributeEntryPointOnly()
	{
		var repoRoot = ResolveRepositoryRoot();
		var shellPath = Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "Pinia.cs");
		var source = System.IO.File.ReadAllText(shellPath);

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
		var source = System.IO.File.ReadAllText(projectPath);

		StringAssert.Contains(source, "<PackageId>ECMAScript.Pinia</PackageId>");
		StringAssert.Contains(source, "<RootNamespace>ECMAScript</RootNamespace>");
		StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript\\ECMAScript.csproj\" />");
		StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript.Vue3\\ECMAScript.Vue3.csproj\" />");
	}

	[TestMethod]
	public void Pinia_PublishScript_UsesFullArtifactPreparationAndGuardsNoBuildInputs()
	{
		var repoRoot = ResolveRepositoryRoot();
		var scriptPath = Path.Combine(repoRoot, "scripts", "publish-nuget.ps1");
		var source = System.IO.File.ReadAllText(scriptPath);

		StringAssert.Contains(source, "function Assert-NoBuildPackInputsExist");
		StringAssert.Contains(source, "Run publish-nuget.ps1 once without -NoBuild to prepare the full package artifacts.");
		StringAssert.Contains(source, "if ($NoBuild) {");
		StringAssert.Contains(source, "$packArgs += \"--no-build\"");
		StringAssert.Contains(source, "$packArgs += \"-p:JazorPreparePackageArtifacts=false\"");
		StringAssert.Contains(source, "\"restore\",");
		Assert.IsFalse(
			source.Contains("-p:JazorPreparePackageArtifacts=false", StringComparison.Ordinal)
			&& source.Contains("$packArgs = @(", StringComparison.Ordinal)
			&& source.IndexOf("-p:JazorPreparePackageArtifacts=false", StringComparison.Ordinal) < source.IndexOf("if ($NoBuild) {", StringComparison.Ordinal),
			"publish-nuget.ps1 should only disable JazorPreparePackageArtifacts inside the explicit -NoBuild fast path.");
	}

	[TestMethod]
	public void Pinia_SampleSmokeScript_ExistsAndVerifiesPackConsumerPath()
	{
		var repoRoot = ResolveRepositoryRoot();
		var scriptPath = Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "verify-smoke.ps1");
		var source = System.IO.File.ReadAllText(scriptPath);

		StringAssert.Contains(source, "build-local.ps1");
		StringAssert.Contains(source, "sample host assembly for requested configuration");
		StringAssert.Contains(source, "Assert-GeneratedHostArtifacts");
		StringAssert.Contains(source, "Invoke-Npm -Arguments @(\"run\", \"build\")");
		StringAssert.Contains(source, "Invoke-Npm -Arguments @(\"run\", \"test\", \"--\", \"--run\")");
		StringAssert.Contains(source, "ECMAScript.Pinia sample smoke verification passed.");
	}

	[TestMethod]
	public void Pinia_GitHubWorkflow_ProvidesDedicatedProductionVerificationLane()
	{
		var repoRoot = ResolveRepositoryRoot();
		var workflowPath = Path.Combine(repoRoot, ".github", "workflows", "pinia-verify.yml");
		var source = System.IO.File.ReadAllText(workflowPath);

		StringAssert.Contains(source, "name: Pinia Verify");
		StringAssert.Contains(source, "./scripts/test-dotnet.ps1 `");
		StringAssert.Contains(source, "-Project pinia `");
		StringAssert.Contains(source, "-Project pinia-testing `");
		StringAssert.Contains(source, "./samples/ECMAScript.Pinia.Counter/verify-smoke.ps1 `");
		StringAssert.Contains(source, "./scripts/publish-nuget.ps1 `");
		StringAssert.Contains(source, "-OutputDirectory artifacts/packages `");
		StringAssert.Contains(source, "-OutputDirectory artifacts/packages-nobuild `");
		StringAssert.Contains(source, "-NoBuild `");
		StringAssert.Contains(source, "-BaseOutputPath 'artifacts/out/pinia/'");
		StringAssert.Contains(source, "-BaseIntermediateOutputPath 'artifacts/obj/pinia/'");
		StringAssert.Contains(source, "-BaseOutputPath 'artifacts/out/pinia-testing/'");
		StringAssert.Contains(source, "-BaseIntermediateOutputPath 'artifacts/obj/pinia-testing/'");
		StringAssert.Contains(source, "-BaseOutputPath 'artifacts/out/pinia-sample/'");
		StringAssert.Contains(source, "-BaseIntermediateOutputPath 'artifacts/obj/pinia-sample/'");
		StringAssert.Contains(source, "-BaseOutputPath 'artifacts/out/pinia-pack/'");
		StringAssert.Contains(source, "-BaseIntermediateOutputPath 'artifacts/obj/pinia-pack/'");
	}

	[TestMethod]
	public void Pinia_PublishWorkflow_DependsOnReusableVerificationLane()
	{
		var repoRoot = ResolveRepositoryRoot();
		var workflowPath = Path.Combine(repoRoot, ".github", "workflows", "nuget-publish.yml");
		var source = System.IO.File.ReadAllText(workflowPath);

		StringAssert.Contains(source, "verify-pinia:");
		StringAssert.Contains(source, "uses: ./.github/workflows/pinia-verify.yml");
		StringAssert.Contains(source, "needs: verify-pinia");
	}

	[TestMethod]
	public void Pinia_TestScript_AndSupportingScripts_ExposeIsolatedBuildOutputParameters()
	{
		var repoRoot = ResolveRepositoryRoot();
		var directoryBuildProps = System.IO.File.ReadAllText(Path.Combine(repoRoot, "Directory.Build.props"));
		var testScript = System.IO.File.ReadAllText(Path.Combine(repoRoot, "scripts", "test-dotnet.ps1"));
		var publishScript = System.IO.File.ReadAllText(Path.Combine(repoRoot, "scripts", "publish-nuget.ps1"));
		var sampleBuildScript = System.IO.File.ReadAllText(Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "build-local.ps1"));

		StringAssert.Contains(directoryBuildProps, "<PropertyGroup Condition=\"'$(JazorIsolatedBaseOutputRoot)' != ''\">");
		StringAssert.Contains(directoryBuildProps, "<BaseOutputPath>$([MSBuild]::EnsureTrailingSlash('$(JazorIsolatedBaseOutputRoot)'))$(MSBuildProjectName)\\bin\\</BaseOutputPath>");
		StringAssert.Contains(directoryBuildProps, "<PropertyGroup Condition=\"'$(JazorIsolatedBaseIntermediateOutputRoot)' != ''\">");
		StringAssert.Contains(directoryBuildProps, "<BaseIntermediateOutputPath>$([MSBuild]::EnsureTrailingSlash('$(JazorIsolatedBaseIntermediateOutputRoot)'))$(MSBuildProjectName)\\obj\\</BaseIntermediateOutputPath>");
		StringAssert.Contains(directoryBuildProps, "<MSBuildProjectExtensionsPath>$(BaseIntermediateOutputPath)</MSBuildProjectExtensionsPath>");

		StringAssert.Contains(testScript, "[string]$BaseOutputPath = \"\"");
		StringAssert.Contains(testScript, "[string]$BaseIntermediateOutputPath = \"\"");
		StringAssert.Contains(testScript, "function Get-SharedBuildPathArguments");
		StringAssert.Contains(testScript, "-p:JazorIsolatedBaseOutputRoot=$isolatedOutputRoot");
		StringAssert.Contains(testScript, "-p:JazorIsolatedBaseIntermediateOutputRoot=$isolatedIntermediateRoot");
		StringAssert.Contains(testScript, "/nr:false");
		StringAssert.Contains(testScript, "-p:UseSharedCompilation=false");

		StringAssert.Contains(publishScript, "[string]$BaseOutputPath = \"\"");
		StringAssert.Contains(publishScript, "[string]$BaseIntermediateOutputPath = \"\"");
		StringAssert.Contains(publishScript, "-p:JazorIsolatedBaseOutputRoot=$(Get-IsolatedBuildRoot -Path $BaseOutputPath)");
		StringAssert.Contains(publishScript, "-p:JazorIsolatedBaseIntermediateOutputRoot=$isolatedIntermediateRoot");
		StringAssert.Contains(publishScript, "$(JazorPackageBuildOutputRoot)");

		StringAssert.Contains(sampleBuildScript, "[string]$BaseOutputPath = \"\"");
		StringAssert.Contains(sampleBuildScript, "[string]$BaseIntermediateOutputPath = \"\"");
		StringAssert.Contains(sampleBuildScript, "-p:JazorIsolatedBaseOutputRoot=$BaseOutputPath");
		StringAssert.Contains(sampleBuildScript, "-p:JazorIsolatedBaseIntermediateOutputRoot=$BaseIntermediateOutputPath");
		StringAssert.Contains(sampleBuildScript, "/nr:false");
		StringAssert.Contains(sampleBuildScript, "-p:UseSharedCompilation=false");
	}

	[TestMethod]
	public void Pinia_SafeErasedValueUnions_UseNativeUnionKeyword()
	{
		var repoRoot = ResolveRepositoryRoot();
		var source = System.IO.File.ReadAllText(Path.Combine(repoRoot, "src", "ECMAScript.Pinia", "Types", "Pinia.Types.Store.cs"));

		AssertUsesNativeUnion(source, "SubscriptionMutationEvents");
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
