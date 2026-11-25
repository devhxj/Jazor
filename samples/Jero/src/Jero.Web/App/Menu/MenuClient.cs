using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Menu;

/// <summary>
/// Typed browser boundary for menu management. Snowflake identifiers remain strings so
/// JSON.stringify never crosses JavaScript's unsafe integer range.
/// </summary>
[ECMAScriptModule("components/menu-client.js")]
public static class MenuClient
{
    public static IPromise<ApiOutcome> List(MenuListInput input)
    {
        var path = "/api/sysMenu/list?Title=" + EncodeURIComponent(input.title);
        if (input.type > 0)
            path += "&Type=" + input.type;
        if (input.tenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(input.tenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Add(MenuSaveInput input)
        => BrowserApiClient.Post("/api/sysMenu/add", new MenuAddInput
        {
            pid = input.pid,
            type = input.type,
            name = input.name,
            path = input.path,
            component = input.component,
            redirect = input.redirect,
            permission = input.permission,
            title = input.title,
            icon = input.icon,
            isIframe = input.isIframe,
            outLink = input.outLink,
            isHide = input.isHide,
            isKeepAlive = input.isKeepAlive,
            isAffix = input.isAffix,
            orderNo = input.orderNo,
            status = input.status,
            remark = input.remark,
            tenantId = input.tenantId
        });

    public static IPromise<ApiOutcome> Update(MenuView original, MenuSaveInput input)
        => BrowserApiClient.Post("/api/sysMenu/update", new MenuUpdateInput
        {
            // UpdateMenu maps the request back to the complete SysMenu entity. Keep the
            // non-editable audit snapshot so an edit cannot replace it with CLR defaults.
            id = original.id,
            pid = input.pid,
            type = input.type,
            name = input.name,
            path = input.path,
            component = input.component,
            redirect = input.redirect,
            permission = input.permission,
            title = input.title,
            icon = input.icon,
            isIframe = input.isIframe,
            outLink = input.outLink,
            isHide = input.isHide,
            isKeepAlive = input.isKeepAlive,
            isAffix = input.isAffix,
            orderNo = input.orderNo,
            status = input.status,
            remark = input.remark,
            tenantId = input.tenantId,
            createTime = original.createTime,
            createUserId = original.createUserId,
            createUserName = original.createUserName,
            updateTime = original.updateTime,
            updateUserId = original.updateUserId,
            updateUserName = original.updateUserName
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysMenu/delete", new MenuIdInput { id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysMenu/setStatus", new MenuStatusInput
        {
            id = id,
            status = status
        });

    public static MenuView[] ReadMenus(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new MenuView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadMenu(values[index]!);
        return result;
    }

    public static MenuTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new MenuTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new MenuTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static MenuView ReadMenu(object value)
        => new()
        {
            id = ReadString(value, "id"),
            pid = ReadString(value, "pid"),
            tenantId = ReadString(value, "tenantId"),
            type = ReadInt(value, "type", 2),
            name = ReadString(value, "name"),
            path = ReadString(value, "path"),
            component = ReadString(value, "component"),
            redirect = ReadString(value, "redirect"),
            permission = ReadString(value, "permission"),
            title = ReadString(value, "title"),
            icon = ReadString(value, "icon"),
            isIframe = ReadBool(value, "isIframe"),
            outLink = ReadString(value, "outLink"),
            isHide = ReadBool(value, "isHide"),
            isKeepAlive = ReadBool(value, "isKeepAlive"),
            isAffix = ReadBool(value, "isAffix"),
            orderNo = ReadInt(value, "orderNo", 100),
            status = ReadInt(value, "status", 1),
            remark = ReadString(value, "remark"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadOptionalString(value, "createUserId"),
            createUserName = ReadOptionalString(value, "createUserName"),
            updateTime = ReadOptionalString(value, "updateTime"),
            updateUserId = ReadOptionalString(value, "updateUserId"),
            updateUserName = ReadOptionalString(value, "updateUserName"),
            children = ReadMenus(Reflect.Get(value, "children"))
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }

    private static string? ReadOptionalString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? null : StringValue(value);
    }

    private static int ReadInt(object data, string key, int defaultValue)
    {
        var value = Reflect.Get(data, key);
        return value is null ? defaultValue : (int)NumberValue(value);
    }

    private static bool ReadBool(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }
}

public sealed record MenuListInput
{
    public string title { get; init; } = string.Empty;
    public int type { get; init; }
    public string tenantId { get; init; } = string.Empty;
}

public sealed record MenuSaveInput
{
    public string id { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public int type { get; init; } = 2;
    public string name { get; init; } = string.Empty;
    public string path { get; init; } = string.Empty;
    public string component { get; init; } = string.Empty;
    public string redirect { get; init; } = string.Empty;
    public string permission { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string icon { get; init; } = string.Empty;
    public bool isIframe { get; init; }
    public string outLink { get; init; } = string.Empty;
    public bool isHide { get; init; }
    public bool isKeepAlive { get; init; } = true;
    public bool isAffix { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record MenuAddInput
{
    public string pid { get; init; } = "0";
    public int type { get; init; } = 2;
    public string name { get; init; } = string.Empty;
    public string path { get; init; } = string.Empty;
    public string component { get; init; } = string.Empty;
    public string redirect { get; init; } = string.Empty;
    public string permission { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string icon { get; init; } = string.Empty;
    public bool isIframe { get; init; }
    public string outLink { get; init; } = string.Empty;
    public bool isHide { get; init; }
    public bool isKeepAlive { get; init; } = true;
    public bool isAffix { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record MenuUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public int type { get; init; } = 2;
    public string name { get; init; } = string.Empty;
    public string path { get; init; } = string.Empty;
    public string component { get; init; } = string.Empty;
    public string redirect { get; init; } = string.Empty;
    public string permission { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string icon { get; init; } = string.Empty;
    public bool isIframe { get; init; }
    public string outLink { get; init; } = string.Empty;
    public bool isHide { get; init; }
    public bool isKeepAlive { get; init; } = true;
    public bool isAffix { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record MenuIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record MenuStatusInput
{
    public string id { get; init; } = string.Empty;
    public int status { get; init; }
}

public sealed record MenuView
{
    public string id { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public string tenantId { get; init; } = string.Empty;
    public int type { get; init; }
    public string name { get; init; } = string.Empty;
    public string path { get; init; } = string.Empty;
    public string component { get; init; } = string.Empty;
    public string redirect { get; init; } = string.Empty;
    public string permission { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string icon { get; init; } = string.Empty;
    public bool isIframe { get; init; }
    public string outLink { get; init; } = string.Empty;
    public bool isHide { get; init; }
    public bool isKeepAlive { get; init; }
    public bool isAffix { get; init; }
    public int orderNo { get; init; }
    public int status { get; set; }
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public MenuView[] children { get; init; } = [];
}

public sealed record MenuFlatView
{
    public MenuView menu { get; init; } = new();
    public int depth { get; init; }
}

public sealed record MenuTenantOption
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}
