namespace ECMAScript;

/// <summary>
/// PressureUpdateCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PressureUpdateCallback(PressureRecord[] changes, PressureObserver observer);

/// <summary>
/// ViewTransitionUpdateCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> ViewTransitionUpdateCallback();

/// <summary>
/// MutationCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void MutationCallback(MutationRecord[] mutations, MutationObserver observer);

/// <summary>
/// EventListener
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void HandleEventCallback(Event @event);

/// <summary>
/// EventListener
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class EventListenerLiteral
{
    /// <summary>
    /// handleEvent
    /// </summary>
    [ECMAScript]
    [Description("@#handleEvent")]
    public HandleEventCallback? HandleEvent { get; set; }
}

/// <summary>
/// NodeFilter
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate ushort AcceptNodeCallback(Node node);

/// <summary>
/// NodeFilter
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class NodeFilterLiteral
{
    /// <summary>
    /// FILTER_ACCEPT
    /// </summary>
    [Description("@#FILTER_ACCEPT")]
    public const ushort FILTER_ACCEPT = 1;

    /// <summary>
    /// FILTER_REJECT
    /// </summary>
    [Description("@#FILTER_REJECT")]
    public const ushort FILTER_REJECT = 2;

    /// <summary>
    /// FILTER_SKIP
    /// </summary>
    [Description("@#FILTER_SKIP")]
    public const ushort FILTER_SKIP = 3;

    /// <summary>
    /// SHOW_ALL
    /// </summary>
    [Description("@#SHOW_ALL")]
    public const uint SHOW_ALL = 0xFFFFFFFF;

    /// <summary>
    /// SHOW_ELEMENT
    /// </summary>
    [Description("@#SHOW_ELEMENT")]
    public const uint SHOW_ELEMENT = 0x1;

    /// <summary>
    /// SHOW_ATTRIBUTE
    /// </summary>
    [Description("@#SHOW_ATTRIBUTE")]
    public const uint SHOW_ATTRIBUTE = 0x2;

    /// <summary>
    /// SHOW_TEXT
    /// </summary>
    [Description("@#SHOW_TEXT")]
    public const uint SHOW_TEXT = 0x4;

    /// <summary>
    /// SHOW_CDATA_SECTION
    /// </summary>
    [Description("@#SHOW_CDATA_SECTION")]
    public const uint SHOW_CDATA_SECTION = 0x8;

    /// <summary>
    /// SHOW_ENTITY_REFERENCE
    /// </summary>
    [Description("@#SHOW_ENTITY_REFERENCE")]
    public const uint SHOW_ENTITY_REFERENCE = 0x10;

    /// <summary>
    /// SHOW_ENTITY
    /// </summary>
    [Description("@#SHOW_ENTITY")]
    public const uint SHOW_ENTITY = 0x20;

    /// <summary>
    /// SHOW_PROCESSING_INSTRUCTION
    /// </summary>
    [Description("@#SHOW_PROCESSING_INSTRUCTION")]
    public const uint SHOW_PROCESSING_INSTRUCTION = 0x40;

    /// <summary>
    /// SHOW_COMMENT
    /// </summary>
    [Description("@#SHOW_COMMENT")]
    public const uint SHOW_COMMENT = 0x80;

    /// <summary>
    /// SHOW_DOCUMENT
    /// </summary>
    [Description("@#SHOW_DOCUMENT")]
    public const uint SHOW_DOCUMENT = 0x100;

    /// <summary>
    /// SHOW_DOCUMENT_TYPE
    /// </summary>
    [Description("@#SHOW_DOCUMENT_TYPE")]
    public const uint SHOW_DOCUMENT_TYPE = 0x200;

    /// <summary>
    /// SHOW_DOCUMENT_FRAGMENT
    /// </summary>
    [Description("@#SHOW_DOCUMENT_FRAGMENT")]
    public const uint SHOW_DOCUMENT_FRAGMENT = 0x400;

    /// <summary>
    /// SHOW_NOTATION
    /// </summary>
    [Description("@#SHOW_NOTATION")]
    public const uint SHOW_NOTATION = 0x800;

    /// <summary>
    /// acceptNode
    /// </summary>
    [ECMAScript]
    [Description("@#acceptNode")]
    public AcceptNodeCallback? AcceptNode { get; set; }
}

/// <summary>
/// XPathNSResolver
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? LookupNamespaceURICallback(string? prefix);

/// <summary>
/// XPathNSResolver
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class XPathNSResolverLiteral
{
    /// <summary>
    /// lookupNamespaceURI
    /// </summary>
    [ECMAScript]
    [Description("@#lookupNamespaceURI")]
    public LookupNamespaceURICallback? LookupNamespaceURI { get; set; }
}

/// <summary>
/// ErrorCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ErrorCallback(DOMException err);

/// <summary>
/// FileSystemEntryCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileSystemEntryCallback(FileSystemEntry entry);

/// <summary>
/// FileSystemEntriesCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileSystemEntriesCallback(FileSystemEntry[] entries);

/// <summary>
/// FileCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileCallback(File file);

/// <summary>
/// PositionCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PositionCallback(GeolocationPosition position);

/// <summary>
/// PositionErrorCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PositionErrorCallback(GeolocationPositionError positionError);

/// <summary>
/// BlobCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void BlobCallback(Blob? blob);

/// <summary>
/// CustomElementConstructor
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate HTMLElement CustomElementConstructor();

/// <summary>
/// FunctionStringCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FunctionStringCallback(string data);

/// <summary>
/// NavigationInterceptHandler
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> NavigationInterceptHandler();

/// <summary>
/// EventHandlerNonNull
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object EventHandlerNonNull(Event @event);

/// <summary>
/// OnErrorEventHandlerNonNull
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object OnErrorEventHandlerNonNull(Either<Event, string> @event, string source, uint lineno, uint colno, object error);

/// <summary>
/// OnBeforeUnloadEventHandlerNonNull
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? OnBeforeUnloadEventHandlerNonNull(Event @event);

/// <summary>
/// FrameRequestCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FrameRequestCallback(double time);

/// <summary>
/// IntersectionObserverCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void IntersectionObserverCallback(IntersectionObserverEntry[] entries, IntersectionObserver observer);

/// <summary>
/// MediaSessionActionHandler
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void MediaSessionActionHandler(MediaSessionActionDetails details);

/// <summary>
/// NotificationPermissionCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void NotificationPermissionCallback(NotificationPermission permission);

/// <summary>
/// SubscribeCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void SubscribeCallback(Subscriber subscriber);

/// <summary>
/// ObservableSubscriptionCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ObservableSubscriptionCallback(object value);

/// <summary>
/// ObservableInspectorAbortHandler
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ObservableInspectorAbortHandler(object value);

/// <summary>
/// Predicate
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate bool Predicate(object value, ulong index);

/// <summary>
/// Reducer
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Reducer(object accumulator, object currentValue, ulong index);

/// <summary>
/// Mapper
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Mapper(object value, ulong index);

/// <summary>
/// Visitor
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void Visitor(object value, ulong index);

/// <summary>
/// CatchCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object CatchCallback(object value);

/// <summary>
/// PerformanceObserverCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PerformanceObserverCallback(PerformanceObserverEntryList entries, PerformanceObserver observer, PerformanceObserverCallbackOptions options);

/// <summary>
/// RemotePlaybackAvailabilityCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RemotePlaybackAvailabilityCallback(bool available);

/// <summary>
/// ReportingObserverCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ReportingObserverCallback(Report[] reports, ReportingObserver observer);

/// <summary>
/// IdleRequestCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void IdleRequestCallback(IdleDeadline deadline);

/// <summary>
/// ResizeObserverCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ResizeObserverCallback(ResizeObserverEntry[] entries, ResizeObserver observer);

/// <summary>
/// SchedulerPostTaskCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object SchedulerPostTaskCallback();

/// <summary>
/// RunFunctionForSharedStorageSelectURLOperation
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<uint> RunFunctionForSharedStorageSelectURLOperation(string[] urls, object data);

/// <summary>
/// UnderlyingSourceStartCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object UnderlyingSourceStartCallback(ReadableStreamController controller);

/// <summary>
/// UnderlyingSourcePullCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> UnderlyingSourcePullCallback(ReadableStreamController controller);

/// <summary>
/// UnderlyingSourceCancelCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> UnderlyingSourceCancelCallback(object reason);

/// <summary>
/// UnderlyingSinkStartCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object UnderlyingSinkStartCallback(WritableStreamDefaultController controller);

/// <summary>
/// UnderlyingSinkWriteCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> UnderlyingSinkWriteCallback(object chunk, WritableStreamDefaultController controller);

/// <summary>
/// UnderlyingSinkCloseCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> UnderlyingSinkCloseCallback();

/// <summary>
/// UnderlyingSinkAbortCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> UnderlyingSinkAbortCallback(object reason);

/// <summary>
/// TransformerStartCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object TransformerStartCallback(TransformStreamDefaultController controller);

/// <summary>
/// TransformerFlushCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> TransformerFlushCallback(TransformStreamDefaultController controller);

/// <summary>
/// TransformerTransformCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> TransformerTransformCallback(object chunk, TransformStreamDefaultController controller);

/// <summary>
/// TransformerCancelCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> TransformerCancelCallback(object reason);

/// <summary>
/// QueuingStrategySize
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate double QueuingStrategySize(object chunk);

/// <summary>
/// CreateHTMLCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateHTMLCallback(string input, object arguments);

/// <summary>
/// CreateScriptCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateScriptCallback(string input, object arguments);

/// <summary>
/// CreateScriptURLCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateScriptURLCallback(string input, object arguments);

/// <summary>
/// VideoFrameRequestCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VideoFrameRequestCallback(double now, VideoFrameCallbackMetadata metadata);

/// <summary>
/// EffectCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EffectCallback(double? progress, Either<Element, CSSPseudoElement> currentTarget, Animation animation);

/// <summary>
/// LaunchConsumer
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object LaunchConsumer(LaunchParams @params);

/// <summary>
/// LockGrantedCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> LockGrantedCallback(Lock? @lock);

/// <summary>
/// DecodeErrorCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void DecodeErrorCallback(DOMException error);

/// <summary>
/// DecodeSuccessCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void DecodeSuccessCallback(AudioBuffer decodedData);

/// <summary>
/// AudioWorkletProcessorConstructor
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate AudioWorkletProcessor AudioWorkletProcessorConstructor(object options);

/// <summary>
/// AudioWorkletProcessCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate bool AudioWorkletProcessCallback(FrozenSet<FrozenSet<Float32Array>> inputs, FrozenSet<FrozenSet<Float32Array>> outputs, object parameters);

/// <summary>
/// AudioDataOutputCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void AudioDataOutputCallback(AudioData output);

/// <summary>
/// VideoFrameOutputCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VideoFrameOutputCallback(VideoFrame output);

/// <summary>
/// EncodedAudioChunkOutputCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EncodedAudioChunkOutputCallback(EncodedAudioChunk output, EncodedAudioChunkMetadata metadata);

/// <summary>
/// EncodedVideoChunkOutputCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EncodedVideoChunkOutputCallback(EncodedVideoChunk chunk, EncodedVideoChunkMetadata metadata);

/// <summary>
/// WebCodecsErrorCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void WebCodecsErrorCallback(DOMException error);

/// <summary>
/// Function
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Function(object arguments);

/// <summary>
/// VoidFunction
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VoidFunction();

/// <summary>
/// GenerateAssertionCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<RTCIdentityAssertionResult> GenerateAssertionCallback(string contents, string origin, RTCIdentityProviderOptions options);

/// <summary>
/// ValidateAssertionCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<RTCIdentityValidationResult> ValidateAssertionCallback(string assertion, string origin);

/// <summary>
/// RTCPeerConnectionErrorCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RTCPeerConnectionErrorCallback(DOMException error);

/// <summary>
/// RTCSessionDescriptionCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RTCSessionDescriptionCallback(RTCSessionDescriptionInit description);

/// <summary>
/// XRFrameRequestCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void XRFrameRequestCallback(double time, XRFrame frame);

/// <summary>
/// CreateMonitorCallback
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void CreateMonitorCallback(CreateMonitor monitor);