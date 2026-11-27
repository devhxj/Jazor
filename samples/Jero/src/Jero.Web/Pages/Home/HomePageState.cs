using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Job;
using Jero.Web.Infrastructure.Api.Notice;
using Jero.Web.Infrastructure.Api.Tenant;
using Jero.Web.Infrastructure.Api.User;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using Jero.Web.Pages.Job;
using Jero.Web.Pages.Notice;
using Jero.Web.Pages.Tenant;
using Jero.Web.Pages.User;

namespace Jero.Web.Pages.Home;

[ECMAScriptModule("components/home-page-state.js")]
public static class HomePageModule
{
    public static HomePageState Create() => new();

    public sealed class HomePageState : global::Jero.Web.Composition.IPageState<HomePageProps>
    {
        private readonly JeroNavigation _navigation = new();

        private CurrentUser? User => ApplicationState.User;

        // 概览卡计数：加载前显示"—"；无权限或网络失败时保持"—"，不弹错不炸页（首页不是错误面）。
        private string _userCount = "—";
        private string _tenantCount = "—";
        private string _jobCount = "—";
        private string _noticeCount = "—";

        public void Initialize()
        {
            UserClient.Page(new UserPageInput { Page = 1, PageSize = 1 })
                .Then(outcome => { if (outcome.Ok) _userCount = UserClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
            TenantClient.Page(new TenantPageInput { Page = 1, PageSize = 1 })
                .Then(outcome => { if (outcome.Ok) _tenantCount = TenantClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
            JobClient.Page(new JobPageInput { Page = 1, PageSize = 1 })
                .Then(outcome => { if (outcome.Ok) _jobCount = JobClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
            NoticeClient.Page(new NoticePageInput { Page = 1, PageSize = 1 })
                .Then(outcome => { if (outcome.Ok) _noticeCount = NoticeClient.ReadTotal(outcome.Data).ToString(); }, _ => { });
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

            // 路由别名和应用基路径由 Shell 管理。工作台只选择对应的菜单项，
            // 避免自行拼接地址。
            _navigation.NavigateTo(_navigation.BaseUri + navigation.Route);
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
            UserCount = _userCount,
            TenantCount = _tenantCount,
            JobCount = _jobCount,
            NoticeCount = _noticeCount,
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
