namespace ECMAScript;

/// <summary>
/// SnapEventInit
/// </summary>
[ECMAScript]
[Description("@#SnapEventInit")]
public record SnapEventInit(
    [property: Description("@#snapTargetBlock")]Node? SnapTargetBlock = default,
	[property: Description("@#snapTargetInline")]Node? SnapTargetInline = default): EventInit;

/// <summary>
/// SnapEvent
/// </summary>
[ECMAScript]
[Description("@#SnapEvent")]
public class SnapEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern SnapEvent(string type, SnapEventInit eventInitDict);

    /// <summary>
    /// snapTargetBlock
    /// </summary>
    [Description("@#snapTargetBlock")]
    public extern Node? SnapTargetBlock { get; }

    /// <summary>
    /// snapTargetInline
    /// </summary>
    [Description("@#snapTargetInline")]
    public extern Node? SnapTargetInline { get; }
}