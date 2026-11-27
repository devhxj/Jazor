using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.WechatUser;

/// <summary>查询第三方账号和平台类型，并维护绑定记录。</summary>
[ECMAScriptModule("components/wechat-user-client.js")]
public static class WechatUserClient
{
    public static IPromise<ApiOutcome> Page(WechatUserPageInput input)
        => BrowserApiClient.Post("/api/sysWechatUser/page", input);

    public static IPromise<ApiOutcome> PlatformTypes()
        => BrowserApiClient.Get("/api/sysDictType/dataList?code=PlatformTypeEnum");

    public static IPromise<ApiOutcome> Update(WechatUserSaveInput input)
        => BrowserApiClient.Post("/api/sysWechatUser/update", input);

    public static IPromise<ApiOutcome> Delete(string id)
        => BrowserApiClient.Post("/api/sysWechatUser/delete", new WechatUserIdInput { Id = id });

    public static WechatUserView[] ReadItems(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "items")) as object?[] ?? [];
        var result = new WechatUserView[values.Length];
        for (var index = 0; index < values.Length; index++)
            result[index] = ReadItem(values[index]!);
        return result;
    }

    public static int ReadTotal(object? data)
        => data is null ? 0 : ReadInt(data, "total", 0);

    public static WechatUserPlatformOption[] ReadPlatformOptions(object? data)
    {
        var values = data as object?[] ?? [];
        var validCount = 0;
        for (var index = 0; index < values.Length; index++)
            if (ReadInt(values[index]!, "value", 0) > 0)
                validCount++;

        var result = new WechatUserPlatformOption[validCount];
        var target = 0;
        for (var index = 0; index < values.Length; index++)
        {
            var item = values[index]!;
            var value = ReadInt(item, "value", 0);
            if (value <= 0)
                continue;

            var label = ReadString(item, "label");
            if (label.Length == 0)
                label = ReadString(item, "name");

            result[target] = new WechatUserPlatformOption
            {
                Value = value,
                Label = label.Length == 0 ? "平台" + value : label,
                TagType = ReadString(item, "tagType")
            };
            target++;
        }
        return result;
    }

    private static WechatUserView ReadItem(object value)
        => new()
        {
            Id = ReadString(value, "id"),
            UserId = ReadString(value, "userId"),
            PlatformType = ReadInt(value, "platformType", 1),
            OpenId = ReadString(value, "openId"),
            SessionKey = ReadString(value, "sessionKey"),
            UnionId = ReadString(value, "unionId"),
            NickName = ReadString(value, "nickName"),
            Avatar = ReadString(value, "avatar"),
            Mobile = ReadString(value, "mobile"),
            Sex = ReadNullableInt(value, "sex"),
            Language = ReadString(value, "language"),
            City = ReadString(value, "city"),
            Province = ReadString(value, "province"),
            Country = ReadString(value, "country"),
            AccessToken = ReadString(value, "accessToken"),
            RefreshToken = ReadString(value, "refreshToken"),
            ExpiresIn = ReadNullableInt(value, "expiresIn"),
            Scope = ReadString(value, "scope"),
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

    private static int? ReadNullableInt(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is null ? null : (int)NumberValue(value);
    }
}

public sealed record WechatUserPageInput
{
    [ECMAScriptName("page")]
    public int Page { get; init; } = 1;
    [ECMAScriptName("pageSize")]
    public int PageSize { get; init; } = 50;
    [ECMAScriptName("nickName")]
    public string NickName { get; init; } = string.Empty;
    [ECMAScriptName("mobile")]
    public string Mobile { get; init; } = string.Empty;
}

/// <summary>
/// 服务端将请求映射回 SysWechatUser 实体。更新时带回选中行中的不可编辑字段，以保留账号绑定。
/// </summary>
public sealed record WechatUserSaveInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
    [ECMAScriptName("userId")]
    public string UserId { get; init; } = "0";
    [ECMAScriptName("platformType")]
    public int PlatformType { get; init; } = 1;
    [ECMAScriptName("openId")]
    public string OpenId { get; init; } = string.Empty;
    [ECMAScriptName("sessionKey")]
    public string? SessionKey { get; init; }
    [ECMAScriptName("unionId")]
    public string? UnionId { get; init; }
    [ECMAScriptName("nickName")]
    public string? NickName { get; init; }
    [ECMAScriptName("avatar")]
    public string? Avatar { get; init; }
    [ECMAScriptName("mobile")]
    public string? Mobile { get; init; }
    [ECMAScriptName("sex")]
    public int? Sex { get; init; }
    [ECMAScriptName("language")]
    public string? Language { get; init; }
    [ECMAScriptName("city")]
    public string? City { get; init; }
    [ECMAScriptName("province")]
    public string? Province { get; init; }
    [ECMAScriptName("country")]
    public string? Country { get; init; }
    [ECMAScriptName("accessToken")]
    public string? AccessToken { get; init; }
    [ECMAScriptName("refreshToken")]
    public string? RefreshToken { get; init; }
    [ECMAScriptName("expiresIn")]
    public int? ExpiresIn { get; init; }
    [ECMAScriptName("scope")]
    public string? Scope { get; init; }
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

public sealed record WechatUserIdInput
{
    [ECMAScriptName("id")]
    public string Id { get; init; } = string.Empty;
}
