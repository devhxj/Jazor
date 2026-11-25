using System.Linq.Expressions;
using System.Reflection;
using SqlSugar;
using Jero.Plugin.SaveLife;
using Jero.Plugin.SaveLife.Service;
using Xunit;

namespace Jero.Test.SaveLife;

public class SaveLifePublicKnowledgeBaseIndexQueryTests
{
    [Fact]
    public async Task KnowledgeProcessablePredicate_SelectAndClaim_ExecutesOnSqlite()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);

            var now = new DateTime(2026, 7, 8, 10, 0, 0);
            CreateDiseaseKnowledgeTable(db);
            InsertDiseaseKnowledge(
                db,
                1,
                isPublished: true,
                isInKnowledgeBase: false,
                now.AddMinutes(-10),
                now.AddMinutes(-9),
                nextRetryTime: null);
            InsertDiseaseKnowledge(
                db,
                2,
                isPublished: false,
                isInKnowledgeBase: true,
                now.AddMinutes(-8),
                now.AddMinutes(-7),
                nextRetryTime: now.AddMinutes(-1));
            InsertDiseaseKnowledge(
                db,
                3,
                isPublished: true,
                isInKnowledgeBase: true,
                now.AddMinutes(-6),
                now.AddMinutes(-5),
                nextRetryTime: null);
            InsertDiseaseKnowledge(
                db,
                4,
                isPublished: true,
                isInKnowledgeBase: false,
                now.AddMinutes(-4),
                now.AddMinutes(-3),
                nextRetryTime: now.AddMinutes(30));
            InsertDiseaseKnowledge(
                db,
                5,
                isPublished: true,
                isInKnowledgeBase: null,
                now.AddMinutes(-2),
                now.AddMinutes(-1),
                nextRetryTime: null);

            var predicate = BuildKnowledgePredicate(now);

            var ids = await db.Queryable<SaveLifeDiseaseKnowledge>()
                .Where(predicate)
                .OrderBy(u => u.UpdateTime, OrderByType.Asc)
                .OrderBy(u => u.CreateTime, OrderByType.Asc)
                .Select(u => u.Id)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(new long[] { 1, 2, 5 }, ids);

            var affected = await db.Updateable<SaveLifeDiseaseKnowledge>()
                .SetColumns(u => new SaveLifeDiseaseKnowledge
                {
                    KnowledgeBaseLastAttemptTime = now,
                    KnowledgeBaseNextRetryTime = now.AddMinutes(30)
                })
                .Where(u => u.Id == 1)
                .Where(predicate)
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);

            Assert.Equal(1, affected);

            var nullStateAffected = await db.Updateable<SaveLifeDiseaseKnowledge>()
                .SetColumns(u => new SaveLifeDiseaseKnowledge
                {
                    KnowledgeBaseLastAttemptTime = now,
                    KnowledgeBaseNextRetryTime = now.AddMinutes(30)
                })
                .Where(u => u.Id == 5)
                .Where(predicate)
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);

            Assert.Equal(1, nullStateAffected);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    [Fact]
    public async Task TrialProcessablePredicate_SelectAndClaim_ExecutesOnSqlite()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.db");
        try
        {
            using var db = CreateSqliteClient(dbPath);

            var now = new DateTime(2026, 7, 8, 10, 0, 0);
            CreateClinicalTrialTable(db);
            InsertClinicalTrial(
                db,
                11,
                "recruiting",
                isInKnowledgeBase: false,
                now.AddMinutes(-10),
                now.AddMinutes(-9),
                nextRetryTime: null);
            InsertClinicalTrial(
                db,
                12,
                "completed",
                isInKnowledgeBase: false,
                now.AddMinutes(-8),
                now.AddMinutes(-7),
                nextRetryTime: now.AddMinutes(-1));
            InsertClinicalTrial(
                db,
                13,
                "recruiting",
                isInKnowledgeBase: true,
                now.AddMinutes(-6),
                now.AddMinutes(-5),
                nextRetryTime: null);
            InsertClinicalTrial(
                db,
                14,
                "recruiting",
                isInKnowledgeBase: false,
                now.AddMinutes(-4),
                now.AddMinutes(-3),
                nextRetryTime: now.AddMinutes(30));
            InsertClinicalTrial(
                db,
                15,
                "recruiting",
                isInKnowledgeBase: null,
                now.AddMinutes(-2),
                now.AddMinutes(-1),
                nextRetryTime: null);

            var predicate = BuildTrialPredicate(now);

            var ids = await db.Queryable<SaveLifeClinicalTrial>()
                .Where(predicate)
                .OrderBy(u => u.UpdateTime, OrderByType.Asc)
                .OrderBy(u => u.CreateTime, OrderByType.Asc)
                .Select(u => u.Id)
                .ToListAsync(TestContext.Current.CancellationToken);

            Assert.Equal(new long[] { 11, 12, 15 }, ids);

            var affected = await db.Updateable<SaveLifeClinicalTrial>()
                .SetColumns(u => new SaveLifeClinicalTrial
                {
                    KnowledgeBaseLastAttemptTime = now,
                    KnowledgeBaseNextRetryTime = now.AddMinutes(30)
                })
                .Where(u => u.Id == 11)
                .Where(predicate)
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);

            Assert.Equal(1, affected);

            var nullStateAffected = await db.Updateable<SaveLifeClinicalTrial>()
                .SetColumns(u => new SaveLifeClinicalTrial
                {
                    KnowledgeBaseLastAttemptTime = now,
                    KnowledgeBaseNextRetryTime = now.AddMinutes(30)
                })
                .Where(u => u.Id == 15)
                .Where(predicate)
                .ExecuteCommandAsync(TestContext.Current.CancellationToken);

            Assert.Equal(1, nullStateAffected);
        }
        finally
        {
            if (File.Exists(dbPath))
                File.Delete(dbPath);
        }
    }

    private static SqlSugarClient CreateSqliteClient(string dbPath)
    {
        return new SqlSugarClient(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath};Pooling=False",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true
        });
    }

    private static Expression<Func<SaveLifeDiseaseKnowledge, bool>> BuildKnowledgePredicate(DateTime now)
    {
        return BuildPredicate<SaveLifeDiseaseKnowledge>("BuildKnowledgeProcessablePredicate", now);
    }

    private static Expression<Func<SaveLifeClinicalTrial, bool>> BuildTrialPredicate(DateTime now)
    {
        return BuildPredicate<SaveLifeClinicalTrial>("BuildTrialProcessablePredicate", now);
    }

    private static void CreateDiseaseKnowledgeTable(SqlSugarClient db)
    {
        db.Ado.ExecuteCommand("""
            CREATE TABLE sl_disease_knowledge (
                Id bigint NOT NULL PRIMARY KEY,
                IsPublished INTEGER NOT NULL,
                IsInKnowledgeBase INTEGER NULL,
                KnowledgeBaseError varchar(512) NULL,
                KnowledgeBaseLastAttemptTime datetime NULL,
                KnowledgeBaseNextRetryTime datetime NULL,
                CreateTime datetime NOT NULL,
                UpdateTime datetime NULL
            );
            """);
    }

    private static void CreateClinicalTrialTable(SqlSugarClient db)
    {
        db.Ado.ExecuteCommand("""
            CREATE TABLE sl_clinical_trial (
                Id bigint NOT NULL PRIMARY KEY,
                Status varchar(32) NOT NULL,
                IsInKnowledgeBase INTEGER NULL,
                KnowledgeBaseError varchar(512) NULL,
                KnowledgeBaseLastAttemptTime datetime NULL,
                KnowledgeBaseNextRetryTime datetime NULL,
                CreateTime datetime NOT NULL,
                UpdateTime datetime NULL
            );
            """);
    }

    private static void InsertDiseaseKnowledge(
        SqlSugarClient db,
        long id,
        bool isPublished,
        bool? isInKnowledgeBase,
        DateTime createTime,
        DateTime updateTime,
        DateTime? nextRetryTime)
    {
        db.Ado.ExecuteCommand(
            """
            INSERT INTO sl_disease_knowledge
                (Id, IsPublished, IsInKnowledgeBase, KnowledgeBaseNextRetryTime, CreateTime, UpdateTime)
            VALUES
                (@Id, @IsPublished, @IsInKnowledgeBase, @KnowledgeBaseNextRetryTime, @CreateTime, @UpdateTime);
            """,
            new SugarParameter("@Id", id),
            new SugarParameter("@IsPublished", isPublished ? 1 : 0),
            new SugarParameter("@IsInKnowledgeBase", isInKnowledgeBase.HasValue ? (isInKnowledgeBase.Value ? 1 : 0) : null),
            new SugarParameter("@KnowledgeBaseNextRetryTime", nextRetryTime),
            new SugarParameter("@CreateTime", createTime),
            new SugarParameter("@UpdateTime", updateTime));
    }

    private static void InsertClinicalTrial(
        SqlSugarClient db,
        long id,
        string status,
        bool? isInKnowledgeBase,
        DateTime createTime,
        DateTime updateTime,
        DateTime? nextRetryTime)
    {
        db.Ado.ExecuteCommand(
            """
            INSERT INTO sl_clinical_trial
                (Id, Status, IsInKnowledgeBase, KnowledgeBaseNextRetryTime, CreateTime, UpdateTime)
            VALUES
                (@Id, @Status, @IsInKnowledgeBase, @KnowledgeBaseNextRetryTime, @CreateTime, @UpdateTime);
            """,
            new SugarParameter("@Id", id),
            new SugarParameter("@Status", status),
            new SugarParameter("@IsInKnowledgeBase", isInKnowledgeBase.HasValue ? (isInKnowledgeBase.Value ? 1 : 0) : null),
            new SugarParameter("@KnowledgeBaseNextRetryTime", nextRetryTime),
            new SugarParameter("@CreateTime", createTime),
            new SugarParameter("@UpdateTime", updateTime));
    }

    private static Expression<Func<T, bool>> BuildPredicate<T>(string methodName, DateTime now)
    {
        var method = typeof(SaveLifePublicKnowledgeBaseIndexProcessor)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!;
        return (Expression<Func<T, bool>>)method.Invoke(null, new object[] { now })!;
    }
}
