using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

public sealed class JazorDevelopmentAssetOptions
{
    public const string DefaultDevelopmentOutputProbeRelativePath = "jazor-manifest.json";

    public JazorDevelopmentAssetOptions()
    {
        DevelopmentOutputProbeRelativePaths =
        [
            DefaultDevelopmentOutputProbeRelativePath
        ];

        ImmutableCachePathPrefixes = [];
    }

    public PathString RequestPath { get; set; } = new("/jazor");

    public string? DevelopmentOutputRootPath { get; set; }

    public string DevelopmentOutputDirectoryName { get; set; } = "jazor";

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

    [Obsolete("Prefer DevelopmentOutputProbeRelativePath.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string EntryModuleRelativePath
    {
        get => DevelopmentOutputProbeRelativePath;
        set => DevelopmentOutputProbeRelativePath = value;
    }

    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    public bool ReturnNotFoundWhenMountedPathMisses { get; set; } = true;
}
