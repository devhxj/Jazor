using System.Collections.Immutable;
using Jazor.Analyzer.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorSourceGeneratorTailOutputTests
{
    [TestMethod]
    public void Emit_WhenEnabledAndInputIsUnreadableWithoutRazorVueCandidate_DoesNotReportTailFailureDiagnostic()
    {
        var diagnostics = RunTailOutputGenerator(static (context, compilation) =>
        {
            RazorSourceGeneratorTailOutput.Emit(
                context,
                compilation,
                source: new object(),
                new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: false));
        });

        AssertNoTailFailureDiagnostic(diagnostics);
    }

    [TestMethod]
    public void Emit_WhenEnabledAndGeneratorDocumentsAreEmptyWithoutRazorVueCandidate_DoesNotReportTailFailureDiagnostic()
    {
        var diagnostics = RunTailOutputGenerator(static (context, compilation) =>
        {
            RazorSourceGeneratorTailOutput.Emit<object>(
                context,
                compilation,
                ImmutableArray<object>.Empty,
                new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: false));
        });

        AssertNoTailFailureDiagnostic(diagnostics);
    }

    [TestMethod]
    public void Emit_WhenEnabledAndInputIsUnreadableWithRazorVueCandidate_ReportsTailFailureDiagnostic()
    {
        var diagnostics = RunTailOutputGenerator(
            static (context, compilation) =>
            {
                RazorSourceGeneratorTailOutput.Emit(
                    context,
                    compilation,
                    source: new object(),
                    new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: false));
            },
            includeRazorVueCandidate: true);

        AssertTailFailureDiagnostic(
            diagnostics,
            "could not read the Razor SG tail output input");
    }

    [TestMethod]
    public void Emit_WhenEnabledAndGeneratorDocumentsAreEmptyWithRazorVueCandidate_ReportsTailFailureDiagnostic()
    {
        var diagnostics = RunTailOutputGenerator(
            static (context, compilation) =>
            {
                RazorSourceGeneratorTailOutput.Emit<object>(
                    context,
                    compilation,
                    ImmutableArray<object>.Empty,
                    new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: false));
            },
            includeRazorVueCandidate: true);

        AssertTailFailureDiagnostic(
            diagnostics,
            "did not receive any Razor source generator documents");
    }

    [TestMethod]
    public void Emit_WhenEnabledWithHandwrittenBuildRenderTree_DoesNotRequireTailOutput()
    {
        var diagnostics = RunTailOutputGenerator(
            static (context, compilation) =>
            {
                RazorSourceGeneratorTailOutput.Emit(
                    context,
                    compilation,
                    source: new object(),
                    new RazorSourceGeneratorTailOutputOptions(Enabled: true, TestHookEnabled: false));
            },
            includeRazorVueCandidate: true,
            includeHandwrittenBuildRenderTree: true);

        AssertNoTailFailureDiagnostic(diagnostics);
    }

    private static ImmutableArray<Diagnostic> RunTailOutputGenerator(
        Action<SourceProductionContext, Compilation> emit,
        bool includeRazorVueCandidate = false,
        bool includeHandwrittenBuildRenderTree = false)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.RazorSourceGeneratorTailOutput.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    includeHandwrittenBuildRenderTree ? RazorVueCandidateWithHandwrittenBuildRenderTreeSource :
                    includeRazorVueCandidate ? RazorVueCandidateSource : EmptySource,
                    options: parseOptions,
                    path: includeRazorVueCandidate ? "EntryPoint.razor.cs" : "EntryPoint.cs")
            ],
            references: RazorIrTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new TailOutputTestGenerator(emit).AsSourceGenerator()],
            parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);
        return diagnostics;
    }

    private static void AssertNoTailFailureDiagnostic(ImmutableArray<Diagnostic> diagnostics)
    {
        Assert.IsFalse(
            diagnostics.Any(static item => item.Id == "JAZORVGA020"),
            "Did not expect JAZORVGA020. Actual diagnostics: " + string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
    }

    private static void AssertTailFailureDiagnostic(
        ImmutableArray<Diagnostic> diagnostics,
        string expectedMessage)
    {
        var diagnostic = diagnostics.SingleOrDefault(static item => item.Id == "JAZORVGA020");
        Assert.IsNotNull(
            diagnostic,
            "Expected JAZORVGA020. Actual diagnostics: " + string.Join(Environment.NewLine, diagnostics.Select(static item => item.ToString())));
        StringAssert.Contains(diagnostic!.GetMessage(), expectedMessage);
    }

    private const string EmptySource = """
        internal static class EntryPoint
        {
        }
        """;

    private const string RazorVueCandidateSource = """
        using ECMAScript;
        using static ECMAScript.Vue3;
        using Microsoft.AspNetCore.Components;

        [ECMAScriptModule("./components/counter")]
        public partial class Counter : ComponentBase, IVueComponent
        {
        }
        """;

    private const string RazorVueCandidateWithHandwrittenBuildRenderTreeSource = """
        using ECMAScript;
        using static ECMAScript.Vue3;
        using Microsoft.AspNetCore.Components;
        using Microsoft.AspNetCore.Components.Rendering;

        [ECMAScriptModule("./components/counter")]
        public partial class Counter : ComponentBase, IVueComponent
        {
            protected override void BuildRenderTree(RenderTreeBuilder builder)
            {
                builder.AddContent(0, "handwritten");
            }
        }
        """;

    [Generator]
    private sealed class TailOutputTestGenerator(
        Action<SourceProductionContext, Compilation> emit) : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var compilationProvider = context.CompilationProvider;
            context.RegisterSourceOutput(
                compilationProvider,
                (sourceProductionContext, compilation) => emit(sourceProductionContext, compilation));
        }
    }
}
