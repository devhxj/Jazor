namespace Jazor.AspNetCore;

/// <summary>Configures Jazor SSR artifacts.</summary>
public sealed class JazorSSROptions
{
    /// <summary>
    /// Overrides the generated Jazor artifact root. Relative paths are resolved from the
    /// ASP.NET Core content root. Leave empty to discover the current debug or SSR release output.
    /// </summary>
    public string? ArtifactRootPath { get; set; }

    /// <summary>
    /// Overrides the browser URL prefix for the generated artifact root, for example <c>/jazor</c>.
    /// Leave empty to derive it from the discovered output directory.
    /// </summary>
    public string? AssetPath { get; set; }

    /// <summary>Identifies the element that receives both the server HTML and client hydration.</summary>
    public string MountElementId { get; set; } = "app";

}
