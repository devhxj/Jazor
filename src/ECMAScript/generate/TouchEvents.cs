namespace ECMAScript;

/// <summary>
/// TouchInit
/// </summary>
[ECMAScript]
[Description("@#TouchInit")]
public record TouchInit(
    [property: Description("@#identifier")]int Identifier = default,
	[property: Description("@#target")]EventTarget? Target = default,
	[property: Description("@#clientX")]double ClientX = 0d,
	[property: Description("@#clientY")]double ClientY = 0d,
	[property: Description("@#screenX")]double ScreenX = 0d,
	[property: Description("@#screenY")]double ScreenY = 0d,
	[property: Description("@#pageX")]double PageX = 0d,
	[property: Description("@#pageY")]double PageY = 0d,
	[property: Description("@#radiusX")]float RadiusX = 0f,
	[property: Description("@#radiusY")]float RadiusY = 0f,
	[property: Description("@#rotationAngle")]float RotationAngle = 0f,
	[property: Description("@#force")]float Force = 0f,
	[property: Description("@#altitudeAngle")]double AltitudeAngle = 0d,
	[property: Description("@#azimuthAngle")]double AzimuthAngle = 0d,
	[property: Description("@#touchType")]TouchType TouchType = TouchType.Direct);

/// <summary>
/// TouchEventInit
/// </summary>
[ECMAScript]
[Description("@#TouchEventInit")]
public record TouchEventInit(
    [property: Description("@#touches")]Touch[]? Touches = default,
	[property: Description("@#targetTouches")]Touch[]? TargetTouches = default,
	[property: Description("@#changedTouches")]Touch[]? ChangedTouches = default): EventModifierInit;

/// <summary>
/// Touch
/// </summary>
[ECMAScript]
[Description("@#Touch")]
public class Touch
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="touchInitDict">touchInitDict</param>
    public extern Touch(TouchInit touchInitDict);

    /// <summary>
    /// identifier
    /// </summary>
    [Description("@#identifier")]
    public extern int Identifier { get; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern EventTarget Target { get; }

    /// <summary>
    /// screenX
    /// </summary>
    [Description("@#screenX")]
    public extern double ScreenX { get; }

    /// <summary>
    /// screenY
    /// </summary>
    [Description("@#screenY")]
    public extern double ScreenY { get; }

    /// <summary>
    /// clientX
    /// </summary>
    [Description("@#clientX")]
    public extern double ClientX { get; }

    /// <summary>
    /// clientY
    /// </summary>
    [Description("@#clientY")]
    public extern double ClientY { get; }

    /// <summary>
    /// pageX
    /// </summary>
    [Description("@#pageX")]
    public extern double PageX { get; }

    /// <summary>
    /// pageY
    /// </summary>
    [Description("@#pageY")]
    public extern double PageY { get; }

    /// <summary>
    /// radiusX
    /// </summary>
    [Description("@#radiusX")]
    public extern float RadiusX { get; }

    /// <summary>
    /// radiusY
    /// </summary>
    [Description("@#radiusY")]
    public extern float RadiusY { get; }

    /// <summary>
    /// rotationAngle
    /// </summary>
    [Description("@#rotationAngle")]
    public extern float RotationAngle { get; }

    /// <summary>
    /// force
    /// </summary>
    [Description("@#force")]
    public extern float Force { get; }

    /// <summary>
    /// altitudeAngle
    /// </summary>
    [Description("@#altitudeAngle")]
    public extern float AltitudeAngle { get; }

    /// <summary>
    /// azimuthAngle
    /// </summary>
    [Description("@#azimuthAngle")]
    public extern float AzimuthAngle { get; }

    /// <summary>
    /// touchType
    /// </summary>
    [Description("@#touchType")]
    public extern TouchType TouchType { get; }
}

/// <summary>
/// TouchList
/// </summary>
[ECMAScript]
[Description("@#TouchList")]
public class TouchList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern Touch? GetItem(uint index);
}

/// <summary>
/// TouchEvent
/// </summary>
[ECMAScript]
[Description("@#TouchEvent")]
public class TouchEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern TouchEvent(string type, TouchEventInit eventInitDict);

    /// <summary>
    /// touches
    /// </summary>
    [Description("@#touches")]
    public extern TouchList Touches { get; }

    /// <summary>
    /// targetTouches
    /// </summary>
    [Description("@#targetTouches")]
    public extern TouchList TargetTouches { get; }

    /// <summary>
    /// changedTouches
    /// </summary>
    [Description("@#changedTouches")]
    public extern TouchList ChangedTouches { get; }

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
    /// getModifierState
    /// </summary>
    /// <param name="keyArg">keyArg</param>
    [Description("@#getModifierState")]
    public extern bool GetModifierState(string keyArg);
}