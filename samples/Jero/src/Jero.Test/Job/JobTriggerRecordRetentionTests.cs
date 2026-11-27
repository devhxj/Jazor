using System.Reflection;
using SqlSugar;
using Jero.Core;
using Jero.Core.Service;
using Xunit;

namespace Jero.Test.Job;

[Collection("SqlSugar tenant tests")]
public class JobTriggerRecordRetentionTests
{
    [Fact]
    public async Task ClearJobTriggerRecord_SnowflakeTable_DeletesRowsWithoutIdentitySequence()
    {
        var root = TestWorkspace.Create("jobs");
        var dbPath = Path.Combine(root, "records.db");
        var tenantField = typeof(SqlSugarSetup)
            .GetField("_tenant", BindingFlags.NonPublic | BindingFlags.Static)!;
        var originalTenant = tenantField.GetValue(null);
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SysJobTriggerRecord));
            var sequenceTableCount = db.Ado.SqlQuery<int>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = 'sqlite_sequence'")
                .Single();
            Assert.Equal(0, sequenceTableCount);
            await db.Insertable(CreateRecord(1, "clear-job", "clear-trigger"))
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);

            using var tenant = CreateTenant(dbPath);
            SqlSugarSetup.ITenant = tenant;
            var service = CreateService();

            service.ClearJobTriggerRecord();

            var remaining = await db.Queryable<SysJobTriggerRecord>()
                .CountAsync(TestContext.Current.CancellationToken);
            Assert.Equal(0, remaining);
        }
        finally
        {
            tenantField.SetValue(null, originalTenant);
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ClearExpireJobTriggerRecord_MoreThanLimit_KeepsNewestRecordsForTrigger()
    {
        var root = TestWorkspace.Create("jobs");
        var dbPath = Path.Combine(root, "records.db");
        var tenantField = typeof(SqlSugarSetup)
            .GetField("_tenant", BindingFlags.NonPublic | BindingFlags.Static)!;
        var originalTenant = tenantField.GetValue(null);
        try
        {
            using var db = CreateSqliteClient(dbPath);
            db.CodeFirst.InitTables(typeof(SysJobTriggerRecord));

            var records = Enumerable.Range(1, 31)
                .Select(number => CreateRecord(number, "target-job", "target-trigger"))
                .Append(CreateRecord(1, "target-job", "other-trigger", idOffset: 100))
                .ToList();
            await db.Insertable(records).ExecuteCommandAsync(TestContext.Current.CancellationToken);

            using var tenant = CreateTenant(dbPath);
            SqlSugarSetup.ITenant = tenant;
            var service = CreateService();
            await service.ClearExpireJobTriggerRecord(new SysJobTriggerRecord
            {
                JobId = "target-job",
                TriggerId = "target-trigger"
            });

            var retainedRuns = await db.Queryable<SysJobTriggerRecord>()
                .Where(u => u.JobId == "target-job" && u.TriggerId == "target-trigger")
                .OrderBy(u => u.NumberOfRuns)
                .Select(u => u.NumberOfRuns)
                .ToListAsync(TestContext.Current.CancellationToken);
            var otherTriggerCount = await db.Queryable<SysJobTriggerRecord>()
                .CountAsync(u => u.JobId == "target-job" && u.TriggerId == "other-trigger",
                    TestContext.Current.CancellationToken);

            Assert.Equal(Enumerable.Range(2, 30).Select(number => (long)number), retainedRuns);
            Assert.Equal(1, otherTriggerCount);
        }
        finally
        {
            tenantField.SetValue(null, originalTenant);
            Directory.Delete(root, recursive: true);
        }
    }

    private static SysJobTriggerRecord CreateRecord(
        int number,
        string jobId,
        string triggerId,
        long idOffset = 0)
    {
        var runTime = new DateTime(2026, 7, 12, 0, 0, 0).AddMinutes(number);
        return new SysJobTriggerRecord
        {
            Id = idOffset + number,
            JobId = jobId,
            TriggerId = triggerId,
            NumberOfRuns = number,
            LastRunTime = runTime,
            CreatedTime = runTime
        };
    }

    private static SysJobService CreateService()
    {
        return new SysJobService(
            new SqlSugarRepository<SysJobDetail>(),
            new SqlSugarRepository<SysJobTrigger>(),
            new SqlSugarRepository<SysJobTriggerRecord>(),
            new SqlSugarRepository<SysJobCluster>(),
            null!,
            null!);
    }

    private static SqlSugarScope CreateTenant(string dbPath)
    {
        return new SqlSugarScope(new ConnectionConfig
        {
            ConfigId = SqlSugarConst.MainConfigId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath};Pooling=False",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true
        });
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
}

[CollectionDefinition("SqlSugar tenant tests", DisableParallelization = true)]
public sealed class SqlSugarTenantTestCollection;
