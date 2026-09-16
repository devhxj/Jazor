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
    /// <remarks>默认 /api、/assets、/health、/jazor；按路径段匹配。自定义产物 URL 时也应加入该列表。</remarks>
    public IList<PathString> ExcludedPathPrefixes { get; }

    /// <summary>File-like request suffixes that are still eligible for SPA fallback.</summary>
    /// <remarks>默认只有 /；无扩展名路由本来就允许。此列表允许额外的文件样式路由后缀，不绕过 excluded 前缀。</remarks>
    public IList<string> AllowedPathSuffixes { get; }

    /// <summary>Requires an HTML navigation accept header before writing fallback HTML.</summary>
    /// <remarks>默认 true。缺少 Accept 仍允许；显式 */* 本身不足以通过，text/html 或 application/xhtml+xml 需 q 大于零。</remarks>
    public bool RequireHtmlAcceptHeader { get; set; } = true;
}
