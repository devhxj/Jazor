using Jero.Web.App.Http;
using static ECMAScript.Global;

namespace Jero.Web.App.InfoSetting;

/// <summary>系统配置读取与保存的浏览器 API 边界。</summary>
[ECMAScriptModule("components/info-setting-client.js")]
public static class InfoSettingClient
{
    public static IPromise<ApiOutcome> GetSysInfo()
        => BrowserApiClient.Get("/api/sysConfig/sysInfo");

    public static IPromise<ApiOutcome> SaveSysInfo(InfoSettingSaveInput input)
    {
        if (input.logoBase64.Length == 0)
        {
            // Absence preserves the current file. Empty string keys are intentionally omitted so
            // selecting no file cannot be confused with an explicit Logo replacement.
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
                    label = ReadString(value, "label"),
                    // 雪花 ID 必须保持 string，不能经过 JavaScript number。
                    value = ReadString(value, "value")
                };
        }

        return new InfoSettingView
        {
            logo = ReadString(data, "logo"),
            title = ReadString(data, "title"),
            viceTitle = ReadString(data, "viceTitle"),
            viceDesc = ReadString(data, "viceDesc"),
            watermark = ReadString(data, "watermark"),
            copyright = ReadString(data, "copyright"),
            icp = ReadString(data, "icp"),
            icpUrl = ReadString(data, "icpUrl"),
            regWayId = ReadString(data, "regWayId"),
            enableReg = ReadInt(data, "enableReg", 2),
            secondVer = ReadInt(data, "secondVer", 2),
            captcha = ReadInt(data, "captcha", 2),
            hideTenantForLogin = ReadBool(data, "hideTenantForLogin"),
            defaultHomePath = ReadString(data, "defaultHomePath"),
            wayList = ways
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
    public string logoBase64 { get; init; } = string.Empty;
    public string logoFileName { get; init; } = string.Empty;
    public string watermark { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string viceTitle { get; init; } = string.Empty;
    public string viceDesc { get; init; } = string.Empty;
    public string copyright { get; init; } = string.Empty;
    public string icp { get; init; } = string.Empty;
    public string icpUrl { get; init; } = string.Empty;
    public int enableReg { get; init; } = 2;
    public int secondVer { get; init; } = 2;
    public int captcha { get; init; } = 2;
    // 后端字段是 long；浏览器侧仍用数字字符串，禁用注册时发送 "0"。
    public string regWayId { get; init; } = "0";
    public string defaultHomePath { get; init; } = string.Empty;
}

public sealed record InfoSettingView
{
    public string logo { get; init; } = string.Empty;
    public string title { get; init; } = string.Empty;
    public string viceTitle { get; init; } = string.Empty;
    public string viceDesc { get; init; } = string.Empty;
    public string watermark { get; init; } = string.Empty;
    public string copyright { get; init; } = string.Empty;
    public string icp { get; init; } = string.Empty;
    public string icpUrl { get; init; } = string.Empty;
    public string regWayId { get; init; } = string.Empty;
    public int enableReg { get; init; } = 2;
    public int secondVer { get; init; } = 2;
    public int captcha { get; init; } = 2;
    public bool hideTenantForLogin { get; init; }
    public string defaultHomePath { get; init; } = string.Empty;
    public InfoSettingWayOption[] wayList { get; init; } = [];
}

public sealed record InfoSettingWayOption : VueProps
{
    public string value { get; init; } = string.Empty;
    public string label { get; init; } = string.Empty;
}
