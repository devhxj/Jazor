namespace ECMAScript;

/// <summary>
/// PerformanceLongAnimationFrameTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceLongAnimationFrameTiming")]
public class PerformanceLongAnimationFrameTiming : PerformanceEntry
{
    /// <summary>
    /// renderStart
    /// </summary>
    [Description("@#renderStart")]
    public extern double RenderStart { get; }

    /// <summary>
    /// styleAndLayoutStart
    /// </summary>
    [Description("@#styleAndLayoutStart")]
    public extern double StyleAndLayoutStart { get; }

    /// <summary>
    /// blockingDuration
    /// </summary>
    [Description("@#blockingDuration")]
    public extern double BlockingDuration { get; }

    /// <summary>
    /// firstUIEventTimestamp
    /// </summary>
    [Description("@#firstUIEventTimestamp")]
    public extern double FirstUIEventTimestamp { get; }

    /// <summary>
    /// scripts
    /// </summary>
    [Description("@#scripts")]
    public extern FrozenSet<PerformanceScriptTiming> Scripts { get; }

    #region mixin PaintTimingMixin
    /// <summary>
    /// paintTime
    /// </summary>
    [Description("@#paintTime")]
    public extern double PaintTime { get; }

    /// <summary>
    /// presentationTime
    /// </summary>
    [Description("@#presentationTime")]
    public extern double? PresentationTime { get; }
    #endregion
}

/// <summary>
/// PerformanceScriptTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceScriptTiming")]
public class PerformanceScriptTiming : PerformanceEntry
{
    /// <summary>
    /// invokerType
    /// </summary>
    [Description("@#invokerType")]
    public extern ScriptInvokerType InvokerType { get; }

    /// <summary>
    /// invoker
    /// </summary>
    [Description("@#invoker")]
    public extern string Invoker { get; }

    /// <summary>
    /// executionStart
    /// </summary>
    [Description("@#executionStart")]
    public extern double ExecutionStart { get; }

    /// <summary>
    /// sourceURL
    /// </summary>
    [Description("@#sourceURL")]
    public extern string SourceURL { get; }

    /// <summary>
    /// sourceFunctionName
    /// </summary>
    [Description("@#sourceFunctionName")]
    public extern string SourceFunctionName { get; }

    /// <summary>
    /// sourceCharPosition
    /// </summary>
    [Description("@#sourceCharPosition")]
    public extern long SourceCharPosition { get; }

    /// <summary>
    /// pauseDuration
    /// </summary>
    [Description("@#pauseDuration")]
    public extern double PauseDuration { get; }

    /// <summary>
    /// forcedStyleAndLayoutDuration
    /// </summary>
    [Description("@#forcedStyleAndLayoutDuration")]
    public extern double ForcedStyleAndLayoutDuration { get; }

    /// <summary>
    /// window
    /// </summary>
    [Description("@#window")]
    public extern Window? Window { get; }

    /// <summary>
    /// windowAttribution
    /// </summary>
    [Description("@#windowAttribution")]
    public extern ScriptWindowAttribution WindowAttribution { get; }
}