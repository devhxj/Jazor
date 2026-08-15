namespace ECMAScript.VueDataUi.Test;

[TestClass]
public sealed class VueDataUiCompilerTests
{
    [TestMethod]
    public async Task Convert_TypedXyDataAndStructuredConfig_LowersPlainChartObjects()
    {
        var script = await VueDataUiTestCompiler.ConvertModuleAsync(
            """
            using System.ComponentModel;
            using ECMAScript;
            using ECMAScript.VueDataUi;

            namespace Demo
            {
                [ECMAScript]
                [Description("@#")]
                public sealed record XyChartOptions : Vue.VueProps
                {
                    [Description("@#title")]
                    public VueDataUiChartTitle? Title { get; init; }
                }

                [ECMAScriptModule("charts/xy-data.mjs")]
                public static class XyDataModule
                {
                    public static VueUiXyDatasetItem[] Dataset()
                        =>
                        [
                            new VueUiXyDatasetItem
                            {
                                Name = "Revenue",
                                Series = new double?[] { 12, null, 29 },
                                Type = VueUiXySeriesType.Line,
                                UseArea = true,
                                Smooth = true
                            }
                        ];

                    public static VueUiXyConfig Config()
                        => new()
                        {
                            Responsive = true,
                            Downsample = new VueDataUiDownsampleOptions { Threshold = 500 },
                            ["chart"] = new XyChartOptions { Title = new VueDataUiChartTitle { Text = "Revenue" } }
                        };
                }
            }
            """,
            "XyDataModule");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "name: \"Revenue\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "series: [12, null, 29]", StringComparison.Ordinal);
        StringAssert.Contains(script, "type: \"line\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "useArea: true", StringComparison.Ordinal);
        StringAssert.Contains(script, "responsive: true", StringComparison.Ordinal);
        StringAssert.Contains(script, "downsample: { threshold: 500 }", StringComparison.Ordinal);
        StringAssert.Contains(script, "chart: { title: { text: \"Revenue\" } }", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_TypedTableAndQuickChartUnions_PreserveUpstreamDataShapes()
    {
        var script = await VueDataUiTestCompiler.ConvertModuleAsync(
            """
            using ECMAScript;
            using ECMAScript.VueDataUi;

            namespace Demo
            {
                [ECMAScriptModule("charts/table-data.mjs")]
                public static class TableDataModule
                {
                    public static VueUiTableDataset Table()
                        => new()
                        {
                            Header =
                            [
                                new VueUiTableDatasetHeaderItem { Name = "Month", Type = VueUiTableColumnType.Text },
                                new VueUiTableDatasetHeaderItem { Name = "Revenue", Type = VueUiTableColumnType.Numeric, Sum = true }
                            ],
                            Body =
                            [
                                new VueUiTableDatasetBodyItem { Td = new VueDataUiCellValue[] { "Jan", 12 } },
                                new VueUiTableDatasetBodyItem { Td = new VueDataUiCellValue[] { "Feb", 29 } }
                            ]
                        };

                    public static VueUiQuickChartDataset Quick()
                        => new double?[] { 2, null, 5 };
                }
            }
            """,
            "TableDataModule");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "header: [{ name: \"Month\", type: \"text\" }", StringComparison.Ordinal);
        StringAssert.Contains(script, "td: [\"Jan\", 12]", StringComparison.Ordinal);
        StringAssert.Contains(script, "return [2, null, 5];", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_TypedHeatmapSparklineAndOhlcRows_PreserveChartSpecificShapes()
    {
        var script = await VueDataUiTestCompiler.ConvertModuleAsync(
            """
            using ECMAScript;
            using ECMAScript.VueDataUi;

            namespace Demo
            {
                [ECMAScriptModule("charts/specialized-data.mjs")]
                public static class SpecializedDataModule
                {
                    public static VueUiTableHeatmapDatasetItem[] Heatmap()
                        =>
                        [
                            new VueUiTableHeatmapDatasetItem
                            {
                                Name = "North",
                                Values = new VueDataUiCellValue?[] { "Jan", 42, null },
                                Color = "#0f766e",
                                Shape = VueUiTableHeatmapShape.Diamond
                            }
                        ];

                    public static VueUiTableSparklineDatasetItem[] Sparkline()
                        =>
                        [
                            new VueUiTableSparklineDatasetItem
                            {
                                Name = "Revenue",
                                Values = new double?[] { 12, null, 29 },
                                Color = "#2563eb"
                            }
                        ];

                    public static VueDataUiCellValue[][] Candles()
                        => [VueUiCandlestickData.Ohlc("2026-08-14", 12, 18, 10, 16, 4200)];
                }
            }
            """,
            "SpecializedDataModule");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "name: \"North\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "values: [\"Jan\", 42, null]", StringComparison.Ordinal);
        StringAssert.Contains(script, "shape: \"diamond\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "name: \"Revenue\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "values: [12, null, 29]", StringComparison.Ordinal);
        StringAssert.Contains(script, "return [[\"2026-08-14\", 12, 18, 10, 16, 4200]];", StringComparison.Ordinal);
    }

    [TestMethod]
    public async Task Convert_ExtendedCatalogDataShapes_PreservesPositionalAndDictionaryInputs()
    {
        var script = await VueDataUiTestCompiler.ConvertModuleAsync(
            """
            using ECMAScript;
            using ECMAScript.VueDataUi;

            namespace Demo
            {
                [ECMAScriptModule("charts/extended-data.mjs")]
                public static class ExtendedDataModule
                {
                    public static VueDataUiCellValue[][] AgePyramid()
                        => [VueUiAgePyramidData.Row("2026", 1, 42, null)];

                    public static VueDataUiCellValue[][] Flow()
                        => [VueUiFlowData.Link("Source", "Target", 12)];

                    public static VueUiWorldDataset World()
                        => new()
                        {
                            ["CN"] = new VueUiWorldDatasetItem
                            {
                                Value = 42,
                                Category = "Active",
                                Color = "#0f766e"
                            }
                        };

                    public static VueUiRatingDataset Rating()
                        => new()
                        {
                            Rating = new VueUiRatingDatasetDetailed { ["quality"] = 4.5 }
                        };
                }
            }
            """,
            "ExtendedDataModule");

        Assert.IsNotNull(script);
        StringAssert.Contains(script, "return [[\"2026\", 1, 42, null]];", StringComparison.Ordinal);
        StringAssert.Contains(script, "return [[\"Source\", \"Target\", 12]];", StringComparison.Ordinal);
        StringAssert.Contains(script, "return { CN:", StringComparison.Ordinal);
        StringAssert.Contains(script, "value: 42", StringComparison.Ordinal);
        StringAssert.Contains(script, "category: \"Active\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "color: \"#0f766e\"", StringComparison.Ordinal);
        StringAssert.Contains(script, "rating: { quality: 4.5 }", StringComparison.Ordinal);
    }
}
