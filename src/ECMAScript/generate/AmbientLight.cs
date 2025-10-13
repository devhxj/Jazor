namespace ECMAScript;

/// <summary>
/// AmbientLightSensor
/// </summary>
[ECMAScript]
[Description("@#AmbientLightSensor")]
public class AmbientLightSensor : Sensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sensorOptions">sensorOptions</param>
    public extern AmbientLightSensor(SensorOptions sensorOptions);

    /// <summary>
    /// illuminance
    /// </summary>
    [Description("@#illuminance")]
    public extern double? Illuminance { get; }
}