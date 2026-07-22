using System.Diagnostics;
using System.Text;
using Jazor.Analyzer.RazorVue.Generation;
using Jazor.RazorVue.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.NET.Sdk.Razor.SourceGenerators;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSourceGeneratorBootstrapPatchTests
{
    [TestMethod]
    public void ExternalBuild_BootstrapHook_BindsOfficialGeneratedCSharpThroughImplementationSourceOutput()
    {
        var rootDirectory = CreateTemporaryDirectory();

        try
        {
            var toolset = RequireRazorSdkToolset();
            var analyzerDirectory = Path.Combine(rootDirectory, "analyzer");
            var projectDirectory = Path.Combine(rootDirectory, "project");
            Directory.CreateDirectory(analyzerDirectory);
            Directory.CreateDirectory(projectDirectory);

            var analyzerAssemblyPath = typeof(Jazor.RazorVue.Analysis.RazorVueGenerator).Assembly.Location;
            CopyAnalyzerPayload(analyzerAssemblyPath, analyzerDirectory);
            var outputRoot = Path.Combine(rootDirectory, "out");
            var intermediateRoot = Path.Combine(rootDirectory, "obj");
            var generatedRoot = Path.Combine(projectDirectory, "Generated");

            WriteProjectFiles(
                projectDirectory,
                analyzerDirectory,
                toolset.SdkVersion,
                toolset.TargetFramework,
                toolset.RazorLangVersion,
                enableRazorSgIntegration: true);

            var buildResult = RunDotNetBuild(
                projectDirectory,
                outputRoot,
                intermediateRoot);

            TestContext.WriteLine("Build stdout:");
            TestContext.WriteLine(buildResult.StandardOutput);
            TestContext.WriteLine("Build stderr:");
            TestContext.WriteLine(buildResult.StandardError);

            Assert.IsTrue(buildResult.ExitCode == 0, buildResult.DescribeFailure());

            TestContext.WriteLine("Analyzer assembly: " + Path.Combine(analyzerDirectory, Path.GetFileName(analyzerAssemblyPath)));
            foreach (var analyzerFile in Directory.EnumerateFiles(analyzerDirectory, "*", SearchOption.TopDirectoryOnly))
            {
                TestContext.WriteLine("Analyzer file: " + Path.GetFileName(analyzerFile));
            }

            foreach (var generatedFile in Directory.EnumerateFiles(generatedRoot, "*.cs", SearchOption.AllDirectories))
            {
                TestContext.WriteLine("Generated file: " + generatedFile);
            }

            var generatedTracePath = Directory
                .EnumerateFiles(generatedRoot, "Jazor.RazorVue.RazorSgBootstrapTrace.g.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
            Assert.IsFalse(string.IsNullOrWhiteSpace(generatedTracePath), "The bootstrap implementation source-output hook did not emit the trace generated source.");
            var generatedTrace = File.ReadAllText(generatedTracePath!);
            StringAssert.Contains(generatedTrace, "internal const bool CurrentContextKeyAvailable = true;");
            StringAssert.Contains(generatedTrace, "internal const bool ImplementationSourceOutputHookInstalled = true;");
            StringAssert.Contains(generatedTrace, "internal const bool ImplementationSourceOutputObserved = true;");
            StringAssert.Contains(generatedTrace, "internal const bool TailOutputRegistered = true;");
            StringAssert.Contains(generatedTrace, "internal const bool TailOutputRegisteredForCurrentContext = true;");
            StringAssert.Contains(generatedTrace, "internal const string TailOutputRegistrationKind = \"implementation-source-output\";");
            StringAssert.Contains(generatedTrace, "internal const bool PatchFailed = false;");
            StringAssert.Contains(
                generatedTrace,
                "internal const string RazorSourceGeneratorAssemblyVersion = \"" +
                RazorSourceGeneratorCompatibilityGuard.SupportedRazorCompilerAssemblyVersion +
                "\";");
            StringAssert.Contains(generatedTrace, "internal const string RazorSourceGeneratorModuleVersionId = \"");
            Assert.IsFalse(
                generatedTrace.Contains("internal const string RazorSourceGeneratorModuleVersionId = \"\";", StringComparison.Ordinal),
                "The bootstrap trace must record the patched Razor SG module MVID.");
            StringAssert.Contains(
                generatedTrace,
                "internal const int RazorSourceGeneratorInitializeMethodIlLength = " +
                RazorSourceGeneratorCompatibilityGuard.SupportedInitializeMethodIlLength +
                ";");
            StringAssert.Contains(
                generatedTrace,
                "internal const string RazorSourceGeneratorInitializeMethodIlSha256 = \"" +
                RazorSourceGeneratorCompatibilityGuard.SupportedInitializeMethodIlSha256 +
                "\";");

            var generatedTailTracePath = Directory
                .EnumerateFiles(generatedRoot, "Jazor.RazorVue.RazorSgTailTrace.g.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
            Assert.IsFalse(string.IsNullOrWhiteSpace(generatedTailTracePath), "The RazorVue tail output did not emit its test trace.");
            var generatedTailTrace = File.ReadAllText(generatedTailTracePath!);
            TestContext.WriteLine("Tail trace:");
            TestContext.WriteLine(generatedTailTrace);
            StringAssert.Contains(generatedTailTrace, "internal const string State = \"bound\";");
            StringAssert.Contains(generatedTailTrace, "internal const int ReusedGeneratedTreeCount = 0;");
            StringAssert.Contains(generatedTailTrace, "internal const int DerivedGeneratedTreeCount = 1;");
            StringAssert.Contains(generatedTailTrace, "internal const string BindingMode = \"DerivedHookCompilation\";");

            var generatedRazorPath = Directory
                .EnumerateFiles(generatedRoot, "*_razor.g.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
            Assert.IsFalse(string.IsNullOrWhiteSpace(generatedRazorPath), "The official Razor source generator output was not found.");

            var generatedEvidencePath = Directory
                .EnumerateFiles(generatedRoot, "Jazor.Generated.RazorSgFinalDocumentEvidence.g.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
            Assert.IsFalse(string.IsNullOrWhiteSpace(generatedEvidencePath), "The Razor SG tail output did not emit final-document evidence.");
            var generatedEvidence = File.ReadAllText(generatedEvidencePath!);
            StringAssert.Contains(generatedEvidence, "internal const int SchemaVersion = 2;");
            StringAssert.Contains(generatedEvidence, "internal const string InputContract = \"OfficialRazorSgFinalDocument\";");
            StringAssert.Contains(generatedEvidence, "internal const bool ConsumesRazorIntermediateRepresentation = false;");
            StringAssert.Contains(generatedEvidence, "internal const string GeneratedDocumentContentHash = \"");
            StringAssert.Contains(generatedEvidence, "internal const string BuildRenderTreeOperationInventory = \"");
            StringAssert.Contains(generatedEvidence, "internal const int ComponentCount = 1;");
            StringAssert.Contains(generatedEvidence, "internal const string BindingMode = \"DerivedHookCompilation\";");

            AssertNoExternalGeneratedSource(generatedRoot, "Jazor.Generated.RazorVueCatalog.g.cs");
            AssertNoExternalGeneratedSource(generatedRoot, "Jazor.Generated.RazorVue.Artifact_*.g.cs");
        }
        finally
        {
            TryDeleteDirectory(rootDirectory);
        }
    }

    [TestMethod]
    public void ExternalBuild_RazorSgIntegrationDisabled_DoesNotEmitTailOutput()
    {
        var rootDirectory = CreateTemporaryDirectory();

        try
        {
            var toolset = RequireRazorSdkToolset();
            var analyzerDirectory = Path.Combine(rootDirectory, "analyzer");
            var projectDirectory = Path.Combine(rootDirectory, "project");
            Directory.CreateDirectory(analyzerDirectory);
            Directory.CreateDirectory(projectDirectory);

            var analyzerAssemblyPath = typeof(RazorVueGenerator).Assembly.Location;
            CopyAnalyzerPayload(analyzerAssemblyPath, analyzerDirectory);
            var outputRoot = Path.Combine(rootDirectory, "out");
            var intermediateRoot = Path.Combine(rootDirectory, "obj");
            var generatedRoot = Path.Combine(projectDirectory, "Generated");

            WriteProjectFiles(
                projectDirectory,
                analyzerDirectory,
                toolset.SdkVersion,
                toolset.TargetFramework,
                toolset.RazorLangVersion,
                enableRazorSgIntegration: false);

            var buildResult = RunDotNetBuild(
                projectDirectory,
                outputRoot,
                intermediateRoot);

            TestContext.WriteLine("Build stdout:");
            TestContext.WriteLine(buildResult.StandardOutput);
            TestContext.WriteLine("Build stderr:");
            TestContext.WriteLine(buildResult.StandardError);

            Assert.IsTrue(buildResult.ExitCode == 0, buildResult.DescribeFailure());

            var generatedRazorPath = Directory
                .EnumerateFiles(generatedRoot, "*_razor.g.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
            Assert.IsFalse(string.IsNullOrWhiteSpace(generatedRazorPath), "Disabling RazorVue integration must not suppress official Razor source generator output.");

            var generatedTracePath = Directory
                .EnumerateFiles(generatedRoot, "Jazor.RazorVue.RazorSgBootstrapTrace.g.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
            Assert.IsFalse(string.IsNullOrWhiteSpace(generatedTracePath), "The bootstrap trace source was not emitted.");
            var generatedTrace = File.ReadAllText(generatedTracePath!);
            StringAssert.Contains(generatedTrace, "internal const bool CurrentContextKeyAvailable = true;");
            StringAssert.Contains(generatedTrace, "internal const bool ImplementationSourceOutputHookInstalled = true;");
            StringAssert.Contains(generatedTrace, "internal const bool ImplementationSourceOutputObserved = true;");
            StringAssert.Contains(generatedTrace, "internal const bool TailOutputRegistered = true;");
            StringAssert.Contains(generatedTrace, "internal const bool TailOutputRegisteredForCurrentContext = true;");
            StringAssert.Contains(generatedTrace, "internal const string TailOutputRegistrationKind = \"implementation-source-output\";");
            StringAssert.Contains(generatedTrace, "internal const bool PatchFailed = false;");
        }
        finally
        {
            TryDeleteDirectory(rootDirectory);
        }
    }

    [TestMethod]
    public void InProcess_RazorSourceGeneratorTailOutput_BindsForEachInitialize()
    {
        var firstRun = RunHookedRazorSourceGenerator(
            assemblyName: "Jazor.RazorVue.Sg.Bootstrap.First",
            documentPath: @"D:\repo\Demo\Pages\FirstCounter.razor",
            componentName: "FirstCounter",
            moduleImport: "./components/first-counter");
        AssertHookedRunEmittedFinalDocumentEvidence(firstRun);

        var secondRun = RunHookedRazorSourceGenerator(
            assemblyName: "Jazor.RazorVue.Sg.Bootstrap.Second",
            documentPath: @"D:\repo\Demo\Pages\SecondCounter.razor",
            componentName: "SecondCounter",
            moduleImport: "./components/second-counter");
        AssertHookedRunEmittedFinalDocumentEvidence(secondRun);
    }

    private static GeneratorDriverRunResult RunHookedRazorSourceGenerator(
        string assemblyName,
        string documentPath,
        string componentName,
        string moduleImport)
    {
        const string projectDirectory = @"D:\repo\Demo";
        const string documentText = """
            <h1>Hello</h1>
            """;
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    $$"""
                    using ECMAScript;
                    using static ECMAScript.Vue3;
                    using Microsoft.AspNetCore.Components;

                    namespace Demo.Pages;

                    [ECMAScriptModule("{{moduleImport}}")]
                    public partial class {{componentName}} : ComponentBase, IVueComponent
                    {
                    }
                    """,
                    options: parseOptions,
                    path: componentName + ".razor.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var additionalText = new InMemoryAdditionalText(documentPath, documentText);
        var optionsProvider = new TestAnalyzerConfigOptionsProvider(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["build_property.RazorLangVersion"] = "10.0",
                ["build_property.RootNamespace"] = "Demo",
                ["build_property.SupportLocalizedComponentNames"] = "true",
                ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
                ["build_property.MSBuildProjectDirectory"] = projectDirectory,
                ["build_property.EnableRazorHostOutputs"] = "true",
                ["build_property.JazorRazorVueEnableRazorSgIntegration"] = "true"
            },
            new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                [documentPath] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("Pages/" + componentName + ".razor"))
                }
            });

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators:
            [
                new RazorVueGenerator().AsSourceGenerator(),
                new RazorSourceGenerator().AsSourceGenerator()
            ],
            additionalTexts: [additionalText],
            parseOptions: parseOptions,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);
        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        var errors = outputCompilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static item => item.ToString())));
        return driver.GetRunResult();
    }

    private static void AssertHookedRunEmittedFinalDocumentEvidence(GeneratorDriverRunResult runResult)
    {
        var generatedSources = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .ToArray();
        Assert.IsTrue(
            generatedSources.Any(static source => source.HintName == "Jazor.Generated.RazorSgFinalDocumentEvidence.g.cs"),
            "The hooked Razor SG run did not emit final-document evidence.");
        Assert.IsTrue(
            generatedSources.All(static source => source.HintName != "Jazor.Generated.RazorVueCatalog.g.cs"),
            "The final-document G0 tail must not emit the legacy RazorVue catalog.");
        Assert.IsTrue(
            generatedSources.All(static source => !source.HintName.StartsWith("Jazor.Generated.RazorVue.Artifact_", StringComparison.Ordinal)),
            "The final-document G0 tail must not emit an SFC artifact.");
        Assert.IsTrue(
            generatedSources.Any(static source => source.HintName.EndsWith("_razor.g.cs", StringComparison.Ordinal)),
            "The official Razor source generator output was not found.");
    }

    private static void AssertNoExternalGeneratedSource(string generatedRoot, string searchPattern)
    {
        if (!Directory.Exists(generatedRoot))
            return;

        var generatedPath = Directory
            .EnumerateFiles(generatedRoot, searchPattern, SearchOption.AllDirectories)
            .FirstOrDefault();
        Assert.IsTrue(
            string.IsNullOrWhiteSpace(generatedPath),
            "Generated source '" + searchPattern + "' was not expected: " + generatedPath);
    }

    public TestContext TestContext { get; set; } = default!;

    private static void WriteProjectFiles(
        string projectDirectory,
        string analyzerDirectory,
        string sdkVersion,
        string targetFramework,
        string razorLangVersion,
        bool enableRazorSgIntegration)
    {
        File.WriteAllText(
            Path.Combine(projectDirectory, "global.json"),
            """
            {
              "sdk": {
                "version": "SDK_VERSION"
              }
            }
            """.Replace("SDK_VERSION", sdkVersion, StringComparison.Ordinal));

        File.WriteAllText(
            Path.Combine(projectDirectory, "PatchProbe.csproj"),
            """
            <Project Sdk="Microsoft.NET.Sdk.Razor">
              <PropertyGroup>
                <TargetFramework>TARGET_FRAMEWORK</TargetFramework>
                <RootNamespace>Demo</RootNamespace>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <RazorLangVersion>RAZOR_LANG_VERSION</RazorLangVersion>
                <UseRazorSourceGenerator>true</UseRazorSourceGenerator>
                <JazorRazorVueTestHook>true</JazorRazorVueTestHook>
                <JazorRazorVueEnableRazorSgIntegration>ENABLE_RAZOR_SG_INTEGRATION</JazorRazorVueEnableRazorSgIntegration>
                <EnableRazorHostOutputs>true</EnableRazorHostOutputs>
                <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
                <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
              </PropertyGroup>

              <ItemGroup>
                <CompilerVisibleProperty Include="JazorRazorVueTestHook" />
                <CompilerVisibleProperty Include="JazorRazorVueEnableRazorSgIntegration" />
                <CompilerVisibleProperty Include="EnableRazorHostOutputs" />
                <FrameworkReference Include="Microsoft.AspNetCore.App" />
                <Analyzer Include="ANALYZER_DIRECTORY\*.dll"
                          Exclude="ANALYZER_DIRECTORY\Microsoft.CodeAnalysis.Razor.Compiler.dll" />
                <Reference Include="ECMAScript.Contract">
                  <HintPath>ANALYZER_DIRECTORY\ECMAScript.Contract.dll</HintPath>
                </Reference>
                <Reference Include="ECMAScript">
                  <HintPath>ANALYZER_DIRECTORY\ECMAScript.dll</HintPath>
                </Reference>
                <Reference Include="ECMAScript.VueContract">
                  <HintPath>ANALYZER_DIRECTORY\ECMAScript.VueContract.dll</HintPath>
                </Reference>
                <Reference Include="ECMAScript.Vue3">
                  <HintPath>ANALYZER_DIRECTORY\ECMAScript.Vue3.dll</HintPath>
                </Reference>
                <Reference Include="Jazor.RazorVue">
                  <HintPath>ANALYZER_DIRECTORY\Jazor.RazorVue.dll</HintPath>
                </Reference>
              </ItemGroup>
            </Project>
            """.Replace("ANALYZER_DIRECTORY", analyzerDirectory, StringComparison.Ordinal)
            .Replace("TARGET_FRAMEWORK", targetFramework, StringComparison.Ordinal)
            .Replace("RAZOR_LANG_VERSION", razorLangVersion, StringComparison.Ordinal)
            .Replace("ENABLE_RAZOR_SG_INTEGRATION", enableRazorSgIntegration ? "true" : "false", StringComparison.Ordinal));

        File.WriteAllText(
            Path.Combine(projectDirectory, "Counter.razor"),
            """
            <h1>Hello</h1>
            """);

        File.WriteAllText(
            Path.Combine(projectDirectory, "Counter.razor.cs"),
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace Demo;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
            }
            """);
    }

    private static RazorSdkToolsetProbe RequireRazorSdkToolset()
    {
        var toolset = RazorSdkToolsetProbeResolver.Resolve();
        if (toolset is null)
            Assert.Inconclusive("A Razor SDK toolset could not be resolved from the installed dotnet SDKs.");

        return toolset;
    }

    private static void CopyAnalyzerPayload(string analyzerAssemblyPath, string analyzerDirectory)
    {
        var sourceDirectory = Path.GetDirectoryName(analyzerAssemblyPath)
            ?? throw new InvalidOperationException("Analyzer assembly directory could not be resolved.");
        var payloadFiles = new[]
        {
            "Jazor.Analyzer.dll",
            "ECMAScript.dll",
            "Jazor.RazorVue.dll",
            "Jazor.Compiler.dll",
            "Jazor.Common.dll",
            "ECMAScript.Contract.dll",
            "ECMAScript.VueContract.dll",
            "ECMAScript.Vue3.dll",
            "Acornima.dll",
            "Acornima.Extras.dll",
        };

        foreach (var payloadFile in payloadFiles)
        {
            var sourcePath = Path.Combine(sourceDirectory, payloadFile);
            if (!File.Exists(sourcePath))
                continue;

            File.Copy(sourcePath, Path.Combine(analyzerDirectory, payloadFile), overwrite: true);
        }
    }

    private static BuildProcessResult RunDotNetBuild(
        string projectDirectory,
        string outputRoot,
        string intermediateRoot)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments =
                    "build \"PatchProbe.csproj\" /nodeReuse:false -m:1 -p:UseSharedCompilation=false " +
                    "-p:BaseOutputPath=\"" + EnsureTrailingDirectorySeparator(outputRoot) + "\" " +
                    "-p:BaseIntermediateOutputPath=\"" + EnsureTrailingDirectorySeparator(intermediateRoot) + "\" " +
                    "-v minimal",
                WorkingDirectory = projectDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.StartInfo.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";

        Assert.IsTrue(process.Start(), "Failed to start dotnet build for bootstrap patch probe.");
        var standardOutputTask = process.StandardOutput.ReadToEndAsync();
        var standardErrorTask = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(milliseconds: 120_000))
        {
            KillProcessTree(process);

            var timedOutOutput = standardOutputTask.IsCompletedSuccessfully ? standardOutputTask.Result : string.Empty;
            var timedOutError = standardErrorTask.IsCompletedSuccessfully ? standardErrorTask.Result : string.Empty;
            return new BuildProcessResult(-1, timedOutOutput, timedOutError, TimedOut: true);
        }

        var standardOutput = WaitForOutputOrFallback(standardOutputTask);
        var standardError = WaitForOutputOrFallback(standardErrorTask);
        return new BuildProcessResult(process.ExitCode, standardOutput, standardError, TimedOut: false);
    }

    private static string WaitForOutputOrFallback(Task<string> outputTask)
        => outputTask.Wait(millisecondsTimeout: 5_000) && outputTask.IsCompletedSuccessfully
            ? outputTask.Result
            : string.Empty;

    private static void KillProcessTree(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }

        try
        {
            process.WaitForExit(milliseconds: 5_000);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _text;
    }

    private sealed class TestAnalyzerConfigOptionsProvider(
        IReadOnlyDictionary<string, string> globalOptions,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> additionalFileOptions) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _additionalFileOptions = additionalFileOptions;
        private static readonly AnalyzerConfigOptions EmptyOptions = new TestAnalyzerConfigOptions(new Dictionary<string, string>(StringComparer.Ordinal));

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
            => EmptyOptions;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => _additionalFileOptions.TryGetValue(textFile.Path, out var values)
                ? new TestAnalyzerConfigOptions(values)
                : EmptyOptions;
    }

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        private readonly IReadOnlyDictionary<string, string> _values = values;

        public override bool TryGetValue(string key, out string value)
            => _values.TryGetValue(key, out value!);
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "JazorRazorSourceGeneratorBootstrapPatchTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string EnsureTrailingDirectorySeparator(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty.", nameof(path));

        var normalized = Path.GetFullPath(path).Replace('\\', '/');
        return normalized.EndsWith("/", StringComparison.Ordinal)
            ? normalized
            : normalized + "/";
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record BuildProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError,
        bool TimedOut)
    {
        public string DescribeFailure()
        {
            var builder = new StringBuilder();
            builder.AppendLine("dotnet build failed.");
            builder.AppendLine("ExitCode: " + ExitCode);
            builder.AppendLine("TimedOut: " + TimedOut);
            if (!string.IsNullOrWhiteSpace(StandardOutput))
            {
                builder.AppendLine("stdout:");
                builder.AppendLine(StandardOutput);
            }

            if (!string.IsNullOrWhiteSpace(StandardError))
            {
                builder.AppendLine("stderr:");
                builder.AppendLine(StandardError);
            }

            return builder.ToString();
        }
    }
}
