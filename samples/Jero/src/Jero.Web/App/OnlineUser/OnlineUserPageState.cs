using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Realtime;
using static ECMAScript.Global;

namespace Jero.Web.App.OnlineUser;

[ECMAScriptModule("components/online-user-page-state.js")]
public static class OnlineUserPageModule
{
    public static OnlineUserPageState Create() => new();

    public sealed class OnlineUserPageState : global::Jero.Web.App.IPageState<OnlineUserPageProps>
    {
        private OnlineUserView[] items = [];
        private OnlineUserView[] rows = [];
        private OnlineUserTenantOption[] tenantItems = [];
        private int currentPage = 1;
        private int pageSize = 50;
        private int total;
        private string tenantId = string.Empty;
        private string userName = string.Empty;
        private string realName = string.Empty;
        private bool loading;
        private bool tenantLoading;
        private bool forcing;
        private bool pageActive;
        private string? error;
        private RealtimeConnectionStatus realtimeStatus = RealtimeState.ConnectionStatus;
        private Action? realtimeStatusSubscription;
        private Action? onlineUserListSubscription;

        private bool IsSuperAdmin => ApplicationState.User?.AccountType == 999;
        private bool CanQuery => ApplicationState.HasPermission("sysOnlineUser:page");
        private bool CanForceOffline => ApplicationState.HasPermission("sysOnlineUser:forceOffline");
        private string DefaultTenantId => IsSuperAdmin
            ? ApplicationState.User?.CurrentTenantId ?? string.Empty
            : ApplicationState.User?.TenantId ?? string.Empty;

        public void Initialize()
        {
            pageActive = true;
            OnBeforeUnmount(ReleasePage);
            realtimeStatusSubscription = RealtimeState.SubscribeConnectionStatus(SetRealtimeStatus);
            onlineUserListSubscription = RealtimeState.SubscribeOnlineUserList(HandleOnlineUserList);
            tenantId = DefaultTenantId;
            if (IsSuperAdmin)
                LoadTenants();
            if (CanQuery)
                LoadPage();
        }

        private void SetRealtimeStatus(RealtimeConnectionStatus status)
            => realtimeStatus = status;

        private void HandleOnlineUserList()
        {
            if (pageActive && !loading)
                LoadPage();
        }

        private void ReleasePage()
        {
            pageActive = false;
            realtimeStatusSubscription?.Invoke();
            onlineUserListSubscription?.Invoke();
            realtimeStatusSubscription = null;
            onlineUserListSubscription = null;
        }

        private string RealtimeStatusText()
        {
            if (realtimeStatus == RealtimeConnectionStatus.Connected) return "已连接";
            if (realtimeStatus == RealtimeConnectionStatus.Connecting) return "连接中";
            if (realtimeStatus == RealtimeConnectionStatus.Reconnecting) return "重连中";
            return "未连接";
        }

        private bool IsConnected => realtimeStatus == RealtimeConnectionStatus.Connected;
        private bool IsConnecting => realtimeStatus == RealtimeConnectionStatus.Connecting ||
                                     realtimeStatus == RealtimeConnectionStatus.Reconnecting;
        private void LoadTenants()
        {
            tenantLoading = true;
            OnlineUserClient.Tenants().Then(outcome =>
            {
                tenantLoading = false;
                if (!outcome.Ok)
                {
                    tenantItems = [];
                    error = outcome.Error ?? "加载租户失败。";
                    return;
                }

                tenantItems = OnlineUserClient.ReadTenants(outcome.Data);
                error = null;
            }, _ =>
            {
                tenantLoading = false;
                tenantItems = [];
                error = "加载租户失败，请检查网络后重试。";
            });
        }

        private void LoadPage()
        {
            if (!CanQuery)
                return;

            loading = true;
            OnlineUserClient.Page(new OnlineUserPageInput
            {
                page = currentPage,
                pageSize = pageSize,
                tenantId = IsSuperAdmin ? tenantId : DefaultTenantId,
                userName = userName.Trim(),
                realName = realName.Trim()
            }).Then(outcome =>
            {
                loading = false;
                if (!outcome.Ok)
                {
                    ClearPage(outcome.Error ?? "加载在线用户失败。");
                    return;
                }

                items = OnlineUserClient.ReadItems(outcome.Data);
                rows = items;
                total = OnlineUserClient.ReadTotal(outcome.Data);
                error = null;
            }, _ =>
            {
                loading = false;
                ClearPage("加载在线用户失败，请检查网络后重试。");
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
            userName = string.Empty;
            realName = string.Empty;
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

        private void ForceOffline(string id)
        {
            if (!CanForceOffline || forcing)
                return;

            var row = Find(id);
            if (row is null)
                return;
            if (row.connectionId.Length == 0)
            {
                error = "连接 ID 缺失，无法强制下线。";
                return;
            }

            forcing = true;
            OnlineUserClient.ForceOffline(new OnlineUserForceOfflineInput
            {
                id = row.id,
                tenantId = Optional(row.tenantId),
                connectionId = row.connectionId,
                userId = row.userId,
                userName = row.userName,
                realName = Optional(row.realName),
                time = Optional(row.time),
                ip = Optional(row.ip),
                browser = Optional(row.browser),
                os = Optional(row.os)
            }).Then(outcome =>
            {
                forcing = false;
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "强制下线失败。";
                    return;
                }

                error = null;
                ElementPlus.ElMessageMembers.Success("已发送强制下线指令");
                LoadPage();
            }, _ =>
            {
                forcing = false;
                error = "强制下线失败，请检查网络后重试。";
            });
        }

        private OnlineUserView? Find(string id)
        {
            for (var index = 0; index < items.Length; index++)
                if (items[index].id == id)
                    return items[index];
            return null;
        }

        private void ClearPage(string message)
        {
            items = [];
            rows = [];
            total = 0;
            error = message;
        }

        private static string? Optional(string value)
            => value.Length == 0 ? null : value;

        public OnlineUserPageProps CreateProps() => new()
        {
            Error = error,
            IsSuperAdmin = IsSuperAdmin,
            CanQuery = CanQuery,
            Loading = loading,
            UserName = userName,
            RealName = realName,
            CurrentPage = currentPage,
            PageSize = pageSize,
            Total = total,
            DismissError = () => error = null,
            Search = Search,
            ResetSearch = ResetSearch,
            Refresh = Refresh,
            SetUserName = value => userName = value,
            SetRealName = value => realName = value,
            ChangePage = ChangePage,
            ChangePageSize = ChangePageSize,
            Rows = rows,
            TenantOptions = tenantItems,
            TenantId = tenantId,
            TenantLoading = tenantLoading,
            CanForceOffline = CanForceOffline,
            Forcing = forcing,
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
