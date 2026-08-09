using Jazor.RazorVue.RazorSdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class ComponentSelectorContractTests
{
    [TestMethod]
    public void DiscoverCurrentComponents_RejectsNullAndReturnsEmptyWhenRequiredMetadataIsUnavailable()
    {
        var nullCompilation = Assert.Throws<ArgumentNullException>(() => ComponentSelector.DiscoverCurrentComponents(null!));
        Assert.AreEqual("compilation", nullCompilation.ParamName);

        var noMetadata = CSharpCompilation.Create(
            "RazorVue.ComponentSelector.NoMetadata",
            [CSharpSyntaxTree.ParseText("namespace Demo; public sealed class PlainComponent { }")],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.IsTrue(ComponentSelector.DiscoverCurrentComponents(noMetadata).IsDefaultOrEmpty);

        var sourceTree = CSharpSyntaxTree.ParseText(
            """
            using ECMAScript;

            namespace Demo;

            [ECMAScriptModule("./components/no-vue-marker")]
            public sealed class ModuleOnlyComponent
            {
            }
            """,
            new CSharpParseOptions(LanguageVersion.Preview));
        var noMarker = CSharpCompilation.Create(
            "RazorVue.ComponentSelector.NoVueMarker",
            [sourceTree],
            TestMetadataReferences.Net11
                .Add(MetadataReference.CreateFromFile(typeof(ECMAScript.ECMAScriptModuleAttribute).Assembly.Location)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(noMarker);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        Assert.IsTrue(ComponentSelector.DiscoverCurrentComponents(noMarker).IsDefaultOrEmpty);
    }

    [TestMethod]
    public void FindBuildRenderTreeMethod_RequiresSourceInstanceRenderTreeSignatureAndSkipsGeneratedMethods()
    {
        var compilation = CreateCompilation(
            """
            using ECMAScript;
            using static ECMAScript.Vue3;
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Rendering;

            namespace Demo;

            internal static class StaticCandidate
            {
                public static void BuildRenderTree(RenderTreeBuilder builder)
                {
                }
            }

            internal sealed class WrongSignatureCandidate
            {
                public void BuildRenderTree(string content)
                {
                }
            }

            [ECMAScriptModule("./components/designer")]
            public sealed class DesignerComponent : ComponentBase, IVueComponent
            {
                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                    builder.AddContent(0, "designer");
                }
            }
            """,
            "Candidates/DesignerComponent.designer.cs");
        var staticCandidate = GetNamedType(compilation, "Demo.StaticCandidate");
        var wrongSignatureCandidate = GetNamedType(compilation, "Demo.WrongSignatureCandidate");
        var designerComponent = GetNamedType(compilation, "Demo.DesignerComponent");

        Assert.IsNull(ComponentSelector.FindBuildRenderTreeMethod(staticCandidate));
        Assert.IsNull(ComponentSelector.FindBuildRenderTreeMethod(wrongSignatureCandidate));
        Assert.IsNotNull(ComponentSelector.FindBuildRenderTreeMethod(designerComponent));
        Assert.IsNull(ComponentSelector.FindHandwrittenBuildRenderTreeMethod(designerComponent));

        Assert.AreEqual(1, ComponentSelector.DiscoverCurrentComponents(compilation).Length);
        Assert.IsTrue(ComponentSelector.DiscoverHandwrittenComponents(compilation).IsDefaultOrEmpty);
        Assert.AreEqual(1, ComponentSelector.DiscoverTailOutputComponents(compilation).Length);
    }

    [TestMethod]
    public void TrySelect_RejectsNullBinding()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => ComponentSelector.TrySelect(null!, out _, out _));
        Assert.AreEqual("binding", exception.ParamName);
    }

    private static CSharpCompilation CreateCompilation(string source, string path)
    {
        var compilation = CSharpCompilation.Create(
            "RazorVue.ComponentSelector.Contract.Tests",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path)],
            RazorSgTestHost.CreateMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var errors = RazorSgTestHost.GetCompilationErrors(compilation);
        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors));
        return compilation;
    }

    private static INamedTypeSymbol GetNamedType(Compilation compilation, string metadataName)
    {
        var symbol = compilation.GetTypeByMetadataName(metadataName);
        Assert.IsNotNull(symbol, metadataName);
        return symbol!;
    }
}
