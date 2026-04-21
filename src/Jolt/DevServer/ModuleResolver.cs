using Jazor.VueContracts.Protocol;

namespace Jolt.DevServer;

internal sealed class ModuleResolver
{
    private static readonly string[] SupportedExtensions =
    [
        ".jazor",
        ".vue",
        ".ts",
        ".js",
        ".css",
        ".html"
    ];

    private readonly string _rootDirectory;
    private readonly IReadOnlyList<ResolveAliasRule> _resolveAliasRules;

    public ModuleResolver(
        string rootDirectory,
        IReadOnlyDictionary<string, string>? resolveAliases = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        _rootDirectory = Path.GetFullPath(rootDirectory);
        _resolveAliasRules = CreateResolveAliasRules(resolveAliases);
    }

    public ResolveResult Resolve(string requestPath, string? importerPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestPath);

        var sanitizedRequestPath = StripQueryAndHash(requestPath);
        if (string.Equals(sanitizedRequestPath, "/", StringComparison.Ordinal))
        {
            return ResolveAbsolutePath(Path.Combine(_rootDirectory, "index.html"), "/index.html");
        }

        if (sanitizedRequestPath.StartsWith("/@jazor/", StringComparison.Ordinal))
        {
            return new ResolveResult
            {
                AbsolutePath = sanitizedRequestPath,
                ResolvedUrl = sanitizedRequestPath,
                DocumentKind = DocumentKind.Unknown,
                IsVirtual = true,
                Found = true
            };
        }

        if (TryResolveAliasPath(sanitizedRequestPath, out var aliasedAbsolutePath))
        {
            return ResolveCandidate(aliasedAbsolutePath, BuildResolvedUrl(aliasedAbsolutePath));
        }

        if (sanitizedRequestPath.StartsWith("/", StringComparison.Ordinal))
        {
            var relativePath = sanitizedRequestPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return ResolveCandidate(Path.Combine(_rootDirectory, relativePath), sanitizedRequestPath);
        }

        var baseDirectory = importerPath is null
            ? _rootDirectory
            : Path.GetDirectoryName(importerPath) ?? _rootDirectory;
        var combinedPath = Path.GetFullPath(Path.Combine(baseDirectory, sanitizedRequestPath.Replace('/', Path.DirectorySeparatorChar)));
        var combinedUrl = BuildResolvedUrl(combinedPath);
        return ResolveCandidate(combinedPath, combinedUrl);
    }

    public string GetResolvedUrlForAbsolutePath(string absolutePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);

        var fullPath = Path.GetFullPath(absolutePath);
        if (!IsInsideRoot(fullPath))
        {
            throw new InvalidOperationException("Resolved path escapes the dev-server root.");
        }

        return BuildResolvedUrl(fullPath);
    }

    public string GetStyleTargetIdForAbsolutePath(string absolutePath)
        => GetResolvedUrlForAbsolutePath(absolutePath);

    private ResolveResult ResolveCandidate(string absolutePath, string resolvedUrl)
    {
        if (!string.IsNullOrWhiteSpace(Path.GetExtension(absolutePath)))
        {
            return ResolveAbsolutePath(absolutePath, resolvedUrl);
        }

        foreach (var extension in SupportedExtensions)
        {
            var candidate = absolutePath + extension;
            var result = ResolveAbsolutePath(candidate, BuildResolvedUrl(candidate));
            if (result.Found)
            {
                return result;
            }
        }

        return new ResolveResult
        {
            AbsolutePath = absolutePath,
            ResolvedUrl = resolvedUrl,
            DocumentKind = DocumentKind.Unknown,
            IsVirtual = false,
            Found = false,
            Error = $"Could not resolve '{resolvedUrl}'."
        };
    }

    private ResolveResult ResolveAbsolutePath(string absolutePath, string resolvedUrl)
    {
        var fullPath = Path.GetFullPath(absolutePath);
        if (!IsInsideRoot(fullPath))
        {
            return new ResolveResult
            {
                AbsolutePath = fullPath,
                ResolvedUrl = resolvedUrl,
                DocumentKind = DocumentKind.Unknown,
                IsVirtual = false,
                Found = false,
                Error = "Resolved path escapes the dev-server root."
            };
        }

        return new ResolveResult
        {
            AbsolutePath = fullPath,
            ResolvedUrl = resolvedUrl,
            DocumentKind = MapDocumentKind(fullPath),
            IsVirtual = false,
            Found = File.Exists(fullPath),
            Error = File.Exists(fullPath) ? null : $"File '{resolvedUrl}' was not found."
        };
    }

    private string BuildResolvedUrl(string absolutePath)
    {
        var relativePath = Path.GetRelativePath(_rootDirectory, absolutePath)
            .Replace('\\', '/');
        return relativePath.StartsWith("../", StringComparison.Ordinal)
            ? "/" + Path.GetFileName(absolutePath)
            : "/" + relativePath;
    }

    private bool IsInsideRoot(string fullPath)
    {
        var relativePath = Path.GetRelativePath(_rootDirectory, fullPath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }

    private static string StripQueryAndHash(string requestPath)
    {
        var queryIndex = requestPath.IndexOfAny(['?', '#']);
        return queryIndex >= 0
            ? requestPath[..queryIndex]
            : requestPath;
    }

    private static DocumentKind MapDocumentKind(string documentPath)
        => Path.GetExtension(documentPath).ToLowerInvariant() switch
        {
            ".jazor" => DocumentKind.Jazor,
            ".vue" => DocumentKind.Vue,
            ".ts" => DocumentKind.TypeScript,
            ".js" => DocumentKind.JavaScript,
            ".css" => DocumentKind.Css,
            _ => DocumentKind.Unknown
        };

    private bool TryResolveAliasPath(string requestPath, out string absolutePath)
    {
        absolutePath = string.Empty;
        if (_resolveAliasRules.Count == 0)
        {
            return false;
        }

        foreach (var aliasRule in _resolveAliasRules)
        {
            if (!TryMatchAlias(requestPath, aliasRule.Prefix, out var suffix))
            {
                continue;
            }

            absolutePath = string.IsNullOrEmpty(suffix)
                ? aliasRule.AbsoluteTargetPath
                : Path.Combine(aliasRule.AbsoluteTargetPath, suffix.Replace('/', Path.DirectorySeparatorChar));
            return true;
        }

        return false;
    }

    private IReadOnlyList<ResolveAliasRule> CreateResolveAliasRules(
        IReadOnlyDictionary<string, string>? resolveAliases)
    {
        if (resolveAliases is null || resolveAliases.Count == 0)
        {
            return [];
        }

        var rules = new List<ResolveAliasRule>(resolveAliases.Count);
        foreach (var (rawPrefix, rawTarget) in resolveAliases)
        {
            if (string.IsNullOrWhiteSpace(rawPrefix) || string.IsNullOrWhiteSpace(rawTarget))
            {
                continue;
            }

            var normalizedPrefix = NormalizeAliasPrefix(rawPrefix);
            if (string.IsNullOrWhiteSpace(normalizedPrefix))
            {
                continue;
            }

            rules.Add(new ResolveAliasRule(
                normalizedPrefix,
                ToAbsoluteAliasTargetPath(rawTarget)));
        }

        rules.Sort(static (left, right) => right.Prefix.Length.CompareTo(left.Prefix.Length));
        return rules;
    }

    private string ToAbsoluteAliasTargetPath(string rawTarget)
    {
        var trimmedTarget = rawTarget.Trim();
        if (trimmedTarget.StartsWith("/", StringComparison.Ordinal))
        {
            var relativePath = trimmedTarget.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));
        }

        var normalizedTarget = trimmedTarget.Replace('/', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalizedTarget))
        {
            return Path.GetFullPath(normalizedTarget);
        }

        return Path.GetFullPath(Path.Combine(_rootDirectory, normalizedTarget));
    }

    private static string NormalizeAliasPrefix(string value)
    {
        var normalized = value.Trim().Replace('\\', '/');
        if (normalized.Length > 1 && normalized.EndsWith("/", StringComparison.Ordinal))
        {
            normalized = normalized.TrimEnd('/');
        }

        return normalized;
    }

    private static bool TryMatchAlias(
        string requestPath,
        string aliasPrefix,
        out string suffix)
    {
        suffix = string.Empty;
        if (string.Equals(requestPath, aliasPrefix, StringComparison.Ordinal))
        {
            return true;
        }

        if (!requestPath.StartsWith(aliasPrefix, StringComparison.Ordinal)
            || requestPath.Length <= aliasPrefix.Length
            || requestPath[aliasPrefix.Length] != '/')
        {
            return false;
        }

        suffix = requestPath[(aliasPrefix.Length + 1)..];
        return true;
    }

    private sealed record ResolveAliasRule(
        string Prefix,
        string AbsoluteTargetPath);
}

internal sealed class ResolveResult
{
    public required string AbsolutePath { get; init; }

    public required string ResolvedUrl { get; init; }

    public required DocumentKind DocumentKind { get; init; }

    public required bool IsVirtual { get; init; }

    public bool Found { get; init; } = true;

    public string? Error { get; init; }
}
