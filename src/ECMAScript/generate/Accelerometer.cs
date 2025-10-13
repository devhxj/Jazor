namespace ECMAScript;

/// <summary>
/// AccelerometerSensorOptions
/// </summary>
[ECMAScript]
[Description("@#AccelerometerSensorOptions")]
public record AccelerometerSensorOptions(
    [property: Description("@#referenceFrame")]AccelerometerLocalCoordinateSystem ReferenceFrame = AccelerometerLocalCoordinateSystem.Device): SensorOptions;

/// <summary>
/// Accelerometer
/// </summary>
[ECMAScript]
[Description("@#Accelerometer")]
public class Accelerometer : Sensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern Accelerometer(AccelerometerSensorOptions options);

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
/// LinearAccelerationSensor
/// </summary>
[ECMAScript]
[Description("@#LinearAccelerationSensor")]
public class LinearAccelerationSensor : Accelerometer
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern LinearAccelerationSensor(AccelerometerSensorOptions options);
}

/// <summary>
/// GravitySensor
/// </summary>
[ECMAScript]
[Description("@#GravitySensor")]
public class GravitySensor : Accelerometer
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern GravitySensor(AccelerometerSensorOptions options);
}