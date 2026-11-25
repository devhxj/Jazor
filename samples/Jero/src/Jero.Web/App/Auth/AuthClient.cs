using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Auth;

/// <summary>
/// Authentication endpoint client. Mirrors <c>/api/sysAuth/*</c> only; the transport
/// boundary stays in <see cref="BrowserApiClient"/>.
/// </summary>
[ECMAScriptModule("components/auth-client.js")]
public static class AuthClient
{
    public static IPromise<ApiOutcome> Login(string account, string password, string code, double codeId)
        => BrowserApiClient.Post("/api/sysAuth/login", new LoginInput
        {
            account = account,
            password = password,
            code = code,
            codeId = codeId
        });

    public static IPromise<ApiOutcome> LoginConfig()
        => BrowserApiClient.Get("/api/sysAuth/loginConfig");

    public static IPromise<ApiOutcome> Captcha()
        => BrowserApiClient.Get("/api/sysAuth/captcha");

    public static IPromise<ApiOutcome> UserInfo()
        => BrowserApiClient.Get("/api/sysAuth/userInfo");

    public static IPromise<ApiOutcome> Logout()
        => BrowserApiClient.Post("/api/sysAuth/logout");

    public static IPromise<ApiOutcome> Refresh()
        => BrowserApiClient.Refresh();

    public static string ReadAccessToken(object? data) => ReadString(data, "accessToken");

    public static string ReadRefreshToken(object? data) => ReadString(data, "refreshToken");

    public static string ReadHomepage(object? data) => ReadString(data, "homepage");

    /// <summary>Captcha id arrives as a long serialized to string (JS precision protection);
    /// read back numerically for the login DTO. Ids stay below 2^53, and the DTO member is
    /// double because Jazor lowers C# long to JS BigInt, which JSON.stringify rejects.</summary>
    public static double ReadCaptchaId(object? data)
        => data is null ? 0 : (double)NumberValue(Reflect.Get(data, "id"));

    /// <summary>The img field is bare base64; the page owns the data-URL prefix.</summary>
    public static string ReadCaptchaImage(object? data) => ReadString(data, "img");

    /// <summary>SM2 public key for password encryption; empty means the backend wants plaintext.</summary>
    public static string ReadPublicKey(object? data) => ReadString(data, "publicKey");

    /// <summary>loginConfig.captcha follows the YesNo enum: 1 = enabled, 2 = disabled (runtime-observed).</summary>
    public static bool ReadCaptchaEnabled(object? data)
        => data is null || (int)NumberValue(Reflect.Get(data, "captcha")) != 2;

    private static string ReadString(object? data, string key)
        => data is null ? string.Empty : StringValue(Reflect.Get(data, key));
}

public sealed record LoginInput
{
    public string account { get; init; } = string.Empty;

    public string password { get; init; } = string.Empty;

    public string code { get; init; } = string.Empty;

    /// <summary>double, not long: Jazor lowers long to JS BigInt which JSON.stringify rejects;
    /// the backend binds the JSON number to its long CodeId, and ids stay below 2^53.</summary>
    public double codeId { get; init; }
}
