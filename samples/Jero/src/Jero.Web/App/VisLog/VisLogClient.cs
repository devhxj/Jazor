using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.VisLog;

/// <summary>Typed browser boundary for visit-log queries.</summary>
[ECMAScriptModule("components/vis-log-client.js")]
public static class VisLogClient
{
    public static IPromise<ApiOutcome> Page(VisLogPageInput input)
    {
        if (input.tenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysLogVis/page", input);

        return BrowserApiClient.Post("/api/sysLogVis/page", new VisLogPageAllInput
        {
            page = input.page,
            pageSize = input.pageSize,
            startTime = input.startTime,
            endTime = input.endTime,
            account = input.account,
            actionName = input.actionName,
            elapsed = input.elapsed,
            remoteIp = input.remoteIp,
            status = input.status
        });
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static VisLogView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new VisLogView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static VisLogTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new VisLogTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new VisLogTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static VisLogView ReadItem(object value)
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
            elapsed = ReadString(value, "elapsed"),
            logDateTime = ReadString(value, "logDateTime"),
            account = ReadString(value, "account"),
            realName = ReadString(value, "realName"),
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
}

public sealed record VisLogPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string? startTime { get; init; }
    public string? endTime { get; init; }
    public string tenantId { get; init; } = string.Empty;
    public string? account { get; init; }
    public string? actionName { get; init; }
    public double? elapsed { get; init; }
    public string? remoteIp { get; init; }
    public string status { get; init; } = string.Empty;
}

public sealed record VisLogPageAllInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string? startTime { get; init; }
    public string? endTime { get; init; }
    public string? account { get; init; }
    public string? actionName { get; init; }
    public double? elapsed { get; init; }
    public string? remoteIp { get; init; }
    public string status { get; init; } = string.Empty;
}

public sealed record VisLogView : VueProps
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
    public string elapsed { get; init; } = string.Empty;
    public string logDateTime { get; init; } = string.Empty;
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
}

public sealed record VisLogTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}
