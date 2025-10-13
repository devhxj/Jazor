namespace ECMAScript;

/// <summary>
/// PerformanceLongTaskTiming
/// </summary>
[ECMAScript]
[Description("@#PerformanceLongTaskTiming")]
public class PerformanceLongTaskTiming : PerformanceEntry
{
    /// <summary>
    /// attribution
    /// </summary>
    [Description("@#attribution")]
    public extern FrozenSet<TaskAttributionTiming> Attribution { get; }
}

/// <summary>
/// TaskAttributionTiming
/// </summary>
[ECMAScript]
[Description("@#TaskAttributionTiming")]
public class TaskAttributionTiming : PerformanceEntry
{
    /// <summary>
    /// containerType
    /// </summary>
    [Description("@#containerType")]
    public extern string ContainerType { get; }

    /// <summary>
    /// containerSrc
    /// </summary>
    [Description("@#containerSrc")]
    public extern string ContainerSrc { get; }

    /// <summary>
    /// containerId
    /// </summary>
    [Description("@#containerId")]
    public extern string ContainerId { get; }

    /// <summary>
    /// containerName
    /// </summary>
    [Description("@#containerName")]
    public extern string ContainerName { get; }
}