// Supplies the real session- and platform-scoped data used by the administration overview.
// 为管理首页提供会话与平台两级真实数据：会话来自 /api/auth/session，平台统计来自 /api/overview。
using JazorAdmin.Features.Identity;
using JazorAdmin.Features.Overview;

namespace JazorAdmin;

[ECMAScriptModule("./components/dashboard")]
public partial class DashboardPage : AppComponentBase, IVueContainerComponent
{
    [Parameter]
    public SessionResponse? Session { get; set; }

    [Parameter]
    public OrganizationSummary? SelectedOrganization { get; set; }

    private bool loading = true;
    private string? error;
    private OverviewView? overview;

    protected override void OnParametersSet()
    {
        // 平台统计与路由参数无关；每次参数变化重新拉取，保证多次进入仪表盘的数据新鲜度。
        Load();
    }

    private void Load()
    {
        loading = true;
        error = null;
        ApiClient.GetOverview().Then(outcome =>
        {
            loading = false;
            if (!outcome.Ok || outcome.Data is null)
            {
                error = outcome.Error ?? L("Unable to load the platform overview.", "无法加载平台概览。");
                return;
            }

            overview = ApiClient.ToOverview(outcome.Data);
        });
    }

    private int DailyPeak
    {
        get
        {
            if (overview?.RecentRuns is not { Length: > 0 } runs)
                return 0;

            var peak = 0;
            foreach (var run in runs)
            {
                var total = run.Succeeded + run.Failed;
                if (total > peak)
                    peak = total;
            }

            return peak;
        }
    }

    // 柱高按当日计数相对 7 天峰值归一化；无执行记录时全部为 0，不出现除零。
    private int ToChartHeight(int value)
        => DailyPeak <= 0 ? 0 : value * 100 / DailyPeak;

    private static string ToDayLabel(string date)
        => date.Length > 5 ? date.Substring(5) : date;
}
