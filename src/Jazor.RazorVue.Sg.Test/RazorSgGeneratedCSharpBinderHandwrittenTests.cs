using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class GeneratedCSharpBinderHandwrittenTests
{
    [TestMethod]
    public void TryBindHandwritten_ReusesCurrentCompilationBuildRenderTreeBody()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

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
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        var result = GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var failure);

        Assert.IsTrue(result, failure);
        Assert.IsNotNull(binding);
        Assert.AreSame(compilation, binding!.Compilation);
        Assert.AreEqual(CompilationBindingMode.ReusedHookCompilation, binding.BindingMode);
        Assert.AreEqual(1, binding.Documents.Length);
        Assert.AreSame(sourceTree, binding.Components.Single().BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree);
        Assert.AreSame(binding.Documents.Single(), binding.Components.Single().Document);
        Assert.AreEqual(3, binding.Components.Single().BuildRenderTreeBody.Operations.Length);
        Assert.AreEqual(1, binding.ReusedGeneratedTreeCount);
        Assert.AreEqual(0, binding.DerivedGeneratedTreeCount);
    }

    [TestMethod]
    public void TryBindHandwritten_OrdersComponentsAndSharesTheirSourceDocument()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/zebra")]
            public sealed class Zebra : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "zebra");
                }
            }

            [ECMAScriptModule("./components/alpha")]
            public sealed class Alpha : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "alpha");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Pages/Components.razor.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.HandwrittenCompilation.StableOrder",
            [sourceTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var alpha = compilation.GetTypeByMetadataName("Demo.Pages.Alpha");
        var zebra = compilation.GetTypeByMetadataName("Demo.Pages.Zebra");

        Assert.IsNotNull(alpha);
        Assert.IsNotNull(zebra);
        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            ImmutableArray.Create(zebra!, alpha!),
            out var binding,
            out var failure), failure);
        Assert.IsNotNull(binding);

        CollectionAssert.AreEqual(
            new[] { "Demo.Pages.Alpha", "Demo.Pages.Zebra" },
            binding!.Components.Select(static component => component.ComponentSymbol.ToDisplayString()).ToArray());
        Assert.AreEqual(1, binding.Documents.Length);
        Assert.AreSame(binding.Documents[0], binding.Components[0].Document);
        Assert.AreSame(binding.Documents[0], binding.Components[1].Document);
        Assert.AreEqual("Pages/Components.razor.cs", binding.Documents[0].SourcePath);
        Assert.AreEqual(1, binding.ReusedGeneratedTreeCount);
    }

    [TestMethod]
    public void TryBuildHandwrittenClosure_BlockBodiedComputedPropertyRemainsExecutableMember()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue;

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
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();

        Assert.IsTrue(MemberClosureBuilder.TryBuild(
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
            using static ECMAScript.Vue;

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
        var components = ComponentSelector.DiscoverHandwrittenComponents(compilation);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        var component = binding!.Components.Single();
        Assert.IsTrue(MemberClosureBuilder.TryBuild(
            binding,
            component,
            out var closure,
            out var closureFailure), closureFailure);
        Assert.IsNotNull(closure);

        var artifact = await VueModuleBuilder.BuildAsync(binding, component, closure!);
        var script = artifact.ModuleText.ReplaceLineEndings("\n");

        StringAssert.Contains(script, "function Label()", StringComparison.Ordinal);
        StringAssert.Contains(script, "return state.count.toString();", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("this.count", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("createRenderContext", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("builder.finish()", StringComparison.Ordinal), script);
    }
}
