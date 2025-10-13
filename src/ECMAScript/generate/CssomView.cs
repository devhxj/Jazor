namespace ECMAScript;

/// <summary>
/// ScrollOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollOptions")]
public record ScrollOptions(
    [property: Description("@#behavior")]ScrollBehavior Behavior = ScrollBehavior.Auto);

/// <summary>
/// ScrollToOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollToOptions")]
public record ScrollToOptions(
    [property: Description("@#left")]double Left = default,
	[property: Description("@#top")]double Top = default): ScrollOptions;

/// <summary>
/// MediaQueryListEventInit
/// </summary>
[ECMAScript]
[Description("@#MediaQueryListEventInit")]
public record MediaQueryListEventInit(
    [property: Description("@#media")]string? Media = default,
	[property: Description("@#matches")]bool Matches = false): EventInit;

/// <summary>
/// CaretPositionFromPointOptions
/// </summary>
[ECMAScript]
[Description("@#CaretPositionFromPointOptions")]
public record CaretPositionFromPointOptions(
    [property: Description("@#shadowRoots")]ShadowRoot[]? ShadowRoots = default);

/// <summary>
/// ScrollIntoViewOptions
/// </summary>
[ECMAScript]
[Description("@#ScrollIntoViewOptions")]
public record ScrollIntoViewOptions(
    [property: Description("@#block")]ScrollLogicalPosition Block = ScrollLogicalPosition.Start,
	[property: Description("@#inline")]ScrollLogicalPosition Inline = ScrollLogicalPosition.Nearest,
	[property: Description("@#container")]ScrollIntoViewContainer Container = ScrollIntoViewContainer.All): ScrollOptions;

/// <summary>
/// CheckVisibilityOptions
/// </summary>
[ECMAScript]
[Description("@#CheckVisibilityOptions")]
public record CheckVisibilityOptions(
    [property: Description("@#checkOpacity")]bool CheckOpacity = false,
	[property: Description("@#checkVisibilityCSS")]bool CheckVisibilityCSS = false,
	[property: Description("@#contentVisibilityAuto")]bool ContentVisibilityAuto = false,
	[property: Description("@#opacityProperty")]bool OpacityProperty = false,
	[property: Description("@#visibilityProperty")]bool VisibilityProperty = false);

/// <summary>
/// BoxQuadOptions
/// </summary>
[ECMAScript]
[Description("@#BoxQuadOptions")]
public record BoxQuadOptions(
    [property: Description("@#box")]CSSBoxType Box = CSSBoxType.Border,
	[property: Description("@#relativeTo")]GeometryNode? RelativeTo = default);

/// <summary>
/// ConvertCoordinateOptions
/// </summary>
[ECMAScript]
[Description("@#ConvertCoordinateOptions")]
public record ConvertCoordinateOptions(
    [property: Description("@#fromBox")]CSSBoxType FromBox = CSSBoxType.Border,
	[property: Description("@#toBox")]CSSBoxType ToBox = CSSBoxType.Border);

/// <summary>
/// MediaQueryList
/// </summary>
[ECMAScript]
[Description("@#MediaQueryList")]
public class MediaQueryList : EventTarget
{
    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern string Media { get; }

    /// <summary>
    /// matches
    /// </summary>
    [Description("@#matches")]
    public extern bool Matches { get; }

    /// <summary>
    /// addListener
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#addListener")]
    public extern void AddListener(EventListener? callback);

    /// <summary>
    /// removeListener
    /// </summary>
    /// <param name="callback">callback</param>
    [Description("@#removeListener")]
    public extern void RemoveListener(EventListener? callback);

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}

/// <summary>
/// MediaQueryListEvent
/// </summary>
[ECMAScript]
[Description("@#MediaQueryListEvent")]
public class MediaQueryListEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MediaQueryListEvent(string type, MediaQueryListEventInit eventInitDict);

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern string Media { get; }

    /// <summary>
    /// matches
    /// </summary>
    [Description("@#matches")]
    public extern bool Matches { get; }
}

/// <summary>
/// Screen
/// </summary>
[ECMAScript]
[Description("@#Screen")]
public partial class Screen
{
    /// <summary>
    /// availWidth
    /// </summary>
    [Description("@#availWidth")]
    public extern int AvailWidth { get; }

    /// <summary>
    /// availHeight
    /// </summary>
    [Description("@#availHeight")]
    public extern int AvailHeight { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern int Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern int Height { get; }

    /// <summary>
    /// colorDepth
    /// </summary>
    [Description("@#colorDepth")]
    public extern uint ColorDepth { get; }

    /// <summary>
    /// pixelDepth
    /// </summary>
    [Description("@#pixelDepth")]
    public extern uint PixelDepth { get; }

    /// <summary>
    /// orientation
    /// </summary>
    [Description("@#orientation")]
    public extern ScreenOrientation Orientation { get; }

    /// <summary>
    /// isExtended
    /// </summary>
    [Description("@#isExtended")]
    public extern bool IsExtended { get; }

    /// <summary>
    /// onchange
    /// </summary>
    [Description("@#onchange")]
    public extern EventHandler Onchange { get; set; }
}

/// <summary>
/// CaretPosition
/// </summary>
[ECMAScript]
[Description("@#CaretPosition")]
public class CaretPosition
{
    /// <summary>
    /// offsetNode
    /// </summary>
    [Description("@#offsetNode")]
    public extern Node OffsetNode { get; }

    /// <summary>
    /// offset
    /// </summary>
    [Description("@#offset")]
    public extern uint Offset { get; }

    /// <summary>
    /// getClientRect
    /// </summary>
    [Description("@#getClientRect")]
    public extern DOMRect? GetClientRect();
}

/// <summary>
/// HTMLElement
/// </summary>
[ECMAScript]
[Description("@#HTMLElement")]
public partial class HTMLElement : Element
{
    /// <summary>
    /// scrollParent
    /// </summary>
    [Description("@#scrollParent")]
    public extern Element? ScrollParent { get; }

    /// <summary>
    /// offsetParent
    /// </summary>
    [Description("@#offsetParent")]
    public extern Element? OffsetParent { get; }

    /// <summary>
    /// offsetTop
    /// </summary>
    [Description("@#offsetTop")]
    public extern int OffsetTop { get; }

    /// <summary>
    /// offsetLeft
    /// </summary>
    [Description("@#offsetLeft")]
    public extern int OffsetLeft { get; }

    /// <summary>
    /// offsetWidth
    /// </summary>
    [Description("@#offsetWidth")]
    public extern int OffsetWidth { get; }

    /// <summary>
    /// offsetHeight
    /// </summary>
    [Description("@#offsetHeight")]
    public extern int OffsetHeight { get; }

    /// <summary>
    /// editContext
    /// </summary>
    [Description("@#editContext")]
    public extern EditContext? EditContext { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLElement();

    /// <summary>
    /// title
    /// </summary>
    [Description("@#title")]
    public extern string Title { get; set; }

    /// <summary>
    /// lang
    /// </summary>
    [Description("@#lang")]
    public extern string Lang { get; set; }

    /// <summary>
    /// translate
    /// </summary>
    [Description("@#translate")]
    public extern bool Translate { get; set; }

    /// <summary>
    /// dir
    /// </summary>
    [Description("@#dir")]
    public extern string Dir { get; set; }

    /// <summary>
    /// hidden
    /// </summary>
    [Description("@#hidden")]
    public extern Either<bool, double, string>? Hidden { get; set; }

    /// <summary>
    /// inert
    /// </summary>
    [Description("@#inert")]
    public extern bool Inert { get; set; }

    /// <summary>
    /// click
    /// </summary>
    [Description("@#click")]
    public extern void Click();

    /// <summary>
    /// accessKey
    /// </summary>
    [Description("@#accessKey")]
    public extern string AccessKey { get; set; }

    /// <summary>
    /// accessKeyLabel
    /// </summary>
    [Description("@#accessKeyLabel")]
    public extern string AccessKeyLabel { get; }

    /// <summary>
    /// draggable
    /// </summary>
    [Description("@#draggable")]
    public extern bool Draggable { get; set; }

    /// <summary>
    /// spellcheck
    /// </summary>
    [Description("@#spellcheck")]
    public extern bool Spellcheck { get; set; }

    /// <summary>
    /// writingSuggestions
    /// </summary>
    [Description("@#writingSuggestions")]
    public extern string WritingSuggestions { get; set; }

    /// <summary>
    /// autocapitalize
    /// </summary>
    [Description("@#autocapitalize")]
    public extern string Autocapitalize { get; set; }

    /// <summary>
    /// autocorrect
    /// </summary>
    [Description("@#autocorrect")]
    public extern bool Autocorrect { get; set; }

    /// <summary>
    /// innerText
    /// </summary>
    [Description("@#innerText")]
    public extern string InnerText { get; set; }

    /// <summary>
    /// outerText
    /// </summary>
    [Description("@#outerText")]
    public extern string OuterText { get; set; }

    /// <summary>
    /// attachInternals
    /// </summary>
    [Description("@#attachInternals")]
    public extern ElementInternals AttachInternals();

    /// <summary>
    /// showPopover
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#showPopover")]
    public extern void ShowPopover(ShowPopoverOptions? options = default);

    /// <summary>
    /// hidePopover
    /// </summary>
    [Description("@#hidePopover")]
    public extern void HidePopover();

    /// <summary>
    /// togglePopover
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#togglePopover")]
    public extern bool TogglePopover(Either<TogglePopoverOptions, bool> options);

    /// <summary>
    /// togglePopover
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#togglePopover")]
    public extern bool TogglePopover(TogglePopoverOptions? options = default);

    /// <summary>
    /// togglePopover
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#togglePopover")]
    public extern bool TogglePopover(bool options);

    /// <summary>
    /// popover
    /// </summary>
    [Description("@#popover")]
    public extern string? Popover { get; set; }

    #region mixin ElementCSSInlineStyle
    /// <summary>
    /// attributeStyleMap
    /// </summary>
    [Description("@#attributeStyleMap")]
    public extern StylePropertyMap AttributeStyleMap { get; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSStyleDeclaration Style { get; }
    #endregion

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

    #region mixin ElementContentEditable
    /// <summary>
    /// contentEditable
    /// </summary>
    [Description("@#contentEditable")]
    public extern string ContentEditable { get; set; }

    /// <summary>
    /// enterKeyHint
    /// </summary>
    [Description("@#enterKeyHint")]
    public extern string EnterKeyHint { get; set; }

    /// <summary>
    /// isContentEditable
    /// </summary>
    [Description("@#isContentEditable")]
    public extern bool IsContentEditable { get; }

    /// <summary>
    /// inputMode
    /// </summary>
    [Description("@#inputMode")]
    public extern string InputMode { get; set; }

    /// <summary>
    /// virtualKeyboardPolicy
    /// </summary>
    [Description("@#virtualKeyboardPolicy")]
    public extern string VirtualKeyboardPolicy { get; set; }
    #endregion

    #region mixin HTMLOrSVGElement
    /// <summary>
    /// dataset
    /// </summary>
    [Description("@#dataset")]
    public extern DOMStringMap Dataset { get; }

    /// <summary>
    /// nonce
    /// </summary>
    [Description("@#nonce")]
    public extern string Nonce { get; set; }

    /// <summary>
    /// autofocus
    /// </summary>
    [Description("@#autofocus")]
    public extern bool Autofocus { get; set; }

    /// <summary>
    /// tabIndex
    /// </summary>
    [Description("@#tabIndex")]
    public extern int TabIndex { get; set; }

    /// <summary>
    /// focus
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#focus")]
    public extern void Focus(FocusOptions? options = default);

    /// <summary>
    /// blur
    /// </summary>
    [Description("@#blur")]
    public extern void Blur();
    #endregion
}

/// <summary>
/// HTMLImageElement
/// </summary>
[ECMAScript]
[Description("@#HTMLImageElement")]
public partial class HTMLImageElement : HTMLElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern int X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern int Y { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLImageElement();

    /// <summary>
    /// alt
    /// </summary>
    [Description("@#alt")]
    public extern string Alt { get; set; }

    /// <summary>
    /// src
    /// </summary>
    [Description("@#src")]
    public extern string Src { get; set; }

    /// <summary>
    /// srcset
    /// </summary>
    [Description("@#srcset")]
    public extern string Srcset { get; set; }

    /// <summary>
    /// sizes
    /// </summary>
    [Description("@#sizes")]
    public extern string Sizes { get; set; }

    /// <summary>
    /// crossOrigin
    /// </summary>
    [Description("@#crossOrigin")]
    public extern string? CrossOrigin { get; set; }

    /// <summary>
    /// useMap
    /// </summary>
    [Description("@#useMap")]
    public extern string UseMap { get; set; }

    /// <summary>
    /// isMap
    /// </summary>
    [Description("@#isMap")]
    public extern bool IsMap { get; set; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern uint Width { get; set; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern uint Height { get; set; }

    /// <summary>
    /// naturalWidth
    /// </summary>
    [Description("@#naturalWidth")]
    public extern uint NaturalWidth { get; }

    /// <summary>
    /// naturalHeight
    /// </summary>
    [Description("@#naturalHeight")]
    public extern uint NaturalHeight { get; }

    /// <summary>
    /// complete
    /// </summary>
    [Description("@#complete")]
    public extern bool Complete { get; }

    /// <summary>
    /// currentSrc
    /// </summary>
    [Description("@#currentSrc")]
    public extern string CurrentSrc { get; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// decoding
    /// </summary>
    [Description("@#decoding")]
    public extern string Decoding { get; set; }

    /// <summary>
    /// loading
    /// </summary>
    [Description("@#loading")]
    public extern string Loading { get; set; }

    /// <summary>
    /// fetchPriority
    /// </summary>
    [Description("@#fetchPriority")]
    public extern string FetchPriority { get; set; }

    /// <summary>
    /// decode
    /// </summary>
    [Description("@#decode")]
    public extern PromiseResult<object> Decode();

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; set; }

    /// <summary>
    /// lowsrc
    /// </summary>
    [Description("@#lowsrc")]
    public extern string Lowsrc { get; set; }

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern string Align { get; set; }

    /// <summary>
    /// hspace
    /// </summary>
    [Description("@#hspace")]
    public extern uint Hspace { get; set; }

    /// <summary>
    /// vspace
    /// </summary>
    [Description("@#vspace")]
    public extern uint Vspace { get; set; }

    /// <summary>
    /// longDesc
    /// </summary>
    [Description("@#longDesc")]
    public extern string LongDesc { get; set; }

    /// <summary>
    /// border
    /// </summary>
    [Description("@#border")]
    public extern string Border { get; set; }

    #region mixin HTMLAttributionSrcElementUtils
    /// <summary>
    /// attributionSrc
    /// </summary>
    [Description("@#attributionSrc")]
    public extern string AttributionSrc { get; set; }
    #endregion

    #region mixin HTMLSharedStorageWritableElementUtils
    /// <summary>
    /// sharedStorageWritable
    /// </summary>
    [Description("@#sharedStorageWritable")]
    public extern bool SharedStorageWritable { get; set; }
    #endregion
}

/// <summary>
/// Range
/// </summary>
[ECMAScript]
[Description("@#Range")]
public partial class Range : AbstractRange
{
    /// <summary>
    /// getClientRects
    /// </summary>
    [Description("@#getClientRects")]
    public extern DOMRectList GetClientRects();

    /// <summary>
    /// getBoundingClientRect
    /// </summary>
    [Description("@#getBoundingClientRect")]
    public extern DOMRect GetBoundingClientRect();

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern Range();

    /// <summary>
    /// commonAncestorContainer
    /// </summary>
    [Description("@#commonAncestorContainer")]
    public extern Node CommonAncestorContainer { get; }

    /// <summary>
    /// setStart
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="offset">offset</param>
    [Description("@#setStart")]
    public extern void SetStart(Node node, uint offset);

    /// <summary>
    /// setEnd
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="offset">offset</param>
    [Description("@#setEnd")]
    public extern void SetEnd(Node node, uint offset);

    /// <summary>
    /// setStartBefore
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#setStartBefore")]
    public extern void SetStartBefore(Node node);

    /// <summary>
    /// setStartAfter
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#setStartAfter")]
    public extern void SetStartAfter(Node node);

    /// <summary>
    /// setEndBefore
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#setEndBefore")]
    public extern void SetEndBefore(Node node);

    /// <summary>
    /// setEndAfter
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#setEndAfter")]
    public extern void SetEndAfter(Node node);

    /// <summary>
    /// collapse
    /// </summary>
    /// <param name="toStart">toStart</param>
    [Description("@#collapse")]
    public extern void Collapse(bool toStart = false);

    /// <summary>
    /// selectNode
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#selectNode")]
    public extern void SelectNode(Node node);

    /// <summary>
    /// selectNodeContents
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#selectNodeContents")]
    public extern void SelectNodeContents(Node node);

    /// <summary>
    /// START_TO_START
    /// </summary>
    [Description("@#START_TO_START")]
    public const ushort START_TO_START = 0;

    /// <summary>
    /// START_TO_END
    /// </summary>
    [Description("@#START_TO_END")]
    public const ushort START_TO_END = 1;

    /// <summary>
    /// END_TO_END
    /// </summary>
    [Description("@#END_TO_END")]
    public const ushort END_TO_END = 2;

    /// <summary>
    /// END_TO_START
    /// </summary>
    [Description("@#END_TO_START")]
    public const ushort END_TO_START = 3;

    /// <summary>
    /// compareBoundaryPoints
    /// </summary>
    /// <param name="how">how</param>
    /// <param name="sourceRange">sourceRange</param>
    [Description("@#compareBoundaryPoints")]
    public extern short CompareBoundaryPoints(ushort how, Range sourceRange);

    /// <summary>
    /// deleteContents
    /// </summary>
    [Description("@#deleteContents")]
    public extern void DeleteContents();

    /// <summary>
    /// extractContents
    /// </summary>
    [Description("@#extractContents")]
    public extern DocumentFragment ExtractContents();

    /// <summary>
    /// cloneContents
    /// </summary>
    [Description("@#cloneContents")]
    public extern DocumentFragment CloneContents();

    /// <summary>
    /// insertNode
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#insertNode")]
    public extern void InsertNode(Node node);

    /// <summary>
    /// surroundContents
    /// </summary>
    /// <param name="newParent">newParent</param>
    [Description("@#surroundContents")]
    public extern void SurroundContents(Node newParent);

    /// <summary>
    /// cloneRange
    /// </summary>
    [Description("@#cloneRange")]
    public extern Range CloneRange();

    /// <summary>
    /// detach
    /// </summary>
    [Description("@#detach")]
    public extern void Detach();

    /// <summary>
    /// isPointInRange
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="offset">offset</param>
    [Description("@#isPointInRange")]
    public extern bool IsPointInRange(Node node, uint offset);

    /// <summary>
    /// comparePoint
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="offset">offset</param>
    [Description("@#comparePoint")]
    public extern short ComparePoint(Node node, uint offset);

    /// <summary>
    /// intersectsNode
    /// </summary>
    /// <param name="node">node</param>
    [Description("@#intersectsNode")]
    public extern bool IntersectsNode(Node node);

    /// <summary>
    /// createContextualFragment
    /// </summary>
    /// <param name="string">string</param>
    [Description("@#createContextualFragment")]
    public extern DocumentFragment CreateContextualFragment(Either<TrustedHTML, string> @string);

    /// <summary>
    /// createContextualFragment
    /// </summary>
    /// <param name="string">string</param>
    [Description("@#createContextualFragment")]
    public extern DocumentFragment CreateContextualFragment(TrustedHTML @string);

    /// <summary>
    /// createContextualFragment
    /// </summary>
    /// <param name="string">string</param>
    [Description("@#createContextualFragment")]
    public extern DocumentFragment CreateContextualFragment(string @string);
}

/// <summary>
/// MouseEvent
/// </summary>
[ECMAScript]
[Description("@#MouseEvent")]
public partial class MouseEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// pageX
    /// </summary>
    [Description("@#pageX")]
    public extern double PageX { get; }

    /// <summary>
    /// pageY
    /// </summary>
    [Description("@#pageY")]
    public extern double PageY { get; }

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern double X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern double Y { get; }

    /// <summary>
    /// offsetX
    /// </summary>
    [Description("@#offsetX")]
    public extern double OffsetX { get; }

    /// <summary>
    /// offsetY
    /// </summary>
    [Description("@#offsetY")]
    public extern double OffsetY { get; }

    /// <summary>
    /// movementX
    /// </summary>
    [Description("@#movementX")]
    public extern double MovementX { get; }

    /// <summary>
    /// movementY
    /// </summary>
    [Description("@#movementY")]
    public extern double MovementY { get; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern MouseEvent(string type, MouseEventInit eventInitDict);

    /// <summary>
    /// screenX
    /// </summary>
    [Description("@#screenX")]
    public extern int ScreenX { get; }

    /// <summary>
    /// screenY
    /// </summary>
    [Description("@#screenY")]
    public extern int ScreenY { get; }

    /// <summary>
    /// clientX
    /// </summary>
    [Description("@#clientX")]
    public extern int ClientX { get; }

    /// <summary>
    /// clientY
    /// </summary>
    [Description("@#clientY")]
    public extern int ClientY { get; }

    /// <summary>
    /// layerX
    /// </summary>
    [Description("@#layerX")]
    public extern int LayerX { get; }

    /// <summary>
    /// layerY
    /// </summary>
    [Description("@#layerY")]
    public extern int LayerY { get; }

    /// <summary>
    /// ctrlKey
    /// </summary>
    [Description("@#ctrlKey")]
    public extern bool CtrlKey { get; }

    /// <summary>
    /// shiftKey
    /// </summary>
    [Description("@#shiftKey")]
    public extern bool ShiftKey { get; }

    /// <summary>
    /// altKey
    /// </summary>
    [Description("@#altKey")]
    public extern bool AltKey { get; }

    /// <summary>
    /// metaKey
    /// </summary>
    [Description("@#metaKey")]
    public extern bool MetaKey { get; }

    /// <summary>
    /// button
    /// </summary>
    [Description("@#button")]
    public extern short Button { get; }

    /// <summary>
    /// buttons
    /// </summary>
    [Description("@#buttons")]
    public extern ushort Buttons { get; }

    /// <summary>
    /// relatedTarget
    /// </summary>
    [Description("@#relatedTarget")]
    public extern EventTarget? RelatedTarget { get; }

    /// <summary>
    /// getModifierState
    /// </summary>
    /// <param name="keyArg">keyArg</param>
    [Description("@#getModifierState")]
    public extern bool GetModifierState(string keyArg);

    /// <summary>
    /// initMouseEvent
    /// </summary>
    /// <param name="typeArg">typeArg</param>
    /// <param name="bubblesArg">bubblesArg</param>
    /// <param name="cancelableArg">cancelableArg</param>
    /// <param name="viewArg">viewArg</param>
    /// <param name="detailArg">detailArg</param>
    /// <param name="screenXarg">screenXArg</param>
    /// <param name="screenYarg">screenYArg</param>
    /// <param name="clientXarg">clientXArg</param>
    /// <param name="clientYarg">clientYArg</param>
    /// <param name="ctrlKeyArg">ctrlKeyArg</param>
    /// <param name="altKeyArg">altKeyArg</param>
    /// <param name="shiftKeyArg">shiftKeyArg</param>
    /// <param name="metaKeyArg">metaKeyArg</param>
    /// <param name="buttonArg">buttonArg</param>
    /// <param name="relatedTargetArg">relatedTargetArg</param>
    [Description("@#initMouseEvent")]
    public extern void InitMouseEvent(string typeArg, bool bubblesArg = false, bool cancelableArg = false, Window? viewArg = null, int detailArg = 0, int screenXarg = 0, int screenYarg = 0, int clientXarg = 0, int clientYarg = 0, bool ctrlKeyArg = false, bool altKeyArg = false, bool shiftKeyArg = false, bool metaKeyArg = false, short buttonArg = 0, EventTarget? relatedTargetArg = null);
}

/// <summary>
/// VisualViewport
/// </summary>
[ECMAScript]
[Description("@#VisualViewport")]
public class VisualViewport : EventTarget
{
    /// <summary>
    /// offsetLeft
    /// </summary>
    [Description("@#offsetLeft")]
    public extern double OffsetLeft { get; }

    /// <summary>
    /// offsetTop
    /// </summary>
    [Description("@#offsetTop")]
    public extern double OffsetTop { get; }

    /// <summary>
    /// pageLeft
    /// </summary>
    [Description("@#pageLeft")]
    public extern double PageLeft { get; }

    /// <summary>
    /// pageTop
    /// </summary>
    [Description("@#pageTop")]
    public extern double PageTop { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern double Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern double Height { get; }

    /// <summary>
    /// scale
    /// </summary>
    [Description("@#scale")]
    public extern double Scale { get; }

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
}