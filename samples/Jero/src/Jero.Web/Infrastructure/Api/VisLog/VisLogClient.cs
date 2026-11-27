using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.VisLog;

/// <summary>分页查询访问日志并加载租户筛选选项。</summary>
[ECMAScriptModule("components/vis-log-client.js")]
public static class VisLogClient
{
    public static IPromise<ApiOutcome> Page(VisLogPageInput input)
    {
        if (input.TenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysLogVis/page", input);

        return BrowserApiClient.Post("/api/sysLogVis/page", new VisLogPageAllInput
        {
            Page = input.Page,
            PageSize = input.PageSize,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Account = input.Account,
            ActionName = input.ActionName,
            Elapsed = input.Elapsed,
            RemoteIp = input.RemoteIp,
            Status = input.Status
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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static VisLogView ReadItem(object value)
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
            Elapsed = ReadString(value, "elapsed"),
            LogDateTime = ReadString(value, "logDateTime"),
            Account = ReadString(value, "account"),
            RealName = ReadString(value, "realName"),
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
}

public sealed record VisLogPageInput
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
    [ECMAScriptName("account")]
    public string? Account { get; init; }
    [ECMAScriptName("actionName")]
    public string? ActionName { get; init; }
    [ECMAScriptName("elapsed")]
    public double? Elapsed { get; init; }
    [ECMAScriptName("remoteIp")]
    public string? RemoteIp { get; init; }
    [ECMAScriptName("status")]
    public string Status { get; init; } = string.Empty;
}

public sealed record VisLogPageAllInput
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
    public string? Account { get; init; }
    [ECMAScriptName("actionName")]
    public string? ActionName { get; init; }
    [ECMAScriptName("elapsed")]
    public double? Elapsed { get; init; }
    [ECMAScriptName("remoteIp")]
    public string? RemoteIp { get; init; }
    [ECMAScriptName("status")]
    public string Status { get; init; } = string.Empty;
}

public sealed record VisLogView : VueProps
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
    public string Elapsed { get; init; } = string.Empty;
    public string LogDateTime { get; init; } = string.Empty;
    public string Account { get; init; } = string.Empty;
    public string RealName { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
}

public sealed record VisLogTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}
