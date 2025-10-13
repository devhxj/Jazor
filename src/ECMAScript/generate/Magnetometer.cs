namespace ECMAScript;

/// <summary>
/// MagnetometerSensorOptions
/// </summary>
[ECMAScript]
[Description("@#MagnetometerSensorOptions")]
public record MagnetometerSensorOptions(
    [property: Description("@#referenceFrame")]MagnetometerLocalCoordinateSystem ReferenceFrame = MagnetometerLocalCoordinateSystem.Device): SensorOptions;

/// <summary>
/// Magnetometer
/// </summary>
[ECMAScript]
[Description("@#Magnetometer")]
public class Magnetometer : Sensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sensorOptions">sensorOptions</param>
    public extern Magnetometer(MagnetometerSensorOptions sensorOptions);

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
/// UncalibratedMagnetometer
/// </summary>
[ECMAScript]
[Description("@#UncalibratedMagnetometer")]
public class UncalibratedMagnetometer : Sensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sensorOptions">sensorOptions</param>
    public extern UncalibratedMagnetometer(MagnetometerSensorOptions sensorOptions);

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

    /// <summary>
    /// xBias
    /// </summary>
    [Description("@#xBias")]
    public extern double? XBias { get; }

    /// <summary>
    /// yBias
    /// </summary>
    [Description("@#yBias")]
    public extern double? YBias { get; }

    /// <summary>
    /// zBias
    /// </summary>
    [Description("@#zBias")]
    public extern double? ZBias { get; }
}