namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialReferenceAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorElementReferenceFocus_UsesDomCarrierMapping()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReferenceFocus.razor"),
            documentText:
            """
            <input @ref="inputElement" />
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/reference-focus")]
            public partial class ReferenceFocus : ComponentBase, IVueComponent
            {
                private ElementReference inputElement;

                protected override async Task OnAfterRenderAsync(bool firstRender)
                {
                    if (!firstRender)
                        return;

                    await inputElement.FocusAsync();
                    await inputElement.FocusAsync(true);
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReferenceFocus");

        StringAssert.Contains(
            observation.ModuleText,
            "from \"Microsoft/AspNetCore/Components/ElementReferenceExtensionsModule.js\"",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "focusAsync(state.inputElement)", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "focusAsyncWithOptions(state.inputElement, true)",
            StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorElementReferenceFocus_PreservesMountAndUnmountFailureContractOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReferenceFocusRuntime.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components
            @using Microsoft.AspNetCore.Components.Web

            <button id="focus" @onclick="Focus">Focus</button>
            <button id="toggle" @onclick="Toggle">Toggle</button>
            @if (Visible)
            {
                <input id="focus-target" @ref="inputElement" />
            }
            <span id="status">@Status</span>
            """,
            codeBehindSource:
            """
            using System;
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/reference-focus-runtime")]
            public partial class ReferenceFocusRuntime : ComponentBase, IVueComponent
            {
                private ElementReference inputElement;
                private bool Visible { get; set; } = true;
                private string Status { get; set; } = "ready";

                private async Task Focus()
                {
                    try
                    {
                        await inputElement.FocusAsync();
                        await inputElement.FocusAsync(true);
                        Status = "focused";
                    }
                    catch (Exception error)
                    {
                        Status = error.Message;
                    }
                }

                private void Toggle()
                {
                    Visible = !Visible;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReferenceFocusRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "ElementReferenceExtensionsModule.js",
            StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/reference-focus-runtime.mjs",
            observation.ModuleText,
            "official-reference-focus-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/reference-focus-runtime.mjs";

            function findNode(node, predicate) {
                if (Array.isArray(node)) {
                    for (const child of node) {
                        const found = findNode(child, predicate);
                        if (found) return found;
                    }
                    return null;
                }

                if (!node || typeof node !== "object") return null;
                if (predicate(node)) return node;
                return findNode(node.children, predicate);
            }

            function text(node) {
                if (Array.isArray(node)) return node.map(text).join("");
                if (!node || typeof node !== "object") return node == null ? "" : String(node);
                return text(node.children);
            }

            test("official Razor @ref focus uses the mounted element and preserves the unmounted failure", async () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const focusButton = findNode(initial, node => node.name === "button" && node.props?.id === "focus");
                const toggleButton = findNode(initial, node => node.name === "button" && node.props?.id === "toggle");
                const input = findNode(initial, node => node.name === "input" && node.props?.id === "focus-target");
                assert.ok(focusButton);
                assert.ok(toggleButton);
                assert.ok(input);

                await focusButton.props.onClick();
                const emptyStatus = findNode(render(), node => node.name === "span" && node.props?.id === "status");
                assert.ok(emptyStatus);
                assert.equal(text(emptyStatus), "InvalidOperationException: ElementReference has not been configured correctly.");

                const focusCalls = [];
                input.props.ref({
                    focus(...args) {
                        focusCalls.push(args);
                    }
                });

                await focusButton.props.onClick();
                const focusedStatus = findNode(render(), node => node.name === "span" && node.props?.id === "status");
                assert.ok(focusedStatus);
                assert.equal(text(focusedStatus), "focused");
                assert.deepEqual(focusCalls, [[], [{ preventScroll: true }]]);

                toggleButton.props.onClick();
                input.props.ref(null);
                const unmounted = render();
                assert.equal(findNode(unmounted, node => node.name === "input" && node.props?.id === "focus-target"), null);

                const unmountedFocusButton = findNode(unmounted, node => node.name === "button" && node.props?.id === "focus");
                assert.ok(unmountedFocusButton);
                await unmountedFocusButton.props.onClick();
                const unmountedStatus = findNode(render(), node => node.name === "span" && node.props?.id === "status");
                assert.ok(unmountedStatus);
                assert.equal(text(unmountedStatus), "InvalidOperationException: ElementReference has not been configured correctly.");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorReferences_EmitElementAndComponentRefCallbacksInSourceOrder()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/References.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components

            <input @ref="inputElement" data-role="primary" />
            <ReferenceChild @ref="child" Status="ready" />
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/references")]
            public partial class References : ComponentBase, IVueComponent
            {
                private ElementReference inputElement;
                private ReferenceChild? child { get; set; }
            }

            [ECMAScriptModule("./components/reference-child")]
            public partial class ReferenceChild : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Status { get; set; } = "";

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.References");

        StringAssert.Contains(observation.GeneratedCSharp, "AddElementReferenceCapture", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentReferenceCapture", StringComparison.Ordinal);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "from \"./reference-child.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "inputElement: null", StringComparison.Ordinal);
        StringAssert.Contains(script, "child: null", StringComparison.Ordinal);
        StringAssert.Contains(script, "ref:", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.inputElement = __value", StringComparison.Ordinal);
        StringAssert.Contains(script, "state.child = __value", StringComparison.Ordinal);

        var elementCapture = script.IndexOf("state.inputElement = __value", StringComparison.Ordinal);
        var componentCapture = script.IndexOf("state.child = __value", StringComparison.Ordinal);
        Assert.IsTrue(elementCapture < componentCapture, script);

        Assert.IsFalse(script.Contains("AddElementReferenceCapture", StringComparison.Ordinal), script);
        Assert.IsFalse(script.Contains("AddComponentReferenceCapture", StringComparison.Ordinal), script);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(script);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorComponentReference_TracksMountAndUnmountOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReferenceRuntime.razor"),
            documentText:
            """
            <div>
                <span>@(child is null ? "waiting" : child.Status)</span>
                <ReferenceChild @ref="child" />
            </div>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/reference-runtime")]
            public partial class ReferenceRuntime : ComponentBase, IVueComponent
            {
                private ReferenceChild? child;
            }

            [ECMAScriptModule("./components/reference-child")]
            public partial class ReferenceChild : ComponentBase, IVueComponent
            {
                public string Status { get; set; } = string.Empty;

                protected override void BuildRenderTree(RenderTreeBuilder builder)
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReferenceRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentReferenceCapture", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/reference-runtime.mjs",
            observation.ModuleText,
            "official-reference-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/reference-runtime.mjs";

            test("official Razor component references follow Vue mount and unmount callbacks", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const initialStatus = initial.children.find(node => node.name === "span");
                const initialChild = initial.children.find(node => node.name?.name === "reference-child");
                assert.equal(initial.name, "div");
                assert.ok(initialStatus);
                assert.ok(initialChild);
                assert.equal(initialStatus.name, "span");
                assert.equal(initialStatus.children, "waiting");
                assert.equal(initialChild.name.name, "reference-child");

                initialChild.props.ref({ Status: "attached" });

                const attached = render();
                const attachedStatus = attached.children.find(node => node.name === "span");
                const attachedChild = attached.children.find(node => node.name?.name === "reference-child");
                assert.ok(attachedStatus);
                assert.ok(attachedChild);
                assert.equal(attachedStatus.children, "attached");

                attachedChild.props.ref(null);

                const detached = render();
                const detachedStatus = detached.children.find(node => node.name === "span");
                assert.ok(detachedStatus);
                assert.equal(detachedStatus.children, "waiting");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/reference-child.mjs"] = "export default { name: \"reference-child\" };"
            });
    }
}
