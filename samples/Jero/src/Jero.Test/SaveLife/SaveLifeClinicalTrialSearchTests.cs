using System.Reflection;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeClinicalTrialSearchTests
{
    [Theory]
    [InlineData("CTR20262654")]
    [InlineData("ctr-2026-2654")]
    public async Task MiniAppList_WithCdeRegistrationNumber_ReturnsMatchingRecruitingTrial(string keyword)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
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
            db.CodeFirst.InitTables(typeof(SaveLifeClinicalTrial));
            db.Insertable(new SaveLifeClinicalTrial
            {
                Id = 701,
                CdeRegistrationNumber = "CTR20262654",
                ClinicalPhase = "BE",
                Status = "recruiting",
                Title = "CTR 编号检索项目"
            }).ExecuteCommand();

            var service = new SaveLifeClinicalTrialService(
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(db),
                null!,
                null!,
                null!,
                null!,
                null!);

            var result = await service.MiniAppList(new SaveLifeClinicalTrialInput
            {
                Keyword = keyword,
                Page = 1,
                PageSize = 10
            });

            var trial = Assert.Single(result.Items);
            Assert.Equal("701", trial.Id);
            Assert.Equal("CTR20262654", trial.CdeRegistrationNumber);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task ReindexAll_MarksEveryActiveProjectPendingRegardlessOfStatus()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
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
            db.CodeFirst.InitTables(typeof(SaveLifeClinicalTrial));
            db.Insertable(new[]
            {
                new SaveLifeClinicalTrial
                {
                    Id = 801,
                    CdeRegistrationNumber = "CTR2026801",
                    IsInKnowledgeBase = true,
                    KnowledgeBaseError = "旧错误",
                    KnowledgeBaseNextRetryTime = DateTime.Now.AddHours(1),
                    Status = "recruiting",
                    Title = "招募中项目"
                },
                new SaveLifeClinicalTrial
                {
                    Id = 802,
                    CdeRegistrationNumber = "CTR2026802",
                    IsInKnowledgeBase = true,
                    KnowledgeBaseError = "旧错误",
                    KnowledgeBaseNextRetryTime = DateTime.Now.AddHours(1),
                    Status = "completed",
                    Title = "已完成项目"
                },
                new SaveLifeClinicalTrial
                {
                    Id = 803,
                    CdeRegistrationNumber = "CTR2026803",
                    IsDelete = true,
                    IsInKnowledgeBase = true,
                    Status = "completed",
                    Title = "已删除项目"
                }
            }).ExecuteCommand();
            var service = new SaveLifeClinicalTrialService(
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(db),
                null!,
                null!,
                null!,
                null!,
                null!);

            var affected = await service.ReindexAll();

            Assert.Equal(2, affected);
            var projects = db.Queryable<SaveLifeClinicalTrial>()
                .OrderBy(item => item.Id)
                .ToList();
            Assert.False(projects[0].IsInKnowledgeBase);
            Assert.Null(projects[0].KnowledgeBaseError);
            Assert.Null(projects[0].KnowledgeBaseNextRetryTime);
            Assert.False(projects[1].IsInKnowledgeBase);
            Assert.Null(projects[1].KnowledgeBaseError);
            Assert.Null(projects[1].KnowledgeBaseNextRetryTime);
            Assert.True(projects[2].IsInKnowledgeBase);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task TestKnowledgeBase_UsesProjectDiscoveryAndReturnsProjectDetails()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
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
            db.CodeFirst.InitTables(typeof(SaveLifeClinicalTrial));
            db.Insertable(new SaveLifeClinicalTrial
            {
                Id = 901,
                CdeRegistrationNumber = "CTR2026901",
                IsInKnowledgeBase = true,
                Status = "recruiting",
                Title = "交通补贴项目"
            }).ExecuteCommand();
            var retrievalService = new RecordingRetrievalService
            {
                Result = new SaveLifeAgentRetrievalResult
                {
                    Diagnostics = new SaveLifeAgentRetrievalDiagnostics
                    {
                        TextEmbeddingStatus = "succeeded",
                        TrialCandidateCount = 3,
                        TrialStatus = SaveLifeAgentRetrievalSourceStatuses.Succeeded
                    },
                    Trials =
                    [
                        new SaveLifeAgentTrialReference
                        {
                            Id = "901",
                            Title = "交通补贴项目",
                            DiseaseArea = "肿瘤",
                            ClinicalPhase = "III期",
                            Status = "recruiting",
                            Score = 0.93,
                            Description = "项目描述内容",
                            Benefits = "受试者交通补贴"
                        }
                    ]
                }
            };
            var service = new SaveLifeClinicalTrialService(
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(db),
                null!,
                null!,
                null!,
                null!,
                retrievalService);

            var output = await service.TestKnowledgeBase(
                new TestSaveLifeClinicalTrialKnowledgeBaseInput { Keyword = "受试者交通补贴" },
                TestContext.Current.CancellationToken);

            Assert.Equal("受试者交通补贴", retrievalService.Query);
            Assert.Equal(
                SaveLifeAgentTrialRetrievalModes.Discovery,
                retrievalService.Scope?.TrialRetrievalMode);
            Assert.Equal(3, output.CandidateCount);
            Assert.Equal(1, output.HitCount);
            Assert.Equal("succeeded", output.TextEmbeddingStatus);
            var project = Assert.Single(output.Projects);
            Assert.Equal(901, project.Id);
            Assert.Equal("CTR2026901", project.CdeRegistrationNumber);
            Assert.True(project.IsInKnowledgeBase);
            Assert.Equal("项目描述内容", project.Description);
            Assert.Equal("受试者交通补贴", project.Benefits);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private sealed class RecordingRetrievalService : ISaveLifeAgentRetrievalService
    {
        public string? Query { get; private set; }

        public SaveLifeAgentRetrievalResult Result { get; init; } = new();

        public SaveLifeAgentRetrievalScope? Scope { get; private set; }

        public Task<SaveLifeAgentRetrievalResult> BuildRetrievalAsync(
            string query,
            SaveLifeAgentPatientSignal signal,
            string? externalUserId = null,
            long? enrollmentRecordId = null,
            CancellationToken cancellationToken = default,
            SaveLifeAgentRetrievalScope? scope = null)
        {
            Query = query;
            Scope = scope;
            return Task.FromResult(Result);
        }

        public Task SyncKnowledgeIndexAsync(SaveLifeDiseaseKnowledge knowledge)
        {
            return Task.CompletedTask;
        }

        public Task SyncTrialIndexAsync(SaveLifeClinicalTrial trial)
        {
            return Task.CompletedTask;
        }

        public Task SyncMaterialIndexAsync(
            SaveLifeEnrollmentMaterial material,
            Func<Task<Stream>>? openFileStream = null)
        {
            return Task.CompletedTask;
        }

        public Task DeleteMaterialIndexAsync(long materialId, string externalUserId)
        {
            return Task.CompletedTask;
        }
    }
}
