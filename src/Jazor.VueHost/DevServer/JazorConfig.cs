using Jazor.VueHost.Build;

namespace Jazor.VueHost.DevServer;

internal sealed class JazorConfig
{
    public JazorServerConfig? Server { get; init; }

    public Dictionary<string, JazorProxyConfig>? Proxy { get; init; }

    public JazorResolveConfig? Resolve { get; init; }

    public JazorBuildConfig? Build { get; init; }

    public JazorExtensionsConfig? Extensions { get; init; }
}

internal sealed class JazorServerConfig
{
    public int? Port { get; init; }

    public string? Host { get; init; }

    public bool? Open { get; init; }

    public bool? Hmr { get; init; }
}

internal sealed class JazorProxyConfig
{
    public string? Target { get; init; }

    public bool? Secure { get; init; }

    public bool? WebSocket { get; init; }

    public string? RewritePath { get; init; }
}

internal sealed class JazorResolveConfig
{
    public Dictionary<string, string>? Alias { get; init; }
}

internal sealed class JazorBuildConfig
{
    public string? OutDir { get; init; }

    public string? SourceMap { get; init; }

    public bool? Minify { get; init; }

    public string? Target { get; init; }

    public bool? CodeSplitting { get; init; }

    public string? AssetsDir { get; init; }

    public int? AssetHashLength { get; init; }

    public int? ChunkSizeWarningLimit { get; init; }

    public BuildOptions ToBuildOptions(string rootDirectory)
    {
        return new BuildOptions
        {
            RootDirectory = rootDirectory,
            OutDir = OutDir ?? "dist",
            SourceMap = ParseSourceMapOption(SourceMap),
            Minify = Minify ?? true,
            Target = Target ?? "es2020",
            CodeSplitting = CodeSplitting ?? true,
            AssetsDir = AssetsDir ?? "assets",
            AssetHashLength = AssetHashLength ?? 8,
            ChunkSizeWarningLimit = ChunkSizeWarningLimit ?? 500_000
        };
    }

    private static SourceMapOption ParseSourceMapOption(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "inline" => SourceMapOption.Inline,
            "false" => SourceMapOption.None,
            "external" => SourceMapOption.External,
            _ => SourceMapOption.External
        };
    }
}

internal sealed class JazorExtensionsConfig
{
    public bool? Enabled { get; init; }

    public string? Directory { get; init; }

    public bool? AllowExternalDirectory { get; init; }

    public string[]? Disabled { get; init; }

    public string[]? Trusted { get; init; }

    public Dictionary<string, string>? TrustedPublicKeys { get; init; }

    public string? TrustKeysFile { get; init; }

    public bool? RequireAssemblyHash { get; init; }

    public bool? EnforceProviderPermissions { get; init; }

    public bool? RequireManifestSignature { get; init; }
}
