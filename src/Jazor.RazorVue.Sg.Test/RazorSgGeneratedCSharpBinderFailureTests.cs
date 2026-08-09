using System.Collections.Immutable;
using System.Reflection;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class GeneratedCSharpBinderFailureTests
{
    [TestMethod]
    public void TryBindMethods_RejectNullCompilation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            GeneratedCSharpBinder.TryBindHandwritten(
                null!,
                ImmutableArray<INamedTypeSymbol>.Empty,
                out _,
                out _));
        Assert.Throws<ArgumentNullException>(() =>
            GeneratedCSharpBinder.TryBindFinalCompilation(
                null!,
                ImmutableArray<INamedTypeSymbol>.Empty,
                out _,
                out _));
    }

    [TestMethod]
    public void TryBindFinalCompilation_PrefersPhysicalRazorPartialSourceWhenNoMappedSpanExists()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/physical")]
            public partial class Physical : ComponentBase, IVueComponent
            {
            }
            """,
            parseOptions,
            "Pages/Physical.razor");
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public partial class Physical
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "physical");
                }
            }
            """,
            parseOptions,
            "Pages/Physical.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.GeneratedCSharpBinder.PhysicalRazorPath",
            [authoredTree, generatedTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var component = compilation.GetTypeByMetadataName("Demo.Pages.Physical");

        Assert.IsNotNull(component);
        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(component!),
            out var binding,
            out var failure), failure);
        Assert.IsNotNull(binding);
        Assert.AreEqual("Pages/Physical.razor", binding!.Documents.Single().SourcePath);
    }

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

    [TestMethod]
    public void TryBindBuildRenderTreeBody_RejectsMetadataOnlyMethodWithoutSourceDeclaration()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/metadata-only-render")]
            public sealed class MetadataOnlyRender : ComponentBase;
            """,
            "Pages/MetadataOnlyRender.razor.cs");
        var component = compilation.GetTypeByMetadataName("Demo.Pages.MetadataOnlyRender");
        var baseComponent = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");

        Assert.IsNotNull(component);
        Assert.IsNotNull(baseComponent);
        var buildRenderTree = baseComponent!
            .GetMembers("BuildRenderTree")
            .OfType<IMethodSymbol>()
            .Single();
        var bindMethod = typeof(GeneratedCSharpBinder).GetMethod(
            "TryBindBuildRenderTreeBody",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(bindMethod);

        var arguments = new object?[]
        {
            compilation,
            component!,
            buildRenderTree,
            new Dictionary<SyntaxTree, GeneratedDocument>(),
            ImmutableArray.CreateBuilder<GeneratedDocument>(),
            null,
            null
        };
        Assert.IsFalse((bool)bindMethod!.Invoke(null, arguments)!);
        StringAssert.Contains((string)arguments[6]!, "did not expose a bindable BuildRenderTree body", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryBindHandwritten_RejectsCandidateWithoutHandwrittenBuildRenderTree()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/missing-handwritten-render")]
            public sealed class MissingHandwrittenRender : ComponentBase, IVueComponent
            {
            }
            """,
            "Pages/MissingHandwrittenRender.razor.cs");
        var component = compilation.GetTypeByMetadataName("Demo.Pages.MissingHandwrittenRender");

        Assert.IsNotNull(component);
        Assert.IsFalse(GeneratedCSharpBinder.TryBindHandwritten(
            compilation,
            ImmutableArray.Create(component!),
            out var binding,
            out var failure));

        Assert.IsNull(binding);
        StringAssert.Contains(failure, "Demo.Pages.MissingHandwrittenRender", StringComparison.Ordinal);
        StringAssert.Contains(failure, "did not declare a handwritten BuildRenderTree", StringComparison.Ordinal);
    }

    private static CSharpCompilation CreateCompilation(string source, string path)
        => CSharpCompilation.Create(
            "RazorVue.GeneratedCSharpBinder.FailureTests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path)],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
