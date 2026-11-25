using OpenAI.Chat;
using OpenAI.Embeddings;
using System.Reflection;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentRoutedRetrievalTests
{
    [Fact]
    public async Task BuildRetrievalAsync_KnowledgeOnly_QueriesKnowledgeAndReportsOtherSourcesNotRequested()
    {
        using var database = new RetrievalDatabase(
            typeof(SaveLifeAgentVectorIndex),
            typeof(SaveLifeDiseaseKnowledge),
            typeof(SaveLifeClinicalTrial),
            typeof(SaveLifeEnrollmentMaterial));
        database.Db.Insertable(new SaveLifeDiseaseKnowledge
        {
            Id = 101,
            Title = "肺癌治疗副作用",
            DiseaseName = "肺癌",
            Summary = "介绍常见副作用",
            Content = "肺癌治疗期间需要关注不良反应。",
            IsPublished = true,
            SortOrder = 10
        }).ExecuteCommand();
        database.Db.Insertable(new SaveLifeAgentVectorIndex
        {
            Id = 201,
            ResourceType = "knowledge",
            ResourceId = 101,
            Title = "肺癌治疗副作用",
            DiseaseName = "肺癌",
            Content = "肺癌治疗期间需要关注副作用。",
            EmbeddingJson = "[1,0]",
            SyncStatus = "synced",
            SortOrder = 10
        }).ExecuteCommand();
        database.Sql.Clear();
        var embedding = new RecordingEmbeddingService();
        var service = database.CreateService(embedding);
        const string latestMessage = "肺癌副作用怎么处理";

        var result = await service.BuildRetrievalAsync(
            latestMessage,
            new SaveLifeAgentPatientSignal { DiseaseKeyword = "肺癌", City = "成都" },
            "openid-a",
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.KnowledgeOnly);

        Assert.Single(result.Knowledge);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.Succeeded, result.Diagnostics.KnowledgeStatus);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NotRequested, result.Diagnostics.TrialStatus);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NotRequested, result.Diagnostics.MaterialStatus);
        Assert.Contains(database.Sql, sql => ContainsTable(sql, "sl_disease_knowledge"));
        Assert.DoesNotContain(database.Sql, sql => ContainsTable(sql, "sl_clinical_trial"));
        Assert.DoesNotContain(database.Sql, sql => ContainsTable(sql, "sl_enrollment_material"));
        var embeddingInput = Assert.Single(embedding.Inputs);
        Assert.Equal(1, CountOccurrences(embeddingInput, latestMessage));
        Assert.Equal(1, CountOccurrences(embeddingInput, "肺癌"));
        Assert.Equal(1, CountOccurrences(embeddingInput, "副作用"));
        Assert.Contains("成都", embeddingInput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task BuildRetrievalAsync_TrialsAndMaterials_QueriesOnlyAllowedSourcesAndReportsNoHits()
    {
        using var database = new RetrievalDatabase(
            typeof(SaveLifeAgentVectorIndex),
            typeof(SaveLifeDiseaseKnowledge),
            typeof(SaveLifeClinicalTrial),
            typeof(SaveLifeEnrollmentMaterial));
        database.Sql.Clear();
        var service = database.CreateService(new RecordingEmbeddingService());

        var result = await service.BuildRetrievalAsync(
            "肺癌报告能否用于试验报名",
            CompletePatientSignal(),
            "openid-a",
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.TrialsAndMaterials with { HasMaterialIntent = true });

        Assert.Empty(result.Trials);
        Assert.Empty(result.Materials);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NotRequested, result.Diagnostics.KnowledgeStatus);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NoHits, result.Diagnostics.TrialStatus);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NoHits, result.Diagnostics.MaterialStatus);
        Assert.DoesNotContain(database.Sql, sql => ContainsTable(sql, "sl_disease_knowledge"));
        Assert.Contains(database.Sql, sql => ContainsTable(sql, "sl_clinical_trial"));
        Assert.Contains(database.Sql, sql => ContainsTable(sql, "sl_enrollment_material"));
    }

    [Fact]
    public async Task BuildRetrievalAsync_IllnessHelpWithoutMaterialContext_DoesNotRequestMaterials()
    {
        using var database = new RetrievalDatabase(
            typeof(SaveLifeAgentVectorIndex),
            typeof(SaveLifeDiseaseKnowledge),
            typeof(SaveLifeClinicalTrial),
            typeof(SaveLifeEnrollmentMaterial));
        database.Sql.Clear();
        var service = database.CreateService(new RecordingEmbeddingService());

        var result = await service.BuildRetrievalAsync(
            "肺癌有哪些临床试验",
            CompletePatientSignal(),
            "openid-a",
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.TrialsAndMaterials);

        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NoHits, result.Diagnostics.TrialStatus);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NotRequested, result.Diagnostics.MaterialStatus);
        Assert.DoesNotContain(database.Sql, sql => ContainsTable(sql, "sl_enrollment_material"));
    }

    [Fact]
    public async Task BuildRetrievalAsync_WhenMaterialSourceFails_PreservesSourceDiagnosticsAndFailsClosed()
    {
        using var database = new RetrievalDatabase(
            typeof(SaveLifeAgentVectorIndex),
            typeof(SaveLifeClinicalTrial));
        database.Sql.Clear();
        var service = database.CreateService(new RecordingEmbeddingService());

        var error = await Assert.ThrowsAsync<SaveLifeAgentUnavailableException>(() =>
            service.BuildRetrievalAsync(
                "请检查我的肺癌报告是否够报名",
                CompletePatientSignal(),
                "openid-a",
                cancellationToken: TestContext.Current.CancellationToken,
                scope: SaveLifeAgentRetrievalScope.TrialsAndMaterials with { HasMaterialIntent = true }));

        Assert.Equal(SaveLifeAgentFailureCodes.RetrievalUnavailable, error.Code);
        Assert.Equal(SaveLifeAgentFailureStages.Retrieval, error.Stage);
        Assert.NotNull(error.RetrievalDiagnostics);
        Assert.Equal(
            SaveLifeAgentRetrievalSourceStatuses.NotRequested,
            error.RetrievalDiagnostics.KnowledgeStatus);
        Assert.Equal(
            SaveLifeAgentRetrievalSourceStatuses.NoHits,
            error.RetrievalDiagnostics.TrialStatus);
        Assert.Equal(
            SaveLifeAgentRetrievalSourceStatuses.Failed,
            error.RetrievalDiagnostics.MaterialStatus);
    }

    private static bool ContainsTable(string sql, string table)
    {
        return sql.Contains(table, StringComparison.OrdinalIgnoreCase);
    }

    private static SaveLifeAgentPatientSignal CompletePatientSignal()
    {
        return new SaveLifeAgentPatientSignal
        {
            DiseaseKeyword = "肺癌",
            CurrentPatient = new SaveLifePatientProfileOutput
            {
                Id = "501",
                Diagnosis = "肺癌",
                DiseaseStage = "晚期",
                TreatmentHistory = "已接受一线化疗"
            }
        };
    }

    private static int CountOccurrences(string value, string term)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(term, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += term.Length;
        }

        return count;
    }

    private sealed class RetrievalDatabase : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        public RetrievalDatabase(params Type[] tables)
        {
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
            Db.CodeFirst.InitTables(tables);
            Db.Aop.OnLogExecuting = (sql, _) => Sql.Add(sql);
        }

        public SqlSugarClient Db { get; }

        public List<string> Sql { get; } = new();

        public SaveLifeAgentRetrievalService CreateService(RecordingEmbeddingService embedding)
        {
            return new SaveLifeAgentRetrievalService(
                SqlSugarRepository<SaveLifeAgentVectorIndex>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeDiseaseKnowledge>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(Db),
                new FixedClientFactory(),
                embedding,
                new RecordingMaterialEmbeddingService());
        }

        public void Dispose()
        {
            Db.Dispose();
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }

    private sealed class FixedClientFactory : ISaveLifeAgentClientFactory
    {
        public Task<SaveLifeAgentOptions> GetOptionsAsync() => Task.FromResult(new SaveLifeAgentOptions());

        public Task<bool> CanUseChatAsync() => Task.FromResult(false);

        public Task<bool> CanUseEmbeddingsAsync() => Task.FromResult(true);

        public Task<ChatClient> CreateChatClientAsync() => throw new NotSupportedException();

        public Task<EmbeddingClient> CreateEmbeddingClientAsync() => throw new NotSupportedException();
    }

    private sealed class RecordingEmbeddingService : ISaveLifeAgentEmbeddingService
    {
        public List<string> Inputs { get; } = new();

        public Task<float[]?> GenerateEmbeddingAsync(string text)
        {
            Inputs.Add(text);
            return Task.FromResult<float[]?>([1F, 0F]);
        }

        public string ComputeContentHash(string text) => "hash";

        public double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right) => 1D;

        public string SerializeVector(IReadOnlyList<float> vector) => "[1,0]";

        public float[] DeserializeVector(string? json) => [1F, 0F];
    }

    private sealed class RecordingMaterialEmbeddingService : ISaveLifeMaterialEmbeddingService
    {
        public Task<SaveLifeMaterialEmbeddingResult?> GenerateQueryEmbeddingAsync(string text)
        {
            return Task.FromResult<SaveLifeMaterialEmbeddingResult?>(new SaveLifeMaterialEmbeddingResult
            {
                Embedding = [1F, 0F],
                Model = "test-material-embedding"
            });
        }

        public Task<SaveLifeMaterialEmbeddingResult?> GenerateMaterialEmbeddingAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>> openFileStream) => throw new NotSupportedException();
    }
}
