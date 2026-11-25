using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.Auth;

/// <summary>
/// Browser client for the public authentication subpages. Snowflake and other backend
/// <c>long</c> values stay decimal strings across the JSON boundary.
/// </summary>
[ECMAScriptModule("components/auth-subpage-client.js")]
public static class AuthSubpageClient
{
    public static IPromise<ApiOutcome> TenantList()
        => BrowserApiClient.Get("/api/sysTenant/list");

    public static IPromise<ApiOutcome> SendSms(string phone)
        => BrowserApiClient.Post("/api/sysSms/sendSms/" + EncodeURIComponent(phone) + "/0");

    public static IPromise<ApiOutcome> LoginPhone(string phone, string code, string tenantId)
        => BrowserApiClient.Post("/api/sysAuth/loginPhone", new PhoneLoginInput
        {
            phone = phone,
            code = code,
            tenantId = tenantId
        });

    public static IPromise<ApiOutcome> SystemInfo()
        => BrowserApiClient.Get("/api/sysConfig/sysInfo");

    public static AuthTenantOption[] ReadTenants(object? data)
    {
        var values = data as object?[] ?? [];
        var result = new AuthTenantOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new AuthTenantOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label"),
                host = ReadString(value, "host")
            };
        }

        return result;
    }

    public static AuthRegistrationWayOption[] ReadRegistrationWays(object? data)
    {
        var values = (data is null ? null : Reflect.Get(data, "wayList")) as object?[] ?? [];
        var result = new AuthRegistrationWayOption[values.Length];
        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index]!;
            result[index] = new AuthRegistrationWayOption
            {
                value = ReadString(value, "value"),
                label = ReadString(value, "label")
            };
        }

        return result;
    }

    public static bool ReadRegistrationEnabled(object? data)
        => data is not null && (int)NumberValue(Reflect.Get(data, "enableReg")) == 1;

    public static string ReadDefaultRegistrationWayId(object? data)
        => ReadString(data, "regWayId");

    /// <summary>YesNo enum: 1 = enabled, 2 = disabled.</summary>
    public static bool ReadCaptchaEnabled(object? data)
        => data is null || (int)NumberValue(Reflect.Get(data, "captcha")) != 2;

    public static string ReadAccessToken(object? data) => ReadString(data, "accessToken");

    public static string ReadRefreshToken(object? data) => ReadString(data, "refreshToken");

    public static string ReadHomepage(object? data) => ReadString(data, "homepage");

    private static string ReadString(object? data, string key)
    {
        if (data is null)
            return string.Empty;

        var value = Reflect.Get(data, key);
        return value is null ? string.Empty : StringValue(value);
    }
}

public sealed record PhoneLoginInput
{
    public string phone { get; init; } = string.Empty;

    public string code { get; init; } = string.Empty;

    public string tenantId { get; init; } = string.Empty;
}
