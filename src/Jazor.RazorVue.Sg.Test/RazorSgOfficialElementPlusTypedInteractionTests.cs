using ECMAScript.ElementPlus;
using Microsoft.AspNetCore.Components;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialElementPlusTypedInteractionTests
{
    [TestMethod]
    public void TypedContracts_PreserveRowsModelsCommandsAndServiceReturnTypes()
    {
        Assert.AreEqual(typeof(string), typeof(ElTypedSelect<string>).GetProperty("ModelValue")!.PropertyType);
        Assert.AreEqual(typeof(EventCallback<string>), typeof(ElTypedSelect<string>).GetProperty("ModelValueChanged")!.PropertyType);
        Assert.AreEqual(typeof(ElTableColumnSelectableCallback<Row>), typeof(ElTypedTableColumn<Row>).GetProperty("Selectable")!.PropertyType);
        Assert.AreEqual(typeof(RenderFragment<ElTableSlotContext<Row>>), typeof(ElTypedTableColumn<Row>).GetProperty("ChildContent")!.PropertyType);
        Assert.AreEqual(typeof(EventCallback<Row[]>), typeof(ElTypedTable<Row>).GetProperty("OnSelectionChange")!.PropertyType);
        Assert.AreEqual(typeof(EventCallback<Row>), typeof(ElTypedTable<Row>).GetProperty("OnRowClick")!.PropertyType);
        Assert.AreEqual(typeof(EventCallback<ElTableSort>), typeof(ElTypedTable<Row>).GetProperty("OnSortChange")!.PropertyType);
        Assert.AreEqual(typeof(EventCallback<string>), typeof(ElTypedDropdown<string>).GetProperty("OnCommand")!.PropertyType);
        Assert.AreEqual(typeof(ElMessageHandle), typeof(ElMessageService).GetMethod("Success", [typeof(string)])!.ReturnType);
        Assert.AreEqual(typeof(ECMAScript.IPromise<ElMessageBoxPromptResult>), typeof(ElMessageBoxService).GetMethod("Prompt", [typeof(string)])!.ReturnType);
    }

    [TestMethod]
    public async Task OfficialRazor_TypedSelectTableDropdownAndServices_PreservePayloadsAndStyles()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            RazorSgTestHost.GetTestDocumentPath("Pages/ElementPlusTypedInteractions.razor"),
            """
            @using ECMAScript.ElementPlus
            @using Demo.Pages
            @using Microsoft.AspNetCore.Components.Web

            <ElTypedSelect TValue="string" @bind-ModelValue="Id" Filterable="true" AllowCreate="true" DefaultFirstOption="true" Clearable="true" OnChange="@Changed">
                <ElTypedOption TValue="string" Value="@("9007199254740993")" Label="Existing" />
            </ElTypedSelect>
            <ElTypedTable TRow="TypedRow" Data="Rows" OnRowClick="@RowClicked" OnSelectionChange="@SelectionChanged" OnSortChange="@SortChanged">
                <ElTypedTableColumn TRow="TypedRow" Type="selection" Selectable="@CanSelect" />
                <ElTypedTableColumn TRow="TypedRow" Prop="id" Label="ID" Context="cell">
                    <span data-row="@cell.Row.Id" data-index="@cell.Index">@cell.Row.Id</span>
                </ElTypedTableColumn>
            </ElTypedTable>
            <ElTypedDropdown TCommand="string" OnCommand="@CommandChanged">
                <ChildContent>Export</ChildContent>
                <Dropdown>
                    <ElDropdownMenu><ElTypedDropdownItem TCommand="string" Command="@("selected")">Selected</ElTypedDropdownItem></ElDropdownMenu>
                </Dropdown>
            </ElTypedDropdown>
            <button @onclick="@ShowFeedback">Feedback</button>
            <span data-state="true" data-id="@Id" data-changed="@LastChange" data-row="@LastRow" data-count="@SelectedCount" data-sort="@SortProp" data-command="@Command" data-prompt="@PromptText" data-confirmed="@Confirmed"></span>
            """,
            """
            using ECMAScript.ElementPlus;
            namespace Demo.Pages;

            [ECMAScript]
            public sealed record TypedRow : VueProps
            {
                public string Id { get; init; } = "";
                public bool Allowed { get; init; }
            }

            [ECMAScriptModule("./components/element-plus-typed-interactions")]
            public partial class ElementPlusTypedInteractions : ComponentBase, IVueComponent
            {
                private string? Id { get; set; } = "9007199254740993";
                private string? LastChange { get; set; }
                private string? LastRow { get; set; }
                private int SelectedCount { get; set; }
                private string? SortProp { get; set; }
                private string? Command { get; set; }
                private bool Confirmed { get; set; }
                private string? PromptText { get; set; }
                private TypedRow[] Rows { get; } = [new TypedRow { Id = "9007199254740993", Allowed = true }, new TypedRow { Id = "9007199254740995", Allowed = false }];
                private void Changed(string? value) => LastChange = value;
                private void RowClicked(TypedRow row) => LastRow = row.Id;
                private void SelectionChanged(TypedRow[] rows) => SelectedCount = rows.Length;
                private void SortChanged(ElTableSort sort) => SortProp = sort.Prop;
                private void CommandChanged(string command) => Command = command;
                private bool CanSelect(TypedRow row, Number index) => row.Allowed;
                private void ShowFeedback()
                {
                    ElMessage.Service.Success(new ElMessageOptions { Message = "Saved", Grouping = true }).Close();
                    ElNotification.Service.Info(new ElNotificationOptions { Title = "Export", Message = "Ready" }).Close();
                    ElMessageBox.Service.Confirm("Delete?", "Confirm", new ElMessageBoxOptions { Type = ElFeedbackType.Warning })
                        .Then(action => { Confirmed = action == ElMessageBoxAction.Confirm; });
                    ElMessageBox.Service.Prompt("Name?").Then(result => { PromptText = result.Value; });
                }
            }
            """,
            "Demo.Pages", "Demo.Pages.ElementPlusTypedInteractions");

        foreach (var name in new[] { "select", "table", "dropdown", "message", "message-box", "notification" })
            StringAssert.Contains(observation.ModuleText, $"import \"element-plus/es/components/{name}/style/css.mjs\";", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "ElTableSlotContext<", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/element-plus-typed-interactions.js", observation.ModuleText,
            "typed-interactions.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import component from "./components/element-plus-typed-interactions.js";
            import { calls } from "element-plus/es/index.mjs";

            test("typed native values survive model, table and service boundaries", async () => {
                const render = component.setup({}, { slots: {} });
                const find = (name) => render().children.find(node => node?.name?.name === name || node?.name === name);
                const state = () => render().children.find(node => node?.props?.["data-state"] === "true").props;
                const select = find("el-select");
                assert.equal(select.props.modelValue, "9007199254740993");
                assert.equal(select.props.allowCreate, true);
                assert.equal(select.props.filterable, true);
                assert.equal(select.props.defaultFirstOption, true);
                select.props["onUpdate:modelValue"]("New group");
                select.props.onChange("New group");
                assert.equal(state()["data-id"], "New group");
                assert.equal(state()["data-changed"], "New group");
                select.props["onUpdate:modelValue"]("");
                assert.equal(state()["data-id"], "");
                select.props["onUpdate:modelValue"]("9007199254740993");

                const table = find("el-table");
                const [allowed, disabled] = table.props.data;
                const descendants = (nodes) => nodes.flatMap(node => [node, ...(Array.isArray(node?.children) ? descendants(node.children) : [])]);
                const columns = descendants(table.children.default()).filter(node => node?.name?.name === "el-table-column");
                assert.equal(columns.length, 2, JSON.stringify(table.children.default()));
                assert.equal(columns[0].props.selectable(allowed, 0), true);
                assert.equal(columns[0].props.selectable(disabled, 1), false);
                const cell = columns[1].children.default({ row: allowed, column: {}, $index: 0 })[0];
                assert.equal(cell.props["data-row"], "9007199254740993");
                assert.equal(cell.props["data-index"], 0);
                table.props.onRowClick(allowed, {}, {});
                table.props.onSelectionChange([allowed]);
                table.props.onSortChange({ prop: "id", order: "ascending" });
                assert.equal(state()["data-row"], "9007199254740993");
                assert.equal(state()["data-count"], 1);
                assert.equal(state()["data-sort"], "id");
                find("el-dropdown").props.onCommand("selected");
                assert.equal(state()["data-command"], "selected");

                render().children.find(node => node?.name === "button" || node?.name?.name === "button").props.onClick({});
                await Promise.resolve();
                assert.equal(state()["data-confirmed"], true);
                assert.equal(state()["data-prompt"], "Typed name");
                assert.deepEqual(calls, [
                    ["message", { message: "Saved", grouping: true }], ["close-message"],
                    ["notification", { title: "Export", message: "Ready" }], ["close-notification"],
                    ["confirm", "Delete?", "Confirm", { type: "warning" }], ["prompt", "Name?"]
                ]);
            });
            """,
            SupportingModules());

        // Exercise the actual Element Plus and Vue implementations: the lightweight
        // fixture above verifies payload identity, while this checks real input and
        // disabled selection behavior at the DOM boundary.
        var realModules = new Dictionary<string, string>
        {
            // Happy DOM exercises component behavior; CSS is asserted above and
            // has no module loader in Deno. Keep this fixture outside node_modules
            // because npm restoration replaces package directories with symlinks.
            ["fixtures/empty-style.mjs"] = ""
        };
        var npmImports = new Dictionary<string, string>
        {
            ["element-plus/es/index.mjs"] = "npm:element-plus@2.14.5/es/index.mjs"
        };
        foreach (var path in new[] { "select", "table", "dropdown" })
            npmImports[$"element-plus/es/components/{path}/index.mjs"] = $"npm:element-plus@2.14.5/es/components/{path}/index.mjs";
        foreach (var path in new[] { "select", "table", "dropdown", "message", "message-box", "notification" })
            npmImports[$"element-plus/es/components/{path}/style/css.mjs"] = "./fixtures/empty-style.mjs";
        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/element-plus-typed-interactions.js", observation.ModuleText,
            "typed-interactions-real-dom.test.mjs",
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
            const { default: component } = await import("./components/element-plus-typed-interactions.js");
            const settle = async () => { await nextTick(); await new Promise(resolve => setTimeout(resolve, 40)); await nextTick(); };

            Deno.test("actual Element Plus accepts string IDs and created values, and blocks disabled rows", async () => {
                const host = document.createElement("main");
                document.body.append(host);
                const app = createApp(component);
                app.mount(host);
                await settle();
                const state = () => host.querySelector("[data-state]");
                assert.equal(state().getAttribute("data-id"), "9007199254740993");
                const input = host.querySelector(".el-select input");
                input.focus();
                input.dispatchEvent(new MouseEvent("click", { bubbles: true }));
                input.value = "Created group";
                input.dispatchEvent(new Event("input", { bubbles: true }));
                await settle();
                await new Promise(resolve => setTimeout(resolve, 350));
                await settle();
                const created = [...document.querySelectorAll(".el-select-dropdown__item")].find(item => item.textContent.includes("Created group"));
                assert.ok(created, "allow-create should offer the entered text as an option");
                created.dispatchEvent(new MouseEvent("click", { bubbles: true }));
                await settle();
                assert.equal(state().getAttribute("data-id"), "Created group");
                assert.equal(state().getAttribute("data-changed"), "Created group");

                const rows = host.querySelectorAll(".el-table__body tbody tr");
                assert.equal(rows.length, 2);
                assert.equal(rows[1].querySelector("input[type=checkbox]").disabled, true);
                rows[0].dispatchEvent(new MouseEvent("click", { bubbles: true }));
                await settle();
                assert.equal(state().getAttribute("data-row"), "9007199254740993");
                const checkbox = rows[0].querySelector("input[type=checkbox]");
                checkbox.checked = true;
                checkbox.dispatchEvent(new Event("change", { bubbles: true }));
                await settle();
                assert.equal(state().getAttribute("data-count"), "1");
                assert.ok(host.querySelector('[data-row="9007199254740993"][data-index="0"]'));
                app.unmount();
                await nextTick();
                window.happyDOM.abort();
            });
            """, realModules,
            vueModuleSpecifier: "npm:vue@3.5.42",
            restoreNpmDependencies: true,
            importSpecifiers: npmImports);
    }

    private static Dictionary<string, string> SupportingModules()
    {
        var modules = new Dictionary<string, string>
        {
            ["node_modules/element-plus/package.json"] = """{"type":"module"}""",
            ["node_modules/element-plus/es/index.mjs"] = """
                export const calls = [];
                export const ElMessage = { success(options) { calls.push(["message", options]); return { close() { calls.push(["close-message"]); } }; } };
                export const ElNotification = { info(options) { calls.push(["notification", options]); return { close() { calls.push(["close-notification"]); } }; } };
                export const ElMessageBox = {
                    confirm(...args) { calls.push(["confirm", ...args]); return Promise.resolve("confirm"); },
                    prompt(...args) { calls.push(["prompt", ...args]); return Promise.resolve({ value: "Typed name", action: "confirm" }); }
                };
                """
        };
        foreach (var (path, names) in new[]
        {
            ("select", new[] { "ElSelect", "ElOption" }),
            ("table", new[] { "ElTable", "ElTableColumn" }),
            ("dropdown", new[] { "ElDropdown", "ElDropdownMenu", "ElDropdownItem" })
        })
            modules[$"node_modules/element-plus/es/components/{path}/index.mjs"] = string.Join("\n", names.Select(name => $"export const {name} = {{ name: \"{System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1-$2").ToLowerInvariant()}\" }};"));
        foreach (var path in new[] { "select", "table", "dropdown", "message", "message-box", "notification" })
            modules[$"node_modules/element-plus/es/components/{path}/style/css.mjs"] = "";
        return modules;
    }

    private sealed record Row;
}
