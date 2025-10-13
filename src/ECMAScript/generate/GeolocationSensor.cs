namespace ECMAScript;

/// <summary>
/// GeolocationSensorOptions
/// </summary>
[ECMAScript]
[Description("@#GeolocationSensorOptions")]
public abstract record GeolocationSensorOptions();

/// <summary>
/// ReadOptions
/// </summary>
[ECMAScript]
[Description("@#ReadOptions")]
public record ReadOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default): GeolocationSensorOptions;

/// <summary>
/// GeolocationSensorReading
/// </summary>
[ECMAScript]
[Description("@#GeolocationSensorReading")]
public record GeolocationSensorReading(
    [property: Description("@#timestamp")]double Timestamp = default,
	[property: Description("@#latitude")]double Latitude = default,
	[property: Description("@#longitude")]double Longitude = default,
	[property: Description("@#altitude")]double Altitude = default,
	[property: Description("@#accuracy")]double Accuracy = default,
	[property: Description("@#altitudeAccuracy")]double AltitudeAccuracy = default,
	[property: Description("@#heading")]double Heading = default,
	[property: Description("@#speed")]double Speed = default);

/// <summary>
/// GeolocationSensor
/// </summary>
[ECMAScript]
[Description("@#GeolocationSensor")]
public class GeolocationSensor : Sensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern GeolocationSensor(GeolocationSensorOptions options);

    /// <summary>
    /// read
    /// </summary>
    /// <param name="readOptions">readOptions</param>
    [Description("@#read")]
    public extern static PromiseResult<GeolocationSensorReading> Read(ReadOptions? readOptions = default);

    /// <summary>
    /// latitude
    /// </summary>
    [Description("@#latitude")]
    public extern double? Latitude { get; }

    /// <summary>
    /// longitude
    /// </summary>
    [Description("@#longitude")]
    public extern double? Longitude { get; }

    /// <summary>
    /// altitude
    /// </summary>
    [Description("@#altitude")]
    public extern double? Altitude { get; }

    /// <summary>
    /// accuracy
    /// </summary>
    [Description("@#accuracy")]
    public extern double? Accuracy { get; }

    /// <summary>
    /// altitudeAccuracy
    /// </summary>
    [Description("@#altitudeAccuracy")]
    public extern double? AltitudeAccuracy { get; }

    /// <summary>
    /// heading
    /// </summary>
    [Description("@#heading")]
    public extern double? Heading { get; }

    /// <summary>
    /// speed
    /// </summary>
    [Description("@#speed")]
    public extern double? Speed { get; }
}