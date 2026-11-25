namespace Jero.Core;

/// <summary>
/// 字符串扩展方法
/// </summary>
public static class StringExtension
{
    /// <summary>
    /// 字符串截断
    /// </summary>
    public static string Truncate(this string str, int maxLength, string ellipsis = "...")
    {
        if (string.IsNullOrWhiteSpace(str)) return str;
        if (maxLength <= 0) return string.Empty;
        if (str.Length <= maxLength) return str;

        // 确保省略号不会导致字符串超出最大长度
        int ellipsisLength = ellipsis?.Length ?? 0;
        int truncateLength = Math.Min(maxLength, str.Length - ellipsisLength);
        return str[..truncateLength] + ellipsis;
    }

    /// <summary>
    /// 单词首字母全部大写
    /// </summary>
    public static string ToTitleCase(this string str)
    {
        return string.IsNullOrWhiteSpace(str) ? str : System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }

    /// <summary>
    /// 检查是否包含子串，忽略大小写
    /// </summary>
    public static bool ContainsIgnoreCase(this string str, string substring)
    {
        if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(substring)) return false;
        return str.Contains(substring, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 判断是否是 JSON 数据
    /// </summary>
    public static bool IsJson(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return false;
        str = str.Trim();
        return (str.StartsWith("{") && str.EndsWith("}")) || (str.StartsWith("[") && str.EndsWith("]"));
    }

    /// <summary>
    /// 判断是否是 HTML 数据
    /// </summary>
    public static bool IsHtml(this string str)
    {
        if (string.IsNullOrWhiteSpace(str)) return false;
        str = str.Trim();

        // 检查是否以 <!DOCTYPE html> 或 <html> 开头
        if (str.StartsWith("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase) || str.StartsWith("<html>", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 检查是否包含 HTML 标签
        return Regex.IsMatch(str, @"<\s*[^>]+>.*<\s*/\s*[^>]+>|<\s*[^>]+\s*/>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 字符串反转
    /// </summary>
    public static string Reverse(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        // 使用 Span<char> 提高性能
        Span<char> charSpan = stackalloc char[str.Length];
        for (int i = 0; i < str.Length; i++)
        {
            charSpan[str.Length - 1 - i] = str[i];
        }
        return new string(charSpan);
    }

    /// <summary>
    /// 转首字母小写
    /// </summary>
    public static string ToFirstLetterLowerCase(this string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        if (input.Length == 1) return input.ToLower(); // 处理单字符字符串

        return char.ToLower(input[0]) + input[1..];
    }

    /// <summary>
    /// 渲染字符串，替换占位符
    /// </summary>
    /// <param name="template">模板内容</param>
    /// <param name="parameters">参数对象</param>
    /// <returns></returns>
    public static string Render(this string template, object parameters)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;

        // 将参数转换为字典（忽略大小写）
        var paramDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (parameters != null)
        {
            foreach (var prop in parameters.GetType().GetProperties())
            {
                paramDict[prop.Name] = prop.GetValue(parameters)?.ToString() ?? string.Empty;
            }
        }

        // 使用正则表达式替换占位符
        return Regex.Replace(template, @"\{(\w+)\}", match =>
        {
            string key = match.Groups[1].Value; // 获取占位符中的 key
            return paramDict.TryGetValue(key, out var value) ? value ?? string.Empty : string.Empty;
        });
    }

    /// <summary>
    /// 驼峰转下划线
    /// </summary>
    /// <param name="str"></param>
    /// <param name="isToUpper"></param>
    /// <returns></returns>
    public static string ToUnderLine(this string str, bool isToUpper = false)
    {
        if (string.IsNullOrEmpty(str) || str.Contains("_"))
        {
            return str;
        }

        int length = str.Length;
        var result = new System.Text.StringBuilder(length + (length / 3));

        result.Append(char.ToLowerInvariant(str[0]));

        int lastIndex = length - 1;

        for (int i = 1; i < length; i++)
        {
            char current = str[i];
            if (!char.IsUpper(current))
            {
                result.Append(current);
                continue;
            }

            bool prevIsLower = char.IsLower(str[i - 1]);
            bool nextIsLower = (i < lastIndex) && char.IsLower(str[i + 1]);

            if (prevIsLower || nextIsLower)
            {
                result.Append('_');
            }

            result.Append((char)(current | 0x20));
        }

        string converted = result.ToString();
        return isToUpper ? converted.ToUpperInvariant() : converted;
    }
}
