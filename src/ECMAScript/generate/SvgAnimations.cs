namespace ECMAScript;

/// <summary>
/// TimeEvent
/// </summary>
[ECMAScript]
[Description("@#TimeEvent")]
public class TimeEvent(string type, EventInit eventInitDict) : Event(type, eventInitDict)
{
    /// <summary>
    /// view
    /// </summary>
    [Description("@#view")]
    public extern WindowProxy? View { get; }

    /// <summary>
    /// detail
    /// </summary>
    [Description("@#detail")]
    public extern int Detail { get; }

    /// <summary>
    /// initTimeEvent
    /// </summary>
    /// <param name="typeArg">typeArg</param>
    /// <param name="viewArg">viewArg</param>
    /// <param name="detailArg">detailArg</param>
    [Description("@#initTimeEvent")]
    public extern void InitTimeEvent(string typeArg, Window? viewArg, int detailArg);
}

/// <summary>
/// SVGAnimationElement
/// </summary>
[ECMAScript]
[Description("@#SVGAnimationElement")]
public class SVGAnimationElement : SVGElement
{
    /// <summary>
    /// targetElement
    /// </summary>
    [Description("@#targetElement")]
    public extern SVGElement? TargetElement { get; }

    /// <summary>
    /// onbegin
    /// </summary>
    [Description("@#onbegin")]
    public extern EventHandler Onbegin { get; set; }

    /// <summary>
    /// onend
    /// </summary>
    [Description("@#onend")]
    public extern EventHandler Onend { get; set; }

    /// <summary>
    /// onrepeat
    /// </summary>
    [Description("@#onrepeat")]
    public extern EventHandler Onrepeat { get; set; }

    /// <summary>
    /// getStartTime
    /// </summary>
    [Description("@#getStartTime")]
    public extern float GetStartTime();

    /// <summary>
    /// getCurrentTime
    /// </summary>
    [Description("@#getCurrentTime")]
    public extern float GetCurrentTime();

    /// <summary>
    /// getSimpleDuration
    /// </summary>
    [Description("@#getSimpleDuration")]
    public extern float GetSimpleDuration();

    /// <summary>
    /// beginElement
    /// </summary>
    [Description("@#beginElement")]
    public extern void BeginElement();

    /// <summary>
    /// beginElementAt
    /// </summary>
    /// <param name="offset">offset</param>
    [Description("@#beginElementAt")]
    public extern void BeginElementAt(float offset);

    /// <summary>
    /// endElement
    /// </summary>
    [Description("@#endElement")]
    public extern void EndElement();

    /// <summary>
    /// endElementAt
    /// </summary>
    /// <param name="offset">offset</param>
    [Description("@#endElementAt")]
    public extern void EndElementAt(float offset);

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
/// SVGAnimateElement
/// </summary>
[ECMAScript]
[Description("@#SVGAnimateElement")]
public class SVGAnimateElement : SVGAnimationElement
{

}

/// <summary>
/// SVGSetElement
/// </summary>
[ECMAScript]
[Description("@#SVGSetElement")]
public class SVGSetElement : SVGAnimationElement
{

}

/// <summary>
/// SVGAnimateMotionElement
/// </summary>
[ECMAScript]
[Description("@#SVGAnimateMotionElement")]
public class SVGAnimateMotionElement : SVGAnimationElement
{

}

/// <summary>
/// SVGMPathElement
/// </summary>
[ECMAScript]
[Description("@#SVGMPathElement")]
public class SVGMPathElement : SVGElement
{


    #region mixin SVGURIReference
    /// <summary>
    /// href
    /// </summary>
    [Description("@#href")]
    public extern SVGAnimatedString Href { get; }
    #endregion
}

/// <summary>
/// SVGAnimateTransformElement
/// </summary>
[ECMAScript]
[Description("@#SVGAnimateTransformElement")]
public class SVGAnimateTransformElement : SVGAnimationElement
{

}

/// <summary>
/// SVGDiscardElement
/// </summary>
[ECMAScript]
[Description("@#SVGDiscardElement")]
public class SVGDiscardElement : SVGAnimationElement
{

}

/// <summary>
/// SVGSVGElement
/// </summary>
[ECMAScript]
[Description("@#SVGSVGElement")]
public partial class SVGSVGElement : SVGGraphicsElement
{
    /// <summary>
    /// pauseAnimations
    /// </summary>
    [Description("@#pauseAnimations")]
    public extern void PauseAnimations();

    /// <summary>
    /// unpauseAnimations
    /// </summary>
    [Description("@#unpauseAnimations")]
    public extern void UnpauseAnimations();

    /// <summary>
    /// animationsPaused
    /// </summary>
    [Description("@#animationsPaused")]
    public extern bool AnimationsPaused();

    /// <summary>
    /// getCurrentTime
    /// </summary>
    [Description("@#getCurrentTime")]
    public extern float GetCurrentTime();

    /// <summary>
    /// setCurrentTime
    /// </summary>
    /// <param name="seconds">seconds</param>
    [Description("@#setCurrentTime")]
    public extern void SetCurrentTime(float seconds);

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
    /// currentScale
    /// </summary>
    [Description("@#currentScale")]
    public extern float CurrentScale { get; set; }

    /// <summary>
    /// currentTranslate
    /// </summary>
    [Description("@#currentTranslate")]
    public extern DOMPointReadOnly CurrentTranslate { get; }

    /// <summary>
    /// getIntersectionList
    /// </summary>
    /// <param name="rect">rect</param>
    /// <param name="referenceElement">referenceElement</param>
    [Description("@#getIntersectionList")]
    public extern NodeList GetIntersectionList(DOMRectReadOnly rect, SVGElement? referenceElement);

    /// <summary>
    /// getEnclosureList
    /// </summary>
    /// <param name="rect">rect</param>
    /// <param name="referenceElement">referenceElement</param>
    [Description("@#getEnclosureList")]
    public extern NodeList GetEnclosureList(DOMRectReadOnly rect, SVGElement? referenceElement);

    /// <summary>
    /// checkIntersection
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="rect">rect</param>
    [Description("@#checkIntersection")]
    public extern bool CheckIntersection(SVGElement element, DOMRectReadOnly rect);

    /// <summary>
    /// checkEnclosure
    /// </summary>
    /// <param name="element">element</param>
    /// <param name="rect">rect</param>
    [Description("@#checkEnclosure")]
    public extern bool CheckEnclosure(SVGElement element, DOMRectReadOnly rect);

    /// <summary>
    /// deselectAll
    /// </summary>
    [Description("@#deselectAll")]
    public extern void DeselectAll();

    /// <summary>
    /// createSVGNumber
    /// </summary>
    [Description("@#createSVGNumber")]
    public extern SVGNumber CreateSVGNumber();

    /// <summary>
    /// createSVGLength
    /// </summary>
    [Description("@#createSVGLength")]
    public extern SVGLength CreateSVGLength();

    /// <summary>
    /// createSVGAngle
    /// </summary>
    [Description("@#createSVGAngle")]
    public extern SVGAngle CreateSVGAngle();

    /// <summary>
    /// createSVGPoint
    /// </summary>
    [Description("@#createSVGPoint")]
    public extern DOMPoint CreateSVGPoint();

    /// <summary>
    /// createSVGMatrix
    /// </summary>
    [Description("@#createSVGMatrix")]
    public extern DOMMatrix CreateSVGMatrix();

    /// <summary>
    /// createSVGRect
    /// </summary>
    [Description("@#createSVGRect")]
    public extern DOMRect CreateSVGRect();

    /// <summary>
    /// createSVGTransform
    /// </summary>
    [Description("@#createSVGTransform")]
    public extern SVGTransform CreateSVGTransform();

    /// <summary>
    /// createSVGTransformFromMatrix
    /// </summary>
    /// <param name="matrix">matrix</param>
    [Description("@#createSVGTransformFromMatrix")]
    public extern SVGTransform CreateSVGTransformFromMatrix(DOMMatrix2DInit? matrix = default);

    /// <summary>
    /// getElementById
    /// </summary>
    /// <param name="elementId">elementId</param>
    [Description("@#getElementById")]
    public extern Element GetElementById(string elementId);

    /// <summary>
    /// suspendRedraw
    /// </summary>
    /// <param name="maxWaitMilliseconds">maxWaitMilliseconds</param>
    [Description("@#suspendRedraw")]
    public extern uint SuspendRedraw(uint maxWaitMilliseconds);

    /// <summary>
    /// unsuspendRedraw
    /// </summary>
    /// <param name="suspendHandleId">suspendHandleID</param>
    [Description("@#unsuspendRedraw")]
    public extern void UnsuspendRedraw(uint suspendHandleId);

    /// <summary>
    /// unsuspendRedrawAll
    /// </summary>
    [Description("@#unsuspendRedrawAll")]
    public extern void UnsuspendRedrawAll();

    /// <summary>
    /// forceRedraw
    /// </summary>
    [Description("@#forceRedraw")]
    public extern void ForceRedraw();

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
}