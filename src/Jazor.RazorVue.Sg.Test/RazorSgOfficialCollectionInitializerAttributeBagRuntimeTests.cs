namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialCollectionInitializerAttributeBagRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorCollectionInitializerAttributeBag_ExpandsDictionaryAddEntriesOnDenoHost()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ReleaseCollectionAttributes.razor",
            documentText:
            """
            @using System.Collections.Generic

            <button @attributes="@(new Dictionary<string, object> { { "data-source", "collection" }, { "data-release", Release } })">@Label</button>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/release-collection-attributes")]
            public partial class ReleaseCollectionAttributes : ComponentBase, IVueComponent
            {
                [Parameter] public string Release { get; set; } = string.Empty;

                [Parameter] public string Label { get; set; } = string.Empty;
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ReleaseCollectionAttributes");

        StringAssert.Contains(observation.GeneratedCSharp, "AddMultipleAttributes", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(observation.ModuleText, "\"data-source\": \"collection\"", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("new Dictionary", StringComparison.Ordinal), observation.ModuleText);

        await RazorSgOfficialDenoRuntimeTestHost.RunModuleTestAsync(
            "components/release-collection-attributes.mjs",
            observation.ModuleText,
            "official-release-collection-attributes-runtime.test.mjs",
            """
            import assert from "node:assert/strict";
            import test from "node:test";

            import component from "./components/release-collection-attributes.mjs";

            test("official Razor collection initializer attributes retain every dictionary entry", () => {
                const button = component.setup({ Release: "2026.08", Label: "Deploy" }, { slots: {} })();
                assert.equal(button.name, "button");
                assert.equal(button.props["data-source"], "collection");
                assert.equal(button.props["data-release"], "2026.08");
                assert.equal(button.children, "Deploy");
            });
            """);
    }
}
