namespace ECMAScript;

/// <summary>
/// VTTCue
/// </summary>
[ECMAScript]
[Description("@#VTTCue")]
public class VTTCue : TextTrackCue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="startTime">startTime</param>
    /// <param name="endTime">endTime</param>
    /// <param name="text">text</param>
    public extern VTTCue(double startTime, double endTime, string text);

    /// <summary>
    /// region
    /// </summary>
    [Description("@#region")]
    public extern VTTRegion? Region { get; set; }

    /// <summary>
    /// vertical
    /// </summary>
    [Description("@#vertical")]
    public extern DirectionSetting Vertical { get; set; }

    /// <summary>
    /// snapToLines
    /// </summary>
    [Description("@#snapToLines")]
    public extern bool SnapToLines { get; set; }

    /// <summary>
    /// line
    /// </summary>
    [Description("@#line")]
    public extern LineAndPositionSetting Line { get; set; }

    /// <summary>
    /// lineAlign
    /// </summary>
    [Description("@#lineAlign")]
    public extern LineAlignSetting LineAlign { get; set; }

    /// <summary>
    /// position
    /// </summary>
    [Description("@#position")]
    public extern LineAndPositionSetting Position { get; set; }

    /// <summary>
    /// positionAlign
    /// </summary>
    [Description("@#positionAlign")]
    public extern PositionAlignSetting PositionAlign { get; set; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern double Size { get; set; }

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern AlignSetting Align { get; set; }

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }

    /// <summary>
    /// getCueAsHTML
    /// </summary>
    [Description("@#getCueAsHTML")]
    public extern DocumentFragment GetCueAsHTML();
}

/// <summary>
/// VTTRegion
/// </summary>
[ECMAScript]
[Description("@#VTTRegion")]
public class VTTRegion
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern VTTRegion();

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern double Width { get; set; }

    /// <summary>
    /// lines
    /// </summary>
    [Description("@#lines")]
    public extern uint Lines { get; set; }

    /// <summary>
    /// regionAnchorX
    /// </summary>
    [Description("@#regionAnchorX")]
    public extern double RegionAnchorX { get; set; }

    /// <summary>
    /// regionAnchorY
    /// </summary>
    [Description("@#regionAnchorY")]
    public extern double RegionAnchorY { get; set; }

    /// <summary>
    /// viewportAnchorX
    /// </summary>
    [Description("@#viewportAnchorX")]
    public extern double ViewportAnchorX { get; set; }

    /// <summary>
    /// viewportAnchorY
    /// </summary>
    [Description("@#viewportAnchorY")]
    public extern double ViewportAnchorY { get; set; }

    /// <summary>
    /// scroll
    /// </summary>
    [Description("@#scroll")]
    public extern ScrollSetting Scroll { get; set; }
}