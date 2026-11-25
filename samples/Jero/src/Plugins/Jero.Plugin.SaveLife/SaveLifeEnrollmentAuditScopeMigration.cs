namespace Jero.Plugin.SaveLife;

public static class SaveLifeEnrollmentAuditScopeMigration
{
    public static void EnsureSchema(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);
        db.CodeFirst.InitTables(typeof(SaveLifeEnrollmentAuditScope));
    }
}
