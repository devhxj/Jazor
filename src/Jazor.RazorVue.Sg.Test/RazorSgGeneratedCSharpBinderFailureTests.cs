using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class GeneratedCSharpBinderFailureTests
{
    [TestMethod]
    public void TryBindFinalCompilation_RejectsCandidateWithoutGeneratedBuildRenderTree()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/missing-render")]
            public partial class MissingRender : ComponentBase, IVueComponent
            {
            }
            """,
            "Pages/MissingRender.razor.cs");
        var component = compilation.GetTypeByMetadataName("Demo.Pages.MissingRender");

        Assert.IsNotNull(component);
        Assert.IsFalse(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(component!),
            out var binding,
            out var failure));

        Assert.IsNull(binding);
        StringAssert.Contains(failure, "Demo.Pages.MissingRender", StringComparison.Ordinal);
        StringAssert.Contains(failure, "did not declare BuildRenderTree(RenderTreeBuilder)", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryBindHandwritten_RejectsExpressionBodiedBuildRenderTree()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/expression-bodied-render")]
            public sealed class ExpressionBodiedRender : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "content");
            }
            """,
            "Pages/ExpressionBodiedRender.razor.cs");
        var component = compilation.GetTypeByMetadataName("Demo.Pages.ExpressionBodiedRender");

        Assert.IsNotNull(component);
        Assert.IsFalse(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            ImmutableArray.Create(component!),
            out var binding,
            out var failure));

        Assert.IsNull(binding);
        StringAssert.Contains(failure, "Demo.Pages.ExpressionBodiedRender", StringComparison.Ordinal);
        StringAssert.Contains(failure, "did not expose a bindable BuildRenderTree body", StringComparison.Ordinal);
    }

    private static CSharpCompilation CreateCompilation(string source, string path)
        => CSharpCompilation.Create(
            "RazorVue.GeneratedCSharpBinder.FailureTests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path)],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
