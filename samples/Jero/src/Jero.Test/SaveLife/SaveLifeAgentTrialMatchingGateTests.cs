using System.Reflection;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentTrialMatchingGateTests
{
    [Fact]
    public void EvaluateMatchingGate_WithoutCurrentPatient_ReturnsAllRequiredFields()
    {
        var gate = SaveLifeAgentTrialSelection.EvaluateMatchingGate(null);

        Assert.True(gate.WasEvaluated);
        Assert.False(gate.CanMatch);
        Assert.Equal(
            [
                SaveLifeAgentTrialMatchingFields.PatientProfile,
                SaveLifeAgentTrialMatchingFields.Diagnosis,
                SaveLifeAgentTrialMatchingFields.DiseaseStageOrStatus,
                SaveLifeAgentTrialMatchingFields.Treatment
            ],
            gate.MissingFields);
    }

    [Fact]
    public void EvaluateMatchingGate_WithStageAndCurrentTreatment_AllowsMatching()
    {
        var gate = SaveLifeAgentTrialSelection.EvaluateMatchingGate(new SaveLifePatientProfileOutput
        {
            Id = "501",
            Diagnosis = "肺癌",
            DiseaseStage = "晚期",
            CurrentTreatment = "正在接受二线化疗"
        });

        Assert.True(gate.WasEvaluated);
        Assert.True(gate.CanMatch);
        Assert.Empty(gate.MissingFields);
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithIncompleteProfile_ReturnsMissingFieldsWithoutQueryingTrials()
    {
        using var database = new TrialDatabase();
        database.Sql.Clear();
        var service = database.CreateService();

        var result = await service.BuildRetrievalAsync(
            "还有其他机会吗",
            new SaveLifeAgentPatientSignal
            {
                CurrentPatient = new SaveLifePatientProfileOutput
                {
                    Id = "501",
                    Diagnosis = "肺癌"
                }
            },
            "openid-a",
            cancellationToken: TestContext.Current.CancellationToken,
            scope: new SaveLifeAgentRetrievalScope(false, true, false));

        Assert.True(result.TrialMatchingGate.WasEvaluated);
        Assert.False(result.TrialMatchingGate.CanMatch);
        Assert.Equal(
            [
                SaveLifeAgentTrialMatchingFields.DiseaseStageOrStatus,
                SaveLifeAgentTrialMatchingFields.Treatment
            ],
            result.TrialMatchingGate.MissingFields);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NotRequested, result.Diagnostics.TrialStatus);
        Assert.DoesNotContain(database.Sql, sql => ContainsTable(sql, "sl_agent_vector_index"));
        Assert.DoesNotContain(database.Sql, sql => ContainsTable(sql, "sl_clinical_trial"));
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithNoReliableMatch_DoesNotReturnUnrelatedPinnedTrial()
    {
        using var database = new TrialDatabase();
        database.Db.Insertable(new SaveLifeClinicalTrial
        {
            Id = 701,
            Title = "乳腺癌置顶招募项目",
            CdeRegistrationNumber = "CTR20260701",
            DiseaseArea = "乳腺癌",
            Indication = "晚期乳腺癌",
            Status = SaveLifeAgentTrialSelection.RecruitingStatus,
            IsPinned = true,
            SortOrder = 1
        }).ExecuteCommand();
        database.Db.Insertable(new SaveLifeAgentVectorIndex
        {
            Id = 801,
            ResourceType = "trial",
            ResourceId = 701,
            Title = "乳腺癌置顶招募项目",
            DiseaseName = "乳腺癌",
            Content = "面向晚期乳腺癌患者",
            EmbeddingJson = "[1,0]",
            SyncStatus = "synced",
            IsPinned = true,
            SortOrder = 1
        }).ExecuteCommand();
        database.Sql.Clear();
        var service = database.CreateService();

        var result = await service.BuildRetrievalAsync(
            "还有其他机会吗",
            CompleteSignal(),
            "openid-a",
            cancellationToken: TestContext.Current.CancellationToken,
            scope: new SaveLifeAgentRetrievalScope(false, true, false));

        Assert.True(result.TrialMatchingGate.CanMatch);
        Assert.Empty(result.Trials);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.NoHits, result.Diagnostics.TrialStatus);
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithCompleteProfile_UsesConfirmedFieldsForMatching()
    {
        using var database = new TrialDatabase();
        database.Db.Insertable(new SaveLifeClinicalTrial
        {
            Id = 702,
            Title = "肺癌二线治疗项目",
            CdeRegistrationNumber = "CTR20260702",
            DiseaseArea = "肺癌",
            Indication = "晚期或复发肺癌",
            Requirements = "既往一线化疗后进展",
            Status = SaveLifeAgentTrialSelection.RecruitingStatus,
            SortOrder = 20
        }).ExecuteCommand();
        database.Db.Insertable(new SaveLifeAgentVectorIndex
        {
            Id = 802,
            ResourceType = "trial",
            ResourceId = 702,
            Title = "肺癌二线治疗项目",
            DiseaseName = "肺癌",
            Content = "晚期或复发肺癌，一线化疗后进展",
            EmbeddingJson = "[1,0]",
            SyncStatus = "synced",
            SortOrder = 20
        }).ExecuteCommand();
        var embedding = new FixedEmbeddingService();
        var service = database.CreateService(embedding);

        var result = await service.BuildRetrievalAsync(
            "还有其他机会吗",
            CompleteSignal(),
            "openid-a",
            cancellationToken: TestContext.Current.CancellationToken,
            scope: new SaveLifeAgentRetrievalScope(false, true, false));

        Assert.Single(result.Trials);
        var embeddingInput = Assert.Single(embedding.Inputs);
        Assert.Contains("还有其他机会吗", embeddingInput, StringComparison.Ordinal);
        Assert.Contains("肺癌", embeddingInput, StringComparison.Ordinal);
        Assert.Contains("复发", embeddingInput, StringComparison.Ordinal);
        Assert.Contains("一线化疗后进展", embeddingInput, StringComparison.Ordinal);
    }

    [Fact]
    public void OrderMatchesByRelevance_PrioritizesScoreBeforePinnedAndOperationalOrder()
    {
        var trials = new[]
        {
            new SaveLifeAgentTrialReference { Id = "low-pinned", Score = 0.70, IsPinned = true, SortOrder = 1 },
            new SaveLifeAgentTrialReference { Id = "high-normal", Score = 0.90, IsPinned = false, SortOrder = 1 },
            new SaveLifeAgentTrialReference { Id = "high-pinned-later", Score = 0.90, IsPinned = true, SortOrder = 20 },
            new SaveLifeAgentTrialReference { Id = "high-pinned-first", Score = 0.90, IsPinned = true, SortOrder = 10 }
        };

        var orderedIds = trials
            .OrderMatchesByRelevance()
            .Select(trial => trial.Id)
            .ToList();

        Assert.Equal(
            ["high-pinned-first", "high-pinned-later", "high-normal", "low-pinned"],
            orderedIds);
    }

    private static SaveLifeAgentPatientSignal CompleteSignal()
    {
        return new SaveLifeAgentPatientSignal
        {
            CurrentPatient = new SaveLifePatientProfileOutput
            {
                Id = "501",
                Diagnosis = "肺癌",
                DiseaseStatus = "复发",
                TreatmentHistory = "一线化疗后进展",
                City = "成都"
            }
        };
    }

    private static bool ContainsTable(string sql, string table)
    {
        return sql.Contains(table, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TrialDatabase : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        public TrialDatabase()
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
            Db.CodeFirst.InitTables(typeof(SaveLifeAgentVectorIndex), typeof(SaveLifeClinicalTrial));
            Db.Aop.OnLogExecuting = (sql, _) => Sql.Add(sql);
        }

        public SqlSugarClient Db { get; }

        public List<string> Sql { get; } = new();

        public SaveLifeAgentRetrievalService CreateService(FixedEmbeddingService? embedding = null)
        {
            return new SaveLifeAgentRetrievalService(
                SqlSugarRepository<SaveLifeAgentVectorIndex>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeDiseaseKnowledge>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(Db),
                new FixedClientFactory(),
                embedding ?? new FixedEmbeddingService(),
                new FixedMaterialEmbeddingService());
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

    private sealed class FixedEmbeddingService : ISaveLifeAgentEmbeddingService
    {
        public List<string> Inputs { get; } = [];

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

    private sealed class FixedMaterialEmbeddingService : ISaveLifeMaterialEmbeddingService
    {
        public Task<SaveLifeMaterialEmbeddingResult?> GenerateQueryEmbeddingAsync(string text) =>
            Task.FromResult<SaveLifeMaterialEmbeddingResult?>(new SaveLifeMaterialEmbeddingResult
            {
                Embedding = [1F, 0F],
                Model = "test-material-embedding"
            });

        public Task<SaveLifeMaterialEmbeddingResult?> GenerateMaterialEmbeddingAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>> openFileStream) => throw new NotSupportedException();
    }
}
