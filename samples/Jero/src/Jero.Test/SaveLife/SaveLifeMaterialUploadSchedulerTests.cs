using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMaterialUploadSchedulerTests
{
    [Fact]
    public async Task DispatchMaterialAsync_WhenQueueFails_DoesNotFailCompletedUpload()
    {
        var taskQueue = CreateTaskQueueProxy(out var proxy);
        proxy.ThrowOnInvoke = true;
        var logger = new RecordingLogger<SaveLifeKnowledgeBaseIndexTaskDispatcher>();
        var dispatcher = CreateDispatcher(taskQueue, logger);

        var accepted = await dispatcher.DispatchMaterialAsync(123);

        Assert.False(accepted);
        var warning = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Warning, warning.Level);
        Assert.Contains("123", warning.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("病例", warning.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task DispatchMaterialAsync_WhenQueueSucceeds_EnqueuesExactlyOnce()
    {
        var taskQueue = CreateTaskQueueProxy(out var proxy);
        var dispatcher = CreateDispatcher(
            taskQueue,
            new RecordingLogger<SaveLifeKnowledgeBaseIndexTaskDispatcher>());

        var accepted = await dispatcher.DispatchMaterialAsync(456);

        Assert.True(accepted);
        Assert.Equal(1, proxy.EnqueueAsyncCallCount);
    }

    [Fact]
    public async Task ProcessPendingAsync_WhenQueueIsUnavailable_LeavesDurableRetry()
    {
        await WithOutboxDatabase(async db =>
        {
            db.Insertable(Outbox(1, SaveLifeMaterialWorkTypes.IndexMaterial, 101)).ExecuteCommand();
            var processor = CreateOutboxProcessor(
                db,
                new StubDispatcher { AcceptMaterial = false },
                new StubFileCleanup());

            var result = await processor.ProcessPendingAsync(10, TestContext.Current.CancellationToken);

            var persisted = db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().Single();
            Assert.Equal(1, result.FailedCount);
            Assert.Equal(SaveLifeMaterialWorkStatuses.Pending, persisted.Status);
            Assert.Equal(1, persisted.AttemptCount);
            Assert.NotNull(persisted.NextAttemptTime);
            Assert.Equal("dispatch_unavailable", persisted.LastErrorCode);
        });
    }

    [Fact]
    public async Task ProcessPendingAsync_AfterUploadProcessExited_DispatchesPersistedOutbox()
    {
        await WithOutboxDatabase(async db =>
        {
            db.Insertable(Outbox(1, SaveLifeMaterialWorkTypes.IndexMaterial, 101)).ExecuteCommand();
            var dispatcher = new StubDispatcher { AcceptMaterial = true };
            var processor = CreateOutboxProcessor(db, dispatcher, new StubFileCleanup());

            var result = await processor.ProcessPendingAsync(10, TestContext.Current.CancellationToken);

            var persisted = db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().Single();
            Assert.Equal(1, result.CompletedCount);
            Assert.Equal(SaveLifeMaterialWorkStatuses.Dispatched, persisted.Status);
            Assert.Equal([101L], dispatcher.MaterialIds);
        });
    }

    [Fact]
    public async Task ProcessPendingAsync_WithExpiredDispatchLease_ReplaysWork()
    {
        await WithOutboxDatabase(async db =>
        {
            var outbox = Outbox(1, SaveLifeMaterialWorkTypes.IndexMaterial, 101);
            outbox.Status = SaveLifeMaterialWorkStatuses.Dispatching;
            outbox.LeaseOwner = "dead-process";
            outbox.LeaseExpiresTime = DateTime.Now.AddMinutes(-1);
            db.Insertable(outbox).ExecuteCommand();
            var dispatcher = new StubDispatcher { AcceptMaterial = true };
            var processor = CreateOutboxProcessor(db, dispatcher, new StubFileCleanup());

            await processor.ProcessPendingAsync(10, TestContext.Current.CancellationToken);

            Assert.Equal(SaveLifeMaterialWorkStatuses.Dispatched,
                db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().Single().Status);
            Assert.Single(dispatcher.MaterialIds);
        });
    }

    [Fact]
    public async Task ProcessPendingAsync_WhenFileCleanupFails_RetriesWithoutLosingTask()
    {
        await WithOutboxDatabase(async db =>
        {
            db.Insertable(Outbox(1, SaveLifeMaterialWorkTypes.CleanupFile, 901)).ExecuteCommand();
            var cleanup = new StubFileCleanup { ThrowOnDelete = true };
            var processor = CreateOutboxProcessor(db, new StubDispatcher(), cleanup);

            await processor.ProcessPendingAsync(10, TestContext.Current.CancellationToken);

            var persisted = db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().Single();
            Assert.Equal(SaveLifeMaterialWorkStatuses.Pending, persisted.Status);
            Assert.Equal("file_cleanup_failed", persisted.LastErrorCode);
            Assert.Equal([901L], cleanup.FileIds);
        });
    }

    [Fact]
    public async Task ProcessPendingAsync_WhenMaterialIndexCleanupFails_RetriesWithoutLosingTask()
    {
        await WithOutboxDatabase(async db =>
        {
            db.Insertable(Outbox(1, SaveLifeMaterialWorkTypes.CleanupMaterialIndex, 101)).ExecuteCommand();
            var cleanup = new StubIndexCleanup { ThrowOnDelete = true };
            var processor = CreateOutboxProcessor(
                db,
                new StubDispatcher(),
                new StubFileCleanup(),
                cleanup);

            await processor.ProcessPendingAsync(10, TestContext.Current.CancellationToken);

            var persisted = db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().Single();
            Assert.Equal(SaveLifeMaterialWorkStatuses.Pending, persisted.Status);
            Assert.Equal("index_cleanup_failed", persisted.LastErrorCode);
            Assert.Equal([101L], cleanup.MaterialIds);
        });
    }

    [Fact]
    public async Task ProcessPendingAsync_WithMaterialIndexCleanup_RemovesIndexAndCompletesTask()
    {
        await WithOutboxDatabase(async db =>
        {
            db.Insertable(Outbox(1, SaveLifeMaterialWorkTypes.CleanupMaterialIndex, 101)).ExecuteCommand();
            db.Insertable(new SaveLifeAgentVectorIndex
            {
                Id = 801,
                ResourceType = "material",
                ResourceId = 101,
                ExternalUserId = "account-a",
                PatientProfileId = 501,
                Title = "synthetic index",
                SyncStatus = "synced",
                CreateTime = DateTime.Now
            }).ExecuteCommand();
            var processor = CreateOutboxProcessor(
                db,
                new StubDispatcher(),
                new StubFileCleanup(),
                new SaveLifeMaterialIndexCleanup(
                    SqlSugarRepository<SaveLifeAgentVectorIndex>.CreateForContext(db)));

            await processor.ProcessPendingAsync(10, TestContext.Current.CancellationToken);

            Assert.Empty(db.Queryable<SaveLifeAgentVectorIndex>().ClearFilter().ToList());
            Assert.Equal(
                SaveLifeMaterialWorkStatuses.Completed,
                db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().Single().Status);
        });
    }

    [Theory]
    [InlineData(nameof(SaveLifeEnrollmentMaterialService.MiniAppUpload))]
    [InlineData(nameof(SaveLifeEnrollmentMaterialService.MiniAppPersonalUpload))]
    public void MiniAppUploadEndpoints_UseReliableWorkflowWithNonThrowingScheduler(string methodName)
    {
        var endpoint = typeof(SaveLifeEnrollmentMaterialService).GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public)!;
        var processMethod = typeof(ISaveLifeMaterialWorkOutboxProcessor).GetMethod(
            nameof(ISaveLifeMaterialWorkOutboxProcessor.ProcessOneAsync),
            BindingFlags.Instance | BindingFlags.Public)!;
        var coordinatorMethod = typeof(ISaveLifeReliableMaterialUploadCoordinator).GetMethod(
            nameof(ISaveLifeReliableMaterialUploadCoordinator.ExecuteAsync),
            BindingFlags.Instance | BindingFlags.Public)!;
        var coordinatorImplementation = typeof(SaveLifeReliableMaterialUploadCoordinator).GetMethod(
            nameof(SaveLifeReliableMaterialUploadCoordinator.ExecuteAsync),
            BindingFlags.Instance | BindingFlags.Public)!;
        var dispatchWorkflow = typeof(SaveLifeReliableMaterialUploadCoordinator).GetMethod(
            "TryDispatchOutboxAsync",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var workflow = typeof(SaveLifeEnrollmentMaterialService).GetMethod(
            "UploadReliableMaterialAsync",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

        Assert.True(AsyncMethodCalls(endpoint, workflow));
        Assert.True(AsyncMethodCalls(workflow, coordinatorMethod));
        Assert.True(AsyncMethodCalls(coordinatorImplementation, dispatchWorkflow));
        Assert.True(AsyncMethodCalls(dispatchWorkflow, processMethod));
    }

    private static bool AsyncMethodCalls(MethodInfo asyncMethod, MethodInfo calledMethod)
    {
        var stateMachine = asyncMethod.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType;
        var moveNext = stateMachine?.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
        var il = moveNext?.GetMethodBody()?.GetILAsByteArray();
        if (il == null)
            return false;

        return il.AsSpan().IndexOf(BitConverter.GetBytes(calledMethod.MetadataToken)) >= 0;
    }

    private static object CreateTaskQueueProxy(out TaskQueueProxy proxy)
    {
        var taskQueueType = typeof(SaveLifeKnowledgeBaseIndexTaskDispatcher)
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
            .Single()
            .GetParameters()[0]
            .ParameterType;
        var taskQueue = DispatchProxy.Create(taskQueueType, typeof(TaskQueueProxy));
        proxy = (TaskQueueProxy)taskQueue;
        return taskQueue;
    }

    private static SaveLifeKnowledgeBaseIndexTaskDispatcher CreateDispatcher(
        object taskQueue,
        ILogger<SaveLifeKnowledgeBaseIndexTaskDispatcher> logger)
    {
        return (SaveLifeKnowledgeBaseIndexTaskDispatcher)Activator.CreateInstance(
            typeof(SaveLifeKnowledgeBaseIndexTaskDispatcher),
            taskQueue,
            logger)!;
    }

    private static SaveLifeMaterialWorkOutboxProcessor CreateOutboxProcessor(
        ISqlSugarClient db,
        ISaveLifeKnowledgeBaseIndexTaskDispatcher dispatcher,
        ISaveLifeMaterialFileCleanup cleanup,
        ISaveLifeMaterialIndexCleanup? indexCleanup = null)
    {
        return new SaveLifeMaterialWorkOutboxProcessor(
            SqlSugarRepository<SaveLifeMaterialWorkOutbox>.CreateForContext(db),
            SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(db),
            dispatcher,
            cleanup,
            indexCleanup ?? new StubIndexCleanup());
    }

    private static SaveLifeMaterialWorkOutbox Outbox(long id, string workType, long aggregateId)
    {
        return new SaveLifeMaterialWorkOutbox
        {
            Id = id,
            TenantId = 1300000000001,
            WorkType = workType,
            AggregateId = aggregateId,
            Status = SaveLifeMaterialWorkStatuses.Pending,
            CreateTime = DateTime.Now
        };
    }

    private static async Task WithOutboxDatabase(Func<ISqlSugarClient, Task> test)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={path};Pooling=False",
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
            db.CodeFirst.InitTables(
                typeof(SaveLifeMaterialWorkOutbox),
                typeof(SaveLifeEnrollmentMaterial),
                typeof(SaveLifeAgentVectorIndex));
            await test(db);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    public class TaskQueueProxy : DispatchProxy
    {
        public bool ThrowOnInvoke { get; set; }

        public int EnqueueAsyncCallCount { get; private set; }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name != "EnqueueAsync")
                throw new NotSupportedException(targetMethod?.Name);

            EnqueueAsyncCallCount++;
            if (ThrowOnInvoke)
                throw new InvalidOperationException("queue unavailable");

            if (targetMethod.ReturnType == typeof(ValueTask))
                return ValueTask.CompletedTask;
            if (targetMethod.ReturnType.IsGenericType &&
                targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                return Activator.CreateInstance(targetMethod.ReturnType);
            }
            if (targetMethod.ReturnType == typeof(Task))
                return Task.CompletedTask;

            throw new NotSupportedException(targetMethod.ReturnType.FullName);
        }
    }

    private sealed class StubDispatcher : ISaveLifeKnowledgeBaseIndexTaskDispatcher
    {
        public bool AcceptMaterial { get; set; }

        public List<long> MaterialIds { get; } = [];

        public ValueTask DispatchKnowledgeAsync(long knowledgeId) => ValueTask.CompletedTask;

        public ValueTask DispatchTrialAsync(long trialId) => ValueTask.CompletedTask;

        public ValueTask<bool> DispatchMaterialAsync(long materialId)
        {
            MaterialIds.Add(materialId);
            return ValueTask.FromResult(AcceptMaterial);
        }
    }

    private sealed class StubFileCleanup : ISaveLifeMaterialFileCleanup
    {
        public bool ThrowOnDelete { get; set; }

        public List<long> FileIds { get; } = [];

        public Task DeleteAsync(long fileId)
        {
            FileIds.Add(fileId);
            return ThrowOnDelete
                ? Task.FromException(new InvalidOperationException("cleanup unavailable"))
                : Task.CompletedTask;
        }
    }

    private sealed class StubIndexCleanup : ISaveLifeMaterialIndexCleanup
    {
        public bool ThrowOnDelete { get; set; }

        public List<long> MaterialIds { get; } = [];

        public Task DeleteAsync(long materialId, CancellationToken cancellationToken = default)
        {
            MaterialIds.Add(materialId);
            return ThrowOnDelete
                ? Task.FromException(new InvalidOperationException("index cleanup unavailable"))
                : Task.CompletedTask;
        }
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Entries.Add(new LogEntry(logLevel, formatter(state, exception)));
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message);
}
