using SqlSugar;

namespace Jero.Plugin.SaveLife;

public static class SaveLifeAgentCommandMigration
{
    public const string CommandScopeUniqueIndexName = "uq_sl_agent_business_command_scope";

    public static void Apply(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.CurrentConnectionConfig.DbType != DbType.Sqlite)
        {
            throw new NotSupportedException(
                $"当前数据库 {db.CurrentConnectionConfig.DbType} 尚未实现业务命令迁移");
        }

        db.CodeFirst.InitTables(typeof(SaveLifeAgentBusinessCommand));
        db.Ado.ExecuteCommand($"""
            CREATE UNIQUE INDEX IF NOT EXISTS "{CommandScopeUniqueIndexName}"
            ON "sl_agent_business_command" (
                IFNULL("TenantId", 0),
                "ExternalUserId",
                "ConversationId",
                "CommandType",
                "ClientCommandId"
            )
            """);
    }
}
