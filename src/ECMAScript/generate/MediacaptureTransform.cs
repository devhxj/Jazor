namespace ECMAScript;

/// <summary>
/// MediaStreamTrackProcessorInit
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackProcessorInit")]
public record MediaStreamTrackProcessorInit(
    [property: Description("@#track")]MediaStreamTrack? Track = default,
	[property: Description("@#maxBufferSize")]ushort MaxBufferSize = default);

/// <summary>
/// MediaStreamTrackProcessor
/// </summary>
[ECMAScript]
[Description("@#MediaStreamTrackProcessor")]
public class MediaStreamTrackProcessor
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern MediaStreamTrackProcessor(MediaStreamTrackProcessorInit init);

    /// <summary>
    /// readable
    /// </summary>
    [Description("@#readable")]
    public extern ReadableStream Readable { get; }
}

/// <summary>
/// VideoTrackGenerator
/// </summary>
[ECMAScript]
[Description("@#VideoTrackGenerator")]
public class VideoTrackGenerator
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern VideoTrackGenerator();

    /// <summary>
    /// writable
    /// </summary>
    [Description("@#writable")]
    public extern WritableStream Writable { get; }

    /// <summary>
    /// muted
    /// </summary>
    [Description("@#muted")]
    public extern bool Muted { get; set; }

    /// <summary>
    /// track
    /// </summary>
    [Description("@#track")]
    public extern MediaStreamTrack Track { get; }
}