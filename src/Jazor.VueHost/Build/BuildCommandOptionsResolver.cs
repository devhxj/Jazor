using Jazor.VueHost.DevServer;

namespace Jazor.VueHost.Build;

internal static class BuildCommandOptionsResolver
{
    public static BuildOptions ResolveBuildOptions(
        string[] args,
        string rootDirectory,
        JazorConfig? config)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);

        var buildOptions = config?.Build?.ToBuildOptions(rootDirectory)
            ?? new BuildOptions { RootDirectory = rootDirectory };
        var resolveAliases = NormalizeResolveAliases(config?.Resolve?.Alias);
        if (resolveAliases.Count > 0)
        {
            buildOptions = buildOptions with { ResolveAliases = resolveAliases };
        }

        if (TryGetOptionValue(args, "--sourcemap", out var sourcemapOverride))
        {
            buildOptions = buildOptions with
            {
                SourceMap = sourcemapOverride.ToLowerInvariant() switch
                {
                    "inline" => SourceMapOption.Inline,
                    "true" or "external" or "linked" => SourceMapOption.External,
                    "false" or "none" => SourceMapOption.None,
                    _ => buildOptions.SourceMap
                }
            };
        }

        if (TryGetOptionValue(args, "--minify", out var minifyOverride)
            && bool.TryParse(minifyOverride, out var minify))
        {
            buildOptions = buildOptions with { Minify = minify };
        }

        if (TryGetOptionValue(args, "--out-dir", out var outDirOverride))
        {
            buildOptions = buildOptions with { OutDir = outDirOverride };
        }

        if (TryGetOptionValue(args, "--target", out var targetOverride)
            && !string.IsNullOrWhiteSpace(targetOverride))
        {
            buildOptions = buildOptions with { Target = targetOverride };
        }

        if (TryGetOptionValue(args, "--code-splitting", out var codeSplittingOverride)
            && bool.TryParse(codeSplittingOverride, out var codeSplitting))
        {
            buildOptions = buildOptions with { CodeSplitting = codeSplitting };
        }

        if (TryGetOptionValue(args, "--assets-dir", out var assetsDirOverride))
        {
            buildOptions = buildOptions with { AssetsDir = assetsDirOverride };
        }

        if (TryGetOptionValue(args, "--asset-hash-length", out var assetHashLengthOverride)
            && int.TryParse(assetHashLengthOverride, out var assetHashLength))
        {
            buildOptions = buildOptions with { AssetHashLength = assetHashLength };
        }

        if (TryGetOptionValue(args, "--chunk-size-warning-limit", out var chunkSizeWarningLimitOverride)
            && int.TryParse(chunkSizeWarningLimitOverride, out var chunkSizeWarningLimit))
        {
            buildOptions = buildOptions with { ChunkSizeWarningLimit = chunkSizeWarningLimit };
        }

        return buildOptions;
    }

    public static string ResolveOutputDirectory(
        string[] args,
        string rootDirectory,
        JazorConfig? config)
    {
        var buildOptions = ResolveBuildOptions(args, rootDirectory, config);
        return Path.GetFullPath(Path.Combine(rootDirectory, buildOptions.OutDir));
    }

    private static bool TryGetOptionValue(string[] args, string optionName, out string value)
    {
        foreach (var arg in args)
        {
            if (arg.StartsWith(optionName + "=", StringComparison.OrdinalIgnoreCase))
            {
                value = arg[(optionName.Length + 1)..];
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    private static IReadOnlyDictionary<string, string> NormalizeResolveAliases(
        IDictionary<string, string>? aliases)
    {
        if (aliases is null || aliases.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in aliases)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            normalized[key.Trim()] = value.Trim();
        }

        return normalized;
    }
}
