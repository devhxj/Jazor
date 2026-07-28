using Jazor.RazorVue.Generator.Generation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSourceGeneratorTailOutputTests
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
                    global using static ECMAScript.Vue3;
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

        var result = RazorSourceGeneratorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var failure);

        Assert.IsTrue(result, failure);
        Assert.IsNotNull(catalogSource);
        StringAssert.Contains(catalogSource, "components/razor-counter.mjs");
        StringAssert.Contains(catalogSource, "components/handwritten-status.mjs");
        StringAssert.Contains(catalogSource, "function $renderDirect()");
        Assert.IsFalse(catalogSource.Contains("createRenderContext", StringComparison.Ordinal));
        Assert.IsFalse(catalogSource.Contains(".vue", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void TryBuildFinalCompilationCatalog_WithoutRazorVueComponent_DoesNotCreateCatalog()
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.Empty",
            [CSharpSyntaxTree.ParseText("internal static class EntryPoint { }")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var result = RazorSourceGeneratorTailOutput.TryBuildFinalCompilationCatalog(
            compilation,
            CancellationToken.None,
            out var catalogSource,
            out var failure);

        Assert.IsTrue(result, failure);
        Assert.IsNull(catalogSource);
    }
}
