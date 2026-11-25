using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public sealed class SaveLifeMaterialLiveEndToEndTests
{
    private const string EnabledVariable = "SAVELIFE_LIVE_MULTIMODAL_E2E";
    private const string ConfigDatabaseVariable = "SAVELIFE_LIVE_CONFIG_DB";
    private const string CaseImageVariable = "SAVELIFE_CASE_IMAGE";
    private const string ReportPathVariable = "SAVELIFE_LIVE_REPORT_PATH";
    private const string Owner = "live-e2e-account";
    private const long CurrentPatientId = 501;
    private const long OtherPatientId = 502;
    private const long TargetMaterialId = 1001;
    private const long DistractorMaterialId = 1002;
    private const long OtherPatientMaterialId = 2001;

    private const string SyntheticRedPngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAdSURBVDhPY/jPwPCfEsyALkAqHjVg1IBRAwaLAQAwxP4Q7zYsrwAAAABJRU5ErkJggg==";

    [Fact]
    [Trait("Category", "Live")]
    public async Task AuthorizedCaseImage_CompletesIndexRetrievalIsolationAndDeletionFlow()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(EnabledVariable),
                "1",
                StringComparison.Ordinal))
        {
            Assert.Skip($"设置 {EnabledVariable}=1 后运行真实病例多模态端到端验证。");
        }

        var configDatabasePath = RequireFileEnvironmentVariable(ConfigDatabaseVariable);
        var caseImagePath = RequireFileEnvironmentVariable(CaseImageVariable);
        var originalConfigHash = await ComputeFileHashAsync(configDatabasePath);
        var caseImage = await ReadCaseImageAsync(caseImagePath);
        var distractorImage = Convert.FromBase64String(SyntheticRedPngBase64);

        try
        {
            using var configDb = CreateSqliteClient(configDatabasePath, readOnly: true);
            using var businessDb = CreateInMemorySqliteClient();
            using var httpClientFactory = new LiveHttpClientFactory();
            businessDb.CodeFirst.InitTables(
                typeof(SaveLifeAgentVectorIndex),
                typeof(SaveLifeEnrollmentMaterial));

            var provider = new SaveLifeAgentConfigProvider(
                SqlSugarRepository<SysConfig>.CreateForContext(configDb),
                new SaveLifeAgentOptions());
            var options = await provider.GetOptionsAsync();
            Assert.Equal(SaveLifeAgentOptions.DefaultChatModel, options.ChatModel);
            Assert.Equal(
                SaveLifeAgentOptions.DefaultMaterialEmbeddingModel,
                options.MaterialEmbeddingModel);
            Assert.True(options.HasChatConfiguration);
            Assert.True(options.HasMaterialEmbeddingConfiguration);

            var target = CreateImageMaterial(
                TargetMaterialId,
                CurrentPatientId,
                caseImage,
                "左脚踝扭伤诊断证明病例图片");
            var distractor = CreateImageMaterial(
                DistractorMaterialId,
                CurrentPatientId,
                distractorImage,
                "病例图片合成色块干扰材料");
            var otherPatientTarget = CreateImageMaterial(
                OtherPatientMaterialId,
                OtherPatientId,
                caseImage,
                "另一患者左脚踝扭伤诊断证明病例图片");
            businessDb.Insertable(new[] { target, distractor, otherPatientTarget }).ExecuteCommand();
            businessDb.Insertable(new[]
            {
                CreatePendingIndex(9001, target),
                CreatePendingIndex(9002, distractor),
                CreatePendingIndex(9003, otherPatientTarget)
            }).ExecuteCommand();

            var clientFactory = new SaveLifeAgentClientFactory(provider);
            var materialEmbedding = new SaveLifeMaterialEmbeddingService(
                clientFactory,
                httpClientFactory);
            var retrievalService = new SaveLifeAgentRetrievalService(
                SqlSugarRepository<SaveLifeAgentVectorIndex>.CreateForContext(businessDb),
                SqlSugarRepository<SaveLifeDiseaseKnowledge>.CreateForContext(businessDb),
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(businessDb),
                SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(businessDb),
                clientFactory,
                new SaveLifeAgentEmbeddingService(clientFactory),
                materialEmbedding);

            var indexStopwatch = Stopwatch.StartNew();
            await retrievalService.SyncMaterialIndexAsync(
                target,
                () => OpenBytesAsync(caseImage));
            await retrievalService.SyncMaterialIndexAsync(
                distractor,
                () => OpenBytesAsync(distractorImage));
            await retrievalService.SyncMaterialIndexAsync(
                otherPatientTarget,
                () => OpenBytesAsync(caseImage));
            indexStopwatch.Stop();

            const string question = "请解读左脚踝扭伤诊断证明病例图片中能看见的主要信息，并说明不确定性和下一步。";
            var retrievalStopwatch = Stopwatch.StartNew();
            var currentPatientRetrieval = await retrievalService.BuildRetrievalAsync(
                question,
                PatientSignal(CurrentPatientId),
                Owner,
                cancellationToken: TestContext.Current.CancellationToken,
                scope: SaveLifeAgentRetrievalScope.MaterialsOnly with
                {
                    HasMaterialIntent = true
                });
            retrievalStopwatch.Stop();

            Assert.NotEmpty(currentPatientRetrieval.Materials);
            Assert.True(
                currentPatientRetrieval.Materials[0].MaterialId == TargetMaterialId.ToString(),
                $"Expected the diagnosis image first, but received: {string.Join(", ", currentPatientRetrieval.Materials.Select(item => $"{item.MaterialId}={item.Score:0.0000}"))}");
            Assert.DoesNotContain(
                currentPatientRetrieval.Materials,
                item => item.MaterialId == OtherPatientMaterialId.ToString());
            var distractorPosition = currentPatientRetrieval.Materials.FindIndex(
                item => item.MaterialId == DistractorMaterialId.ToString());
            Assert.True(distractorPosition is -1 or > 0);

            var noPatientRetrieval = await retrievalService.BuildRetrievalAsync(
                question,
                new SaveLifeAgentPatientSignal(),
                Owner,
                cancellationToken: TestContext.Current.CancellationToken,
                scope: SaveLifeAgentRetrievalScope.MaterialsOnly with
                {
                    HasMaterialIntent = true
                });
            Assert.Empty(noPatientRetrieval.Materials);

            var otherPatientRetrieval = await retrievalService.BuildRetrievalAsync(
                question,
                PatientSignal(OtherPatientId),
                Owner,
                cancellationToken: TestContext.Current.CancellationToken,
                scope: SaveLifeAgentRetrievalScope.MaterialsOnly with
                {
                    HasMaterialIntent = true
                });
            Assert.NotEmpty(otherPatientRetrieval.Materials);
            Assert.All(
                otherPatientRetrieval.Materials,
                item => Assert.Equal(OtherPatientMaterialId.ToString(), item.MaterialId));

            target.IsDelete = true;
            businessDb.Updateable(target).ExecuteCommand();
            await retrievalService.DeleteMaterialIndexAsync(TargetMaterialId, Owner);
            var afterDeletion = await retrievalService.BuildRetrievalAsync(
                question,
                PatientSignal(CurrentPatientId),
                Owner,
                cancellationToken: TestContext.Current.CancellationToken,
                scope: SaveLifeAgentRetrievalScope.MaterialsOnly with
                {
                    HasMaterialIntent = true
                });
            Assert.DoesNotContain(
                afterDeletion.Materials,
                item => item.MaterialId == TargetMaterialId.ToString());

            await WriteReportAsync(
                options,
                indexStopwatch.ElapsedMilliseconds,
                retrievalStopwatch.ElapsedMilliseconds,
                currentPatientRetrieval.Materials.Count);
        }
        finally
        {
            Assert.Equal(
                originalConfigHash,
                await ComputeFileHashAsync(configDatabasePath));
        }
    }

    private static SaveLifeEnrollmentMaterial CreateImageMaterial(
        long id,
        long patientProfileId,
        byte[] content,
        string displayName) => new()
        {
            Id = id,
            ContentSha256 = Convert.ToHexString(SHA256.HashData(content)),
            ContentType = "image/png",
            CreateTime = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc),
            EnrollmentRecordId = 0,
            ExternalUserId = Owner,
            ExtractStatus = "synced",
            FileId = id,
            FileName = displayName,
            FileSuffix = ".png",
            IndexStatus = "synced",
            MaterialType = "diagnosis_report",
            PatientProfileId = patientProfileId,
            Remark = displayName,
            SizeBytes = content.LongLength,
            SizeKb = Math.Max(1, (content.LongLength + 1023) / 1024),
            Status = "submitted"
        };

    private static SaveLifeAgentVectorIndex CreatePendingIndex(
        long id,
        SaveLifeEnrollmentMaterial material) => new()
        {
            Id = id,
            CreateTime = new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc),
            ExternalUserId = material.ExternalUserId,
            PatientProfileId = material.PatientProfileId,
            ResourceId = material.Id,
            ResourceType = "material",
            SyncStatus = "pending",
            Title = material.FileName
        };

    private static SaveLifeAgentPatientSignal PatientSignal(long patientProfileId) => new()
    {
        CurrentPatient = new SaveLifePatientProfileOutput
        {
            Id = patientProfileId.ToString(),
            IsCurrent = true
        }
    };

    private static Task<Stream> OpenBytesAsync(byte[] content) =>
        Task.FromResult<Stream>(new MemoryStream(content, writable: false));

    private static async Task<byte[]> ReadCaseImageAsync(string path)
    {
        var info = new FileInfo(path);
        Assert.Equal(".png", info.Extension, ignoreCase: true);
        Assert.InRange(
            info.Length,
            1,
            SaveLifeEnrollmentMaterialHelper.MaxMaterialFileBytes);
        var bytes = await File.ReadAllBytesAsync(path);
        Assert.True(
            bytes.Length >= 8 &&
            bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "病例图片不是有效 PNG 签名。");
        return bytes;
    }

    private static SqlSugarClient CreateSqliteClient(string path, bool readOnly)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (property, column) =>
                {
                    if (new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable)
                        column.IsNullable = true;
                }
            },
            ConnectionString = readOnly
                ? $"Data Source={path};Mode=ReadOnly;Pooling=False"
                : $"Data Source={path};Pooling=False",
            DbType = DbType.Sqlite,
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true
        });
    }

    private static SqlSugarClient CreateInMemorySqliteClient()
    {
        var db = CreateSqliteClient(":memory:", readOnly: false);
        db.CurrentConnectionConfig.IsAutoCloseConnection = false;
        db.Ado.Open();
        return db;
    }

    private static string RequireFileEnvironmentVariable(string variable)
    {
        var value = Environment.GetEnvironmentVariable(variable)?.Trim();
        Assert.False(string.IsNullOrWhiteSpace(value), $"环境变量 {variable} 未设置。");
        var path = ResolveRepositoryPath(value!);
        Assert.True(File.Exists(path), $"环境变量 {variable} 指向的文件不存在。");
        return path;
    }

    private static string ResolveRepositoryPath(string path)
    {
        if (Path.IsPathFullyQualified(path))
            return Path.GetFullPath(path);

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "src", "Jero.slnx")))
                return Path.GetFullPath(Path.Combine(directory.FullName, path));
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("无法定位仓库根目录。");
    }

    private static async Task<string> ComputeFileHashAsync(string path)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 81920,
            useAsync: true);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream));
    }

    private static async Task WriteReportAsync(
        SaveLifeAgentOptions options,
        long indexMilliseconds,
        long retrievalMilliseconds,
        int selectedMaterialCount)
    {
        var reportPath = Environment.GetEnvironmentVariable(ReportPathVariable)?.Trim();
        Assert.False(string.IsNullOrWhiteSpace(reportPath), $"环境变量 {ReportPathVariable} 未设置。");
        var fullPath = ResolveRepositoryPath(reportPath!);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var report = new
        {
            Checks = new
            {
                CaseImageIndexed = true,
                CurrentPatientIsolation = true,
                DeletedMaterialExcluded = true,
                NoPatientReturnsNoPrivateMaterial = true,
                SamePatientDistractorDidNotOutrankTarget = true
            },
            GeneratedAtUtc = DateTime.UtcNow,
            Model = options.ChatModel,
            Passed = true,
            SelectedMaterialCount = selectedMaterialCount,
            Timings = new
            {
                IndexMilliseconds = indexMilliseconds,
                RetrievalMilliseconds = retrievalMilliseconds
            },
            VectorDimensions = options.MaterialEmbeddingDimensions,
            VectorModel = options.MaterialEmbeddingModel
        };
        await File.WriteAllTextAsync(
            fullPath,
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed class LiveHttpClientFactory : IHttpClientFactory, IDisposable
    {
        private readonly HttpClient _client = new()
        {
            Timeout = TimeSpan.FromSeconds(120)
        };

        public HttpClient CreateClient(string name) => _client;

        public void Dispose() => _client.Dispose();
    }
}
