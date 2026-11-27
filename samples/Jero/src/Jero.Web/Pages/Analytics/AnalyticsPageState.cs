namespace Jero.Web.Pages.Analytics;

[ECMAScriptModule("components/analytics-page-state.js")]
public static class AnalyticsPageModule
{
    public static AnalyticsPageState Create() => new();

    public sealed class AnalyticsPageState : global::Jero.Web.Composition.IPageState<AnalyticsPageProps>
    {
        private void MountCharts(string tab) => AnalyticsChartBrowserActions.Mount(tab);
        private void SwitchChart(string tab) => AnalyticsChartBrowserActions.SwitchMain(tab);
        private void DestroyCharts() => AnalyticsChartBrowserActions.Destroy();

        public void Initialize() { }
        public AnalyticsPageProps CreateProps() => new()
        {
            MountCharts = MountCharts,
            SwitchChart = SwitchChart,
            DestroyCharts = DestroyCharts,
        };
    }
}

public sealed record AnalyticsPageProps : VueProps
{
    public required Action<string> MountCharts { get; init; }
    public required Action<string> SwitchChart { get; init; }
    public required Action DestroyCharts { get; init; }
}
