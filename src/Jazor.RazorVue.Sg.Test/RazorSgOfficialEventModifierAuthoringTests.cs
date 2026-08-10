namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialEventModifierAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorEventModifiers_EmitOrderedDirectHandler()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\EventModifiers.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button @onclick="HandleClick"
                    @onclick:preventDefault="PreventDefault"
                    @onclick:stopPropagation>
                Save
            </button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/event-modifiers")]
            public partial class EventModifiers : ComponentBase, IVueComponent
            {
                [Parameter]
                public bool PreventDefault { get; set; }

                private void HandleClick()
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.EventModifiers");

        StringAssert.Contains(observation.GeneratedCSharp, "AddEventPreventDefaultAttribute", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddEventStopPropagationAttribute", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "onClick", StringComparison.Ordinal);
        StringAssert.Contains(script, "if (props.PreventDefault)", StringComparison.Ordinal);
        StringAssert.Contains(script, "event?.preventDefault?.();", StringComparison.Ordinal);
        StringAssert.Contains(script, "event?.stopPropagation?.();", StringComparison.Ordinal);
        StringAssert.Contains(script, "HandleClick(event, ...args)", StringComparison.Ordinal);

        var preventCondition = script.IndexOf("if (props.PreventDefault)", StringComparison.Ordinal);
        var preventCall = script.IndexOf("event?.preventDefault?.();", StringComparison.Ordinal);
        var stopCall = script.IndexOf("event?.stopPropagation?.();", StringComparison.Ordinal);
        var handlerCall = script.IndexOf("HandleClick(event, ...args)", StringComparison.Ordinal);
        Assert.IsTrue(preventCondition < preventCall, script);
        Assert.IsTrue(preventCall < stopCall, script);
        Assert.IsTrue(stopCall < handlerCall, script);

        Assert.IsFalse(script.Contains("function preventDefault(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("function set_PreventDefault(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddEventPreventDefaultAttribute", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddEventStopPropagationAttribute", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }
}
