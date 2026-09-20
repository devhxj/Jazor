using System.Reflection;
using ECMAScript;
using ECMAScript.VueDataUi;

namespace Jazor.RazorVue.Sg.Test;

[TestClass]
public sealed class RazorSgVueDataUiLibraryComponentTests
{
    [TestMethod]
    public async Task BuildComponent_OfficialRazorVueDataUiDonut_UsesPerChartImportAndInheritedParameters()
    {
        var descriptor = typeof(VdDonut).GetCustomAttribute<ECMAScriptAttribute>();
        Assert.IsNotNull(descriptor);
        Assert.AreEqual("vue-data-ui/vue-ui-donut", descriptor!.Import);
        Assert.AreEqual("VueUiDonut", typeof(VdDonut).GetCustomAttribute<ECMAScriptNameAttribute>()?.Name);

        var observation = await RazorSgOfficialAuthoringTestHost.BuildComponentAsync(
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/RevenueDonut.razor"),
            documentText:
            """
            @using ECMAScript.VueDataUi

            <VdDonut Dataset="@DonutDataset" Config="@DonutConfig" OnSelectLegend="HandleLegend" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueDataUi;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/revenue-donut")]
            public partial class RevenueDonut : ComponentBase, IVueComponent
            {
                private VdDonutDatasetItem[] DonutDataset { get; } =
                [
                    new VdDonutDatasetItem { Name = "Revenue", Values = new double[] { 48, 52 }, Color = "#0f766e" }
                ];

                private VdDonutConfig DonutConfig { get; } = new()
                {
                    Responsive = true
                };

                private double SelectedLegendValue { get; set; }

                private void HandleLegend(VdDonutLegendItem[] legend)
                {
                    SelectedLegendValue = legend[0].Value;
                }
            }
            """,
            rootNamespace: "Demo.Pages",
            componentMetadataName: "Demo.Pages.RevenueDonut");

        StringAssert.Contains(observation.GeneratedCSharp, "OpenComponent<global::ECMAScript.VueDataUi.VdDonut>", StringComparison.Ordinal);
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
            documentPath: RazorSgTestHost.GetTestDocumentPath("Pages/ExtendedCharts.razor"),
            documentText:
            """
            @using ECMAScript.VueDataUi

            <VdFlow Dataset="@FlowDataset" Config="@FlowConfig" />
            <VdAgePyramid Dataset="@AgePyramidDataset" Config="@AgePyramidConfig" />
            <VdWorld Config="@WorldConfig" />
            <VdIcon Name="@VdIconName.ArrowLeft" Size="18" />
            """,
            codeBehindSource:
            """
            using ECMAScript.VueDataUi;

            namespace Demo.Pages;

            [ECMAScriptModule("./components/extended-charts")]
            public partial class ExtendedCharts : ComponentBase, IVueComponent
            {
                private VdCellValue[][] FlowDataset { get; } =
                [
                    VdFlowData.Link("Source", "Target", 12)
                ];

                private VdCellValue[][] AgePyramidDataset { get; } =
                [
                    VdAgePyramidData.Row("2026", 1, 42, null)
                ];

                private VdFlowConfig FlowConfig { get; } = new() { Responsive = true };

                private VdAgePyramidConfig AgePyramidConfig { get; } = new() { Responsive = true };

                private VdWorldConfig WorldConfig { get; } = new() { Theme = VdTheme.Light };
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
