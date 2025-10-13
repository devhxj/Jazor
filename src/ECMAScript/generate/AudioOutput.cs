namespace ECMAScript;

/// <summary>
/// AudioOutputOptions
/// </summary>
[ECMAScript]
[Description("@#AudioOutputOptions")]
public record AudioOutputOptions(
    [property: Description("@#deviceId")]string? DeviceId = default);

/// <summary>
/// HTMLMediaElement
/// </summary>
[ECMAScript]
[Description("@#HTMLMediaElement")]
public partial class HTMLMediaElement : HTMLElement
{
    /// <summary>
    /// sinkId
    /// </summary>
    [Description("@#sinkId")]
    public extern string SinkId { get; }

    /// <summary>
    /// setSinkId
    /// </summary>
    /// <param name="sinkId">sinkId</param>
    [Description("@#setSinkId")]
    public extern PromiseResult<object> SetSinkId(string sinkId);

    /// <summary>
    /// mediaKeys
    /// </summary>
    [Description("@#mediaKeys")]
    public extern MediaKeys? MediaKeys { get; }

    /// <summary>
    /// onencrypted
    /// </summary>
    [Description("@#onencrypted")]
    public extern EventHandler Onencrypted { get; set; }

    /// <summary>
    /// onwaitingforkey
    /// </summary>
    [Description("@#onwaitingforkey")]
    public extern EventHandler Onwaitingforkey { get; set; }

    /// <summary>
    /// setMediaKeys
    /// </summary>
    /// <param name="mediaKeys">mediaKeys</param>
    [Description("@#setMediaKeys")]
    public extern PromiseResult<object> SetMediaKeys(MediaKeys? mediaKeys);

    /// <summary>
    /// error
    /// </summary>
    [Description("@#error")]
    public extern MediaError? Error { get; }

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// srcObject
    /// </summary>
    [Description("@#srcObject")]
    public extern MediaProvider? SrcObject { get; set; }

    /// <summary>
    /// currentSrc
    /// </summary>
    [Description("@#currentSrc")]
    public extern string CurrentSrc { get; }

    /// <summary>
    /// crossOrigin
    /// </summary>
    [Description("@#crossOrigin")]
    public extern string? CrossOrigin { get; set; }

    /// <summary>
    /// NETWORK_EMPTY
    /// </summary>
    [Description("@#NETWORK_EMPTY")]
    public const ushort NETWORK_EMPTY = 0;

    /// <summary>
    /// NETWORK_IDLE
    /// </summary>
    [Description("@#NETWORK_IDLE")]
    public const ushort NETWORK_IDLE = 1;

    /// <summary>
    /// NETWORK_LOADING
    /// </summary>
    [Description("@#NETWORK_LOADING")]
    public const ushort NETWORK_LOADING = 2;

    /// <summary>
    /// NETWORK_NO_SOURCE
    /// </summary>
    [Description("@#NETWORK_NO_SOURCE")]
    public const ushort NETWORK_NO_SOURCE = 3;

    /// <summary>
    /// networkState
    /// </summary>
    [Description("@#networkState")]
    public extern ushort NetworkState { get; }

    /// <summary>
    /// preload
    /// </summary>
    [Description("@#preload")]
    public extern string Preload { get; set; }

    /// <summary>
    /// buffered
    /// </summary>
    [Description("@#buffered")]
    public extern TimeRanges Buffered { get; }

    /// <summary>
    /// load
    /// </summary>
    [Description("@#load")]
    public extern void Load();

    /// <summary>
    /// canPlayType
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#canPlayType")]
    public extern CanPlayTypeResult CanPlayType(string type);

    /// <summary>
    /// HAVE_NOTHING
    /// </summary>
    [Description("@#HAVE_NOTHING")]
    public const ushort HAVE_NOTHING = 0;

    /// <summary>
    /// HAVE_METADATA
    /// </summary>
    [Description("@#HAVE_METADATA")]
    public const ushort HAVE_METADATA = 1;

    /// <summary>
    /// HAVE_CURRENT_DATA
    /// </summary>
    [Description("@#HAVE_CURRENT_DATA")]
    public const ushort HAVE_CURRENT_DATA = 2;

    /// <summary>
    /// HAVE_FUTURE_DATA
    /// </summary>
    [Description("@#HAVE_FUTURE_DATA")]
    public const ushort HAVE_FUTURE_DATA = 3;

    /// <summary>
    /// HAVE_ENOUGH_DATA
    /// </summary>
    [Description("@#HAVE_ENOUGH_DATA")]
    public const ushort HAVE_ENOUGH_DATA = 4;

    /// <summary>
    /// readyState
    /// </summary>
    [Description("@#readyState")]
    public extern ushort ReadyState { get; }

    /// <summary>
    /// seeking
    /// </summary>
    [Description("@#seeking")]
    public extern bool Seeking { get; }

    /// <summary>
    /// currentTime
    /// </summary>
    [Description("@#currentTime")]
    public extern double CurrentTime { get; set; }

    /// <summary>
    /// fastSeek
    /// </summary>
    /// <param name="time">time</param>
    [Description("@#fastSeek")]
    public extern void FastSeek(double time);

    /// <summary>
    /// duration
    /// </summary>
    [Description("@#duration")]
    public extern double Duration { get; }

    /// <summary>
    /// getStartDate
    /// </summary>
    [Description("@#getStartDate")]
    public extern object GetStartDate();

    /// <summary>
    /// paused
    /// </summary>
    [Description("@#paused")]
    public extern bool Paused { get; }

    /// <summary>
    /// defaultPlaybackRate
    /// </summary>
    [Description("@#defaultPlaybackRate")]
    public extern double DefaultPlaybackRate { get; set; }

    /// <summary>
    /// playbackRate
    /// </summary>
    [Description("@#playbackRate")]
    public extern double PlaybackRate { get; set; }

    /// <summary>
    /// preservesPitch
    /// </summary>
    [Description("@#preservesPitch")]
    public extern bool PreservesPitch { get; set; }

    /// <summary>
    /// played
    /// </summary>
    [Description("@#played")]
    public extern TimeRanges Played { get; }

    /// <summary>
    /// seekable
    /// </summary>
    [Description("@#seekable")]
    public extern TimeRanges Seekable { get; }

    /// <summary>
    /// ended
    /// </summary>
    [Description("@#ended")]
    public extern bool Ended { get; }

    /// <summary>
    /// autoplay
    /// </summary>
    [Description("@#autoplay")]
    public extern bool Autoplay { get; set; }

    /// <summary>
    /// loop
    /// </summary>
    [Description("@#loop")]
    public extern bool Loop { get; set; }

    /// <summary>
    /// play
    /// </summary>
    [Description("@#play")]
    public extern PromiseResult<object> Play();

    /// <summary>
    /// pause
    /// </summary>
    [Description("@#pause")]
    public extern void Pause();

    /// <summary>
    /// controls
    /// </summary>
    [Description("@#controls")]
    public extern bool Controls { get; set; }

    /// <summary>
    /// volume
    /// </summary>
    [Description("@#volume")]
    public extern double Volume { get; set; }

    /// <summary>
    /// muted
    /// </summary>
    [Description("@#muted")]
    public extern bool Muted { get; set; }

    /// <summary>
    /// defaultMuted
    /// </summary>
    [Description("@#defaultMuted")]
    public extern bool DefaultMuted { get; set; }

    /// <summary>
    /// audioTracks
    /// </summary>
    [Description("@#audioTracks")]
    public extern AudioTrackList AudioTracks { get; }

    /// <summary>
    /// videoTracks
    /// </summary>
    [Description("@#videoTracks")]
    public extern VideoTrackList VideoTracks { get; }

    /// <summary>
    /// textTracks
    /// </summary>
    [Description("@#textTracks")]
    public extern TextTrackList TextTracks { get; }

    /// <summary>
    /// addTextTrack
    /// </summary>
    /// <param name="kind">kind</param>
    /// <param name="label">label</param>
    /// <param name="language">language</param>
    [Description("@#addTextTrack")]
    public extern TextTrack AddTextTrack(TextTrackKind kind, string? label = default, string? language = default);

    /// <summary>
    /// captureStream
    /// </summary>
    [Description("@#captureStream")]
    public extern MediaStream CaptureStream();

    /// <summary>
    /// remote
    /// </summary>
    [Description("@#remote")]
    public extern RemotePlayback Remote { get; }

    /// <summary>
    /// disableRemotePlayback
    /// </summary>
    [Description("@#disableRemotePlayback")]
    public extern bool DisableRemotePlayback { get; set; }
}

/// <summary>
/// MediaDevices
/// </summary>
[ECMAScript]
[Description("@#MediaDevices")]
public partial class MediaDevices : EventTarget
{
    /// <summary>
    /// selectAudioOutput
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#selectAudioOutput")]
    public extern PromiseResult<MediaDeviceInfo> SelectAudioOutput(AudioOutputOptions? options = default);

    /// <summary>
    /// setCaptureHandleConfig
    /// </summary>
    /// <param name="config">config</param>
    [Description("@#setCaptureHandleConfig")]
    public extern void SetCaptureHandleConfig(CaptureHandleConfig? config = default);

    /// <summary>
    /// setSupportedCaptureActions
    /// </summary>
    /// <param name="actions">actions</param>
    [Description("@#setSupportedCaptureActions")]
    public extern void SetSupportedCaptureActions(string[] actions);

    /// <summary>
    /// oncaptureaction
    /// </summary>
    [Description("@#oncaptureaction")]
    public extern EventHandler Oncaptureaction { get; set; }

    /// <summary>
    /// ondevicechange
    /// </summary>
    [Description("@#ondevicechange")]
    public extern EventHandler Ondevicechange { get; set; }

    /// <summary>
    /// enumerateDevices
    /// </summary>
    [Description("@#enumerateDevices")]
    public extern PromiseResult<MediaDeviceInfo[]> EnumerateDevices();

    /// <summary>
    /// getSupportedConstraints
    /// </summary>
    [Description("@#getSupportedConstraints")]
    public extern MediaTrackSupportedConstraints GetSupportedConstraints();

    /// <summary>
    /// getUserMedia
    /// </summary>
    /// <param name="constraints">constraints</param>
    [Description("@#getUserMedia")]
    public extern PromiseResult<MediaStream> GetUserMedia(MediaStreamConstraints? constraints = default);

    /// <summary>
    /// getViewportMedia
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getViewportMedia")]
    public extern PromiseResult<MediaStream> GetViewportMedia(DisplayMediaStreamOptions? options = default);

    /// <summary>
    /// getDisplayMedia
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getDisplayMedia")]
    public extern PromiseResult<MediaStream> GetDisplayMedia(DisplayMediaStreamOptions? options = default);
}