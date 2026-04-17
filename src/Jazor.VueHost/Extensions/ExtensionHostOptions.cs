namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionHostOptions
{
    public required string RootDirectory { get; init; }

    public bool Enabled { get; init; } = true;

    public required string ExtensionsDirectory { get; init; }

    public bool AllowExternalDirectory { get; init; }

    public IReadOnlySet<string> DisabledExtensionIds { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlySet<string> TrustedExtensionIds { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, string> TrustedPublicKeys { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public bool RequireAssemblyHash { get; init; } = true;

    public bool EnforceProviderPermissions { get; init; } = true;

    public bool RequireManifestSignature { get; init; } = true;
}
