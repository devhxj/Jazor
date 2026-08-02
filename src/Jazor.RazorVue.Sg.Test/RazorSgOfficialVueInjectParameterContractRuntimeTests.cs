namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueInjectParameterContractRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorVueInject_PreservesRequiredAndUnmatchedParameterContract()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\InjectedShellParameterContractRuntime.razor",
            documentText:
            """
            @using Demo.Components

            <ContractShell Title="Account" data-region="release">
                <strong data-slot="content">Deploy</strong>
            </ContractShell>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/injected-shell-parameter-contract-parent-runtime")]
                public partial class InjectedShellParameterContractRuntime : ComponentBase, IVueComponent
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.InjectedShellParameterContractRuntime",
            supportingSources: new Dictionary<string, string>
            {
                [@"D:\repo\Demo\Components\ContainerParameterContractComponents.cs"] =
                """
                using System.Collections.Generic;
                using ECMAScript.VueContract;
                using ECMAScript.VueContract.Descriptor;

                [assembly: VueInject(typeof(Demo.Components.ContractShell), typeof(Demo.Components.InjectedShell))]

                namespace Demo.Components
                {
                    [ECMAScriptModule("./components/contract-shell-parameter-contract-runtime")]
                    public partial class ContractShell : ComponentBase, IVueComponent, IVueContainerComponent
                    {
                        [Parameter, EditorRequired]
                        public string? Title { get; set; }

                        [Parameter(CaptureUnmatchedValues = true)]
                        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

                        [Parameter]
                        public RenderFragment? ChildContent { get; set; }
                    }

                    [ECMAScriptModule("./components/injected-shell-parameter-contract-runtime")]
                    [VueProp(nameof(Title), Name = "injectedTitle")]
                    [VueSlot(nameof(ChildContent), Name = "injected-content")]
                    public partial class InjectedShell : ComponentBase, IVueComponent, IVueContainerImplementation<ContractShell>
                    {
                        [Parameter, EditorRequired]
                        public string? Title { get; set; }

                        [Parameter(CaptureUnmatchedValues = true)]
                        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

                        [Parameter]
                        public RenderFragment? ChildContent { get; set; }
                    }
                }
                """
            });

        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentParameter(2, \"data-region\", \"release\")", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "from \"./injected-shell-parameter-contract-runtime.mjs\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "injectedTitle: \"Account\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "\"data-region\": \"release\"", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/injected-shell-parameter-contract-parent-runtime.mjs",
            observation.ModuleText,
            "official-vue-inject-parameter-contract-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import parent from "./components/injected-shell-parameter-contract-parent-runtime.mjs";
            import injectedShell from "./components/injected-shell-parameter-contract-runtime.mjs";

            test("official Razor VueInject preserves required and unmatched parameters", () => {
                const vnode = parent.setup({}, { slots: {} })();
                assert.equal(vnode.name, injectedShell);
                assert.equal(vnode.props.injectedTitle, "Account");
                assert.equal(vnode.props["data-region"], "release");
                assert.equal(typeof vnode.children["injected-content"], "function");

                const content = vnode.children["injected-content"]();
                assert.equal(content.length, 1);
                assert.equal(content[0].name, "__static");
                assert.equal(content[0].props.html, "<strong data-slot=\"content\">Deploy</strong>");
            });
            """,
            new Dictionary<string, string>
            {
                ["components/injected-shell-parameter-contract-runtime.mjs"] = "export default { name: \"injected-shell-parameter-contract-runtime\" };"
            });
    }
}
