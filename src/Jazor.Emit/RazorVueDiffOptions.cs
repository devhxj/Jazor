namespace Jazor.Emit;

internal sealed record RazorVueDiffOptions(
    string PreviousManifestPath,
    string CurrentManifestPath,
    string OutputPath)
{
    public static bool TryParse(string[] args, out RazorVueDiffOptions? options, out string? error)
    {
        options = null;
        error = null;

        var previousManifestPath = string.Empty;
        var currentManifestPath = string.Empty;
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
                case "--previous":
                    previousManifestPath = value;
                    break;
                case "--current":
                    currentManifestPath = value;
                    break;
                case "--out":
                case "--write-plan":
                    outputPath = value;
                    break;
                default:
                    error = $"Unknown argument '{arg}'.";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(previousManifestPath))
        {
            error = "Missing required argument --previous.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(currentManifestPath))
        {
            error = "Missing required argument --current.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            error = "Missing required argument --out.";
            return false;
        }

        options = new RazorVueDiffOptions(
            Path.GetFullPath(previousManifestPath),
            Path.GetFullPath(currentManifestPath),
            Path.GetFullPath(outputPath));
        return true;
    }
}
