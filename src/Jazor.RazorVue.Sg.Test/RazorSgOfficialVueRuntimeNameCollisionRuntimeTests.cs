namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueRuntimeNameCollisionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorMemberNamedH_UsesStableAliasInsteadOfVueRuntimeImportOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\RuntimeNameCollision.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <button @onclick="h">@Status</button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/runtime-name-collision")]
            public partial class RuntimeNameCollision : ComponentBase, IVueComponent
            {
                private string Status { get; set; } = "idle";

                private void h()
                {
                    Status = "queued";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.RuntimeNameCollision");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "defineComponent", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "h", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "function m$", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onClick: m$", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("function h()", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/runtime-name-collision.mjs",
            observation.ModuleText,
            "official-runtime-name-collision.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/runtime-name-collision.mjs";

            test("official Razor member names do not shadow Vue render imports", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "button");
                assert.equal(initial.children, "idle");
                assert.equal(typeof initial.props.onClick, "function");

                initial.props.onClick();

                assert.equal(render().children, "queued");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorNestedRuntimeClassNamedHAndInitState_PreserveVueRenderImportOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\NestedRuntimeNameCollision.razor",
            documentText:
            """
            <section>@Message</section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/nested-runtime-name-collision")]
            public partial class NestedRuntimeNameCollision : ComponentBase, IVueComponent
            {
                private string Message { get; init; } = new h().Read();

                private sealed class h
                {
                    public string Read() => "nested";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.NestedRuntimeNameCollision");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "defineComponent", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "h", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "class m$", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "new m$", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("class h ", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/nested-runtime-name-collision.mjs",
            observation.ModuleText,
            "official-nested-runtime-name-collision.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/nested-runtime-name-collision.mjs";

            test("nested runtime types do not shadow Vue render imports", () => {
                const section = component.setup({}, { slots: {} })();
                assert.equal(section.name, "section");
                assert.equal(section.children, "nested");
            });
            """);
    }
}
