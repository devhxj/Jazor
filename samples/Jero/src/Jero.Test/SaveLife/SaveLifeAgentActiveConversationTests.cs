using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentActiveConversationTests
{
    private const long DefaultTenantId = 1300000000001;

    [Fact]
    public void Apply_Repeatedly_CreatesOneConditionalUniqueIndex()
    {
        WithDatabase(db =>
        {
            db.CodeFirst.InitTables(typeof(SaveLifeAgentConversation));

            var first = SaveLifeAgentConversationMigration.Apply(db);
            var second = SaveLifeAgentConversationMigration.Apply(db);

            Assert.Equal(0, first.AccountConflictCount);
            Assert.Equal(0, second.AccountConflictCount);
            Assert.Equal(
                1,
                db.Ado.SqlQuery<int>(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = @Name",
                    new SugarParameter("@Name", SaveLifeAgentConversationMigration.ActiveAccountUniqueIndexName))
                    .Single());
        });
    }

    [Fact]
    public void ActiveAccountUniqueIndex_RejectsOnlySameTenantActiveRows()
    {
        WithDatabase(db =>
        {
            db.CodeFirst.InitTables(typeof(SaveLifeAgentConversation));
            SaveLifeAgentConversationMigration.Apply(db);
            db.Insertable(Conversation(1, DefaultTenantId, "account-a", "active")).ExecuteCommand();

            Assert.ThrowsAny<Exception>(() =>
                db.Insertable(Conversation(2, DefaultTenantId, "account-a", "active")).ExecuteCommand());
            db.Insertable(Conversation(7, null, "account-null", "active")).ExecuteCommand();
            Assert.ThrowsAny<Exception>(() =>
                db.Insertable(Conversation(8, null, "account-null", "active")).ExecuteCommand());

            db.Insertable(new[]
            {
                Conversation(3, DefaultTenantId + 1, "account-a", "active"),
                Conversation(4, DefaultTenantId, "account-a", "closed"),
                Conversation(5, DefaultTenantId, "account-b", "active"),
                DeletedConversation(6, DefaultTenantId, "account-a")
            }).ExecuteCommand();

            Assert.Equal(6, db.Queryable<SaveLifeAgentConversation>().ClearFilter().Count());
        });
    }

    [Fact]
    public void Apply_WithHistoricalDuplicates_ReportsOnlyAggregatesAndRollsBack()
    {
        WithDatabase(db =>
        {
            db.CodeFirst.InitTables(typeof(SaveLifeAgentConversation));
            var rows = new[]
            {
                Conversation(11, DefaultTenantId, "account-a", "active"),
                Conversation(12, DefaultTenantId, "account-a", "active"),
                Conversation(13, DefaultTenantId, "account-b", "active"),
                Conversation(14, DefaultTenantId, "account-b", "active"),
                Conversation(15, DefaultTenantId, "account-b", "active"),
                Conversation(16, DefaultTenantId + 1, "account-c", "active"),
                Conversation(17, DefaultTenantId + 1, "account-c", "active")
            };
            db.Insertable(rows).ExecuteCommand();

            var error = Assert.Throws<SaveLifeAgentConversationMigrationConflictException>(() =>
                SaveLifeAgentConversationMigration.Apply(db));

            Assert.Equal(2, error.Report.TenantConflictCount);
            Assert.Equal(3, error.Report.AccountConflictCount);
            Assert.Equal(3, error.Report.MaxDuplicateCount);
            Assert.DoesNotContain("account-", error.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("11", error.Message, StringComparison.Ordinal);
            Assert.Equal(rows.Select(item => item.Id), db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .OrderBy(item => item.Id)
                .Select(item => item.Id)
                .ToList());
            Assert.Equal(
                0,
                db.Ado.SqlQuery<int>(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = @Name",
                    new SugarParameter("@Name", SaveLifeAgentConversationMigration.ActiveAccountUniqueIndexName))
                    .Single());
        });
    }

    [Fact]
    public async Task CreateOrGetActiveAsync_WithTwentyConcurrentFirstCreates_ReturnsOneConversation()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using (var setup = CreateSqliteClient(dbPath))
            {
                setup.CodeFirst.InitTables(typeof(SaveLifeAgentConversation));
                SaveLifeAgentConversationMigration.Apply(setup);
                setup.Ado.ExecuteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;");
            }

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(1, 20).Select(async sequence =>
            {
                await start.Task;
                using var db = CreateSqliteClient(dbPath);
                db.Ado.ExecuteCommand("PRAGMA busy_timeout=30000;");
                return await SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                    db,
                    Conversation(100 + sequence, DefaultTenantId, "account-a", "active"),
                    TestContext.Current.CancellationToken);
            }).ToArray();

            start.SetResult();
            var results = await Task.WhenAll(tasks);

            Assert.Single(results.Select(item => item.Id).Distinct());
            using var verify = CreateSqliteClient(dbPath);
            Assert.Single(verify.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Where(item =>
                    item.TenantId == DefaultTenantId &&
                    item.ExternalUserId == "account-a" &&
                    item.Status == "active" &&
                    !item.IsDelete)
                .ToList());
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task CreateOrGetActiveAsync_AfterClose_CreatesNewConversation()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeAgentConversation));
            SaveLifeAgentConversationMigration.Apply(db);

            var first = await SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                db,
                Conversation(201, DefaultTenantId, "account-a", "active"),
                TestContext.Current.CancellationToken);
            var restored = await SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                db,
                Conversation(202, DefaultTenantId, "account-a", "active"),
                TestContext.Current.CancellationToken);
            Assert.Equal(first.Id, restored.Id);

            var closed = await db.Updateable<SaveLifeAgentConversation>()
                .SetColumns(item => item.Status == "closed")
                .Where(item => item.Id == first.Id && item.Status == "active")
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);
            Assert.Equal(1, closed);

            var next = await SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                db,
                Conversation(203, DefaultTenantId, "account-a", "active"),
                TestContext.Current.CancellationToken);

            Assert.NotEqual(first.Id, next.Id);
            Assert.Equal(
                1,
                db.Queryable<SaveLifeAgentConversation>()
                    .ClearFilter()
                    .Count(item =>
                        item.TenantId == DefaultTenantId &&
                        item.ExternalUserId == "account-a" &&
                        item.Status == "active" &&
                        !item.IsDelete));
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task CreateOrGetActiveAsync_WithDifferentUniqueViolation_DoesNotHideDatabaseFailure()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeAgentConversation));
            SaveLifeAgentConversationMigration.Apply(db);
            db.Ado.ExecuteCommand("CREATE UNIQUE INDEX uq_test_conversation_phase ON sl_agent_conversation (Phase)");
            db.Insertable(Conversation(301, DefaultTenantId, "account-a", "closed")).ExecuteCommand();

            var error = await Assert.ThrowsAnyAsync<Exception>(() =>
                SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                    db,
                    Conversation(302, DefaultTenantId, "account-b", "active"),
                    TestContext.Current.CancellationToken));

            Assert.Contains("UNIQUE constraint failed", error.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.Empty(db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Where(item => item.ExternalUserId == "account-b")
                .ToList());
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task CloseRacingWithCreates_ConvergesToOneActiveConversation()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using (var setup = CreateSqliteClient(dbPath))
            {
                setup.CodeFirst.InitTables(typeof(SaveLifeAgentConversation));
                SaveLifeAgentConversationMigration.Apply(setup);
                setup.Ado.ExecuteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;");
                setup.Insertable(Conversation(401, DefaultTenantId, "account-a", "active"))
                    .ExecuteCommand();
            }

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var closeTask = Task.Run(async () =>
            {
                await start.Task;
                using var db = CreateSqliteClient(dbPath);
                db.Ado.ExecuteCommand("PRAGMA busy_timeout=30000;");
                await db.Updateable<SaveLifeAgentConversation>()
                    .SetColumns(item => item.Status == "closed")
                    .Where(item => item.Id == 401 && item.Status == "active")
                    .ExecuteCommandAsync(TestContext.Current.CancellationToken);
            }, TestContext.Current.CancellationToken);
            var createTasks = Enumerable.Range(1, 20).Select(async sequence =>
            {
                await start.Task;
                using var db = CreateSqliteClient(dbPath);
                db.Ado.ExecuteCommand("PRAGMA busy_timeout=30000;");
                return await SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                    db,
                    Conversation(500 + sequence, DefaultTenantId, "account-a", "active"),
                    TestContext.Current.CancellationToken);
            }).ToArray();

            start.SetResult();
            await Task.WhenAll(createTasks.Cast<Task>().Append(closeTask));

            using var ensure = CreateSqliteClient(dbPath);
            await SaveLifeAgentActiveConversationPersistence.CreateOrGetActiveAsync(
                ensure,
                Conversation(601, DefaultTenantId, "account-a", "active"),
                TestContext.Current.CancellationToken);
            Assert.Equal(
                1,
                ensure.Queryable<SaveLifeAgentConversation>()
                    .ClearFilter()
                    .Count(item =>
                        item.TenantId == DefaultTenantId &&
                        item.ExternalUserId == "account-a" &&
                        item.Status == "active" &&
                        !item.IsDelete));
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    private static SaveLifeAgentConversation Conversation(
        long id,
        long? tenantId,
        string externalUserId,
        string status)
    {
        return new SaveLifeAgentConversation
        {
            Id = id,
            TenantId = tenantId,
            ExternalUserId = externalUserId,
            Status = status,
            Phase = "created",
            CreateTime = new DateTime(2026, 7, 16).AddSeconds(id)
        };
    }

    private static SaveLifeAgentConversation DeletedConversation(
        long id,
        long? tenantId,
        string externalUserId)
    {
        var conversation = Conversation(id, tenantId, externalUserId, "active");
        conversation.IsDelete = true;
        return conversation;
    }

    private static void WithDatabase(Action<ISqlSugarClient> test)
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            test(db);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    private static string NewDatabasePath()
    {
        return Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
    }

    private static void DeleteDatabase(string dbPath)
    {
        foreach (var suffix in new[] { string.Empty, "-wal", "-shm" })
        {
            var path = dbPath + suffix;
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"Data Source={dbPath};Pooling=False;Default Timeout=30",
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
