namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialReferenceAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorReferences_EmitElementAndComponentRefCallbacksInSourceOrder()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\References.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components

            <input @ref="inputElement" data-role="primary" />
            <ReferenceChild @ref="child" Status="ready" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/references")]
            public partial class References : ComponentBase, IVueComponent
            {
                private ElementReference inputElement;
                private ReferenceChild? child { get; set; }
            }

            [ECMAScriptModule("./components/reference-child")]
            public partial class ReferenceChild : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Status { get; set; } = "";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.References");

        StringAssert.Contains(observation.GeneratedCSharp, "AddElementReferenceCapture", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentReferenceCapture", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"./reference-child.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "inputElement: null", StringComparison.Ordinal);
        StringAssert.Contains(script, "child: null", StringComparison.Ordinal);
        StringAssert.Contains(script, "ref:", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.inputElement = __value", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.child = __value", StringComparison.Ordinal);

        var elementCapture = script.IndexOf("state.inputElement = __value", StringComparison.Ordinal);
        var componentCapture = script.IndexOf("state.child = __value", StringComparison.Ordinal);
        Assert.IsTrue(elementCapture < componentCapture, script);

        Assert.IsFalse(script.Contains("AddElementReferenceCapture", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddComponentReferenceCapture", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }
}
