using ECMAScript.ElementPlus;
using Microsoft.AspNetCore.Components;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialElementPlusPaginationDateTests
{
    [TestMethod]
    public async Task OfficialAuthoring_PaginationAndStringDatesPreserveNativePayloads()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/PaginationDate.razor"),
            """
            @using ECMAScript.ElementPlus
            <div class="first">
                <ElPagination CurrentPage="@Page" PageSize="@Size" Total="@(100)" PageSizes="@Sizes"
                              Layout="sizes, prev, pager, next" Teleported="false"
                              OnCurrentChange="@PageChanged" OnSizeChange="@SizeChanged" />
            </div>
            <div class="second">
                <ElPagination CurrentPage="@OtherPage" PageSize="@(10)" Total="@(100)"
                              OnCurrentChange="@OtherPageChanged" />
            </div>
            <div class="date">
                <ElStringDatePicker @bind-ModelValue="Date" Type="date" Clearable="true" Teleported="false"
                                    Format="YYYY-MM-DD" ValueFormat="YYYY-MM-DD" />
            </div>
            <div class="datetime">
                <ElStringDatePicker @bind-ModelValue="Time" Type="datetime" Clearable="true" Teleported="false"
                                    Format="YYYY-MM-DD HH:mm:ss" ValueFormat="YYYY-MM-DD HH:mm:ss" />
            </div>
            <span data-state="true" data-page="@Page" data-size="@Size" data-other="@OtherPage"
                  data-date="@(Date ?? "empty")" data-time="@(Time ?? "empty")"></span>
            """,
            """
            namespace Demo.Pages;
            [ECMAScriptModule("./components/pagination-date")]
            public partial class PaginationDate : ComponentBase, IVueComponent
            {
                private Number Page { get; set; } = 1;
                private Number Size { get; set; } = 10;
                private Number OtherPage { get; set; } = 1;
                private Number[] Sizes { get; } = [10, 20];
                private string? Date { get; set; } = "2026-10-09";
                private string? Time { get; set; } = "2026-10-09 21:30:45";
                private void PageChanged(Number value) => Page = value;
                private void SizeChanged(Number value) { Size = value; Page = 1; }
                private void OtherPageChanged(Number value) => OtherPage = value;
            }
            """, "Demo.Pages", "Demo.Pages.PaginationDate");

        Assert.AreEqual(typeof(EventCallback<ECMAScript.Number>), typeof(ElPagination).GetProperty("OnCurrentChange")!.PropertyType);
        Assert.AreEqual(typeof(EventCallback<ECMAScript.Number>), typeof(ElPagination).GetProperty("OnSizeChange")!.PropertyType);
        Assert.AreEqual(typeof(EventCallback<string>), typeof(ElStringDatePicker).GetProperty("ModelValueChanged")!.PropertyType);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        var modules = new Dictionary<string, string> { ["fixtures/empty-style.mjs"] = "" };
        var imports = new Dictionary<string, string>();
        foreach (var path in new[] { "pagination", "date-picker" })
        {
            imports[$"element-plus/es/components/{path}/index.mjs"] = $"npm:element-plus@2.14.5/es/components/{path}/index.mjs";
            imports[$"element-plus/es/components/{path}/style/css.mjs"] = "./fixtures/empty-style.mjs";
        }
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/pagination-date.js", observation.ModuleText, "pagination-date.test.mjs",
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
            const { default: component } = await import("./components/pagination-date.js");
            const settle = async () => { await nextTick(); await new Promise(resolve => setTimeout(resolve, 40)); await nextTick(); };

            Deno.test("actual pagination numbers and formatted strings reach their authored fields", async () => {
                const host = document.createElement("div");
                document.body.append(host);
                const app = createApp(component);
                app.mount(host);
                await settle();
                const state = name => host.querySelector('[data-state="true"]').getAttribute(`data-${name}`);
                const clickPage = (section, page) => [...host.querySelectorAll(`${section} .el-pager li.number`)]
                    .find(node => node.textContent === String(page)).click();
                clickPage(".first", 2);
                await settle();
                assert.equal(state("page"), "2");
                assert.equal(state("other"), "1");
                clickPage(".second", 3);
                await settle();
                assert.equal(state("other"), "3");
                assert.equal(state("page"), "2");
                host.querySelector(".first .el-select__wrapper").click();
                await settle();
                [...host.querySelectorAll(".first .el-select-dropdown__item")].find(node => node.textContent.includes("20")).click();
                await settle();
                assert.equal(state("size"), "20");
                assert.equal(state("page"), "1");
                assert.equal(state("other"), "3");

                for (const [section, field, value] of [[".date", "date", "2026-11-12"], [".datetime", "time", "2026-11-12 08:09:10"]]) {
                    const input = host.querySelector(`${section} input`);
                    input.value = value;
                    input.dispatchEvent(new Event("input", { bubbles: true }));
                    input.dispatchEvent(new Event("change", { bubbles: true }));
                    await settle();
                    assert.equal(state(field), value);
                    assert.equal(input.value, value);
                    host.querySelector(`${section} .el-input`).dispatchEvent(new MouseEvent("mouseenter"));
                    await settle();
                    host.querySelector(`${section} .clear-icon`).click();
                    await settle();
                    assert.equal(state(field), "empty");
                    assert.equal(input.value, "");
                }
                app.unmount();
                await nextTick();
                window.happyDOM.abort();
            });
            """, modules, vueModuleSpecifier: "npm:vue@3.5.42", restoreNpmDependencies: true, importSpecifiers: imports);
    }
}
