using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

public sealed class JazorDevelopmentAssetOptions
{
    public PathString RequestPath { get; set; } = new("/jazor");

    public string? DevelopmentOutputRootPath { get; set; }

    public string DevelopmentOutputDirectoryName { get; set; } = "jazor";

    public string EntryModuleRelativePath { get; set; } = "main.mjs";

    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    public bool ReturnNotFoundWhenMountedPathMisses { get; set; } = true;
}
