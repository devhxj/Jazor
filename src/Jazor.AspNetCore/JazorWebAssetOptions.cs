using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

public sealed class JazorWebAssetOptions
{
    public JazorWebAssetOptions()
    {
        DevelopmentOutputProbeRelativePaths =
        [
            JazorDevelopmentAssetOptions.DefaultDevelopmentOutputProbeRelativePath
        ];

        ImmutableCachePathPrefixes = [];
    }

    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    public bool ServeDefaultFiles { get; set; } = true;

    public bool ServeWebRootFiles { get; set; } = true;

    public bool ServeDevelopmentAssets { get; set; } = true;

    public IList<string> DevelopmentOutputProbeRelativePaths { get; }

    public IList<string> ImmutableCachePathPrefixes { get; }

    public string DevelopmentOutputProbeRelativePath
    {
        get => DevelopmentOutputProbeRelativePaths.Count == 0
            ? string.Empty
            : DevelopmentOutputProbeRelativePaths[0];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            DevelopmentOutputProbeRelativePaths.Clear();
            DevelopmentOutputProbeRelativePaths.Add(value);
        }
    }

    public Action<JazorDevelopmentAssetOptions>? ConfigureDevelopmentAssets { get; set; }
}
