namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Verifies the shallow Vue watch contract used for ComponentBase parameter lifecycle hooks.
/// 参数替换触发生命周期，同一引用内部 mutation 不会伪装成新的参数赋值。
/// </summary>
[TestClass]
public sealed class RazorSgParameterLifecycleWatchRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_ShallowParameterWatch_TriggersOnlyForValueOrReferenceReplacement()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ParameterLifecycle.razor",
            documentText:
            """
            <p>@Message</p>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/parameter-lifecycle-runtime")]
            public partial class ParameterLifecycle : ComponentBase, IVueComponent
            {
                [Parameter]
                public ParameterModel Model { get; set; } = new("initial");

                [Parameter]
                public string Label { get; set; } = "initial";

                private string Message { get; set; } = "";

                protected override void OnParametersSet()
                {
                    Message += Label + ":" + Model.Value + "|";
                }

                public sealed class ParameterModel(string value)
                {
                    public string Value { get; set; } = value;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ParameterLifecycle");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "() => [props.Label, props.Model]", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("deep: true", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/parameter-lifecycle-runtime.mjs",
            observation.ModuleText,
            "parameter-lifecycle-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runWatchers } from "vue";

            import component from "./components/parameter-lifecycle-runtime.mjs";

            test("parameter lifecycle follows shallow value and reference replacement", () => {
                const model = { Value: "one" };
                const props = { Label: "first", Model: model };
                const render = component.setup(props, { slots: {} });

                assert.equal(render().children, "first:one|");

                model.Value = "nested";
                __runWatchers();
                assert.equal(render().children, "first:one|");

                props.Label = "second";
                __runWatchers();
                assert.equal(render().children, "first:one|second:nested|");

                props.Model = { Value: "replacement" };
                __runWatchers();
                assert.equal(render().children, "first:one|second:nested|second:replacement|");
            });
            """);
    }
}
