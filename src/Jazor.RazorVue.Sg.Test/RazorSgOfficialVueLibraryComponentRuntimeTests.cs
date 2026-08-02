namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueLibraryComponentRuntimeTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorTDesignComponent_UsesNamedLibraryImport()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TDesignButtonUsage.razor",
            documentText:
            """
            @using Demo.Library

            <TButton Status="@Status" />
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-button-usage")]
            public partial class TDesignButtonUsage : ComponentBase, IVueComponent
            {
                [Parameter]
                public string Status { get; set; } = "ready";
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignButtonUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TButton.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [VueLibraryComponent("tdesign-vue-next", "Button")]
                public sealed class TButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter]
                    public string Status { get; set; } = string.Empty;
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { Button } from \"tdesign-vue-next\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "status: props.status", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "h(Button, { status: props.status })", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("createRenderContext", StringComparison.Ordinal), observation.ModuleText);
    }
}
