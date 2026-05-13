using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore;

public sealed class JazorSpaFallbackOptions
{
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

    public IList<PathString> ExcludedPathPrefixes { get; }

    public IList<string> AllowedPathSuffixes { get; }

    public bool RequireHtmlAcceptHeader { get; set; } = true;
}
