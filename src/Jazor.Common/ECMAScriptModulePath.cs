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
    /// <summary>
    /// Validates an external ESM specifier and returns it without generated-module rewriting.
    /// 外部 ESM specifier 只做协议边界校验，保留作者提供的文本，不补扩展名或改写目录。
    /// </summary>
    public static string ValidateExternalImportSpecifier(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("ECMAScript external import specifier cannot be empty.");

        var trimmed = path.Trim();
        // Windows drive and UNC paths are filesystem locations, not portable ESM specifiers.
        if ((trimmed.Length >= 2 && char.IsLetter(trimmed[0]) && trimmed[1] == ':') ||
            trimmed.StartsWith("\\", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"ECMAScript external import specifier '{trimmed}' cannot be a disk absolute path.");
        }

        return trimmed;
    }

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

    /// <summary>
    /// Resolves a relative module import against the importer's output path.
    ///
    /// The normalizer intentionally rejects <c>..</c>; that is correct for an already resolved
    /// output path, but it cannot be used while resolving a legal import such as
    /// <c>../../components/page.mjs</c>. Resolve the segments first, then apply the same output
    /// path rules. Both compiler and emit-side producers use this method so they cannot disagree
    /// about generated-module dependency identities.
    /// </summary>
    public static string ResolveRelativePath(string importerPath, string importSpecifier)
    {
        if (string.IsNullOrWhiteSpace(importerPath))
            throw new InvalidOperationException("ECMAScriptModule importer path cannot be empty.");
        if (string.IsNullOrWhiteSpace(importSpecifier))
            throw new InvalidOperationException("ECMAScriptModule relative import cannot be empty.");

        var importer = importerPath.Replace('\\', '/').Trim();
        var specifier = importSpecifier.Replace('\\', '/').Trim();
        if (!specifier.StartsWith("./", StringComparison.Ordinal) &&
            !specifier.StartsWith("../", StringComparison.Ordinal) &&
            !string.Equals(specifier, ".", StringComparison.Ordinal) &&
            !string.Equals(specifier, "..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"ECMAScriptModule import '{importSpecifier}' is not a relative specifier.");
        }

        var importerSegments = importer
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToList();
        if (importerSegments.Count == 0)
            throw new InvalidOperationException("ECMAScriptModule importer path cannot be empty.");

        // The final segment is the importer file. Do not let a relative import pop above the
        // generated output root; an escaping path is a producer error, not a runtime fallback.
        importerSegments.RemoveAt(importerSegments.Count - 1);
        foreach (var segment in specifier.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment == ".")
                continue;
            if (segment == "..")
            {
                if (importerSegments.Count == 0)
                    throw new InvalidOperationException(
                        $"ECMAScriptModule import path cannot escape the output directory: '{importSpecifier}'.");
                importerSegments.RemoveAt(importerSegments.Count - 1);
                continue;
            }

            importerSegments.Add(segment);
        }

        return NormalizeRelativePath(string.Join("/", importerSegments));
    }

    /// <summary>
    /// Returns whether an import is resolved by a package manifest rather than Jazor's
    /// generated-module namespaces.
    /// </summary>
    public static bool IsPackageSpecifier(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var normalized = path.Replace('\\', '/').Trim();
        // System/* and @jazor/* are packaged JS resources, just like vue or vuetify.
        // Only generated-root paths and relative paths bypass a library manifest.
        return !normalized.StartsWith(".", StringComparison.Ordinal) &&
               !normalized.StartsWith("/", StringComparison.Ordinal) &&
               !normalized.StartsWith("components/", StringComparison.Ordinal) &&
               !string.Equals(normalized, "style.mjs", StringComparison.Ordinal);
    }

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
