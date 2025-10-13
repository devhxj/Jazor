namespace ECMAScript;

/// <summary>
/// RTCIceParameters
/// </summary>
[ECMAScript]
[Description("@#RTCIceParameters")]
public record RTCIceParameters(
    [property: Description("@#iceLite")]bool IceLite = default,
	[property: Description("@#usernameFragment")]string? UsernameFragment = default,
	[property: Description("@#password")]string? Password = default)
{
    [Category("optional")]
    public extern static RTCIceParameters OptionalIceLite(
        [Description("@#iceLite")]bool IceLite = default);

    [Category("optional")]
    public extern static RTCIceParameters OptionalUsernameFragmentPassword(
        [Description("@#usernameFragment")]string? UsernameFragment = default,
        [Description("@#password")]string? Password = default);
}

/// <summary>
/// RTCIceGatherOptions
/// </summary>
[ECMAScript]
[Description("@#RTCIceGatherOptions")]
public record RTCIceGatherOptions(
    [property: Description("@#gatherPolicy")]RTCIceTransportPolicy GatherPolicy = RTCIceTransportPolicy.All,
	[property: Description("@#iceServers")]RTCIceServer[]? IceServers = default);

/// <summary>
/// RTCIceTransport
/// </summary>
[ECMAScript]
[Description("@#RTCIceTransport")]
public partial class RTCIceTransport : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern RTCIceTransport();

    /// <summary>
    /// gather
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#gather")]
    public extern void Gather(RTCIceGatherOptions? options = default);

    /// <summary>
    /// start
    /// </summary>
    /// <param name="remoteParameters">remoteParameters</param>
    /// <param name="role">role</param>
    [Description("@#start")]
    public extern void Start(RTCIceParameters? remoteParameters = default, RTCIceRole role = RTCIceRole.Controlled);

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();

    /// <summary>
    /// addRemoteCandidate
    /// </summary>
    /// <param name="remoteCandidate">remoteCandidate</param>
    [Description("@#addRemoteCandidate")]
    public extern void AddRemoteCandidate(RTCIceCandidateInit? remoteCandidate = default);

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern EventHandler Onerror { get; set; }

    /// <summary>
    /// onicecandidate
    /// </summary>
    [Description("@#onicecandidate")]
    public extern EventHandler Onicecandidate { get; set; }

    /// <summary>
    /// role
    /// </summary>
    [Description("@#role")]
    public extern RTCIceRole Role { get; }

    /// <summary>
    /// component
    /// </summary>
    [Description("@#component")]
    public extern RTCIceComponent Component { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern RTCIceTransportState State { get; }

    /// <summary>
    /// gatheringState
    /// </summary>
    [Description("@#gatheringState")]
    public extern RTCIceGathererState GatheringState { get; }

    /// <summary>
    /// getLocalCandidates
    /// </summary>
    [Description("@#getLocalCandidates")]
    public extern RTCIceCandidate[] GetLocalCandidates();

    /// <summary>
    /// getRemoteCandidates
    /// </summary>
    [Description("@#getRemoteCandidates")]
    public extern RTCIceCandidate[] GetRemoteCandidates();

    /// <summary>
    /// getSelectedCandidatePair
    /// </summary>
    [Description("@#getSelectedCandidatePair")]
    public extern RTCIceCandidatePair? GetSelectedCandidatePair();

    /// <summary>
    /// getLocalParameters
    /// </summary>
    [Description("@#getLocalParameters")]
    public extern RTCIceParameters? GetLocalParameters();

    /// <summary>
    /// getRemoteParameters
    /// </summary>
    [Description("@#getRemoteParameters")]
    public extern RTCIceParameters? GetRemoteParameters();

    /// <summary>
    /// onstatechange
    /// </summary>
    [Description("@#onstatechange")]
    public extern EventHandler Onstatechange { get; set; }

    /// <summary>
    /// ongatheringstatechange
    /// </summary>
    [Description("@#ongatheringstatechange")]
    public extern EventHandler Ongatheringstatechange { get; set; }

    /// <summary>
    /// onselectedcandidatepairchange
    /// </summary>
    [Description("@#onselectedcandidatepairchange")]
    public extern EventHandler Onselectedcandidatepairchange { get; set; }
}