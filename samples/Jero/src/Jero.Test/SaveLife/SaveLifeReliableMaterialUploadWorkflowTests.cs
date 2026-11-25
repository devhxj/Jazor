using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SqlSugar;
using Jero.Core;
using Jero.Core.Service;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeReliableMaterialUploadWorkflowTests
{
    private const long TenantId = 1300000000001;

    [Fact]
    public async Task ExecuteAsync_WhenFileWriteFails_DoesNotCreateDatabaseRows()
    {
        using var database = new WorkflowDatabase();
        var files = new StubFileStore { ThrowOnUpload = true };
        var workflow = database.CreateWorkflow(files, new StubOutboxProcessor());

        await Assert.ThrowsAnyAsync<Exception>(() => workflow.ExecuteAsync(
            database.Operation("command-1"), TestContext.Current.CancellationToken));

        Assert.Empty(database.Materials());
        Assert.Empty(database.Commands());
        Assert.Empty(database.Outbox());
        Assert.Empty(files.DeletedFileIds);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDatabaseCommitFails_DeletesUploadedFile()
    {
        using var database = new WorkflowDatabase();
        database.FailMaterialInsert();
        var files = new StubFileStore();
        var workflow = database.CreateWorkflow(files, new StubOutboxProcessor());

        await Assert.ThrowsAnyAsync<Exception>(() => workflow.ExecuteAsync(
            database.Operation("command-1"), TestContext.Current.CancellationToken));

        Assert.Equal([9001L], files.DeletedFileIds);
        Assert.Empty(database.Materials());
        Assert.Empty(database.Commands());
        Assert.Empty(database.Outbox());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCompensationDeleteFails_PersistsCleanupOutbox()
    {
        using var database = new WorkflowDatabase();
        database.FailMaterialInsert();
        var files = new StubFileStore { ThrowOnDelete = true };
        var workflow = database.CreateWorkflow(files, new StubOutboxProcessor());

        await Assert.ThrowsAnyAsync<Exception>(() => workflow.ExecuteAsync(
            database.Operation("command-1"), TestContext.Current.CancellationToken));

        var cleanup = Assert.Single(database.Outbox());
        Assert.Equal(SaveLifeMaterialWorkTypes.CleanupFile, cleanup.WorkType);
        Assert.Equal(9001, cleanup.AggregateId);
        Assert.Equal(SaveLifeMaterialWorkStatuses.Pending, cleanup.Status);
        Assert.Empty(database.Materials());
        Assert.Empty(database.Commands());
    }

    [Fact]
    public async Task ExecuteAsync_AfterResponseLoss_ReplaysWithoutWritingAnotherFile()
    {
        using var database = new WorkflowDatabase();
        var files = new StubFileStore();
        var outbox = new StubOutboxProcessor();
        var workflow = database.CreateWorkflow(files, outbox);

        var first = await workflow.ExecuteAsync(
            database.Operation("command-1"), TestContext.Current.CancellationToken);
        var replay = await workflow.ExecuteAsync(
            database.Operation("command-1"), TestContext.Current.CancellationToken);

        Assert.Equal(first.Id, replay.Id);
        Assert.Equal(1, files.UploadCount);
        Assert.Empty(files.DeletedFileIds);
        Assert.Single(database.Materials());
        Assert.Single(database.Commands());
        Assert.Single(database.Outbox());
        Assert.Single(outbox.OutboxIds);
    }

    [Fact]
    public async Task ExecuteAsync_WhenImmediateDispatchFails_ReturnsPendingMaterialForScheduledRecovery()
    {
        using var database = new WorkflowDatabase();
        var outbox = new StubOutboxProcessor { Result = false };
        var workflow = database.CreateWorkflow(new StubFileStore(), outbox);

        var material = await workflow.ExecuteAsync(
            database.Operation("command-1"), TestContext.Current.CancellationToken);

        Assert.Equal("pending", material.ExtractStatus);
        Assert.Equal("pending", material.IndexStatus);
        Assert.Equal(SaveLifeMaterialWorkStatuses.Pending, Assert.Single(database.Outbox()).Status);
        Assert.Single(outbox.OutboxIds);
    }

    [Fact]
    public async Task ExecuteAsync_WhenImmediateDispatchThrows_ReturnsCommittedMaterialWithoutDeletingFile()
    {
        using var database = new WorkflowDatabase();
        var files = new StubFileStore();
        var outbox = new StubOutboxProcessor { ThrowOnProcess = true };
        var workflow = database.CreateWorkflow(files, outbox);

        var material = await workflow.ExecuteAsync(
            database.Operation("command-1"), TestContext.Current.CancellationToken);

        Assert.Equal("pending", material.ExtractStatus);
        Assert.Equal("pending", material.IndexStatus);
        Assert.Empty(files.DeletedFileIds);
        Assert.Single(database.Materials());
        Assert.Single(database.Commands());
        Assert.Equal(SaveLifeMaterialWorkStatuses.Pending, Assert.Single(database.Outbox()).Status);
        Assert.Single(outbox.OutboxIds);
    }

    private sealed class WorkflowDatabase : IDisposable
    {
        private readonly string _path;
        private readonly SqlSugarClient _db;

        public WorkflowDatabase()
        {
            _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
            _db = new SqlSugarClient(new ConnectionConfig
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
            _db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifeEnrollmentRecord));
            SaveLifeMaterialUploadMigration.Apply(_db);
            _db.Insertable(new SaveLifePatientProfile
            {
                Id = 501,
                TenantId = TenantId,
                OwnerExternalUserId = "account-a",
                AccountPatientKey = "self:account-a",
                RelationshipToAccount = SaveLifePatientRelationships.Self,
                RelationshipDescription = "本人",
                DisplayName = "本人",
                CreateTime = DateTime.Now
            }).ExecuteCommand();
            _db.Insertable(new SaveLifeEnrollmentRecord
            {
                Id = 3001,
                TenantId = TenantId,
                ExternalUserId = "account-a",
                PatientProfileId = 501,
                ProjectId = 901,
                Status = "pending",
                CreateTime = DateTime.Now
            }).ExecuteCommand();
        }

        public SaveLifeReliableMaterialUploadCoordinator CreateWorkflow(
            ISaveLifeMaterialFileStore files,
            ISaveLifeMaterialWorkOutboxProcessor outbox)
        {
            long nextId = 10_000;
            return new SaveLifeReliableMaterialUploadCoordinator(
                SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(_db),
                files,
                outbox,
                NullLogger<SaveLifeReliableMaterialUploadCoordinator>.Instance,
                () => Interlocked.Increment(ref nextId));
        }

        public SaveLifeReliableMaterialUploadOperation Operation(string commandId)
        {
            var bytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37, 0x0A };
            var stream = new MemoryStream(bytes);
            var file = new FormFile(stream, 0, bytes.Length, "file", "report.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };
            return new SaveLifeReliableMaterialUploadOperation(
                TenantId,
                "account-a",
                commandId,
                PatientProfileId: 501,
                EnrollmentRecordId: 3001,
                ProjectId: 901,
                UserProfileId: null,
                file,
                MaterialType: "diagnosis_report",
                Remark: null,
                FileType: "save-life-enrollment-material",
                DataId: 3001);
        }

        public void FailMaterialInsert()
        {
            _db.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_material_insert BEFORE INSERT ON sl_enrollment_material
                BEGIN SELECT RAISE(ABORT, 'forced material insert failure'); END;
                """);
        }

        public List<SaveLifeEnrollmentMaterial> Materials() =>
            _db.Queryable<SaveLifeEnrollmentMaterial>().ClearFilter().ToList();

        public List<SaveLifeMaterialUploadCommand> Commands() =>
            _db.Queryable<SaveLifeMaterialUploadCommand>().ClearFilter().ToList();

        public List<SaveLifeMaterialWorkOutbox> Outbox() =>
            _db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().ToList();

        public void Dispose()
        {
            _db.Dispose();
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }

    private sealed class StubFileStore : ISaveLifeMaterialFileStore
    {
        public bool ThrowOnUpload { get; set; }

        public bool ThrowOnDelete { get; set; }

        public int UploadCount { get; private set; }

        public List<long> DeletedFileIds { get; } = [];

        public Task<SysFile> UploadAsync(UploadFileInput input, string targetPath)
        {
            UploadCount++;
            if (ThrowOnUpload)
                throw new InvalidOperationException("file store unavailable");
            return Task.FromResult(new SysFile
            {
                Id = 9001,
                FileName = "report",
                Suffix = ".pdf",
                SizeKb = 1,
                SizeInfo = "1KB"
            });
        }

        public Task DeleteAsync(long fileId)
        {
            DeletedFileIds.Add(fileId);
            return ThrowOnDelete
                ? Task.FromException(new InvalidOperationException("delete unavailable"))
                : Task.CompletedTask;
        }
    }

    private sealed class StubOutboxProcessor : ISaveLifeMaterialWorkOutboxProcessor
    {
        public bool Result { get; set; } = true;

        public bool ThrowOnProcess { get; set; }

        public List<long> OutboxIds { get; } = [];

        public Task<SaveLifeMaterialOutboxBatchResult> ProcessPendingAsync(
            int limit,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new SaveLifeMaterialOutboxBatchResult());

        public Task<bool> ProcessOneAsync(long outboxId, CancellationToken cancellationToken = default)
        {
            OutboxIds.Add(outboxId);
            if (ThrowOnProcess)
                throw new InvalidOperationException("dispatcher unavailable");
            return Task.FromResult(Result);
        }
    }
}
