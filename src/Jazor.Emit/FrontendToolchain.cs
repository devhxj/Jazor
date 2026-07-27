namespace Jazor.Emit;

internal enum FrontendToolchainKind
{
    Deno,
    Netpack
}

internal enum FrontendBuildMode
{
    Production,
    Development
}

internal enum FrontendToolchainCapability
{
    ProductionBuild,
    DevelopmentServer,
    Hmr,
    SourceMaps,
    Minify
}

internal sealed record FrontendToolchainDiagnostic(
    string Code,
    string Message);

internal sealed record FrontendToolchainRequest(
    FrontendToolchainKind Toolchain,
    string ManifestPath,
    string ArtifactRoot,
    string SourceRoot,
    string OutputRoot,
    FrontendBuildMode Mode,
    bool SourceMaps,
    bool Minify,
    IReadOnlyDictionary<string, string> Environment,
    IReadOnlySet<FrontendToolchainCapability> RequiredCapabilities,
    IReadOnlyDictionary<string, string> VersionConstraints)
{
    public const string DefaultBundleFileName = "bundle.js";

    public string BundleOutputPath => Path.Combine(OutputRoot, DefaultBundleFileName);

    public static FrontendToolchainRequest Create(
        FrontendToolchainKind toolchain,
        string manifestPath,
        string artifactRoot,
        string sourceRoot,
        string outputRoot,
        FrontendBuildMode mode = FrontendBuildMode.Production,
        bool sourceMaps = true,
        bool minify = false,
        IReadOnlyDictionary<string, string>? environment = null,
        IReadOnlySet<FrontendToolchainCapability>? requiredCapabilities = null,
        IReadOnlyDictionary<string, string>? versionConstraints = null)
    {
        return new FrontendToolchainRequest(
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
            CopyDictionary(versionConstraints));
    }

    private static string RequirePath(string path, string name)
        => string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentException("Path must be provided.", name)
            : path;

    private static IReadOnlyDictionary<string, string> CopyDictionary(IReadOnlyDictionary<string, string>? source)
        => source is null || source.Count == 0
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(source, StringComparer.Ordinal);

    private static IReadOnlySet<FrontendToolchainCapability> CopySet(IReadOnlySet<FrontendToolchainCapability>? source)
        => source is null || source.Count == 0
            ? new HashSet<FrontendToolchainCapability>()
            : new HashSet<FrontendToolchainCapability>(source);

}

internal sealed record FrontendToolchainCommand(
    FrontendBuildMode Mode,
    FrontendToolchainRequest Request)
{
    public static bool TryParse(string[] args, out FrontendToolchainCommand? command, out string? error)
    {
        command = null;
        error = null;

        if (args.Length == 0)
        {
            error = "Missing toolchain command. Expected 'build' or 'serve'.";
            return false;
        }

        FrontendBuildMode mode;
        switch (args[0].ToLowerInvariant())
        {
            case "build":
                mode = FrontendBuildMode.Production;
                break;
            case "serve":
                mode = FrontendBuildMode.Development;
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
                default:
                    error = $"Unknown argument '{arg}'.";
                    return false;
            }
        }

        if (!Enum.TryParse<FrontendToolchainKind>(toolchainText, ignoreCase: true, out var toolchain))
        {
            error = string.IsNullOrWhiteSpace(toolchainText)
                ? "Missing required argument --toolchain."
                : $"Unknown frontend toolchain '{toolchainText}'.";
            return false;
        }

        if (!TryRequireArgument(manifestPath, "--manifest", out error) ||
            !TryRequireArgument(artifactRoot, "--artifacts", out error) ||
            !TryRequireArgument(sourceRoot, "--source-root", out error) ||
            !TryRequireArgument(outputRoot, "--out-root", out error))
        {
            return false;
        }

        var capabilities = new HashSet<FrontendToolchainCapability>
        {
            mode == FrontendBuildMode.Production
                ? FrontendToolchainCapability.ProductionBuild
                : FrontendToolchainCapability.DevelopmentServer
        };

        if (mode == FrontendBuildMode.Development)
            capabilities.Add(FrontendToolchainCapability.Hmr);

        if (sourceMaps)
            capabilities.Add(FrontendToolchainCapability.SourceMaps);

        if (minify)
            capabilities.Add(FrontendToolchainCapability.Minify);

        command = new FrontendToolchainCommand(
            mode,
            FrontendToolchainRequest.Create(
                toolchain,
                manifestPath,
                artifactRoot,
                sourceRoot,
                outputRoot,
                mode,
                sourceMaps,
                minify,
                requiredCapabilities: capabilities));
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

internal sealed record FrontendToolchainResult(
    bool IsSuccess,
    FrontendToolchainKind Toolchain,
    int ExitCode,
    FrontendToolchainDiagnostic? Diagnostic,
    string? OutputPath,
    int ModuleCount)
{
    public static FrontendToolchainResult Success(FrontendToolchainKind toolchain, string outputPath, int moduleCount)
        => new(true, toolchain, 0, null, outputPath, moduleCount);

    public static FrontendToolchainResult Fail(FrontendToolchainKind toolchain, int exitCode, string code, string message)
        => new(false, toolchain, exitCode, new FrontendToolchainDiagnostic(code, message), null, 0);
}

internal sealed class FrontendToolchainRunner
{
    private const int ContractFailureExitCode = 10;
    private const int UnsupportedExitCode = 11;

    public async Task<FrontendToolchainResult> BuildAsync(FrontendToolchainRequest request)
    {
        var contractFailure = ValidateRequest(request);
        if (contractFailure is not null)
            return contractFailure;

        return request.Toolchain switch
        {
            FrontendToolchainKind.Deno => await BuildDenoAsync(request),
            FrontendToolchainKind.Netpack => await BuildNetpackAsync(request),
            _ => Unsupported(request, "JAZOR_TOOLCHAIN_UNKNOWN", $"Unsupported frontend toolchain '{request.Toolchain}'.")
        };
    }

    private static FrontendToolchainResult? ValidateRequest(FrontendToolchainRequest request)
    {
        if (!File.Exists(request.ManifestPath))
        {
            return FrontendToolchainResult.Fail(
                request.Toolchain,
                ContractFailureExitCode,
                "JAZOR_TOOLCHAIN_MANIFEST_NOT_FOUND",
                $"Manifest was not found: '{request.ManifestPath}'.");
        }

        if (!Directory.Exists(request.ArtifactRoot))
        {
            return FrontendToolchainResult.Fail(
                request.Toolchain,
                ContractFailureExitCode,
                "JAZOR_TOOLCHAIN_ARTIFACT_ROOT_NOT_FOUND",
                $"Artifact root was not found: '{request.ArtifactRoot}'.");
        }

        if (!Directory.Exists(request.SourceRoot))
        {
            return FrontendToolchainResult.Fail(
                request.Toolchain,
                ContractFailureExitCode,
                "JAZOR_TOOLCHAIN_SOURCE_ROOT_NOT_FOUND",
                $"Source root was not found: '{request.SourceRoot}'.");
        }

        return null;
    }

    private static FrontendToolchainResult Unsupported(FrontendToolchainRequest request, string code, string message)
        => FrontendToolchainResult.Fail(request.Toolchain, UnsupportedExitCode, code, message);

    private static FrontendToolchainResult UnsupportedCapability(FrontendToolchainRequest request, FrontendToolchainCapability capability)
        => Unsupported(
            request,
            "JAZOR_TOOLCHAIN_CAPABILITY_UNSUPPORTED",
            $"{request.Toolchain} does not support required capability '{capability}'.");

    private static async Task<FrontendToolchainResult> BuildDenoAsync(FrontendToolchainRequest request)
    {
        if (request.Mode != FrontendBuildMode.Production)
            return Unsupported(request, "JAZOR_TOOLCHAIN_MODE_UNSUPPORTED", "Deno development mode is not implemented yet.");

        foreach (var capability in request.RequiredCapabilities)
        {
            if (capability is FrontendToolchainCapability.ProductionBuild or FrontendToolchainCapability.SourceMaps)
                continue;

            return UnsupportedCapability(request, capability);
        }

        Directory.CreateDirectory(request.OutputRoot);

        var bundler = new ModuleBundler();
        var bundleResult = await bundler.BundleAsync(new BundleOptions(
            request.ArtifactRoot,
            request.ManifestPath,
            request.BundleOutputPath,
            request.SourceRoot));

        return bundleResult.IsSuccess
            ? FrontendToolchainResult.Success(request.Toolchain, bundleResult.OutputPath!, bundleResult.ModuleCount)
            : FrontendToolchainResult.Fail(
                request.Toolchain,
                bundleResult.ExitCode,
                "JAZOR_TOOLCHAIN_DENO_FAILED",
                bundleResult.Error ?? "Deno build failed.");
    }

    private static async Task<FrontendToolchainResult> BuildNetpackAsync(FrontendToolchainRequest request)
    {
        if (request.Mode != FrontendBuildMode.Production)
            return Unsupported(request, "JAZOR_TOOLCHAIN_MODE_UNSUPPORTED", "Netpack development mode is not implemented yet.");

        foreach (var capability in request.RequiredCapabilities)
        {
            if (capability is FrontendToolchainCapability.ProductionBuild or FrontendToolchainCapability.SourceMaps)
                continue;

            return UnsupportedCapability(request, capability);
        }

        Directory.CreateDirectory(request.OutputRoot);

        var bundler = new NetpackModuleBundler();
        var bundleResult = await bundler.BundleAsync(new BundleOptions(
            request.ArtifactRoot,
            request.ManifestPath,
            request.BundleOutputPath,
            request.SourceRoot));

        return bundleResult.IsSuccess
            ? FrontendToolchainResult.Success(request.Toolchain, bundleResult.OutputPath!, bundleResult.ModuleCount)
            : FrontendToolchainResult.Fail(
                request.Toolchain,
                bundleResult.ExitCode,
                "JAZOR_TOOLCHAIN_NETPACK_FAILED",
                bundleResult.Error ?? "Netpack build failed.");
    }
}
