using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeEnrollmentEfficiencyContractTests
{
    [Fact]
    public async Task EnrollmentTrialSiteProjection_ReadsSitesWithSqlSugar()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={dbPath};Pooling=False",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
            });
            db.Ado.ExecuteCommand("""
                CREATE TABLE sl_clinical_trial (Id INTEGER PRIMARY KEY, Sites TEXT);
                INSERT INTO sl_clinical_trial (Id, Sites) VALUES
                    (7, '[{"institution_name":"中心医院"}]');
                """);

            var projectIds = new List<long> { 7 };
            var trials = await db.Queryable<SaveLifeClinicalTrial>()
                .Where(u => projectIds.Contains(u.Id))
                .Select(u => new SaveLifeClinicalTrial { Id = u.Id, Sites = u.Sites })
                .ToListAsync(TestContext.Current.CancellationToken);

            var trial = Assert.Single(trials);
            Assert.Equal(7, trial.Id);
            Assert.Equal("中心医院", SaveLifeTrialSiteFormatter.FormatInstitutionNames(trial.Sites));
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task EnrollmentStatusCounts_QueryAsync_GroupsInDatabase()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = $"Data Source={dbPath};Pooling=False",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
            });
            db.Ado.ExecuteCommand("""
                CREATE TABLE sl_enrollment_record (
                    Id INTEGER PRIMARY KEY,
                    ExternalUserId TEXT,
                    Status TEXT NOT NULL,
                    IsDelete INTEGER NOT NULL DEFAULT 0
                );
                INSERT INTO sl_enrollment_record (Id, ExternalUserId, Status, IsDelete) VALUES
                    (1, 'user-a', 'pending', 0),
                    (2, 'user-a', 'pending', 0),
                    (3, 'user-a', 'approved', 0),
                    (4, 'user-b', 'rejected', 0),
                    (5, 'user-a', 'closed', 1);
                """);

            string? executedSql = null;
            db.Aop.OnLogExecuting = (sql, _) => executedSql = sql;

            var counts = await SaveLifeEnrollmentStatusCounts.QueryAsync(
                db.Queryable<SaveLifeEnrollmentRecord>()
                    .Where(u => !u.IsDelete && u.ExternalUserId == "user-a"));

            Assert.Equal(3, counts.Total);
            Assert.Equal(2, counts.Pending);
            Assert.Equal(1, counts.Approved);
            Assert.Equal(0, counts.Rejected);
            Assert.Contains("GROUP BY", executedSql, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void EnrollmentStatusCounts_CountsEveryStatusWithoutPaging()
    {
        var counts = SaveLifeEnrollmentStatusCounts.Build(
        [
            "pending",
            "pending",
            "approved",
            "rejected",
            "need_supplement",
            "contacted",
            "cancelled",
        ]);

        Assert.Equal(7, counts.Total);
        Assert.Equal(2, counts.Pending);
        Assert.Equal(1, counts.Approved);
        Assert.Equal(1, counts.Rejected);
        Assert.Equal(1, counts.NeedSupplement);
        Assert.Equal(1, counts.Contacted);
        Assert.Equal(1, counts.Cancelled);
    }

    [Fact]
    public void EnrollmentEfficiencyContracts_ExposeCountsAndEmbeddedDetailMaterials()
    {
        var method = typeof(SaveLifeEnrollmentRecordService).GetMethod(
            "MiniAppStatusCounts",
            BindingFlags.Instance | BindingFlags.Public);
        var route = method?.GetCustomAttribute<HttpGetAttribute>();

        Assert.NotNull(method);
        Assert.Equal("/api/v1/save-life/enrollments/status-counts", route?.Template);
        Assert.NotNull(typeof(SaveLifeMiniAppEnrollmentOutput).GetProperty("Materials"));
        Assert.NotNull(typeof(SaveLifeMiniAppEnrollmentOutput).GetProperty("InstitutionName"));
        Assert.Equal(
            "中心医院、第一医院",
            SaveLifeTrialSiteFormatter.FormatInstitutionNames(
                """[{"institution_name":"中心医院"},{"institutionName":"第一医院"}]"""));
        Assert.Equal(
            "中心医院、第一医院",
            SaveLifeTrialSiteFormatter.FormatInstitutionNames("中心医院；第一医院"));
        Assert.Equal(
            "研究医院",
            SaveLifeTrialSiteFormatter.FormatInstitutionNames("""{"siteName":"研究医院"}"""));
        Assert.Equal(20, SaveLifeEnrollmentMaterialHelper.MaxMaterialListCount);
    }

    [Fact]
    public async Task Page_WithProjectAndAuditFilters_ReturnsCurrentProjectAndUserFields()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
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
            db.CodeFirst.InitTables(
                typeof(SaveLifeClinicalTrial),
                typeof(SaveLifeEnrollmentRecord),
                typeof(SaveLifeUserProfile),
                typeof(SaveLifePatientProfile));
            db.Insertable(new SaveLifeClinicalTrial
            {
                Id = 101,
                CdeRegistrationNumber = "CTR20262001",
                ClinicalPhase = "BE",
                DiseaseArea = "肿瘤",
                Sponsor = "示例申办方",
                Status = "recruiting",
                Title = "当前项目名称"
            }).ExecuteCommand();
            db.Insertable(new SaveLifeUserProfile
            {
                Id = 501,
                ExternalUserId = "account-1",
                RealName = "报名用户"
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                new SaveLifeEnrollmentRecord
                {
                    Id = 1001,
                    ExternalUserId = "account-1",
                    ProjectId = 101,
                    ProjectTitle = "历史项目名称",
                    Status = "pending",
                    UserProfileId = 501
                },
                new SaveLifeEnrollmentRecord
                {
                    Id = 1002,
                    ExternalUserId = "account-1",
                    ProjectId = 101,
                    ProjectTitle = "当前项目名称",
                    Status = "approved",
                    UserProfileId = 501
                }
            }).ExecuteCommand();

            var service = new SaveLifeEnrollmentRecordService(
                SqlSugarRepository<SaveLifeEnrollmentRecord>.CreateForContext(db),
                SqlSugarRepository<SaveLifeClinicalTrial>.CreateForContext(db),
                SqlSugarRepository<SaveLifeUserProfile>.CreateForContext(db),
                SqlSugarRepository<SaveLifePatientProfile>.CreateForContext(db),
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            var result = await service.Page(new SaveLifeEnrollmentRecordInput
            {
                AuditStatus = "pending",
                ClinicalPhase = "BE",
                DiseaseArea = "肿瘤",
                Keyword = "ctr-2026-2001",
                Page = 1,
                PageSize = 10,
                ProjectStatus = "recruiting"
            });

            var record = Assert.Single(result.Items);
            Assert.Equal(1001, record.Id);
            Assert.Equal("CTR20262001", record.ProjectCdeRegistrationNumber);
            Assert.Equal("肿瘤", record.ProjectDiseaseArea);
            Assert.Equal("BE", record.ProjectClinicalPhase);
            Assert.Equal("当前项目名称", record.ProjectTitle);
            Assert.Equal("报名用户", record.UserProfile?.RealName);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }
}
