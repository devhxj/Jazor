using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.OnlineUser;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.OnlineUser;

[ECMAScriptModule("components/online-user-page-state.js")]
public static class OnlineUserPageModule
{
    public static OnlineUserPageState Create() => new();

    public sealed class OnlineUserPageState : global::Jero.Web.Composition.IPageState<OnlineUserPageProps>
    {
        private OnlineUserView[] _items = [];
        private OnlineUserView[] _rows = [];
        private OnlineUserTenantOption[] _tenantItems = [];
        private int _currentPage = 1;
        private int _pageSize = 50;
        private int _total;
        private string _tenantId = string.Empty;
        private string _userName = string.Empty;
        private string _realName = string.Empty;
        private bool _loading;
        private bool _tenantLoading;
        private bool _forcing;
        private bool _pageActive;
        private string? _error;
        private RealtimeConnectionStatus _realtimeStatus = RealtimeState.ConnectionStatus;
        private Action? _realtimeStatusSubscription;
        private Action? _onlineUserListSubscription;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanQuery => ApplicationState.HasPermission("sysOnlineUser:page");
        private bool CanForceOffline => ApplicationState.HasPermission("sysOnlineUser:forceOffline");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            _pageActive = true;
            OnBeforeUnmount(ReleasePage);
            _realtimeStatusSubscription = RealtimeState.SubscribeConnectionStatus(SetRealtimeStatus);
            _onlineUserListSubscription = RealtimeState.SubscribeOnlineUserList(HandleOnlineUserList);
            _tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            if (CanQuery)
                LoadPage();
        }

        private void SetRealtimeStatus(RealtimeConnectionStatus status)
            => _realtimeStatus = status;

        private void HandleOnlineUserList()
        {
            if (_pageActive && !_loading)
                LoadPage();
        }

        private void ReleasePage()
        {
            _pageActive = false;
            _realtimeStatusSubscription?.Invoke();
            _onlineUserListSubscription?.Invoke();
            _realtimeStatusSubscription = null;
            _onlineUserListSubscription = null;
        }

        private string RealtimeStatusText()
        {
            if (_realtimeStatus == RealtimeConnectionStatus.Connected) return "已连接";
            if (_realtimeStatus == RealtimeConnectionStatus.Connecting) return "连接中";
            if (_realtimeStatus == RealtimeConnectionStatus.Reconnecting) return "重连中";
            return "未连接";
        }

        private bool IsConnected => _realtimeStatus == RealtimeConnectionStatus.Connected;
        private bool IsConnecting => _realtimeStatus == RealtimeConnectionStatus.Connecting ||
                                     _realtimeStatus == RealtimeConnectionStatus.Reconnecting;
        private void LoadTenants()
        {
            _tenantLoading = true;
            OnlineUserClient.Tenants().Then(outcome =>
            {
                _tenantLoading = false;
                if (!outcome.Ok)
                {
                    _tenantItems = [];
                    _error = outcome.Error ?? "加载租户失败。";
                    return;
                }

                _tenantItems = OnlineUserClient.ReadTenants(outcome.Data);
                _error = null;
            }, _ =>
            {
                _tenantLoading = false;
                _tenantItems = [];
                _error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            if (!CanQuery)
                return;

            _loading = true;
            OnlineUserClient.Page(new OnlineUserPageInput
            {
                Page = _currentPage,
                PageSize = _pageSize,
                TenantId = IsSuperAdmin ? _tenantId : DefaultTenantId,
                UserName = _userName.Trim(),
                RealName = _realName.Trim()
            }).Then(outcome =>
            {
                _loading = false;
                if (!outcome.Ok)
                {
                    ClearPage(outcome.Error ?? "加载在线用户失败。");
                    return;
                }

                _items = OnlineUserClient.ReadItems(outcome.Data);
                _rows = _items;
                _total = OnlineUserClient.ReadTotal(outcome.Data);
                _error = null;
            }, _ =>
            {
                _loading = false;
                ClearPage("加载在线用户失败，请检查网络后重试。");
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
            _userName = string.Empty;
            _realName = string.Empty;
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

        private void ForceOffline(string id)
        {
            if (!CanForceOffline || _forcing)
                return;

            var row = Find(id);
            if (row is null)
                return;
            if (row.ConnectionId.Length == 0)
            {
                _error = "连接 ID 缺失，无法强制下线。";
                return;
            }

            _forcing = true;
            OnlineUserClient.ForceOffline(new OnlineUserForceOfflineInput
            {
                Id = row.Id,
                TenantId = Optional(row.TenantId),
                ConnectionId = row.ConnectionId,
                UserId = row.UserId,
                UserName = row.UserName,
                RealName = Optional(row.RealName),
                Time = Optional(row.Time),
                Ip = Optional(row.Ip),
                Browser = Optional(row.Browser),
                Os = Optional(row.Os)
            }).Then(outcome =>
            {
                _forcing = false;
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "强制下线失败。";
                    return;
                }

                _error = null;
                ElementPlus.ElMessageMembers.Success("已发送强制下线指令");
                LoadPage();
            }, _ =>
            {
                _forcing = false;
                _error = "强制下线失败，请检查网络后重试。";
            });
        }

        private OnlineUserView? Find(string id)
        {
            for (var index = 0; index < _items.Length; index++)
                if (_items[index].Id == id)
                    return _items[index];
            return null;
        }

        private void ClearPage(string message)
        {
            _items = [];
            _rows = [];
            _total = 0;
            _error = message;
        }

        private static string? Optional(string value)
            => value.Length == 0 ? null : value;

        public OnlineUserPageProps CreateProps() => new()
        {
            Error = _error,
            IsSuperAdmin = IsSuperAdmin,
            CanQuery = CanQuery,
            Loading = _loading,
            UserName = _userName,
            RealName = _realName,
            CurrentPage = _currentPage,
            PageSize = _pageSize,
            Total = _total,
            DismissError = () => _error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            SetUserName = value => _userName = value,
            SetRealName = value => _realName = value,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            Rows = _rows,
            TenantOptions = _tenantItems,
            TenantId = _tenantId,
            TenantLoading = _tenantLoading,
            CanForceOffline = CanForceOffline,
            Forcing = _forcing,
            ForceOffline = ForceOffline,
            TenantChanged = TenantChanged,
            Connected = IsConnected,
            Connecting = IsConnecting,
            RealtimeText = RealtimeStatusText(),
        };
    }
}

public sealed record OnlineUserPageProps : VueProps
{
    public required string? Error { get; init; }
    public required bool IsSuperAdmin { get; init; }
    public required bool CanQuery { get; init; }
    public required bool Loading { get; init; }
    public required string UserName { get; init; }
    public required string RealName { get; init; }
    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; }
    public required int Total { get; init; }
    public required Action DismissError { get; init; }
    public required Action Search { get; init; }
    public required Action ResetSearch { get; init; }
    public required Action Refresh { get; init; }
    public required Action<string> SetUserName { get; init; }
    public required Action<string> SetRealName { get; init; }
    public required Action<double> ChangePage { get; init; }
    public required Action<double> ChangePageSize { get; init; }
    public required OnlineUserView[] Rows { get; init; }
    public required OnlineUserTenantOption[] TenantOptions { get; init; }
    public required string TenantId { get; init; }
    public required bool TenantLoading { get; init; }
    public required bool CanForceOffline { get; init; }
    public required bool Forcing { get; init; }
    public required Action<string> ForceOffline { get; init; }
    public required Action<string> TenantChanged { get; init; }
    public required bool Connected { get; init; }
    public required bool Connecting { get; init; }
    public required string RealtimeText { get; init; }
}
