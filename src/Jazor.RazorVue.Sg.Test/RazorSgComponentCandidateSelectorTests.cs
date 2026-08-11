using System.Collections.Immutable;
using System.Reflection;
using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ComponentSelectorTests
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
                    using static ECMAScript.Vue;

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

        var components = ComponentSelector.DiscoverCurrentComponents(compilation);

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
                    using static ECMAScript.Vue;

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

        var components = ComponentSelector.DiscoverTailRequiredComponents(compilation);

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
            using static ECMAScript.Vue;

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

        var tailRequired = ComponentSelector.DiscoverTailRequiredComponents(compilation);
        var handwritten = ComponentSelector.DiscoverHandwrittenComponents(compilation);
        var tailOutput = ComponentSelector.DiscoverTailOutputComponents(compilation);

        Assert.AreEqual(1, tailRequired.Length);
        Assert.AreEqual("Demo.Pages.LineMapped", tailRequired[0].ToDisplayString());
        Assert.IsTrue(handwritten.IsDefaultOrEmpty);
        Assert.AreEqual(1, tailOutput.Length);
        Assert.IsNull(ComponentSelector.FindHandwrittenBuildRenderTreeMethod(tailRequired[0]));
    }

    [TestMethod]
    public void RazorSourceIdentity_RejectsPlainCSharpTypesAndMethods()
    {
        var sourceTree = CSharpSyntaxTree.ParseText(
            "namespace Demo; public sealed class PlainComponent { public void Execute() { } }",
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/PlainComponent.cs");
        var compilation = CreateCompilation("RazorSg.PlainSourceIdentity", sourceTree);
        var component = compilation.GetTypeByMetadataName("Demo.PlainComponent");
        Assert.IsNotNull(component);
        var execute = component.GetMembers("Execute").OfType<IMethodSymbol>().Single();

        Assert.IsFalse(InvokeHasRazorSourceIdentity(component!));
        Assert.IsFalse(InvokeHasRazorSourceIdentity(execute));
    }

    [TestMethod]
    public void RazorSourceIdentity_RejectsMetadataTypesAndMethods()
    {
        var compilation = CreateCompilation("RazorSg.MetadataSourceIdentity");
        var componentBase = compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        Assert.IsNotNull(componentBase);
        var onInitialized = componentBase.GetMembers("OnInitialized").OfType<IMethodSymbol>().Single();

        Assert.IsFalse(InvokeHasRazorSourceIdentity(componentBase));
        Assert.IsFalse(InvokeHasRazorSourceIdentity(onInitialized));
    }

    [TestMethod]
    public void ModuleAttributeRecognition_UsesMetadataNameFallbackAcrossCompilationSnapshots()
    {
        var candidateCompilation = CSharpCompilation.Create(
            "RazorSg.ModuleAttributeCandidate",
            [CSharpSyntaxTree.ParseText(
                """
                using ECMAScript;

                [ECMAScriptModule]
                public sealed class Candidate { }

                [System.Obsolete]
                public sealed class DecoratedPlain { }
                """,
                new CSharpParseOptions(LanguageVersion.Preview),
                path: "Candidate.cs")],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var candidate = candidateCompilation.GetTypeByMetadataName("Candidate");
        Assert.IsNotNull(candidate);

        var fallbackAttributeCompilation = CSharpCompilation.Create(
            "RazorSg.ModuleAttributeFallback",
            [CSharpSyntaxTree.ParseText(
                "namespace ECMAScript; public sealed class ECMAScriptModuleAttribute : System.Attribute { }",
                new CSharpParseOptions(LanguageVersion.Preview),
                path: "FallbackAttribute.cs")],
            TestMetadataReferences.Net11,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var fallbackAttribute = fallbackAttributeCompilation.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute");
        var actualModuleAttribute = candidateCompilation.GetTypeByMetadataName("ECMAScript.ECMAScriptModuleAttribute");
        var decoratedPlain = candidateCompilation.GetTypeByMetadataName("DecoratedPlain");
        Assert.IsNotNull(fallbackAttribute);
        Assert.IsNotNull(actualModuleAttribute);
        Assert.IsNotNull(decoratedPlain);

        var method = typeof(ComponentSelector).GetMethod(
            "HasECMAScriptModuleAttribute",
            BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method);
        Assert.IsTrue((bool)method.Invoke(null, [candidate!, fallbackAttribute!])!);
        Assert.IsFalse((bool)method.Invoke(null, [decoratedPlain!, actualModuleAttribute!])!);
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
            using static ECMAScript.Vue;

            namespace Demo.Components;

            [ECMAScriptModule("./components/child")]
            public sealed partial class Child : GeneratedBase, IVueComponent
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview),
            path: "Components/Child.razor.cs");
        var compilation = CreateCompilation("RazorSg.InheritedGenerated.Candidate", baseTree, childTree);

        var tailRequired = ComponentSelector.DiscoverTailRequiredComponents(compilation);
        var tailOutput = ComponentSelector.DiscoverTailOutputComponents(compilation);
        var child = compilation.GetTypeByMetadataName("Demo.Components.Child");

        Assert.IsNotNull(child);
        Assert.AreEqual(1, tailRequired.Length);
        Assert.AreSame(child, tailRequired[0]);
        Assert.AreEqual(1, tailOutput.Length);
        Assert.AreSame(child, tailOutput[0]);
        Assert.AreEqual(
            "Demo.Components.GeneratedBase",
            ComponentSelector.FindBuildRenderTreeMethod(child!)!.ContainingType.ToDisplayString());
        Assert.IsNull(ComponentSelector.FindHandwrittenBuildRenderTreeMethod(child!));
    }

    [TestMethod]
    public void TrySelect_RestrictsGeneratedBindingsToCurrentComponentCandidates()
    {
        var authoredTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;
            using Microsoft.AspNetCore.Components;
            using static ECMAScript.Vue;

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

        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            components,
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        Assert.IsTrue(ComponentSelector.TrySelect(binding!, out var selectedBinding, out var selectionFailure), selectionFailure);

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
            using static ECMAScript.Vue;

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
        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(generatedOnly!),
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        Assert.IsFalse(ComponentSelector.TrySelect(binding!, out var selectedBinding, out var selectionFailure));

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
        Assert.IsTrue(GeneratedCSharpBinder.TryBindFinalCompilation(
            compilation,
            ImmutableArray.Create(generatedOnly!),
            out var binding,
            out var bindingFailure), bindingFailure);
        Assert.IsNotNull(binding);
        Assert.IsFalse(ComponentSelector.TrySelect(binding!, out var selectedBinding, out var selectionFailure));

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

    private static bool InvokeHasRazorSourceIdentity(INamedTypeSymbol symbol)
        => InvokeHasRazorSourceIdentityCore(symbol, typeof(INamedTypeSymbol));

    private static bool InvokeHasRazorSourceIdentity(IMethodSymbol symbol)
        => InvokeHasRazorSourceIdentityCore(symbol, typeof(IMethodSymbol));

    private static bool InvokeHasRazorSourceIdentityCore(ISymbol symbol, Type parameterType)
    {
        var method = typeof(ComponentSelector).GetMethod(
            "HasRazorSourceIdentity",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            types: [parameterType],
            modifiers: null);
        Assert.IsNotNull(method);
        return (bool)method.Invoke(null, [symbol])!;
    }
}
