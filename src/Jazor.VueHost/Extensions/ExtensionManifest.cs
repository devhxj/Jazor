using System.Text.Json;

namespace Jazor.VueHost.Extensions;

internal sealed class ExtensionManifest
{
    public string? Id { get; init; }

    public string? Assembly { get; init; }

    public string? AssemblySha256 { get; init; }

    public string? Type { get; init; }

    public ExtensionPermissionManifest? Permissions { get; init; }

    public ExtensionSignatureManifest? Signature { get; init; }

    public Dictionary<string, JsonElement>? Settings { get; init; }
}

internal sealed class ExtensionPermissionManifest
{
    public string[]? Providers { get; init; }

    public ExtensionIoPermissionManifest? Io { get; init; }

    public ExtensionNetworkPermissionManifest? Network { get; init; }

    public bool? ProcessIsolation { get; init; }
}

internal sealed class ExtensionSignatureManifest
{
    public string? KeyId { get; init; }

    public string? Algorithm { get; init; }

    public string? Value { get; init; }
}

internal sealed class ExtensionIoPermissionManifest
{
    public string? Level { get; init; }

    public string[]? ReadRoots { get; init; }

    public string[]? WriteRoots { get; init; }
}

internal sealed class ExtensionNetworkPermissionManifest
{
    public string? Level { get; init; }

    public string[]? AllowedHosts { get; init; }
}
