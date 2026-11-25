using System.Linq.Expressions;
using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeMaterialPrivateKnowledgeBaseIndexJobTests
{
    [Fact]
    public void ResolveMissingEmbeddingConfigurationError_WithImageMaterialMissingMultimodalConfig_ReturnsMaterialError()
    {
        var options = new SaveLifeAgentOptions
        {
            Enabled = true,
            EmbeddingApiKey = "text-key",
            MaterialEmbeddingApiKey = null,
        };

        var error = ResolveMissingEmbeddingConfigurationError(options);

        Assert.Equal(SaveLifeEnrollmentMaterialHelper.MissingMaterialEmbeddingConfigurationMessage, error);
    }

    [Fact]
    public void ResolveMissingEmbeddingConfigurationError_WithDocumentMissingTextConfig_ReturnsNull()
    {
        var options = new SaveLifeAgentOptions
        {
            Enabled = true,
            EmbeddingApiKey = null,
            MaterialEmbeddingApiKey = "vision-key",
        };

        var error = ResolveMissingEmbeddingConfigurationError(options);

        Assert.Null(error);
    }

    [Fact]
    public void ResolveMissingEmbeddingConfigurationError_WithDocumentMissingMultimodalConfig_ReturnsMaterialError()
    {
        var options = new SaveLifeAgentOptions
        {
            Enabled = true,
            EmbeddingApiKey = "text-key",
            MaterialEmbeddingApiKey = null,
        };

        var error = ResolveMissingEmbeddingConfigurationError(options);

        Assert.Equal(SaveLifeEnrollmentMaterialHelper.MissingMaterialEmbeddingConfigurationMessage, error);
    }

    [Fact]
    public void ResolveMissingEmbeddingConfigurationError_WithMatchingConfig_ReturnsNull()
    {
        var options = new SaveLifeAgentOptions
        {
            Enabled = true,
            EmbeddingApiKey = "text-key",
            MaterialEmbeddingApiKey = "vision-key",
        };

        Assert.Null(ResolveMissingEmbeddingConfigurationError(options));
    }

    [Theory]
    [InlineData("报名材料缺少用户归属，不能进入私有知识库")]
    [InlineData("PDF 已加密或受密码保护，无法后台解析文本层")]
    [InlineData("PDF 文件结构异常，无法解析文本层")]
    [InlineData("PDF 页数超过安全解析限制")]
    [InlineData("PDF 字形数量超过安全解析限制")]
    [InlineData("旧版 Word 文档结构异常，无法后台解析文本")]
    [InlineData("病例资料文件超过多模态解析限制")]
    [InlineData("病例文档解析资源超限，无法安全处理")]
    public void IsPermanentFailure_WithNonRetryableMaterialErrors_ReturnsTrue(string error)
    {
        Assert.True(SaveLifeMaterialProcessingFailurePolicy.IsPermanentFailure(error));
    }

    [Theory]
    [InlineData("未配置病例资料文本向量模型")]
    [InlineData("Embedding API Key 未配置")]
    [InlineData("timeout")]
    [InlineData("病例资料多模态向量调用失败：502")]
    public void IsPermanentFailure_WithConfigOrTransientErrors_ReturnsFalse(string error)
    {
        Assert.False(SaveLifeMaterialProcessingFailurePolicy.IsPermanentFailure(error));
    }

    [Fact]
    public async Task MaterialProcessablePredicate_SelectAndClaim_ExecutesOnSqlite()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            CreateEnrollmentMaterialTable(db);

            var now = new DateTime(2026, 7, 8, 10, 0, 0);
            InsertMaterial(
                db,
                1,
                extractStatus: "pending",
                indexStatus: "synced",
                indexError: null,
                now.AddMinutes(-10),
                now.AddMinutes(-9),
                nextRetryTime: null);
            InsertMaterial(
                db,
                2,
                extractStatus: "failed",
                indexStatus: "synced",
                indexError: null,
                now.AddMinutes(-8),
                now.AddMinutes(-7),
                nextRetryTime: now.AddMinutes(-1));
            InsertMaterial(
                db,
                3,
                extractStatus: "failed",
                indexStatus: "synced",
                indexError: null,
                now.AddMinutes(-6),
                now.AddMinutes(-5),
                nextRetryTime: now.AddMinutes(30));
            InsertMaterial(
                db,
                4,
                extractStatus: "synced",
                indexStatus: "skipped",
                indexError: SaveLifeEnrollmentMaterialHelper.MissingTextEmbeddingConfigurationMessage,
                now.AddMinutes(-4),
                now.AddMinutes(-3),
                nextRetryTime: null);
            InsertMaterial(
                db,
                5,
                extractStatus: "processing",
                indexStatus: "synced",
                indexError: null,
                now.AddHours(-3),
                now.AddHours(-3),
                nextRetryTime: null,
                processingLeaseOwner: "expired-worker",
                processingLeaseExpiresTime: now.AddMinutes(-1));
            InsertMaterial(
                db,
                6,
                extractStatus: "processing",
                indexStatus: "synced",
                indexError: null,
                now.AddMinutes(-2),
                now.AddMinutes(-1),
                nextRetryTime: null,
                processingLeaseOwner: "active-worker",
                processingLeaseExpiresTime: now.AddMinutes(5));

            var predicate = BuildMaterialPredicate(now);

            var ids = await db.Queryable<SaveLifeEnrollmentMaterial>()
                .Where(predicate)
                .OrderBy(u => u.UpdateTime, OrderByType.Asc)
                .OrderBy(u => u.CreateTime, OrderByType.Asc)
                .Select(u => u.Id)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(new long[] { 5, 1, 2, 4 }, ids);

            var affected = await db.Updateable<SaveLifeEnrollmentMaterial>()
                .SetColumns(u => new SaveLifeEnrollmentMaterial
                {
                    ExtractStatus = "processing",
                    ExtractError = null,
                    IndexStatus = "pending",
                    IndexError = null,
                    NextProcessRetryTime = null,
                    UpdateTime = now
                })
                .Where(u => u.Id == 1)
                .Where(predicate)
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);

            Assert.Equal(1, affected);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task OutdatedMaterialIndexPredicate_SelectsTextKindAndPreviousMaterialModel()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SaveLifeAgentVectorIndex>();
            db.Insertable(new[]
            {
                VectorIndex(1, "material", "text", "BAAI/bge-m3", "doubao-embedding-vision", 1024),
                VectorIndex(2, "material", "multimodal", "provider-revision-old", "doubao-embedding-vision-old", 1024),
                VectorIndex(3, "material", "multimodal", "provider-revision-new", "doubao-embedding-vision", 1024),
                VectorIndex(4, "knowledge", "text", "BAAI/bge-m3", "BAAI/bge-m3", 1024),
                VectorIndex(5, "material", "multimodal", "provider-revision-new", "doubao-embedding-vision", null),
                VectorIndex(6, "material", "multimodal", "provider-revision-new", "doubao-embedding-vision", 512)
            }).ExecuteCommand();

            var ids = await db.Queryable<SaveLifeAgentVectorIndex>()
                .Where(BuildOutdatedMaterialIndexPredicate("doubao-embedding-vision", 1024))
                .OrderBy(u => u.Id)
                .Select(u => u.Id)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(new long[] { 1, 2, 5, 6 }, ids);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task RequeueOutdatedAsync_WithLegacyDimensionsAndChangedContent_RequeuesOnlyOutdatedMaterials()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SaveLifeEnrollmentMaterial, SaveLifeAgentVectorIndex>();
            db.Insertable(new[]
            {
                VersionedMaterial(101, 'A'),
                VersionedMaterial(102, 'B'),
                VersionedMaterial(103, 'C')
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                VersionedIndex(201, 101, 'A', 1024),
                VersionedIndex(202, 102, 'X', 1024),
                VersionedIndex(203, 103, 'C', null)
            }).ExecuteCommand();

            var requeued = await SaveLifeMaterialIndexVersionPersistence.RequeueOutdatedAsync(
                db,
                "doubao-embedding-vision",
                1024,
                100,
                new DateTime(2026, 7, 16, 21, 0, 0),
                TestContext.Current.CancellationToken);

            Assert.Equal(2, requeued);
            var materials = db.Queryable<SaveLifeEnrollmentMaterial>().OrderBy(item => item.Id).ToList();
            Assert.Equal("synced", materials[0].IndexStatus);
            Assert.Equal(1, materials[0].ProcessingRevision);
            Assert.Equal("pending", materials[1].IndexStatus);
            Assert.Equal(2, materials[1].ProcessingRevision);
            Assert.Equal("pending", materials[2].IndexStatus);
            Assert.Equal(2, materials[2].ProcessingRevision);
            var indexes = db.Queryable<SaveLifeAgentVectorIndex>().OrderBy(item => item.Id).ToList();
            Assert.Equal("synced", indexes[0].SyncStatus);
            Assert.Equal("pending", indexes[1].SyncStatus);
            Assert.Equal("pending", indexes[2].SyncStatus);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task RequeueOutdatedAsync_WhenIndexUpdateFails_RollsBackMaterialRevisionAndStatus()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables<SaveLifeEnrollmentMaterial, SaveLifeAgentVectorIndex>();
            db.Insertable(VersionedMaterial(101, 'A')).ExecuteCommand();
            db.Insertable(VersionedIndex(201, 101, 'X', 1024)).ExecuteCommand();
            db.Ado.ExecuteCommand("""
                CREATE TRIGGER fail_index_requeue BEFORE UPDATE OF SyncStatus ON sl_agent_vector_index
                WHEN NEW.SyncStatus = 'pending'
                BEGIN SELECT RAISE(ABORT, 'forced index requeue failure'); END;
                """);

            await Assert.ThrowsAnyAsync<Exception>(() =>
                SaveLifeMaterialIndexVersionPersistence.RequeueOutdatedAsync(
                    db,
                    "doubao-embedding-vision",
                    1024,
                    100,
                    new DateTime(2026, 7, 16, 21, 0, 0),
                    TestContext.Current.CancellationToken));

            var material = db.Queryable<SaveLifeEnrollmentMaterial>().Single();
            var index = db.Queryable<SaveLifeAgentVectorIndex>().Single();
            Assert.Equal("synced", material.IndexStatus);
            Assert.Equal(1, material.ProcessingRevision);
            Assert.Equal("synced", index.SyncStatus);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static string? ResolveMissingEmbeddingConfigurationError(SaveLifeAgentOptions options)
    {
        var method = typeof(SaveLifeMaterialIndexProcessor)
            .GetMethod("ResolveMissingEmbeddingConfigurationError", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (string?)method.Invoke(null, new object[] { options });
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath};Pooling=False",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
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

    private static void CreateEnrollmentMaterialTable(SqlSugarClient db)
    {
        db.Ado.ExecuteCommand("""
            CREATE TABLE sl_enrollment_material (
                Id bigint NOT NULL PRIMARY KEY,
                ExtractStatus varchar(32) NOT NULL,
                ExtractError varchar(512) NULL,
                IndexStatus varchar(32) NOT NULL,
                IndexError varchar(512) NULL,
                NextProcessRetryTime datetime NULL,
                ProcessingLeaseOwner varchar(64) NULL,
                ProcessingLeaseExpiresTime datetime NULL,
                CreateTime datetime NOT NULL,
                UpdateTime datetime NULL
            );
            """);
    }

    private static void InsertMaterial(
        SqlSugarClient db,
        long id,
        string extractStatus,
        string indexStatus,
        string? indexError,
        DateTime createTime,
        DateTime updateTime,
        DateTime? nextRetryTime,
        string? processingLeaseOwner = null,
        DateTime? processingLeaseExpiresTime = null)
    {
        db.Ado.ExecuteCommand(
            """
            INSERT INTO sl_enrollment_material
                (Id, ExtractStatus, IndexStatus, IndexError, NextProcessRetryTime,
                 ProcessingLeaseOwner, ProcessingLeaseExpiresTime, CreateTime, UpdateTime)
            VALUES
                (@Id, @ExtractStatus, @IndexStatus, @IndexError, @NextProcessRetryTime,
                 @ProcessingLeaseOwner, @ProcessingLeaseExpiresTime, @CreateTime, @UpdateTime);
            """,
            new SugarParameter("@Id", id),
            new SugarParameter("@ExtractStatus", extractStatus),
            new SugarParameter("@IndexStatus", indexStatus),
            new SugarParameter("@IndexError", indexError),
            new SugarParameter("@NextProcessRetryTime", nextRetryTime),
            new SugarParameter("@ProcessingLeaseOwner", processingLeaseOwner),
            new SugarParameter("@ProcessingLeaseExpiresTime", processingLeaseExpiresTime),
            new SugarParameter("@CreateTime", createTime),
            new SugarParameter("@UpdateTime", updateTime));
    }

    private static Expression<Func<SaveLifeEnrollmentMaterial, bool>> BuildMaterialPredicate(DateTime now)
    {
        var method = typeof(SaveLifeMaterialIndexProcessor)
            .GetMethod("BuildMaterialProcessablePredicate", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (Expression<Func<SaveLifeEnrollmentMaterial, bool>>)method.Invoke(null, new object[] { now })!;
    }

    private static Expression<Func<SaveLifeAgentVectorIndex, bool>> BuildOutdatedMaterialIndexPredicate(
        string materialEmbeddingModel,
        int materialEmbeddingDimensions)
    {
        var method = typeof(SaveLifeMaterialIndexProcessor)
            .GetMethod("BuildOutdatedMaterialIndexPredicate", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (Expression<Func<SaveLifeAgentVectorIndex, bool>>)method.Invoke(
            null,
            new object[] { materialEmbeddingModel, materialEmbeddingDimensions })!;
    }

    private static SaveLifeAgentVectorIndex VectorIndex(
        long id,
        string resourceType,
        string embeddingKind,
        string embeddingModel,
        string embeddingConfiguredModel,
        int? embeddingDimensions)
    {
        return new SaveLifeAgentVectorIndex
        {
            Id = id,
            ResourceType = resourceType,
            ResourceId = id,
            Title = $"index-{id}",
            EmbeddingKind = embeddingKind,
            EmbeddingModel = embeddingModel,
            EmbeddingConfiguredModel = embeddingConfiguredModel,
            EmbeddingDimensions = embeddingDimensions,
            SyncStatus = "synced",
            CreateTime = new DateTime(2026, 7, 15)
        };
    }

    private static SaveLifeEnrollmentMaterial VersionedMaterial(long id, char contentSeed)
    {
        return new SaveLifeEnrollmentMaterial
        {
            Id = id,
            TenantId = 1300000000001,
            ExternalUserId = "account-a",
            PatientProfileId = 501,
            EnrollmentRecordId = 0,
            FileId = id + 1000,
            ContentSha256 = new string(contentSeed, 64),
            MaterialType = "diagnosis_report",
            FileName = $"material-{id}",
            ExtractStatus = "synced",
            IndexStatus = "synced",
            ProcessingRevision = 1,
            CreateTime = new DateTime(2026, 7, 16)
        };
    }

    private static SaveLifeAgentVectorIndex VersionedIndex(
        long id,
        long materialId,
        char contentSeed,
        int? dimensions)
    {
        return new SaveLifeAgentVectorIndex
        {
            Id = id,
            TenantId = 1300000000001,
            ResourceType = "material",
            ResourceId = materialId,
            ExternalUserId = "account-a",
            PatientProfileId = 501,
            Title = $"index-{id}",
            EmbeddingKind = "multimodal",
            EmbeddingModel = "provider-revision",
            EmbeddingConfiguredModel = "doubao-embedding-vision",
            EmbeddingDimensions = dimensions,
            SourceContentSha256 = new string(contentSeed, 64),
            SyncStatus = "synced",
            CreateTime = new DateTime(2026, 7, 16)
        };
    }
}
