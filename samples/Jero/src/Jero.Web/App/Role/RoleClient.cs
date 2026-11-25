using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Role;

/// <summary>Typed browser boundary for role, menu, and data-scope management.</summary>
[ECMAScriptModule("components/role-client.js")]
public static class RoleClient
{
    public static IPromise<ApiOutcome> Page(RolePageInput input)
        => BrowserApiClient.Post("/api/sysRole/page", input);

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Add(RoleAddInput input)
        => BrowserApiClient.Post("/api/sysRole/add", input);

    public static IPromise<ApiOutcome> Update(RoleUpdateInput input)
        => BrowserApiClient.Post("/api/sysRole/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysRole/delete", new RoleIdInput { id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysRole/setStatus", new RoleStatusInput
        {
            id = id,
            status = status
        });

    public static IPromise<ApiOutcome> MenuTree(string tenantId)
    {
        var path = "/api/sysMenu/list";
        if (tenantId.Length > 0)
            path += "?TenantId=" + EncodeURIComponent(tenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> OwnMenuList(string id)
        => BrowserApiClient.Get("/api/sysRole/ownMenuList?Id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> OrgTree(string tenantId)
    {
        var path = "/api/sysOrg/tree?Id=0";
        if (tenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(tenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> OwnOrgList(string id)
        => BrowserApiClient.Get("/api/sysRole/ownOrgList?Id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> GrantDataScope(RoleGrantDataInput input)
        => BrowserApiClient.Post("/api/sysRole/grantDataScope", input);

    public static RoleView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new RoleView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadRole(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static RoleTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new RoleTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new RoleTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static RoleTreeNode[] ReadMenuTree(object? data)
    {
        var result = new RoleTreeNode[CountTree(data)];
        WriteMenuTree(data, result, 0, 0);
        return result;
    }

    public static RoleTreeNode[] ReadOrgTree(object? data)
    {
        var result = new RoleTreeNode[CountTree(data)];
        WriteOrgTree(data, result, 0, 0);
        return result;
    }

    public static string[] ReadIds(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = values[index] is null ? string.Empty : StringValue(values[index]!);
        return result;
    }

    private static RoleView ReadRole(object value)
        => new()
        {
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            name = ReadString(value, "name"),
            code = ReadString(value, "code"),
            orderNo = ReadInt(value, "orderNo", 100),
            dataScope = ReadInt(value, "dataScope", 4),
            remark = ReadString(value, "remark"),
            status = ReadInt(value, "status", 1),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
        };

    private static int CountTree(object? data)
    {
        var values = data as object?[] ?? [];
        var count = values.Length;
        for (var index = 0; index < values.Length; index++)
            count += CountTree(Reflect.Get(values[index]!, "children"));
        return count;
    }

    private static int WriteMenuTree(object? data, RoleTreeNode[] target, int targetIndex, int depth)
    {
        var values = data as object?[] ?? [];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var children = Reflect.Get(value, "children");
            target[targetIndex] = new RoleTreeNode
            {
                id = ReadString(value, "id"),
                parentId = ReadString(value, "pid"),
                label = ReadString(value, "title"),
                depth = depth,
                disabled = false,
                hasChildren = (children as object?[] ?? []).Length > 0
            };
            targetIndex++;
            targetIndex = WriteMenuTree(children, target, targetIndex, depth + 1);
        }
        return targetIndex;
    }

    private static int WriteOrgTree(object? data, RoleTreeNode[] target, int targetIndex, int depth)
    {
        var values = data as object?[] ?? [];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var children = Reflect.Get(value, "children");
            target[targetIndex] = new RoleTreeNode
            {
                id = ReadString(value, "id"),
                parentId = ReadString(value, "pid"),
                label = ReadString(value, "name"),
                depth = depth,
                disabled = ReadBool(value, "disabled"),
                hasChildren = (children as object?[] ?? []).Length > 0
            };
            targetIndex++;
            targetIndex = WriteOrgTree(children, target, targetIndex, depth + 1);
        }
        return targetIndex;
    }

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

    private static bool ReadBool(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }
}

public sealed record RolePageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 20;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string tenantId { get; init; } = "0";
}

public sealed record RoleAddInput
{
    public string? tenantId { get; init; }
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public int dataScope { get; init; } = 4;
    public string? remark { get; init; }
    public int status { get; init; } = 1;
    public string[]? menuIdList { get; init; }
}

/// <summary>
/// Full SysRole scalar snapshot. UpdateRole adapts the body to the entity, so audit and tenant
/// fields from the selected row remain present even though this page edits only role settings.
/// </summary>
public sealed record RoleUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string? tenantId { get; init; }
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public int dataScope { get; init; } = 4;
    public string? remark { get; init; }
    public int status { get; init; } = 1;
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
    public string[]? menuIdList { get; init; }
}

public sealed record RoleIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record RoleStatusInput
{
    public string id { get; init; } = string.Empty;
    public int status { get; init; }
}

public sealed record RoleGrantDataInput
{
    public string id { get; init; } = string.Empty;
    public int dataScope { get; init; }
    public string[] orgIdList { get; init; } = [];
}

public sealed partial record RoleView : VueProps;
public sealed partial record RoleTenantOption : VueProps;
