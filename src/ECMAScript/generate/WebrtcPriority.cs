namespace ECMAScript;

/// <summary>
/// RTCRtpEncodingParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpEncodingParameters")]
public record RTCRtpEncodingParameters(
    [property: Description("@#priority")]RTCPriorityType Priority = RTCPriorityType.Low,
	[property: Description("@#networkPriority")]RTCPriorityType? NetworkPriority = default,
	[property: Description("@#scalabilityMode")]string? ScalabilityMode = default,
	[property: Description("@#active")]bool Active = true,
	[property: Description("@#codec")]RTCRtpCodec? Codec = default,
	[property: Description("@#maxBitrate")]uint MaxBitrate = default,
	[property: Description("@#maxFramerate")]double MaxFramerate = default,
	[property: Description("@#scaleResolutionDownBy")]double ScaleResolutionDownBy = default): RTCRtpCodingParameters
{
    [Category("optional")]
    public extern static RTCRtpEncodingParameters OptionalPriorityNetworkPriority(
        [Description("@#priority")]RTCPriorityType priority = RTCPriorityType.Low,
        [Description("@#networkPriority")]RTCPriorityType? NetworkPriority = default);

    [Category("optional")]
    public extern static RTCRtpEncodingParameters OptionalScalabilityMode(
        [Description("@#scalabilityMode")]string? ScalabilityMode = default);

    [Category("optional")]
    public extern static RTCRtpEncodingParameters OptionalActiveCodecMaxBitrate5(
        [Description("@#active")]bool active = true,
        [Description("@#codec")]RTCRtpCodec? Codec = default,
        [Description("@#maxBitrate")]uint MaxBitrate = default,
        [Description("@#maxFramerate")]double MaxFramerate = default,
        [Description("@#scaleResolutionDownBy")]double ScaleResolutionDownBy = default);
}

/// <summary>
/// RTCDataChannelInit
/// </summary>
[ECMAScript]
[Description("@#RTCDataChannelInit")]
public record RTCDataChannelInit(
    [property: Description("@#priority")]RTCPriorityType Priority = RTCPriorityType.Low,
	[property: Description("@#ordered")]bool Ordered = true,
	[property: Description("@#maxPacketLifeTime")]ushort MaxPacketLifeTime = default,
	[property: Description("@#maxRetransmits")]ushort MaxRetransmits = default,
	[property: Description("@#protocol")]string? Protocol = default,
	[property: Description("@#negotiated")]bool Negotiated = false,
	[property: Description("@#id")]ushort Id = default)
{
    [Category("optional")]
    public extern static RTCDataChannelInit OptionalPriority(
        [Description("@#priority")]RTCPriorityType priority = RTCPriorityType.Low);

    [Category("optional")]
    public extern static RTCDataChannelInit OptionalOrderedMaxPacketLifeTimeMaxRetransmits6(
        [Description("@#ordered")]bool ordered = true,
        [Description("@#maxPacketLifeTime")]ushort MaxPacketLifeTime = default,
        [Description("@#maxRetransmits")]ushort MaxRetransmits = default,
        [Description("@#protocol")]string? protocol = default,
        [Description("@#negotiated")]bool negotiated = false,
        [Description("@#id")]ushort Id = default);
}

/// <summary>
/// RTCDataChannel
/// </summary>
[ECMAScript]
[Description("@#RTCDataChannel")]
public partial class RTCDataChannel : EventTarget
{
    /// <summary>
    /// priority
    /// </summary>
    [Description("@#priority")]
    public extern RTCPriorityType Priority { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; }

    /// <summary>
    /// ordered
    /// </summary>
    [Description("@#ordered")]
    public extern bool Ordered { get; }

    /// <summary>
    /// maxPacketLifeTime
    /// </summary>
    [Description("@#maxPacketLifeTime")]
    public extern ushort? MaxPacketLifeTime { get; }

    /// <summary>
    /// maxRetransmits
    /// </summary>
    [Description("@#maxRetransmits")]
    public extern ushort? MaxRetransmits { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; }

    /// <summary>
    /// negotiated
    /// </summary>
    [Description("@#negotiated")]
    public extern bool Negotiated { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern ushort? Id { get; }

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern RTCDataChannelState ReadyState { get; }

    /// <summary>
    /// bufferedAmount
    /// </summary>
    [Description("@#bufferedAmount")]
    public extern uint BufferedAmount { get; }

    /// <summary>
    /// bufferedAmountLowThreshold
    /// </summary>
    [Description("@#bufferedAmountLowThreshold")]
    public extern uint BufferedAmountLowThreshold { get; set; }

    /// <summary>
    /// onopen
    /// </summary>
    [Description("@#onopen")]
    public extern EventHandler Onopen { get; set; }

    /// <summary>
    /// onbufferedamountlow
    /// </summary>
    [Description("@#onbufferedamountlow")]
    public extern EventHandler Onbufferedamountlow { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onclosing
    /// </summary>
    [Description("@#onclosing")]
    public extern EventHandler Onclosing { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

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
    public extern void Send(string data);

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