using Jero.Web.App.Components;
using Jero.Web.App.Shell;
using static ECMAScript.Global;

namespace Jero.Web.App.Job;

[ECMAScriptModule("components/job-dashboard-page-state.js")]
public static class JobDashboardPageModule
{
    public static JobDashboardPageState Create() => new();

    public sealed class JobDashboardPageState : global::Jero.Web.App.IPageState<JobDashboardPageProps>
    {
        private bool loading;
        private int pendingRequests;
        private string? error;
        private JobDashboardView dashboard = new();
        private JobDashboardGroupView[] groupRows = [];
        private JobClusterView[] clusterRows = [];
        private JobView[] jobRows = [];
        private JobTriggerRecordView[] recordRows = [];

        private readonly JeroNavigation Navigation = new();

        public void Initialize()
            => LoadDashboard();

        private void LoadDashboard()
        {
            if (loading)
                return;

            loading = true;
            pendingRequests = 2;
            error = null;

            JobClient.Dashboard().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    error = outcome.Error ?? "加载任务看板失败。";
                }
                else
                {
                    dashboard = JobClient.ReadDashboard(outcome.Data);
                    groupRows = dashboard.groupRows;
                    clusterRows = dashboard.clusterRows;
                    jobRows = dashboard.recentlyUpdatedJobs;
                }
                FinishRequest();
            }, _ =>
            {
                error = "加载任务看板失败，请检查网络后重试。";
                FinishRequest();
            });

            JobClient.TriggerRecords(new JobTriggerRecordPageInput { page = 1, pageSize = 10 }).Then(outcome =>
            {
                if (!outcome.Ok)
                    error = outcome.Error ?? "加载最近执行记录失败。";
                else
                    recordRows = JobClient.ReadRecords(outcome.Data);
                FinishRequest();
            }, _ =>
            {
                error = "加载最近执行记录失败，请检查网络后重试。";
                FinishRequest();
            });
        }

        private void FinishRequest()
        {
            pendingRequests--;
            if (pendingRequests <= 0)
                loading = false;
        }

        private void BackToJobs()
            => Navigation.NavigateTo(Navigation.BaseUri + "job");

        public JobDashboardPageProps CreateProps() => new()
        {
            TotalJobs = dashboard.totalJobs,
            TotalTriggers = dashboard.totalTriggers,
            RunningTriggerCount = dashboard.runningTriggerCount,
            AbnormalTriggerCount = dashboard.abnormalTriggerCount,
            ClusterWorkingCount = dashboard.clusterWorkingCount,
            ClusterWaitingCount = dashboard.clusterWaitingCount,
            PausedTriggerCount = dashboard.pausedTriggerCount,
            Loading = loading,
            Error = error,
            LoadDashboard = LoadDashboard,
            BackToJobs = BackToJobs,
            GroupRows = groupRows,
            ClusterRows = clusterRows,
            JobRows = jobRows,
            RecordRows = recordRows,
        };
    }
}

public sealed record JobDashboardPageProps : VueProps
{
    public required int TotalJobs { get; init; }
    public required int TotalTriggers { get; init; }
    public required int RunningTriggerCount { get; init; }
    public required int AbnormalTriggerCount { get; init; }
    public required int ClusterWorkingCount { get; init; }
    public required int ClusterWaitingCount { get; init; }
    public required int PausedTriggerCount { get; init; }
    public required bool Loading { get; init; }
    public required string? Error { get; init; }
    public required Action LoadDashboard { get; init; }
    public required Action BackToJobs { get; init; }
    public required JobDashboardGroupView[] GroupRows { get; init; }
    public required JobClusterView[] ClusterRows { get; init; }
    public required JobView[] JobRows { get; init; }
    public required JobTriggerRecordView[] RecordRows { get; init; }
}
