namespace ECMAScript;

/// <summary>
/// InkPresenterParam
/// </summary>
[ECMAScript]
[Description("@#InkPresenterParam")]
public record InkPresenterParam(
    [property: Description("@#presentationArea")]Element? PresentationArea = null);

/// <summary>
/// InkTrailStyle
/// </summary>
[ECMAScript]
[Description("@#InkTrailStyle")]
public record InkTrailStyle(
    [property: Description("@#color")]string? Color = default,
	[property: Description("@#diameter")]double Diameter = default);

/// <summary>
/// Ink
/// </summary>
[ECMAScript]
[Description("@#Ink")]
public class Ink
{
    /// <summary>
    /// requestPresenter
    /// </summary>
    /// <param name="param">param</param>
    [Description("@#requestPresenter")]
    public extern PromiseResult<DelegatedInkTrailPresenter> RequestPresenter(InkPresenterParam? param = default);
}

/// <summary>
/// DelegatedInkTrailPresenter
/// </summary>
[ECMAScript]
[Description("@#DelegatedInkTrailPresenter")]
public class DelegatedInkTrailPresenter
{
    /// <summary>
    /// presentationArea
    /// </summary>
    [Description("@#presentationArea")]
    public extern Element? PresentationArea { get; }

    /// <summary>
    /// updateInkTrailStartPoint
    /// </summary>
    /// <param name="event">event</param>
    /// <param name="style">style</param>
    [Description("@#updateInkTrailStartPoint")]
    public extern void UpdateInkTrailStartPoint(PointerEvent @event, InkTrailStyle style);
}