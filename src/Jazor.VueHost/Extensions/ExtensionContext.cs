namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionContext
{
    public ExtensionContext(
        string rootDirectory,
        string extensionDirectory,
        IExtensionRegistry registry,
        IReadOnlyDictionary<string, string>? settings = null)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(rootDirectory));
        }

        if (string.IsNullOrWhiteSpace(extensionDirectory))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(extensionDirectory));
        }

        RootDirectory = rootDirectory;
        ExtensionDirectory = extensionDirectory;
        Registry = registry ?? throw new ArgumentNullException(nameof(registry));
        Settings = settings ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public string RootDirectory { get; }

    public string ExtensionDirectory { get; }

    public IExtensionRegistry Registry { get; }

    public IReadOnlyDictionary<string, string> Settings { get; }
}
