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
            <TInput T="string" Value="@Name" OnChange="@OnNameChanged" />
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
                        Cell = (TPrimaryTableColCell<Row>)((RenderFragment<TPrimaryTableCellParams<Row>>)(context => builder =>
                        {
                            builder.AddContent(0, context.Row.Label);
                        }))
                    }
                ];

                private void Activate() => Activated++;

                private void OnNameChanged(string value) => Name = value;
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
}
