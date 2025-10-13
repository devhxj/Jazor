namespace ECMAScript;

/// <summary>
/// DetectedText
/// </summary>
[ECMAScript]
[Description("@#DetectedText")]
public record DetectedText(
    [property: Description("@#boundingBox")]DOMRectReadOnly? BoundingBox = default,
	[property: Description("@#rawValue")]string? RawValue = default,
	[property: Description("@#cornerPoints")]Point2D[]? CornerPoints = default);

/// <summary>
/// TextDetector
/// </summary>
[ECMAScript]
[Description("@#TextDetector")]
public class TextDetector
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern TextDetector();

    /// <summary>
    /// detect
    /// </summary>
    /// <param name="image">image</param>
    [Description("@#detect")]
    public extern PromiseResult<DetectedText[]> Detect(ImageBitmapSource image);
}