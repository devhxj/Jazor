namespace Jero.Plugin.SaveLife;

public static class SaveLifeSqlitePinnedColumnMigration
{
    private const string PinnedColumnName = "IsPinned";

    public static void Apply(ISqlSugarClient db)
    {
        if (db.CurrentConnectionConfig.DbType != DbType.Sqlite)
            return;

        EnsurePinnedColumn(db, "sl_clinical_trial", "index_sl_clinical_trial_P");
        EnsurePinnedColumn(db, "sl_agent_vector_index", "index_sl_agent_vector_index_P");
    }

    public static void EnsurePinnedColumn(ISqlSugarClient db, string tableName, string indexName)
    {
        if (db.CurrentConnectionConfig.DbType != DbType.Sqlite)
            return;

        var columns = db.DbMaintenance.GetColumnInfosByTableName(tableName, false) ?? new List<DbColumnInfo>();
        if (columns.Count == 0)
            return;

        var pinnedColumn = columns.FirstOrDefault(u => u.DbColumnName.Equals(PinnedColumnName, StringComparison.OrdinalIgnoreCase));
        if (pinnedColumn == null)
        {
            AddPinnedColumn(db, tableName);
            CreatePinnedIndex(db, tableName, indexName);
            return;
        }

        if (IsIntegerColumn(pinnedColumn))
        {
            db.Ado.ExecuteCommand($"UPDATE {Quote(tableName)} SET {Quote(PinnedColumnName)} = 0 WHERE {Quote(PinnedColumnName)} IS NULL");
            CreatePinnedIndex(db, tableName, indexName);
            return;
        }

        var legacyColumnName = ResolveLegacyColumnName(columns);
        db.Ado.ExecuteCommand($"DROP INDEX IF EXISTS {Quote(indexName)}");
        db.Ado.ExecuteCommand($"ALTER TABLE {Quote(tableName)} RENAME COLUMN {Quote(PinnedColumnName)} TO {Quote(legacyColumnName)}");
        AddPinnedColumn(db, tableName);
        db.Ado.ExecuteCommand($"""
            UPDATE {Quote(tableName)}
            SET {Quote(PinnedColumnName)} =
                CASE
                    WHEN {Quote(legacyColumnName)} IS NULL THEN 0
                    WHEN {Quote(legacyColumnName)} IN (1, '1', 'true', 'True', 'TRUE') THEN 1
                    ELSE 0
                END
            """);
        CreatePinnedIndex(db, tableName, indexName);
    }

    private static void AddPinnedColumn(ISqlSugarClient db, string tableName)
    {
        db.Ado.ExecuteCommand($"ALTER TABLE {Quote(tableName)} ADD COLUMN {Quote(PinnedColumnName)} INTEGER NOT NULL DEFAULT 0");
    }

    private static void CreatePinnedIndex(ISqlSugarClient db, string tableName, string indexName)
    {
        db.Ado.ExecuteCommand($"CREATE INDEX IF NOT EXISTS {Quote(indexName)} ON {Quote(tableName)}({Quote(PinnedColumnName)} DESC)");
    }

    private static bool IsIntegerColumn(DbColumnInfo column)
    {
        return string.Equals(column.DataType, "INTEGER", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveLegacyColumnName(IReadOnlyCollection<DbColumnInfo> columns)
    {
        const string baseName = "__IsPinnedLegacy";
        var candidate = baseName;
        var index = 1;
        while (columns.Any(u => u.DbColumnName.Equals(candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{baseName}{index++}";
        }

        return candidate;
    }

    private static string Quote(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
