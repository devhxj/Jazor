using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Explicit mapping from an emitted Jazor artifact root to its browser URL root.
/// Dynamic module loading is disabled when a changed artifact has no mapping.
/// </summary>
public sealed class JazorHmrMapping
{
    /// <summary>Content-root artifact directory matched against changed files.</summary>
    public string ArtifactRootPath { get; set; } = "jazor";

    /// <summary>Browser URL prefix that corresponds to <see cref="ArtifactRootPath"/>.</summary>
    public PathString RequestPath { get; set; } = new("/jazor");
}
