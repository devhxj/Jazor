// Supplies the real session- and platform-scoped data used by the administration overview.
// 为管理首页提供会话与平台两级真实数据：会话来自 /api/auth/session，平台统计来自 /api/overview。
using ECMAScript.TDesign;
using ECMAScript.VueDataUi;
using JazorAdmin.Features.Identity;
using JazorAdmin.Features.Overview;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

// 行类型保持命名空间级：嵌套私有类型会让 Razor SG 为泛型组件生成未闭合的 OpenComponent<TComponent> 形状。
public sealed record DashboardRoleCell(string Code);

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

    // KPI 数字卡走 VueUiKpi；Responsive 只解析宽度，图表容器必须提供确定高度。
    private static readonly VueUiKpiConfig ApplicationKpiConfig = new()
    {
        Title = "Active applications",
        UseAnimation = true
    };

    private static readonly VueUiKpiConfig SignInKpiConfig = new()
    {
        Title = "Sign-ins (7d)",
        UseAnimation = true
    };

    private static readonly VueUiKpiConfig TokenKpiConfig = new()
    {
        Title = "Token issuances (7d)",
        UseAnimation = true
    };

    private static readonly VueUiKpiConfig AuditKpiConfig = new()
    {
        Title = "Audit events (7d)",
        UseAnimation = true
    };

    private static readonly VueUiVerticalBarConfig SignInTrendConfig = new()
    {
        Responsive = true
    };

    private static readonly VueUiDonutConfig DistributionConfig = new()
    {
        Responsive = true
    };

    // 登录趋势直接来自审计中的 authorization_code 签发，而不是把 Quartz 调度记录误作用户活动。
    // Sign-in trend is derived from audited authorization-code issuance, not Quartz task activity.
    private VueUiVerticalBarDatasetItem[] SignInTrendItems
    {
        get
        {
            if (overview?.RecentAudit is not { Length: > 0 } audit)
                return [];

            var items = new VueUiVerticalBarDatasetItem[audit.Length];
            for (var index = 0; index < audit.Length; index++)
            {
                var day = audit[index];
                items[index] = new VueUiVerticalBarDatasetItem
                {
                    Name = ToDayLabel(day.Date),
                    Value = day.SignIns,
                    Color = "#0052d9"
                };
            }

            return items;
        }
    }

    // 平台运营库存：账号、OpenID 应用和当前有效令牌。
    private VueUiDonutDatasetItem[] DistributionItems =>
    [
        new() { Name = "Accounts", Values = [overview?.Accounts ?? 0], Color = "#0052d9" },
        new() { Name = "OpenID applications", Values = [overview?.Applications ?? 0], Color = "#00a6a6" },
        new() { Name = "Tokens", Values = [overview?.Tokens ?? 0], Color = "#c9cdd4" }
    ];

    private TPrimaryTableCol<OrganizationSummary>[] OrganizationColumns =>
    [
        new() { ColKey = "DisplayName", Title = (TPrimaryTableColTitle<OrganizationSummary>)L("Organization", "组织") },
        new() { Title = (TPrimaryTableColTitle<OrganizationSummary>)L("Code", "编码"), Cell = (TPrimaryTableColCell<OrganizationSummary>)((RenderFragment<TPrimaryTableCellParams<OrganizationSummary>>)(context => builder =>
            {
        builder.OpenElement(0, "code");
        builder.AddContent(1, context.Row.Code);
        builder.CloseElement();
            })) },
        new() { Title = (TPrimaryTableColTitle<OrganizationSummary>)L("Status", "状态"), Cell = (TPrimaryTableColCell<OrganizationSummary>)((RenderFragment<TPrimaryTableCellParams<OrganizationSummary>>)(context => builder => builder.AddContent(0, L("Available", "可用")))) }
    ];

    private TPrimaryTableCol<DashboardRoleCell>[] RoleColumns =>
    [
        new() { ColKey = "Code", Title = (TPrimaryTableColTitle<DashboardRoleCell>)L("Role", "角色") },
        new() { Title = (TPrimaryTableColTitle<DashboardRoleCell>)L("Scope", "范围"), Cell = (TPrimaryTableColCell<DashboardRoleCell>)((RenderFragment<TPrimaryTableCellParams<DashboardRoleCell>>)(context => builder => builder.AddContent(0, L("Platform", "平台")))) },
        new() { Title = (TPrimaryTableColTitle<DashboardRoleCell>)L("Status", "状态"), Cell = (TPrimaryTableColCell<DashboardRoleCell>)((RenderFragment<TPrimaryTableCellParams<DashboardRoleCell>>)(context => builder => builder.AddContent(0, L("Effective", "生效")))) }
    ];
    // Session.Roles 是 string[]；表格行键需要稳定对象，包装为最小记录。
    private DashboardRoleCell[] RoleRows
    {
        get
        {
            if (Session?.Roles is not { Length: > 0 } roles)
                return [];

            var rows = new DashboardRoleCell[roles.Length];
            for (var index = 0; index < roles.Length; index++)
                rows[index] = new DashboardRoleCell(roles[index]);
            return rows;
        }
    }

    private OrganizationSummary[] OrganizationRows => Session?.Organizations ?? [];

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

    private int TotalSignIns
    {
        get
        {
            if (overview?.RecentAudit is not { Length: > 0 } audit)
                return 0;

            var total = 0;
            foreach (var day in audit)
                total += day.SignIns;
            return total;
        }
    }

    private static string ToDayLabel(string date)
        => date.Length > 5 ? date.Substring(5) : date;
}
