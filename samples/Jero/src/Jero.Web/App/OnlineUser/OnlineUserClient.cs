using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.OnlineUser;

/// <summary>Typed browser boundary for online-user paging and forced logout.</summary>
[ECMAScriptModule("components/online-user-client.js")]
public static class OnlineUserClient
{
    public static IPromise<ApiOutcome> Page(OnlineUserPageInput input)
    {
        if (input.tenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysOnlineUser/page", input);

        // TenantId is a non-nullable long on PageOnlineUserInput. Omitting a cleared
        // super-admin filter lets model binding use 0, which means all tenants.
        return BrowserApiClient.Post("/api/sysOnlineUser/page", new OnlineUserPageAllInput
        {
            page = input.page,
            pageSize = input.pageSize,
            userName = input.userName,
            realName = input.realName
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
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static OnlineUserView ReadItem(object value)
        => new()
        {
            // Snowflake identifiers remain strings across the JavaScript boundary.
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            connectionId = ReadString(value, "connectionId"),
            userId = ReadString(value, "userId"),
            userName = ReadString(value, "userName"),
            realName = ReadString(value, "realName"),
            time = ReadString(value, "time"),
            ip = ReadString(value, "ip"),
            browser = ReadString(value, "browser"),
            os = ReadString(value, "os")
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string userName { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record OnlineUserPageAllInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string userName { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
}

/// <summary>
/// The backend accepts SysOnlineUser and deletes by its primary key after notifying the
/// connection, so the selected row must be posted with both id and connectionId intact.
/// </summary>
public sealed record OnlineUserForceOfflineInput
{
    public string id { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string? connectionId { get; init; }
    public string userId { get; init; } = string.Empty;
    public string userName { get; init; } = string.Empty;
    public string? realName { get; init; }
    public string? time { get; init; }
    public string? ip { get; init; }
    public string? browser { get; init; }
    public string? os { get; init; }
}

public sealed partial record OnlineUserView : VueProps;
public sealed partial record OnlineUserTenantOption : VueProps;
