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
		StringAssert.Contains(source, "<ProjectReference Include=\"..\\ECMAScript.Vue\\ECMAScript.Vue.csproj\" />");
	}

	[TestMethod]
	public void Pinia_PublishScript_UsesFullArtifactPreparationAndGuardsNoBuildInputs()
	{
		var repoRoot = ResolveRepositoryRoot();
		var scriptPath = Path.Combine(repoRoot, "scripts", "csharp", "publish-nuget.cs");
		var source = System.IO.File.ReadAllText(scriptPath);

		StringAssert.Contains(source, "AssertNoBuildPackInputsExist");
		StringAssert.Contains(source, "GetNoBuildPackInputRoots");
		StringAssert.Contains(source, "PackageCatalog.ResolveSelectedPackages");
		StringAssert.Contains(source, "Default package set: Jazor, Jazor.Vue, ECMAScript.Style, Jazor.Admin, ECMAScript.Vue.Devtools, ECMAScript.VueDataUi, ECMAScript.Pinia, ECMAScript.Pinia.Testing, ECMAScript.VueRoute, ECMAScript.Vuetify, ECMAScript.ElementPlus, ECMAScript.TDesign");
		StringAssert.Contains(source, "Selected packages: ");
		StringAssert.Contains(source, "case \"--package\"");
		StringAssert.Contains(source, "case \"--package-version\"");
		StringAssert.Contains(source, "-p:MinVerVersionOverride=");
		StringAssert.Contains(source, "-p:JazorPackageVersion=");
		StringAssert.Contains(source, "PackageAliases");
		StringAssert.Contains(source, "[\"jazor-vue\"] = \"Jazor.Vue\"");
		StringAssert.Contains(source, "[\"style\"] = \"ECMAScript.Style\"");
		StringAssert.Contains(source, "[\"admin\"] = \"Jazor.Admin\"");
		StringAssert.Contains(source, "[\"devtools\"] = \"ECMAScript.Vue.Devtools\"");
		StringAssert.Contains(source, "[\"dataui\"] = \"ECMAScript.VueDataUi\"");
		StringAssert.Contains(source, "[\"elementplus\"] = \"ECMAScript.ElementPlus\"");
		StringAssert.Contains(source, "[\"tdesign\"] = \"ECMAScript.TDesign\"");
		StringAssert.Contains(source, "RequiresJazorEmitPublishOutput: true");
		StringAssert.Contains(source, "DisableJazorPreparePackageArtifactsOnNoBuild: true");
		StringAssert.Contains(source, "DisableJazorPreparePackageArtifactsOnNoBuild: false");
		StringAssert.Contains(source, "Run publish-nuget.cs once without --no-build to prepare the full package artifacts.");
		StringAssert.Contains(source, "if (options.NoBuild)");
		StringAssert.Contains(source, "packArguments.Add(\"--no-build\")");
		StringAssert.Contains(source, "packArguments.Add(\"-p:JazorPreparePackageArtifacts=false\")");
		StringAssert.Contains(source, "\"restore\"");
		StringAssert.Contains(source, "ResolveBuildRoot(repoRoot, baseOutputPath)");
		StringAssert.Contains(source, "Path.Combine(packageBuildOutputRoot, \"Jazor.Emit\", \"bin\", configuration, \"net11.0\", \"publish\")");
		Assert.IsFalse(
			source.Contains("-p:JazorPreparePackageArtifacts=false", StringComparison.Ordinal)
			&& source.Contains("var packArguments = new List<string>", StringComparison.Ordinal)
			&& source.IndexOf("-p:JazorPreparePackageArtifacts=false", StringComparison.Ordinal) < source.IndexOf("if (options.NoBuild)", StringComparison.Ordinal),
			"publish-nuget.cs should only disable JazorPreparePackageArtifacts inside the explicit --no-build fast path.");
	}

	[TestMethod]
	public void Pinia_SampleSmokeScript_ExistsAndVerifiesPackConsumerPath()
	{
		var repoRoot = ResolveRepositoryRoot();
		var consumerRoot = Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "pinia-consumer");
		var denoConfigPath = Path.Combine(consumerRoot, "deno.json");
		var nugetConfigPath = Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "NuGet.Config");
		var scriptPath = Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "verify-smoke.cs");
		var consumerBuildPath = Path.Combine(consumerRoot, "scripts", "build.ts");
		var packageJsonPath = Path.Combine(consumerRoot, "package.json");
		var viteConfigPath = Path.Combine(consumerRoot, "vite.config.js");
		var denoConfig = System.IO.File.ReadAllText(denoConfigPath);
		var nugetConfig = System.IO.File.ReadAllText(nugetConfigPath);
		var source = System.IO.File.ReadAllText(scriptPath);
		var consumerBuild = System.IO.File.ReadAllText(consumerBuildPath);

		StringAssert.Contains(denoConfig, "\"build\": \"deno run -A scripts/build.ts\"");
		StringAssert.Contains(denoConfig, "\"test\": \"deno run -A scripts/test.ts\"");
		StringAssert.Contains(nugetConfig, "https://api.nuget.org/v3/index.json");
		StringAssert.Contains(source, "RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { \"task\", \"build\" })");
		StringAssert.Contains(source, "RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { \"test\", \"-A\", \"--frozen\", \"--import-map\"");
		StringAssert.Contains(source, "sample host assembly for requested configuration");
		StringAssert.Contains(source, "AssertGeneratedHostArtifacts");
		StringAssert.Contains(source, "ResolveDenoHostRuntime(repoRoot, restorePackagesPath, resolvedPackageInfo)");
		StringAssert.Contains(source, "AssertNetpackBundleArtifacts(bundleOutputRoot)");
		StringAssert.Contains(source, "[\"JAZOR_BUNDLE_ROOT\"] = bundleOutputRoot");
		StringAssert.Contains(source, "RestoreAdditionalProjectSources={packageOutput}");
		StringAssert.Contains(source, "RunDeno(denoExePath, consumerRoot, denoEnvironment");
		StringAssert.Contains(source, "ECMAScript.Pinia sample smoke verification passed.");
		Assert.IsFalse(nugetConfig.Contains(".tmp\\nupkg-sample", StringComparison.OrdinalIgnoreCase), "Sample NuGet.Config should not depend on a transient local package output path.");
		Assert.IsFalse(nugetConfig.Contains("JazorLocal", StringComparison.OrdinalIgnoreCase), "Sample NuGet.Config should keep only stable baseline sources.");
		Assert.IsFalse(source.Contains("vite", StringComparison.OrdinalIgnoreCase), "Smoke verification script should not depend on Vite anymore.");
		StringAssert.Contains(consumerBuild, "Netpack browser bundle");
		StringAssert.Contains(consumerBuild, "copyDirectoryContents(workspace.bundleRoot, workspace.assetsDirectory)");
		Assert.IsFalse(consumerBuild.Contains("deno bundle", StringComparison.OrdinalIgnoreCase), "The active sample must use Netpack rather than Deno for browser bundling.");
		Assert.IsFalse(System.IO.File.Exists(packageJsonPath), $"Vite package manifest should not remain in the Deno consumer: {packageJsonPath}");
		Assert.IsFalse(System.IO.File.Exists(viteConfigPath), $"Vite config should not remain in the Deno consumer: {viteConfigPath}");
	}

	[TestMethod]
	public void Pinia_GitHubWorkflow_ProvidesDedicatedProductionVerificationLane()
	{
		var repoRoot = ResolveRepositoryRoot();
		var workflowPath = Path.Combine(repoRoot, ".github", "workflows", "pinia-verify.yml");
		var source = System.IO.File.ReadAllText(workflowPath);

		StringAssert.Contains(source, "name: Pinia Verify");
		StringAssert.Contains(source, "dotnet run --file ./scripts/csharp/test-dotnet.cs -- `");
		StringAssert.Contains(source, "--project pinia `");
		StringAssert.Contains(source, "--project pinia-testing `");
		StringAssert.Contains(source, "--project dataui `");
		StringAssert.Contains(source, "dotnet run --file ./samples/ECMAScript.Pinia.Counter/verify-smoke.cs");
		StringAssert.Contains(source, "dotnet run --file ./samples/ECMAScript.VueDataUi.Dashboard/build-local.cs");
		StringAssert.Contains(source, "dotnet run --file ./scripts/csharp/publish-nuget.cs -- `");
		StringAssert.Contains(source, "--output-directory artifacts/packages `");
		StringAssert.Contains(source, "--output-directory artifacts/packages-nobuild `");
		StringAssert.Contains(source, "--no-build `");
		StringAssert.Contains(source, "--package jazor `");
		StringAssert.Contains(source, "--package dataui `");
		StringAssert.Contains(source, "--package pinia `");
		StringAssert.Contains(source, "--package pinia-testing `");
		StringAssert.Contains(source, "--package vueroute `");
		StringAssert.Contains(source, "--package vuetify `");
		StringAssert.Contains(source, "--package elementplus `");
		StringAssert.Contains(source, "--package tdesign `");
		StringAssert.Contains(source, "--base-output-path 'artifacts/out/pinia/'");
		StringAssert.Contains(source, "--base-intermediate-output-path 'artifacts/obj/pinia/'");
		StringAssert.Contains(source, "--base-output-path 'artifacts/out/pinia-testing/'");
		StringAssert.Contains(source, "--base-intermediate-output-path 'artifacts/obj/pinia-testing/'");
		StringAssert.Contains(source, "-BaseOutputPath 'artifacts/out/pinia-sample/'");
		StringAssert.Contains(source, "-BaseIntermediateOutputPath 'artifacts/obj/pinia-sample/'");
		StringAssert.Contains(source, "--base-output-path 'artifacts/out/pinia-pack/'");
		StringAssert.Contains(source, "--base-intermediate-output-path 'artifacts/obj/pinia-pack/'");
	}

	[TestMethod]
	public void Pinia_PublishWorkflow_DependsOnReusableVerificationLane()
	{
		var repoRoot = ResolveRepositoryRoot();
		var workflowPath = Path.Combine(repoRoot, ".github", "workflows", "nuget-publish-ref.yml");
		var source = System.IO.File.ReadAllText(workflowPath);

		StringAssert.Contains(source, "name: Publish NuGet From Ref");
		StringAssert.Contains(source, "tags:");
		StringAssert.Contains(source, "- \"v*\"");
		StringAssert.Contains(source, "verify-pinia:");
		StringAssert.Contains(source, "uses: ./.github/workflows/pinia-verify.yml");
		StringAssert.Contains(source, "needs: verify-pinia");
		StringAssert.Contains(source, "github.event_name != 'push' || needs.verify-pinia.result == 'success'");
		StringAssert.Contains(source, "id: release_ref");
		StringAssert.Contains(source, "$releaseRef = '${{ github.ref_name }}'");
		StringAssert.Contains(source, "--package jazor `");
		StringAssert.Contains(source, "--package dataui `");
		StringAssert.Contains(source, "--package pinia `");
		StringAssert.Contains(source, "--package pinia-testing `");
		StringAssert.Contains(source, "--package vueroute `");
		StringAssert.Contains(source, "--package vuetify `");
		StringAssert.Contains(source, "--package elementplus `");
		StringAssert.Contains(source, "--package tdesign `");
		StringAssert.Contains(source, "Get-ChildItem 'artifacts/packages/Jazor.*.nupkg'");
		StringAssert.Contains(source, "Get-ChildItem 'artifacts/packages/*.nupkg' -Exclude '*.snupkg'");
		StringAssert.Contains(source, "NuGet trusted publishing login");
		StringAssert.Contains(source, "Push to GitHub Packages");
		StringAssert.Contains(source, "Create GitHub Release");
	}

	[TestMethod]
	public void Pinia_LegacyNuGetWorkflow_RemainsPackOnlyDryRun()
	{
		var repoRoot = ResolveRepositoryRoot();
		var workflowPath = Path.Combine(repoRoot, ".github", "workflows", "nuget-publish.yml");
		var source = System.IO.File.ReadAllText(workflowPath);

		StringAssert.Contains(source, "name: Pack NuGet Dry Run");
		StringAssert.Contains(source, "workflow_dispatch:");
		StringAssert.Contains(source, "ref: ${{ inputs.ref || github.ref }}");
		StringAssert.Contains(source, "--skip-push");
		Assert.IsFalse(source.Contains("push:", StringComparison.Ordinal), "Dry-run workflow must not run on tag pushes.");
		Assert.IsFalse(source.Contains("id-token: write", StringComparison.Ordinal), "Dry-run workflow must not request NuGet trusted-publishing permissions.");
		Assert.IsFalse(source.Contains("NuGet trusted publishing login", StringComparison.Ordinal), "Dry-run workflow must not publish to nuget.org.");
		Assert.IsFalse(source.Contains("Push to GitHub Packages", StringComparison.Ordinal), "Dry-run workflow must not publish to GitHub Packages.");
		Assert.IsFalse(source.Contains("Create GitHub Release", StringComparison.Ordinal), "Dry-run workflow must not create GitHub releases.");
	}

	[TestMethod]
	public void Pinia_JazorBuildTransitive_DefaultsConsumerLangVersionToPreview()
	{
		var repoRoot = ResolveRepositoryRoot();
		var propsPath = Path.Combine(repoRoot, "src", "Jazor", "buildTransitive", "Jazor.props");
		var source = System.IO.File.ReadAllText(propsPath);

		StringAssert.Contains(source, "<LangVersion Condition=\"'$(LangVersion)' == ''\">preview</LangVersion>");
	}

	[TestMethod]
	public void Pinia_TestScript_AndSupportingScripts_ExposeIsolatedBuildOutputParameters()
	{
		var repoRoot = ResolveRepositoryRoot();
		var directoryBuildProps = System.IO.File.ReadAllText(Path.Combine(repoRoot, "Directory.Build.props"));
		var testScript = System.IO.File.ReadAllText(Path.Combine(repoRoot, "scripts", "csharp", "test-dotnet.cs"));
		var publishScript = System.IO.File.ReadAllText(Path.Combine(repoRoot, "scripts", "csharp", "publish-nuget.cs"));
		var sampleBuildScript = System.IO.File.ReadAllText(Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "build-local.cs"));
		var sampleVerifyScript = System.IO.File.ReadAllText(Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "verify-smoke.cs"));

		StringAssert.Contains(directoryBuildProps, "<PropertyGroup Condition=\"'$(JazorIsolatedBaseOutputRoot)' != ''\">");
		StringAssert.Contains(directoryBuildProps, "<BaseOutputPath>$([MSBuild]::EnsureTrailingSlash('$(JazorIsolatedBaseOutputRoot)'))$(MSBuildProjectName)\\bin\\</BaseOutputPath>");
		StringAssert.Contains(directoryBuildProps, "<PropertyGroup Condition=\"'$(JazorIsolatedBaseIntermediateOutputRoot)' != ''\">");
		StringAssert.Contains(directoryBuildProps, "<BaseIntermediateOutputPath>$([MSBuild]::EnsureTrailingSlash('$(JazorIsolatedBaseIntermediateOutputRoot)'))$(MSBuildProjectName)\\obj\\</BaseIntermediateOutputPath>");
		StringAssert.Contains(directoryBuildProps, "<MSBuildProjectExtensionsPath>$(BaseIntermediateOutputPath)</MSBuildProjectExtensionsPath>");

		StringAssert.Contains(testScript, "BaseOutputPath");
		StringAssert.Contains(testScript, "BaseIntermediateOutputPath");
		StringAssert.Contains(testScript, "GetSharedBuildPathArguments");
		StringAssert.Contains(testScript, "JazorIsolatedBaseOutputRoot=");
		StringAssert.Contains(testScript, "JazorIsolatedBaseIntermediateOutputRoot=");
		StringAssert.Contains(testScript, "/nr:false");
		StringAssert.Contains(testScript, "-p:UseSharedCompilation=false");

		StringAssert.Contains(publishScript, "BaseOutputPath");
		StringAssert.Contains(publishScript, "BaseIntermediateOutputPath");
		StringAssert.Contains(publishScript, "JazorIsolatedBaseOutputRoot=");
		StringAssert.Contains(publishScript, "JazorIsolatedBaseIntermediateOutputRoot=");
		StringAssert.Contains(publishScript, "$(JazorPackageBuildOutputRoot)");

		StringAssert.Contains(sampleBuildScript, "BaseOutputPath");
		StringAssert.Contains(sampleBuildScript, "BaseIntermediateOutputPath");
		StringAssert.Contains(sampleBuildScript, "var packArguments = new List<string>");
		StringAssert.Contains(sampleBuildScript, "publish-nuget.cs");
		StringAssert.Contains(sampleBuildScript, "--skip-push");
		StringAssert.Contains(sampleBuildScript, "\"--package\", \"jazor\"");
		StringAssert.Contains(sampleBuildScript, "\"--package\", \"pinia\"");
		StringAssert.Contains(sampleBuildScript, "\"--package\", \"pinia-testing\"");
		StringAssert.Contains(sampleBuildScript, "packageOutput");
		StringAssert.Contains(sampleBuildScript, "RestoreAdditionalProjectSources={packageOutput}");
		StringAssert.Contains(sampleBuildScript, "--bundle-out-dir");
		StringAssert.Contains(sampleBuildScript, "-p:JazorMode=release");
		StringAssert.Contains(sampleBuildScript, "JazorIsolatedBaseOutputRoot=");
		StringAssert.Contains(sampleBuildScript, "JazorIsolatedBaseIntermediateOutputRoot=");
		StringAssert.Contains(sampleBuildScript, "/nr:false");
		StringAssert.Contains(sampleBuildScript, "-p:UseSharedCompilation=false");

		StringAssert.Contains(sampleVerifyScript, "GetIsolationArguments(options)");
		StringAssert.Contains(sampleVerifyScript, "ResolveHostAssemblyPath(hostRoot, options)");
		StringAssert.Contains(sampleVerifyScript, "ResolveDenoHostRuntime(repoRoot, restorePackagesPath, resolvedPackageInfo)");
		StringAssert.Contains(sampleVerifyScript, "AssertNetpackBundleArtifacts(bundleOutputRoot)");
		StringAssert.Contains(sampleVerifyScript, "RestoreAdditionalProjectSources={packageOutput}");
		StringAssert.Contains(sampleVerifyScript, "PINIA_DENO_DIST_ROOT");
		StringAssert.Contains(sampleVerifyScript, "consumerDistRoot");
		StringAssert.Contains(sampleVerifyScript, "RunDeno(denoExePath, consumerRoot, denoEnvironment, new[] { \"task\", \"build\" })");
		StringAssert.Contains(sampleVerifyScript, "Path.Combine(consumerRoot, \".deno-build\", \"import-map.generated.json\")");
		StringAssert.Contains(sampleVerifyScript, "GetIsolatedBuildRoot(options.BaseOutputPath!, repoRoot: null)");
		StringAssert.Contains(sampleVerifyScript, "GetIsolatedBuildRoot(options.BaseIntermediateOutputPath!, repoRoot: null)");
	}

	[TestMethod]
	public void Pinia_SampleProjects_DefaultToPreviewLangVersion()
	{
		var repoRoot = ResolveRepositoryRoot();
		var sharedSamplesProps = System.IO.File.ReadAllText(Path.Combine(repoRoot, "samples", "Directory.Build.props"));
		var multiProjectProps = System.IO.File.ReadAllText(Path.Combine(repoRoot, "samples", "Jazor.MultiProject", "Directory.Build.props"));
		var piniaSampleProps = System.IO.File.ReadAllText(Path.Combine(repoRoot, "samples", "ECMAScript.Pinia.Counter", "Directory.Build.props"));

		StringAssert.Contains(sharedSamplesProps, "<LangVersion>preview</LangVersion>");
		StringAssert.Contains(sharedSamplesProps, "<Import Project=\"..\\Directory.Build.props\"");
		StringAssert.Contains(multiProjectProps, "<LangVersion>preview</LangVersion>");
		StringAssert.Contains(piniaSampleProps, "<Import Project=\"..\\Directory.Build.props\" />");
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
