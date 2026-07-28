using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgGeneratedCSharpBinderInvariantTests
{
    [TestMethod]
    public void TryBindFinalCompilation_ReusesGeneratedRazorTreeWithoutParsingItAgain()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
            }
            """,
            parseOptions,
            "Pages/Counter.razor.cs");
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public partial class Counter
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "counter");
                }
            }
            """,
            parseOptions,
            "Pages/Counter.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.Binder",
            [authoredTree, generatedTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var candidates = RazorSgComponentCandidateSelector.DiscoverTailRequiredComponents(compilation);

        var result = RazorSgGeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            candidates,
            out var binding,
            out var failure);

        Assert.IsTrue(result, failure);
        Assert.IsNotNull(binding);
        Assert.AreSame(compilation, binding!.Compilation);
        Assert.AreEqual(RazorSgCompilationBindingMode.ReusedHookCompilation, binding.BindingMode);
        Assert.AreEqual(1, binding.Components.Length);
        Assert.AreSame(generatedTree, binding.Components[0].BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree);
        Assert.AreEqual(0, binding.DerivedGeneratedTreeCount);
    }
}
