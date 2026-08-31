namespace Jazor.Emit;

/// <summary>Parsed inputs for materializing catalog modules and their manifest.</summary>
internal sealed record EmitOptions(
    string RootAssemblyPath,
    IReadOnlyList<string> AssemblyPaths,
    string OutputDirectory,
    string ManifestPath,
    bool Clean,
    BuildMode Mode,
    string? SourceRoot,
    IReadOnlyList<string> LibraryManifests,
    bool EnableSsr)
{
    public static bool TryParse(string[] args, out EmitOptions? options, out string? error)
    {
        options = null;
        error = null;

        var rootAssemblyPath = string.Empty;
        var outputDirectory = string.Empty;
        var manifestPath = string.Empty;
        var assemblyPaths = new List<string>();
        var clean = true;
        var mode = BuildMode.Development;
        var sourceRoot = string.Empty;
        var libraryManifests = new List<string>();
        var enableSsr = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (i + 1 >= args.Length)
            {
                error = $"Missing value for argument '{arg}'.";
                return false;
            }

            var value = args[++i];
            switch (arg)
            {
                case "--root":
                    rootAssemblyPath = value;
                    break;
                case "--assembly":
                    assemblyPaths.Add(value);
                    break;
                case "--out":
                    outputDirectory = value;
                    break;
                case "--write-manifest":
                    manifestPath = value;
                    break;
                case "--clean":
                    if (!bool.TryParse(value, out clean))
                    {
                        error = $"Invalid boolean for --clean: '{value}'.";
                        return false;
                    }

                    break;
                case "--mode":
                    if (!TryParseMode(value, out mode))
                    {
                        error = $"Invalid Emit mode '{value}'. Expected 'debug' or 'release'.";
                        return false;
                    }

                    break;
                case "--source-root":
                    sourceRoot = value;
                    break;
                case "--library-manifest":
                    libraryManifests.Add(value);
                    break;
                case "--ssr":
                    if (!bool.TryParse(value, out enableSsr))
                    {
                        error = $"Invalid boolean for --ssr: '{value}'.";
                        return false;
                    }

                    break;
                default:
                    error = $"Unknown argument '{arg}'.";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(rootAssemblyPath))
        {
            error = "Missing required argument --root.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            error = "Missing required argument --out.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            error = "Missing required argument --write-manifest.";
            return false;
        }

        options = new EmitOptions(
            Path.GetFullPath(rootAssemblyPath),
            [.. assemblyPaths.Select(Path.GetFullPath)],
            Path.GetFullPath(outputDirectory),
            Path.GetFullPath(manifestPath),
            clean,
            mode,
            string.IsNullOrWhiteSpace(sourceRoot) ? null : Path.GetFullPath(sourceRoot),
            [.. libraryManifests
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)],
            enableSsr);
        return true;
    }

    private static bool TryParseMode(string value, out BuildMode mode)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "debug":
            case "development":
                mode = BuildMode.Development;
                return true;
            case "release":
            case "production":
                mode = BuildMode.Production;
                return true;
            default:
                mode = default;
                return false;
        }
    }
}
