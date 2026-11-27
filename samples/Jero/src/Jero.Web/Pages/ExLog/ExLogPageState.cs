using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.ExLog;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Pages.Logs;
using static ECMAScript.Global;

namespace Jero.Web.Pages.ExLog;

[ECMAScriptModule("components/ex-log-page-state.js")]
public static class ExLogPageModule
{
    public static ExLogPageState Create() => new();

    public sealed class ExLogPageState : global::Jero.Web.Composition.IPageState<ExLogPageProps>
    {
        private ExLogView[] _items = [];
        private ExLogView[] _rows = [];
        private LogTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _tenantId = string.Empty;
        private string _startTime = string.Empty;
        private string _endTime = string.Empty;
        private string _controllerName = string.Empty;
        private string _actionName = string.Empty;
        private string _account = string.Empty;
        private string _elapsed = string.Empty;
        private string _remoteIp = string.Empty;
        private string _status = string.Empty;
        private string _sortField = "createTime";
        private string _sortOrder = "descending";
        private bool _loading;
        private bool _tenantLoading;
        private bool _clearing;
        private bool _exporting;
        private string? _error;

        private bool _detailDialogVisible;
        private bool _detailLoading;
        private string _detailContent = "暂无详情";

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanViewDetail => ApplicationState.HasPermission("sysExlog:page");
        private bool CanClear => ApplicationState.HasPermission("sysExlog:clear");
        private bool CanExport => ApplicationState.HasPermission("sysExlog:export");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            LoadPage();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            LogShared.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }
                _tenantItems = LogShared.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            _loading = true;
            ExLogClient.Page(BuildPageInput()).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载异常日志失败。";
                    _items = [];
                    _rows = [];
                    _total = 0;
                    return;
                }

                _items = ExLogClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = ExLogClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载异常日志失败，请检查网络后重试。";
            });
        }

        private ExLogPageInput BuildPageInput()
            => new()
            {
                Page = _currentPage,
                PageSize = _pageSize,
                StartTime = Optional(_startTime),
                EndTime = Optional(_endTime),
                TenantId = IsSuperAdmin && _tenantId.Length > 0 ? _tenantId : "0",
                ControllerName = _controllerName.Trim(),
                ActionName = _actionName.Trim(),
                Account = _account.Trim(),
                Elapsed = PositiveLong(_elapsed),
                RemoteIp = _remoteIp.Trim(),
                Status = _status,
                Field = _sortField,
                Order = _sortOrder,
                DescStr = _sortOrder
            };

        private void Search()
        {
            _currentPage = 1;
            LoadPage();
        }

        private void ResetSearch()
        {
            _tenantId = DefaultTenantId;
            _startTime = string.Empty;
            _endTime = string.Empty;
            _controllerName = string.Empty;
            _actionName = string.Empty;
            _account = string.Empty;
            _elapsed = string.Empty;
            _remoteIp = string.Empty;
            _status = string.Empty;
            _sortField = "createTime";
            _sortOrder = "descending";
            _currentPage = 1;
            LoadPage();
        }

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void Sort(string prop, string order)
        {
            _sortField = string.IsNullOrWhiteSpace(prop) ? "createTime" : prop;
            _sortOrder = order == "descending" ? "descending" : "ascending";
            LoadPage();
        }

        private void ChangePage(double value)
        {
            _currentPage = (int)value;
            LoadPage();
        }

        private void ChangePageSize(double value)
        {
            _pageSize = (int)value;
            _currentPage = 1;
            LoadPage();
        }

        private void ClearLogs()
        {
            if (!CanClear || _clearing)
                return;

            _clearing = true;
            ExLogClient.Clear().Then(outcome =>
            {
                _clearing = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "清空异常日志失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("异常日志已清空");
                _currentPage = 1;
                LoadPage();
            }, _ =>
            {
                _clearing = false;
                _error = "清空异常日志失败，请检查网络后重试。";
            });
        }

        private void ExportLogs()
        {
            if (!CanExport || _exporting)
                return;

            _exporting = true;
            LogShared.Download("/api/sysLogEx/export", new LogExportInput
            {
                StartTime = Optional(_startTime),
                EndTime = Optional(_endTime)
            }, "异常日志.xlsx").Then(outcome =>
            {
                _exporting = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "导出异常日志失败。";
                    return;
                }
                _error = null;
                ElementPlus.ElMessageMembers.Success("异常日志已导出");
            }, _ =>
            {
                _exporting = false;
                _error = "导出异常日志失败，请检查网络后重试。";
            });
        }

        private void OpenDetail(string id)
        {
            if (!CanViewDetail || id.Length == 0)
                return;

            _detailDialogVisible = true;
            _detailLoading = true;
            _detailContent = string.Empty;
            ExLogClient.Detail(id).Then(outcome =>
            {
                _detailLoading = false;
                if (!outcome.Ok)
                {
                    _detailContent = outcome.Error ?? "加载异常日志详情失败。";
                    return;
                }

                var detail = ExLogClient.ReadDetail(outcome.Data);
                _detailContent = detail is null
                    ? "暂无详情"
                    : DisplayValue(detail.Message.Length > 0 ? detail.Message : detail.Exception);
            }, _ =>
            {
                _detailLoading = false;
                _detailContent = "加载异常日志详情失败，请检查网络后重试。";
            });
        }


        private LogRow[] ViewRows()
        {
            var result = new LogRow[_rows.Length];
            for (var index = 0; index < _rows.Length; index++)
            {
                var row = _rows[index];
                result[index] = new LogRow
                {
                    Id = CellText(row.Id),
                    ControllerName = CellText(row.ControllerName),
                    DisplayTitle = CellText(row.DisplayTitle),
                    ActionName = CellText(row.ActionName),
                    HttpMethod = CellText(row.HttpMethod),
                    RequestUrl = CellText(row.RequestUrl),
                    EventId = CellText(row.EventId),
                    ThreadId = CellText(row.ThreadId),
                    TraceId = CellText(row.TraceId),
                    Account = CellText(row.Account),
                    RealName = CellText(row.RealName),
                    RemoteIp = CellText(row.RemoteIp),
                    Location = CellText(row.Location),
                    Browser = CellText(row.Browser),
                    Os = CellText(row.Os),
                    Longitude = CellText(row.Longitude),
                    Latitude = CellText(row.Latitude),
                    Status = CellText(row.Status),
                    Elapsed = CellText(row.Elapsed),
                    LogDateTime = CellText(row.LogDateTime),
                    Exception = CellText(row.Exception),
                    LogLevel = CellText(row.LogLevel),
                };
            }
            return result;
        }

        private LogOption[] ViewTenants()
        {
            var result = new LogOption[_tenantItems.Length];
            for (var index = 0; index < _tenantItems.Length; index++)
            {
                var tenant = _tenantItems[index];
                result[index] = new LogOption { Value = tenant.Value, Label = TenantLabel(tenant) };
            }
            return result;
        }

        private static string TenantLabel(LogTenantOption tenant)
            => tenant.Host.Length == 0 ? tenant.Label : tenant.Label + "（" + tenant.Host + "）";

        private static string CellText(object? value)
        {
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

        public ExLogPageProps CreateProps() => new()
        {
            Title = "异常日志",
            Description = "支持租户、时间、模块、方法、账号、状态、耗时和 IP 筛选，以及详情、清空和 Excel 导出。",
            Kind = "exception",
            Rows = ViewRows(),
            Tenants = ViewTenants(),
            TenantLoading = _tenantLoading,
            TenantId = _tenantId,
            SetTenant = TenantChanged,
            IsSuperAdmin = IsSuperAdmin,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            StartTime = _startTime,
            SetStartTime = v => _startTime = v,
            EndTime = _endTime,
            SetEndTime = v => _endTime = v,
            ActionName = _actionName,
            SetActionName = v => _actionName = v,
            Account = _account,
            SetAccount = v => _account = v,
            Elapsed = _elapsed,
            SetElapsed = v => _elapsed = v,
            RemoteIp = _remoteIp,
            SetRemoteIp = v => _remoteIp = v,
            Status = _status,
            SetStatus = v => _status = v,
            Error = _error,
            DismissError = () => _error = null,
            Loading = _loading,
            Search = Search,
            ResetSearch = ResetSearch,
            CanClear = CanClear,
            ControllerName = _controllerName,
            SetControllerName = v => _controllerName = v,
            Refresh = LoadPage,
            CanViewDetail = CanViewDetail,
            CanExport = CanExport,
            Clearing = _clearing,
            Exporting = _exporting,
            ClearLogs = ClearLogs,
            ExportLogs = ExportLogs,
            Sort = Sort,
            OpenDetail = OpenDetail,
            DetailDialogVisible = _detailDialogVisible,
            DetailLoading = _detailLoading,
            DetailContent = _detailContent,
            SetDialogVisible = v => _detailDialogVisible = v,
        };
    }
}

public sealed record ExLogPageProps : VueProps
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
