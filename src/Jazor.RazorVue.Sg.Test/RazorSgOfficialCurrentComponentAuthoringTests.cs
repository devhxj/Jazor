namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialCurrentComponentAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorEventHandlerStateHasChanged_EmitsSetupInvalidator()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\RefreshButton.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button type="button" @onclick="Refresh">Refresh</button>
            <span>@RefreshCount</span>
            """,
            codeBehindSource:
            """
            namespace Demo.Components;

            [ECMAScriptModule("./components/refresh-button")]
            public partial class RefreshButton : ComponentBase, IVueComponent
            {
                private int RefreshCount { get; set; }

                private void Refresh()
                {
                    RefreshCount++;
                    StateHasChanged();
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.RefreshButton");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this,",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "Refresh", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onClick: refresh", StringComparison.Ordinal);
        StringAssert.Contains(script, "function refresh()", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.refreshCount++;", StringComparison.Ordinal);
        StringAssert.Contains(script, "stateHasChanged();", StringComparison.Ordinal);

        var increment = script.IndexOf("state.refreshCount++;", StringComparison.Ordinal);
        var invalidate = script.IndexOf("stateHasChanged();", increment, StringComparison.Ordinal);
        Assert.IsTrue(increment < invalidate, script);

        Assert.IsFalse(script.Contains("StateHasChanged", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTaskEventHandlerInvokeAsync_EmitsSetupDispatcher()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\QueuedRefreshButton.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button type="button" @onclick="QueueRefreshAsync">Queue refresh</button>
            <span>@RefreshCount</span>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Components;

            [ECMAScriptModule("./components/queued-refresh-button")]
            public partial class QueuedRefreshButton : ComponentBase, IVueComponent
            {
                private int RefreshCount { get; set; }

                private Task QueueRefreshAsync()
                    => InvokeAsync(() => RefreshCount++);
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.QueuedRefreshButton");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this,",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "QueueRefreshAsync", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onClick: queueRefreshAsync", StringComparison.Ordinal);
        StringAssert.Contains(script, "function queueRefreshAsync()", StringComparison.Ordinal);
        StringAssert.Contains(script, "return invokeAsync(() => {", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.refreshCount++;", StringComparison.Ordinal);

        var dispatch = script.IndexOf("return invokeAsync(() => {", StringComparison.Ordinal);
        var increment = script.IndexOf("state.refreshCount++;", dispatch, StringComparison.Ordinal);
        var actionReturn = script.IndexOf("return;", increment, StringComparison.Ordinal);
        Assert.IsTrue(dispatch < increment, script);
        Assert.IsTrue(increment < actionReturn, script);

        Assert.IsFalse(script.Contains("InvokeAsync", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }
}
