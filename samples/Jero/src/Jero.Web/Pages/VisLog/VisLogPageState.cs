using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.VisLog;
using Jero.Web.Pages.Logs;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.VisLog;

[ECMAScriptModule("components/vis-log-page-state.js")]
public static class VisLogPageModule
{
    public static VisLogPageState Create() => new();

    public sealed class VisLogPageState : global::Jero.Web.Composition.IPageState<VisLogPageProps>
    {
        private VisLogView[] _rows = [];
        private VisLogTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _tenantId = string.Empty;
        private string _startTime = string.Empty;
        private string _endTime = string.Empty;
        private string _actionName = string.Empty;
        private string _account = string.Empty;
        private string _status = string.Empty;
        private string _elapsed = string.Empty;
        private string _remoteIp = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private string? _error;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanClear => ApplicationState.HasPermission("sysVislog:clear");
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
            VisLogClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = VisLogClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            _loading = true;
            VisLogClient.Page(new VisLogPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                TenantId = IsSuperAdmin ? _tenantId : DefaultTenantId,
                StartTime = Optional(_startTime),
                EndTime = Optional(_endTime),
                ActionName = Optional(_actionName),
                Account = Optional(_account),
                Status = _status.Trim(),
                Elapsed = PositiveNumber(_elapsed),
                RemoteIp = Optional(_remoteIp)
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载访问日志失败。";
                    _rows = [];
                    _total = 0;
                    return;
                }

                _rows = VisLogClient.ReadItems(outcome.Data);
                _total = VisLogClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载访问日志失败，请检查网络后重试。";
            });
        }

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
            _actionName = string.Empty;
            _account = string.Empty;
            _status = string.Empty;
            _elapsed = string.Empty;
            _remoteIp = string.Empty;
            _currentPage = 1;
            LoadPage();
        }

        private void Refresh() => LoadPage();

        private void TenantChanged(string? value)
        {
            _tenantId = value ?? string.Empty;
            _currentPage = 1;
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
                    // 共用表格也显示操作和异常日志的专用列，访问日志没有这些字段。
                    HttpMethod = string.Empty,
                    RequestUrl = string.Empty,
                    EventId = string.Empty,
                    ThreadId = string.Empty,
                    TraceId = string.Empty,
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
                    Exception = string.Empty,
                    LogLevel = string.Empty,
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

        private static string CellText(object? value)
        {
            if (value is null)
                return string.Empty;
            var result = StringValue(value);
            return result == "null" || result == "undefined" ? string.Empty : result;
        }

        private static string TenantLabel(VisLogTenantOption tenant)
        {
            if (tenant.Host.Length > 0)
                return tenant.Label + " · " + tenant.Host;
            return tenant.Label.Length == 0 ? tenant.Value : tenant.Label;
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
