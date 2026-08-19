using System.Collections.Immutable;
using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorTailOutputPipelineContractTests
{
    [TestMethod]
    public void TryBuildFinalCompilationCatalog_ReportsInternalFailureForMissingCompilation()
    {
        var built = RazorTailOutput.TryBuildFinalCompilationCatalog(
            null!,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsFalse(built);
        Assert.IsNull(catalogSource);
        Assert.HasCount(1, diagnostics);
        Assert.AreEqual(RazorVueDiagnosticCategory.Internal, diagnostics[0].Category);
        StringAssert.Contains(diagnostics[0].Message, "compilation", StringComparison.OrdinalIgnoreCase);
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_ReportsOneVueInjectDiagnosticBeforeArtifactFanOut()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using ECMAScript.VueContract;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            [assembly: VueInject(typeof(string[]), typeof(string))]

            namespace Demo.Pages;

            [ECMAScriptModule("./components/invalid-inject-page")]
            public sealed class InvalidInjectPage : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "invalid inject metadata");
                }
            }
            """,
            "Pages/InvalidInjectPage.razor.cs");

        var built = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsFalse(built);
        Assert.IsNull(catalogSource);
        Assert.HasCount(1, diagnostics);
        Assert.AreEqual(RazorVueDiagnosticCategory.VueInject, diagnostics[0].Category);
        StringAssert.Contains(diagnostics[0].Message, "contract argument", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_DeduplicatesRepeatedDirectComponentImportsAndPreservesVueAssets()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/repeated-child.vue")]
            public sealed class RepeatedChild : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "child");
                }
            }

            [ECMAScriptModule("./components/repeated-parent")]
            public sealed class RepeatedParent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenComponent<RepeatedChild>(0);
                    builder.CloseComponent();
                    builder.OpenComponent<RepeatedChild>(1);
                    builder.CloseComponent();
                }
            }
            """,
            "Pages/RepeatedParent.razor.cs");

        var built = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsTrue(built, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.Message)));
        Assert.IsNotNull(catalogSource);
        Assert.IsEmpty(diagnostics);
        StringAssert.Contains(catalogSource, "repeated-parent.mjs", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "repeated-child.vue", StringComparison.Ordinal);
    }

    private static CSharpCompilation CreateCompilation(string source, string path)
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.RazorTailOutput.Contract." + Guid.NewGuid().ToString("N"),
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path)],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }
}
