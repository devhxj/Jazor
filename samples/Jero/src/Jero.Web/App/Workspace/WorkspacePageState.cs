using static ECMAScript.Global;
using Jero.Web.App.Components;
using Jero.Web.App.Shell;

namespace Jero.Web.App.Workspace;

[ECMAScriptModule("components/workspace-page-state.js")]
public static class WorkspacePageModule
{
    public static WorkspacePageState Create() => new();

    public sealed class WorkspacePageState : global::Jero.Web.App.IPageState<WorkspacePageProps>
    {
        private readonly JeroNavigation Navigation = new();

        private CurrentUser? User => ApplicationState.User;

        private string UserName
            => !string.IsNullOrWhiteSpace(User?.RealName)
                ? User!.RealName
                : !string.IsNullOrWhiteSpace(User?.Account) ? User!.Account : "管理员";

        private string Avatar => User?.Avatar ?? string.Empty;

        private void MountChart() => WorkspaceChartBrowserActions.Mount();
        private void DestroyChart() => WorkspaceChartBrowserActions.Destroy();
        private static void OpenProject(string url) => Window.Open(url, "_blank", "noopener=yes,noreferrer=yes");
        private void OpenQuickNav(string route) => Navigation.NavigateTo(Navigation.BaseUri + route);

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
