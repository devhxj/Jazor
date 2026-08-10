namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialEventCallbackAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorEventCallbackInvocation_EmitsAwaitedOptionalListener()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\CallbackEmitter.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button type="button" @onclick="CommitAsync">@Label</button>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Components;

            [ECMAScriptModule("./components/callback-emitter")]
            public partial class CallbackEmitter : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Label { get; set; } = "Commit";

                [Parameter]
                public EventCallback<string> OnCommit { get; set; }

                private string Value { get; set; } = "draft";

                private async Task CommitAsync()
                {
                    await OnCommit.InvokeAsync(Value);
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.CallbackEmitter");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<global::Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this,",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "CommitAsync", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onClick: CommitAsync", StringComparison.Ordinal);
        StringAssert.Contains(script, "async function CommitAsync()", StringComparison.Ordinal);
        StringAssert.Contains(script, "await props.OnCommit?.(state.Value);", StringComparison.Ordinal);
        StringAssert.Contains(script, "[props.Label]", StringComparison.Ordinal);

        Assert.IsFalse(script.Contains("EventCallback.Factory", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("InvokeAsync", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("this.", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }
}
