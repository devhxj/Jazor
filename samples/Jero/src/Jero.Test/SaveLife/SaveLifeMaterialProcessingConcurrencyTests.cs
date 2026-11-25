using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMaterialProcessingConcurrencyTests
{
    private const long TenantId = 1300000000001;
    private static readonly DateTime Now = new(2026, 7, 16, 20, 0, 0);

    [Fact]
    public async Task DeleteAsync_AfterWorkerClaim_InvalidatesLeaseAndQueuesBothCleanups()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial();
        var lease = await SaveLifeMaterialProcessingPersistence.TryClaimAsync(
            database.Db,
            101,
            Now,
            () => "lease-a",
            TestContext.Current.CancellationToken);
        Assert.NotNull(lease);

        var deletion = await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(),
            Now.AddSeconds(1),
            database.NextId,
            TestContext.Current.CancellationToken);
        var staleState = database.Material();
        staleState.ExtractStatus = "synced";
        staleState.IndexStatus = "synced";
        staleState.ExtractedText = "must not survive";

        var committed = await SaveLifeMaterialProcessingPersistence.TryCommitAsync(
            database.Db,
            lease!,
            staleState,
            Now.AddSeconds(2),
            TestContext.Current.CancellationToken);

        var deleted = database.Material();
        Assert.False(deletion.WasAlreadyDeleted);
        Assert.False(committed);
        Assert.True(deleted.IsDelete);
        Assert.Equal(2, deleted.ProcessingRevision);
        Assert.Equal("deleted", deleted.ExtractStatus);
        Assert.Equal("deleted", deleted.IndexStatus);
        Assert.Null(deleted.ExtractedText);
        Assert.Null(deleted.ProcessingLeaseOwner);
        Assert.Null(deleted.ProcessingLeaseExpiresTime);
        Assert.Collection(
            database.Outbox().OrderBy(item => item.WorkType),
            item =>
            {
                Assert.Equal(SaveLifeMaterialWorkTypes.CleanupFile, item.WorkType);
                Assert.Equal(901, item.AggregateId);
            },
            item =>
            {
                Assert.Equal(SaveLifeMaterialWorkTypes.CleanupMaterialIndex, item.WorkType);
                Assert.Equal(101, item.AggregateId);
            });
    }

    [Fact]
    public async Task DeleteAsync_WhenRepeated_ReusesCleanupTasksAndKeepsMonotonicTombstone()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial();

        await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(),
            Now,
            database.NextId,
            TestContext.Current.CancellationToken);
        var replay = await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(),
            Now.AddMinutes(1),
            database.NextId,
            TestContext.Current.CancellationToken);

        Assert.True(replay.WasAlreadyDeleted);
        Assert.Equal(2, database.Material().ProcessingRevision);
        Assert.Equal(2, database.Outbox().Count);
    }

    [Fact]
    public async Task TryPublishAsync_WhenMaterialWasDeletedAfterVendorResponse_DiscardsLateIndex()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial();
        var lease = await SaveLifeMaterialProcessingPersistence.TryClaimAsync(
            database.Db,
            101,
            Now,
            () => "lease-a",
            TestContext.Current.CancellationToken);
        Assert.NotNull(lease);
        await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(),
            Now.AddSeconds(1),
            database.NextId,
            TestContext.Current.CancellationToken);
        var publishCalled = false;

        var published = await SaveLifeMaterialIndexPublicationPersistence.TryPublishAsync(
            database.Db,
            lease!,
            async () =>
            {
                publishCalled = true;
                await database.Db.Insertable(new SaveLifeAgentVectorIndex
                {
                    Id = 801,
                    ResourceType = "material",
                    ResourceId = 101,
                    ExternalUserId = "account-a",
                    PatientProfileId = 501,
                    Title = "late index",
                    SyncStatus = "synced",
                    CreateTime = Now
                }).ExecuteCommandAsync(TestContext.Current.CancellationToken);
            },
            TestContext.Current.CancellationToken);

        Assert.False(published);
        Assert.False(publishCalled);
        Assert.Empty(database.Indexes());
    }

    [Fact]
    public async Task RequeueAsync_InvalidatesCurrentLeaseBeforeReturningPendingState()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial();
        var lease = await SaveLifeMaterialProcessingPersistence.TryClaimAsync(
            database.Db,
            101,
            Now,
            () => "lease-a",
            TestContext.Current.CancellationToken);
        Assert.NotNull(lease);

        var requeued = await SaveLifeMaterialProcessingPersistence.RequeueAsync(
            database.Db,
            101,
            Now.AddSeconds(1),
            TestContext.Current.CancellationToken);

        Assert.Equal(2, requeued.ProcessingRevision);
        Assert.Equal("pending", requeued.ExtractStatus);
        Assert.Equal("pending", requeued.IndexStatus);
        Assert.False(await SaveLifeMaterialProcessingPersistence.IsCurrentAsync(
            database.Db,
            lease!,
            Now.AddSeconds(2),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TrySetContentSha256Async_WithCurrentLegacyLease_UpdatesFingerprintAndLease()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial(includeContentSha256: false);
        var lease = await SaveLifeMaterialProcessingPersistence.TryClaimAsync(
            database.Db,
            101,
            Now,
            () => "lease-a",
            TestContext.Current.CancellationToken);
        Assert.NotNull(lease);
        Assert.Null(lease!.ContentSha256);
        var contentSha256 = new string('B', 64);

        var updatedLease = await SaveLifeMaterialProcessingPersistence.TrySetContentSha256Async(
            database.Db,
            lease,
            contentSha256,
            Now.AddSeconds(1),
            TestContext.Current.CancellationToken);

        Assert.NotNull(updatedLease);
        Assert.Equal(contentSha256, updatedLease!.ContentSha256);
        Assert.Equal(contentSha256, database.Material().ContentSha256);
        Assert.False(await SaveLifeMaterialProcessingPersistence.IsCurrentAsync(
            database.Db,
            lease,
            Now.AddSeconds(2),
            TestContext.Current.CancellationToken));
        Assert.True(await SaveLifeMaterialProcessingPersistence.IsCurrentAsync(
            database.Db,
            updatedLease,
            Now.AddSeconds(2),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TrySetContentSha256Async_AfterDelete_DoesNotRestoreLegacyFingerprint()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial(includeContentSha256: false);
        var lease = await SaveLifeMaterialProcessingPersistence.TryClaimAsync(
            database.Db,
            101,
            Now,
            () => "lease-a",
            TestContext.Current.CancellationToken);
        Assert.NotNull(lease);
        await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(),
            Now.AddSeconds(1),
            database.NextId,
            TestContext.Current.CancellationToken);

        var updatedLease = await SaveLifeMaterialProcessingPersistence.TrySetContentSha256Async(
            database.Db,
            lease!,
            new string('B', 64),
            Now.AddSeconds(2),
            TestContext.Current.CancellationToken);

        Assert.Null(updatedLease);
        Assert.Null(database.Material().ContentSha256);
    }

    [Fact]
    public async Task DeleteAsync_WhenSecondOutboxWriteFails_RollsBackTombstoneAndFirstOutbox()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial();
        database.Db.Ado.ExecuteCommand("""
            CREATE TRIGGER fail_index_cleanup BEFORE INSERT ON sl_material_work_outbox
            WHEN NEW.WorkType = 'cleanup_material_index'
            BEGIN SELECT RAISE(ABORT, 'forced index cleanup failure'); END;
            """);

        await Assert.ThrowsAnyAsync<Exception>(() => SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(),
            Now,
            database.NextId,
            TestContext.Current.CancellationToken));

        var material = database.Material();
        Assert.False(material.IsDelete);
        Assert.Equal(1, material.ProcessingRevision);
        Assert.Empty(database.Outbox());
    }

    [Fact]
    public async Task DeleteAsync_WhenSharedFileCleanupPreviouslyCompleted_ReactivatesCleanupForLastReference()
    {
        using var database = new ProcessingDatabase();
        database.InsertMaterial();
        database.InsertMaterial(id: 102, slot: 2);
        await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(),
            Now,
            database.NextId,
            TestContext.Current.CancellationToken);
        await database.Db.Updateable<SaveLifeMaterialWorkOutbox>()
            .SetColumns(item => new SaveLifeMaterialWorkOutbox { Status = SaveLifeMaterialWorkStatuses.Completed })
            .Where(item => item.WorkType == SaveLifeMaterialWorkTypes.CleanupFile)
            .ExecuteCommandAsync(TestContext.Current.CancellationToken);

        await SaveLifeMaterialDeletionPersistence.DeleteAsync(
            database.Db,
            Request(materialId: 102),
            Now.AddMinutes(1),
            database.NextId,
            TestContext.Current.CancellationToken);

        var cleanup = Assert.Single(
            database.Outbox(),
            item => item.WorkType == SaveLifeMaterialWorkTypes.CleanupFile);
        Assert.Equal(SaveLifeMaterialWorkStatuses.Pending, cleanup.Status);
    }

    private static SaveLifeMaterialDeletionRequest Request(long materialId = 101) => new(
        TenantId,
        "account-a",
        PatientProfileId: 501,
        EnrollmentRecordId: 3001,
        MaterialId: materialId);

    private sealed class ProcessingDatabase : IDisposable
    {
        private readonly string _path;
        private long _nextId = 10_000;

        public ProcessingDatabase()
        {
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            Db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={_path};Pooling=False",
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
            Db.CodeFirst.InitTables<SaveLifeAgentVectorIndex>();
            SaveLifeMaterialUploadMigration.Apply(Db);
        }

        public SqlSugarClient Db { get; }

        public long NextId() => Interlocked.Increment(ref _nextId);

        public void InsertMaterial(long id = 101, int slot = 1, bool includeContentSha256 = true)
        {
            Db.Insertable(new SaveLifeEnrollmentMaterial
            {
                Id = id,
                TenantId = TenantId,
                PatientProfileId = 501,
                EnrollmentRecordId = 3001,
                ExternalUserId = "account-a",
                ProjectId = 701,
                FileId = 901,
                SizeBytes = 1024,
                ContentSha256 = includeContentSha256 ? new string('A', 64) : null,
                UploadSlot = slot,
                MaterialType = "diagnosis_report",
                FileName = "report",
                FileSuffix = ".pdf",
                ContentType = "application/pdf",
                Status = "submitted",
                ExtractStatus = "pending",
                IndexStatus = "pending",
                ProcessingRevision = 1,
                CreateTime = Now
            }).ExecuteCommand();
        }

        public SaveLifeEnrollmentMaterial Material(long id = 101) =>
            Db.Queryable<SaveLifeEnrollmentMaterial>().ClearFilter().Single(item => item.Id == id);

        public List<SaveLifeMaterialWorkOutbox> Outbox() =>
            Db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().ToList();

        public List<SaveLifeAgentVectorIndex> Indexes() =>
            Db.Queryable<SaveLifeAgentVectorIndex>().ClearFilter().ToList();

        public void Dispose()
        {
            Db.Dispose();
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }
}
