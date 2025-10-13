namespace ECMAScript;

/// <summary>
/// PerformanceNavigationTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceNavigationTiming")]
public partial class PerformanceNavigationTiming : PerformanceResourceTiming
{
    /// <summary>
    /// unloadEventStart
    /// </summary>
    [Description("@#unloadEventStart")]
    public extern double UnloadEventStart { get; }

    /// <summary>
    /// unloadEventEnd
    /// </summary>
    [Description("@#unloadEventEnd")]
    public extern double UnloadEventEnd { get; }

    /// <summary>
    /// domInteractive
    /// </summary>
    [Description("@#domInteractive")]
    public extern double DomInteractive { get; }

    /// <summary>
    /// domContentLoadedEventStart
    /// </summary>
    [Description("@#domContentLoadedEventStart")]
    public extern double DomContentLoadedEventStart { get; }

    /// <summary>
    /// domContentLoadedEventEnd
    /// </summary>
    [Description("@#domContentLoadedEventEnd")]
    public extern double DomContentLoadedEventEnd { get; }

    /// <summary>
    /// domComplete
    /// </summary>
    [Description("@#domComplete")]
    public extern double DomComplete { get; }

    /// <summary>
    /// loadEventStart
    /// </summary>
    [Description("@#loadEventStart")]
    public extern double LoadEventStart { get; }

    /// <summary>
    /// loadEventEnd
    /// </summary>
    [Description("@#loadEventEnd")]
    public extern double LoadEventEnd { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern NavigationTimingType Type { get; }

    /// <summary>
    /// redirectCount
    /// </summary>
    [Description("@#redirectCount")]
    public extern ushort RedirectCount { get; }

    /// <summary>
    /// criticalCHRestart
    /// </summary>
    [Description("@#criticalCHRestart")]
    public extern double CriticalCHRestart { get; }

    /// <summary>
    /// notRestoredReasons
    /// </summary>
    [Description("@#notRestoredReasons")]
    public extern NotRestoredReasons? NotRestoredReasons { get; }

    /// <summary>
    /// activationStart
    /// </summary>
    [Description("@#activationStart")]
    public extern double ActivationStart { get; }
}

/// <summary>
/// PerformanceTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceTiming")]
public class PerformanceTiming
{
    /// <summary>
    /// navigationStart
    /// </summary>
    [Description("@#navigationStart")]
    public extern ulong NavigationStart { get; }

    /// <summary>
    /// unloadEventStart
    /// </summary>
    [Description("@#unloadEventStart")]
    public extern ulong UnloadEventStart { get; }

    /// <summary>
    /// unloadEventEnd
    /// </summary>
    [Description("@#unloadEventEnd")]
    public extern ulong UnloadEventEnd { get; }

    /// <summary>
    /// redirectStart
    /// </summary>
    [Description("@#redirectStart")]
    public extern ulong RedirectStart { get; }

    /// <summary>
    /// redirectEnd
    /// </summary>
    [Description("@#redirectEnd")]
    public extern ulong RedirectEnd { get; }

    /// <summary>
    /// fetchStart
    /// </summary>
    [Description("@#fetchStart")]
    public extern ulong FetchStart { get; }

    /// <summary>
    /// domainLookupStart
    /// </summary>
    [Description("@#domainLookupStart")]
    public extern ulong DomainLookupStart { get; }

    /// <summary>
    /// domainLookupEnd
    /// </summary>
    [Description("@#domainLookupEnd")]
    public extern ulong DomainLookupEnd { get; }

    /// <summary>
    /// connectStart
    /// </summary>
    [Description("@#connectStart")]
    public extern ulong ConnectStart { get; }

    /// <summary>
    /// connectEnd
    /// </summary>
    [Description("@#connectEnd")]
    public extern ulong ConnectEnd { get; }

    /// <summary>
    /// secureConnectionStart
    /// </summary>
    [Description("@#secureConnectionStart")]
    public extern ulong SecureConnectionStart { get; }

    /// <summary>
    /// requestStart
    /// </summary>
    [Description("@#requestStart")]
    public extern ulong RequestStart { get; }

    /// <summary>
    /// responseStart
    /// </summary>
    [Description("@#responseStart")]
    public extern ulong ResponseStart { get; }

    /// <summary>
    /// responseEnd
    /// </summary>
    [Description("@#responseEnd")]
    public extern ulong ResponseEnd { get; }

    /// <summary>
    /// domLoading
    /// </summary>
    [Description("@#domLoading")]
    public extern ulong DomLoading { get; }

    /// <summary>
    /// domInteractive
    /// </summary>
    [Description("@#domInteractive")]
    public extern ulong DomInteractive { get; }

    /// <summary>
    /// domContentLoadedEventStart
    /// </summary>
    [Description("@#domContentLoadedEventStart")]
    public extern ulong DomContentLoadedEventStart { get; }

    /// <summary>
    /// domContentLoadedEventEnd
    /// </summary>
    [Description("@#domContentLoadedEventEnd")]
    public extern ulong DomContentLoadedEventEnd { get; }

    /// <summary>
    /// domComplete
    /// </summary>
    [Description("@#domComplete")]
    public extern ulong DomComplete { get; }

    /// <summary>
    /// loadEventStart
    /// </summary>
    [Description("@#loadEventStart")]
    public extern ulong LoadEventStart { get; }

    /// <summary>
    /// loadEventEnd
    /// </summary>
    [Description("@#loadEventEnd")]
    public extern ulong LoadEventEnd { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}

/// <summary>
/// PerformanceNavigation
/// </summary>
[ECMAScript]
[Description("@#PerformanceNavigation")]
public class PerformanceNavigation
{
    /// <summary>
    /// TYPE_NAVIGATE
    /// </summary>
    [Description("@#TYPE_NAVIGATE")]
    public const ushort TYPE_NAVIGATE = 0;

    /// <summary>
    /// TYPE_RELOAD
    /// </summary>
    [Description("@#TYPE_RELOAD")]
    public const ushort TYPE_RELOAD = 1;

    /// <summary>
    /// TYPE_BACK_FORWARD
    /// </summary>
    [Description("@#TYPE_BACK_FORWARD")]
    public const ushort TYPE_BACK_FORWARD = 2;

    /// <summary>
    /// TYPE_RESERVED
    /// </summary>
    [Description("@#TYPE_RESERVED")]
    public const ushort TYPE_RESERVED = 255;

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern ushort Type { get; }

    /// <summary>
    /// redirectCount
    /// </summary>
    [Description("@#redirectCount")]
    public extern ushort RedirectCount { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}