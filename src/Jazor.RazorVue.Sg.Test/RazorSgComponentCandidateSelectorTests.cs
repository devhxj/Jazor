using System.Collections.Immutable;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgComponentCandidateSelectorTests
{
    [TestMethod]
    public void DiscoverCurrentComponents_UsesOnlyTheRoslynComponentEntryContract()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorSg.ComponentCandidateSelector.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
                    """
                    using ECMAScript;
                    using Microsoft.AspNetCore.Components;
                    using static ECMAScript.Vue3;

                    namespace Demo.Pages;

                    [ECMAScriptModule("./components/valid")]
                    public partial class ValidComponent : ComponentBase, IVueComponent
                    {
                    }

                    [ECMAScriptModule("./components/invalid")]
                    public partial class MissingVueMarker : ComponentBase
                    {
                    }

                    public partial class UnmarkedComponent : ComponentBase, IVueComponent
                    {
                    }
                    """,
                    new CSharpParseOptions(LanguageVersion.Preview),
                    path: "Components.razor.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var components = RazorSgComponentCandidateSelector.DiscoverCurrentComponents(compilation);

        Assert.AreEqual(1, components.Length);
        Assert.AreEqual("Demo.Pages.ValidComponent", components[0].ToDisplayString());
    }

    [TestMethod]
    public void DiscoverTailRequiredComponents_ExcludesHandwrittenBuildRenderTree()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "RazorSg.ComponentCandidateSelector.Handwritten.Tests",
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(
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
                            builder.AddContent(0, "handwritten");
                        }
                    }
                    """,
                    new CSharpParseOptions(LanguageVersion.Preview),
                    path: "Counter.razor.cs")
            ],
            references: RazorSgTestHost.CreateMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var components = RazorSgComponentCandidateSelector.DiscoverTailRequiredComponents(compilation);

        Assert.IsTrue(components.IsDefaultOrEmpty);
    }

    [TestMethod]
    public void DiscoverTailRequiredComponents_TreatsLineMappedBuildRenderTreeAsRazorGenerated()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;
            using static ECMAScript.Vue3;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/line-mapped")]
            public partial class LineMapped : ComponentBase, IVueComponent
            {
            #line 1 "Pages/LineMapped.razor"
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "generated");
                }
            #line default
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Generated/LineMapped.cs");
        var compilation = CreateCompilation("RazorSg.LineMapped.Candidate", sourceTree);

        var tailRequired = RazorSgComponentCandidateSelector.DiscoverTailRequiredComponents(compilation);
        var handwritten = RazorSgComponentCandidateSelector.DiscoverHandwrittenComponents(compilation);
        var tailOutput = RazorSgComponentCandidateSelector.DiscoverTailOutputComponents(compilation);

        Assert.AreEqual(1, tailRequired.Length);
        Assert.AreEqual("Demo.Pages.LineMapped", tailRequired[0].ToDisplayString());
        Assert.IsTrue(handwritten.IsDefaultOrEmpty);
        Assert.AreEqual(1, tailOutput.Length);
        Assert.IsNull(RazorSgComponentCandidateSelector.FindHandwrittenBuildRenderTreeMethod(tailRequired[0]));
    }

    [TestMethod]
    public void DiscoverTailOutputComponents_UsesInheritedGeneratedBuildRenderTree()
    {
        var baseTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Components;

            public abstract partial class GeneratedBase : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "base");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/GeneratedBase.razor.g.cs");
        var childTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;

            namespace Demo.Components;

            [ECMAScriptModule("./components/child")]
            public sealed partial class Child : GeneratedBase, IVueComponent
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/Child.razor.cs");
        var compilation = CreateCompilation("RazorSg.InheritedGenerated.Candidate", baseTree, childTree);

        var tailRequired = RazorSgComponentCandidateSelector.DiscoverTailRequiredComponents(compilation);
        var tailOutput = RazorSgComponentCandidateSelector.DiscoverTailOutputComponents(compilation);
        var child = compilation.GetTypeByMetadataName("Demo.Components.Child");

        Assert.IsNotNull(child);
        Assert.AreEqual(1, tailRequired.Length);
        Assert.AreSame(child, tailRequired[0]);
        Assert.AreEqual(1, tailOutput.Length);
        Assert.AreSame(child, tailOutput[0]);
        Assert.AreEqual(
            "Demo.Components.GeneratedBase",
            RazorSgComponentCandidateSelector.FindBuildRenderTreeMethod(child!)!.ContainingType.ToDisplayString());
        Assert.IsNull(RazorSgComponentCandidateSelector.FindHandwrittenBuildRenderTreeMethod(child!));
    }

    [TestMethod]
    public void TrySelect_RestrictsGeneratedBindingsToCurrentComponentCandidates()
    {
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Components;

            [ECMAScriptModule("./components/selected")]
            public partial class Selected : ComponentBase, IVueComponent
            {
            }

            [ECMAScriptModule("./components/missing-marker")]
            public partial class MissingMarker : ComponentBase
            {
            }

            public partial class MissingModule : ComponentBase, IVueComponent
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/Components.razor.cs");
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Components;

            public partial class Selected
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "selected");
                }
            }

            public partial class MissingMarker
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "missing marker");
                }
            }

            public partial class MissingModule
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "missing module");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/Components.razor.g.cs");
        var compilation = CreateCompilation("RazorSg.Select.Candidate", authoredTree, generatedTree);
        var components = ImmutableArray.Create(
            compilation.GetTypeByMetadataName("Demo.Components.MissingModule")!,
            compilation.GetTypeByMetadataName("Demo.Components.Selected")!,
            compilation.GetTypeByMetadataName("Demo.Components.MissingMarker")!);

        Assert.IsTrue(RazorSgGeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        Assert.IsTrue(RazorSgComponentCandidateSelector.TrySelect(binding!, out var selectedBinding, out var selectionFailure), selectionFailure);

        Assert.IsNotNull(selectedBinding);
        Assert.AreEqual(1, selectedBinding!.Components.Length);
        Assert.AreEqual("Demo.Components.Selected", selectedBinding.Components[0].ComponentSymbol.ToDisplayString());
        Assert.AreEqual(3, binding!.Components.Length);
    }

    [TestMethod]
    public void TrySelect_ReportsCandidatesAndGeneratedComponentsWhenTheyDoNotIntersect()
    {
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue3;

            namespace Demo.Components;

            [ECMAScriptModule("./components/candidate")]
            public partial class Candidate : ComponentBase, IVueComponent
            {
            }

            public partial class GeneratedOnly : ComponentBase
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/Components.razor.cs");
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Components;

            public partial class GeneratedOnly
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "generated");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/GeneratedOnly.razor.g.cs");
        var compilation = CreateCompilation("RazorSg.Select.Mismatch", authoredTree, generatedTree);
        var generatedOnly = compilation.GetTypeByMetadataName("Demo.Components.GeneratedOnly");

        Assert.IsNotNull(generatedOnly);
        Assert.IsTrue(RazorSgGeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(generatedOnly!),
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        Assert.IsFalse(RazorSgComponentCandidateSelector.TrySelect(binding!, out var selectedBinding, out var selectionFailure));

        Assert.IsNull(selectedBinding);
        Assert.IsNotNull(selectionFailure);
        StringAssert.Contains(selectionFailure, "Demo.Components.Candidate", StringComparison.Ordinal);
        StringAssert.Contains(selectionFailure, "Demo.Components.GeneratedOnly", StringComparison.Ordinal);
    }

    [TestMethod]
    public void TrySelect_ReportsGeneratedComponentsWhenNoCurrentCandidatesExist()
    {
        var generatedTree = CSharpSyntaxTree.ParseText(
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo.Components;

            public partial class GeneratedOnly : ComponentBase
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "generated");
                }
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/GeneratedOnly.razor.g.cs");
        var compilation = CreateCompilation("RazorSg.Select.NoCandidates", generatedTree);
        var generatedOnly = compilation.GetTypeByMetadataName("Demo.Components.GeneratedOnly");

        Assert.IsNotNull(generatedOnly);
        Assert.IsTrue(RazorSgGeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(generatedOnly!),
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        Assert.IsFalse(RazorSgComponentCandidateSelector.TrySelect(binding!, out var selectedBinding, out var selectionFailure));

        Assert.IsNull(selectedBinding);
        Assert.IsNotNull(selectionFailure);
        StringAssert.Contains(selectionFailure, "RazorVue candidates: <none>", StringComparison.Ordinal);
        StringAssert.Contains(selectionFailure, "Demo.Components.GeneratedOnly", StringComparison.Ordinal);
    }

    private static Compilation CreateCompilation(string assemblyName, params SyntaxTree[] syntaxTrees)
        => CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
}
