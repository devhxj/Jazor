using ECMAScript.ElementPlus;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialElementPlusNumericUnionTests
{
    [TestMethod]
    public async Task OfficialAuthoring_NumericAndUnionMatrix_PreservesBranchIdentityAndClearValues()
    {
        // The guide links these authored inputs; compile the checked-in example rather than a separate copy.
        var documentPath = RazorSgTestHost.GetTestDocumentPath("Pages/NumericUnionAuthoring.razor");
        var samplePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(documentPath)!,
            "../../../../samples/RazorVue.NumericUnion/NumericUnionAuthoring.razor"));
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath, await File.ReadAllTextAsync(samplePath), await File.ReadAllTextAsync(samplePath + ".cs"),
            "Demo.Pages", "Demo.Pages.NumericUnionAuthoring");
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        Assert.AreEqual(typeof(ECMAScript.Vue.VueStringNumberValue?), typeof(ElAvatar).GetProperty("Size")!.PropertyType);
        Assert.AreEqual(typeof(ECMAScript.Number?), typeof(ElPagination).GetProperty("CurrentPage")!.PropertyType);
        Assert.AreEqual(typeof(string), typeof(ElTypedSelect<string>).GetProperty("ModelValue")!.PropertyType);

        var imports = new Dictionary<string, string>();
        foreach (var path in new[] { "avatar", "pagination", "select", "option" })
        {
            imports[$"element-plus/es/components/{path}/index.mjs"] = $"npm:element-plus@2.14.5/es/components/{path}/index.mjs";
            imports[$"element-plus/es/components/{path}/style/css.mjs"] = "./fixtures/empty-style.mjs";
        }
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/numeric-union-authoring.js", observation.ModuleText, "numeric-union-authoring.test.mjs",
            """
            import assert from "node:assert/strict";
            import { Window } from "npm:happy-dom@20.10.6";
            const window = new Window({ url: "http://localhost/" });
            for (const name of ["document", "Node", "Element", "HTMLElement", "SVGElement", "HTMLInputElement", "Event", "MouseEvent", "KeyboardEvent", "MutationObserver", "ResizeObserver", "DOMException", "navigator"])
                globalThis[name] = window[name];
            globalThis.window = window;
            globalThis.getComputedStyle = window.getComputedStyle.bind(window);
            globalThis.requestAnimationFrame = window.requestAnimationFrame.bind(window);
            globalThis.cancelAnimationFrame = window.cancelAnimationFrame.bind(window);
            const { createApp, nextTick } = await import("vue");
            const { default: component } = await import("./components/numeric-union-authoring.js");
            const { ElAvatar } = await import("element-plus/es/components/avatar/index.mjs");
            const settle = async () => { await nextTick(); await new Promise(resolve => setTimeout(resolve, 40)); await nextTick(); };

            Deno.test("N4 numeric props and nested unions keep exact values through actual Element Plus", async () => {
                // Inspect final Vue props before mounting: string "32" must stay distinguishable from numeric 32.
                const render = component.setup({}, { slots: {} });
                const avatars = render().children.filter(node => node.type === ElAvatar);
                assert.equal(avatars[0].props.size, 32);
                assert.equal(avatars[1].props.size, "32");
                assert.equal(avatars[2].props.size, "small");
                assert.equal(avatars[3].props?.size, undefined);

                const host = document.createElement("main");
                document.body.append(host);
                const app = createApp(component);
                app.mount(host);
                await settle();
                const state = name => host.querySelector('[data-state="true"]').getAttribute(`data-${name}`);
                assert.equal(host.querySelector('[data-size="numeric"]').style.getPropertyValue("--el-avatar-size"), "32px");
                assert.equal(host.querySelector('[data-size="text"]').classList.contains("el-avatar--32"), true);
                assert.equal(host.querySelector('[data-size="small"]').classList.contains("el-avatar--small"), true);
                assert.equal(state("number"), "32");
                assert.equal(state("string"), null);
                assert.equal(state("count"), "1");
                [...host.querySelectorAll(".pagination .el-pager li.number")].find(node => node.textContent === "3").click();
                await settle();
                assert.equal(state("page"), "3");

                const choose = async (section, label) => {
                    host.querySelector(`${section} .el-select__wrapper`).click();
                    await settle();
                    [...host.querySelectorAll(`${section} .el-select-dropdown__item`)].find(node => node.textContent.trim() === label).click();
                    await settle();
                };
                await choose(".single", "Text");
                assert.equal(state("string"), "32");
                assert.equal(state("number"), null, "numeric strings must not become numeric branches");
                await choose(".single", "Zero");
                assert.equal(state("number"), "0", "zero must not be treated as an absent number");
                assert.equal(state("string"), null);
                await choose(".multiple", "Text");
                assert.equal(state("count"), "2");

                const clear = async section => {
                    host.querySelector(`${section} .el-select`).dispatchEvent(new MouseEvent("mouseenter"));
                    await settle();
                    const icon = host.querySelector(`${section} .el-select__clear`);
                    assert.ok(icon, `${section} should expose its clear control`);
                    icon.click();
                    await settle();
                };
                await clear(".single");
                assert.equal(state("number"), null);
                assert.equal(state("string"), null);
                await clear(".multiple");
                assert.equal(state("count"), "0");
                assert.equal(state("id"), "9007199254740993");
                await clear(".typed");
                assert.equal(state("id"), "", "ValueOnClear supplies the authored empty-string policy");
                await choose(".typed", "Exact string ID");
                assert.equal(state("id"), "9007199254740993", "typed IDs must preserve all digits");
                app.unmount();
                await nextTick();
                window.happyDOM.abort();
            });
            """, new Dictionary<string, string> { ["fixtures/empty-style.mjs"] = "" },
            vueModuleSpecifier: "npm:vue@3.5.42", restoreNpmDependencies: true, importSpecifiers: imports);
    }
}
