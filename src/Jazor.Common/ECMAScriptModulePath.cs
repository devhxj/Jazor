using System;
using System.Linq;

namespace Jazor.Common;

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
