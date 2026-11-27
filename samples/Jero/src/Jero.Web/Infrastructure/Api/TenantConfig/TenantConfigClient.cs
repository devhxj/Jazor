using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.TenantConfig;

/// <summary>按分组查询和维护租户配置，支持批量删除。</summary>
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
            Name = input.Name,
            Code = input.Code,
            Value = input.Value,
            SysFlag = input.SysFlag,
            GroupCode = input.GroupCode,
            OrderNo = input.OrderNo,
            Remark = input.Remark
        });

    public static IPromise<ApiOutcome> Update(TenantConfigSaveInput input)
        => BrowserApiClient.Post("/api/sysTenantConfig/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysTenantConfig/delete", new TenantConfigIdInput { Id = id });

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
            Id = ReadString(value, "id"),
            Name = ReadString(value, "name"),
            Code = ReadString(value, "code"),
            Value = ReadString(value, "value"),
            SysFlag = ReadInt(value, "sysFlag", 2),
            GroupCode = ReadString(value, "groupCode"),
            OrderNo = ReadInt(value, "orderNo", 100),
            Remark = ReadString(value, "remark"),
            CreateTime = ReadString(value, "createTime"),
            CreateUserId = ReadString(value, "createUserId"),
            CreateUserName = ReadString(value, "createUserName"),
            UpdateTime = ReadString(value, "updateTime"),
            UpdateUserId = ReadString(value, "updateUserId"),
            UpdateUserName = ReadString(value, "updateUserName")
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
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("field")]
    public string Field { get; init; } = "orderNo";
    [ECMAScriptName("order")]
    public string Order { get; init; } = "ascending";
    [ECMAScriptName("descStr")]
    public string DescStr { get; init; } = "descending";
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("groupCode")]
    public string GroupCode { get; init; } = string.Empty;
}

public sealed record TenantConfigSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public string Value { get; init; } = string.Empty;
    [ECMAScriptName("sysFlag")]
    public int SysFlag { get; init; } = 2;
    [ECMAScriptName("groupCode")]
    public string GroupCode { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record TenantConfigAddInput
{
    [ECMAScriptName("name")]
    public string Name { get; init; } = string.Empty;
    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;
    [ECMAScriptName("value")]
    public string Value { get; init; } = string.Empty;
    [ECMAScriptName("sysFlag")]
    public int SysFlag { get; init; } = 2;
    [ECMAScriptName("groupCode")]
    public string GroupCode { get; init; } = string.Empty;
    [ECMAScriptName("orderNo")]
    public int OrderNo { get; init; } = 100;
    [ECMAScriptName("remark")]
    public string Remark { get; init; } = string.Empty;
}

public sealed record TenantConfigIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record TenantConfigView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public int SysFlag { get; init; }
    public string GroupCode { get; init; } = string.Empty;
    public int OrderNo { get; init; }
    public string Remark { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}
