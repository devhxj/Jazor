namespace ECMAScript;

/// <summary>
/// IntersectionObserverEntryInit
/// </summary>
[ECMAScript]
[Description("@#IntersectionObserverEntryInit")]
public record IntersectionObserverEntryInit(
    [property: Description("@#time")]double Time = default,
	[property: Description("@#rootBounds")]DOMRectInit? RootBounds = default,
	[property: Description("@#boundingClientRect")]DOMRectInit? BoundingClientRect = default,
	[property: Description("@#intersectionRect")]DOMRectInit? IntersectionRect = default,
	[property: Description("@#isIntersecting")]bool IsIntersecting = default,
	[property: Description("@#isVisible")]bool IsVisible = default,
	[property: Description("@#intersectionRatio")]double IntersectionRatio = default,
	[property: Description("@#target")]Element? Target = default);

/// <summary>
/// IntersectionObserverInit
/// </summary>
[ECMAScript]
[Description("@#IntersectionObserverInit")]
public record IntersectionObserverInit(
    [property: Description("@#root")]Either<Element, Document>? Root = default,
	[property: Description("@#rootMargin")]string? RootMargin = default,
	[property: Description("@#scrollMargin")]string? ScrollMargin = default,
	[property: Description("@#threshold")]Either<double, double[]>? Threshold = default,
	[property: Description("@#delay")]int Delay = 0,
	[property: Description("@#trackVisibility")]bool TrackVisibility = false);

/// <summary>
/// IntersectionObserver
/// </summary>
[ECMAScript]
[Description("@#IntersectionObserver")]
public class IntersectionObserver
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="callback">callback</param>
    /// <param name="options">options</param>
    public extern IntersectionObserver(IntersectionObserverCallback callback, IntersectionObserverInit options);

    /// <summary>
    /// root
    /// </summary>
    [Description("@#root")]
    public extern Either<Element, Document>? Root { get; }

    /// <summary>
    /// rootMargin
    /// </summary>
    [Description("@#rootMargin")]
    public extern string RootMargin { get; }

    /// <summary>
    /// scrollMargin
    /// </summary>
    [Description("@#scrollMargin")]
    public extern string ScrollMargin { get; }

    /// <summary>
    /// thresholds
    /// </summary>
    [Description("@#thresholds")]
    public extern FrozenSet<double> Thresholds { get; }

    /// <summary>
    /// delay
    /// </summary>
    [Description("@#delay")]
    public extern int Delay { get; }

    /// <summary>
    /// trackVisibility
    /// </summary>
    [Description("@#trackVisibility")]
    public extern bool TrackVisibility { get; }

    /// <summary>
    /// observe
    /// </summary>
    /// <param name="target">target</param>
    [Description("@#observe")]
    public extern void Observe(Element target);

    /// <summary>
    /// unobserve
    /// </summary>
    /// <param name="target">target</param>
    [Description("@#unobserve")]
    public extern void Unobserve(Element target);

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();

    /// <summary>
    /// takeRecords
    /// </summary>
    [Description("@#takeRecords")]
    public extern IntersectionObserverEntry[] TakeRecords();
}

/// <summary>
/// IntersectionObserverEntry
/// </summary>
[ECMAScript]
[Description("@#IntersectionObserverEntry")]
public class IntersectionObserverEntry
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="intersectionObserverEntryInit">intersectionObserverEntryInit</param>
    public extern IntersectionObserverEntry(IntersectionObserverEntryInit intersectionObserverEntryInit);

    /// <summary>
    /// time
    /// </summary>
    [Description("@#time")]
    public extern double Time { get; }

    /// <summary>
    /// rootBounds
    /// </summary>
    [Description("@#rootBounds")]
    public extern DOMRectReadOnly? RootBounds { get; }

    /// <summary>
    /// boundingClientRect
    /// </summary>
    [Description("@#boundingClientRect")]
    public extern DOMRectReadOnly BoundingClientRect { get; }

    /// <summary>
    /// intersectionRect
    /// </summary>
    [Description("@#intersectionRect")]
    public extern DOMRectReadOnly IntersectionRect { get; }

    /// <summary>
    /// isIntersecting
    /// </summary>
    [Description("@#isIntersecting")]
    public extern bool IsIntersecting { get; }

    /// <summary>
    /// isVisible
    /// </summary>
    [Description("@#isVisible")]
    public extern bool IsVisible { get; }

    /// <summary>
    /// intersectionRatio
    /// </summary>
    [Description("@#intersectionRatio")]
    public extern double IntersectionRatio { get; }

    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern Element Target { get; }
}