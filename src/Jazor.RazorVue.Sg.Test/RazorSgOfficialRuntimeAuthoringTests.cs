namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialRuntimeAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorInputBindAfterAction_UpdatesStateBeforeCallbackOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InputBindingAfterRuntime.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind="Text" @bind:event="oninput" @bind:after="RecordTextChanged" data-last="@LastText" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/input-binding-after-runtime")]
            public partial class InputBindingAfterRuntime : ComponentBase, IVueComponent
            {
                private string Text { get; set; } = "initial";
                private string LastText { get; set; } = "none";

                private void RecordTextChanged()
                {
                    LastText = Text;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InputBindingAfterRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "InvokeAsynchronousDelegate", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/input-binding-after-runtime.mjs",
            observation.ModuleText,
            "official-input-bind-after-action.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/input-binding-after-runtime.mjs";

            test("official Razor bind after callback observes the newly assigned value", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "input");
                assert.equal(initial.props.value, "initial");
                assert.equal(initial.props["data-last"], "none");
                assert.equal(typeof initial.props.onInput, "function");

                await Promise.resolve(initial.props.onInput("updated"));

                const updated = render();
                assert.equal(updated.props.value, "updated");
                assert.equal(updated.props["data-last"], "updated");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentBindAfterAction_UpdatesModelBeforeCallbackOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentBindingAfterRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <BindAfterChild @bind-Value="Selected" @bind-Value:after="RecordSelected" LastObserved="@LastSelected" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/bind-after-child-runtime")]
                public sealed class BindAfterChild : ComponentBase, IVueComponent
                {
                    [ECMAScriptName("modelValue")]
                    [Parameter] public string Value { get; set; } = "";
                    [Parameter] public EventCallback<string> ValueChanged { get; set; }
                    [Parameter] public string LastObserved { get; set; } = "";

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/component-binding-after-runtime")]
                public partial class ComponentBindingAfterRuntime : ComponentBase, IVueComponent
                {
                    private string Selected { get; set; } = "initial";
                    private string LastSelected { get; set; } = "none";

                    private void RecordSelected()
                    {
                        LastSelected = Selected;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentBindingAfterRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "CreateInferredBindSetter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "InvokeAsynchronousDelegate", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/component-binding-after-runtime.mjs",
            observation.ModuleText,
            "official-component-bind-after-action.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/component-binding-after-runtime.mjs";

            test("official Razor component bind after callback observes the newly assigned model", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.props.modelValue, "initial");
                assert.equal(initial.props.lastObserved, "none");
                assert.equal(typeof initial.props["onUpdate:modelValue"], "function");

                await Promise.resolve(initial.props["onUpdate:modelValue"]("updated"));

                const updated = render();
                assert.equal(updated.props.modelValue, "updated");
                assert.equal(updated.props.lastObserved, "updated");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/bind-after-child-runtime.mjs"] = "export default { name: \"bind-after-child-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorEventModifiers_RunPlatformControlsBeforeHandlerOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\EventModifiersRuntime.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button @onclick="HandleClick"
                    @onclick:preventDefault="PreventDefault"
                    @onclick:stopPropagation
                    data-count="@ClickCount">
                Save
            </button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/event-modifiers-runtime")]
            public partial class EventModifiersRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public bool PreventDefault { get; set; }

                private int ClickCount { get; set; }

                private void HandleClick()
                {
                    ClickCount++;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.EventModifiersRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "AddEventPreventDefaultAttribute", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddEventStopPropagationAttribute", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/event-modifiers-runtime.mjs",
            observation.ModuleText,
            "official-event-modifiers-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/event-modifiers-runtime.mjs";

            test("official Razor event modifiers execute before the handler", async () => {
                const render = component.setup({ preventDefault: true }, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "button");
                assert.equal(initial.props["data-count"], 0);
                assert.equal(typeof initial.props.onClick, "function");

                const calls = [];
                await Promise.resolve(initial.props.onClick({
                    preventDefault() { calls.push("prevent"); },
                    stopPropagation() { calls.push("stop"); }
                }));

                assert.deepEqual(calls, ["prevent", "stop"]);
                const updated = render();
                assert.equal(updated.props["data-count"], 1);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorAttributeSplat_PreservesExplicitAttributePrecedenceOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\AttributeSplatRuntime.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @attributes="InputAttributes" class="form-control" data-role="account-name" />
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/attribute-splat-runtime")]
            public partial class AttributeSplatRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyDictionary<string, object>? InputAttributes { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.AttributeSplatRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/attribute-splat-runtime.mjs",
            observation.ModuleText,
            "official-attribute-splat-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/attribute-splat-runtime.mjs";

            test("official Razor attributes preserve explicit precedence", () => {
                const render = component.setup({
                    inputAttributes: {
                        class: "external-class",
                        "data-role": "external-role",
                        "aria-label": "Account name"
                    }
                }, { slots: {} });
                const input = render();

                assert.equal(input.name, "input");
                assert.equal(input.props.class, "form-control");
                assert.equal(input.props["data-role"], "account-name");
                assert.equal(input.props["aria-label"], "Account name");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorKeyedLoop_PreservesPerItemVNodeKeysOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\KeyedLoopRuntime.razor",
            documentText:
            """
            @foreach (var item in Items)
            {
                <li @key="item.Id" data-id="@item.Id">@item.Name</li>
            }
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/keyed-loop-runtime")]
            public partial class KeyedLoopRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyList<KeyedItem> Items { get; set; } = [];
            }

            public sealed record KeyedItem(int Id, string Name);
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.KeyedLoopRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "__builder.SetKey(", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/keyed-loop-runtime.mjs",
            observation.ModuleText,
            "official-keyed-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/keyed-loop-runtime.mjs";

            test("official Razor keyed loop assigns each item its stable VNode key", () => {
                const render = component.setup({
                    items: [
                        { id: 7, name: "Audit" },
                        { id: 9, name: "Deploy" }
                    ]
                }, { slots: {} });
                const nodes = render();

                assert.equal(nodes.length, 2);
                assert.deepEqual(nodes.map(node => node.name), ["li", "li"]);
                assert.deepEqual(nodes.map(node => node.props.key), [7, 9]);
                assert.deepEqual(nodes.map(node => node.props["data-id"]), [7, 9]);
                assert.deepEqual(nodes.map(node => node.children), [["Audit"], ["Deploy"]]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorNamedTupleLoop_PreservesStructuralBindingsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NamedTupleLoopRuntime.razor",
            documentText:
            """
            @foreach (var (id, label) in Entries)
            {
                <li data-id="@id">@label</li>
            }
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/named-tuple-loop-runtime")]
            public partial class NamedTupleLoopRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyList<(int Id, string Label)> Entries { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NamedTupleLoopRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "foreach", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/named-tuple-loop-runtime.mjs",
            observation.ModuleText,
            "official-named-tuple-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/named-tuple-loop-runtime.mjs";

            test("official Razor named tuple loop preserves entry bindings", () => {
                const render = component.setup({
                    entries: [
                        { id: 7, label: "Audit" },
                        { id: 9, label: "Deploy" }
                    ]
                }, { slots: {} });
                const nodes = render();

                assert.equal(nodes.length, 2);
                assert.deepEqual(nodes.map(node => node.name), ["li", "li"]);
                assert.deepEqual(nodes.map(node => node.props["data-id"]), [7, 9]);
                assert.deepEqual(nodes.map(node => node.children), [["Audit"], ["Deploy"]]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorDictionaryLoop_PreservesMapEntryBindingsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\DictionaryLoopRuntime.razor",
            documentText:
            """
            @foreach (var (stage, count) in Counts)
            {
                <li data-stage="@stage" data-count="@count"></li>
            }
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/dictionary-loop-runtime")]
            public partial class DictionaryLoopRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public IReadOnlyDictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DictionaryLoopRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "foreach", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/dictionary-loop-runtime.mjs",
            observation.ModuleText,
            "official-dictionary-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/dictionary-loop-runtime.mjs";

            test("official Razor dictionary loop preserves map entry bindings", () => {
                const render = component.setup({
                    counts: new Map([
                        ["Queued", 2],
                        ["Complete", 4]
                    ])
                }, { slots: {} });
                const nodes = render();

                assert.equal(nodes.length, 2);
                assert.deepEqual(nodes.map(node => node.name), ["li", "li"]);
                assert.deepEqual(nodes.map(node => node.props["data-stage"]), ["Queued", "Complete"]);
                assert.deepEqual(nodes.map(node => node.props["data-count"]), [2, 4]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentSlots_ExposeExecutableNamedAndDefaultSlotCallbacksOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentSlotsRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Heading="@Title">
                <Header>
                    <span data-slot="header">@Title</span>
                </Header>
                <ChildContent>
                    <strong data-slot="default">@Title</strong>
                </ChildContent>
            </SlotPanel>
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-runtime")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public string Heading { get; set; } = "";
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                    [Parameter] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/component-slots-runtime")]
                public partial class ComponentSlotsRuntime : ComponentBase, IVueComponent
                {
                    private string Title { get; } = "Account";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentSlotsRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentParameter", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "ChildContent", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "Header", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/component-slots-runtime.mjs",
            observation.ModuleText,
            "official-component-slots-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/component-slots-runtime.mjs";
            import slotPanel from "./components/slot-panel-runtime.mjs";

            test("official Razor component slots retain named callbacks and current state", () => {
                const render = component.setup({}, { slots: {} });
                const panel = render();

                assert.equal(panel.name, slotPanel);
                assert.equal(panel.props.heading, "Account");
                assert.equal(typeof panel.children.header, "function");
                assert.equal(typeof panel.children.default, "function");

                const header = panel.children.header();
                assert.equal(Array.isArray(header), true);
                assert.equal(header.length, 1);
                const [headerNode] = header;
                assert.equal(headerNode.name, "span");
                assert.equal(headerNode.props["data-slot"], "header");
                assert.deepEqual(headerNode.children, ["Account"]);

                const content = panel.children.default();
                assert.equal(Array.isArray(content), true);
                assert.equal(content.length, 1);
                const [contentNode] = content;
                assert.equal(contentNode.name, "strong");
                assert.equal(contentNode.props["data-slot"], "default");
                assert.deepEqual(contentNode.children, ["Account"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/slot-panel-runtime.mjs"] = "export default { name: \"slot-panel-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorGenericItemTemplate_ExecutesTypedSlotCallbackOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseTemplateRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <ReleaseTemplateList Entries="@Entries">
                <ItemTemplate Context="release">
                    <li data-id="@release.Id">@release.Label</li>
                </ItemTemplate>
            </ReleaseTemplateList>
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;
            using Demo.Models;
            using ECMAScript.VueContract;

            namespace Demo.Models
            {
                public sealed record ReleaseEntry(int Id, string Label);
            }

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/release-template-list-runtime")]
                public sealed class ReleaseTemplateList : ComponentBase, IVueComponent
                {
                    [Parameter] public IReadOnlyList<ReleaseEntry> Entries { get; set; } = [];
                    [ECMAScriptName("item")]
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-template-runtime")]
                public partial class ReleaseTemplateRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public IReadOnlyList<ReleaseEntry> Entries { get; set; } = [];
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseTemplateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderFragment<global::Demo.Models.ReleaseEntry>", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-template-runtime.mjs",
            observation.ModuleText,
            "official-generic-item-template-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-template-runtime.mjs";
            import templateList from "./components/release-template-list-runtime.mjs";

            test("official Razor generic item template preserves props and typed slot content", () => {
                const render = component.setup({
                    entries: [{ id: 7, label: "Audit" }]
                }, { slots: {} });
                const list = render();

                assert.equal(list.name, templateList);
                assert.deepEqual(list.props.entries, [{ id: 7, label: "Audit" }]);
                assert.equal(typeof list.children.item, "function");

                const nodes = list.children.item({ id: 9, label: "Deploy" });
                assert.equal(Array.isArray(nodes), true);
                assert.equal(nodes.length, 1);
                assert.equal(nodes[0].name, "li");
                assert.equal(nodes[0].props["data-id"], 9);
                assert.deepEqual(nodes[0].children, ["Deploy"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-template-list-runtime.mjs"] = "export default { name: \"release-template-list-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorGenericItemTemplate_ComposesConditionalAndLoopedSlotContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseTemplateControlFlowRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <ReleaseTemplateControlFlowList Entries="@Entries">
                <ItemTemplate Context="release">
                    <article data-id="@release.Id">
                        <h3>@release.Label</h3>
                        @if (release.IsUrgent)
                        {
                            <strong data-status="urgent">Urgent</strong>
                        }
                        else
                        {
                            <span data-status="standard">Standard</span>
                        }
                        <ul>
                            @foreach (var tag in release.Tags)
                            {
                                <li data-tag="@tag">@tag</li>
                            }
                        </ul>
                    </article>
                </ItemTemplate>
            </ReleaseTemplateControlFlowList>
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;
            using Demo.Models;
            using ECMAScript.VueContract;

            namespace Demo.Models
            {
                public sealed record ReleaseEntry(int Id, string Label, bool IsUrgent, string[] Tags);
            }

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/release-template-control-flow-list-runtime")]
                public sealed class ReleaseTemplateControlFlowList : ComponentBase, IVueComponent
                {
                    [Parameter] public IReadOnlyList<ReleaseEntry> Entries { get; set; } = [];
                    [ECMAScriptName("item")]
                    [Parameter] public RenderFragment<ReleaseEntry>? ItemTemplate { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-template-control-flow-runtime")]
                public partial class ReleaseTemplateControlFlowRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public IReadOnlyList<ReleaseEntry> Entries { get; set; } = [];
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseTemplateControlFlowRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderFragment<global::Demo.Models.ReleaseEntry>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "foreach", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-template-control-flow-runtime.mjs",
            observation.ModuleText,
            "official-generic-item-template-control-flow-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-template-control-flow-runtime.mjs";
            import templateList from "./components/release-template-control-flow-list-runtime.mjs";

            const collectNodes = value => {
                if (Array.isArray(value))
                    return value.flatMap(collectNodes);
                if (value === null || typeof value !== "object")
                    return [];
                return [value, ...collectNodes(value.children)];
            };

            test("official Razor generic item template composes context, conditional content, and looped rows", () => {
                const entries = [{ id: 7, label: "Audit", isUrgent: true, tags: ["release", "production"] }];
                const render = component.setup({ entries }, { slots: {} });
                const list = render();

                assert.equal(list.name, templateList);
                assert.deepEqual(list.props.entries, entries);
                assert.equal(typeof list.children.item, "function");

                const urgentNodes = collectNodes(list.children.item(entries[0]));
                const urgentArticle = urgentNodes.find(node => node.name === "article");
                assert.equal(urgentArticle.props["data-id"], 7);
                assert.deepEqual(urgentNodes.filter(node => node.name === "h3")[0].children, ["Audit"]);
                assert.equal(urgentNodes.filter(node => node.name === "__static" && node.props?.html === "<strong data-status=\"urgent\">Urgent</strong>").length, 1);
                assert.equal(urgentNodes.filter(node => node.name === "__static" && node.props?.html === "<span data-status=\"standard\">Standard</span>").length, 0);
                assert.deepEqual(
                    urgentNodes.filter(node => node.name === "li").map(node => [node.props["data-tag"], node.children]),
                    [["release", ["release"]], ["production", ["production"]]]);

                const standardNodes = collectNodes(list.children.item({ id: 9, label: "Deploy", isUrgent: false, tags: [] }));
                assert.equal(standardNodes.filter(node => node.name === "__static" && node.props?.html === "<strong data-status=\"urgent\">Urgent</strong>").length, 0);
                assert.equal(standardNodes.filter(node => node.name === "__static" && node.props?.html === "<span data-status=\"standard\">Standard</span>").length, 1);
                assert.equal(standardNodes.filter(node => node.name === "li").length, 0);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-template-control-flow-list-runtime.mjs"] = "export default { name: \"release-template-control-flow-list-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorRenderFragmentMethodGroup_ExecutesAsNamedSlotWithCurrentStateOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseHeaderMethodGroupRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <ReleaseHeaderPanel Header="@RenderHeader" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/release-header-panel-runtime")]
                public sealed class ReleaseHeaderPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-header-method-group-runtime")]
                public partial class ReleaseHeaderMethodGroupRuntime : ComponentBase, IVueComponent
                {
                    [Parameter] public string Title { get; set; } = "";
                    [Parameter] public bool IsUrgent { get; set; }

                    private void RenderHeader(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "header");
                        builder.AddAttribute(1, "data-status", IsUrgent ? "urgent" : "standard");
                        builder.OpenElement(2, "h2");
                        builder.AddContent(3, Title);
                        builder.CloseElement();
                        builder.CloseElement();
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseHeaderMethodGroupRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "RenderHeader", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentParameter", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-header-method-group-runtime.mjs",
            observation.ModuleText,
            "official-render-fragment-method-group-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-header-method-group-runtime.mjs";
            import panelComponent from "./components/release-header-panel-runtime.mjs";

            const renderHeader = props => {
                const panel = component.setup(props, { slots: {} })();
                assert.equal(panel.name, panelComponent);
                assert.equal(typeof panel.children.header, "function");
                const nodes = panel.children.header();
                assert.equal(Array.isArray(nodes), true);
                assert.equal(nodes.length, 1);
                return nodes[0];
            };

            test("official Razor RenderFragment method group captures current component state", () => {
                const urgentHeader = renderHeader({ title: "Deploy now", isUrgent: true });
                assert.equal(urgentHeader.name, "header");
                assert.equal(urgentHeader.props["data-status"], "urgent");
                assert.equal(urgentHeader.children[0].name, "h2");
                assert.deepEqual(urgentHeader.children[0].children, ["Deploy now"]);

                const standardHeader = renderHeader({ title: "Scheduled", isUrgent: false });
                assert.equal(standardHeader.props["data-status"], "standard");
                assert.deepEqual(standardHeader.children[0].children, ["Scheduled"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/release-header-panel-runtime.mjs"] = "export default { name: \"release-header-panel-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorGenericComponent_ErasesTypeArgumentAndRetainsTypedPropOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\GenericComponentRuntime.razor",
            documentText:
            """
            @typeparam TItem
            @using Demo.Components

            <GenericValue TItem="TItem" Value="@Value" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/generic-value-runtime")]
                public sealed class GenericValue<TItem> : ComponentBase, IVueComponent
                {
                    [Parameter] public TItem Value { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/generic-component-runtime")]
                public partial class GenericComponentRuntime<TItem> : ComponentBase, IVueComponent
                {
                    [Parameter] public TItem Value { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.GenericComponentRuntime`1");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "OpenComponent<global::Demo.Components.GenericValue<",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.TypeCheck<TItem>", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"./generic-value-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "value: props.value", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("TItem", StringComparison.Ordinal), script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/generic-component-runtime.mjs",
            script,
            "official-generic-component-runtime.test.mjs",
            """
            import component from "./components/generic-component-runtime.mjs";
            import genericValue from "./components/generic-value-runtime.mjs";

            Deno.test("official Razor generic component erases its type parameter but preserves the prop value", () => {
                const value = { id: 9, label: "Deploy" };
                const render = component.setup({ value }, { slots: {} });
                const node = render();

                if (node.name !== genericValue)
                    throw new Error("generic component import was not retained");
                if (node.props.value !== value)
                    throw new Error("generic component prop value was not retained");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/generic-value-runtime.mjs"] = "export default { name: \"generic-value-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorGenericComponent_MapsTypedEventCallbackToVueListenerOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\GenericEventCallbackRuntime.razor",
            documentText:
            """
            @typeparam TItem
            @using Demo.Components

            <GenericSelectable TItem="TItem"
                               Entry="@Value"
                               Selected="HandleSelected"
                               WasSelected="@WasSelected" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/generic-selectable-runtime")]
                [VueLibraryEmit(nameof(Selected), Name = "select")]
                public sealed class GenericSelectable<TItem> : ComponentBase, IVueComponent
                {
                    [Parameter] public TItem Entry { get; set; }
                    [Parameter] public EventCallback<TItem> Selected { get; set; }
                    [Parameter] public bool WasSelected { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/generic-event-callback-runtime")]
                public partial class GenericEventCallbackRuntime<TItem> : ComponentBase, IVueComponent
                {
                    [Parameter] public TItem Value { get; set; }

                    private bool WasSelected { get; set; }

                    private void HandleSelected(TItem value)
                    {
                        WasSelected = true;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.GenericEventCallbackRuntime`1");

        StringAssert.Contains(
            observation.GeneratedCSharp,
            "OpenComponent<global::Demo.Components.GenericSelectable<",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "EventCallback.Factory.Create<TItem>",
            StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"./generic-selectable-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "entry: props.value", StringComparison.Ordinal);
        StringAssert.Contains(script, "wasSelected: state.wasSelected", StringComparison.Ordinal);
        StringAssert.Contains(script, "onSelect:", StringComparison.Ordinal);
        Assert.IsFalse(script.Contains("TItem", StringComparison.Ordinal), script);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/generic-event-callback-runtime.mjs",
            script,
            "official-generic-event-callback-runtime.test.mjs",
            """
            import component from "./components/generic-event-callback-runtime.mjs";
            import genericSelectable from "./components/generic-selectable-runtime.mjs";

            Deno.test("official Razor generic callbacks retain the listener and update parent state", () => {
                const value = { id: 9, label: "Deploy" };
                const render = component.setup({ value }, { slots: {} });
                const initial = render();

                if (initial.name !== genericSelectable)
                    throw new Error("generic child component import was not retained");
                if (initial.props.entry !== value)
                    throw new Error("generic component prop value was not retained");
                if (initial.props.wasSelected !== false)
                    throw new Error("parent selection state did not start false");
                if (typeof initial.props.onSelect !== "function")
                    throw new Error("generic EventCallback<TItem> was not emitted as a Vue listener");

                initial.props.onSelect(value);

                if (render().props.wasSelected !== true)
                    throw new Error("generic Vue listener did not update parent state");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/generic-selectable-runtime.mjs"] = "export default { name: \"generic-selectable-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentSlotFromContentDescriptor_ExecutesMethodGroupOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ContentDescriptorRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <SlotPanel Heading="@Title" Header="@BuildPanelContent().Header" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/content-descriptor-slot-panel-runtime")]
                public sealed class SlotPanel : ComponentBase, IVueComponent
                {
                    [Parameter] public string Heading { get; set; } = "";
                    [Parameter] public RenderFragment? Header { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/content-descriptor-runtime")]
                public partial class ContentDescriptorRuntime : ComponentBase, IVueComponent
                {
                    private string Title { get; } = "Account";

                    private SlotContent BuildPanelContent()
                        => new() { Header = RenderHeader };

                    private void RenderHeader(RenderTreeBuilder builder)
                    {
                        builder.OpenElement(0, "strong");
                        builder.AddAttribute(1, "data-source", "descriptor");
                        builder.AddContent(2, Title);
                        builder.CloseElement();
                    }

                    private sealed class SlotContent
                    {
                        public required RenderFragment Header { get; init; }
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ContentDescriptorRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "BuildPanelContent().Header", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/content-descriptor-runtime.mjs",
            observation.ModuleText,
            "official-content-descriptor-slot-runtime.test.mjs",
            """
            import component from "./components/content-descriptor-runtime.mjs";
            import slotPanel from "./components/content-descriptor-slot-panel-runtime.mjs";

            Deno.test("official Razor content descriptor resolves a method-group slot", () => {
                const panel = component.setup({}, { slots: {} })();
                if (panel.name !== slotPanel)
                    throw new Error("slot panel import was not retained");
                if (panel.props.heading !== "Account")
                    throw new Error("component prop was not retained");
                if (typeof panel.children.header !== "function")
                    throw new Error("descriptor header was not emitted as a Vue slot");

                const nodes = panel.children.header();
                if (!Array.isArray(nodes) || nodes.length !== 1)
                    throw new Error("method-group slot did not return one node");
                const [header] = nodes;
                if (header.name !== "strong")
                    throw new Error("method-group slot element was not retained");
                if (header.props["data-source"] !== "descriptor")
                    throw new Error("method-group slot attribute was not retained");
                if (header.children[0] !== "Account")
                    throw new Error("method-group slot state was not retained");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/content-descriptor-slot-panel-runtime.mjs"] = "export default { name: \"content-descriptor-slot-panel-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorDirectRender_RetainsReachableHelperAndPrunesUnreachableHelperOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\DirectRenderReachabilityRuntime.razor",
            documentText:
            """
            <section data-title="@FormatTitle()">@FormatTitle()</section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/direct-render-reachability-runtime")]
            public partial class DirectRenderReachabilityRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = "Release";

                private string FormatTitle() => Title + " ready";

                private string NeverRendered() => "unreachable";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DirectRenderReachabilityRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "function formatTitle", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("neverRendered", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/direct-render-reachability-runtime.mjs",
            observation.ModuleText,
            "official-direct-render-reachability-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/direct-render-reachability-runtime.mjs";

            test("official Razor direct render keeps helpers reachable from the generated render expression", () => {
                const section = component.setup({ title: "Deploy" }, { slots: {} })();
                assert.equal(section.name, "section");
                assert.equal(section.props["data-title"], "Deploy ready");
                assert.deepEqual(section.children, ["Deploy ready"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLifecycleHooks_RunAcrossPropUpdateAndUnmountOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\LifecycleRuntime.razor",
            documentText:
            """
            <p data-title="@Title">@Log</p>
            """,
            codeBehindSource:
            """
            using System;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/lifecycle-runtime")]
            public partial class LifecycleRuntime : ComponentBase, IVueComponent, IDisposable
            {
                [Parameter]
                public string Title { get; set; } = "";

                private string log = "";

                private string Log => log;

                protected override void OnInitialized()
                {
                    log += "init|";
                }

                protected override void OnParametersSet()
                {
                    log += "params:" + Title + "|";
                }

                protected override void OnAfterRender(bool firstRender)
                {
                    log += firstRender ? "after:first|" : "after:update|";
                }

                public void Dispose()
                {
                    log += "dispose|";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LifecycleRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "watch(", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onMounted(", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onUpdated(", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onUnmounted(", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/lifecycle-runtime.mjs",
            observation.ModuleText,
            "official-lifecycle-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runMounted, __runUpdated, __runUnmounted, __runWatchers } from "vue";

            import component from "./components/lifecycle-runtime.mjs";

            test("official Razor lifecycle hooks observe props and dispose after unmount", () => {
                const props = { title: "one" };
                const render = component.setup(props, { slots: {} });

                assert.equal(render().props["data-title"], "one");
                assert.deepEqual(render().children, ["init|params:one|"]);

                __runMounted();
                assert.deepEqual(render().children, ["init|params:one|after:first|"]);

                props.title = "two";
                __runWatchers();
                __runUpdated();
                assert.equal(render().props["data-title"], "two");
                assert.deepEqual(render().children, ["init|params:one|after:first|params:two|after:update|"]);

                __runUnmounted();
                assert.deepEqual(render().children, ["init|params:one|after:first|params:two|after:update|dispose|"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentReference_UpdatesStateUsedByNextRenderOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ComponentReferenceRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <ReferenceChild @ref="child" HasReference="@(child is not null)" />
            """,
            codeBehindSource:
            """
            using Demo.Components;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/reference-child-runtime")]
                public partial class ReferenceChild : ComponentBase, IVueComponent
                {
                    [Parameter]
                    public bool HasReference { get; set; }

                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                    }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/component-reference-runtime")]
                public partial class ComponentReferenceRuntime : ComponentBase, IVueComponent
                {
                    private ReferenceChild? child;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ComponentReferenceRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentReferenceCapture", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/component-reference-runtime.mjs",
            observation.ModuleText,
            "official-component-reference-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/component-reference-runtime.mjs";

            test("official Razor component ref changes state observed by the next render", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.props.hasReference, false);
                assert.equal(typeof initial.props.ref, "function");

                initial.props.ref({ name: "child-instance" });

                const updated = render();
                assert.equal(updated.props.hasReference, true);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/reference-child-runtime.mjs"] = "export default { name: \"reference-child-runtime\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorEventCallback_AwaitsListenerBeforeUpdatingStateOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Components\EventCallbackRuntime.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button type="button" @onclick="CommitAsync" data-last="@LastCommit">@Label</button>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Components;

            [ECMAScriptModule("./components/event-callback-runtime")]
            public partial class EventCallbackRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Label { get; set; } = "Commit";

                [Parameter]
                public EventCallback<string> OnCommit { get; set; }

                private string Value { get; set; } = "draft";
                private string LastCommit { get; set; } = "none";

                private async Task CommitAsync()
                {
                    await OnCommit.InvokeAsync(Value);
                    LastCommit = Value;
                }
            }
            """,
            rootNamespace: "Demo.Components",
            componentMetadataName: "Demo.Components.EventCallbackRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "EventCallback.Factory.Create", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/event-callback-runtime.mjs",
            observation.ModuleText,
            "official-event-callback-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/event-callback-runtime.mjs";

            test("official Razor EventCallback waits for the listener before continuing", async () => {
                let releaseListener;
                const calls = [];
                const render = component.setup({
                    onCommit: async value => {
                        calls.push(`started:${value}`);
                        await new Promise(resolve => { releaseListener = resolve; });
                        calls.push(`completed:${value}`);
                    }
                }, { slots: {} });
                const initial = render();
                assert.equal(initial.props["data-last"], "none");
                assert.equal(typeof initial.props.onClick, "function");

                const pending = initial.props.onClick();
                await Promise.resolve();
                assert.deepEqual(calls, ["started:draft"]);
                assert.equal(render().props["data-last"], "none");

                releaseListener();
                await pending;

                assert.deepEqual(calls, ["started:draft", "completed:draft"]);
                assert.equal(render().props["data-last"], "draft");
            });
            """);
    }
}
