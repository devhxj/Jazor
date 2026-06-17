using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrFragmentSlotCarrierBoundaryTests
{
    [TestMethod]
    public void CreateRenderTree_WithOutParameterFragmentFactory_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using Microsoft.AspNetCore.Components

                @(CreateTemplate(out _))

                @code {
                    private RenderFragment CreateTemplate(out string? title)
                    {
                        title = "fallback";
                        return @<section>safe</section>;
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "out");
    }

    [TestMethod]
    public void CreateRenderTree_WithRefParameterFragmentFactory_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using Microsoft.AspNetCore.Components

                @{
                    var title = Title;
                }

                @(CreateTemplate(ref title))

                @code {
                    private RenderFragment CreateTemplate(ref string? title)
                    {
                        var captured = title;
                        return @<section>@captured</section>;
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "ref");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithRefParameterFragmentFactory_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            LowerSfc(
                """
                @using Microsoft.AspNetCore.Components

                @{
                    var title = Title;
                }

                @(CreateTemplate(ref title))

                @code {
                    private RenderFragment CreateTemplate(ref string? title)
                    {
                        var captured = title;
                        return @<section>@captured</section>;
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "ref");
    }

    [TestMethod]
    public void RazorVuePipeline_WithRefParameterFragmentFactory_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            LowerPipeline(
                """
                @using Microsoft.AspNetCore.Components

                @{
                    var title = Title;
                }

                @(CreateTemplate(ref title))

                @code {
                    private RenderFragment CreateTemplate(ref string? title)
                    {
                        var captured = title;
                        return @<section>@captured</section>;
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "ref");
    }

    [TestMethod]
    public void RazorVuePipeline_WithOutParameterFragmentFactory_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            LowerPipeline(
                """
                @using Microsoft.AspNetCore.Components

                @(CreateTemplate(out _))

                @code {
                    private RenderFragment CreateTemplate(out string? title)
                    {
                        title = "fallback";
                        return @<section>safe</section>;
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "out");
    }

    [TestMethod]
    public void CreateRenderTree_WithDataflowGetterFragmentCarrier_ProducesStructuredSlotTemplate()
    {
        var renderTree = CreateRenderTree(
            """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment<int> template = Template;
            }

            <LayoutCard ItemTemplate="template" />

            @code {
                private RenderFragment<int> Template
                {
                    get
                    {
                        var template = CreateTemplate(Title);
                        return template;
                    }
                }

                private RenderFragment<int> CreateTemplate(string? title)
                    => item => @<span>@title @item</span>;
            }
            """);

        var component = Assert.IsInstanceOfType<RazorVueComponentNode>(renderTree.Children.Single());
        var slot = component.SlotTemplates.Single();
        Assert.AreEqual("ItemTemplate", slot.PublicName);

        var titleScope = Assert.IsInstanceOfType<RazorVueTemplateScopeNode>(slot.Children.Children.Single());
        Assert.AreEqual("title", titleScope.ScopeName);

        var span = Assert.IsInstanceOfType<RazorVueElementNode>(titleScope.Children.Children.Single());
        Assert.AreEqual("span", span.TagName);
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithFragmentFactoryReturningLocalDelegate_LowersStructuredTemplate()
    {
        var artifact = LowerSfc(
            """
            @using Microsoft.AspNetCore.Components

            @CreateTemplate(Title)

            @code {
                private RenderFragment CreateTemplate(string? title)
                {
                    RenderFragment template = @<section>@title</section>;
                    return template;
                }
            }
            """);

        StringAssert.Contains(artifact.SfcText, "section");
        StringAssert.Contains(artifact.SfcText, "props.title");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithFragmentFactoryReturningLocalDelegateAliasChain_LowersStructuredTemplate()
    {
        var artifact = LowerSfc(
            """
            @using Microsoft.AspNetCore.Components

            @{
                RenderFragment CreateTemplate(string? title)
                {
                    RenderFragment template = @<section>@title</section>;
                    RenderFragment alias;
                    alias = template;
                    return alias;
                }
            }

            @CreateTemplate(Title)
            """);

        StringAssert.Contains(artifact.SfcText, "section");
        StringAssert.Contains(artifact.SfcText, "props.title");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithSideEffectBeforeReturnedFragmentFactoryLocalDelegate_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            LowerSfc(
                """
                @using Microsoft.AspNetCore.Components

                @CreateTemplate(Title)

                @code {
                    private RenderFragment CreateTemplate(string? title)
                    {
                        _ = title?.Trim();
                        RenderFragment template = @<section>@title</section>;
                        return template;
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "fragment factory");
        StringAssert.Contains(exception.Issue.Message, "analyzable return value");
    }

    [TestMethod]
    public void RazorVuePipeline_WithConditionalGetterFragmentCarrier_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            LowerPipeline(
                """
                @using Microsoft.AspNetCore.Components

                @{
                    RenderFragment<int> template = Template;
                }

                <LayoutCard ItemTemplate="template" />

                @code {
                    private RenderFragment<int> Template => UseAlternate
                        ? CreateTemplate(Title)
                        : CreateTemplate(Subtitle);

                    private RenderFragment<int> CreateTemplate(string? title)
                        => item => @<span>@title @item</span>;
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "RenderFragment local 'template'");
    }

    private static RazorVueRenderFragment CreateRenderTree(string documentText)
    {
        var (context, snapshot) = CreateContext(documentText);
        return new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
    }

    private static Artifacts.VueSfcArtifact LowerSfc(string documentText)
    {
        var (context, snapshot) = CreateContext(documentText);
        return new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
    }

    private static void LowerPipeline(string documentText)
    {
        var (context, snapshot) = CreateContext(documentText);
        _ = new RazorVueArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
    }

    private static (RazorVueCompilationContext Context, Artifacts.RazorVueSemanticSnapshot Snapshot) CreateContext(
        string documentText)
        => RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.FragmentSlotCarrierBoundary.Tests",
            @"D:\repo\Demo\Pages\TodoApp.razor",
            documentText,
            """
            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/layout-card")]
                public partial class LayoutCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public RenderFragment<int>? ItemTemplate { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool UseAlternate { get; set; }

                    [Parameter]
                    public string? Title { get; set; }

                    [Parameter]
                    public string? Subtitle { get; set; }
                }
            }
            """);
}
