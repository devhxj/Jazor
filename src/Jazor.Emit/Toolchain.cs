namespace Jazor.Emit;

/// <summary>Selects the local JavaScript bundling engine.</summary>
internal enum ToolchainKind
{
    Deno,
    Netpack
}

/// <summary>Chooses development or production library entries.</summary>
internal enum BuildMode
{
    Production,
    Development
}

/// <summary>Declares behavior a selected toolchain must support.</summary>
internal enum ToolchainCapability
{
    ProductionBuild,
    DevelopmentServer,
    Hmr,
    SourceMaps,
    Minify
}

/// <summary>Stable error code and message returned by the toolchain boundary.</summary>
internal sealed record ToolchainDiagnostic(
    string Code,
    string Message);

/// <summary>Normalized inputs shared by the Deno and Netpack lanes.</summary>
internal sealed record ToolchainRequest(
    ToolchainKind Toolchain,
    string ManifestPath,
    string ArtifactRoot,
    string SourceRoot,
    string OutputRoot,
    BuildMode Mode,
    bool SourceMaps,
    bool Minify,
    IReadOnlyDictionary<string, string> Environment,
    IReadOnlySet<ToolchainCapability> RequiredCapabilities,
    IReadOnlyDictionary<string, string> VersionConstraints,
    IReadOnlyList<string> LibraryManifests)
{
    public const string DefaultBundleFileName = "bundle.js";

    public string BundleOutputPath => Path.Combine(OutputRoot, DefaultBundleFileName);

    public static ToolchainRequest Create(
        ToolchainKind toolchain,
        string manifestPath,
        string artifactRoot,
        string sourceRoot,
        string outputRoot,
        BuildMode mode = BuildMode.Production,
        bool sourceMaps = true,
        bool minify = false,
        IReadOnlyDictionary<string, string>? environment = null,
        IReadOnlySet<ToolchainCapability>? requiredCapabilities = null,
        IReadOnlyDictionary<string, string>? versionConstraints = null,
        IReadOnlyList<string>? libraryManifests = null)
    {
        return new ToolchainRequest(
            toolchain,
            Path.GetFullPath(RequirePath(manifestPath, nameof(manifestPath))),
            Path.GetFullPath(RequirePath(artifactRoot, nameof(artifactRoot))),
            Path.GetFullPath(RequirePath(sourceRoot, nameof(sourceRoot))),
            Path.GetFullPath(RequirePath(outputRoot, nameof(outputRoot))),
            mode,
            sourceMaps,
            minify,
            CopyDictionary(environment),
            CopySet(requiredCapabilities),
            CopyDictionary(versionConstraints),
            CopyManifestPaths(libraryManifests));
    }

    private static string RequirePath(string path, string name)
        => string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentException("Path must be provided.", name)
            : path;

    private static IReadOnlyDictionary<string, string> CopyDictionary(IReadOnlyDictionary<string, string>? source)
        => source is null || source.Count == 0
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(source, StringComparer.Ordinal);

    private static IReadOnlySet<ToolchainCapability> CopySet(IReadOnlySet<ToolchainCapability>? source)
        => source is null || source.Count == 0
            ? new HashSet<ToolchainCapability>()
            : new HashSet<ToolchainCapability>(source);

    private static IReadOnlyList<string> CopyManifestPaths(IReadOnlyList<string>? source)
        => source is null || source.Count == 0
            ? []
            : source
                .Where(static path => !string.IsNullOrWhiteSpace(path))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
}

/// <summary>Parses the toolchain CLI contract into a normalized request.</summary>
internal sealed record ToolchainCommand(
    BuildMode Mode,
    ToolchainRequest Request)
{
    public static bool TryParse(string[] args, out ToolchainCommand? command, out string? error)
    {
        command = null;
        error = null;

        if (args.Length == 0)
        {
            error = "Missing toolchain command. Expected 'build' or 'serve'.";
            return false;
        }

        BuildMode mode;
        switch (args[0].ToLowerInvariant())
        {
            case "build":
                mode = BuildMode.Production;
                break;
            case "serve":
                mode = BuildMode.Development;
                break;
            default:
                error = $"Unknown toolchain command '{args[0]}'.";
                return false;
        }

        var toolchainText = string.Empty;
        var manifestPath = string.Empty;
        var artifactRoot = string.Empty;
        var sourceRoot = string.Empty;
        var outputRoot = string.Empty;
        var sourceMaps = true;
        var minify = false;
        var libraryManifests = new List<string>();

        for (var i = 1; i < args.Length; i++)
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
                case "--toolchain":
                    toolchainText = value;
                    break;
                case "--manifest":
                    manifestPath = value;
                    break;
                case "--artifacts":
                    artifactRoot = value;
                    break;
                case "--source-root":
                    sourceRoot = value;
                    break;
                case "--out-root":
                    outputRoot = value;
                    break;
                case "--sourcemaps":
                    if (!bool.TryParse(value, out sourceMaps))
                    {
                        error = $"Invalid boolean value for --sourcemaps: '{value}'.";
                        return false;
                    }

                    break;
                case "--minify":
                    if (!bool.TryParse(value, out minify))
                    {
                        error = $"Invalid boolean value for --minify: '{value}'.";
                        return false;
                    }

                    break;
                case "--library-manifest":
                    libraryManifests.Add(value);
                    break;
                default:
                    error = $"Unknown argument '{arg}'.";
                    return false;
            }
        }

        if (!Enum.TryParse<ToolchainKind>(toolchainText, ignoreCase: true, out var toolchain))
        {
            error = string.IsNullOrWhiteSpace(toolchainText)
                ? "Missing required argument --toolchain."
                : $"Unknown toolchain '{toolchainText}'.";
            return false;
        }

        if (!TryRequireArgument(manifestPath, "--manifest", out error) ||
            !TryRequireArgument(artifactRoot, "--artifacts", out error) ||
            !TryRequireArgument(sourceRoot, "--source-root", out error) ||
            !TryRequireArgument(outputRoot, "--out-root", out error))
        {
            return false;
        }

        var capabilities = new HashSet<ToolchainCapability>
        {
            mode == BuildMode.Production
                ? ToolchainCapability.ProductionBuild
                : ToolchainCapability.DevelopmentServer
        };

        if (mode == BuildMode.Development)
            capabilities.Add(ToolchainCapability.Hmr);

        if (sourceMaps)
            capabilities.Add(ToolchainCapability.SourceMaps);

        if (minify)
            capabilities.Add(ToolchainCapability.Minify);

        command = new ToolchainCommand(
            mode,
            ToolchainRequest.Create(
                toolchain,
                manifestPath,
                artifactRoot,
                sourceRoot,
                outputRoot,
                mode,
                sourceMaps,
                minify,
                requiredCapabilities: capabilities,
                libraryManifests: libraryManifests));
        return true;
    }

    private static bool TryRequireArgument(string value, string name, out string? error)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            error = null;
            return true;
        }

        error = $"Missing required argument {name}.";
        return false;
    }
}

/// <summary>Reports a toolchain build without leaking engine-specific result types.</summary>
internal sealed record ToolchainResult(
    bool IsSuccess,
    ToolchainKind Toolchain,
    int ExitCode,
    ToolchainDiagnostic? Diagnostic,
    string? OutputPath,
    int ModuleCount)
{
    public static ToolchainResult Success(ToolchainKind toolchain, string outputPath, int moduleCount)
        => new(true, toolchain, 0, null, outputPath, moduleCount);

    public static ToolchainResult Fail(ToolchainKind toolchain, int exitCode, string code, string message)
        => new(false, toolchain, exitCode, new ToolchainDiagnostic(code, message), null, 0);
}

/// <summary>Validates a request and dispatches it to the selected local bundler.</summary>
internal sealed class Toolchain
{
    private const int ContractFailureExitCode = 10;
    private const int UnsupportedExitCode = 11;

    public async Task<ToolchainResult> BuildAsync(ToolchainRequest request)
    {
        var contractFailure = ValidateRequest(request);
        if (contractFailure is not null)
            return contractFailure;

        return request.Toolchain switch
        {
            ToolchainKind.Deno => await BuildDenoAsync(request),
            ToolchainKind.Netpack => await BuildNetpackAsync(request),
            _ => Unsupported(request, "JAZOR_TOOLCHAIN_UNKNOWN", $"Unsupported toolchain '{request.Toolchain}'.")
        };
    }

    private static ToolchainResult? ValidateRequest(ToolchainRequest request)
    {
        if (!File.Exists(request.ManifestPath))
        {
            return ToolchainResult.Fail(
                request.Toolchain,
                ContractFailureExitCode,
                "JAZOR_TOOLCHAIN_MANIFEST_NOT_FOUND",
                $"Manifest was not found: '{request.ManifestPath}'.");
        }

        if (!Directory.Exists(request.ArtifactRoot))
        {
            return ToolchainResult.Fail(
                request.Toolchain,
                ContractFailureExitCode,
                "JAZOR_TOOLCHAIN_ARTIFACT_ROOT_NOT_FOUND",
                $"Artifact root was not found: '{request.ArtifactRoot}'.");
        }

        if (!Directory.Exists(request.SourceRoot))
        {
            return ToolchainResult.Fail(
                request.Toolchain,
                ContractFailureExitCode,
                "JAZOR_TOOLCHAIN_SOURCE_ROOT_NOT_FOUND",
                $"Source root was not found: '{request.SourceRoot}'.");
        }

        foreach (var libraryManifest in request.LibraryManifests)
        {
            if (!File.Exists(libraryManifest))
            {
                return ToolchainResult.Fail(
                    request.Toolchain,
                    ContractFailureExitCode,
                    "JAZOR_TOOLCHAIN_LIBRARY_MANIFEST_NOT_FOUND",
                    $"Library manifest was not found: '{libraryManifest}'.");
            }
        }

        return null;
    }

    private static ToolchainResult Unsupported(ToolchainRequest request, string code, string message)
        => ToolchainResult.Fail(request.Toolchain, UnsupportedExitCode, code, message);

    private static ToolchainResult UnsupportedCapability(ToolchainRequest request, ToolchainCapability capability)
        => Unsupported(
            request,
            "JAZOR_TOOLCHAIN_CAPABILITY_UNSUPPORTED",
            $"{request.Toolchain} does not support required capability '{capability}'.");

    private static async Task<ToolchainResult> BuildDenoAsync(ToolchainRequest request)
    {
        if (request.Mode != BuildMode.Production)
            return Unsupported(request, "JAZOR_TOOLCHAIN_MODE_UNSUPPORTED", "Deno development mode is not implemented yet.");

        foreach (var capability in request.RequiredCapabilities)
        {
            if (capability is ToolchainCapability.ProductionBuild or ToolchainCapability.SourceMaps)
                continue;

            return UnsupportedCapability(request, capability);
        }

        Directory.CreateDirectory(request.OutputRoot);

        var bundler = new DenoBundler();
        var bundleResult = await bundler.BundleAsync(new BundleOptions(
            request.ArtifactRoot,
            request.ManifestPath,
            request.BundleOutputPath,
            request.SourceRoot,
            request.LibraryManifests));

        return bundleResult.IsSuccess
            ? ToolchainResult.Success(request.Toolchain, bundleResult.OutputPath!, bundleResult.ModuleCount)
            : ToolchainResult.Fail(
                request.Toolchain,
                bundleResult.ExitCode,
                "JAZOR_TOOLCHAIN_DENO_FAILED",
                bundleResult.Error ?? "Deno build failed.");
    }

    private static async Task<ToolchainResult> BuildNetpackAsync(ToolchainRequest request)
    {
        if (request.Mode != BuildMode.Production)
            return Unsupported(request, "JAZOR_TOOLCHAIN_MODE_UNSUPPORTED", "Netpack development mode is not implemented yet.");

        foreach (var capability in request.RequiredCapabilities)
        {
            if (capability is ToolchainCapability.ProductionBuild or ToolchainCapability.SourceMaps)
                continue;

            return UnsupportedCapability(request, capability);
        }

        Directory.CreateDirectory(request.OutputRoot);

        var bundler = new NetpackBundler();
        var bundleResult = await bundler.BundleAsync(new BundleOptions(
            request.ArtifactRoot,
            request.ManifestPath,
            request.BundleOutputPath,
            request.SourceRoot,
            request.LibraryManifests));

        return bundleResult.IsSuccess
            ? ToolchainResult.Success(request.Toolchain, bundleResult.OutputPath!, bundleResult.ModuleCount)
            : ToolchainResult.Fail(
                request.Toolchain,
                bundleResult.ExitCode,
                "JAZOR_TOOLCHAIN_NETPACK_FAILED",
                bundleResult.Error ?? "Netpack build failed.");
    }
}
