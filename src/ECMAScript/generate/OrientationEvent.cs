namespace ECMAScript;

/// <summary>
/// DeviceOrientationEventInit
/// </summary>
[ECMAScript]
[Description("@#DeviceOrientationEventInit")]
public record DeviceOrientationEventInit(
    [property: Description("@#alpha")]double? Alpha = null,
	[property: Description("@#beta")]double? Beta = null,
	[property: Description("@#gamma")]double? Gamma = null,
	[property: Description("@#absolute")]bool Absolute = false): EventInit;

/// <summary>
/// DeviceMotionEventAccelerationInit
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventAccelerationInit")]
public record DeviceMotionEventAccelerationInit(
    [property: Description("@#x")]double? X = null,
	[property: Description("@#y")]double? Y = null,
	[property: Description("@#z")]double? Z = null);

/// <summary>
/// DeviceMotionEventRotationRateInit
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventRotationRateInit")]
public record DeviceMotionEventRotationRateInit(
    [property: Description("@#alpha")]double? Alpha = null,
	[property: Description("@#beta")]double? Beta = null,
	[property: Description("@#gamma")]double? Gamma = null);

/// <summary>
/// DeviceMotionEventInit
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventInit")]
public record DeviceMotionEventInit(
    [property: Description("@#acceleration")]DeviceMotionEventAccelerationInit? Acceleration = default,
	[property: Description("@#accelerationIncludingGravity")]DeviceMotionEventAccelerationInit? AccelerationIncludingGravity = default,
	[property: Description("@#rotationRate")]DeviceMotionEventRotationRateInit? RotationRate = default,
	[property: Description("@#interval")]double Interval = 0d): EventInit;

/// <summary>
/// DeviceOrientationEvent
/// </summary>
[ECMAScript]
[Description("@#DeviceOrientationEvent")]
public class DeviceOrientationEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern DeviceOrientationEvent(string type, DeviceOrientationEventInit eventInitDict);

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern double? Alpha { get; }

    /// <summary>
    /// beta
    /// </summary>
    [Description("@#beta")]
    public extern double? Beta { get; }

    /// <summary>
    /// gamma
    /// </summary>
    [Description("@#gamma")]
    public extern double? Gamma { get; }

    /// <summary>
    /// absolute
    /// </summary>
    [Description("@#absolute")]
    public extern bool Absolute { get; }

    /// <summary>
    /// requestPermission
    /// </summary>
    /// <param name="absolute">absolute</param>
    [Description("@#requestPermission")]
    public extern static PromiseResult<PermissionState> RequestPermission(bool absolute = false);
}

/// <summary>
/// DeviceMotionEventAcceleration
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventAcceleration")]
public class DeviceMotionEventAcceleration
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern double? X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern double? Y { get; }

    /// <summary>
    /// z
    /// </summary>
    [Description("@#z")]
    public extern double? Z { get; }
}

/// <summary>
/// DeviceMotionEventRotationRate
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEventRotationRate")]
public class DeviceMotionEventRotationRate
{
    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern double? Alpha { get; }

    /// <summary>
    /// beta
    /// </summary>
    [Description("@#beta")]
    public extern double? Beta { get; }

    /// <summary>
    /// gamma
    /// </summary>
    [Description("@#gamma")]
    public extern double? Gamma { get; }
}

/// <summary>
/// DeviceMotionEvent
/// </summary>
[ECMAScript]
[Description("@#DeviceMotionEvent")]
public class DeviceMotionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern DeviceMotionEvent(string type, DeviceMotionEventInit eventInitDict);

    /// <summary>
    /// acceleration
    /// </summary>
    [Description("@#acceleration")]
    public extern DeviceMotionEventAcceleration? Acceleration { get; }

    /// <summary>
    /// accelerationIncludingGravity
    /// </summary>
    [Description("@#accelerationIncludingGravity")]
    public extern DeviceMotionEventAcceleration? AccelerationIncludingGravity { get; }

    /// <summary>
    /// rotationRate
    /// </summary>
    [Description("@#rotationRate")]
    public extern DeviceMotionEventRotationRate? RotationRate { get; }

    /// <summary>
    /// interval
    /// </summary>
    [Description("@#interval")]
    public extern double Interval { get; }

    /// <summary>
    /// requestPermission
    /// </summary>
    [Description("@#requestPermission")]
    public extern static PromiseResult<PermissionState> RequestPermission();
}