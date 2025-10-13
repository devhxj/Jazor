namespace ECMAScript;

/// <summary>
/// VideoPlaybackQuality
/// </summary>
[ECMAScript]
[Description("@#VideoPlaybackQuality")]
public class VideoPlaybackQuality
{
    /// <summary>
    /// creationTime
    /// </summary>
    [Description("@#creationTime")]
    public extern double CreationTime { get; }

    /// <summary>
    /// droppedVideoFrames
    /// </summary>
    [Description("@#droppedVideoFrames")]
    public extern uint DroppedVideoFrames { get; }

    /// <summary>
    /// totalVideoFrames
    /// </summary>
    [Description("@#totalVideoFrames")]
    public extern uint TotalVideoFrames { get; }

    /// <summary>
    /// corruptedVideoFrames
    /// </summary>
    [Description("@#corruptedVideoFrames")]
    public extern uint CorruptedVideoFrames { get; }
}