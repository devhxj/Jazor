namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionHostOptions
{
    public required string RootDirectory { get; init; }

    public bool Enabled { get; init; } = true;

    public required string ExtensionsDirectory { get; init; }

    public IReadOnlySet<string> DisabledExtensionIds { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
