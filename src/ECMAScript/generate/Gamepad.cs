namespace ECMAScript;

/// <summary>
/// GamepadTouch
/// </summary>
[ECMAScript]
[Description("@#GamepadTouch")]
public record GamepadTouch(
    [property: Description("@#touchId")]uint TouchId = default,
	[property: Description("@#surfaceId")]byte SurfaceId = default,
	[property: Description("@#position")]DOMPointReadOnly? Position = default,
	[property: Description("@#surfaceDimensions")]DOMRectReadOnly? SurfaceDimensions = default);

/// <summary>
/// GamepadEffectParameters
/// </summary>
[ECMAScript]
[Description("@#GamepadEffectParameters")]
public record GamepadEffectParameters(
    [property: Description("@#duration")]ulong Duration = 0,
	[property: Description("@#startDelay")]ulong StartDelay = 0,
	[property: Description("@#strongMagnitude")]double StrongMagnitude = 0.0d,
	[property: Description("@#weakMagnitude")]double WeakMagnitude = 0.0d,
	[property: Description("@#leftTrigger")]double LeftTrigger = 0.0d,
	[property: Description("@#rightTrigger")]double RightTrigger = 0.0d);

/// <summary>
/// GamepadEventInit
/// </summary>
[ECMAScript]
[Description("@#GamepadEventInit")]
public record GamepadEventInit(
    [property: Description("@#gamepad")]Gamepad? Gamepad = default): EventInit;

/// <summary>
/// GamepadButton
/// </summary>
[ECMAScript]
[Description("@#GamepadButton")]
public class GamepadButton
{
    /// <summary>
    /// pressed
    /// </summary>
    [Description("@#pressed")]
    public extern bool Pressed { get; }

    /// <summary>
    /// touched
    /// </summary>
    [Description("@#touched")]
    public extern bool Touched { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern double Value { get; }
}

/// <summary>
/// GamepadEvent
/// </summary>
[ECMAScript]
[Description("@#GamepadEvent")]
public class GamepadEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern GamepadEvent(string type, GamepadEventInit eventInitDict);

    /// <summary>
    /// gamepad
    /// </summary>
    [Description("@#gamepad")]
    public extern Gamepad Gamepad { get; }
}