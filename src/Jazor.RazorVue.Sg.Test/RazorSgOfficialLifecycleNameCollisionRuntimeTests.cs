namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialLifecycleNameCollisionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_LifecycleMethodRunsWhenRazorLocalUsesItsRuntimeName()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/LifecycleNameCollision.razor"),
            documentText:
            """
            @{
                var onInitialized = "local";
            }
            <p data-local="@onInitialized">@Log</p>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/lifecycle-name-collision")]
            public partial class LifecycleNameCollision : ComponentBase, IVueComponent
            {
                private string log = "";

                private string Log => log;

                protected override void OnInitialized()
                {
                    log = "initialized";
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LifecycleNameCollision");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/lifecycle-name-collision.mjs",
            observation.ModuleText,
            "official-lifecycle-name-collision.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/lifecycle-name-collision.mjs";

            test("official Razor lifecycle uses its declared runtime alias", () => {
                const vnode = component.setup({}, { slots: {} })();

                assert.equal(vnode.props["data-local"], "local");
                assert.equal(vnode.children, "initialized");
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_AllLifecycleMethodsRunWhenRazorLocalsUseTheirRuntimeNames()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/LifecycleNames.razor"),
            documentText:
            """
            @{
                var onInitialized = "initialized-local";
                var onInitializedAsync = "initialized-async-local";
                var onParametersSet = "parameters-local";
                var onParametersSetAsync = "parameters-async-local";
                var onAfterRender = "after-render-local";
                var onAfterRenderAsync = "after-render-async-local";
                var shouldRender = "should-render-local";
            }
            <p data-initialized="@onInitialized"
               data-initialized-async="@onInitializedAsync"
               data-parameters="@onParametersSet"
               data-parameters-async="@onParametersSetAsync"
               data-after-render="@onAfterRender"
               data-after-render-async="@onAfterRenderAsync"
               data-should-render="@shouldRender">@Log</p>
            """,
            codeBehindSource:
            """
            using System.Threading.Tasks;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/lifecycle-names")]
            public partial class LifecycleNames : ComponentBase, IVueComponent
            {
                private string log = "";

                private string Log => log;

                protected override void OnInitialized()
                {
                    log += "init|";
                }

                protected override Task OnInitializedAsync()
                {
                    log += "initAsync|";
                    return Task.CompletedTask;
                }

                protected override void OnParametersSet()
                {
                    log += "params|";
                }

                protected override Task OnParametersSetAsync()
                {
                    log += "paramsAsync|";
                    return Task.CompletedTask;
                }

                protected override void OnAfterRender(bool firstRender)
                {
                    log += firstRender ? "after:first|" : "after:update|";
                }

                protected override Task OnAfterRenderAsync(bool firstRender)
                {
                    log += firstRender ? "afterAsync:first|" : "afterAsync:update|";
                    return Task.CompletedTask;
                }

                protected override bool ShouldRender()
                {
                    log += "should|";
                    return true;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LifecycleNames");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/lifecycle-names.mjs",
            observation.ModuleText,
            "official-lifecycle-names.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { __runMounted, __runUpdated } from "vue";

            import component from "./components/lifecycle-names.mjs";

            test("official Razor lifecycle aliases remain semantic", async () => {
                const render = component.setup({}, { slots: {} });
                const first = render();

                assert.equal(first.props["data-initialized"], "initialized-local");
                assert.equal(first.props["data-initialized-async"], "initialized-async-local");
                assert.equal(first.props["data-parameters"], "parameters-local");
                assert.equal(first.props["data-parameters-async"], "parameters-async-local");
                assert.equal(first.props["data-after-render"], "after-render-local");
                assert.equal(first.props["data-after-render-async"], "after-render-async-local");
                assert.equal(first.props["data-should-render"], "should-render-local");

                await Promise.resolve();
                await Promise.resolve();
                await new Promise(resolve => setTimeout(resolve, 0));
                const afterParameters = render();
                assert.equal(afterParameters.children, "init|initAsync|params|paramsAsync|should|");

                __runMounted();
                await Promise.resolve();
                await new Promise(resolve => setTimeout(resolve, 0));
                const afterMounted = render();
                assert.equal(afterMounted.children, "init|initAsync|params|paramsAsync|should|after:first|afterAsync:first|should|");

                __runUpdated();
                await Promise.resolve();
                await new Promise(resolve => setTimeout(resolve, 0));
                const afterUpdated = render();
                assert.equal(afterUpdated.children, "init|initAsync|params|paramsAsync|should|after:first|afterAsync:first|should|after:update|afterAsync:update|should|");
            });
            """);
    }
}
