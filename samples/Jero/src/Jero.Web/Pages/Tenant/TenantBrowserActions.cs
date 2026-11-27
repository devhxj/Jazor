using static ECMAScript.Global;

namespace Jero.Web.Pages.Tenant;

/// <summary>租户 Logo 的原生 FileReader 边界。</summary>
[ECMAScriptModule("components/tenant-browser-actions.js")]
public static class TenantBrowserActions
{
    public static IPromise<TenantLogoOutcome> ReadSelectedLogo(string elementId)
    {
        var input = Document.GetElementById(elementId) as HTMLInputElement;
        var file = input?.Files?.GetItem(0);
        if (file is null)
        {
            return Promise<TenantLogoOutcome>.Resolve(new TenantLogoOutcome
            {
                Ok = false,
                Error = "请先选择 Logo 图片。"
            });
        }

        var pending = Promise<TenantLogoOutcome>.WithResolvers();
        var reader = new FileReader();
        reader.Onload = _ =>
        {
            // 后端正则要求完整 data:image/...;base64,...，不可像旧 Vue 一样剥离前缀。
            var result = reader.Result;
            var dataUrl = result is null ? string.Empty : StringValue(result);
            pending.Resolve(new TenantLogoOutcome
            {
                Ok = dataUrl.Length > 0,
                FileName = file.Name,
                DataUrl = dataUrl,
                Error = dataUrl.Length == 0 ? "Logo 图片读取结果为空。" : null
            });
            return true;
        };
        reader.Onerror = _ =>
        {
            pending.Resolve(new TenantLogoOutcome
            {
                Ok = false,
                Error = "Logo 图片读取失败。"
            });
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

public sealed record TenantLogoOutcome
{
    public bool Ok { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string DataUrl { get; init; } = string.Empty;
    public string? Error { get; init; }
}
