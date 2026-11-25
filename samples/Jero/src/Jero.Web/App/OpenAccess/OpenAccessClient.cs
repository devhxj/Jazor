using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.OpenAccess;

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
            tenantId = tenantId
        });

    public static IPromise<ApiOutcome> CreateSecret()
        // Furion maps the Create* verb to POST and strips the verb from the action route.
        => BrowserApiClient.Post("/api/sysOpenAccess/secret", new OpenAccessEmptyInput());

    public static IPromise<ApiOutcome> GenerateSignature(OpenAccessSignatureInput input)
        => BrowserApiClient.Post("/api/sysOpenAccess/generateSignature", input);

    public static IPromise<ApiOutcome> Add(OpenAccessDraft input)
        => BrowserApiClient.Post("/api/sysOpenAccess/add", new OpenAccessAddInput
        {
            accessKey = input.accessKey,
            accessSecret = input.accessSecret,
            bindTenantId = input.bindTenantId,
            bindUserId = input.bindUserId
        });

    public static IPromise<ApiOutcome> Update(OpenAccessView original, OpenAccessDraft input)
        => BrowserApiClient.Post("/api/sysOpenAccess/update", new OpenAccessUpdateInput
        {
            id = original.id,
            accessKey = input.accessKey,
            accessSecret = input.accessSecret,
            bindTenantId = input.bindTenantId,
            bindUserId = input.bindUserId,
            createTime = NullIfEmpty(original.createTime),
            createUserId = NullIfEmpty(original.createUserId),
            createUserName = NullIfEmpty(original.createUserName),
            updateTime = NullIfEmpty(original.updateTime),
            updateUserId = NullIfEmpty(original.updateUserId),
            updateUserName = NullIfEmpty(original.updateUserName)
        });

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysOpenAccess/delete", new OpenAccessIdInput { id = id });

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
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
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
                id = ReadString(value, "id"),
                tenantId = ReadString(value, "tenantId"),
                account = ReadString(value, "account"),
                realName = ReadString(value, "realName")
            };
        }
        return result;
    }

    public static string ReadText(object? data)
        => data is null ? string.Empty : StringValue(data);

    private static OpenAccessView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            accessKey = ReadString(value, "accessKey"),
            accessSecret = ReadString(value, "accessSecret"),
            bindTenantId = ReadString(value, "bindTenantId"),
            bindTenantName = ReadString(value, "bindTenantName"),
            bindUserId = ReadString(value, "bindUserId"),
            bindUserAccount = ReadString(value, "bindUserAccount"),
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

    private static string? NullIfEmpty(string value)
        => value.Length == 0 ? null : value;
}

public sealed record OpenAccessPageInput
{
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string accessKey { get; init; } = string.Empty;
}

public sealed record OpenAccessTenantIdInput
{
    public string tenantId { get; init; } = string.Empty;
}

public sealed record OpenAccessEmptyInput;

public sealed record OpenAccessDraft
{
    public string accessKey { get; init; } = string.Empty;
    public string accessSecret { get; init; } = string.Empty;
    public string bindTenantId { get; init; } = string.Empty;
    public string bindUserId { get; init; } = string.Empty;
}

public sealed record OpenAccessAddInput
{
    public string accessKey { get; init; } = string.Empty;
    public string accessSecret { get; init; } = string.Empty;
    public string bindTenantId { get; init; } = string.Empty;
    public string bindUserId { get; init; } = string.Empty;
}

/// <summary>
/// UpdateAsync 会按实体快照更新；审计字段来自列表原始行，不能在编辑时丢弃。
/// Navigation fields are intentionally absent because the API's established Vue contract does not send them.
/// </summary>
public sealed record OpenAccessUpdateInput
{
    public string id { get; init; } = string.Empty;
    public string accessKey { get; init; } = string.Empty;
    public string accessSecret { get; init; } = string.Empty;
    public string bindTenantId { get; init; } = string.Empty;
    public string bindUserId { get; init; } = string.Empty;
    public string? createTime { get; init; }
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record OpenAccessIdInput
{
    public string id { get; init; } = string.Empty;
}

public sealed record OpenAccessSignatureInput
{
    public string accessKey { get; init; } = string.Empty;
    public string accessSecret { get; init; } = string.Empty;
    public int method { get; init; }
    public string url { get; init; } = string.Empty;
    public string timestamp { get; init; } = string.Empty;
    public string nonce { get; init; } = string.Empty;
}

public sealed record OpenAccessView : VueProps
{
    public string id { get; init; } = string.Empty;
    public string accessKey { get; init; } = string.Empty;
    public string accessSecret { get; init; } = string.Empty;
    public string bindTenantId { get; init; } = string.Empty;
    public string bindTenantName { get; init; } = string.Empty;
    public string bindUserId { get; init; } = string.Empty;
    public string bindUserAccount { get; init; } = string.Empty;
    public string createTime { get; init; } = string.Empty;
    public string createUserId { get; init; } = string.Empty;
    public string createUserName { get; init; } = string.Empty;
    public string updateTime { get; init; } = string.Empty;
    public string updateUserId { get; init; } = string.Empty;
    public string updateUserName { get; init; } = string.Empty;
}

public sealed record OpenAccessTenantOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
    public string host { get; init; } = string.Empty;
}

public sealed record OpenAccessUserOption : VueProps
{
    public string id { get; init; } = string.Empty;
    public string tenantId { get; init; } = string.Empty;
    public string account { get; init; } = string.Empty;
    public string realName { get; init; } = string.Empty;
}
