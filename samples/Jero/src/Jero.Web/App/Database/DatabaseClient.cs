using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Database;

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

    public static IPromise<ApiOutcome> ApplicationNamespaces()
        => BrowserApiClient.Get("/api/sysCodeGen/applicationNamespaces");

    public static IPromise<ApiOutcome> EntityBaseClasses()
        => BrowserApiClient.Get(
            "/api/sysDictType/dataList?code=" + EncodeURIComponent("code_gen_base_class"));

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

    // File-generation boundary / 文件生成边界：后端会在源码目录写入实体或种子文件。
    // 这里只表达旧页面已有的客户端请求契约，验收时同样必须 route mock。
    public static IPromise<ApiOutcome> CreateEntity(DatabaseCreateEntityInput input)
        => BrowserApiClient.Post("/api/sysDatabase/createEntity", input);

    public static IPromise<ApiOutcome> CreateSeedData(DatabaseCreateSeedDataInput input)
        => BrowserApiClient.Post("/api/sysDatabase/createSeedData", input);

    public static DatabaseOptionView[] ReadDatabases(object? data)
    {
        var values = ReadArray(data);
        var result = new DatabaseOptionView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DatabaseOptionView
            {
                configId = ReadString(value, "configId"),
                dbNickName = ReadString(value, "dbNickName")
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
                name = name,
                description = ReadString(value, "description")
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
                tableName = ReadString(value, "tableName"),
                tableId = ReadInt(value, "tableId", 0),
                dbColumnName = ReadString(value, "dbColumnName"),
                propertyName = ReadString(value, "propertyName"),
                dataType = ReadString(value, "dataType"),
                length = ReadInt(value, "length", 0),
                columnDescription = ReadString(value, "columnDescription"),
                defaultValue = ReadString(value, "defaultValue"),
                isNullable = ReadBool(value, "isNullable", false),
                isIdentity = ReadBool(value, "isIdentity", false),
                isPrimarykey = ReadBool(value, "isPrimarykey", false),
                decimalDigits = ReadInt(value, "decimalDigits", 0),
                scale = ReadInt(value, "scale", 0),
                isArray = ReadBool(value, "isArray", false),
                isJson = ReadBool(value, "isJson", false),
                isUnsigned = ReadBool(value, "isUnsigned", false),
                createTableFieldSort = ReadInt(value, "createTableFieldSort", index)
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

    public static DatabaseNamedOption[] ReadNamedOptions(object? data)
    {
        var values = ReadArray(data);
        var result = new DatabaseNamedOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DatabaseNamedOption
            {
                label = ReadString(value, "label"),
                value = ReadString(value, "value")
            };
        }
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
                tableName = ReadString(value, "tableName"),
                tableComents = ReadString(value, "tableComents"),
                x = ReadInt(value, "x", 0),
                y = ReadInt(value, "y", 0)
            };
        }

        var columnValues = ReadArray(Reflect.Get(data, "visualColumnList"));
        var columns = new DatabaseVisualColumnView[columnValues.Length];
        for (var index = 0; index < columnValues.Length; index++)
        {
            var value = columnValues[index]!;
            columns[index] = new DatabaseVisualColumnView
            {
                tableName = ReadString(value, "tableName"),
                columnName = ReadString(value, "columnName"),
                dataType = ReadString(value, "dataType"),
                dataLength = ReadString(value, "dataLength"),
                columnDescription = ReadString(value, "columnDescription")
            };
        }

        var relationValues = ReadArray(Reflect.Get(data, "columnRelationList"));
        var relations = new DatabaseColumnRelationView[relationValues.Length];
        for (var index = 0; index < relationValues.Length; index++)
        {
            var value = relationValues[index]!;
            relations[index] = new DatabaseColumnRelationView
            {
                sourceTableName = ReadString(value, "sourceTableName"),
                sourceColumnName = ReadString(value, "sourceColumnName"),
                type = ReadString(value, "type"),
                targetTableName = ReadString(value, "targetTableName"),
                targetColumnName = ReadString(value, "targetColumnName")
            };
        }

        return new DatabaseVisualView
        {
            tables = tables,
            columns = columns,
            relations = relations
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
    public string configId { get; init; } = string.Empty;
    public string dbNickName { get; init; } = string.Empty;
}

public sealed record DatabaseTableView : VueProps
{
    public string name { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
}

public sealed record DatabaseNamedOption : VueProps
{
    public string label { get; init; } = string.Empty;
    public string value { get; init; } = string.Empty;
}

public sealed record DatabaseAddColumnInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string dbColumnName { get; init; } = string.Empty;
    public string dataType { get; init; } = string.Empty;
    public int length { get; init; }
    public string columnDescription { get; init; } = string.Empty;
    public int isNullable { get; init; }
    public int isIdentity { get; init; }
    public int isPrimarykey { get; init; }
    public int decimalDigits { get; init; }
    public string defaultValue { get; init; } = string.Empty;
}

public sealed record DatabaseUpdateColumnInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string columnName { get; init; } = string.Empty;
    public string oldColumnName { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
    public string defaultValue { get; init; } = string.Empty;
}

public sealed record DatabaseDeleteColumnInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string dbColumnName { get; init; } = string.Empty;
}

public sealed record DatabaseMoveColumnInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string columnName { get; init; } = string.Empty;
    public string afterColumnName { get; init; } = string.Empty;
}

public sealed record DatabaseAddTableInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
    public DatabaseAddColumnInput[] dbColumnInfoList { get; init; } = [];
}

public sealed record DatabaseUpdateTableInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string oldTableName { get; init; } = string.Empty;
    public string description { get; init; } = string.Empty;
}

public sealed record DatabaseDeleteTableInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
}

public sealed record DatabaseCreateEntityInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string entityName { get; init; } = string.Empty;
    public string baseClassName { get; init; } = string.Empty;
    public string position { get; init; } = string.Empty;
}

public sealed record DatabaseCreateSeedDataInput
{
    public string configId { get; init; } = string.Empty;
    public string tableName { get; init; } = string.Empty;
    public string position { get; init; } = string.Empty;
    public string suffix { get; init; } = string.Empty;
    public bool filterExistingData { get; init; }
}

public sealed record DatabaseVisualTableView : VueProps
{
    public string tableName { get; init; } = string.Empty;
    public string tableComents { get; init; } = string.Empty;
    public int x { get; init; }
    public int y { get; init; }
}

public sealed record DatabaseVisualColumnView : VueProps
{
    public string tableName { get; init; } = string.Empty;
    public string columnName { get; init; } = string.Empty;
    public string dataType { get; init; } = string.Empty;
    public string dataLength { get; init; } = string.Empty;
    public string columnDescription { get; init; } = string.Empty;
}

public sealed record DatabaseColumnRelationView : VueProps
{
    public string sourceTableName { get; init; } = string.Empty;
    public string sourceColumnName { get; init; } = string.Empty;
    public string type { get; init; } = string.Empty;
    public string targetTableName { get; init; } = string.Empty;
    public string targetColumnName { get; init; } = string.Empty;
}

public sealed record DatabaseVisualView
{
    public DatabaseVisualTableView[] tables { get; init; } = [];
    public DatabaseVisualColumnView[] columns { get; init; } = [];
    public DatabaseColumnRelationView[] relations { get; init; } = [];
}
