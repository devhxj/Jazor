using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentConversationClearTests
{
    private const long TenantId = 1300000000001;

    [Fact]
    public async Task ClearAsync_DeletesOnlyOwnedConversationData_AndIsIdempotent()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage),
                typeof(SaveLifePatientProfileProposal),
                typeof(SaveLifeAgentBusinessCommand));
            db.Insertable(new[]
            {
                Conversation(101, "owner-a"),
                Conversation(102, "owner-a"),
                Conversation(201, "owner-b"),
                Conversation(301, "owner-a", TenantId + 1)
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                Message(1001, 101, "owner-a"),
                Message(1002, 102, "owner-a"),
                Message(2001, 201, "owner-b"),
                Message(2101, 301, "owner-a", TenantId + 1)
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                Proposal(3001, 101, 1001, "owner-a"),
                Proposal(3002, 201, 2001, "owner-b")
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                Command(4001, 101, "owner-a"),
                Command(4002, 201, "owner-b")
            }).ExecuteCommand();

            var first = await SaveLifeAgentConversationClearPersistence.ClearAsync(
                db,
                "owner-a",
                new DateTime(2026, 7, 17, 15, 30, 0),
                TestContext.Current.CancellationToken);
            var second = await SaveLifeAgentConversationClearPersistence.ClearAsync(
                db,
                "owner-a",
                new DateTime(2026, 7, 17, 15, 31, 0),
                TestContext.Current.CancellationToken);

            Assert.Equal(3, first.ConversationCount);
            Assert.Equal(3, first.MessageCount);
            Assert.Equal(1, first.ProfileProposalCount);
            Assert.Equal(1, first.BusinessCommandCount);
            Assert.Equal(0, second.ConversationCount);
            Assert.All(
                db.Queryable<SaveLifeAgentConversation>().ClearFilter()
                    .Where(item => item.ExternalUserId == "owner-a")
                    .ToList(),
                item => Assert.True(item.IsDelete));
            Assert.All(
                db.Queryable<SaveLifeAgentMessage>().ClearFilter()
                    .Where(item => item.ExternalUserId == "owner-a")
                    .ToList(),
                item => Assert.True(item.IsDelete));
            Assert.False(db.Queryable<SaveLifeAgentConversation>().ClearFilter()
                .Single(item => item.ExternalUserId == "owner-b").IsDelete);
            Assert.False(db.Queryable<SaveLifeAgentMessage>().ClearFilter()
                .Single(item => item.ExternalUserId == "owner-b").IsDelete);
            Assert.False(db.Queryable<SaveLifePatientProfileProposal>().ClearFilter()
                .Single(item => item.OwnerExternalUserId == "owner-b").IsDelete);
            Assert.False(db.Queryable<SaveLifeAgentBusinessCommand>().ClearFilter()
                .Single(item => item.ExternalUserId == "owner-b").IsDelete);
            Assert.True(db.Queryable<SaveLifeAgentConversation>().ClearFilter()
                .Single(item => item.Id == 301).IsDelete);
            Assert.True(db.Queryable<SaveLifeAgentMessage>().ClearFilter()
                .Single(item => item.Id == 2101).IsDelete);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    private static SaveLifeAgentConversation Conversation(
        long id,
        string owner,
        long tenantId = TenantId) => new()
    {
        Id = id,
        TenantId = tenantId,
        ExternalUserId = owner,
        Status = "active",
        Phase = "conversation",
        CreateTime = new DateTime(2026, 7, 17)
    };

    private static SaveLifeAgentMessage Message(
        long id,
        long conversationId,
        string owner,
        long tenantId = TenantId) => new()
    {
        Id = id,
        TenantId = tenantId,
        ConversationId = conversationId,
        ExternalUserId = owner,
        Role = "user",
        Content = "测试消息",
        CreateTime = new DateTime(2026, 7, 17)
    };

    private static SaveLifePatientProfileProposal Proposal(
        long id,
        long conversationId,
        long sourceMessageId,
        string owner) => new()
    {
        Id = id,
        TenantId = TenantId,
        OwnerExternalUserId = owner,
        ConversationId = conversationId,
        PatientProfileId = 501,
        SourceUserMessageId = sourceMessageId,
        SourceAssistantMessageId = sourceMessageId,
        FieldName = SaveLifePatientProfileProposalFields.Diagnosis,
        ProposedValue = "测试诊断",
        Evidence = "测试证据",
        ExpiresTime = new DateTime(2026, 7, 18),
        CreateTime = new DateTime(2026, 7, 17)
    };

    private static SaveLifeAgentBusinessCommand Command(
        long id,
        long conversationId,
        string owner) => new()
    {
        Id = id,
        TenantId = TenantId,
        ExternalUserId = owner,
        ConversationId = conversationId,
        CommandType = "test",
        ClientCommandId = $"command-{id}",
        PayloadHashAlgorithm = "SHA256",
        PayloadHash = new string('a', 64),
        Status = "completed",
        CreateTime = new DateTime(2026, 7, 17)
    };

    private static SqlSugarClient CreateSqliteClient(string dbPath) => new(new ConnectionConfig
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

    private static void DeleteDatabase(string dbPath)
    {
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var path = dbPath + suffix;
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
