using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Ldap;

/// <summary>维护 LDAP 配置，按配置触发用户或组织同步。</summary>
[ECMAScriptModule("components/ldap-client.js")]
public static class LdapClient
{
    public static IPromise<ApiOutcome> Page(LdapPageInput input)
    {
        if (input.TenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysLdap/page", input);

        // 服务端的 TenantId 是不可空的 long。未选择租户时省略该字段，避免提交无法绑定的空字符串。
        return BrowserApiClient.Post("/api/sysLdap/page", new LdapPageAllInput
        {
            Page = input.Page,
            PageSize = input.PageSize,
            Keyword = input.Keyword,
            Host = input.Host
        });
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Detail(string id)
        => BrowserApiClient.Get("/api/sysLdap/detail?Id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> Add(LdapSaveInput input)
        => BrowserApiClient.Post("/api/sysLdap/add", new LdapAddInput
        {
            TenantId = input.TenantId,
            Host = input.Host,
            Port = input.Port,
            BaseDn = input.BaseDn,
            BindDn = input.BindDn,
            BindPass = input.BindPass,
            AuthFilter = input.AuthFilter,
            Version = input.Version,
            BindAttrAccount = input.BindAttrAccount,
            BindAttrEmployeeId = input.BindAttrEmployeeId,
            BindAttrCode = input.BindAttrCode,
            Status = input.Status,
            Remark = input.Remark
        });

    public static IPromise<ApiOutcome> Update(LdapSaveInput input)
        => BrowserApiClient.Post("/api/sysLdap/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysLdap/delete", new LdapIdInput { Id = id });

    public static IPromise<ApiOutcome> SyncUser(string id)
        => BrowserApiClient.Post("/api/sysLdap/syncUser", new LdapIdInput { Id = id });

    public static IPromise<ApiOutcome> SyncOrg(string id)
        => BrowserApiClient.Post("/api/sysLdap/syncDept", new LdapIdInput { Id = id });

    public static LdapView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new LdapView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static LdapTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new LdapTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new LdapTenantOption
            {
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static LdapView ReadDetail(object? data)
        => data is null ? new LdapView() : ReadItem(data);

    private static LdapView ReadItem(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            TenantId = ReadString(value, "tenantId"),
            Host = ReadString(value, "host"),
            Port = ReadInt(value, "port", 389),
            BaseDn = ReadString(value, "baseDn"),
            BindDn = ReadString(value, "bindDn"),
            BindPass = ReadString(value, "bindPass"),
            AuthFilter = ReadString(value, "authFilter"),
            Version = ReadInt(value, "version", 3),
            BindAttrAccount = ReadString(value, "bindAttrAccount"),
            BindAttrEmployeeId = ReadString(value, "bindAttrEmployeeId"),
            BindAttrCode = ReadString(value, "bindAttrCode"),
            Status = ReadInt(value, "status", 1),
            Remark = ReadString(value, "remark"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName"),
            IsDelete = ReadBool(value, "isDelete"),
            DeleteTime = ReadString(value, "deleteTime")
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

    private static bool ReadBool(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }
}

public sealed record LdapPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("host")]
    public string Host { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record LdapPageAllInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("keyword")]
    public string Keyword { get; init; } = string.Empty;
    [ECMAScriptName("host")]
    public string Host { get; init; } = string.Empty;
}

/// <summary>提交完整的 SysLdap 数据。ID 经浏览器 JSON 传输时保留为字符串。</summary>
public sealed record LdapSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("host")]
    public string Host { get; init; } = string.Empty;
    [ECMAScriptName("port")]
    public int Port { get; init; } = 389;
    [ECMAScriptName("baseDn")]
    public string BaseDn { get; init; } = string.Empty;
    [ECMAScriptName("bindDn")]
    public string BindDn { get; init; } = string.Empty;
    [ECMAScriptName("bindPass")]
    public string BindPass { get; init; } = string.Empty;
    [ECMAScriptName("authFilter")]
    public string AuthFilter { get; init; } = "sAMAccountName=%s";
    [ECMAScriptName("version")]
    public int Version { get; init; } = 3;
    [ECMAScriptName("bindAttrAccount")]
    public string BindAttrAccount { get; init; } = "sAMAccountName";
    [ECMAScriptName("bindAttrEmployeeId")]
    public string BindAttrEmployeeId { get; init; } = "EmployeeId";
    [ECMAScriptName("bindAttrCode")]
    public string BindAttrCode { get; init; } = "objectGUID";
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
    [ECMAScriptName("createTime")]
    public string CreateTime { get; init; } = string.Empty;
    [ECMAScriptName("createUserId")]
    public string? CreateUserId { get; init; }
    [ECMAScriptName("createUserName")]
    public string CreateUserName { get; init; } = string.Empty;
    [ECMAScriptName("updateTime")]
    public string? UpdateTime { get; init; }
    [ECMAScriptName("updateUserId")]
    public string? UpdateUserId { get; init; }
    [ECMAScriptName("updateUserName")]
    public string UpdateUserName { get; init; } = string.Empty;
    [ECMAScriptName("isDelete")]
    public bool IsDelete { get; init; }
    [ECMAScriptName("deleteTime")]
    public string? DeleteTime { get; init; }
}

public sealed record LdapAddInput
{
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
    [ECMAScriptName("host")]
    public string Host { get; init; } = string.Empty;
    [ECMAScriptName("port")]
    public int Port { get; init; } = 389;
    [ECMAScriptName("baseDn")]
    public string BaseDn { get; init; } = string.Empty;
    [ECMAScriptName("bindDn")]
    public string BindDn { get; init; } = string.Empty;
    [ECMAScriptName("bindPass")]
    public string BindPass { get; init; } = string.Empty;
    [ECMAScriptName("authFilter")]
    public string AuthFilter { get; init; } = "sAMAccountName=%s";
    [ECMAScriptName("version")]
    public int Version { get; init; } = 3;
    [ECMAScriptName("bindAttrAccount")]
    public string BindAttrAccount { get; init; } = "sAMAccountName";
    [ECMAScriptName("bindAttrEmployeeId")]
    public string BindAttrEmployeeId { get; init; } = "EmployeeId";
    [ECMAScriptName("bindAttrCode")]
    public string BindAttrCode { get; init; } = "objectGUID";
    [ECMAScriptName("status")]
    public int Status { get; init; } = 1;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record LdapIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record LdapView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; }
    public string BaseDn { get; init; } = string.Empty;
    public string BindDn { get; init; } = string.Empty;
    public string BindPass { get; init; } = string.Empty;
    public string AuthFilter { get; init; } = string.Empty;
    public int Version { get; init; }
    public string BindAttrAccount { get; init; } = string.Empty;
    public string BindAttrEmployeeId { get; init; } = string.Empty;
    public string BindAttrCode { get; init; } = string.Empty;
    public int Status { get; init; }
    public string Remark { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
    public bool IsDelete { get; init; }
    public string DeleteTime { get; init; } = string.Empty;
}

public sealed record LdapTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}
