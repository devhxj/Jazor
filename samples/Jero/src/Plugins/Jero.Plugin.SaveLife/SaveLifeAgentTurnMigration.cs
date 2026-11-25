using SqlSugar;

namespace Jero.Plugin.SaveLife;

public static class SaveLifeAgentTurnMigration
{
    public const string TurnSequenceUniqueIndexName = "uq_sl_agent_message_conversation_turn";

    public static void Apply(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (db.CurrentConnectionConfig.DbType != DbType.Sqlite)
            throw new NotSupportedException($"当前数据库 {db.CurrentConnectionConfig.DbType} 尚未实现会话轮次迁移");

        db.CodeFirst.InitTables(
            typeof(SaveLifeAgentConversation),
            typeof(SaveLifeAgentMessage));
        db.Ado.BeginTran();
        try
        {
            BackfillTurns(db);
            db.Ado.ExecuteCommand($"""
                CREATE UNIQUE INDEX IF NOT EXISTS "{TurnSequenceUniqueIndexName}"
                ON "sl_agent_message" ("ConversationId", "TurnSequence")
                WHERE "Role" = 'user'
                  AND "TurnSequence" IS NOT NULL
                  AND "IsDelete" = 0
                """);
            db.Ado.CommitTran();
        }
        catch
        {
            db.Ado.RollbackTran();
            throw;
        }
    }

    private static void BackfillTurns(ISqlSugarClient db)
    {
        var conversations = db.Queryable<SaveLifeAgentConversation>()
            .ClearFilter()
            .ToList();
        var messages = db.Queryable<SaveLifeAgentMessage>()
            .ClearFilter()
            .OrderBy(item => item.CreateTime)
            .OrderBy(item => item.Id)
            .ToList();
        var repliesByUserMessageId = messages
            .Where(item => item.Role == "assistant" && item.ReplyToMessageId.HasValue)
            .GroupBy(item => item.ReplyToMessageId!.Value)
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.Id).First());

        foreach (var conversation in conversations)
        {
            var userMessages = messages
                .Where(item => item.ConversationId == conversation.Id && item.Role == "user")
                .OrderBy(item => item.CreateTime)
                .ThenBy(item => item.Id)
                .ToList();
            var needsSequenceBackfill = userMessages.Any(item => item.TurnSequence is not > 0);
            if (needsSequenceBackfill)
            {
                for (var index = 0; index < userMessages.Count; index++)
                    userMessages[index].TurnSequence = index + 1L;
            }

            foreach (var message in userMessages)
            {
                var originalStatus = message.TurnStatus;
                var normalizedStatus = NormalizeTurnStatus(
                    message.TurnStatus,
                    repliesByUserMessageId.GetValueOrDefault(message.Id));
                message.TurnStatus = normalizedStatus;
                if (!needsSequenceBackfill && originalStatus == normalizedStatus)
                    continue;

                db.Updateable(message)
                    .UpdateColumns(item => new
                    {
                        item.TurnSequence,
                        item.TurnStatus,
                        item.TurnStateRevision
                    })
                    .ExecuteCommand();
            }

            var nextSequence = userMessages.Count == 0
                ? 1
                : checked(userMessages.Max(item => item.TurnSequence!.Value) + 1);
            var normalizedNextSequence = Math.Max(Math.Max(1, conversation.NextTurnSequence), nextSequence);
            var normalizedStateRevision = Math.Max(0, conversation.StateRevision);
            if (conversation.NextTurnSequence == normalizedNextSequence &&
                conversation.StateRevision == normalizedStateRevision)
            {
                continue;
            }

            conversation.NextTurnSequence = normalizedNextSequence;
            conversation.StateRevision = normalizedStateRevision;
            db.Updateable(conversation)
                .UpdateColumns(item => new
                {
                    item.NextTurnSequence,
                    item.StateRevision
                })
                .ExecuteCommand();
        }
    }

    private static string NormalizeTurnStatus(
        string? currentStatus,
        SaveLifeAgentMessage? assistantReply)
    {
        if (currentStatus is SaveLifeAgentTurnStatuses.Completed or
            SaveLifeAgentTurnStatuses.Failed or
            SaveLifeAgentTurnStatuses.Superseded)
        {
            return currentStatus;
        }

        return assistantReply?.GenerationStatus switch
        {
            "completed" => SaveLifeAgentTurnStatuses.Completed,
            "failed" => SaveLifeAgentTurnStatuses.Failed,
            _ => SaveLifeAgentTurnStatuses.Pending
        };
    }
}
