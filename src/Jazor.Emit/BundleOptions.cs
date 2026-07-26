namespace Jazor.Emit;

internal sealed record BundleOptions(
    string InputDirectory,
    string ManifestPath,
    string OutputPath)
{
    public static bool TryParse(string[] args, out BundleOptions? options, out string? error)
    {
        options = null;
        error = null;

        var inputDirectory = string.Empty;
        var manifestPath = string.Empty;
        var outputPath = string.Empty;

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
                case "--in":
                    inputDirectory = value;
                    break;
                case "--manifest":
                    manifestPath = value;
                    break;
                case "--out":
                    outputPath = value;
                    break;
                default:
                    error = $"Unknown argument '{arg}'.";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(inputDirectory))
        {
            error = "Missing required argument --in.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(manifestPath))
        {
            error = "Missing required argument --manifest.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            error = "Missing required argument --out.";
            return false;
        }

        options = new BundleOptions(
            Path.GetFullPath(inputDirectory),
            Path.GetFullPath(manifestPath),
            Path.GetFullPath(outputPath));
        return true;
    }
}
