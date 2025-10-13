namespace ECMAScript;

/// <summary>
/// InputDeviceCapabilitiesInit
/// </summary>
[ECMAScript]
[Description("@#InputDeviceCapabilitiesInit")]
public record InputDeviceCapabilitiesInit(
    [property: Description("@#firesTouchEvents")]bool FiresTouchEvents = false,
	[property: Description("@#pointerMovementScrolls")]bool PointerMovementScrolls = false);

/// <summary>
/// UIEventInit
/// </summary>
[ECMAScript]
[Description("@#UIEventInit")]
public record UIEventInit(
    [property: Description("@#sourceCapabilities")]InputDeviceCapabilities? SourceCapabilities = null,
	[property: Description("@#view")]Window? View = null,
	[property: Description("@#detail")]int Detail = 0,
	[property: Description("@#which")]uint Which = 0): EventInit
{
    [Category("optional")]
    public extern static UIEventInit OptionalSourceCapabilities(
        [Description("@#sourceCapabilities")]InputDeviceCapabilities? sourceCapabilities = null);

    [Category("optional")]
    public extern static UIEventInit OptionalViewDetail(
        [Description("@#view")]Window? view = null,
        [Description("@#detail")]int detail = 0);

    [Category("optional")]
    public extern static UIEventInit OptionalWhich(
        [Description("@#which")]uint which = 0);
}

/// <summary>
/// InputDeviceCapabilities
/// </summary>
[ECMAScript]
[Description("@#InputDeviceCapabilities")]
public class InputDeviceCapabilities
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="deviceInitDict">deviceInitDict</param>
    public extern InputDeviceCapabilities(InputDeviceCapabilitiesInit deviceInitDict);

    /// <summary>
    /// firesTouchEvents
    /// </summary>
    [Description("@#firesTouchEvents")]
    public extern bool FiresTouchEvents { get; }

    /// <summary>
    /// pointerMovementScrolls
    /// </summary>
    [Description("@#pointerMovementScrolls")]
    public extern bool PointerMovementScrolls { get; }
}

/// <summary>
/// UIEvent
/// </summary>
[ECMAScript]
[Description("@#UIEvent")]
public partial class UIEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// sourceCapabilities
    /// </summary>
    [Description("@#sourceCapabilities")]
    public extern InputDeviceCapabilities? SourceCapabilities { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern UIEvent(string type, UIEventInit eventInitDict);

    /// <summary>
    /// view
    /// </summary>
    [Description("@#view")]
    public extern Window? View { get; }

    /// <summary>
    /// detail
    /// </summary>
    [Description("@#detail")]
    public extern int Detail { get; }

    /// <summary>
    /// initUIEvent
    /// </summary>
    /// <param name="typeArg">typeArg</param>
    /// <param name="bubblesArg">bubblesArg</param>
    /// <param name="cancelableArg">cancelableArg</param>
    /// <param name="viewArg">viewArg</param>
    /// <param name="detailArg">detailArg</param>
    [Description("@#initUIEvent")]
    public extern void InitUIEvent(string typeArg, bool bubblesArg = false, bool cancelableArg = false, Window? viewArg = null, int detailArg = 0);

    /// <summary>
    /// which
    /// </summary>
    [Description("@#which")]
    public extern uint Which { get; }
}