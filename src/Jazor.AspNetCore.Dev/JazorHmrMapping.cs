using Microsoft.AspNetCore.Http;

namespace Jazor.AspNetCore.Dev;

/// <summary>
/// Explicit mapping from an emitted Jazor artifact root to its browser URL root.
/// Dynamic module loading is disabled when a changed artifact has no mapping.
/// </summary>
public sealed class JazorHmrMapping
{
    /// <summary>Content-root artifact directory matched against changed files.</summary>
    /// <remarks>默认 jazor；相对路径基于 ContentRootPath。该目录自动加入文件观察范围，无需重复配置 WatchPaths。</remarks>
    public string ArtifactRootPath { get; set; } = "jazor";

    /// <summary>Browser URL prefix that corresponds to <see cref="ArtifactRootPath"/>.</summary>
    /// <remarks>默认 /jazor，必须与资源挂载一致；不包含 PathBase，客户端 URL 会自动添加应用前缀。</remarks>
    public PathString RequestPath { get; set; } = new("/jazor");
}
