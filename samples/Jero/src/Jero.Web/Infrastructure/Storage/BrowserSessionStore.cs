using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Storage;

/// <summary>
/// 将登录令牌保存在浏览器本地存储中，并同步维护嵌入页面读取的键。
/// </summary>
[ECMAScriptModule("components/browser-session-store.js")]
public static class BrowserSessionStore
{
    private const string AccessTokenKey = "jero.jazor.access-token";
    private const string RefreshTokenKey = "jero.jazor.refresh-token";
    private const string EmbeddedAccessTokenKey = "jero:access-token";
    private const string EmbeddedRefreshTokenKey = "jero:x-access-token";

    /// <summary>读取访问令牌。写入时同步更新嵌入页面使用的访问令牌键。</summary>
    public static string? AccessToken
    {
        get => Read(AccessTokenKey);
        set
        {
            Write(AccessTokenKey, value);
            Write(EmbeddedAccessTokenKey, value);
        }
    }

    /// <summary>读取刷新令牌。写入时同步更新嵌入页面使用的刷新令牌键。</summary>
    public static string? RefreshToken
    {
        get => Read(RefreshTokenKey);
        set
        {
            Write(RefreshTokenKey, value);
            Write(EmbeddedRefreshTokenKey, value);
        }
    }

    /// <summary>指示本地是否存有访问令牌。令牌是否仍然有效由服务端判断。</summary>
    public static bool HasSession => !string.IsNullOrWhiteSpace(AccessToken);

    /// <summary>更新两项令牌。每一项为 null、空串或仅含空白时，删除该项对应的两个存储键。</summary>
    public static void SetTokens(string? accessToken, string? refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    /// <summary>将当前登录令牌同步到嵌入页面使用的存储键。</summary>
    public static void SyncEmbeddedSession()
    {
        Write(EmbeddedAccessTokenKey, AccessToken);
        Write(EmbeddedRefreshTokenKey, RefreshToken);
    }

    /// <summary>清除应用和嵌入页面的访问令牌及刷新令牌。</summary>
    public static void Clear()
    {
        Window.LocalStorage.RemoveItem(AccessTokenKey);
        Window.LocalStorage.RemoveItem(RefreshTokenKey);
        Window.LocalStorage.RemoveItem(EmbeddedAccessTokenKey);
        Window.LocalStorage.RemoveItem(EmbeddedRefreshTokenKey);
    }

    private static string? Read(string key)
    {
        var value = Window.LocalStorage.GetItem(key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void Write(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Window.LocalStorage.RemoveItem(key);
            return;
        }

        Window.LocalStorage.SetItem(key, value);
    }
}
