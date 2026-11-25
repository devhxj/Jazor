


using Microsoft.AspNetCore.Authentication;
namespace Jero.Core;

public static class HttpContextExtension
{
    public static async Task<AuthenticationScheme[]> GetExternalProvidersAsync(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var schemes = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

        return (from scheme in await schemes.GetAllSchemesAsync()
                where !string.IsNullOrEmpty(scheme.DisplayName)
                select scheme).ToArray();
    }

    public static async Task<bool> IsProviderSupportedAsync(this HttpContext context, string provider)
    {
        ArgumentNullException.ThrowIfNull(context);

        return (from scheme in await context.GetExternalProvidersAsync()
                where string.Equals(scheme.Name, provider, StringComparison.OrdinalIgnoreCase)
                select scheme).Any();
    }

    /// <summary>
    /// 获取设备信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetClientDeviceInfo(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return CommonUtil.GetClientDeviceInfo(context.Request.Headers.UserAgent.ToString());
    }

    /// <summary>
    /// 获取浏览器信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetClientBrowser(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string userAgent = context.Request.Headers.UserAgent.ToString();
        try
        {
            if (userAgent != null)
            {
                var client = Parser.GetDefault().Parse(userAgent);
                if (client.Device.IsSpider)
                    return "爬虫";
                return $"{client.UA.Family} {client.UA.Major}.{client.UA.Minor} / {client.Device.Family}";
            }
        }
        catch
        { }
        return "未知";
    }

    /// <summary>
    /// 获取操作系统信息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetClientOs(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string userAgent = context.Request.Headers.UserAgent.ToString();
        try
        {
            if (userAgent != null)
            {
                var client = Parser.GetDefault().Parse(userAgent);
                if (client.Device.IsSpider)
                    return "爬虫";
                return $"{client.OS.Family} {client.OS.Major} {client.OS.Minor}";
            }
        }
        catch
        { }
        return "未知";
    }
}
