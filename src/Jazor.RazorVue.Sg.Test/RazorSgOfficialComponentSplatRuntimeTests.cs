namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialComponentSplatRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentSplat_MapsParameterNamesAndPreservesExplicitBindPrecedenceOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentSplatRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <section>
                <SplatBindChild @attributes="ChildAttributes" data-case="splat-only" />
                <SplatBindChild @attributes="ChildAttributes" @bind-Value="Selected" data-case="explicit-bind" />
            </section>
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/splat-bind-child-runtime")]
                public sealed class SplatBindChild : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("modelValue")]
                    [Parameter] public string Value { get; set; } = "";
                    [Parameter, System.ComponentModel.Description("@#onUpdate:modelValue")] public EventCallback<string> ValueChanged { get; set; }

                    [Parameter(CaptureUnmatchedValues = true)]
                    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/component-splat-runtime")]
                public partial class ComponentSplatRuntime : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public IReadOnlyDictionary<string, object>? ChildAttributes { get; set; }

                    private string Selected { get; set; } = "initial";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentSplatRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "ValueChanged", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "function __normalizeComponentAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/component-splat-runtime.mjs",
            observation.ModuleText,
            "official-component-splat-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/component-splat-runtime.mjs";

            test("official Razor component splats map C# names before explicit bind precedence", async () => {
                let splatUpdates = 0;
                const render = component.setup({
                    ChildAttributes: {
                        Value: "from-splat",
                        ValueChanged(value) {
                            splatUpdates++;
                            assert.equal(value, "splat-update");
                        },
                        "data-case": "from-splat"
                    }
                }, { slots: {} });

                const initial = render();
                const children = initial.children.filter(child => child.props && child.props["data-case"]);
                const splatOnly = children.find(child => child.props["data-case"] === "splat-only");
                const explicitBind = children.find(child => child.props["data-case"] === "explicit-bind");

                assert.equal(splatOnly.props.modelValue, "from-splat");
                assert.equal(splatOnly.props["data-case"], "splat-only");
                assert.equal(typeof splatOnly.props["onUpdate:modelValue"], "function");
                assert.equal(splatOnly.props.Value, undefined);
                assert.equal(splatOnly.props.ValueChanged, undefined);

                await Promise.resolve(splatOnly.props["onUpdate:modelValue"]("splat-update"));
                assert.equal(splatUpdates, 1);

                assert.equal(explicitBind.props.modelValue, "initial");
                assert.equal(explicitBind.props["data-case"], "explicit-bind");
                assert.equal(typeof explicitBind.props["onUpdate:modelValue"], "function");
                assert.equal(explicitBind.props.Value, undefined);
                assert.equal(explicitBind.props.ValueChanged, undefined);

                await Promise.resolve(explicitBind.props["onUpdate:modelValue"]("bound-update"));
                assert.equal(splatUpdates, 1);
                assert.equal(
                    render().children.find(child => child.props && child.props["data-case"] === "explicit-bind").props.modelValue,
                    "bound-update");

                const mapRender = component.setup({
                    ChildAttributes: new Map([
                        ["Value", "from-map"],
                        ["ValueChanged", value => assert.equal(value, "map-update")],
                        ["aria-label", "Map carrier"]
                    ])
                }, { slots: {} });
                const mapChild = mapRender().children.find(child => child.props && child.props["data-case"] === "splat-only");

                assert.equal(mapChild.props.modelValue, "from-map");
                assert.equal(mapChild.props["aria-label"], "Map carrier");
                assert.equal(mapChild.props.Value, undefined);
                await Promise.resolve(mapChild.props["onUpdate:modelValue"]("map-update"));

                const pairRender = component.setup({
                    ChildAttributes: [
                        { Key: "Value", Value: "from-pairs" },
                        { Key: "ValueChanged", Value: value => assert.equal(value, "pair-update") },
                        { Key: "data-source", Value: "key-value-pairs" }
                    ]
                }, { slots: {} });
                const pairChild = pairRender().children.find(child => child.props && child.props["data-case"] === "splat-only");

                assert.equal(pairChild.props.modelValue, "from-pairs");
                assert.equal(pairChild.props["data-source"], "key-value-pairs");
                assert.equal(pairChild.props.ValueChanged, undefined);
                await Promise.resolve(pairChild.props["onUpdate:modelValue"]("pair-update"));
            });
            """,
            new Dictionary<string, string>
            {
                ["components/splat-bind-child-runtime.mjs"] = "export default { name: \"splat-bind-child-runtime\" };"
            });
    }
}
