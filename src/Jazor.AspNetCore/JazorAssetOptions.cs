using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

/// <summary>Configures the combined generated-artifact and ordinary static-file pipeline.</summary>
public sealed class JazorAssetOptions
{
    /// <summary>Initializes asset hosting with generated artifacts and ordinary web-root files enabled.</summary>
    public JazorAssetOptions()
    {
        ArtifactProbeRelativePaths =
        [
            JazorArtifactOptions.ManifestProbeRelativePath,
            JazorArtifactOptions.BundleProbeRelativePath
        ];

        ImmutableCachePathPrefixes = [];
    }

    /// <summary>Runs after Jazor applies default headers to every static response.</summary>
    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    /// <summary>Determines whether default-file resolution runs before web-root static files.</summary>
    public bool ServeDefaultFiles { get; set; } = true;

    /// <summary>Determines whether ordinary files in <c>wwwroot</c> are served.</summary>
    public bool ServeWebRoot { get; set; } = true;

    /// <summary>Determines whether the generated content-root artifact graph is served.</summary>
    public bool ServeArtifacts { get; set; } = true;

    /// <summary>Artifact files whose presence makes the generated graph ready to mount.</summary>
    public IList<string> ArtifactProbeRelativePaths { get; }

    /// <summary>Request-path prefixes that may receive immutable cache headers.</summary>
    public IList<string> ImmutableCachePathPrefixes { get; }

    /// <summary>Compatibility shorthand for the first generated artifact probe path.</summary>
    public string ArtifactProbeRelativePath
    {
        get => ArtifactProbeRelativePaths.Count == 0
            ? string.Empty
            : ArtifactProbeRelativePaths[0];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            ArtifactProbeRelativePaths.Clear();
            ArtifactProbeRelativePaths.Add(value);
        }
    }

    /// <summary>Further configures generated artifact discovery and response handling.</summary>
    public Action<JazorArtifactOptions>? ConfigureArtifacts { get; set; }
}
