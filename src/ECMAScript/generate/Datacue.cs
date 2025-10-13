namespace ECMAScript;

/// <summary>
/// DataCue
/// </summary>
[ECMAScript]
[Description("@#DataCue")]
public class DataCue : TextTrackCue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="startTime">startTime</param>
    /// <param name="endTime">endTime</param>
    /// <param name="value">value</param>
    /// <param name="type">type</param>
    public extern DataCue(double startTime, double endTime, object value, string type);

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern object Value { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }
}