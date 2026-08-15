namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialForLoopRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorForLoop_LowersIndexedKeyedContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\IndexedForLoopRuntime.razor",
            documentText:
            """
            @for (var index = 0; index < Items.Length; index++)
            {
                <li @key="Items[index]" data-index="@index">@Items[index]</li>
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/indexed-for-loop-runtime")]
            public partial class IndexedForLoopRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string[] Items { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.IndexedForLoopRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "for (var index", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "for (;", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/indexed-for-loop-runtime.mjs",
            observation.ModuleText,
            "official-indexed-for-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/indexed-for-loop-runtime.mjs";

            test("official Razor @for lowers indexed keyed children", () => {
                const fragment = component.setup({ Items: ["Audit", "Deploy"] }, { slots: {} })();
                assert.equal(fragment.block, "element");
                assert.equal(fragment.patchFlag, 128);
                assert.deepEqual(fragment.children.map(node => node.name), ["li", "li"]);
                assert.deepEqual(fragment.children.map(node => node.props.key), ["Audit", "Deploy"]);
                assert.deepEqual(fragment.children.map(node => node.props["data-index"]), [0, 1]);
                assert.deepEqual(fragment.children.map(node => node.children), ["Audit", "Deploy"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorWhileLoop_PreservesTrailingCounterUpdateOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\IndexedWhileLoopRuntime.razor",
            documentText:
            """
            @{
                var index = 0;
            }
            @while (index < Items.Length)
            {
                <li data-index="@index">@Items[index]</li>
                index++;
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/indexed-while-loop-runtime")]
            public partial class IndexedWhileLoopRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string[] Items { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.IndexedWhileLoopRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "while (index", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "let index = 0", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/indexed-while-loop-runtime.mjs",
            observation.ModuleText,
            "official-indexed-while-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/indexed-while-loop-runtime.mjs";

            test("official Razor @while preserves the body-then-update ordering", () => {
                const fragment = component.setup({ Items: ["Audit", "Deploy"] }, { slots: {} })();
                assert.equal(fragment.block, "element");
                assert.equal(fragment.patchFlag, 256);
                assert.deepEqual(fragment.children.map(node => node.props["data-index"]), [0, 1]);
                assert.deepEqual(fragment.children.map(node => node.children), ["Audit", "Deploy"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorDoWhileLoop_EvaluatesBodyBeforeConditionOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\DoWhileLoopRuntime.razor",
            documentText:
            """
            @{
                var index = 0;
            }
            @do
            {
                <span data-index="@index">@index</span>
                index++;
            }
            while (index < Count);
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/do-while-loop-runtime")]
            public partial class DoWhileLoopRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Count { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DoWhileLoopRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "do", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "let index = 0", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/do-while-loop-runtime.mjs",
            observation.ModuleText,
            "official-do-while-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/do-while-loop-runtime.mjs";

            test("official Razor @do executes one iteration before checking Count", () => {
                const zeroCount = component.setup({ Count: 0 }, { slots: {} })();
                assert.equal(zeroCount.block, "element");
                assert.deepEqual(zeroCount.children.map(node => node.props["data-index"]), [0]);
                assert.deepEqual(zeroCount.children.map(node => node.children), [[0]]);

                const repeated = component.setup({ Count: 2 }, { slots: {} })();
                assert.deepEqual(repeated.children.map(node => node.props["data-index"]), [0, 1]);
                assert.deepEqual(repeated.children.map(node => node.children), [[0], [1]]);
            });
            """);
    }
}
