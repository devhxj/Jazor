namespace ECMAScript;

/// <summary>
/// RTCIceServer
/// </summary>
[ECMAScript]
[Description("@#RTCIceServer")]
public record RTCIceServer(
    [property: Description("@#urls")]Either<string, string[]>? Urls = default,
	[property: Description("@#username")]string? Username = default,
	[property: Description("@#credential")]string? Credential = default);

/// <summary>
/// RTCOfferAnswerOptions
/// </summary>
[ECMAScript]
[Description("@#RTCOfferAnswerOptions")]
public abstract record RTCOfferAnswerOptions();

/// <summary>
/// RTCOfferOptions
/// </summary>
[ECMAScript]
[Description("@#RTCOfferOptions")]
public record RTCOfferOptions(
    [property: Description("@#iceRestart")]bool IceRestart = false,
	[property: Description("@#offerToReceiveAudio")]bool OfferToReceiveAudio = default,
	[property: Description("@#offerToReceiveVideo")]bool OfferToReceiveVideo = default): RTCOfferAnswerOptions
{
    [Category("optional")]
    public extern static RTCOfferOptions OptionalIceRestart(
        [Description("@#iceRestart")]bool iceRestart = false);

    [Category("optional")]
    public extern static RTCOfferOptions OptionalOfferToReceiveAudioOfferToReceiveVideo(
        [Description("@#offerToReceiveAudio")]bool OfferToReceiveAudio = default,
        [Description("@#offerToReceiveVideo")]bool OfferToReceiveVideo = default);
}

/// <summary>
/// RTCAnswerOptions
/// </summary>
[ECMAScript]
[Description("@#RTCAnswerOptions")]
public abstract record RTCAnswerOptions();

/// <summary>
/// RTCSessionDescriptionInit
/// </summary>
[ECMAScript]
[Description("@#RTCSessionDescriptionInit")]
public record RTCSessionDescriptionInit(
    [property: Description("@#type")]RTCSdpType? Type = default,
	[property: Description("@#sdp")]string? Sdp = default);

/// <summary>
/// RTCLocalSessionDescriptionInit
/// </summary>
[ECMAScript]
[Description("@#RTCLocalSessionDescriptionInit")]
public record RTCLocalSessionDescriptionInit(
    [property: Description("@#type")]RTCSdpType? Type = default,
	[property: Description("@#sdp")]string? Sdp = default);

/// <summary>
/// RTCIceCandidateInit
/// </summary>
[ECMAScript]
[Description("@#RTCIceCandidateInit")]
public record RTCIceCandidateInit(
    [property: Description("@#candidate")]string? Candidate = default,
	[property: Description("@#sdpMid")]string? SdpMid = null,
	[property: Description("@#sdpMLineIndex")]ushort? SdpMLineIndex = null,
	[property: Description("@#usernameFragment")]string? UsernameFragment = null);

/// <summary>
/// RTCLocalIceCandidateInit
/// </summary>
[ECMAScript]
[Description("@#RTCLocalIceCandidateInit")]
public record RTCLocalIceCandidateInit(
    [property: Description("@#relayProtocol")]RTCIceServerTransportProtocol? RelayProtocol = null,
	[property: Description("@#url")]string? Url = null): RTCIceCandidateInit;

/// <summary>
/// RTCPeerConnectionIceEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionIceEventInit")]
public record RTCPeerConnectionIceEventInit(
    [property: Description("@#candidate")]RTCIceCandidate? Candidate = default,
	[property: Description("@#url")]string? Url = default): EventInit;

/// <summary>
/// RTCPeerConnectionIceErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionIceErrorEventInit")]
public record RTCPeerConnectionIceErrorEventInit(
    [property: Description("@#address")]string? Address = default,
	[property: Description("@#port")]ushort Port = default,
	[property: Description("@#url")]string? Url = default,
	[property: Description("@#errorCode")]ushort ErrorCode = default,
	[property: Description("@#errorText")]string? ErrorText = default): EventInit;

/// <summary>
/// RTCCertificateExpiration
/// </summary>
[ECMAScript]
[Description("@#RTCCertificateExpiration")]
public record RTCCertificateExpiration(
    [property: Description("@#expires")]ulong Expires = default);

/// <summary>
/// RTCRtpTransceiverInit
/// </summary>
[ECMAScript]
[Description("@#RTCRtpTransceiverInit")]
public record RTCRtpTransceiverInit(
    [property: Description("@#direction")]RTCRtpTransceiverDirection Direction = RTCRtpTransceiverDirection.Sendrecv,
	[property: Description("@#streams")]MediaStream[]? Streams = default,
	[property: Description("@#sendEncodings")]RTCRtpEncodingParameters[]? SendEncodings = default);

/// <summary>
/// RTCRtpParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpParameters")]
public record RTCRtpParameters(
    [property: Description("@#headerExtensions")]RTCRtpHeaderExtensionParameters[]? HeaderExtensions = default,
	[property: Description("@#rtcp")]RTCRtcpParameters? Rtcp = default,
	[property: Description("@#codecs")]RTCRtpCodecParameters[]? Codecs = default);

/// <summary>
/// RTCRtpReceiveParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpReceiveParameters")]
public abstract record RTCRtpReceiveParameters();

/// <summary>
/// RTCRtpCodingParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCodingParameters")]
public record RTCRtpCodingParameters(
    [property: Description("@#rid")]string? Rid = default);

/// <summary>
/// RTCRtcpParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtcpParameters")]
public record RTCRtcpParameters(
    [property: Description("@#cname")]string? Cname = default,
	[property: Description("@#reducedSize")]bool ReducedSize = default);

/// <summary>
/// RTCRtpHeaderExtensionParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpHeaderExtensionParameters")]
public record RTCRtpHeaderExtensionParameters(
    [property: Description("@#uri")]string? Uri = default,
	[property: Description("@#id")]ushort Id = default,
	[property: Description("@#encrypted")]bool Encrypted = false);

/// <summary>
/// RTCRtpCodec
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCodec")]
public record RTCRtpCodec(
    [property: Description("@#mimeType")]string? MimeType = default,
	[property: Description("@#clockRate")]uint ClockRate = default,
	[property: Description("@#channels")]ushort Channels = default,
	[property: Description("@#sdpFmtpLine")]string? SdpFmtpLine = default);

/// <summary>
/// RTCRtpCodecParameters
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCodecParameters")]
public record RTCRtpCodecParameters(
    [property: Description("@#payloadType")]byte PayloadType = default): RTCRtpCodec;

/// <summary>
/// RTCRtpCapabilities
/// </summary>
[ECMAScript]
[Description("@#RTCRtpCapabilities")]
public record RTCRtpCapabilities(
    [property: Description("@#codecs")]RTCRtpCodec[]? Codecs = default,
	[property: Description("@#headerExtensions")]RTCRtpHeaderExtensionCapability[]? HeaderExtensions = default);

/// <summary>
/// RTCRtpHeaderExtensionCapability
/// </summary>
[ECMAScript]
[Description("@#RTCRtpHeaderExtensionCapability")]
public record RTCRtpHeaderExtensionCapability(
    [property: Description("@#uri")]string? Uri = default);

/// <summary>
/// RTCSetParameterOptions
/// </summary>
[ECMAScript]
[Description("@#RTCSetParameterOptions")]
public abstract record RTCSetParameterOptions();

/// <summary>
/// RTCRtpContributingSource
/// </summary>
[ECMAScript]
[Description("@#RTCRtpContributingSource")]
public record RTCRtpContributingSource(
    [property: Description("@#timestamp")]double Timestamp = default,
	[property: Description("@#source")]uint Source = default,
	[property: Description("@#audioLevel")]double AudioLevel = default,
	[property: Description("@#rtpTimestamp")]uint RtpTimestamp = default);

/// <summary>
/// RTCRtpSynchronizationSource
/// </summary>
[ECMAScript]
[Description("@#RTCRtpSynchronizationSource")]
public abstract record RTCRtpSynchronizationSource();

/// <summary>
/// RTCDtlsFingerprint
/// </summary>
[ECMAScript]
[Description("@#RTCDtlsFingerprint")]
public record RTCDtlsFingerprint(
    [property: Description("@#algorithm")]string? Algorithm = default,
	[property: Description("@#value")]string? Value = default);

/// <summary>
/// RTCTrackEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCTrackEventInit")]
public record RTCTrackEventInit(
    [property: Description("@#receiver")]RTCRtpReceiver? Receiver = default,
	[property: Description("@#track")]MediaStreamTrack? Track = default,
	[property: Description("@#streams")]MediaStream[]? Streams = default,
	[property: Description("@#transceiver")]RTCRtpTransceiver? Transceiver = default): EventInit;

/// <summary>
/// RTCDataChannelEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCDataChannelEventInit")]
public record RTCDataChannelEventInit(
    [property: Description("@#channel")]RTCDataChannel? Channel = default): EventInit;

/// <summary>
/// RTCDTMFToneChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCDTMFToneChangeEventInit")]
public record RTCDTMFToneChangeEventInit(
    [property: Description("@#tone")]string? Tone = default): EventInit;

/// <summary>
/// RTCStats
/// </summary>
[ECMAScript]
[Description("@#RTCStats")]
public record RTCStats(
    [property: Description("@#timestamp")]double Timestamp = default,
	[property: Description("@#type")]RTCStatsType? Type = default,
	[property: Description("@#id")]string? Id = default);

/// <summary>
/// RTCErrorEventInit
/// </summary>
[ECMAScript]
[Description("@#RTCErrorEventInit")]
public record RTCErrorEventInit(
    [property: Description("@#error")]RTCError? Error = default): EventInit;

/// <summary>
/// RTCSessionDescription
/// </summary>
[ECMAScript]
[Description("@#RTCSessionDescription")]
public class RTCSessionDescription
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="descriptionInitDict">descriptionInitDict</param>
    public extern RTCSessionDescription(RTCSessionDescriptionInit descriptionInitDict);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern RTCSdpType Type { get; }

    /// <summary>
    /// sdp
    /// </summary>
    [Description("@#sdp")]
    public extern string Sdp { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern RTCSessionDescriptionInit ToJSON();
}

/// <summary>
/// RTCIceCandidate
/// </summary>
[ECMAScript]
[Description("@#RTCIceCandidate")]
public class RTCIceCandidate
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="candidateInitDict">candidateInitDict</param>
    public extern RTCIceCandidate(RTCLocalIceCandidateInit candidateInitDict);

    /// <summary>
    /// candidate
    /// </summary>
    [Description("@#candidate")]
    public extern string Candidate { get; }

    /// <summary>
    /// sdpMid
    /// </summary>
    [Description("@#sdpMid")]
    public extern string? SdpMid { get; }

    /// <summary>
    /// sdpMLineIndex
    /// </summary>
    [Description("@#sdpMLineIndex")]
    public extern ushort? SdpMLineIndex { get; }

    /// <summary>
    /// foundation
    /// </summary>
    [Description("@#foundation")]
    public extern string? Foundation { get; }

    /// <summary>
    /// component
    /// </summary>
    [Description("@#component")]
    public extern RTCIceComponent? Component { get; }

    /// <summary>
    /// priority
    /// </summary>
    [Description("@#priority")]
    public extern uint? Priority { get; }

    /// <summary>
    /// address
    /// </summary>
    [Description("@#address")]
    public extern string? Address { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern RTCIceProtocol? Protocol { get; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern ushort? Port { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern RTCIceCandidateType? Type { get; }

    /// <summary>
    /// tcpType
    /// </summary>
    [Description("@#tcpType")]
    public extern RTCIceTcpCandidateType? TcpType { get; }

    /// <summary>
    /// relatedAddress
    /// </summary>
    [Description("@#relatedAddress")]
    public extern string? RelatedAddress { get; }

    /// <summary>
    /// relatedPort
    /// </summary>
    [Description("@#relatedPort")]
    public extern ushort? RelatedPort { get; }

    /// <summary>
    /// usernameFragment
    /// </summary>
    [Description("@#usernameFragment")]
    public extern string? UsernameFragment { get; }

    /// <summary>
    /// relayProtocol
    /// </summary>
    [Description("@#relayProtocol")]
    public extern RTCIceServerTransportProtocol? RelayProtocol { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string? Url { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern RTCIceCandidateInit ToJSON();
}

/// <summary>
/// RTCPeerConnectionIceEvent
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionIceEvent")]
public class RTCPeerConnectionIceEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern RTCPeerConnectionIceEvent(string type, RTCPeerConnectionIceEventInit eventInitDict);

    /// <summary>
    /// candidate
    /// </summary>
    [Description("@#candidate")]
    public extern RTCIceCandidate? Candidate { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string? Url { get; }
}

/// <summary>
/// RTCPeerConnectionIceErrorEvent
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnectionIceErrorEvent")]
public class RTCPeerConnectionIceErrorEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern RTCPeerConnectionIceErrorEvent(string type, RTCPeerConnectionIceErrorEventInit eventInitDict);

    /// <summary>
    /// address
    /// </summary>
    [Description("@#address")]
    public extern string? Address { get; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern ushort? Port { get; }

    /// <summary>
    /// url
    /// </summary>
    [Description("@#url")]
    public extern string Url { get; }

    /// <summary>
    /// errorCode
    /// </summary>
    [Description("@#errorCode")]
    public extern ushort ErrorCode { get; }

    /// <summary>
    /// errorText
    /// </summary>
    [Description("@#errorText")]
    public extern string ErrorText { get; }
}

/// <summary>
/// RTCCertificate
/// </summary>
[ECMAScript]
[Description("@#RTCCertificate")]
public class RTCCertificate
{
    /// <summary>
    /// expires
    /// </summary>
    [Description("@#expires")]
    public extern EpochTimeStamp Expires { get; }

    /// <summary>
    /// getFingerprints
    /// </summary>
    [Description("@#getFingerprints")]
    public extern RTCDtlsFingerprint[] GetFingerprints();
}

/// <summary>
/// RTCRtpTransceiver
/// </summary>
[ECMAScript]
[Description("@#RTCRtpTransceiver")]
public class RTCRtpTransceiver
{
    /// <summary>
    /// mid
    /// </summary>
    [Description("@#mid")]
    public extern string? Mid { get; }

    /// <summary>
    /// sender
    /// </summary>
    [Description("@#sender")]
    public extern RTCRtpSender Sender { get; }

    /// <summary>
    /// receiver
    /// </summary>
    [Description("@#receiver")]
    public extern RTCRtpReceiver Receiver { get; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern RTCRtpTransceiverDirection Direction { get; set; }

    /// <summary>
    /// currentDirection
    /// </summary>
    [Description("@#currentDirection")]
    public extern RTCRtpTransceiverDirection? CurrentDirection { get; }

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();

    /// <summary>
    /// setCodecPreferences
    /// </summary>
    /// <param name="codecs">codecs</param>
    [Description("@#setCodecPreferences")]
    public extern void SetCodecPreferences(RTCRtpCodec[] codecs);
}

/// <summary>
/// RTCDtlsTransport
/// </summary>
[ECMAScript]
[Description("@#RTCDtlsTransport")]
public class RTCDtlsTransport : EventTarget
{
    /// <summary>
    /// iceTransport
    /// </summary>
    [Description("@#iceTransport")]
    public extern RTCIceTransport IceTransport { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern RTCDtlsTransportState State { get; }

    /// <summary>
    /// getRemoteCertificates
    /// </summary>
    [Description("@#getRemoteCertificates")]
    public extern ArrayBuffer[] GetRemoteCertificates();

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }
}

/// <summary>
/// RTCIceCandidatePair
/// </summary>
[ECMAScript]
[Description("@#RTCIceCandidatePair")]
public class RTCIceCandidatePair
{
    /// <summary>
    /// local
    /// </summary>
    [Description("@#local")]
    public extern RTCIceCandidate Local { get; }

    /// <summary>
    /// remote
    /// </summary>
    [Description("@#remote")]
    public extern RTCIceCandidate Remote { get; }
}

/// <summary>
/// RTCTrackEvent
/// </summary>
[ECMAScript]
[Description("@#RTCTrackEvent")]
public class RTCTrackEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern RTCTrackEvent(string type, RTCTrackEventInit eventInitDict);

    /// <summary>
    /// receiver
    /// </summary>
    [Description("@#receiver")]
    public extern RTCRtpReceiver Receiver { get; }

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern MediaStreamTrack Track { get; }

    /// <summary>
    /// streams
    /// </summary>
    [Description("@#streams")]
    public extern FrozenSet<MediaStream> Streams { get; }

    /// <summary>
    /// transceiver
    /// </summary>
    [Description("@#transceiver")]
    public extern RTCRtpTransceiver Transceiver { get; }
}

/// <summary>
/// RTCSctpTransport
/// </summary>
[ECMAScript]
[Description("@#RTCSctpTransport")]
public class RTCSctpTransport : EventTarget
{
    /// <summary>
    /// transport
    /// </summary>
    [Description("@#transport")]
    public extern RTCDtlsTransport Transport { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern RTCSctpTransportState State { get; }

    /// <summary>
    /// maxMessageSize
    /// </summary>
    [Description("@#maxMessageSize")]
    public extern double MaxMessageSize { get; }

    /// <summary>
    /// maxChannels
    /// </summary>
    [Description("@#maxChannels")]
    public extern ushort? MaxChannels { get; }

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }
}

/// <summary>
/// RTCDataChannelEvent
/// </summary>
[ECMAScript]
[Description("@#RTCDataChannelEvent")]
public class RTCDataChannelEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern RTCDataChannelEvent(string type, RTCDataChannelEventInit eventInitDict);

    /// <summary>
    /// channel
    /// </summary>
    [Description("@#channel")]
    public extern RTCDataChannel Channel { get; }
}

/// <summary>
/// RTCDTMFSender
/// </summary>
[ECMAScript]
[Description("@#RTCDTMFSender")]
public class RTCDTMFSender : EventTarget
{
    /// <summary>
    /// insertDTMF
    /// </summary>
    /// <param name="tones">tones</param>
    /// <param name="duration">duration</param>
    /// <param name="interToneGap">interToneGap</param>
    [Description("@#insertDTMF")]
    public extern void InsertDTMF(string tones, uint duration = 100, uint interToneGap = 70);

    /// <summary>
    /// ontonechange
    /// </summary>
    [Description("@#ontonechange")]
    public extern EventHandler Ontonechange { get; set; }

    /// <summary>
    /// canInsertDTMF
    /// </summary>
    [Description("@#canInsertDTMF")]
    public extern bool CanInsertDTMF { get; }

    /// <summary>
    /// toneBuffer
    /// </summary>
    [Description("@#toneBuffer")]
    public extern string ToneBuffer { get; }
}

/// <summary>
/// RTCDTMFToneChangeEvent
/// </summary>
[ECMAScript]
[Description("@#RTCDTMFToneChangeEvent")]
public class RTCDTMFToneChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern RTCDTMFToneChangeEvent(string type, RTCDTMFToneChangeEventInit eventInitDict);

    /// <summary>
    /// tone
    /// </summary>
    [Description("@#tone")]
    public extern string Tone { get; }
}

/// <summary>
/// RTCStatsReport
/// </summary>
[ECMAScript]
[Description("@#RTCStatsReport")]
public class RTCStatsReport : IDictionary<string, object>
{
    #region Dictionary
    extern object IDictionary<string, object>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, object>.Keys { get; }
    extern ICollection<object> IDictionary<string, object>.Values { get; }
    extern int ICollection<KeyValuePair<string, object>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, object>>.IsReadOnly { get; }
    extern void IDictionary<string, object>.Add(string key, object value);
    extern void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item);
    extern void ICollection<KeyValuePair<string, object>>.Clear();
    extern bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item);
    extern bool IDictionary<string, object>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator();
    extern bool IDictionary<string, object>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item);
    extern bool IDictionary<string, object>.TryGetValue(string key, [MaybeNullWhen(false)] out object value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// RTCErrorEvent
/// </summary>
[ECMAScript]
[Description("@#RTCErrorEvent")]
public class RTCErrorEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern RTCErrorEvent(string type, RTCErrorEventInit eventInitDict);

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern RTCError Error { get; }
}