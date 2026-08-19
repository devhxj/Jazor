namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLoopMemberNameCollisionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLoopLocalAndMethodWithSameJavaScriptName_PreservesBothBindingsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseRefreshQueue.razor"),
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <section data-last="@LastRefresh">
                @foreach (var refresh in Releases)
                {
                    <button data-release="@refresh" @onclick="() => Refresh(refresh)">@refresh</button>
                }
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-refresh-queue-runtime")]
            public partial class ReleaseRefreshQueue : ComponentBase, IVueComponent
            {
                [Parameter] public string[] Releases { get; set; } = [];

                private string LastRefresh { get; set; } = "none";

                private void Refresh(string release)
                {
                    LastRefresh = release;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseRefreshQueue");

        StringAssert.Contains(observation.GeneratedCSharp, "foreach", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "Array.from(props.Releases ?? []", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "data-last", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-refresh-queue-runtime.mjs",
            observation.ModuleText,
            "official-release-refresh-queue-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-refresh-queue-runtime.mjs";

            function collect(node, name) {
                if (node == null) return [];
                if (Array.isArray(node)) return node.flatMap(item => collect(item, name));
                const children = collect(node.children, name);
                return node.name === name ? [node, ...children] : children;
            }

            test("official Razor loop locals do not shadow lowered component methods", async () => {
                const render = component.setup({ Releases: ["Accounts API", "Billing API"] }, { slots: {} });
                const initial = render();
                assert.equal(initial.name, "section");
                assert.equal(initial.props["data-last"], "none");

                const buttons = collect(initial, "button");
                assert.equal(buttons.length, 2);
                assert.equal(buttons[0].props["data-release"], "Accounts API");
                assert.equal(buttons[1].props["data-release"], "Billing API");
                assert.equal(buttons[0].children, "Accounts API");
                assert.equal(buttons[1].children, "Billing API");

                await Promise.resolve(buttons[1].props.onClick());

                assert.equal(render().props["data-last"], "Billing API");
            });
            """);
    }
}
