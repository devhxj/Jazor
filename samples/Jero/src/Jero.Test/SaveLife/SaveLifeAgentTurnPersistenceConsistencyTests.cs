using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentTurnPersistenceConsistencyTests
{
    [Fact]
    public async Task CompleteGeneration_WithMatchingLease_UpdatesStateAndAdvancesRevisionAtomically()
    {
        await WithDatabase(async db =>
        {
            InitTurn(db);
            var lease = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 1001, "openid-1", "lease-1", new DateTime(2026, 7, 16, 10, 0, 0),
                TimeSpan.FromMinutes(1), () => 2001, TestContext.Current.CancellationToken);

            await SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                db,
                lease.Lease!,
                (conversation, _) =>
                {
                    conversation.FlowStateJson = "{\"version\":2,\"selectedTrialId\":9001}";
                    conversation.FlowStateVersion = 2;
                    conversation.MatchedTrialsJson = "[{\"id\":9001}]";
                    conversation.Phase = "answered";
                    conversation.LastMessage = "已完成回答";
                    return Task.FromResult(new SaveLifeAgentTurnCompletion<bool>("已完成回答", null, true));
                },
                new DateTime(2026, 7, 16, 10, 0, 5),
                TestContext.Current.CancellationToken);

            var restored = db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();
            var userMessage = db.Queryable<SaveLifeAgentMessage>().ClearFilter().Single(item => item.Id == 1001);
            var assistantMessage = db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Single(item => item.Role == "assistant");
            Assert.Equal(501, restored.CurrentPatientProfileId);
            Assert.Equal(7, restored.PatientContextVersion);
            Assert.Equal(1, restored.StateRevision);
            Assert.Equal("answered", restored.Phase);
            Assert.Equal("已完成回答", restored.LastMessage);
            Assert.Equal(SaveLifeAgentTurnStatuses.Completed, userMessage.TurnStatus);
            Assert.Equal("completed", assistantMessage.GenerationStatus);
            Assert.Equal("已完成回答", assistantMessage.Content);
        });
    }

    [Fact]
    public async Task CompleteGeneration_AfterPatientContextChanges_DoesNotOverwriteNewState()
    {
        await WithDatabase(async db =>
        {
            InitTurn(db);
            var lease = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 1001, "openid-1", "lease-1", new DateTime(2026, 7, 16, 11, 0, 0),
                TimeSpan.FromMinutes(1), () => 2001, TestContext.Current.CancellationToken);
            db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(item => new SaveLifeAgentConversation
                {
                    CurrentPatientProfileId = 502,
                    PatientContextVersion = 8,
                    StateRevision = 1,
                    Phase = "patient_changed",
                    FlowStateJson = "{\"version\":2}"
                })
                .Where(item => item.Id == 101)
                .ExecuteCommand();

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                    db,
                    lease.Lease!,
                    (conversation, _) =>
                    {
                        conversation.Phase = "answered";
                        conversation.LastMessage = "不应写入的回复";
                        return Task.FromResult(
                            new SaveLifeAgentTurnCompletion<bool>("不应写入的回复", null, true));
                    },
                    new DateTime(2026, 7, 16, 11, 0, 5),
                    TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeAgentFailureCodes.PatientContextChanged, error.Code);
            var restored = db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();
            var assistantMessage = db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Single(item => item.Role == "assistant");
            Assert.Equal(502, restored.CurrentPatientProfileId);
            Assert.Equal(8, restored.PatientContextVersion);
            Assert.Equal(1, restored.StateRevision);
            Assert.Equal("patient_changed", restored.Phase);
            Assert.Equal("{\"version\":2}", restored.FlowStateJson);
            Assert.Null(restored.LastMessage);
            Assert.Equal(string.Empty, assistantMessage.Content);
            Assert.Equal("processing", assistantMessage.GenerationStatus);
        });
    }

    private static void InitTurn(ISqlSugarClient db)
    {
        db.CodeFirst.InitTables(
            typeof(SaveLifeAgentConversation),
            typeof(SaveLifeAgentMessage));
        SaveLifeAgentTurnMigration.Apply(db);
        db.Insertable(new SaveLifeAgentConversation
        {
            Id = 101,
            ExternalUserId = "openid-1",
            CurrentPatientProfileId = 501,
            PatientContextVersion = 7,
            NextTurnSequence = 2,
            StateRevision = 0,
            Status = "active",
            CreateTime = new DateTime(2026, 7, 16)
        }).ExecuteCommand();
        db.Insertable(new SaveLifeAgentMessage
        {
            Id = 1001,
            ConversationId = 101,
            ExternalUserId = "openid-1",
            PatientProfileId = 501,
            PatientContextVersion = 7,
            TurnSequence = 1,
            TurnStatus = SaveLifeAgentTurnStatuses.Pending,
            Role = "user",
            Content = "合成测试消息",
            ClientMessageId = "client-1",
            CreateTime = new DateTime(2026, 7, 16, 9, 0, 0)
        }).ExecuteCommand();
    }

    private static async Task WithDatabase(Func<ISqlSugarClient, Task> test)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            await test(db);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
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
