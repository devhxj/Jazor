namespace Jazor.Emit;

internal sealed record RazorVueSfcBridgeOptions(
    string HostJazorRoot,
    string OutputDirectory,
    string? ManifestPath,
    RazorVueSfcBridgeMode Mode,
    bool Production,
    bool Clean)
{
    public static bool TryParse(string[] args, out RazorVueSfcBridgeOptions? options, out string? error)
    {
        options = null;
        error = null;

        var hostJazorRoot = string.Empty;
        var outputDirectory = string.Empty;
        string? manifestPath = null;
        var mode = RazorVueSfcBridgeMode.Browser;
        var production = true;
        var clean = true;

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
                case "--host-root":
                    hostJazorRoot = value;
                    break;
                case "--manifest":
                    manifestPath = value;
                    break;
                case "--out":
                    outputDirectory = value;
                    break;
                case "--mode":
                    if (!TryParseMode(value, out mode))
                    {
                        error = $"Invalid --mode value '{value}'. Expected 'browser' or 'ssr'.";
                        return false;
                    }

                    break;
                case "--production":
                    if (!TryParseBoolean(value, out production))
                    {
                        error = $"Invalid --production value '{value}'. Expected 'true' or 'false'.";
                        return false;
                    }

                    break;
                case "--clean":
                    if (!TryParseBoolean(value, out clean))
                    {
                        error = $"Invalid --clean value '{value}'. Expected 'true' or 'false'.";
                        return false;
                    }

                    break;
                default:
                    error = $"Unknown argument '{arg}'.";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(hostJazorRoot))
        {
            error = "Missing required argument --host-root.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            error = "Missing required argument --out.";
            return false;
        }

        var resolvedHostJazorRoot = Path.GetFullPath(hostJazorRoot);
        options = new RazorVueSfcBridgeOptions(
            resolvedHostJazorRoot,
            Path.GetFullPath(outputDirectory),
            string.IsNullOrWhiteSpace(manifestPath)
                ? Path.Combine(resolvedHostJazorRoot, "jazor-manifest.json")
                : Path.GetFullPath(manifestPath),
            mode,
            production,
            clean);
        return true;
    }

    private static bool TryParseMode(string value, out RazorVueSfcBridgeMode mode)
    {
        if (string.Equals(value, "browser", StringComparison.OrdinalIgnoreCase))
        {
            mode = RazorVueSfcBridgeMode.Browser;
            return true;
        }

        if (string.Equals(value, "ssr", StringComparison.OrdinalIgnoreCase))
        {
            mode = RazorVueSfcBridgeMode.Ssr;
            return true;
        }

        mode = RazorVueSfcBridgeMode.Browser;
        return false;
    }

    private static bool TryParseBoolean(string value, out bool result)
    {
        if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            return true;
        }

        if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            return true;
        }

        result = false;
        return false;
    }
}

internal enum RazorVueSfcBridgeMode
{
    Browser,
    Ssr
}
