using System.Reflection;
using System.Text.Json;

namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionLoader
{
    private const string ManifestFileName = "extension.json";

    private readonly IExtensionRegistry _registry;

    public ExtensionLoader(IExtensionRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async ValueTask LoadBuiltinExtensionsAsync(
        IEnumerable<IExtension> builtinExtensions,
        string rootDirectory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builtinExtensions);

        foreach (var extension in builtinExtensions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (extension is null)
            {
                continue;
            }

            var extensionDirectory = Path.GetDirectoryName(extension.GetType().Assembly.Location);
            await LoadExtensionCoreAsync(
                extension,
                rootDirectory,
                extensionDirectory ?? rootDirectory,
                settings: null,
                cancellationToken);
        }
    }

    public async ValueTask LoadUserExtensionsAsync(
        ExtensionHostOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled || !Directory.Exists(options.ExtensionsDirectory))
        {
            return;
        }

        foreach (var extensionDirectory in Directory.EnumerateDirectories(options.ExtensionsDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var extension = CreateExtensionFromDirectory(extensionDirectory);
                if (extension is null)
                {
                    continue;
                }

                if (options.DisabledExtensionIds.Contains(extension.Metadata.Id))
                {
                    continue;
                }

                var manifest = ReadManifest(extensionDirectory);
                var settings = manifest?.Settings is null
                    ? null
                    : manifest.Settings
                        .Where(static pair => pair.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
                        .ToDictionary(
                            static pair => pair.Key,
                            static pair => pair.Value.ToString(),
                            StringComparer.OrdinalIgnoreCase);
                await LoadExtensionCoreAsync(
                    extension,
                    options.RootDirectory,
                    extensionDirectory,
                    settings,
                    cancellationToken);
            }
            catch
            {
                // Phase7 initial rollout: extension load failures should not block host startup.
            }
        }
    }

    private static IExtension? CreateExtensionFromDirectory(string extensionDirectory)
    {
        var manifest = ReadManifest(extensionDirectory);
        if (manifest is null
            || string.IsNullOrWhiteSpace(manifest.Assembly)
            || string.IsNullOrWhiteSpace(manifest.Type))
        {
            return null;
        }

        var assemblyPath = ResolveAssemblyPath(extensionDirectory, manifest.Assembly);
        if (!File.Exists(assemblyPath))
        {
            return null;
        }

        var assembly = Assembly.LoadFrom(assemblyPath);
        var extensionType = assembly.GetType(manifest.Type, throwOnError: false, ignoreCase: false);
        if (extensionType is null || !typeof(IExtension).IsAssignableFrom(extensionType))
        {
            return null;
        }

        if (Activator.CreateInstance(extensionType) is not IExtension extension)
        {
            return null;
        }

        return extension;
    }

    private async ValueTask LoadExtensionCoreAsync(
        IExtension extension,
        string rootDirectory,
        string extensionDirectory,
        IReadOnlyDictionary<string, string>? settings,
        CancellationToken cancellationToken)
    {
        var context = new ExtensionContext(
            rootDirectory: Path.GetFullPath(rootDirectory),
            extensionDirectory: Path.GetFullPath(extensionDirectory),
            registry: _registry,
            settings: settings);
        await extension.InitializeAsync(context, cancellationToken);
        _registry.RegisterExtension(extension);
        await extension.ActivateAsync(cancellationToken);
    }

    private static ExtensionManifest? ReadManifest(string extensionDirectory)
    {
        var manifestPath = Path.Combine(extensionDirectory, ManifestFileName);
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

    private static string ResolveAssemblyPath(
        string extensionDirectory,
        string assemblyPath)
    {
        var combined = Path.IsPathRooted(assemblyPath)
            ? Path.GetFullPath(assemblyPath)
            : Path.GetFullPath(Path.Combine(extensionDirectory, assemblyPath));

        var normalizedExtensionDirectory = Path.GetFullPath(extensionDirectory);
        if (!combined.StartsWith(normalizedExtensionDirectory, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Extension assembly path '{assemblyPath}' escapes extension directory '{extensionDirectory}'.");
        }

        return combined;
    }
}
