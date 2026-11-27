using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Shell;
using Jero.Web.Infrastructure.Storage;
using Jero.Web.Pages.Auth;
using Jero.Web.Pages.Profile;
using static ECMAScript.Global;
using static ECMAScript.VueRoute;

namespace Jero.Web.Pages.Shell;

[ECMAScriptModule("components/shell-state.js")]
public static class ShellState
{
    public const string LockKey = "jero.jazor.lock-screen.active";
    public const string LockPasswordKey = "jero.jazor.lock-screen.password";
    private static ShellSnapshot _state = null!;
    private static IPromise<bool>? _pending;
    private static int _sessionGeneration;
    private static bool _initialized;

    public static ShellSnapshot GetState()
    {
        if (!_initialized)
        {
            _state = Reactive(new ShellSnapshot());
            _initialized = true;
            ApplicationState.BindUserChanged(user => _state.User = user);
            ApplicationState.BindBrandChanged(brand => _state.Brand = brand);
            ProfileStateChange.Bind(UpdateProfile);
        }
        return _state;
    }

    public static IPromise<bool> Refresh()
    {
        GetState();
        if (_pending is not null)
            return _pending;

        var generation = _sessionGeneration;
        _state.Loading = true;
        _pending = new Promise<bool>((Action<bool> resolve, Action<Error> reject) =>
        {
            ShellClient.Load().Then(snapshot => resolve(ApplySnapshot(snapshot, generation)), error =>
            {
                if (generation == _sessionGeneration)
                    _state.Error = "加载应用信息失败，请检查网络后刷新页面。";
                resolve(false);
            });
        }).Finally(() =>
        {
            // 退出后可能已开始新的加载，旧请求的回调不能清掉新的 pending。
            if (generation == _sessionGeneration)
            {
                _state.Loading = false;
                _pending = null;
            }
        });
        return _pending;
    }

    private static bool ApplySnapshot(AppBootstrapState snapshot, int generation)
    {
        // 退出会作废在途请求，防止迟到的响应写回已清空的会话。
        if (generation != _sessionGeneration)
            return false;
        if (!snapshot.Ok && snapshot.Unauthorized)
        {
            Clear();
            return false;
        }
        if (!snapshot.Ok)
        {
            _state.Error = snapshot.Error ?? "加载应用信息失败。";
            return false;
        }

        ApplicationState.Apply(snapshot);
        _state.User = snapshot.User;
        _state.Brand = snapshot.Brand;
        _state.Navigation = snapshot.Navigation;
        _state.Error = null;
        _state.Ready = true;
        return true;
    }

    public static void Clear()
    {
        GetState();
        _sessionGeneration++;
        _pending = null;
        BrowserSessionStore.Clear();
        ApplicationState.Clear();
        Window.LocalStorage.RemoveItem(LockKey);
        Window.LocalStorage.RemoveItem(LockPasswordKey);
        _state.User = null;
        _state.Brand = new AppBrandState();
        _state.Navigation = [];
        _state.Ready = false;
        _state.Loading = false;
        _state.Error = null;
    }

    public static string ResolveHome(string? path = null)
        => "/" + RouteRegistry.ResolveHome(path ?? GetState().User?.HomePath);

    public static IPromise<RouteNavigationResult?> CompleteSignIn(Router router, string? accessToken, string? refreshToken, string? home)
    {
        Clear();
        BrowserSessionStore.SetTokens(accessToken, refreshToken);
        return router.Push(ResolveHome(home));
    }

    // 受保护路由才需要加载用户和权限；登录页可在未建立会话时进入。
    public static IPromise<NavigationGuardReturn?> GuardNavigation(RouteLocationNormalized to)
        => GetState().Ready
            ? Promise<NavigationGuardReturn?>.Resolve(GuardAfterRefresh(to))
            : Refresh().Then(_ => GuardAfterRefresh(to));

    private static NavigationGuardReturn? GuardAfterRefresh(RouteLocationNormalized to)
    {
        if (!BrowserSessionStore.HasSession)
            return (NavigationGuardReturn)"/login";

        if (to.Path == "/home" && to.RedirectedFrom?.Path == "/")
        {
            var home = ResolveHome();
            if (home != "/home")
                return (NavigationGuardReturn)home;
        }

        return true;
    }

    public static bool HasPermission(string name)
        => ApplicationState.HasPermission(name);

    public static AppIframeDestination? FindIframe(string menuId)
    {
        var item = GetState().Navigation.FirstOrDefault(entry => entry.IsIframe && entry.Id == menuId);
        return item is null ? null : new AppIframeDestination(item.Title, item.ExternalUrl);
    }

    private static void UpdateProfile(ProfileShellChange change)
    {
        var user = _state.User;
        if (user is null)
            return;
        ApplicationState.UpdateUser(new CurrentUser
        {
            Id = user.Id,
            Account = user.Account,
            RealName = change.RealName ?? user.RealName,
            Avatar = change.Avatar ?? user.Avatar,
            OrgName = user.OrgName,
            PosName = user.PosName,
            WatermarkText = user.WatermarkText,
            LangCode = user.LangCode,
            HomePath = user.HomePath,
            TenantId = user.TenantId,
            CurrentTenantId = user.CurrentTenantId,
            AccountType = user.AccountType,
            Buttons = user.Buttons,
            RoleIds = user.RoleIds
        });
    }

    public sealed class ShellSnapshot
    {
        public CurrentUser? User;
        public AppBrandState Brand = new();
        public AppNavigationItem[] Navigation = [];
        public bool Ready;
        public bool Loading;
        public string? Error;
    }
}
