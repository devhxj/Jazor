using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.OnlineUser;

/// <summary>分页查询在线用户，加载租户筛选选项并提交强制下线。</summary>
[ECMAScriptModule("components/online-user-client.js")]
public static class OnlineUserClient
{
    public static IPromise<ApiOutcome> Page(OnlineUserPageInput input)
    {
        if (input.TenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysOnlineUser/page", input);

        // 清空超管的租户筛选时省略 TenantId，使不可空的 long 绑定为 0，表示全部租户。
        return BrowserApiClient.Post("/api/sysOnlineUser/page", new OnlineUserPageAllInput
        {
            Page = input.Page,
            PageSize = input.PageSize,
            UserName = input.UserName,
            RealName = input.RealName
        });
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> ForceOffline(OnlineUserForceOfflineInput input)
        => BrowserApiClient.Post("/api/sysOnlineUser/forceOffline", input);

    public static OnlineUserView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new OnlineUserView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static OnlineUserTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new OnlineUserTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new OnlineUserTenantOption
            {
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static OnlineUserView ReadItem(object value)
        => new()
        {
            // 雪花 ID 跨浏览器边界时保留为字符串，避免 JavaScript 数值丢失精度。
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            ConnectionId = ReadString(value, "connectionId"),
            UserId = ReadString(value, "userId"),
            UserName = ReadString(value, "userName"),
            RealName = ReadString(value, "realName"),
            Time = ReadString(value, "time"),
            Ip = ReadString(value, "ip"),
            Browser = ReadString(value, "browser"),
            Os = ReadString(value, "os")
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

public sealed record OnlineUserPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("userName")]
    public string UserName { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string RealName { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record OnlineUserPageAllInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("userName")]
    public string UserName { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string RealName { get; init; } = string.Empty;
}

/// <summary>
/// 服务端先通知对应连接，再按 SysOnlineUser 主键删除记录。提交选中行时需同时保留 id 和 connectionId。
/// </summary>
public sealed record OnlineUserForceOfflineInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("connectionId")]
    public string? ConnectionId { get; init; }
    [ECMAScriptName("userId")]
    public string UserId { get; init; } = string.Empty;
    [ECMAScriptName("userName")]
    public string UserName { get; init; } = string.Empty;
    [ECMAScriptName("realName")]
    public string? RealName { get; init; }
    [ECMAScriptName("time")]
    public string? Time { get; init; }
    [ECMAScriptName("ip")]
    public string? Ip { get; init; }
    [ECMAScriptName("browser")]
    public string? Browser { get; init; }
    [ECMAScriptName("os")]
    public string? Os { get; init; }
}
