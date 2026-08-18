using System.Collections.Immutable;
using Jazor.RazorVue.Generation;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class GeneratedCSharpBinderInvariantTests
{
    [TestMethod]
    public void TryBindFinalCompilation_CollectsMissingAndUnbindableRenderRootDiagnostics()
    {
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public sealed class MissingRenderRoot
            {
            }

            public sealed class ExpressionRenderRoot
            {
                public void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "expression");
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            "Pages/InvalidRoots.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.InvalidRoots",
            [generatedTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var missing = compilation.GetTypeByMetadataName("Demo.Pages.MissingRenderRoot");
        var expression = compilation.GetTypeByMetadataName("Demo.Pages.ExpressionRenderRoot");

        Assert.IsNotNull(missing);
        Assert.IsNotNull(expression);
        Assert.IsFalse(GeneratedCSharpBinder.TryBindFinalCompilationWithDiagnostics(
            compilation,
            ImmutableArray.Create(missing!, expression!),
            out var binding,
            out var diagnostics));

        Assert.IsNull(binding);
        Assert.HasCount(2, diagnostics);
        Assert.AreEqual(RazorVueDiagnosticCategory.ComponentBinding, diagnostics[0].Category);
        Assert.AreEqual(RazorVueDiagnosticCategory.ComponentBinding, diagnostics[1].Category);
        StringAssert.Contains(diagnostics[0].Message, "ExpressionRenderRoot", StringComparison.Ordinal);
        StringAssert.Contains(diagnostics[0].Message, "bindable BuildRenderTree body", StringComparison.Ordinal);
        StringAssert.Contains(diagnostics[1].Message, "MissingRenderRoot", StringComparison.Ordinal);
        StringAssert.Contains(diagnostics[1].Message, "did not declare BuildRenderTree", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TryBindFinalCompilation_ProducesAnEmptyBindingForAnEmptyCandidateSet()
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.EmptyBinding",
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray<INamedTypeSymbol>.Empty,
            out var binding,
            out var failure), failure);
        Assert.IsNotNull(binding);
        Assert.IsEmpty(binding!.Documents);
        Assert.IsEmpty(binding.Components);
    }

    [TestMethod]
    public void TryBindFinalCompilation_ReusesOneGeneratedDocumentForComponentsInTheSameTree()
    {
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public sealed class Alpha : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "alpha");
                }
            }

            public sealed class Zebra : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "zebra");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            "Pages/Shared.razor.g.cs");
        var compilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.SharedGeneratedDocument",
            [generatedTree],
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
        Assert.HasCount(1, binding!.Documents);
        CollectionAssert.AreEqual(
            new[] { "Demo.Pages.Alpha", "Demo.Pages.Zebra" },
            binding.Components.Select(static component => component.ComponentSymbol.ToDisplayString()).ToArray());
        Assert.AreSame(binding.Documents[0], binding.Components[0].Document);
        Assert.AreSame(binding.Documents[0], binding.Components[1].Document);
    }

    [TestMethod]
    public void TryBindFinalCompilation_ReusesGeneratedRazorTreeWithoutParsingItAgain()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue;
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
        Assert.AreEqual(1, binding.Documents.Length);
        Assert.AreEqual(1, binding.Components.Length);
        Assert.AreSame(generatedTree, binding.Components[0].BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree);
    }

    [TestMethod]
    public void TryBindFinalCompilation_OrdersMultipleGeneratedComponentsIndependentlyOfCallerOrder()
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue;

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
        Assert.IsTrue(binding.Components.All(static component =>
            string.Equals(
                component.Document.SourcePath,
                component.BuildRenderTreeMethod.DeclaringSyntaxReferences.Single().SyntaxTree.FilePath,
                StringComparison.Ordinal)));
    }

    [TestMethod]
    public void TryBindFinalCompilation_PreservesMappedRazorIdentityAndGeneratedFallbackPath()
    {
        var mappedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public sealed class MappedComponent : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
            #line 7 "Pages/MappedComponent.razor"
                    builder.AddContent(0, "mapped");
            #line default
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            "Pages/MappedComponent.razor.g.cs");
        var mappedCompilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.MappedSource",
            [mappedTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var mappedComponent = mappedCompilation.GetTypeByMetadataName("Demo.Pages.MappedComponent");
        Assert.IsNotNull(mappedComponent);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            mappedCompilation,
            ImmutableArray.Create(mappedComponent!),
            out var mappedBinding,
            out var mappedFailure), mappedFailure);
        Assert.IsNotNull(mappedBinding);
        Assert.AreEqual("Pages/MappedComponent.razor", mappedBinding!.Documents.Single().SourcePath);

        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Pages;

            public sealed class GeneratedOnlyComponent : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "generated");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            "Pages/GeneratedOnlyComponent.razor.g.cs");
        var generatedCompilation = CSharpCompilation.Create(
            "RazorVue.FinalCompilation.GeneratedFallback",
            [generatedTree],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var generatedComponent = generatedCompilation.GetTypeByMetadataName("Demo.Pages.GeneratedOnlyComponent");
        Assert.IsNotNull(generatedComponent);

        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            generatedCompilation,
            ImmutableArray.Create(generatedComponent!),
            out var generatedBinding,
            out var generatedFailure), generatedFailure);
        Assert.IsNotNull(generatedBinding);
        Assert.AreEqual("Pages/GeneratedOnlyComponent.razor.g.cs", generatedBinding!.Documents.Single().SourcePath);
    }
}
