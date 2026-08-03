namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialEnumerableSetAuthoringTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorSetOperators_UseCompilerOwnedEnumerableContracts()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseSetSummary.razor",
            documentText:
            """
            @using System.Linq

            <ul data-summary="release-set">
                @foreach (var release in Primary.Distinct().Union(Secondary).Except(Removed).Intersect(Allowed))
                {
                    <li data-primary="@Primary.Contains(release)">@release</li>
                }
            </ul>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-set-summary")]
            public partial class ReleaseSetSummary : ComponentBase, IVueComponent
            {
                private readonly int[] Primary = [1, 2, 2, 3];
                private readonly int[] Secondary = [2, 3, 4];
                private readonly int[] Removed = [1];
                private readonly int[] Allowed = [2, 3, 4];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseSetSummary");

        StringAssert.Contains(observation.GeneratedCSharp, ".Distinct()", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, ".Union(", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, ".Except(", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, ".Intersect(", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, ".Contains(", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);

        var script = observation.ModuleText;
        StringAssert.Contains(script, "System/Linq/EnumerableModule.js", StringComparison.Ordinal);
        StringAssert.Contains(script, "_a2bc38786226403e", StringComparison.Ordinal);
        StringAssert.Contains(script, "_b5fae0c231974056", StringComparison.Ordinal);
        StringAssert.Contains(script, "_c71d4ff9a863431d", StringComparison.Ordinal);
        StringAssert.Contains(script, "_d83c9e4a7bf747a8", StringComparison.Ordinal);
        StringAssert.Contains(script, "System/MemoryExtensionsModule.js", StringComparison.Ordinal);
        StringAssert.Contains(script, "_a4ed2b50c69946de", StringComparison.Ordinal);
        StringAssert.Contains(script, "Array.from(", StringComparison.Ordinal);
        StringAssert.Contains(script, "data-primary", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-set-summary.mjs",
            observation.ModuleText,
            "official-release-set-summary.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-set-summary.mjs";

            function collect(node, name) {
                if (node == null) return [];
                if (Array.isArray(node)) return node.flatMap(item => collect(item, name));
                const children = collect(node.children, name);
                return node.name === name ? [node, ...children] : children;
            }

            test("official Razor set operations use CLR catalog modules at runtime", () => {
                const root = component.setup({}, { slots: {} })();
                assert.equal(root.name, "ul");
                assert.equal(root.props["data-summary"], "release-set");

                const rows = collect(root, "li");
                assert.equal(rows.length, 3);
                assert.deepEqual(
                    rows.map(row => [row.children[0], row.props["data-primary"]]),
                    [[2, true], [3, true], [4, false]]);
            });
            """);
    }
}
