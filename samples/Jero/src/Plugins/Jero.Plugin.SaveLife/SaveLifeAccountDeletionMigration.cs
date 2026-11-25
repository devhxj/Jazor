namespace Jero.Plugin.SaveLife;

public static class SaveLifeAccountDeletionMigration
{
    public static void EnsureSchema(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);
        db.CodeFirst.InitTables(typeof(SaveLifeAccountDeletionRecord));
    }
}
