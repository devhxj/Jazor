namespace ECMAScript;

/// <summary>
/// MediaMetadataInit
/// </summary>
[ECMAScript]
[Description("@#MediaMetadataInit")]
public record MediaMetadataInit(
    [property: Description("@#title")]string? Title = default,
	[property: Description("@#artist")]string? Artist = default,
	[property: Description("@#album")]string? Album = default,
	[property: Description("@#artwork")]MediaImage[]? Artwork = default,
	[property: Description("@#chapterInfo")]ChapterInformationInit[]? ChapterInfo = default);

/// <summary>
/// ChapterInformationInit
/// </summary>
[ECMAScript]
[Description("@#ChapterInformationInit")]
public record ChapterInformationInit(
    [property: Description("@#title")]string? Title = default,
	[property: Description("@#startTime")]double StartTime = 0d,
	[property: Description("@#artwork")]MediaImage[]? Artwork = default);

/// <summary>
/// MediaImage
/// </summary>
[ECMAScript]
[Description("@#MediaImage")]
public record MediaImage(
    [property: Description("@#src")]string? Src = default,
	[property: Description("@#sizes")]string? Sizes = default,
	[property: Description("@#type")]string? Type = default);

/// <summary>
/// MediaPositionState
/// </summary>
[ECMAScript]
[Description("@#MediaPositionState")]
public record MediaPositionState(
    [property: Description("@#duration")]double Duration = default,
	[property: Description("@#playbackRate")]double PlaybackRate = default,
	[property: Description("@#position")]double Position = default);

/// <summary>
/// MediaSessionActionDetails
/// </summary>
[ECMAScript]
[Description("@#MediaSessionActionDetails")]
public record MediaSessionActionDetails(
    [property: Description("@#action")]MediaSessionAction? Action = default,
	[property: Description("@#seekOffset")]double SeekOffset = default,
	[property: Description("@#seekTime")]double SeekTime = default,
	[property: Description("@#fastSeek")]bool FastSeek = default,
	[property: Description("@#isActivating")]bool IsActivating = default);

/// <summary>
/// MediaSession
/// </summary>
[ECMAScript]
[Description("@#MediaSession")]
public class MediaSession
{
    /// <summary>
    /// metadata
    /// </summary>
    [Description("@#metadata")]
    public extern MediaMetadata? Metadata { get; set; }

    /// <summary>
    /// playbackState
    /// </summary>
    [Description("@#playbackState")]
    public extern MediaSessionPlaybackState PlaybackState { get; set; }

    /// <summary>
    /// setActionHandler
    /// </summary>
    /// <param name="action">action</param>
    /// <param name="handler">handler</param>
    [Description("@#setActionHandler")]
    public extern void SetActionHandler(MediaSessionAction action, MediaSessionActionHandler? handler);

    /// <summary>
    /// setPositionState
    /// </summary>
    /// <param name="state">state</param>
    [Description("@#setPositionState")]
    public extern void SetPositionState(MediaPositionState? state = default);

    /// <summary>
    /// setMicrophoneActive
    /// </summary>
    /// <param name="active">active</param>
    [Description("@#setMicrophoneActive")]
    public extern PromiseResult<object> SetMicrophoneActive(bool active);

    /// <summary>
    /// setCameraActive
    /// </summary>
    /// <param name="active">active</param>
    [Description("@#setCameraActive")]
    public extern PromiseResult<object> SetCameraActive(bool active);

    /// <summary>
    /// setScreenshareActive
    /// </summary>
    /// <param name="active">active</param>
    [Description("@#setScreenshareActive")]
    public extern PromiseResult<object> SetScreenshareActive(bool active);
}

/// <summary>
/// MediaMetadata
/// </summary>
[ECMAScript]
[Description("@#MediaMetadata")]
public class MediaMetadata
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern MediaMetadata(MediaMetadataInit init);

    /// <summary>
    /// title
    /// </summary>
    [Description("@#title")]
    public extern string Title { get; set; }

    /// <summary>
    /// artist
    /// </summary>
    [Description("@#artist")]
    public extern string Artist { get; set; }

    /// <summary>
    /// album
    /// </summary>
    [Description("@#album")]
    public extern string Album { get; set; }

    /// <summary>
    /// artwork
    /// </summary>
    [Description("@#artwork")]
    public extern FrozenSet<object> Artwork { get; set; }

    /// <summary>
    /// chapterInfo
    /// </summary>
    [Description("@#chapterInfo")]
    public extern FrozenSet<ChapterInformation> ChapterInfo { get; }
}

/// <summary>
/// ChapterInformation
/// </summary>
[ECMAScript]
[Description("@#ChapterInformation")]
public class ChapterInformation
{
    /// <summary>
    /// title
    /// </summary>
    [Description("@#title")]
    public extern string Title { get; }

    /// <summary>
    /// startTime
    /// </summary>
    [Description("@#startTime")]
    public extern double StartTime { get; }

    /// <summary>
    /// artwork
    /// </summary>
    [Description("@#artwork")]
    public extern FrozenSet<MediaImage> Artwork { get; }
}