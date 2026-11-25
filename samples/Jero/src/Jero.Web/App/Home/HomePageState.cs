using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using Jero.Web.App.Job;
using Jero.Web.App.Notice;
using Jero.Web.App.Tenant;
using Jero.Web.App.User;

namespace Jero.Web.App.Home;

[ECMAScriptModule("components/home-page-state.js")]
public static class HomePageModule
{
    public static HomePageState Create() => new();

    public sealed class HomePageState : global::Jero.Web.App.IPageState<HomePageProps>
    {
        private readonly JeroNavigation Navigation = new();

        private CurrentUser? User => ApplicationState.User;

        // 概览卡计数：加载前显示"—"；无权限或网络失败时保持"—"，不弹错不炸页（首页不是错误面）。
        private string userCount = "—";
        private string tenantCount = "—";
        private string jobCount = "—";
        private string noticeCount = "—";

        public void Initialize()
        {
            UserClient.Page(new UserPageInput { page = 1, pageSize = 1 })
                .Then(outcome => { if (outcome.Ok) userCount = UserClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
            TenantClient.Page(new TenantPageInput { page = 1, pageSize = 1 })
                .Then(outcome => { if (outcome.Ok) tenantCount = TenantClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
            JobClient.Page(new JobPageInput { page = 1, pageSize = 1 })
                .Then(outcome => { if (outcome.Ok) jobCount = JobClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
            NoticeClient.Page(new NoticePageInput { page = 1, pageSize = 1 })
                .Then(outcome => { if (outcome.Ok) noticeCount = NoticeClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
        }

        private string UserName
            => string.IsNullOrWhiteSpace(User?.RealName) ? "管理员" : User!.RealName;

        private string Avatar => User?.Avatar ?? string.Empty;

        private bool CanNavigate(string backendPath)
        {
            var navigation = ResolveNavigation(backendPath);
            return navigation is not null && navigation.IsSupported;
        }

        private void OpenQuickNav(string backendPath)
        {
            var navigation = ResolveNavigation(backendPath);
            if (navigation is null || !navigation.IsSupported)
                return;

            // Route aliases and the application base stay owned by the shell; workbench entries
            // only select the typed menu item, avoiding hand-built host URLs.
            Navigation.NavigateTo(Navigation.BaseUri + navigation.Route);
        }

        private static AppNavigationItem? ResolveNavigation(string backendPath)
        {
            var navigation = ApplicationState.Navigation;
            for (var index = 0; index < navigation.Length; index++)
            {
                if (navigation[index].BackendPath == backendPath)
                    return navigation[index];
            }

            return null;
        }

        public HomePageProps CreateProps() => new()
        {
            UserName = UserName,
            Avatar = Avatar,
            UserCount = userCount,
            TenantCount = tenantCount,
            JobCount = jobCount,
            NoticeCount = noticeCount,
            CanNavigate = CanNavigate,
            OpenQuickNav = OpenQuickNav,
        };
    }
}

public sealed record HomePageProps : VueProps
{
    public required string UserName { get; init; }
    public required string Avatar { get; init; }
    public required string UserCount { get; init; }
    public required string TenantCount { get; init; }
    public required string JobCount { get; init; }
    public required string NoticeCount { get; init; }
    public required Func<string, bool> CanNavigate { get; init; }
    public required Action<string> OpenQuickNav { get; init; }
}
