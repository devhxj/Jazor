using Jero.Web.App.Logs;
using Jero.Web.App.Components;
using Jero.Web.App.Shell;

namespace Jero.Web.App.DiffLog;

[ECMAScriptModule("components/diff-log-page-state.js")]
public static class DiffLogPageModule
{
    public static DiffLogPageState Create() => new();

    public sealed class DiffLogPageState : global::Jero.Web.App.IPageState<DiffLogPageProps>
    {
        private DiffLogView[] rows = [];
        private DiffLogTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string tenantId = string.Empty;
        private string startTime = string.Empty;
        private string endTime = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private string? error;

        private bool detailDialogVisible;
        private bool detailLoading;
        private DiffLogView? detail;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanQuery => ApplicationState.HasPermission("sysDifflog:page");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            if (CanQuery)
                LoadPage();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            DiffLogClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = DiffLogClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            if (!CanQuery)
                return;

            loading = true;
            DiffLogClient.Page(new DiffLogPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                tenantId = IsSuperAdmin ? tenantId : DefaultTenantId,
                startTime = Optional(startTime),
                endTime = Optional(endTime)
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载差异日志失败。";
                    rows = [];
                    total = 0;
                    return;
                }

                rows = DiffLogClient.ReadItems(outcome.Data);
                total = DiffLogClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载差异日志失败，请检查网络后重试。";
            });
        }

        private void Search()
        {
            currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            tenantId = DefaultTenantId;
            startTime = string.Empty;
            endTime = string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void Refresh() => LoadPage();

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void ChangePage(double value)
        {
            currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            pageSize = (int)value;
            currentPage = 1;
            LoadPage();
        }

        private void OpenDetail(string id)
        {
            if (id.Length == 0 || !CanQuery || detailLoading)
                return;

            detail = null;
            detailDialogVisible = true;
            detailLoading = true;
            DiffLogClient.Detail(id).Then(outcome =>
            {
                detailLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载差异日志详情失败。";
                    return;
                }

                detail = DiffLogClient.ReadDetail(outcome.Data);
                if (detail is null)
                {
                    error = "差异日志详情不存在。";
                    return;
                }
                error = null;
            }, _ =>
            {
                detailLoading = false;
                error = "加载差异日志详情失败，请检查网络后重试。";
            });
        }

        private LogOption[] ViewTenants()
        {
            var options = new LogOption[tenantItems.Length];
            for (var index = 0; index < tenantItems.Length; index++)
            {
                var tenant = tenantItems[index];
                options[index] = new() { value = tenant.value, label = TenantLabel(tenant) };
            }
            return options;
        }

        private static string TenantLabel(DiffLogTenantOption tenant)
        {
            if (tenant.host.Length > 0)
                return tenant.label + " · " + tenant.host;
            return tenant.label.Length == 0 ? tenant.value : tenant.label;
        }

        private static string? Optional(string value)
        {
            var normalized = value.Trim();
            return normalized.Length == 0 ? null : normalized;
        }

        public DiffLogPageProps CreateProps() => new()
        {
            Rows = rows,
            Tenants = ViewTenants(),
            Detail = detail,
            Error = error,
            TenantId = tenantId,
            StartTime = startTime,
            EndTime = endTime,
            IsSuperAdmin = IsSuperAdmin,
            CanQuery = CanQuery,
            Loading = loading,
            TenantLoading = tenantLoading,
            DetailLoading = detailLoading,
            DetailDialogVisible = detailDialogVisible,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            DismissError = () => error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            SetTenant = TenantChanged,
            SetStartTime = value => startTime = value,
            SetEndTime = value => endTime = value,
            OpenDetail = OpenDetail,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetDialogVisible = value => detailDialogVisible = value,
        };
    }
}

public sealed record DiffLogPageProps : VueProps
{
    public required DiffLogView[] Rows { get; init; }
    public required LogOption[] Tenants { get; init; }
    public required DiffLogView? Detail { get; init; }
    public required string? Error { get; init; }
    public required string TenantId { get; init; }
    public required string StartTime { get; init; }
    public required string EndTime { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool CanQuery { get; init; }
    public required bool Loading { get; init; }
    public required bool TenantLoading { get; init; }
    public required bool DetailLoading { get; init; }
    public required bool DetailDialogVisible { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required Action DismissError { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action Refresh { get; init; }
    public required Action<string> SetTenant { get; init; }
    public required Action<string> SetStartTime { get; init; }
    public required Action<string> SetEndTime { get; init; }
    public required Action<string> OpenDetail { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required Action<bool> SetDialogVisible { get; init; }
}
