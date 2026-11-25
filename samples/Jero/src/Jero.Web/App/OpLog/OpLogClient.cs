using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.OpLog;

/// <summary>Typed browser boundary for operation log queries and commands.</summary>
[ECMAScriptModule("components/op-log-client.js")]
public static class OpLogClient
{
    public static IPromise<ApiOutcome> Page(OpLogPageInput input)
        => BrowserApiClient.Post("/api/sysLogOp/page", input);

    public static IPromise<ApiOutcome> Detail(string id)
        => BrowserApiClient.Get("/api/sysLogOp/detail/" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> Clear()
        => BrowserApiClient.Post("/api/sysLogOp/clear");

    public static OpLogView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new OpLogView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static OpLogView? ReadDetail(object? data)
        => data is null ? null : ReadItem(data);

    private static OpLogView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            controllerName = ReadString(value, "controllerName"),
            actionName = ReadString(value, "actionName"),
            displayTitle = ReadString(value, "displayTitle"),
            status = ReadString(value, "status"),
            remoteIp = ReadString(value, "remoteIp"),
            location = ReadString(value, "location"),
            longitude = ReadString(value, "longitude"),
            latitude = ReadString(value, "latitude"),
            browser = ReadString(value, "browser"),
            os = ReadString(value, "os"),
            elapsed = ReadNullableLong(value, "elapsed"),
            logDateTime = ReadString(value, "logDateTime"),
            logLevel = ReadNullableInt(value, "logLevel"),
            account = ReadString(value, "account"),
            realName = ReadString(value, "realName"),
            httpMethod = ReadString(value, "httpMethod"),
            requestUrl = ReadString(value, "requestUrl"),
            requestParam = ReadString(value, "requestParam"),
            returnResult = ReadString(value, "returnResult"),
            eventId = ReadNullableInt(value, "eventId"),
            threadId = ReadNullableInt(value, "threadId"),
            traceId = ReadString(value, "traceId"),
            exception = ReadString(value, "exception"),
            message = ReadString(value, "message"),
            createTime = ReadString(value, "createTime")
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var result = StringValue(value);
        return result == "null" || result == "undefined" ? string.Empty : result;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static int? ReadNullableInt(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? null : (int)NumberValue(value);
    }

    private static long? ReadNullableLong(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? null : (long)(double)NumberValue(value);
    }
}

public sealed record OpLogPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string? startTime { get; init; }
    public string? endTime { get; init; }
    public string account { get; init; } = string.Empty;
    public long? elapsed { get; init; }
    public string status { get; init; } = string.Empty;
    public string remoteIp { get; init; } = string.Empty;
    public string tenantId { get; init; } = "0";
    public string actionName { get; init; } = string.Empty;
    public string controllerName { get; init; } = string.Empty;
    public string field { get; init; } = "createTime";
    public string order { get; init; } = "descending";
    public string descStr { get; init; } = "descending";
}

public sealed record OpLogView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string controllerName { get; init; } = string.Empty;
    public string actionName { get; init; } = string.Empty;
    public string displayTitle { get; init; } = string.Empty;
    public string status { get; init; } = string.Empty;
    public string remoteIp { get; init; } = string.Empty;
    public string location { get; init; } = string.Empty;
    public string longitude { get; init; } = string.Empty;
    public string latitude { get; init; } = string.Empty;
    public string browser { get; init; } = string.Empty;
    public string os { get; init; } = string.Empty;
    public long? elapsed { get; init; }
    public string logDateTime { get; init; } = string.Empty;
    public int? logLevel { get; init; }
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string httpMethod { get; init; } = string.Empty;
    public string requestUrl { get; init; } = string.Empty;
    public string requestParam { get; init; } = string.Empty;
    public string returnResult { get; init; } = string.Empty;
    public int? eventId { get; init; }
    public int? threadId { get; init; }
    public string traceId { get; init; } = string.Empty;
    public string exception { get; init; } = string.Empty;
    public string message { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
}
