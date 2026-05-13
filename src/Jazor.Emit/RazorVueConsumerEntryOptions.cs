namespace Jazor.Emit;

internal sealed record RazorVueConsumerEntryOptions(
    string HostJazorRoot,
    string OutputDirectory,
    string? ManifestPath,
    string? HostRequirementsModulePath,
    string? BrowserGeneratedRoot,
    string? SsrGeneratedRoot,
    string? ClientEntryPath,
    string? SsrEntryPath,
    string? VueFeatureFlagsPath,
    string? ClientRuntimeModulePath,
    string? SsrRuntimeModulePath,
    string? ClientRuntimeExportName,
    string? SsrRuntimeExportName,
    string? SsrExecuteExportName,
    IReadOnlyList<RazorVueConsumerComponentSelection> Components,
    RazorVueConsumerEntryMode Mode,
    bool Production,
    bool Clean,
    string? WriteResultPath)
{
    public static bool TryParse(string[] args, out RazorVueConsumerEntryOptions? options, out string? error)
    {
        options = null;
        error = null;

        var hostJazorRoot = string.Empty;
        var outputDirectory = string.Empty;
        string? manifestPath = null;
        string? hostRequirementsModulePath = null;
        string? browserGeneratedRoot = null;
        string? ssrGeneratedRoot = null;
        string? clientEntryPath = null;
        string? ssrEntryPath = null;
        string? vueFeatureFlagsPath = null;
        string? clientRuntimeModulePath = null;
        string? ssrRuntimeModulePath = null;
        string? clientRuntimeExportName = null;
        string? ssrRuntimeExportName = null;
        string? ssrExecuteExportName = null;
        var components = new List<RazorVueConsumerComponentSelection>();
        var mode = RazorVueConsumerEntryMode.Both;
        var production = true;
        var clean = true;
        string? writeResultPath = null;

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
                case "--out":
                    outputDirectory = value;
                    break;
                case "--manifest":
                    manifestPath = value;
                    break;
                case "--host-requirements":
                    hostRequirementsModulePath = value;
                    break;
                case "--browser-generated-root":
                    browserGeneratedRoot = value;
                    break;
                case "--ssr-generated-root":
                    ssrGeneratedRoot = value;
                    break;
                case "--client-entry":
                    clientEntryPath = value;
                    break;
                case "--ssr-entry":
                    ssrEntryPath = value;
                    break;
                case "--vue-feature-flags":
                    vueFeatureFlagsPath = value;
                    break;
                case "--client-runtime":
                    clientRuntimeModulePath = value;
                    break;
                case "--ssr-runtime":
                    ssrRuntimeModulePath = value;
                    break;
                case "--client-runtime-export":
                    clientRuntimeExportName = value;
                    break;
                case "--ssr-runtime-export":
                    ssrRuntimeExportName = value;
                    break;
                case "--ssr-execute-export":
                    ssrExecuteExportName = value;
                    break;
                case "--component":
                    if (!TryParseComponentSelection(value, out var component, out error))
                        return false;

                    components.Add(component);
                    break;
                case "--mode":
                    if (!TryParseMode(value, out mode))
                    {
                        error = $"Invalid --mode value '{value}'. Expected 'browser', 'ssr', or 'both'.";
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
                case "--write-result":
                    writeResultPath = value;
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

        if (components.Count == 0)
        {
            error = "At least one --component Alias=selector argument is required.";
            return false;
        }

        var resolvedHostJazorRoot = Path.GetFullPath(hostJazorRoot);
        var resolvedOutputDirectory = Path.GetFullPath(outputDirectory);

        options = new RazorVueConsumerEntryOptions(
            resolvedHostJazorRoot,
            resolvedOutputDirectory,
            ResolveOptionalPath(manifestPath, resolvedHostJazorRoot, "jazor-manifest-razorvue.json"),
            ResolveOptionalPath(hostRequirementsModulePath, resolvedHostJazorRoot, "__jazor", "razorvue-host.mjs"),
            ResolveOptionalPath(browserGeneratedRoot, resolvedOutputDirectory, "generated-browser"),
            ResolveOptionalPath(ssrGeneratedRoot, resolvedOutputDirectory, "generated-ssr"),
            ResolveOptionalPath(clientEntryPath, resolvedOutputDirectory, "client-entry.mjs"),
            ResolveOptionalPath(ssrEntryPath, resolvedOutputDirectory, "ssr-entry.mjs"),
            ResolveOptionalPath(vueFeatureFlagsPath, resolvedOutputDirectory, "vue-feature-flags.mjs"),
            ResolveOptionalPath(clientRuntimeModulePath, Directory.GetCurrentDirectory()),
            ResolveOptionalPath(ssrRuntimeModulePath, Directory.GetCurrentDirectory()),
            string.IsNullOrWhiteSpace(clientRuntimeExportName) ? null : clientRuntimeExportName.Trim(),
            string.IsNullOrWhiteSpace(ssrRuntimeExportName) ? null : ssrRuntimeExportName.Trim(),
            string.IsNullOrWhiteSpace(ssrExecuteExportName) ? "executeSsr" : ssrExecuteExportName.Trim(),
            components,
            mode,
            production,
            clean,
            ResolveOptionalPath(writeResultPath, resolvedOutputDirectory, "razorvue-consumer-entry.json"));
        return true;
    }

    private static bool TryParseComponentSelection(
        string value,
        out RazorVueConsumerComponentSelection component,
        out string? error)
    {
        component = new RazorVueConsumerComponentSelection(string.Empty, string.Empty);
        error = null;

        var separatorIndex = value.IndexOf('=', StringComparison.Ordinal);
        if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
        {
            error = $"Invalid --component value '{value}'. Expected Alias=selector.";
            return false;
        }

        var alias = value[..separatorIndex].Trim();
        var selector = value[(separatorIndex + 1)..].Trim();
        if (alias.Length == 0 || selector.Length == 0)
        {
            error = $"Invalid --component value '{value}'. Expected non-empty Alias=selector.";
            return false;
        }

        component = new RazorVueConsumerComponentSelection(alias, selector);
        return true;
    }

    private static string? ResolveOptionalPath(string? value, string baseDirectory, params string[] defaultSegments)
    {
        var configured = string.IsNullOrWhiteSpace(value)
            ? defaultSegments.Length == 0 ? null : Path.Combine(defaultSegments)
            : value.Trim();
        if (string.IsNullOrWhiteSpace(configured))
            return null;

        return Path.GetFullPath(Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(baseDirectory, configured));
    }

    private static bool TryParseMode(string value, out RazorVueConsumerEntryMode mode)
    {
        if (string.Equals(value, "browser", StringComparison.OrdinalIgnoreCase))
        {
            mode = RazorVueConsumerEntryMode.Browser;
            return true;
        }

        if (string.Equals(value, "ssr", StringComparison.OrdinalIgnoreCase))
        {
            mode = RazorVueConsumerEntryMode.Ssr;
            return true;
        }

        if (string.Equals(value, "both", StringComparison.OrdinalIgnoreCase))
        {
            mode = RazorVueConsumerEntryMode.Both;
            return true;
        }

        mode = RazorVueConsumerEntryMode.Both;
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

internal sealed record RazorVueConsumerComponentSelection(
    string Alias,
    string Selector);

internal enum RazorVueConsumerEntryMode
{
    Browser,
    Ssr,
    Both
}
