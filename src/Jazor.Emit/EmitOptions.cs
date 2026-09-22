namespace Jazor.Emit;

/// <summary>Parsed inputs for materializing catalog modules and their manifest.</summary>
internal sealed record EmitOptions(
    string RootAssemblyPath,
    IReadOnlyList<string> AssemblyPaths,
    string OutputDirectory,
    string ManifestPath,
    BuildMode Mode,
    string? SourceRoot,
    IReadOnlyList<string> LibraryManifests,
    bool EnableSsr,
    string? DenoExecutablePath = null,
    IReadOnlyList<string>? ModuleAssemblyPaths = null)
{
    public static bool TryParse(string[] args, out EmitOptions? options, out string? error)
    {
        options = null;
        error = null;

        var rootAssemblyPath = string.Empty;
        var outputDirectory = string.Empty;
        var manifestPath = string.Empty;
        var assemblyPaths = new List<string>();
        var moduleAssemblyPaths = new List<string>();
        var mode = BuildMode.Development;
        var sourceRoot = string.Empty;
        var libraryManifests = new List<string>();
        var enableSsr = false;
        string? denoExecutablePath = null;

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
                case "--assembly-list":
                    if (!TryReadPathList(value, assemblyPaths, out error))
                        return false;
                    break;
                case "--module-assembly":
                    moduleAssemblyPaths.Add(value);
                    break;
                case "--module-assembly-list":
                    if (!TryReadPathList(value, moduleAssemblyPaths, out error))
                        return false;
                    break;
                case "--out":
                    outputDirectory = value;
                    break;
                case "--write-manifest":
                    manifestPath = value;
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
                case "--library-manifest-list":
                    if (!TryReadPathList(value, libraryManifests, out error))
                        return false;
                    break;
                case "--ssr":
                    if (!bool.TryParse(value, out enableSsr))
                    {
                        error = $"Invalid boolean for --ssr: '{value}'.";
                        return false;
                    }

                    break;
                case "--deno":
                    denoExecutablePath = value;
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
            mode,
            string.IsNullOrWhiteSpace(sourceRoot) ? null : Path.GetFullPath(sourceRoot),
            [.. libraryManifests
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)],
            enableSsr,
            string.IsNullOrWhiteSpace(denoExecutablePath) ? null : Path.GetFullPath(denoExecutablePath),
            [.. moduleAssemblyPaths.Select(Path.GetFullPath)]);
        return true;
    }

    private static bool TryReadPathList(string path, List<string> paths, out string? error)
    {
        try
        {
            paths.AddRange(File.ReadLines(Path.GetFullPath(path))
                .Where(static line => !string.IsNullOrWhiteSpace(line))
                .Select(static line => line.Trim()));
            error = null;
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            error = $"Could not read Emit path list '{path}': {exception.Message}";
            return false;
        }
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
