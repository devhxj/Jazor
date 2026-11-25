using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeEnrollmentAuditScopeTests
{
    [Fact]
    public async Task GetCurrentAccessScope_UserHasMultipleAuditRoles_ReturnsConfiguredAreaUnion()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitializeTables(db);
            SeedAuditRoles(db, userId: 9001, roleIds: [101, 102, 103]);
            db.Insertable(new[]
            {
                new SaveLifeEnrollmentAuditScope { Id = 21, RoleId = 101, DiseaseArea = "肿瘤" },
                new SaveLifeEnrollmentAuditScope { Id = 22, RoleId = 102, DiseaseArea = "呼吸系统疾病" },
                new SaveLifeEnrollmentAuditScope { Id = 23, RoleId = 103, DiseaseArea = "循环系统疾病" }
            }).ExecuteCommand();

            var accessService = CreateAccessService(db, userId: 9001);

            var scope = await accessService.GetCurrentAccessScopeAsync();

            Assert.False(scope.IsUnrestricted);
            Assert.True(scope.Includes("肿瘤"));
            Assert.True(scope.Includes("呼吸系统疾病"));
            Assert.False(scope.Includes("循环系统疾病"));
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public async Task Page_UserHasConfiguredScope_ReturnsOnlyMatchingProjectEnrollments()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            InitializeTables(db);
            SeedAuditRoles(db, userId: 9002, roleIds: [101]);
            db.Insertable(new SaveLifeEnrollmentAuditScope
            {
                Id = 24,
                RoleId = 101,
                DiseaseArea = "肿瘤"
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                new SaveLifeClinicalTrial
                {
                    Id = 201,
                    CdeRegistrationNumber = "CTR20262001",
                    DiseaseArea = "肿瘤",
                    Status = "recruiting",
                    Title = "肿瘤项目"
                },
                new SaveLifeClinicalTrial
                {
                    Id = 202,
                    CdeRegistrationNumber = "CTR20262002",
                    DiseaseArea = "呼吸系统疾病",
                    Status = "recruiting",
                    Title = "呼吸项目"
                }
            }).ExecuteCommand();
            db.Insertable(new[]
            {
                new SaveLifeEnrollmentRecord
                {
                    Id = 301,
                    ExternalUserId = "patient-1",
                    ProjectId = 201,
                    ProjectTitle = "肿瘤项目",
                    Status = "pending"
                },
                new SaveLifeEnrollmentRecord
                {
                    Id = 302,
                    ExternalUserId = "patient-2",
                    ProjectId = 202,
                    ProjectTitle = "呼吸项目",
                    Status = "pending"
                }
            }).ExecuteCommand();

            var userManager = CreateUserManager(9002);
            var accessService = CreateAccessService(db, userId: 9002, userManager);
            var service = new SaveLifeEnrollmentRecordService(
                Repository<SaveLifeEnrollmentRecord>(db),
                Repository<SaveLifeClinicalTrial>(db),
                Repository<SaveLifeUserProfile>(db),
                Repository<SaveLifePatientProfile>(db),
                null!,
                null!,
                null!,
                null!,
                null!,
                userManager,
                null!,
                accessService);

            var result = await service.Page(new SaveLifeEnrollmentRecordInput
            {
                Page = 1,
                PageSize = 10
            });

            var record = Assert.Single(result.Items);
            Assert.Equal(301, record.Id);
            Assert.Equal("肿瘤项目", record.ProjectTitle);
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    private static void SeedAuditRoles(SqlSugarClient db, long userId, IReadOnlyCollection<long> roleIds)
    {
        db.Insertable(new[]
        {
            new SysMenu
            {
                Id = 1,
                Permission = SaveLifeEnrollmentAuditScopeService.EnrollmentAuditPermission,
                Title = "审核",
                Type = MenuTypeEnum.Btn
            }
        }).ExecuteCommand();
        db.Insertable(new[]
        {
            new SysRole { Id = 101, Name = "肿瘤审核员", Status = StatusEnum.Enable },
            new SysRole { Id = 102, Name = "呼吸审核员", Status = StatusEnum.Enable },
            new SysRole { Id = 103, Name = "非审核角色", Status = StatusEnum.Enable }
        }).ExecuteCommand();
        db.Insertable(new[]
        {
            new SysRoleMenu { Id = 11, RoleId = 101, MenuId = 1 },
            new SysRoleMenu { Id = 12, RoleId = 102, MenuId = 1 }
        }).ExecuteCommand();
        db.Insertable(roleIds.Select((roleId, index) => new SysUserRole
        {
            Id = 100 + index,
            UserId = userId,
            RoleId = roleId
        }).ToList()).ExecuteCommand();
    }

    private static SaveLifeEnrollmentAuditScopeService CreateAccessService(
        ISqlSugarClient db,
        long userId,
        UserManager? userManager = null)
    {
        return new SaveLifeEnrollmentAuditScopeService(
            Repository<SaveLifeEnrollmentAuditScope>(db),
            Repository<SysUserRole>(db),
            Repository<SysRole>(db),
            Repository<SysRoleMenu>(db),
            Repository<SysMenu>(db),
            Repository<SaveLifeClinicalTrial>(db),
            userManager ?? CreateUserManager(userId),
            null!);
    }

    private static UserManager CreateUserManager(long userId)
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimConst.UserId, userId.ToString()),
                    new Claim(ClaimConst.AccountType, ((int)AccountTypeEnum.NormalUser).ToString())
                ],
                "test"))
        };
        return new UserManager(new HttpContextAccessor { HttpContext = context });
    }

    private static void InitializeTables(SqlSugarClient db)
    {
        db.CodeFirst.InitTables(
            typeof(SaveLifeEnrollmentAuditScope),
            typeof(SaveLifeClinicalTrial),
            typeof(SaveLifeEnrollmentRecord),
            typeof(SaveLifeUserProfile),
            typeof(SaveLifePatientProfile),
            typeof(SysUserRole),
            typeof(SysRole),
            typeof(SysRoleMenu),
            typeof(SysMenu));
    }

    private static SqlSugarRepository<T> Repository<T>(ISqlSugarClient db)
        where T : class, new()
    {
        return SqlSugarRepository<T>.CreateForContext(db);
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
