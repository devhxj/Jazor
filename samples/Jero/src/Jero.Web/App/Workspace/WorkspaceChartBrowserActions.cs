using Jero.Web.App.Components;
using static ECMAScript.Global;

namespace Jero.Web.App.Workspace;

/// <summary>
/// Workspace 环图生命周期边界。离页时必须先解绑 resize，再释放 ECharts 实例。
/// </summary>
[ECMAScriptModule("components/workspace-chart-browser-actions.js")]
internal static class WorkspaceChartBrowserActions
{
    public const string ChartId = "jero-workspace-visits-source";

    private static EChartsInstance? chart;
    private static HandleEventCallback? resizeHandler;

    public static void Mount()
    {
        Destroy();

        var element = Document.GetElementById(ChartId) as HTMLElement;
        if (element is null)
            throw new Exception("工作空间访问来源图表容器尚未挂载。");

        chart = ECharts.Init(element);
        chart.SetOption(ChartOption());
        resizeHandler = HandleResize;
        Window.AddEventListener("resize", resizeHandler, false);
    }

    public static void Destroy()
    {
        if (resizeHandler is not null)
            Window.RemoveEventListener("resize", resizeHandler, false);
        resizeHandler = null;

        if (chart is not null)
            chart.Dispose();
        chart = null;
    }

    private static void HandleResize(EventRef _)
        => chart?.Resize();

    private static WorkspaceChartOption ChartOption()
        => new()
        {
            tooltip = new WorkspaceChartTooltip { trigger = "item" },
            legend = new WorkspaceChartLegend { bottom = "2%", left = "center" },
            series =
            [
                new WorkspaceChartSeries
                {
                    type = "pie",
                    name = "访问来源",
                    radius = ["40%", "65%"],
                    avoidLabelOverlap = false,
                    color = ["#5ab1ef", "#b6a2de", "#67e0e3", "#2ec7c9"],
                    data =
                    [
                        new WorkspaceChartNamedValue { name = "搜索引擎", value = 1_048 },
                        new WorkspaceChartNamedValue { name = "直接访问", value = 735 },
                        new WorkspaceChartNamedValue { name = "邮件营销", value = 580 },
                        new WorkspaceChartNamedValue { name = "联盟广告", value = 484 }
                    ],
                    itemStyle = new WorkspaceChartItemStyle { borderRadius = 10, borderWidth = 2 },
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
}
