using Jero.Web.Application;
using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.Auth;

/// <summary>
/// 公开认证子页面的接口客户端。雪花 ID 等后端 <c>long</c> 值经 JSON 传输时保留十进制字符串。
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
            Phone = phone,
            Code = code,
            TenantId = tenantId
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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label"),
                Host = ReadString(value, "host")
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
                Value = ReadString(value, "value"),
                Label = ReadString(value, "label")
            };
        }

        return result;
    }

    public static bool ReadRegistrationEnabled(object? data)
        => data is not null && (int)NumberValue(Reflect.Get(data, "enableReg")) == 1;

    public static string ReadDefaultRegistrationWayId(object? data)
        => ReadString(data, "regWayId");

    /// <summary>验证码配置值转为数值后等于 2 时关闭。没有响应数据时默认启用。</summary>
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
    [ECMAScriptName("phone")]
    public string Phone { get; init; } = string.Empty;

    [ECMAScriptName("code")]
    public string Code { get; init; } = string.Empty;

    [ECMAScriptName("tenantId")]
    public string TenantId { get; init; } = string.Empty;
}
