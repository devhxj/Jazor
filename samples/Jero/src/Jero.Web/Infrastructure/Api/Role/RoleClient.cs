using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Role;

/// <summary>维护角色，读取菜单与组织授权，并提交数据范围授权。</summary>
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
        => BrowserApiClient.Post("/api/sysRole/delete", new RoleIdInput { Id = id });

    public static IPromise<ApiOutcome> SetStatus(string id, int status)
        => BrowserApiClient.Post("/api/sysRole/setStatus", new RoleStatusInput
        {
            Id = id,
            Status = status
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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
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
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            Name = ReadString(value, "name"),
            Code = ReadString(value, "code"),
            OrderNo = ReadInt(value, "orderNo", 100),
            DataScope = ReadInt(value, "dataScope", 4),
            Remark = ReadString(value, "remark"),
            Status = ReadInt(value, "status", 1),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
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
                Id = ReadString(value, "id"),
                ParentId = ReadString(value, "pid"),
                Label = ReadString(value, "title"),
                Depth = depth,
                Disabled = false,
                HasChildren = (children as object?[] ?? []).Length > 0
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
                Id = ReadString(value, "id"),
                ParentId = ReadString(value, "pid"),
                Label = ReadString(value, "name"),
                Depth = depth,
                Disabled = ReadBool(value, "disabled"),
                HasChildren = (children as object?[] ?? []).Length > 0
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 20;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = "0";
}

public sealed record RoleAddInput
{
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("dataScope")]
    public int DataScope { get; init; } = 4;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("menuIdList")]
    public string[]? MenuIdList { get; init; }
}

/// <summary>
/// 提交完整的 SysRole 标量字段。服务端将更新请求映射为实体，因此需保留选中行的审计和租户字段。
/// </summary>
public sealed record RoleUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string? TenantId { get; init; }
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("dataScope")]
    public int DataScope { get; init; } = 4;
    [ECMAScriptName("remark")]
    public string? Remark { get; init; }
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
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
    [ECMAScriptName("menuIdList")]
    public string[]? MenuIdList { get; init; }
}

public sealed record RoleIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record RoleStatusInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("status")]
    public int Status { get; init; }
}

public sealed record RoleGrantDataInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("dataScope")]
    public int DataScope { get; init; }
    [ECMAScriptName("orgIdList")]
    public string[] OrgIdList { get; init; } = [];
}
