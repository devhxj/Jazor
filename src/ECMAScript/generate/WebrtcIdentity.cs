namespace ECMAScript;

/// <summary>
/// RTCIdentityProvider
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProvider")]
public record RTCIdentityProvider(
    [property: Description("@#generateAssertion")]GenerateAssertionCallback? GenerateAssertion = default,
	[property: Description("@#validateAssertion")]ValidateAssertionCallback? ValidateAssertion = default);

/// <summary>
/// RTCIdentityAssertionResult
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityAssertionResult")]
public record RTCIdentityAssertionResult(
    [property: Description("@#idp")]RTCIdentityProviderDetails? Idp = default,
	[property: Description("@#assertion")]string? Assertion = default);

/// <summary>
/// RTCIdentityProviderDetails
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProviderDetails")]
public record RTCIdentityProviderDetails(
    [property: Description("@#domain")]string? Domain = default,
	[property: Description("@#protocol")]string? Protocol = default);

/// <summary>
/// RTCIdentityValidationResult
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityValidationResult")]
public record RTCIdentityValidationResult(
    [property: Description("@#identity")]string? Identity = default,
	[property: Description("@#contents")]string? Contents = default);

/// <summary>
/// RTCConfiguration
/// </summary>
[ECMAScript]
[Description("@#RTCConfiguration")]
public record RTCConfiguration(
    [property: Description("@#peerIdentity")]string? PeerIdentity = default,
	[property: Description("@#iceServers")]RTCIceServer[]? IceServers = default,
	[property: Description("@#iceTransportPolicy")]RTCIceTransportPolicy IceTransportPolicy = RTCIceTransportPolicy.All,
	[property: Description("@#bundlePolicy")]RTCBundlePolicy BundlePolicy = RTCBundlePolicy.Balanced,
	[property: Description("@#rtcpMuxPolicy")]RTCRtcpMuxPolicy RtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
	[property: Description("@#certificates")]RTCCertificate[]? Certificates = default,
	[property: Description("@#iceCandidatePoolSize")]byte IceCandidatePoolSize = 0)
{
    [Category("optional")]
    public extern static RTCConfiguration OptionalPeerIdentity(
        [Description("@#peerIdentity")]string? PeerIdentity = default);

    [Category("optional")]
    public extern static RTCConfiguration OptionalIceServersIceTransportPolicyBundlePolicy6(
        [Description("@#iceServers")]RTCIceServer[]? iceServers = default,
        [Description("@#iceTransportPolicy")]RTCIceTransportPolicy iceTransportPolicy = RTCIceTransportPolicy.All,
        [Description("@#bundlePolicy")]RTCBundlePolicy bundlePolicy = RTCBundlePolicy.Balanced,
        [Description("@#rtcpMuxPolicy")]RTCRtcpMuxPolicy rtcpMuxPolicy = RTCRtcpMuxPolicy.Require,
        [Description("@#certificates")]RTCCertificate[]? certificates = default,
        [Description("@#iceCandidatePoolSize")]byte iceCandidatePoolSize = 0);
}

/// <summary>
/// RTCIdentityProviderOptions
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProviderOptions")]
public record RTCIdentityProviderOptions(
    [property: Description("@#protocol")]string? Protocol = default,
	[property: Description("@#usernameHint")]string? UsernameHint = default,
	[property: Description("@#peerIdentity")]string? PeerIdentity = default);

/// <summary>
/// RTCErrorInit
/// </summary>
[ECMAScript]
[Description("@#RTCErrorInit")]
public record RTCErrorInit(
    [property: Description("@#httpRequestStatusCode")]int HttpRequestStatusCode = default,
	[property: Description("@#errorDetail")]RTCErrorDetailType? ErrorDetail = default,
	[property: Description("@#sdpLineNumber")]int SdpLineNumber = default,
	[property: Description("@#sctpCauseCode")]int SctpCauseCode = default,
	[property: Description("@#receivedAlert")]uint ReceivedAlert = default,
	[property: Description("@#sentAlert")]uint SentAlert = default)
{
    [Category("optional")]
    public extern static RTCErrorInit OptionalHttpRequestStatusCode(
        [Description("@#httpRequestStatusCode")]int HttpRequestStatusCode = default);

    [Category("optional")]
    public extern static RTCErrorInit OptionalErrorDetailSdpLineNumberSctpCauseCode5(
        [Description("@#errorDetail")]RTCErrorDetailType? ErrorDetail = default,
        [Description("@#sdpLineNumber")]int SdpLineNumber = default,
        [Description("@#sctpCauseCode")]int SctpCauseCode = default,
        [Description("@#receivedAlert")]uint ReceivedAlert = default,
        [Description("@#sentAlert")]uint SentAlert = default);
}

/// <summary>
/// RTCIdentityProviderGlobalScope
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProviderGlobalScope")]
public class RTCIdentityProviderGlobalScope : WorkerGlobalScope
{
    /// <summary>
    /// rtcIdentityProvider
    /// </summary>
    [Description("@#rtcIdentityProvider")]
    public extern RTCIdentityProviderRegistrar RtcIdentityProvider { get; }
}

/// <summary>
/// RTCIdentityProviderRegistrar
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityProviderRegistrar")]
public class RTCIdentityProviderRegistrar
{
    /// <summary>
    /// register
    /// </summary>
    /// <param name="idp">idp</param>
    [Description("@#register")]
    public extern void Register(RTCIdentityProvider idp);
}

/// <summary>
/// RTCPeerConnection
/// </summary>
[ECMAScript]
[Description("@#RTCPeerConnection")]
public partial class RTCPeerConnection : EventTarget
{
    /// <summary>
    /// setIdentityProvider
    /// </summary>
    /// <param name="provider">provider</param>
    /// <param name="options">options</param>
    [Description("@#setIdentityProvider")]
    public extern void SetIdentityProvider(string provider, RTCIdentityProviderOptions? options = default);

    /// <summary>
    /// getIdentityAssertion
    /// </summary>
    [Description("@#getIdentityAssertion")]
    public extern PromiseResult<string> GetIdentityAssertion();

    /// <summary>
    /// peerIdentity
    /// </summary>
    [Description("@#peerIdentity")]
    public extern PromiseResult<RTCIdentityAssertion> PeerIdentity { get; }

    /// <summary>
    /// idpLoginUrl
    /// </summary>
    [Description("@#idpLoginUrl")]
    public extern string? IdpLoginUrl { get; }

    /// <summary>
    /// idpErrorInfo
    /// </summary>
    [Description("@#idpErrorInfo")]
    public extern string? IdpErrorInfo { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="configuration">configuration</param>
    public extern RTCPeerConnection(RTCConfiguration configuration);

    /// <summary>
    /// createOffer
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#createOffer")]
    public extern PromiseResult<RTCSessionDescriptionInit> CreateOffer(RTCOfferOptions? options = default);

    /// <summary>
    /// createAnswer
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#createAnswer")]
    public extern PromiseResult<RTCSessionDescriptionInit> CreateAnswer(RTCAnswerOptions? options = default);

    /// <summary>
    /// setLocalDescription
    /// </summary>
    /// <param name="description">description</param>
    [Description("@#setLocalDescription")]
    public extern PromiseResult<object> SetLocalDescription(RTCLocalSessionDescriptionInit? description = default);

    /// <summary>
    /// localDescription
    /// </summary>
    [Description("@#localDescription")]
    public extern RTCSessionDescription? LocalDescription { get; }

    /// <summary>
    /// currentLocalDescription
    /// </summary>
    [Description("@#currentLocalDescription")]
    public extern RTCSessionDescription? CurrentLocalDescription { get; }

    /// <summary>
    /// pendingLocalDescription
    /// </summary>
    [Description("@#pendingLocalDescription")]
    public extern RTCSessionDescription? PendingLocalDescription { get; }

    /// <summary>
    /// setRemoteDescription
    /// </summary>
    /// <param name="description">description</param>
    [Description("@#setRemoteDescription")]
    public extern PromiseResult<object> SetRemoteDescription(RTCSessionDescriptionInit description);

    /// <summary>
    /// remoteDescription
    /// </summary>
    [Description("@#remoteDescription")]
    public extern RTCSessionDescription? RemoteDescription { get; }

    /// <summary>
    /// currentRemoteDescription
    /// </summary>
    [Description("@#currentRemoteDescription")]
    public extern RTCSessionDescription? CurrentRemoteDescription { get; }

    /// <summary>
    /// pendingRemoteDescription
    /// </summary>
    [Description("@#pendingRemoteDescription")]
    public extern RTCSessionDescription? PendingRemoteDescription { get; }

    /// <summary>
    /// addIceCandidate
    /// </summary>
    /// <param name="candidate">candidate</param>
    [Description("@#addIceCandidate")]
    public extern PromiseResult<object> AddIceCandidate(RTCIceCandidateInit? candidate = default);

    /// <summary>
    /// signalingState
    /// </summary>
    [Description("@#signalingState")]
    public extern RTCSignalingState SignalingState { get; }

    /// <summary>
    /// iceGatheringState
    /// </summary>
    [Description("@#iceGatheringState")]
    public extern RTCIceGatheringState IceGatheringState { get; }

    /// <summary>
    /// iceConnectionState
    /// </summary>
    [Description("@#iceConnectionState")]
    public extern RTCIceConnectionState IceConnectionState { get; }

    /// <summary>
    /// connectionState
    /// </summary>
    [Description("@#connectionState")]
    public extern RTCPeerConnectionState ConnectionState { get; }

    /// <summary>
    /// canTrickleIceCandidates
    /// </summary>
    [Description("@#canTrickleIceCandidates")]
    public extern bool? CanTrickleIceCandidates { get; }

    /// <summary>
    /// restartIce
    /// </summary>
    [Description("@#restartIce")]
    public extern void RestartIce();

    /// <summary>
    /// getConfiguration
    /// </summary>
    [Description("@#getConfiguration")]
    public extern RTCConfiguration GetConfiguration();

    /// <summary>
    /// setConfiguration
    /// </summary>
    /// <param name="configuration">configuration</param>
    [Description("@#setConfiguration")]
    public extern void SetConfiguration(RTCConfiguration? configuration = default);

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// onnegotiationneeded
    /// </summary>
    [Description("@#onnegotiationneeded")]
    public extern EventHandler Onnegotiationneeded { get; set; }

    /// <summary>
    /// onicecandidate
    /// </summary>
    [Description("@#onicecandidate")]
    public extern EventHandler Onicecandidate { get; set; }

    /// <summary>
    /// onicecandidateerror
    /// </summary>
    [Description("@#onicecandidateerror")]
    public extern EventHandler Onicecandidateerror { get; set; }

    /// <summary>
    /// onsignalingstatechange
    /// </summary>
    [Description("@#onsignalingstatechange")]
    public extern EventHandler Onsignalingstatechange { get; set; }

    /// <summary>
    /// oniceconnectionstatechange
    /// </summary>
    [Description("@#oniceconnectionstatechange")]
    public extern EventHandler Oniceconnectionstatechange { get; set; }

    /// <summary>
    /// onicegatheringstatechange
    /// </summary>
    [Description("@#onicegatheringstatechange")]
    public extern EventHandler Onicegatheringstatechange { get; set; }

    /// <summary>
    /// onconnectionstatechange
    /// </summary>
    [Description("@#onconnectionstatechange")]
    public extern EventHandler Onconnectionstatechange { get; set; }

    /// <summary>
    /// createOffer
    /// </summary>
    /// <param name="successCallback">successCallback</param>
    /// <param name="failureCallback">failureCallback</param>
    /// <param name="options">options</param>
    [Description("@#createOffer")]
    public extern PromiseResult<object> CreateOffer(RTCSessionDescriptionCallback successCallback, RTCPeerConnectionErrorCallback failureCallback, RTCOfferOptions? options = default);

    /// <summary>
    /// setLocalDescription
    /// </summary>
    /// <param name="description">description</param>
    /// <param name="successCallback">successCallback</param>
    /// <param name="failureCallback">failureCallback</param>
    [Description("@#setLocalDescription")]
    public extern PromiseResult<object> SetLocalDescription(RTCLocalSessionDescriptionInit description, Action successCallback, RTCPeerConnectionErrorCallback failureCallback);

    /// <summary>
    /// createAnswer
    /// </summary>
    /// <param name="successCallback">successCallback</param>
    /// <param name="failureCallback">failureCallback</param>
    [Description("@#createAnswer")]
    public extern PromiseResult<object> CreateAnswer(RTCSessionDescriptionCallback successCallback, RTCPeerConnectionErrorCallback failureCallback);

    /// <summary>
    /// setRemoteDescription
    /// </summary>
    /// <param name="description">description</param>
    /// <param name="successCallback">successCallback</param>
    /// <param name="failureCallback">failureCallback</param>
    [Description("@#setRemoteDescription")]
    public extern PromiseResult<object> SetRemoteDescription(RTCSessionDescriptionInit description, Action successCallback, RTCPeerConnectionErrorCallback failureCallback);

    /// <summary>
    /// addIceCandidate
    /// </summary>
    /// <param name="candidate">candidate</param>
    /// <param name="successCallback">successCallback</param>
    /// <param name="failureCallback">failureCallback</param>
    [Description("@#addIceCandidate")]
    public extern PromiseResult<object> AddIceCandidate(RTCIceCandidateInit candidate, Action successCallback, RTCPeerConnectionErrorCallback failureCallback);

    /// <summary>
    /// generateCertificate
    /// </summary>
    /// <param name="keygenAlgorithm">keygenAlgorithm</param>
    [Description("@#generateCertificate")]
    public extern static PromiseResult<RTCCertificate> GenerateCertificate(AlgorithmIdentifier keygenAlgorithm);

    /// <summary>
    /// getSenders
    /// </summary>
    [Description("@#getSenders")]
    public extern RTCRtpSender[] GetSenders();

    /// <summary>
    /// getReceivers
    /// </summary>
    [Description("@#getReceivers")]
    public extern RTCRtpReceiver[] GetReceivers();

    /// <summary>
    /// getTransceivers
    /// </summary>
    [Description("@#getTransceivers")]
    public extern RTCRtpTransceiver[] GetTransceivers();

    /// <summary>
    /// addTrack
    /// </summary>
    /// <param name="track">track</param>
    /// <param name="streams">streams</param>
    [Description("@#addTrack")]
    public extern RTCRtpSender AddTrack(MediaStreamTrack track, MediaStream streams);

    /// <summary>
    /// removeTrack
    /// </summary>
    /// <param name="sender">sender</param>
    [Description("@#removeTrack")]
    public extern void RemoveTrack(RTCRtpSender sender);

    /// <summary>
    /// addTransceiver
    /// </summary>
    /// <param name="trackOrKind">trackOrKind</param>
    /// <param name="init">init</param>
    [Description("@#addTransceiver")]
    public extern RTCRtpTransceiver AddTransceiver(Either<MediaStreamTrack, string> trackOrKind, RTCRtpTransceiverInit? init = default);

    /// <summary>
    /// ontrack
    /// </summary>
    [Description("@#ontrack")]
    public extern EventHandler Ontrack { get; set; }

    /// <summary>
    /// sctp
    /// </summary>
    [Description("@#sctp")]
    public extern RTCSctpTransport? Sctp { get; }

    /// <summary>
    /// createDataChannel
    /// </summary>
    /// <param name="label">label</param>
    /// <param name="dataChannelDict">dataChannelDict</param>
    [Description("@#createDataChannel")]
    public extern RTCDataChannel CreateDataChannel(string label, RTCDataChannelInit? dataChannelDict = default);

    /// <summary>
    /// ondatachannel
    /// </summary>
    [Description("@#ondatachannel")]
    public extern EventHandler Ondatachannel { get; set; }

    /// <summary>
    /// getStats
    /// </summary>
    /// <param name="selector">selector</param>
    [Description("@#getStats")]
    public extern PromiseResult<RTCStatsReport> GetStats(MediaStreamTrack? selector = null);
}

/// <summary>
/// RTCIdentityAssertion
/// </summary>
[ECMAScript]
[Description("@#RTCIdentityAssertion")]
public class RTCIdentityAssertion
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="idp">idp</param>
    /// <param name="name">name</param>
    public extern RTCIdentityAssertion(string idp, string name);

    /// <summary>
    /// idp
    /// </summary>
    [Description("@#idp")]
    public extern string Idp { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }
}

/// <summary>
/// RTCError
/// </summary>
[ECMAScript]
[Description("@#RTCError")]
public partial class RTCError(string message, string name) : DOMException(message, name)
{
    /// <summary>
    /// httpRequestStatusCode
    /// </summary>
    [Description("@#httpRequestStatusCode")]
    public extern int? HttpRequestStatusCode { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    /// <param name="message">message</param>
    public extern RTCError(RTCErrorInit init, string message);

    /// <summary>
    /// errorDetail
    /// </summary>
    [Description("@#errorDetail")]
    public extern RTCErrorDetailType ErrorDetail { get; }

    /// <summary>
    /// sdpLineNumber
    /// </summary>
    [Description("@#sdpLineNumber")]
    public extern int? SdpLineNumber { get; }

    /// <summary>
    /// sctpCauseCode
    /// </summary>
    [Description("@#sctpCauseCode")]
    public extern int? SctpCauseCode { get; }

    /// <summary>
    /// receivedAlert
    /// </summary>
    [Description("@#receivedAlert")]
    public extern uint? ReceivedAlert { get; }

    /// <summary>
    /// sentAlert
    /// </summary>
    [Description("@#sentAlert")]
    public extern uint? SentAlert { get; }
}