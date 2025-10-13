namespace ECMAScript;

/// <summary>
/// FenceEvent
/// </summary>
[ECMAScript]
[Description("@#FenceEvent")]
public record FenceEvent(
    [property: Description("@#eventType")]string? EventType = default,
	[property: Description("@#eventData")]string? EventData = default,
	[property: Description("@#destination")]FenceReportingDestination[]? Destination = default,
	[property: Description("@#crossOriginExposed")]bool CrossOriginExposed = false,
	[property: Description("@#once")]bool Once = false,
	[property: Description("@#destinationURL")]string? DestinationURL = default);

/// <summary>
/// HTMLFencedFrameElement
/// </summary>
[ECMAScript]
[Description("@#HTMLFencedFrameElement")]
public class HTMLFencedFrameElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLFencedFrameElement();

    /// <summary>
    /// config
    /// </summary>
    [Description("@#config")]
    public extern FencedFrameConfig? Config { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern string Height { get; set; }

    /// <summary>
    /// sandbox
    /// </summary>
    [Description("@#sandbox")]
    public extern List<string> Sandbox { get; }

    /// <summary>
    /// allow
    /// </summary>
    [Description("@#allow")]
    public extern string Allow { get; set; }
}

/// <summary>
/// FencedFrameConfig
/// </summary>
[ECMAScript]
[Description("@#FencedFrameConfig")]
public class FencedFrameConfig
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="url">url</param>
    public extern FencedFrameConfig(string url);

    /// <summary>
    /// setSharedStorageContext
    /// </summary>
    /// <param name="contextString">contextString</param>
    [Description("@#setSharedStorageContext")]
    public extern void SetSharedStorageContext(string contextString);
}

/// <summary>
/// Fence
/// </summary>
[ECMAScript]
[Description("@#Fence")]
public class Fence
{
    /// <summary>
    /// reportEvent
    /// </summary>
    /// <param name="event">event</param>
    [Description("@#reportEvent")]
    public extern void ReportEvent(ReportEventType? @event = default);

    /// <summary>
    /// setReportEventDataForAutomaticBeacons
    /// </summary>
    /// <param name="event">event</param>
    [Description("@#setReportEventDataForAutomaticBeacons")]
    public extern void SetReportEventDataForAutomaticBeacons(FenceEvent? @event = default);

    /// <summary>
    /// getNestedConfigs
    /// </summary>
    [Description("@#getNestedConfigs")]
    public extern FencedFrameConfig[] GetNestedConfigs();

    /// <summary>
    /// disableUntrustedNetwork
    /// </summary>
    [Description("@#disableUntrustedNetwork")]
    public extern PromiseResult<object> DisableUntrustedNetwork();

    /// <summary>
    /// notifyEvent
    /// </summary>
    /// <param name="event">event</param>
    [Description("@#notifyEvent")]
    public extern void NotifyEvent(Event @event);
}