using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore;

/// <summary>Resolves the generated artifact graph and rewrites browser URLs for a request path base.</summary>
internal sealed class SsrArtifactLocator
{
    private const string SsrEntryFileName = "ssr-entry.js";
    private const string DefaultRequestPath = "/jazor";

    private readonly IWebHostEnvironment _environment;
    private readonly JazorSsrOptions _options;

    public SsrArtifactLocator(
        IWebHostEnvironment environment,
        IOptions<JazorSsrOptions> options)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>Finds the complete SSR artifact graph for the current content-root layout.</summary>
    public SsrArtifacts Resolve()
    {
        foreach (var candidate in GetArtifactRootCandidates())
        {
            if (!File.Exists(Path.Combine(candidate, SsrEntryFileName)) ||
                !File.Exists(Path.Combine(candidate, "package.json")) ||
                !File.Exists(Path.Combine(candidate, "deno.lock")))
            {
                continue;
            }

            return new SsrArtifacts(
                candidate,
                Path.Combine(candidate, SsrEntryFileName),
                ResolveRequestPath());
        }

        throw new InvalidOperationException(
            "Jazor SSR could not find a standard project containing '" + SsrEntryFileName +
            "', package.json, and deno.lock. Build with Jazor SSR enabled.");
    }

    public static string CreateBrowserArtifactUrl(
        SsrArtifacts artifacts,
        PathString pathBase,
        string relativePath)
    {
        var normalizedRelativePath = NormalizeRelativePath(relativePath, "module path");
        var normalizedPathBase = pathBase.Value?.TrimEnd('/') ?? string.Empty;
        return normalizedPathBase + artifacts.RequestPath + "/" + normalizedRelativePath;
    }

    public static string NormalizeRelativePath(string relativePath, string valueName)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            throw new ArgumentException("Jazor SSR " + valueName + " cannot be empty.", valueName);

        var normalized = relativePath.Trim().Replace('\\', '/');
        if (normalized.StartsWith('/', StringComparison.Ordinal) ||
            Uri.TryCreate(normalized, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Jazor SSR " + valueName + " must be a relative artifact path.", valueName);
        }

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Length == 0 || segments.Any(static segment => segment == ".."))
        {
            throw new ArgumentException(
                "Jazor SSR " + valueName + " cannot escape the artifact root.",
                valueName);
        }

        return string.Join("/", segments);
    }

    private IEnumerable<string> GetArtifactRootCandidates()
    {
        if (!string.IsNullOrWhiteSpace(_options.ArtifactRootPath))
        {
            yield return ResolveConfiguredArtifactRoot(_options.ArtifactRootPath);
            yield break;
        }

        // SSR and the JavaScript web service consume the same standard project root.
        yield return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "jazor"));
    }

    private string ResolveConfiguredArtifactRoot(string configuredPath)
    {
        var candidate = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
        return Path.GetFullPath(candidate);
    }

    private string ResolveRequestPath()
        => string.IsNullOrWhiteSpace(_options.RequestPath)
            ? DefaultRequestPath
            : NormalizeRequestPath(_options.RequestPath);

    private static string NormalizeRequestPath(string requestPath)
    {
        var normalized = requestPath.Trim().Replace('\\', '/');
        if (normalized.Length == 0 || !normalized.StartsWith('/', StringComparison.Ordinal))
            throw new ArgumentException("Jazor SSR request path must start with '/'.", nameof(requestPath));

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new ArgumentException("Jazor SSR request path cannot contain '..'.", nameof(requestPath));

        return "/" + string.Join("/", segments);
    }

}

/// <summary>Resolved paths for one self-contained SSR artifact graph.</summary>
internal sealed record SsrArtifacts(
    string RootPath,
    string SsrEntryPath,
    string RequestPath);
