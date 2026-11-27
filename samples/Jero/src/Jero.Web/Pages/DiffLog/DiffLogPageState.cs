using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.DiffLog;
using Jero.Web.Pages.Logs;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;

namespace Jero.Web.Pages.DiffLog;

[ECMAScriptModule("components/diff-log-page-state.js")]
public static class DiffLogPageModule
{
    public static DiffLogPageState Create() => new();

    public sealed class DiffLogPageState : global::Jero.Web.Composition.IPageState<DiffLogPageProps>
    {
        private DiffLogView[] _rows = [];
        private DiffLogTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _tenantId = string.Empty;
        private string _startTime = string.Empty;
        private string _endTime = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private string? _error;

        private bool _detailDialogVisible;
        private bool _detailLoading;
        private DiffLogView? _detail;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanQuery => ApplicationState.HasPermission("sysDifflog:page");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            if (CanQuery)
                LoadPage();
        }

        private void LoadTenants()
        {
            _tenantLoading = true;
            DiffLogClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载租户失败。";
                    _tenantItems = [];
                    return;
                }

                _tenantItems = DiffLogClient.ReadTenants(outcome.Data);
            }, _ =>
            {
                _tenantLoading = false;
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            if (!CanQuery)
                return;

            _loading = true;
            DiffLogClient.Page(new DiffLogPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                TenantId = IsSuperAdmin ? _tenantId : DefaultTenantId,
                StartTime = Optional(_startTime),
                EndTime = Optional(_endTime)
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载差异日志失败。";
                    _rows = [];
                    _total = 0;
                    return;
                }

                _rows = DiffLogClient.ReadItems(outcome.Data);
                _total = DiffLogClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                _error = "加载差异日志失败，请检查网络后重试。";
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

        private void OpenDetail(string id)
        {
            if (id.Length == 0 || !CanQuery || _detailLoading)
                return;

            _detail = null;
            _detailDialogVisible = true;
            _detailLoading = true;
            DiffLogClient.Detail(id).Then(outcome =>
            {
                _detailLoading = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载差异日志详情失败。";
                    return;
                }

                _detail = DiffLogClient.ReadDetail(outcome.Data);
                if (_detail is null)
                {
                    _error = "差异日志详情不存在。";
                    return;
                }
                _error = null;
            }, _ =>
            {
                _detailLoading = false;
                _error = "加载差异日志详情失败，请检查网络后重试。";
            });
        }

        private LogOption[] ViewTenants()
        {
            var options = new LogOption[_tenantItems.Length];
            for (var index = 0; index < _tenantItems.Length; index++)
            {
                var tenant = _tenantItems[index];
                options[index] = new() { Value = tenant.Value, Label = TenantLabel(tenant) };
            }
            return options;
        }

        private static string TenantLabel(DiffLogTenantOption tenant)
        {
            if (tenant.Host.Length > 0)
                return tenant.Label + " · " + tenant.Host;
            return tenant.Label.Length == 0 ? tenant.Value : tenant.Label;
        }

        private static string? Optional(string value)
        {
            var normalized = value.Trim();
            return normalized.Length == 0 ? null : normalized;
        }

        public DiffLogPageProps CreateProps() => new()
        {
            Rows = _rows,
            Tenants = ViewTenants(),
            Detail = _detail,
            Error = _error,
            TenantId = _tenantId,
            StartTime = _startTime,
            EndTime = _endTime,
            IsSuperAdmin = IsSuperAdmin,
            CanQuery = CanQuery,
            Loading = _loading,
            TenantLoading = _tenantLoading,
            DetailLoading = _detailLoading,
            DetailDialogVisible = _detailDialogVisible,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            DismissError = () => _error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            SetTenant = TenantChanged,
            SetStartTime = value => _startTime = value,
            SetEndTime = value => _endTime = value,
            OpenDetail = OpenDetail,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            SetDialogVisible = value => _detailDialogVisible = value,
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
