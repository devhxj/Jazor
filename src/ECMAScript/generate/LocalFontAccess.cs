namespace ECMAScript;

/// <summary>
/// QueryOptions
/// </summary>
[ECMAScript]
[Description("@#QueryOptions")]
public record QueryOptions(
    [property: Description("@#postscriptNames")]string[]? PostscriptNames = default);

/// <summary>
/// FontData
/// </summary>
[ECMAScript]
[Description("@#FontData")]
public class FontData
{
    /// <summary>
    /// blob
    /// </summary>
    [Description("@#blob")]
    public extern PromiseResult<Blob> Blob();

    /// <summary>
    /// postscriptName
    /// </summary>
    [Description("@#postscriptName")]
    public extern string PostscriptName { get; }

    /// <summary>
    /// fullName
    /// </summary>
    [Description("@#fullName")]
    public extern string FullName { get; }

    /// <summary>
    /// family
    /// </summary>
    [Description("@#family")]
    public extern string Family { get; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern string Style { get; }
}