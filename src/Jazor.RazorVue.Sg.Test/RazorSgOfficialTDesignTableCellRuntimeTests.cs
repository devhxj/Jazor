namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialTDesignTableCellRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OrdinaryTDesignTableCellRenderFragment_ProducesCallableVNodeFactoryOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignTableCellRuntime.razor"),
            documentText:
            """
            @using Demo.Library

            <CellCapture Columns="@Columns" />
            <span data-activated="@activated">@activated</span>
            """,
            codeBehindSource:
            """
            using Demo.Library;
            using ECMAScript.TDesign;

            namespace Demo.Pages;

            public sealed record CellRow(int Id, string Label);

            [ECMAScriptModule("./components/t-design-table-cell-runtime")]
            public partial class TDesignTableCellRuntime : ComponentBase, IVueComponent
            {
                private int activated;

                private TPrimaryTableCol<CellRow>[] Columns =>
                [
                    new()
                    {
                        Cell = (TPrimaryTableColCell<CellRow>)((RenderFragment<TPrimaryTableCellParams<CellRow>>)(context => builder =>
                        {
                            builder.OpenElement(0, "div");
                            builder.AddAttribute(1, "data-cell-row", context.Row.Id);
                            builder.OpenElement(2, "strong");
                            builder.AddContent(3, context.Row.Label);
                            builder.CloseElement();
                            builder.OpenComponent<TButton>(4);
                            builder.AddComponentParameter(5, nameof(TButton.OnClick),
                                EventCallback.Factory.Create(this, () => activated = context.Row.Id));
                            builder.AddComponentParameter(6, nameof(TButton.ChildContent),
                                (RenderFragment)(child => child.AddContent(0, "Activate")));
                            builder.CloseComponent();
                            builder.CloseElement();
                        }))
                    }
                ];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignTableCellRuntime",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TDesignTableContracts.cs"] =
                """
                using ECMAScript;
                using Microsoft.AspNetCore.Components;

                namespace ECMAScript.TDesign;

                [ECMAScript]
                public record TPrimaryTableCellParams<T>
                {
                    [ECMAScriptName("row")]
                    public T Row { get; init; } = default!;
                }

                [ECMAScript]
                public record TPrimaryTableCol<T>
                {
                    [ECMAScriptName("cell")]
                    public TPrimaryTableColCell<T>? Cell { get; init; }
                }

                [ECMAScript]
                public readonly union TPrimaryTableColCell<T>(string, RenderFragment<TPrimaryTableCellParams<T>>)
                {
                }
                """,
                ["Library/TableCellComponents.cs"] =
                """
                using ECMAScript;
                using ECMAScript.TDesign;
                using ECMAScript.VueContract;
                using Microsoft.AspNetCore.Components;

                namespace Demo.Library;

                [ECMAScript("table-cell-capture", Transform.Component, "CellCapture")]
                public sealed class CellCapture : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("columns")]
                    public TPrimaryTableCol<Demo.Pages.CellRow>[]? Columns { get; set; }
                }

                [ECMAScript("tdesign-vue-next", Transform.Component, "Button")]
                public sealed class TButton : ComponentBase, IVueComponent
                {
                    [Parameter, ECMAScriptName("onClick")]
                    public EventCallback OnClick { get; set; }

                    [Parameter, ECMAScriptName("default")]
                    public RenderFragment? ChildContent { get; set; }
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("builder.Open", StringComparison.Ordinal), observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "__jazor$renderH, context", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "import { Button } from \"tdesign-vue-next\";", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/t-design-table-cell-runtime.mjs",
            observation.ModuleText,
            "official-t-design-table-cell-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/t-design-table-cell-runtime.mjs";

            const find = (node, predicate) => {
                if (node && predicate(node)) return node;
                const children = Array.isArray(node?.children) ? node.children : [];
                for (const child of children) {
                    const match = find(child, predicate);
                    if (match) return match;
                }
                return undefined;
            };

            test("ordinary TDesign table cell executes as a VNode callback", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const capture = find(initial, node => node?.name?.name === "cell-capture");
                assert.ok(capture);
                assert.equal(typeof capture.props.columns[0].cell, "function");

                const cell = capture.props.columns[0].cell(null, { row: { Id: 42, Label: "Release" } });
                assert.equal(cell.name, "div");
                assert.equal(cell.props["data-cell-row"], 42);
                const label = find(cell, node => node?.name === "strong");
                const button = find(cell, node => node?.name?.name === "button");
                assert.ok(label);
                assert.ok(button);
                assert.equal(label.children, "Release");
                assert.equal(button.children.default()[0], "Activate");

                button.props.onClick();
                const updated = render();
                const state = find(updated, node => node?.name === "span");
                assert.ok(state);
                assert.equal(state.props["data-activated"], 42);
                assert.deepEqual(state.children, [42]);
            });
            """,
            new Dictionary<string, string>
            {
                ["node_modules/table-cell-capture/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/table-cell-capture/index.mjs"] = "export const CellCapture = { name: \"cell-capture\" };",
                ["node_modules/tdesign-vue-next/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/tdesign-vue-next/index.mjs"] = "export const Button = { name: \"button\" };"
            });
    }
}
