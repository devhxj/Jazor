using Jero.Web.App.Components;
using static ECMAScript.Global;

namespace Jero.Web.App.Analytics;

/// <summary>
/// 分析页图表生命周期边界。ECharts 实例不能跨页面残留；route cleanup 必须先解绑 resize 再 dispose。
/// </summary>
[ECMAScriptModule("components/analytics-chart-browser-actions.js")]
internal static class AnalyticsChartBrowserActions
{
    public const string MainChartId = "jero-analytics-main-chart";
    public const string VisitsDataChartId = "jero-analytics-visits-data";
    public const string VisitsSourceChartId = "jero-analytics-visits-source";
    public const string VisitsSalesChartId = "jero-analytics-visits-sales";

    private static EChartsInstance? mainChart;
    private static EChartsInstance? visitsDataChart;
    private static EChartsInstance? visitsSourceChart;
    private static EChartsInstance? visitsSalesChart;
    private static HandleEventCallback? resizeHandler;

    public static void Mount(string activeTab)
    {
        Destroy();
        mainChart = Create(MainChartId, MainOption(activeTab));
        visitsDataChart = Create(VisitsDataChartId, VisitsDataOption());
        visitsSourceChart = Create(VisitsSourceChartId, VisitsSourceOption());
        visitsSalesChart = Create(VisitsSalesChartId, VisitsSalesOption());
        resizeHandler = HandleResize;
        Window.AddEventListener("resize", resizeHandler, false);
    }

    public static void SwitchMain(string activeTab)
	{
		mainChart?.Dispose();
        mainChart = Create(MainChartId, MainOption(activeTab));
    }

    public static void Resize()
    {
        if (mainChart is not null)
            mainChart.Resize();
        if (visitsDataChart is not null)
            visitsDataChart.Resize();
        if (visitsSourceChart is not null)
            visitsSourceChart.Resize();
        if (visitsSalesChart is not null)
            visitsSalesChart.Resize();
    }

    public static void Destroy()
    {
        if (resizeHandler is not null)
            Window.RemoveEventListener("resize", resizeHandler, false);
        resizeHandler = null;

        if (mainChart is not null)
            mainChart.Dispose();
        if (visitsDataChart is not null)
            visitsDataChart.Dispose();
        if (visitsSourceChart is not null)
            visitsSourceChart.Dispose();
        if (visitsSalesChart is not null)
            visitsSalesChart.Dispose();
        mainChart = null;
        visitsDataChart = null;
        visitsSourceChart = null;
        visitsSalesChart = null;
    }

    private static void HandleResize(EventRef _)
        => Resize();

    private static EChartsInstance Create(string elementId, AnalyticsChartOption option)
    {
        var element = Document.GetElementById(elementId) as HTMLElement;
        if (element is null)
            throw new Exception("分析页图表容器尚未挂载: " + elementId);

        var chart = ECharts.Init(element);
        chart.SetOption(option);
        return chart;
    }

    private static AnalyticsChartOption MainOption(string activeTab)
        => activeTab == "visits" ? VisitsOption() : TrendsOption();

    private static AnalyticsChartOption TrendsOption()
        => new()
        {
            grid = new AnalyticsChartGrid(),
            tooltip = new AnalyticsChartTooltip
            {
                trigger = "axis",
                axisPointer = new AnalyticsChartAxisPointer
                {
                    lineStyle = new AnalyticsChartLineStyle { color = "#019680", width = 1 }
                }
            },
            xAxis = new AnalyticsChartAxis
            {
                type = "category",
                boundaryGap = false,
                axisTick = new ChartToggle { show = false },
                data = HourLabels(),
                splitLine = new AnalyticsChartSplitLine
                {
                    show = true,
                    lineStyle = new AnalyticsChartLineStyle { color = "#e8edf3", type = "solid", width = 1 }
                }
            },
            yAxis = new AnalyticsChartAxis
            {
                type = "value",
                max = 80_000,
                splitNumber = 4,
                axisTick = new ChartToggle { show = false },
                splitArea = new ChartToggle { show = true }
            },
            series =
            [
                new AnalyticsChartSeries
                {
                    type = "line",
                    smooth = true,
                    data = new int[]
                    {
                        111, 2_000, 6_000, 16_000, 33_333, 55_555, 64_000, 33_333, 18_000,
                        36_000, 70_000, 42_444, 23_222, 13_000, 8_000, 4_000, 1_200, 333
                    },
                    areaStyle = new AnalyticsChartAreaStyle { opacity = .24 },
                    itemStyle = new AnalyticsChartItemStyle { color = "#5ab1ef" }
                },
                new AnalyticsChartSeries
                {
                    type = "line",
                    smooth = true,
                    data = new int[]
                    {
                        33, 66, 88, 333, 3_333, 6_200, 20_000, 3_000, 1_200,
                        13_000, 22_000, 11_000, 2_221, 1_201, 390, 198, 60, 30
                    },
                    areaStyle = new AnalyticsChartAreaStyle { opacity = .22 },
                    itemStyle = new AnalyticsChartItemStyle { color = "#019680" }
                }
            ]
        };

    private static AnalyticsChartOption VisitsOption()
        => new()
        {
            grid = new AnalyticsChartGrid(),
            tooltip = new AnalyticsChartTooltip { trigger = "axis" },
            xAxis = new AnalyticsChartAxis
            {
                type = "category",
                data = MonthLabels()
            },
            yAxis = new AnalyticsChartAxis
            {
                type = "value",
                max = 8_000,
                splitNumber = 4
            },
            series =
            [
                new AnalyticsChartSeries
                {
                    type = "bar",
                    barMaxWidth = 80,
                    data = new int[] { 3_000, 2_000, 3_333, 5_000, 3_200, 4_200, 3_200, 2_100, 3_000, 5_100, 6_000, 3_200 },
                    itemStyle = new AnalyticsChartItemStyle { color = "#4f69fd", borderRadius = 4 }
                }
            ]
        };

    private static AnalyticsChartOption VisitsDataOption()
        => new()
        {
            tooltip = new AnalyticsChartTooltip(),
            legend = new AnalyticsChartLegend { bottom = "0", data = ["访问", "趋势"] },
            radar = new AnalyticsChartRadar
            {
                indicator =
                [
                    new AnalyticsChartRadarIndicator { name = "网页" },
                    new AnalyticsChartRadarIndicator { name = "移动端" },
                    new AnalyticsChartRadarIndicator { name = "Ipad" },
                    new AnalyticsChartRadarIndicator { name = "客户端" },
                    new AnalyticsChartRadarIndicator { name = "第三方" },
                    new AnalyticsChartRadarIndicator { name = "其它" }
                ]
            },
            series =
            [
                new AnalyticsChartSeries
                {
                    type = "radar",
                    symbolSize = 0,
                    data = new AnalyticsChartRadarValue[]
                    {
                        new()
                        {
                            name = "访问",
                            value = [90, 50, 86, 40, 50, 20],
                            itemStyle = new AnalyticsChartItemStyle { color = "#b6a2de" }
                        },
                        new()
                        {
                            name = "趋势",
                            value = [70, 75, 70, 76, 20, 85],
                            itemStyle = new AnalyticsChartItemStyle { color = "#5ab1ef" }
                        }
                    },
                    areaStyle = new AnalyticsChartAreaStyle
                    {
                        opacity = .7,
                        shadowColor = "rgba(0,0,0,.2)",
                        shadowOffsetY = 10
                    },
                    itemStyle = new AnalyticsChartItemStyle { borderRadius = 10, borderWidth = 2 }
                }
            ]
        };

    private static AnalyticsChartOption VisitsSourceOption()
        => new()
        {
            tooltip = new AnalyticsChartTooltip { trigger = "item" },
            legend = new AnalyticsChartLegend { bottom = "2%", left = "center" },
            series =
            [
                new AnalyticsChartSeries
                {
                    type = "pie",
                    name = "访问来源",
                    radius = new string[] { "40%", "65%" },
                    avoidLabelOverlap = false,
                    color = ["#5ab1ef", "#b6a2de", "#67e0e3", "#2ec7c9"],
                    data = new AnalyticsChartNamedValue[]
                    {
                        new() { name = "搜索引擎", value = 1_048 },
                        new() { name = "直接访问", value = 735 },
                        new() { name = "邮件营销", value = 580 },
                        new() { name = "联盟广告", value = 484 }
                    },
                    itemStyle = new AnalyticsChartItemStyle { borderRadius = 10, borderWidth = 2 },
                    label = new ChartLabel { show = false, position = "center" },
                    labelLine = new ChartToggle { show = false },
                    emphasis = new ChartEmphasis
                    {
                        label = new ChartLabel
                        {
                            show = true,
                            fontSize = 12,
                            fontWeight = "bold"
                        }
                    }
                }
            ]
        };

    private static AnalyticsChartOption VisitsSalesOption()
        => new()
        {
            tooltip = new AnalyticsChartTooltip { trigger = "item" },
            series =
            [
                new AnalyticsChartSeries
                {
                    type = "pie",
                    name = "商业占比",
                    radius = "80%",
                    roseType = "radius",
                    center = ["50%", "50%"],
                    color = ["#5ab1ef", "#b6a2de", "#67e0e3", "#2ec7c9"],
                    data = new AnalyticsChartNamedValue[]
                    {
                        new() { name = "技术支持", value = 274 },
                        new() { name = "定制", value = 310 },
                        new() { name = "远程", value = 400 },
                        new() { name = "外包", value = 500 }
                    }
                }
            ]
        };

    private static string[] HourLabels()
    {
        var labels = new string[18];
        for (var index = 0; index < labels.Length; index++)
            labels[index] = (index + 6).ToString() + ":00";
        return labels;
    }

    private static string[] MonthLabels()
    {
        var labels = new string[12];
        for (var index = 0; index < labels.Length; index++)
            labels[index] = (index + 1).ToString() + "月";
        return labels;
    }
}
