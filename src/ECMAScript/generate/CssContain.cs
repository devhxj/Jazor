namespace ECMAScript;

/// <summary>
/// ContentVisibilityAutoStateChangeEventInit
/// </summary>
[ECMAScript]
[Description("@#ContentVisibilityAutoStateChangeEventInit")]
public record ContentVisibilityAutoStateChangeEventInit(
    [property: Description("@#skipped")]bool Skipped = false): EventInit;

/// <summary>
/// ContentVisibilityAutoStateChangeEvent
/// </summary>
[ECMAScript]
[Description("@#ContentVisibilityAutoStateChangeEvent")]
public class ContentVisibilityAutoStateChangeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern ContentVisibilityAutoStateChangeEvent(string type, ContentVisibilityAutoStateChangeEventInit eventInitDict);

    /// <summary>
    /// skipped
    /// </summary>
    [Description("@#skipped")]
    public extern bool Skipped { get; }
}