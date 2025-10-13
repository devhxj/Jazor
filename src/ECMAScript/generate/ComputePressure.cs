namespace ECMAScript;

/// <summary>
/// PressureObserverOptions
/// </summary>
[ECMAScript]
[Description("@#PressureObserverOptions")]
public record PressureObserverOptions(
    [property: Description("@#sampleInterval")]uint SampleInterval = 0);

/// <summary>
/// PressureObserver
/// </summary>
[ECMAScript]
[Description("@#PressureObserver")]
public class PressureObserver
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="callback">callback</param>
    public extern PressureObserver(PressureUpdateCallback callback);

    /// <summary>
    /// observe
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="options">options</param>
    [Description("@#observe")]
    public extern PromiseResult<object> Observe(PressureSource source, PressureObserverOptions? options = default);

    /// <summary>
    /// unobserve
    /// </summary>
    /// <param name="source">source</param>
    [Description("@#unobserve")]
    public extern void Unobserve(PressureSource source);

    /// <summary>
    /// disconnect
    /// </summary>
    [Description("@#disconnect")]
    public extern void Disconnect();

    /// <summary>
    /// takeRecords
    /// </summary>
    [Description("@#takeRecords")]
    public extern PressureRecord[] TakeRecords();

    /// <summary>
    /// knownSources
    /// </summary>
    [Description("@#knownSources")]
    public extern static FrozenSet<PressureSource> KnownSources { get; }
}

/// <summary>
/// PressureRecord
/// </summary>
[ECMAScript]
[Description("@#PressureRecord")]
public class PressureRecord
{
    /// <summary>
    /// source
    /// </summary>
    [Description("@#source")]
    public extern PressureSource Source { get; }

    /// <summary>
    /// state
    /// </summary>
    [Description("@#state")]
    public extern PressureState State { get; }

    /// <summary>
    /// time
    /// </summary>
    [Description("@#time")]
    public extern double Time { get; }

    /// <summary>
    /// toJSON
    /// </summary>
    [Description("@#toJSON")]
    public extern object ToJSON();
}