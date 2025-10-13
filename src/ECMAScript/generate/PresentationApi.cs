namespace ECMAScript;

/// <summary>
/// PresentationConnectionAvailableEventInit
/// </summary>
[ECMAScript]
[Description("@#PresentationConnectionAvailableEventInit")]
public record PresentationConnectionAvailableEventInit(
    [property: Description("@#connection")]PresentationConnection? Connection = default): EventInit;

/// <summary>
/// PresentationConnectionCloseEventInit
/// </summary>
[ECMAScript]
[Description("@#PresentationConnectionCloseEventInit")]
public record PresentationConnectionCloseEventInit(
    [property: Description("@#reason")]PresentationConnectionCloseReason? Reason = default,
	[property: Description("@#message")]string? Message = default): EventInit;

/// <summary>
/// Presentation
/// </summary>
[ECMAScript]
[Description("@#Presentation")]
public partial class Presentation
{
    /// <summary>
    /// defaultRequest
    /// </summary>
    [Description("@#defaultRequest")]
    public extern PresentationRequest? DefaultRequest { get; set; }

    /// <summary>
    /// receiver
    /// </summary>
    [Description("@#receiver")]
    public extern PresentationReceiver? Receiver { get; }
}

/// <summary>
/// PresentationRequest
/// </summary>
[ECMAScript]
[Description("@#PresentationRequest")]
public class PresentationRequest : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="url">url</param>
    public extern PresentationRequest(string url);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="urls">urls</param>
    public extern PresentationRequest(string[] urls);

    /// <summary>
    /// start
    /// </summary>
    [Description("@#start")]
    public extern PromiseResult<PresentationConnection> Start();

    /// <summary>
    /// reconnect
    /// </summary>
    /// <param name="presentationId">presentationId</param>
    [Description("@#reconnect")]
    public extern PromiseResult<PresentationConnection> Reconnect(string presentationId);

    /// <summary>
    /// getAvailability
    /// </summary>
    [Description("@#getAvailability")]
    public extern PromiseResult<PresentationAvailability> GetAvailability();

    /// <summary>
    /// onconnectionavailable
    /// </summary>
    [Description("@#onconnectionavailable")]
    public extern EventHandler Onconnectionavailable { get; set; }
}

/// <summary>
/// PresentationAvailability
/// </summary>
[ECMAScript]
[Description("@#PresentationAvailability")]
public class PresentationAvailability : EventTarget
{
    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern bool Value { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}

/// <summary>
/// PresentationConnectionAvailableEvent
/// </summary>
[ECMAScript]
[Description("@#PresentationConnectionAvailableEvent")]
public class PresentationConnectionAvailableEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PresentationConnectionAvailableEvent(string type, PresentationConnectionAvailableEventInit eventInitDict);

    /// <summary>
    /// connection
    /// </summary>
    [Description("@#connection")]
    public extern PresentationConnection Connection { get; }
}

/// <summary>
/// PresentationConnection
/// </summary>
[ECMAScript]
[Description("@#PresentationConnection")]
public class PresentationConnection : EventTarget
{
    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern PresentationConnectionState State { get; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// terminate
    /// </summary>
    [Description("@#terminate")]
    public extern void Terminate();

    /// <summary>
    /// onconnect
    /// </summary>
    [Description("@#onconnect")]
    public extern EventHandler Onconnect { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    /// <summary>
    /// onterminate
    /// </summary>
    [Description("@#onterminate")]
    public extern EventHandler Onterminate { get; set; }

    /// <summary>
    /// binaryType
    /// </summary>
    [Description("@#binaryType")]
    public extern BinaryType BinaryType { get; set; }

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// send
    /// </summary>
    /// <param name="message">message</param>
    [Description("@#send")]
    public extern void Send(string message);

    /// <summary>
    /// send
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#send")]
    public extern void Send(Blob data);

    /// <summary>
    /// send
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#send")]
    public extern void Send(ArrayBuffer data);

    /// <summary>
    /// send
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#send")]
    public extern void Send(IArrayBufferView data);
}

/// <summary>
/// PresentationConnectionCloseEvent
/// </summary>
[ECMAScript]
[Description("@#PresentationConnectionCloseEvent")]
public class PresentationConnectionCloseEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern PresentationConnectionCloseEvent(string type, PresentationConnectionCloseEventInit eventInitDict);

    /// <summary>
    /// reason
    /// </summary>
    [Description("@#reason")]
    public extern PresentationConnectionCloseReason Reason { get; }

    /// <summary>
    /// message
    /// </summary>
    [Description("@#message")]
    public extern string Message { get; }
}

/// <summary>
/// PresentationReceiver
/// </summary>
[ECMAScript]
[Description("@#PresentationReceiver")]
public class PresentationReceiver
{
    /// <summary>
    /// connectionList
    /// </summary>
    [Description("@#connectionList")]
    public extern PromiseResult<PresentationConnectionList> ConnectionList { get; }
}

/// <summary>
/// PresentationConnectionList
/// </summary>
[ECMAScript]
[Description("@#PresentationConnectionList")]
public class PresentationConnectionList : EventTarget
{
    /// <summary>
    /// connections
    /// </summary>
    [Description("@#connections")]
    public extern FrozenSet<PresentationConnection> Connections { get; }

    /// <summary>
    /// onconnectionavailable
    /// </summary>
    [Description("@#onconnectionavailable")]
    public extern EventHandler Onconnectionavailable { get; set; }
}