using SqlSugar;

namespace Jero.Plugin.SaveLife;

public static class SaveLifeAgentConversationMigration
{
    public const string ActiveAccountUniqueIndexName = "uq_sl_agent_conversation_active_account";

    public static SaveLifeAgentConversationMigrationReport Apply(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.CurrentConnectionConfig.DbType != DbType.Sqlite)
        {
            throw new NotSupportedException(
                $"当前数据库 {db.CurrentConnectionConfig.DbType} 尚未实现活跃会话唯一约束迁移");
        }

        db.Ado.BeginTran();
        try
        {
            var report = ReadConflictReport(db);
            if (report.AccountConflictCount > 0)
                throw new SaveLifeAgentConversationMigrationConflictException(report);

            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{ActiveAccountUniqueIndexName}"
                ON "sl_agent_conversation" (
                    IFNULL("TenantId", 0),
                    "ExternalUserId"
                )
                WHERE "Status" = 'active'
                  AND "IsDelete" = 0
                """);
            db.Ado.CommitTran();
            return report;
        }
        catch
        {
            db.Ado.RollbackTran();
            throw;
        }
    }

    private static SaveLifeAgentConversationMigrationReport ReadConflictReport(ISqlSugarClient db)
    {
        return db.Ado.SqlQuery<SaveLifeAgentConversationMigrationReport>("""
            SELECT
                COUNT(DISTINCT "TenantScope") AS "TenantConflictCount",
                COUNT(*) AS "AccountConflictCount",
                IFNULL(MAX("DuplicateCount"), 0) AS "MaxDuplicateCount"
            FROM (
                SELECT
                    IFNULL("TenantId", 0) AS "TenantScope",
                    "ExternalUserId",
                    COUNT(*) AS "DuplicateCount"
                FROM "sl_agent_conversation"
                WHERE "Status" = 'active'
                  AND "IsDelete" = 0
                GROUP BY IFNULL("TenantId", 0), "ExternalUserId"
                HAVING COUNT(*) > 1
            ) AS "Conflicts"
            """).Single();
    }
}

public sealed class SaveLifeAgentConversationMigrationReport
{
    public int TenantConflictCount { get; set; }

    public int AccountConflictCount { get; set; }

    public int MaxDuplicateCount { get; set; }
}

public sealed class SaveLifeAgentConversationMigrationConflictException : InvalidOperationException
{
    public SaveLifeAgentConversationMigrationConflictException(
        SaveLifeAgentConversationMigrationReport report)
        : base(
            $"活跃会话存在历史冲突：租户范围数={report.TenantConflictCount}，" +
            $"账号冲突数={report.AccountConflictCount}，最大重复数={report.MaxDuplicateCount}；迁移已回滚")
    {
        Report = report;
    }

    public SaveLifeAgentConversationMigrationReport Report { get; }
}
