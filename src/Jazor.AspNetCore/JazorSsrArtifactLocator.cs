using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Jazor.AspNetCore;

/// <summary>Resolves the generated artifact graph and rewrites its browser URLs for the request path base.</summary>
internal sealed class JazorSSRArtifactLocator
{
    private const string ArtifactManifestFileName = "jazor-manifest.json";
    private const string BrowserImportMapFileName = "importmap.json";
    private const string SsrImportMapFileName = "ssr-importmap.json";
    private const string AssetManifestFileName = "manifest.json";
    private const string DefaultAssetPath = "/jazor";
    private const string MaterializedAssetPrefix = "/jazor/";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.Default
    };

    private readonly IWebHostEnvironment _environment;
    private readonly JazorSSROptions _options;

    public JazorSSRArtifactLocator(
        IWebHostEnvironment environment,
        IOptions<JazorSSROptions> options)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public JazorSSRArtifacts Resolve()
    {
        foreach (var candidate in GetArtifactRootCandidates())
        {
            if (!File.Exists(Path.Combine(candidate, ArtifactManifestFileName)) ||
                !File.Exists(Path.Combine(candidate, BrowserImportMapFileName)) ||
                !File.Exists(Path.Combine(candidate, SsrImportMapFileName)) ||
                !File.Exists(Path.Combine(candidate, AssetManifestFileName)))
            {
                continue;
            }

            return new JazorSSRArtifacts(
                candidate,
                Path.Combine(candidate, ArtifactManifestFileName),
                Path.Combine(candidate, BrowserImportMapFileName),
                Path.Combine(candidate, SsrImportMapFileName),
                Path.Combine(candidate, AssetManifestFileName),
                ResolveAssetPath(candidate));
        }

        throw new InvalidOperationException(
            "Jazor SSR could not find a materialized artifact root containing '" +
            ArtifactManifestFileName + "', '" + BrowserImportMapFileName + "', and '" +
            SsrImportMapFileName + "', and '" + AssetManifestFileName +
            "'. Build with Jazor debug output, or enable the SSR release artifact target.");
    }

    public static string ReadBrowserImportMap(JazorSSRArtifacts artifacts, PathString pathBase)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(artifacts.BrowserImportMapPath));
        if (!document.RootElement.TryGetProperty("imports", out var importsElement) ||
            importsElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                "Jazor SSR browser import map must contain an object property named 'imports': '" +
                artifacts.BrowserImportMapPath + "'.");
        }

        var imports = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var import in importsElement.EnumerateObject())
        {
            if (import.Value.ValueKind != JsonValueKind.String || import.Value.GetString() is not { } target)
            {
                throw new InvalidOperationException(
                    "Jazor SSR browser import map entries must be strings: '" +
                    artifacts.BrowserImportMapPath + "'.");
            }

            imports.Add(import.Name, RewriteArtifactUrl(target, artifacts.AssetPath, pathBase));
        }

        return JsonSerializer.Serialize(new { imports }, JsonOptions);
    }

    public static IReadOnlyList<string> ReadStylePaths(JazorSSRArtifacts artifacts)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(artifacts.AssetManifestPath));
        if (!document.RootElement.TryGetProperty("styles", out var stylesElement) ||
            stylesElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var styles = new List<string>();
        foreach (var style in stylesElement.EnumerateArray())
        {
            if (style.ValueKind != JsonValueKind.String || style.GetString() is not { } path)
            {
                throw new InvalidOperationException(
                    "Jazor SSR style manifest entries must be strings: '" + artifacts.AssetManifestPath + "'.");
            }

            styles.Add(NormalizeStylePath(path));
        }

        return styles;
    }

    public static string CreateBrowserArtifactUrl(
        JazorSSRArtifacts artifacts,
        PathString pathBase,
        string relativePath)
    {
        var normalizedRelativePath = NormalizeRelativePath(relativePath, "module path");
        var normalizedPathBase = pathBase.Value?.TrimEnd('/') ?? string.Empty;
        return normalizedPathBase + artifacts.AssetPath + "/" + normalizedRelativePath;
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

    private static string NormalizeStylePath(string stylePath)
    {
        // manifest.json is the browser asset contract, so its generated style URLs are rooted
        // at /jazor. SSR rebuilds those URLs for the host's actual asset path and PathBase.
        if (stylePath.StartsWith(MaterializedAssetPrefix, StringComparison.Ordinal))
            return NormalizeRelativePath(stylePath[MaterializedAssetPrefix.Length..], "style path");

        return NormalizeRelativePath(stylePath, "style path");
    }

    private IEnumerable<string> GetArtifactRootCandidates()
    {
        if (!string.IsNullOrWhiteSpace(_options.ArtifactRootPath))
        {
            yield return ResolveConfiguredArtifactRoot(_options.ArtifactRootPath);
            yield break;
        }

        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            yield return Path.GetFullPath(Path.Combine(_environment.WebRootPath, "jazor", "ssr"));
            yield return Path.GetFullPath(Path.Combine(_environment.WebRootPath, "jazor"));
        }

        yield return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "jazor", "ssr"));
        yield return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "jazor"));
    }

    private string ResolveConfiguredArtifactRoot(string configuredPath)
    {
        var candidate = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);
        return Path.GetFullPath(candidate);
    }

    private string ResolveAssetPath(string artifactRoot)
    {
        if (!string.IsNullOrWhiteSpace(_options.AssetPath))
            return NormalizeAssetPath(_options.AssetPath);

        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath))
        {
            var webRoot = Path.GetFullPath(_environment.WebRootPath);
            var ssrRoot = Path.Combine(webRoot, "jazor", "ssr");
            if (PathsEqual(artifactRoot, ssrRoot))
                return "/jazor/ssr";

            var root = Path.Combine(webRoot, "jazor");
            if (PathsEqual(artifactRoot, root))
                return DefaultAssetPath;
        }

        return DefaultAssetPath;
    }

    private static string RewriteArtifactUrl(string target, string assetPath, PathString pathBase)
    {
        const string materializedPrefix = "/jazor";
        if (!target.StartsWith(materializedPrefix, StringComparison.Ordinal) ||
            (target.Length > materializedPrefix.Length && target[materializedPrefix.Length] != '/'))
        {
            return target;
        }

        var normalizedPathBase = pathBase.Value?.TrimEnd('/') ?? string.Empty;
        return normalizedPathBase + assetPath + target[materializedPrefix.Length..];
    }

    private static string NormalizeAssetPath(string assetPath)
    {
        var normalized = assetPath.Trim().Replace('\\', '/');
        if (normalized.Length == 0 || !normalized.StartsWith('/', StringComparison.Ordinal))
            throw new ArgumentException("Jazor SSR asset path must start with '/'.", nameof(assetPath));

        var segments = normalized
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Where(static segment => segment != ".")
            .ToArray();
        if (segments.Any(static segment => segment == ".."))
            throw new ArgumentException("Jazor SSR asset path cannot contain '..'.", nameof(assetPath));

        return "/" + string.Join("/", segments);
    }

    private static bool PathsEqual(string left, string right)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
}

internal sealed record JazorSSRArtifacts(
    string RootPath,
    string ApplicationManifestPath,
    string BrowserImportMapPath,
    string SsrImportMapPath,
    string AssetManifestPath,
    string AssetPath);
