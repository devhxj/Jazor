using Jazor.VueContracts.Protocol;

namespace Jazor.VueHost.DevServer;

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

    public ModuleResolver(string rootDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        _rootDirectory = Path.GetFullPath(rootDirectory);
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
            _ => DocumentKind.Unknown
        };
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
