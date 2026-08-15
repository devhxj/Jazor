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
    private DateTimeOffset _startedAt;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string ScheduleKey { get; set; } = string.Empty;

    public Schedule Schedule { get; set; } = null!;

    public string Trigger { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset StartedAt
    {
        get => _startedAt;
        set
        {
            _startedAt = value;
            StartedAtUtc = value.UtcDateTime;
        }
    }

    // SQLite cannot order or compare DateTimeOffset in SQL. Keep the authored timestamp for the
    // API, and persist its exact UTC instant separately as the query/index key.
    // SQLite 不能在 SQL 中排序或比较 DateTimeOffset；原始时间保留给 API，精确 UTC 时刻单独
    // 持久化为查询与索引键，避免按本地 offset 或插入顺序近似排序。
    public DateTime? StartedAtUtc { get; private set; }

    public DateTimeOffset? FinishedAt { get; set; }

    public string? Message { get; set; }

    // Existing rows predate StartedAtUtc. Migration startup calls this after materialization so the
    // backfill uses DateTimeOffset's exact UTC conversion instead of SQLite text/date functions.
    // 历史行早于 StartedAtUtc 列；迁移启动后按该方法回填，避免 SQLite 文本日期函数丢失精度。
    internal void NormalizeStartedAtUtc() => StartedAtUtc = StartedAt.UtcDateTime;
}
