using System;
using System.Linq;

namespace Jazor.Common;

/// <summary>
/// 规范化 ECMAScript module 的相对路径和 import specifier。
/// </summary>
/// <remarks>
/// 该规则同时服务 generator、compiler 和 emit 层，统一斜杠、扩展名和根相对前缀，
/// 并拒绝通过 <c>..</c> 逃出输出目录。调用方不应各自重新拼接模块路径。
/// </remarks>
public static class ECMAScriptModulePath
{
    public static string NormalizeRelativePath(string path)
        => NormalizeCore(path, includeRelativePrefix: false);

    public static string NormalizeImportSpecifier(string path)
    {
        var trimmed = path.Trim();
        var includeRelativePrefix = trimmed.StartsWith("./", StringComparison.Ordinal) ||
                                    trimmed.StartsWith("../", StringComparison.Ordinal) ||
                                    trimmed.StartsWith("/", StringComparison.Ordinal);
        if (!includeRelativePrefix)
            return trimmed;

        return NormalizeCore(trimmed, includeRelativePrefix);
    }

    public static string NormalizeRootRelativeImportSpecifier(string path)
        => NormalizeCore(path, includeRelativePrefix: true);

    private static string NormalizeCore(string path, bool includeRelativePrefix)
    {
        var normalized = path.Replace('\\', '/').Trim();
        while (normalized.StartsWith("./", StringComparison.Ordinal))
            normalized = normalized.Substring(2);

        normalized = normalized.TrimStart('/');
        var segments = normalized
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0)
            throw new InvalidOperationException("ECMAScriptModule import path cannot be empty.");
        if (segments.Any(static segment => segment == ".."))
            throw new InvalidOperationException("ECMAScriptModule import path cannot escape the output directory.");

        normalized = string.Join("/", segments);
        if (!normalized.EndsWith(".js", StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase))
        {
            normalized += ".mjs";
        }

        return includeRelativePrefix ? "./" + normalized : normalized;
    }
}
