using System.Reflection;
using ECMAScript.VueDataUi;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgVueDataUiLibraryComponentTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorVueDataUiDonut_UsesPerChartImportAndInheritedParameters()
    {
        var descriptor = typeof(VueUiDonut).GetCustomAttribute<ECMAScript.VueContract.VueLibraryComponentAttribute>();
        Assert.IsNotNull(descriptor);
        Assert.AreEqual("vue-data-ui/vue-ui-donut", descriptor!.ImportSpecifier);

        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\RevenueDonut.razor",
            documentText:
            """
            @using ECMAScript.VueDataUi

            <VueUiDonut Dataset="@DonutDataset" Config="@DonutConfig" OnSelectLegend="HandleLegend" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueDataUi;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/revenue-donut")]
            public partial class RevenueDonut : ComponentBase, IVueComponent
            {
                private VueUiDonutDatasetItem[] DonutDataset { get; } =
                [
                    new VueUiDonutDatasetItem { Name = "Revenue", Values = new double[] { 48, 52 }, Color = "#0f766e" }
                ];

                private VueUiDonutConfig DonutConfig { get; } = new()
                {
                    Responsive = true
                };

                private double SelectedLegendValue { get; set; }

                private void HandleLegend(VueUiDonutLegendItem[] legend)
                {
                    SelectedLegendValue = legend[0].Value;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.RevenueDonut");

        StringAssert.Contains(observation.GeneratedCSharp, "OpenComponent<global::ECMAScript.VueDataUi.VueUiDonut>", StringComparison.Ordinal);
        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { VueUiDonut } from \"vue-data-ui/vue-ui-donut\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "dataset: state.DonutDataset", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "config: state.DonutConfig", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "onSelectLegend: HandleLegend", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("from \"vue-data-ui\"", StringComparison.Ordinal), observation.ModuleText);
    }
}
