using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.DiffLog;

/// <summary>查询差异日志和变更详情，加载租户筛选选项。</summary>
[ECMAScriptModule("components/diff-log-client.js")]
public static class DiffLogClient
{
    public static IPromise<ApiOutcome> Page(DiffLogPageInput input)
    {
        if (input.TenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysLogDiff/page", input);

        return BrowserApiClient.Post("/api/sysLogDiff/page", new DiffLogPageAllInput
        {
            Page = input.Page,
            PageSize = input.PageSize,
            StartTime = input.StartTime,
            EndTime = input.EndTime
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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
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
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            DiffData = diffData,
            Sql = ReadString(value, "sql"),
            Parameters = parameters,
            BusinessData = ReadString(value, "businessData"),
            DiffType = ReadString(value, "diffType"),
            Elapsed = ReadString(value, "elapsed"),
            CreateTime = ReadString(value, "createTime"),
            TableItems = ReadTables(diffData),
            ParameterItems = ReadParameters(parameters)
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
                TableName = ReadEitherString(value, "tableName", "TableName"),
                TableDescription = ReadEitherString(value, "tableDescription", "TableDescription"),
                Columns = ReadColumns(ReadEither(value, "columns", "Columns"))
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
                ColumnName = ReadEitherString(value, "columnName", "ColumnName"),
                ColumnDescription = ReadEitherString(value, "columnDescription", "ColumnDescription"),
                IsPrimaryKey = ReadEitherBool(value, "isPrimaryKey", "IsPrimaryKey"),
                BeforeValue = FormatValue(ReadEither(value, "beforeValue", "BeforeValue")),
                AfterValue = FormatValue(ReadEither(value, "afterValue", "AfterValue"))
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
                ParameterName = ReadEitherString(value, "parameterName", "ParameterName"),
                TypeName = ReadEitherString(value, "typeName", "TypeName"),
                Value = FormatValue(ReadEither(value, "value", "Value"))
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("startTime")]
    public string? StartTime { get; init; }
    [ECMAScriptName("endTime")]
    public string? EndTime { get; init; }
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record DiffLogPageAllInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("startTime")]
    public string? StartTime { get; init; }
    [ECMAScriptName("endTime")]
    public string? EndTime { get; init; }
}

public sealed record DiffLogTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}
