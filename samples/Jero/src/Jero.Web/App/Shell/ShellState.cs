using Jero.Web.App.Auth;
using Jero.Web.App.Profile;
using static ECMAScript.Global;
using static ECMAScript.VueRoute;

namespace Jero.Web.App.Shell;

[ECMAScriptModule("components/shell-state.js")]
public static class ShellState
{
    public const string LockKey = "jero.jazor.lock-screen.active";
    public const string LockPasswordKey = "jero.jazor.lock-screen.password";
    private static ShellSnapshot state = null!;
    private static IPromise<bool>? pending;
    private static int sessionGeneration;
    private static bool initialized;

    public static ShellSnapshot GetState()
    {
        if (!initialized)
        {
            state = Reactive(new ShellSnapshot());
            initialized = true;
            ApplicationState.BindUserChanged(user => state.User = user);
            ApplicationState.BindBrandChanged(brand => state.Brand = brand);
            ProfileStateChange.Bind(UpdateProfile);
        }
        return state;
    }

    public static IPromise<bool> Refresh()
    {
        GetState();
        if (pending is not null)
            return pending;

        var generation = sessionGeneration;
        state.Loading = true;
        pending = new Promise<bool>((Action<bool> resolve, Action<Error> reject) =>
        {
            ShellClient.Load().Then(snapshot => resolve(ApplySnapshot(snapshot, generation)), error =>
            {
                if (generation == sessionGeneration)
                    state.Error = "加载应用信息失败，请检查网络后刷新页面。";
                resolve(false);
            });
        }).Finally(() =>
        {
            if (generation == sessionGeneration)
            {
                state.Loading = false;
                pending = null;
            }
        });
        return pending;
    }

    private static bool ApplySnapshot(AppBootstrapState snapshot, int generation)
    {
        // Logout invalidates the in-flight request before it can restore its session.
        if (generation != sessionGeneration)
            return false;
        if (!snapshot.Ok && snapshot.Unauthorized)
        {
            Clear();
            return false;
        }
        if (!snapshot.Ok)
        {
            state.Error = snapshot.Error ?? "加载应用信息失败。";
            return false;
        }

        ApplicationState.Apply(snapshot);
        state.User = snapshot.User;
        state.Brand = snapshot.Brand;
        state.Navigation = snapshot.Navigation;
        state.Error = null;
        state.Ready = true;
        return true;
    }

    public static void Clear()
    {
        GetState();
        sessionGeneration++;
        pending = null;
        BrowserSessionStore.Clear();
        ApplicationState.Clear();
        Window.LocalStorage.RemoveItem(LockKey);
        Window.LocalStorage.RemoveItem(LockPasswordKey);
        state.User = null;
        state.Brand = new AppBrandState();
        state.Navigation = [];
        state.Ready = false;
        state.Loading = false;
        state.Error = null;
    }

    public static string ResolveHome(string? path = null)
        => "/" + RouteRegistry.ResolveHome(path ?? GetState().User?.HomePath);

    public static IPromise<RouteNavigationResult?> CompleteSignIn(Router router, string? accessToken, string? refreshToken, string? home)
    {
        Clear();
        BrowserSessionStore.SetTokens(accessToken, refreshToken);
        return router.Push(ResolveHome(home));
    }

    // Only protected navigation loads the shell and refreshes the session.
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
        var user = state.User;
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
