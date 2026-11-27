using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.OpLog;

/// <summary>分页查询操作日志、查看详情和清理记录。</summary>
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
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            ControllerName = ReadString(value, "controllerName"),
            ActionName = ReadString(value, "actionName"),
            DisplayTitle = ReadString(value, "displayTitle"),
            Status = ReadString(value, "status"),
            RemoteIp = ReadString(value, "remoteIp"),
            Location = ReadString(value, "location"),
            Longitude = ReadString(value, "longitude"),
            Latitude = ReadString(value, "latitude"),
            Browser = ReadString(value, "browser"),
            Os = ReadString(value, "os"),
            Elapsed = ReadNullableLong(value, "elapsed"),
            LogDateTime = ReadString(value, "logDateTime"),
            LogLevel = ReadNullableInt(value, "logLevel"),
            Account = ReadString(value, "account"),
            RealName = ReadString(value, "realName"),
            HttpMethod = ReadString(value, "httpMethod"),
            RequestUrl = ReadString(value, "requestUrl"),
            RequestParam = ReadString(value, "requestParam"),
            ReturnResult = ReadString(value, "returnResult"),
            EventId = ReadNullableInt(value, "eventId"),
            ThreadId = ReadNullableInt(value, "threadId"),
            TraceId = ReadString(value, "traceId"),
            Exception = ReadString(value, "exception"),
            Message = ReadString(value, "message"),
            CreateTime = ReadString(value, "createTime")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("startTime")]
    public string? StartTime { get; init; }
    [ECMAScriptName("endTime")]
    public string? EndTime { get; init; }
    [ECMAScriptName("account")]
    public string Account { get; init; } = string.Empty;
    [ECMAScriptName("elapsed")]
    public long? Elapsed { get; init; }
    [ECMAScriptName("status")]
    public string Status { get; init; } = string.Empty;
    [ECMAScriptName("remoteIp")]
    public string RemoteIp { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = "0";
    [ECMAScriptName("actionName")]
    public string ActionName { get; init; } = string.Empty;
    [ECMAScriptName("controllerName")]
    public string ControllerName { get; init; } = string.Empty;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "createTime";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "descending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
}

public sealed record OpLogView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string ControllerName { get; init; } = string.Empty;
    public string ActionName { get; init; } = string.Empty;
    public string DisplayTitle { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string RemoteIp { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Longitude { get; init; } = string.Empty;
    public string Latitude { get; init; } = string.Empty;
    public string Browser { get; init; } = string.Empty;
    public string Os { get; init; } = string.Empty;
    public long? Elapsed { get; init; }
    public string LogDateTime { get; init; } = string.Empty;
    public int? LogLevel { get; init; }
    public string Account { get; init; } = string.Empty;
    public string RealName { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = string.Empty;
    public string RequestUrl { get; init; } = string.Empty;
    public string RequestParam { get; init; } = string.Empty;
    public string ReturnResult { get; init; } = string.Empty;
    public int? EventId { get; init; }
    public int? ThreadId { get; init; }
    public string TraceId { get; init; } = string.Empty;
    public string Exception { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
}
