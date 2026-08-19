namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialForLoopRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorForLoop_LowersIndexedKeyedContentOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/IndexedForLoopRuntime.razor"),
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
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/IndexedWhileLoopRuntime.razor"),
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
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/DoWhileLoopRuntime.razor"),
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

    [TestMethod]
    public async Task BuildComponent_OfficialRazorForLoopWithoutUpdateList_PreservesTrailingBodyMutationOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ForLoopWithoutUpdateRuntime.razor"),
            documentText:
            """
            @for (var index = 0; index < Items.Length;)
            {
                <li data-index="@index">@Items[index]</li>
                index++;
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/for-loop-without-update-runtime")]
            public partial class ForLoopWithoutUpdateRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string[] Items { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ForLoopWithoutUpdateRuntime");

        StringAssert.Contains(observation.GeneratedCSharp, "for (var index", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "for (;", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/for-loop-without-update-runtime.mjs",
            observation.ModuleText,
            "official-for-loop-without-update-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/for-loop-without-update-runtime.mjs";

            test("official Razor @for accepts a body-owned counter update", () => {
                const fragment = component.setup({ Items: ["Audit", "Deploy"] }, { slots: {} })();
                const items = fragment.children.filter(node => node.name === "li");
                assert.deepEqual(items.map(node => node.props["data-index"]), [0, 1]);
                assert.deepEqual(items.map(node => node.children), ["Audit", "Deploy"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorForLoopWithMultipleUpdates_PreservesUpdateOrderOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ForLoopMultipleUpdatesRuntime.razor"),
            documentText:
            """
            @for (int index = 0, ordinal = 10; index < Items.Length; index++, ordinal += 10)
            {
                <li data-index="@index" data-ordinal="@ordinal">@Items[index]</li>
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/for-loop-multiple-updates-runtime")]
            public partial class ForLoopMultipleUpdatesRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string[] Items { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ForLoopMultipleUpdatesRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "ordinal += 10", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/for-loop-multiple-updates-runtime.mjs",
            observation.ModuleText,
            "official-for-loop-multiple-updates-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/for-loop-multiple-updates-runtime.mjs";

            test("official Razor @for emits all update expressions in source order", () => {
                const fragment = component.setup({ Items: ["Audit", "Deploy", "Ship"] }, { slots: {} })();
                const items = fragment.children.filter(node => node.name === "li");
                assert.deepEqual(items.map(node => node.props["data-index"]), [0, 1, 2]);
                assert.deepEqual(items.map(node => node.props["data-ordinal"]), [10, 20, 30]);
                assert.deepEqual(items.map(node => node.children), ["Audit", "Deploy", "Ship"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorWhileLoopWithKey_PreservesKeyedIterationIdentityOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/KeyedWhileLoopRuntime.razor"),
            documentText:
            """
            @{
                var index = 0;
            }
            @while (index < Items.Length)
            {
                <li @key="Items[index]" data-index="@index">@Items[index]</li>
                index++;
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/keyed-while-loop-runtime")]
            public partial class KeyedWhileLoopRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public string[] Items { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.KeyedWhileLoopRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "key: props.Items[index]", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/keyed-while-loop-runtime.mjs",
            observation.ModuleText,
            "official-keyed-while-loop-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/keyed-while-loop-runtime.mjs";

            test("official Razor @while retains authored keys", () => {
                const fragment = component.setup({ Items: ["Audit", "Deploy"] }, { slots: {} })();
                const items = fragment.children.filter(node => node.name === "li");
                assert.deepEqual(items.map(node => node.props.key), ["Audit", "Deploy"]);
                assert.deepEqual(items.map(node => node.children), ["Audit", "Deploy"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorDoWhileWithMultipleRoots_PreservesIterationFragmentsOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/DoWhileMultipleRootsRuntime.razor"),
            documentText:
            """
            @{
                var index = 0;
            }
            @do
            {
                <span data-index="@index">first:@index</span>
                <em data-index="@index">second:@index</em>
                index++;
            }
            while (index < Count);
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/do-while-multiple-roots-runtime")]
            public partial class DoWhileMultipleRootsRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public int Count { get; set; }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.DoWhileMultipleRootsRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "createElementBlock(Fragment", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/do-while-multiple-roots-runtime.mjs",
            observation.ModuleText,
            "official-do-while-multiple-roots-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/do-while-multiple-roots-runtime.mjs";

            test("official Razor @do retains every root in each rendered iteration", () => {
                const fragment = component.setup({ Count: 2 }, { slots: {} })();
                const spans = fragment.children.map(iteration =>
                    iteration.children.find(node => node.name === "span").children);
                const ems = fragment.children.map(iteration =>
                    iteration.children.find(node => node.name === "em").children);
                assert.deepEqual(spans, [["first:", 0], ["first:", 1]]);
                assert.deepEqual(ems, [["second:", 0], ["second:", 1]]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorLoopBranches_PreserveForeachForAndWhileControlFlowOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/LoopBranchesRuntime.razor"),
            documentText:
            """
            <section data-loop="foreach">
                @foreach (var value in Values)
                {
                    if (value < 0)
                    {
                        continue;
                    }

                    <span>@value</span>
                    if (value >= 3)
                    {
                        break;
                    }
                }
            </section>

            <section data-loop="for">
                @for (var index = 0; index < Values.Length; index++)
                {
                    if (Values[index] < 0)
                    {
                        continue;
                    }

                    <em>@Values[index]</em>
                    if (Values[index] >= 3)
                    {
                        break;
                    }
                }
            </section>

            @{
                var whileIndex = 0;
            }
            <section data-loop="while">
                @while (whileIndex < Values.Length)
                {
                    var current = Values[whileIndex];
                    whileIndex++;
                    if (current < 0)
                    {
                        continue;
                    }

                    <strong>@current</strong>
                    if (current >= 3)
                    {
                        break;
                    }
                }
            </section>

            @{
                var doIndex = 0;
            }
            <section data-loop="do">
                @do
                {
                    var current = Values[doIndex];
                    doIndex++;
                    if (current < 0)
                    {
                        continue;
                    }

                    <i>@current</i>
                    if (current >= 3)
                    {
                        break;
                    }
                }
                while (doIndex < Values.Length);
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/loop-branches-runtime")]
            public partial class LoopBranchesRuntime : ComponentBase, IVueComponent
            {
                [Parameter]
                public int[] Values { get; set; } = [];
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LoopBranchesRuntime");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "continue;", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "break;", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "for (let value of", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/loop-branches-runtime.mjs",
            observation.ModuleText,
            "official-loop-branches-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/loop-branches-runtime.mjs";

            function collectText(node, name, values = []) {
                if (Array.isArray(node)) {
                    for (const child of node)
                        collectText(child, name, values);
                    return values;
                }
                if (node === null || typeof node !== "object")
                    return values;
                if (node.name === name)
                    values.push(node.children);
                collectText(node.children, name, values);
                return values;
            }

            test("ordinary loop branches target the generated JavaScript loop", () => {
                const root = component.setup({ Values: [-1, 1, 3, 9] }, { slots: {} })();
                assert.deepEqual(collectText(root, "span"), [[1], [3]]);
                assert.deepEqual(collectText(root, "em"), [[1], [3]]);
                assert.deepEqual(collectText(root, "strong"), [[1], [3]]);
                assert.deepEqual(collectText(root, "i"), [[1], [3]]);
            });
            """);
    }
}
