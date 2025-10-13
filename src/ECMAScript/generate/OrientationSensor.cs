namespace ECMAScript;

/// <summary>
/// OrientationSensorOptions
/// </summary>
[ECMAScript]
[Description("@#OrientationSensorOptions")]
public record OrientationSensorOptions(
    [property: Description("@#referenceFrame")]OrientationSensorLocalCoordinateSystem ReferenceFrame = OrientationSensorLocalCoordinateSystem.Device): SensorOptions;

/// <summary>
/// OrientationSensor
/// </summary>
[ECMAScript]
[Description("@#OrientationSensor")]
public class OrientationSensor : Sensor
{
    /// <summary>
    /// quaternion
    /// </summary>
    [Description("@#quaternion")]
    public extern FrozenSet<double>? Quaternion { get; }

    /// <summary>
    /// populateMatrix
    /// </summary>
    /// <param name="targetMatrix">targetMatrix</param>
    [Description("@#populateMatrix")]
    public extern void PopulateMatrix(RotationMatrixType targetMatrix);
}

/// <summary>
/// AbsoluteOrientationSensor
/// </summary>
[ECMAScript]
[Description("@#AbsoluteOrientationSensor")]
public class AbsoluteOrientationSensor : OrientationSensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sensorOptions">sensorOptions</param>
    public extern AbsoluteOrientationSensor(OrientationSensorOptions sensorOptions);
}

/// <summary>
/// RelativeOrientationSensor
/// </summary>
[ECMAScript]
[Description("@#RelativeOrientationSensor")]
public class RelativeOrientationSensor : OrientationSensor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="sensorOptions">sensorOptions</param>
    public extern RelativeOrientationSensor(OrientationSensorOptions sensorOptions);
}