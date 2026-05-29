using Jazor.RazorVue.Descriptor;
using Jazor.RazorVue.Lowering;
using Jazor.RazorVue.RazorSdk;
using Jazor.RazorVue.RenderTree;

namespace Jazor.RazorVue.RazorIr.Test;

[TestClass]
public sealed class RazorVueRazorIrComponentTypeCarrierBoundaryTests
{
    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsContent_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using System
                @{
                    var childType = typeof(ChildCard);
                }

                @childType
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime render value");
    }

    [TestMethod]
    public void CreateRenderTree_WithDirectTypeOfUsedAsContent_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using System
                @typeof(string)
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "typeof(...)");
        StringAssert.Contains(exception.Issue.Message, "runtime render value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsAttribute_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using System
                @{
                    var childType = typeof(ChildCard);
                }

                <section data-type="@childType">ready</section>
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime render value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsKey_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using System
                @{
                    var childType = typeof(ChildCard);
                }

                <section @key="childType">ready</section>
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime render value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsCondition_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using System
                @{
                    var childType = typeof(ChildCard);
                    if (childType is not null)
                    {
                        <section>ready</section>
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime render value");
    }

    [TestMethod]
    public void CreateRenderTree_WithComponentTypeLocalCarrierUsedAsLoopSource_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using System
                @{
                    var childType = typeof(ChildCard);
                    foreach (var item in new[] { childType })
                    {
                        <section>@item.Name</section>
                    }
                }
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime render value");
    }

    [TestMethod]
    public void CreateRenderTree_WithBranchAssignedComponentTypeLocalCarrier_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            CreateRenderTree(
                """
                @using System
                @{
                    Type childType;
                    if (UseAlternate)
                    {
                        childType = typeof(ChildCard);
                    }
                    else
                    {
                        childType = typeof(OtherCard);
                    }
                }

                @childType
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "later writes");
    }

    [TestMethod]
    public void RazorVueSfcArtifactFactory_WithComponentTypeLocalCarrierUsedAsAttribute_ThrowsCanonicalizationFailed()
    {
        var exception = Assert.ThrowsExactly<RazorVueCompilationIssueException>(() =>
            LowerSfc(
                """
                @using System
                @{
                    var childType = typeof(ChildCard);
                }

                <section data-type="@childType">ready</section>
                """));

        Assert.AreEqual(RazorVueIssueCode.CanonicalizationFailed, exception.Issue.Code);
        StringAssert.Contains(exception.Issue.Message, "System.Type local 'childType'");
        StringAssert.Contains(exception.Issue.Message, "runtime render value");
    }

    private static RazorVueRenderFragment CreateRenderTree(string documentText)
    {
        var (context, snapshot) = CreateContext(documentText);
        return new RazorVueRazorIrTemplateFrontend().CreateRenderTree(context, snapshot);
    }

    private static void LowerSfc(string documentText)
    {
        var (context, snapshot) = CreateContext(documentText);
        _ = new RazorVueSfcArtifactFactory(new RazorVueRazorIrTemplateFrontend()).Lower(context, snapshot);
    }

    private static (RazorVueCompilationContext Context, Artifacts.RazorVueSemanticSnapshot Snapshot) CreateContext(
        string documentText)
        => RazorVueRazorIrTestContextFactory.CreateAlignedContext(
            "RazorVue.RazorIr.ComponentTypeCarrierBoundary.Tests",
            @"D:\repo\Demo\Pages\TodoApp.razor",
            documentText,
            """
            using System;

            namespace Demo.Pages
            {
                [ECMAScript.ECMAScriptModule("./components/child-card")]
                public partial class ChildCard : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public string? Title { get; set; }
                }

                [ECMAScript.ECMAScriptModule("./components/other-card")]
                public partial class OtherCard : ComponentBase, IVueComponent
                {
                }

                [ECMAScript.ECMAScriptModule("./components/todo-app")]
                public partial class TodoApp : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool UseAlternate { get; set; }
                }
            }
            """);
}
