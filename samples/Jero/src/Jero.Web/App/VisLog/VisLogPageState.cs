using Jero.Web.App.Logs;
using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.VisLog;

[ECMAScriptModule("components/vis-log-page-state.js")]
public static class VisLogPageModule
{
    public static VisLogPageState Create() => new();

    public sealed class VisLogPageState : global::Jero.Web.App.IPageState<VisLogPageProps>
    {
        private VisLogView[] rows = [];
        private VisLogTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string tenantId = string.Empty;
        private string startTime = string.Empty;
        private string endTime = string.Empty;
        private string actionName = string.Empty;
        private string account = string.Empty;
        private string status = string.Empty;
        private string elapsed = string.Empty;
        private string remoteIp = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private string? error;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanClear => ApplicationState.HasPermission("sysVislog:clear");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPage();
        }

        private void LoadTenants()
        {
            tenantLoading = true;
            VisLogClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }

                tenantItems = VisLogClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            loading = true;
            VisLogClient.Page(new VisLogPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                tenantId = IsSuperAdmin ? tenantId : DefaultTenantId,
                startTime = Optional(startTime),
                endTime = Optional(endTime),
                actionName = Optional(actionName),
                account = Optional(account),
                status = status.Trim(),
                elapsed = PositiveNumber(elapsed),
                remoteIp = Optional(remoteIp)
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载访问日志失败。";
                    rows = [];
                    total = 0;
                    return;
                }

                rows = VisLogClient.ReadItems(outcome.Data);
                total = VisLogClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载访问日志失败，请检查网络后重试。";
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
            actionName = string.Empty;
            account = string.Empty;
            status = string.Empty;
            elapsed = string.Empty;
            remoteIp = string.Empty;
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

        private LogRow[] ViewRows()
        {
            var result = new LogRow[rows.Length];
            for (var index = 0; index < rows.Length; index++)
            {
                var row = rows[index];
                result[index] = new LogRow
                {
                    id = RowString(row, "id"),
                    controllerName = RowString(row, "controllerName"),
                    displayTitle = RowString(row, "displayTitle"),
                    actionName = RowString(row, "actionName"),
                    httpMethod = RowString(row, "httpMethod"),
                    requestUrl = RowString(row, "requestUrl"),
                    eventId = RowString(row, "eventId"),
                    threadId = RowString(row, "threadId"),
                    traceId = RowString(row, "traceId"),
                    account = RowString(row, "account"),
                    realName = RowString(row, "realName"),
                    remoteIp = RowString(row, "remoteIp"),
                    location = RowString(row, "location"),
                    browser = RowString(row, "browser"),
                    os = RowString(row, "os"),
                    longitude = RowString(row, "longitude"),
                    latitude = RowString(row, "latitude"),
                    status = RowString(row, "status"),
                    elapsed = RowString(row, "elapsed"),
                    logDateTime = RowString(row, "logDateTime"),
                    exception = RowString(row, "exception"),
                    logLevel = RowString(row, "logLevel"),
                };
            }
            return result;
        }

        private LogOption[] ViewTenants()
        {
            var result = new LogOption[tenantItems.Length];
            for (var index = 0; index < tenantItems.Length; index++)
            {
                var tenant = tenantItems[index];
                result[index] = new LogOption { value = tenant.value, label = TenantLabel(tenant) };
            }
            return result;
        }

        private static string RowString(VueProps? row, string key)
        {
            if (row is null)
                return string.Empty;
            var value = Reflect.Get(row, key);
            if (value is null)
                return string.Empty;
            var result = StringValue(value);
            return result == "null" || result == "undefined" ? string.Empty : result;
        }

        private static string TenantLabel(VisLogTenantOption tenant)
        {
            if (tenant.host.Length > 0)
                return tenant.label + " · " + tenant.host;
            return tenant.label.Length == 0 ? tenant.value : tenant.label;
        }

        private static double? PositiveNumber(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length == 0)
                return null;
            var result = (double)NumberValue(normalized);
            return IsFinite(result) && result > 0 && result <= 2147483647 ? result : null;
        }

        private static string? Optional(string value)
        {
            var normalized = value.Trim();
            return normalized.Length == 0 ? null : normalized;
        }

        public VisLogPageProps CreateProps() => new()
        {
            Title = "访问日志",
            Description = "支持租户、时间、方法、账号、状态、耗时和 IP 筛选；结果由后端按创建时间倒序分页。",
            Kind = "visit",
            Rows = ViewRows(),
            Tenants = ViewTenants(),
            TenantLoading = tenantLoading,
            TenantId = tenantId,
            SetTenant = TenantChanged,
            IsSuperAdmin = IsSuperAdmin,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            StartTime = startTime,
            SetStartTime = v => startTime = v,
            EndTime = endTime,
            SetEndTime = v => endTime = v,
            ActionName = actionName,
            SetActionName = v => actionName = v,
            Account = account,
            SetAccount = v => account = v,
            Elapsed = elapsed,
            SetElapsed = v => elapsed = v,
            RemoteIp = remoteIp,
            SetRemoteIp = v => remoteIp = v,
            Status = status,
            SetStatus = v => status = v,
            Error = error,
            DismissError = () => error = null,
            Loading = loading,
            Search = Search,
            ResetSearch = ResetSearch,
            CanClear = CanClear,
            Refresh = Refresh,
        };
    }
}

public sealed record VisLogPageProps : VueProps
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Kind { get; init; }
    public required LogRow[] Rows { get; init; }
    public required LogOption[] Tenants { get; init; }
    public required bool TenantLoading { get; init; }
    public required string TenantId { get; init; }
    public required Action<string> SetTenant { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required string StartTime { get; init; }
    public required Action<string> SetStartTime { get; init; }
    public required string EndTime { get; init; }
    public required Action<string> SetEndTime { get; init; }
    public required string ActionName { get; init; }
    public required Action<string> SetActionName { get; init; }
    public required string Account { get; init; }
    public required Action<string> SetAccount { get; init; }
    public required string Elapsed { get; init; }
    public required Action<string> SetElapsed { get; init; }
    public required string RemoteIp { get; init; }
    public required Action<string> SetRemoteIp { get; init; }
    public required string Status { get; init; }
    public required Action<string> SetStatus { get; init; }
    public required string? Error { get; init; }
    public required Action DismissError { get; init; }
    public required bool Loading { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required bool CanClear { get; init; }
    public required Action Refresh { get; init; }
}
