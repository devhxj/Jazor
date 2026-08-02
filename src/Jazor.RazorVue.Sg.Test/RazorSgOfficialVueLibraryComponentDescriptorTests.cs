namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueLibraryComponentDescriptorTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryButton_MapsPropEmitAndDefaultSlot()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\TDesignReleaseAction.razor",
            documentText:
            """
            @using Demo.Library

            <TButton Theme="primary" OnClick="QueueRelease">
                <span data-action="deploy">Deploy release</span>
            </TButton>
            """,
            codeBehindSource:
            """
            using Demo.Library;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/t-design-release-action")]
            public partial class TDesignReleaseAction : ComponentBase, IVueComponent
            {
                private int QueueCount { get; set; }

                private void QueueRelease()
                {
                    QueueCount++;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.TDesignReleaseAction",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TButton.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [VueProp(nameof(Theme), Name = "theme")]
                [VueLibraryEmit(nameof(OnClick), Name = "click")]
                [VueSlot(nameof(ChildContent), IsDefault = true)]
                [VueLibraryComponent("tdesign-vue-next", "Button")]
                public sealed class TButton : ComponentBase, IVueLibraryComponent
                {
                    [Parameter] public string Theme { get; set; } = string.Empty;
                    [Parameter] public EventCallback OnClick { get; set; }
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
                """
            });

        StringAssert.Contains(observation.GeneratedCSharp, "OpenComponent<global::Demo.Library.TButton>", StringComparison.Ordinal);
        StringAssert.Contains(observation.GeneratedCSharp, "AddComponentParameter", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { Button } from \"tdesign-vue-next\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "theme: \"primary\"", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onClick: queueRelease", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "default: () =>", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "data-action", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("createRenderContext", StringComparison.Ordinal), observation.ModuleText);
    }
}
