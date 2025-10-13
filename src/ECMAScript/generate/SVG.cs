namespace ECMAScript;

/// <summary>
/// SVGBoundingBoxOptions
/// </summary>
[ECMAScript]
[Description("@#SVGBoundingBoxOptions")]
public record SVGBoundingBoxOptions(
    [property: Description("@#fill")]bool Fill = true,
	[property: Description("@#stroke")]bool Stroke = false,
	[property: Description("@#markers")]bool Markers = false,
	[property: Description("@#clipped")]bool Clipped = false);

/// <summary>
/// SVGElement
/// </summary>
[ECMAScript]
[Description("@#SVGElement")]
public class SVGElement : Element
{
    /// <summary>
    /// className
    /// </summary>
    [Description("@#className")]
    public extern new SVGAnimatedString ClassName { get; }

    /// <summary>
    /// ownerSVGElement
    /// </summary>
    [Description("@#ownerSVGElement")]
    public extern SVGSVGElement? OwnerSVGElement { get; }

    /// <summary>
    /// viewportElement
    /// </summary>
    [Description("@#viewportElement")]
    public extern SVGElement? ViewportElement { get; }

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

    #region mixin SVGElementInstance
    /// <summary>
    /// correspondingElement
    /// </summary>
    [Description("@#correspondingElement")]
    public extern SVGElement? CorrespondingElement { get; }

    /// <summary>
    /// correspondingUseElement
    /// </summary>
    [Description("@#correspondingUseElement")]
    public extern SVGUseElement? CorrespondingUseElement { get; }
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
/// SVGGraphicsElement
/// </summary>
[ECMAScript]
[Description("@#SVGGraphicsElement")]
public class SVGGraphicsElement : SVGElement
{
    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern SVGAnimatedTransformList Transform { get; }

    /// <summary>
    /// getBBox
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getBBox")]
    public extern DOMRect GetBBox(SVGBoundingBoxOptions? options = default);

    /// <summary>
    /// getCTM
    /// </summary>
    [Description("@#getCTM")]
    public extern DOMMatrix? GetCTM();

    /// <summary>
    /// getScreenCTM
    /// </summary>
    [Description("@#getScreenCTM")]
    public extern DOMMatrix? GetScreenCTM();

    #region mixin SVGTests
    /// <summary>
    /// requiredExtensions
    /// </summary>
    [Description("@#requiredExtensions")]
    public extern SVGStringList RequiredExtensions { get; }

    /// <summary>
    /// systemLanguage
    /// </summary>
    [Description("@#systemLanguage")]
    public extern SVGStringList SystemLanguage { get; }
    #endregion
}

/// <summary>
/// SVGGeometryElement
/// </summary>
[ECMAScript]
[Description("@#SVGGeometryElement")]
public class SVGGeometryElement : SVGGraphicsElement
{
    /// <summary>
    /// pathLength
    /// </summary>
    [Description("@#pathLength")]
    public extern SVGAnimatedNumber PathLength { get; }

    /// <summary>
    /// isPointInFill
    /// </summary>
    /// <param name="point">point</param>
    [Description("@#isPointInFill")]
    public extern bool IsPointInFill(DOMPointInit? point = default);

    /// <summary>
    /// isPointInStroke
    /// </summary>
    /// <param name="point">point</param>
    [Description("@#isPointInStroke")]
    public extern bool IsPointInStroke(DOMPointInit? point = default);

    /// <summary>
    /// getTotalLength
    /// </summary>
    [Description("@#getTotalLength")]
    public extern float GetTotalLength();

    /// <summary>
    /// getPointAtLength
    /// </summary>
    /// <param name="distance">distance</param>
    [Description("@#getPointAtLength")]
    public extern DOMPoint GetPointAtLength(float distance);
}

/// <summary>
/// SVGNumber
/// </summary>
[ECMAScript]
[Description("@#SVGNumber")]
public class SVGNumber
{
    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern float Value { get; set; }
}

/// <summary>
/// SVGLength
/// </summary>
[ECMAScript]
[Description("@#SVGLength")]
public class SVGLength
{
    /// <summary>
    /// SVG_LENGTHTYPE_UNKNOWN
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_UNKNOWN")]
    public const ushort SVG_LENGTHTYPE_UNKNOWN = 0;

    /// <summary>
    /// SVG_LENGTHTYPE_NUMBER
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_NUMBER")]
    public const ushort SVG_LENGTHTYPE_NUMBER = 1;

    /// <summary>
    /// SVG_LENGTHTYPE_PERCENTAGE
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_PERCENTAGE")]
    public const ushort SVG_LENGTHTYPE_PERCENTAGE = 2;

    /// <summary>
    /// SVG_LENGTHTYPE_EMS
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_EMS")]
    public const ushort SVG_LENGTHTYPE_EMS = 3;

    /// <summary>
    /// SVG_LENGTHTYPE_EXS
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_EXS")]
    public const ushort SVG_LENGTHTYPE_EXS = 4;

    /// <summary>
    /// SVG_LENGTHTYPE_PX
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_PX")]
    public const ushort SVG_LENGTHTYPE_PX = 5;

    /// <summary>
    /// SVG_LENGTHTYPE_CM
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_CM")]
    public const ushort SVG_LENGTHTYPE_CM = 6;

    /// <summary>
    /// SVG_LENGTHTYPE_MM
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_MM")]
    public const ushort SVG_LENGTHTYPE_MM = 7;

    /// <summary>
    /// SVG_LENGTHTYPE_IN
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_IN")]
    public const ushort SVG_LENGTHTYPE_IN = 8;

    /// <summary>
    /// SVG_LENGTHTYPE_PT
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_PT")]
    public const ushort SVG_LENGTHTYPE_PT = 9;

    /// <summary>
    /// SVG_LENGTHTYPE_PC
    /// </summary>
    [Description("@#SVG_LENGTHTYPE_PC")]
    public const ushort SVG_LENGTHTYPE_PC = 10;

    /// <summary>
    /// unitType
    /// </summary>
    [Description("@#unitType")]
    public extern ushort UnitType { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern float Value { get; set; }

    /// <summary>
    /// valueInSpecifiedUnits
    /// </summary>
    [Description("@#valueInSpecifiedUnits")]
    public extern float ValueInSpecifiedUnits { get; set; }

    /// <summary>
    /// valueAsString
    /// </summary>
    [Description("@#valueAsString")]
    public extern string ValueAsString { get; set; }

    /// <summary>
    /// newValueSpecifiedUnits
    /// </summary>
    /// <param name="unitType">unitType</param>
    /// <param name="valueInSpecifiedUnits">valueInSpecifiedUnits</param>
    [Description("@#newValueSpecifiedUnits")]
    public extern void NewValueSpecifiedUnits(ushort unitType, float valueInSpecifiedUnits);

    /// <summary>
    /// convertToSpecifiedUnits
    /// </summary>
    /// <param name="unitType">unitType</param>
    [Description("@#convertToSpecifiedUnits")]
    public extern void ConvertToSpecifiedUnits(ushort unitType);
}

/// <summary>
/// SVGAngle
/// </summary>
[ECMAScript]
[Description("@#SVGAngle")]
public class SVGAngle
{
    /// <summary>
    /// SVG_ANGLETYPE_UNKNOWN
    /// </summary>
    [Description("@#SVG_ANGLETYPE_UNKNOWN")]
    public const ushort SVG_ANGLETYPE_UNKNOWN = 0;

    /// <summary>
    /// SVG_ANGLETYPE_UNSPECIFIED
    /// </summary>
    [Description("@#SVG_ANGLETYPE_UNSPECIFIED")]
    public const ushort SVG_ANGLETYPE_UNSPECIFIED = 1;

    /// <summary>
    /// SVG_ANGLETYPE_DEG
    /// </summary>
    [Description("@#SVG_ANGLETYPE_DEG")]
    public const ushort SVG_ANGLETYPE_DEG = 2;

    /// <summary>
    /// SVG_ANGLETYPE_RAD
    /// </summary>
    [Description("@#SVG_ANGLETYPE_RAD")]
    public const ushort SVG_ANGLETYPE_RAD = 3;

    /// <summary>
    /// SVG_ANGLETYPE_GRAD
    /// </summary>
    [Description("@#SVG_ANGLETYPE_GRAD")]
    public const ushort SVG_ANGLETYPE_GRAD = 4;

    /// <summary>
    /// unitType
    /// </summary>
    [Description("@#unitType")]
    public extern ushort UnitType { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern float Value { get; set; }

    /// <summary>
    /// valueInSpecifiedUnits
    /// </summary>
    [Description("@#valueInSpecifiedUnits")]
    public extern float ValueInSpecifiedUnits { get; set; }

    /// <summary>
    /// valueAsString
    /// </summary>
    [Description("@#valueAsString")]
    public extern string ValueAsString { get; set; }

    /// <summary>
    /// newValueSpecifiedUnits
    /// </summary>
    /// <param name="unitType">unitType</param>
    /// <param name="valueInSpecifiedUnits">valueInSpecifiedUnits</param>
    [Description("@#newValueSpecifiedUnits")]
    public extern void NewValueSpecifiedUnits(ushort unitType, float valueInSpecifiedUnits);

    /// <summary>
    /// convertToSpecifiedUnits
    /// </summary>
    /// <param name="unitType">unitType</param>
    [Description("@#convertToSpecifiedUnits")]
    public extern void ConvertToSpecifiedUnits(ushort unitType);
}

/// <summary>
/// SVGNumberList
/// </summary>
[ECMAScript]
[Description("@#SVGNumberList")]
public class SVGNumberList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// numberOfItems
    /// </summary>
    [Description("@#numberOfItems")]
    public extern uint NumberOfItems { get; }

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// initialize
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#initialize")]
    public extern SVGNumber Initialize(SVGNumber newItem);

    /// <summary>
    /// getItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getItem")]
    public extern SVGNumber GetItem(uint index);

    /// <summary>
    /// insertItemBefore
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#insertItemBefore")]
    public extern SVGNumber InsertItemBefore(SVGNumber newItem, uint index);

    /// <summary>
    /// replaceItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#replaceItem")]
    public extern SVGNumber ReplaceItem(SVGNumber newItem, uint index);

    /// <summary>
    /// removeItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeItem")]
    public extern SVGNumber RemoveItem(uint index);

    /// <summary>
    /// appendItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#appendItem")]
    public extern SVGNumber AppendItem(SVGNumber newItem);
}

/// <summary>
/// SVGLengthList
/// </summary>
[ECMAScript]
[Description("@#SVGLengthList")]
public class SVGLengthList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// numberOfItems
    /// </summary>
    [Description("@#numberOfItems")]
    public extern uint NumberOfItems { get; }

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// initialize
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#initialize")]
    public extern SVGLength Initialize(SVGLength newItem);

    /// <summary>
    /// getItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getItem")]
    public extern SVGLength GetItem(uint index);

    /// <summary>
    /// insertItemBefore
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#insertItemBefore")]
    public extern SVGLength InsertItemBefore(SVGLength newItem, uint index);

    /// <summary>
    /// replaceItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#replaceItem")]
    public extern SVGLength ReplaceItem(SVGLength newItem, uint index);

    /// <summary>
    /// removeItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeItem")]
    public extern SVGLength RemoveItem(uint index);

    /// <summary>
    /// appendItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#appendItem")]
    public extern SVGLength AppendItem(SVGLength newItem);
}

/// <summary>
/// SVGStringList
/// </summary>
[ECMAScript]
[Description("@#SVGStringList")]
public class SVGStringList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// numberOfItems
    /// </summary>
    [Description("@#numberOfItems")]
    public extern uint NumberOfItems { get; }

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// initialize
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#initialize")]
    public extern string Initialize(string newItem);

    /// <summary>
    /// getItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getItem")]
    public extern string GetItem(uint index);

    /// <summary>
    /// insertItemBefore
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#insertItemBefore")]
    public extern string InsertItemBefore(string newItem, uint index);

    /// <summary>
    /// replaceItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#replaceItem")]
    public extern string ReplaceItem(string newItem, uint index);

    /// <summary>
    /// removeItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeItem")]
    public extern string RemoveItem(uint index);

    /// <summary>
    /// appendItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#appendItem")]
    public extern string AppendItem(string newItem);
}

/// <summary>
/// SVGAnimatedBoolean
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedBoolean")]
public class SVGAnimatedBoolean
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern bool BaseVal { get; set; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern bool AnimVal { get; }
}

/// <summary>
/// SVGAnimatedEnumeration
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedEnumeration")]
public class SVGAnimatedEnumeration
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern ushort BaseVal { get; set; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern ushort AnimVal { get; }
}

/// <summary>
/// SVGAnimatedInteger
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedInteger")]
public class SVGAnimatedInteger
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern int BaseVal { get; set; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern int AnimVal { get; }
}

/// <summary>
/// SVGAnimatedNumber
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedNumber")]
public class SVGAnimatedNumber
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern float BaseVal { get; set; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern float AnimVal { get; }
}

/// <summary>
/// SVGAnimatedLength
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedLength")]
public class SVGAnimatedLength
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern SVGLength BaseVal { get; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern SVGLength AnimVal { get; }
}

/// <summary>
/// SVGAnimatedAngle
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedAngle")]
public class SVGAnimatedAngle
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern SVGAngle BaseVal { get; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern SVGAngle AnimVal { get; }
}

/// <summary>
/// SVGAnimatedString
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedString")]
public class SVGAnimatedString
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern string BaseVal { get; set; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern string AnimVal { get; }
}

/// <summary>
/// SVGAnimatedRect
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedRect")]
public class SVGAnimatedRect
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern DOMRect BaseVal { get; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern DOMRectReadOnly AnimVal { get; }
}

/// <summary>
/// SVGAnimatedNumberList
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedNumberList")]
public class SVGAnimatedNumberList
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern SVGNumberList BaseVal { get; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern SVGNumberList AnimVal { get; }
}

/// <summary>
/// SVGAnimatedLengthList
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedLengthList")]
public class SVGAnimatedLengthList
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern SVGLengthList BaseVal { get; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern SVGLengthList AnimVal { get; }
}

/// <summary>
/// SVGUnitTypes
/// </summary>
[ECMAScript]
[Description("@#SVGUnitTypes")]
public class SVGUnitTypes
{
    /// <summary>
    /// SVG_UNIT_TYPE_UNKNOWN
    /// </summary>
    [Description("@#SVG_UNIT_TYPE_UNKNOWN")]
    public const ushort SVG_UNIT_TYPE_UNKNOWN = 0;

    /// <summary>
    /// SVG_UNIT_TYPE_USERSPACEONUSE
    /// </summary>
    [Description("@#SVG_UNIT_TYPE_USERSPACEONUSE")]
    public const ushort SVG_UNIT_TYPE_USERSPACEONUSE = 1;

    /// <summary>
    /// SVG_UNIT_TYPE_OBJECTBOUNDINGBOX
    /// </summary>
    [Description("@#SVG_UNIT_TYPE_OBJECTBOUNDINGBOX")]
    public const ushort SVG_UNIT_TYPE_OBJECTBOUNDINGBOX = 2;
}

/// <summary>
/// SVGGElement
/// </summary>
[ECMAScript]
[Description("@#SVGGElement")]
public class SVGGElement : SVGGraphicsElement
{

}

/// <summary>
/// SVGDefsElement
/// </summary>
[ECMAScript]
[Description("@#SVGDefsElement")]
public class SVGDefsElement : SVGGraphicsElement
{

}

/// <summary>
/// SVGDescElement
/// </summary>
[ECMAScript]
[Description("@#SVGDescElement")]
public class SVGDescElement : SVGElement
{

}

/// <summary>
/// SVGMetadataElement
/// </summary>
[ECMAScript]
[Description("@#SVGMetadataElement")]
public class SVGMetadataElement : SVGElement
{

}

/// <summary>
/// SVGTitleElement
/// </summary>
[ECMAScript]
[Description("@#SVGTitleElement")]
public class SVGTitleElement : SVGElement
{

}

/// <summary>
/// SVGSymbolElement
/// </summary>
[ECMAScript]
[Description("@#SVGSymbolElement")]
public class SVGSymbolElement : SVGGraphicsElement
{


    #region mixin SVGFitToViewBox
    /// <summary>
    /// viewBox
    /// </summary>
    [Description("@#viewBox")]
    public extern SVGAnimatedRect ViewBox { get; }

    /// <summary>
    /// preserveAspectRatio
    /// </summary>
    [Description("@#preserveAspectRatio")]
    public extern SVGAnimatedPreserveAspectRatio PreserveAspectRatio { get; }
    #endregion
}

/// <summary>
/// SVGUseElement
/// </summary>
[ECMAScript]
[Description("@#SVGUseElement")]
public class SVGUseElement : SVGGraphicsElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// instanceRoot
    /// </summary>
    [Description("@#instanceRoot")]
    public extern SVGElement? InstanceRoot { get; }

    /// <summary>
    /// animatedInstanceRoot
    /// </summary>
    [Description("@#animatedInstanceRoot")]
    public extern SVGElement? AnimatedInstanceRoot { get; }

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGUseElementShadowRoot
/// </summary>
[ECMAScript]
[Description("@#SVGUseElementShadowRoot")]
public class SVGUseElementShadowRoot : ShadowRoot
{

}

/// <summary>
/// ShadowAnimation
/// </summary>
[ECMAScript]
[Description("@#ShadowAnimation")]
public class ShadowAnimation(AnimationEffect? effect, AnimationTimeline? timeline) : Animation(effect, timeline)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="source">source</param>
    /// <param name="newTarget">newTarget</param>
    public extern ShadowAnimation(Animation source, Either<Element, CSSPseudoElement> newTarget);

    /// <summary>
    /// sourceAnimation
    /// </summary>
    [Description("@#sourceAnimation")]
    public extern Animation SourceAnimation { get; }
}

/// <summary>
/// SVGSwitchElement
/// </summary>
[ECMAScript]
[Description("@#SVGSwitchElement")]
public class SVGSwitchElement : SVGGraphicsElement
{

}

/// <summary>
/// SVGStyleElement
/// </summary>
[ECMAScript]
[Description("@#SVGStyleElement")]
public class SVGStyleElement : SVGElement
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern string Media { get; set; }

    /// <summary>
    /// title
    /// </summary>
    [Description("@#title")]
    public extern string Title { get; set; }

    #region mixin LinkStyle
    /// <summary>
    /// sheet
    /// </summary>
    [Description("@#sheet")]
    public extern CSSStyleSheet? Sheet { get; }
    #endregion
}

/// <summary>
/// SVGTransform
/// </summary>
[ECMAScript]
[Description("@#SVGTransform")]
public class SVGTransform
{
    /// <summary>
    /// SVG_TRANSFORM_UNKNOWN
    /// </summary>
    [Description("@#SVG_TRANSFORM_UNKNOWN")]
    public const ushort SVG_TRANSFORM_UNKNOWN = 0;

    /// <summary>
    /// SVG_TRANSFORM_MATRIX
    /// </summary>
    [Description("@#SVG_TRANSFORM_MATRIX")]
    public const ushort SVG_TRANSFORM_MATRIX = 1;

    /// <summary>
    /// SVG_TRANSFORM_TRANSLATE
    /// </summary>
    [Description("@#SVG_TRANSFORM_TRANSLATE")]
    public const ushort SVG_TRANSFORM_TRANSLATE = 2;

    /// <summary>
    /// SVG_TRANSFORM_SCALE
    /// </summary>
    [Description("@#SVG_TRANSFORM_SCALE")]
    public const ushort SVG_TRANSFORM_SCALE = 3;

    /// <summary>
    /// SVG_TRANSFORM_ROTATE
    /// </summary>
    [Description("@#SVG_TRANSFORM_ROTATE")]
    public const ushort SVG_TRANSFORM_ROTATE = 4;

    /// <summary>
    /// SVG_TRANSFORM_SKEWX
    /// </summary>
    [Description("@#SVG_TRANSFORM_SKEWX")]
    public const ushort SVG_TRANSFORM_SKEWX = 5;

    /// <summary>
    /// SVG_TRANSFORM_SKEWY
    /// </summary>
    [Description("@#SVG_TRANSFORM_SKEWY")]
    public const ushort SVG_TRANSFORM_SKEWY = 6;

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern ushort Type { get; }

    /// <summary>
    /// matrix
    /// </summary>
    [Description("@#matrix")]
    public extern DOMMatrix Matrix { get; }

    /// <summary>
    /// angle
    /// </summary>
    [Description("@#angle")]
    public extern float Angle { get; }

    /// <summary>
    /// setMatrix
    /// </summary>
    /// <param name="matrix">matrix</param>
    [Description("@#setMatrix")]
    public extern void SetMatrix(DOMMatrix2DInit? matrix = default);

    /// <summary>
    /// setTranslate
    /// </summary>
    /// <param name="tx">tx</param>
    /// <param name="ty">ty</param>
    [Description("@#setTranslate")]
    public extern void SetTranslate(float tx, float ty);

    /// <summary>
    /// setScale
    /// </summary>
    /// <param name="sx">sx</param>
    /// <param name="sy">sy</param>
    [Description("@#setScale")]
    public extern void SetScale(float sx, float sy);

    /// <summary>
    /// setRotate
    /// </summary>
    /// <param name="angle">angle</param>
    /// <param name="cx">cx</param>
    /// <param name="cy">cy</param>
    [Description("@#setRotate")]
    public extern void SetRotate(float angle, float cx, float cy);

    /// <summary>
    /// setSkewX
    /// </summary>
    /// <param name="angle">angle</param>
    [Description("@#setSkewX")]
    public extern void SetSkewX(float angle);

    /// <summary>
    /// setSkewY
    /// </summary>
    /// <param name="angle">angle</param>
    [Description("@#setSkewY")]
    public extern void SetSkewY(float angle);
}

/// <summary>
/// SVGTransformList
/// </summary>
[ECMAScript]
[Description("@#SVGTransformList")]
public class SVGTransformList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// numberOfItems
    /// </summary>
    [Description("@#numberOfItems")]
    public extern uint NumberOfItems { get; }

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// initialize
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#initialize")]
    public extern SVGTransform Initialize(SVGTransform newItem);

    /// <summary>
    /// getItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getItem")]
    public extern SVGTransform GetItem(uint index);

    /// <summary>
    /// insertItemBefore
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#insertItemBefore")]
    public extern SVGTransform InsertItemBefore(SVGTransform newItem, uint index);

    /// <summary>
    /// replaceItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#replaceItem")]
    public extern SVGTransform ReplaceItem(SVGTransform newItem, uint index);

    /// <summary>
    /// removeItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeItem")]
    public extern SVGTransform RemoveItem(uint index);

    /// <summary>
    /// appendItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#appendItem")]
    public extern SVGTransform AppendItem(SVGTransform newItem);

    /// <summary>
    /// createSVGTransformFromMatrix
    /// </summary>
    /// <param name="matrix">matrix</param>
    [Description("@#createSVGTransformFromMatrix")]
    public extern SVGTransform CreateSVGTransformFromMatrix(DOMMatrix2DInit? matrix = default);

    /// <summary>
    /// consolidate
    /// </summary>
    [Description("@#consolidate")]
    public extern SVGTransform? Consolidate();
}

/// <summary>
/// SVGAnimatedTransformList
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedTransformList")]
public class SVGAnimatedTransformList
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern SVGTransformList BaseVal { get; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern SVGTransformList AnimVal { get; }
}

/// <summary>
/// SVGPreserveAspectRatio
/// </summary>
[ECMAScript]
[Description("@#SVGPreserveAspectRatio")]
public class SVGPreserveAspectRatio
{
    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_UNKNOWN
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_UNKNOWN")]
    public const ushort SVG_PRESERVEASPECTRATIO_UNKNOWN = 0;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_NONE
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_NONE")]
    public const ushort SVG_PRESERVEASPECTRATIO_NONE = 1;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMINYMIN
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMINYMIN")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMINYMIN = 2;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMIDYMIN
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMIDYMIN")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMIDYMIN = 3;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMAXYMIN
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMAXYMIN")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMAXYMIN = 4;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMINYMID
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMINYMID")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMINYMID = 5;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMIDYMID
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMIDYMID")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMIDYMID = 6;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMAXYMID
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMAXYMID")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMAXYMID = 7;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMINYMAX
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMINYMAX")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMINYMAX = 8;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMIDYMAX
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMIDYMAX")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMIDYMAX = 9;

    /// <summary>
    /// SVG_PRESERVEASPECTRATIO_XMAXYMAX
    /// </summary>
    [Description("@#SVG_PRESERVEASPECTRATIO_XMAXYMAX")]
    public const ushort SVG_PRESERVEASPECTRATIO_XMAXYMAX = 10;

    /// <summary>
    /// SVG_MEETORSLICE_UNKNOWN
    /// </summary>
    [Description("@#SVG_MEETORSLICE_UNKNOWN")]
    public const ushort SVG_MEETORSLICE_UNKNOWN = 0;

    /// <summary>
    /// SVG_MEETORSLICE_MEET
    /// </summary>
    [Description("@#SVG_MEETORSLICE_MEET")]
    public const ushort SVG_MEETORSLICE_MEET = 1;

    /// <summary>
    /// SVG_MEETORSLICE_SLICE
    /// </summary>
    [Description("@#SVG_MEETORSLICE_SLICE")]
    public const ushort SVG_MEETORSLICE_SLICE = 2;

    /// <summary>
    /// align
    /// </summary>
    [Description("@#align")]
    public extern ushort Align { get; set; }

    /// <summary>
    /// meetOrSlice
    /// </summary>
    [Description("@#meetOrSlice")]
    public extern ushort MeetOrSlice { get; set; }
}

/// <summary>
/// SVGAnimatedPreserveAspectRatio
/// </summary>
[ECMAScript]
[Description("@#SVGAnimatedPreserveAspectRatio")]
public class SVGAnimatedPreserveAspectRatio
{
    /// <summary>
    /// baseVal
    /// </summary>
    [Description("@#baseVal")]
    public extern SVGPreserveAspectRatio BaseVal { get; }

    /// <summary>
    /// animVal
    /// </summary>
    [Description("@#animVal")]
    public extern SVGPreserveAspectRatio AnimVal { get; }
}

/// <summary>
/// SVGRectElement
/// </summary>
[ECMAScript]
[Description("@#SVGRectElement")]
public class SVGRectElement : SVGGeometryElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// rx
    /// </summary>
    [Description("@#rx")]
    public extern SVGAnimatedLength Rx { get; }

    /// <summary>
    /// ry
    /// </summary>
    [Description("@#ry")]
    public extern SVGAnimatedLength Ry { get; }
}

/// <summary>
/// SVGCircleElement
/// </summary>
[ECMAScript]
[Description("@#SVGCircleElement")]
public class SVGCircleElement : SVGGeometryElement
{
    /// <summary>
    /// cx
    /// </summary>
    [Description("@#cx")]
    public extern SVGAnimatedLength Cx { get; }

    /// <summary>
    /// cy
    /// </summary>
    [Description("@#cy")]
    public extern SVGAnimatedLength Cy { get; }

    /// <summary>
    /// r
    /// </summary>
    [Description("@#r")]
    public extern SVGAnimatedLength R { get; }
}

/// <summary>
/// SVGEllipseElement
/// </summary>
[ECMAScript]
[Description("@#SVGEllipseElement")]
public class SVGEllipseElement : SVGGeometryElement
{
    /// <summary>
    /// cx
    /// </summary>
    [Description("@#cx")]
    public extern SVGAnimatedLength Cx { get; }

    /// <summary>
    /// cy
    /// </summary>
    [Description("@#cy")]
    public extern SVGAnimatedLength Cy { get; }

    /// <summary>
    /// rx
    /// </summary>
    [Description("@#rx")]
    public extern SVGAnimatedLength Rx { get; }

    /// <summary>
    /// ry
    /// </summary>
    [Description("@#ry")]
    public extern SVGAnimatedLength Ry { get; }
}

/// <summary>
/// SVGLineElement
/// </summary>
[ECMAScript]
[Description("@#SVGLineElement")]
public class SVGLineElement : SVGGeometryElement
{
    /// <summary>
    /// x1
    /// </summary>
    [Description("@#x1")]
    public extern SVGAnimatedLength X1 { get; }

    /// <summary>
    /// y1
    /// </summary>
    [Description("@#y1")]
    public extern SVGAnimatedLength Y1 { get; }

    /// <summary>
    /// x2
    /// </summary>
    [Description("@#x2")]
    public extern SVGAnimatedLength X2 { get; }

    /// <summary>
    /// y2
    /// </summary>
    [Description("@#y2")]
    public extern SVGAnimatedLength Y2 { get; }
}

/// <summary>
/// SVGPointList
/// </summary>
[ECMAScript]
[Description("@#SVGPointList")]
public class SVGPointList
{
    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    /// <summary>
    /// numberOfItems
    /// </summary>
    [Description("@#numberOfItems")]
    public extern uint NumberOfItems { get; }

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();

    /// <summary>
    /// initialize
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#initialize")]
    public extern DOMPoint Initialize(DOMPoint newItem);

    /// <summary>
    /// getItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#getItem")]
    public extern DOMPoint GetItem(uint index);

    /// <summary>
    /// insertItemBefore
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#insertItemBefore")]
    public extern DOMPoint InsertItemBefore(DOMPoint newItem, uint index);

    /// <summary>
    /// replaceItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    /// <param name="index">index</param>
    [Description("@#replaceItem")]
    public extern DOMPoint ReplaceItem(DOMPoint newItem, uint index);

    /// <summary>
    /// removeItem
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeItem")]
    public extern DOMPoint RemoveItem(uint index);

    /// <summary>
    /// appendItem
    /// </summary>
    /// <param name="newItem">newItem</param>
    [Description("@#appendItem")]
    public extern DOMPoint AppendItem(DOMPoint newItem);
}

/// <summary>
/// SVGPolylineElement
/// </summary>
[ECMAScript]
[Description("@#SVGPolylineElement")]
public class SVGPolylineElement : SVGGeometryElement
{


    #region mixin SVGAnimatedPoints
    /// <summary>
    /// points
    /// </summary>
    [Description("@#points")]
    public extern SVGPointList Points { get; }

    /// <summary>
    /// animatedPoints
    /// </summary>
    [Description("@#animatedPoints")]
    public extern SVGPointList AnimatedPoints { get; }
    #endregion
}

/// <summary>
/// SVGPolygonElement
/// </summary>
[ECMAScript]
[Description("@#SVGPolygonElement")]
public class SVGPolygonElement : SVGGeometryElement
{


    #region mixin SVGAnimatedPoints
    /// <summary>
    /// points
    /// </summary>
    [Description("@#points")]
    public extern SVGPointList Points { get; }

    /// <summary>
    /// animatedPoints
    /// </summary>
    [Description("@#animatedPoints")]
    public extern SVGPointList AnimatedPoints { get; }
    #endregion
}

/// <summary>
/// SVGTextContentElement
/// </summary>
[ECMAScript]
[Description("@#SVGTextContentElement")]
public class SVGTextContentElement : SVGGraphicsElement
{
    /// <summary>
    /// LENGTHADJUST_UNKNOWN
    /// </summary>
    [Description("@#LENGTHADJUST_UNKNOWN")]
    public const ushort LENGTHADJUST_UNKNOWN = 0;

    /// <summary>
    /// LENGTHADJUST_SPACING
    /// </summary>
    [Description("@#LENGTHADJUST_SPACING")]
    public const ushort LENGTHADJUST_SPACING = 1;

    /// <summary>
    /// LENGTHADJUST_SPACINGANDGLYPHS
    /// </summary>
    [Description("@#LENGTHADJUST_SPACINGANDGLYPHS")]
    public const ushort LENGTHADJUST_SPACINGANDGLYPHS = 2;

    /// <summary>
    /// textLength
    /// </summary>
    [Description("@#textLength")]
    public extern SVGAnimatedLength TextLength { get; }

    /// <summary>
    /// lengthAdjust
    /// </summary>
    [Description("@#lengthAdjust")]
    public extern SVGAnimatedEnumeration LengthAdjust { get; }

    /// <summary>
    /// getNumberOfChars
    /// </summary>
    [Description("@#getNumberOfChars")]
    public extern int GetNumberOfChars();

    /// <summary>
    /// getComputedTextLength
    /// </summary>
    [Description("@#getComputedTextLength")]
    public extern float GetComputedTextLength();

    /// <summary>
    /// getSubStringLength
    /// </summary>
    /// <param name="charnum">charnum</param>
    /// <param name="nchars">nchars</param>
    [Description("@#getSubStringLength")]
    public extern float GetSubStringLength(uint charnum, uint nchars);

    /// <summary>
    /// getStartPositionOfChar
    /// </summary>
    /// <param name="charnum">charnum</param>
    [Description("@#getStartPositionOfChar")]
    public extern DOMPoint GetStartPositionOfChar(uint charnum);

    /// <summary>
    /// getEndPositionOfChar
    /// </summary>
    /// <param name="charnum">charnum</param>
    [Description("@#getEndPositionOfChar")]
    public extern DOMPoint GetEndPositionOfChar(uint charnum);

    /// <summary>
    /// getExtentOfChar
    /// </summary>
    /// <param name="charnum">charnum</param>
    [Description("@#getExtentOfChar")]
    public extern DOMRect GetExtentOfChar(uint charnum);

    /// <summary>
    /// getRotationOfChar
    /// </summary>
    /// <param name="charnum">charnum</param>
    [Description("@#getRotationOfChar")]
    public extern float GetRotationOfChar(uint charnum);

    /// <summary>
    /// getCharNumAtPosition
    /// </summary>
    /// <param name="point">point</param>
    [Description("@#getCharNumAtPosition")]
    public extern int GetCharNumAtPosition(DOMPointInit? point = default);

    /// <summary>
    /// selectSubString
    /// </summary>
    /// <param name="charnum">charnum</param>
    /// <param name="nchars">nchars</param>
    [Description("@#selectSubString")]
    public extern void SelectSubString(uint charnum, uint nchars);
}

/// <summary>
/// SVGTextPositioningElement
/// </summary>
[ECMAScript]
[Description("@#SVGTextPositioningElement")]
public class SVGTextPositioningElement : SVGTextContentElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLengthList X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLengthList Y { get; }

    /// <summary>
    /// dx
    /// </summary>
    [Description("@#dx")]
    public extern SVGAnimatedLengthList Dx { get; }

    /// <summary>
    /// dy
    /// </summary>
    [Description("@#dy")]
    public extern SVGAnimatedLengthList Dy { get; }

    /// <summary>
    /// rotate
    /// </summary>
    [Description("@#rotate")]
    public extern SVGAnimatedNumberList Rotate { get; }
}

/// <summary>
/// SVGTextElement
/// </summary>
[ECMAScript]
[Description("@#SVGTextElement")]
public class SVGTextElement : SVGTextPositioningElement
{

}

/// <summary>
/// SVGTSpanElement
/// </summary>
[ECMAScript]
[Description("@#SVGTSpanElement")]
public class SVGTSpanElement : SVGTextPositioningElement
{

}

/// <summary>
/// SVGTextPathElement
/// </summary>
[ECMAScript]
[Description("@#SVGTextPathElement")]
public class SVGTextPathElement : SVGTextContentElement
{
    /// <summary>
    /// TEXTPATH_METHODTYPE_UNKNOWN
    /// </summary>
    [Description("@#TEXTPATH_METHODTYPE_UNKNOWN")]
    public const ushort TEXTPATH_METHODTYPE_UNKNOWN = 0;

    /// <summary>
    /// TEXTPATH_METHODTYPE_ALIGN
    /// </summary>
    [Description("@#TEXTPATH_METHODTYPE_ALIGN")]
    public const ushort TEXTPATH_METHODTYPE_ALIGN = 1;

    /// <summary>
    /// TEXTPATH_METHODTYPE_STRETCH
    /// </summary>
    [Description("@#TEXTPATH_METHODTYPE_STRETCH")]
    public const ushort TEXTPATH_METHODTYPE_STRETCH = 2;

    /// <summary>
    /// TEXTPATH_SPACINGTYPE_UNKNOWN
    /// </summary>
    [Description("@#TEXTPATH_SPACINGTYPE_UNKNOWN")]
    public const ushort TEXTPATH_SPACINGTYPE_UNKNOWN = 0;

    /// <summary>
    /// TEXTPATH_SPACINGTYPE_AUTO
    /// </summary>
    [Description("@#TEXTPATH_SPACINGTYPE_AUTO")]
    public const ushort TEXTPATH_SPACINGTYPE_AUTO = 1;

    /// <summary>
    /// TEXTPATH_SPACINGTYPE_EXACT
    /// </summary>
    [Description("@#TEXTPATH_SPACINGTYPE_EXACT")]
    public const ushort TEXTPATH_SPACINGTYPE_EXACT = 2;

    /// <summary>
    /// startOffset
    /// </summary>
    [Description("@#startOffset")]
    public extern SVGAnimatedLength StartOffset { get; }

    /// <summary>
    /// method
    /// </summary>
    [Description("@#method")]
    public extern SVGAnimatedEnumeration Method { get; }

    /// <summary>
    /// spacing
    /// </summary>
    [Description("@#spacing")]
    public extern SVGAnimatedEnumeration Spacing { get; }

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGImageElement
/// </summary>
[ECMAScript]
[Description("@#SVGImageElement")]
public class SVGImageElement : SVGGraphicsElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    /// <summary>
    /// preserveAspectRatio
    /// </summary>
    [Description("@#preserveAspectRatio")]
    public extern SVGAnimatedPreserveAspectRatio PreserveAspectRatio { get; }

    /// <summary>
    /// crossOrigin
    /// </summary>
    [Description("@#crossOrigin")]
    public extern string? CrossOrigin { get; set; }

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGForeignObjectElement
/// </summary>
[ECMAScript]
[Description("@#SVGForeignObjectElement")]
public class SVGForeignObjectElement : SVGGraphicsElement
{
    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }
}

/// <summary>
/// SVGMarkerElement
/// </summary>
[ECMAScript]
[Description("@#SVGMarkerElement")]
public class SVGMarkerElement : SVGElement
{
    /// <summary>
    /// SVG_MARKERUNITS_UNKNOWN
    /// </summary>
    [Description("@#SVG_MARKERUNITS_UNKNOWN")]
    public const ushort SVG_MARKERUNITS_UNKNOWN = 0;

    /// <summary>
    /// SVG_MARKERUNITS_USERSPACEONUSE
    /// </summary>
    [Description("@#SVG_MARKERUNITS_USERSPACEONUSE")]
    public const ushort SVG_MARKERUNITS_USERSPACEONUSE = 1;

    /// <summary>
    /// SVG_MARKERUNITS_STROKEWIDTH
    /// </summary>
    [Description("@#SVG_MARKERUNITS_STROKEWIDTH")]
    public const ushort SVG_MARKERUNITS_STROKEWIDTH = 2;

    /// <summary>
    /// SVG_MARKER_ORIENT_UNKNOWN
    /// </summary>
    [Description("@#SVG_MARKER_ORIENT_UNKNOWN")]
    public const ushort SVG_MARKER_ORIENT_UNKNOWN = 0;

    /// <summary>
    /// SVG_MARKER_ORIENT_AUTO
    /// </summary>
    [Description("@#SVG_MARKER_ORIENT_AUTO")]
    public const ushort SVG_MARKER_ORIENT_AUTO = 1;

    /// <summary>
    /// SVG_MARKER_ORIENT_ANGLE
    /// </summary>
    [Description("@#SVG_MARKER_ORIENT_ANGLE")]
    public const ushort SVG_MARKER_ORIENT_ANGLE = 2;

    /// <summary>
    /// refX
    /// </summary>
    [Description("@#refX")]
    public extern SVGAnimatedLength RefX { get; }

    /// <summary>
    /// refY
    /// </summary>
    [Description("@#refY")]
    public extern SVGAnimatedLength RefY { get; }

    /// <summary>
    /// markerUnits
    /// </summary>
    [Description("@#markerUnits")]
    public extern SVGAnimatedEnumeration MarkerUnits { get; }

    /// <summary>
    /// markerWidth
    /// </summary>
    [Description("@#markerWidth")]
    public extern SVGAnimatedLength MarkerWidth { get; }

    /// <summary>
    /// markerHeight
    /// </summary>
    [Description("@#markerHeight")]
    public extern SVGAnimatedLength MarkerHeight { get; }

    /// <summary>
    /// orientType
    /// </summary>
    [Description("@#orientType")]
    public extern SVGAnimatedEnumeration OrientType { get; }

    /// <summary>
    /// orientAngle
    /// </summary>
    [Description("@#orientAngle")]
    public extern SVGAnimatedAngle OrientAngle { get; }

    /// <summary>
    /// orient
    /// </summary>
    [Description("@#orient")]
    public extern string Orient { get; set; }

    /// <summary>
    /// setOrientToAuto
    /// </summary>
    [Description("@#setOrientToAuto")]
    public extern void SetOrientToAuto();

    /// <summary>
    /// setOrientToAngle
    /// </summary>
    /// <param name="angle">angle</param>
    [Description("@#setOrientToAngle")]
    public extern void SetOrientToAngle(SVGAngle angle);

    #region mixin SVGFitToViewBox
    /// <summary>
    /// viewBox
    /// </summary>
    [Description("@#viewBox")]
    public extern SVGAnimatedRect ViewBox { get; }

    /// <summary>
    /// preserveAspectRatio
    /// </summary>
    [Description("@#preserveAspectRatio")]
    public extern SVGAnimatedPreserveAspectRatio PreserveAspectRatio { get; }
    #endregion
}

/// <summary>
/// SVGGradientElement
/// </summary>
[ECMAScript]
[Description("@#SVGGradientElement")]
public class SVGGradientElement : SVGElement
{
    /// <summary>
    /// SVG_SPREADMETHOD_UNKNOWN
    /// </summary>
    [Description("@#SVG_SPREADMETHOD_UNKNOWN")]
    public const ushort SVG_SPREADMETHOD_UNKNOWN = 0;

    /// <summary>
    /// SVG_SPREADMETHOD_PAD
    /// </summary>
    [Description("@#SVG_SPREADMETHOD_PAD")]
    public const ushort SVG_SPREADMETHOD_PAD = 1;

    /// <summary>
    /// SVG_SPREADMETHOD_REFLECT
    /// </summary>
    [Description("@#SVG_SPREADMETHOD_REFLECT")]
    public const ushort SVG_SPREADMETHOD_REFLECT = 2;

    /// <summary>
    /// SVG_SPREADMETHOD_REPEAT
    /// </summary>
    [Description("@#SVG_SPREADMETHOD_REPEAT")]
    public const ushort SVG_SPREADMETHOD_REPEAT = 3;

    /// <summary>
    /// gradientUnits
    /// </summary>
    [Description("@#gradientUnits")]
    public extern SVGAnimatedEnumeration GradientUnits { get; }

    /// <summary>
    /// gradientTransform
    /// </summary>
    [Description("@#gradientTransform")]
    public extern SVGAnimatedTransformList GradientTransform { get; }

    /// <summary>
    /// spreadMethod
    /// </summary>
    [Description("@#spreadMethod")]
    public extern SVGAnimatedEnumeration SpreadMethod { get; }

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGLinearGradientElement
/// </summary>
[ECMAScript]
[Description("@#SVGLinearGradientElement")]
public class SVGLinearGradientElement : SVGGradientElement
{
    /// <summary>
    /// x1
    /// </summary>
    [Description("@#x1")]
    public extern SVGAnimatedLength X1 { get; }

    /// <summary>
    /// y1
    /// </summary>
    [Description("@#y1")]
    public extern SVGAnimatedLength Y1 { get; }

    /// <summary>
    /// x2
    /// </summary>
    [Description("@#x2")]
    public extern SVGAnimatedLength X2 { get; }

    /// <summary>
    /// y2
    /// </summary>
    [Description("@#y2")]
    public extern SVGAnimatedLength Y2 { get; }
}

/// <summary>
/// SVGRadialGradientElement
/// </summary>
[ECMAScript]
[Description("@#SVGRadialGradientElement")]
public class SVGRadialGradientElement : SVGGradientElement
{
    /// <summary>
    /// cx
    /// </summary>
    [Description("@#cx")]
    public extern SVGAnimatedLength Cx { get; }

    /// <summary>
    /// cy
    /// </summary>
    [Description("@#cy")]
    public extern SVGAnimatedLength Cy { get; }

    /// <summary>
    /// r
    /// </summary>
    [Description("@#r")]
    public extern SVGAnimatedLength R { get; }

    /// <summary>
    /// fx
    /// </summary>
    [Description("@#fx")]
    public extern SVGAnimatedLength Fx { get; }

    /// <summary>
    /// fy
    /// </summary>
    [Description("@#fy")]
    public extern SVGAnimatedLength Fy { get; }

    /// <summary>
    /// fr
    /// </summary>
    [Description("@#fr")]
    public extern SVGAnimatedLength Fr { get; }
}

/// <summary>
/// SVGStopElement
/// </summary>
[ECMAScript]
[Description("@#SVGStopElement")]
public class SVGStopElement : SVGElement
{
    /// <summary>
    /// offset
    /// </summary>
    [Description("@#offset")]
    public extern SVGAnimatedNumber Offset { get; }
}

/// <summary>
/// SVGPatternElement
/// </summary>
[ECMAScript]
[Description("@#SVGPatternElement")]
public class SVGPatternElement : SVGElement
{
    /// <summary>
    /// patternUnits
    /// </summary>
    [Description("@#patternUnits")]
    public extern SVGAnimatedEnumeration PatternUnits { get; }

    /// <summary>
    /// patternContentUnits
    /// </summary>
    [Description("@#patternContentUnits")]
    public extern SVGAnimatedEnumeration PatternContentUnits { get; }

    /// <summary>
    /// patternTransform
    /// </summary>
    [Description("@#patternTransform")]
    public extern SVGAnimatedTransformList PatternTransform { get; }

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern SVGAnimatedLength X { get; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern SVGAnimatedLength Y { get; }

    /// <summary>
    /// width
    /// </summary>
    [Description("@#width")]
    public extern SVGAnimatedLength Width { get; }

    /// <summary>
    /// height
    /// </summary>
    [Description("@#height")]
    public extern SVGAnimatedLength Height { get; }

    #region mixin SVGFitToViewBox
    /// <summary>
    /// viewBox
    /// </summary>
    [Description("@#viewBox")]
    public extern SVGAnimatedRect ViewBox { get; }

    /// <summary>
    /// preserveAspectRatio
    /// </summary>
    [Description("@#preserveAspectRatio")]
    public extern SVGAnimatedPreserveAspectRatio PreserveAspectRatio { get; }
    #endregion

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGScriptElement
/// </summary>
[ECMAScript]
[Description("@#SVGScriptElement")]
public class SVGScriptElement : SVGElement
{
    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// crossOrigin
    /// </summary>
    [Description("@#crossOrigin")]
    public extern string? CrossOrigin { get; set; }

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGAElement
/// </summary>
[ECMAScript]
[Description("@#SVGAElement")]
public partial class SVGAElement : SVGGraphicsElement
{
    /// <summary>
    /// target
    /// </summary>
    [Description("@#target")]
    public extern SVGAnimatedString Target { get; }

    /// <summary>
    /// download
    /// </summary>
    [Description("@#download")]
    public extern string Download { get; set; }

    /// <summary>
    /// ping
    /// </summary>
    [Description("@#ping")]
    public extern string Ping { get; set; }

    /// <summary>
    /// rel
    /// </summary>
    [Description("@#rel")]
    public extern string Rel { get; set; }

    /// <summary>
    /// relList
    /// </summary>
    [Description("@#relList")]
    public extern List<string> RelList { get; }

    /// <summary>
    /// hreflang
    /// </summary>
    [Description("@#hreflang")]
    public extern string Hreflang { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern string Type { get; set; }

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }

    /// <summary>
    /// referrerPolicy
    /// </summary>
    [Description("@#referrerPolicy")]
    public extern string ReferrerPolicy { get; set; }

    /// <summary>
    /// origin
    /// </summary>
    [Description("@#origin")]
    public extern string Origin { get; }

    /// <summary>
    /// protocol
    /// </summary>
    [Description("@#protocol")]
    public extern string Protocol { get; set; }

    /// <summary>
    /// username
    /// </summary>
    [Description("@#username")]
    public extern string Username { get; set; }

    /// <summary>
    /// password
    /// </summary>
    [Description("@#password")]
    public extern string Password { get; set; }

    /// <summary>
    /// host
    /// </summary>
    [Description("@#host")]
    public extern string Host { get; set; }

    /// <summary>
    /// hostname
    /// </summary>
    [Description("@#hostname")]
    public extern string Hostname { get; set; }

    /// <summary>
    /// port
    /// </summary>
    [Description("@#port")]
    public extern string Port { get; set; }

    /// <summary>
    /// pathname
    /// </summary>
    [Description("@#pathname")]
    public extern string Pathname { get; set; }

    /// <summary>
    /// search
    /// </summary>
    [Description("@#search")]
    public extern string Search { get; set; }

    /// <summary>
    /// hash
    /// </summary>
    [Description("@#hash")]
    public extern string Hash { get; set; }

    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGViewElement
/// </summary>
[ECMAScript]
[Description("@#SVGViewElement")]
public class SVGViewElement : SVGElement
{


    #region mixin SVGFitToViewBox
    /// <summary>
    /// viewBox
    /// </summary>
    [Description("@#viewBox")]
    public extern SVGAnimatedRect ViewBox { get; }

    /// <summary>
    /// preserveAspectRatio
    /// </summary>
    [Description("@#preserveAspectRatio")]
    public extern SVGAnimatedPreserveAspectRatio PreserveAspectRatio { get; }
    #endregion
}