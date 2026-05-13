using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

public sealed class JazorWebAssetOptions
{
    public JazorWebAssetOptions()
    {
        DevelopmentOutputProbeRelativePaths =
        [
            "jazor-manifest.json"
        ];

        ImmutableCachePathPrefixes = [];
    }

    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    public bool ServeDefaultFiles { get; set; } = true;

    public bool ServeWebRootFiles { get; set; } = true;

    public bool ServeDevelopmentAssets { get; set; } = true;

    public IList<string> DevelopmentOutputProbeRelativePaths { get; }

    public IList<string> ImmutableCachePathPrefixes { get; }

    public Action<JazorDevelopmentAssetOptions>? ConfigureDevelopmentAssets { get; set; }
}
