using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentTurnSerializationTests
{
    private const long TenantId = 1300000000001;

    [Fact]
    public void Apply_WithLegacyMessages_BackfillsOrderedTurnStateAndIsIdempotent()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage));
            db.Insertable(Conversation(1)).ExecuteCommand();
            db.Insertable(new[]
            {
                UserMessage(11, "client-1", new DateTime(2026, 7, 16, 8, 0, 0)),
                AssistantMessage(12, 11, "completed", new DateTime(2026, 7, 16, 8, 0, 1)),
                UserMessage(21, "client-2", new DateTime(2026, 7, 16, 8, 1, 0)),
                AssistantMessage(22, 21, "failed", new DateTime(2026, 7, 16, 8, 1, 1)),
                UserMessage(31, "client-3", new DateTime(2026, 7, 16, 8, 2, 0))
            }).ExecuteCommand();
            DropTurnColumns(db);

            SaveLifeAgentTurnMigration.Apply(db);
            SaveLifeAgentTurnMigration.Apply(db);

            var conversation = db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();
            var users = db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Where(item => item.Role == "user")
                .OrderBy(item => item.TurnSequence)
                .ToList();
            Assert.Equal(4, conversation.NextTurnSequence);
            Assert.Equal(0, conversation.StateRevision);
            Assert.Equal([1L, 2L, 3L], users.Select(item => item.TurnSequence!.Value));
            Assert.Equal(
                [
                    SaveLifeAgentTurnStatuses.Completed,
                    SaveLifeAgentTurnStatuses.Failed,
                    SaveLifeAgentTurnStatuses.Pending
                ],
                users.Select(item => item.TurnStatus));
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public void Apply_WhenBackfillFails_RollsBackAllTurnDataAndIndex()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(
                typeof(SaveLifeAgentConversation),
                typeof(SaveLifeAgentMessage));
            db.Insertable(Conversation(1)).ExecuteCommand();
            var first = UserMessage(11, "client-1", new DateTime(2026, 7, 16, 8, 0, 0));
            var second = UserMessage(21, "client-2", new DateTime(2026, 7, 16, 8, 1, 0));
            first.TurnSequence = null;
            first.TurnStatus = null;
            second.TurnSequence = null;
            second.TurnStatus = null;
            db.Insertable(new[] { first, second }).ExecuteCommand();
            db.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_turn_backfill
                BEFORE UPDATE OF TurnSequence ON sl_agent_message
                WHEN NEW.Id = 21
                BEGIN
                    SELECT RAISE(ABORT, 'forced turn backfill failure');
                END;
                """);

            Assert.ThrowsAny<Exception>(() => SaveLifeAgentTurnMigration.Apply(db));

            var messages = db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Where(item => item.Role == "user")
                .OrderBy(item => item.Id)
                .ToList();
            Assert.All(messages, item => Assert.Null(item.TurnSequence));
            Assert.All(messages, item => Assert.Null(item.TurnStatus));
            Assert.Equal(1, db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Single()
                .NextTurnSequence);
            Assert.Equal(
                0,
                db.Ado.SqlQuery<int>(
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = @Name",
                    new SugarParameter("@Name", SaveLifeAgentTurnMigration.TurnSequenceUniqueIndexName))
                    .Single());
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task AcceptUserMessageAsync_WithTwentyDifferentMessages_AssignsUniqueMonotonicSequences()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using (var setup = CreateSqliteClient(dbPath))
            {
                setup.CodeFirst.InitTables(
                    typeof(SaveLifeAgentConversation),
                    typeof(SaveLifeAgentMessage));
                SaveLifeAgentTurnMigration.Apply(setup);
                setup.Insertable(Conversation(1)).ExecuteCommand();
                setup.Ado.ExecuteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;");
            }

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(1, 20).Select(async sequence =>
            {
                await start.Task;
                using var db = CreateSqliteClient(dbPath);
                db.Ado.ExecuteCommand("PRAGMA busy_timeout=30000;");
                return await SaveLifeAgentTurnPersistence.AcceptUserMessageAsync(
                    db,
                    conversationId: 1,
                    externalUserId: "account-a",
                    content: $"合成消息-{sequence}",
                    clientMessageId: $"client-{sequence}",
                    contextSourceMessageId: null,
                    expectedPatientProfileId: 501,
                    expectedPatientContextVersion: 3,
                    acceptedTime: new DateTime(2026, 7, 16, 9, 0, 0).AddSeconds(sequence),
                    idGenerator: () => 1000 + sequence,
                    cancellationToken: TestContext.Current.CancellationToken);
            }).ToArray();

            start.SetResult();
            var messages = await Task.WhenAll(tasks);

            Assert.Equal(Enumerable.Range(1, 20).Select(value => (long)value),
                messages.Select(item => item.TurnSequence!.Value).Order());
            Assert.All(messages, item => Assert.Equal(SaveLifeAgentTurnStatuses.Pending, item.TurnStatus));
            using var verify = CreateSqliteClient(dbPath);
            Assert.Equal(21, verify.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Single()
                .NextTurnSequence);
            Assert.Equal(20, verify.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Count(item => item.Role == "user"));
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task AcceptUserMessageAsync_ReplaysSamePayloadAndRejectsDifferentPayload()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitCurrentSchema(db);
            db.Insertable(Conversation(1)).ExecuteCommand();
            var now = new DateTime(2026, 7, 16, 9, 30, 0);
            var first = await SaveLifeAgentTurnPersistence.AcceptUserMessageAsync(
                db, 1, "account-a", "相同内容", "client-1", null, 501, 3, now,
                () => 101, TestContext.Current.CancellationToken);
            var replay = await SaveLifeAgentTurnPersistence.AcceptUserMessageAsync(
                db, 1, "account-a", "相同内容", "client-1", null, 501, 3, now.AddSeconds(1),
                () => 102, TestContext.Current.CancellationToken);

            var error = await Assert.ThrowsAsync<SaveLifeAgentTurnIdempotencyConflictException>(() =>
                SaveLifeAgentTurnPersistence.AcceptUserMessageAsync(
                    db, 1, "account-a", "不同内容", "client-1", null, 501, 3, now.AddSeconds(2),
                    () => 103, TestContext.Current.CancellationToken));

            Assert.Equal(first.Id, replay.Id);
            Assert.Contains("客户端消息", error.Message, StringComparison.Ordinal);
            Assert.Single(db.Queryable<SaveLifeAgentMessage>().ClearFilter().ToList());
            Assert.Equal(2, db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Single()
                .NextTurnSequence);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task GenerationLease_OnlyAllowsTheSmallestUnfinishedTurn()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitCurrentSchema(db);
            db.Insertable(Conversation(1)).ExecuteCommand();
            db.Insertable(new[]
            {
                PendingUserMessage(101, 1, "client-1"),
                PendingUserMessage(102, 2, "client-2")
            }).ExecuteCommand();
            var now = new DateTime(2026, 7, 16, 10, 0, 0);

            var blocked = await Assert.ThrowsAsync<SaveLifeAgentTurnBlockedException>(() =>
                SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                    db, 102, "account-a", "lease-2", now, TimeSpan.FromMinutes(1),
                    () => 202, TestContext.Current.CancellationToken));
            Assert.Equal(SaveLifeAgentTurnBlockReasons.EarlierTurnPending, blocked.Reason);

            var first = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 101, "account-a", "lease-1", now, TimeSpan.FromMinutes(1),
                () => 201, TestContext.Current.CancellationToken);
            Assert.True(first.OwnsGeneration);
            await Assert.ThrowsAsync<SaveLifeAgentTurnBlockedException>(() =>
                SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                    db, 102, "account-a", "lease-2", now, TimeSpan.FromMinutes(1),
                    () => 202, TestContext.Current.CancellationToken));

            await SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                db,
                first.Lease!,
                (conversation, _) =>
                {
                    conversation.Phase = "first_completed";
                    return Task.FromResult(new SaveLifeAgentTurnCompletion<bool>("第一轮完成", null, true));
                },
                now.AddSeconds(10),
                TestContext.Current.CancellationToken);
            var second = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 102, "account-a", "lease-2", now.AddSeconds(11), TimeSpan.FromMinutes(1),
                () => 202, TestContext.Current.CancellationToken);

            Assert.True(second.OwnsGeneration);
            Assert.Equal(2, second.Lease?.TurnSequence);
            Assert.Equal(1, second.Lease?.StateRevision);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task ExpiredLeaseTakeover_PreventsLateOwnerFromCommitting()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitCurrentSchema(db);
            db.Insertable(Conversation(1)).ExecuteCommand();
            db.Insertable(PendingUserMessage(101, 1, "client-1")).ExecuteCommand();
            var now = new DateTime(2026, 7, 16, 11, 0, 0);
            var oldLease = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 101, "account-a", "lease-old", now, TimeSpan.FromSeconds(30),
                () => 201, TestContext.Current.CancellationToken);
            var newLease = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 101, "account-a", "lease-new", now.AddMinutes(1), TimeSpan.FromMinutes(1),
                () => 202, TestContext.Current.CancellationToken);

            var lateError = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                    db,
                    oldLease.Lease!,
                    (_, _) => Task.FromResult(
                        new SaveLifeAgentTurnCompletion<bool>("不应提交", null, true)),
                    now.AddMinutes(1).AddSeconds(1),
                    TestContext.Current.CancellationToken));
            Assert.Equal(SaveLifeAgentFailureCodes.TurnSuperseded, lateError.Code);

            await SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                db,
                newLease.Lease!,
                (conversation, _) =>
                {
                    conversation.Phase = "new_owner_completed";
                    return Task.FromResult(new SaveLifeAgentTurnCompletion<bool>("新租约结果", null, true));
                },
                now.AddMinutes(1).AddSeconds(2),
                TestContext.Current.CancellationToken);

            var conversation = db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();
            var reply = db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Single(item => item.Role == "assistant");
            Assert.Equal(1, conversation.StateRevision);
            Assert.Equal("new_owner_completed", conversation.Phase);
            Assert.Null(conversation.GenerationLeaseOwner);
            Assert.Equal("新租约结果", reply.Content);
            Assert.Equal("completed", reply.GenerationStatus);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task ExpiredLease_WithoutTakeover_CannotCommit()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitCurrentSchema(db);
            db.Insertable(Conversation(1)).ExecuteCommand();
            db.Insertable(PendingUserMessage(101, 1, "client-1")).ExecuteCommand();
            var now = new DateTime(2026, 7, 16, 11, 30, 0);
            var reservation = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 101, "account-a", "lease-1", now, TimeSpan.FromSeconds(30),
                () => 201, TestContext.Current.CancellationToken);

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                    db,
                    reservation.Lease!,
                    (_, _) => Task.FromResult(new SaveLifeAgentTurnCompletion<bool>("过期结果", null, true)),
                    now.AddSeconds(31),
                    TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeAgentFailureCodes.TurnSuperseded, error.Code);
            Assert.Equal(0, db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Single()
                .StateRevision);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task FailedOldTurn_AfterLaterCompletion_CannotBeRetriedInPlace()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitCurrentSchema(db);
            db.Insertable(Conversation(1)).ExecuteCommand();
            db.Insertable(new[]
            {
                PendingUserMessage(101, 1, "client-1"),
                PendingUserMessage(102, 2, "client-2")
            }).ExecuteCommand();
            var now = new DateTime(2026, 7, 16, 12, 0, 0);
            var first = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 101, "account-a", "lease-1", now, TimeSpan.FromMinutes(1),
                () => 201, TestContext.Current.CancellationToken);
            await SaveLifeAgentTurnPersistence.FailGenerationAsync(
                db, first.Lease!, "{\"code\":\"SYNTHETIC\"}", now.AddSeconds(1),
                TestContext.Current.CancellationToken);

            var second = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 102, "account-a", "lease-2", now.AddSeconds(2), TimeSpan.FromMinutes(1),
                () => 202, TestContext.Current.CancellationToken);
            await SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                db,
                second.Lease!,
                (_, _) => Task.FromResult(new SaveLifeAgentTurnCompletion<bool>("第二轮完成", null, true)),
                now.AddSeconds(3),
                TestContext.Current.CancellationToken);

            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                    db, 101, "account-a", "lease-retry", now.AddSeconds(4), TimeSpan.FromMinutes(1),
                    () => 203, TestContext.Current.CancellationToken));

            Assert.Equal(SaveLifeAgentFailureCodes.TurnSuperseded, error.Code);
            Assert.Equal(SaveLifeAgentTurnStatuses.Failed, db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Single(item => item.Id == 101)
                .TurnStatus);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task RevisionGuard_WithTwentyCommandsFromSameRevision_AllowsOnlyOne()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using (var setup = CreateSqliteClient(dbPath))
            {
                InitCurrentSchema(setup);
                setup.Insertable(Conversation(1)).ExecuteCommand();
                setup.Ado.ExecuteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;");
            }

            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(1, 20).Select(async sequence =>
            {
                await start.Task;
                using var db = CreateSqliteClient(dbPath);
                db.Ado.ExecuteCommand("PRAGMA busy_timeout=30000;");
                try
                {
                    await SaveLifeAgentConversationRevisionGuard.AdvanceRevisionAsync(
                        db,
                        conversationId: 1,
                        externalUserId: "account-a",
                        expectedRevision: 0,
                        changedTime: new DateTime(2026, 7, 16, 13, 0, 0).AddSeconds(sequence),
                        cancellationToken: TestContext.Current.CancellationToken);
                    return true;
                }
                catch (SaveLifeAgentUnavailableException error)
                    when (error.Code == SaveLifeAgentFailureCodes.TurnSuperseded)
                {
                    return false;
                }
            }).ToArray();

            start.SetResult();
            var results = await Task.WhenAll(tasks);

            Assert.Single(results, value => value);
            using var verify = CreateSqliteClient(dbPath);
            Assert.Equal(1, verify.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Single()
                .StateRevision);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task PatientSwitch_InvalidatesInFlightLeaseWithoutPersistingLateAnswer()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitCurrentSchema(db);
            db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifePatientProfileProposal));
            db.Insertable(Conversation(1)).ExecuteCommand();
            db.Insertable(PendingUserMessage(101, 1, "client-1")).ExecuteCommand();
            db.Insertable(new SaveLifePatientProfile
            {
                Id = 502,
                TenantId = TenantId,
                OwnerExternalUserId = "account-a",
                AccountPatientKey = SaveLifePatientProfileKeys.ForOther(502),
                RelationshipToAccount = SaveLifePatientRelationships.Parent,
                RelationshipDescription = "母亲",
                DisplayName = "母亲",
                CreateTime = new DateTime(2026, 7, 16)
            }).ExecuteCommand();
            var now = new DateTime(2026, 7, 16, 14, 0, 0);
            var reservation = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 101, "account-a", "lease-1", now, TimeSpan.FromMinutes(1),
                () => 201, TestContext.Current.CancellationToken);

            var switched = await SaveLifePatientSwitchPolicy.ConfirmInTransactionAsync(
                db, 1, "account-a", 502, now.AddSeconds(1));
            var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
                SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                    db,
                    reservation.Lease!,
                    (_, _) => Task.FromResult(
                        new SaveLifeAgentTurnCompletion<bool>("不应提交的旧患者回答", null, true)),
                    now.AddSeconds(2),
                    TestContext.Current.CancellationToken));

            Assert.Equal(1, switched.StateRevision);
            Assert.Equal(SaveLifeAgentFailureCodes.PatientContextChanged, error.Code);
            var persisted = db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();
            var reply = db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Single(item => item.Role == "assistant");
            Assert.Equal(502, persisted.CurrentPatientProfileId);
            Assert.Equal("created", persisted.Phase);
            Assert.Equal(string.Empty, reply.Content);
            Assert.Equal("processing", reply.GenerationStatus);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task CompleteGeneration_WhenAssistantWriteFails_RollsBackEveryBoundary()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitCurrentSchema(db);
            db.Insertable(Conversation(1)).ExecuteCommand();
            db.Insertable(PendingUserMessage(101, 1, "client-1")).ExecuteCommand();
            var now = new DateTime(2026, 7, 16, 15, 0, 0);
            var reservation = await SaveLifeAgentTurnPersistence.AcquireGenerationAsync(
                db, 101, "account-a", "lease-1", now, TimeSpan.FromMinutes(1),
                () => 201, TestContext.Current.CancellationToken);
            db.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_assistant_completion
                BEFORE UPDATE OF GenerationStatus ON sl_agent_message
                WHEN NEW.GenerationStatus = 'completed'
                BEGIN
                    SELECT RAISE(ABORT, 'forced assistant completion failure');
                END;
                """);

            await Assert.ThrowsAnyAsync<Exception>(() =>
                SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                    db,
                    reservation.Lease!,
                    (conversation, _) =>
                    {
                        conversation.Phase = "should_rollback";
                        conversation.LastMessage = "不应提交";
                        return Task.FromResult(
                            new SaveLifeAgentTurnCompletion<bool>("不应提交", null, true));
                    },
                    now.AddSeconds(1),
                    TestContext.Current.CancellationToken));

            var rolledBackConversation = db.Queryable<SaveLifeAgentConversation>().ClearFilter().Single();
            var rolledBackUser = db.Queryable<SaveLifeAgentMessage>().ClearFilter().Single(item => item.Id == 101);
            var rolledBackAssistant = db.Queryable<SaveLifeAgentMessage>()
                .ClearFilter()
                .Single(item => item.Role == "assistant");
            Assert.Equal(0, rolledBackConversation.StateRevision);
            Assert.Null(rolledBackConversation.Phase);
            Assert.Equal("lease-1", rolledBackConversation.GenerationLeaseOwner);
            Assert.Equal(SaveLifeAgentTurnStatuses.Processing, rolledBackUser.TurnStatus);
            Assert.Equal("processing", rolledBackAssistant.GenerationStatus);
            Assert.Equal(string.Empty, rolledBackAssistant.Content);

            db.Ado.ExecuteCommand("DROP TRIGGER fail_assistant_completion");
            await SaveLifeAgentTurnPersistence.CompleteGenerationAsync(
                db,
                reservation.Lease!,
                (conversation, _) =>
                {
                    conversation.Phase = "completed_after_retry";
                    return Task.FromResult(new SaveLifeAgentTurnCompletion<bool>("完成", null, true));
                },
                now.AddSeconds(2),
                TestContext.Current.CancellationToken);
            Assert.Equal(1, db.Queryable<SaveLifeAgentConversation>()
                .ClearFilter()
                .Single()
                .StateRevision);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    private static SaveLifeAgentConversation Conversation(long id)
    {
        return new SaveLifeAgentConversation
        {
            Id = id,
            TenantId = TenantId,
            ExternalUserId = "account-a",
            CurrentPatientProfileId = 501,
            PatientContextVersion = 3,
            NextTurnSequence = 1,
            StateRevision = 0,
            Status = "active",
            CreateTime = new DateTime(2026, 7, 16)
        };
    }

    private static SaveLifeAgentMessage PendingUserMessage(
        long id,
        long turnSequence,
        string clientMessageId)
    {
        var message = UserMessage(
            id,
            clientMessageId,
            new DateTime(2026, 7, 16, 9, 0, 0).AddSeconds(turnSequence));
        message.TurnSequence = turnSequence;
        message.TurnStatus = SaveLifeAgentTurnStatuses.Pending;
        return message;
    }

    private static void InitCurrentSchema(ISqlSugarClient db)
    {
        db.CodeFirst.InitTables(
            typeof(SaveLifeAgentConversation),
            typeof(SaveLifeAgentMessage));
        SaveLifeAgentTurnMigration.Apply(db);
    }

    private static SaveLifeAgentMessage UserMessage(long id, string clientMessageId, DateTime createTime)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            TenantId = TenantId,
            ConversationId = 1,
            ExternalUserId = "account-a",
            PatientProfileId = 501,
            PatientContextVersion = 3,
            Role = "user",
            Content = $"用户消息-{id}",
            ClientMessageId = clientMessageId,
            CreateTime = createTime
        };
    }

    private static SaveLifeAgentMessage AssistantMessage(
        long id,
        long replyToMessageId,
        string generationStatus,
        DateTime createTime)
    {
        return new SaveLifeAgentMessage
        {
            Id = id,
            TenantId = TenantId,
            ConversationId = 1,
            ExternalUserId = "account-a",
            PatientProfileId = 501,
            PatientContextVersion = 3,
            Role = "assistant",
            Content = generationStatus == "completed" ? "合成回答" : string.Empty,
            ReplyToMessageId = replyToMessageId,
            GenerationStatus = generationStatus,
            CreateTime = createTime
        };
    }

    private static void DropTurnColumns(ISqlSugarClient db)
    {
        foreach (var (table, column) in new[]
        {
            ("sl_agent_conversation", "NextTurnSequence"),
            ("sl_agent_conversation", "StateRevision"),
            ("sl_agent_conversation", "GenerationLeaseOwner"),
            ("sl_agent_conversation", "GenerationLeaseTurnSequence"),
            ("sl_agent_conversation", "GenerationLeaseExpiresTime"),
            ("sl_agent_message", "TurnSequence"),
            ("sl_agent_message", "TurnStatus"),
            ("sl_agent_message", "TurnStateRevision"),
            ("sl_agent_message", "GenerationLeaseOwner"),
            ("sl_agent_message", "GenerationLeaseExpiresTime")
        })
        {
            if (db.DbMaintenance.GetColumnInfosByTableName(table, false)
                .Any(item => item.DbColumnName.Equals(column, StringComparison.OrdinalIgnoreCase)))
            {
                db.Ado.ExecuteCommand($"ALTER TABLE \"{table}\" DROP COLUMN \"{column}\"");
            }
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
