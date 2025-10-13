namespace ECMAScript;

/// <summary>
/// GyroscopeSensorOptions
/// </summary>
[ECMAScript]
[Description("@#GyroscopeSensorOptions")]
public record GyroscopeSensorOptions(
    [property: Description("@#referenceFrame")]GyroscopeLocalCoordinateSystem ReferenceFrame = GyroscopeLocalCoordinateSystem.Device): SensorOptions;

/// <summary>
/// Gyroscope
/// </summary>
[ECMAScript]
[Description("@#Gyroscope")]
public class Gyroscope : Sensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sensorOptions">sensorOptions</param>
    public extern Gyroscope(GyroscopeSensorOptions sensorOptions);

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