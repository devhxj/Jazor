using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Auth;

/// <summary>
/// 账户认证接口客户端。请求和响应解包由 <see cref="BrowserApiClient"/> 处理。
/// </summary>
[ECMAScriptModule("components/auth-client.js")]
public static class AuthClient
{
    public static IPromise<ApiOutcome> Login(string account, string password, string code, double codeId)
        => BrowserApiClient.Post("/api/sysAuth/login", new LoginInput
        {
            Account = account,
            Password = password,
            Code = code,
            CodeId = codeId
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

    /// <summary>验证码 ID 由后端以十进制字符串传入。其范围低于 2^53，登录时可转为 JSON 数值。</summary>
    public static double ReadCaptchaId(object? data)
        => data is null ? 0 : (double)NumberValue(Reflect.Get(data, "id"));

    /// <summary>读取裸 Base64 图片内容，由页面添加数据 URL 前缀。</summary>
    public static string ReadCaptchaImage(object? data) => ReadString(data, "img");

    /// <summary>读取密码加密使用的 SM2 公钥。登录页在公钥为空字符串时会中止提交。</summary>
    public static string ReadPublicKey(object? data) => ReadString(data, "publicKey");

    /// <summary>验证码配置值转为数值后等于 2 时关闭。没有响应数据时默认启用。</summary>
    public static bool ReadCaptchaEnabled(object? data)
        => data is null || (int)NumberValue(Reflect.Get(data, "captcha")) != 2;

    private static string ReadString(object? data, string key)
        => data is null ? string.Empty : StringValue(Reflect.Get(data, key));
}

public sealed record LoginInput
{
    [ECMAScriptName("account")]
    public string Account { get; init; } = string.Empty;

    [ECMAScriptName("password")]
    public string Password { get; init; } = string.Empty;

    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;

    /// <summary>使用 <c>double</c> 以便序列化为 JSON 数值。Jazor 会将 <c>long</c> 降为 BigInt，无法直接用 JSON.stringify 序列化。</summary>
    [ECMAScriptName("codeId")]
    public double CodeId { get; init; }
}
