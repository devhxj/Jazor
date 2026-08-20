// Defines the append-only audit record used by the administration operation log.
// 定义管理操作日志的只追加审计记录；查询使用 UTC 键，展示保留原始 ISO 时间。
namespace JazorAdmin.Data;

public sealed class AuditEvent
{
    private DateTimeOffset _occurredAt;

    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset OccurredAt
    {
        get => _occurredAt;
        set
        {
            _occurredAt = value;
            OccurredAtUtc = value.UtcDateTime;
        }
    }

    // SQLite cannot compare DateTimeOffset values in SQL. Keep the UTC instant as the indexed
    // query key so time-range filters remain database-side and deterministic.
    // SQLite 不能在 SQL 中比较 DateTimeOffset；索引 UTC 时刻让时间范围筛选保持在数据库侧。
    public DateTime OccurredAtUtc { get; private set; }

    public string? ActorId { get; set; }

    public string? ActorName { get; set; }

    public string Action { get; set; } = string.Empty;

    public string ObjectType { get; set; } = string.Empty;

    public string ObjectId { get; set; } = string.Empty;

    public string? Summary { get; set; }
}
