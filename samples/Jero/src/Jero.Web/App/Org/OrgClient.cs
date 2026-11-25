using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Org;

/// <summary>Typed browser boundary for organization management.</summary>
[ECMAScriptModule("components/org-client.js")]
public static class OrgClient
{
    public static IPromise<ApiOutcome> List(OrgListInput input)
    {
        var path = "/api/sysOrg/list?Id=" + EncodeURIComponent(input.id) +
                   "&Name=" + EncodeURIComponent(input.name) +
                   "&Code=" + EncodeURIComponent(input.code) +
                   "&Type=" + EncodeURIComponent(input.type);
        if (input.tenantId.Length > 0)
            path += "&TenantId=" + EncodeURIComponent(input.tenantId);
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
            tenantId = input.tenantId,
            pid = input.pid,
            name = input.name,
            code = input.code,
            level = input.level,
            type = input.type,
            directorId = input.directorId,
            orderNo = input.orderNo,
            status = input.status,
            remark = input.remark
        });

    public static IPromise<ApiOutcome> Update(OrgFlatView original, OrgSaveInput input)
        => BrowserApiClient.Post("/api/sysOrg/update", new OrgUpdateInput
        {
            // UpdateOrg maps the request back to the complete SysOrg entity. Keep the original
            // audit snapshot so an edit cannot replace non-editable fields with CLR defaults.
            id = original.id,
            tenantId = input.tenantId,
            pid = input.pid,
            name = input.name,
            code = input.code,
            level = input.level,
            type = input.type,
            directorId = input.directorId,
            orderNo = input.orderNo,
            status = input.status,
            remark = input.remark,
            createTime = original.createTime,
            createUserId = Optional(original.createUserId),
            createUserName = Optional(original.createUserName),
            updateTime = Optional(original.updateTime),
            updateUserId = Optional(original.updateUserId),
            updateUserName = Optional(original.updateUserName)
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysOrg/delete", new OrgIdInput { id = id });

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
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
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
                id = ReadString(value, "id"),
                value = ReadString(value, "value"),
                label = name.Length > 0 ? name : label,
                tagType = ReadString(value, "tagType")
            };
        }
        return result;
    }

    /// <summary>
    /// JSON long values are projected to strings immediately. They must never enter reactive
    /// state as JavaScript numbers because snowflake IDs exceed the precise integer range.
    /// </summary>
    private static OrgView ReadOrganization(object value)
        => new()
        {
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            pid = ReadString(value, "pid"),
            name = ReadString(value, "name"),
            code = ReadString(value, "code"),
            level = ReadInt(value, "level", 1),
            type = ReadString(value, "type"),
            directorId = ReadString(value, "directorId"),
            orderNo = ReadInt(value, "orderNo", 100),
            status = ReadInt(value, "status", 1),
            remark = ReadString(value, "remark"),
            disabled = ReadBool(value, "disabled"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName"),
            children = ReadOrganizations(Reflect.Get(value, "children"))
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
    public string id { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string type { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record OrgSaveInput
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int level { get; init; } = 1;
    public string type { get; init; } = string.Empty;
    public string? directorId { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
}

public sealed record OrgAddInput
{
    public string tenantId { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int level { get; init; } = 1;
    public string type { get; init; } = string.Empty;
    public string? directorId { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
}

public sealed record OrgUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int level { get; init; } = 1;
    public string type { get; init; } = string.Empty;
    public string? directorId { get; init; }
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record OrgIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record OrgView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string pid { get; init; } = "0";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public int level { get; init; } = 1;
    public string type { get; init; } = string.Empty;
    public string directorId { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
    public bool disabled { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
    public OrgView[] children { get; init; } = [];
}

public sealed partial record OrgFlatView : VueProps;
public sealed partial record OrgTenantOption : VueProps;
public sealed partial record OrgTypeOption : VueProps;
