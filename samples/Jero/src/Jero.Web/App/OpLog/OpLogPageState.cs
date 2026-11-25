using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Logs;
using static ECMAScript.Global;

namespace Jero.Web.App.OpLog;

[ECMAScriptModule("components/op-log-page-state.js")]
public static class OpLogPageModule
{
    public static OpLogPageState Create() => new();

    public sealed class OpLogPageState : global::Jero.Web.App.IPageState<OpLogPageProps>
    {
        private OpLogView[] items = [];
        private OpLogView[] rows = [];
        private LogTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string tenantId = string.Empty;
        private string startTime = string.Empty;
        private string endTime = string.Empty;
        private string controllerName = string.Empty;
        private string actionName = string.Empty;
        private string account = string.Empty;
        private string elapsed = string.Empty;
        private string remoteIp = string.Empty;
        private string sortField = "createTime";
        private string sortOrder = "descending";
        private bool loading;
        private bool tenantLoading;
        private bool clearing;
        private bool exporting;
        private string? error;

        private bool detailDialogVisible;
        private bool detailLoading;
        private string detailContent = "暂无详情";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanViewDetail => ApplicationState.HasPermission("sysOplog:page");
        private bool CanClear => ApplicationState.HasPermission("sysOplog:clear");
        private bool CanExport => ApplicationState.HasPermission("sysOplog:export");
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
            LogShared.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载租户失败。";
                    tenantItems = [];
                    return;
                }
                tenantItems = LogShared.ReadTenants(outcome.Data);
            }, _ =>
            {
                tenantLoading = false;
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            loading = true;
            OpLogClient.Page(BuildPageInput()).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载操作日志失败。";
                    items = [];
                    rows = [];
                    total = 0;
                    return;
                }

                items = OpLogClient.ReadItems(outcome.Data);
                rows = items;
                total = OpLogClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                error = "加载操作日志失败，请检查网络后重试。";
            });
        }

        private OpLogPageInput BuildPageInput()
            => new()
            {
                page = currentPage,
                pageSize = pageSize,
                startTime = Optional(startTime),
                endTime = Optional(endTime),
                tenantId = IsSuperAdmin && tenantId.Length > 0 ? tenantId : "0",
                controllerName = controllerName.Trim(),
                actionName = actionName.Trim(),
                account = account.Trim(),
                elapsed = PositiveLong(elapsed),
                remoteIp = remoteIp.Trim(),
                field = sortField,
                order = sortOrder,
                descStr = sortOrder
            };

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
            controllerName = string.Empty;
            actionName = string.Empty;
            account = string.Empty;
            elapsed = string.Empty;
            remoteIp = string.Empty;
            sortField = "createTime";
            sortOrder = "descending";
            currentPage = 1;
            LoadPage();
        }

        private void TenantChanged(string? value)
        {
            tenantId = value ?? string.Empty;
            currentPage = 1;
            LoadPage();
        }

        private void Sort(string prop, string order)
        {
            sortField = string.IsNullOrWhiteSpace(prop) ? "createTime" : prop;
            sortOrder = order == "descending" ? "descending" : "ascending";
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

        private void ClearLogs()
        {
            if (!CanClear || clearing)
                return;

            clearing = true;
            OpLogClient.Clear().Then(outcome =>
            {
                clearing = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "清空操作日志失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("操作日志已清空");
                currentPage = 1;
                LoadPage();
            }, _ =>
            {
                clearing = false;
                error = "清空操作日志失败，请检查网络后重试。";
            });
        }

        private void ExportLogs()
        {
            if (!CanExport || exporting)
                return;

            exporting = true;
            LogShared.Download("/api/sysLogOp/export", new LogExportInput
            {
                startTime = Optional(startTime),
                endTime = Optional(endTime)
            }, "操作日志.xlsx").Then(outcome =>
            {
                exporting = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "导出操作日志失败。";
                    return;
                }
                error = null;
                ElementPlus.ElMessageMembers.Success("操作日志已导出");
            }, _ =>
            {
                exporting = false;
                error = "导出操作日志失败，请检查网络后重试。";
            });
        }

        private void OpenDetail(string id)
        {
            if (!CanViewDetail || id.Length == 0)
                return;

            detailDialogVisible = true;
            detailLoading = true;
            detailContent = string.Empty;
            OpLogClient.Detail(id).Then(outcome =>
            {
                detailLoading = false;
                if (!outcome.Ok)
                {
                    detailContent = outcome.Error ?? "加载操作日志详情失败。";
                    return;
                }

                var detail = OpLogClient.ReadDetail(outcome.Data);
                detailContent = detail is null
                    ? "暂无详情"
                    : DisplayValue(detail.message.Length > 0 ? detail.message : detail.exception);
            }, _ =>
            {
                detailLoading = false;
                detailContent = "加载操作日志详情失败，请检查网络后重试。";
            });
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

        private static string TenantLabel(LogTenantOption tenant)
            => tenant.host.Length == 0 ? tenant.label : tenant.label + "（" + tenant.host + "）";

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

        private static long? PositiveLong(string value)
        {
            var normalized = value.Trim();
            if (normalized.Length == 0)
                return null;
            var number = (long)(double)NumberValue(normalized);
            return number > 0 ? number : null;
        }

        private static string? Optional(string value)
        {
            var normalized = value.Trim();
            return normalized.Length == 0 ? null : normalized;
        }

        private static string DisplayValue(string value)
            => value.Length == 0 ? "-" : value;

        public OpLogPageProps CreateProps() => new()
        {
            Title = "操作日志",
            Description = "支持租户、时间、模块、方法、账号、耗时和 IP 筛选，以及详情、清空和 Excel 导出。",
            Kind = "operation",
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
            Error = error,
            DismissError = () => error = null,
            Loading = loading,
            Search = Search,
            ResetSearch = ResetSearch,
            CanClear = CanClear,
            ControllerName = controllerName,
            SetControllerName = v => controllerName = v,
            Refresh = LoadPage,
            CanViewDetail = CanViewDetail,
            CanExport = CanExport,
            Clearing = clearing,
            Exporting = exporting,
            ClearLogs = ClearLogs,
            ExportLogs = ExportLogs,
            Sort = Sort,
            OpenDetail = OpenDetail,
            DetailDialogVisible = detailDialogVisible,
            DetailLoading = detailLoading,
            DetailContent = detailContent,
            SetDialogVisible = v => detailDialogVisible = v,
        };
    }
}

public sealed record OpLogPageProps : VueProps
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
    public required string? Error { get; init; }
    public required Action DismissError { get; init; }
    public required bool Loading { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required bool CanClear { get; init; }
    public required string ControllerName { get; init; }
    public required Action<string> SetControllerName { get; init; }
    public required Action Refresh { get; init; }
    public required bool CanViewDetail { get; init; }
    public required bool CanExport { get; init; }
    public required bool Clearing { get; init; }
    public required bool Exporting { get; init; }
    public required Action ClearLogs { get; init; }
    public required Action ExportLogs { get; init; }
    public required Action<string, string> Sort { get; init; }
    public required Action<string> OpenDetail { get; init; }
    public required bool DetailDialogVisible { get; init; }
    public required bool DetailLoading { get; init; }
    public required string DetailContent { get; init; }
    public required Action<bool> SetDialogVisible { get; init; }
}
