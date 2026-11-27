using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.OpenAccess;

/// <summary>
/// 开放接口浏览器 API。所有 long / 雪花 ID 都以十进制字符串跨越 JSON 边界，
/// 避免 JavaScript Number 精度截断后绑定到错误租户或用户。
/// </summary>
[ECMAScriptModule("components/open-access-client.js")]
public static class OpenAccessClient
{
    public static IPromise<ApiOutcome> Page(OpenAccessPageInput input)
        => BrowserApiClient.Post("/api/sysOpenAccess/page", input);

    public static IPromise<ApiOutcome> Tenants()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> Users(string tenantId)
        => BrowserApiClient.Post("/api/sysTenant/userList", new OpenAccessTenantIdInput
        {
            TenantId = tenantId
        });

    public static IPromise<ApiOutcome> CreateSecret()
        // Furion 将 Create 前缀映射为 POST，生成操作路由时会去掉该前缀。
        => BrowserApiClient.Post("/api/sysOpenAccess/secret", new OpenAccessEmptyInput());

    public static IPromise<ApiOutcome> GenerateSignature(OpenAccessSignatureInput input)
        => BrowserApiClient.Post("/api/sysOpenAccess/generateSignature", input);

    public static IPromise<ApiOutcome> Add(OpenAccessDraft input)
        => BrowserApiClient.Post("/api/sysOpenAccess/add", new OpenAccessAddInput
        {
            AccessKey = input.AccessKey,
            AccessSecret = input.AccessSecret,
            BindTenantId = input.BindTenantId,
            BindUserId = input.BindUserId
        });

    public static IPromise<ApiOutcome> Update(OpenAccessView original, OpenAccessDraft input)
        => BrowserApiClient.Post("/api/sysOpenAccess/update", new OpenAccessUpdateInput
        {
            Id = original.Id,
            AccessKey = input.AccessKey,
            AccessSecret = input.AccessSecret,
            BindTenantId = input.BindTenantId,
            BindUserId = input.BindUserId,
            CreateTime = NullIfEmpty(original.CreateTime),
            CreateUserId = NullIfEmpty(original.CreateUserId),
            CreateUserName = NullIfEmpty(original.CreateUserName),
            UpdateTime = NullIfEmpty(original.UpdateTime),
            UpdateUserId = NullIfEmpty(original.UpdateUserId),
            UpdateUserName = NullIfEmpty(original.UpdateUserName)
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysOpenAccess/delete", new OpenAccessIdInput { Id = id });

    public static OpenAccessView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new OpenAccessView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static OpenAccessTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new OpenAccessTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new OpenAccessTenantOption
            {
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
            };
        }
        return result;
    }

    public static OpenAccessUserOption[] ReadUsers(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new OpenAccessUserOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new OpenAccessUserOption
            {
                Id = ReadString(value, "id"),
                TenantId = ReadString(value, "tenantId"),
                Account = ReadString(value, "account"),
                RealName = ReadString(value, "realName")
            };
        }
        return result;
    }

    public static string ReadText(object? data)
        => data is null ? string.Empty : StringValue(data);

    private static OpenAccessView ReadItem(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            AccessKey = ReadString(value, "accessKey"),
            AccessSecret = ReadString(value, "accessSecret"),
            BindTenantId = ReadString(value, "bindTenantId"),
            BindTenantName = ReadString(value, "bindTenantName"),
            BindUserId = ReadString(value, "bindUserId"),
            BindUserAccount = ReadString(value, "bindUserAccount"),
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

    private static string? NullIfEmpty(string value)
        => value.Length == 0 ? null : value;
}

public sealed record OpenAccessPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("accessKey")]
    public string AccessKey { get; init; } = string.Empty;
}

public sealed record OpenAccessTenantIdInput
{
    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}

public sealed record OpenAccessEmptyInput;

public sealed record OpenAccessDraft
{
    public string AccessKey { get; init; } = string.Empty;
    public string AccessSecret { get; init; } = string.Empty;
    public string BindTenantId { get; init; } = string.Empty;
    public string BindUserId { get; init; } = string.Empty;
}

public sealed record OpenAccessAddInput
{
    [ECMAScriptName("accessKey")]
    public string AccessKey { get; init; } = string.Empty;
    [ECMAScriptName("accessSecret")]
    public string AccessSecret { get; init; } = string.Empty;
    [ECMAScriptName("bindTenantId")]
    public string BindTenantId { get; init; } = string.Empty;
    [ECMAScriptName("bindUserId")]
    public string BindUserId { get; init; } = string.Empty;
}

/// <summary>
/// UpdateAsync 会按实体快照更新；审计字段来自列表原始行，不能在编辑时丢弃。
/// 更新请求沿用页面现有的字段契约，省略实体导航属性。
/// </summary>
public sealed record OpenAccessUpdateInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("accessKey")]
    public string AccessKey { get; init; } = string.Empty;
    [ECMAScriptName("accessSecret")]
    public string AccessSecret { get; init; } = string.Empty;
    [ECMAScriptName("bindTenantId")]
    public string BindTenantId { get; init; } = string.Empty;
    [ECMAScriptName("bindUserId")]
    public string BindUserId { get; init; } = string.Empty;
    [ECMAScriptName("createTime")]
    public string? CreateTime { get; init; }
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

public sealed record OpenAccessIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record OpenAccessSignatureInput
{
    [ECMAScriptName("accessKey")]
    public string AccessKey { get; init; } = string.Empty;
    [ECMAScriptName("accessSecret")]
    public string AccessSecret { get; init; } = string.Empty;
    [ECMAScriptName("method")]
    public int Method { get; init; }
    [ECMAScriptName("url")]
    public string Url { get; init; } = string.Empty;
    [ECMAScriptName("timestamp")]
    public string Timestamp { get; init; } = string.Empty;
    [ECMAScriptName("nonce")]
    public string Nonce { get; init; } = string.Empty;
}

public sealed record OpenAccessView : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string AccessSecret { get; init; } = string.Empty;
    public string BindTenantId { get; init; } = string.Empty;
    public string BindTenantName { get; init; } = string.Empty;
    public string BindUserId { get; init; } = string.Empty;
    public string BindUserAccount { get; init; } = string.Empty;
    public string CreateTime { get; init; } = string.Empty;
    public string CreateUserId { get; init; } = string.Empty;
    public string CreateUserName { get; init; } = string.Empty;
    public string UpdateTime { get; init; } = string.Empty;
    public string UpdateUserId { get; init; } = string.Empty;
    public string UpdateUserName { get; init; } = string.Empty;
}

public sealed record OpenAccessTenantOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
}

public sealed record OpenAccessUserOption : VueProps
{
    public string Id { get; init; } = string.Empty;
    public string TenantId { get; init; } = string.Empty;
    public string Account { get; init; } = string.Empty;
    public string RealName { get; init; } = string.Empty;
}
