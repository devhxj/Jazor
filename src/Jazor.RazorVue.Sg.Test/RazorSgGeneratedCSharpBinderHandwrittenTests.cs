using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgGeneratedCSharpBinderHandwrittenTests
{
    [TestMethod]
    public void TryBindHandwritten_ReusesCurrentCompilationBuildRenderTreeBody()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "section");
                    builder.AddContent(1, "handwritten");
                    builder.CloseElement();
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Counter.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenCompilation.Binder",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = RazorSgComponentCandidateSelector.DiscoverHandwrittenComponents(compilation);

        var result = RazorSgGeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var failure);

        Assert.IsTrue(result, failure);
        Assert.IsNotNull(binding);
        Assert.AreSame(compilation, binding!.Compilation);
        Assert.AreEqual(RazorSgCompilationBindingMode.ReusedHookCompilation, binding.BindingMode);
        Assert.AreEqual(1, binding.Documents.Length);
        Assert.AreSame(sourceTree, binding.Components.Single().BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree);
        Assert.AreSame(binding.Documents.Single(), binding.Components.Single().Document);
        Assert.AreEqual(3, binding.Components.Single().BuildRenderTreeBody.Operations.Length);
        Assert.AreEqual(1, binding.ReusedGeneratedTreeCount);
        Assert.AreEqual(0, binding.DerivedGeneratedTreeCount);
    }

    [TestMethod]
    public void TryBuildHandwrittenClosure_BlockBodiedComputedPropertyRemainsExecutableMember()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private int count = 2;

                private string Label
                {
                    get
                    {
                        return count.ToString();
                    }
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, Label);
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Counter.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenClosure.ComputedProperty",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = RazorSgComponentCandidateSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(RazorSgGeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();

        Assert.IsTrue(RazorSgComponentMemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);
        CollectionAssert.AreEqual(new[] { "Label" }, closure!.ComputedProperties.Select(static property => property.Name).ToArray());
        Assert.IsFalse(closure.StateProperties.Any(static property => property.Name == "Label"));
        Assert.IsTrue(closure.CreateMemberFilter()(component.ComponentSymbol.GetMembers("get_Label").OfType<IMethodSymbol>().Single()));
    }

    [TestMethod]
    public async Task BuildHandwrittenArtifact_BlockBodiedComputedPropertyLowersThroughComponentModule()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/counter")]
            public partial class Counter : ComponentBase, IVueComponent
            {
                private int count = 2;

                private string Label
                {
                    get
                    {
                        return count.ToString();
                    }
                }

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.OpenElement(0, "span");
                    builder.AddContent(1, Label);
                    builder.CloseElement();
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Counter.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenArtifact.ComputedProperty",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var components = RazorSgComponentCandidateSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(RazorSgGeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();
        Assert.IsTrue(RazorSgComponentMemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await RazorSgVueComponentModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "function label()", StringComparison.Ordinal);
        StringAssert.Contains(script, "return state.count.toString();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("this.count", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("createRenderContext", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish()", StringComparison.Ordinal), script);
    }
}
