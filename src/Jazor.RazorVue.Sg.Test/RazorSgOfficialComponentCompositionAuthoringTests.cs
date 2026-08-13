namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialComponentCompositionAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentComposition_EmitsMemberMappedPropsModelAndSlots()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentComposition.razor",
            documentText:
            """
            @using Demo.Components

            <AuthoringChild Title="@Title" @bind-Value="Selected">
                <Header>
                    <strong>@Title</strong>
                </Header>
                <ChildContent>
                    <span>@Selected</span>
                </ChildContent>
                <ItemTemplate Context="item">
                    <em>@item</em>
                </ItemTemplate>
            </AuthoringChild>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/authoring-child")]
                public sealed class AuthoringChild : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("heading")]
                    [Parameter] public string Title { get; set; } = "";
                    [ECMAScriptName("modelValue")]
                    [Parameter] public string Value { get; set; } = "";
                    [ECMAScriptName("onUpdate:modelValue")]
                    [Parameter] public EventCallback<string> ValueChanged { get; set; }
                    [ECMAScriptName("default")]
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                    [ECMAScriptName("header")]
                    [Parameter] public RenderFragment? Header { get; set; }
                    [ECMAScriptName("item")]
                    [Parameter] public RenderFragment<string>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/component-composition")]
                public partial class ComponentComposition : ComponentBase, IVueComponent
                {
                    private string Title { get; } = "Account";
                    private string Selected { get; set; } = "initial";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentComposition");

        StringAssert.Contains(observation.GeneratedCSharp, "OpenComponent<global::Demo.Components.AuthoringChild>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentParameter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.Create<global::System.String>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.CreateInferredEventCallback", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RenderFragment<global::System.String>", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "RuntimeHelpers.TypeCheck<global::Microsoft.AspNetCore.Components.EventCallback<global::System.String>>(global::Microsoft.AspNetCore.Components.EventCallback.Factory.Create<global::System.String>(this, global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.CreateInferredEventCallback(this, __value => Selected = __value, Selected)))",
            StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"./authoring-child.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "heading: state.Title", StringComparison.Ordinal);
        StringAssert.Contains(script, "modelValue: state.Selected", StringComparison.Ordinal);
        StringAssert.Contains(
            script,
            "\"onUpdate:modelValue\": __jazor$handlerCache[0] || (__jazor$handlerCache[0] = __value => state.Selected = __value)",
            StringComparison.Ordinal);
        StringAssert.Contains(script, "header: () =>", StringComparison.Ordinal);
        StringAssert.Contains(script, "default: () =>", StringComparison.Ordinal);
        StringAssert.Contains(script, "item: item =>", StringComparison.Ordinal);
        StringAssert.Contains(script, "h(\"strong\", null, [state.Title])", StringComparison.Ordinal);
        StringAssert.Contains(script, "h(\"span\", null, [state.Selected])", StringComparison.Ordinal);
        StringAssert.Contains(script, "h(\"em\", null, [item])", StringComparison.Ordinal);

        Assert.IsFalse(script.Contains("function title(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("function selected(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("function set_Selected(", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddComponentParameter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CreateInferredEventCallback", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentBindAfter_EmitsModelAssignmentBeforeAsyncCallback()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentBindAfter.razor",
            documentText:
            """
            @using Demo.Components

            <BindAfterChild @bind-Value="Selected" @bind-Value:after="PersistSelectedAsync" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/bind-after-child")]
                public sealed class BindAfterChild : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("modelValue")]
                    [Parameter] public string Value { get; set; } = "";
                    [ECMAScriptName("onUpdate:modelValue")]
                    [Parameter] public EventCallback<string> ValueChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/component-bind-after")]
                public partial class ComponentBindAfter : ComponentBase, IVueComponent
                {
                    private string Selected { get; set; } = "initial";

                    private Task PersistSelectedAsync() => Task.CompletedTask;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentBindAfter");

        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.InvokeAsynchronousDelegate", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.CreateInferredEventCallback", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"./bind-after-child.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "modelValue: state.Selected", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.Selected = __value", StringComparison.Ordinal);
        StringAssert.Contains(script, "PersistSelectedAsync()", StringComparison.Ordinal);

        var assignment = script.IndexOf("state.Selected = __value", StringComparison.Ordinal);
        var callback = script.IndexOf("PersistSelectedAsync()", assignment, StringComparison.Ordinal);
        Assert.IsTrue(assignment < callback, script);

        Assert.IsFalse(script.Contains("CreateInferredBindSetter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("InvokeAsynchronousDelegate", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CreateInferredEventCallback", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentBindGetAndAsyncSet_EmitsModelSetterHandler()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentBindSet.razor",
            documentText:
            """
            @using Demo.Components

            <BindSetChild @bind-Value:get="Selected" @bind-Value:set="SetSelectedAsync" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/bind-set-child")]
                public sealed class BindSetChild : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("modelValue")]
                    [Parameter] public string Value { get; set; } = "";
                    [ECMAScriptName("onUpdate:modelValue")]
                    [Parameter] public EventCallback<string> ValueChanged { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/component-bind-set")]
                public partial class ComponentBindSet : ComponentBase, IVueComponent
                {
                    private string Selected { get; set; } = "initial";

                    private Task SetSelectedAsync(string value)
                    {
                        Selected = value;
                        return Task.CompletedTask;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentBindSet");

        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.Create", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.CreateInferredEventCallback", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"./bind-set-child.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "modelValue: state.Selected", StringComparison.Ordinal);
        StringAssert.Contains(script, "\"onUpdate:modelValue\": SetSelectedAsync", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.Selected = value", StringComparison.Ordinal);

        Assert.IsFalse(script.Contains("CreateInferredBindSetter", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("CreateInferredEventCallback", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }
}
