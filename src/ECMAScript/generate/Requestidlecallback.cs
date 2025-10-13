namespace ECMAScript;

/// <summary>
/// IdleRequestOptions
/// </summary>
[ECMAScript]
[Description("@#IdleRequestOptions")]
public record IdleRequestOptions(
    [property: Description("@#timeout")]uint Timeout = default);

/// <summary>
/// IdleDeadline
/// </summary>
[ECMAScript]
[Description("@#IdleDeadline")]
public class IdleDeadline
{
    /// <summary>
    /// timeRemaining
    /// </summary>
    [Description("@#timeRemaining")]
    public extern double TimeRemaining();

    /// <summary>
    /// didTimeout
    /// </summary>
    [Description("@#didTimeout")]
    public extern bool DidTimeout { get; }
}