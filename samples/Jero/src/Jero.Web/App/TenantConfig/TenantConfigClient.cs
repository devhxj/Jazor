using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.TenantConfig;

/// <summary>Typed browser boundary for tenant-scoped system configuration management.</summary>
[ECMAScriptModule("components/tenant-config-client.js")]
public static class TenantConfigClient
{
    public static IPromise<ApiOutcome> Page(TenantConfigPageInput input)
        => BrowserApiClient.Post("/api/sysTenantConfig/page", input);

    public static IPromise<ApiOutcome> Groups()
        => BrowserApiClient.Get("/api/sysTenantConfig/groupList");

    public static IPromise<ApiOutcome> Add(TenantConfigSaveInput input)
        => BrowserApiClient.Post("/api/sysTenantConfig/add", new TenantConfigAddInput
        {
            name = input.name,
            code = input.code,
            value = input.value,
            sysFlag = input.sysFlag,
            groupCode = input.groupCode,
            orderNo = input.orderNo,
            remark = input.remark
        });

    public static IPromise<ApiOutcome> Update(TenantConfigSaveInput input)
        => BrowserApiClient.Post("/api/sysTenantConfig/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysTenantConfig/delete", new TenantConfigIdInput { id = id });

    public static IPromise<ApiOutcome> BatchDelete(string[] ids)
        => BrowserApiClient.Post("/api/sysTenantConfig/batchDelete", ids);

    public static TenantConfigView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new TenantConfigView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static string[] ReadGroups(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new string[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = values[index] is null ? string.Empty : StringValue(values[index]);
        return result;
    }

    private static TenantConfigView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            name = ReadString(value, "name"),
            code = ReadString(value, "code"),
            value = ReadString(value, "value"),
            sysFlag = ReadInt(value, "sysFlag", 2),
            groupCode = ReadString(value, "groupCode"),
            orderNo = ReadInt(value, "orderNo", 100),
            remark = ReadString(value, "remark"),
            createTime = ReadString(value, "createTime"),
            createUserId = ReadString(value, "createUserId"),
            createUserName = ReadString(value, "createUserName"),
            updateTime = ReadString(value, "updateTime"),
            updateUserId = ReadString(value, "updateUserId"),
            updateUserName = ReadString(value, "updateUserName")
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
}

public sealed record TenantConfigPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string field { get; init; } = "orderNo";
    public string order { get; init; } = "ascending";
    public string descStr { get; init; } = "descending";
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string groupCode { get; init; } = string.Empty;
}

public sealed record TenantConfigSaveInput
{
    public string id { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string value { get; init; } = string.Empty;
    public int sysFlag { get; init; } = 2;
    public string groupCode { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
}

public sealed record TenantConfigAddInput
{
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string value { get; init; } = string.Empty;
    public int sysFlag { get; init; } = 2;
    public string groupCode { get; init; } = string.Empty;
    public int orderNo { get; init; } = 100;
    public string remark { get; init; } = string.Empty;
}

public sealed record TenantConfigIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record TenantConfigView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string name { get; init; } = string.Empty;
    public string code { get; init; } = string.Empty;
    public string value { get; init; } = string.Empty;
    public int sysFlag { get; init; }
    public string groupCode { get; init; } = string.Empty;
    public int orderNo { get; init; }
    public string remark { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}
