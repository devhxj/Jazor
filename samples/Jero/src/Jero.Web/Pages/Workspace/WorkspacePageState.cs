using Jero.Web.Application;
using static ECMAScript.Global;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;

namespace Jero.Web.Pages.Workspace;

[ECMAScriptModule("components/workspace-page-state.js")]
public static class WorkspacePageModule
{
    public static WorkspacePageState Create() => new();

    public sealed class WorkspacePageState : global::Jero.Web.Composition.IPageState<WorkspacePageProps>
    {
        private readonly JeroNavigation _navigation = new();

        private CurrentUser? User => ApplicationState.User;

        private string UserName
            => !string.IsNullOrWhiteSpace(User?.RealName)
                ? User!.RealName
                : !string.IsNullOrWhiteSpace(User?.Account) ? User!.Account : "管理员";

        private string Avatar => User?.Avatar ?? string.Empty;

        private void MountChart() => WorkspaceChartBrowserActions.Mount();
        private void DestroyChart() => WorkspaceChartBrowserActions.Destroy();
        private static void OpenProject(string url) => Window.Open(url, "_blank", "noopener=yes,noreferrer=yes");
        private void OpenQuickNav(string route) => _navigation.NavigateTo(_navigation.BaseUri + route);

        public void Initialize() { }
        public WorkspacePageProps CreateProps() => new()
        {
            UserName = UserName,
            Avatar = Avatar,
            MountChart = MountChart,
            DestroyChart = DestroyChart,
            OpenProject = OpenProject,
            OpenQuickNav = OpenQuickNav,
        };
    }
}

public sealed record WorkspacePageProps : VueProps
{
    public required string UserName { get; init; }
    public required string Avatar { get; init; }
    public required Action MountChart { get; init; }
    public required Action DestroyChart { get; init; }
    public required Action<string> OpenProject { get; init; }
    public required Action<string> OpenQuickNav { get; init; }
}
