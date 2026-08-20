namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Locks the ComponentBase.SetParametersAsync adapter contract. The authored component stays
/// entirely in the normal Blazor shape; Vue setup only owns the runtime bridge.
/// 自定义参数入口必须保留 Blazor 生命周期顺序，不能退化成独立 props watch。
/// </summary>
[TestClass]
public sealed class RazorSgSetParametersAsyncRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_CustomSetParametersAsyncDrivesBaseLifecycleForInitialAndReplacementValues()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SetParametersRuntime.razor"),
            documentText:
            """
            <p>@Log</p>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/set-parameters-runtime")]
            public partial class SetParametersRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = "initial";

                private string log = "";

                private string Log => log;

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    log += "before:" + Title + "|";
                    await base.SetParametersAsync(parameters);
                    log += "after:" + Title + "|";
                }

                protected override void OnInitialized()
                {
                    log += "init|";
                }

                protected override void OnParametersSet()
                {
                    log += "parameters:" + Title + "|";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SetParametersRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "runSetParametersAsync", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "applyComponentBaseParameters", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/set-parameters-runtime.mjs",
            observation.ModuleText,
            "set-parameters-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runWatchers } from "vue";

            import component from "./components/set-parameters-runtime.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("custom SetParametersAsync delegates lifecycle through ComponentBase", async () => {
                const props = { Title: "first" };
                const render = component.setup(props, { slots: {} });

                await settle();
                assert.equal(
                    render().children,
                    "before:initial|init|parameters:first|after:first|");

                props.Title = "second";
                __runWatchers();
                await settle();

                assert.equal(
                    render().children,
                    "before:initial|init|parameters:first|after:first|before:first|parameters:second|after:second|");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_ParameterViewPreservesSparseValuesAliasesAndExplicitUndefined()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SetParametersSparse.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/set-parameters-sparse")]
            public partial class SetParametersSparse : ComponentBase, IVueComponent
            {
                [Parameter, ECMAScriptName("title-value")]
                public string? Title { get; set; } = "default";

                private string Log => Title ?? "<null>";

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SetParametersSparse");

        StringAssert.Contains(observation.ModuleText, "title-value", StringComparison.Ordinal);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/set-parameters-sparse.mjs",
            observation.ModuleText,
            "set-parameters-sparse.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runWatchers } from "vue";

            import component from "./components/set-parameters-sparse.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("ParameterView uses source names while preserving sparse presence", async () => {
                const props = { "title-value": "first" };
                const render = component.setup(props, { slots: {} });

                await settle();
                assert.equal(render().children, "first");

                delete props["title-value"];
                __runWatchers();
                await settle();
                assert.equal(render().children, "first");

                props["title-value"] = undefined;
                __runWatchers();
                await settle();
                assert.equal(render().children, "<null>");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_ParameterViewSetParameterPropertiesUsesNormalBlazorEntryPoint()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SetParametersProperties.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/set-parameters-properties")]
            public partial class SetParametersProperties : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = "default";

                private string log = "";
                private string Log => log;

                public override Task SetParametersAsync(ParameterView parameters)
                {
                    log += "before:" + Title + "|";
                    parameters.SetParameterProperties(this);
                    log += "after:" + Title + "|";
                    return Task.CompletedTask;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SetParametersProperties");

        StringAssert.Contains(observation.ModuleText, "applyParameterProperties", StringComparison.Ordinal);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/set-parameters-properties.mjs",
            observation.ModuleText,
            "set-parameters-properties.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/set-parameters-properties.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("ParameterView.SetParameterProperties keeps the authored Blazor call shape", async () => {
                const render = component.setup({ Title: "next" }, { slots: {} });
                await settle();
                assert.equal(render().children, "before:default|after:next|");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_ParameterViewCarriesRenderFragmentSlotsBySourceName()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SetParametersSlot.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/set-parameters-slot")]
            public partial class SetParametersSlot : ComponentBase, IVueComponent
            {
                [Parameter, ECMAScriptName("header-slot")]
                public RenderFragment? Header { get; set; }

                private string Log => Header is null ? "none" : "provided";

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SetParametersSlot");

        StringAssert.Contains(observation.ModuleText, "header-slot", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "createSnapshot", StringComparison.Ordinal);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/set-parameters-slot.mjs",
            observation.ModuleText,
            "set-parameters-slot.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runWatchers } from "vue";

            import component from "./components/set-parameters-slot.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("RenderFragment parameters participate in the ParameterView snapshot", async () => {
                const slots = { "header-slot": () => null };
                const props = {};
                const render = component.setup(props, { slots });

                await settle();
                assert.equal(render().children, "provided");

                delete slots["header-slot"];
                __runWatchers();
                await settle();
                assert.equal(render().children, "none");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_ParameterViewQueuesAsyncUpdatesInOrder()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SetParametersQueue.razor"),
            documentText: "<p>@Log</p>",
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/set-parameters-queue")]
            public partial class SetParametersQueue : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = "default";

                private string log = "";
                private string Log => log;

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    log += "start:" + Title + "|";
                    await Task.CompletedTask;
                    await base.SetParametersAsync(parameters);
                    log += "end:" + Title + "|";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SetParametersQueue");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/set-parameters-queue.mjs",
            observation.ModuleText,
            "set-parameters-queue.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runWatchers } from "vue";

            import component from "./components/set-parameters-queue.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("queued ParameterView updates do not overlap authored async work", async () => {
                const props = { Title: "one" };
                const render = component.setup(props, { slots: {} });

                props.Title = "two";
                __runWatchers();
                props.Title = "three";
                __runWatchers();
                await settle();
                await settle();

                assert.equal(
                    render().children,
                    "start:default|end:one|start:one|end:two|start:two|end:three|");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_ParameterViewRethrowsAuthoredFailureThroughRender()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/SetParametersFailure.razor"),
            documentText: "<p>@Title</p>",
            codeBehindSource:
            """
            using System;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/set-parameters-failure")]
            public partial class SetParametersFailure : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = "default";

                public override Task SetParametersAsync(ParameterView parameters)
                    => throw new Exception("parameter failure");
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.SetParametersFailure");

        StringAssert.Contains(observation.ModuleText, "hasParameterFailure", StringComparison.Ordinal);
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/set-parameters-failure.mjs",
            observation.ModuleText,
            "set-parameters-failure.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/set-parameters-failure.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));

            test("a rejected SetParametersAsync task reaches the render error path", async () => {
                const render = component.setup({ Title: "next" }, { slots: {} });
                await settle();

                assert.throws(() => render(), /parameter failure/);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_CaptureUnmatchedValuesFlowsThroughParameterViewAndAttrs()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CaptureUnmatchedRuntime.razor"),
            documentText: "<p>@HasAttributes:@Title</p>",
            codeBehindSource:
            """
            using System.Collections.Generic;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/capture-unmatched-runtime")]
            public partial class CaptureUnmatchedRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Title { get; set; } = "default";

                [Parameter(CaptureUnmatchedValues = true)]
                public IReadOnlyDictionary<string, object?>? AdditionalAttributes { get; set; }

                private string HasAttributes => AdditionalAttributes is null ? "none" : "present";

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    await base.SetParametersAsync(parameters);
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CaptureUnmatchedRuntime");

        StringAssert.Contains(observation.ModuleText, "parameterProps", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "attrs", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "Object.defineProperty", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/capture-unmatched-runtime.mjs",
            observation.ModuleText,
            "capture-unmatched-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runWatchers } from "vue";

            import component from "./components/capture-unmatched-runtime.mjs";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));
            const text = render => render().children.map(child => child.children ?? child).join("");

            test("CaptureUnmatchedValues combines Vue attrs with the authored parameter dictionary", async () => {
                const props = {
                    Title: "first",
                    AdditionalAttributes: { "data-explicit": "yes" }
                };
                const render = component.setup(props, {
                    slots: {},
                    attrs: { "data-fallthrough": "yes" }
                });

                await settle();
                assert.equal(text(render), "present:first");

                props.Title = "second";
                props.AdditionalAttributes = {
                    "data-explicit": "yes",
                    "aria-label": "updated"
                };
                __runWatchers();
                await settle();
                assert.equal(text(render), "present:second");
            });
            """);
    }
}
