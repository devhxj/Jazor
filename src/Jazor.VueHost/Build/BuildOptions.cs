namespace Jazor.VueHost.Build;

internal sealed record BuildOptions
{
    public required string RootDirectory { get; init; }

    public string OutDir { get; init; } = "dist";

    public SourceMapOption SourceMap { get; init; } = SourceMapOption.External;

    public bool Minify { get; init; } = true;

    public string Target { get; init; } = "es2020";

    public bool CodeSplitting { get; init; } = true;

    public int ChunkSizeWarningLimit { get; init; } = 500_000;

    public string AssetsDir { get; init; } = "assets";

    public int AssetHashLength { get; init; } = 8;

    public IReadOnlyDictionary<string, string> ResolveAliases { get; init; }
        = new Dictionary<string, string>(StringComparer.Ordinal);

    public bool Incremental { get; init; } = false;

    public bool GenerateSourceMap => SourceMap != SourceMapOption.None;
}

internal enum SourceMapOption
{
    None,
    Inline,
    External
}
