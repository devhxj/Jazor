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
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/bind-after-child-runtime")]
                [VueProp(nameof(Value), VuePropKind.Model, Name = "modelValue", AcceptsBinding = true)]
                [VueLibraryEmit(nameof(ValueChanged), VueEmitKind.ModelUpdate, Name = "update:modelValue")]
                [VueProp(nameof(LastObserved), Name = "lastObserved")]
                public sealed class BindAfterChild : ComponentBase, IVueComponent
                {
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
            using ECMAScript.VueContract.Descriptor;

            namespace Demo.Components
            {
                [ECMAScriptModule("./components/slot-panel-runtime")]
                [VueProp(nameof(Heading), Name = "heading")]
                [VueSlot(nameof(ChildContent), IsDefault = true)]
                [VueSlot(nameof(Header), Name = "header")]
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
