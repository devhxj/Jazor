namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialInheritedSlotNameRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorDerivedLayout_UsesNewMemberNameForInheritedParameter()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ReleaseLayout.razor"),
            documentText:
            """
            @inherits Demo.Components.HeaderLayoutBase

            <section data-layout="release">
                @Header
            </section>
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public abstract class HeaderLayoutBase : ComponentBase
                {
                    [Parameter] public RenderFragment? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/release-layout-inherited-slot-runtime")]
                public partial class ReleaseLayout : HeaderLayoutBase, IVueComponent
                {
                    [Parameter]
                    [ECMAScriptName("release-header")]
                    public new RenderFragment? Header { get; set; }
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseLayout");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "slots[\"release-header\"]", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("slots.header", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-layout-inherited-slot-runtime.mjs",
            observation.ModuleText,
            "official-inherited-slot-descriptor-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-layout-inherited-slot-runtime.mjs";

            test("official Razor derived layout resolves the new slot member name", () => {
                const render = component.setup({}, {
                    slots: {
                        "release-header": () => ["Release queue"]
                    }
                });
                const root = render();
                assert.equal(root.name, "section");
                assert.equal(root.props["data-layout"], "release");
                assert.deepEqual(root.children, ["Release queue"]);
            });
            """);
    }

    [TestMethod]
    public async Task BuildComponent_OfficialRazorDerivedLayout_InheritsBaseSlotNameWhenNotHidden()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/AuditLayout.razor"),
            documentText:
            """
            @inherits Demo.Components.HeaderLayoutBase

            <section data-layout="audit">
                @Header
            </section>
            """,
            codeBehindSource:
            """
            using Demo.Components;
            using ECMAScript.VueContract;

            namespace Demo.Components
            {
                public abstract class HeaderLayoutBase : ComponentBase
                {
                    [ECMAScriptName("base-header")]
                    [Parameter] public RenderFragment? Header { get; set; }
                }
            }

            namespace Demo.Pages
            {
                [ECMAScriptModule("./components/audit-layout-inherited-slot-runtime")]
                public partial class AuditLayout : HeaderLayoutBase, IVueComponent
                {
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.AuditLayout");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "slots[\"base-header\"]", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("slots.header", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/audit-layout-inherited-slot-runtime.mjs",
            observation.ModuleText,
            "official-inherited-base-slot-descriptor-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/audit-layout-inherited-slot-runtime.mjs";

            test("official Razor derived layout inherits its base slot member name", () => {
                const render = component.setup({}, {
                    slots: {
                        "base-header": () => ["Audit trail"]
                    }
                });
                const root = render();
                assert.equal(root.name, "section");
                assert.equal(root.props["data-layout"], "audit");
                assert.deepEqual(root.children, ["Audit trail"]);
            });
            """);
    }
}
