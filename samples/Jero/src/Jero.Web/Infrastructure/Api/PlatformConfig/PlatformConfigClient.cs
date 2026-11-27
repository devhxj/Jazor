using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.PlatformConfig;

/// <summary>按分组查询和维护平台配置，支持批量删除。</summary>
[ECMAScriptModule("components/platform-config-client.js")]
public static class PlatformConfigClient
{
    public static IPromise<ApiOutcome> Page(PlatformConfigPageInput input)
        => BrowserApiClient.Post("/api/sysConfig/page", input);

    public static IPromise<ApiOutcome> Groups()
        => BrowserApiClient.Get("/api/sysConfig/groupList");

    public static IPromise<ApiOutcome> Add(PlatformConfigSaveInput input)
        => BrowserApiClient.Post("/api/sysConfig/add", new PlatformConfigAddInput
        {
            Name = input.Name,
            Code = input.Code,
            Value = input.Value,
            SysFlag = input.SysFlag,
            GroupCode = input.GroupCode,
            OrderNo = input.OrderNo,
            Remark = input.Remark
        });

    public static IPromise<ApiOutcome> Update(PlatformConfigSaveInput input)
        => BrowserApiClient.Post("/api/sysConfig/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysConfig/delete", new PlatformConfigIdInput { Id = id });

    public static IPromise<ApiOutcome> BatchDelete(string[] ids)
        => BrowserApiClient.Post("/api/sysConfig/batchDelete", ids);

    public static PlatformConfigView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new PlatformConfigView[values.Length];
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

    private static PlatformConfigView ReadItem(object value)
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

public sealed record PlatformConfigPageInput
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

public sealed record PlatformConfigSaveInput
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

public sealed record PlatformConfigAddInput
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

public sealed record PlatformConfigIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record PlatformConfigView : VueProps
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
