namespace ECMAScript;

/// <summary>
/// FaceDetectorOptions
/// </summary>
[ECMAScript]
[Description("@#FaceDetectorOptions")]
public record FaceDetectorOptions(
    [property: Description("@#maxDetectedFaces")]ushort MaxDetectedFaces = default,
	[property: Description("@#fastMode")]bool FastMode = default);

/// <summary>
/// DetectedFace
/// </summary>
[ECMAScript]
[Description("@#DetectedFace")]
public record DetectedFace(
    [property: Description("@#boundingBox")]DOMRectReadOnly? BoundingBox = default,
	[property: Description("@#landmarks")]Landmark[]? Landmarks = default);

/// <summary>
/// Landmark
/// </summary>
[ECMAScript]
[Description("@#Landmark")]
public record Landmark(
    [property: Description("@#locations")]Point2D[]? Locations = default,
	[property: Description("@#type")]LandmarkType? Type = default);

/// <summary>
/// BarcodeDetectorOptions
/// </summary>
[ECMAScript]
[Description("@#BarcodeDetectorOptions")]
public record BarcodeDetectorOptions(
    [property: Description("@#formats")]BarcodeFormat[]? Formats = default);

/// <summary>
/// DetectedBarcode
/// </summary>
[ECMAScript]
[Description("@#DetectedBarcode")]
public record DetectedBarcode(
    [property: Description("@#boundingBox")]DOMRectReadOnly? BoundingBox = default,
	[property: Description("@#rawValue")]string? RawValue = default,
	[property: Description("@#format")]BarcodeFormat? Format = default,
	[property: Description("@#cornerPoints")]Point2D[]? CornerPoints = default);

/// <summary>
/// FaceDetector
/// </summary>
[ECMAScript]
[Description("@#FaceDetector")]
public class FaceDetector
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="faceDetectorOptions">faceDetectorOptions</param>
    public extern FaceDetector(FaceDetectorOptions faceDetectorOptions);

    /// <summary>
    /// detect
    /// </summary>
    /// <param name="image">image</param>
    [Description("@#detect")]
    public extern PromiseResult<DetectedFace[]> Detect(ImageBitmapSource image);
}

/// <summary>
/// BarcodeDetector
/// </summary>
[ECMAScript]
[Description("@#BarcodeDetector")]
public class BarcodeDetector
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="barcodeDetectorOptions">barcodeDetectorOptions</param>
    public extern BarcodeDetector(BarcodeDetectorOptions barcodeDetectorOptions);

    /// <summary>
    /// getSupportedFormats
    /// </summary>
    [Description("@#getSupportedFormats")]
    public extern static PromiseResult<BarcodeFormat[]> GetSupportedFormats();

    /// <summary>
    /// detect
    /// </summary>
    /// <param name="image">image</param>
    [Description("@#detect")]
    public extern PromiseResult<DetectedBarcode[]> Detect(ImageBitmapSource image);
}