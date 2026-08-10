namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialReferenceAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorReferences_EmitElementAndComponentRefCallbacksInSourceOrder()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\References.razor",
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
            documentPath: @"D:\repo\Demo\Pages\ReferenceRuntime.razor",
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
                assert.deepEqual(initialStatus.children, ["waiting"]);
                assert.equal(initialChild.name.name, "reference-child");

                initialChild.props.ref({ Status: "attached" });

                const attached = render();
                const attachedStatus = attached.children.find(node => node.name === "span");
                const attachedChild = attached.children.find(node => node.name?.name === "reference-child");
                assert.ok(attachedStatus);
                assert.ok(attachedChild);
                assert.deepEqual(attachedStatus.children, ["attached"]);

                attachedChild.props.ref(null);

                const detached = render();
                const detachedStatus = detached.children.find(node => node.name === "span");
                assert.ok(detachedStatus);
                assert.deepEqual(detachedStatus.children, ["waiting"]);
            });
            """,
            new Dictionary<string, string>
            {
                ["components/reference-child.mjs"] = "export default { name: \"reference-child\" };"
            });
    }
}
