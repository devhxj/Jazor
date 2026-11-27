using Jero.Web.Infrastructure.Browser;
using Jero.Web.Pages.Components;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Analytics;

/// <summary>
/// 管理分析页的 ECharts 图表。离开页面时先移除窗口大小监听，再释放图表，避免回调访问已销毁的实例。
/// </summary>
[ECMAScriptModule("components/analytics-chart-browser-actions.js")]
internal static class AnalyticsChartBrowserActions
{
    public const string MainChartId = "jero-analytics-main-chart";
    public const string VisitsDataChartId = "jero-analytics-visits-data";
    public const string VisitsSourceChartId = "jero-analytics-visits-source";
    public const string VisitsSalesChartId = "jero-analytics-visits-sales";

    private static EChartsInstance? _mainChart;
    private static EChartsInstance? _visitsDataChart;
    private static EChartsInstance? _visitsSourceChart;
    private static EChartsInstance? _visitsSalesChart;
    private static HandleEventCallback? _resizeHandler;

    public static void Mount(string activeTab)
    {
        Destroy();
        _mainChart = Create(MainChartId, MainOption(activeTab));
        _visitsDataChart = Create(VisitsDataChartId, VisitsDataOption());
        _visitsSourceChart = Create(VisitsSourceChartId, VisitsSourceOption());
        _visitsSalesChart = Create(VisitsSalesChartId, VisitsSalesOption());
        _resizeHandler = HandleResize;
        Window.AddEventListener("resize", _resizeHandler, false);
    }

    public static void SwitchMain(string activeTab)
    {
        _mainChart?.Dispose();
        _mainChart = Create(MainChartId, MainOption(activeTab));
    }

    public static void Resize()
    {
        if (_mainChart is not null)
            _mainChart.Resize();
        if (_visitsDataChart is not null)
            _visitsDataChart.Resize();
        if (_visitsSourceChart is not null)
            _visitsSourceChart.Resize();
        if (_visitsSalesChart is not null)
            _visitsSalesChart.Resize();
    }

    public static void Destroy()
    {
        if (_resizeHandler is not null)
            Window.RemoveEventListener("resize", _resizeHandler, false);
        _resizeHandler = null;

        if (_mainChart is not null)
            _mainChart.Dispose();
        if (_visitsDataChart is not null)
            _visitsDataChart.Dispose();
        if (_visitsSourceChart is not null)
            _visitsSourceChart.Dispose();
        if (_visitsSalesChart is not null)
            _visitsSalesChart.Dispose();
        _mainChart = null;
        _visitsDataChart = null;
        _visitsSourceChart = null;
        _visitsSalesChart = null;
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
            Grid = new AnalyticsChartGrid(),
            Tooltip = new AnalyticsChartTooltip
            {
                Trigger = "axis",
                AxisPointer = new AnalyticsChartAxisPointer
                {
                    LineStyle = new AnalyticsChartLineStyle { Color = "#019680", Width = 1 }
                }
            },
            XAxis = new AnalyticsChartAxis
            {
                Type = "category",
                BoundaryGap = false,
                AxisTick = new ChartToggle { Show = false },
                Data = HourLabels(),
                SplitLine = new AnalyticsChartSplitLine
                {
                    Show = true,
                    LineStyle = new AnalyticsChartLineStyle { Color = "#e8edf3", Type = "solid", Width = 1 }
                }
            },
            YAxis = new AnalyticsChartAxis
            {
                Type = "value",
                Max = 80_000,
                SplitNumber = 4,
                AxisTick = new ChartToggle { Show = false },
                SplitArea = new ChartToggle { Show = true }
            },
            Series =
            [
                new AnalyticsChartSeries
                {
                    Type = "line",
                    Smooth = true,
                    Data = new int[]
                    {
                        111, 2_000, 6_000, 16_000, 33_333, 55_555, 64_000, 33_333, 18_000,
                        36_000, 70_000, 42_444, 23_222, 13_000, 8_000, 4_000, 1_200, 333
                    },
                    AreaStyle = new AnalyticsChartAreaStyle { Opacity = .24 },
                    ItemStyle = new AnalyticsChartItemStyle { Color = "#5ab1ef" }
                },
                new AnalyticsChartSeries
                {
                    Type = "line",
                    Smooth = true,
                    Data = new int[]
                    {
                        33, 66, 88, 333, 3_333, 6_200, 20_000, 3_000, 1_200,
                        13_000, 22_000, 11_000, 2_221, 1_201, 390, 198, 60, 30
                    },
                    AreaStyle = new AnalyticsChartAreaStyle { Opacity = .22 },
                    ItemStyle = new AnalyticsChartItemStyle { Color = "#019680" }
                }
            ]
        };

    private static AnalyticsChartOption VisitsOption()
        => new()
        {
            Grid = new AnalyticsChartGrid(),
            Tooltip = new AnalyticsChartTooltip { Trigger = "axis" },
            XAxis = new AnalyticsChartAxis
            {
                Type = "category",
                Data = MonthLabels()
            },
            YAxis = new AnalyticsChartAxis
            {
                Type = "value",
                Max = 8_000,
                SplitNumber = 4
            },
            Series =
            [
                new AnalyticsChartSeries
                {
                    Type = "bar",
                    BarMaxWidth = 80,
                    Data = new int[] { 3_000, 2_000, 3_333, 5_000, 3_200, 4_200, 3_200, 2_100, 3_000, 5_100, 6_000, 3_200 },
                    ItemStyle = new AnalyticsChartItemStyle { Color = "#4f69fd", BorderRadius = 4 }
                }
            ]
        };

    private static AnalyticsChartOption VisitsDataOption()
        => new()
        {
            Tooltip = new AnalyticsChartTooltip(),
            Legend = new AnalyticsChartLegend { Bottom = "0", Data = ["访问", "趋势"] },
            Radar = new AnalyticsChartRadar
            {
                Indicator =
                [
                    new AnalyticsChartRadarIndicator { Name = "网页" },
                    new AnalyticsChartRadarIndicator { Name = "移动端" },
                    new AnalyticsChartRadarIndicator { Name = "Ipad" },
                    new AnalyticsChartRadarIndicator { Name = "客户端" },
                    new AnalyticsChartRadarIndicator { Name = "第三方" },
                    new AnalyticsChartRadarIndicator { Name = "其它" }
                ]
            },
            Series =
            [
                new AnalyticsChartSeries
                {
                    Type = "radar",
                    SymbolSize = 0,
                    Data = new AnalyticsChartRadarValue[]
                    {
                        new()
                        {
                            Name = "访问",
                            Value = [90, 50, 86, 40, 50, 20],
                            ItemStyle = new AnalyticsChartItemStyle { Color = "#b6a2de" }
                        },
                        new()
                        {
                            Name = "趋势",
                            Value = [70, 75, 70, 76, 20, 85],
                            ItemStyle = new AnalyticsChartItemStyle { Color = "#5ab1ef" }
                        }
                    },
                    AreaStyle = new AnalyticsChartAreaStyle
                    {
                        Opacity = .7,
                        ShadowColor = "rgba(0,0,0,.2)",
                        ShadowOffsetY = 10
                    },
                    ItemStyle = new AnalyticsChartItemStyle { BorderRadius = 10, BorderWidth = 2 }
                }
            ]
        };

    private static AnalyticsChartOption VisitsSourceOption()
        => new()
        {
            Tooltip = new AnalyticsChartTooltip { Trigger = "item" },
            Legend = new AnalyticsChartLegend { Bottom = "2%", Left = "center" },
            Series =
            [
                new AnalyticsChartSeries
                {
                    Type = "pie",
                    Name = "访问来源",
                    Radius = new string[] { "40%", "65%" },
                    AvoidLabelOverlap = false,
                    Color = ["#5ab1ef", "#b6a2de", "#67e0e3", "#2ec7c9"],
                    Data = new AnalyticsChartNamedValue[]
                    {
                        new() { Name = "搜索引擎", Value = 1_048 },
                        new() { Name = "直接访问", Value = 735 },
                        new() { Name = "邮件营销", Value = 580 },
                        new() { Name = "联盟广告", Value = 484 }
                    },
                    ItemStyle = new AnalyticsChartItemStyle { BorderRadius = 10, BorderWidth = 2 },
                    Label = new ChartLabel { Show = false, Position = "center" },
                    LabelLine = new ChartToggle { Show = false },
                    Emphasis = new ChartEmphasis
                    {
                        Label = new ChartLabel
                        {
                            Show = true,
                            FontSize = 12,
                            FontWeight = "bold"
                        }
                    }
                }
            ]
        };

    private static AnalyticsChartOption VisitsSalesOption()
        => new()
        {
            Tooltip = new AnalyticsChartTooltip { Trigger = "item" },
            Series =
            [
                new AnalyticsChartSeries
                {
                    Type = "pie",
                    Name = "商业占比",
                    Radius = "80%",
                    RoseType = "radius",
                    Center = ["50%", "50%"],
                    Color = ["#5ab1ef", "#b6a2de", "#67e0e3", "#2ec7c9"],
                    Data = new AnalyticsChartNamedValue[]
                    {
                        new() { Name = "技术支持", Value = 274 },
                        new() { Name = "定制", Value = 310 },
                        new() { Name = "远程", Value = 400 },
                        new() { Name = "外包", Value = 500 }
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
