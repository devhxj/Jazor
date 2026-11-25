using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.WechatUser;

/// <summary>Typed browser boundary for third-party account management.</summary>
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
        => BrowserApiClient.Post("/api/sysWechatUser/delete", new WechatUserIdInput { id = id });

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
                value = value,
                label = label.Length == 0 ? "平台" + value : label,
                tagType = ReadString(item, "tagType")
            };
            target++;
        }
        return result;
    }

    private static WechatUserView ReadItem(object value)
        => new()
        {
            id = ReadString(value, "id"),
            userId = ReadString(value, "userId"),
            platformType = ReadInt(value, "platformType", 1),
            openId = ReadString(value, "openId"),
            sessionKey = ReadString(value, "sessionKey"),
            unionId = ReadString(value, "unionId"),
            nickName = ReadString(value, "nickName"),
            avatar = ReadString(value, "avatar"),
            mobile = ReadString(value, "mobile"),
            sex = ReadNullableInt(value, "sex"),
            language = ReadString(value, "language"),
            city = ReadString(value, "city"),
            province = ReadString(value, "province"),
            country = ReadString(value, "country"),
            accessToken = ReadString(value, "accessToken"),
            refreshToken = ReadString(value, "refreshToken"),
            expiresIn = ReadNullableInt(value, "expiresIn"),
            scope = ReadString(value, "scope"),
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
    public int page { get; init; } = 1;
    public int pageSize { get; init; } = 50;
    public string nickName { get; init; } = string.Empty;
    public string mobile { get; init; } = string.Empty;
}

/// <summary>
/// Full entity payload. The service adapts the request back to SysWechatUser, so fields that
/// are not editable in this page still travel with the selected row to avoid clearing bindings.
/// </summary>
public sealed record WechatUserSaveInput
{
    public string id { get; init; } = string.Empty;
    public string userId { get; init; } = "0";
    public int platformType { get; init; } = 1;
    public string openId { get; init; } = string.Empty;
    public string? sessionKey { get; init; }
    public string? unionId { get; init; }
    public string? nickName { get; init; }
    public string? avatar { get; init; }
    public string? mobile { get; init; }
    public int? sex { get; init; }
    public string? language { get; init; }
    public string? city { get; init; }
    public string? province { get; init; }
    public string? country { get; init; }
    public string? accessToken { get; init; }
    public string? refreshToken { get; init; }
    public int? expiresIn { get; init; }
    public string? scope { get; init; }
    public string createTime { get; init; } = string.Empty;
    public string? createUserId { get; init; }
    public string? createUserName { get; init; }
    public string? updateTime { get; init; }
    public string? updateUserId { get; init; }
    public string? updateUserName { get; init; }
}

public sealed record WechatUserIdInput
{
    public string id { get; init; } = string.Empty;
}
