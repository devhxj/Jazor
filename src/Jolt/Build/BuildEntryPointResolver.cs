using System.Text.RegularExpressions;

namespace Jolt.Build;

internal static class BuildEntryPointResolver
{
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private static readonly Regex ScriptTagPattern = new(
        """<script\b(?<attrs>[^>]*)\bsrc\s*=\s*["'](?<src>[^"']+)["'][^>]*>""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex ModuleTypeAttributePattern = new(
        """\btype\s*=\s*["']module["']""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly HashSet<string> SupportedEntryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".js",
        ".jsx",
        ".jazor",
        ".mjs",
        ".mts",
        ".ts",
        ".tsx",
        ".vue"
    };

    private static readonly string[] CandidateEntryPoints =
    [
        Path.Combine("src", "main.ts"),
        Path.Combine("src", "main.js"),
        Path.Combine("src", "main.mjs"),
        Path.Combine("src", "main.tsx"),
        Path.Combine("src", "main.jsx"),
        "main.ts",
        "main.js",
        "main.mjs",
        "main.tsx",
        "main.jsx"
    ];

    public static string ResolveEntryPoint(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        rootDirectory = Path.GetFullPath(rootDirectory);

        if (TryResolveFromIndexHtml(rootDirectory, out var indexHtmlEntryPoint))
        {
            return indexHtmlEntryPoint;
        }

        foreach (var candidate in CandidateEntryPoints)
        {
            var absoluteCandidate = Path.Combine(rootDirectory, candidate);
            if (TryResolveTrustedProjectFilePath(rootDirectory, absoluteCandidate, out var trustedCandidatePath))
            {
                return trustedCandidatePath;
            }
        }

        throw new InvalidOperationException(
            $"Unable to locate a frontend entry point under '{rootDirectory}'. " +
            "Add a module script to index.html or create one of the standard entry files such as src/main.ts or src/main.js.");
    }

    private static bool TryResolveFromIndexHtml(string rootDirectory, out string entryPoint)
    {
        var indexHtmlPath = Path.Combine(rootDirectory, "index.html");
        if (!TryResolveTrustedProjectFilePath(rootDirectory, indexHtmlPath, out var trustedIndexHtmlPath))
        {
            entryPoint = string.Empty;
            return false;
        }

        var html = File.ReadAllText(trustedIndexHtmlPath);
        string? fallbackCandidate = null;
        foreach (Match match in ScriptTagPattern.Matches(html))
        {
            var attrs = match.Groups["attrs"].Value;
            var src = StripQueryAndHash(match.Groups["src"].Value);
            if (string.IsNullOrWhiteSpace(src)
                || IsExternalSource(src)
                || !HasSupportedEntryExtension(src)
                || !TryResolveLocalScriptPath(rootDirectory, src, out var absolutePath)
                || !TryResolveTrustedProjectFilePath(rootDirectory, absolutePath, out var trustedAbsolutePath))
            {
                continue;
            }

            if (ModuleTypeAttributePattern.IsMatch(attrs))
            {
                entryPoint = trustedAbsolutePath;
                return true;
            }

            fallbackCandidate ??= trustedAbsolutePath;
        }

        if (fallbackCandidate is not null)
        {
            entryPoint = fallbackCandidate;
            return true;
        }

        entryPoint = string.Empty;
        return false;
    }

    private static bool TryResolveLocalScriptPath(
        string rootDirectory,
        string src,
        out string absolutePath)
    {
        var normalizedPath = src.Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
        absolutePath = Path.GetFullPath(Path.Combine(rootDirectory, normalizedPath));
        return IsInsideRoot(rootDirectory, absolutePath);
    }

    internal static bool IsTrustedProjectPath(
        string rootDirectory,
        string candidatePath,
        FileAttributes attributes)
    {
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        return IsInsideRoot(fullRootDirectory, fullCandidatePath)
            && (attributes & FileAttributes.ReparsePoint) == 0;
    }

    private static bool HasSupportedEntryExtension(string src)
        => SupportedEntryExtensions.Contains(Path.GetExtension(src));

    private static string StripQueryAndHash(string src)
    {
        var index = src.IndexOfAny(['?', '#']);
        return index >= 0 ? src[..index] : src;
    }

    private static bool IsInsideRoot(string rootDirectory, string absolutePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, absolutePath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }

    private static bool TryResolveTrustedProjectFilePath(
        string rootDirectory,
        string candidatePath,
        out string trustedPath)
    {
        trustedPath = string.Empty;
        var fullRootDirectory = Path.GetFullPath(rootDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        if (!IsInsideRoot(fullRootDirectory, fullCandidatePath) || !File.Exists(fullCandidatePath))
        {
            return false;
        }

        var inspectionPath = fullCandidatePath;
        while (!string.IsNullOrWhiteSpace(inspectionPath))
        {
            FileAttributes attributes;
            try
            {
                attributes = File.GetAttributes(inspectionPath);
            }
            catch (DirectoryNotFoundException)
            {
                return false;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            // 入口解析不跟随项目内的 reparse point，避免把工作区外脚本通过联接路径带入生产构建。
            if (!IsTrustedProjectPath(fullRootDirectory, inspectionPath, attributes))
            {
                return false;
            }

            if (string.Equals(inspectionPath, fullRootDirectory, PathComparison))
            {
                trustedPath = fullCandidatePath;
                return true;
            }

            inspectionPath = GetContainingDirectoryPath(inspectionPath);
        }

        return false;
    }

    private static string GetContainingDirectoryPath(string path)
        => Path.GetDirectoryName(path)
            ?? Path.GetPathRoot(path)
            ?? string.Empty;

    private static bool IsExternalSource(string src)
        => src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || src.StartsWith("//", StringComparison.Ordinal)
            || src.StartsWith("data:", StringComparison.OrdinalIgnoreCase);
}
