namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgCascadingValueRuntimeTests
{
    private const string CascadeVueRuntime =
        """
        const providers = new Map();
        const watchers = [];
        const sameWatchValue = (left, right) => Array.isArray(left) && Array.isArray(right)
            ? left.length === right.length && left.every((value, index) => Object.is(value, right[index]))
            : Object.is(left, right);
        export function __resetProviders() { providers.clear(); watchers.length = 0; }
        export function __runWatchers() { for (const watcher of watchers) watcher(); }
        export function defineComponent(options) { return options; }
        export const Fragment = Symbol("Fragment");
        export function h(name, props, children) { return { name, props, children }; }
        export function reactive(value) { return value; }
        export function ref(value) { return { value }; }
        export function unref(value) { return value && typeof value === "object" && "value" in value ? value.value : value; }
        export function provide(key, value) { providers.set(key, value); }
        export function inject(key, fallback) { return providers.has(key) ? providers.get(key) : fallback; }
        export function onServerPrefetch(callback) { callback(); }
        export function watch(source, callback) {
            let previous = source();
            watchers.push(() => {
                const next = source();
                if (!sameWatchValue(next, previous)) {
                    previous = next;
                    callback(next);
                }
            });
        }
        export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
        export function createCommentVNode(comment) { return { name: "__comment", children: comment }; }
        export function openBlock() { return null; }
        export function createElementBlock(name, props, children) { return { name, props, children }; }
        export function createBlock(name, props, children) { return { name, props, children }; }
        export function withCtx(fn) { return fn; }
        """;

    [TestMethod]
    public async Task BuildComponent_OfficialCascadingValueProvidesTypedNamedValueToChildOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CascadingRuntime.razor"),
            documentText:
            """
            <CascadingValue Value="@Theme" Name="theme">
                <span>provider</span>
            </CascadingValue>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/cascading-runtime")]
                public partial class CascadingRuntime : ComponentBase, IVueComponent
                {
                    public string Theme { get; } = "dark";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CascadingRuntime");

        var consumerObservation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CascadingConsumer.razor"),
            documentText: "<span>@Theme:@Updates</span>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/cascading-consumer")]
                public partial class CascadingConsumer : ComponentBase, IVueComponent
                {
                    [CascadingParameter(Name = "theme")]
                    public string Theme { get; set; } = "missing";

                    public int Updates { get; private set; }

                    protected override void OnParametersSet()
                    {
                        Updates++;
                    }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CascadingConsumer");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "@jazor/vue-runtime/cascading.mjs", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "__jazorCascadeType", StringComparison.Ordinal);
        StringAssert.Contains(consumerObservation.ModuleText, "jazor:cascade:", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("JAZORVCA008", StringComparison.Ordinal));

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/cascading-runtime.mjs",
            observation.ModuleText,
            "cascading-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import host from "./components/cascading-runtime.mjs";
            import consumer from "./components/cascading-consumer.mjs";
            import { __runWatchers, __resetProviders } from "vue";

            test("standard CascadingValue keeps named typed values in the browser adapter", () => {
                __resetProviders();
                const hostRender = host.setup({}, { slots: {} });
                const rootVNode = hostRender();
                const providerVNode = Array.isArray(rootVNode)
                    ? rootVNode.find(node => node?.props?.__jazorCascadeType)
                    : rootVNode;
                const provider = providerVNode.name;
                assert.equal(providerVNode.props.value, "dark");
                provider.setup(providerVNode.props, { slots: providerVNode.children });
                const consumerRender = consumer.setup({}, { slots: {} });

                assert.equal(consumerRender().children.join(""), "dark:1");

                providerVNode.props.value = "light";
                __runWatchers();
                assert.equal(consumerRender().children.join(""), "light:2");
            });
            """,
            vueRuntimeSource:
            """
            const providers = new Map();
            const watchers = [];
            export function __resetProviders() { providers.clear(); watchers.length = 0; }
            export function __runWatchers() { for (const watcher of watchers) watcher(); }
            export function defineComponent(options) { return options; }
            export const Fragment = Symbol("Fragment");
            export function h(name, props, children) { return { name, props, children }; }
            export function reactive(value) { return value; }
            export function ref(value) { return { value }; }
            export function unref(value) { return value?.value ?? value; }
            export function provide(key, value) { providers.set(key, value); }
            export function inject(key, fallback) { return providers.has(key) ? providers.get(key) : fallback; }
            export function watch(source, callback) { watchers.push(() => callback(source())); }
            export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
            export function createCommentVNode(comment) { return { name: "__comment", children: comment }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            export function withCtx(fn) { return fn; }
            """,
            supportingModules: new Dictionary<string, string>
            {
                ["components/cascading-consumer.mjs"] = consumerObservation.ModuleText
            });
    }

    [TestMethod]
    public async Task BuildComponent_CustomSetParametersAsyncQueuesCascadeSnapshotsWithBlazorOrdering()
    {
        var provider = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CascadingParameterViewProvider.razor"),
            documentText:
            """
            <CascadingValue Value="@Theme" Name="theme">
                <span>provider</span>
            </CascadingValue>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/cascading-parameter-view-provider")]
            public partial class CascadingParameterViewProvider : ComponentBase, IVueComponent
            {
                public string Theme { get; } = "dark";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CascadingParameterViewProvider");

        var consumer = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/CascadingParameterViewConsumer.razor"),
            documentText: "<span>@Log</span>",
            codeBehindSource:
            """
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/cascading-parameter-view-consumer")]
            public partial class CascadingParameterViewConsumer : ComponentBase, IVueComponent
            {
                [CascadingParameter(Name = "theme")]
                public string Theme { get; set; } = "default";

                private string log = "";

                private string Log => log;

                public override async Task SetParametersAsync(ParameterView parameters)
                {
                    log += "before:" + Theme + "|";
                    await base.SetParametersAsync(parameters);
                    log += "after:" + Theme + "|";
                }

                protected override void OnParametersSet()
                {
                    log += "parameters:" + Theme + "|";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CascadingParameterViewConsumer");

        StringAssert.Contains(consumer.ModuleText, "cascadingParameterSnapshot", StringComparison.Ordinal);
        StringAssert.Contains(consumer.ModuleText, "runSetParametersAsync", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/cascading-parameter-view-provider.mjs",
            provider.ModuleText,
            "cascading-parameter-view.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import providerHost from "./components/cascading-parameter-view-provider.mjs";
            import consumer from "./components/cascading-parameter-view-consumer.mjs";
            import { __runWatchers, __resetProviders } from "vue";

            const settle = () => new Promise(resolve => setTimeout(resolve, 0));
            const text = render => {
                const children = render().children;
                return Array.isArray(children) ? children.join("") : children;
            };

            test("cascade updates preserve SetParametersAsync before/base/after ordering", async () => {
                __resetProviders();
                const rootVNode = providerHost.setup({}, { slots: {} })();
                const providerVNode = Array.isArray(rootVNode)
                    ? rootVNode.find(node => node?.props?.__jazorCascadeType)
                    : rootVNode;
                providerVNode.name.setup(providerVNode.props, { slots: providerVNode.children });
                const render = consumer.setup({}, { slots: {} });

                await settle();
                assert.equal(text(render), "before:default|parameters:dark|after:dark|");

                providerVNode.props.value = "light";
                __runWatchers();
                await settle();
                assert.equal(text(render), "before:default|parameters:dark|after:dark|before:dark|parameters:light|after:light|");
            });
            """,
            vueRuntimeSource: CascadeVueRuntime,
            supportingModules: new Dictionary<string, string>
            {
                ["components/cascading-parameter-view-consumer.mjs"] = consumer.ModuleText
            });
    }

    [TestMethod]
    public async Task BuildComponent_CascadesRetainDefaultsAllowExplicitNullAndHonorIsFixed()
    {
        var provider = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/FixedNullCascadeProvider.razor"),
            documentText:
            """
            <CascadingValue Value="@Theme" Name="theme" IsFixed="true">
                <span>provider</span>
            </CascadingValue>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/fixed-null-cascade-provider")]
            public partial class FixedNullCascadeProvider : ComponentBase, IVueComponent
            {
                public string? Theme { get; } = null;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.FixedNullCascadeProvider");

        var consumer = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/FixedNullCascadeConsumer.razor"),
            documentText: "<span>@Display:@Updates</span>",
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/fixed-null-cascade-consumer")]
            public partial class FixedNullCascadeConsumer : ComponentBase, IVueComponent
            {
                [CascadingParameter(Name = "theme")]
                public string? Theme { get; set; } = "fallback";

                public int Updates { get; private set; }

                private string Display => Theme is null ? "<null>" : Theme;

                protected override void OnParametersSet()
                {
                    Updates++;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.FixedNullCascadeConsumer");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/fixed-null-cascade-provider.mjs",
            provider.ModuleText,
            "fixed-null-cascade.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import providerHost from "./components/fixed-null-cascade-provider.mjs";
            import consumer from "./components/fixed-null-cascade-consumer.mjs";
            import { __runWatchers, __resetProviders } from "vue";

            const text = render => {
                const children = render().children;
                return Array.isArray(children) ? children.join("") : children;
            };

            test("no provider keeps the CLR default while null and fixed provider values remain distinct", () => {
                __resetProviders();
                const noProviderRender = consumer.setup({}, { slots: {} });
                assert.equal(text(noProviderRender), "fallback:1");

                __resetProviders();
                const rootVNode = providerHost.setup({}, { slots: {} })();
                const providerVNode = Array.isArray(rootVNode)
                    ? rootVNode.find(node => node?.props?.__jazorCascadeType)
                    : rootVNode;
                providerVNode.name.setup(providerVNode.props, { slots: providerVNode.children });
                const render = consumer.setup({}, { slots: {} });
                assert.equal(text(render), "<null>:1");

                providerVNode.props.value = "ignored";
                __runWatchers();
                assert.equal(text(render), "<null>:1");
            });
            """,
            vueRuntimeSource: CascadeVueRuntime,
            supportingModules: new Dictionary<string, string>
            {
                ["components/fixed-null-cascade-consumer.mjs"] = consumer.ModuleText
            });
    }
}
