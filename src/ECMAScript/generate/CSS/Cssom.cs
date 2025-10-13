namespace ECMAScript.CSS;

/// <summary>
/// CSSStyleSheetInit
/// </summary>
[ECMAScript]
[Description("@#CSSStyleSheetInit")]
public record CSSStyleSheetInit(
    [property: Description("@#baseURL")]string? BaseURL = default,
	[property: Description("@#media")]Either<MediaList, string>? Media = default,
	[property: Description("@#disabled")]bool Disabled = false);

/// <summary>
/// MediaList
/// </summary>
[ECMAScript]
[Description("@#MediaList")]
public class MediaList
{
    /// <summary>
    /// mediaText
    /// </summary>
    [Description("@#mediaText")]
    public extern string MediaText { get; set; }

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern string? GetItem(uint index);

    /// <summary>
    /// appendMedium
    /// </summary>
    /// <param name="medium">medium</param>
    [Description("@#appendMedium")]
    public extern void AppendMedium(string medium);

    /// <summary>
    /// deleteMedium
    /// </summary>
    /// <param name="medium">medium</param>
    [Description("@#deleteMedium")]
    public extern void DeleteMedium(string medium);
}

/// <summary>
/// StyleSheet
/// </summary>
[ECMAScript]
[Description("@#StyleSheet")]
public class StyleSheet
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; }

    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string? Href { get; }

    /// <summary>
    /// ownerNode
    /// </summary>
    [Description("@#ownerNode")]
    public extern Either<Element, ProcessingInstruction>? OwnerNode { get; }

    /// <summary>
    /// parentStyleSheet
    /// </summary>
    [Description("@#parentStyleSheet")]
    public extern CSSStyleSheet? ParentStyleSheet { get; }

    /// <summary>
    /// title
    /// </summary>
    [Description("@#title")]
    public extern string? Title { get; }

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern MediaList Media { get; }

    /// <summary>
    /// disabled
    /// </summary>
    [Description("@#disabled")]
    public extern bool Disabled { get; set; }
}

/// <summary>
/// CSSStyleSheet
/// </summary>
[ECMAScript]
[Description("@#CSSStyleSheet")]
public partial class CSSStyleSheet : StyleSheet
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern CSSStyleSheet(CSSStyleSheetInit options);

    /// <summary>
    /// ownerRule
    /// </summary>
    [Description("@#ownerRule")]
    public extern CSSRule? OwnerRule { get; }

    /// <summary>
    /// cssRules
    /// </summary>
    [Description("@#cssRules")]
    public extern CSSRuleList CssRules { get; }

    /// <summary>
    /// insertRule
    /// </summary>
    /// <param name="rule">rule</param>
    /// <param name="index">index</param>
    [Description("@#insertRule")]
    public extern uint InsertRule(string rule, uint index = 0);

    /// <summary>
    /// deleteRule
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#deleteRule")]
    public extern void DeleteRule(uint index);

    /// <summary>
    /// replace
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#replace")]
    public extern PromiseResult<CSSStyleSheet> Replace(string text);

    /// <summary>
    /// replaceSync
    /// </summary>
    /// <param name="text">text</param>
    [Description("@#replaceSync")]
    public extern void ReplaceSync(string text);

    /// <summary>
    /// rules
    /// </summary>
    [Description("@#rules")]
    public extern CSSRuleList Rules { get; }

    /// <summary>
    /// addRule
    /// </summary>
    /// <param name="selector">selector</param>
    /// <param name="style">style</param>
    /// <param name="index">index</param>
    [Description("@#addRule")]
    public extern int AddRule(string? selector = default, string? style = default, uint? index = default);

    /// <summary>
    /// removeRule
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeRule")]
    public extern void RemoveRule(uint index = 0);
}

/// <summary>
/// StyleSheetList
/// </summary>
[ECMAScript]
[Description("@#StyleSheetList")]
public class StyleSheetList
{
    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern CSSStyleSheet? GetItem(uint index);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }
}

/// <summary>
/// CSSRuleList
/// </summary>
[ECMAScript]
[Description("@#CSSRuleList")]
public class CSSRuleList
{
    /// <summary>
    /// item
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#item")]
    public extern CSSRule? GetItem(uint index);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }
}

/// <summary>
/// CSSImportRule
/// </summary>
[ECMAScript]
[Description("@#CSSImportRule")]
public class CSSImportRule : CSSRule
{
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern string Href { get; }

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern MediaList Media { get; }

    /// <summary>
    /// styleSheet
    /// </summary>
    [Description("@#styleSheet")]
    public extern CSSStyleSheet? StyleSheet { get; }

    /// <summary>
    /// layerName
    /// </summary>
    [Description("@#layerName")]
    public extern string? LayerName { get; }

    /// <summary>
    /// supportsText
    /// </summary>
    [Description("@#supportsText")]
    public extern string? SupportsText { get; }
}

/// <summary>
/// CSSGroupingRule
/// </summary>
[ECMAScript]
[Description("@#CSSGroupingRule")]
public class CSSGroupingRule : CSSRule
{
    /// <summary>
    /// cssRules
    /// </summary>
    [Description("@#cssRules")]
    public extern CSSRuleList CssRules { get; }

    /// <summary>
    /// insertRule
    /// </summary>
    /// <param name="rule">rule</param>
    /// <param name="index">index</param>
    [Description("@#insertRule")]
    public extern uint InsertRule(string rule, uint index = 0);

    /// <summary>
    /// deleteRule
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#deleteRule")]
    public extern void DeleteRule(uint index);
}

/// <summary>
/// CSSPageDescriptors
/// </summary>
[ECMAScript]
[Description("@#CSSPageDescriptors")]
public class CSSPageDescriptors : CSSStyleDeclaration
{
    /// <summary>
    /// margin-top
    /// </summary>
    [Description("@#margin-top")]
    public extern string Margin_Top { get; set; }

    /// <summary>
    /// margin-right
    /// </summary>
    [Description("@#margin-right")]
    public extern string Margin_Right { get; set; }

    /// <summary>
    /// margin-bottom
    /// </summary>
    [Description("@#margin-bottom")]
    public extern string Margin_Bottom { get; set; }

    /// <summary>
    /// margin-left
    /// </summary>
    [Description("@#margin-left")]
    public extern string Margin_Left { get; set; }

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern string Size { get; set; }

    /// <summary>
    /// pageOrientation
    /// </summary>
    [Description("@#pageOrientation")]
    public extern string PageOrientation { get; set; }

    /// <summary>
    /// page-orientation
    /// </summary>
    [Description("@#page-orientation")]
    public extern string Page_Orientation { get; set; }

    /// <summary>
    /// marks
    /// </summary>
    [Description("@#marks")]
    public extern string Marks { get; set; }

    /// <summary>
    /// bleed
    /// </summary>
    [Description("@#bleed")]
    public extern string Bleed { get; set; }
}

/// <summary>
/// CSSPageRule
/// </summary>
[ECMAScript]
[Description("@#CSSPageRule")]
public class CSSPageRule : CSSGroupingRule
{
    /// <summary>
    /// selectorText
    /// </summary>
    [Description("@#selectorText")]
    public extern string SelectorText { get; set; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSPageDescriptors Style { get; }
}

/// <summary>
/// CSSMarginRule
/// </summary>
[ECMAScript]
[Description("@#CSSMarginRule")]
public class CSSMarginRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSStyleDeclaration Style { get; }
}

/// <summary>
/// CSSNamespaceRule
/// </summary>
[ECMAScript]
[Description("@#CSSNamespaceRule")]
public class CSSNamespaceRule : CSSRule
{
    /// <summary>
    /// namespaceURI
    /// </summary>
    [Description("@#namespaceURI")]
    public extern string NamespaceURI { get; }

    /// <summary>
    /// prefix
    /// </summary>
    [Description("@#prefix")]
    public extern string Prefix { get; }
}

/// <summary>
/// CSSStyleProperties
/// </summary>
[ECMAScript]
[Description("@#CSSStyleProperties")]
public class CSSStyleProperties : CSSStyleDeclaration
{
    /// <summary>
    /// cssFloat
    /// </summary>
    [Description("@#cssFloat")]
    public extern string CssFloat { get; set; }
}

/// <summary>
/// Window
/// </summary>
[ECMAScript]
[Description("@#Window")]
public partial class Window
{
    /// <summary>
    /// getComputedStyle
    /// </summary>
    /// <param name="elt">elt</param>
    /// <param name="pseudoElt">pseudoElt</param>
    [Description("@#getComputedStyle")]
    public extern CSSStyleDeclaration GetComputedStyle(Element elt, string? pseudoElt);

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