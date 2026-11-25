using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMaterialUploadCommandTests
{
    private const long TenantId = 1300000000001;
    private static readonly DateTime Now = new(2026, 7, 16, 18, 0, 0);

    [Fact]
    public void UploadContract_RequiresStableClientCommandId()
    {
        var property = typeof(UploadSaveLifeMiniAppEnrollmentMaterialInput)
            .GetProperty(nameof(UploadSaveLifeMiniAppEnrollmentMaterialInput.ClientCommandId));

        Assert.NotNull(property);
        Assert.NotNull(property!.GetCustomAttribute<RequiredAttribute>());
    }

    [Theory]
    [InlineData(nameof(UploadSaveLifeMiniAppEnrollmentMaterialInput.ClientCommandId), 128)]
    [InlineData(nameof(UploadSaveLifeMiniAppEnrollmentMaterialInput.MaterialType), 64)]
    [InlineData(nameof(UploadSaveLifeMiniAppEnrollmentMaterialInput.Remark), 512)]
    [InlineData(nameof(UploadSaveLifeMiniAppEnrollmentMaterialInput.OriginalFileName), 255)]
    [InlineData(nameof(UploadSaveLifeMiniAppEnrollmentMaterialInput.PatientProfileId), 32)]
    public void UploadContract_BoundsEveryFormTextField(string propertyName, int maximumLength)
    {
        var property = typeof(UploadSaveLifeMiniAppEnrollmentMaterialInput).GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(maximumLength, property!.GetCustomAttribute<MaxLengthAttribute>()?.Length);
    }

    [Fact]
    public void PayloadHash_IsStableAndCoversEveryWriteAffectingField()
    {
        var payload = Payload();
        var first = SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload);

        Assert.Equal("sha256-v1", first.Algorithm);
        Assert.Matches("^[0-9A-F]{64}$", first.Value);
        Assert.Equal(first, SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload));
        Assert.Equal(9, new[]
        {
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { PatientProfileId = 502 }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { EnrollmentRecordId = 3002 }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { ContentSha256 = new string('B', 64) }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { FileName = "另一份报告.pdf" }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { FileSuffix = ".docx" }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { ContentType = "application/msword" }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { MaterialType = "lab_report" }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { Remark = "另一备注" }),
            SaveLifeMaterialUploadCommandPolicy.ComputePayloadHash(payload with { ActualSizeBytes = 2048 })
        }.Distinct().Count());
    }

    [Fact]
    public async Task ComputeContentFingerprintAsync_ReadsActualBytesAndReturnsSha256()
    {
        var content = Encoding.UTF8.GetBytes("synthetic material content");
        await using var stream = new MemoryStream(content);
        var file = new FormFile(stream, 0, content.Length, "file", "report.pdf");

        var result = await SaveLifeMaterialUploadCommandPolicy.ComputeContentFingerprintAsync(
            file,
            TestContext.Current.CancellationToken);

        Assert.Equal(content.Length, result.ActualSizeBytes);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(content)), result.ContentSha256);
    }

    [Fact]
    public async Task ComputeContentFingerprintAsync_WhenDeclaredLengthDiffersFromStream_RejectsUpload()
    {
        var content = Encoding.UTF8.GetBytes("short");
        await using var stream = new MemoryStream(content);
        var file = new FormFile(stream, 0, content.Length + 1, "file", "report.pdf");

        await Assert.ThrowsAnyAsync<Exception>(() =>
            SaveLifeMaterialUploadCommandPolicy.ComputeContentFingerprintAsync(
                file,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ComputeContentFingerprintAsync_WithExactlyTwentyMegabytes_AcceptsUpload()
    {
        var content = new byte[SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes];
        await using var stream = new MemoryStream(content, writable: false);
        var file = new FormFile(stream, 0, content.Length, "file", "scan.png");

        var result = await SaveLifeMaterialUploadCommandPolicy.ComputeContentFingerprintAsync(
            file,
            TestContext.Current.CancellationToken);

        Assert.Equal(SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes, result.ActualSizeBytes);
    }

    [Fact]
    public async Task ComputeContentFingerprintAsync_WithTwentyMegabytesPlusOne_RejectsBeforeRead()
    {
        await using var stream = new MemoryStream([1], writable: false);
        var file = new FormFile(
            stream,
            0,
            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes + 1,
            "file",
            "scan.png");

        await Assert.ThrowsAnyAsync<Exception>(() =>
            SaveLifeMaterialUploadCommandPolicy.ComputeContentFingerprintAsync(
                file,
                TestContext.Current.CancellationToken));

        Assert.Equal(0, stream.Position);
    }

    [Fact]
    public void Migration_Repeatedly_CreatesPermanentCommandAndActiveSlotUniqueness()
    {
        WithDatabase(db =>
        {
            SaveLifeMaterialUploadMigration.Apply(db);
            var legacy = Material(10, fileId: 100, slot: 10);
            legacy.ProcessingRevision = 0;
            db.Insertable(legacy).ExecuteCommand();
            SaveLifeMaterialUploadMigration.Apply(db);
            Assert.Equal(1, db.Queryable<SaveLifeEnrollmentMaterial>()
                .ClearFilter()
                .Single(item => item.Id == legacy.Id)
                .ProcessingRevision);
            db.Insertable(Command(1, "command-1")).ExecuteCommand();
            Assert.ThrowsAny<Exception>(() =>
                db.Insertable(Command(2, "command-1")).ExecuteCommand());

            var deleted = Command(3, "command-deleted");
            deleted.IsDelete = true;
            db.Insertable(deleted).ExecuteCommand();
            Assert.ThrowsAny<Exception>(() =>
                db.Insertable(Command(4, "command-deleted")).ExecuteCommand());

            db.Insertable(Material(11, fileId: 101, slot: 1)).ExecuteCommand();
            Assert.ThrowsAny<Exception>(() =>
                db.Insertable(Material(12, fileId: 102, slot: 1)).ExecuteCommand());
            var otherScope = Material(13, fileId: 103, slot: 1);
            otherScope.EnrollmentRecordId = 3002;
            db.Insertable(otherScope).ExecuteCommand();
        });
    }

    [Fact]
    public void Migration_WithMoreThanTenLegacyMaterials_BackfillsSlots()
    {
        WithDatabase(db =>
        {
            db.CodeFirst.InitTables(typeof(SaveLifeEnrollmentMaterial));
            db.Insertable(Enumerable.Range(1, 12)
                .Select(index =>
                {
                    var material = Material(index, 100 + index, index <= 2 ? index : 0);
                    material.UploadSlot = index <= 2 ? index : null;
                    return material;
                })
                .ToList()).ExecuteCommand();

            SaveLifeMaterialUploadMigration.Apply(db);

            var slots = db.Queryable<SaveLifeEnrollmentMaterial>()
                .ClearFilter()
                .ToList()
                .Select(item => item.UploadSlot!.Value)
                .OrderBy(value => value);
            Assert.Equal(Enumerable.Range(1, 12), slots);
        });
    }

    [Fact]
    public async Task CommitAsync_WithTwentyConcurrentIdenticalCommands_ReplaysOneMaterial()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using (var setup = CreateDatabase(dbPath, initialize: true))
                EnableConcurrency(setup);
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(1, 20).Select(async sequence =>
            {
                using var db = CreateDatabase(dbPath, initialize: false);
                EnableConcurrency(db);
                await start.Task;
                var nextId = 10_000L + sequence * 10;
                return await SaveLifeMaterialUploadPersistence.CommitAsync(
                    db,
                    Request("same-command", fileId: 1000 + sequence),
                    Now,
                    () => Interlocked.Increment(ref nextId),
                    TestContext.Current.CancellationToken);
            }).ToArray();

            start.SetResult();
            var results = await Task.WhenAll(tasks);

            Assert.Single(results.Select(item => item.Material.Id).Distinct());
            using var verify = CreateDatabase(dbPath, initialize: false);
            Assert.Single(verify.Queryable<SaveLifeEnrollmentMaterial>().ClearFilter().ToList());
            Assert.Single(verify.Queryable<SaveLifeMaterialUploadCommand>().ClearFilter().ToList());
            Assert.Single(verify.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().ToList());
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task CommitAsync_WithConcurrentUploadsBeyondTen_AssignsAdditionalSlots()
    {
        var dbPath = NewDatabasePath();
        try
        {
            using (var setup = CreateDatabase(dbPath, initialize: true))
            {
                setup.Insertable(Enumerable.Range(1, 9)
                    .Select(index => Material(index, 100 + index, index))
                    .ToList()).ExecuteCommand();
                EnableConcurrency(setup);
            }
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var tasks = Enumerable.Range(1, 20).Select(async sequence =>
            {
                using var db = CreateDatabase(dbPath, initialize: false);
                EnableConcurrency(db);
                await start.Task;
                var nextId = 20_000L + sequence * 10;
                return await SaveLifeMaterialUploadPersistence.CommitAsync(
                    db,
                    Request($"command-{sequence}", 2000 + sequence, contentSeed: (char)('A' + sequence)),
                    Now,
                    () => Interlocked.Increment(ref nextId),
                    TestContext.Current.CancellationToken);
            }).ToArray();

            start.SetResult();
            var results = await Task.WhenAll(tasks);

            Assert.Equal(20, results.Select(item => item.Material.Id).Distinct().Count());
            using var verify = CreateDatabase(dbPath, initialize: false);
            var materials = verify.Queryable<SaveLifeEnrollmentMaterial>().ClearFilter().ToList();
            Assert.Equal(29, materials.Count);
            Assert.Equal(29, materials.Select(item => item.UploadSlot).Distinct().Count());
            Assert.Equal(
                Enumerable.Range(1, 29),
                materials.Select(item => item.UploadSlot!.Value).OrderBy(value => value));
            Assert.Equal(20, verify.Queryable<SaveLifeMaterialUploadCommand>().ClearFilter().ToList().Count);
            Assert.Equal(20, verify.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().ToList().Count);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Theory]
    [InlineData("command")]
    [InlineData("material")]
    [InlineData("outbox")]
    [InlineData("command_result")]
    public async Task CommitAsync_WhenDatabaseWriteFails_RollsBackEveryRow(string boundary)
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateDatabase(dbPath, initialize: true);
            InstallFailure(db, boundary);

            await Assert.ThrowsAnyAsync<Exception>(() => SaveLifeMaterialUploadPersistence.CommitAsync(
                db,
                Request("failed-command", 9001),
                Now,
                () => Random.Shared.NextInt64(30_000, 40_000),
                TestContext.Current.CancellationToken));

            Assert.Empty(db.Queryable<SaveLifeEnrollmentMaterial>().ClearFilter().ToList());
            Assert.Empty(db.Queryable<SaveLifeMaterialUploadCommand>().ClearFilter().ToList());
            Assert.Empty(db.Queryable<SaveLifeMaterialWorkOutbox>().ClearFilter().ToList());
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    private static SaveLifeMaterialUploadPayload Payload()
    {
        return new SaveLifeMaterialUploadPayload(
            PatientProfileId: 501,
            EnrollmentRecordId: 3001,
            ContentSha256: new string('A', 64),
            ActualSizeBytes: 1024,
            FileName: "检查报告.pdf",
            FileSuffix: ".pdf",
            ContentType: "application/pdf",
            MaterialType: "diagnosis_report",
            Remark: "初次上传");
    }

    private static SaveLifeMaterialUploadRequest Request(
        string clientCommandId,
        long fileId,
        char contentSeed = 'A')
    {
        var payload = Payload() with { ContentSha256 = new string(contentSeed, 64) };
        return new SaveLifeMaterialUploadRequest(
            TenantId,
            "account-a",
            clientCommandId,
            payload,
            new SaveLifeEnrollmentMaterial
            {
                TenantId = TenantId,
                PatientProfileId = payload.PatientProfileId,
                EnrollmentRecordId = payload.EnrollmentRecordId,
                ExternalUserId = "account-a",
                ProjectId = 901,
                FileId = fileId,
                MaterialType = payload.MaterialType,
                FileName = payload.FileName,
                FileSuffix = payload.FileSuffix,
                ContentType = payload.ContentType,
                SizeKb = 1,
                SizeInfo = "1KB",
                Status = "submitted",
                ExtractStatus = "pending",
                IndexStatus = "pending",
                Remark = payload.Remark,
                ContentSha256 = payload.ContentSha256
            });
    }

    private static SaveLifeMaterialUploadCommand Command(long id, string clientCommandId)
    {
        return new SaveLifeMaterialUploadCommand
        {
            Id = id,
            TenantId = TenantId,
            ExternalUserId = "account-a",
            PatientProfileId = 501,
            EnrollmentRecordId = 3001,
            ClientCommandId = clientCommandId,
            PayloadHashAlgorithm = "sha256-v1",
            PayloadHash = new string('A', 64),
            ContentSha256 = new string('A', 64),
            Status = "completed",
            MaterialId = 11,
            ResultJson = "{}",
            CreateTime = Now
        };
    }

    private static SaveLifeEnrollmentMaterial Material(long id, long fileId, int slot)
    {
        return new SaveLifeEnrollmentMaterial
        {
            Id = id,
            TenantId = TenantId,
            PatientProfileId = 501,
            EnrollmentRecordId = 3001,
            ExternalUserId = "account-a",
            ProjectId = 901,
            FileId = fileId,
            MaterialType = "diagnosis_report",
            FileName = $"报告-{id}",
            ContentSha256 = new string('A', 64),
            UploadSlot = slot,
            Status = "submitted",
            ExtractStatus = "pending",
            IndexStatus = "pending",
            CreateTime = Now
        };
    }

    private static void InstallFailure(ISqlSugarClient db, string boundary)
    {
        var sql = boundary switch
        {
            "command" => """
                CREATE TRIGGER fail_upload BEFORE INSERT ON sl_material_upload_command
                BEGIN SELECT RAISE(ABORT, 'forced command insert failure'); END;
                """,
            "material" => """
                CREATE TRIGGER fail_upload BEFORE INSERT ON sl_enrollment_material
                BEGIN SELECT RAISE(ABORT, 'forced material failure'); END;
                """,
            "outbox" => """
                CREATE TRIGGER fail_upload BEFORE INSERT ON sl_material_work_outbox
                BEGIN SELECT RAISE(ABORT, 'forced outbox failure'); END;
                """,
            "command_result" => """
                CREATE TRIGGER fail_upload BEFORE UPDATE OF Status ON sl_material_upload_command
                WHEN NEW.Status = 'completed' BEGIN SELECT RAISE(ABORT, 'forced command failure'); END;
                """,
            _ => throw new ArgumentOutOfRangeException(nameof(boundary))
        };
        db.Ado.ExecuteCommand(sql);
    }

    private static void WithDatabase(Action<ISqlSugarClient> test)
    {
        var dbPath = NewDatabasePath();
        try
        {
            using var db = CreateDatabase(dbPath, initialize: false);
            test(db);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    private static SqlSugarClient CreateDatabase(string dbPath, bool initialize)
    {
        var db = new SqlSugarClient(new ConnectionConfig
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
        if (initialize)
        {
            db.CodeFirst.InitTables(
                typeof(SaveLifePatientProfile),
                typeof(SaveLifeEnrollmentRecord));
            SaveLifeMaterialUploadMigration.Apply(db);
            db.Insertable(new SaveLifePatientProfile
            {
                Id = 501,
                TenantId = TenantId,
                OwnerExternalUserId = "account-a",
                AccountPatientKey = "self:account-a",
                RelationshipToAccount = SaveLifePatientRelationships.Self,
                RelationshipDescription = "本人",
                DisplayName = "本人",
                CreateTime = Now
            }).ExecuteCommand();
            db.Insertable(new SaveLifeEnrollmentRecord
            {
                Id = 3001,
                TenantId = TenantId,
                ExternalUserId = "account-a",
                PatientProfileId = 501,
                ProjectId = 901,
                Status = "pending",
                CreateTime = Now
            }).ExecuteCommand();
        }
        return db;
    }

    private static void EnableConcurrency(ISqlSugarClient db)
    {
        db.Ado.ExecuteCommand("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=30000;");
    }

    private static string NewDatabasePath() =>
        Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

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
