namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Exercises the conservative Vue compiler-style optimizations on the official Razor SG path.
/// 这里验证可观测身份和 scope 边界，避免只靠生成文本掩盖跨实例/循环闭包回归。
/// </summary>
[TestClass]
public sealed class RazorSgVueCompilerOptimizationTests
{
    [TestMethod]
    public async Task BuildComponent_StaticPropsAndMarkupVNode_AreModuleHoistsSharedByRenderAndSetupInstances()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\StaticHoists.razor",
            documentText:
            """
            <section class="fixed" data-kind="catalog">@Label</section>
            <strong>module-static</strong>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/static-hoists")]
            public partial class StaticHoists : ComponentBase, IVueComponent
            {
                [Parameter] public string Label { get; set; } = string.Empty;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.StaticHoists");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "const __jazor$hoistedProps0 = { class: \"fixed\", \"data-kind\": \"catalog\" };",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "createStaticVNode(\"<strong>module-static</strong>\", 1)",
            StringComparison.Ordinal);
        Assert.IsLessThan(
            observation.ModuleText.IndexOf("function createStaticHoistsSetupScope", StringComparison.Ordinal),
            observation.ModuleText.IndexOf("const __jazor$hoistedProps0", StringComparison.Ordinal));

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/static-hoists.mjs",
            observation.ModuleText,
            "static-hoists-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/static-hoists.mjs";

            function findSection(vnode) {
                return vnode.children.find(child => child?.name === "section");
            }

            function findStatic(vnode) {
                return vnode.children.find(child =>
                    child?.name === "__static" && child.props.html.includes("module-static"));
            }

            test("static Vue values are reused without crossing a setup closure boundary", () => {
                const firstRender = component.setup({ Label: "first" }, { slots: {} });
                const secondRender = component.setup({ Label: "second" }, { slots: {} });
                const first = firstRender();
                const again = firstRender();
                const otherInstance = secondRender();

                const firstSection = findSection(first);
                const sameSection = findSection(again);
                const otherSection = findSection(otherInstance);
                assert.equal(firstSection.props, sameSection.props);
                assert.equal(firstSection.props, otherSection.props);

                const firstStatic = findStatic(first);
                const sameStatic = findStatic(again);
                const otherStatic = findStatic(otherInstance);
                assert.equal(firstStatic, sameStatic);
                assert.equal(firstStatic, otherStatic);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_DynamicAndCommentFirstMarkup_ImportSharedRuntimeOnlyOnce()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\DynamicRawMarkup.razor",
            documentText:
            """
            @((MarkupString)"<!--lead--><b>fixed</b>")
            @Summary
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/dynamic-raw-markup")]
            public partial class DynamicRawMarkup : ComponentBase, IVueComponent
            {
                [Parameter] public MarkupString Summary { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DynamicRawMarkup");

        Assert.AreEqual(
            1,
            CountOccurrences(observation.ModuleText, "from \"@jazor/vue-runtime/raw-markup.mjs\""),
            observation.ModuleText);
        Assert.AreEqual(
            0,
            CountOccurrences(observation.ModuleText, "function __jazor$createRawMarkup"),
            observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_ChildPlans_UseTextAndNestedBlocksWithoutPromotingOpaqueChildren()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ChildPlans.razor",
            documentText:
            """
            <section id="single">@Value</section>
            <section id="mixed"><strong>fixed</strong>@Value</section>
            <article id="nested"><span>@Value</span></article>
            <section id="conditional">
                @if (Show)
                {
                    <span>@Value</span>
                }
            </section>
            <section id="raw">@Summary</section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/child-plans")]
            public partial class ChildPlans : ComponentBase, IVueComponent
            {
                [Parameter] public string Value { get; set; } = string.Empty;
                [Parameter] public bool Show { get; set; }
                [Parameter] public MarkupString? Summary { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ChildPlans");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "createElementBlock(\"section\", __jazor$hoistedProps0, props.Value, 1)",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "createTextVNode(props.Value, 1)",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "createElementBlock(\"article\", __jazor$hoistedProps2, [(openBlock(), createElementBlock(\"span\", null, props.Value, 1))])",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "h(\"section\", __jazor$hoistedProps3",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "h(\"section\", __jazor$hoistedProps4",
            StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/child-plans.mjs",
            observation.ModuleText,
            "child-plans-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/child-plans.mjs";

            function findById(vnode, id) {
                if (vnode == null || typeof vnode !== "object") return null;
                if (vnode?.props?.id === id) return vnode;
                const children = Array.isArray(vnode?.children) ? vnode.children : [vnode?.children];
                for (const child of children) {
                    const found = findById(child, id);
                    if (found) return found;
                }
                return null;
            }

            test("child plans retain Vue patch metadata and leave opaque content on h", () => {
                const props = { Value: "first", Show: true, Summary: null };
                const render = component.setup(props, { slots: {} });
                const initial = render();
                const single = findById(initial, "single");
                const mixed = findById(initial, "mixed");
                const nested = findById(initial, "nested");
                const conditional = findById(initial, "conditional");
                const raw = findById(initial, "raw");

                assert.equal(single.block, "element");
                assert.equal(single.patchFlag, 1);
                assert.equal(mixed.block, "element");
                assert.equal(mixed.children.at(-1).name, "__text");
                assert.equal(mixed.children.at(-1).patchFlag, 1);
                assert.equal(nested.block, "element");
                assert.equal(nested.children[0].block, "element");
                assert.equal(conditional.block, undefined);
                assert.equal(raw.block, undefined);

                props.Value = "second";
                props.Show = false;
                const updated = render();
                assert.equal(findById(updated, "single").children, "second");
                assert.equal(findById(updated, "mixed").children.at(-1).children, "second");
                assert.equal(findById(updated, "nested").children[0].children, "second");
            });
            """);
    }

    private static int CountOccurrences(string value, string needle)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(needle, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += needle.Length;
        }

        return count;
    }

    [TestMethod]
    public async Task BuildComponent_DynamicLeafAndImmediateChildren_UseTheConservativeBlockBoundary()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\BlockBoundary.razor",
            documentText:
            """
            <input value="@Value" /><section class="@Css">@Value</section>@Summary
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/block-boundary")]
            public partial class BlockBoundary : ComponentBase, IVueComponent
            {
                [Parameter] public string Value { get; set; } = string.Empty;
                [Parameter] public string Css { get; set; } = string.Empty;
                [Parameter] public MarkupString? Summary { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.BlockBoundary");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "openBlock(), createElementBlock(\"input\", { value: props.Value }, null, 8, [\"value\"])",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "openBlock(), createElementBlock(\"section\", { class: props.Css }, props.Value, 3)",
            StringComparison.Ordinal);
        Assert.IsFalse(
            observation.ModuleText.Contains("const __jazor$hoistedProps", StringComparison.Ordinal),
            observation.ModuleText);
        Assert.IsFalse(
            observation.ModuleText.Contains("const __jazor$hoistedStatic0 = createStaticVNode(props.Summary", StringComparison.Ordinal),
            observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_SlotsAndForeachBodies_DoNotHoistOrCacheCapturedValues()
    {
        var slotObservation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\SlotScope.razor",
            documentText:
            """
            @using Demo.Components

            <SlotHost>
                <span class="slot">slot body</span>
            </SlotHost>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/slot-scope")]
            public partial class SlotScope : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SlotScope",
            supportingSources: new Dictionary<string, string>
            {
                ["Components/SlotHost.cs"] =
                """
                namespace Demo.Components;

                [ECMAScriptModule("./components/slot-host")]
                public sealed class SlotHost : ComponentBase, IVueComponent
                {
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(slotObservation.ModuleText);
        StringAssert.Contains(slotObservation.ModuleText, "createBlock(", StringComparison.Ordinal);
        StringAssert.Contains(slotObservation.ModuleText, "}, 1024)", StringComparison.Ordinal);
        Assert.IsFalse(
            slotObservation.ModuleText.Contains("const __jazor$hoistedProps", StringComparison.Ordinal),
            slotObservation.ModuleText);

        var loopObservation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\LoopScope.razor",
            documentText:
            """
            @foreach (var entry in Entries)
            {
                <button class="fixed" @onclick="() => Select(entry)">@entry</button>
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/loop-scope")]
            public partial class LoopScope : ComponentBase, IVueComponent
            {
                private string[] Entries { get; } = ["first", "second"];

                private void Select(string entry)
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LoopScope");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(loopObservation.ModuleText);
        StringAssert.Contains(loopObservation.ModuleText, "Array.from(state.Entries ?? []", StringComparison.Ordinal);
        Assert.IsFalse(loopObservation.ModuleText.Contains("renderList(state.Entries", StringComparison.Ordinal), loopObservation.ModuleText);
        Assert.IsFalse(
            loopObservation.ModuleText.Contains("__jazor$handlerCache", StringComparison.Ordinal),
            loopObservation.ModuleText);
        Assert.IsFalse(
            loopObservation.ModuleText.Contains("const __jazor$hoistedProps", StringComparison.Ordinal),
            loopObservation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_StableBindHandler_IsCachedPerSetupInstanceOnly()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\StableHandler.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <input @bind="Value" @bind:event="oninput" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/stable-handler")]
            public partial class StableHandler : ComponentBase, IVueComponent
            {
                private string Value { get; set; } = "initial";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.StableHandler");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "const __jazor$handlerCache = [];",
            StringComparison.Ordinal);
        Assert.IsGreaterThan(
            observation.ModuleText.IndexOf("function createStableHandlerSetupScope", StringComparison.Ordinal),
            observation.ModuleText.IndexOf("const __jazor$handlerCache = [];", StringComparison.Ordinal));

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/stable-handler.mjs",
            observation.ModuleText,
            "stable-handler-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/stable-handler.mjs";

            test("cached bind handlers keep identity inside one setup and not across instances", () => {
                const firstRender = component.setup({}, { slots: {} });
                const secondRender = component.setup({}, { slots: {} });

                const firstHandler = firstRender().props.onInput;
                const sameHandler = firstRender().props.onInput;
                const otherHandler = secondRender().props.onInput;
                assert.equal(firstHandler, sameHandler);
                assert.notEqual(firstHandler, otherHandler);
            });
            """);
    }
}
