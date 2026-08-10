namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialBindingAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorInputBind_EmitsStateAssignmentHandler()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InputBinding.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind="Text" @bind:event="oninput" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/input-binding")]
            public partial class InputBinding : ComponentBase, IVueComponent
            {
                private string Text { get; set; } = "initial";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InputBinding");

        StringAssert.Contains(observation.GeneratedCSharp, "BindConverter.FormatValue", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.CreateBinder", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "SetUpdatesAttributeName(\"value\")", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "value: state.Text", StringComparison.Ordinal);
        StringAssert.Contains(script, "onInput", StringComparison.Ordinal);
        StringAssert.Contains(script, "eventOrValue", StringComparison.Ordinal);
        StringAssert.Contains(script, "return (__value => state.Text = __value)(value, ...args);", StringComparison.Ordinal);

        var valueRead = script.IndexOf("value: state.Text", StringComparison.Ordinal);
        var handler = script.IndexOf("onInput", valueRead, StringComparison.Ordinal);
        var assignment = script.IndexOf("state.Text = __value", handler, StringComparison.Ordinal);
        Assert.IsTrue(valueRead < handler, script);
        Assert.IsTrue(handler < assignment, script);

        Assert.IsFalse(script.Contains("function Text(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("function set_Text(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("BindConverter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CreateBinder", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("SetUpdatesAttributeName", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorInputBindAfter_EmitsAssignmentThenAsyncCallback()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InputBindingAfter.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind="Text" @bind:event="oninput" @bind:after="AfterTextChangedAsync" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/input-binding-after")]
            public partial class InputBindingAfter : ComponentBase, IVueComponent
            {
                private string Text { get; set; } = "initial";

                private Task AfterTextChangedAsync() => Task.CompletedTask;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InputBindingAfter");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "InvokeAsynchronousDelegate", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "state.Text = __value", StringComparison.Ordinal);
        StringAssert.Contains(script, "AfterTextChangedAsync()", StringComparison.Ordinal);

        var assignment = script.IndexOf("state.Text = __value", StringComparison.Ordinal);
        var callback = script.IndexOf("AfterTextChangedAsync()", assignment, StringComparison.Ordinal);
        Assert.IsTrue(assignment < callback, script);

        Assert.IsFalse(script.Contains("CreateInferredBindSetter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("InvokeAsynchronousDelegate", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorInputBindAfterAction_EmitsAssignmentThenCallback()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InputBindingAfterAction.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind="Text" @bind:event="oninput" @bind:after="RecordTextChanged" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/input-binding-after-action")]
            public partial class InputBindingAfterAction : ComponentBase, IVueComponent
            {
                private string Text { get; set; } = "initial";
                private string LastText { get; set; } = "";

                private void RecordTextChanged()
                {
                    LastText = Text;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InputBindingAfterAction");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "InvokeAsynchronousDelegate", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "state.Text = __value", StringComparison.Ordinal);
        StringAssert.Contains(script, "RecordTextChanged()", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.LastText = state.Text", StringComparison.Ordinal);

        var assignment = script.IndexOf("state.Text = __value", StringComparison.Ordinal);
        var callback = script.IndexOf("RecordTextChanged()", assignment, StringComparison.Ordinal);
        Assert.IsTrue(assignment < callback, script);

        Assert.IsFalse(script.Contains("CreateInferredBindSetter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("InvokeAsynchronousDelegate", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorInputBindAfterInlineAsyncLambda_EmitsAssignmentThenInvokesCallbackOnce()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InputBindingAfterInline.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind="Text" @bind:event="oninput" @bind:after="async () => await PersistTextAsync()" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/input-binding-after-inline")]
            public partial class InputBindingAfterInline : ComponentBase, IVueComponent
            {
                private string Text { get; set; } = "initial";

                private Task PersistTextAsync() => Task.CompletedTask;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InputBindingAfterInline");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "InvokeAsynchronousDelegate", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "state.Text = __value", StringComparison.Ordinal);
        StringAssert.Contains(script, "PersistTextAsync()", StringComparison.Ordinal);

        var assignment = script.IndexOf("state.Text = __value", StringComparison.Ordinal);
        var callback = script.IndexOf("PersistTextAsync()", assignment, StringComparison.Ordinal);
        Assert.IsTrue(assignment < callback, script);
        Assert.AreEqual(1, CountOccurrences(script, "await PersistTextAsync()"), script);

        Assert.IsFalse(script.Contains("CreateInferredBindSetter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("InvokeAsynchronousDelegate", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorInputBindGetAndAsyncSet_EmitsInferredSetterWithoutSdkHelper()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InputBindingSet.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind:get="Text" @bind:set="SetTextAsync" @bind:event="oninput" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/input-binding-set")]
            public partial class InputBindingSet : ComponentBase, IVueComponent
            {
                private string Text { get; set; } = "initial";

                private Task SetTextAsync(string value)
                {
                    Text = value;
                    return Task.CompletedTask;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InputBindingSet");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "value: state.Text", StringComparison.Ordinal);
        StringAssert.Contains(script, "SetTextAsync(__value)", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.Text = value", StringComparison.Ordinal);

        var handler = script.IndexOf("onInput", StringComparison.Ordinal);
        var setter = script.IndexOf("SetTextAsync(__value)", handler, StringComparison.Ordinal);
        Assert.IsTrue(handler < setter, script);

        Assert.IsFalse(script.Contains("CreateInferredBindSetter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CreateBinder", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorInputBindGetAndSet_EmitsCurrentComponentSetterHandler()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InputBindingSyncSet.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind:get="Text" @bind:set="SetText" @bind:event="oninput" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/input-binding-sync-set")]
            public partial class InputBindingSyncSet : ComponentBase, IVueComponent
            {
                private string Text { get; set; } = "initial";

                private void SetText(string value)
                {
                    Text = value;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InputBindingSyncSet");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "value: state.Text", StringComparison.Ordinal);
        StringAssert.Contains(script, "SetText(__value)", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.Text = value", StringComparison.Ordinal);

        var handler = script.IndexOf("onInput", StringComparison.Ordinal);
        var setter = script.IndexOf("SetText(__value)", handler, StringComparison.Ordinal);
        Assert.IsTrue(handler < setter, script);

        Assert.IsFalse(script.Contains("CreateBinder", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    private static int CountOccurrences(string value, string fragment)
    {
        var count = 0;
        var index = 0;
        while ((index = value.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += fragment.Length;
        }

        return count;
    }
}
