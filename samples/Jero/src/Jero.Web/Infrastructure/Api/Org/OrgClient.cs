using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Org;

/// <summary>读取租户组织列表及树形结构，并维护组织记录。</summary>
[ECMAScriptModule("components/org-client.js")]
public static class OrgClient
{
    public static IPromise<ApiOutcome> List(OrgListInput input)
    {
        var path = "/api/sysOrg/list?Id=" + EncodeURIComponent(input.Id) +
                   "&Name=" + EncodeURIComponent(input.Name) +
                   "&Code=" + EncodeURIComponent(input.Code) +
                   "&Type=" + EncodeURIComponent(input.Type);
        if (input.TenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(input.TenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Tree(string tenantId)
    {
        var path = "/api/sysOrg/tree?Id=0";
        if (tenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(tenantId);
        return BrowserApiClient.Get(path);
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Types()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=org_type");

    public static IPromise<ApiOutcome> Add(OrgSaveInput input)
        => BrowserApiClient.Post("/api/sysOrg/add", new OrgAddInput
        {
            TenantId = input.TenantId,
            Pid = input.Pid,
            Name = input.Name,
            Code = input.Code,
            Level = input.Level,
            Type = input.Type,
            DirectorId = input.DirectorId,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark
        });

    public static IPromise<ApiOutcome> Update(OrgFlatView original, OrgSaveInput input)
        => BrowserApiClient.Post("/api/sysOrg/update", new OrgUpdateInput
        {
            // UpdateOrg 会将请求映射为完整的 SysOrg。保留原行的审计字段，避免写入 CLR 默认值。
            Id = original.Id,
            TenantId = input.TenantId,
            Pid = input.Pid,
            Name = input.Name,
            Code = input.Code,
            Level = input.Level,
            Type = input.Type,
            DirectorId = input.DirectorId,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreateTime = original.CreateTime,
            CreateUserId = Optional(original.CreateUserId),
            CreateUserName = Optional(original.CreateUserName),
            UpdateTime = Optional(original.UpdateTime),
            UpdateUserId = Optional(original.UpdateUserId),
            UpdateUserName = Optional(original.UpdateUserName)
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysOrg/delete", new OrgIdInput { Id = id });

    public static OrgView[] ReadOrganizations(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new OrgView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadOrganization(values[index]!);
        return result;
    }

    public static OrgTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new OrgTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new OrgTenantOption
            {
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static OrgTypeOption[] ReadTypes(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new OrgTypeOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            var name = ReadString(value, "name");
            var label = ReadString(value, "label");
            result[index] = new OrgTypeOption
            {
                Id = ReadString(value, "id"),
                Value = ReadString(value, "value"),
                Label = name.Length > 0 ? name : label,
                TagType = ReadString(value, "tagType")
            };
        }
        return result;
    }

    /// <summary>
    /// 读取响应时将 ID 投影为字符串，再写入响应式状态。雪花 ID 可能超过 JavaScript Number 的安全整数范围。
    /// </summary>
    private static OrgView ReadOrganization(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            Pid = ReadString(value, "pid"),
            Name = ReadString(value, "name"),
            Code = ReadString(value, "code"),
            Level = ReadInt(value, "level", 1),
            Type = ReadString(value, "type"),
            DirectorId = ReadString(value, "directorId"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Status = ReadInt(value, "status", 1),
            Remark = ReadString(value, "remark"),
            Disabled = ReadBool(value, "disabled"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName"),
            Children = ReadOrganizations(Reflect.Get(value, "children"))
        };

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
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

    private static string? Optional(string value)
        => value.Length == 0 ? null : value;
}

public sealed record OrgListInput
{
    public string Id { get; init; } = "0";
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
}

public sealed record OrgSaveInput
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Pid { get; init; } = "0";
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public int Level { get; init; } = 1;
    public string Type { get; init; } = string.Empty;
    public string? DirectorId { get; init; }
    public int OrderNo { get; init; } = 100;
    public int Status { get; init; } = 1;
    public string Remark { get; init; } = string.Empty;
}

public sealed record OrgAddInput
{
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("pid")]
    public string Pid { get; init; } = "0";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("level")]
    public int Level { get; init; } = 1;
    [ECMAScriptName("type")]
    public string Type { get; init; } = string.Empty;
    [ECMAScriptName("directorId")]
    public string? DirectorId { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record OrgUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("pid")]
    public string Pid { get; init; } = "0";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("level")]
    public int Level { get; init; } = 1;
    [ECMAScriptName("type")]
    public string Type { get; init; } = string.Empty;
    [ECMAScriptName("directorId")]
    public string? DirectorId { get; init; }
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
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

public sealed record OrgIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record OrgView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Pid { get; init; } = "0";
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public int Level { get; init; } = 1;
    public string Type { get; init; } = string.Empty;
    public string DirectorId { get; init; } = string.Empty;
    public int OrderNo { get; init; } = 100;
    public int Status { get; init; } = 1;
    public string Remark { get; init; } = string.Empty;
    public bool Disabled { get; init; }
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
    public OrgView[] Children { get; init; } = [];
}
