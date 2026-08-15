using JazorAdmin.Features.Scheduling;

namespace JazorAdmin;

[ECMAScriptModule("components/schedules")]
public partial class SchedulePage : AppComponentBase, IVueContainerComponent
{
    private bool loading = true;
    private string? error;
    private ScheduleView[] schedules = [];
    private ScheduleRunView[] runs = [];
    private string? selectedKey;
    private string name = string.Empty;
    private string description = string.Empty;
    private string cron = string.Empty;
    private bool enabled;
    private string? nextRunAt;
    private string? lastStatus;
    private string? lastMessage;

    protected override void OnInitialized() => Load();

    private void Load()
    {
        loading = true;
        error = null;
        ApiClient.GetSchedules().Then(ApplySchedules);
    }

    private void ApplySchedules(ApiOutcome outcome)
    {
        loading = false;
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to load schedules.", "无法加载计划任务。");
            return;
        }

        schedules = ApiClient.ToSchedules(outcome.Data);
        if (selectedKey is not null)
        {
            var selected = schedules.FirstOrDefault(item => item.Key == selectedKey);
            if (selected is not null)
            {
                Select(selected);
                return;
            }
        }
        if (schedules.Length > 0)
            Select(schedules[0]);
    }

    private void Select(ScheduleView schedule)
    {
        selectedKey = schedule.Key;
        name = schedule.Name;
        description = schedule.Description;
        cron = schedule.Cron;
        enabled = schedule.Enabled;
        nextRunAt = schedule.NextRunAt;
        lastStatus = schedule.LastStatus;
        lastMessage = schedule.LastMessage;
        LoadRuns();
    }

    private void Save()
    {
        if (selectedKey is null)
            return;

        ApiClient.UpdateSchedule(selectedKey, new ScheduleUpdate(cron, enabled)).Then(ApplySaved);
    }

    private void ApplySaved(ApiOutcome outcome)
    {
        if (!outcome.Ok)
        {
            error = outcome.Error ?? L("Unable to update the schedule.", "无法更新计划任务。");
            return;
        }
        Load();
    }

    private void RunNow()
    {
        if (selectedKey is null)
            return;

        ApiClient.TriggerSchedule(selectedKey).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to trigger the task.", "无法触发该任务。");
                return;
            }
            LoadRuns();
        });
    }

    private void LoadRuns()
    {
        if (selectedKey is null)
            return;

        ApiClient.GetScheduleRuns(selectedKey).Then(outcome =>
        {
            if (!outcome.Ok)
            {
                error = outcome.Error ?? L("Unable to load task runs.", "无法加载任务执行记录。");
                return;
            }
            runs = ApiClient.ToScheduleRuns(outcome.Data);
        });
    }
}
