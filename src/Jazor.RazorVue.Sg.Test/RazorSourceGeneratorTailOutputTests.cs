using Jazor.RazorVue.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorTailOutputTests
{
    [TestMethod]
    public void TryBuildFinalCompilationCatalog_MixedRazorAndHandwrittenComponents_EmitsBothModules()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.Mixed",
            [
                CSharpSyntaxTree.ParseText(
                    """
                    global using ECMAScript;
                    global using Microsoft.AspNetCore.Components;
                    global using Microsoft.AspNetCore.Components.Rendering;
                    global using static ECMAScript.Vue;
                    """,
                    parseOptions,
                    "GlobalUsings.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Demo.Pages;

                    [ECMAScriptModule("./components/razor-counter")]
                    public sealed class RazorCounter : ComponentBase, IVueComponent
                    {
                        protected override void BuildRenderTree(RenderTreeBuilder builder)
                        {
                            builder.AddContent(0, "razor");
                        }
                    }
                    """,
                    parseOptions,
                    "RazorCounter.razor.g.cs"),
                CSharpSyntaxTree.ParseText(
                    """
                    namespace Demo.Components;

                    [ECMAScriptModule("./components/handwritten-status")]
                    public sealed class HandwrittenStatus : ComponentBase, IVueComponent
                    {
                        protected override void BuildRenderTree(RenderTreeBuilder builder)
                        {
                            builder.AddContent(0, "handwritten");
                        }
                    }
                    """,
                    parseOptions,
                    "HandwrittenStatus.razor.cs")
            ],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));

        var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsTrue(result, DescribeDiagnostics(diagnostics));
        Assert.IsEmpty(diagnostics, DescribeDiagnostics(diagnostics));
        Assert.IsNotNull(catalogSource);
        StringAssert.Contains(catalogSource, "components/razor-counter.mjs");
        StringAssert.Contains(catalogSource, "components/handwritten-status.mjs");
        StringAssert.Contains(catalogSource, "internal static partial class ArtifactCatalog");
        StringAssert.Contains(catalogSource, "hmrProviderId: \"jazor.vue\"");
        StringAssert.Contains(catalogSource, "hmrModuleId:");
        StringAssert.Contains(catalogSource, "hmrPayload:");
        StringAssert.Contains(catalogSource, "descriptorHash");
        StringAssert.Contains(catalogSource, "templateHash");
        StringAssert.Contains(catalogSource, "logicHash");
        StringAssert.Contains(catalogSource, "boundaryKind");
        StringAssert.Contains(catalogSource, "function $renderDirect()");
        Assert.IsFalse(catalogSource.Contains("\"module-source\"", StringComparison.Ordinal));
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_WithoutRazorVueComponent_DoesNotCreateCatalog()
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.Empty",
            [CSharpSyntaxTree.ParseText("internal static class EntryPoint { }")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsTrue(result, DescribeDiagnostics(diagnostics));
        Assert.IsEmpty(diagnostics, DescribeDiagnostics(diagnostics));
        Assert.IsNull(catalogSource);
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_ReportsDirectRenderFailureWithComponentIdentity()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.DynamicTag",
            [CSharpSyntaxTree.ParseText(
                """
                using ECMAScript;
                using Microsoft.AspNetCore.Components;
                using Microsoft.AspNetCore.Components.Rendering;
                using static ECMAScript.Vue;

                namespace Demo.Pages;

                [ECMAScriptModule("./components/dynamic-tag")]
                public sealed class DynamicTag : ComponentBase, IVueComponent
                {
                    [Parameter] public string TagName { get; set; } = "section";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, TagName);
                        builder.CloseElement();
                    }
                }
                """,
                parseOptions,
                "Pages/DynamicTag.razor.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));

        var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsFalse(result);
        Assert.IsNull(catalogSource);
        Assert.HasCount(1, diagnostics, DescribeDiagnostics(diagnostics));
        var diagnostic = diagnostics.Single();
        Assert.AreEqual(RazorVueDiagnosticCategory.DirectRender, diagnostic.Category);
        Assert.AreEqual("global::Demo.Pages.DynamicTag", diagnostic.ComponentId);
        Assert.AreEqual(RazorVueDiagnosticSourceKind.AuthoredCSharp, diagnostic.SourceKind);
        StringAssert.Contains(diagnostic.Message, "OpenElement tag names must be compile-time strings", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_RejectsExpressionBodiedBuildRenderTreeAtBindingBoundary()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.ExpressionBodied",
            [CSharpSyntaxTree.ParseText(
                """
                using ECMAScript;
                using Microsoft.AspNetCore.Components;
                using Microsoft.AspNetCore.Components.Rendering;
                using static ECMAScript.Vue;

                namespace Demo.Pages;

                [ECMAScriptModule("./components/expression-bodied")]
                public sealed class ExpressionBodied : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                        => builder.AddContent(0, "expression-bodied");
                }
                """,
                parseOptions,
                "Pages/ExpressionBodied.razor.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));

        var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var diagnostics);

        Assert.IsFalse(result);
        Assert.IsNull(catalogSource);
        Assert.HasCount(1, diagnostics, DescribeDiagnostics(diagnostics));
        var diagnostic = diagnostics.Single();
        Assert.AreEqual(RazorVueDiagnosticCategory.ComponentBinding, diagnostic.Category);
        Assert.AreEqual("global::Demo.Pages.ExpressionBodied", diagnostic.ComponentId);
        StringAssert.Contains(diagnostic.Message, "did not expose a bindable BuildRenderTree body", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_ParallelArtifactBuildsRemainByteForByteDeterministic()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.DeterministicParallel",
            [CSharpSyntaxTree.ParseText(
                """
                global using ECMAScript;
                global using Microsoft.AspNetCore.Components;
                global using Microsoft.AspNetCore.Components.Rendering;
                global using static ECMAScript.Vue;

                namespace Demo.Pages;

                [ECMAScriptModule("./components/alpha")]
                public sealed class Alpha : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, "alpha");
                        builder.CloseElement();
                    }
                }

                [ECMAScriptModule("./components/bravo")]
                public sealed class Bravo : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, "bravo");
                        builder.CloseElement();
                    }
                }

                [ECMAScriptModule("./components/charlie")]
                public sealed class Charlie : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, "charlie");
                        builder.CloseElement();
                    }
                }

                [ECMAScriptModule("./components/delta")]
                public sealed class Delta : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1, "delta");
                        builder.CloseElement();
                    }
                }
                """,
                parseOptions,
                "Pages/ParallelComponents.razor.g.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));

        string? baseline = null;
        for (var attempt = 0; attempt < 8; attempt++)
        {
            var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
                compilation,
                CancellationToken.None,
                out var catalogSource,
                out var diagnostics);

            Assert.IsTrue(result, DescribeDiagnostics(diagnostics));
            Assert.IsEmpty(diagnostics, DescribeDiagnostics(diagnostics));
            Assert.IsNotNull(catalogSource);
            baseline ??= catalogSource;
            Assert.AreEqual(baseline, catalogSource, "parallel artifact catalog changed on attempt " + attempt);
        }
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_ParallelArtifactFailuresUseStableComponentOrder()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.StableParallelFailure",
            [CSharpSyntaxTree.ParseText(
                """
                global using ECMAScript;
                global using Microsoft.AspNetCore.Components;
                global using Microsoft.AspNetCore.Components.Rendering;
                global using static ECMAScript.Vue;

                namespace Demo.Pages;

                [ECMAScriptModule("./components/alpha-invalid")]
                public sealed class AlphaInvalid : ComponentBase, IVueComponent
                {
                    [Parameter] public string TagName { get; set; } = "section";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, TagName);
                        builder.CloseElement();
                    }
                }

                [ECMAScriptModule("./components/zeta-invalid")]
                public sealed class ZetaInvalid : ComponentBase, IVueComponent
                {
                    [Parameter] public string TagName { get; set; } = "section";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, TagName);
                        builder.CloseElement();
                    }
                }
                """,
                parseOptions,
                "Pages/ParallelInvalidComponents.razor.g.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.AreEqual(0, errors.Length, string.Join(Environment.NewLine, errors));

        for (var attempt = 0; attempt < 8; attempt++)
        {
            var result = RazorTailOutput.TryBuildFinalCompilationCatalog(
                compilation,
                CancellationToken.None,
                out var catalogSource,
                out var diagnostics);

            Assert.IsFalse(result);
            Assert.IsNull(catalogSource);
            Assert.HasCount(2, diagnostics, DescribeDiagnostics(diagnostics));
            CollectionAssert.AreEqual(
                new[] { "global::Demo.Pages.AlphaInvalid", "global::Demo.Pages.ZetaInvalid" },
                diagnostics.Select(static diagnostic => diagnostic.ComponentId).ToArray());
            Assert.IsTrue(diagnostics.All(static diagnostic => diagnostic.Category == RazorVueDiagnosticCategory.DirectRender));
            Assert.IsTrue(diagnostics.All(static diagnostic => diagnostic.PrimaryLocation != Location.None));
        }
    }

    private static string DescribeDiagnostics(IEnumerable<RazorVueDiagnosticInfo> diagnostics)
        => string.Join(
            Environment.NewLine,
            diagnostics.Select(static diagnostic =>
                Diagnostics.GetDescriptor(diagnostic.Category).Id + ": " + diagnostic.Message));
}
