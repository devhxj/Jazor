using static ECMAScript.Global;

namespace Jero.Web.App.OpenAccess;

/// <summary>Typed browser boundary for signature timestamps, nonce generation, and clipboard copy.</summary>
[ECMAScriptModule("components/open-access-browser-actions.js")]
public static class OpenAccessBrowserActions
{
    public static string CurrentTimestampSeconds()
    {
        // Keep this browser boundary string-only. Casting divided milliseconds to long makes Jazor
        // emit BigInt(nonInteger), which throws before the signature dialog can open.
        var milliseconds = StringValue(new Date().GetTime());
        return milliseconds.Substring(0, milliseconds.Length - 3);
    }

    public static string CreateNonce()
    {
        // randomUUID avoids relying on unqualified Math helpers in emitted ESM; the mapping keeps
        // the legacy six-digit nonce shape while retaining browser-provided entropy.
        var raw = Window.Crypto.RandomUUID().Replace("-", string.Empty);
        var value = string.Empty;
        for (var index = 0; index < 6; index++)
            value += ToDigit(raw.Substring(index, 1));
        return value;
    }

    public static void CopyText(string value)
        => Window.Navigator.Clipboard.WriteText(value);

    private static string ToDigit(string value)
    {
        if (value == "a") return "0";
        if (value == "b") return "1";
        if (value == "c") return "2";
        if (value == "d") return "3";
        if (value == "e") return "4";
        if (value == "f") return "5";
        return value;
    }
}
