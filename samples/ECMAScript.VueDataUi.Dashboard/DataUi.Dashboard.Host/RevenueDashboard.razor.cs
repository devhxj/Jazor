using ECMAScript;
using ECMAScript.VueDataUi;
using Microsoft.AspNetCore.Components;
using static ECMAScript.Vue;

namespace DataUi.Dashboard.Host;

/// <summary>
/// Dashboard sample keeps every chart in a bounded container. Responsive charts read their
/// parent dimensions, so height is part of the component contract rather than page decoration.
/// </summary>
[ECMAScriptModule("dashboard/revenue")]
public partial class RevenueDashboard : ComponentBase, IVueComponent
{
    private readonly VueUiDonutDatasetItem[] revenue =
    [
        new() { Name = "Subscription", Values = [68], Color = "#0f766e" },
        new() { Name = "Usage", Values = [32], Color = "#2563eb" }
    ];

    private readonly VueUiDonutConfig donutConfig = new()
    {
        Responsive = true,
        UseCssAnimation = true,
        CustomPalette = ["#0f766e", "#2563eb"]
    };

    private readonly VueUiGaugeDataset conversion = new()
    {
        Base = 100,
        Value = 74,
        Series =
        [
            new() { From = 0, To = 60, Color = "#dc2626", Name = "Below target" },
            new() { From = 60, To = 80, Color = "#d97706", Name = "Watch" },
            new() { From = 80, To = 100, Color = "#15803d", Name = "Target" }
        ]
    };

    private readonly VueUiGaugeConfig gaugeConfig = new()
    {
        Responsive = true,
        Theme = VueDataUiTheme.Light
    };

    private readonly VueUiSparklineDatasetItem[] trend =
    [
        new() { Period = "Mon", Value = 54 },
        new() { Period = "Tue", Value = 61 },
        new() { Period = "Wed", Value = 57 },
        new() { Period = "Thu", Value = 69 },
        new() { Period = "Fri", Value = 74 }
    ];

    private readonly VueUiSparklineConfig trendConfig = new()
    {
        Responsive = true,
        Type = VueUiSparklineType.Line,
        UseCssAnimation = true
    };
}
