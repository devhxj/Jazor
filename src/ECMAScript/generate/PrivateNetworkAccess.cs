namespace ECMAScript;

/// <summary>
/// PrivateNetworkAccessPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#PrivateNetworkAccessPermissionDescriptor")]
public record PrivateNetworkAccessPermissionDescriptor(
    [property: Description("@#id")]string? Id = default): PermissionDescriptor;