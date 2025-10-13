namespace ECMAScript;

/// <summary>
/// WebTransportHash
/// </summary>
[ECMAScript]
[Description("@#WebTransportHash")]
public record WebTransportHash(
    [property: Description("@#algorithm")]string? Algorithm = default,
	[property: Description("@#value")]IBufferSource? Value = default);

/// <summary>
/// WebTransportOptions
/// </summary>
[ECMAScript]
[Description("@#WebTransportOptions")]
public record WebTransportOptions(
    [property: Description("@#allowPooling")]bool AllowPooling = false,
	[property: Description("@#requireUnreliable")]bool RequireUnreliable = false,
	[property: Description("@#serverCertificateHashes")]WebTransportHash[]? ServerCertificateHashes = default,
	[property: Description("@#congestionControl")]WebTransportCongestionControl CongestionControl = WebTransportCongestionControl.Default,
	[property: Description("@#anticipatedConcurrentIncomingUnidirectionalStreams")]ushort? AnticipatedConcurrentIncomingUnidirectionalStreams = null,
	[property: Description("@#anticipatedConcurrentIncomingBidirectionalStreams")]ushort? AnticipatedConcurrentIncomingBidirectionalStreams = null,
	[property: Description("@#protocols")]string[]? Protocols = default);

/// <summary>
/// WebTransportCloseInfo
/// </summary>
[ECMAScript]
[Description("@#WebTransportCloseInfo")]
public record WebTransportCloseInfo(
    [property: Description("@#closeCode")]uint CloseCode = 0,
	[property: Description("@#reason")]string? Reason = default);

/// <summary>
/// WebTransportSendOptions
/// </summary>
[ECMAScript]
[Description("@#WebTransportSendOptions")]
public record WebTransportSendOptions(
    [property: Description("@#sendGroup")]WebTransportSendGroup? SendGroup = null,
	[property: Description("@#sendOrder")]long SendOrder = 0);

/// <summary>
/// WebTransportSendStreamOptions
/// </summary>
[ECMAScript]
[Description("@#WebTransportSendStreamOptions")]
public record WebTransportSendStreamOptions(
    [property: Description("@#waitUntilAvailable")]bool WaitUntilAvailable = false): WebTransportSendOptions;

/// <summary>
/// WebTransportConnectionStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportConnectionStats")]
public record WebTransportConnectionStats(
    [property: Description("@#bytesSent")]ulong BytesSent = 0,
	[property: Description("@#packetsSent")]ulong PacketsSent = 0,
	[property: Description("@#bytesLost")]ulong BytesLost = 0,
	[property: Description("@#packetsLost")]ulong PacketsLost = 0,
	[property: Description("@#bytesReceived")]ulong BytesReceived = 0,
	[property: Description("@#packetsReceived")]ulong PacketsReceived = 0,
	[property: Description("@#smoothedRtt")]double SmoothedRtt = default,
	[property: Description("@#rttVariation")]double RttVariation = default,
	[property: Description("@#minRtt")]double MinRtt = default,
	[property: Description("@#datagrams")]WebTransportDatagramStats? Datagrams = default,
	[property: Description("@#estimatedSendRate")]ulong? EstimatedSendRate = null,
	[property: Description("@#atSendCapacity")]bool AtSendCapacity = false);

/// <summary>
/// WebTransportDatagramStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportDatagramStats")]
public record WebTransportDatagramStats(
    [property: Description("@#droppedIncoming")]ulong DroppedIncoming = 0,
	[property: Description("@#expiredIncoming")]ulong ExpiredIncoming = 0,
	[property: Description("@#expiredOutgoing")]ulong ExpiredOutgoing = 0,
	[property: Description("@#lostOutgoing")]ulong LostOutgoing = 0);

/// <summary>
/// WebTransportSendStreamStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportSendStreamStats")]
public record WebTransportSendStreamStats(
    [property: Description("@#bytesWritten")]ulong BytesWritten = 0,
	[property: Description("@#bytesSent")]ulong BytesSent = 0,
	[property: Description("@#bytesAcknowledged")]ulong BytesAcknowledged = 0);

/// <summary>
/// WebTransportReceiveStreamStats
/// </summary>
[ECMAScript]
[Description("@#WebTransportReceiveStreamStats")]
public record WebTransportReceiveStreamStats(
    [property: Description("@#bytesReceived")]ulong BytesReceived = 0,
	[property: Description("@#bytesRead")]ulong BytesRead = 0);

/// <summary>
/// WebTransportErrorOptions
/// </summary>
[ECMAScript]
[Description("@#WebTransportErrorOptions")]
public record WebTransportErrorOptions(
    [property: Description("@#source")]WebTransportErrorSource Source = WebTransportErrorSource.Stream,
	[property: Description("@#streamErrorCode")]uint? StreamErrorCode = null);

/// <summary>
/// WebTransportDatagramsWritable
/// </summary>
[ECMAScript]
[Description("@#WebTransportDatagramsWritable")]
public class WebTransportDatagramsWritable(object underlyingSink, QueuingStrategy strategy) : WritableStream(underlyingSink, strategy)
{
    /// <summary>
    /// sendGroup
    /// </summary>
    [Description("@#sendGroup")]
    public extern WebTransportSendGroup? SendGroup { get; set; }

    /// <summary>
    /// sendOrder
    /// </summary>
    [Description("@#sendOrder")]
    public extern long SendOrder { get; set; }
}

/// <summary>
/// WebTransportDatagramDuplexStream
/// </summary>
[ECMAScript]
[Description("@#WebTransportDatagramDuplexStream")]
public class WebTransportDatagramDuplexStream
{
    /// <summary>
    /// createWritable
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#createWritable")]
    public extern WebTransportDatagramsWritable CreateWritable(WebTransportSendOptions? options = default);

    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern ReadableStream Readable { get; }

    /// <summary>
    /// maxDatagramSize
    /// </summary>
    [Description("@#maxDatagramSize")]
    public extern uint MaxDatagramSize { get; }

    /// <summary>
    /// incomingMaxAge
    /// </summary>
    [Description("@#incomingMaxAge")]
    public extern double? IncomingMaxAge { get; set; }

    /// <summary>
    /// outgoingMaxAge
    /// </summary>
    [Description("@#outgoingMaxAge")]
    public extern double? OutgoingMaxAge { get; set; }

    /// <summary>
    /// incomingHighWaterMark
    /// </summary>
    [Description("@#incomingHighWaterMark")]
    public extern double IncomingHighWaterMark { get; set; }

    /// <summary>
    /// outgoingHighWaterMark
    /// </summary>
    [Description("@#outgoingHighWaterMark")]
    public extern double OutgoingHighWaterMark { get; set; }
}

/// <summary>
/// WebTransport
/// </summary>
[ECMAScript]
[Description("@#WebTransport")]
public class WebTransport
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="options">options</param>
    public extern WebTransport(string url, WebTransportOptions options);

    /// <summary>
    /// getStats
    /// </summary>
    [Description("@#getStats")]
    public extern PromiseResult<WebTransportConnectionStats> GetStats();

    /// <summary>
    /// exportKeyingMaterial
    /// </summary>
    /// <param name="label">label</param>
    /// <param name="context">context</param>
    [Description("@#exportKeyingMaterial")]
    public extern PromiseResult<ArrayBuffer> ExportKeyingMaterial(IBufferSource label, IBufferSource context);

    /// <summary>
    /// ready
    /// </summary>
    [Description("@#ready")]
    public extern PromiseResult<object> Ready { get; }

    /// <summary>
    /// reliability
    /// </summary>
    [Description("@#reliability")]
    public extern WebTransportReliabilityMode Reliability { get; }

    /// <summary>
    /// congestionControl
    /// </summary>
    [Description("@#congestionControl")]
    public extern WebTransportCongestionControl CongestionControl { get; }

    /// <summary>
    /// anticipatedConcurrentIncomingUnidirectionalStreams
    /// </summary>
    [Description("@#anticipatedConcurrentIncomingUnidirectionalStreams")]
    public extern ushort? AnticipatedConcurrentIncomingUnidirectionalStreams { get; set; }

    /// <summary>
    /// anticipatedConcurrentIncomingBidirectionalStreams
    /// </summary>
    [Description("@#anticipatedConcurrentIncomingBidirectionalStreams")]
    public extern ushort? AnticipatedConcurrentIncomingBidirectionalStreams { get; set; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; }

    /// <summary>
    /// closed
    /// </summary>
    [Description("@#closed")]
    public extern PromiseResult<WebTransportCloseInfo> Closed { get; }

    /// <summary>
    /// draining
    /// </summary>
    [Description("@#draining")]
    public extern PromiseResult<object> Draining { get; }

    /// <summary>
    /// close
    /// </summary>
    /// <param name="closeInfo">closeInfo</param>
    [Description("@#close")]
    public extern void Close(WebTransportCloseInfo? closeInfo = default);

    /// <summary>
    /// datagrams
    /// </summary>
    [Description("@#datagrams")]
    public extern WebTransportDatagramDuplexStream Datagrams { get; }

    /// <summary>
    /// createBidirectionalStream
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#createBidirectionalStream")]
    public extern PromiseResult<WebTransportBidirectionalStream> CreateBidirectionalStream(WebTransportSendStreamOptions? options = default);

    /// <summary>
    /// incomingBidirectionalStreams
    /// </summary>
    [Description("@#incomingBidirectionalStreams")]
    public extern ReadableStream IncomingBidirectionalStreams { get; }

    /// <summary>
    /// createUnidirectionalStream
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#createUnidirectionalStream")]
    public extern PromiseResult<WebTransportSendStream> CreateUnidirectionalStream(WebTransportSendStreamOptions? options = default);

    /// <summary>
    /// incomingUnidirectionalStreams
    /// </summary>
    [Description("@#incomingUnidirectionalStreams")]
    public extern ReadableStream IncomingUnidirectionalStreams { get; }

    /// <summary>
    /// createSendGroup
    /// </summary>
    [Description("@#createSendGroup")]
    public extern WebTransportSendGroup CreateSendGroup();

    /// <summary>
    /// supportsReliableOnly
    /// </summary>
    [Description("@#supportsReliableOnly")]
    public extern static bool SupportsReliableOnly { get; }
}

/// <summary>
/// WebTransportSendStream
/// </summary>
[ECMAScript]
[Description("@#WebTransportSendStream")]
public class WebTransportSendStream(object underlyingSink, QueuingStrategy strategy) : WritableStream(underlyingSink, strategy)
{
    /// <summary>
    /// sendGroup
    /// </summary>
    [Description("@#sendGroup")]
    public extern WebTransportSendGroup? SendGroup { get; set; }

    /// <summary>
    /// sendOrder
    /// </summary>
    [Description("@#sendOrder")]
    public extern long SendOrder { get; set; }

    /// <summary>
    /// getStats
    /// </summary>
    [Description("@#getStats")]
    public extern PromiseResult<WebTransportSendStreamStats> GetStats();

    /// <summary>
    /// getWriter
    /// </summary>
    [Description("@#getWriter")]
    public extern new WebTransportWriter GetWriter();
}

/// <summary>
/// WebTransportSendGroup
/// </summary>
[ECMAScript]
[Description("@#WebTransportSendGroup")]
public class WebTransportSendGroup
{
    /// <summary>
    /// getStats
    /// </summary>
    [Description("@#getStats")]
    public extern PromiseResult<WebTransportSendStreamStats> GetStats();
}

/// <summary>
/// WebTransportReceiveStream
/// </summary>
[ECMAScript]
[Description("@#WebTransportReceiveStream")]
public class WebTransportReceiveStream(object underlyingSource, QueuingStrategy strategy) : ReadableStream(underlyingSource, strategy)
{
    /// <summary>
    /// getStats
    /// </summary>
    [Description("@#getStats")]
    public extern PromiseResult<WebTransportReceiveStreamStats> GetStats();
}

/// <summary>
/// WebTransportBidirectionalStream
/// </summary>
[ECMAScript]
[Description("@#WebTransportBidirectionalStream")]
public class WebTransportBidirectionalStream
{
    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern WebTransportReceiveStream Readable { get; }

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern WebTransportSendStream Writable { get; }
}

/// <summary>
/// WebTransportWriter
/// </summary>
[ECMAScript]
[Description("@#WebTransportWriter")]
public class WebTransportWriter(WritableStream stream) : WritableStreamDefaultWriter(stream)
{
    /// <summary>
    /// atomicWrite
    /// </summary>
    /// <param name="chunk">chunk</param>
    [Description("@#atomicWrite")]
    public extern PromiseResult<object> AtomicWrite(object chunk);
}

/// <summary>
/// WebTransportError
/// </summary>
[ECMAScript]
[Description("@#WebTransportError")]
public class WebTransportError(string message, string name) : DOMException(message, name)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    public extern WebTransportError(string message, WebTransportErrorOptions options);

    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern WebTransportErrorSource Source { get; }

    /// <summary>
    /// streamErrorCode
    /// </summary>
    [Description("@#streamErrorCode")]
    public extern uint? StreamErrorCode { get; }
}