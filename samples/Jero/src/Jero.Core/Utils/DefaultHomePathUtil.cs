namespace Jero.Core;

/// <summary>
/// 后台默认首页地址工具
/// </summary>
public static class DefaultHomePathUtil
{
    /// <summary>
    /// 系统兜底首页地址
    /// </summary>
    public const string FallbackHomePath = "/dashboard/home";

    /// <summary>
    /// 配置表 Value 字段最大长度
    /// </summary>
    public const int MaxLength = 512;

    /// <summary>
    /// 规范化首页地址，非法时返回系统兜底地址。
    /// </summary>
    public static string NormalizeOrFallback(string? value)
    {
        return TryNormalize(value, out var normalized) ? normalized : FallbackHomePath;
    }

    /// <summary>
    /// 尝试规范化后台内部路由地址。
    /// </summary>
    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;

        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed)) return false;
        if (trimmed.Length > MaxLength) return false;
        if (!trimmed.StartsWith("/", StringComparison.Ordinal)) return false;
        if (trimmed.StartsWith("//", StringComparison.Ordinal)) return false;
        if (trimmed.Contains("://", StringComparison.Ordinal)) return false;
        if (trimmed.Contains('\\')) return false;
        if (trimmed.Any(char.IsControl) || trimmed.Any(char.IsWhiteSpace)) return false;

        normalized = trimmed;
        return true;
    }
}
