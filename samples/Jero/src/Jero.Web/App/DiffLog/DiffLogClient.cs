using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.DiffLog;

/// <summary>Typed browser boundary for diff-log queries and detail parsing.</summary>
[ECMAScriptModule("components/diff-log-client.js")]
public static class DiffLogClient
{
    public static IPromise<ApiOutcome> Page(DiffLogPageInput input)
    {
        if (input.tenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysLogDiff/page", input);

        return BrowserApiClient.Post("/api/sysLogDiff/page", new DiffLogPageAllInput
        {
            page = input.page,
            pageSize = input.pageSize,
            startTime = input.startTime,
            endTime = input.endTime
        });
    }

    public static IPromise<ApiOutcome> Detail(string id)
        => BrowserApiClient.Get("/api/sysLogDiff/detail/" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static DiffLogView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new DiffLogView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static DiffLogView? ReadDetail(object? data)
        => data is null ? null : ReadItem(data);

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static DiffLogTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new DiffLogTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DiffLogTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static DiffLogView ReadItem(object value)
    {
        var diffData = ReadString(value, "diffData");
        var parameters = ReadString(value, "parameters");
        return new DiffLogView
        {
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            diffData = diffData,
            sql = ReadString(value, "sql"),
            parameters = parameters,
            businessData = ReadString(value, "businessData"),
            diffType = ReadString(value, "diffType"),
            elapsed = ReadString(value, "elapsed"),
            createTime = ReadString(value, "createTime"),
            tableItems = ReadTables(diffData),
            parameterItems = ReadParameters(parameters)
        };
    }

    private static DiffLogTableView[] ReadTables(string json)
    {
        var values = ReadJsonArray(json);
        var result = new DiffLogTableView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DiffLogTableView
            {
                tableName = ReadEitherString(value, "tableName", "TableName"),
                tableDescription = ReadEitherString(value, "tableDescription", "TableDescription"),
                columns = ReadColumns(ReadEither(value, "columns", "Columns"))
            };
        }
        return result;
    }

    private static DiffLogColumnView[] ReadColumns(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new DiffLogColumnView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DiffLogColumnView
            {
                columnName = ReadEitherString(value, "columnName", "ColumnName"),
                columnDescription = ReadEitherString(value, "columnDescription", "ColumnDescription"),
                isPrimaryKey = ReadEitherBool(value, "isPrimaryKey", "IsPrimaryKey"),
                beforeValue = FormatValue(ReadEither(value, "beforeValue", "BeforeValue")),
                afterValue = FormatValue(ReadEither(value, "afterValue", "AfterValue"))
            };
        }
        return result;
    }

    private static DiffLogParameterView[] ReadParameters(string json)
    {
        var values = ReadJsonArray(json);
        var result = new DiffLogParameterView[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new DiffLogParameterView
            {
                parameterName = ReadEitherString(value, "parameterName", "ParameterName"),
                typeName = ReadEitherString(value, "typeName", "TypeName"),
                value = FormatValue(ReadEither(value, "value", "Value"))
            };
        }
        return result;
    }

    private static object?[] ReadJsonArray(string json)
    {
        var normalized = json.Trim();
        if (normalized.Length < 2 || normalized[0] != '[' || normalized[normalized.Length - 1] != ']')
            return [];

        // 差异日志是持久化审计数据，历史脏值不能阻断整页渲染；与现有 Vue 页保持相同的解析边界。
        try
        {
            return JSON.Parse(normalized) as object?[] ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static object? ReadEither(object data, string camelKey, string pascalKey)
    {
        var value = Reflect.Get(data, camelKey);
        return value ?? Reflect.Get(data, pascalKey);
    }

    private static string ReadEitherString(object data, string camelKey, string pascalKey)
        => FormatSimple(ReadEither(data, camelKey, pascalKey));

    private static bool ReadEitherBool(object data, string camelKey, string pascalKey)
    {
        var value = ReadEither(data, camelKey, pascalKey);
        return value is not null && BooleanValue(value);
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
            return "-";
        if (value is string text)
            return text.Length == 0 ? "-" : text;
        return JSON.Stringify(value) ?? StringValue(value);
    }

    private static string FormatSimple(object? value)
    {
        if (value is null)
            return string.Empty;
        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static string ReadString(object data, string key)
        => FormatSimple(Reflect.Get(data, key));

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }
}

public sealed record DiffLogPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string? startTime { get; init; }
    public string? endTime { get; init; }
    public string tenantId { get; init; } = string.Empty;
}

public sealed record DiffLogPageAllInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string? startTime { get; init; }
    public string? endTime { get; init; }
}

public sealed record DiffLogTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}
