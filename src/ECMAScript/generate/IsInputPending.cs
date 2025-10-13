namespace ECMAScript;

/// <summary>
/// IsInputPendingOptions
/// </summary>
[ECMAScript]
[Description("@#IsInputPendingOptions")]
public record IsInputPendingOptions(
    [property: Description("@#includeContinuous")]bool IncludeContinuous = false);

/// <summary>
/// Scheduling
/// </summary>
[ECMAScript]
[Description("@#Scheduling")]
public class Scheduling
{
    /// <summary>
    /// isInputPending
    /// </summary>
    /// <param name="isInputPendingOptions">isInputPendingOptions</param>
    [Description("@#isInputPending")]
    public extern bool IsInputPending(IsInputPendingOptions? isInputPendingOptions = default);
}