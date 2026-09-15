namespace ECMAScript;

/// <summary>
/// WebIDL callback MutationCallback。定义于 DOM Standard。
/// </summary>
/// <remarks>
/// <see href="https://dom.spec.whatwg.org/#callbackdef-mutationcallback">DOM Standard: 4.3.1 Interface MutationObserver</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void MutationCallback(MutationRecord[] mutations, MutationObserver observer);

/// <summary>
/// Note: This feature is available in Web Workers. The addEventListener() method of the EventTarget interface sets up a function that will be called whenever the specified event is delivered to the target. Common targets are Element, or its children, Document, and Window, but the target may be any object that supports events (such as IDBRequest). Note: The addEventListener() method is the recommended way to register an event listener. The benefits are as follows: It allows adding more than one handler for an event. This is particularly useful for libraries, JavaScript modules, or any other kind of code that needs to work well with other libraries or extensions. In contrast to using an onXYZ property, it gives you finer-grained control of the phase when the listener is activated (capturing vs. bubbling). It works on any event target, not just HTML or SVG elements. The method addEventListener() works by adding a function, or an object that implements a handleEvent() function, to the list of event listeners for the specified event type on the EventTarget on which it&apos;s called. If the function or object is already in the list of event listeners for this target, the function or object is not added a second time. Note: If a particular anonymous function is in the list of event listeners registered for a certain target, and then later in the code, an identical anonymous function is given in an addEventListener call, the second function will also be added to the list of event listeners for that target. Indeed, anonymous functions are not identical even if defined using the same unchanging source-code called repeatedly, even if in a loop. Repeatedly defining the same unnamed function in such cases can be problematic. (See Memory issues, below.) If an event listener is added to an EventTarget from inside another listener — that is, during the processing of the event — that event will not trigger the new listener. However, the new listener may be triggered during a later stage of event flow, such as during the bubbling phase.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener">MDN Web Docs: EventListener.handleEvent</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void HandleEventCallback(EventRef @event);

/// <summary>
/// The Document.createNodeIterator() method returns a new NodeIterator object.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/createNodeIterator">MDN Web Docs: NodeFilter.acceptNode</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate ushort AcceptNodeCallback(Node node);

/// <summary>
/// The lookupNamespaceURI() method of the Node interface takes a prefix as parameter and returns the namespace URI associated with it on the given node if found (and null if not). This method&apos;s existence allows Node objects to be passed as a namespace resolver to XPathEvaluator.createExpression() and XPathEvaluator.evaluate().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Node/lookupNamespaceURI">MDN Web Docs: XPathNSResolver.lookupNamespaceURI</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? LookupNamespaceURICallback(string? prefix);

/// <summary>
/// WebIDL callback ViewTransitionUpdateCallback。定义于 CSS View Transitions Module Level 2。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/css-view-transitions-2/#callbackdef-viewtransitionupdatecallback">CSS View Transitions Module Level 2: 2.1 Additions to Document</see>
/// </remarks>
/// <example>
/// <code>viewTransition = document.startViewTransition(updateCallback)</code>
/// </example>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> ViewTransitionUpdateCallback();

/// <summary>
/// WebIDL callback ResizeObserverCallback。定义于 Resize Observer Module Level 1。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/resize-observer-1/#callbackdef-resizeobservercallback">Resize Observer Module Level 1: 2.2 ResizeObserverCallback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ResizeObserverCallback(ResizeObserverEntry[] entries, ResizeObserver observer);

/// <summary>
/// WebIDL callback EffectCallback。定义于 Web Animations Module Level 2。
/// </summary>
/// <remarks>
/// <see href="https://drafts.csswg.org/web-animations-2/#callbackdef-effectcallback">Web Animations Module Level 2: 4.12 The EffectCallback callback function</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EffectCallback(double? progress, ParameterCurrentTarget currentTarget, Animation animation);

/// <summary>
/// WebIDL callback BlobCallback。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/canvas.html#blobcallback">HTML Standard: 4.12.5 The canvas element</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void BlobCallback(Blob? blob);

/// <summary>
/// WebIDL callback FunctionStringCallback。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/dnd.html#functionstringcallback">HTML Standard: 6.11.3.2 The DataTransferItem interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FunctionStringCallback(string data);

/// <summary>
/// WebIDL callback FrameRequestCallback。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/imagebitmap-and-animations.html#framerequestcallback">HTML Standard: 8.12 Animation frames</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FrameRequestCallback(double time);

/// <summary>
/// WebIDL callback EventHandlerNonNull。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/webappapis.html#eventhandlernonnull">HTML Standard: 8.1.8.1 Event handlers</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object EventHandlerNonNull(EventRef @event);

/// <summary>
/// WebIDL callback OnBeforeUnloadEventHandlerNonNull。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/webappapis.html#onbeforeunloadeventhandlernonnull">HTML Standard: 8.1.8.1 Event handlers</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? OnBeforeUnloadEventHandlerNonNull(EventRef @event);

/// <summary>
/// WebIDL callback OnErrorEventHandlerNonNull。定义于 HTML Standard。
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/webappapis.html#onerroreventhandlernonnull">HTML Standard: 8.1.8.1 Event handlers</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object OnErrorEventHandlerNonNull(ParameterEvent @event, string source, uint lineno, uint colno, object error);

/// <summary>
/// Experimental: This is an experimental technologyCheck the Browser compatibility table carefully before using this in production. Secure context: This feature is available only in secure contexts (HTTPS), in some or all supporting browsers. The XRSession method requestAnimationFrame(), much like the Window method of the same name, schedules a callback to be executed the next time the browser is ready to paint the session&apos;s virtual environment to the XR display. The specified callback is executed once before the next repaint; if you wish for it to be executed for the following repaint, you must call requestAnimationFrame() again. This can be done from within the callback itself. The callback takes two parameters as inputs: an XRFrame describing the state of all tracked objects for the session, and a timestamp you can use to compute any animation updates needed. You can cancel a previously scheduled animation by calling cancelAnimationFrame(). Note: Despite the obvious similarities between these methods and the global requestAnimationFrame() function provided by the Window interface, you must not treat these as interchangeable. There is no guarantee that the latter will work at all while an immersive XR session is underway.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/XRSession/requestAnimationFrame">MDN Web Docs: XRFrameRequestCallback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void XRFrameRequestCallback(double time, XRFrame frame);

/// <summary>
/// WebIDL callback NotificationPermissionCallback。定义于 Notifications API Standard。
/// </summary>
/// <remarks>
/// <see href="https://notifications.spec.whatwg.org/#callbackdef-notificationpermissioncallback">Notifications API Standard: 3 API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void NotificationPermissionCallback(NotificationPermission permission);

/// <summary>
/// WebIDL callback QueuingStrategySize。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-queuingstrategysize">Streams Standard: 7.1 The queuing strategy API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate double QueuingStrategySize(object chunk);

/// <summary>
/// WebIDL callback TransformerCancelCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformercancelcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult TransformerCancelCallback(object reason);

/// <summary>
/// WebIDL callback TransformerFlushCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformerflushcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult TransformerFlushCallback(TransformStreamDefaultController controller);

/// <summary>
/// WebIDL callback TransformerStartCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformerstartcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object TransformerStartCallback(TransformStreamDefaultController controller);

/// <summary>
/// WebIDL callback TransformerTransformCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-transformertransformcallback">Streams Standard: 6.2.3 The transformer API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult TransformerTransformCallback(object chunk, TransformStreamDefaultController controller);

/// <summary>
/// WebIDL callback UnderlyingSinkAbortCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkabortcallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSinkAbortCallback(object reason);

/// <summary>
/// WebIDL callback UnderlyingSinkCloseCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkclosecallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSinkCloseCallback();

/// <summary>
/// WebIDL callback UnderlyingSinkStartCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkstartcallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object UnderlyingSinkStartCallback(WritableStreamDefaultController controller);

/// <summary>
/// WebIDL callback UnderlyingSinkWriteCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsinkwritecallback">Streams Standard: 5.2.3 The underlying sink API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSinkWriteCallback(object chunk, WritableStreamDefaultController controller);

/// <summary>
/// WebIDL callback UnderlyingSourceCancelCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsourcecancelcallback">Streams Standard: 4.2.3 The underlying source API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSourceCancelCallback(object reason);

/// <summary>
/// WebIDL callback UnderlyingSourcePullCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsourcepullcallback">Streams Standard: 4.2.3 The underlying source API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult UnderlyingSourcePullCallback(ReadableStreamController controller);

/// <summary>
/// WebIDL callback UnderlyingSourceStartCallback。定义于 Streams Standard。
/// </summary>
/// <remarks>
/// <see href="https://streams.spec.whatwg.org/#callbackdef-underlyingsourcestartcallback">Streams Standard: 4.2.3 The underlying source API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object UnderlyingSourceStartCallback(ReadableStreamController controller);

/// <summary>
/// WebIDL callback IntersectionObserverCallback。定义于 Intersection Observer。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/IntersectionObserver/#callbackdef-intersectionobservercallback">Intersection Observer: 2.1 The IntersectionObserverCallback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void IntersectionObserverCallback(IntersectionObserverEntry[] entries, IntersectionObserver observer);

/// <summary>
/// WebIDL callback PositionCallback。定义于 Geolocation。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/geolocation/#dom-positioncallback">Geolocation: 6 Geolocation interface and callbacks</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PositionCallback(GeolocationPosition position);

/// <summary>
/// WebIDL callback PositionErrorCallback。定义于 Geolocation。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/geolocation/#dom-positionerrorcallback">Geolocation: 6 Geolocation interface and callbacks</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PositionErrorCallback(GeolocationPositionError positionError);

/// <summary>
/// WebIDL callback PerformanceObserverCallback。定义于 Performance Timeline。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/performance-timeline/#callbackdef-performanceobservercallback">Performance Timeline: 4 The PerformanceObserver interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void PerformanceObserverCallback(PerformanceObserverEntryList entries, PerformanceObserver observer, PerformanceObserverCallbackOptions options);

/// <summary>
/// WebIDL callback ReportingObserverCallback。定义于 Reporting API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/reporting/#callbackdef-reportingobservercallback">Reporting API: 4.1 Interface ReportingObserver</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ReportingObserverCallback(Report[] reports, ReportingObserver observer);

/// <summary>
/// WebIDL callback IdleRequestCallback。定义于 requestIdleCallback()。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/requestidlecallback/#dom-idlerequestcallback">requestIdleCallback(): 4.1 The requestIdleCallback() method</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void IdleRequestCallback(IdleDeadline deadline);

/// <summary>
/// WebIDL callback CreateHTMLCallback。定义于 Trusted Types。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/trusted-types/dist/spec/#callbackdef-createhtmlcallback">Trusted Types: 2.3.3 TrustedTypePolicyOptions</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateHTMLCallback(string input, object arguments);

/// <summary>
/// WebIDL callback CreateScriptCallback。定义于 Trusted Types。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/trusted-types/dist/spec/#callbackdef-createscriptcallback">Trusted Types: 2.3.3 TrustedTypePolicyOptions</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateScriptCallback(string input, object arguments);

/// <summary>
/// WebIDL callback CreateScriptURLCallback。定义于 Trusted Types。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/trusted-types/dist/spec/#callbackdef-createscripturlcallback">Trusted Types: 2.3.3 TrustedTypePolicyOptions</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate string? CreateScriptURLCallback(string input, object arguments);

/// <summary>
/// WebIDL callback LockGrantedCallback。定义于 Web Locks API。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/web-locks/#callbackdef-lockgrantedcallback">Web Locks API: 3.2 LockManager class</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> LockGrantedCallback(Lock? @lock);

/// <summary>
/// WebIDL callback AudioDataOutputCallback。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-audiodataoutputcallback">WebCodecs: 3 AudioDecoder Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void AudioDataOutputCallback(AudioData output);

/// <summary>
/// WebIDL callback EncodedAudioChunkOutputCallback。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-encodedaudiochunkoutputcallback">WebCodecs: 5 AudioEncoder Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EncodedAudioChunkOutputCallback(EncodedAudioChunk output, EncodedAudioChunkMetadata metadata);

/// <summary>
/// WebIDL callback EncodedVideoChunkOutputCallback。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-encodedvideochunkoutputcallback">WebCodecs: 6 VideoEncoder Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void EncodedVideoChunkOutputCallback(EncodedVideoChunk chunk, EncodedVideoChunkMetadata metadata);

/// <summary>
/// WebIDL callback VideoFrameOutputCallback。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-videoframeoutputcallback">WebCodecs: 4 VideoDecoder Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VideoFrameOutputCallback(VideoFrame output);

/// <summary>
/// WebIDL callback WebCodecsErrorCallback。定义于 WebCodecs。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webcodecs/#callbackdef-webcodecserrorcallback">WebCodecs: 7.16 WebCodecsErrorCallback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void WebCodecsErrorCallback(DOMException error);

/// <summary>
/// WebIDL callback RTCPeerConnectionErrorCallback。定义于 WebRTC: Real-Time Communication in Browsers。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcpeerconnectionerrorcallback">WebRTC: Real-Time Communication in Browsers: RTCPeerConnectionErrorCallback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RTCPeerConnectionErrorCallback(DOMException error);

/// <summary>
/// WebIDL callback RTCSessionDescriptionCallback。定义于 WebRTC: Real-Time Communication in Browsers。
/// </summary>
/// <remarks>
/// <see href="https://w3c.github.io/webrtc-pc/#dom-rtcsessiondescriptioncallback">WebRTC: Real-Time Communication in Browsers: RTCSessionDescriptionCallback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void RTCSessionDescriptionCallback(RTCSessionDescriptionInit description);

/// <summary>
/// WebIDL callback AudioWorkletProcessCallback。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#audioworkletprocess-callback-parameters">Web Audio API 1.1: 1.32.5.3.1 Callback AudioWorkletProcessCallback Parameters</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate bool AudioWorkletProcessCallback(FrozenSet<FrozenSet<Float32Array>> inputs, FrozenSet<FrozenSet<Float32Array>> outputs, object parameters);

/// <summary>
/// WebIDL callback DecodeErrorCallback。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#callback-decodeerrorcallback-parameters">Web Audio API 1.1: 1.1.4 Callback DecodeErrorCallback() Parameters</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void DecodeErrorCallback(DOMException error);

/// <summary>
/// WebIDL callback DecodeSuccessCallback。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#callback-decodesuccesscallback-parameters">Web Audio API 1.1: 1.1.3 Callback DecodeSuccessCallback() Parameters</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void DecodeSuccessCallback(AudioBuffer decodedData);

/// <summary>
/// WebIDL callback AudioWorkletProcessorConstructor。定义于 Web Audio API 1.1。
/// </summary>
/// <remarks>
/// <see href="https://webaudio.github.io/web-audio-api/#callbackdef-audioworkletprocessorconstructor">Web Audio API 1.1: 1.32.3 The AudioWorkletGlobalScope Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate AudioWorkletProcessor AudioWorkletProcessorConstructor(object options);

/// <summary>
/// WebIDL callback VoidFunction。定义于 Web IDL Standard。
/// </summary>
/// <remarks>
/// <see href="https://webidl.spec.whatwg.org/#VoidFunction">Web IDL Standard: 4.6 VoidFunction</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VoidFunction();

/// <summary>
/// WebIDL callback LanguageModelToolFunction。定义于 Prompt API。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/prompt-api/#callbackdef-languagemodeltoolfunction">Prompt API: 3 The API</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<string> LanguageModelToolFunction(object arguments);

/// <summary>
/// WebIDL callback ToolExecuteCallback。定义于 WebMCP。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/webmcp/#callbackdef-toolexecutecallback">WebMCP: 4.2.1 ModelContextTool Dictionary</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult<object> ToolExecuteCallback(object inputObject, ToolExecuteCallbackOptions options);

/// <summary>
/// WebIDL callback CreateMonitorCallback。定义于 Writing Assistance APIs。
/// </summary>
/// <remarks>
/// <see href="https://webmachinelearning.github.io/writing-assistance-apis/#callbackdef-createmonitorcallback">Writing Assistance APIs: 5.1 Common APIs</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void CreateMonitorCallback(CreateMonitor monitor);

/// <summary>
/// WebIDL callback RefillCallback。定义于 Autofill Event。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/autofill-event/#callbackdef-refillcallback">Autofill Event: 3 The AutofillEvent Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult RefillCallback();

/// <summary>
/// WebIDL callback ErrorCallback。定义于 File and Directory Entries API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-errorcallback">File and Directory Entries API: 7 Files and Directories</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ErrorCallback(DOMException err);

/// <summary>
/// WebIDL callback FileCallback。定义于 File and Directory Entries API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-filecallback">File and Directory Entries API: 7.4 The FileSystemFileEntry Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileCallback(FileRef file);

/// <summary>
/// WebIDL callback FileSystemEntriesCallback。定义于 File and Directory Entries API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-filesystementriescallback">File and Directory Entries API: 7.3 The FileSystemDirectoryReader Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileSystemEntriesCallback(FileSystemEntry[] entries);

/// <summary>
/// WebIDL callback FileSystemEntryCallback。定义于 File and Directory Entries API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/entries-api/#callbackdef-filesystementrycallback">File and Directory Entries API: 7.2 The FileSystemDirectoryEntry Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void FileSystemEntryCallback(FileSystemEntry entry);

/// <summary>
/// WebIDL callback CatchCallback。定义于 Observable。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-catchcallback">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object CatchCallback(object value);

/// <summary>
/// WebIDL callback Mapper。定义于 Observable。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-mapper">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Mapper(object value, Number index);

/// <summary>
/// WebIDL callback ObservableInspectorAbortHandler。定义于 Observable。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-observableinspectoraborthandler">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void ObservableInspectorAbortHandler(object value);

/// <summary>
/// WebIDL callback Predicate。定义于 Observable。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-predicate">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate bool Predicate(object value, Number index);

/// <summary>
/// WebIDL callback Reducer。定义于 Observable。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-reducer">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Reducer(object accumulator, object currentValue, Number index);

/// <summary>
/// WebIDL callback SubscribeCallback。定义于 Observable。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-subscribecallback">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void SubscribeCallback(Subscriber subscriber);

/// <summary>
/// WebIDL callback Visitor。定义于 Observable。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/observable/#callbackdef-visitor">Observable: 2.2 The Observable interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void Visitor(object value, Number index);

/// <summary>
/// WebIDL callback SchedulerPostTaskCallback。定义于 Prioritized Task Scheduling。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/scheduling-apis/#callbackdef-schedulerposttaskcallback">Prioritized Task Scheduling: 2.2 The Scheduler Interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object SchedulerPostTaskCallback();

/// <summary>
/// WebIDL callback VideoFrameRequestCallback。定义于 HTMLVideoElement.requestVideoFrameCallback()。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/video-rvfc/#callbackdef-videoframerequestcallback">HTMLVideoElement.requestVideoFrameCallback(): 3 VideoFrameRequestCallback</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate void VideoFrameRequestCallback(double now, VideoFrameCallbackMetadata metadata);

/// <summary>
/// WebIDL callback LaunchConsumer。定义于 Web App Launch Handler API。
/// </summary>
/// <remarks>
/// <see href="https://wicg.github.io/web-app-launch/#dom-launchconsumer">Web App Launch Handler API: 5.2 LaunchConsumer function</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object LaunchConsumer(LaunchParams @params);

/// <summary>
/// A Web IDL CustomElementConstructor callback function type value wrapping the custom element constructor
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/custom-elements.html#customelementconstructor">HTML Standard: 4.13.4 The CustomElementRegistry interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate HTMLElement CustomElementConstructor();

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
/// Each NavigateEvent has a navigation handler list, a list of NavigationInterceptHandler callbacks, initially empty.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationintercepthandler">HTML Standard: 7.2.6.10.1 The NavigateEvent interface</see>
/// </remarks>
/// <example>
/// <code>precommitController.addHandler(NavigationInterceptHandler handler)</code>
/// </example>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult NavigationInterceptHandler();

/// <summary>
/// Each NavigateEvent has a navigation precommit handler list, a list of NavigationPrecommitHandler callbacks, initially empty.
/// </summary>
/// <remarks>
/// <see href="https://html.spec.whatwg.org/multipage/nav-history-apis.html#navigationprecommithandler">HTML Standard: 7.2.6.10.2 The NavigationPrecommitController interface</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate PromiseResult NavigationPrecommitHandler(NavigationPrecommitController controller);

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
/// The &quot;Custom DOM Elements&quot; spec wants to use callback function types for platform object provided functions. Should we rename &quot;callback functions&quot; to just &quot;functions&quot; to make it clear that they can be used for both purposes?
/// </summary>
/// <remarks>
/// <see href="https://webidl.spec.whatwg.org/#Function">Web IDL Standard: 4.5 Function</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public delegate object Function(object arguments);

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

/// <summary>
/// Note: This feature is available in Web Workers. The addEventListener() method of the EventTarget interface sets up a function that will be called whenever the specified event is delivered to the target. Common targets are Element, or its children, Document, and Window, but the target may be any object that supports events (such as IDBRequest). Note: The addEventListener() method is the recommended way to register an event listener. The benefits are as follows: It allows adding more than one handler for an event. This is particularly useful for libraries, JavaScript modules, or any other kind of code that needs to work well with other libraries or extensions. In contrast to using an onXYZ property, it gives you finer-grained control of the phase when the listener is activated (capturing vs. bubbling). It works on any event target, not just HTML or SVG elements. The method addEventListener() works by adding a function, or an object that implements a handleEvent() function, to the list of event listeners for the specified event type on the EventTarget on which it&apos;s called. If the function or object is already in the list of event listeners for this target, the function or object is not added a second time. Note: If a particular anonymous function is in the list of event listeners registered for a certain target, and then later in the code, an identical anonymous function is given in an addEventListener call, the second function will also be added to the list of event listeners for that target. Indeed, anonymous functions are not identical even if defined using the same unchanging source-code called repeatedly, even if in a loop. Repeatedly defining the same unnamed function in such cases can be problematic. (See Memory issues, below.) If an event listener is added to an EventTarget from inside another listener — that is, during the processing of the event — that event will not trigger the new listener. However, the new listener may be triggered during a later stage of event flow, such as during the bubbling phase.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener">MDN Web Docs: EventListener</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class EventListenerLiteral
{
    /// <summary>
    /// Note: This feature is available in Web Workers. The addEventListener() method of the EventTarget interface sets up a function that will be called whenever the specified event is delivered to the target. Common targets are Element, or its children, Document, and Window, but the target may be any object that supports events (such as IDBRequest). Note: The addEventListener() method is the recommended way to register an event listener. The benefits are as follows: It allows adding more than one handler for an event. This is particularly useful for libraries, JavaScript modules, or any other kind of code that needs to work well with other libraries or extensions. In contrast to using an onXYZ property, it gives you finer-grained control of the phase when the listener is activated (capturing vs. bubbling). It works on any event target, not just HTML or SVG elements. The method addEventListener() works by adding a function, or an object that implements a handleEvent() function, to the list of event listeners for the specified event type on the EventTarget on which it&apos;s called. If the function or object is already in the list of event listeners for this target, the function or object is not added a second time. Note: If a particular anonymous function is in the list of event listeners registered for a certain target, and then later in the code, an identical anonymous function is given in an addEventListener call, the second function will also be added to the list of event listeners for that target. Indeed, anonymous functions are not identical even if defined using the same unchanging source-code called repeatedly, even if in a loop. Repeatedly defining the same unnamed function in such cases can be problematic. (See Memory issues, below.) If an event listener is added to an EventTarget from inside another listener — that is, during the processing of the event — that event will not trigger the new listener. However, the new listener may be triggered during a later stage of event flow, such as during the bubbling phase.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener">MDN Web Docs: EventListener.handleEvent</see>
    /// </remarks>
    [ECMAScript]
    [Description("@#handleEvent")]
    public HandleEventCallback? HandleEvent { get; set; }
}

/// <summary>
/// The Document.createNodeIterator() method returns a new NodeIterator object.
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/createNodeIterator">MDN Web Docs: NodeFilter</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class NodeFilterLiteral
{
    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#FILTER_ACCEPT")]
    public const ushort FILTER_ACCEPT = 1;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#FILTER_REJECT")]
    public const ushort FILTER_REJECT = 2;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#FILTER_SKIP")]
    public const ushort FILTER_SKIP = 3;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ALL")]
    public const uint SHOW_ALL = 0xFFFFFFFF;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ELEMENT")]
    public const uint SHOW_ELEMENT = 0x1;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ATTRIBUTE")]
    public const uint SHOW_ATTRIBUTE = 0x2;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_TEXT")]
    public const uint SHOW_TEXT = 0x4;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_CDATA_SECTION")]
    public const uint SHOW_CDATA_SECTION = 0x8;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ENTITY_REFERENCE")]
    public const uint SHOW_ENTITY_REFERENCE = 0x10;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_ENTITY")]
    public const uint SHOW_ENTITY = 0x20;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_PROCESSING_INSTRUCTION")]
    public const uint SHOW_PROCESSING_INSTRUCTION = 0x40;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_COMMENT")]
    public const uint SHOW_COMMENT = 0x80;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_DOCUMENT")]
    public const uint SHOW_DOCUMENT = 0x100;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_DOCUMENT_TYPE")]
    public const uint SHOW_DOCUMENT_TYPE = 0x200;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_DOCUMENT_FRAGMENT")]
    public const uint SHOW_DOCUMENT_FRAGMENT = 0x400;

    /// <summary>
    /// NodeFilter 的规范常量 SHOW_NOTATION，WebIDL 类型为 unsigned long，值为 0x800。
    /// </summary>
    /// <remarks>
    /// <see href="https://dom.spec.whatwg.org/#dom-nodefilter-show_notation">DOM Standard: 6.3 Interface NodeFilter</see>
    /// </remarks>
    [Description("@#SHOW_NOTATION")]
    public const uint SHOW_NOTATION = 0x800;

    /// <summary>
    /// The Document.createNodeIterator() method returns a new NodeIterator object.
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Document/createNodeIterator">MDN Web Docs: NodeFilter.acceptNode</see>
    /// </remarks>
    [ECMAScript]
    [Description("@#acceptNode")]
    public AcceptNodeCallback? AcceptNode { get; set; }
}

/// <summary>
/// The lookupNamespaceURI() method of the Node interface takes a prefix as parameter and returns the namespace URI associated with it on the given node if found (and null if not). This method&apos;s existence allows Node objects to be passed as a namespace resolver to XPathEvaluator.createExpression() and XPathEvaluator.evaluate().
/// </summary>
/// <remarks>
/// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Node/lookupNamespaceURI">MDN Web Docs: XPathNSResolver</see>
/// </remarks>
[ECMAScript]
[Description("@#")]
[Category("literal")]
public sealed class XPathNSResolverLiteral
{
    /// <summary>
    /// The lookupNamespaceURI() method of the Node interface takes a prefix as parameter and returns the namespace URI associated with it on the given node if found (and null if not). This method&apos;s existence allows Node objects to be passed as a namespace resolver to XPathEvaluator.createExpression() and XPathEvaluator.evaluate().
    /// </summary>
    /// <remarks>
    /// <see href="https://developer.mozilla.org/en-US/docs/Web/API/Node/lookupNamespaceURI">MDN Web Docs: XPathNSResolver.lookupNamespaceURI</see>
    /// </remarks>
    [ECMAScript]
    [Description("@#lookupNamespaceURI")]
    public LookupNamespaceURICallback? LookupNamespaceURI { get; set; }
}