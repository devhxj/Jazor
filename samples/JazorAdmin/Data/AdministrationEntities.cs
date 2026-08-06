// Defines persisted system settings and Quartz-managed schedule metadata.
// 配置中心和 Quartz 调度任务的持久化元数据；任务执行由 Quartz 负责。
namespace JazorAdmin.Data;

public sealed class Setting
{
    public string Key { get; set; } = string.Empty;

    public string Group { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Kind { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class Schedule
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Cron { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public DateTimeOffset? LastRunAt { get; set; }

    public string? LastStatus { get; set; }

    public string? LastMessage { get; set; }

    public ICollection<ScheduleRun> Runs { get; } = new List<ScheduleRun>();
}

public sealed class ScheduleRun
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ScheduleKey { get; set; } = string.Empty;

    public Schedule Schedule { get; set; } = null!;

    public string Trigger { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public string? Message { get; set; }
}
