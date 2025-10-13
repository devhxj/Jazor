namespace ECMAScript;

/// <summary>
/// TransitionEventInit
/// </summary>
[ECMAScript]
[Description("@#TransitionEventInit")]
public record TransitionEventInit(
    [property: Description("@#propertyName")]string? PropertyName = default,
	[property: Description("@#elapsedTime")]double ElapsedTime = 0.0d,
	[property: Description("@#pseudoElement")]string? PseudoElement = default): EventInit;

/// <summary>
/// TransitionEvent
/// </summary>
[ECMAScript]
[Description("@#TransitionEvent")]
public class TransitionEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="transitionEventInitDict">transitionEventInitDict</param>
    public extern TransitionEvent(string type, TransitionEventInit transitionEventInitDict);

    /// <summary>
    /// propertyName
    /// </summary>
    [Description("@#propertyName")]
    public extern string PropertyName { get; }

    /// <summary>
    /// elapsedTime
    /// </summary>
    [Description("@#elapsedTime")]
    public extern double ElapsedTime { get; }

    /// <summary>
    /// pseudoElement
    /// </summary>
    [Description("@#pseudoElement")]
    public extern string PseudoElement { get; }
}