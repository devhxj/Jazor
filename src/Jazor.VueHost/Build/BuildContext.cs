namespace Jazor.VueHost.Build;

internal sealed class BuildContext : IDisposable
{
    private const string DefaultBundleTarget = "es2020";

    public string RootDirectory { get; }
    public string OutDirectory { get; }
    public string AssetsDirectory { get; }
    public BuildOptions Options { get; }

    public List<BuildDiagnostic> Diagnostics { get; } = [];

    public BuildContext(BuildOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        RootDirectory = options.RootDirectory;
        OutDirectory = Path.Combine(options.RootDirectory, options.OutDir);
        AssetsDirectory = ResolveAssetsDirectory(OutDirectory, options.AssetsDir);

        if (options.AssetHashLength < 1 || options.AssetHashLength > 64)
        {
            throw new InvalidOperationException(
                $"assetHashLength must be between 1 and 64. Received {options.AssetHashLength}.");
        }

        if (options.ChunkSizeWarningLimit < 0)
        {
            throw new InvalidOperationException(
                $"chunkSizeWarningLimit must be greater than or equal to 0. Received {options.ChunkSizeWarningLimit}.");
        }

        if (!string.Equals(options.Target, DefaultBundleTarget, StringComparison.OrdinalIgnoreCase))
        {
            Diagnostics.Add(new BuildDiagnostic
            {
                Severity = DiagnosticSeverity.Warning,
                Message = $"Bundled Deno browser bundling ignores target '{options.Target}'. Emitting the default browser ESM output instead."
            });
        }
    }

    public void Dispose()
    {
        // Cleanup temporary files if needed
    }

    private static string ResolveAssetsDirectory(string outDirectory, string assetsDir)
    {
        if (Path.IsPathRooted(assetsDir))
        {
            throw new InvalidOperationException(
                $"assetsDir '{assetsDir}' must be a relative path inside the build output directory.");
        }

        var candidatePath = string.IsNullOrWhiteSpace(assetsDir)
            ? outDirectory
            : Path.Combine(outDirectory, assetsDir);
        var fullOutDirectory = Path.GetFullPath(outDirectory);
        var fullCandidatePath = Path.GetFullPath(candidatePath);
        if (!IsInsideDirectory(fullOutDirectory, fullCandidatePath))
        {
            throw new InvalidOperationException(
                $"Resolved assets directory '{fullCandidatePath}' must stay inside build output directory '{fullOutDirectory}'.");
        }

        return fullCandidatePath;
    }

    private static bool IsInsideDirectory(string rootDirectory, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(rootDirectory, candidatePath);
        return string.Equals(relativePath, ".", StringComparison.Ordinal)
            || (!string.Equals(relativePath, "..", StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath));
    }
}
