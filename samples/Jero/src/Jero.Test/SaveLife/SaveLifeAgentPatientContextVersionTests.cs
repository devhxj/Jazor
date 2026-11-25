using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentPatientContextVersionTests
{
    [Fact]
    public void BindNewMessage_CapturesConversationPatientContextVersion()
    {
        var conversation = Conversation(101, "openid-1");
        conversation.CurrentPatientProfileId = 501;
        conversation.PatientContextVersion = 7;
        var message = Message(1001, 101, "openid-1", "user");

        SaveLifeAgentPatientBinding.BindNewMessage(conversation, message);

        Assert.Equal(501, message.PatientProfileId);
        Assert.Equal(7, message.PatientContextVersion);
        Assert.Null(message.ContextSourceMessageId);
    }

    [Fact]
    public void BindReply_CopiesUserMessagePatientContextSnapshotAndSource()
    {
        var userMessage = Message(1001, 101, "openid-1", "user");
        userMessage.PatientProfileId = 501;
        userMessage.PatientContextVersion = 7;
        userMessage.ContextSourceMessageId = 901;
        var assistantMessage = Message(1002, 101, "openid-1", "assistant");

        SaveLifeAgentPatientBinding.BindReply(userMessage, assistantMessage);

        Assert.Equal(501, assistantMessage.PatientProfileId);
        Assert.Equal(7, assistantMessage.PatientContextVersion);
        Assert.Equal(901, assistantMessage.ContextSourceMessageId);
    }

    [Fact]
    public void Apply_WithLegacyAgentTables_AddsAndBackfillsPatientContextColumns()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifeUserProfile),
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage));
            db.Insertable(new SaveLifeUserProfile
            {
                Id = 11,
                ExternalUserId = "openid-1",
                RealName = "张三",
                CreateTime = new DateTime(2026, 7, 14)
            }).ExecuteCommand();
            db.Insertable(Conversation(101, "openid-1")).ExecuteCommand();
            db.Insertable(Message(1001, 101, "openid-1", "user")).ExecuteCommand();
            DropColumn(db, "sl_agent_conversation", nameof(SaveLifeAgentConversation.PatientContextVersion));
            DropColumn(db, "sl_agent_message", nameof(SaveLifeAgentMessage.PatientContextVersion));
            DropColumn(db, "sl_agent_message", nameof(SaveLifeAgentMessage.ContextSourceMessageId));

            SaveLifePatientProfileMigration.Apply(db);
            SaveLifePatientProfileMigration.Apply(db);

            Assert.True(HasColumn(db, "sl_agent_conversation", nameof(SaveLifeAgentConversation.PatientContextVersion)));
            Assert.True(HasColumn(db, "sl_agent_message", nameof(SaveLifeAgentMessage.PatientContextVersion)));
            Assert.True(HasColumn(db, "sl_agent_message", nameof(SaveLifeAgentMessage.ContextSourceMessageId)));
            var conversation = db.Queryable<SaveLifeAgentConversation>().Single();
            var message = db.Queryable<SaveLifeAgentMessage>().Single();
            Assert.Equal(1, conversation.PatientContextVersion);
            Assert.Equal(conversation.PatientContextVersion, message.PatientContextVersion);
            Assert.Null(message.ContextSourceMessageId);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SaveLifeAgentConversation Conversation(long id, string externalUserId)
    {
        return new SaveLifeAgentConversation
        {
            Id = id,
            ExternalUserId = externalUserId,
            Status = "active",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static SaveLifeAgentMessage Message(
        long id,
        long conversationId,
        string externalUserId,
        string role)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            ConversationId = conversationId,
            ExternalUserId = externalUserId,
            Role = role,
            Content = "合成测试消息",
            CreateTime = new DateTime(2026, 7, 14)
        };
    }

    private static void DropColumn(ISqlSugarClient db, string tableName, string columnName)
    {
        db.Ado.ExecuteCommand($"ALTER TABLE \"{tableName}\" DROP COLUMN \"{columnName}\"");
    }

    private static bool HasColumn(ISqlSugarClient db, string tableName, string columnName)
    {
        return db.DbMaintenance.GetColumnInfosByTableName(tableName, false)
            .Any(column => column.DbColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase));
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"Data Source={dbPath};Pooling=False",
            DbType = DbType.Sqlite,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (property, column) =>
                {
                    if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                        column.IsNullable = true;
                }
            }
        });
    }
}
