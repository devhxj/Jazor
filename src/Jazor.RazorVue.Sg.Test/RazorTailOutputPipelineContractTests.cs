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
        var timing = new RazorCompilationTiming();

        var built = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics,
            timing: timing);

        Assert.IsFalse(built);
        Assert.IsNull(catalogSource);
        Assert.HasCount(1, diagnostics);
        Assert.AreEqual(RazorVueDiagnosticCategory.VueInject, diagnostics[0].Category);
        StringAssert.Contains(diagnostics[0].Message, "contract argument", StringComparison.Ordinal);
        Assert.AreEqual(1, timing.ComponentCount);
        Assert.AreEqual("inject-validation", timing.Stages.Last().Key);
        Assert.IsFalse(timing.Stages.Any(static stage => stage.Key == "artifact-emission"));
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
        var timing = new RazorCompilationTiming();

        var built = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics,
            timing: timing);

        Assert.IsTrue(built, string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.Message)));
        Assert.IsNotNull(catalogSource);
        Assert.IsEmpty(diagnostics);
        StringAssert.Contains(catalogSource, "repeated-parent.js", StringComparison.Ordinal);
        StringAssert.Contains(catalogSource, "repeated-child.vue", StringComparison.Ordinal);
        Assert.AreEqual(2, timing.ComponentCount);
        CollectionAssert.AreEqual(new[]
        {
            "component-discovery", "render-binding", "member-closures", "inject-validation",
            "artifact-emission", "route-catalog", "catalog-serialization"
        }, timing.Stages.Select(static stage => stage.Key).ToArray());
        Assert.IsTrue(timing.Stages.All(static stage => stage.Value >= 0));

        Assert.IsTrue(RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation, CancellationToken.None, out var untimedCatalog, out var untimedDiagnostics));
        Assert.IsEmpty(untimedDiagnostics);
        Assert.AreEqual(untimedCatalog, catalogSource, "Timing must not enter generated module/catalog bytes.");
    }

    [TestMethod]
    public void CompilationTiming_FormatsBuildLogWithInvariantMilliseconds()
    {
        var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
            var timing = new RazorCompilationTiming { ComponentCount = 3 };
            timing.CompleteStage("generators");
            var line = timing.Format("Demo.Authoring", succeeded: true);

            StringAssert.Contains(line, "assembly=Demo.Authoring; components=3; status=completed; total=", StringComparison.Ordinal);
            StringAssert.Matches(line, new System.Text.RegularExpressions.Regex("total=[0-9]+\\.[0-9] ms; generators=[0-9]+\\.[0-9] ms$"));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = previousCulture;
        }
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
