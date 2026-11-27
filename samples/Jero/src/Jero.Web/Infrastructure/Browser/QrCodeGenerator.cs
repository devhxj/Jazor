using System.ComponentModel;
using static ECMAScript.Global;

namespace Jero.Web.Infrastructure.Browser;

/// <summary>
/// 登录和支付共用的 <c>qrcode</c> 类型化绑定，供 Emit 收集 npm 依赖。
/// </summary>
[ECMAScript("qrcode")]
internal static class QrCodeGenerator
{
    /// <summary>将文本编码为二维码图片，异步返回可给图片元素使用的数据 URL。</summary>
    [Description("@#toDataURL")]
    public extern static IPromise<string> ToDataUrl(string content, QrCodeOptions options);
}

internal sealed record QrCodeOptions
{
    [ECMAScriptName("width")]
    public int Width { get; init; }
    [ECMAScriptName("margin")]
    public int Margin { get; init; }
    [ECMAScriptName("errorCorrectionLevel")]
    public string ErrorCorrectionLevel { get; init; } = "M";
}
