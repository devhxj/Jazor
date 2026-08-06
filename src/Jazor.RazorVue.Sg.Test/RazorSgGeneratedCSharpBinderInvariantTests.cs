using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class GeneratedCSharpBinderInvariantTests
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
        var candidates = ComponentSelector.DiscoverTailRequiredComponents(compilation);

        var result = GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            candidates,
            out var binding,
            out var failure);

        Assert.IsTrue(result, failure);
        Assert.IsNotNull(binding);
        Assert.AreSame(compilation, binding!.Compilation);
        Assert.AreEqual(CompilationBindingMode.ReusedHookCompilation, binding.BindingMode);
        Assert.AreEqual(1, binding.Components.Length);
        Assert.AreSame(generatedTree, binding.Components[0].BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree);
        Assert.AreEqual(0, binding.DerivedGeneratedTreeCount);
    }

    [TestMethod]
    public void TryBindFinalCompilation_OrdersMultipleGeneratedComponentsIndependentlyOfCallerOrder()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/alpha")]
            public partial class Alpha : ComponentBase, IVueComponent
            {
            }

            [ECMAScriptModule("./components/zebra")]
            public partial class Zebra : ComponentBase, IVueComponent
            {
            }
            """,
            parseOptions,
            "Pages/Components.razor.cs");
        var zebraTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public partial class Zebra
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "zebra");
                }
            }
            """,
            parseOptions,
            "Pages/Zebra.razor.g.cs");
        var alphaTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public partial class Alpha
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "alpha");
                }
            }
            """,
            parseOptions,
            "Pages/Alpha.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.StableOrder",
            [authoredTree, zebraTree, alphaTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var alpha = compilation.GetTypeByMetadataName("Demo.Pages.Alpha");
        var zebra = compilation.GetTypeByMetadataName("Demo.Pages.Zebra");

        Assert.IsNotNull(alpha);
        Assert.IsNotNull(zebra);
        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(zebra!, alpha!),
            out var binding,
            out var failure), failure);
        Assert.IsNotNull(binding);

        CollectionAssert.AreEqual(
            new[] { "Demo.Pages.Alpha", "Demo.Pages.Zebra" },
            binding!.Components.Select(static component => component.ComponentSymbol.ToDisplayString()).ToArray());
        CollectionAssert.AreEqual(
            new[] { "Alpha.razor.g.cs", "Zebra.razor.g.cs" },
            binding.Documents.Select(static document => document.HintName).ToArray());
        Assert.AreEqual(2, binding.ReusedGeneratedTreeCount);
        Assert.IsTrue(binding.Components.All(static component =>
            string.Equals(
                component.Document.SourcePath,
                component.BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree.FilePath,
                StringComparison.Ordinal)));
    }
}
