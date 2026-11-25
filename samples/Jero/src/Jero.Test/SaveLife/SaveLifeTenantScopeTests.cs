// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Reflection;
using SqlSugar;
using Jero.Core;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeTenantScopeTests
{
    private const long CurrentTenantId = 1300000000001;
    private const long OtherTenantId = CurrentTenantId + 1;

    [Fact]
    public void PersistentEntities_DoNotImplementTenantFilter()
    {
        var entityTypes = typeof(SaveLifeEntityBaseDel).Assembly
            .GetTypes()
            .Where(type =>
                type.Namespace == typeof(SaveLifeEntityBaseDel).Namespace &&
                type.IsClass &&
                !type.IsAbstract &&
                type.Name.StartsWith("SaveLife", StringComparison.Ordinal) &&
                typeof(EntityBase).IsAssignableFrom(type))
            .OrderBy(type => type.Name)
            .ToArray();

        Assert.Equal(24, entityTypes.Length);
        Assert.All(entityTypes, type =>
        {
            Assert.True(
                typeof(SaveLifeEntityBaseDel).IsAssignableFrom(type),
                $"{type.FullName} must use the SaveLife global-data entity base class.");
            Assert.False(
                typeof(ITenantIdFilter).IsAssignableFrom(type),
                $"{type.FullName} must not participate in the platform tenant filter.");
        });
    }

    [Fact]
    public void EnrollmentRecords_AreNotFilteredByCurrentTenant()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SaveLifeEnrollmentRecord));
            db.Insertable(new[]
            {
                Enrollment(101, CurrentTenantId),
                Enrollment(102, OtherTenantId),
                Enrollment(103, null)
            }).ExecuteCommand();

            db.QueryFilter.AddTableFilter<ITenantIdFilter>(
                item => item.TenantId == CurrentTenantId);

            var records = db.Queryable<SaveLifeEnrollmentRecord>()
                .OrderBy(item => item.Id)
                .ToList();

            Assert.Equal(new long[] { 101, 102, 103 }, records.Select(item => item.Id));
        }
        finally
        {
            DeleteDatabase(dbPath);
        }
    }

    [Fact]
    public void EnrollmentMenu_UsesEnrollmentRecordPermissions()
    {
        var menus = new Jero.Plugin.SaveLife.SysMenuSeedData().HasData().ToList();

        Assert.Equal(
            "saveLifeEnrollmentRecord:page",
            menus.Single(menu => menu.Id == 1310700020201).Permission);
        Assert.Equal(
            "saveLifeEnrollmentRecord:update",
            menus.Single(menu => menu.Id == 1310700020301).Permission);
        Assert.Equal(
            "saveLifeEnrollmentRecord:audit",
            menus.Single(menu => menu.Id == 1310700020401).Permission);
        Assert.Equal(
            "saveLifeEnrollmentRecord:delete",
            menus.Single(menu => menu.Id == 1310700020501).Permission);
        Assert.Equal(
            "saveLifeEnrollmentRecord:manageAuditScope",
            menus.Single(menu => menu.Id == 1310700020701).Permission);

        var permission = typeof(SaveLifeEnrollmentRecordService)
            .GetField("EnrollmentPagePermission", BindingFlags.Static | BindingFlags.NonPublic)
            ?.GetRawConstantValue() as string;
        Assert.Equal("saveLifeEnrollmentRecord:page", permission);
    }

    private static SaveLifeEnrollmentRecord Enrollment(long id, long? tenantId) => new()
    {
        Id = id,
        TenantId = tenantId,
        ExternalUserId = $"user-{id}",
        ProjectId = id,
        ProjectTitle = $"project-{id}",
        Status = "pending",
        CreateTime = new DateTime(2026, 8, 21)
    };

    private static SqlSugarClient CreateSqliteClient(string dbPath) => new(new ConnectionConfig
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
