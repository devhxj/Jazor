namespace ECMAScript;

/// <summary>
/// SensorOptions
/// </summary>
[ECMAScript]
[Description("@#SensorOptions")]
public record SensorOptions(
    [property: Description("@#frequency")]double Frequency = default);

/// <summary>
/// SensorErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#SensorErrorEventInit")]
public record SensorErrorEventInit(
    [property: Description("@#error")]DOMException? Error = default): EventInit;

/// <summary>
/// Sensor
/// </summary>
[ECMAScript]
[Description("@#Sensor")]
public class Sensor : EventTarget
{
    /// <summary>
    /// activated
    /// </summary>
    [Description("@#activated")]
    public extern bool Activated { get; }

    /// <summary>
    /// hasReading
    /// </summary>
    [Description("@#hasReading")]
    public extern bool HasReading { get; }

    /// <summary>
    /// timestamp
    /// </summary>
    [Description("@#timestamp")]
    public extern double? Timestamp { get; }

    /// <summary>
    /// start
    /// </summary>
    [Description("@#start")]
    public extern void Start();

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();

    /// <summary>
    /// onreading
    /// </summary>
    [Description("@#onreading")]
    public extern EventHandler Onreading { get; set; }

    /// <summary>
    /// onactivate
    /// </summary>
    [Description("@#onactivate")]
    public extern EventHandler Onactivate { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }
}

/// <summary>
/// SensorErrorEvent
/// </summary>
[ECMAScript]
[Description("@#SensorErrorEvent")]
public class SensorErrorEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="errorEventInitDict">errorEventInitDict</param>
    public extern SensorErrorEvent(string type, SensorErrorEventInit errorEventInitDict);

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern DOMException Error { get; }
}