namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueInjectRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorVueInject_UsesImplementationMemberNamesAndSlots()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InjectedShellRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <ContractShell Title="Account">
                <strong data-slot="content">Deploy</strong>
            </ContractShell>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/injected-parent-runtime")]
                public partial class InjectedShellRuntime : ComponentBase, IVueComponent
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InjectedShellRuntime",
            supportingSources: new Dictionary<string, string>
            {
                [@"D:\repo\Demo\Components\ContainerComponents.cs"] =
                """
                using ECMAScript.VueContract;

                [assembly: VueInject(typeof(Demo.Components.ContractShell), typeof(Demo.Components.InjectedShell))]

                namespace Demo.Components
                {
                    [ECMAScriptModule("./components/contract-shell-runtime")]
                    public partial class ContractShell : ComponentBase, IVueComponent, IVueContainerComponent
                    {
                        [Parameter]
                        public string? Title { get; set; }

                        [Parameter]
                        public RenderFragment? ChildContent { get; set; }
                    }

                    [ECMAScriptModule("./components/injected-shell-runtime")]
                    public partial class InjectedShell : ComponentBase, IVueComponent, IVueContainerImplementation<ContractShell>
                    {
                        [Parameter]
                        [ECMAScriptName("injectedTitle")]
                        public string? Title { get; set; }

                        [Parameter]
                        [ECMAScriptName("injected-content")]
                        public RenderFragment? ChildContent { get; set; }
                    }
                }
                """
            });

        StringAssert.Contains(observation.GeneratedCSharp, "OpenComponent<global::Demo.Components.ContractShell>", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "from \"./injected-shell-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "injectedTitle: \"Account\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "\"injected-content\": () =>", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("contract-shell-runtime", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/injected-parent-runtime.mjs",
            observation.ModuleText,
            "official-vue-inject-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import parent from "./components/injected-parent-runtime.mjs";
            import injectedShell from "./components/injected-shell-runtime.mjs";

            test("official Razor VueInject resolves the implementation component and its member names", () => {
                const vnode = parent.setup({}, { slots: {} })();
                assert.equal(vnode.name, injectedShell);
                assert.equal(vnode.props.injectedTitle, "Account");
                assert.equal(typeof vnode.children["injected-content"], "function");

                const content = vnode.children["injected-content"]();
                assert.equal(content.length, 1);
                assert.equal(content[0].name, "__static");
                assert.equal(content[0].props.html, "<strong data-slot=\"content\">Deploy</strong>");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/injected-shell-runtime.mjs"] = "export default { name: \"injected-shell-runtime\" };"
            });
    }
}
