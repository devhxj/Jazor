using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Explicit mapping from an emitted Jazor artifact root to its browser URL root.
/// Dynamic module loading is disabled when a changed artifact has no mapping.
/// </summary>
public sealed class JazorDevelopmentHmrModuleMapping
{
    public string ArtifactRootPath { get; set; } = "wwwroot/jazor";

    public PathString RequestPath { get; set; } = new("/jazor");
}
