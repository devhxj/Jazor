using System.Reflection;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeAgentProjectDiscoveryTests
{
    [Fact]
    public async Task BuildRetrievalAsync_WithoutPatient_UsesConfiguredDiscoveryLimit()
    {
        using var database = new DiscoveryDatabase();
        for (var index = 1; index <= 6; index++)
        {
            database.InsertTrial(
                700 + index,
                $"肺癌公开项目 {index}",
                SaveLifeAgentTrialSelection.RecruitingStatus,
                isPinned: index == 4,
                sortOrder: index);
        }
        database.InsertTrial(707, "肺癌已结束项目", "closed", isPinned: true, sortOrder: 0);
        database.InsertTrial(708, "乳腺癌置顶项目", SaveLifeAgentTrialSelection.RecruitingStatus, isPinned: true, sortOrder: 0);
        database.Sql.Clear();

        var result = await database.CreateService(maxTrialReferences: 4).BuildRetrievalAsync(
            "有哪些肺癌临床试验",
            new SaveLifeAgentPatientSignal { DiseaseKeyword = "肺癌" },
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.ProjectDiscovery);

        Assert.Equal(SaveLifeAgentTrialRetrievalModes.Discovery, result.TrialRetrievalMode);
        Assert.False(result.TrialMatchingGate.WasEvaluated);
        Assert.Equal(4, result.Trials.Count);
        Assert.All(result.Trials, trial => Assert.Contains("肺癌公开项目", trial.Title));
        Assert.DoesNotContain(result.Trials, trial => trial.Id is "707" or "708");
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.Succeeded, result.Diagnostics.TrialStatus);
    }

    [Fact]
    public async Task BuildRetrievalAsync_Knowledge_UsesConfiguredReferenceLimit()
    {
        using var database = new DiscoveryDatabase();
        for (var index = 1; index <= 5; index++)
            database.InsertKnowledge(800 + index, $"肺癌知识 {index}", "肺癌");

        var result = await database.CreateService(maxKnowledgeReferences: 2).BuildRetrievalAsync(
            "肺癌一般怎么治疗",
            new SaveLifeAgentPatientSignal { DiseaseKeyword = "肺癌" },
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.KnowledgeOnly);

        Assert.Equal(2, result.Knowledge.Count);
        Assert.All(result.Knowledge, item => Assert.Contains("肺癌知识", item.Title));
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithoutStructuredTerms_UsesCurrentQueryForPublicDiscovery()
    {
        using var database = new DiscoveryDatabase();
        database.InsertTrial(
            701,
            "乳腺癌置顶项目",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: true,
            sortOrder: 0);
        database.Sql.Clear();

        var result = await database.CreateService().BuildRetrievalAsync(
            "有哪些临床试验",
            new SaveLifeAgentPatientSignal(),
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.ProjectDiscovery);

        var trial = Assert.Single(result.Trials);
        Assert.Equal("701", trial.Id);
        Assert.Equal(SaveLifeAgentTrialRetrievalModes.Discovery, result.TrialRetrievalMode);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.Succeeded, result.Diagnostics.TrialStatus);
        Assert.Contains(database.Sql, sql => ContainsTable(sql, "sl_agent_vector_index"));
        Assert.Contains(database.Sql, sql => ContainsTable(sql, "sl_clinical_trial"));
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithAiTumorTerm_ReturnsRecruitingProject()
    {
        using var database = new DiscoveryDatabase();
        database.InsertTrial(
            707,
            "肿瘤受试者临床研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "肿瘤",
            indication: "实体肿瘤");

        var result = await database.CreateService().BuildRetrievalAsync(
            "现在有肿瘤招募项目吗",
            new SaveLifeAgentPatientSignal { SearchTerms = ["肿瘤"] },
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.ProjectDiscovery);

        var trial = Assert.Single(result.Trials);
        Assert.Equal("707", trial.Id);
        Assert.Equal("实体肿瘤", trial.Indication);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.Succeeded, result.Diagnostics.TrialStatus);
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithRequestedTrial_ReturnsProjectWithoutVectorIndexOrQueryTerms()
    {
        using var database = new DiscoveryDatabase();
        database.InsertTrial(
            708,
            "指定项目",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "罕见病",
            indication: "特定适应症",
            includeIndex: false);

        var result = await database.CreateService().BuildRetrievalAsync(
            "这个项目怎么样",
            new SaveLifeAgentPatientSignal(),
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.ProjectDiscovery with
            {
                RequestedTrialIds = [708]
            });

        var trial = Assert.Single(result.Trials);
        Assert.Equal("708", trial.Id);
        Assert.Equal("指定项目", trial.Title);
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithAiTumorIntent_RetrievesKnowledgeAndProjectTogether()
    {
        using var database = new DiscoveryDatabase();
        database.InsertKnowledge(801, "肿瘤治疗基础知识", "肿瘤");
        database.InsertTrial(
            802,
            "实体肿瘤公开招募研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "肿瘤",
            indication: "实体肿瘤");

        var result = await database.CreateService().BuildRetrievalAsync(
            "肿瘤一般怎么治疗？另外现在有肿瘤招募项目吗？",
            new SaveLifeAgentPatientSignal { SearchTerms = ["肿瘤"] },
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.PublicIllnessHelp);

        Assert.Equal("801", Assert.Single(result.Knowledge).Id);
        Assert.Equal("802", Assert.Single(result.Trials).Id);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.Succeeded, result.Diagnostics.KnowledgeStatus);
        Assert.Equal(SaveLifeAgentRetrievalSourceStatuses.Succeeded, result.Diagnostics.TrialStatus);
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithDifferentIntentTopics_IsolatesKnowledgeAndTrialTerms()
    {
        using var database = new DiscoveryDatabase();
        database.InsertKnowledge(811, "奥希替尼用药知识", "奥希替尼");
        database.InsertKnowledge(812, "白癜风疾病知识", "白癜风");
        database.InsertTrial(
            813,
            "白癜风公开招募研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "皮肤疾病",
            indication: "白癜风");
        database.InsertTrial(
            814,
            "奥希替尼公开招募研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "肿瘤",
            indication: "奥希替尼治疗");

        var result = await database.CreateService().BuildRetrievalAsync(
            "奥希替尼有什么副作用？另外有白癜风项目吗？",
            new SaveLifeAgentPatientSignal
            {
                SearchTerms = ["奥希替尼", "白癜风"],
                KnowledgeSearchTerms = ["奥希替尼"],
                TrialSearchTerms = ["白癜风"],
                UseSourceSearchTerms = true
            },
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.PublicIllnessHelp);

        Assert.Equal("811", Assert.Single(result.Knowledge).Id);
        Assert.Equal("813", Assert.Single(result.Trials).Id);
    }

    [Fact]
    public async Task BuildRetrievalAsync_VitiligoQuery_SearchesAllTrialIndexesBeforeRanking()
    {
        using var database = new DiscoveryDatabase();
        var old = new DateTime(2026, 1, 1);
        database.InsertTrial(
            9001,
            "白癜风公开招募研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "皮肤疾病",
            indication: "白癜风",
            embeddingJson: "[1,0]",
            createdAt: old);
        for (var index = 0; index < 60; index++)
        {
            database.InsertTrial(
                9100 + index,
                $"无关公开招募研究 {index}",
                SaveLifeAgentTrialSelection.RecruitingStatus,
                isPinned: false,
                sortOrder: index,
                diseaseArea: "其他疾病",
                indication: "其他适应症",
                embeddingJson: "[0,1]",
                createdAt: old.AddDays(index + 1));
        }

        var result = await database.CreateService().BuildRetrievalAsync(
            "有没有白癜风实验项目",
            new SaveLifeAgentPatientSignal(),
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.ProjectDiscovery);

        var trial = Assert.Single(result.Trials);
        Assert.Equal("9001", trial.Id);
    }

    [Fact]
    public async Task BuildRetrievalAsync_VitiligoQuery_SearchesAllKnowledgeIndexesBeforeRanking()
    {
        using var database = new DiscoveryDatabase();
        var old = new DateTime(2026, 1, 1);
        database.InsertKnowledge(9201, "白癜风规范诊疗", "白癜风", "[1,0]", old);
        for (var index = 0; index < 20; index++)
        {
            database.InsertKnowledge(
                9300 + index,
                $"无关疾病知识 {index}",
                "其他疾病",
                "[0,1]",
                old.AddDays(index + 1));
        }

        var result = await database.CreateService().BuildRetrievalAsync(
            "白癜风是什么",
            new SaveLifeAgentPatientSignal(),
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.KnowledgeOnly);

        var knowledge = Assert.Single(result.Knowledge);
        Assert.Equal("9201", knowledge.Id);
    }

    [Fact]
    public async Task BuildRetrievalAsync_UnindexedRelevantTrialAndDiseaseOnlyInExclusionCriteria_ReturnsOnlyRelevantTrial()
    {
        using var database = new DiscoveryDatabase();
        database.InsertTrial(
            815,
            "白癜风公开招募研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "皮肤疾病",
            indication: "白癜风",
            includeIndex: false);
        database.InsertTrial(
            816,
            "骨肉瘤公开招募研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: true,
            sortOrder: 0,
            diseaseArea: "肿瘤",
            indication: "骨肉瘤",
            exclusionCriteria: "患有白癜风的受试者不能参加",
            indexContent: "适应症：骨肉瘤\n排除标准：患有白癜风的受试者不能参加");

        var result = await database.CreateService().BuildRetrievalAsync(
            "有白癜风相关项目吗",
            new SaveLifeAgentPatientSignal
            {
                SearchTerms = ["白癜风"],
                TrialSearchTerms = ["白癜风"],
                UseSourceSearchTerms = true
            },
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.ProjectDiscovery);

        var trial = Assert.Single(result.Trials);
        Assert.Equal("815", trial.Id);
    }

    [Theory]
    [InlineData("靶向药物给药安排")]
    [InlineData("受试者交通补贴")]
    public async Task BuildRetrievalAsync_UnindexedTrialWithProjectContent_ReturnsProjectForDescriptionAndBenefitsTerms(
        string searchTerm)
    {
        using var database = new DiscoveryDatabase();
        database.InsertTrial(
            817,
            "项目内容检索研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            diseaseArea: "肿瘤",
            indication: "实体肿瘤",
            includeIndex: false,
            description: "本项目采用靶向药物给药安排，并定期评估疗效。",
            benefits: "受试者交通补贴和规定检查费用由研究方承担。");

        var result = await database.CreateService().BuildRetrievalAsync(
            searchTerm,
            new SaveLifeAgentPatientSignal
            {
                SearchTerms = [searchTerm],
                TrialSearchTerms = [searchTerm],
                UseSourceSearchTerms = true
            },
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.ProjectDiscovery);

        var trial = Assert.Single(result.Trials);
        Assert.Equal("817", trial.Id);
        Assert.Contains("靶向药物给药安排", trial.Description);
        Assert.Contains("受试者交通补贴", trial.Benefits);
    }

    [Fact]
    public async Task SyncTrialIndexAsync_IncludesProjectDescriptionAndParticipantBenefits()
    {
        using var database = new DiscoveryDatabase();
        database.InsertTrial(
            818,
            "项目入库内容研究",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            includeIndex: false,
            description: "项目描述独有内容。",
            benefits: "受试者权益独有内容。");

        var trial = database.Db.Queryable<SaveLifeClinicalTrial>().InSingle(818);
        await database.CreateService().SyncTrialIndexAsync(trial);

        var index = database.Db.Queryable<SaveLifeAgentVectorIndex>()
            .Single(item => item.ResourceType == "trial" && item.ResourceId == 818);
        Assert.Contains("项目描述：项目描述独有内容。", index.Content);
        Assert.Contains("受试者权益：受试者权益独有内容。", index.Content);
    }

    [Fact]
    public async Task BuildRetrievalAsync_WithRequestedTrialAndIncompleteProfile_UsesDiscoveryBoundary()
    {
        using var database = new DiscoveryDatabase();
        database.InsertTrial(
            815,
            "上下文指定项目",
            SaveLifeAgentTrialSelection.RecruitingStatus,
            isPinned: false,
            sortOrder: 10,
            includeIndex: false);

        var result = await database.CreateService().BuildRetrievalAsync(
            "这个项目适合我吗",
            new SaveLifeAgentPatientSignal(),
            cancellationToken: TestContext.Current.CancellationToken,
            scope: SaveLifeAgentRetrievalScope.TrialsAndMaterials with
            {
                RequestedTrialIds = [815]
            });

        Assert.Equal("815", Assert.Single(result.Trials).Id);
        Assert.Equal(SaveLifeAgentTrialRetrievalModes.Discovery, result.TrialRetrievalMode);
        Assert.True(result.TrialMatchingGate.WasEvaluated);
        Assert.False(result.TrialMatchingGate.CanMatch);
    }

    private static bool ContainsTable(string sql, string table)
    {
        return sql.Contains(table, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class DiscoveryDatabase : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");

        public DiscoveryDatabase()
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
            Db.CodeFirst.InitTables(
                typeof(SaveLifeAgentVectorIndex),
                typeof(SaveLifeDiseaseKnowledge),
                typeof(SaveLifeClinicalTrial));
            Db.Aop.OnLogExecuting = (sql, _) => Sql.Add(sql);
        }

        public SqlSugarClient Db { get; }

        public List<string> Sql { get; } = [];

        public void InsertTrial(
            long id,
            string title,
            string status,
            bool isPinned,
            int sortOrder,
            string? diseaseArea = null,
            string? indication = null,
            bool includeIndex = true,
            string? exclusionCriteria = null,
            string? indexContent = null,
            string embeddingJson = "[1,0]",
            DateTime? createdAt = null,
            string? description = null,
            string? benefits = null)
        {
            var disease = diseaseArea ??
                (title.StartsWith("肺癌", StringComparison.Ordinal) ? "肺癌" : "乳腺癌");
            Db.Insertable(new SaveLifeClinicalTrial
            {
                Id = id,
                Title = title,
                CdeRegistrationNumber = $"CTR{id}",
                DiseaseArea = disease,
                Indication = indication ?? $"晚期{disease}",
                Description = description,
                Benefits = benefits,
                ExclusionCriteria = exclusionCriteria,
                Status = status,
                IsPinned = isPinned,
                SortOrder = sortOrder,
                CreateTime = createdAt ?? DateTime.Now
            }).ExecuteCommand();
            if (!includeIndex)
                return;

            Db.Insertable(new SaveLifeAgentVectorIndex
            {
                Id = id + 1000,
                ResourceType = "trial",
                ResourceId = id,
                Title = title,
                DiseaseName = disease,
                Content = indexContent ?? $"{title}，面向晚期{disease}患者",
                EmbeddingJson = embeddingJson,
                SyncStatus = "synced",
                IsPinned = isPinned,
                SortOrder = sortOrder,
                CreateTime = createdAt ?? DateTime.Now
            }).ExecuteCommand();
        }

        public void InsertKnowledge(
            long id,
            string title,
            string diseaseName,
            string embeddingJson = "[1,0]",
            DateTime? createdAt = null)
        {
            Db.Insertable(new SaveLifeDiseaseKnowledge
            {
                Id = id,
                Title = title,
                DiseaseName = diseaseName,
                Summary = $"{diseaseName}常见治疗方式和就医建议",
                Content = $"介绍{diseaseName}的规范诊疗原则。",
                IsPublished = true,
                SortOrder = 10,
                CreateTime = createdAt ?? DateTime.Now
            }).ExecuteCommand();
            Db.Insertable(new SaveLifeAgentVectorIndex
            {
                Id = id + 5000,
                ResourceType = "knowledge",
                ResourceId = id,
                Title = title,
                DiseaseName = diseaseName,
                Content = $"{title}，介绍{diseaseName}的规范诊疗原则。",
                EmbeddingJson = embeddingJson,
                SyncStatus = "synced",
                SortOrder = 10,
                CreateTime = createdAt ?? DateTime.Now
            }).ExecuteCommand();
        }

        public SaveLifeAgentRetrievalService CreateService(
            int? maxKnowledgeReferences = null,
            int? maxTrialReferences = null)
        {
            return new SaveLifeAgentRetrievalService(
                SqlSugarRepository<SaveLifeAgentVectorIndex>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeDiseaseKnowledge>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(Db),
                SqlSugarRepository<SaveLifeEnrollmentMaterial>.CreateForContext(Db),
                new FixedClientFactory(maxKnowledgeReferences, maxTrialReferences),
                new FixedEmbeddingService(),
                new FixedMaterialEmbeddingService());
        }

        public void Dispose()
        {
            Db.Dispose();
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }

    private sealed class FixedClientFactory(
        int? maxKnowledgeReferences = null,
        int? maxTrialReferences = null) : ISaveLifeAgentClientFactory
    {
        public Task<SaveLifeAgentOptions> GetOptionsAsync()
        {
            var options = new SaveLifeAgentOptions();
            if (maxKnowledgeReferences.HasValue)
                options.MaxKnowledgeReferences = maxKnowledgeReferences.Value;
            if (maxTrialReferences.HasValue)
                options.MaxTrialReferences = maxTrialReferences.Value;
            return Task.FromResult(options);
        }

        public Task<bool> CanUseChatAsync() => Task.FromResult(false);

        public Task<bool> CanUseEmbeddingsAsync() => Task.FromResult(true);

        public Task<ChatClient> CreateChatClientAsync() => throw new NotSupportedException();

        public Task<EmbeddingClient> CreateEmbeddingClientAsync() => throw new NotSupportedException();
    }

    private sealed class FixedEmbeddingService : ISaveLifeAgentEmbeddingService
    {
        public Task<float[]?> GenerateEmbeddingAsync(string text) => Task.FromResult<float[]?>([1F, 0F]);

        public string ComputeContentHash(string text) => "hash";

        public double CosineSimilarity(IReadOnlyList<float> left, IReadOnlyList<float> right) => 1D;

        public string SerializeVector(IReadOnlyList<float> vector) => "[1,0]";

        public float[] DeserializeVector(string? json) => [1F, 0F];
    }

    private sealed class FixedMaterialEmbeddingService : ISaveLifeMaterialEmbeddingService
    {
        public Task<SaveLifeMaterialEmbeddingResult?> GenerateQueryEmbeddingAsync(string text) =>
            Task.FromResult<SaveLifeMaterialEmbeddingResult?>(null);

        public Task<SaveLifeMaterialEmbeddingResult?> GenerateMaterialEmbeddingAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>> openFileStream) => throw new NotSupportedException();
    }
}
