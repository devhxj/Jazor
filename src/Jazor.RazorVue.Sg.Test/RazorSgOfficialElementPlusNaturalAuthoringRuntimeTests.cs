using ECMAScript.ElementPlus;
using Microsoft.AspNetCore.Components;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialElementPlusNaturalAuthoringRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorElementPlusButtonAndInput_PreserveTypedPropsEventsSlotsAndSplat()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ElementPlusNaturalAuthoringRuntime.razor"),
            documentText:
            """
            @using ECMAScript.ElementPlus

            <ElButton Type="ElButtonType.Primary" OnClick="@Activate" class="action" data-test="button">
                Save
            </ElButton>
            <ElInput @bind-ModelValue="Value" Placeholder="Name" @attributes="InputAttributes">
                <Prefix><span data-prefix="true">#</span></Prefix>
            </ElInput>
            <span data-value="@Value" data-activated="@Activated"></span>
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;
            using ECMAScript.ElementPlus;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/element-plus-natural-authoring-runtime")]
            public partial class ElementPlusNaturalAuthoringRuntime : ComponentBase, IVueComponent
            {
                private VueStringNumberValue? Value { get; set; } = "Initial";
                private int Activated { get; set; }
                private IReadOnlyDictionary<string, object?> InputAttributes { get; } =
                    new Dictionary<string, object?> { ["data-input"] = "profile" };

                private void Activate() => Activated++;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ElementPlusNaturalAuthoringRuntime");

        var normalizedGeneratedCSharp = string.Concat(
            observation.GeneratedCSharp
                .Split('\n')
                .Where(static line => !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                .SelectMany(static line => line.Where(static character => !char.IsWhiteSpace(character))));
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.ElementPlus.ElButton>", StringComparison.Ordinal);
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.ElementPlus.ElInput>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.CreateInferredEventCallback", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "nameof(global::ECMAScript.ElementPlus.ElInput.ModelValueChanged)", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "import { ElButton, ElInput } from \"element-plus\";", StringComparison.Ordinal);
        Assert.DoesNotContain("builder.OpenComponent", observation.ModuleText, StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/element-plus-natural-authoring-runtime.mjs",
            observation.ModuleText,
            "official-element-plus-natural-authoring-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/element-plus-natural-authoring-runtime.mjs";
            import { ElButton, ElInput } from "element-plus";

            const find = (nodes, name) => nodes.find(node => node?.name === name);

            test("natural Element Plus components preserve typed props, slots, splats and bindings", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const button = find(initial.children, ElButton);
                const input = find(initial.children, ElInput);
                const state = find(initial.children, "span");

                assert.ok(button);
                assert.equal(button.props.type, "primary");
                assert.equal(button.props.class, "action");
                assert.equal(button.props["data-test"], "button");
                assert.equal(typeof button.props.onClick, "function");
                assert.equal(button.children.default()[0].children.trim(), "Save");

                assert.ok(input);
                assert.equal(input.props.modelValue, "Initial");
                assert.equal(input.props.placeholder, "Name");
                assert.equal(input.props["data-input"], "profile");
                assert.equal(typeof input.props["onUpdate:modelValue"], "function");
                assert.equal(typeof input.children.prefix, "function");
                assert.match(JSON.stringify(input.children.prefix()), /data-prefix/);
                assert.equal(state.props["data-value"], "Initial");

                button.props.onClick();
                input.props["onUpdate:modelValue"]("Updated");
                const updated = render();
                const updatedState = find(updated.children, "span");
                assert.equal(updatedState.props["data-value"], "Updated");
                assert.equal(updatedState.props["data-activated"], 1);
            });
            """,
            new Dictionary<string, string>
            {
                ["node_modules/element-plus/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/element-plus/index.mjs"] = "export const ElButton = { name: \"el-button\" }; export const ElInput = { name: \"el-input\" };"
            });
    }
}
