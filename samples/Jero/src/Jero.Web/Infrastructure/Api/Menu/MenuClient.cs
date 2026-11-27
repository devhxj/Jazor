using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Menu;

/// <summary>
/// 查询和维护租户菜单，包括启停操作。雪花 ID 在浏览器侧保留为字符串，避免丢失整数精度。
/// </summary>
[ECMAScriptModule("components/menu-client.js")]
public static class MenuClient
{
    public static IPromise<ApiOutcome> List(MenuListInput input)
    {
        var path = "/api/sysMenu/list?Title=" + EncodeURIComponent(input.Title);
        if (input.Type > 0)
            path += "&Type=" + input.Type;
        if (input.TenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(input.TenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Add(MenuSaveInput input)
        => BrowserApiClient.Post("/api/sysMenu/add", new MenuAddInput
        {
            Pid = input.Pid,
            Type = input.Type,
            Name = input.Name,
            Path = input.Path,
            Component = input.Component,
            Redirect = input.Redirect,
            Permission = input.Permission,
            Title = input.Title,
            Icon = input.Icon,
            IsIframe = input.IsIframe,
            OutLink = input.OutLink,
            IsHide = input.IsHide,
            IsKeepAlive = input.IsKeepAlive,
            IsAffix = input.IsAffix,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            TenantId = input.TenantId
        });

    public static IPromise<ApiOutcome> Update(MenuView original, MenuSaveInput input)
        => BrowserApiClient.Post("/api/sysMenu/update", new MenuUpdateInput
        {
            // UpdateMenu 会将请求映射为完整的 SysMenu。保留表单不可编辑的审计字段，避免写入 CLR 默认值。
            Id = original.Id,
            Pid = input.Pid,
            Type = input.Type,
            Name = input.Name,
            Path = input.Path,
            Component = input.Component,
            Redirect = input.Redirect,
            Permission = input.Permission,
            Title = input.Title,
            Icon = input.Icon,
            IsIframe = input.IsIframe,
            OutLink = input.OutLink,
            IsHide = input.IsHide,
            IsKeepAlive = input.IsKeepAlive,
            IsAffix = input.IsAffix,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            TenantId = input.TenantId,
            CreateTime = original.CreateTime,
            CreateUserId = original.CreateUserId,
            CreateUserName = original.CreateUserName,
            UpdateTime = original.UpdateTime,
            UpdateUserId = original.UpdateUserId,
            UpdateUserName = original.UpdateUserName
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysMenu/delete", new MenuIdInput { Id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysMenu/setStatus", new MenuStatusInput
        {
            Id = id,
            Status = status
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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    private static MenuView ReadMenu(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            Pid = ReadString(value, "pid"),
            TenantId = ReadString(value, "tenantId"),
            Type = ReadInt(value, "type", 2),
            Name = ReadString(value, "name"),
            Path = ReadString(value, "path"),
            Component = ReadString(value, "component"),
            Redirect = ReadString(value, "redirect"),
            Permission = ReadString(value, "permission"),
            Title = ReadString(value, "title"),
            Icon = ReadString(value, "icon"),
            IsIframe = ReadBool(value, "isIframe"),
            OutLink = ReadString(value, "outLink"),
            IsHide = ReadBool(value, "isHide"),
            IsKeepAlive = ReadBool(value, "isKeepAlive"),
            IsAffix = ReadBool(value, "isAffix"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Status = ReadInt(value, "status", 1),
            Remark = ReadString(value, "remark"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadOptionalString(value, "createUserId"),
            CreateUserName = ReadOptionalString(value, "createUserName"),
            UpdateTime = ReadOptionalString(value, "updateTime"),
            UpdateUserId = ReadOptionalString(value, "updateUserId"),
            UpdateUserName = ReadOptionalString(value, "updateUserName"),
            Children = ReadMenus(Reflect.Get(value, "children"))
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
    public string Title { get; init; } = string.Empty;
    public int Type { get; init; }
    public string TenantId { get; init; } = string.Empty;
}

public sealed record MenuSaveInput
{
    public string Id { get; init; } = string.Empty;
    public string Pid { get; init; } = "0";
    public int Type { get; init; } = 2;
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Component { get; init; } = string.Empty;
    public string Redirect { get; init; } = string.Empty;
    public string Permission { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public bool IsIframe { get; init; }
    public string OutLink { get; init; } = string.Empty;
    public bool IsHide { get; init; }
    public bool IsKeepAlive { get; init; } = true;
    public bool IsAffix { get; init; }
    public int OrderNo { get; init; } = 100;
    public int Status { get; init; } = 1;
    public string Remark { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
}

public sealed record MenuAddInput
{
    [ECMAScriptName("pid")]
    public string Pid { get; init; } = "0";
    [ECMAScriptName("type")]
    public int Type { get; init; } = 2;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("path")]
    public string Path { get; init; } = string.Empty;
    [ECMAScriptName("component")]
    public string Component { get; init; } = string.Empty;
    [ECMAScriptName("redirect")]
    public string Redirect { get; init; } = string.Empty;
    [ECMAScriptName("permission")]
    public string Permission { get; init; } = string.Empty;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("icon")]
    public string Icon { get; init; } = string.Empty;
    [ECMAScriptName("isIframe")]
    public bool IsIframe { get; init; }
    [ECMAScriptName("outLink")]
    public string OutLink { get; init; } = string.Empty;
    [ECMAScriptName("isHide")]
    public bool IsHide { get; init; }
    [ECMAScriptName("isKeepAlive")]
    public bool IsKeepAlive { get; init; } = true;
    [ECMAScriptName("isAffix")]
    public bool IsAffix { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record MenuUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("pid")]
    public string Pid { get; init; } = "0";
    [ECMAScriptName("type")]
    public int Type { get; init; } = 2;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("path")]
    public string Path { get; init; } = string.Empty;
    [ECMAScriptName("component")]
    public string Component { get; init; } = string.Empty;
    [ECMAScriptName("redirect")]
    public string Redirect { get; init; } = string.Empty;
    [ECMAScriptName("permission")]
    public string Permission { get; init; } = string.Empty;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("icon")]
    public string Icon { get; init; } = string.Empty;
    [ECMAScriptName("isIframe")]
    public bool IsIframe { get; init; }
    [ECMAScriptName("outLink")]
    public string OutLink { get; init; } = string.Empty;
    [ECMAScriptName("isHide")]
    public bool IsHide { get; init; }
    [ECMAScriptName("isKeepAlive")]
    public bool IsKeepAlive { get; init; } = true;
    [ECMAScriptName("isAffix")]
    public bool IsAffix { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("createTime")]
    public string CreateTime { get; init; } = string.Empty;
    [ECMAScriptName("createUserId")]
    public string? CreateUserId { get; init; }
    [ECMAScriptName("createUserName")]
    public string? CreateUserName { get; init; }
    [ECMAScriptName("updateTime")]
    public string? UpdateTime { get; init; }
    [ECMAScriptName("updateUserId")]
    public string? UpdateUserId { get; init; }
    [ECMAScriptName("updateUserName")]
    public string? UpdateUserName { get; init; }
}

public sealed record MenuIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record MenuStatusInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; }
}

public sealed record MenuView
{
    public string Id { get; init; } = string.Empty;
    public string Pid { get; init; } = "0";
    public string TenantId { get; init; } = string.Empty;
    public int Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Component { get; init; } = string.Empty;
    public string Redirect { get; init; } = string.Empty;
    public string Permission { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public bool IsIframe { get; init; }
    public string OutLink { get; init; } = string.Empty;
    public bool IsHide { get; init; }
    public bool IsKeepAlive { get; init; }
    public bool IsAffix { get; init; }
    public int OrderNo { get; init; }
    public int Status { get; set; }
    public string Remark { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string? CreateUserId { get; init; }
    public string? CreateUserName { get; init; }
    public string? UpdateTime { get; init; }
    public string? UpdateUserId { get; init; }
    public string? UpdateUserName { get; init; }
    public MenuView[] Children { get; init; } = [];
}

public sealed record MenuFlatView
{
    public MenuView Menu { get; init; } = new();
    public int Depth { get; init; }
}

public sealed record MenuTenantOption
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}
