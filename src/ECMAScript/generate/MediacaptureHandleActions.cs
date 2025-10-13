namespace ECMAScript;

/// <summary>
/// CaptureActionEventInit
/// </summary>
[ECMAScript]
[Description("@#CaptureActionEventInit")]
public record CaptureActionEventInit(
    [property: Description("@#action")]string? Action = default): EventInit;

/// <summary>
/// CaptureActionEvent
/// </summary>
[ECMAScript]
[Description("@#CaptureActionEvent")]
public class CaptureActionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="init">init</param>
    public extern CaptureActionEvent(CaptureActionEventInit init);

    /// <summary>
    /// action
    /// </summary>
    [Description("@#action")]
    public extern CaptureAction Action { get; }
}