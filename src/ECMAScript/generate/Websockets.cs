namespace ECMAScript;

/// <summary>
/// CloseEventInit
/// </summary>
[ECMAScript]
[Description("@#CloseEventInit")]
public record CloseEventInit(
    [property: Description("@#wasClean")]bool WasClean = false,
	[property: Description("@#code")]ushort Code = 0,
	[property: Description("@#reason")]string? Reason = default): EventInit;

/// <summary>
/// WebSocket
/// </summary>
[ECMAScript]
[Description("@#WebSocket")]
public class WebSocket : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="protocols">protocols</param>
    public extern WebSocket(string url, Either<string, string[]> protocols);

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// CONNECTING
    /// </summary>
    [Description("@#CONNECTING")]
    public const ushort CONNECTING = 0;

    /// <summary>
    /// OPEN
    /// </summary>
    [Description("@#OPEN")]
    public const ushort OPEN = 1;

    /// <summary>
    /// CLOSING
    /// </summary>
    [Description("@#CLOSING")]
    public const ushort CLOSING = 2;

    /// <summary>
    /// CLOSED
    /// </summary>
    [Description("@#CLOSED")]
    public const ushort CLOSED = 3;

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern ushort ReadyState { get; }

    /// <summary>
    /// bufferedAmount
    /// </summary>
    [Description("@#bufferedAmount")]
    public extern ulong BufferedAmount { get; }

    /// <summary>
    /// onopen
    /// </summary>
    [Description("@#onopen")]
    public extern EventHandler Onopen { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    /// <summary>
    /// extensions
    /// </summary>
    [Description("@#extensions")]
    public extern string Extensions { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; }

    /// <summary>
    /// close
    /// </summary>
    /// <param name="code">code</param>
    /// <param name="reason">reason</param>
    [Description("@#close")]
    public extern void Close(ushort code, string reason);

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// binaryType
    /// </summary>
    [Description("@#binaryType")]
    public extern BinaryType BinaryType { get; set; }

    /// <summary>
    /// send
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#send")]
    public extern void Send(Either<IBufferSource, Blob, string> data);

    /// <summary>
    /// send
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#send")]
    public extern void Send(IBufferSource data);

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
    public extern void Send(string data);
}

/// <summary>
/// CloseEvent
/// </summary>
[ECMAScript]
[Description("@#CloseEvent")]
public class CloseEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern CloseEvent(string type, CloseEventInit eventInitDict);

    /// <summary>
    /// wasClean
    /// </summary>
    [Description("@#wasClean")]
    public extern bool WasClean { get; }

    /// <summary>
    /// code
    /// </summary>
    [Description("@#code")]
    public extern ushort Code { get; }

    /// <summary>
    /// reason
    /// </summary>
    [Description("@#reason")]
    public extern string Reason { get; }
}