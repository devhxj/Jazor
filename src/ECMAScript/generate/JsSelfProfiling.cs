namespace ECMAScript;

/// <summary>
/// ProfilerTrace
/// </summary>
[ECMAScript]
[Description("@#ProfilerTrace")]
public record ProfilerTrace(
    [property: Description("@#resources")]ProfilerResource[]? Resources = default,
	[property: Description("@#frames")]ProfilerFrame[]? Frames = default,
	[property: Description("@#stacks")]ProfilerStack[]? Stacks = default,
	[property: Description("@#samples")]ProfilerSample[]? Samples = default);

/// <summary>
/// ProfilerSample
/// </summary>
[ECMAScript]
[Description("@#ProfilerSample")]
public record ProfilerSample(
    [property: Description("@#timestamp")]double Timestamp = default,
	[property: Description("@#stackId")]ulong StackId = default);

/// <summary>
/// ProfilerStack
/// </summary>
[ECMAScript]
[Description("@#ProfilerStack")]
public record ProfilerStack(
    [property: Description("@#parentId")]ulong ParentId = default,
	[property: Description("@#frameId")]ulong FrameId = default);

/// <summary>
/// ProfilerFrame
/// </summary>
[ECMAScript]
[Description("@#ProfilerFrame")]
public record ProfilerFrame(
    [property: Description("@#name")]string? Name = default,
	[property: Description("@#resourceId")]ulong ResourceId = default,
	[property: Description("@#line")]ulong Line = default,
	[property: Description("@#column")]ulong Column = default);

/// <summary>
/// ProfilerInitOptions
/// </summary>
[ECMAScript]
[Description("@#ProfilerInitOptions")]
public record ProfilerInitOptions(
    [property: Description("@#sampleInterval")]double SampleInterval = default,
	[property: Description("@#maxBufferSize")]uint MaxBufferSize = default);

/// <summary>
/// Profiler
/// </summary>
[ECMAScript]
[Description("@#Profiler")]
public class Profiler : EventTarget
{
    /// <summary>
    /// sampleInterval
    /// </summary>
    [Description("@#sampleInterval")]
    public extern double SampleInterval { get; }

    /// <summary>
    /// stopped
    /// </summary>
    [Description("@#stopped")]
    public extern bool Stopped { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern Profiler(ProfilerInitOptions options);

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern PromiseResult<ProfilerTrace> Stop();
}