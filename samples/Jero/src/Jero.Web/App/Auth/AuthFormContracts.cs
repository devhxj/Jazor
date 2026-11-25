using ECMAScript;

namespace Jero.Web.App.Auth;

public sealed record AuthTenantOption
{
    public required string value { get; init; }

    public required string label { get; init; }

    public string host { get; init; } = string.Empty;
}

public sealed record AuthRegistrationWayOption
{
    public required string value { get; init; }

    public required string label { get; init; }
}


[ECMAScript("@jero/runtime/components/auth-client.js")]
internal static class PasswordAuth
{
    [System.ComponentModel.Description("@#Login")]
    public static extern ECMAScript.IPromise<AuthResponse> Login(string account, string password, string code, double codeId);
    [System.ComponentModel.Description("@#LoginConfig")]
    public static extern ECMAScript.IPromise<AuthResponse> LoginConfig();
    [System.ComponentModel.Description("@#Captcha")]
    public static extern ECMAScript.IPromise<AuthResponse> Captcha();
    [System.ComponentModel.Description("@#ReadCaptchaId")]
    public static extern double ReadCaptchaId(object? data);
    [System.ComponentModel.Description("@#ReadCaptchaImage")]
    public static extern string ReadCaptchaImage(object? data);
    [System.ComponentModel.Description("@#ReadPublicKey")]
    public static extern string ReadPublicKey(object? data);
    [System.ComponentModel.Description("@#ReadCaptchaEnabled")]
    public static extern bool ReadCaptchaEnabled(object? data);
}

[ECMAScript("@jero/runtime/components/auth-subpage-client.js")]
internal static class PublicAuth
{
    [System.ComponentModel.Description("@#TenantList")]
    public static extern ECMAScript.IPromise<AuthResponse> TenantList();
    [System.ComponentModel.Description("@#SendSms")]
    public static extern ECMAScript.IPromise<AuthResponse> SendSms(string phone);
    [System.ComponentModel.Description("@#LoginPhone")]
    public static extern ECMAScript.IPromise<AuthResponse> LoginPhone(string phone, string code, string tenantId);
    [System.ComponentModel.Description("@#SystemInfo")]
    public static extern ECMAScript.IPromise<AuthResponse> SystemInfo();
    [System.ComponentModel.Description("@#ReadTenants")]
    public static extern AuthTenantOption[] ReadTenants(object? data);
    [System.ComponentModel.Description("@#ReadRegistrationWays")]
    public static extern AuthRegistrationWayOption[] ReadRegistrationWays(object? data);
    [System.ComponentModel.Description("@#ReadRegistrationEnabled")]
    public static extern bool ReadRegistrationEnabled(object? data);
    [System.ComponentModel.Description("@#ReadDefaultRegistrationWayId")]
    public static extern string ReadDefaultRegistrationWayId(object? data);
    [System.ComponentModel.Description("@#ReadCaptchaEnabled")]
    public static extern bool ReadCaptchaEnabled(object? data);
}

internal sealed record AuthResponse(bool Ok, AuthResponseData? Data, string? Error);

internal sealed record AuthResponseData(string? accessToken, string? refreshToken, string? homepage);

[ECMAScript("/route-modules.mjs")]
internal static class AuthSession
{
    [System.ComponentModel.Description("@#completeSignIn")]
    public static extern IPromise<RouteNavigationResult?> CompleteSignIn(Router router, string? accessToken, string? refreshToken, string? home);
}

[ECMAScript("qrcode")]
internal static class LoginQrCode
{
    [System.ComponentModel.Description("@#toDataURL")]
    public static extern ECMAScript.IPromise<string> ToDataUrl(string content, LoginQrOptions options);
}

internal sealed record LoginQrOptions(int width, int margin, string errorCorrectionLevel);
