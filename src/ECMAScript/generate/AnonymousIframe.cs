namespace ECMAScript;

/// <summary>
/// HTMLIFrameElement
/// </summary>
[ECMAScript]
[Description("@#HTMLIFrameElement")]
public partial class HTMLIFrameElement : HTMLElement
{
    /// <summary>
    /// credentialless
    /// </summary>
    [Description("@#credentialless")]
    public extern bool Credentialless { get; set; }

    /// <summary>
    /// csp
    /// </summary>
    [Description("@#csp")]
    public extern string Csp { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLIFrameElement();

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// srcdoc
    /// </summary>
    [Description("@#srcdoc")]
    public extern Either<TrustedHTML, string> Srcdoc { get; set; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// sandbox
    /// </summary>
    [Description("@#sandbox")]
    public extern List<string> Sandbox { get; }

    /// <summary>
    /// allow
    /// </summary>
    [Description("@#allow")]
    public extern string Allow { get; set; }

    /// <summary>
    /// allowFullscreen
    /// </summary>
    [Description("@#allowFullscreen")]
    public extern bool AllowFullscreen { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern string Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern string Height { get; set; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// loading
    /// </summary>
    [Description("@#loading")]
    public extern string Loading { get; set; }

    /// <summary>
    /// contentDocument
    /// </summary>
    [Description("@#contentDocument")]
    public extern Document? ContentDocument { get; }

    /// <summary>
    /// contentWindow
    /// </summary>
    [Description("@#contentWindow")]
    public extern WindowProxy? ContentWindow { get; }

    /// <summary>
    /// getSVGDocument
    /// </summary>
    [Description("@#getSVGDocument")]
    public extern Document? GetSVGDocument();

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// scrolling
    /// </summary>
    [Description("@#scrolling")]
    public extern string Scrolling { get; set; }

    /// <summary>
    /// frameBorder
    /// </summary>
    [Description("@#frameBorder")]
    public extern string FrameBorder { get; set; }

    /// <summary>
    /// longDesc
    /// </summary>
    [Description("@#longDesc")]
    public extern string LongDesc { get; set; }

    /// <summary>
    /// marginHeight
    /// </summary>
    [Description("@#marginHeight")]
    public extern string MarginHeight { get; set; }

    /// <summary>
    /// marginWidth
    /// </summary>
    [Description("@#marginWidth")]
    public extern string MarginWidth { get; set; }

    /// <summary>
    /// permissionsPolicy
    /// </summary>
    [Description("@#permissionsPolicy")]
    public extern PermissionsPolicy PermissionsPolicy { get; }

    /// <summary>
    /// privateToken
    /// </summary>
    [Description("@#privateToken")]
    public extern string PrivateToken { get; set; }

    /// <summary>
    /// adAuctionHeaders
    /// </summary>
    [Description("@#adAuctionHeaders")]
    public extern bool AdAuctionHeaders { get; set; }

    #region mixin HTMLSharedStorageWritableElementUtils
    /// <summary>
    /// sharedStorageWritable
    /// </summary>
    [Description("@#sharedStorageWritable")]
    public extern bool SharedStorageWritable { get; set; }
    #endregion
}

/// <summary>
/// Window
/// </summary>
[ECMAScript]
[Description("@#Window")]
public partial class Window : EventTarget
{
    /// <summary>
    /// credentialless
    /// </summary>
    [Description("@#credentialless")]
    public extern bool Credentialless { get; }

    /// <summary>
    /// orientation
    /// </summary>
    [Description("@#orientation")]
    public extern short Orientation { get; }

    /// <summary>
    /// onorientationchange
    /// </summary>
    [Description("@#onorientationchange")]
    public extern EventHandler Onorientationchange { get; set; }

    /// <summary>
    /// cookieStore
    /// </summary>
    [Description("@#cookieStore")]
    public extern CookieStore CookieStore { get; }

    /// <summary>
    /// navigate
    /// </summary>
    /// <param name="dir">dir</param>
    [Description("@#navigate")]
    public extern void Navigate(SpatialNavigationDirection dir);

    /// <summary>
    /// viewport
    /// </summary>
    [Description("@#viewport")]
    public extern Viewport Viewport { get; }

    /// <summary>
    /// matchMedia
    /// </summary>
    /// <param name="query">query</param>
    [Description("@#matchMedia")]
    public extern MediaQueryList MatchMedia(string query);

    /// <summary>
    /// screen
    /// </summary>
    [Description("@#screen")]
    public extern Screen Screen { get; }

    /// <summary>
    /// visualViewport
    /// </summary>
    [Description("@#visualViewport")]
    public extern VisualViewport? VisualViewport { get; }

    /// <summary>
    /// moveTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#moveTo")]
    public extern void MoveTo(int x, int y);

    /// <summary>
    /// moveBy
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#moveBy")]
    public extern void MoveBy(int x, int y);

    /// <summary>
    /// resizeTo
    /// </summary>
    /// <param name="width">width</param>
    /// <param name="height">height</param>
    [Description("@#resizeTo")]
    public extern void ResizeTo(int width, int height);

    /// <summary>
    /// resizeBy
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#resizeBy")]
    public extern void ResizeBy(int x, int y);

    /// <summary>
    /// innerWidth
    /// </summary>
    [Description("@#innerWidth")]
    public extern int InnerWidth { get; }

    /// <summary>
    /// innerHeight
    /// </summary>
    [Description("@#innerHeight")]
    public extern int InnerHeight { get; }

    /// <summary>
    /// scrollX
    /// </summary>
    [Description("@#scrollX")]
    public extern double ScrollX { get; }

    /// <summary>
    /// pageXOffset
    /// </summary>
    [Description("@#pageXOffset")]
    public extern double PageXOffset { get; }

    /// <summary>
    /// scrollY
    /// </summary>
    [Description("@#scrollY")]
    public extern double ScrollY { get; }

    /// <summary>
    /// pageYOffset
    /// </summary>
    [Description("@#pageYOffset")]
    public extern double PageYOffset { get; }

    /// <summary>
    /// scroll
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#scroll")]
    public extern void Scroll(ScrollToOptions? options = default);

    /// <summary>
    /// scroll
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#scroll")]
    public extern void Scroll(double x, double y);

    /// <summary>
    /// scrollTo
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#scrollTo")]
    public extern void ScrollTo(ScrollToOptions? options = default);

    /// <summary>
    /// scrollTo
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#scrollTo")]
    public extern void ScrollTo(double x, double y);

    /// <summary>
    /// scrollBy
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#scrollBy")]
    public extern void ScrollBy(ScrollToOptions? options = default);

    /// <summary>
    /// scrollBy
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    [Description("@#scrollBy")]
    public extern void ScrollBy(double x, double y);

    /// <summary>
    /// screenX
    /// </summary>
    [Description("@#screenX")]
    public extern int ScreenX { get; }

    /// <summary>
    /// screenLeft
    /// </summary>
    [Description("@#screenLeft")]
    public extern int ScreenLeft { get; }

    /// <summary>
    /// screenY
    /// </summary>
    [Description("@#screenY")]
    public extern int ScreenY { get; }

    /// <summary>
    /// screenTop
    /// </summary>
    [Description("@#screenTop")]
    public extern int ScreenTop { get; }

    /// <summary>
    /// outerWidth
    /// </summary>
    [Description("@#outerWidth")]
    public extern int OuterWidth { get; }

    /// <summary>
    /// outerHeight
    /// </summary>
    [Description("@#outerHeight")]
    public extern int OuterHeight { get; }

    /// <summary>
    /// devicePixelRatio
    /// </summary>
    [Description("@#devicePixelRatio")]
    public extern double DevicePixelRatio { get; }

    /// <summary>
    /// getDigitalGoodsService
    /// </summary>
    /// <param name="serviceProvider">serviceProvider</param>
    [Description("@#getDigitalGoodsService")]
    public extern PromiseResult<DigitalGoodsService> GetDigitalGoodsService(string serviceProvider);

    /// <summary>
    /// documentPictureInPicture
    /// </summary>
    [Description("@#documentPictureInPicture")]
    public extern DocumentPictureInPicture DocumentPictureInPicture { get; }

    /// <summary>
    /// event
    /// </summary>
    [Description("@#event")]
    public extern Either<Event, object> Event { get; }

    /// <summary>
    /// fence
    /// </summary>
    [Description("@#fence")]
    public extern Fence? Fence { get; }

    /// <summary>
    /// showOpenFilePicker
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#showOpenFilePicker")]
    public extern PromiseResult<FileSystemFileHandle[]> ShowOpenFilePicker(OpenFilePickerOptions? options = default);

    /// <summary>
    /// showSaveFilePicker
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#showSaveFilePicker")]
    public extern PromiseResult<FileSystemFileHandle> ShowSaveFilePicker(SaveFilePickerOptions? options = default);

    /// <summary>
    /// showDirectoryPicker
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#showDirectoryPicker")]
    public extern PromiseResult<FileSystemDirectoryHandle> ShowDirectoryPicker(DirectoryPickerOptions? options = default);

    /// <summary>
    /// window
    /// </summary>
    [Description("@#window")]
    public extern WindowProxy Window_ { get; }

    /// <summary>
    /// self
    /// </summary>
    [Description("@#self")]
    public extern WindowProxy Self { get; }

    /// <summary>
    /// document
    /// </summary>
    [Description("@#document")]
    public extern Document Document { get; }

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// location
    /// </summary>
    [Description("@#location")]
    public extern Location Location { get; }

    /// <summary>
    /// history
    /// </summary>
    [Description("@#history")]
    public extern History History { get; }

    /// <summary>
    /// navigation
    /// </summary>
    [Description("@#navigation")]
    public extern Navigation Navigation { get; }

    /// <summary>
    /// customElements
    /// </summary>
    [Description("@#customElements")]
    public extern CustomElementRegistry CustomElements { get; }

    /// <summary>
    /// locationbar
    /// </summary>
    [Description("@#locationbar")]
    public extern BarProp Locationbar { get; }

    /// <summary>
    /// menubar
    /// </summary>
    [Description("@#menubar")]
    public extern BarProp Menubar { get; }

    /// <summary>
    /// personalbar
    /// </summary>
    [Description("@#personalbar")]
    public extern BarProp Personalbar { get; }

    /// <summary>
    /// scrollbars
    /// </summary>
    [Description("@#scrollbars")]
    public extern BarProp Scrollbars { get; }

    /// <summary>
    /// statusbar
    /// </summary>
    [Description("@#statusbar")]
    public extern BarProp Statusbar { get; }

    /// <summary>
    /// toolbar
    /// </summary>
    [Description("@#toolbar")]
    public extern BarProp Toolbar { get; }

    /// <summary>
    /// status
    /// </summary>
    [Description("@#status")]
    public extern string Status { get; set; }

    /// <summary>
    /// close
    /// </summary>
    [Description("@#close")]
    public extern void Close();

    /// <summary>
    /// closed
    /// </summary>
    [Description("@#closed")]
    public extern bool Closed { get; }

    /// <summary>
    /// stop
    /// </summary>
    [Description("@#stop")]
    public extern void Stop();

    /// <summary>
    /// focus
    /// </summary>
    [Description("@#focus")]
    public extern void Focus();

    /// <summary>
    /// blur
    /// </summary>
    [Description("@#blur")]
    public extern void Blur();

    /// <summary>
    /// frames
    /// </summary>
    [Description("@#frames")]
    public extern WindowProxy Frames { get; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// top
    /// </summary>
    [Description("@#top")]
    public extern WindowProxy? Top { get; }

    /// <summary>
    /// opener
    /// </summary>
    [Description("@#opener")]
    public extern object Opener { get; set; }

    /// <summary>
    /// parent
    /// </summary>
    [Description("@#parent")]
    public extern WindowProxy? Parent { get; }

    /// <summary>
    /// frameElement
    /// </summary>
    [Description("@#frameElement")]
    public extern Element? FrameElement { get; }

    /// <summary>
    /// open
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="target">target</param>
    /// <param name="features">features</param>
    [Description("@#open")]
    public extern WindowProxy? Open(string? url = default, string? target = default, string? features = default);

    [Description("@#")]
    public extern object this[string name] { get; }

    /// <summary>
    /// navigator
    /// </summary>
    [Description("@#navigator")]
    public extern Navigator Navigator { get; }

    /// <summary>
    /// clientInformation
    /// </summary>
    [Description("@#clientInformation")]
    public extern Navigator ClientInformation { get; }

    /// <summary>
    /// originAgentCluster
    /// </summary>
    [Description("@#originAgentCluster")]
    public extern bool OriginAgentCluster { get; }

    /// <summary>
    /// alert
    /// </summary>
    [Description("@#alert")]
    public extern void Alert();

    /// <summary>
    /// alert
    /// </summary>
    /// <param name="message">message</param>
    [Description("@#alert")]
    public extern void Alert(string message);

    /// <summary>
    /// confirm
    /// </summary>
    /// <param name="message">message</param>
    [Description("@#confirm")]
    public extern bool Confirm(string? message = default);

    /// <summary>
    /// prompt
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="default">default</param>
    [Description("@#prompt")]
    public extern string? Prompt(string? message = default, string? @default = default);

    /// <summary>
    /// print
    /// </summary>
    [Description("@#print")]
    public extern void Print();

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="targetOrigin">targetOrigin</param>
    /// <param name="transfer">transfer</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, string targetOrigin, object[]? transfer = default);

    /// <summary>
    /// postMessage
    /// </summary>
    /// <param name="message">message</param>
    /// <param name="options">options</param>
    [Description("@#postMessage")]
    public extern void PostMessage(object message, WindowPostMessageOptions? options = default);

    /// <summary>
    /// captureEvents
    /// </summary>
    [Description("@#captureEvents")]
    public extern void CaptureEvents();

    /// <summary>
    /// releaseEvents
    /// </summary>
    [Description("@#releaseEvents")]
    public extern void ReleaseEvents();

    /// <summary>
    /// external
    /// </summary>
    [Description("@#external")]
    public extern External External { get; }

    /// <summary>
    /// queryLocalFonts
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#queryLocalFonts")]
    public extern PromiseResult<FontData[]> QueryLocalFonts(QueryOptions? options = default);

    /// <summary>
    /// onappinstalled
    /// </summary>
    [Description("@#onappinstalled")]
    public extern EventHandler Onappinstalled { get; set; }

    /// <summary>
    /// onbeforeinstallprompt
    /// </summary>
    [Description("@#onbeforeinstallprompt")]
    public extern EventHandler Onbeforeinstallprompt { get; set; }

    /// <summary>
    /// ondeviceorientation
    /// </summary>
    [Description("@#ondeviceorientation")]
    public extern EventHandler Ondeviceorientation { get; set; }

    /// <summary>
    /// ondeviceorientationabsolute
    /// </summary>
    [Description("@#ondeviceorientationabsolute")]
    public extern EventHandler Ondeviceorientationabsolute { get; set; }

    /// <summary>
    /// ondevicemotion
    /// </summary>
    [Description("@#ondevicemotion")]
    public extern EventHandler Ondevicemotion { get; set; }

    /// <summary>
    /// portalHost
    /// </summary>
    [Description("@#portalHost")]
    public extern PortalHost? PortalHost { get; }

    /// <summary>
    /// requestIdleCallback
    /// </summary>
    /// <param name="callback">callback</param>
    /// <param name="options">options</param>
    [Description("@#requestIdleCallback")]
    public extern uint RequestIdleCallback(IdleRequestCallback callback, IdleRequestOptions? options = default);

    /// <summary>
    /// cancelIdleCallback
    /// </summary>
    /// <param name="handle">handle</param>
    [Description("@#cancelIdleCallback")]
    public extern void CancelIdleCallback(uint handle);

    /// <summary>
    /// getSelection
    /// </summary>
    [Description("@#getSelection")]
    public extern Selection? GetSelection();

    /// <summary>
    /// sharedStorage
    /// </summary>
    [Description("@#sharedStorage")]
    public extern SharedStorage? SharedStorage { get; }

    /// <summary>
    /// speechSynthesis
    /// </summary>
    [Description("@#speechSynthesis")]
    public extern SpeechSynthesis SpeechSynthesis { get; }

    /// <summary>
    /// launchQueue
    /// </summary>
    [Description("@#launchQueue")]
    public extern LaunchQueue LaunchQueue { get; }

    /// <summary>
    /// getScreenDetails
    /// </summary>
    [Description("@#getScreenDetails")]
    public extern PromiseResult<ScreenDetails> GetScreenDetails();

    #region mixin GlobalEventHandlers
    /// <summary>
    /// onanimationstart
    /// </summary>
    [Description("@#onanimationstart")]
    public extern EventHandler Onanimationstart { get; set; }

    /// <summary>
    /// onanimationiteration
    /// </summary>
    [Description("@#onanimationiteration")]
    public extern EventHandler Onanimationiteration { get; set; }

    /// <summary>
    /// onanimationend
    /// </summary>
    [Description("@#onanimationend")]
    public extern EventHandler Onanimationend { get; set; }

    /// <summary>
    /// onanimationcancel
    /// </summary>
    [Description("@#onanimationcancel")]
    public extern EventHandler Onanimationcancel { get; set; }

    /// <summary>
    /// onsnapchanged
    /// </summary>
    [Description("@#onsnapchanged")]
    public extern EventHandler Onsnapchanged { get; set; }

    /// <summary>
    /// onsnapchanging
    /// </summary>
    [Description("@#onsnapchanging")]
    public extern EventHandler Onsnapchanging { get; set; }

    /// <summary>
    /// ontransitionrun
    /// </summary>
    [Description("@#ontransitionrun")]
    public extern EventHandler Ontransitionrun { get; set; }

    /// <summary>
    /// ontransitionstart
    /// </summary>
    [Description("@#ontransitionstart")]
    public extern EventHandler Ontransitionstart { get; set; }

    /// <summary>
    /// ontransitionend
    /// </summary>
    [Description("@#ontransitionend")]
    public extern EventHandler Ontransitionend { get; set; }

    /// <summary>
    /// ontransitioncancel
    /// </summary>
    [Description("@#ontransitioncancel")]
    public extern EventHandler Ontransitioncancel { get; set; }

    /// <summary>
    /// onfencedtreeclick
    /// </summary>
    [Description("@#onfencedtreeclick")]
    public extern EventHandler Onfencedtreeclick { get; set; }

    /// <summary>
    /// onabort
    /// </summary>
    [Description("@#onabort")]
    public extern EventHandler Onabort { get; set; }

    /// <summary>
    /// onauxclick
    /// </summary>
    [Description("@#onauxclick")]
    public extern EventHandler Onauxclick { get; set; }

    /// <summary>
    /// onbeforeinput
    /// </summary>
    [Description("@#onbeforeinput")]
    public extern EventHandler Onbeforeinput { get; set; }

    /// <summary>
    /// onbeforematch
    /// </summary>
    [Description("@#onbeforematch")]
    public extern EventHandler Onbeforematch { get; set; }

    /// <summary>
    /// onbeforetoggle
    /// </summary>
    [Description("@#onbeforetoggle")]
    public extern EventHandler Onbeforetoggle { get; set; }

    /// <summary>
    /// onblur
    /// </summary>
    [Description("@#onblur")]
    public extern EventHandler Onblur { get; set; }

    /// <summary>
    /// oncancel
    /// </summary>
    [Description("@#oncancel")]
    public extern EventHandler Oncancel { get; set; }

    /// <summary>
    /// oncanplay
    /// </summary>
    [Description("@#oncanplay")]
    public extern EventHandler Oncanplay { get; set; }

    /// <summary>
    /// oncanplaythrough
    /// </summary>
    [Description("@#oncanplaythrough")]
    public extern EventHandler Oncanplaythrough { get; set; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }

    /// <summary>
    /// onclick
    /// </summary>
    [Description("@#onclick")]
    public extern EventHandler Onclick { get; set; }

    /// <summary>
    /// onclose
    /// </summary>
    [Description("@#onclose")]
    public extern EventHandler Onclose { get; set; }

    /// <summary>
    /// oncommand
    /// </summary>
    [Description("@#oncommand")]
    public extern EventHandler Oncommand { get; set; }

    /// <summary>
    /// oncontextlost
    /// </summary>
    [Description("@#oncontextlost")]
    public extern EventHandler Oncontextlost { get; set; }

    /// <summary>
    /// oncontextmenu
    /// </summary>
    [Description("@#oncontextmenu")]
    public extern EventHandler Oncontextmenu { get; set; }

    /// <summary>
    /// oncontextrestored
    /// </summary>
    [Description("@#oncontextrestored")]
    public extern EventHandler Oncontextrestored { get; set; }

    /// <summary>
    /// oncopy
    /// </summary>
    [Description("@#oncopy")]
    public extern EventHandler Oncopy { get; set; }

    /// <summary>
    /// oncuechange
    /// </summary>
    [Description("@#oncuechange")]
    public extern EventHandler Oncuechange { get; set; }

    /// <summary>
    /// oncut
    /// </summary>
    [Description("@#oncut")]
    public extern EventHandler Oncut { get; set; }

    /// <summary>
    /// ondblclick
    /// </summary>
    [Description("@#ondblclick")]
    public extern EventHandler Ondblclick { get; set; }

    /// <summary>
    /// ondrag
    /// </summary>
    [Description("@#ondrag")]
    public extern EventHandler Ondrag { get; set; }

    /// <summary>
    /// ondragend
    /// </summary>
    [Description("@#ondragend")]
    public extern EventHandler Ondragend { get; set; }

    /// <summary>
    /// ondragenter
    /// </summary>
    [Description("@#ondragenter")]
    public extern EventHandler Ondragenter { get; set; }

    /// <summary>
    /// ondragleave
    /// </summary>
    [Description("@#ondragleave")]
    public extern EventHandler Ondragleave { get; set; }

    /// <summary>
    /// ondragover
    /// </summary>
    [Description("@#ondragover")]
    public extern EventHandler Ondragover { get; set; }

    /// <summary>
    /// ondragstart
    /// </summary>
    [Description("@#ondragstart")]
    public extern EventHandler Ondragstart { get; set; }

    /// <summary>
    /// ondrop
    /// </summary>
    [Description("@#ondrop")]
    public extern EventHandler Ondrop { get; set; }

    /// <summary>
    /// ondurationchange
    /// </summary>
    [Description("@#ondurationchange")]
    public extern EventHandler Ondurationchange { get; set; }

    /// <summary>
    /// onemptied
    /// </summary>
    [Description("@#onemptied")]
    public extern EventHandler Onemptied { get; set; }

    /// <summary>
    /// onended
    /// </summary>
    [Description("@#onended")]
    public extern EventHandler Onended { get; set; }

    /// <summary>
    /// onerror
    /// </summary>
    [Description("@#onerror")]
    public extern OnErrorEventHandler Onerror { get; set; }

    /// <summary>
    /// onfocus
    /// </summary>
    [Description("@#onfocus")]
    public extern EventHandler Onfocus { get; set; }

    /// <summary>
    /// onformdata
    /// </summary>
    [Description("@#onformdata")]
    public extern EventHandler Onformdata { get; set; }

    /// <summary>
    /// oninput
    /// </summary>
    [Description("@#oninput")]
    public extern EventHandler Oninput { get; set; }

    /// <summary>
    /// oninvalid
    /// </summary>
    [Description("@#oninvalid")]
    public extern EventHandler Oninvalid { get; set; }

    /// <summary>
    /// onkeydown
    /// </summary>
    [Description("@#onkeydown")]
    public extern EventHandler Onkeydown { get; set; }

    /// <summary>
    /// onkeypress
    /// </summary>
    [Description("@#onkeypress")]
    public extern EventHandler Onkeypress { get; set; }

    /// <summary>
    /// onkeyup
    /// </summary>
    [Description("@#onkeyup")]
    public extern EventHandler Onkeyup { get; set; }

    /// <summary>
    /// onload
    /// </summary>
    [Description("@#onload")]
    public extern EventHandler Onload { get; set; }

    /// <summary>
    /// onloadeddata
    /// </summary>
    [Description("@#onloadeddata")]
    public extern EventHandler Onloadeddata { get; set; }

    /// <summary>
    /// onloadedmetadata
    /// </summary>
    [Description("@#onloadedmetadata")]
    public extern EventHandler Onloadedmetadata { get; set; }

    /// <summary>
    /// onloadstart
    /// </summary>
    [Description("@#onloadstart")]
    public extern EventHandler Onloadstart { get; set; }

    /// <summary>
    /// onmousedown
    /// </summary>
    [Description("@#onmousedown")]
    public extern EventHandler Onmousedown { get; set; }

    /// <summary>
    /// onmouseenter
    /// </summary>
    [Description("@#onmouseenter")]
    public extern EventHandler Onmouseenter { get; set; }

    /// <summary>
    /// onmouseleave
    /// </summary>
    [Description("@#onmouseleave")]
    public extern EventHandler Onmouseleave { get; set; }

    /// <summary>
    /// onmousemove
    /// </summary>
    [Description("@#onmousemove")]
    public extern EventHandler Onmousemove { get; set; }

    /// <summary>
    /// onmouseout
    /// </summary>
    [Description("@#onmouseout")]
    public extern EventHandler Onmouseout { get; set; }

    /// <summary>
    /// onmouseover
    /// </summary>
    [Description("@#onmouseover")]
    public extern EventHandler Onmouseover { get; set; }

    /// <summary>
    /// onmouseup
    /// </summary>
    [Description("@#onmouseup")]
    public extern EventHandler Onmouseup { get; set; }

    /// <summary>
    /// onpaste
    /// </summary>
    [Description("@#onpaste")]
    public extern EventHandler Onpaste { get; set; }

    /// <summary>
    /// onpause
    /// </summary>
    [Description("@#onpause")]
    public extern EventHandler Onpause { get; set; }

    /// <summary>
    /// onplay
    /// </summary>
    [Description("@#onplay")]
    public extern EventHandler Onplay { get; set; }

    /// <summary>
    /// onplaying
    /// </summary>
    [Description("@#onplaying")]
    public extern EventHandler Onplaying { get; set; }

    /// <summary>
    /// onprogress
    /// </summary>
    [Description("@#onprogress")]
    public extern EventHandler Onprogress { get; set; }

    /// <summary>
    /// onratechange
    /// </summary>
    [Description("@#onratechange")]
    public extern EventHandler Onratechange { get; set; }

    /// <summary>
    /// onreset
    /// </summary>
    [Description("@#onreset")]
    public extern EventHandler Onreset { get; set; }

    /// <summary>
    /// onresize
    /// </summary>
    [Description("@#onresize")]
    public extern EventHandler Onresize { get; set; }

    /// <summary>
    /// onscroll
    /// </summary>
    [Description("@#onscroll")]
    public extern EventHandler Onscroll { get; set; }

    /// <summary>
    /// onscrollend
    /// </summary>
    [Description("@#onscrollend")]
    public extern EventHandler Onscrollend { get; set; }

    /// <summary>
    /// onsecuritypolicyviolation
    /// </summary>
    [Description("@#onsecuritypolicyviolation")]
    public extern EventHandler Onsecuritypolicyviolation { get; set; }

    /// <summary>
    /// onseeked
    /// </summary>
    [Description("@#onseeked")]
    public extern EventHandler Onseeked { get; set; }

    /// <summary>
    /// onseeking
    /// </summary>
    [Description("@#onseeking")]
    public extern EventHandler Onseeking { get; set; }

    /// <summary>
    /// onselect
    /// </summary>
    [Description("@#onselect")]
    public extern EventHandler Onselect { get; set; }

    /// <summary>
    /// onslotchange
    /// </summary>
    [Description("@#onslotchange")]
    public extern EventHandler Onslotchange { get; set; }

    /// <summary>
    /// onstalled
    /// </summary>
    [Description("@#onstalled")]
    public extern EventHandler Onstalled { get; set; }

    /// <summary>
    /// onsubmit
    /// </summary>
    [Description("@#onsubmit")]
    public extern EventHandler Onsubmit { get; set; }

    /// <summary>
    /// onsuspend
    /// </summary>
    [Description("@#onsuspend")]
    public extern EventHandler Onsuspend { get; set; }

    /// <summary>
    /// ontimeupdate
    /// </summary>
    [Description("@#ontimeupdate")]
    public extern EventHandler Ontimeupdate { get; set; }

    /// <summary>
    /// ontoggle
    /// </summary>
    [Description("@#ontoggle")]
    public extern EventHandler Ontoggle { get; set; }

    /// <summary>
    /// onvolumechange
    /// </summary>
    [Description("@#onvolumechange")]
    public extern EventHandler Onvolumechange { get; set; }

    /// <summary>
    /// onwaiting
    /// </summary>
    [Description("@#onwaiting")]
    public extern EventHandler Onwaiting { get; set; }

    /// <summary>
    /// onwebkitanimationend
    /// </summary>
    [Description("@#onwebkitanimationend")]
    public extern EventHandler Onwebkitanimationend { get; set; }

    /// <summary>
    /// onwebkitanimationiteration
    /// </summary>
    [Description("@#onwebkitanimationiteration")]
    public extern EventHandler Onwebkitanimationiteration { get; set; }

    /// <summary>
    /// onwebkitanimationstart
    /// </summary>
    [Description("@#onwebkitanimationstart")]
    public extern EventHandler Onwebkitanimationstart { get; set; }

    /// <summary>
    /// onwebkittransitionend
    /// </summary>
    [Description("@#onwebkittransitionend")]
    public extern EventHandler Onwebkittransitionend { get; set; }

    /// <summary>
    /// onwheel
    /// </summary>
    [Description("@#onwheel")]
    public extern EventHandler Onwheel { get; set; }

    /// <summary>
    /// onpointerover
    /// </summary>
    [Description("@#onpointerover")]
    public extern EventHandler Onpointerover { get; set; }

    /// <summary>
    /// onpointerenter
    /// </summary>
    [Description("@#onpointerenter")]
    public extern EventHandler Onpointerenter { get; set; }

    /// <summary>
    /// onpointerdown
    /// </summary>
    [Description("@#onpointerdown")]
    public extern EventHandler Onpointerdown { get; set; }

    /// <summary>
    /// onpointermove
    /// </summary>
    [Description("@#onpointermove")]
    public extern EventHandler Onpointermove { get; set; }

    /// <summary>
    /// onpointerrawupdate
    /// </summary>
    [Description("@#onpointerrawupdate")]
    public extern EventHandler Onpointerrawupdate { get; set; }

    /// <summary>
    /// onpointerup
    /// </summary>
    [Description("@#onpointerup")]
    public extern EventHandler Onpointerup { get; set; }

    /// <summary>
    /// onpointercancel
    /// </summary>
    [Description("@#onpointercancel")]
    public extern EventHandler Onpointercancel { get; set; }

    /// <summary>
    /// onpointerout
    /// </summary>
    [Description("@#onpointerout")]
    public extern EventHandler Onpointerout { get; set; }

    /// <summary>
    /// onpointerleave
    /// </summary>
    [Description("@#onpointerleave")]
    public extern EventHandler Onpointerleave { get; set; }

    /// <summary>
    /// ongotpointercapture
    /// </summary>
    [Description("@#ongotpointercapture")]
    public extern EventHandler Ongotpointercapture { get; set; }

    /// <summary>
    /// onlostpointercapture
    /// </summary>
    [Description("@#onlostpointercapture")]
    public extern EventHandler Onlostpointercapture { get; set; }

    /// <summary>
    /// onselectstart
    /// </summary>
    [Description("@#onselectstart")]
    public extern EventHandler Onselectstart { get; set; }

    /// <summary>
    /// onselectionchange
    /// </summary>
    [Description("@#onselectionchange")]
    public extern EventHandler Onselectionchange { get; set; }

    /// <summary>
    /// ontouchstart
    /// </summary>
    [Description("@#ontouchstart")]
    public extern EventHandler Ontouchstart { get; set; }

    /// <summary>
    /// ontouchend
    /// </summary>
    [Description("@#ontouchend")]
    public extern EventHandler Ontouchend { get; set; }

    /// <summary>
    /// ontouchmove
    /// </summary>
    [Description("@#ontouchmove")]
    public extern EventHandler Ontouchmove { get; set; }

    /// <summary>
    /// ontouchcancel
    /// </summary>
    [Description("@#ontouchcancel")]
    public extern EventHandler Ontouchcancel { get; set; }

    /// <summary>
    /// onbeforexrselect
    /// </summary>
    [Description("@#onbeforexrselect")]
    public extern EventHandler Onbeforexrselect { get; set; }
    #endregion

    #region mixin WindowEventHandlers
    /// <summary>
    /// ongamepadconnected
    /// </summary>
    [Description("@#ongamepadconnected")]
    public extern EventHandler Ongamepadconnected { get; set; }

    /// <summary>
    /// ongamepaddisconnected
    /// </summary>
    [Description("@#ongamepaddisconnected")]
    public extern EventHandler Ongamepaddisconnected { get; set; }

    /// <summary>
    /// onafterprint
    /// </summary>
    [Description("@#onafterprint")]
    public extern EventHandler Onafterprint { get; set; }

    /// <summary>
    /// onbeforeprint
    /// </summary>
    [Description("@#onbeforeprint")]
    public extern EventHandler Onbeforeprint { get; set; }

    /// <summary>
    /// onbeforeunload
    /// </summary>
    [Description("@#onbeforeunload")]
    public extern OnBeforeUnloadEventHandler Onbeforeunload { get; set; }

    /// <summary>
    /// onhashchange
    /// </summary>
    [Description("@#onhashchange")]
    public extern EventHandler Onhashchange { get; set; }

    /// <summary>
    /// onlanguagechange
    /// </summary>
    [Description("@#onlanguagechange")]
    public extern EventHandler Onlanguagechange { get; set; }

    /// <summary>
    /// onmessage
    /// </summary>
    [Description("@#onmessage")]
    public extern EventHandler Onmessage { get; set; }

    /// <summary>
    /// onmessageerror
    /// </summary>
    [Description("@#onmessageerror")]
    public extern EventHandler Onmessageerror { get; set; }

    /// <summary>
    /// onoffline
    /// </summary>
    [Description("@#onoffline")]
    public extern EventHandler Onoffline { get; set; }

    /// <summary>
    /// ononline
    /// </summary>
    [Description("@#ononline")]
    public extern EventHandler Ononline { get; set; }

    /// <summary>
    /// onpagehide
    /// </summary>
    [Description("@#onpagehide")]
    public extern EventHandler Onpagehide { get; set; }

    /// <summary>
    /// onpagereveal
    /// </summary>
    [Description("@#onpagereveal")]
    public extern EventHandler Onpagereveal { get; set; }

    /// <summary>
    /// onpageshow
    /// </summary>
    [Description("@#onpageshow")]
    public extern EventHandler Onpageshow { get; set; }

    /// <summary>
    /// onpageswap
    /// </summary>
    [Description("@#onpageswap")]
    public extern EventHandler Onpageswap { get; set; }

    /// <summary>
    /// onpopstate
    /// </summary>
    [Description("@#onpopstate")]
    public extern EventHandler Onpopstate { get; set; }

    /// <summary>
    /// onrejectionhandled
    /// </summary>
    [Description("@#onrejectionhandled")]
    public extern EventHandler Onrejectionhandled { get; set; }

    /// <summary>
    /// onstorage
    /// </summary>
    [Description("@#onstorage")]
    public extern EventHandler Onstorage { get; set; }

    /// <summary>
    /// onunhandledrejection
    /// </summary>
    [Description("@#onunhandledrejection")]
    public extern EventHandler Onunhandledrejection { get; set; }

    /// <summary>
    /// onunload
    /// </summary>
    [Description("@#onunload")]
    public extern EventHandler Onunload { get; set; }

    /// <summary>
    /// onportalactivate
    /// </summary>
    [Description("@#onportalactivate")]
    public extern EventHandler Onportalactivate { get; set; }
    #endregion

    #region mixin WindowOrWorkerGlobalScope
    /// <summary>
    /// fetch
    /// </summary>
    /// <param name="input">input</param>
    /// <param name="init">init</param>
    [Description("@#fetch")]
    public extern PromiseResult<Response> Fetch(RequestInfo input, RequestInit? init = default);

    /// <summary>
    /// performance
    /// </summary>
    [Description("@#performance")]
    public extern Performance Performance { get; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// isSecureContext
    /// </summary>
    [Description("@#isSecureContext")]
    public extern bool IsSecureContext { get; }

    /// <summary>
    /// crossOriginIsolated
    /// </summary>
    [Description("@#crossOriginIsolated")]
    public extern bool CrossOriginIsolated { get; }

    /// <summary>
    /// reportError
    /// </summary>
    /// <param name="e">e</param>
    [Description("@#reportError")]
    public extern void ReportError(object e);

    /// <summary>
    /// btoa
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#btoa")]
    public extern string Btoa(string data);

    /// <summary>
    /// atob
    /// </summary>
    /// <param name="data">data</param>
    [Description("@#atob")]
    public extern byte[] Atob(string data);

    /// <summary>
    /// setTimeout
    /// </summary>
    /// <param name="handler">handler</param>
    /// <param name="timeout">timeout</param>
    /// <param name="arguments">arguments</param>
    [Description("@#setTimeout")]
    public extern int SetTimeout(TimerHandler handler, int timeout = 0, object? arguments = default);

    /// <summary>
    /// clearTimeout
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#clearTimeout")]
    public extern void ClearTimeout(int id = 0);

    /// <summary>
    /// setInterval
    /// </summary>
    /// <param name="handler">handler</param>
    /// <param name="timeout">timeout</param>
    /// <param name="arguments">arguments</param>
    [Description("@#setInterval")]
    public extern int SetInterval(TimerHandler handler, int timeout = 0, object? arguments = default);

    /// <summary>
    /// clearInterval
    /// </summary>
    /// <param name="id">id</param>
    [Description("@#clearInterval")]
    public extern void ClearInterval(int id = 0);

    /// <summary>
    /// queueMicrotask
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#queueMicrotask")]
    public extern void QueueMicrotask(Action callback);

    /// <summary>
    /// createImageBitmap
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="options">options</param>
    [Description("@#createImageBitmap")]
    public extern PromiseResult<ImageBitmap> CreateImageBitmap(ImageBitmapSource image, ImageBitmapOptions? options = default);

    /// <summary>
    /// createImageBitmap
    /// </summary>
    /// <param name="image">image</param>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    /// <param name="sw">sw</param>
    /// <param name="sh">sh</param>
    /// <param name="options">options</param>
    [Description("@#createImageBitmap")]
    public extern PromiseResult<ImageBitmap> CreateImageBitmap(ImageBitmapSource image, int sx, int sy, int sw, int sh, ImageBitmapOptions? options = default);

    /// <summary>
    /// structuredClone
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="options">options</param>
    [Description("@#structuredClone")]
    public extern object StructuredClone(object value, StructuredSerializeOptions? options = default);

    /// <summary>
    /// indexedDB
    /// </summary>
    [Description("@#indexedDB")]
    public extern IDBFactory IndexedDB { get; }

    /// <summary>
    /// scheduler
    /// </summary>
    [Description("@#scheduler")]
    public extern Scheduler Scheduler { get; }

    /// <summary>
    /// caches
    /// </summary>
    [Description("@#caches")]
    public extern CacheStorage Caches { get; }

    /// <summary>
    /// trustedTypes
    /// </summary>
    [Description("@#trustedTypes")]
    public extern TrustedTypePolicyFactory TrustedTypes { get; }

    /// <summary>
    /// crypto
    /// </summary>
    [Description("@#crypto")]
    public extern Crypto Crypto { get; }
    #endregion

    #region mixin AnimationFrameProvider
    /// <summary>
    /// requestAnimationFrame
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#requestAnimationFrame")]
    public extern uint RequestAnimationFrame(FrameRequestCallback callback);

    /// <summary>
    /// cancelAnimationFrame
    /// </summary>
    /// <param name="handle">handle</param>
    [Description("@#cancelAnimationFrame")]
    public extern void CancelAnimationFrame(uint handle);
    #endregion

    #region mixin WindowSessionStorage
    /// <summary>
    /// sessionStorage
    /// </summary>
    [Description("@#sessionStorage")]
    public extern Storage SessionStorage { get; }
    #endregion

    #region mixin WindowLocalStorage
    /// <summary>
    /// localStorage
    /// </summary>
    [Description("@#localStorage")]
    public extern Storage LocalStorage { get; }
    #endregion
}