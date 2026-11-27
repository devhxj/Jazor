using Jero.Web.Application;
using Jero.Web.Infrastructure.Api.Job;
using Jero.Web.Pages.Components;
using Jero.Web.Pages.Shell;
using static ECMAScript.Global;

namespace Jero.Web.Pages.Job;

[ECMAScriptModule("components/job-dashboard-page-state.js")]
public static class JobDashboardPageModule
{
    public static JobDashboardPageState Create() => new();

    public sealed class JobDashboardPageState : global::Jero.Web.Composition.IPageState<JobDashboardPageProps>
    {
        private bool _loading;
        private int _pendingRequests;
        private string? _error;
        private JobDashboardView _dashboard = new();
        private JobDashboardGroupView[] _groupRows = [];
        private JobClusterView[] _clusterRows = [];
        private JobView[] _jobRows = [];
        private JobTriggerRecordView[] _recordRows = [];

        private readonly JeroNavigation _navigation = new();

        public void Initialize()
            => LoadDashboard();

        private void LoadDashboard()
        {
            if (_loading)
                return;

            _loading = true;
            _pendingRequests = 2;
            _error = null;

            JobClient.Dashboard().Then(outcome =>
            {
                if (!outcome.Ok)
                {
                    _error = outcome.Error ?? "加载任务看板失败。";
                }
                else
                {
                    _dashboard = JobClient.ReadDashboard(outcome.Data);
                    _groupRows = _dashboard.GroupRows;
                    _clusterRows = _dashboard.ClusterRows;
                    _jobRows = _dashboard.RecentlyUpdatedJobs;
                }
                FinishRequest();
            }, _ =>
            {
                _error = "加载任务看板失败，请检查网络后重试。";
                FinishRequest();
            });

            JobClient.TriggerRecords(new JobTriggerRecordPageInput { Page = 1, PageSize = 10 }).Then(outcome =>
            {
                if (!outcome.Ok)
                    _error = outcome.Error ?? "加载最近执行记录失败。";
                else
                    _recordRows = JobClient.ReadRecords(outcome.Data);
                FinishRequest();
            }, _ =>
            {
                _error = "加载最近执行记录失败，请检查网络后重试。";
                FinishRequest();
            });
        }

        private void FinishRequest()
        {
            _pendingRequests--;
            if (_pendingRequests <= 0)
                _loading = false;
        }

        private void BackToJobs()
            => _navigation.NavigateTo(_navigation.BaseUri + "job");

        public JobDashboardPageProps CreateProps() => new()
        {
            TotalJobs = _dashboard.TotalJobs,
            TotalTriggers = _dashboard.TotalTriggers,
            RunningTriggerCount = _dashboard.RunningTriggerCount,
            AbnormalTriggerCount = _dashboard.AbnormalTriggerCount,
            ClusterWorkingCount = _dashboard.ClusterWorkingCount,
            ClusterWaitingCount = _dashboard.ClusterWaitingCount,
            PausedTriggerCount = _dashboard.PausedTriggerCount,
            Loading = _loading,
            Error = _error,
            LoadDashboard = LoadDashboard,
            BackToJobs = BackToJobs,
            GroupRows = _groupRows,
            ClusterRows = _clusterRows,
            JobRows = _jobRows,
            RecordRows = _recordRows,
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
