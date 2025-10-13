namespace ECMAScript;

/// <summary>
/// HandwritingModelConstraint
/// </summary>
[ECMAScript]
[Description("@#HandwritingModelConstraint")]
public record HandwritingModelConstraint(
    [property: Description("@#languages")]string[]? Languages = default);

/// <summary>
/// HandwritingRecognizerQueryResult
/// </summary>
[ECMAScript]
[Description("@#HandwritingRecognizerQueryResult")]
public record HandwritingRecognizerQueryResult(
    [property: Description("@#textAlternatives")]bool TextAlternatives = default,
	[property: Description("@#textSegmentation")]bool TextSegmentation = default,
	[property: Description("@#hints")]HandwritingHintsQueryResult? Hints = default);

/// <summary>
/// HandwritingHintsQueryResult
/// </summary>
[ECMAScript]
[Description("@#HandwritingHintsQueryResult")]
public record HandwritingHintsQueryResult(
    [property: Description("@#recognitionType")]HandwritingRecognitionType[]? RecognitionType = default,
	[property: Description("@#inputType")]HandwritingInputType[]? InputType = default,
	[property: Description("@#textContext")]bool TextContext = default,
	[property: Description("@#alternatives")]bool Alternatives = default);

/// <summary>
/// HandwritingHints
/// </summary>
[ECMAScript]
[Description("@#HandwritingHints")]
public record HandwritingHints(
    [property: Description("@#recognitionType")]string? RecognitionType = default,
	[property: Description("@#inputType")]string? InputType = default,
	[property: Description("@#textContext")]string? TextContext = default,
	[property: Description("@#alternatives")]uint Alternatives = 3);

/// <summary>
/// HandwritingPoint
/// </summary>
[ECMAScript]
[Description("@#HandwritingPoint")]
public record HandwritingPoint(
    [property: Description("@#x")]double X = default,
	[property: Description("@#y")]double Y = default,
	[property: Description("@#t")]double T = default);

/// <summary>
/// HandwritingPrediction
/// </summary>
[ECMAScript]
[Description("@#HandwritingPrediction")]
public record HandwritingPrediction(
    [property: Description("@#text")]string? Text = default,
	[property: Description("@#segmentationResult")]HandwritingSegment[]? SegmentationResult = default);

/// <summary>
/// HandwritingSegment
/// </summary>
[ECMAScript]
[Description("@#HandwritingSegment")]
public record HandwritingSegment(
    [property: Description("@#grapheme")]string? Grapheme = default,
	[property: Description("@#beginIndex")]uint BeginIndex = default,
	[property: Description("@#endIndex")]uint EndIndex = default,
	[property: Description("@#drawingSegments")]HandwritingDrawingSegment[]? DrawingSegments = default);

/// <summary>
/// HandwritingDrawingSegment
/// </summary>
[ECMAScript]
[Description("@#HandwritingDrawingSegment")]
public record HandwritingDrawingSegment(
    [property: Description("@#strokeIndex")]uint StrokeIndex = default,
	[property: Description("@#beginPointIndex")]uint BeginPointIndex = default,
	[property: Description("@#endPointIndex")]uint EndPointIndex = default);

/// <summary>
/// HandwritingRecognizer
/// </summary>
[ECMAScript]
[Description("@#HandwritingRecognizer")]
public class HandwritingRecognizer
{
    /// <summary>
    /// startDrawing
    /// </summary>
    /// <param name="hints">hints</param>
    [Description("@#startDrawing")]
    public extern HandwritingDrawing StartDrawing(HandwritingHints? hints = default);

    /// <summary>
    /// finish
    /// </summary>
    [Description("@#finish")]
    public extern void Finish();
}

/// <summary>
/// HandwritingDrawing
/// </summary>
[ECMAScript]
[Description("@#HandwritingDrawing")]
public class HandwritingDrawing
{
    /// <summary>
    /// addStroke
    /// </summary>
    /// <param name="stroke">stroke</param>
    [Description("@#addStroke")]
    public extern void AddStroke(HandwritingStroke stroke);

    /// <summary>
    /// removeStroke
    /// </summary>
    /// <param name="stroke">stroke</param>
    [Description("@#removeStroke")]
    public extern void RemoveStroke(HandwritingStroke stroke);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// getStrokes
    /// </summary>
    [Description("@#getStrokes")]
    public extern HandwritingStroke[] GetStrokes();

    /// <summary>
    /// getPrediction
    /// </summary>
    [Description("@#getPrediction")]
    public extern PromiseResult<HandwritingPrediction[]> GetPrediction();
}

/// <summary>
/// HandwritingStroke
/// </summary>
[ECMAScript]
[Description("@#HandwritingStroke")]
public class HandwritingStroke
{
    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HandwritingStroke();

    /// <summary>
    /// addPoint
    /// </summary>
    /// <param name="point">point</param>
    [Description("@#addPoint")]
    public extern void AddPoint(HandwritingPoint point);

    /// <summary>
    /// getPoints
    /// </summary>
    [Description("@#getPoints")]
    public extern HandwritingPoint[] GetPoints();

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();
}