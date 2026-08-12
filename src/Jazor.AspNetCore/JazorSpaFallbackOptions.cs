using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore;

/// <summary>Controls which unhandled navigation requests may receive SPA HTML.</summary>
public sealed class JazorSpaFallbackOptions
{
    /// <summary>Initializes default exclusions for API, assets, health, and Jazor artifact paths.</summary>
    public JazorSpaFallbackOptions()
    {
        ExcludedPathPrefixes =
        [
            new PathString("/api"),
            new PathString("/assets"),
            new PathString("/health"),
            new PathString("/jazor")
        ];

        AllowedPathSuffixes =
        [
            "/"
        ];
    }

    /// <summary>Path prefixes that must never be rewritten to SPA HTML.</summary>
    public IList<PathString> ExcludedPathPrefixes { get; }

    /// <summary>File-like request suffixes that are still eligible for SPA fallback.</summary>
    public IList<string> AllowedPathSuffixes { get; }

    /// <summary>Requires an HTML navigation accept header before writing fallback HTML.</summary>
    public bool RequireHtmlAcceptHeader { get; set; } = true;
}
