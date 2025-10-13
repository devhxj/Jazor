namespace ECMAScript;

/// <summary>
/// FocusEventInit
/// </summary>
[ECMAScript]
[Description("@#FocusEventInit")]
public record FocusEventInit(
    [property: Description("@#relatedTarget")]EventTarget? RelatedTarget = null): UIEventInit;

/// <summary>
/// EventModifierInit
/// </summary>
[ECMAScript]
[Description("@#EventModifierInit")]
public record EventModifierInit(
    [property: Description("@#ctrlKey")]bool CtrlKey = false,
	[property: Description("@#shiftKey")]bool ShiftKey = false,
	[property: Description("@#altKey")]bool AltKey = false,
	[property: Description("@#metaKey")]bool MetaKey = false,
	[property: Description("@#modifierAltGraph")]bool ModifierAltGraph = false,
	[property: Description("@#modifierCapsLock")]bool ModifierCapsLock = false,
	[property: Description("@#modifierFn")]bool ModifierFn = false,
	[property: Description("@#modifierFnLock")]bool ModifierFnLock = false,
	[property: Description("@#modifierHyper")]bool ModifierHyper = false,
	[property: Description("@#modifierNumLock")]bool ModifierNumLock = false,
	[property: Description("@#modifierScrollLock")]bool ModifierScrollLock = false,
	[property: Description("@#modifierSuper")]bool ModifierSuper = false,
	[property: Description("@#modifierSymbol")]bool ModifierSymbol = false,
	[property: Description("@#modifierSymbolLock")]bool ModifierSymbolLock = false): UIEventInit;

/// <summary>
/// WheelEventInit
/// </summary>
[ECMAScript]
[Description("@#WheelEventInit")]
public record WheelEventInit(
    [property: Description("@#deltaX")]double DeltaX = 0.0d,
	[property: Description("@#deltaY")]double DeltaY = 0.0d,
	[property: Description("@#deltaZ")]double DeltaZ = 0.0d,
	[property: Description("@#deltaMode")]uint DeltaMode = 0): MouseEventInit;

/// <summary>
/// KeyboardEventInit
/// </summary>
[ECMAScript]
[Description("@#KeyboardEventInit")]
public record KeyboardEventInit(
    [property: Description("@#key")]string? Key = default,
	[property: Description("@#code")]string? Code = default,
	[property: Description("@#location")]uint Location = 0,
	[property: Description("@#repeat")]bool Repeat = false,
	[property: Description("@#isComposing")]bool IsComposing = false,
	[property: Description("@#charCode")]uint CharCode = 0,
	[property: Description("@#keyCode")]uint KeyCode = 0): EventModifierInit
{
    [Category("optional")]
    public extern static KeyboardEventInit OptionalKeyCodeLocation5(
        [Description("@#key")]string? key = default,
        [Description("@#code")]string? code = default,
        [Description("@#location")]uint location = 0,
        [Description("@#repeat")]bool repeat = false,
        [Description("@#isComposing")]bool isComposing = false);

    [Category("optional")]
    public extern static KeyboardEventInit OptionalCharCodeKeyCode(
        [Description("@#charCode")]uint charCode = 0,
        [Description("@#keyCode")]uint keyCode = 0);
}

/// <summary>
/// CompositionEventInit
/// </summary>
[ECMAScript]
[Description("@#CompositionEventInit")]
public record CompositionEventInit(
    [property: Description("@#data")]string? Data = default): UIEventInit;

/// <summary>
/// FocusEvent
/// </summary>
[ECMAScript]
[Description("@#FocusEvent")]
public class FocusEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern FocusEvent(string type, FocusEventInit eventInitDict);

    /// <summary>
    /// relatedTarget
    /// </summary>
    [Description("@#relatedTarget")]
    public extern EventTarget? RelatedTarget { get; }
}

/// <summary>
/// WheelEvent
/// </summary>
[ECMAScript]
[Description("@#WheelEvent")]
public class WheelEvent(string type, MouseEventInit eventInitDict) : MouseEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern WheelEvent(string type, WheelEventInit eventInitDict);

    /// <summary>
    /// DOM_DELTA_PIXEL
    /// </summary>
    [Description("@#DOM_DELTA_PIXEL")]
    public const uint DOM_DELTA_PIXEL = 0x00;

    /// <summary>
    /// DOM_DELTA_LINE
    /// </summary>
    [Description("@#DOM_DELTA_LINE")]
    public const uint DOM_DELTA_LINE = 0x01;

    /// <summary>
    /// DOM_DELTA_PAGE
    /// </summary>
    [Description("@#DOM_DELTA_PAGE")]
    public const uint DOM_DELTA_PAGE = 0x02;

    /// <summary>
    /// deltaX
    /// </summary>
    [Description("@#deltaX")]
    public extern double DeltaX { get; }

    /// <summary>
    /// deltaY
    /// </summary>
    [Description("@#deltaY")]
    public extern double DeltaY { get; }

    /// <summary>
    /// deltaZ
    /// </summary>
    [Description("@#deltaZ")]
    public extern double DeltaZ { get; }

    /// <summary>
    /// deltaMode
    /// </summary>
    [Description("@#deltaMode")]
    public extern uint DeltaMode { get; }
}

/// <summary>
/// KeyboardEvent
/// </summary>
[ECMAScript]
[Description("@#KeyboardEvent")]
public partial class KeyboardEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern KeyboardEvent(string type, KeyboardEventInit eventInitDict);

    /// <summary>
    /// DOM_KEY_LOCATION_STANDARD
    /// </summary>
    [Description("@#DOM_KEY_LOCATION_STANDARD")]
    public const uint DOM_KEY_LOCATION_STANDARD = 0x00;

    /// <summary>
    /// DOM_KEY_LOCATION_LEFT
    /// </summary>
    [Description("@#DOM_KEY_LOCATION_LEFT")]
    public const uint DOM_KEY_LOCATION_LEFT = 0x01;

    /// <summary>
    /// DOM_KEY_LOCATION_RIGHT
    /// </summary>
    [Description("@#DOM_KEY_LOCATION_RIGHT")]
    public const uint DOM_KEY_LOCATION_RIGHT = 0x02;

    /// <summary>
    /// DOM_KEY_LOCATION_NUMPAD
    /// </summary>
    [Description("@#DOM_KEY_LOCATION_NUMPAD")]
    public const uint DOM_KEY_LOCATION_NUMPAD = 0x03;

    /// <summary>
    /// key
    /// </summary>
    [Description("@#key")]
    public extern string Key { get; }

    /// <summary>
    /// code
    /// </summary>
    [Description("@#code")]
    public extern string Code { get; }

    /// <summary>
    /// location
    /// </summary>
    [Description("@#location")]
    public extern uint Location { get; }

    /// <summary>
    /// ctrlKey
    /// </summary>
    [Description("@#ctrlKey")]
    public extern bool CtrlKey { get; }

    /// <summary>
    /// shiftKey
    /// </summary>
    [Description("@#shiftKey")]
    public extern bool ShiftKey { get; }

    /// <summary>
    /// altKey
    /// </summary>
    [Description("@#altKey")]
    public extern bool AltKey { get; }

    /// <summary>
    /// metaKey
    /// </summary>
    [Description("@#metaKey")]
    public extern bool MetaKey { get; }

    /// <summary>
    /// repeat
    /// </summary>
    [Description("@#repeat")]
    public extern bool Repeat { get; }

    /// <summary>
    /// isComposing
    /// </summary>
    [Description("@#isComposing")]
    public extern bool IsComposing { get; }

    /// <summary>
    /// getModifierState
    /// </summary>
    /// <param name="keyArg">keyArg</param>
    [Description("@#getModifierState")]
    public extern bool GetModifierState(string keyArg);

    /// <summary>
    /// initKeyboardEvent
    /// </summary>
    /// <param name="typeArg">typeArg</param>
    /// <param name="bubblesArg">bubblesArg</param>
    /// <param name="cancelableArg">cancelableArg</param>
    /// <param name="viewArg">viewArg</param>
    /// <param name="keyArg">keyArg</param>
    /// <param name="locationArg">locationArg</param>
    /// <param name="ctrlKey">ctrlKey</param>
    /// <param name="altKey">altKey</param>
    /// <param name="shiftKey">shiftKey</param>
    /// <param name="metaKey">metaKey</param>
    [Description("@#initKeyboardEvent")]
    public extern void InitKeyboardEvent(string typeArg, bool bubblesArg = false, bool cancelableArg = false, Window? viewArg = null, string? keyArg = default, uint locationArg = 0, bool ctrlKey = false, bool altKey = false, bool shiftKey = false, bool metaKey = false);

    /// <summary>
    /// charCode
    /// </summary>
    [Description("@#charCode")]
    public extern uint CharCode { get; }

    /// <summary>
    /// keyCode
    /// </summary>
    [Description("@#keyCode")]
    public extern uint KeyCode { get; }
}

/// <summary>
/// CompositionEvent
/// </summary>
[ECMAScript]
[Description("@#CompositionEvent")]
public partial class CompositionEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern CompositionEvent(string type, CompositionEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern string Data { get; }

    /// <summary>
    /// initCompositionEvent
    /// </summary>
    /// <param name="typeArg">typeArg</param>
    /// <param name="bubblesArg">bubblesArg</param>
    /// <param name="cancelableArg">cancelableArg</param>
    /// <param name="viewArg">viewArg</param>
    /// <param name="dataArg">dataArg</param>
    [Description("@#initCompositionEvent")]
    public extern void InitCompositionEvent(string typeArg, bool bubblesArg = false, bool cancelableArg = false, WindowProxy? viewArg = null, string? dataArg = default);
}

/// <summary>
/// TextEvent
/// </summary>
[ECMAScript]
[Description("@#TextEvent")]
public class TextEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern string Data { get; }

    /// <summary>
    /// initTextEvent
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="bubbles">bubbles</param>
    /// <param name="cancelable">cancelable</param>
    /// <param name="view">view</param>
    /// <param name="data">data</param>
    [Description("@#initTextEvent")]
    public extern void InitTextEvent(string type, bool bubbles = false, bool cancelable = false, Window? view = null, string? data = default);
}