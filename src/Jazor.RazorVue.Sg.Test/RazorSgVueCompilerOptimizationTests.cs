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
            "createStaticVNode(\"<strong>module-static</strong>",
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
        StringAssert.Contains(observation.ModuleText, "h(\"section\", { class: props.Css }", StringComparison.Ordinal);
        Assert.IsFalse(
            observation.ModuleText.Contains("createElementBlock(\"section\"", StringComparison.Ordinal),
            observation.ModuleText);
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
