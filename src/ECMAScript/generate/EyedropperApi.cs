namespace ECMAScript;

/// <summary>
/// ColorSelectionResult
/// </summary>
[ECMAScript]
[Description("@#ColorSelectionResult")]
public record ColorSelectionResult(
    [property: Description("@#sRGBHex")]string? SRGBHex = default);

/// <summary>
/// ColorSelectionOptions
/// </summary>
[ECMAScript]
[Description("@#ColorSelectionOptions")]
public record ColorSelectionOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// EyeDropper
/// </summary>
[ECMAScript]
[Description("@#EyeDropper")]
public class EyeDropper
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern EyeDropper();

    /// <summary>
    /// open
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#open")]
    public extern PromiseResult<ColorSelectionResult> Open(ColorSelectionOptions? options = default);
}