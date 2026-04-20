namespace Jolt.Extensions;

internal sealed class ExtensionHostOptions
{
    public const string IoCapabilityNone = "none";
    public const string IoCapabilityRead = "read";
    public const string IoCapabilityReadWrite = "readWrite";

    public const string NetworkCapabilityNone = "none";
    public const string NetworkCapabilityLoopback = "loopback";
    public const string NetworkCapabilityInternet = "internet";

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

    public bool RequireProcessIsolation { get; init; }

    public string MaxIoCapability { get; init; } = IoCapabilityRead;

    public string MaxNetworkCapability { get; init; } = NetworkCapabilityLoopback;

    public string? LoadLogFilePath { get; init; }

    public int LoadEventRetention { get; init; } = 200;

    public string? ProviderLogFilePath { get; init; }

    public int ProviderEventRetention { get; init; } = 500;
}
