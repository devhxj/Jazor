using System.Reflection;
using System.Text.Json;

namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionLoader : IAsyncDisposable
{
    private const string BuiltinSource = "builtin";
    private const string UserSource = "user";
    private const string ManifestFileName = "extension.json";

    private readonly IExtensionRegistry _registry;
    private readonly Action<ExtensionLoadInvocation>? _loadEventSink;
    private readonly Lock _stateGate = new();
    private readonly List<LoadedExtensionState> _loadedExtensions = [];

    private bool _disposed;

    public ExtensionLoader(
        IExtensionRegistry registry,
        Action<ExtensionLoadInvocation>? loadEventSink = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _loadEventSink = loadEventSink;
    }

    public async ValueTask LoadBuiltinExtensionsAsync(
        IEnumerable<IExtension> builtinExtensions,
        string rootDirectory,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(builtinExtensions);

        foreach (var extension in builtinExtensions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (extension is null)
            {
                continue;
            }

            var extensionDirectory = Path.GetDirectoryName(extension.GetType().Assembly.Location) ?? rootDirectory;
            var extensionId = NormalizeExtensionId(extension.Metadata.Id, extension.GetType().FullName ?? "builtin.unknown");
            try
            {
                await LoadExtensionCoreAsync(
                    extension,
                    rootDirectory,
                    extensionDirectory,
                    settings: null,
                    source: BuiltinSource,
                    extensionId: extensionId,
                    manifestPath: null,
                    assemblyPath: extension.GetType().Assembly.Location,
                    loadContext: null,
                    sandboxProfile: null,
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                ReportLoad(
                    extensionId,
                    BuiltinSource,
                    extensionDirectory,
                    manifestPath: null,
                    assemblyPath: extension.GetType().Assembly.Location,
                    status: ExtensionLoadStatus.Failed,
                    reason: $"builtin extension load failed: {ex.Message}");
                throw;
            }
        }
    }

    public async ValueTask LoadUserExtensionsAsync(
        ExtensionHostOptions options,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled || !Directory.Exists(options.ExtensionsDirectory))
        {
            return;
        }

        foreach (var extensionDirectory in Directory.EnumerateDirectories(options.ExtensionsDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await LoadUserExtensionFromDirectoryAsync(options, extensionDirectory, cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        List<LoadedExtensionState> loadedExtensions;
        lock (_stateGate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            loadedExtensions = [.. _loadedExtensions];
            _loadedExtensions.Clear();
        }

        foreach (var loaded in loadedExtensions.AsEnumerable().Reverse())
        {
            try
            {
                await loaded.Extension.DeactivateAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                ReportLoad(
                    extensionId: loaded.ExtensionId,
                    source: loaded.Source,
                    extensionDirectory: loaded.ExtensionDirectory,
                    manifestPath: loaded.ManifestPath,
                    assemblyPath: loaded.AssemblyPath,
                    status: ExtensionLoadStatus.Failed,
                    reason: $"deactivate failed: {ex.Message}");
            }
            finally
            {
                _registry.UnregisterExtension(loaded.Extension);
            }
        }

        var collectibleContexts = loadedExtensions
            .Select(static loaded => loaded.LoadContext)
            .OfType<CollectibleExtensionLoadContext>()
            .Distinct()
            .ToArray();
        if (collectibleContexts.Length == 0)
        {
            return;
        }

        foreach (var collectibleContext in collectibleContexts)
        {
            collectibleContext.Unload();
        }

        // Force finalization cycle so collectible contexts can fully unload and release file handles.
        for (var cycle = 0; cycle < 3; cycle++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private async ValueTask LoadUserExtensionFromDirectoryAsync(
        ExtensionHostOptions options,
        string extensionDirectory,
        CancellationToken cancellationToken)
    {
        var manifestPath = Path.Combine(extensionDirectory, ManifestFileName);
        var fallbackExtensionId = NormalizeExtensionId(
            Path.GetFileName(extensionDirectory),
            "user.unknown");
        var extensionId = fallbackExtensionId;
        string? assemblyPath = null;

        try
        {
            var manifest = ReadManifest(manifestPath);
            if (manifest is null)
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "manifest file is missing or invalid json");
                return;
            }

            if (string.IsNullOrWhiteSpace(manifest.Id))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "manifest id is required");
                return;
            }

            extensionId = NormalizeExtensionId(manifest.Id, fallbackExtensionId);
            if (string.IsNullOrWhiteSpace(manifest.Assembly))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "manifest assembly is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(manifest.Type))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "manifest type is required");
                return;
            }

            if (options.DisabledExtensionIds.Contains(extensionId))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "extension id is disabled by host policy");
                return;
            }

            if (options.TrustedExtensionIds.Count > 0
                && !options.TrustedExtensionIds.Contains(extensionId))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "extension id is not in trusted allow-list");
                return;
            }

            try
            {
                assemblyPath = ResolveAssemblyPath(extensionDirectory, manifest.Assembly);
            }
            catch (InvalidOperationException ex)
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    ex.Message);
                return;
            }

            if (!File.Exists(assemblyPath))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "extension assembly file does not exist");
                return;
            }

            if (options.RequireAssemblyHash
                && !ExtensionSecurityPolicy.IsAssemblyHashSatisfied(
                    assemblyPath,
                    manifest.AssemblySha256 ?? string.Empty))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    "assembly sha256 verification failed");
                return;
            }

            var requireSignatureValidation = options.RequireManifestSignature || manifest.Signature is not null;
            if (requireSignatureValidation
                && !ExtensionSecurityPolicy.IsManifestSignatureSatisfied(
                    manifest,
                    options.TrustedPublicKeys,
                    out var signatureFailureReason))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    signatureFailureReason ?? "manifest signature verification failed");
                return;
            }

            if (!ExtensionSecurityPolicy.IsSandboxPermissionSatisfied(
                    manifest,
                    options,
                    options.RootDirectory,
                    extensionDirectory,
                    out var sandboxFailureReason))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    sandboxFailureReason ?? "sandbox permission validation failed");
                return;
            }

            var sandboxProfile = ExtensionSecurityPolicy.CreateRuntimeSandboxProfile(
                manifest,
                options.RootDirectory,
                extensionDirectory);
            var settings = NormalizeSettings(manifest.Settings);
            if (manifest.Permissions?.ProcessIsolation == true)
            {
                var isolatedCreation = await TryCreateOutOfProcessExtensionAsync(
                    options.RootDirectory,
                    extensionDirectory,
                    assemblyPath,
                    manifest.Type,
                    sandboxProfile,
                    settings,
                    cancellationToken);
                if (!isolatedCreation.Success || isolatedCreation.Extension is null)
                {
                    ReportLoad(
                        extensionId,
                        UserSource,
                        extensionDirectory,
                        manifestPath,
                        assemblyPath,
                        ExtensionLoadStatus.Rejected,
                        isolatedCreation.FailureReason);
                    return;
                }

                var isolatedExtension = isolatedCreation.Extension;
                if (!string.Equals(isolatedExtension.Metadata.Id, extensionId, StringComparison.OrdinalIgnoreCase))
                {
                    await TryDeactivateSilentlyAsync(isolatedExtension);
                    ReportLoad(
                        extensionId,
                        UserSource,
                        extensionDirectory,
                        manifestPath,
                        assemblyPath,
                        ExtensionLoadStatus.Rejected,
                        $"extension metadata id '{isolatedExtension.Metadata.Id}' does not match manifest id '{extensionId}'");
                    return;
                }

                if (options.EnforceProviderPermissions
                    && !ExtensionSecurityPolicy.IsProviderPermissionSatisfied(
                        isolatedCreation.ProvidedCapabilities,
                        manifest,
                        out var isolatedPermissionFailureReason))
                {
                    await TryDeactivateSilentlyAsync(isolatedExtension);
                    ReportLoad(
                        extensionId,
                        UserSource,
                        extensionDirectory,
                        manifestPath,
                        assemblyPath,
                        ExtensionLoadStatus.Rejected,
                        isolatedPermissionFailureReason ?? "provider permission validation failed");
                    return;
                }

                await LoadExtensionCoreAsync(
                    isolatedExtension,
                    options.RootDirectory,
                    extensionDirectory,
                    settings,
                    source: UserSource,
                    extensionId: extensionId,
                    manifestPath: manifestPath,
                    assemblyPath: assemblyPath,
                    loadContext: null,
                    sandboxProfile: sandboxProfile,
                    cancellationToken: cancellationToken);
                return;
            }

            if (!TryCreateUserExtension(
                    assemblyPath,
                    manifest.Type,
                    out var extension,
                    out var loadContext,
                    out var creationFailureReason))
            {
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    creationFailureReason);
                return;
            }

            if (!string.Equals(extension.Metadata.Id, extensionId, StringComparison.OrdinalIgnoreCase))
            {
                loadContext.Unload();
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    $"extension metadata id '{extension.Metadata.Id}' does not match manifest id '{extensionId}'");
                return;
            }

            if (options.EnforceProviderPermissions
                && !ExtensionSecurityPolicy.IsProviderPermissionSatisfied(
                    extension.GetType(),
                    manifest,
                    out var providerPermissionFailureReason))
            {
                loadContext.Unload();
                ReportLoad(
                    extensionId,
                    UserSource,
                    extensionDirectory,
                    manifestPath,
                    assemblyPath,
                    ExtensionLoadStatus.Rejected,
                    providerPermissionFailureReason ?? "provider permission validation failed");
                return;
            }

            await LoadExtensionCoreAsync(
                extension,
                options.RootDirectory,
                extensionDirectory,
                settings,
                source: UserSource,
                extensionId: extensionId,
                manifestPath: manifestPath,
                assemblyPath: assemblyPath,
                loadContext: loadContext,
                sandboxProfile: sandboxProfile,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            ReportLoad(
                extensionId,
                UserSource,
                extensionDirectory,
                manifestPath,
                assemblyPath,
                ExtensionLoadStatus.Failed,
                $"unexpected load failure: {ex.Message}");
        }
    }

    private async ValueTask LoadExtensionCoreAsync(
        IExtension extension,
        string rootDirectory,
        string extensionDirectory,
        IReadOnlyDictionary<string, string>? settings,
        string source,
        string extensionId,
        string? manifestPath,
        string? assemblyPath,
        CollectibleExtensionLoadContext? loadContext,
        ExtensionSandboxProfile? sandboxProfile,
        CancellationToken cancellationToken)
    {
        var context = new ExtensionContext(
            rootDirectory: Path.GetFullPath(rootDirectory),
            extensionDirectory: Path.GetFullPath(extensionDirectory),
            registry: _registry,
            settings: settings,
            sandboxProfile: sandboxProfile);

        try
        {
            await extension.InitializeAsync(context, cancellationToken);
            await extension.ActivateAsync(cancellationToken);
            _registry.RegisterExtension(extension);
            TrackLoadedExtension(
                extension,
                extensionId,
                source,
                extensionDirectory,
                manifestPath,
                assemblyPath,
                loadContext);
            ReportLoad(
                extensionId,
                source,
                extensionDirectory,
                manifestPath,
                assemblyPath,
                ExtensionLoadStatus.Loaded,
                "extension loaded");
        }
        catch (Exception)
        {
            await TryDeactivateSilentlyAsync(extension);
            loadContext?.Unload();
            throw;
        }
    }

    private static IReadOnlyDictionary<string, string>? NormalizeSettings(Dictionary<string, JsonElement>? settings)
    {
        if (settings is null || settings.Count == 0)
        {
            return null;
        }

        var normalized = settings
            .Where(static pair => pair.Value.ValueKind is JsonValueKind.String
                or JsonValueKind.Number
                or JsonValueKind.True
                or JsonValueKind.False)
            .ToDictionary(
                static pair => pair.Key,
                static pair => pair.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);

        return normalized.Count == 0
            ? null
            : normalized;
    }

    private static ExtensionManifest? ReadManifest(string manifestPath)
    {
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ExtensionManifest>(
                File.ReadAllText(manifestPath),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            return null;
        }
    }

    private static bool TryCreateUserExtension(
        string assemblyPath,
        string extensionTypeName,
        out IExtension extension,
        out CollectibleExtensionLoadContext loadContext,
        out string failureReason)
    {
        extension = null!;
        loadContext = null!;
        failureReason = string.Empty;

        if (string.IsNullOrWhiteSpace(assemblyPath) || string.IsNullOrWhiteSpace(extensionTypeName))
        {
            failureReason = "assembly path and type name are required";
            return false;
        }

        CollectibleExtensionLoadContext? candidateContext = null;
        try
        {
            candidateContext = new CollectibleExtensionLoadContext(assemblyPath);
            var assembly = candidateContext.LoadMainAssembly(assemblyPath);
            var extensionType = assembly.GetType(extensionTypeName, throwOnError: false, ignoreCase: false);
            if (extensionType is null)
            {
                candidateContext.Unload();
                failureReason = $"extension type '{extensionTypeName}' was not found";
                return false;
            }

            if (!typeof(IExtension).IsAssignableFrom(extensionType))
            {
                candidateContext.Unload();
                failureReason = $"extension type '{extensionTypeName}' does not implement IExtension";
                return false;
            }

            if (Activator.CreateInstance(extensionType) is not IExtension created)
            {
                candidateContext.Unload();
                failureReason = $"extension type '{extensionTypeName}' cannot be instantiated";
                return false;
            }

            extension = created;
            loadContext = candidateContext;
            return true;
        }
        catch (Exception ex)
        {
            candidateContext?.Unload();
            failureReason = $"failed to load extension assembly: {ex.Message}";
            return false;
        }
    }

    private static async ValueTask<OutOfProcessExtensionCreationResult> TryCreateOutOfProcessExtensionAsync(
        string rootDirectory,
        string extensionDirectory,
        string assemblyPath,
        string extensionTypeName,
        ExtensionSandboxProfile sandboxProfile,
        IReadOnlyDictionary<string, string>? settings,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory)
            || string.IsNullOrWhiteSpace(extensionDirectory)
            || string.IsNullOrWhiteSpace(assemblyPath)
            || string.IsNullOrWhiteSpace(extensionTypeName))
        {
            return new OutOfProcessExtensionCreationResult(
                Success: false,
                Extension: null,
                ProvidedCapabilities: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                FailureReason: "process-isolated extension bootstrap requires root, extension directory, assembly path, and type name");
        }

        try
        {
            var proxy = await OutOfProcessExtensionProxy.CreateAsync(
                rootDirectory: Path.GetFullPath(rootDirectory),
                extensionDirectory: Path.GetFullPath(extensionDirectory),
                assemblyPath: Path.GetFullPath(assemblyPath),
                extensionTypeName: extensionTypeName,
                sandboxProfile: sandboxProfile,
                settings: settings,
                cancellationToken: cancellationToken);
            return new OutOfProcessExtensionCreationResult(
                Success: true,
                Extension: proxy,
                ProvidedCapabilities: proxy.ProvidedCapabilities,
                FailureReason: string.Empty);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new OutOfProcessExtensionCreationResult(
                Success: false,
                Extension: null,
                ProvidedCapabilities: new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                FailureReason: $"process-isolated worker bootstrap failed: {exception.Message}");
        }
    }

    private static async ValueTask TryDeactivateSilentlyAsync(IExtension extension)
    {
        try
        {
            await extension.DeactivateAsync(CancellationToken.None);
        }
        catch
        {
            // Ignore deactivate failures during failed activation path.
        }
    }

    private void TrackLoadedExtension(
        IExtension extension,
        string extensionId,
        string source,
        string extensionDirectory,
        string? manifestPath,
        string? assemblyPath,
        CollectibleExtensionLoadContext? loadContext)
    {
        lock (_stateGate)
        {
            _loadedExtensions.Add(new LoadedExtensionState(
                extension,
                extensionId,
                source,
                extensionDirectory,
                manifestPath,
                assemblyPath,
                loadContext));
        }
    }

    private void ReportLoad(
        string extensionId,
        string source,
        string extensionDirectory,
        string? manifestPath,
        string? assemblyPath,
        string status,
        string reason)
    {
        var invocation = new ExtensionLoadInvocation(
            ExtensionId: NormalizeExtensionId(extensionId, "unknown"),
            Source: source,
            ExtensionDirectory: Path.GetFullPath(extensionDirectory),
            ManifestPath: string.IsNullOrWhiteSpace(manifestPath)
                ? null
                : Path.GetFullPath(manifestPath),
            AssemblyPath: string.IsNullOrWhiteSpace(assemblyPath)
                ? null
                : Path.GetFullPath(assemblyPath),
            Status: status,
            Reason: reason,
            Timestamp: DateTimeOffset.UtcNow);
        _registry.ReportExtensionLoad(invocation);

        if (_loadEventSink is null)
        {
            return;
        }

        try
        {
            _loadEventSink(invocation);
        }
        catch
        {
            // Ignore sink errors to keep extension loading isolated from telemetry output.
        }
    }

    private void ThrowIfDisposed()
    {
        lock (_stateGate)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ExtensionLoader));
            }
        }
    }

    private static string ResolveAssemblyPath(
        string extensionDirectory,
        string assemblyPath)
    {
        var combined = Path.IsPathRooted(assemblyPath)
            ? Path.GetFullPath(assemblyPath)
            : Path.GetFullPath(Path.Combine(extensionDirectory, assemblyPath));

        var normalizedExtensionDirectory = Path.GetFullPath(extensionDirectory);
        if (!IsPathInsideDirectory(normalizedExtensionDirectory, combined))
        {
            throw new InvalidOperationException(
                $"Extension assembly path '{assemblyPath}' escapes extension directory '{extensionDirectory}'.");
        }

        return combined;
    }

    private static bool IsPathInsideDirectory(string directoryPath, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(directoryPath, candidatePath);
        return !string.IsNullOrWhiteSpace(relativePath)
            && !relativePath.StartsWith("..", StringComparison.Ordinal)
            && !Path.IsPathRooted(relativePath);
    }

    private static string NormalizeExtensionId(string? value, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value.Trim();
        }

        return string.IsNullOrWhiteSpace(fallback)
            ? "unknown"
            : fallback.Trim();
    }

    private sealed record LoadedExtensionState(
        IExtension Extension,
        string ExtensionId,
        string Source,
        string ExtensionDirectory,
        string? ManifestPath,
        string? AssemblyPath,
        CollectibleExtensionLoadContext? LoadContext);

    private sealed record OutOfProcessExtensionCreationResult(
        bool Success,
        IExtension? Extension,
        IReadOnlySet<string> ProvidedCapabilities,
        string FailureReason);
}
