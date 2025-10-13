namespace ECMAScript;

/// <summary>
/// CanvasCaptureMediaStreamTrack
/// </summary>
[ECMAScript]
[Description("@#CanvasCaptureMediaStreamTrack")]
public class CanvasCaptureMediaStreamTrack : MediaStreamTrack
{
    /// <summary>
    /// canvas
    /// </summary>
    [Description("@#canvas")]
    public extern HTMLCanvasElement Canvas { get; }

    /// <summary>
    /// requestFrame
    /// </summary>
    [Description("@#requestFrame")]
    public extern void RequestFrame();
}