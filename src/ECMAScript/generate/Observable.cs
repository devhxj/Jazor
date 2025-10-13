namespace ECMAScript;

/// <summary>
/// SubscriptionObserver
/// </summary>
[ECMAScript]
[Description("@#SubscriptionObserver")]
public record SubscriptionObserver(
    [property: Description("@#next")]ObservableSubscriptionCallback? Next = default,
	[property: Description("@#error")]ObservableSubscriptionCallback? Error = default,
	[property: Description("@#complete")]Action? Complete = default);

/// <summary>
/// ObservableInspector
/// </summary>
[ECMAScript]
[Description("@#ObservableInspector")]
public record ObservableInspector(
    [property: Description("@#next")]ObservableSubscriptionCallback? Next = default,
	[property: Description("@#error")]ObservableSubscriptionCallback? Error = default,
	[property: Description("@#complete")]Action? Complete = default,
	[property: Description("@#subscribe")]Action? Subscribe = default,
	[property: Description("@#abort")]ObservableInspectorAbortHandler? Abort = default);

/// <summary>
/// SubscribeOptions
/// </summary>
[ECMAScript]
[Description("@#SubscribeOptions")]
public record SubscribeOptions(
    [property: Description("@#signal")]AbortSignal? Signal = default);

/// <summary>
/// ObservableEventListenerOptions
/// </summary>
[ECMAScript]
[Description("@#ObservableEventListenerOptions")]
public record ObservableEventListenerOptions(
    [property: Description("@#capture")]bool Capture = false,
	[property: Description("@#passive")]bool Passive = default);

/// <summary>
/// Subscriber
/// </summary>
[ECMAScript]
[Description("@#Subscriber")]
public class Subscriber
{
    /// <summary>
    /// next
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#next")]
    public extern void Next(object value);

    /// <summary>
    /// error
    /// </summary>
    /// <param name="error">error</param>
    [Description("@#error")]
    public extern void Error(object error);

    /// <summary>
    /// complete
    /// </summary>
    [Description("@#complete")]
    public extern void Complete();

    /// <summary>
    /// addTeardown
    /// </summary>
    /// <param name="teardown">teardown</param>
    [Description("@#addTeardown")]
    public extern void AddTeardown(Action teardown);

    /// <summary>
    /// active
    /// </summary>
    [Description("@#active")]
    public extern bool Active { get; }

    /// <summary>
    /// signal
    /// </summary>
    [Description("@#signal")]
    public extern AbortSignal Signal { get; }
}

/// <summary>
/// Observable
/// </summary>
[ECMAScript]
[Description("@#Observable")]
public class Observable
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="callback">callback</param>
    public extern Observable(SubscribeCallback callback);

    /// <summary>
    /// subscribe
    /// </summary>
    /// <param name="observer">observer</param>
    /// <param name="options">options</param>
    [Description("@#subscribe")]
    public extern void Subscribe(ObserverUnion? observer = default, SubscribeOptions? options = default);

    /// <summary>
    /// from
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#from")]
    public extern static Observable From(object value);

    /// <summary>
    /// takeUntil
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#takeUntil")]
    public extern Observable TakeUntil(object value);

    /// <summary>
    /// map
    /// </summary>
    /// <param name="mapper">mapper</param>
    [Description("@#map")]
    public extern Observable Map(Mapper mapper);

    /// <summary>
    /// filter
    /// </summary>
    /// <param name="predicate">predicate</param>
    [Description("@#filter")]
    public extern Observable Filter(Predicate predicate);

    /// <summary>
    /// take
    /// </summary>
    /// <param name="amount">amount</param>
    [Description("@#take")]
    public extern Observable Take(ulong amount);

    /// <summary>
    /// drop
    /// </summary>
    /// <param name="amount">amount</param>
    [Description("@#drop")]
    public extern Observable Drop(ulong amount);

    /// <summary>
    /// flatMap
    /// </summary>
    /// <param name="mapper">mapper</param>
    [Description("@#flatMap")]
    public extern Observable FlatMap(Mapper mapper);

    /// <summary>
    /// switchMap
    /// </summary>
    /// <param name="mapper">mapper</param>
    [Description("@#switchMap")]
    public extern Observable SwitchMap(Mapper mapper);

    /// <summary>
    /// inspect
    /// </summary>
    /// <param name="inspectorUnion">inspectorUnion</param>
    [Description("@#inspect")]
    public extern Observable Inspect(ObservableInspectorUnion? inspectorUnion = default);

    /// <summary>
    /// catch
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#catch")]
    public extern Observable Catch(CatchCallback callback);

    /// <summary>
    /// finally
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#finally")]
    public extern Observable Finally(Action callback);

    /// <summary>
    /// toArray
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#toArray")]
    public extern PromiseResult<object[]> ToArray(SubscribeOptions? options = default);

    /// <summary>
    /// forEach
    /// </summary>
    /// <param name="callback">callback</param>
    /// <param name="options">options</param>
    [Description("@#forEach")]
    public extern PromiseResult<object> ForEach(Visitor callback, SubscribeOptions? options = default);

    /// <summary>
    /// every
    /// </summary>
    /// <param name="predicate">predicate</param>
    /// <param name="options">options</param>
    [Description("@#every")]
    public extern PromiseResult<bool> Every(Predicate predicate, SubscribeOptions? options = default);

    /// <summary>
    /// first
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#first")]
    public extern PromiseResult<object> First(SubscribeOptions? options = default);

    /// <summary>
    /// last
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#last")]
    public extern PromiseResult<object> Last(SubscribeOptions? options = default);

    /// <summary>
    /// find
    /// </summary>
    /// <param name="predicate">predicate</param>
    /// <param name="options">options</param>
    [Description("@#find")]
    public extern PromiseResult<object> Find(Predicate predicate, SubscribeOptions? options = default);

    /// <summary>
    /// some
    /// </summary>
    /// <param name="predicate">predicate</param>
    /// <param name="options">options</param>
    [Description("@#some")]
    public extern PromiseResult<bool> Some(Predicate predicate, SubscribeOptions? options = default);

    /// <summary>
    /// reduce
    /// </summary>
    /// <param name="reducer">reducer</param>
    /// <param name="initialValue">initialValue</param>
    /// <param name="options">options</param>
    [Description("@#reduce")]
    public extern PromiseResult<object> Reduce(Reducer reducer, object initialValue, SubscribeOptions? options = default);
}