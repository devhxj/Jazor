using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Database;

/// <summary>库表管理 API 与浏览器 JSON 边界。</summary>
[ECMAScriptModule("components/database-client.js")]
public static class DatabaseClient
{
    public static IPromise<ApiOutcome> Databases()
        => BrowserApiClient.Get("/api/sysDatabase/list");

    public static IPromise<ApiOutcome> Tables(string configId)
        => BrowserApiClient.Get("/api/sysDatabase/tableList/" + EncodeURIComponent(configId));

    public static IPromise<ApiOutcome> Columns(string tableName, string configId)
        => BrowserApiClient.Get(
            "/api/sysDatabase/columnList/" + EncodeURIComponent(tableName) + "/" + EncodeURIComponent(configId));

    public static IPromise<ApiOutcome> DatabaseTypes(string configId)
        => BrowserApiClient.Get("/api/sysDatabase/dbTypeList/" + EncodeURIComponent(configId));

    public static IPromise<ApiOutcome> Visual(string configId)
        => BrowserApiClient.Get("/api/sysDatabase/visualDbTable/" + EncodeURIComponent(configId));

    // DDL boundary / 副作用边界：这些请求会直接修改真实数据库结构，
    // 浏览器验收必须先安装 route mock，不能命中真实后端。
    public static IPromise<ApiOutcome> AddColumn(DatabaseAddColumnInput input)
        => BrowserApiClient.Post("/api/sysDatabase/addColumn", input);

    public static IPromise<ApiOutcome> UpdateColumn(DatabaseUpdateColumnInput input)
        => BrowserApiClient.Post("/api/sysDatabase/updateColumn", input);

    public static IPromise<ApiOutcome> DeleteColumn(DatabaseDeleteColumnInput input)
        => BrowserApiClient.Post("/api/sysDatabase/deleteColumn", input);

    public static IPromise<ApiOutcome> MoveColumn(DatabaseMoveColumnInput input)
        => BrowserApiClient.Post("/api/sysDatabase/moveColumn", input);

    public static IPromise<ApiOutcome> AddTable(DatabaseAddTableInput input)
        => BrowserApiClient.Post("/api/sysDatabase/addTable", input);

    public static IPromise<ApiOutcome> UpdateTable(DatabaseUpdateTableInput input)
        => BrowserApiClient.Post("/api/sysDatabase/updateTable", input);

    public static IPromise<ApiOutcome> DeleteTable(DatabaseDeleteTableInput input)
        => BrowserApiClient.Post("/api/sysDatabase/deleteTable", input);

    public static DatabaseOptionView[] ReadDatabases(object? data)
    {
        var values = ReadArray(data);
        var result = new DatabaseOptionView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DatabaseOptionView
            {
                ConfigId = ReadString(value, "configId"),
                DbNickName = ReadString(value, "dbNickName")
            };
        }
        return result;
    }

    public static DatabaseTableView[] ReadTables(object? data)
    {
        var values = ReadArray(data);
        var count = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var name = ReadString(values[index]!, "name");
            if (name.Length > 0 && !name.StartsWith("jero_"))
                count++;
        }

        var result = new DatabaseTableView[count];
        var targetIndex = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var name = ReadString(value, "name");
            if (name.Length == 0 || name.StartsWith("jero_"))
                continue;
            result[targetIndex] = new DatabaseTableView
            {
                Name = name,
                Description = ReadString(value, "description")
            };
            targetIndex++;
        }
        return result;
    }

    public static DatabaseColumnView[] ReadColumns(object? data)
    {
        var values = ReadArray(data);
        var result = new DatabaseColumnView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DatabaseColumnView
            {
                TableName = ReadString(value, "tableName"),
                TableId = ReadInt(value, "tableId", 0),
                DbColumnName = ReadString(value, "dbColumnName"),
                PropertyName = ReadString(value, "propertyName"),
                DataType = ReadString(value, "dataType"),
                Length = ReadInt(value, "length", 0),
                ColumnDescription = ReadString(value, "columnDescription"),
                DefaultValue = ReadString(value, "defaultValue"),
                IsNullable = ReadBool(value, "isNullable", false),
                IsIdentity = ReadBool(value, "isIdentity", false),
                IsPrimarykey = ReadBool(value, "isPrimarykey", false),
                DecimalDigits = ReadInt(value, "decimalDigits", 0),
                Scale = ReadInt(value, "scale", 0),
                IsArray = ReadBool(value, "isArray", false),
                IsJson = ReadBool(value, "isJson", false),
                IsUnsigned = ReadBool(value, "isUnsigned", false),
                CreateTableFieldSort = ReadInt(value, "createTableFieldSort", index)
            };
        }
        return result;
    }

    public static string[] ReadStrings(object? data)
    {
        var values = ReadArray(data);
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = NormalizeString(values[index]);
        return result;
    }

    public static DatabaseVisualView ReadVisual(object? data)
    {
        if (data is null)
            return new DatabaseVisualView();

        var tableValues = ReadArray(Reflect.Get(data, "visualTableList"));
        var tables = new DatabaseVisualTableView[tableValues.Length];
        for (var index = 0; index < tableValues.Length; index++)
        {
            var value = tableValues[index]!;
            tables[index] = new DatabaseVisualTableView
            {
                TableName = ReadString(value, "tableName"),
                TableComents = ReadString(value, "tableComents"),
                X = ReadInt(value, "x", 0),
                Y = ReadInt(value, "y", 0)
            };
        }

        var columnValues = ReadArray(Reflect.Get(data, "visualColumnList"));
        var columns = new DatabaseVisualColumnView[columnValues.Length];
        for (var index = 0; index < columnValues.Length; index++)
        {
            var value = columnValues[index]!;
            columns[index] = new DatabaseVisualColumnView
            {
                TableName = ReadString(value, "tableName"),
                ColumnName = ReadString(value, "columnName"),
                DataType = ReadString(value, "dataType"),
                DataLength = ReadString(value, "dataLength"),
                ColumnDescription = ReadString(value, "columnDescription")
            };
        }

        var relationValues = ReadArray(Reflect.Get(data, "columnRelationList"));
        var relations = new DatabaseColumnRelationView[relationValues.Length];
        for (var index = 0; index < relationValues.Length; index++)
        {
            var value = relationValues[index]!;
            relations[index] = new DatabaseColumnRelationView
            {
                SourceTableName = ReadString(value, "sourceTableName"),
                SourceColumnName = ReadString(value, "sourceColumnName"),
                Type = ReadString(value, "type"),
                TargetTableName = ReadString(value, "targetTableName"),
                TargetColumnName = ReadString(value, "targetColumnName")
            };
        }

        return new DatabaseVisualView
        {
            Tables = tables,
            Columns = columns,
            Relations = relations
        };
    }

    private static object?[] ReadArray(object? value)
        => value as object?[] ?? [];

    private static string ReadString(object data, string key)
        => NormalizeString(Reflect.Get(data, key));

    private static string NormalizeString(object? value)
    {
        if (value is null)
            return string.Empty;
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static bool ReadBool(object data, string key, bool fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : BooleanValue(value);
    }
}

public sealed record DatabaseOptionView : VueProps
{
    public string ConfigId { get; init; } = string.Empty;
    public string DbNickName { get; init; } = string.Empty;
}

public sealed record DatabaseTableView : VueProps
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public sealed record DatabaseAddColumnInput
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
    [ECMAScriptName("dbColumnName")]
    public string DbColumnName { get; init; } = string.Empty;
    [ECMAScriptName("dataType")]
    public string DataType { get; init; } = string.Empty;
    [ECMAScriptName("length")]
    public int Length { get; init; }
    [ECMAScriptName("columnDescription")]
    public string ColumnDescription { get; init; } = string.Empty;
    [ECMAScriptName("isNullable")]
    public int IsNullable { get; init; }
    [ECMAScriptName("isIdentity")]
    public int IsIdentity { get; init; }
    [ECMAScriptName("isPrimarykey")]
    public int IsPrimarykey { get; init; }
    [ECMAScriptName("decimalDigits")]
    public int DecimalDigits { get; init; }
    [ECMAScriptName("defaultValue")]
    public string DefaultValue { get; init; } = string.Empty;
}

public sealed record DatabaseUpdateColumnInput
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
    [ECMAScriptName("columnName")]
    public string ColumnName { get; init; } = string.Empty;
    [ECMAScriptName("oldColumnName")]
    public string OldColumnName { get; init; } = string.Empty;
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
    [ECMAScriptName("defaultValue")]
    public string DefaultValue { get; init; } = string.Empty;
}

public sealed record DatabaseDeleteColumnInput
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
    [ECMAScriptName("dbColumnName")]
    public string DbColumnName { get; init; } = string.Empty;
}

public sealed record DatabaseMoveColumnInput
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
    [ECMAScriptName("columnName")]
    public string ColumnName { get; init; } = string.Empty;
    [ECMAScriptName("afterColumnName")]
    public string AfterColumnName { get; init; } = string.Empty;
}

public sealed record DatabaseAddTableInput
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
    [ECMAScriptName("dbColumnInfoList")]
    public DatabaseAddColumnInput[] DbColumnInfoList { get; init; } = [];
}

public sealed record DatabaseUpdateTableInput
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
    [ECMAScriptName("oldTableName")]
    public string OldTableName { get; init; } = string.Empty;
    [ECMAScriptName("description")]
    public string Description { get; init; } = string.Empty;
}

public sealed record DatabaseDeleteTableInput
{
    [ECMAScriptName("configId")]
    public string ConfigId { get; init; } = string.Empty;
    [ECMAScriptName("tableName")]
    public string TableName { get; init; } = string.Empty;
}

public sealed record DatabaseVisualTableView : VueProps
{
    public string TableName { get; init; } = string.Empty;
    public string TableComents { get; init; } = string.Empty;
    public int X { get; init; }
    public int Y { get; init; }
}

public sealed record DatabaseVisualColumnView : VueProps
{
    public string TableName { get; init; } = string.Empty;
    public string ColumnName { get; init; } = string.Empty;
    public string DataType { get; init; } = string.Empty;
    public string DataLength { get; init; } = string.Empty;
    public string ColumnDescription { get; init; } = string.Empty;
}

public sealed record DatabaseColumnRelationView : VueProps
{
    public string SourceTableName { get; init; } = string.Empty;
    public string SourceColumnName { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string TargetTableName { get; init; } = string.Empty;
    public string TargetColumnName { get; init; } = string.Empty;
}

public sealed record DatabaseVisualView
{
    public DatabaseVisualTableView[] Tables { get; init; } = [];
    public DatabaseVisualColumnView[] Columns { get; init; } = [];
    public DatabaseColumnRelationView[] Relations { get; init; } = [];
}
