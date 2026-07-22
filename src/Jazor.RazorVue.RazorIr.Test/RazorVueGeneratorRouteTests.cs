using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using Jazor.Analyzer.RazorVue.Generation;
using Jazor.RazorVue;
using Jazor.RazorVue.Analysis;
using Jazor.RazorVue.Artifacts;
using Jazor.RazorVue.Extensibility;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueGeneratorRouteTests
{
    [TestMethod]
    public void SfcGenerator_WithHandwrittenBuildRenderTreeAndRazorSgIntegration_RoutesThroughNormalGenerator()
    {
        var recorder = new RecordingSfcLowerer();
        var compilation = CreateCompilation(
            "RazorVue.Route.Handwritten.NormalGenerator.Tests",
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace Demo.Components;

            [ECMAScriptModule("./components/counter-card")]
            public sealed class CounterCard : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, Title);
                    builder.CloseElement();
                }
            }
            """);
        var generator = CreateRouteRecordingGenerator(
            recorder,
            CreateBootstrapTrace(
                hasAttempted: true,
                isInstalled: true,
                tailOutputRegistered: true),
            static () => RazorSourceGeneratorCompatibilityProbeResult.Fail("Handwritten BuildRenderTree route must not query Razor SG compatibility."));

        var runResult = RunRazorVueGenerator(
            compilation,
            generator,
            enableRazorSgIntegration: true);

        var diagnostics = GetDiagnostics(runResult);
        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        Assert.AreEqual(1, recorder.Snapshots.Count);
        var snapshot = recorder.Snapshots.Single();
        Assert.AreEqual("CounterCard", snapshot.Descriptor.Name);
        Assert.IsNotNull(snapshot.BuildRenderTreeMethod);
        Assert.IsNull(snapshot.RazorSourceGeneratorDocument);
        Assert.IsNull(snapshot.RazorIrCarrier);
        Assert.IsTrue(RazorVueBuildRenderTreeAuthoringClassifier.IsHandwrittenBuildRenderTree(snapshot));
        AssertGeneratedArtifact(runResult, "components/counter-card");
    }

    [TestMethod]
    public void TailOutput_WithRazorSourceGeneratorDocument_BindsRazorComponentThroughFinalDocumentPath()
    {
        var compilation = CreateCompilation(
            "RazorVue.Route.RazorTail.Tests",
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);
        var document = CreateRazorSourceGeneratorDocument(
            compilation,
            @"D:\repo\Demo\Pages\TodoApp.razor",
            "<section>@Title</section>");

        var runResult = RunTailOutputGenerator(
            compilation,
            context => RazorSourceGeneratorTailOutput.EmitDocuments(
                context,
                compilation,
                ImmutableArray.Create(document),
                new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: true)));

        var diagnostics = GetDiagnostics(runResult);
        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        AssertGeneratedSourceContains(runResult, "Jazor.RazorVue.RazorSgTailTrace.g.cs", "internal const string State = \"bound\";");
        AssertGeneratedSourceContains(runResult, "Jazor.RazorVue.RazorSgTailTrace.g.cs", "internal const string BindingMode = \"DerivedHookCompilation\";");
        AssertGeneratedSourceContains(runResult, "Jazor.Generated.RazorSgFinalDocumentEvidence.g.cs", "internal const int ComponentCount = 1;");
        Assert.IsTrue(
            runResult.Results
                .SelectMany(static result => result.GeneratedSources)
                .All(static source => source.HintName != "Jazor.Generated.RazorVueCatalog.g.cs" &&
                              !source.HintName.StartsWith("Jazor.Generated.RazorVue.Artifact_", StringComparison.Ordinal)),
            "The final-document G0 tail must not emit legacy SFC output.");
    }

    [TestMethod]
    public void TailOutput_WithRazorComponentButNoGeneratedDocument_ReportsDiagnosticInsteadOfSilentArtifactLoss()
    {
        var compilation = CreateCompilation(
            "RazorVue.Route.RazorTail.MissingDocument.Tests",
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);

        var runResult = RunTailOutputGenerator(
            compilation,
            context => RazorSourceGeneratorTailOutput.Emit<object>(
                context,
                compilation,
                ImmutableArray<object>.Empty,
                new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: true)));

        var diagnostics = GetDiagnostics(runResult);
        var tailDiagnostics = diagnostics
            .Where(static item => item.Id == "JAZORVGA020")
            .ToArray();
        Assert.IsTrue(
            tailDiagnostics.Length > 0,
            "Expected missing Razor SG tail input to report JAZORVGA020. Actual diagnostics: " +
            string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        Assert.IsTrue(
            tailDiagnostics.Any(static diagnostic => diagnostic.GetMessage().Contains("did not receive any Razor source generator documents", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, tailDiagnostics.Select(static item => item.ToString())));
        AssertGeneratedSourceContains(runResult, "Jazor.RazorVue.RazorSgTailTrace.g.cs", "internal const string State = \"no-generator-documents\";");
        AssertNoGeneratedArtifact(runResult);
    }

    [TestMethod]
    public void TailOutput_WithGeneratedDocumentThatDoesNotMatchComponent_ReportsDiagnostic()
    {
        var compilation = CreateCompilation(
            "RazorVue.Route.RazorTail.UnmatchedDocument.Tests",
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """);
        var document = CreateRazorSourceGeneratorDocument(
            compilation,
            @"D:\repo\Demo\Pages\OtherPage.razor",
            "<section>Unmatched</section>",
            rootNamespace: "Demo.OtherPages");

        var runResult = RunTailOutputGenerator(
            compilation,
            context => RazorSourceGeneratorTailOutput.EmitDocuments(
                context,
                compilation,
                ImmutableArray.Create(document),
                new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: true)));

        var diagnostics = GetDiagnostics(runResult);
        var tailDiagnostics = diagnostics
            .Where(static item => item.Id == "JAZORVGA020")
            .ToArray();
        Assert.IsTrue(
            tailDiagnostics.Length > 0,
            "Expected JAZORVGA020. Actual diagnostics:" + Environment.NewLine + string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        var message = tailDiagnostics[0].GetMessage();
        StringAssert.Contains(message, "did not match any RazorVue component candidate");
        StringAssert.Contains(message, "Demo.Pages.TodoApp");
        StringAssert.Contains(message, "Demo.OtherPages.OtherPage");
        StringAssert.Contains(message, "BuildRenderTree");
        AssertGeneratedSourceContains(runResult, "Jazor.RazorVue.RazorSgTailTrace.g.cs", "internal const string State = \"component-mismatch\";");
        AssertNoGeneratedArtifact(runResult);
    }

    [TestMethod]
    public void FallbackOutput_WithRazorComponent_ReportsDiagnosticInsteadOfRunningPrivateRazorSourceGenerator()
    {
        var documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        var compilation = CreateCompilation(
            "RazorVue.Route.RazorFallback.Tests",
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """,
            "TodoApp.razor.cs");

        var runResult = RunFallbackGenerator(
            compilation,
            [new InMemoryAdditionalText(documentPath, "<section>@Title</section>")],
            CreateAnalyzerOptionsProvider(
                enableRazorSgIntegration: true,
                enableRazorHostOutputs: true,
                additionalTexts:
                [
                    new AdditionalTextOptions(
                        documentPath,
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("Pages/TodoApp.razor"))
                        })
                ]));

        var diagnostics = GetDiagnostics(runResult);
        var fallbackDiagnostics = diagnostics
            .Where(static item => item.Id == "JAZORVGA020")
            .ToArray();
        Assert.IsTrue(
            fallbackDiagnostics.Length > 0,
            "Expected forbidden fallback diagnostic. Actual diagnostics: " +
            string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        var message = fallbackDiagnostics[0].GetMessage();
        StringAssert.Contains(message, "does not run a private Razor source generator fallback");
        StringAssert.Contains(message, "official Razor source generator");
        StringAssert.Contains(message, "Razor IR");
        StringAssert.Contains(message, "generated C#");
        StringAssert.Contains(message, "Demo.Pages.TodoApp");
        AssertGeneratedSourceContains(runResult, "Jazor.RazorVue.RazorSgFallbackTrace.g.cs", "internal const string State = \"forbidden\";");
        AssertNoGeneratedSource(runResult, "Jazor.RazorVue.RazorSgTailTrace.g.cs");
        AssertNoGeneratedArtifact(runResult);
    }

    [TestMethod]
    public void FallbackOutput_WithUnavailableHookPlatform_NoOpsSoGeneratorCanReportPlatformDiagnostic()
    {
        var documentPath = @"D:\repo\Demo\Pages\TodoApp.razor";
        var compilation = CreateCompilation(
            "RazorVue.Route.RazorFallback.PatchUnavailable.Tests",
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/todo-app")]
            public partial class TodoApp : ComponentBase, IVueComponent
            {
                [Parameter]
                public string? Title { get; set; }
            }
            """,
            "TodoApp.razor.cs");

        RazorSourceGeneratorBootstrapState.ResetForTests();
        RazorSourceGeneratorBootstrapState.MarkPatchUnavailable("OS: Other; Architecture: X64.");
        var runResult = RunFallbackGenerator(
            compilation,
            [new InMemoryAdditionalText(documentPath, "<section>@Title</section>")],
            CreateAnalyzerOptionsProvider(
                enableRazorSgIntegration: true,
                enableRazorHostOutputs: true,
                additionalTexts:
                [
                    new AdditionalTextOptions(
                        documentPath,
                        new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["build_metadata.AdditionalFiles.TargetPath"] = Convert.ToBase64String(Encoding.UTF8.GetBytes("Pages/TodoApp.razor"))
                        })
                ]));

        var diagnostics = GetDiagnostics(runResult)
            .Where(static item => item.Id == "JAZORVGA020")
            .ToArray();

        Assert.AreEqual(0, diagnostics.Length, string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        AssertGeneratedSourceContains(runResult, "Jazor.RazorVue.RazorSgFallbackTrace.g.cs", "internal const string State = \"bootstrap-unavailable\";");
        AssertNoGeneratedArtifact(runResult);
    }

    [TestMethod]
    public void NativeHookPlatformGuard_RejectsUnsupportedArchitectureBeforePatching()
    {
        var supported = RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
            RazorSourceGeneratorInitializeNativeHookOperatingSystem.Linux,
            Architecture.Arm,
            out var failure);

        Assert.IsFalse(supported, "Unsupported architectures must be rejected before native patching.");
        StringAssert.Contains(failure, "x64 and arm64");
        StringAssert.Contains(failure, "OS: Linux; Architecture: Arm");
    }

    [TestMethod]
    public void NativeHookPlatformGuard_RejectsLinuxArm64UntilInstructionCacheFlushIsValidated()
    {
        var supported = RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
            RazorSourceGeneratorInitializeNativeHookOperatingSystem.Linux,
            Architecture.Arm64,
            out var failure);

        Assert.IsFalse(supported, "Linux arm64 must stay disabled until instruction cache flushing is validated.");
        StringAssert.Contains(failure, "Linux arm64");
        StringAssert.Contains(failure, "instruction cache");
    }

    [TestMethod]
    public void NativeHookPlatformGuard_AllowsDocumentedDesktopMatrix()
    {
        Assert.IsTrue(
            RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
                RazorSourceGeneratorInitializeNativeHookOperatingSystem.Windows,
                Architecture.X64,
                out var windowsX64Failure),
            windowsX64Failure);
        Assert.IsTrue(
            RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
                RazorSourceGeneratorInitializeNativeHookOperatingSystem.Linux,
                Architecture.X64,
                out var linuxX64Failure),
            linuxX64Failure);
        Assert.IsTrue(
            RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
                RazorSourceGeneratorInitializeNativeHookOperatingSystem.MacOS,
                Architecture.X64,
                out var macOSX64Failure),
            macOSX64Failure);
        Assert.IsTrue(
            RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
                RazorSourceGeneratorInitializeNativeHookOperatingSystem.MacOS,
                Architecture.Arm64,
                out var macOSArm64Failure),
            macOSArm64Failure);
        Assert.IsTrue(
            RazorSourceGeneratorInitializeNativeHook.IsSupportedPlatform(
                RazorSourceGeneratorInitializeNativeHookOperatingSystem.Windows,
                Architecture.Arm64,
                out var windowsArm64Failure),
            windowsArm64Failure);
    }

    [TestMethod]
    public void NativeHookCurrentPlatformSelfTest_CanPatchAndRestoreSimpleMethod()
    {
        var supported = RazorSourceGeneratorInitializeNativeHook.TryValidateCurrentPlatform(out var failure);

        Assert.IsTrue(supported, failure);
    }

    private static CSharpCompilation CreateCompilation(
        string assemblyName,
        string source,
        string sourcePath = "Component.cs")
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(source, options: parseOptions, path: sourcePath)
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        AssertNoCompilationErrors(compilation);
        return compilation;
    }

    private static RazorVueGenerator CreateRouteRecordingGenerator(
        RecordingSfcLowerer recorder,
        RazorSourceGeneratorBootstrapTrace bootstrapTrace,
        Func<RazorSourceGeneratorCompatibilityProbeResult>? compatibilityProbeFactory = null)
        => new(
            static () => new RazorVuePipeline(RazorVueRazorDocumentSemanticFrontend.Instance, RazorVueLegacyIrFirstTemplateFrontend.Instance),
            () => new RazorVueSfcPipeline(RazorVueRazorDocumentSemanticFrontend.Instance, recorder),
            compatibilityProbeFactory ?? (static () => RazorSourceGeneratorCompatibilityProbe.CollectCurrent()),
            _ => bootstrapTrace);

    private static GeneratorDriverRunResult RunRazorVueGenerator(
        Compilation compilation,
        RazorVueGenerator generator,
        bool enableRazorSgIntegration)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [generator.AsSourceGenerator()],
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options,
            optionsProvider: CreateAnalyzerOptionsProvider(enableRazorSgIntegration, enableRazorHostOutputs: false));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        AssertNoCompilationErrors(outputCompilation);
        return driver.GetRunResult();
    }

    private static GeneratorDriverRunResult RunTailOutputGenerator(
        Compilation compilation,
        Action<SourceProductionContext> emit)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new TailOutputTestGenerator(emit).AsSourceGenerator()],
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options);

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);
        AssertNoCompilationErrors(outputCompilation);
        return driver.GetRunResult();
    }

    private static GeneratorDriverRunResult RunFallbackGenerator(
        Compilation compilation,
        ImmutableArray<AdditionalText> additionalTexts,
        AnalyzerConfigOptionsProvider optionsProvider)
    {
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new FallbackOutputTestGenerator().AsSourceGenerator()],
            additionalTexts: additionalTexts,
            parseOptions: (CSharpParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options,
            optionsProvider: optionsProvider);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out _);
        AssertNoCompilationErrors(outputCompilation);
        return driver.GetRunResult();
    }

    private static RazorSourceGeneratorDocumentOutput CreateRazorSourceGeneratorDocument(
        Compilation compilation,
        string documentPath,
        string documentText,
        string rootNamespace = "Demo.Pages")
    {
        var parseOptions = (CSharpParseOptions?)compilation.SyntaxTrees.FirstOrDefault()?.Options
                           ?? new CSharpParseOptions(LanguageVersion.Preview);
        var projectEngine = RazorVueRazorCodeDocumentProvider.CreateProjectEngine(
            documentPath,
            parseOptions,
            rootNamespace: rootNamespace);
        var tagHelpers = RazorVueRazorCodeDocumentProvider.DiscoverTagHelpers(projectEngine, compilation);
        var codeDocument = projectEngine.Process(
            RazorVueRazorCodeDocumentProvider.CreateSourceDocument(
                new RazorVueRazorDocument(documentPath, SourceText.From(documentText))),
            Microsoft.AspNetCore.Razor.Language.RazorFileKind.Component,
            ImmutableArray<Microsoft.AspNetCore.Razor.Language.RazorSourceDocument>.Empty,
            tagHelpers.Length == 0 ? null : Microsoft.AspNetCore.Razor.Language.TagHelperCollection.Create(tagHelpers));
        var csharpDocument = RazorVueRazorCodeDocumentProvider.GetRequiredCSharpDocument(codeDocument);
        var generatedTree = CSharpSyntaxTree.ParseText(
            csharpDocument.Text,
            options: parseOptions,
            path: "TodoApp.razor.g.cs");
        var generatedCompilation = compilation.AddSyntaxTrees(generatedTree);
        AssertNoCompilationErrors(generatedCompilation);

        return new RazorSourceGeneratorDocumentOutput(
            "TodoApp.razor.g.cs",
            codeDocument,
            csharpDocument);
    }

    private static RazorSourceGeneratorBootstrapTrace CreateBootstrapTrace(
        bool hasAttempted,
        bool isInstalled,
        bool tailOutputRegistered)
        => new(
            HasAttempted: hasAttempted,
            IsInstalled: isInstalled,
            RazorAssemblyObserved: isInstalled,
            PatchAttempted: isInstalled,
            GeneratorTypeFound: isInstalled,
            InitializeMethodFound: isInstalled,
            PostfixMethodFound: isInstalled,
            PatchSucceeded: isInstalled,
            PatchFailed: false,
            PatchUnavailable: false,
            PostfixInvoked: tailOutputRegistered,
            ImplementationSourceOutputHookInstalled: tailOutputRegistered,
            ImplementationSourceOutputObserved: tailOutputRegistered,
            TailOutputRegistered: tailOutputRegistered,
            CurrentContextKeyAvailable: true,
            TailOutputRegisteredForCurrentContext: tailOutputRegistered,
            TailOutputRegistrationKind: tailOutputRegistered ? "implementation-source-output" : string.Empty,
            TestHookObserved: false,
            Failure: null);

    private static AnalyzerConfigOptionsProvider CreateAnalyzerOptionsProvider(
        bool enableRazorSgIntegration,
        bool enableRazorHostOutputs,
        ImmutableArray<AdditionalTextOptions> additionalTexts = default)
    {
        var globalOptions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["build_property.JazorRazorVueOutputMode"] = "sfc",
            ["build_property.JazorRazorVueTestHook"] = "true",
            ["build_property.RazorLangVersion"] = "10.0",
            ["build_property.RootNamespace"] = "Demo",
            ["build_property.SupportLocalizedComponentNames"] = "true",
            ["build_property.GenerateRazorMetadataSourceChecksumAttributes"] = "false",
            ["build_property.MSBuildProjectDirectory"] = @"D:\repo\Demo"
        };
        if (enableRazorSgIntegration)
            globalOptions["build_property.JazorRazorVueEnableRazorSgIntegration"] = "true";
        if (enableRazorHostOutputs)
            globalOptions["build_property.EnableRazorHostOutputs"] = "true";

        return new TestAnalyzerConfigOptionsProvider(globalOptions, additionalTexts);
    }

    private static ImmutableArray<Diagnostic> GetDiagnostics(GeneratorDriverRunResult runResult)
        => runResult.Diagnostics
            .Concat(runResult.Results.SelectMany(static result => result.Diagnostics))
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();

    private static void AssertGeneratedArtifact(GeneratorDriverRunResult runResult, string relativeSfcPath)
    {
        var artifactSources = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Where(static source => source.HintName.StartsWith("Jazor.Generated.RazorVue.Artifact_", StringComparison.Ordinal))
            .Select(static source => source.SourceText.ToString())
            .ToArray();

        Assert.IsTrue(
            artifactSources.Any(source => source.Contains(relativeSfcPath, StringComparison.Ordinal)),
            "Expected generated SFC artifact path '" + relativeSfcPath + "'. Generated artifacts:" + Environment.NewLine + string.Join(Environment.NewLine, artifactSources));
    }

    private static void AssertGeneratedArtifactContains(GeneratorDriverRunResult runResult, string expectedText)
    {
        var artifactSources = GetGeneratedArtifactSources(runResult);
        Assert.IsTrue(
            artifactSources.Any(source => source.Contains(expectedText, StringComparison.Ordinal)),
            "Expected generated SFC artifact to contain '" + expectedText + "'. Generated artifacts:" + Environment.NewLine + string.Join(Environment.NewLine, artifactSources));
    }

    private static void AssertGeneratedArtifactDoesNotContain(GeneratorDriverRunResult runResult, string unexpectedText)
    {
        var artifactSources = GetGeneratedArtifactSources(runResult);
        Assert.IsFalse(
            artifactSources.Any(source => source.Contains(unexpectedText, StringComparison.Ordinal)),
            "Generated SFC artifact unexpectedly contained '" + unexpectedText + "'. Generated artifacts:" + Environment.NewLine + string.Join(Environment.NewLine, artifactSources));
    }

    private static string[] GetGeneratedArtifactSources(GeneratorDriverRunResult runResult)
        => runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Where(static source => source.HintName.StartsWith("Jazor.Generated.RazorVue.Artifact_", StringComparison.Ordinal))
            .Select(static source => source.SourceText.ToString())
            .ToArray();

    private static void AssertNoGeneratedArtifact(GeneratorDriverRunResult runResult)
    {
        Assert.IsFalse(
            runResult.Results
                .SelectMany(static result => result.GeneratedSources)
                .Any(static source => source.HintName.StartsWith("Jazor.Generated.RazorVue.Artifact_", StringComparison.Ordinal)),
            "Did not expect SFC artifact output.");
    }

    private static void AssertNoGeneratedSource(GeneratorDriverRunResult runResult, string hintName)
    {
        Assert.IsFalse(
            runResult.Results
                .SelectMany(static result => result.GeneratedSources)
                .Any(source => string.Equals(source.HintName, hintName, StringComparison.Ordinal)),
            "Generated source '" + hintName + "' was not expected.");
    }

    private static void AssertGeneratedSourceContains(
        GeneratorDriverRunResult runResult,
        string hintName,
        string expectedText)
    {
        var sources = runResult.Results
            .SelectMany(static result => result.GeneratedSources)
            .Where(source => string.Equals(source.HintName, hintName, StringComparison.Ordinal))
            .ToArray();
        Assert.AreEqual(
            1,
            sources.Length,
            "Generated source '" + hintName + "' was not emitted. Actual hints: " +
            string.Join(Environment.NewLine, runResult.Results.SelectMany(static result => result.GeneratedSources).Select(static source => source.HintName)));
        StringAssert.Contains(sources[0].SourceText.ToString(), expectedText);
    }

    private static void AssertNoCompilationErrors(Compilation compilation)
    {
        var errors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors.Select(static item => item.ToString())));
    }

    [Generator]
    private sealed class TailOutputTestGenerator(Action<SourceProductionContext> emit) : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterSourceOutput(
                context.CompilationProvider,
                (productionContext, _) => emit(productionContext));
        }
    }

    [Generator]
    private sealed class FallbackOutputTestGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
            => RazorSourceGeneratorFallbackOutput.Register(context);
    }

    private sealed class RecordingSfcLowerer : IRazorVueSfcArtifactLowerer
    {
        private readonly List<RazorVueSemanticSnapshot> _snapshots = [];

        public IReadOnlyList<RazorVueSemanticSnapshot> Snapshots => _snapshots;

        public VueSfcArtifact Lower(RazorVueCompilationContext context, RazorVueSemanticSnapshot snapshot)
        {
            _ = context;
            _snapshots.Add(snapshot);
            return CreateMinimalArtifact(snapshot);
        }
    }

    private static VueSfcArtifact CreateMinimalArtifact(RazorVueSemanticSnapshot snapshot)
    {
        var relativePath = snapshot.Descriptor.ImportSpecifier
            .TrimStart('.', '/')
            .Replace('\\', '/');
        if (!relativePath.EndsWith(".vue", StringComparison.OrdinalIgnoreCase))
            relativePath += ".vue";

        return new VueSfcArtifact(
            ComponentName: snapshot.Descriptor.Name,
            RelativeSfcPath: relativePath,
            SfcText: "<template><div /></template>\n",
            TemplateBlock: new VueSfcTemplateBlock("<div />\n", ImmutableArray<RazorVueSourceOrigin>.Empty),
            ScriptSetupBlock: new VueSfcScriptSetupBlock(string.Empty, null, ImmutableArray<RazorVueSourceOrigin>.Empty),
            ScriptBlock: new VueSfcScriptBlock(string.Empty, null, ImmutableArray<RazorVueSourceOrigin>.Empty),
            RenderMode: VueSfcArtifactRenderMode.Template,
            StyleBlocks: ImmutableArray<VueSfcStyleBlock>.Empty,
            CustomBlocks: ImmutableArray<VueSfcCustomBlock>.Empty,
            RouteTemplates: ImmutableArray<string>.Empty,
            Imports: ImmutableArray<string>.Empty,
            Styles: ImmutableArray<string>.Empty,
            PluginRequirements: ImmutableArray<string>.Empty,
            Identity: new VueSfcArtifactIdentity(
                snapshot.Descriptor.FullName,
                relativePath,
                "descriptor",
                "template",
                "logic",
                "style",
                HmrBoundaryKind.TemplateOnly),
            Hints: new VueRuntimeHints(
                RequiresVueRuntime: true,
                RequiresHydration: false,
                SupportsSsr: true,
                UsesTeleport: false,
                UsesSuspense: false,
                UsesKeepAlive: false),
            SourceOrigins: ImmutableArray<RazorVueSourceOrigin>.Empty);
    }

    private sealed class InMemoryAdditionalText(string path, string text) : AdditionalText
    {
        private readonly SourceText _text = SourceText.From(text);

        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
            => _text;
    }

    private readonly record struct AdditionalTextOptions(
        string Path,
        IReadOnlyDictionary<string, string> Options);

    private sealed class TestAnalyzerConfigOptionsProvider(
        IReadOnlyDictionary<string, string> globalOptions,
        ImmutableArray<AdditionalTextOptions> additionalTextOptions) : AnalyzerConfigOptionsProvider
    {
        private readonly AnalyzerConfigOptions _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _additionalTextOptions =
            additionalTextOptions.IsDefaultOrEmpty
                ? new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
                : additionalTextOptions.ToDictionary(
                    static item => item.Path,
                    static item => item.Options,
                    StringComparer.OrdinalIgnoreCase);
        private static readonly AnalyzerConfigOptions EmptyOptions =
            new TestAnalyzerConfigOptions(new Dictionary<string, string>(StringComparer.Ordinal));

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            _ = tree;
            return EmptyOptions;
        }

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
            => _additionalTextOptions.TryGetValue(textFile.Path, out var options)
                ? new TestAnalyzerConfigOptions(options)
                : EmptyOptions;
    }

    private sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, out string value)
            => values.TryGetValue(key, out value!);
    }
}
