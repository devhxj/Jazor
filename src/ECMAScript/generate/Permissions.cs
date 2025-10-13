namespace ECMAScript;

/// <summary>
/// PermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#PermissionDescriptor")]
public record PermissionDescriptor(
    [property: Description("@#name")]string? Name = default);

/// <summary>
/// PermissionSetParameters
/// </summary>
[ECMAScript]
[Description("@#PermissionSetParameters")]
public record PermissionSetParameters(
    [property: Description("@#descriptor")]object? Descriptor = default,
	[property: Description("@#state")]PermissionState? State = default);

/// <summary>
/// PermissionStatus
/// </summary>
[ECMAScript]
[Description("@#PermissionStatus")]
public class PermissionStatus : EventTarget
{
    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern PermissionState State { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}