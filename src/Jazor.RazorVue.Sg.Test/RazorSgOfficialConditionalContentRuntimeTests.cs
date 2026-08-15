namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialConditionalContentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalContent_WithSharedImportModule_RetainsEveryRuntimeHelper()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ConditionalCount.razor",
            documentText:
            """
            @using System.Linq

            @if (ShowCount)
            {
                <p data-count="@Values.Where(value => value > 0).Count()">@VisibleValues.Count()</p>
            }
            """,
            codeBehindSource:
            """
            using System.Collections.Generic;
            using System.Linq;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/conditional-count")]
            public partial class ConditionalCount : ComponentBase, IVueComponent
            {
                private bool ShowCount { get; set; } = true;

                private int[] Values { get; } = [1, 2, 3];

                private IEnumerable<int> VisibleValues => Values.Where(value => value > 0);
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ConditionalCount");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "System/Linq/EnumerableModule.js", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "_1cb3ec9a7fb8aaab", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/conditional-count.mjs",
            observation.ModuleText,
            "official-conditional-count-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/conditional-count.mjs";

            test("conditional imported expressions retain every helper from a shared module", () => {
                const vnode = component.setup({}, { slots: {} })();
                assert.equal(vnode.name, "p");
                assert.equal(vnode.props["data-count"], 3);
                assert.deepEqual(vnode.children, [3]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorAdjacentConditionalSiblings_UseDistinctBranchKeysOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ConditionalProfiles.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            <section>
                @if (ShowSecret)
                {
                    <div class="branch-secret" role="status" data-secret="@ShowSecret">
                        <button type="button" @onclick="ActivateProfile">Continue</button>
                    </div>
                }

                @if (ShowProfiles)
                {
                    <div class="branch-profiles" role="group" aria-label="Profile presets" data-profiles="@ShowProfiles">
                        <button type="button">Interactive</button>
                    </div>
                }
            </section>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/conditional-profiles")]
            public partial class ConditionalProfiles : ComponentBase, IVueComponent
            {
                private bool ShowSecret { get; set; } = true;

                private bool ShowProfiles { get; set; }

                private void ActivateProfile()
                {
                    ShowSecret = false;
                    ShowProfiles = true;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ConditionalProfiles");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "__jazor$if_", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/conditional-profiles.mjs",
            observation.ModuleText,
            "official-conditional-profiles-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/conditional-profiles.mjs";

            const findNode = (node, predicate) => {
                if (node && predicate(node)) return node;
                const children = Array.isArray(node?.children) ? node.children : [];
                for (const child of children) {
                    const match = findNode(child, predicate);
                    if (match) return match;
                }
                return undefined;
            };

            const findByClass = (root, className) =>
                findNode(root, node => node?.props?.class === className);

            test("adjacent Razor conditional branches do not share vnode identity", () => {
                const render = component.setup({}, { slots: {} });
                const initial = render();
                const secret = findByClass(initial, "branch-secret");
                assert.ok(secret);
                assert.match(secret.props.key, /^__jazor\$if_/);

                const continueButton = findNode(secret, node => node?.name === "button");
                continueButton.props.onClick();

                const profiles = findByClass(render(), "branch-profiles");
                assert.ok(profiles);
                assert.equal(profiles.props.role, "group");
                assert.equal(profiles.props["aria-label"], "Profile presets");
                assert.match(profiles.props.key, /^__jazor\$if_/);
                assert.notEqual(secret.props.key, profiles.props.key);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorConditionalContent_UsesFragmentOnlyForTheMultiNodeBranchOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseDetailsToggle.razor",
            documentText:
            """
            @using Microsoft.AspNetCore.Components.Web

            @if (ShowDetails)
            {
                <strong data-release="@ReleaseName">@ReleaseName</strong>
                <button type="button" @onclick="ToggleDetails">Hide details</button>
            }
            else
            {
                <span data-release="@ReleaseName">Details hidden</span>
            }
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-details-toggle-runtime")]
            public partial class ReleaseDetailsToggle : ComponentBase, IVueComponent
            {
                private string ReleaseName { get; set; } = "Accounts API";

                private bool ShowDetails { get; set; } = true;

                private void ToggleDetails()
                {
                    ShowDetails = !ShowDetails;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseDetailsToggle");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "Fragment", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "ToggleDetails", StringComparison.Ordinal);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-details-toggle-runtime.mjs",
            observation.ModuleText,
            "official-release-details-toggle-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";
            import { Fragment } from "vue";

            import component from "./components/release-details-toggle-runtime.mjs";

            test("official Razor conditional content creates a fragment only for multiple active nodes", () => {
                const render = component.setup({}, { slots: {} });
                const details = render();
                assert.equal(details.name, Fragment);
                const heading = details.children.find(node => node?.name === "strong");
                const toggle = details.children.find(node => node?.name === "button");
                assert.ok(heading);
                assert.ok(toggle);
                assert.equal(heading.props["data-release"], "Accounts API");
                assert.equal(heading.children, "Accounts API");
                assert.equal(typeof toggle.props.onClick, "function");

                toggle.props.onClick();

                const hidden = render();
                assert.equal(hidden.name, "span");
                assert.equal(hidden.props["data-release"], "Accounts API");
                assert.deepEqual(hidden.children, ["Details hidden"]);
            });
            """);
    }
}
