using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeClinicalTrialDataMigrationTests
{
    [Theory]
    [InlineData("适应症：冠心病；参与者类型：患者", "冠心病")]
    [InlineData("药物名称：匹伐他汀钙片;适应症:高胆固醇血症;试验分期:III期", "高胆固醇血症")]
    public void ExtractIndication_WithFollowingSameLineField_StopsAtFieldSeparator(
        string source,
        string expected)
    {
        var indication = SaveLifeClinicalTrialDataMigration.ExtractIndication(source);

        Assert.Equal(expected, indication);
    }

    [Fact]
    public void Apply_WithLegacyIndicationsAndDiseaseConflicts_BackfillsAndNormalizesIdempotently()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeClinicalTrial));
            db.Insertable(new[]
            {
                CreateTrial(1, "冠心病项目", "呼吸系统疾病", null,
                    requirements: "参与者类型：患者；适应症：冠心病"),
                CreateTrial(2, "匹伐他汀钙片研究", "消化系统疾病", null,
                    description: "药物名称：匹伐他汀钙片；适应症：高胆固醇血症"),
                CreateTrial(3, "阴道炎治疗研究", "消化系统疾病", "细菌性阴道病"),
                CreateTrial(4, "未分类疾病项目", null, "罕见未分类综合征"),
            }).ExecuteCommand();

            var first = SaveLifeClinicalTrialDataMigration.Apply(db);
            var second = SaveLifeClinicalTrialDataMigration.Apply(db);

            Assert.Equal(4, first.TotalCount);
            Assert.Equal(2, first.IndicationBackfilledCount);
            Assert.Equal(3, first.DiseaseAreaCorrectedCount);
            Assert.Equal(3, first.ConflictCount);
            Assert.Equal(1, first.UnresolvedCount);
            Assert.Equal(3, first.ChangedCount);

            var trials = db.Queryable<SaveLifeClinicalTrial>().OrderBy(u => u.Id).ToList();
            Assert.Equal("冠心病", trials[0].Indication);
            Assert.Equal("循环系统疾病", trials[0].DiseaseArea);
            Assert.Equal("高胆固醇血症", trials[1].Indication);
            Assert.Equal("代谢系统与营养疾病", trials[1].DiseaseArea);
            Assert.Equal("生殖系统疾病", trials[2].DiseaseArea);
            Assert.Null(trials[3].DiseaseArea);
            Assert.All(trials.Take(3), trial => Assert.False(trial.IsInKnowledgeBase));

            Assert.Equal(0, second.IndicationBackfilledCount);
            Assert.Equal(0, second.DiseaseAreaCorrectedCount);
            Assert.Equal(0, second.ConflictCount);
            Assert.Equal(1, second.UnresolvedCount);
            Assert.Equal(0, second.ChangedCount);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SaveLifeClinicalTrial CreateTrial(
        long id,
        string title,
        string? diseaseArea,
        string? indication,
        string? description = null,
        string? requirements = null)
    {
        return new SaveLifeClinicalTrial
        {
            Id = id,
            Title = title,
            CdeRegistrationNumber = $"CTR2026{id:D4}",
            DiseaseArea = diseaseArea,
            Indication = indication,
            Description = description,
            Requirements = requirements,
            Status = "recruiting",
            IsInKnowledgeBase = true,
            KnowledgeBaseRetryCount = 2,
            CreateTime = new DateTime(2026, 7, 11),
        };
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = $"Data Source={dbPath};Pooling=False",
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
    }
}
