namespace ECMAScript;

/// <summary>
/// MediaTrackConstraints
/// </summary>
[ECMAScript]
[Description("@#MediaTrackConstraints")]
public record MediaTrackConstraints(
    [property: Description("@#advanced")]MediaTrackConstraintSet[]? Advanced = default): MediaTrackConstraintSet;

/// <summary>
/// MediaStreamTrackEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackEventInit")]
public record MediaStreamTrackEventInit(
    [property: Description("@#track")]MediaStreamTrack? Track = default): EventInit;

/// <summary>
/// DeviceChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#DeviceChangeEventInit")]
public record DeviceChangeEventInit(
    [property: Description("@#devices")]MediaDeviceInfo[]? Devices = default): EventInit;

/// <summary>
/// MediaStreamConstraints
/// </summary>
[ECMAScript]
[Description("@#MediaStreamConstraints")]
public record MediaStreamConstraints(
    [property: Description("@#video")]Either<bool, MediaTrackConstraints>? Video = default,
	[property: Description("@#audio")]Either<bool, MediaTrackConstraints>? Audio = default,
	[property: Description("@#preferCurrentTab")]bool PreferCurrentTab = false,
	[property: Description("@#peerIdentity")]string? PeerIdentity = default)
{
    [Category("optional")]
    public extern static MediaStreamConstraints OptionalVideoAudio(
        [Description("@#video")]Either<bool, MediaTrackConstraints>? video = default,
        [Description("@#audio")]Either<bool, MediaTrackConstraints>? audio = default);

    [Category("optional")]
    public extern static MediaStreamConstraints OptionalPreferCurrentTab(
        [Description("@#preferCurrentTab")]bool preferCurrentTab = false);

    [Category("optional")]
    public extern static MediaStreamConstraints OptionalPeerIdentity(
        [Description("@#peerIdentity")]string? PeerIdentity = default);
}

/// <summary>
/// DoubleRange
/// </summary>
[ECMAScript]
[Description("@#DoubleRange")]
public record DoubleRange(
    [property: Description("@#max")]double Max = default,
	[property: Description("@#min")]double Min = default);

/// <summary>
/// ConstrainDoubleRange
/// </summary>
[ECMAScript]
[Description("@#ConstrainDoubleRange")]
public record ConstrainDoubleRange(
    [property: Description("@#exact")]double Exact = default,
	[property: Description("@#ideal")]double Ideal = default): DoubleRange;

/// <summary>
/// ULongRange
/// </summary>
[ECMAScript]
[Description("@#ULongRange")]
public record ULongRange(
    [property: Description("@#max")]uint Max = default,
	[property: Description("@#min")]uint Min = default);

/// <summary>
/// ConstrainULongRange
/// </summary>
[ECMAScript]
[Description("@#ConstrainULongRange")]
public record ConstrainULongRange(
    [property: Description("@#exact")]uint Exact = default,
	[property: Description("@#ideal")]uint Ideal = default): ULongRange;

/// <summary>
/// ConstrainBooleanParameters
/// </summary>
[ECMAScript]
[Description("@#ConstrainBooleanParameters")]
public record ConstrainBooleanParameters(
    [property: Description("@#exact")]bool Exact = default,
	[property: Description("@#ideal")]bool Ideal = default);

/// <summary>
/// ConstrainDOMStringParameters
/// </summary>
[ECMAScript]
[Description("@#ConstrainDOMStringParameters")]
public record ConstrainDOMStringParameters(
    [property: Description("@#exact")]Either<string, string[]>? Exact = default,
	[property: Description("@#ideal")]Either<string, string[]>? Ideal = default);

/// <summary>
/// ConstrainBooleanOrDOMStringParameters
/// </summary>
[ECMAScript]
[Description("@#ConstrainBooleanOrDOMStringParameters")]
public record ConstrainBooleanOrDOMStringParameters(
    [property: Description("@#exact")]Either<bool, string>? Exact = default,
	[property: Description("@#ideal")]Either<bool, string>? Ideal = default);

/// <summary>
/// CameraDevicePermissionDescriptor
/// </summary>
[ECMAScript]
[Description("@#CameraDevicePermissionDescriptor")]
public record CameraDevicePermissionDescriptor(
    [property: Description("@#panTiltZoom")]bool PanTiltZoom = false): PermissionDescriptor;

/// <summary>
/// MediaStream
/// </summary>
[ECMAScript]
[Description("@#MediaStream")]
public class MediaStream : EventTarget
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern MediaStream();

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="stream">stream</param>
    public extern MediaStream(MediaStream stream);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="tracks">tracks</param>
    public extern MediaStream(MediaStreamTrack[] tracks);

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; }

    /// <summary>
    /// getAudioTracks
    /// </summary>
    [Description("@#getAudioTracks")]
    public extern MediaStreamTrack[] GetAudioTracks();

    /// <summary>
    /// getVideoTracks
    /// </summary>
    [Description("@#getVideoTracks")]
    public extern MediaStreamTrack[] GetVideoTracks();

    /// <summary>
    /// getTracks
    /// </summary>
    [Description("@#getTracks")]
    public extern MediaStreamTrack[] GetTracks();

    /// <summary>
    /// getTrackById
    /// </summary>
    /// <param name="trackId">trackId</param>
    [Description("@#getTrackById")]
    public extern MediaStreamTrack? GetTrackById(string trackId);

    /// <summary>
    /// addTrack
    /// </summary>
    /// <param name="track">track</param>
    [Description("@#addTrack")]
    public extern void AddTrack(MediaStreamTrack track);

    /// <summary>
    /// removeTrack
    /// </summary>
    /// <param name="track">track</param>
    [Description("@#removeTrack")]
    public extern void RemoveTrack(MediaStreamTrack track);

    /// <summary>
    /// clone
    /// </summary>
    [Description("@#clone")]
    public extern MediaStream Clone();

    /// <summary>
    /// active
    /// </summary>
    [Description("@#active")]
    public extern bool Active { get; }

    /// <summary>
    /// onaddtrack
    /// </summary>
    [Description("@#onaddtrack")]
    public extern EventHandler Onaddtrack { get; set; }

    /// <summary>
    /// onremovetrack
    /// </summary>
    [Description("@#onremovetrack")]
    public extern EventHandler Onremovetrack { get; set; }
}

/// <summary>
/// MediaStreamTrackEvent
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackEvent")]
public class MediaStreamTrackEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MediaStreamTrackEvent(string type, MediaStreamTrackEventInit eventInitDict);

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern MediaStreamTrack Track { get; }
}

/// <summary>
/// OverconstrainedError
/// </summary>
[ECMAScript]
[Description("@#OverconstrainedError")]
public class OverconstrainedError : DOMException
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="constraint">constraint</param>
    /// <param name="message">message</param>
    public extern OverconstrainedError(string constraint, string message);

    /// <summary>
    /// constraint
    /// </summary>
    [Description("@#constraint")]
    public extern string Constraint { get; }
}

/// <summary>
/// MediaDeviceInfo
/// </summary>
[ECMAScript]
[Description("@#MediaDeviceInfo")]
public class MediaDeviceInfo
{
    /// <summary>
    /// deviceId
    /// </summary>
    [Description("@#deviceId")]
    public extern string DeviceId { get; }

    /// <summary>
    /// kind
    /// </summary>
    [Description("@#kind")]
    public extern MediaDeviceKind Kind { get; }

    /// <summary>
    /// label
    /// </summary>
    [Description("@#label")]
    public extern string Label { get; }

    /// <summary>
    /// groupId
    /// </summary>
    [Description("@#groupId")]
    public extern string GroupId { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// InputDeviceInfo
/// </summary>
[ECMAScript]
[Description("@#InputDeviceInfo")]
public class InputDeviceInfo : MediaDeviceInfo
{
    /// <summary>
    /// getCapabilities
    /// </summary>
    [Description("@#getCapabilities")]
    public extern MediaTrackCapabilities GetCapabilities();
}

/// <summary>
/// DeviceChangeEvent
/// </summary>
[ECMAScript]
[Description("@#DeviceChangeEvent")]
public class DeviceChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern DeviceChangeEvent(string type, DeviceChangeEventInit eventInitDict);

    /// <summary>
    /// devices
    /// </summary>
    [Description("@#devices")]
    public extern FrozenSet<MediaDeviceInfo> Devices { get; }

    /// <summary>
    /// userInsertedDevices
    /// </summary>
    [Description("@#userInsertedDevices")]
    public extern FrozenSet<MediaDeviceInfo> UserInsertedDevices { get; }
}