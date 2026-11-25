#if NET10_0_OR_GREATER

namespace Jero.Core.Update;

/// <summary>
/// 自动版本更新使用的版本比较器。
/// </summary>
public static class AutoVersionUpdateVersionComparer
{
    /// <summary>
    /// 比较两个版本号。优先按 <see cref="Version"/> 语义比较，无法解析时按字符串比较兜底。
    /// </summary>
    public static int Compare(string? left, string? right)
    {
        if (string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (string.IsNullOrWhiteSpace(left))
        {
            return string.IsNullOrWhiteSpace(right) ? 0 : -1;
        }

        if (string.IsNullOrWhiteSpace(right))
        {
            return 1;
        }

        if (TryParseVersion(left, out var leftVersion) &&
            TryParseVersion(right, out var rightVersion))
        {
            return leftVersion.CompareTo(rightVersion);
        }

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseVersion(string value, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Version? version)
    {
        var normalizedValue = value.Trim();
        if (Version.TryParse(normalizedValue, out var parsedVersion))
        {
            version = NormalizeVersion(parsedVersion);
            return true;
        }

        var suffixIndex = normalizedValue.IndexOfAny(['-', '+']);
        if (suffixIndex > 0)
        {
            if (Version.TryParse(normalizedValue[..suffixIndex], out parsedVersion))
            {
                version = NormalizeVersion(parsedVersion);
                return true;
            }
        }

        version = null;
        return false;
    }

    private static Version NormalizeVersion(Version version)
    {
        return new Version(
            Math.Max(0, version.Major),
            Math.Max(0, version.Minor),
            Math.Max(0, version.Build),
            Math.Max(0, version.Revision));
    }
}

#endif // NET10_0_OR_GREATER
