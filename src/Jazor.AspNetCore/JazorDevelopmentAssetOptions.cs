using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

public sealed class JazorDevelopmentAssetOptions
{
    public JazorDevelopmentAssetOptions()
    {
        DevelopmentOutputProbeRelativePaths =
        [
            "jazor-manifest.json"
        ];
    }

    public PathString RequestPath { get; set; } = new("/jazor");

    public string? DevelopmentOutputRootPath { get; set; }

    public string DevelopmentOutputDirectoryName { get; set; } = "jazor";

    public IList<string> DevelopmentOutputProbeRelativePaths { get; }

    public string EntryModuleRelativePath
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

    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    public bool ReturnNotFoundWhenMountedPathMisses { get; set; } = true;
}
