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
                    public VdChartTitle? Title { get; init; }
                }

                [ECMAScriptModule("charts/xy-data.mjs")]
                public static class XyDataModule
                {
                    public static VdXyDatasetItem[] Dataset()
                        =>
                        [
                            new VdXyDatasetItem
                            {
                                Name = "Revenue",
                                Series = new double?[] { 12, null, 29 },
                                Type = VdXySeriesType.Line,
                                UseArea = true,
                                Smooth = true
                            }
                        ];

                    public static VdXyConfig Config()
                        => new()
                        {
                            Responsive = true,
                            Downsample = new VdDownsampleOptions { Threshold = 500 },
                            ["chart"] = new XyChartOptions { Title = new VdChartTitle { Text = "Revenue" } }
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
                    public static VdTableDataset Table()
                        => new()
                        {
                            Header =
                            [
                                new VdTableDatasetHeaderItem { Name = "Month", Type = VdTableColumnType.Text },
                                new VdTableDatasetHeaderItem { Name = "Revenue", Type = VdTableColumnType.Numeric, Sum = true }
                            ],
                            Body =
                            [
                                new VdTableDatasetBodyItem { Td = new VdCellValue[] { "Jan", 12 } },
                                new VdTableDatasetBodyItem { Td = new VdCellValue[] { "Feb", 29 } }
                            ]
                        };

                    public static VdQuickChartDataset Quick()
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
                    public static VdTableHeatmapDatasetItem[] Heatmap()
                        =>
                        [
                            new VdTableHeatmapDatasetItem
                            {
                                Name = "North",
                                Values = new VdCellValue?[] { "Jan", 42, null },
                                Color = "#0f766e",
                                Shape = VdTableHeatmapShape.Diamond
                            }
                        ];

                    public static VdTableSparklineDatasetItem[] Sparkline()
                        =>
                        [
                            new VdTableSparklineDatasetItem
                            {
                                Name = "Revenue",
                                Values = new double?[] { 12, null, 29 },
                                Color = "#2563eb"
                            }
                        ];

                    public static VdCellValue[][] Candles()
                        => [VdCandlestickData.Ohlc("2026-08-14", 12, 18, 10, 16, 4200)];
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
                    public static VdCellValue[][] AgePyramid()
                        => [VdAgePyramidData.Row("2026", 1, 42, null)];

                    public static VdCellValue[][] Flow()
                        => [VdFlowData.Link("Source", "Target", 12)];

                    public static VdWorldDataset World()
                        => new()
                        {
                            ["CN"] = new VdWorldDatasetItem
                            {
                                Value = 42,
                                Category = "Active",
                                Color = "#0f766e"
                            }
                        };

                    public static VdRatingDataset Rating()
                        => new()
                        {
                            Rating = new VdRatingDatasetDetailed { ["quality"] = 4.5 }
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
