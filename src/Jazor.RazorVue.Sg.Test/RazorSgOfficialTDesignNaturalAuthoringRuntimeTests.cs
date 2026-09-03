using ECMAScript.TDesign;
using Microsoft.AspNetCore.Components;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialTDesignNaturalAuthoringRuntimeTests
{
    [TestMethod]
    public void GenericDefaultAliases_AreNotPublicRazorComponents()
    {
        var assembly = typeof(TInput<>).Assembly;
        var aliases = new[]
        {
            "TForm",
            "TInput",
            "TPrimaryTable",
            "TRadioGroup",
            "TSwitch",
            "TTable"
        };

        foreach (var aliasName in aliases)
        {
            var alias = assembly.GetType($"ECMAScript.TDesign.{aliasName}");
            Assert.IsNotNull(alias, $"The generated default alias {aliasName} is missing.");
            Assert.IsFalse(alias!.IsPublic, $"The generated default alias {aliasName} must stay out of Razor component discovery.");
            Assert.IsFalse(alias.IsNestedPublic, $"The generated default alias {aliasName} must stay out of Razor component discovery.");
        }

        foreach (var genericComponent in assembly.GetTypes().Where(static type =>
                     type.IsClass &&
                     type.IsPublic &&
                     type.IsGenericTypeDefinition &&
                     typeof(ComponentBase).IsAssignableFrom(type)))
        {
            var alias = assembly.GetType($"{genericComponent.Namespace}.{genericComponent.Name}");
            if (alias is not null && !alias.IsGenericType)
            {
                Assert.IsFalse(
                    alias.IsPublic,
                    $"The generated default alias {alias.Name} must stay out of Razor component discovery.");
            }
        }
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignGenericAndNonGenericComponents_BindWithoutBridge()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignNaturalAuthoringRuntime.razor"),
            documentText:
            """
            @using ECMAScript.TDesign

            <TButton Theme="@TButtonThemeValue.Primary" OnClick="@Activate">Save</TButton>
            <TInput T="string" @bind-Value="Name" @bind-Value:event="OnChange" />
            <TPrimaryTable T="Row" Data="@Rows" Columns="@Columns" RowKey="Id" />
            <span data-name="@Name" data-activated="@Activated">@Name:@Activated</span>
            """,
            codeBehindSource:
            """
            using ECMAScript.TDesign;

            namespace Demo.Pages;

            public sealed record Row(int Id, string Label);

            [ECMAScriptModule("./components/tdesign-natural-authoring-runtime")]
            public partial class TDesignNaturalAuthoringRuntime : ComponentBase, IVueComponent
            {
                private string Name { get; set; } = "Initial";
                private int Activated { get; set; }
                private Row[] Rows { get; } = [new(7, "Release")];

                private TPrimaryTableCol<Row>[] Columns =>
                [
                    new()
                    {
                        Title = "Label",
                        Cell = new(context => builder =>
                        {
                            builder.AddContent(0, context.Row.Label);
                        })
                    }
                ];

                private void Activate() => Activated++;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignNaturalAuthoringRuntime");

        var normalizedGeneratedCSharp = string.Concat(
            observation.GeneratedCSharp
                .Split('\n')
                .Where(static line => !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                .SelectMany(static line => line.Where(static character => !char.IsWhiteSpace(character))));
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TInput<string>>", StringComparison.Ordinal);
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TPrimaryTable<Row>>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.CreateInferredEventCallback", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "nameof(global::ECMAScript.TDesign.TInput<string>.OnChange)",
            StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "import { Button, Input, PrimaryTable } from \"tdesign-vue-next\";", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("admin-input", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("builder.OpenComponent", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/tdesign-natural-authoring-runtime.mjs",
            observation.ModuleText,
            "official-tdesign-natural-authoring-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/tdesign-natural-authoring-runtime.mjs";
            import { Button, Input, PrimaryTable } from "tdesign-vue-next";

            test("natural TDesign components preserve typed props and callbacks", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                assert.equal(initial.name.description, "Fragment");
                const nodes = initial.children.filter(node => [Button, Input, PrimaryTable].includes(node?.name) || node?.name === "span");
                assert.equal(nodes.length, 4);
                assert.equal(nodes[0].name, Button);
                assert.equal(nodes[0].props.theme, "primary");
                assert.equal(nodes[1].name, Input);
                assert.equal(nodes[1].props.value, "Initial");
                assert.equal(nodes[2].name, PrimaryTable);
                assert.equal(nodes[2].props.rowKey, "Id");
                assert.equal(nodes[2].props.data[0].Label, "Release");
                assert.equal(nodes[2].props.columns.length, 1);
                assert.equal(typeof nodes[2].props.columns[0].cell, "function");
                assert.equal(typeof nodes[0].props.onClick, "function");
                assert.equal(typeof nodes[1].props.onChange, "function");

                nodes[0].props.onClick();
                nodes[1].props.onChange("Updated");
                const updated = render();
                const updatedSpan = updated.children.find(node => node?.name === "span");
                assert.equal(updatedSpan.props["data-name"], "Updated");
                assert.equal(updatedSpan.props["data-activated"], 1);
            });
            """,
            new Dictionary<string, string>
            {
                ["node_modules/tdesign-vue-next/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/tdesign-vue-next/index.mjs"] = "export const Button = { name: \"button\" }; export const Input = { name: \"input\" }; export const PrimaryTable = { name: \"primary-table\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignFormAndControlGenerics_BindAndSlotWithoutBridge()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignFormControlNaturalAuthoringRuntime.razor"),
            documentText:
            """
            @using ECMAScript.TDesign

            <TForm FormData="EditorModel" Data="@FormData" OnSubmit="@Submit">
                <TFormItem LabelValue="Name" Name="name">
                    <TInput T="string" @attributes="InputAttributes" Value="@FormData.Name" />
                </TFormItem>
            </TForm>
            <TRadioGroup T="string" @bind-Value="Stage" @bind-Value:event="OnChange">
                <TRadioButton T="string" Value="@("draft")">Draft</TRadioButton>
                <TRadioButton T="string" Value="@("published")">Published</TRadioButton>
            </TRadioGroup>
            <TSwitch T="bool" @bind-Value="Enabled" @bind-Value:event="OnChange">
                <LabelContent Context="label">
                    <span data-switch-value="@label.Value">Toggle</span>
                </LabelContent>
            </TSwitch>
            <span data-stage="@Stage" data-enabled="@Enabled" data-submitted="@Submitted"></span>
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;
            using ECMAScript.TDesign;

            namespace Demo.Pages;

            public sealed record EditorModel(string Name);

            [ECMAScriptModule("./components/tdesign-form-control-natural-authoring-runtime")]
            public partial class TDesignFormControlNaturalAuthoringRuntime : ComponentBase, IVueComponent
            {
                private EditorModel FormData { get; } = new("Ada");
                private IReadOnlyDictionary<string, object?> InputAttributes { get; } = new Dictionary<string, object?>
                {
                    ["data-editor"] = "profile-name"
                };
                private string Stage { get; set; } = "draft";
                private bool Enabled { get; set; }
                private int Submitted { get; set; }

                private void Submit(TSubmitContext<EditorModel> context) => Submitted++;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignFormControlNaturalAuthoringRuntime");

        var normalizedGeneratedCSharp = string.Concat(
            observation.GeneratedCSharp
                .Split('\n')
                .Where(static line => !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                .SelectMany(static line => line.Where(static character => !char.IsWhiteSpace(character))));
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TForm<EditorModel>>", StringComparison.Ordinal);
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TRadioGroup<string>>", StringComparison.Ordinal);
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TSwitch<bool>>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "RuntimeHelpers.CreateInferredEventCallback", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "import { Form, FormItem, Input, RadioButton, RadioGroup, Switch } from \"tdesign-vue-next\";", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("AdminForm", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("AdminRadioGroup", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("AdminToggle", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/tdesign-form-control-natural-authoring-runtime.mjs",
            observation.ModuleText,
            "official-tdesign-form-control-natural-authoring-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/tdesign-form-control-natural-authoring-runtime.mjs";
            import { Form, FormItem, Input, RadioButton, RadioGroup, Switch } from "tdesign-vue-next";

            const findAll = (value, predicate) => {
                if (Array.isArray(value)) {
                    return value.flatMap(entry => findAll(entry, predicate));
                }
                if (!value || typeof value !== "object") {
                    return [];
                }
                return [
                    ...(predicate(value) ? [value] : []),
                    ...findAll(value.children, predicate)
                ];
            };

            test("natural TDesign form controls preserve generic props, splats, slots, and bindings", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const nodes = initial.children.filter(node => [Form, RadioGroup, Switch].includes(node?.name) || node?.name === "span");
                assert.equal(nodes.length, 4);

                const form = nodes[0];
                const radioGroup = nodes[1];
                const toggle = nodes[2];
                assert.equal(form.name, Form);
                assert.equal(form.props.data.Name, "Ada");
                assert.equal(typeof form.props.onSubmit, "function");
                assert.equal(typeof form.children.default, "function");

                const formItem = findAll(form.children.default(), node => node?.name === FormItem)[0];
                assert.ok(formItem);
                assert.equal(formItem.props.label, "Name");
                const input = findAll(formItem.children.default(), node => node?.name === Input)[0];
                assert.ok(input);
                assert.equal(input.props.value, "Ada");
                assert.equal(input.props["data-editor"], "profile-name");

                assert.equal(radioGroup.name, RadioGroup);
                assert.equal(radioGroup.props.value, "draft");
                assert.equal(typeof radioGroup.props.onChange, "function");
                const radioButtons = findAll(radioGroup.children.default(), node => node?.name === RadioButton);
                assert.equal(radioButtons.length, 2);
                assert.equal(radioButtons[0].props.value, "draft");
                assert.equal(radioButtons[1].props.value, "published");

                assert.equal(toggle.name, Switch);
                assert.equal(toggle.props.value, false);
                assert.equal(typeof toggle.props.onChange, "function");
                assert.equal(typeof toggle.children.label, "function");
                const label = findAll(toggle.children.label({ value: true }), node => node?.name === "span")[0];
                assert.ok(label);
                assert.equal(label.props["data-switch-value"], true);

                form.props.onSubmit({});
                radioGroup.props.onChange("published");
                toggle.props.onChange(true);
                const updated = render();
                const state = updated.children.find(node => node?.name === "span" && node.props["data-stage"] !== undefined);
                assert.equal(state.props["data-stage"], "published");
                assert.equal(state.props["data-enabled"], true);
                assert.equal(state.props["data-submitted"], 1);
            });
            """,
            new Dictionary<string, string>
            {
                ["node_modules/tdesign-vue-next/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/tdesign-vue-next/index.mjs"] = "export const Form = { name: \"form\" }; export const FormItem = { name: \"form-item\" }; export const Input = { name: \"input\" }; export const RadioButton = { name: \"radio-button\" }; export const RadioGroup = { name: \"radio-group\" }; export const Switch = { name: \"switch\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignTextarea_BindsNativeUnionWithoutBridge()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignTextareaNaturalAuthoringRuntime.razor"),
            documentText:
            """
            @using ECMAScript.TDesign

            <TTextarea @bind-Value="Draft.Notes" @bind-Value:event="OnChange" />
            <span data-notes="@Draft.Notes"></span>
            """,
            codeBehindSource:
            """
            using ECMAScript.TDesign;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/tdesign-textarea-natural-authoring-runtime")]
            public partial class TDesignTextareaNaturalAuthoringRuntime : ComponentBase, IVueComponent
            {
                private sealed record EditorDraft
                {
                    public TTextareaValue Notes { get; set; } = string.Empty;
                }

                private EditorDraft Draft { get; } = new()
                {
                    Notes = "Initial note"
                };
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignTextareaNaturalAuthoringRuntime");

        var normalizedGeneratedCSharp = string.Concat(
            observation.GeneratedCSharp
                .Split('\n')
                .Where(static line => !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                .SelectMany(static line => line.Where(static character => !char.IsWhiteSpace(character))));
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TTextarea>", StringComparison.Ordinal);
        StringAssert.Contains(
            observation.GeneratedCSharp,
            "nameof(global::ECMAScript.TDesign.TTextarea.OnChange)",
            StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "import { Textarea } from \"tdesign-vue-next\";", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("AdminTextarea", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/tdesign-textarea-natural-authoring-runtime.mjs",
            observation.ModuleText,
            "official-tdesign-textarea-natural-authoring-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/tdesign-textarea-natural-authoring-runtime.mjs";
            import { Textarea } from "tdesign-vue-next";

            test("natural TDesign textarea binds its native union value", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const textarea = initial.children.find(node => node?.name === Textarea);
                const initialState = initial.children.find(node => node?.name === "span");

                assert.ok(textarea);
                assert.equal(textarea.props.value, "Initial note");
                assert.equal(typeof textarea.props.onChange, "function");
                assert.equal(initialState.props["data-notes"], "Initial note");

                textarea.props.onChange("Updated note");
                const updated = render();
                const updatedState = updated.children.find(node => node?.name === "span");
                assert.equal(updatedState.props["data-notes"], "Updated note");
            });
            """,
            new Dictionary<string, string>
            {
                ["node_modules/tdesign-vue-next/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/tdesign-vue-next/index.mjs"] = "export const Textarea = { name: \"textarea\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignDialogAndTableSlots_PreserveValueAndContentContractsWithoutBridge()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/TDesignDialogAndTableNaturalAuthoringRuntime.razor"),
            documentText:
            """
            @using ECMAScript.TDesign

            <TDialog Visible="@DialogVisible" ConfirmBtnValue="@("Publish")" OnConfirm="@Confirm">
                <BodyContent>
                    <span data-dialog-body="@DialogBody">Body</span>
                </BodyContent>
            </TDialog>
            <TDialog Visible="@DialogVisible" ConfirmBtnValue="@ConfirmButton">
            </TDialog>
            <TDialog Visible="@DialogVisible">
                <ConfirmBtnContent>
                    <span data-dialog-confirm-slot="@DialogVisible">Confirm slot</span>
                </ConfirmBtnContent>
            </TDialog>
            <TTable T="Row" Data="@Rows" RowKey="Id" LoadingValue="@IsLoading">
                <LoadingContent>
                    <span data-table-loading="@IsLoading">Loading</span>
                </LoadingContent>
                <EmptyContent>
                    <span data-table-empty="@IsLoading">Empty</span>
                </EmptyContent>
            </TTable>
            <TPrimaryTable T="Row" Data="@Rows" RowKey="Id" LoadingValue="@IsLoading">
                <LoadingContent>
                    <span data-primary-table-loading="@IsLoading">Loading</span>
                </LoadingContent>
            </TPrimaryTable>
            <span data-confirmed="@Confirmed"></span>
            """,
            codeBehindSource:
            """
            using ECMAScript.TDesign;

            namespace Demo.Pages;

            public sealed record Row(int Id, string Label);

            [ECMAScriptModule("./components/tdesign-dialog-table-natural-authoring-runtime")]
            public partial class TDesignDialogAndTableNaturalAuthoringRuntime : ComponentBase, IVueComponent
            {
                private Row[] Rows { get; } = [new(7, "Release")];
                private bool DialogVisible { get; } = true;
                private bool IsLoading { get; } = true;
                private string DialogBody { get; } = "Review release";
                private TdButtonProps ConfirmButton { get; } = new()
                {
                    Theme = TdButtonPropsTheme.Primary
                };
                private int Confirmed { get; set; }

                private void Confirm(TDialogConfirmEventContext context) => Confirmed++;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignDialogAndTableNaturalAuthoringRuntime");

        var normalizedGeneratedCSharp = string.Concat(
            observation.GeneratedCSharp
                .Split('\n')
                .Where(static line => !line.TrimStart().StartsWith("#", StringComparison.Ordinal))
                .SelectMany(static line => line.Where(static character => !char.IsWhiteSpace(character))));
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TDialog>", StringComparison.Ordinal);
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TTable<Row>>", StringComparison.Ordinal);
        StringAssert.Contains(normalizedGeneratedCSharp, "OpenComponent<global::ECMAScript.TDesign.TPrimaryTable<Row>>", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "import { Dialog, PrimaryTable, Table } from \"tdesign-vue-next\";", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("AdminDialog", StringComparison.Ordinal), observation.ModuleText);
        Assert.IsFalse(observation.ModuleText.Contains("AdminTable", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/tdesign-dialog-table-natural-authoring-runtime.mjs",
            observation.ModuleText,
            "official-tdesign-dialog-table-natural-authoring-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/tdesign-dialog-table-natural-authoring-runtime.mjs";
            import { Dialog, PrimaryTable, Table } from "tdesign-vue-next";

            const findAll = (value, predicate) => {
                if (Array.isArray(value)) {
                    return value.flatMap(entry => findAll(entry, predicate));
                }
                if (!value || typeof value !== "object") {
                    return [];
                }
                return [
                    ...(predicate(value) ? [value] : []),
                    ...findAll(value.children, predicate)
                ];
            };

            test("natural TDesign dialogs and tables preserve union values and named slots", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const dialogs = findAll(initial, node => node?.name === Dialog);
                const table = findAll(initial, node => node?.name === Table)[0];
                const primaryTable = findAll(initial, node => node?.name === PrimaryTable)[0];

                assert.equal(dialogs.length, 3);
                assert.equal(dialogs[0].props.visible, true);
                assert.equal(dialogs[0].props.confirmBtn, "Publish");
                assert.equal(typeof dialogs[0].props.onConfirm, "function");
                assert.equal(typeof dialogs[0].children.body, "function");
                const body = findAll(dialogs[0].children.body(), node => node?.name === "span")[0];
                assert.ok(body);
                assert.equal(body.props["data-dialog-body"], "Review release");

                assert.equal(dialogs[1].props.confirmBtn.theme, "primary");
                assert.equal(dialogs[1].children, null);
                assert.equal(typeof dialogs[2].children.confirmBtn, "function");
                const confirmSlot = findAll(dialogs[2].children.confirmBtn(), node => node?.name === "span")[0];
                assert.ok(confirmSlot, JSON.stringify(dialogs[2].children.confirmBtn()));
                assert.equal(confirmSlot.props["data-dialog-confirm-slot"], true);

                assert.ok(table);
                assert.equal(table.props.rowKey, "Id");
                assert.equal(table.props.data[0].Label, "Release");
                assert.equal(table.props.loading, true);
                assert.equal(typeof table.children.loading, "function");
                assert.equal(typeof table.children.empty, "function");
                const loading = findAll(table.children.loading(), node => node?.name === "span")[0];
                const empty = findAll(table.children.empty(), node => node?.name === "span")[0];
                assert.ok(loading);
                assert.ok(empty);
                assert.equal(loading.props["data-table-loading"], true);
                assert.equal(empty.props["data-table-empty"], true);

                assert.ok(primaryTable);
                assert.equal(primaryTable.props.rowKey, "Id");
                assert.equal(primaryTable.props.loading, true);
                assert.equal(typeof primaryTable.children.loading, "function");
                const primaryLoading = findAll(primaryTable.children.loading(), node => node?.name === "span")[0];
                assert.ok(primaryLoading);
                assert.equal(primaryLoading.props["data-primary-table-loading"], true);

                dialogs[0].props.onConfirm({});
                const updated = render();
                const state = findAll(updated, node => node?.name === "span" && node.props["data-confirmed"] !== undefined)[0];
                assert.ok(state);
                assert.equal(state.props["data-confirmed"], 1);
            });
            """,
            new Dictionary<string, string>
            {
                ["node_modules/tdesign-vue-next/package.json"] = """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/tdesign-vue-next/index.mjs"] = "export const Dialog = { name: \"dialog\" }; export const PrimaryTable = { name: \"primary-table\" }; export const Table = { name: \"table\" };"
            });
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignTables_ReportMissingRequiredRowKey()
    {
        var documentPath = RazorSgTestHost.GetTestDocumentPath("Pages/TDesignTableRequiredAuthoring.razor");
        var diagnostics = await RazorSgOfficialAuthoringTestHost.GetGeneratorDiagnosticsAsync(
            documentPath,
            """
            @using ECMAScript.TDesign

            <TTable T="Row" Data="@Rows" />
            <TPrimaryTable T="Row" Data="@Rows" />
            """,
            """
            using ECMAScript.TDesign;

            namespace Demo.Pages;

            public sealed record Row(int Id);

            [ECMAScriptModule("./components/tdesign-table-required-authoring")]
            public partial class TDesignTableRequiredAuthoring : ComponentBase, IVueComponent
            {
                private Row[] Rows { get; } = [new(7)];
            }
            """,
            "Demo.Pages");

        var requiredDiagnostics = diagnostics.Where(static diagnostic => diagnostic.Id == "RZ2012").ToArray();
        Assert.AreEqual(
            2,
            requiredDiagnostics.Length,
            string.Join(Environment.NewLine, diagnostics.Select(static diagnostic => diagnostic.Id + ": " + diagnostic.GetMessage())));
        foreach (var diagnostic in requiredDiagnostics)
        {
            StringAssert.Contains(diagnostic.GetMessage(), "RowKey", StringComparison.Ordinal);
        }
    }
}
