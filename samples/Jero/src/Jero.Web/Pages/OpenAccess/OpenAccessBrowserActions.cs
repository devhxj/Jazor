using static ECMAScript.Global;

namespace Jero.Web.Pages.OpenAccess;

/// <summary>为签名调试窗口提供秒级时间戳、六位随机数和剪贴板操作。</summary>
[ECMAScriptModule("components/open-access-browser-actions.js")]
public static class OpenAccessBrowserActions
{
    public static string CurrentTimestampSeconds()
    {
        // 签名使用十进制秒数字符串。直接去掉毫秒的末三位，避免将带小数的值转成 BigInt。
        var milliseconds = StringValue(new Date().GetTime());
        return milliseconds.Substring(0, milliseconds.Length - 3);
    }

    public static string CreateNonce()
    {
        // 此签名接口接受六位十进制随机数，将浏览器 UUID 的前六位逐一映射到数字。
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
