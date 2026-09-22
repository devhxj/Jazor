using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

/// <summary>Configures discovery and serving of the generated Jazor artifact graph.</summary>
public sealed class JazorArtifactOptions
{
    /// <summary>Standard browser entry used to detect a ready generated project.</summary>
    public const string EntryProbeRelativePath = "entry.js";

    /// <summary>
    /// Backward-compatible fallback artifact used when a consumer has no entry probe.
    /// The generated project itself does not require this path; its build tool may choose any output layout.
    /// </summary>
    public const string BundleProbeRelativePath = "dist/bundle.js";

    /// <summary>Initializes artifact discovery with the standard entry and a compatibility fallback probe.</summary>
    public JazorArtifactOptions()
    {
        ProbeRelativePaths =
        [
            EntryProbeRelativePath,
            BundleProbeRelativePath
        ];

        ImmutableCachePathPrefixes = [];
    }

    /// <summary>Browser URL prefix for the generated artifact graph.</summary>
    /// <remarks>默认 /jazor，必须为非根 URL 前缀；不包含 Request.PathBase。自定义时需同步 SSR 与 HMR 映射。</remarks>
    public PathString RequestPath { get; set; } = new("/jazor");

    /// <summary>Overrides the generated artifact root; relative paths resolve from the content root.</summary>
    /// <remarks>默认 null，使用 DirectoryName；显式相对目录基于 IWebHostEnvironment.ContentRootPath。</remarks>
    public string? RootPath { get; set; }

    /// <summary>Content-root directory used when <see cref="RootPath"/> is not configured.</summary>
    /// <remarks>默认 jazor；RootPath 已配置时不使用此值。</remarks>
    public string DirectoryName { get; set; } = "jazor";

    /// <summary>Probe files checked once when registering the artifact mount.</summary>
    /// <remarks>默认包含 entry.js 和一个兼容性 fallback，任意一个存在即通过。仅在注册中间件时探测，不保证整个项目图完整。</remarks>
    public IList<string> ProbeRelativePaths { get; }

    /// <summary>Request-path prefixes that may receive immutable cache headers.</summary>
    /// <remarks>默认空。按 Request.Path 前缀（不含 PathBase）匹配；只有带内容版本的 URL 才应配置为一年 immutable 缓存。</remarks>
    public IList<string> ImmutableCachePathPrefixes { get; }

    /// <summary>Compatibility shorthand for the first artifact probe path.</summary>
    /// <remarks>读取首项，空列表返回空字符串；设置时清空列表并加入一个非空路径。</remarks>
    public string ProbeRelativePath
    {
        get => ProbeRelativePaths.Count == 0
            ? string.Empty
            : ProbeRelativePaths[0];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            ProbeRelativePaths.Clear();
            ProbeRelativePaths.Add(value);
        }
    }

    /// <summary>Runs after Jazor applies its default response headers.</summary>
    /// <remarks>默认 null。默认响应头写入后执行，可覆盖 Cache-Control 等头。</remarks>
    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    /// <summary>Returns 404 for a missing artifact instead of allowing an SPA fallback.</summary>
    /// <remarks>默认 true。仅在产物目录已挂载时阻断该前缀的未命中文件，防止进入 SPA fallback。</remarks>
    public bool ReturnNotFoundOnMiss { get; set; } = true;
}
