namespace ECMAScript;

/// <summary>
/// <see href="https://dom.spec.whatwg.org/#callbackdef-mutationcallback">DOM Standard: 4.3.1 Interface MutationObserver</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void MutationCallback(MutationRecord[] mutations, MutationObserver observer);

/// <summary>
/// <see href="https://dom.spec.whatwg.org/#dom-eventlistener-handleevent">DOM Standard: 2.7 Interface EventTarget</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void HandleEventCallback(Event @event);

/// <summary>
/// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-acceptnode">DOM Standard: 6.3 Interface NodeFilter</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate ushort AcceptNodeCallback(Node node);

/// <summary>
/// <see href="https://dom.spec.whatwg.org/#dom-xpathnsresolver-lookupnamespaceuri">DOM Standard: 8.3 Mixin XPathEvaluatorBase</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? LookupNamespaceURICallback(string? prefix);

/// <summary>
/// <see href="https://drafts.csswg.org/css-view-transitions-2/#callbackdef-viewtransitionupdatecallback">CSS View Transitions Module Level 2: 2.1 Additions to Document</see>
/// </summary>
/// <example>
/// <code>viewTransition = document.startViewTransition(updateCallback)</code>
/// </example>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> ViewTransitionUpdateCallback();

/// <summary>
/// <see href="https://drafts.csswg.org/resize-observer-1/#callbackdef-resizeobservercallback">Resize Observer Module Level 1: 2.2 ResizeObserverCallback</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ResizeObserverCallback(ResizeObserverEntry[] entries, ResizeObserver observer);

/// <summary>
/// <see href="https://drafts.csswg.org/web-animations-2/#callbackdef-effectcallback">Web Animations Module Level 2: 4.12 The EffectCallback callback function</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EffectCallback(double? progress, ParameterCurrentTarget currentTarget, Animation animation);

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#blobcallback">HTML Standard: 4.12.5 The canvas element</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void BlobCallback(Blob? blob);

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/custom-elements.html#customelementconstructor">HTML Standard: 4.13.4 The CustomElementRegistry interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate HTMLElement CustomElementConstructor();

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/dnd.html#functionstringcallback">HTML Standard: 6.11.3.2 The DataTransferItem interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FunctionStringCallback(string data);

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#framerequestcallback">HTML Standard: 8.12 Animation frames</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FrameRequestCallback(double time);

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationintercepthandler">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
/// </summary>
/// <example>
/// <code>precommitController.addHandler(NavigationInterceptHandler handler)</code>
/// </example>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult NavigationInterceptHandler();

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationprecommithandler">HTML Standard: 7.2.6.10.2 The NavigationPrecommitController interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult NavigationPrecommitHandler(NavigationPrecommitController controller);

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/webappapis.html#eventhandlernonnull">HTML Standard: 8.1.8.1 Event handlers</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object EventHandlerNonNull(Event @event);

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/webappapis.html#onbeforeunloadeventhandlernonnull">HTML Standard: 8.1.8.1 Event handlers</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? OnBeforeUnloadEventHandlerNonNull(Event @event);

/// <summary>
/// <see href="https://html.spec.whatwg.org/multipage/webappapis.html#onerroreventhandlernonnull">HTML Standard: 8.1.8.1 Event handlers</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object OnErrorEventHandlerNonNull(ParameterEvent @event, string source, uint lineno, uint colno, object error);

/// <summary>
/// <see href="https://immersive-web.github.io/webxr/#callbackdef-xrframerequestcallback">WebXR Device API: 4.3 Animation Frames</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void XRFrameRequestCallback(double time, XRFrame frame);

/// <summary>
/// <see href="https://notifications.spec.whatwg.org/#callbackdef-notificationpermissioncallback">Notifications API Standard: 3 API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void NotificationPermissionCallback(NotificationPermission permission);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-queuingstrategysize">Streams Standard: 7.1 The queuing strategy API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate double QueuingStrategySize(object chunk);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformercancelcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult TransformerCancelCallback(object reason);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformerflushcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult TransformerFlushCallback(TransformStreamDefaultController controller);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformerstartcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object TransformerStartCallback(TransformStreamDefaultController controller);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformertransformcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult TransformerTransformCallback(object chunk, TransformStreamDefaultController controller);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkabortcallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSinkAbortCallback(object reason);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkclosecallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSinkCloseCallback();

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkstartcallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object UnderlyingSinkStartCallback(WritableStreamDefaultController controller);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkwritecallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSinkWriteCallback(object chunk, WritableStreamDefaultController controller);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsourcecancelcallback">Streams Standard: 4.2.3 The underlying source API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSourceCancelCallback(object reason);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsourcepullcallback">Streams Standard: 4.2.3 The underlying source API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSourcePullCallback(ReadableStreamController controller);

/// <summary>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsourcestartcallback">Streams Standard: 4.2.3 The underlying source API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object UnderlyingSourceStartCallback(ReadableStreamController controller);

/// <summary>
/// <see href="https://w3c.github.io/IntersectionObserver/#callbackdef-intersectionobservercallback">Intersection Observer: 2.1 The IntersectionObserverCallback</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void IntersectionObserverCallback(IntersectionObserverEntry[] entries, IntersectionObserver observer);

/// <summary>
/// <see href="https://w3c.github.io/geolocation/#dom-positioncallback">Geolocation: 6 Geolocation interface and callbacks</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PositionCallback(GeolocationPosition position);

/// <summary>
/// <see href="https://w3c.github.io/geolocation/#dom-positionerrorcallback">Geolocation: 6 Geolocation interface and callbacks</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PositionErrorCallback(GeolocationPositionError positionError);

/// <summary>
/// <see href="https://w3c.github.io/performance-timeline/#callbackdef-performanceobservercallback">Performance Timeline: 4 The PerformanceObserver interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PerformanceObserverCallback(PerformanceObserverEntryList entries, PerformanceObserver observer, PerformanceObserverCallbackOptions options);

/// <summary>
/// <see href="https://w3c.github.io/reporting/#callbackdef-reportingobservercallback">Reporting API: 4.1 Interface ReportingObserver</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ReportingObserverCallback(Report[] reports, ReportingObserver observer);

/// <summary>
/// <see href="https://w3c.github.io/requestidlecallback/#dom-idlerequestcallback">requestIdleCallback(): 4.1 The requestIdleCallback() method</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void IdleRequestCallback(IdleDeadline deadline);

/// <summary>
/// <see href="https://w3c.github.io/trusted-types/dist/spec/#callbackdef-createhtmlcallback">Trusted Types: 2.3.3 TrustedTypePolicyOptions</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateHTMLCallback(string input, object arguments);

/// <summary>
/// <see href="https://w3c.github.io/trusted-types/dist/spec/#callbackdef-createscriptcallback">Trusted Types: 2.3.3 TrustedTypePolicyOptions</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateScriptCallback(string input, object arguments);

/// <summary>
/// <see href="https://w3c.github.io/trusted-types/dist/spec/#callbackdef-createscripturlcallback">Trusted Types: 2.3.3 TrustedTypePolicyOptions</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateScriptURLCallback(string input, object arguments);

/// <summary>
/// <see href="https://w3c.github.io/web-locks/#callbackdef-lockgrantedcallback">Web Locks API: 3.2 LockManager class</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> LockGrantedCallback(Lock? @lock);

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-audiodataoutputcallback">WebCodecs: 3 AudioDecoder Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void AudioDataOutputCallback(AudioData output);

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-encodedaudiochunkoutputcallback">WebCodecs: 5 AudioEncoder Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EncodedAudioChunkOutputCallback(EncodedAudioChunk output, EncodedAudioChunkMetadata metadata);

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-encodedvideochunkoutputcallback">WebCodecs: 6 VideoEncoder Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EncodedVideoChunkOutputCallback(EncodedVideoChunk chunk, EncodedVideoChunkMetadata metadata);

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-videoframeoutputcallback">WebCodecs: 4 VideoDecoder Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VideoFrameOutputCallback(VideoFrame output);

/// <summary>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-webcodecserrorcallback">WebCodecs: 7.16 WebCodecsErrorCallback</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void WebCodecsErrorCallback(DOMException error);

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionerrorcallback">WebRTC: Real-Time Communication in Browsers: RTCPeerConnectionErrorCallback</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RTCPeerConnectionErrorCallback(DOMException error);

/// <summary>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsessiondescriptioncallback">WebRTC: Real-Time Communication in Browsers: RTCSessionDescriptionCallback</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RTCSessionDescriptionCallback(RTCSessionDescriptionInit description);

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#audioworkletprocess-callback-parameters">Web Audio API 1.1: 1.32.5.3.1 Callback AudioWorkletProcessCallback Parameters</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate bool AudioWorkletProcessCallback(FrozenSet<FrozenSet<Float32Array>> inputs, FrozenSet<FrozenSet<Float32Array>> outputs, object parameters);

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#callback-decodeerrorcallback-parameters">Web Audio API 1.1: 1.1.4 Callback DecodeErrorCallback() Parameters</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void DecodeErrorCallback(DOMException error);

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#callback-decodesuccesscallback-parameters">Web Audio API 1.1: 1.1.3 Callback DecodeSuccessCallback() Parameters</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void DecodeSuccessCallback(AudioBuffer decodedData);

/// <summary>
/// <see href="https://webaudio.github.io/web-audio-api/#callbackdef-audioworkletprocessorconstructor">Web Audio API 1.1: 1.32.3 The AudioWorkletGlobalScope Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate AudioWorkletProcessor AudioWorkletProcessorConstructor(object options);

/// <summary>
/// <see href="https://webidl.spec.whatwg.org/#Function">Web IDL Standard: 4.5 Function</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Function(object arguments);

/// <summary>
/// <see href="https://webidl.spec.whatwg.org/#VoidFunction">Web IDL Standard: 4.6 VoidFunction</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VoidFunction();

/// <summary>
/// <see href="https://webmachinelearning.github.io/prompt-api/#callbackdef-languagemodeltoolfunction">Prompt API: 3 The API</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<string> LanguageModelToolFunction(object arguments);

/// <summary>
/// <see href="https://webmachinelearning.github.io/webmcp/#callbackdef-toolexecutecallback">WebMCP: 4.2.1 ModelContextTool Dictionary</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> ToolExecuteCallback(object input);

/// <summary>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#callbackdef-createmonitorcallback">Writing Assistance APIs: 5.1 Common APIs</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void CreateMonitorCallback(CreateMonitor monitor);

/// <summary>
/// <see href="https://wicg.github.io/autofill-event/#callbackdef-refillcallback">Autofill Event: 3 The AutofillEvent Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult RefillCallback();

/// <summary>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-errorcallback">File and Directory Entries API: 7 Files and Directories</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ErrorCallback(DOMException err);

/// <summary>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-filecallback">File and Directory Entries API: 7.4 The FileSystemFileEntry Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileCallback(Files file);

/// <summary>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-filesystementriescallback">File and Directory Entries API: 7.3 The FileSystemDirectoryReader Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileSystemEntriesCallback(FileSystemEntry[] entries);

/// <summary>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-filesystementrycallback">File and Directory Entries API: 7.2 The FileSystemDirectoryEntry Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileSystemEntryCallback(FileSystemEntry entry);

/// <summary>
/// <see href="https://wicg.github.io/observable/#callbackdef-catchcallback">Observable: 2.2 The Observable interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object CatchCallback(object value);

/// <summary>
/// <see href="https://wicg.github.io/observable/#callbackdef-mapper">Observable: 2.2 The Observable interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Mapper(object value, ulong index);

/// <summary>
/// <see href="https://wicg.github.io/observable/#callbackdef-observableinspectoraborthandler">Observable: 2.2 The Observable interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ObservableInspectorAbortHandler(object value);

/// <summary>
/// <see href="https://wicg.github.io/observable/#callbackdef-predicate">Observable: 2.2 The Observable interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate bool Predicate(object value, ulong index);

/// <summary>
/// <see href="https://wicg.github.io/observable/#callbackdef-reducer">Observable: 2.2 The Observable interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Reducer(object accumulator, object currentValue, ulong index);

/// <summary>
/// <see href="https://wicg.github.io/observable/#callbackdef-subscribecallback">Observable: 2.2 The Observable interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void SubscribeCallback(Subscriber subscriber);

/// <summary>
/// <see href="https://wicg.github.io/observable/#callbackdef-visitor">Observable: 2.2 The Observable interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void Visitor(object value, ulong index);

/// <summary>
/// <see href="https://wicg.github.io/scheduling-apis/#callbackdef-schedulerposttaskcallback">Prioritized Task Scheduling: 2.2 The Scheduler Interface</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object SchedulerPostTaskCallback();

/// <summary>
/// <see href="https://wicg.github.io/video-rvfc/#callbackdef-videoframerequestcallback">HTMLVideoElement.requestVideoFrameCallback(): 3 VideoFrameRequestCallback</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VideoFrameRequestCallback(double now, VideoFrameCallbackMetadata metadata);

/// <summary>
/// <see href="https://wicg.github.io/web-app-launch/#dom-launchconsumer">Web App Launch Handler API: 5.2 LaunchConsumer function</see>
/// </summary>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object LaunchConsumer(LaunchParams @params);

/// <summary>
/// All MediaSessions have a map of supported media session actions with, as a key, a media session action and as a value a MediaSessionActionHandler.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/mediasession/#callbackdef-mediasessionactionhandler">Media Session: 5 The MediaSession interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void MediaSessionActionHandler(MediaSessionActionDetails details);

/// <summary>
/// If observer is an ObservableSubscriptionCallback
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-observablesubscriptioncallback">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ObservableSubscriptionCallback(object value);

/// <summary>
/// The RemotePlaybackAvailabilityCallback returns the current remote playback device availability.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/remote-playback/#dom-remoteplaybackavailabilitycallback">Remote Playback API: 5.2 RemotePlayback interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RemotePlaybackAvailabilityCallback(bool available);

/// <summary>
/// a \Callback of type PressureUpdateCallback set on creation.
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/compute-pressure/#dom-pressureupdatecallback">Compute Pressure Level 1: 10.1 The PressureUpdateCallback callback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PressureUpdateCallback(PressureRecord[] changes, PressureObserver observer);

/// <summary>
/// generateAssertion of type GenerateAssertionCallback, required
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-identity/#dom-generateassertioncallback">Identity for WebRTC 1.0: Callback GenerateAssertionCallback Parameters</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<RTCIdentityAssertionResult> GenerateAssertionCallback(string contents, string origin, RTCIdentityProviderOptions options);

/// <summary>
/// validateAssertion of type ValidateAssertionCallback, required
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-identity/#dom-validateassertioncallback">Identity for WebRTC 1.0: Callback ValidateAssertionCallback Parameters</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<RTCIdentityValidationResult> ValidateAssertionCallback(string assertion, string origin);

[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class EventListenerLiteral
{
    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-eventlistener-handleevent">DOM Standard: 2.7 Interface EventTarget</see>
    /// </summary>
    [ECMAScript]
    [Description("@#handleEvent")]
    public HandleEventCallback? HandleEvent { get; set; }
}

[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class NodeFilterLiteral
{
    /// <summary>
    /// If traverser&apos;s filter is null, then return FILTER_ACCEPT.
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-filter_accept">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#FILTER_ACCEPT")]
    public const ushort FILTER_ACCEPT = 1;

    /// <summary>
    /// If result is FILTER_REJECT or sibling is null, then set sibling to node&apos;s next sibling if type is &quot;next&quot;; otherwise to node&apos;s previous sibling.
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-filter_reject">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#FILTER_REJECT")]
    public const ushort FILTER_REJECT = 2;

    /// <summary>
    /// If the nth bit (where 0 is the least significant bit) of traverser&apos;s whatToShow is not set, then return FILTER_SKIP.
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-filter_skip">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#FILTER_SKIP")]
    public const ushort FILTER_SKIP = 3;

    /// <summary>
    /// SHOW_ALL (4294967295, FFFFFFFF in hexadecimal);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_all">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ALL")]
    public const uint SHOW_ALL = 0xFFFFFFFF;

    /// <summary>
    /// SHOW_ELEMENT (1);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_element">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ELEMENT")]
    public const uint SHOW_ELEMENT = 0x1;

    /// <summary>
    /// SHOW_ATTRIBUTE (2);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_attribute">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ATTRIBUTE")]
    public const uint SHOW_ATTRIBUTE = 0x2;

    /// <summary>
    /// SHOW_TEXT (4);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_text">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_TEXT")]
    public const uint SHOW_TEXT = 0x4;

    /// <summary>
    /// SHOW_CDATA_SECTION (8);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_cdata_section">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_CDATA_SECTION")]
    public const uint SHOW_CDATA_SECTION = 0x8;

    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_entity_reference">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </summary>
    [Description("@#SHOW_ENTITY_REFERENCE")]
    public const uint SHOW_ENTITY_REFERENCE = 0x10;

    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_entity">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </summary>
    [Description("@#SHOW_ENTITY")]
    public const uint SHOW_ENTITY = 0x20;

    /// <summary>
    /// SHOW_PROCESSING_INSTRUCTION (64, 40 in hexadecimal);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_processing_instruction">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_PROCESSING_INSTRUCTION")]
    public const uint SHOW_PROCESSING_INSTRUCTION = 0x40;

    /// <summary>
    /// SHOW_COMMENT (128, 80 in hexadecimal);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_comment">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_COMMENT")]
    public const uint SHOW_COMMENT = 0x80;

    /// <summary>
    /// SHOW_DOCUMENT (256, 100 in hexadecimal);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_document">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_DOCUMENT")]
    public const uint SHOW_DOCUMENT = 0x100;

    /// <summary>
    /// SHOW_DOCUMENT_TYPE (512, 200 in hexadecimal);
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_document_type">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_DOCUMENT_TYPE")]
    public const uint SHOW_DOCUMENT_TYPE = 0x200;

    /// <summary>
    /// SHOW_DOCUMENT_FRAGMENT (1024, 400 in hexadecimal).
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_document_fragment">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_DOCUMENT_FRAGMENT")]
    public const uint SHOW_DOCUMENT_FRAGMENT = 0x400;

    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </summary>
    [Description("@#SHOW_NOTATION")]
    public const uint SHOW_NOTATION = 0x800;

    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-acceptnode">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </summary>
    [ECMAScript]
    [Description("@#acceptNode")]
    public AcceptNodeCallback? AcceptNode { get; set; }
}

[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class XPathNSResolverLiteral
{
    /// <summary>
    /// <see href="https://dom.spec.whatwg.org/#dom-xpathnsresolver-lookupnamespaceuri">DOM Standard: 8.3 Mixin XPathEvaluatorBase</see>
    /// </summary>
    [ECMAScript]
    [Description("@#lookupNamespaceURI")]
    public LookupNamespaceURICallback? LookupNamespaceURI { get; set; }
}
