using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Ldap;

/// <summary>Typed browser boundary for LDAP configuration management.</summary>
[ECMAScriptModule("components/ldap-client.js")]
public static class LdapClient
{
    public static IPromise<ApiOutcome> Page(LdapPageInput input)
    {
        if (input.tenantId.Length > 0)
            return BrowserApiClient.Post("/api/sysLdap/page", input);

        // TenantId is a non-nullable long on the service input. Omit it when no
        // tenant is selected instead of serializing an empty string.
        return BrowserApiClient.Post("/api/sysLdap/page", new LdapPageAllInput
        {
            page = input.page,
            pageSize = input.pageSize,
            keyword = input.keyword,
            host = input.host
        });
    }

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Detail(string id)
        => BrowserApiClient.Get("/api/sysLdap/detail?Id=" + EncodeURIComponent(id));

    public static IPromise<ApiOutcome> Add(LdapSaveInput input)
        => BrowserApiClient.Post("/api/sysLdap/add", new LdapAddInput
        {
            tenantId = input.tenantId,
            host = input.host,
            port = input.port,
            baseDn = input.baseDn,
            bindDn = input.bindDn,
            bindPass = input.bindPass,
            authFilter = input.authFilter,
            version = input.version,
            bindAttrAccount = input.bindAttrAccount,
            bindAttrEmployeeId = input.bindAttrEmployeeId,
            bindAttrCode = input.bindAttrCode,
            status = input.status,
            remark = input.remark
        });

    public static IPromise<ApiOutcome> Update(LdapSaveInput input)
        => BrowserApiClient.Post("/api/sysLdap/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysLdap/delete", new LdapIdInput { id = id });

    public static IPromise<ApiOutcome> SyncUser(string id)
        => BrowserApiClient.Post("/api/sysLdap/syncUser", new LdapIdInput { id = id });

    public static IPromise<ApiOutcome> SyncOrg(string id)
        => BrowserApiClient.Post("/api/sysLdap/syncDept", new LdapIdInput { id = id });

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
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static LdapView ReadDetail(object? data)
        => data is null ? new LdapView() : ReadItem(data);

    private static LdapView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            tenantId = ReadString(value, "tenantId"),
            host = ReadString(value, "host"),
            port = ReadInt(value, "port", 389),
            baseDn = ReadString(value, "baseDn"),
            bindDn = ReadString(value, "bindDn"),
            bindPass = ReadString(value, "bindPass"),
            authFilter = ReadString(value, "authFilter"),
            version = ReadInt(value, "version", 3),
            bindAttrAccount = ReadString(value, "bindAttrAccount"),
            bindAttrEmployeeId = ReadString(value, "bindAttrEmployeeId"),
            bindAttrCode = ReadString(value, "bindAttrCode"),
            status = ReadInt(value, "status", 1),
            remark = ReadString(value, "remark"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName"),
            isDelete = ReadBool(value, "isDelete"),
            deleteTime = ReadString(value, "deleteTime")
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string keyword { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
}

public sealed record LdapPageAllInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string keyword { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}

/// <summary>Full SysLdap payload. IDs stay strings across the browser JSON boundary.</summary>
public sealed record LdapSaveInput
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
    public int port { get; init; } = 389;
    public string baseDn { get; init; } = string.Empty;
    public string bindDn { get; init; } = string.Empty;
    public string bindPass { get; init; } = string.Empty;
    public string authFilter { get; init; } = "sAMAccountName=%s";
    public int version { get; init; } = 3;
    public string bindAttrAccount { get; init; } = "sAMAccountName";
    public string bindAttrEmployeeId { get; init; } = "EmployeeId";
    public string bindAttrCode { get; init; } = "objectGUID";
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string createUserName { get; init; } = string.Empty;
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string updateUserName { get; init; } = string.Empty;
    public bool isDelete { get; init; }
    public string? deleteTime { get; init; }
}

public sealed record LdapAddInput
{
    public string tenantId { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
    public int port { get; init; } = 389;
    public string baseDn { get; init; } = string.Empty;
    public string bindDn { get; init; } = string.Empty;
    public string bindPass { get; init; } = string.Empty;
    public string authFilter { get; init; } = "sAMAccountName=%s";
    public int version { get; init; } = 3;
    public string bindAttrAccount { get; init; } = "sAMAccountName";
    public string bindAttrEmployeeId { get; init; } = "EmployeeId";
    public string bindAttrCode { get; init; } = "objectGUID";
    public int status { get; init; } = 1;
    public string remark { get; init; } = string.Empty;
}

public sealed record LdapIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record LdapView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
    public int port { get; init; }
    public string baseDn { get; init; } = string.Empty;
    public string bindDn { get; init; } = string.Empty;
    public string bindPass { get; init; } = string.Empty;
    public string authFilter { get; init; } = string.Empty;
    public int version { get; init; }
    public string bindAttrAccount { get; init; } = string.Empty;
    public string bindAttrEmployeeId { get; init; } = string.Empty;
    public string bindAttrCode { get; init; } = string.Empty;
    public int status { get; init; }
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
    public bool isDelete { get; init; }
    public string deleteTime { get; init; } = string.Empty;
}

public sealed record LdapTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}
