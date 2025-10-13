namespace ECMAScript;

/// <summary>
/// IdleOptions
/// </summary>
[ECMAScript]
[Description("@#IdleOptions")]
public record IdleOptions(
    [property: Description("@#threshold")]ulong Threshold = default,
	[property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// IdleDetector
/// </summary>
[ECMAScript]
[Description("@#IdleDetector")]
public class IdleDetector : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern IdleDetector();

    /// <summary>
    /// userState
    /// </summary>
    [Description("@#userState")]
    public extern UserIdleState? UserState { get; }

    /// <summary>
    /// screenState
    /// </summary>
    [Description("@#screenState")]
    public extern ScreenIdleState? ScreenState { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }

    /// <summary>
    /// requestPermission
    /// </summary>
    [Description("@#requestPermission")]
    public extern static PromiseResult<PermissionState> RequestPermission();

    /// <summary>
    /// start
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#start")]
    public extern PromiseResult<object> Start(IdleOptions? options = default);
}