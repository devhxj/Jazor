namespace ECMAScript;

/// <summary>
/// PromptResponseObject
/// </summary>
[ECMAScript]
[Description("@#PromptResponseObject")]
public record PromptResponseObject(
    [property: Description("@#userChoice")]AppBannerPromptOutcome? UserChoice = default);

/// <summary>
/// BeforeInstallPromptEvent
/// </summary>
[ECMAScript]
[Description("@#BeforeInstallPromptEvent")]
public class BeforeInstallPromptEvent : Event
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern BeforeInstallPromptEvent(string type, EventInit eventInitDict);

    /// <summary>
    /// prompt
    /// </summary>
    [Description("@#prompt")]
    public extern PromiseResult<PromptResponseObject> Prompt();
}