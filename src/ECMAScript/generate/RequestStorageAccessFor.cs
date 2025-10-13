namespace ECMAScript;

/// <summary>
/// TopLevelStorageAccessPermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#TopLevelStorageAccessPermissionDescriptor")]
public record TopLevelStorageAccessPermissionDescriptor(
    [property: Description("@#requestedOrigin")]string? RequestedOrigin = default): PermissionDescriptor;