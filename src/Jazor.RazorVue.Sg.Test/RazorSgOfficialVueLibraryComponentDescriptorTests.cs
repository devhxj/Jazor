namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgOfficialVueLibraryComponentDescriptorTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorLibraryComponents_LoadsDistinctSortedStyles()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\LibraryStyleUsage.razor",
            documentText:
            """
            @using Demo.Library

            <TPanel><TButton /></TPanel>
            """,
            codeBehindSource:
            """
            namespace Demo.Pages;

            [ECMAScriptModule("./components/library-style-usage")]
            public partial class LibraryStyleUsage : ComponentBase, IVueComponent
            {
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.LibraryStyleUsage",
            supportingSources: new Dictionary<string, string>
            {
                ["Library/TPanel.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [VueLibraryComponent("tdesign-vue-next", "Panel", StyleUrls = new[] { " https://cdn.example.test/z.css ", "https://cdn.example.test/a.css" })]
                public sealed class TPanel : ComponentBase
                {
                    [Parameter] public RenderFragment? ChildContent { get; set; }
                }
                """,
                ["Library/TButton.cs"] =
                """
                using ECMAScript.VueContract;

                namespace Demo.Library;

                [VueLibraryComponent("tdesign-vue-next", "Button", StyleUrls = new[] { "https://cdn.example.test/a.css" })]
                public sealed class TButton : ComponentBase
                {
                }
                """
            });

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { ensureLibraryStyles } from \"@jazor/vue-runtime/library-styles.mjs\";",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "ensureLibraryStyles([\"https://cdn.example.test/a.css\", \"https://cdn.example.test/z.css\"]);",
            StringComparison.Ordinal);
        Assert.AreEqual(
            1,
            observation.ModuleText.Split("https://cdn.example.test/a.css", StringSplitOptions.None).Length - 1,
            observation.ModuleText);
    }

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

                [VueLibraryComponent("tdesign-vue-next", "Button")]
                public sealed class TButton : ComponentBase
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
