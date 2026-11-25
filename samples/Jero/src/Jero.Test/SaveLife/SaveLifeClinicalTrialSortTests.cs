using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifeClinicalTrialSortTests
{
    [Fact]
    public void GetPinnedRank_WithPinnedTrial_ReturnsHigherPriorityRank()
    {
        var pinnedRank = SaveLifeClinicalTrialSort.GetPinnedRank(true);
        var normalRank = SaveLifeClinicalTrialSort.GetPinnedRank(false);

        Assert.True(pinnedRank < normalRank);
    }

    [Fact]
    public void OrderByPinnedFirst_OrdersEachPinnedGroupByCdeRegistrationNumberDescending()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={dbPath};Pooling=False",
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            });
            db.Ado.ExecuteCommand("""
                CREATE TABLE sl_clinical_trial (
                    Id INTEGER NOT NULL PRIMARY KEY,
                    IsPinned INTEGER NOT NULL,
                    CdeRegistrationNumber TEXT NOT NULL UNIQUE,
                    SortOrder INTEGER NOT NULL,
                    CreateTime TEXT NOT NULL
                );
                INSERT INTO sl_clinical_trial VALUES (1, 0, 'CTR20260030', 900, '2026-01-01');
                INSERT INTO sl_clinical_trial VALUES (2, 1, 'CTR20260010', 1, '2026-01-02');
                INSERT INTO sl_clinical_trial VALUES (3, 1, 'CTR20260040', 999, '2026-01-03');
                INSERT INTO sl_clinical_trial VALUES (4, 0, 'CTR20260020', 0, '2026-01-04');
                """);

            var orderedIds = db.Queryable<SaveLifeClinicalTrial>()
                .OrderByPinnedFirst()
                .Select(u => u.Id)
                .ToList();

            Assert.Equal([3L, 2L, 1L, 4L], orderedIds);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void CodeFirst_AddsPinnedColumnToExistingRows_WithBindableDefault()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using (var db = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={dbPath};Pooling=False",
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            }))
            {
                db.Ado.ExecuteCommand("""
                    CREATE TABLE sl_trial_sort_test (
                        Id bigint NOT NULL PRIMARY KEY,
                        Title varchar(256) NOT NULL
                    );
                    """);
                db.Ado.ExecuteCommand("INSERT INTO sl_trial_sort_test (Id, Title) VALUES (1, 'existing');");

                db.CodeFirst.InitTables(typeof(ClinicalTrialSortTestEntity));

                var trial = db.Queryable<ClinicalTrialSortTestEntity>()
                    .OrderBy(u => u.IsPinned, OrderByType.Desc)
                    .First();

                Assert.False(trial.IsPinned);
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public void Migration_UpdatesPreviousPinnedBitColumn_ToBindableIntegerColumn()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using (var db = new SqlSugarClient(new ConnectionConfig
            {
                DbType = DbType.Sqlite,
                ConnectionString = $"DataSource={dbPath};Pooling=False",
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = true
            }))
            {
                db.Ado.ExecuteCommand("""
                    CREATE TABLE sl_trial_sort_test (
                        Id bigint NOT NULL PRIMARY KEY,
                        Title varchar(256) NOT NULL
                    );
                    """);
                db.Ado.ExecuteCommand("INSERT INTO sl_trial_sort_test (Id, Title) VALUES (1, 'existing');");
                db.CodeFirst.InitTables(typeof(ClinicalTrialSortTestEntityWithBitPinned));

                SaveLifeSqlitePinnedColumnMigration.EnsurePinnedColumn(
                    db,
                    "sl_trial_sort_test",
                    "index_sl_trial_sort_test_P");

                var trial = db.Queryable<ClinicalTrialSortTestEntity>()
                    .OrderBy(u => u.IsPinned, OrderByType.Desc)
                    .First();

                Assert.False(trial.IsPinned);
                var pinnedColumn = db.DbMaintenance.GetColumnInfosByTableName("sl_trial_sort_test", false)
                    .Single(u => u.DbColumnName == "IsPinned");
                Assert.Equal("INTEGER", pinnedColumn.DataType, ignoreCase: true);
            }
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [SugarTable("sl_trial_sort_test")]
    private sealed class ClinicalTrialSortTestEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        [SugarColumn(ColumnDataType = "INTEGER", DefaultValue = "0")]
        public bool IsPinned { get; set; }
    }

    [SugarTable("sl_trial_sort_test")]
    private sealed class ClinicalTrialSortTestEntityWithBitPinned
    {
        [SugarColumn(IsPrimaryKey = true)]
        public long Id { get; set; }

        public string Title { get; set; } = string.Empty;

        [SugarColumn(DefaultValue = "0")]
        public bool IsPinned { get; set; }
    }

}
