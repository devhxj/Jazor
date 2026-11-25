using System.Reflection;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentHistoryQueryTests
{
    [Fact]
    public async Task ApplyConversationScope_ReturnsAllMessagesRegardlessOfLegacyPatientBinding()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeAgentMessage));
            var otherConversationMessage = Message(40, "user", 502, "其他会话问题");
            otherConversationMessage.ConversationId = 200;
            db.Insertable(new[]
            {
                Message(10, "user", 501, "患者 A 问题"),
                Message(11, "assistant", 501, "患者 A 回答", 10, "completed"),
                Message(20, "user", 502, "患者 B 问题"),
                Message(21, "assistant", 502, "患者 B 回答", 20, "completed"),
                Message(30, "user", null, "未绑定问题"),
                otherConversationMessage
            }).ExecuteCommand();
            var repository = SqlSugarRepository<SaveLifeAgentMessage>.CreateForContext(db);

            var messages = await SaveLifeAgentService
                .ApplyConversationScope(repository.AsQueryable(), 100)
                .OrderBy(item => item.Id)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal([10L, 11L, 20L, 21L, 30L], messages.Select(item => item.Id));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task LoadInitialHistoryRecordsAsync_UsesConversationScopeAcrossLegacyPatientBindings()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeAgentMessage));
            var messages = new[]
            {
                Message(10, "user", null, "通用问题"),
                Message(11, "assistant", null, "通用回答", 10, "completed"),
                Message(20, "user", 501, "患者问题"),
                Message(21, "assistant", 501, "患者回答", 20, "completed"),
                Message(30, "user", null, "失败问题"),
                Message(31, "assistant", null, "失败回答", 30, "failed"),
                Message(40, "user", null, "当前通用问题"),
                Message(50, "user", 501, "当前患者问题")
            };
            db.Insertable(messages).ExecuteCommand();
            var repository = SqlSugarRepository<SaveLifeAgentMessage>.CreateForContext(db);
            var generalHistory = await SaveLifeMafRequestContext.LoadInitialHistoryRecordsAsync(
                repository,
                100,
                40,
                20,
                TestContext.Current.CancellationToken);
            var patientHistory = await SaveLifeMafRequestContext.LoadInitialHistoryRecordsAsync(
                repository,
                100,
                50,
                20,
                TestContext.Current.CancellationToken);

            Assert.Equal([10L, 11L, 20L, 21L], generalHistory.Select(item => item.Id));
            Assert.Equal([10L, 11L, 20L, 21L], patientHistory.Select(item => item.Id));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task LoadInitialHistoryRecordsAsync_ManyRecentFailures_DoNotHideCompletedTurns()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeAgentMessage));
            var messages = new List<SaveLifeAgentMessage>
            {
                Message(10, "user", 501, "有效问题"),
                Message(11, "assistant", 501, "有效回答", 10, "completed")
            };
            for (var index = 0; index < 60; index++)
            {
                var userId = 100 + index * 2;
                messages.Add(Message(userId, "user", 501, $"失败问题 {index}"));
                messages.Add(Message(userId + 1, "assistant", 501, string.Empty, userId, "failed"));
            }
            db.Insertable(messages).ExecuteCommand();
            var repository = SqlSugarRepository<SaveLifeAgentMessage>.CreateForContext(db);

            var history = await SaveLifeMafRequestContext.LoadInitialHistoryRecordsAsync(
                repository,
                100,
                excludedMessageId: 999,
                maximumMessages: 20,
                TestContext.Current.CancellationToken);

            Assert.Equal([10L, 11L], history.Select(item => item.Id));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task AttachProfileProposalsAsync_UsesOwnedPersistedStatusAndMarksEffectiveExpiry()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifePatientProfileProposal));
            db.Insertable(new[]
            {
                Proposal(901, "openid-a", 501, 11, SaveLifePatientProfileProposalFields.Diagnosis),
                Proposal(902, "openid-b", 502, 11, SaveLifePatientProfileProposalFields.City)
            }).ExecuteCommand();
            var messages = new List<SaveLifeAgentMessageOutput>
            {
                new() { Id = "10", ConversationId = "100", Role = "user" },
                new() { Id = "11", ConversationId = "100", Role = "assistant" }
            };

            await SaveLifeAgentService.AttachProfileProposalsAsync(
                db,
                conversationId: 100,
                externalUserId: "openid-a",
                messages,
                now: new DateTime(2026, 7, 20),
                TestContext.Current.CancellationToken);

            Assert.Empty(messages[0].ProfileProposals);
            var proposal = Assert.Single(messages[1].ProfileProposals);
            Assert.Equal("901", proposal.Id);
            Assert.Equal(SaveLifePatientProfileProposalSources.UserMessage, proposal.SourceType);
            Assert.Null(proposal.SourceMaterialId);
            Assert.Equal(SaveLifePatientProfileProposalStatuses.Expired, proposal.Status);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SaveLifeAgentMessage Message(
        long id,
        string role,
        long? patientProfileId,
        string content,
        long? replyToMessageId = null,
        string? generationStatus = null)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            ConversationId = 100,
            Content = content,
            CreateTime = new DateTime(2026, 7, 15).AddMinutes(id),
            ExternalUserId = "openid-a",
            GenerationStatus = generationStatus,
            PatientContextVersion = 1,
            PatientProfileId = patientProfileId,
            ReplyToMessageId = replyToMessageId,
            Role = role
        };
    }

    private static SaveLifePatientProfileProposal Proposal(
        long id,
        string externalUserId,
        long patientProfileId,
        long assistantMessageId,
        string field)
    {
        return new SaveLifePatientProfileProposal
        {
            Id = id,
            OwnerExternalUserId = externalUserId,
            ConversationId = 100,
            PatientProfileId = patientProfileId,
            SourceUserMessageId = 10,
            SourceAssistantMessageId = assistantMessageId,
            SourceType = SaveLifePatientProfileProposalSources.UserMessage,
            FieldName = field,
            ProposedValue = field == SaveLifePatientProfileProposalFields.City ? "成都" : "肺腺癌",
            Evidence = field == SaveLifePatientProfileProposalFields.City ? "成都" : "肺腺癌",
            Status = SaveLifePatientProfileProposalStatuses.Pending,
            ExpiresTime = new DateTime(2026, 7, 19),
            CreateTime = new DateTime(2026, 7, 18)
        };
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
