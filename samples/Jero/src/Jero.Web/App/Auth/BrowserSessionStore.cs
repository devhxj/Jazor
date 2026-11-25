using static ECMAScript.Global;

namespace Jero.Web.App.Auth;

/// <summary>
/// Browser-owned authentication session. Tokens survive a full page reload and every write
/// goes through one boundary so login, transparent renewal, logout and 401 handling cannot
/// drift apart.
/// </summary>
[ECMAScriptModule("components/browser-session-store.js")]
public static class BrowserSessionStore
{
    private const string AccessTokenKey = "jero.jazor.access-token";
    private const string RefreshTokenKey = "jero.jazor.refresh-token";
    private const string EmbeddedAccessTokenKey = "jero:access-token";
    private const string EmbeddedRefreshTokenKey = "jero:x-access-token";

    public static string? AccessToken
    {
        get => Read(AccessTokenKey);
        set
        {
            Write(AccessTokenKey, value);
            Write(EmbeddedAccessTokenKey, value);
        }
    }

    public static string? RefreshToken
    {
        get => Read(RefreshTokenKey);
        set
        {
            Write(RefreshTokenKey, value);
            Write(EmbeddedRefreshTokenKey, value);
        }
    }

    public static bool HasSession => !string.IsNullOrWhiteSpace(AccessToken);

    public static void SetTokens(string? accessToken, string? refreshToken)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public static void SyncEmbeddedSession()
    {
        Write(EmbeddedAccessTokenKey, AccessToken);
        Write(EmbeddedRefreshTokenKey, RefreshToken);
    }

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
