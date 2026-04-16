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

        if (TryGetOptionValue(args, "--sourcemap", out var sourcemapOverride))
        {
            buildOptions = buildOptions with
            {
                SourceMap = sourcemapOverride.ToLowerInvariant() switch
                {
                    "inline" => SourceMapOption.Inline,
                    "external" => SourceMapOption.External,
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
}
