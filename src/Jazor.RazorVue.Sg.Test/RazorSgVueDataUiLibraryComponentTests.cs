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

    [TestMethod]
    public async Task BuildComponent_OfficialRazorVueDataUiExtendedCatalog_UsesTypedAndOptionalDescriptors()
    {
        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: @"D:\repo\Demo\Pages\ExtendedCharts.razor",
            documentText:
            """
            @using ECMAScript.VueDataUi

            <VueUiFlow Dataset="@FlowDataset" Config="@FlowConfig" />
            <VueUiAgePyramid Dataset="@AgePyramidDataset" Config="@AgePyramidConfig" />
            <VueUiWorld Config="@WorldConfig" />
            <VueUiIcon Name="@VueUiIconName.ArrowLeft" Size="18" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueDataUi;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/extended-charts")]
            public partial class ExtendedCharts : ComponentBase, IVueComponent
            {
                private VueDataUiCellValue[][] FlowDataset { get; } =
                [
                    VueUiFlowData.Link("Source", "Target", 12)
                ];

                private VueDataUiCellValue[][] AgePyramidDataset { get; } =
                [
                    VueUiAgePyramidData.Row("2026", 1, 42, null)
                ];

                private VueUiFlowConfig FlowConfig { get; } = new() { Responsive = true };

                private VueUiAgePyramidConfig AgePyramidConfig { get; } = new() { Responsive = true };

                private VueUiWorldConfig WorldConfig { get; } = new() { Theme = VueDataUiTheme.Light };
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.ExtendedCharts");

        RazorSgOfficialAuthoringTestHost.AssertDirectRenderModule(observation.ModuleText);
        StringAssert.Contains(
            observation.ModuleText,
            "import { VueUiFlow } from \"vue-data-ui/vue-ui-flow\";",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "import { VueUiAgePyramid } from \"vue-data-ui/vue-ui-age-pyramid\";",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "import { VueUiWorld } from \"vue-data-ui/vue-ui-world\";",
            StringComparison.Ordinal);
        StringAssert.Contains(
            observation.ModuleText,
            "import { VueUiIcon } from \"vue-data-ui/vue-ui-icon\";",
            StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "dataset: state.FlowDataset", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "dataset: state.AgePyramidDataset", StringComparison.Ordinal);
        StringAssert.Contains(observation.ModuleText, "name: \"arrowLeft\"", StringComparison.Ordinal);
        Assert.IsFalse(observation.ModuleText.Contains("from \"vue-data-ui\"", StringComparison.Ordinal), observation.ModuleText);
    }
}
