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
}
