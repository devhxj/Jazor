namespace ECMAScript;

/// <summary>
/// PointerEventInit
/// </summary>
[ECMAScript]
[Description("@#PointerEventInit")]
public record PointerEventInit(
    [property: Description("@#pointerId")]int PointerId = 0,
	[property: Description("@#width")]double Width = 1d,
	[property: Description("@#height")]double Height = 1d,
	[property: Description("@#pressure")]float Pressure = 0f,
	[property: Description("@#tangentialPressure")]float TangentialPressure = 0f,
	[property: Description("@#tiltX")]int TiltX = default,
	[property: Description("@#tiltY")]int TiltY = default,
	[property: Description("@#twist")]int Twist = 0,
	[property: Description("@#altitudeAngle")]double AltitudeAngle = default,
	[property: Description("@#azimuthAngle")]double AzimuthAngle = default,
	[property: Description("@#pointerType")]string? PointerType = default,
	[property: Description("@#isPrimary")]bool IsPrimary = false,
	[property: Description("@#persistentDeviceId")]int PersistentDeviceId = 0,
	[property: Description("@#coalescedEvents")]PointerEvent[]? CoalescedEvents = default,
	[property: Description("@#predictedEvents")]PointerEvent[]? PredictedEvents = default): MouseEventInit;

/// <summary>
/// PointerEvent
/// </summary>
[ECMAScript]
[Description("@#PointerEvent")]
public class PointerEvent(string type, MouseEventInit eventInitDict) : MouseEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PointerEvent(string type, PointerEventInit eventInitDict);

    /// <summary>
    /// pointerId
    /// </summary>
    [Description("@#pointerId")]
    public extern int PointerId { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern double Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern double Height { get; }

    /// <summary>
    /// pressure
    /// </summary>
    [Description("@#pressure")]
    public extern float Pressure { get; }

    /// <summary>
    /// tangentialPressure
    /// </summary>
    [Description("@#tangentialPressure")]
    public extern float TangentialPressure { get; }

    /// <summary>
    /// tiltX
    /// </summary>
    [Description("@#tiltX")]
    public extern int TiltX { get; }

    /// <summary>
    /// tiltY
    /// </summary>
    [Description("@#tiltY")]
    public extern int TiltY { get; }

    /// <summary>
    /// twist
    /// </summary>
    [Description("@#twist")]
    public extern int Twist { get; }

    /// <summary>
    /// altitudeAngle
    /// </summary>
    [Description("@#altitudeAngle")]
    public extern double AltitudeAngle { get; }

    /// <summary>
    /// azimuthAngle
    /// </summary>
    [Description("@#azimuthAngle")]
    public extern double AzimuthAngle { get; }

    /// <summary>
    /// pointerType
    /// </summary>
    [Description("@#pointerType")]
    public extern string PointerType { get; }

    /// <summary>
    /// isPrimary
    /// </summary>
    [Description("@#isPrimary")]
    public extern bool IsPrimary { get; }

    /// <summary>
    /// persistentDeviceId
    /// </summary>
    [Description("@#persistentDeviceId")]
    public extern int PersistentDeviceId { get; }

    /// <summary>
    /// getCoalescedEvents
    /// </summary>
    [Description("@#getCoalescedEvents")]
    public extern PointerEvent[] GetCoalescedEvents();

    /// <summary>
    /// getPredictedEvents
    /// </summary>
    [Description("@#getPredictedEvents")]
    public extern PointerEvent[] GetPredictedEvents();
}