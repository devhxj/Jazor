namespace ECMAScript;

/// <summary>
/// ProximitySensor
/// </summary>
[ECMAScript]
[Description("@#ProximitySensor")]
public class ProximitySensor : Sensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sensorOptions">sensorOptions</param>
    public extern ProximitySensor(SensorOptions sensorOptions);

    /// <summary>
    /// distance
    /// </summary>
    [Description("@#distance")]
    public extern double? Distance { get; }

    /// <summary>
    /// max
    /// </summary>
    [Description("@#max")]
    public extern double? Max { get; }

    /// <summary>
    /// near
    /// </summary>
    [Description("@#near")]
    public extern bool? Near { get; }
}