using Jero.Web.Infrastructure.Http;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Api.InfoSetting;

/// <summary>系统配置读取与保存的浏览器 API 边界。</summary>
[ECMAScriptModule("components/info-setting-client.js")]
public static class InfoSettingClient
{
    public static IPromise<ApiOutcome> GetSysInfo()
        => BrowserApiClient.Get("/api/sysConfig/sysInfo");

    public static IPromise<ApiOutcome> SaveSysInfo(InfoSettingSaveInput input)
    {
        if (input.LogoBase64.Length == 0)
        {
            // 未选择新文件时省略上传字段，保留当前 Logo。
            Reflect.DeleteProperty(input, "logoBase64");
            Reflect.DeleteProperty(input, "logoFileName");
        }
        return BrowserApiClient.Post("/api/sysConfig/saveSysInfo", input);
    }

    public static InfoSettingView ReadSysInfo(object? data)
    {
        if (data is null)
            return new InfoSettingView();

        var wayValues = Reflect.Get(data, "wayList") as object?[] ?? [];
        var ways = new InfoSettingWayOption[wayValues.Length];
        for (var index = 0; index < wayValues.Length; index++)
        {
            var value = wayValues[index];
            ways[index] = value is null
                ? new InfoSettingWayOption()
                : new InfoSettingWayOption
                {
                    Label = ReadString(value, "label"),
                    // 雪花 ID 必须保持 string，不能经过 JavaScript number。
                    Value = ReadString(value, "value")
                };
        }

        return new InfoSettingView
        {
            Logo = ReadString(data, "logo"),
            Title = ReadString(data, "title"),
            ViceTitle = ReadString(data, "viceTitle"),
            ViceDesc = ReadString(data, "viceDesc"),
            Watermark = ReadString(data, "watermark"),
            Copyright = ReadString(data, "copyright"),
            Icp = ReadString(data, "icp"),
            IcpUrl = ReadString(data, "icpUrl"),
            RegWayId = ReadString(data, "regWayId"),
            EnableReg = ReadInt(data, "enableReg", 2),
            SecondVer = ReadInt(data, "secondVer", 2),
            Captcha = ReadInt(data, "captcha", 2),
            HideTenantForLogin = ReadBool(data, "hideTenantForLogin"),
            DefaultHomePath = ReadString(data, "defaultHomePath"),
            WayList = ways
        };
    }

    private static string ReadString(object data, string key)
    {
        var value = Reflect.Get(data, key);
        if (value is null)
            return string.Empty;
        var text = StringValue(value);
        return text == "null" || text == "undefined" ? string.Empty : text;
    }

    private static int ReadInt(object data, string key, int fallback)
    {
        var value = Reflect.Get(data, key);
        return value is null ? fallback : (int)NumberValue(value);
    }

    private static bool ReadBool(object data, string key)
    {
        var value = Reflect.Get(data, key);
        return value is not null && BooleanValue(value);
    }
}

public sealed record InfoSettingSaveInput
{
    [ECMAScriptName("logoBase64")]
    public string LogoBase64 { get; init; } = string.Empty;
    [ECMAScriptName("logoFileName")]
    public string LogoFileName { get; init; } = string.Empty;
    [ECMAScriptName("watermark")]
    public string Watermark { get; init; } = string.Empty;
    [ECMAScriptName("title")]
    public string Title { get; init; } = string.Empty;
    [ECMAScriptName("viceTitle")]
    public string ViceTitle { get; init; } = string.Empty;
    [ECMAScriptName("viceDesc")]
    public string ViceDesc { get; init; } = string.Empty;
    [ECMAScriptName("copyright")]
    public string Copyright { get; init; } = string.Empty;
    [ECMAScriptName("icp")]
    public string Icp { get; init; } = string.Empty;
    [ECMAScriptName("icpUrl")]
    public string IcpUrl { get; init; } = string.Empty;
    [ECMAScriptName("enableReg")]
    public int EnableReg { get; init; } = 2;
    [ECMAScriptName("secondVer")]
    public int SecondVer { get; init; } = 2;
    [ECMAScriptName("captcha")]
    public int Captcha { get; init; } = 2;
    // 后端字段是 long；浏览器侧仍用数字字符串，禁用注册时发送 "0"。
    [ECMAScriptName("regWayId")]
    public string RegWayId { get; init; } = "0";
    [ECMAScriptName("defaultHomePath")]
    public string DefaultHomePath { get; init; } = string.Empty;
}

public sealed record InfoSettingView
{
    public string Logo { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string ViceTitle { get; init; } = string.Empty;
    public string ViceDesc { get; init; } = string.Empty;
    public string Watermark { get; init; } = string.Empty;
    public string Copyright { get; init; } = string.Empty;
    public string Icp { get; init; } = string.Empty;
    public string IcpUrl { get; init; } = string.Empty;
    public string RegWayId { get; init; } = string.Empty;
    public int EnableReg { get; init; } = 2;
    public int SecondVer { get; init; } = 2;
    public int Captcha { get; init; } = 2;
    public bool HideTenantForLogin { get; init; }
    public string DefaultHomePath { get; init; } = string.Empty;
    public InfoSettingWayOption[] WayList { get; init; } = [];
}

public sealed record InfoSettingWayOption : VueProps
{
    public string Value { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
}
