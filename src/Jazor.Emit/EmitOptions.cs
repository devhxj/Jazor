namespace Jazor.Emit;

/// <summary>Parsed inputs for materializing catalog modules and their manifest.</summary>
internal sealed record EmitOptions(
    string RootAssemblyPath,
    IReadOnlyList<string> AssemblyPaths,
    string OutputDirectory,
    string ManifestPath,
    bool Clean,
    bool FailOnPathConflict)
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
        var failOnPathConflict = true;

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
                case "--fail-on-path-conflict":
                    if (!bool.TryParse(value, out failOnPathConflict))
                    {
                        error = $"Invalid boolean for --fail-on-path-conflict: '{value}'.";
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
            failOnPathConflict);
        return true;
    }
}
