using System.ComponentModel;
using static ECMAScript.Global;

namespace Jero.Web.App.Components;

/// <summary>
/// Minimal qrcode 1.5.4 host contract shared by authentication and payment features.
/// Keep this boundary typed so Emit can collect the npm dependency without a JavaScript bridge.
/// </summary>
[ECMAScript("qrcode")]
internal static class QrCodeGenerator
{
    [Description("@#toDataURL")]
    public extern static IPromise<string> ToDataUrl(string content, QrCodeOptions options);
}

internal sealed record QrCodeOptions
{
    public int width { get; init; }
    public int margin { get; init; }
    public string errorCorrectionLevel { get; init; } = "M";
}
