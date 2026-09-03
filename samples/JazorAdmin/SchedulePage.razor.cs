using ECMAScript.TDesign;
using JazorAdmin.Features.Scheduling;
using Microsoft.AspNetCore.Components;

namespace JazorAdmin;

[ECMAScriptModule("components/schedules")]
public partial class SchedulePage : AppComponentBase, IVueContainerComponent
{
    private sealed record ScheduleDraft
    {
        public string Cron { get; set; } = string.Empty;

        public bool Enabled { get; set; }
    }

    private bool loading = true;
    private string? error;
    private ScheduleView[] schedules = [];
    private ScheduleRunView[] runs = [];
    private string? selectedKey;
    private string name = string.Empty;
    private string description = string.Empty;
    private ScheduleDraft Draft { get; set; } = new();
    private string? nextRunAt;
    private string? lastStatus;
    private string? lastMessage;

    private TFormRules<ScheduleDraft> DraftRules { get; } = new()
    {
        ["cron"] =
        [
            new TFormRule { Required = true, Message = "Enter a Quartz Cron expression." }
        ]
    };

    // 任务表首列承载 data-schedule-key，执行历史表承载 data-schedule-run，
    // 浏览器验证通过这些锚点断言选择态与手动执行结果。
    private TPrimaryTableCol<ScheduleView>[] Columns =>
    [
        new() { Title = (TPrimaryTableColTitle<ScheduleView>)L("Task", "任务"), Cell = (TPrimaryTableColCell<ScheduleView>)((RenderFragment<TPrimaryTableCellParams<ScheduleView>>)(context => builder =>
            {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "data-schedule-key", context.Row.Key);
        builder.OpenElement(2, "strong");
        builder.AddContent(3, context.Row.Name);
        builder.CloseElement();
        builder.OpenElement(4, "span");
        builder.AddContent(5, context.Row.Description);
        builder.CloseElement();
        builder.CloseElement();
            })) },
        new() { Title = (TPrimaryTableColTitle<ScheduleView>)L("Schedule", "调度表达式"), Cell = (TPrimaryTableColCell<ScheduleView>)((RenderFragment<TPrimaryTableCellParams<ScheduleView>>)(context => builder =>
            {
        builder.OpenElement(0, "code");
        builder.AddContent(1, context.Row.Cron);
        builder.CloseElement();
            })) },
        new() { Title = (TPrimaryTableColTitle<ScheduleView>)L("Status", "状态"), Cell = (TPrimaryTableColCell<ScheduleView>)((RenderFragment<TPrimaryTableCellParams<ScheduleView>>)(context => builder =>
            {
        builder.OpenComponent<TTag>(0);
        builder.AddComponentParameter(1, nameof(TTag.Theme),
            context.Row.Enabled ? TTagThemeValue.Success : TTagThemeValue.Default);
        builder.AddComponentParameter(2, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, context.Row.Enabled ? L("Enabled", "已启用") : L("Paused", "已暂停"))));
        builder.CloseComponent();
            })) },
        new() { Title = (TPrimaryTableColTitle<ScheduleView>)L("Actions", "操作"), Cell = (TPrimaryTableColCell<ScheduleView>)((RenderFragment<TPrimaryTableCellParams<ScheduleView>>)(context => builder =>
            {
        builder.OpenComponent<TButton>(0);
        builder.AddComponentParameter(1, nameof(TButton.Variant), TButtonVariantValue.Text);
        builder.AddComponentParameter(2, nameof(TButton.Size), TSizeEnum.Small);
        builder.AddComponentParameter(3, nameof(TButton.OnClick),
            EventCallback.Factory.Create(this, () => Select(context.Row)));
        builder.AddComponentParameter(4, nameof(TContentComponentBase.ChildContent),
            (RenderFragment)(child => child.AddContent(0, L("Manage", "管理"))));
        builder.CloseComponent();
            })) }
    ];

    private TPrimaryTableCol<ScheduleRunView>[] RunColumns =>
    [
        new() { Title = (TPrimaryTableColTitle<ScheduleRunView>)L("Started", "开始时间"), Cell = (TPrimaryTableColCell<ScheduleRunView>)((RenderFragment<TPrimaryTableCellParams<ScheduleRunView>>)(context => builder =>
            {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "data-schedule-run", context.Row.Id);
        builder.AddContent(2, context.Row.StartedAt);
        builder.CloseElement();
            })) },
        new() { ColKey = "Trigger", Title = (TPrimaryTableColTitle<ScheduleRunView>)L("Trigger", "触发方式") },
        new() { ColKey = "Status", Title = (TPrimaryTableColTitle<ScheduleRunView>)L("Status", "状态") },
        new() { Title = (TPrimaryTableColTitle<ScheduleRunView>)L("Message", "消息"), Cell = (TPrimaryTableColCell<ScheduleRunView>)((RenderFragment<TPrimaryTableCellParams<ScheduleRunView>>)(context => builder =>
            {
        builder.AddContent(0, context.Row.Message ?? "-");
            })) }
    ];

    private TTableRowClassNameValue<ScheduleView> SelectedRowClassName
        => (TTableRowClassNameValueOption2<ScheduleView>)SelectedRowClass;

    private TClassName SelectedRowClass(TRowClassNameParams<ScheduleView> parameters)
        => parameters.Row.Key == selectedKey ? (TClassName)"ja-table-row-selected" : (TClassName)string.Empty;

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
        Draft = NewDraft(schedule);
        nextRunAt = schedule.NextRunAt;
        lastStatus = schedule.LastStatus;
        lastMessage = schedule.LastMessage;
        LoadRuns();
    }

    private void Save(TSubmitContext<ScheduleDraft> context)
    {
        if (selectedKey is null)
            return;

        ApiClient.UpdateSchedule(selectedKey, new ScheduleUpdate(Draft.Cron, Draft.Enabled)).Then(ApplySaved);
    }

    private void ResetDraft(TFormResetEventContext<ScheduleDraft> context)
    {
        var selected = schedules.FirstOrDefault(item => item.Key == selectedKey);
        if (selected is not null)
            Draft = NewDraft(selected);
        error = null;
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

    private static ScheduleDraft NewDraft(ScheduleView schedule) => new()
    {
        Cron = schedule.Cron,
        Enabled = schedule.Enabled
    };
}
