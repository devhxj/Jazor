using Jero.Web.Infrastructure.Browser;
using Jero.Web.Pages.Components;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Workspace;

/// <summary>
/// Workspace 环图生命周期边界。离页时必须先解绑 resize，再释放 ECharts 实例。
/// </summary>
[ECMAScriptModule("components/workspace-chart-browser-actions.js")]
internal static class WorkspaceChartBrowserActions
{
    public const string ChartId = "jero-workspace-visits-source";

    private static EChartsInstance? _chart;
    private static HandleEventCallback? _resizeHandler;

    public static void Mount()
    {
        Destroy();

        var element = Document.GetElementById(ChartId) as HTMLElement;
        if (element is null)
            throw new Exception("工作空间访问来源图表容器尚未挂载。");

        _chart = ECharts.Init(element);
        _chart.SetOption(ChartOption());
        _resizeHandler = HandleResize;
        Window.AddEventListener("resize", _resizeHandler, false);
    }

    public static void Destroy()
    {
        if (_resizeHandler is not null)
            Window.RemoveEventListener("resize", _resizeHandler, false);
        _resizeHandler = null;

        if (_chart is not null)
            _chart.Dispose();
        _chart = null;
    }

    private static void HandleResize(EventRef _)
        => _chart?.Resize();

    private static WorkspaceChartOption ChartOption()
        => new()
        {
            Tooltip = new WorkspaceChartTooltip { Trigger = "item" },
            Legend = new WorkspaceChartLegend { Bottom = "2%", Left = "center" },
            Series =
            [
                new WorkspaceChartSeries
                {
                    Type = "pie",
                    Name = "访问来源",
                    Radius = ["40%", "65%"],
                    AvoidLabelOverlap = false,
                    Color = ["#5ab1ef", "#b6a2de", "#67e0e3", "#2ec7c9"],
                    Data =
                    [
                        new WorkspaceChartNamedValue { Name = "搜索引擎", Value = 1_048 },
                        new WorkspaceChartNamedValue { Name = "直接访问", Value = 735 },
                        new WorkspaceChartNamedValue { Name = "邮件营销", Value = 580 },
                        new WorkspaceChartNamedValue { Name = "联盟广告", Value = 484 }
                    ],
                    ItemStyle = new WorkspaceChartItemStyle { BorderRadius = 10, BorderWidth = 2 },
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
}
