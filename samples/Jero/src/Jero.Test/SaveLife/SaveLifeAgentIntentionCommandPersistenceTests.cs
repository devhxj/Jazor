using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentIntentionCommandPersistenceTests
{
    private const long TenantId = 1300000000001;
    private static readonly DateTime Now = new(2026, 7, 16, 16, 0, 0);

    [Fact]
    public async Task ExecuteAsync_AfterResponseLoss_ReplaysTheFirstResultWithoutAdditionalWrites()
    {
        using var database = new CommandDatabase();
        var request = database.Request("command-1");

        var first = await database.ExecuteAsync(request);
        var replay = await database.ExecuteAsync(request);

        AssertEquivalent(first, replay);
        Assert.Equal(2, database.Messages().Count);
        Assert.Single(database.Commands());
        Assert.Single(database.Enrollments());
        var conversation = database.Conversation();
        Assert.Equal(2, conversation.NextTurnSequence);
        Assert.Equal(1, conversation.StateRevision);
    }

    [Fact]
    public async Task TryReplayAsync_AfterPatientContextChanges_ReturnsOriginalResult()
    {
        using var database = new CommandDatabase();
        var request = database.Request("command-replay-after-switch");
        var first = await database.ExecuteAsync(request);
        database.UpdateConversation(conversation =>
        {
            conversation.Status = "active";
            conversation.CurrentPatientProfileId = 502;
            conversation.PatientContextVersion = 4;
            conversation.StateRevision = 2;
        });

        var replay = await database.TryReplayAsync(request);

        Assert.True(replay.Exists);
        AssertEquivalent(first, replay.Result!);
        Assert.Single(database.Commands());
        Assert.Equal(2, database.Messages().Count);
        Assert.Single(database.Enrollments());
    }

    [Fact]
    public async Task ExecuteAsync_WithSameCommandIdAndDifferentPayload_RejectsWithoutSideEffects()
    {
        using var database = new CommandDatabase();
        var request = database.Request("command-1");
        await database.ExecuteAsync(request);

        await Assert.ThrowsAnyAsync<Exception>(() => database.ExecuteAsync(request with
        {
            Intention = "还在考虑"
        }));

        Assert.Equal(2, database.Messages().Count);
        Assert.Single(database.Commands());
        Assert.Single(database.Enrollments());
        Assert.Equal(1, database.Conversation().StateRevision);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentCommandsForSamePatientAndProject_ReusesEnrollment()
    {
        using var database = new CommandDatabase();
        await database.ExecuteAsync(database.Request("command-1"));

        await database.ExecuteAsync(database.Request("command-2") with
        {
            ExpectedStateRevision = 1
        });

        Assert.Equal(2, database.Commands().Count);
        Assert.Equal(4, database.Messages().Count);
        Assert.Single(database.Enrollments());
        Assert.Equal(2, database.Conversation().StateRevision);
    }

    [Fact]
    public async Task ExecuteAsync_AfterPatientSwitch_RejectsStaleCommandWithoutWrites()
    {
        using var database = new CommandDatabase();
        database.UpdateConversation(conversation =>
        {
            conversation.CurrentPatientProfileId = 502;
            conversation.PatientContextVersion = 4;
            conversation.StateRevision = 1;
        });

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            database.ExecuteAsync(database.Request("command-stale-patient")));

        Assert.Equal(SaveLifeAgentFailureCodes.PatientContextChanged, error.Code);
        Assert.Empty(database.Commands());
        Assert.Empty(database.Messages());
        Assert.Empty(database.Enrollments());
    }

    [Fact]
    public async Task ExecuteAsync_AfterOtherStateChange_RejectsStaleRevisionWithoutWrites()
    {
        using var database = new CommandDatabase();
        database.UpdateConversation(conversation => conversation.StateRevision = 1);

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            database.ExecuteAsync(database.Request("command-stale-revision")));

        Assert.Equal(SaveLifeAgentFailureCodes.TurnSuperseded, error.Code);
        Assert.Empty(database.Commands());
        Assert.Empty(database.Messages());
        Assert.Empty(database.Enrollments());
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveGenerationLease_RejectsWithoutWrites()
    {
        using var database = new CommandDatabase();
        database.UpdateConversation(conversation =>
        {
            conversation.GenerationLeaseOwner = "active-lease";
            conversation.GenerationLeaseTurnSequence = 1;
            conversation.GenerationLeaseExpiresTime = Now.AddMinutes(1);
        });

        await Assert.ThrowsAnyAsync<Exception>(() =>
            database.ExecuteAsync(database.Request("command-during-generation")));

        Assert.Empty(database.Commands());
        Assert.Empty(database.Messages());
        Assert.Empty(database.Enrollments());
        Assert.Equal("active-lease", database.Conversation().GenerationLeaseOwner);
    }

    [Theory]
    [InlineData("user_message")]
    [InlineData("enrollment")]
    [InlineData("assistant_message")]
    [InlineData("conversation")]
    [InlineData("command_result")]
    public async Task ExecuteAsync_WhenAnyWriteBoundaryFails_RollsBackEverything(string boundary)
    {
        using var database = new CommandDatabase();
        database.FailAt(boundary);

        await Assert.ThrowsAnyAsync<Exception>(() =>
            database.ExecuteAsync(database.Request("command-failure")));

        Assert.Empty(database.Messages());
        Assert.Empty(database.Commands());
        Assert.Empty(database.Enrollments());
        var conversation = database.Conversation();
        Assert.Equal(1, conversation.NextTurnSequence);
        Assert.Equal(0, conversation.StateRevision);
        Assert.Null(conversation.EnrollmentRecordId);
    }

    [Fact]
    public async Task ExecuteAsync_WithTwentyConcurrentIdenticalCommands_PersistsOneOutcome()
    {
        var dbPath = CommandDatabase.NewDatabasePath();
        try
        {
            using (var setup = new CommandDatabase(dbPath, initialize: true))
                setup.EnableConcurrentAccess();

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(0, 20).Select(async _ =>
            {
                using var database = new CommandDatabase(dbPath, initialize: false);
                database.EnableConcurrentAccess();
                await start.Task;
                return await database.ExecuteAsync(database.Request("command-concurrent"));
            }).ToArray();

            start.SetResult();
            var results = await Task.WhenAll(tasks);

            Assert.All(results, result => AssertEquivalent(results[0], result));
            using var verify = new CommandDatabase(dbPath, initialize: false);
            Assert.Single(verify.Commands());
            Assert.Equal(2, verify.Messages().Count);
            Assert.Single(verify.Enrollments());
            Assert.Equal(1, verify.Conversation().StateRevision);
        }
        finally
        {
            CommandDatabase.DeleteDatabase(dbPath);
        }
    }

    private static void AssertEquivalent(
        SaveLifeAgentIntentionOutput expected,
        SaveLifeAgentIntentionOutput actual)
    {
        Assert.Equal(expected.ConversationId, actual.ConversationId);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.EnrollmentRecordId, actual.EnrollmentRecordId);
        Assert.Equal(expected.Content, actual.Content);
        Assert.Equal(expected.SuggestedActions.Count, actual.SuggestedActions.Count);
    }

    private sealed class CommandDatabase : IDisposable
    {
        private readonly bool _ownsPath;
        private long _nextId = 10_000;

        public CommandDatabase()
            : this(NewDatabasePath(), initialize: true, ownsPath: true)
        {
        }

        public CommandDatabase(string dbPath, bool initialize, bool ownsPath = false)
        {
            Path = dbPath;
            _ownsPath = ownsPath;
            Db = CreateSqliteClient(dbPath);
            Db.Aop.DataExecuting = (_, info) =>
            {
                if (info.OperationType != DataFilterType.InsertByObject ||
                    !info.EntityColumnInfo.IsPrimarykey ||
                    info.EntityColumnInfo.PropertyInfo.PropertyType != typeof(long))
                {
                    return;
                }

                var id = (long?)info.EntityColumnInfo.PropertyInfo.GetValue(info.EntityValue);
                if (id.GetValueOrDefault() == 0)
                    info.SetValue(Interlocked.Increment(ref _nextId));
            };
            if (initialize)
                Initialize();
        }

        private string Path { get; }

        private SqlSugarClient Db { get; }

        public SaveLifeAgentIntentionCommandRequest Request(string clientCommandId)
        {
            return new SaveLifeAgentIntentionCommandRequest(
                TenantId,
                "account-a",
                ConversationId: 1,
                ClientCommandId: clientCommandId,
                ExpectedStateRevision: 0,
                PatientProfileId: 501,
                ProjectId: 901,
                Intention: "明确有意向");
        }

        public Task<SaveLifeAgentIntentionOutput> ExecuteAsync(
            SaveLifeAgentIntentionCommandRequest request)
        {
            return SaveLifeAgentIntentionCommandPersistence.ExecuteAsync(
                Db,
                request,
                async (db, _) =>
                {
                    var enrollment = await db.Queryable<SaveLifeEnrollmentRecord>()
                        .ClearFilter()
                        .Where(item => item.PatientProfileId == 501 && item.ProjectId == 901)
                        .FirstAsync();
                    if (enrollment == null)
                    {
                        enrollment = new SaveLifeEnrollmentRecord
                        {
                            Id = 9001,
                            TenantId = TenantId,
                            ExternalUserId = "account-a",
                            PatientProfileId = 501,
                            ProjectId = 901,
                            ProjectTitle = "测试项目",
                            Status = "pending",
                            CreateTime = Now
                        };
                        await db.Insertable(enrollment).ExecuteCommandAsync();
                    }

                    var output = new SaveLifeAgentIntentionOutput
                    {
                        ConversationId = "1",
                        Status = "pending",
                        EnrollmentRecordId = enrollment.Id.ToString(),
                        Content = "已提交参与意向。"
                    };
                    return new SaveLifeAgentIntentionCommandPreparation<SaveLifeAgentIntentionOutput>(
                        output,
                        UserContent: $"我选择：{request.Intention}",
                        AssistantContent: output.Content,
                        AssistantMetadataJson: "{}",
                        ConversationStatus: "active",
                        ConversationPhase: "pending_intention",
                        EnrollmentRecordId: enrollment.Id);
                },
                Now,
                () => Interlocked.Increment(ref _nextId));
        }

        public Task<SaveLifeAgentIntentionCommandReplay<SaveLifeAgentIntentionOutput>> TryReplayAsync(
            SaveLifeAgentIntentionCommandRequest request)
        {
            return SaveLifeAgentIntentionCommandPersistence
                .TryReplayAsync<SaveLifeAgentIntentionOutput>(Db, request);
        }

        public void EnableConcurrentAccess()
        {
            Db.Ado.ExecuteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;");
        }

        public void FailAt(string boundary)
        {
            var sql = boundary switch
            {
                "user_message" => """
                    CREATE TRIGGER fail_boundary BEFORE INSERT ON sl_agent_message
                    WHEN NEW.Role = 'user' BEGIN SELECT RAISE(ABORT, 'forced user failure'); END;
                    """,
                "enrollment" => """
                    CREATE TRIGGER fail_boundary BEFORE INSERT ON sl_enrollment_record
                    BEGIN SELECT RAISE(ABORT, 'forced enrollment failure'); END;
                    """,
                "assistant_message" => """
                    CREATE TRIGGER fail_boundary BEFORE INSERT ON sl_agent_message
                    WHEN NEW.Role = 'assistant' BEGIN SELECT RAISE(ABORT, 'forced assistant failure'); END;
                    """,
                "conversation" => """
                    CREATE TRIGGER fail_boundary BEFORE UPDATE OF StateRevision ON sl_agent_conversation
                    BEGIN SELECT RAISE(ABORT, 'forced conversation failure'); END;
                    """,
                "command_result" => """
                    CREATE TRIGGER fail_boundary BEFORE UPDATE OF Status ON sl_agent_business_command
                    WHEN NEW.Status = 'completed' BEGIN SELECT RAISE(ABORT, 'forced result failure'); END;
                    """,
                _ => throw new ArgumentOutOfRangeException(nameof(boundary))
            };
            Db.Ado.ExecuteCommand(sql);
        }

        public List<SaveLifeAgentMessage> Messages() =>
            Db.Queryable<SaveLifeAgentMessage>().ClearFilter().ToList();

        public List<SaveLifeAgentBusinessCommand> Commands() =>
            Db.Queryable<SaveLifeAgentBusinessCommand>().ClearFilter().ToList();

        public List<SaveLifeEnrollmentRecord> Enrollments() =>
            Db.Queryable<SaveLifeEnrollmentRecord>().ClearFilter().ToList();

        public SaveLifeAgentConversation Conversation() =>
            Db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();

        public void UpdateConversation(Action<SaveLifeAgentConversation> update)
        {
            var conversation = Conversation();
            update(conversation);
            Db.Updateable(conversation).ExecuteCommand();
        }

        public void Dispose()
        {
            Db.Dispose();
            if (_ownsPath)
                DeleteDatabase(Path);
        }

        private void Initialize()
        {
            Db.CodeFirst.InitTables(
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage),
                typeof(SaveLifeEnrollmentRecord));
            SaveLifeAgentTurnMigration.Apply(Db);
            SaveLifeAgentCommandMigration.Apply(Db);
            Db.Insertable(new SaveLifeAgentConversation
            {
                Id = 1,
                TenantId = TenantId,
                ExternalUserId = "account-a",
                CurrentPatientProfileId = 501,
                PatientContextVersion = 3,
                NextTurnSequence = 1,
                StateRevision = 0,
                Status = "active",
                CreateTime = Now
            }).ExecuteCommand();
        }

        public static string NewDatabasePath() =>
            System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        public static void DeleteDatabase(string dbPath)
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
}
