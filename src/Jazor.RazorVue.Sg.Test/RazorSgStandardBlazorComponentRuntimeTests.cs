namespace Jazor.RazorVue.Sg.Test;

/// <summary>
/// Keeps standard Blazor component tags on the authored surface while asserting that the
/// generated module selects the framework-owned browser adapters.
/// </summary>
[TestClass]
public sealed class RazorSgStandardBlazorComponentRuntimeTests
{
    [TestMethod]
    public async Task DynamicComponent_UsesStaticComponentRegistryAdapter()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/DynamicHost.razor"),
            documentText:
            """
            <DynamicComponent Type="@typeof(Child)" />
            """,
            codeBehindSource:
            """
            using System;
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/dynamic-host")]
            public partial class DynamicHost : ComponentBase, IVueComponent;
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DynamicHost",
            supportingSources: new Dictionary<string, string>
            {
                ["Child.razor.cs"] =
                """
                using Microsoft.AspNetCore.Components;
                using Microsoft.AspNetCore.Components.Rendering;
                using ECMAScript;

                namespace Demo.Pages;

                [ECMAScriptModule("./components/child")]
                public sealed class Child : ComponentBase, IVueComponent
                {
                    protected override void BuildRenderTree(RenderTreeBuilder builder)
                    {
                        builder.AddContent(0, "child");
                    }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "blazor-components.mjs", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "__jazorComponent", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "./child.mjs", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/dynamic-host.mjs",
            observation.ModuleText,
            "dynamic-host.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/dynamic-host.mjs";
            import { DynamicComponent } from "@jazor/vue-runtime/blazor-components.mjs";

            test("DynamicComponent adapter is a normal Vue component", () => {
                assert.equal(typeof DynamicComponent, "object");
                const render = component.setup({}, { slots: {} });
                const vnode = render();
                assert.equal(typeof vnode, "object");
                assert.equal(vnode.name.name, "JazorBlazorDynamicComponent");
            });
            """,
            vueRuntimeSource: """
            export function defineComponent(options) { return options; }
            export function reactive(value) { return value; }
            export function ref(value) { return { value }; }
            export function onErrorCaptured() { return () => {}; }
            export function withCtx(slot) { return slot; }
            export function h(name, props, children) { return { name, props, children }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            """,
            supportingModules: new Dictionary<string, string>
            {
                ["components/child.mjs"] = "export default {};\n"
            }
        );
    }

    [TestMethod]
    public async Task EditFormAndInputText_KeepStandardBindingSurface()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/FormHost.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Forms

            <EditForm EditContext="@editContext">
                <InputText @bind-Value="Name" />
            </EditForm>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/form-host")]
            public partial class FormHost : ComponentBase, IVueComponent
            {
                private Microsoft.AspNetCore.Components.Forms.EditContext? editContext;
                private string Name { get; set; } = "initial";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.FormHost");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "blazor-components.mjs", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "InputText", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "EditForm", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("ValueExpression", StringComparison.Ordinal));

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/form-host.mjs",
            observation.ModuleText,
            "form-host.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/form-host.mjs";

            test("standard form adapters materialize as component VNodes", () => {
                const render = component.setup({}, { slots: {} });
                const vnode = render();
                assert.equal(vnode.name.name, "JazorBlazorEditForm");
                assert.equal(typeof vnode.children, "object");
            });
            """,
            vueRuntimeSource: """
            export function defineComponent(options) { return options; }
            export function reactive(value) { return value; }
            export function ref(value) { return { value }; }
            export function onErrorCaptured() { return () => {}; }
            export function withCtx(slot) { return slot; }
            export function h(name, props, children) { return { name, props, children }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            """
        );
    }

    [TestMethod]
    public async Task RouterRouteViewAndLayoutView_UseGeneratedRouteHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/AppRouter.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Routing

            <Router AppAssembly="@typeof(Program).Assembly">
                <Found Context="routeData">
                    <RouteView RouteData="@routeData" DefaultLayout="@typeof(Layout)" />
                </Found>
                <NotFound><p>Not found</p></NotFound>
            </Router>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            public sealed class Program;

            [ECMAScriptModule("./components/layout")]
            public sealed class Layout : ComponentBase, IVueComponent
            {
                [Parameter] public RenderFragment? ChildContent { get; set; }

                protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
                {
                    builder.AddContent(0, ChildContent);
                }
            }

            [ECMAScriptModule("./components/app-router")]
            public partial class AppRouter : ComponentBase, IVueComponent;
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.AppRouter",
            supportingSources: new Dictionary<string, string>
            {
                ["Layout.razor.cs"] = ""
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "blazor-routing.mjs", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "Router", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "RouteView", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "./layout.mjs", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/app-router.mjs",
            observation.ModuleText,
            "app-router.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/app-router.mjs";

            test("Router adapter is materialized without Vue Router authoring", () => {
                const render = component.setup({}, { slots: {} });
                const vnode = render();
                assert.equal(vnode.name.name, "JazorBlazorRouter");
            });
            """,
            supportingModules: new Dictionary<string, string>
            {
                ["@jazor/vue-runtime/routes.mjs"] = "export const routes = [];\n",
                ["components/layout.mjs"] = "export default {};\n"
            },
            vueRuntimeSource: """
            const provides = new Map();
            export const Fragment = Symbol("Fragment");
            export function defineComponent(options) { return options; }
            export function reactive(value) { return value; }
            export function provide(key, value) { provides.set(key, value); }
            export function inject(key, fallback) { return provides.has(key) ? provides.get(key) : fallback; }
            export function onUnmounted() {}
            export function h(name, props, children) { return { name, props, children }; }
            export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
            export function createCommentVNode(text) { return { name: "__comment", children: text }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            export function withCtx(slot) { return slot; }
            """
        );
    }

    [TestMethod]
    public async Task TypedInputs_EmitValueDescriptorsForNumberDateAndEnum()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TypedInputs.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Forms

            <InputNumber TValue="int" @bind-Value="Count" />
            <InputDate TValue="DateOnly?" @bind-Value="Day" />
            <InputSelect TValue="Choice" @bind-Value="Selected">
                <option value="0">None</option>
                <option value="1">One</option>
            </InputSelect>
            """,
            codeBehindSource:
            """
            using System;
            using Microsoft.AspNetCore.Components;
            using ECMAScript;

            namespace Demo.Pages;

            public enum Choice
            {
                None,
                One
            }

            [ECMAScriptModule("./components/typed-inputs")]
            public partial class TypedInputs : ComponentBase, IVueComponent
            {
                private int Count { get; set; }
                private DateOnly? Day { get; set; }
                private Choice Selected { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TypedInputs");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "__jazorValueType", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "kind: \"number\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "integer: true", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "kind: \"dateonly\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "kind: \"enum\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "None: 0", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "One: 1", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/typed-inputs.mjs",
            observation.ModuleText,
            "typed-inputs.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/typed-inputs.mjs";

            const find = (nodes, name) => nodes.find(node => node?.name?.name === name);

            test("closed InputBase<T> descriptors preserve numeric, date-only, and enum values", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render().children;
                const number = find(initial, "JazorBlazorInputNumber");
                const date = find(initial, "JazorBlazorInputDate");
                const select = find(initial, "JazorBlazorInputSelect");

                assert.equal(number.props.__jazorValueType.kind, "number");
                assert.equal(number.props.__jazorValueType.integer, true);
                assert.equal(date.props.__jazorValueType.kind, "dateonly");
                assert.equal(select.props.__jazorValueType.kind, "enum");

                number.name.setup(number.props, {})().props.onChange({ target: { value: "42" } });
                date.name.setup(date.props, {})().props.onChange({ target: { value: "2026-08-20" } });
                select.name.setup(select.props, { slots: select.children })().props.onChange({ target: { value: "1" } });

                const changed = render().children;
                assert.equal(find(changed, "JazorBlazorInputNumber").props.Value, 42);
                assert.deepEqual(find(changed, "JazorBlazorInputDate").props.Value, {
                    year: 2026, month: 8, day: 20, dayNumber: 739847
                });
                assert.equal(find(changed, "JazorBlazorInputSelect").props.Value, 1);

                number.name.setup(find(changed, "JazorBlazorInputNumber").props, {})().props.onChange({ target: { value: "42.5" } });
                assert.equal(find(render().children, "JazorBlazorInputNumber").props.Value, 42);
            });
            """,
            vueRuntimeSource:
            """
            export const Fragment = Symbol("Fragment");
            export function defineComponent(options) { return options; }
            export function reactive(value) { return value; }
            export function ref(value) { return { value }; }
            export function onErrorCaptured() { return () => {}; }
            export function withCtx(slot) { return slot; }
            export function h(name, props, children) { return { name, props, children }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
            export function createCommentVNode(text) { return { name: "__comment", children: text }; }
            """);
    }

    [TestMethod]
    public async Task ErrorBoundaryReference_ProjectsRecoverToTheBrowserAdapter()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ErrorBoundaryHost.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <ErrorBoundary @ref="boundary">
                <ChildContent>
                    <span>healthy</span>
                </ChildContent>
                <ErrorContent Context="exception">
                    <span>@exception.Message</span>
                </ErrorContent>
            </ErrorBoundary>
            <button @onclick="Recover">Recover</button>
            """,
            codeBehindSource:
            """
            using Microsoft.AspNetCore.Components;
            using Microsoft.AspNetCore.Components.Web;
            using ECMAScript;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/error-boundary-host")]
            public partial class ErrorBoundaryHost : ComponentBase, IVueComponent
            {
                private ErrorBoundary? boundary;

                private void Recover()
                {
                    boundary!.Recover();
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ErrorBoundaryHost");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "blazor-components.mjs", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "state.boundary.Recover()", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/error-boundary-host.mjs",
            observation.ModuleText,
            "error-boundary-host.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/error-boundary-host.mjs";
            import { __capturedErrorHandler } from "vue";

            const find = (nodes, name) => nodes.find(node => node?.name?.name === name || node?.name === name);
            const textOf = value => {
                if (Array.isArray(value)) return value.map(textOf).join("");
                if (value && typeof value === "object" && "children" in value) return textOf(value.children);
                return value ?? "";
            };

            test("ErrorBoundary captures an error and an authored @ref Recover call restores child content", () => {
                const render = component.setup({}, { slots: {} });
                const nodes = render().children;
                const boundary = find(nodes, "JazorBlazorErrorBoundary");
                const button = find(nodes, "button");
                let exposed;
                const boundaryRender = boundary.name.setup(boundary.props, {
                    slots: boundary.children,
                    expose(value) { exposed = value; }
                });

                assert.ok(boundaryRender());
                boundary.props.ref(exposed);
                __capturedErrorHandler()(new Error("broken"));
                assert.equal(textOf(boundaryRender()), "broken");

                button.props.onClick();
                assert.ok(boundaryRender());
                assert.equal(exposed.CurrentException, null);
                assert.equal(textOf(boundaryRender()).replace(/<[^>]*>/g, ""), "healthy");
            });
            """,
            vueRuntimeSource:
            """
            let captured;
            export const Fragment = Symbol("Fragment");
            export function defineComponent(options) { return options; }
            export function reactive(value) { return value; }
            export function ref(value) { return { value }; }
            export function onErrorCaptured(callback) { captured = callback; }
            export function __capturedErrorHandler() { return captured; }
            export function withCtx(slot) { return slot; }
            export function h(name, props, children) { return { name, props, children }; }
            export function openBlock() { return null; }
            export function createElementBlock(name, props, children) { return { name, props, children }; }
            export function createBlock(name, props, children) { return { name, props, children }; }
            export function createStaticVNode(html, count) { return { name: "__static", props: { html, count }, children: html }; }
            export function createCommentVNode(text) { return { name: "__comment", children: text }; }
            export function createTextVNode(text) { return { name: "__text", children: text }; }
            """);
    }
}
