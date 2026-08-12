using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

/// <summary>Configures discovery and serving of the generated Jazor artifact graph.</summary>
public sealed class JazorArtifactOptions
{
    /// <summary>Default manifest used to detect a ready generated artifact graph.</summary>
    public const string ManifestProbeRelativePath = "jazor-manifest.json";

    /// <summary>Fallback browser bundle used to detect a ready release artifact graph.</summary>
    public const string BundleProbeRelativePath = "bundle.js";

    /// <summary>Initializes artifact discovery with manifest and release-bundle probes.</summary>
    public JazorArtifactOptions()
    {
        ProbeRelativePaths =
        [
            ManifestProbeRelativePath,
            BundleProbeRelativePath
        ];

        ImmutableCachePathPrefixes = [];
    }

    /// <summary>Browser URL prefix for the generated artifact graph.</summary>
    public PathString RequestPath { get; set; } = new("/jazor");

    /// <summary>Overrides the generated artifact root; relative paths resolve from the content root.</summary>
    public string? RootPath { get; set; }

    /// <summary>Content-root directory used when <see cref="RootPath"/> is not configured.</summary>
    public string DirectoryName { get; set; } = "jazor";

    /// <summary>Mounting starts only after one probe exists, so an incomplete build is never served.</summary>
    public IList<string> ProbeRelativePaths { get; }

    /// <summary>Request-path prefixes that may receive immutable cache headers.</summary>
    public IList<string> ImmutableCachePathPrefixes { get; }

    /// <summary>Compatibility shorthand for the first artifact probe path.</summary>
    public string ProbeRelativePath
    {
        get => ProbeRelativePaths.Count == 0
            ? string.Empty
            : ProbeRelativePaths[0];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            ProbeRelativePaths.Clear();
            ProbeRelativePaths.Add(value);
        }
    }

    /// <summary>Runs after Jazor applies its default response headers.</summary>
    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    /// <summary>Returns 404 for a missing artifact instead of allowing an SPA fallback.</summary>
    public bool ReturnNotFoundOnMiss { get; set; } = true;
}
