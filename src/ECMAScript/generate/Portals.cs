namespace ECMAScript;

/// <summary>
/// PortalActivateOptions
/// </summary>
[ECMAScript]
[Description("@#PortalActivateOptions")]
public record PortalActivateOptions(
    [property: Description("@#data")]object? Data = default): StructuredSerializeOptions;

/// <summary>
/// PortalActivateEventInit
/// </summary>
[ECMAScript]
[Description("@#PortalActivateEventInit")]
public record PortalActivateEventInit(
    [property: Description("@#data")]object? Data = default): EventInit;

/// <summary>
/// HTMLPortalElement
/// </summary>
[ECMAScript]
[Description("@#HTMLPortalElement")]
public class HTMLPortalElement : HTMLElement
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLPortalElement();

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// activate
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#activate")]
    public extern PromiseResult<object> Activate(PortalActivateOptions? options = default);

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, StructuredSerializeOptions? options = default);

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }
}

/// <summary>
/// PortalHost
/// </summary>
[ECMAScript]
[Description("@#PortalHost")]
public class PortalHost : EventTarget
{
    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, StructuredSerializeOptions? options = default);

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }
}

/// <summary>
/// PortalActivateEvent
/// </summary>
[ECMAScript]
[Description("@#PortalActivateEvent")]
public class PortalActivateEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PortalActivateEvent(string type, PortalActivateEventInit eventInitDict);

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// adoptPredecessor
    /// </summary>
    [Description("@#adoptPredecessor")]
    public extern HTMLPortalElement AdoptPredecessor();
}