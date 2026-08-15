namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialCrossPassImportNameCollisionRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorModuleStaticMember_ImportsOnlyTheMemberOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\StaticModuleText.razor",
            documentText:
            """
            @using Demo.Helpers

            <p data-text="@StaticMessageModule.Get(Value)">@StaticMessageModule.Get(Value)</p>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/static-module-text")]
            public partial class StaticModuleText : ComponentBase, IVueComponent
            {
                private string Value { get; } = "source";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.StaticModuleText",
            supportingSources: new Dictionary<string, string>
            {
                ["Helpers/StaticMessageModule.cs"] =
                """
                using ECMAScript;

                namespace Demo.Helpers;

                [ECMAScriptModule("./helpers/static-message")]
                public static class StaticMessageModule
                {
                    public static string Get(string value) => value;
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "helpers/static-message", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "import { Get }", StringComparison.Ordinal);
        Assert.IsFalse(
            observation.ModuleText.Contains("StaticMessageModule", StringComparison.Ordinal),
            "Static module calls must import their export, not materialize the C# containing type.");

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/static-module-text.mjs",
            observation.ModuleText,
            "official-static-module-member-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/static-module-text.mjs";

            test("static module member calls link without a containing-type export", () => {
                const vnode = component.setup({}, { slots: {} })();
                assert.equal(vnode.name, "p");
                assert.equal(vnode.props["data-text"], "module:source");
                assert.equal(vnode.children, "module:source");
            });
            """,
            new Dictionary<string, string>
            {
                ["helpers/static-message.mjs"] =
                    "export function Get(value) { return `module:${value}`; }"
            });
    }

    [TestMethod]
    public async Task BuildComponent_CompilerAndDirectRenderImportsShareExportName_UsesStableAliasOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\CrossPassImportNameCollision.razor",
            documentText:
            """
            @using Demo.Library

            <DirectNormalize Value="@Text" />
            <p data-compiler="@CompilerText">@CompilerText</p>
            """,
            codeBehindSource:
            """
            using Demo.Helpers;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/cross-pass-import-name-collision")]
            public partial class CrossPassImportNameCollision : ComponentBase, IVueComponent
            {
                private string Text { get; } = " source ";

                private string CompilerText => CompilerNormalize.Normalize(Text);
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.CrossPassImportNameCollision",
            supportingSources: new Dictionary<string, string>
            {
                ["Helpers/CompilerNormalize.cs"] =
                """
                namespace Demo.Helpers;

                [ECMAScriptModule("./helpers/compiler-normalize")]
                public static class CompilerNormalize
                {
                    public static string Normalize(string value) => value.Trim();
                }
                """,
                ["Library/DirectNormalize.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [VueLibraryComponent("direct-normalize-library", "Normalize")]
                public sealed class DirectNormalize : ComponentBase
                {
                    [Parameter, System.ComponentModel.Description("@#value")] public string Value { get; set; } = string.Empty;
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "compiler-normalize", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "direct-normalize-library", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "as i$", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/cross-pass-import-name-collision.mjs",
            observation.ModuleText,
            "official-cross-pass-import-name-collision-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/cross-pass-import-name-collision.mjs";

            test("compiler and direct render imports retain independent same-name exports", () => {
                const vnode = component.setup({}, { slots: {} })();
                assert.equal(vnode.name.description, "Fragment");
                const find = (node, predicate) => {
                    if (node && predicate(node)) return node;
                    const children = Array.isArray(node?.children) ? node.children : [];
                    for (const child of children) {
                        const match = find(child, predicate);
                        if (match) return match;
                    }
                    return undefined;
                };
                const panel = find(vnode, node => node?.name?.name === "direct-normalize");
                const text = find(vnode, node => node?.name === "p");
                assert.ok(panel);
                assert.ok(text);
                assert.equal(panel.name.name, "direct-normalize");
                assert.equal(panel.props.value, " source ");
                assert.equal(text.name, "p");
                assert.equal(text.props["data-compiler"], "compiler:source");
                assert.equal(text.children, "compiler:source");
            });
            """,
            new Dictionary<string, string>
            {
                ["helpers/compiler-normalize.mjs"] =
                    "export function Normalize(value) { return `compiler:${value.trim()}`; }",
                ["node_modules/direct-normalize-library/package.json"] =
                    """{"type":"module","exports":"./index.mjs"}""",
                ["node_modules/direct-normalize-library/index.mjs"] =
                    "export const Normalize = { name: \"direct-normalize\" };"
            });
    }
}
