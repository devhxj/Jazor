using static ECMAScript.Global;

namespace Jero.Web.Pages.InfoSetting;

/// <summary>Logo 文件选择和 Data URL 读取边界。</summary>
[ECMAScriptModule("components/info-setting-browser-actions.js")]
public static class InfoSettingBrowserActions
{
    public static IPromise<InfoSettingLogoSelection> ReadSelectedLogo(FileRef? file)
    {
        if (file is null)
        {
            return Promise<InfoSettingLogoSelection>.Resolve(new InfoSettingLogoSelection { Ok = true });
        }

        var pending = Promise<InfoSettingLogoSelection>.WithResolvers();
        var reader = new FileReader();
        reader.Onload = _ =>
        {
            var result = reader.Result;
            var dataUrl = result is null ? string.Empty : StringValue(result);
            pending.Resolve(dataUrl.Length == 0
                ? new InfoSettingLogoSelection { Error = "读取 Logo 文件失败。" }
                : new InfoSettingLogoSelection
                {
                    Ok = true,
                    HasFile = true,
                    FileName = file.Name,
                    DataUrl = dataUrl
                });
            return true;
        };
        reader.Onerror = _ =>
        {
            pending.Resolve(new InfoSettingLogoSelection { Error = "读取 Logo 文件失败。" });
            return true;
        };
        reader.ReadAsDataURL(file);
        return pending.Promise;
    }

    public static void ClearSelectedLogo(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        if (input is not null)
            input.Value = string.Empty;
    }
}

public sealed record InfoSettingLogoSelection
{
    public bool Ok { get; init; }
    public bool HasFile { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string DataUrl { get; init; } = string.Empty;
    public string? Error { get; init; }
}
