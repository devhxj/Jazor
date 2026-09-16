using Microsoft.AspNetCore.StaticFiles;

namespace Jazor.AspNetCore;

/// <summary>Configures the combined generated-artifact and ordinary static-file pipeline.</summary>
public sealed class JazorAssetOptions
{
    /// <summary>Initializes asset hosting with generated artifacts and ordinary web-root files enabled.</summary>
    public JazorAssetOptions()
    {
        ArtifactProbeRelativePaths =
        [
            JazorArtifactOptions.ManifestProbeRelativePath,
            JazorArtifactOptions.BundleProbeRelativePath
        ];

        ImmutableCachePathPrefixes = [];
    }

    /// <summary>Runs after Jazor applies default headers to every static response.</summary>
    /// <remarks>默认 null。默认响应头写入后执行，可覆盖 Cache-Control 等头。</remarks>
    public Action<StaticFileResponseContext>? OnPrepareResponse { get; set; }

    /// <summary>Determines whether default-file resolution runs before web-root static files.</summary>
    /// <remarks>默认 true；在 UseJazorAssets 注册时读取。</remarks>
    public bool ServeDefaultFiles { get; set; } = true;

    /// <summary>Determines whether ordinary files in <c>wwwroot</c> are served.</summary>
    /// <remarks>默认 true；在 UseJazorAssets 注册时读取。</remarks>
    public bool ServeWebRoot { get; set; } = true;

    /// <summary>Determines whether the generated content-root artifact graph is served.</summary>
    /// <remarks>默认 true；在 UseJazorAssets 注册时读取。</remarks>
    public bool ServeArtifacts { get; set; } = true;

    /// <summary>Artifact files whose presence makes the generated graph ready to mount.</summary>
    /// <remarks>默认 jazor-manifest.json 与 bundle.js，注册时任意一个存在即可挂载。</remarks>
    public IList<string> ArtifactProbeRelativePaths { get; }

    /// <summary>Request-path prefixes that may receive immutable cache headers.</summary>
    /// <remarks>默认空。按 Request.Path 前缀（不含 PathBase）匹配；只有带内容版本的 URL 才应配置为一年 immutable 缓存。</remarks>
    public IList<string> ImmutableCachePathPrefixes { get; }

    /// <summary>Compatibility shorthand for the first generated artifact probe path.</summary>
    /// <remarks>读取首项，空列表返回空字符串；设置时清空列表并加入一个非空路径。</remarks>
    public string ArtifactProbeRelativePath
    {
        get => ArtifactProbeRelativePaths.Count == 0
            ? string.Empty
            : ArtifactProbeRelativePaths[0];
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            ArtifactProbeRelativePaths.Clear();
            ArtifactProbeRelativePaths.Add(value);
        }
    }

    /// <summary>Further configures generated artifact discovery and response handling.</summary>
    /// <remarks>先复制共享探测路径、缓存前缀与响应回调，再执行本回调，因此本回调中的产物配置优先。</remarks>
    public Action<JazorArtifactOptions>? ConfigureArtifacts { get; set; }
}
