namespace ECMAScript;

/// <summary>
/// HTMLBodyElement
/// </summary>
[ECMAScript]
[ECMAScriptName("")]
[Description("@#HTMLBodyElement")]
public partial class HTMLBodyElement : HTMLElement
{
    /// <summary>
    /// onorientationchange
    /// </summary>
    [Description("@#onorientationchange")]
    public extern EventHandler Onorientationchange { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    public extern HTMLBodyElement();

    /// <summary>
    /// text
    /// </summary>
    [Description("@#text")]
    public extern string Text { get; set; }

    /// <summary>
    /// link
    /// </summary>
    [Description("@#link")]
    public extern string Link { get; set; }

    /// <summary>
    /// vLink
    /// </summary>
    [Description("@#vLink")]
    public extern string VLink { get; set; }

    /// <summary>
    /// aLink
    /// </summary>
    [Description("@#aLink")]
    public extern string ALink { get; set; }

    /// <summary>
    /// bgColor
    /// </summary>
    [Description("@#bgColor")]
    public extern string BgColor { get; set; }

    /// <summary>
    /// background
    /// </summary>
    [Description("@#background")]
    public extern string Background { get; set; }

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

/// <summary>
/// CSSStyleDeclaration
/// </summary>
[ECMAScript]
[Description("@#CSSStyleDeclaration")]
public partial class CSSStyleDeclaration
{
    /// <summary>
    /// webkitTextFillColor
    /// </summary>
    [Description("@#webkitTextFillColor")]
    public extern string WebkitTextFillColor { get; set; }

    /// <summary>
    /// webkitTextStrokeColor
    /// </summary>
    [Description("@#webkitTextStrokeColor")]
    public extern string WebkitTextStrokeColor { get; set; }

    /// <summary>
    /// webkitTextStrokeWidth
    /// </summary>
    [Description("@#webkitTextStrokeWidth")]
    public extern string WebkitTextStrokeWidth { get; set; }

    /// <summary>
    /// webkitTextStroke
    /// </summary>
    [Description("@#webkitTextStroke")]
    public extern string WebkitTextStroke { get; set; }

    /// <summary>
    /// touchAction
    /// </summary>
    [Description("@#touchAction")]
    public extern string TouchAction { get; set; }

    /// <summary>
    /// webkitAlignItems
    /// </summary>
    [Description("@#webkitAlignItems")]
    public extern string WebkitAlignItems { get; set; }

    /// <summary>
    /// webkitAlignContent
    /// </summary>
    [Description("@#webkitAlignContent")]
    public extern string WebkitAlignContent { get; set; }

    /// <summary>
    /// webkitAlignSelf
    /// </summary>
    [Description("@#webkitAlignSelf")]
    public extern string WebkitAlignSelf { get; set; }

    /// <summary>
    /// webkitAnimationName
    /// </summary>
    [Description("@#webkitAnimationName")]
    public extern string WebkitAnimationName { get; set; }

    /// <summary>
    /// webkitAnimationDuration
    /// </summary>
    [Description("@#webkitAnimationDuration")]
    public extern string WebkitAnimationDuration { get; set; }

    /// <summary>
    /// webkitAnimationTimingFunction
    /// </summary>
    [Description("@#webkitAnimationTimingFunction")]
    public extern string WebkitAnimationTimingFunction { get; set; }

    /// <summary>
    /// webkitAnimationIterationCount
    /// </summary>
    [Description("@#webkitAnimationIterationCount")]
    public extern string WebkitAnimationIterationCount { get; set; }

    /// <summary>
    /// webkitAnimationDirection
    /// </summary>
    [Description("@#webkitAnimationDirection")]
    public extern string WebkitAnimationDirection { get; set; }

    /// <summary>
    /// webkitAnimationPlayState
    /// </summary>
    [Description("@#webkitAnimationPlayState")]
    public extern string WebkitAnimationPlayState { get; set; }

    /// <summary>
    /// webkitAnimationDelay
    /// </summary>
    [Description("@#webkitAnimationDelay")]
    public extern string WebkitAnimationDelay { get; set; }

    /// <summary>
    /// webkitAnimationFillMode
    /// </summary>
    [Description("@#webkitAnimationFillMode")]
    public extern string WebkitAnimationFillMode { get; set; }

    /// <summary>
    /// webkitAnimation
    /// </summary>
    [Description("@#webkitAnimation")]
    public extern string WebkitAnimation { get; set; }

    /// <summary>
    /// webkitBackfaceVisibility
    /// </summary>
    [Description("@#webkitBackfaceVisibility")]
    public extern string WebkitBackfaceVisibility { get; set; }

    /// <summary>
    /// webkitBackgroundClip
    /// </summary>
    [Description("@#webkitBackgroundClip")]
    public extern string WebkitBackgroundClip { get; set; }

    /// <summary>
    /// webkitBackgroundOrigin
    /// </summary>
    [Description("@#webkitBackgroundOrigin")]
    public extern string WebkitBackgroundOrigin { get; set; }

    /// <summary>
    /// webkitBackgroundSize
    /// </summary>
    [Description("@#webkitBackgroundSize")]
    public extern string WebkitBackgroundSize { get; set; }

    /// <summary>
    /// webkitBorderBottomLeftRadius
    /// </summary>
    [Description("@#webkitBorderBottomLeftRadius")]
    public extern string WebkitBorderBottomLeftRadius { get; set; }

    /// <summary>
    /// webkitBorderBottomRightRadius
    /// </summary>
    [Description("@#webkitBorderBottomRightRadius")]
    public extern string WebkitBorderBottomRightRadius { get; set; }

    /// <summary>
    /// webkitBorderTopLeftRadius
    /// </summary>
    [Description("@#webkitBorderTopLeftRadius")]
    public extern string WebkitBorderTopLeftRadius { get; set; }

    /// <summary>
    /// webkitBorderTopRightRadius
    /// </summary>
    [Description("@#webkitBorderTopRightRadius")]
    public extern string WebkitBorderTopRightRadius { get; set; }

    /// <summary>
    /// webkitBorderRadius
    /// </summary>
    [Description("@#webkitBorderRadius")]
    public extern string WebkitBorderRadius { get; set; }

    /// <summary>
    /// webkitBoxShadow
    /// </summary>
    [Description("@#webkitBoxShadow")]
    public extern string WebkitBoxShadow { get; set; }

    /// <summary>
    /// webkitBoxSizing
    /// </summary>
    [Description("@#webkitBoxSizing")]
    public extern string WebkitBoxSizing { get; set; }

    /// <summary>
    /// webkitFlex
    /// </summary>
    [Description("@#webkitFlex")]
    public extern string WebkitFlex { get; set; }

    /// <summary>
    /// webkitFlexBasis
    /// </summary>
    [Description("@#webkitFlexBasis")]
    public extern string WebkitFlexBasis { get; set; }

    /// <summary>
    /// webkitFlexDirection
    /// </summary>
    [Description("@#webkitFlexDirection")]
    public extern string WebkitFlexDirection { get; set; }

    /// <summary>
    /// webkitFlexFlow
    /// </summary>
    [Description("@#webkitFlexFlow")]
    public extern string WebkitFlexFlow { get; set; }

    /// <summary>
    /// webkitFlexGrow
    /// </summary>
    [Description("@#webkitFlexGrow")]
    public extern string WebkitFlexGrow { get; set; }

    /// <summary>
    /// webkitFlexShrink
    /// </summary>
    [Description("@#webkitFlexShrink")]
    public extern string WebkitFlexShrink { get; set; }

    /// <summary>
    /// webkitFlexWrap
    /// </summary>
    [Description("@#webkitFlexWrap")]
    public extern string WebkitFlexWrap { get; set; }

    /// <summary>
    /// webkitFilter
    /// </summary>
    [Description("@#webkitFilter")]
    public extern string WebkitFilter { get; set; }

    /// <summary>
    /// webkitJustifyContent
    /// </summary>
    [Description("@#webkitJustifyContent")]
    public extern string WebkitJustifyContent { get; set; }

    /// <summary>
    /// webkitMask
    /// </summary>
    [Description("@#webkitMask")]
    public extern string WebkitMask { get; set; }

    /// <summary>
    /// webkitMaskBoxImage
    /// </summary>
    [Description("@#webkitMaskBoxImage")]
    public extern string WebkitMaskBoxImage { get; set; }

    /// <summary>
    /// webkitMaskBoxImageOutset
    /// </summary>
    [Description("@#webkitMaskBoxImageOutset")]
    public extern string WebkitMaskBoxImageOutset { get; set; }

    /// <summary>
    /// webkitMaskBoxImageRepeat
    /// </summary>
    [Description("@#webkitMaskBoxImageRepeat")]
    public extern string WebkitMaskBoxImageRepeat { get; set; }

    /// <summary>
    /// webkitMaskBoxImageSlice
    /// </summary>
    [Description("@#webkitMaskBoxImageSlice")]
    public extern string WebkitMaskBoxImageSlice { get; set; }

    /// <summary>
    /// webkitMaskBoxImageSource
    /// </summary>
    [Description("@#webkitMaskBoxImageSource")]
    public extern string WebkitMaskBoxImageSource { get; set; }

    /// <summary>
    /// webkitMaskBoxImageWidth
    /// </summary>
    [Description("@#webkitMaskBoxImageWidth")]
    public extern string WebkitMaskBoxImageWidth { get; set; }

    /// <summary>
    /// webkitMaskClip
    /// </summary>
    [Description("@#webkitMaskClip")]
    public extern string WebkitMaskClip { get; set; }

    /// <summary>
    /// webkitMaskComposite
    /// </summary>
    [Description("@#webkitMaskComposite")]
    public extern string WebkitMaskComposite { get; set; }

    /// <summary>
    /// webkitMaskImage
    /// </summary>
    [Description("@#webkitMaskImage")]
    public extern string WebkitMaskImage { get; set; }

    /// <summary>
    /// webkitMaskOrigin
    /// </summary>
    [Description("@#webkitMaskOrigin")]
    public extern string WebkitMaskOrigin { get; set; }

    /// <summary>
    /// webkitMaskPosition
    /// </summary>
    [Description("@#webkitMaskPosition")]
    public extern string WebkitMaskPosition { get; set; }

    /// <summary>
    /// webkitMaskRepeat
    /// </summary>
    [Description("@#webkitMaskRepeat")]
    public extern string WebkitMaskRepeat { get; set; }

    /// <summary>
    /// webkitMaskSize
    /// </summary>
    [Description("@#webkitMaskSize")]
    public extern string WebkitMaskSize { get; set; }

    /// <summary>
    /// webkitOrder
    /// </summary>
    [Description("@#webkitOrder")]
    public extern string WebkitOrder { get; set; }

    /// <summary>
    /// webkitPerspective
    /// </summary>
    [Description("@#webkitPerspective")]
    public extern string WebkitPerspective { get; set; }

    /// <summary>
    /// webkitPerspectiveOrigin
    /// </summary>
    [Description("@#webkitPerspectiveOrigin")]
    public extern string WebkitPerspectiveOrigin { get; set; }

    /// <summary>
    /// webkitTransformOrigin
    /// </summary>
    [Description("@#webkitTransformOrigin")]
    public extern string WebkitTransformOrigin { get; set; }

    /// <summary>
    /// webkitTransformStyle
    /// </summary>
    [Description("@#webkitTransformStyle")]
    public extern string WebkitTransformStyle { get; set; }

    /// <summary>
    /// webkitTransform
    /// </summary>
    [Description("@#webkitTransform")]
    public extern string WebkitTransform { get; set; }

    /// <summary>
    /// webkitTransitionDelay
    /// </summary>
    [Description("@#webkitTransitionDelay")]
    public extern string WebkitTransitionDelay { get; set; }

    /// <summary>
    /// webkitTransitionDuration
    /// </summary>
    [Description("@#webkitTransitionDuration")]
    public extern string WebkitTransitionDuration { get; set; }

    /// <summary>
    /// webkitTransitionProperty
    /// </summary>
    [Description("@#webkitTransitionProperty")]
    public extern string WebkitTransitionProperty { get; set; }

    /// <summary>
    /// webkitTransitionTimingFunction
    /// </summary>
    [Description("@#webkitTransitionTimingFunction")]
    public extern string WebkitTransitionTimingFunction { get; set; }

    /// <summary>
    /// webkitTransition
    /// </summary>
    [Description("@#webkitTransition")]
    public extern string WebkitTransition { get; set; }

    /// <summary>
    /// webkitTextSizeAdjust
    /// </summary>
    [Description("@#webkitTextSizeAdjust")]
    public extern string WebkitTextSizeAdjust { get; set; }

    /// <summary>
    /// webkitBoxAlign
    /// </summary>
    [Description("@#webkitBoxAlign")]
    public extern string WebkitBoxAlign { get; set; }

    /// <summary>
    /// webkitBoxFlex
    /// </summary>
    [Description("@#webkitBoxFlex")]
    public extern string WebkitBoxFlex { get; set; }

    /// <summary>
    /// webkitBoxOrdinalGroup
    /// </summary>
    [Description("@#webkitBoxOrdinalGroup")]
    public extern string WebkitBoxOrdinalGroup { get; set; }

    /// <summary>
    /// webkitBoxOrient
    /// </summary>
    [Description("@#webkitBoxOrient")]
    public extern string WebkitBoxOrient { get; set; }

    /// <summary>
    /// webkitBoxPack
    /// </summary>
    [Description("@#webkitBoxPack")]
    public extern string WebkitBoxPack { get; set; }

    /// <summary>
    /// anchorName
    /// </summary>
    [Description("@#anchorName")]
    public extern string AnchorName { get; set; }

    /// <summary>
    /// anchorScope
    /// </summary>
    [Description("@#anchorScope")]
    public extern string AnchorScope { get; set; }

    /// <summary>
    /// positionAnchor
    /// </summary>
    [Description("@#positionAnchor")]
    public extern string PositionAnchor { get; set; }

    /// <summary>
    /// positionArea
    /// </summary>
    [Description("@#positionArea")]
    public extern string PositionArea { get; set; }

    /// <summary>
    /// justifySelf
    /// </summary>
    [Description("@#justifySelf")]
    public extern string JustifySelf { get; set; }

    /// <summary>
    /// alignSelf
    /// </summary>
    [Description("@#alignSelf")]
    public extern string AlignSelf { get; set; }

    /// <summary>
    /// justifyItems
    /// </summary>
    [Description("@#justifyItems")]
    public extern string JustifyItems { get; set; }

    /// <summary>
    /// alignItems
    /// </summary>
    [Description("@#alignItems")]
    public extern string AlignItems { get; set; }

    /// <summary>
    /// top
    /// </summary>
    [Description("@#top")]
    public extern string Top { get; set; }

    /// <summary>
    /// left
    /// </summary>
    [Description("@#left")]
    public extern string Left { get; set; }

    /// <summary>
    /// right
    /// </summary>
    [Description("@#right")]
    public extern string Right { get; set; }

    /// <summary>
    /// bottom
    /// </summary>
    [Description("@#bottom")]
    public extern string Bottom { get; set; }

    /// <summary>
    /// positionVisibility
    /// </summary>
    [Description("@#positionVisibility")]
    public extern string PositionVisibility { get; set; }

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
    /// minWidth
    /// </summary>
    [Description("@#minWidth")]
    public extern string MinWidth { get; set; }

    /// <summary>
    /// minHeight
    /// </summary>
    [Description("@#minHeight")]
    public extern string MinHeight { get; set; }

    /// <summary>
    /// maxWidth
    /// </summary>
    [Description("@#maxWidth")]
    public extern string MaxWidth { get; set; }

    /// <summary>
    /// maxHeight
    /// </summary>
    [Description("@#maxHeight")]
    public extern string MaxHeight { get; set; }

    /// <summary>
    /// marginTop
    /// </summary>
    [Description("@#marginTop")]
    public extern string MarginTop { get; set; }

    /// <summary>
    /// marginLeft
    /// </summary>
    [Description("@#marginLeft")]
    public extern string MarginLeft { get; set; }

    /// <summary>
    /// marginRight
    /// </summary>
    [Description("@#marginRight")]
    public extern string MarginRight { get; set; }

    /// <summary>
    /// marginBottom
    /// </summary>
    [Description("@#marginBottom")]
    public extern string MarginBottom { get; set; }

    /// <summary>
    /// positionTryFallbacks
    /// </summary>
    [Description("@#positionTryFallbacks")]
    public extern string PositionTryFallbacks { get; set; }

    /// <summary>
    /// positionTryOrder
    /// </summary>
    [Description("@#positionTryOrder")]
    public extern string PositionTryOrder { get; set; }

    /// <summary>
    /// positionTry
    /// </summary>
    [Description("@#positionTry")]
    public extern string PositionTry { get; set; }

    /// <summary>
    /// animationDuration
    /// </summary>
    [Description("@#animationDuration")]
    public extern string AnimationDuration { get; set; }

    /// <summary>
    /// animationComposition
    /// </summary>
    [Description("@#animationComposition")]
    public extern string AnimationComposition { get; set; }

    /// <summary>
    /// animationTimeline
    /// </summary>
    [Description("@#animationTimeline")]
    public extern string AnimationTimeline { get; set; }

    /// <summary>
    /// animationTriggerBehavior
    /// </summary>
    [Description("@#animationTriggerBehavior")]
    public extern string AnimationTriggerBehavior { get; set; }

    /// <summary>
    /// animationTriggerTimeline
    /// </summary>
    [Description("@#animationTriggerTimeline")]
    public extern string AnimationTriggerTimeline { get; set; }

    /// <summary>
    /// animationTriggerRange
    /// </summary>
    [Description("@#animationTriggerRange")]
    public extern string AnimationTriggerRange { get; set; }

    /// <summary>
    /// animationTriggerRangeStart
    /// </summary>
    [Description("@#animationTriggerRangeStart")]
    public extern string AnimationTriggerRangeStart { get; set; }

    /// <summary>
    /// animationTriggerRangeEnd
    /// </summary>
    [Description("@#animationTriggerRangeEnd")]
    public extern string AnimationTriggerRangeEnd { get; set; }

    /// <summary>
    /// animationTriggerExitRange
    /// </summary>
    [Description("@#animationTriggerExitRange")]
    public extern string AnimationTriggerExitRange { get; set; }

    /// <summary>
    /// animationTriggerExitRangeStart
    /// </summary>
    [Description("@#animationTriggerExitRangeStart")]
    public extern string AnimationTriggerExitRangeStart { get; set; }

    /// <summary>
    /// animationTriggerExitRangeEnd
    /// </summary>
    [Description("@#animationTriggerExitRangeEnd")]
    public extern string AnimationTriggerExitRangeEnd { get; set; }

    /// <summary>
    /// animationTrigger
    /// </summary>
    [Description("@#animationTrigger")]
    public extern string AnimationTrigger { get; set; }

    /// <summary>
    /// animationName
    /// </summary>
    [Description("@#animationName")]
    public extern string AnimationName { get; set; }

    /// <summary>
    /// animationTimingFunction
    /// </summary>
    [Description("@#animationTimingFunction")]
    public extern string AnimationTimingFunction { get; set; }

    /// <summary>
    /// animationIterationCount
    /// </summary>
    [Description("@#animationIterationCount")]
    public extern string AnimationIterationCount { get; set; }

    /// <summary>
    /// animationDirection
    /// </summary>
    [Description("@#animationDirection")]
    public extern string AnimationDirection { get; set; }

    /// <summary>
    /// animationPlayState
    /// </summary>
    [Description("@#animationPlayState")]
    public extern string AnimationPlayState { get; set; }

    /// <summary>
    /// animationDelay
    /// </summary>
    [Description("@#animationDelay")]
    public extern string AnimationDelay { get; set; }

    /// <summary>
    /// animationFillMode
    /// </summary>
    [Description("@#animationFillMode")]
    public extern string AnimationFillMode { get; set; }

    /// <summary>
    /// animation
    /// </summary>
    [Description("@#animation")]
    public extern string Animation { get; set; }

    /// <summary>
    /// all
    /// </summary>
    [Description("@#all")]
    public extern string All { get; set; }

    /// <summary>
    /// containerType
    /// </summary>
    [Description("@#containerType")]
    public extern string ContainerType { get; set; }

    /// <summary>
    /// containerName
    /// </summary>
    [Description("@#containerName")]
    public extern string ContainerName { get; set; }

    /// <summary>
    /// container
    /// </summary>
    [Description("@#container")]
    public extern string Container { get; set; }

    /// <summary>
    /// contain
    /// </summary>
    [Description("@#contain")]
    public extern string Contain { get; set; }

    /// <summary>
    /// contentVisibility
    /// </summary>
    [Description("@#contentVisibility")]
    public extern string ContentVisibility { get; set; }

    /// <summary>
    /// fontSizeAdjust
    /// </summary>
    [Description("@#fontSizeAdjust")]
    public extern string FontSizeAdjust { get; set; }

    /// <summary>
    /// fontFamily
    /// </summary>
    [Description("@#fontFamily")]
    public extern string FontFamily { get; set; }

    /// <summary>
    /// fontWeight
    /// </summary>
    [Description("@#fontWeight")]
    public extern string FontWeight { get; set; }

    /// <summary>
    /// fontWidth
    /// </summary>
    [Description("@#fontWidth")]
    public extern string FontWidth { get; set; }

    /// <summary>
    /// fontStyle
    /// </summary>
    [Description("@#fontStyle")]
    public extern string FontStyle { get; set; }

    /// <summary>
    /// fontSize
    /// </summary>
    [Description("@#fontSize")]
    public extern string FontSize { get; set; }

    /// <summary>
    /// font
    /// </summary>
    [Description("@#font")]
    public extern string Font { get; set; }

    /// <summary>
    /// fontSynthesisWeight
    /// </summary>
    [Description("@#fontSynthesisWeight")]
    public extern string FontSynthesisWeight { get; set; }

    /// <summary>
    /// fontSynthesisStyle
    /// </summary>
    [Description("@#fontSynthesisStyle")]
    public extern string FontSynthesisStyle { get; set; }

    /// <summary>
    /// fontSynthesisSmallCaps
    /// </summary>
    [Description("@#fontSynthesisSmallCaps")]
    public extern string FontSynthesisSmallCaps { get; set; }

    /// <summary>
    /// fontSynthesisPosition
    /// </summary>
    [Description("@#fontSynthesisPosition")]
    public extern string FontSynthesisPosition { get; set; }

    /// <summary>
    /// fontSynthesis
    /// </summary>
    [Description("@#fontSynthesis")]
    public extern string FontSynthesis { get; set; }

    /// <summary>
    /// fontKerning
    /// </summary>
    [Description("@#fontKerning")]
    public extern string FontKerning { get; set; }

    /// <summary>
    /// fontVariantLigatures
    /// </summary>
    [Description("@#fontVariantLigatures")]
    public extern string FontVariantLigatures { get; set; }

    /// <summary>
    /// fontVariantPosition
    /// </summary>
    [Description("@#fontVariantPosition")]
    public extern string FontVariantPosition { get; set; }

    /// <summary>
    /// fontVariantCaps
    /// </summary>
    [Description("@#fontVariantCaps")]
    public extern string FontVariantCaps { get; set; }

    /// <summary>
    /// fontVariantNumeric
    /// </summary>
    [Description("@#fontVariantNumeric")]
    public extern string FontVariantNumeric { get; set; }

    /// <summary>
    /// fontVariantAlternates
    /// </summary>
    [Description("@#fontVariantAlternates")]
    public extern string FontVariantAlternates { get; set; }

    /// <summary>
    /// fontVariantEastAsian
    /// </summary>
    [Description("@#fontVariantEastAsian")]
    public extern string FontVariantEastAsian { get; set; }

    /// <summary>
    /// fontVariant
    /// </summary>
    [Description("@#fontVariant")]
    public extern string FontVariant { get; set; }

    /// <summary>
    /// fontFeatureSettings
    /// </summary>
    [Description("@#fontFeatureSettings")]
    public extern string FontFeatureSettings { get; set; }

    /// <summary>
    /// fontLanguageOverride
    /// </summary>
    [Description("@#fontLanguageOverride")]
    public extern string FontLanguageOverride { get; set; }

    /// <summary>
    /// fontOpticalSizing
    /// </summary>
    [Description("@#fontOpticalSizing")]
    public extern string FontOpticalSizing { get; set; }

    /// <summary>
    /// fontVariationSettings
    /// </summary>
    [Description("@#fontVariationSettings")]
    public extern string FontVariationSettings { get; set; }

    /// <summary>
    /// fontPalette
    /// </summary>
    [Description("@#fontPalette")]
    public extern string FontPalette { get; set; }

    /// <summary>
    /// fontVariantEmoji
    /// </summary>
    [Description("@#fontVariantEmoji")]
    public extern string FontVariantEmoji { get; set; }

    /// <summary>
    /// fontStretch
    /// </summary>
    [Description("@#fontStretch")]
    public extern string FontStretch { get; set; }

    /// <summary>
    /// clipPath
    /// </summary>
    [Description("@#clipPath")]
    public extern string ClipPath { get; set; }

    /// <summary>
    /// clipRule
    /// </summary>
    [Description("@#clipRule")]
    public extern string ClipRule { get; set; }

    /// <summary>
    /// maskImage
    /// </summary>
    [Description("@#maskImage")]
    public extern string MaskImage { get; set; }

    /// <summary>
    /// maskMode
    /// </summary>
    [Description("@#maskMode")]
    public extern string MaskMode { get; set; }

    /// <summary>
    /// maskRepeat
    /// </summary>
    [Description("@#maskRepeat")]
    public extern string MaskRepeat { get; set; }

    /// <summary>
    /// maskPosition
    /// </summary>
    [Description("@#maskPosition")]
    public extern string MaskPosition { get; set; }

    /// <summary>
    /// maskClip
    /// </summary>
    [Description("@#maskClip")]
    public extern string MaskClip { get; set; }

    /// <summary>
    /// maskOrigin
    /// </summary>
    [Description("@#maskOrigin")]
    public extern string MaskOrigin { get; set; }

    /// <summary>
    /// maskSize
    /// </summary>
    [Description("@#maskSize")]
    public extern string MaskSize { get; set; }

    /// <summary>
    /// maskComposite
    /// </summary>
    [Description("@#maskComposite")]
    public extern string MaskComposite { get; set; }

    /// <summary>
    /// mask
    /// </summary>
    [Description("@#mask")]
    public extern string Mask { get; set; }

    /// <summary>
    /// maskBorderSource
    /// </summary>
    [Description("@#maskBorderSource")]
    public extern string MaskBorderSource { get; set; }

    /// <summary>
    /// maskBorderMode
    /// </summary>
    [Description("@#maskBorderMode")]
    public extern string MaskBorderMode { get; set; }

    /// <summary>
    /// maskBorderSlice
    /// </summary>
    [Description("@#maskBorderSlice")]
    public extern string MaskBorderSlice { get; set; }

    /// <summary>
    /// maskBorderWidth
    /// </summary>
    [Description("@#maskBorderWidth")]
    public extern string MaskBorderWidth { get; set; }

    /// <summary>
    /// maskBorderOutset
    /// </summary>
    [Description("@#maskBorderOutset")]
    public extern string MaskBorderOutset { get; set; }

    /// <summary>
    /// maskBorderRepeat
    /// </summary>
    [Description("@#maskBorderRepeat")]
    public extern string MaskBorderRepeat { get; set; }

    /// <summary>
    /// maskBorder
    /// </summary>
    [Description("@#maskBorder")]
    public extern string MaskBorder { get; set; }

    /// <summary>
    /// maskType
    /// </summary>
    [Description("@#maskType")]
    public extern string MaskType { get; set; }

    /// <summary>
    /// clip
    /// </summary>
    [Description("@#clip")]
    public extern string Clip { get; set; }

    /// <summary>
    /// spatialNavigationContain
    /// </summary>
    [Description("@#spatialNavigationContain")]
    public extern string SpatialNavigationContain { get; set; }

    /// <summary>
    /// spatialNavigationAction
    /// </summary>
    [Description("@#spatialNavigationAction")]
    public extern string SpatialNavigationAction { get; set; }

    /// <summary>
    /// spatialNavigationFunction
    /// </summary>
    [Description("@#spatialNavigationFunction")]
    public extern string SpatialNavigationFunction { get; set; }

    /// <summary>
    /// flowInto
    /// </summary>
    [Description("@#flowInto")]
    public extern string FlowInto { get; set; }

    /// <summary>
    /// flowFrom
    /// </summary>
    [Description("@#flowFrom")]
    public extern string FlowFrom { get; set; }

    /// <summary>
    /// regionFragment
    /// </summary>
    [Description("@#regionFragment")]
    public extern string RegionFragment { get; set; }

    /// <summary>
    /// scrollInitialTarget
    /// </summary>
    [Description("@#scrollInitialTarget")]
    public extern string ScrollInitialTarget { get; set; }

    /// <summary>
    /// transitionBehavior
    /// </summary>
    [Description("@#transitionBehavior")]
    public extern string TransitionBehavior { get; set; }

    /// <summary>
    /// transitionProperty
    /// </summary>
    [Description("@#transitionProperty")]
    public extern string TransitionProperty { get; set; }

    /// <summary>
    /// transitionDuration
    /// </summary>
    [Description("@#transitionDuration")]
    public extern string TransitionDuration { get; set; }

    /// <summary>
    /// transitionTimingFunction
    /// </summary>
    [Description("@#transitionTimingFunction")]
    public extern string TransitionTimingFunction { get; set; }

    /// <summary>
    /// transitionDelay
    /// </summary>
    [Description("@#transitionDelay")]
    public extern string TransitionDelay { get; set; }

    /// <summary>
    /// transition
    /// </summary>
    [Description("@#transition")]
    public extern string Transition { get; set; }

    /// <summary>
    /// viewTransitionClass
    /// </summary>
    [Description("@#viewTransitionClass")]
    public extern string ViewTransitionClass { get; set; }

    /// <summary>
    /// viewTransitionGroup
    /// </summary>
    [Description("@#viewTransitionGroup")]
    public extern string ViewTransitionGroup { get; set; }

    /// <summary>
    /// viewTransitionName
    /// </summary>
    [Description("@#viewTransitionName")]
    public extern string ViewTransitionName { get; set; }

    /// <summary>
    /// zoom
    /// </summary>
    [Description("@#zoom")]
    public extern string Zoom { get; set; }

    /// <summary>
    /// filter
    /// </summary>
    [Description("@#filter")]
    public extern string Filter { get; set; }

    /// <summary>
    /// floodColor
    /// </summary>
    [Description("@#floodColor")]
    public extern string FloodColor { get; set; }

    /// <summary>
    /// floodOpacity
    /// </summary>
    [Description("@#floodOpacity")]
    public extern string FloodOpacity { get; set; }

    /// <summary>
    /// colorInterpolationFilters
    /// </summary>
    [Description("@#colorInterpolationFilters")]
    public extern string ColorInterpolationFilters { get; set; }

    /// <summary>
    /// lightingColor
    /// </summary>
    [Description("@#lightingColor")]
    public extern string LightingColor { get; set; }

    /// <summary>
    /// display
    /// </summary>
    [Description("@#display")]
    public extern string Display { get; set; }

    /// <summary>
    /// mathStyle
    /// </summary>
    [Description("@#mathStyle")]
    public extern string MathStyle { get; set; }

    /// <summary>
    /// mathShift
    /// </summary>
    [Description("@#mathShift")]
    public extern string MathShift { get; set; }

    /// <summary>
    /// mathDepth
    /// </summary>
    [Description("@#mathDepth")]
    public extern string MathDepth { get; set; }

    /// <summary>
    /// pointerTimelineName
    /// </summary>
    [Description("@#pointerTimelineName")]
    public extern string PointerTimelineName { get; set; }

    /// <summary>
    /// pointerTimelineAxis
    /// </summary>
    [Description("@#pointerTimelineAxis")]
    public extern string PointerTimelineAxis { get; set; }

    /// <summary>
    /// pointerTimeline
    /// </summary>
    [Description("@#pointerTimeline")]
    public extern string PointerTimeline { get; set; }

    /// <summary>
    /// animationRangeCenter
    /// </summary>
    [Description("@#animationRangeCenter")]
    public extern string AnimationRangeCenter { get; set; }

    /// <summary>
    /// scrollTimelineName
    /// </summary>
    [Description("@#scrollTimelineName")]
    public extern string ScrollTimelineName { get; set; }

    /// <summary>
    /// scrollTimelineAxis
    /// </summary>
    [Description("@#scrollTimelineAxis")]
    public extern string ScrollTimelineAxis { get; set; }

    /// <summary>
    /// scrollTimeline
    /// </summary>
    [Description("@#scrollTimeline")]
    public extern string ScrollTimeline { get; set; }

    /// <summary>
    /// viewTimelineName
    /// </summary>
    [Description("@#viewTimelineName")]
    public extern string ViewTimelineName { get; set; }

    /// <summary>
    /// viewTimelineAxis
    /// </summary>
    [Description("@#viewTimelineAxis")]
    public extern string ViewTimelineAxis { get; set; }

    /// <summary>
    /// viewTimelineInset
    /// </summary>
    [Description("@#viewTimelineInset")]
    public extern string ViewTimelineInset { get; set; }

    /// <summary>
    /// viewTimeline
    /// </summary>
    [Description("@#viewTimeline")]
    public extern string ViewTimeline { get; set; }

    /// <summary>
    /// animationRange
    /// </summary>
    [Description("@#animationRange")]
    public extern string AnimationRange { get; set; }

    /// <summary>
    /// animationRangeStart
    /// </summary>
    [Description("@#animationRangeStart")]
    public extern string AnimationRangeStart { get; set; }

    /// <summary>
    /// animationRangeEnd
    /// </summary>
    [Description("@#animationRangeEnd")]
    public extern string AnimationRangeEnd { get; set; }

    /// <summary>
    /// timelineScope
    /// </summary>
    [Description("@#timelineScope")]
    public extern string TimelineScope { get; set; }

    /// <summary>
    /// cx
    /// </summary>
    [Description("@#cx")]
    public extern string Cx { get; set; }

    /// <summary>
    /// cy
    /// </summary>
    [Description("@#cy")]
    public extern string Cy { get; set; }

    /// <summary>
    /// r
    /// </summary>
    [Description("@#r")]
    public extern string R { get; set; }

    /// <summary>
    /// rx
    /// </summary>
    [Description("@#rx")]
    public extern string Rx { get; set; }

    /// <summary>
    /// ry
    /// </summary>
    [Description("@#ry")]
    public extern string Ry { get; set; }

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern string X { get; set; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern string Y { get; set; }

    /// <summary>
    /// vectorEffect
    /// </summary>
    [Description("@#vectorEffect")]
    public extern string VectorEffect { get; set; }

    /// <summary>
    /// d
    /// </summary>
    [Description("@#d")]
    public extern string D { get; set; }

    /// <summary>
    /// shapeSubtract
    /// </summary>
    [Description("@#shapeSubtract")]
    public extern string ShapeSubtract { get; set; }

    /// <summary>
    /// textAnchor
    /// </summary>
    [Description("@#textAnchor")]
    public extern string TextAnchor { get; set; }

    /// <summary>
    /// fill
    /// </summary>
    [Description("@#fill")]
    public extern string Fill { get; set; }

    /// <summary>
    /// stroke
    /// </summary>
    [Description("@#stroke")]
    public extern string Stroke { get; set; }

    /// <summary>
    /// markerStart
    /// </summary>
    [Description("@#markerStart")]
    public extern string MarkerStart { get; set; }

    /// <summary>
    /// markerMid
    /// </summary>
    [Description("@#markerMid")]
    public extern string MarkerMid { get; set; }

    /// <summary>
    /// markerEnd
    /// </summary>
    [Description("@#markerEnd")]
    public extern string MarkerEnd { get; set; }

    /// <summary>
    /// marker
    /// </summary>
    [Description("@#marker")]
    public extern string Marker { get; set; }

    /// <summary>
    /// paintOrder
    /// </summary>
    [Description("@#paintOrder")]
    public extern string PaintOrder { get; set; }

    /// <summary>
    /// colorInterpolation
    /// </summary>
    [Description("@#colorInterpolation")]
    public extern string ColorInterpolation { get; set; }

    /// <summary>
    /// shapeRendering
    /// </summary>
    [Description("@#shapeRendering")]
    public extern string ShapeRendering { get; set; }

    /// <summary>
    /// textRendering
    /// </summary>
    [Description("@#textRendering")]
    public extern string TextRendering { get; set; }

    /// <summary>
    /// pointerEvents
    /// </summary>
    [Description("@#pointerEvents")]
    public extern string PointerEvents { get; set; }

    /// <summary>
    /// stopColor
    /// </summary>
    [Description("@#stopColor")]
    public extern string StopColor { get; set; }

    /// <summary>
    /// stopOpacity
    /// </summary>
    [Description("@#stopOpacity")]
    public extern string StopOpacity { get; set; }

    /// <summary>
    /// mixBlendMode
    /// </summary>
    [Description("@#mixBlendMode")]
    public extern string MixBlendMode { get; set; }

    /// <summary>
    /// isolation
    /// </summary>
    [Description("@#isolation")]
    public extern string Isolation { get; set; }

    /// <summary>
    /// backgroundBlendMode
    /// </summary>
    [Description("@#backgroundBlendMode")]
    public extern string BackgroundBlendMode { get; set; }

    /// <summary>
    /// alignContent
    /// </summary>
    [Description("@#alignContent")]
    public extern string AlignContent { get; set; }

    /// <summary>
    /// justifyContent
    /// </summary>
    [Description("@#justifyContent")]
    public extern string JustifyContent { get; set; }

    /// <summary>
    /// placeContent
    /// </summary>
    [Description("@#placeContent")]
    public extern string PlaceContent { get; set; }

    /// <summary>
    /// placeSelf
    /// </summary>
    [Description("@#placeSelf")]
    public extern string PlaceSelf { get; set; }

    /// <summary>
    /// placeItems
    /// </summary>
    [Description("@#placeItems")]
    public extern string PlaceItems { get; set; }

    /// <summary>
    /// rowGap
    /// </summary>
    [Description("@#rowGap")]
    public extern string RowGap { get; set; }

    /// <summary>
    /// columnGap
    /// </summary>
    [Description("@#columnGap")]
    public extern string ColumnGap { get; set; }

    /// <summary>
    /// gap
    /// </summary>
    [Description("@#gap")]
    public extern string Gap { get; set; }

    /// <summary>
    /// gridRowGap
    /// </summary>
    [Description("@#gridRowGap")]
    public extern string GridRowGap { get; set; }

    /// <summary>
    /// gridColumnGap
    /// </summary>
    [Description("@#gridColumnGap")]
    public extern string GridColumnGap { get; set; }

    /// <summary>
    /// gridGap
    /// </summary>
    [Description("@#gridGap")]
    public extern string GridGap { get; set; }

    /// <summary>
    /// backgroundRepeatX
    /// </summary>
    [Description("@#backgroundRepeatX")]
    public extern string BackgroundRepeatX { get; set; }

    /// <summary>
    /// backgroundRepeatY
    /// </summary>
    [Description("@#backgroundRepeatY")]
    public extern string BackgroundRepeatY { get; set; }

    /// <summary>
    /// backgroundRepeatBlock
    /// </summary>
    [Description("@#backgroundRepeatBlock")]
    public extern string BackgroundRepeatBlock { get; set; }

    /// <summary>
    /// backgroundRepeatInline
    /// </summary>
    [Description("@#backgroundRepeatInline")]
    public extern string BackgroundRepeatInline { get; set; }

    /// <summary>
    /// backgroundRepeat
    /// </summary>
    [Description("@#backgroundRepeat")]
    public extern string BackgroundRepeat { get; set; }

    /// <summary>
    /// backgroundPosition
    /// </summary>
    [Description("@#backgroundPosition")]
    public extern string BackgroundPosition { get; set; }

    /// <summary>
    /// backgroundPositionX
    /// </summary>
    [Description("@#backgroundPositionX")]
    public extern string BackgroundPositionX { get; set; }

    /// <summary>
    /// backgroundPositionY
    /// </summary>
    [Description("@#backgroundPositionY")]
    public extern string BackgroundPositionY { get; set; }

    /// <summary>
    /// backgroundPositionInline
    /// </summary>
    [Description("@#backgroundPositionInline")]
    public extern string BackgroundPositionInline { get; set; }

    /// <summary>
    /// backgroundPositionBlock
    /// </summary>
    [Description("@#backgroundPositionBlock")]
    public extern string BackgroundPositionBlock { get; set; }

    /// <summary>
    /// backgroundClip
    /// </summary>
    [Description("@#backgroundClip")]
    public extern string BackgroundClip { get; set; }

    /// <summary>
    /// backgroundTbd
    /// </summary>
    [Description("@#backgroundTbd")]
    public extern string BackgroundTbd { get; set; }

    /// <summary>
    /// backgroundColor
    /// </summary>
    [Description("@#backgroundColor")]
    public extern string BackgroundColor { get; set; }

    /// <summary>
    /// backgroundImage
    /// </summary>
    [Description("@#backgroundImage")]
    public extern string BackgroundImage { get; set; }

    /// <summary>
    /// backgroundAttachment
    /// </summary>
    [Description("@#backgroundAttachment")]
    public extern string BackgroundAttachment { get; set; }

    /// <summary>
    /// backgroundOrigin
    /// </summary>
    [Description("@#backgroundOrigin")]
    public extern string BackgroundOrigin { get; set; }

    /// <summary>
    /// backgroundSize
    /// </summary>
    [Description("@#backgroundSize")]
    public extern string BackgroundSize { get; set; }

    /// <summary>
    /// background
    /// </summary>
    [Description("@#background")]
    public extern string Background { get; set; }

    /// <summary>
    /// borderStyle
    /// </summary>
    [Description("@#borderStyle")]
    public extern string BorderStyle { get; set; }

    /// <summary>
    /// borderWidth
    /// </summary>
    [Description("@#borderWidth")]
    public extern string BorderWidth { get; set; }

    /// <summary>
    /// border
    /// </summary>
    [Description("@#border")]
    public extern string Border { get; set; }

    /// <summary>
    /// borderImageSource
    /// </summary>
    [Description("@#borderImageSource")]
    public extern string BorderImageSource { get; set; }

    /// <summary>
    /// borderImageSlice
    /// </summary>
    [Description("@#borderImageSlice")]
    public extern string BorderImageSlice { get; set; }

    /// <summary>
    /// borderImageWidth
    /// </summary>
    [Description("@#borderImageWidth")]
    public extern string BorderImageWidth { get; set; }

    /// <summary>
    /// borderImageOutset
    /// </summary>
    [Description("@#borderImageOutset")]
    public extern string BorderImageOutset { get; set; }

    /// <summary>
    /// borderImageRepeat
    /// </summary>
    [Description("@#borderImageRepeat")]
    public extern string BorderImageRepeat { get; set; }

    /// <summary>
    /// borderImage
    /// </summary>
    [Description("@#borderImage")]
    public extern string BorderImage { get; set; }

    /// <summary>
    /// borderTopColor
    /// </summary>
    [Description("@#borderTopColor")]
    public extern string BorderTopColor { get; set; }

    /// <summary>
    /// borderRightColor
    /// </summary>
    [Description("@#borderRightColor")]
    public extern string BorderRightColor { get; set; }

    /// <summary>
    /// borderBottomColor
    /// </summary>
    [Description("@#borderBottomColor")]
    public extern string BorderBottomColor { get; set; }

    /// <summary>
    /// borderLeftColor
    /// </summary>
    [Description("@#borderLeftColor")]
    public extern string BorderLeftColor { get; set; }

    /// <summary>
    /// borderBlockStartColor
    /// </summary>
    [Description("@#borderBlockStartColor")]
    public extern string BorderBlockStartColor { get; set; }

    /// <summary>
    /// borderBlockEndColor
    /// </summary>
    [Description("@#borderBlockEndColor")]
    public extern string BorderBlockEndColor { get; set; }

    /// <summary>
    /// borderInlineStartColor
    /// </summary>
    [Description("@#borderInlineStartColor")]
    public extern string BorderInlineStartColor { get; set; }

    /// <summary>
    /// borderInlineEndColor
    /// </summary>
    [Description("@#borderInlineEndColor")]
    public extern string BorderInlineEndColor { get; set; }

    /// <summary>
    /// borderColor
    /// </summary>
    [Description("@#borderColor")]
    public extern string BorderColor { get; set; }

    /// <summary>
    /// borderBlockColor
    /// </summary>
    [Description("@#borderBlockColor")]
    public extern string BorderBlockColor { get; set; }

    /// <summary>
    /// borderInlineColor
    /// </summary>
    [Description("@#borderInlineColor")]
    public extern string BorderInlineColor { get; set; }

    /// <summary>
    /// borderTopStyle
    /// </summary>
    [Description("@#borderTopStyle")]
    public extern string BorderTopStyle { get; set; }

    /// <summary>
    /// borderRightStyle
    /// </summary>
    [Description("@#borderRightStyle")]
    public extern string BorderRightStyle { get; set; }

    /// <summary>
    /// borderBottomStyle
    /// </summary>
    [Description("@#borderBottomStyle")]
    public extern string BorderBottomStyle { get; set; }

    /// <summary>
    /// borderLeftStyle
    /// </summary>
    [Description("@#borderLeftStyle")]
    public extern string BorderLeftStyle { get; set; }

    /// <summary>
    /// borderBlockStartStyle
    /// </summary>
    [Description("@#borderBlockStartStyle")]
    public extern string BorderBlockStartStyle { get; set; }

    /// <summary>
    /// borderBlockEndStyle
    /// </summary>
    [Description("@#borderBlockEndStyle")]
    public extern string BorderBlockEndStyle { get; set; }

    /// <summary>
    /// borderInlineStartStyle
    /// </summary>
    [Description("@#borderInlineStartStyle")]
    public extern string BorderInlineStartStyle { get; set; }

    /// <summary>
    /// borderInlineEndStyle
    /// </summary>
    [Description("@#borderInlineEndStyle")]
    public extern string BorderInlineEndStyle { get; set; }

    /// <summary>
    /// borderBlockStyle
    /// </summary>
    [Description("@#borderBlockStyle")]
    public extern string BorderBlockStyle { get; set; }

    /// <summary>
    /// borderInlineStyle
    /// </summary>
    [Description("@#borderInlineStyle")]
    public extern string BorderInlineStyle { get; set; }

    /// <summary>
    /// borderTopWidth
    /// </summary>
    [Description("@#borderTopWidth")]
    public extern string BorderTopWidth { get; set; }

    /// <summary>
    /// borderRightWidth
    /// </summary>
    [Description("@#borderRightWidth")]
    public extern string BorderRightWidth { get; set; }

    /// <summary>
    /// borderBottomWidth
    /// </summary>
    [Description("@#borderBottomWidth")]
    public extern string BorderBottomWidth { get; set; }

    /// <summary>
    /// borderLeftWidth
    /// </summary>
    [Description("@#borderLeftWidth")]
    public extern string BorderLeftWidth { get; set; }

    /// <summary>
    /// borderBlockStartWidth
    /// </summary>
    [Description("@#borderBlockStartWidth")]
    public extern string BorderBlockStartWidth { get; set; }

    /// <summary>
    /// borderBlockEndWidth
    /// </summary>
    [Description("@#borderBlockEndWidth")]
    public extern string BorderBlockEndWidth { get; set; }

    /// <summary>
    /// borderInlineStartWidth
    /// </summary>
    [Description("@#borderInlineStartWidth")]
    public extern string BorderInlineStartWidth { get; set; }

    /// <summary>
    /// borderInlineEndWidth
    /// </summary>
    [Description("@#borderInlineEndWidth")]
    public extern string BorderInlineEndWidth { get; set; }

    /// <summary>
    /// borderBlockWidth
    /// </summary>
    [Description("@#borderBlockWidth")]
    public extern string BorderBlockWidth { get; set; }

    /// <summary>
    /// borderInlineWidth
    /// </summary>
    [Description("@#borderInlineWidth")]
    public extern string BorderInlineWidth { get; set; }

    /// <summary>
    /// borderTop
    /// </summary>
    [Description("@#borderTop")]
    public extern string BorderTop { get; set; }

    /// <summary>
    /// borderRight
    /// </summary>
    [Description("@#borderRight")]
    public extern string BorderRight { get; set; }

    /// <summary>
    /// borderBottom
    /// </summary>
    [Description("@#borderBottom")]
    public extern string BorderBottom { get; set; }

    /// <summary>
    /// borderLeft
    /// </summary>
    [Description("@#borderLeft")]
    public extern string BorderLeft { get; set; }

    /// <summary>
    /// borderBlockStart
    /// </summary>
    [Description("@#borderBlockStart")]
    public extern string BorderBlockStart { get; set; }

    /// <summary>
    /// borderBlockEnd
    /// </summary>
    [Description("@#borderBlockEnd")]
    public extern string BorderBlockEnd { get; set; }

    /// <summary>
    /// borderInlineStart
    /// </summary>
    [Description("@#borderInlineStart")]
    public extern string BorderInlineStart { get; set; }

    /// <summary>
    /// borderInlineEnd
    /// </summary>
    [Description("@#borderInlineEnd")]
    public extern string BorderInlineEnd { get; set; }

    /// <summary>
    /// borderBlock
    /// </summary>
    [Description("@#borderBlock")]
    public extern string BorderBlock { get; set; }

    /// <summary>
    /// borderInline
    /// </summary>
    [Description("@#borderInline")]
    public extern string BorderInline { get; set; }

    /// <summary>
    /// borderTopLeftRadius
    /// </summary>
    [Description("@#borderTopLeftRadius")]
    public extern string BorderTopLeftRadius { get; set; }

    /// <summary>
    /// borderTopRightRadius
    /// </summary>
    [Description("@#borderTopRightRadius")]
    public extern string BorderTopRightRadius { get; set; }

    /// <summary>
    /// borderBottomRightRadius
    /// </summary>
    [Description("@#borderBottomRightRadius")]
    public extern string BorderBottomRightRadius { get; set; }

    /// <summary>
    /// borderBottomLeftRadius
    /// </summary>
    [Description("@#borderBottomLeftRadius")]
    public extern string BorderBottomLeftRadius { get; set; }

    /// <summary>
    /// borderStartStartRadius
    /// </summary>
    [Description("@#borderStartStartRadius")]
    public extern string BorderStartStartRadius { get; set; }

    /// <summary>
    /// borderStartEndRadius
    /// </summary>
    [Description("@#borderStartEndRadius")]
    public extern string BorderStartEndRadius { get; set; }

    /// <summary>
    /// borderEndStartRadius
    /// </summary>
    [Description("@#borderEndStartRadius")]
    public extern string BorderEndStartRadius { get; set; }

    /// <summary>
    /// borderEndEndRadius
    /// </summary>
    [Description("@#borderEndEndRadius")]
    public extern string BorderEndEndRadius { get; set; }

    /// <summary>
    /// borderTopRadius
    /// </summary>
    [Description("@#borderTopRadius")]
    public extern string BorderTopRadius { get; set; }

    /// <summary>
    /// borderRightRadius
    /// </summary>
    [Description("@#borderRightRadius")]
    public extern string BorderRightRadius { get; set; }

    /// <summary>
    /// borderBottomRadius
    /// </summary>
    [Description("@#borderBottomRadius")]
    public extern string BorderBottomRadius { get; set; }

    /// <summary>
    /// borderLeftRadius
    /// </summary>
    [Description("@#borderLeftRadius")]
    public extern string BorderLeftRadius { get; set; }

    /// <summary>
    /// borderBlockStartRadius
    /// </summary>
    [Description("@#borderBlockStartRadius")]
    public extern string BorderBlockStartRadius { get; set; }

    /// <summary>
    /// borderBlockEndRadius
    /// </summary>
    [Description("@#borderBlockEndRadius")]
    public extern string BorderBlockEndRadius { get; set; }

    /// <summary>
    /// borderInlineStartRadius
    /// </summary>
    [Description("@#borderInlineStartRadius")]
    public extern string BorderInlineStartRadius { get; set; }

    /// <summary>
    /// borderInlineEndRadius
    /// </summary>
    [Description("@#borderInlineEndRadius")]
    public extern string BorderInlineEndRadius { get; set; }

    /// <summary>
    /// borderRadius
    /// </summary>
    [Description("@#borderRadius")]
    public extern string BorderRadius { get; set; }

    /// <summary>
    /// cornerShape
    /// </summary>
    [Description("@#cornerShape")]
    public extern string CornerShape { get; set; }

    /// <summary>
    /// cornerTopLeftShape
    /// </summary>
    [Description("@#cornerTopLeftShape")]
    public extern string CornerTopLeftShape { get; set; }

    /// <summary>
    /// cornerTopRightShape
    /// </summary>
    [Description("@#cornerTopRightShape")]
    public extern string CornerTopRightShape { get; set; }

    /// <summary>
    /// cornerBottomRightShape
    /// </summary>
    [Description("@#cornerBottomRightShape")]
    public extern string CornerBottomRightShape { get; set; }

    /// <summary>
    /// cornerBottomLeftShape
    /// </summary>
    [Description("@#cornerBottomLeftShape")]
    public extern string CornerBottomLeftShape { get; set; }

    /// <summary>
    /// cornerStartStartShape
    /// </summary>
    [Description("@#cornerStartStartShape")]
    public extern string CornerStartStartShape { get; set; }

    /// <summary>
    /// cornerStartEndShape
    /// </summary>
    [Description("@#cornerStartEndShape")]
    public extern string CornerStartEndShape { get; set; }

    /// <summary>
    /// cornerEndStartShape
    /// </summary>
    [Description("@#cornerEndStartShape")]
    public extern string CornerEndStartShape { get; set; }

    /// <summary>
    /// cornerEndEndShape
    /// </summary>
    [Description("@#cornerEndEndShape")]
    public extern string CornerEndEndShape { get; set; }

    /// <summary>
    /// cornerTopShape
    /// </summary>
    [Description("@#cornerTopShape")]
    public extern string CornerTopShape { get; set; }

    /// <summary>
    /// cornerRightShape
    /// </summary>
    [Description("@#cornerRightShape")]
    public extern string CornerRightShape { get; set; }

    /// <summary>
    /// cornerBottomShape
    /// </summary>
    [Description("@#cornerBottomShape")]
    public extern string CornerBottomShape { get; set; }

    /// <summary>
    /// cornerLeftShape
    /// </summary>
    [Description("@#cornerLeftShape")]
    public extern string CornerLeftShape { get; set; }

    /// <summary>
    /// cornerBlockStartShape
    /// </summary>
    [Description("@#cornerBlockStartShape")]
    public extern string CornerBlockStartShape { get; set; }

    /// <summary>
    /// cornerBlockEndShape
    /// </summary>
    [Description("@#cornerBlockEndShape")]
    public extern string CornerBlockEndShape { get; set; }

    /// <summary>
    /// cornerInlineStartShape
    /// </summary>
    [Description("@#cornerInlineStartShape")]
    public extern string CornerInlineStartShape { get; set; }

    /// <summary>
    /// cornerInlineEndShape
    /// </summary>
    [Description("@#cornerInlineEndShape")]
    public extern string CornerInlineEndShape { get; set; }

    /// <summary>
    /// borderLimit
    /// </summary>
    [Description("@#borderLimit")]
    public extern string BorderLimit { get; set; }

    /// <summary>
    /// borderClip
    /// </summary>
    [Description("@#borderClip")]
    public extern string BorderClip { get; set; }

    /// <summary>
    /// borderClipTop
    /// </summary>
    [Description("@#borderClipTop")]
    public extern string BorderClipTop { get; set; }

    /// <summary>
    /// borderClipRight
    /// </summary>
    [Description("@#borderClipRight")]
    public extern string BorderClipRight { get; set; }

    /// <summary>
    /// borderClipBottom
    /// </summary>
    [Description("@#borderClipBottom")]
    public extern string BorderClipBottom { get; set; }

    /// <summary>
    /// borderClipLeft
    /// </summary>
    [Description("@#borderClipLeft")]
    public extern string BorderClipLeft { get; set; }

    /// <summary>
    /// boxShadowColor
    /// </summary>
    [Description("@#boxShadowColor")]
    public extern string BoxShadowColor { get; set; }

    /// <summary>
    /// boxShadowOffset
    /// </summary>
    [Description("@#boxShadowOffset")]
    public extern string BoxShadowOffset { get; set; }

    /// <summary>
    /// boxShadowBlur
    /// </summary>
    [Description("@#boxShadowBlur")]
    public extern string BoxShadowBlur { get; set; }

    /// <summary>
    /// boxShadowSpread
    /// </summary>
    [Description("@#boxShadowSpread")]
    public extern string BoxShadowSpread { get; set; }

    /// <summary>
    /// boxShadowPosition
    /// </summary>
    [Description("@#boxShadowPosition")]
    public extern string BoxShadowPosition { get; set; }

    /// <summary>
    /// boxShadow
    /// </summary>
    [Description("@#boxShadow")]
    public extern string BoxShadow { get; set; }

    /// <summary>
    /// borderShape
    /// </summary>
    [Description("@#borderShape")]
    public extern string BorderShape { get; set; }

    /// <summary>
    /// margin
    /// </summary>
    [Description("@#margin")]
    public extern string Margin { get; set; }

    /// <summary>
    /// marginTrim
    /// </summary>
    [Description("@#marginTrim")]
    public extern string MarginTrim { get; set; }

    /// <summary>
    /// paddingTop
    /// </summary>
    [Description("@#paddingTop")]
    public extern string PaddingTop { get; set; }

    /// <summary>
    /// paddingRight
    /// </summary>
    [Description("@#paddingRight")]
    public extern string PaddingRight { get; set; }

    /// <summary>
    /// paddingBottom
    /// </summary>
    [Description("@#paddingBottom")]
    public extern string PaddingBottom { get; set; }

    /// <summary>
    /// paddingLeft
    /// </summary>
    [Description("@#paddingLeft")]
    public extern string PaddingLeft { get; set; }

    /// <summary>
    /// padding
    /// </summary>
    [Description("@#padding")]
    public extern string Padding { get; set; }

    /// <summary>
    /// breakBefore
    /// </summary>
    [Description("@#breakBefore")]
    public extern string BreakBefore { get; set; }

    /// <summary>
    /// breakAfter
    /// </summary>
    [Description("@#breakAfter")]
    public extern string BreakAfter { get; set; }

    /// <summary>
    /// breakInside
    /// </summary>
    [Description("@#breakInside")]
    public extern string BreakInside { get; set; }

    /// <summary>
    /// orphans
    /// </summary>
    [Description("@#orphans")]
    public extern string Orphans { get; set; }

    /// <summary>
    /// widows
    /// </summary>
    [Description("@#widows")]
    public extern string Widows { get; set; }

    /// <summary>
    /// marginBreak
    /// </summary>
    [Description("@#marginBreak")]
    public extern string MarginBreak { get; set; }

    /// <summary>
    /// boxDecorationBreak
    /// </summary>
    [Description("@#boxDecorationBreak")]
    public extern string BoxDecorationBreak { get; set; }

    /// <summary>
    /// colorScheme
    /// </summary>
    [Description("@#colorScheme")]
    public extern string ColorScheme { get; set; }

    /// <summary>
    /// forcedColorAdjust
    /// </summary>
    [Description("@#forcedColorAdjust")]
    public extern string ForcedColorAdjust { get; set; }

    /// <summary>
    /// printColorAdjust
    /// </summary>
    [Description("@#printColorAdjust")]
    public extern string PrintColorAdjust { get; set; }

    /// <summary>
    /// colorAdjust
    /// </summary>
    [Description("@#colorAdjust")]
    public extern string ColorAdjust { get; set; }

    /// <summary>
    /// dynamicRangeLimit
    /// </summary>
    [Description("@#dynamicRangeLimit")]
    public extern string DynamicRangeLimit { get; set; }

    /// <summary>
    /// color
    /// </summary>
    [Description("@#color")]
    public extern string Color { get; set; }

    /// <summary>
    /// opacity
    /// </summary>
    [Description("@#opacity")]
    public extern string Opacity { get; set; }

    /// <summary>
    /// content
    /// </summary>
    [Description("@#content")]
    public extern string Content { get; set; }

    /// <summary>
    /// quotes
    /// </summary>
    [Description("@#quotes")]
    public extern string Quotes { get; set; }

    /// <summary>
    /// stringSet
    /// </summary>
    [Description("@#stringSet")]
    public extern string StringSet { get; set; }

    /// <summary>
    /// bookmarkLevel
    /// </summary>
    [Description("@#bookmarkLevel")]
    public extern string BookmarkLevel { get; set; }

    /// <summary>
    /// bookmarkLabel
    /// </summary>
    [Description("@#bookmarkLabel")]
    public extern string BookmarkLabel { get; set; }

    /// <summary>
    /// bookmarkState
    /// </summary>
    [Description("@#bookmarkState")]
    public extern string BookmarkState { get; set; }

    /// <summary>
    /// order
    /// </summary>
    [Description("@#order")]
    public extern string Order { get; set; }

    /// <summary>
    /// readingFlow
    /// </summary>
    [Description("@#readingFlow")]
    public extern string ReadingFlow { get; set; }

    /// <summary>
    /// readingOrder
    /// </summary>
    [Description("@#readingOrder")]
    public extern string ReadingOrder { get; set; }

    /// <summary>
    /// visibility
    /// </summary>
    [Description("@#visibility")]
    public extern string Visibility { get; set; }

    /// <summary>
    /// wrapFlow
    /// </summary>
    [Description("@#wrapFlow")]
    public extern string WrapFlow { get; set; }

    /// <summary>
    /// wrapThrough
    /// </summary>
    [Description("@#wrapThrough")]
    public extern string WrapThrough { get; set; }

    /// <summary>
    /// flexDirection
    /// </summary>
    [Description("@#flexDirection")]
    public extern string FlexDirection { get; set; }

    /// <summary>
    /// flexWrap
    /// </summary>
    [Description("@#flexWrap")]
    public extern string FlexWrap { get; set; }

    /// <summary>
    /// flexFlow
    /// </summary>
    [Description("@#flexFlow")]
    public extern string FlexFlow { get; set; }

    /// <summary>
    /// flex
    /// </summary>
    [Description("@#flex")]
    public extern string Flex { get; set; }

    /// <summary>
    /// flexGrow
    /// </summary>
    [Description("@#flexGrow")]
    public extern string FlexGrow { get; set; }

    /// <summary>
    /// flexShrink
    /// </summary>
    [Description("@#flexShrink")]
    public extern string FlexShrink { get; set; }

    /// <summary>
    /// flexBasis
    /// </summary>
    [Description("@#flexBasis")]
    public extern string FlexBasis { get; set; }

    /// <summary>
    /// appearance
    /// </summary>
    [Description("@#appearance")]
    public extern string Appearance { get; set; }

    /// <summary>
    /// fieldSizing
    /// </summary>
    [Description("@#fieldSizing")]
    public extern string FieldSizing { get; set; }

    /// <summary>
    /// sliderOrientation
    /// </summary>
    [Description("@#sliderOrientation")]
    public extern string SliderOrientation { get; set; }

    /// <summary>
    /// inputSecurity
    /// </summary>
    [Description("@#inputSecurity")]
    public extern string InputSecurity { get; set; }

    /// <summary>
    /// columnRuleBreak
    /// </summary>
    [Description("@#columnRuleBreak")]
    public extern string ColumnRuleBreak { get; set; }

    /// <summary>
    /// rowRuleBreak
    /// </summary>
    [Description("@#rowRuleBreak")]
    public extern string RowRuleBreak { get; set; }

    /// <summary>
    /// ruleBreak
    /// </summary>
    [Description("@#ruleBreak")]
    public extern string RuleBreak { get; set; }

    /// <summary>
    /// columnRuleOutset
    /// </summary>
    [Description("@#columnRuleOutset")]
    public extern string ColumnRuleOutset { get; set; }

    /// <summary>
    /// rowRuleOutset
    /// </summary>
    [Description("@#rowRuleOutset")]
    public extern string RowRuleOutset { get; set; }

    /// <summary>
    /// ruleOutset
    /// </summary>
    [Description("@#ruleOutset")]
    public extern string RuleOutset { get; set; }

    /// <summary>
    /// rulePaintOrder
    /// </summary>
    [Description("@#rulePaintOrder")]
    public extern string RulePaintOrder { get; set; }

    /// <summary>
    /// columnRuleColor
    /// </summary>
    [Description("@#columnRuleColor")]
    public extern string ColumnRuleColor { get; set; }

    /// <summary>
    /// rowRuleColor
    /// </summary>
    [Description("@#rowRuleColor")]
    public extern string RowRuleColor { get; set; }

    /// <summary>
    /// columnRuleStyle
    /// </summary>
    [Description("@#columnRuleStyle")]
    public extern string ColumnRuleStyle { get; set; }

    /// <summary>
    /// rowRuleStyle
    /// </summary>
    [Description("@#rowRuleStyle")]
    public extern string RowRuleStyle { get; set; }

    /// <summary>
    /// columnRuleWidth
    /// </summary>
    [Description("@#columnRuleWidth")]
    public extern string ColumnRuleWidth { get; set; }

    /// <summary>
    /// rowRuleWidth
    /// </summary>
    [Description("@#rowRuleWidth")]
    public extern string RowRuleWidth { get; set; }

    /// <summary>
    /// columnRule
    /// </summary>
    [Description("@#columnRule")]
    public extern string ColumnRule { get; set; }

    /// <summary>
    /// rowRule
    /// </summary>
    [Description("@#rowRule")]
    public extern string RowRule { get; set; }

    /// <summary>
    /// ruleColor
    /// </summary>
    [Description("@#ruleColor")]
    public extern string RuleColor { get; set; }

    /// <summary>
    /// ruleStyle
    /// </summary>
    [Description("@#ruleStyle")]
    public extern string RuleStyle { get; set; }

    /// <summary>
    /// ruleWidth
    /// </summary>
    [Description("@#ruleWidth")]
    public extern string RuleWidth { get; set; }

    /// <summary>
    /// rule
    /// </summary>
    [Description("@#rule")]
    public extern string Rule { get; set; }

    /// <summary>
    /// copyInto
    /// </summary>
    [Description("@#copyInto")]
    public extern string CopyInto { get; set; }

    /// <summary>
    /// position
    /// </summary>
    [Description("@#position")]
    public extern string Position { get; set; }

    /// <summary>
    /// float
    /// </summary>
    [Description("@#float")]
    public extern string Float { get; set; }

    /// <summary>
    /// footnoteDisplay
    /// </summary>
    [Description("@#footnoteDisplay")]
    public extern string FootnoteDisplay { get; set; }

    /// <summary>
    /// footnotePolicy
    /// </summary>
    [Description("@#footnotePolicy")]
    public extern string FootnotePolicy { get; set; }

    /// <summary>
    /// itemSlack
    /// </summary>
    [Description("@#itemSlack")]
    public extern string ItemSlack { get; set; }

    /// <summary>
    /// itemDirection
    /// </summary>
    [Description("@#itemDirection")]
    public extern string ItemDirection { get; set; }

    /// <summary>
    /// itemTrack
    /// </summary>
    [Description("@#itemTrack")]
    public extern string ItemTrack { get; set; }

    /// <summary>
    /// itemWrap
    /// </summary>
    [Description("@#itemWrap")]
    public extern string ItemWrap { get; set; }

    /// <summary>
    /// itemCross
    /// </summary>
    [Description("@#itemCross")]
    public extern string ItemCross { get; set; }

    /// <summary>
    /// itemPack
    /// </summary>
    [Description("@#itemPack")]
    public extern string ItemPack { get; set; }

    /// <summary>
    /// itemFlow
    /// </summary>
    [Description("@#itemFlow")]
    public extern string ItemFlow { get; set; }

    /// <summary>
    /// gridTemplateColumns
    /// </summary>
    [Description("@#gridTemplateColumns")]
    public extern string GridTemplateColumns { get; set; }

    /// <summary>
    /// gridTemplateRows
    /// </summary>
    [Description("@#gridTemplateRows")]
    public extern string GridTemplateRows { get; set; }

    /// <summary>
    /// gridTemplateAreas
    /// </summary>
    [Description("@#gridTemplateAreas")]
    public extern string GridTemplateAreas { get; set; }

    /// <summary>
    /// gridTemplate
    /// </summary>
    [Description("@#gridTemplate")]
    public extern string GridTemplate { get; set; }

    /// <summary>
    /// gridAutoColumns
    /// </summary>
    [Description("@#gridAutoColumns")]
    public extern string GridAutoColumns { get; set; }

    /// <summary>
    /// gridAutoRows
    /// </summary>
    [Description("@#gridAutoRows")]
    public extern string GridAutoRows { get; set; }

    /// <summary>
    /// gridAutoFlow
    /// </summary>
    [Description("@#gridAutoFlow")]
    public extern string GridAutoFlow { get; set; }

    /// <summary>
    /// grid
    /// </summary>
    [Description("@#grid")]
    public extern string Grid { get; set; }

    /// <summary>
    /// gridRowStart
    /// </summary>
    [Description("@#gridRowStart")]
    public extern string GridRowStart { get; set; }

    /// <summary>
    /// gridColumnStart
    /// </summary>
    [Description("@#gridColumnStart")]
    public extern string GridColumnStart { get; set; }

    /// <summary>
    /// gridRowEnd
    /// </summary>
    [Description("@#gridRowEnd")]
    public extern string GridRowEnd { get; set; }

    /// <summary>
    /// gridColumnEnd
    /// </summary>
    [Description("@#gridColumnEnd")]
    public extern string GridColumnEnd { get; set; }

    /// <summary>
    /// gridRow
    /// </summary>
    [Description("@#gridRow")]
    public extern string GridRow { get; set; }

    /// <summary>
    /// gridColumn
    /// </summary>
    [Description("@#gridColumn")]
    public extern string GridColumn { get; set; }

    /// <summary>
    /// gridArea
    /// </summary>
    [Description("@#gridArea")]
    public extern string GridArea { get; set; }

    /// <summary>
    /// objectViewBox
    /// </summary>
    [Description("@#objectViewBox")]
    public extern string ObjectViewBox { get; set; }

    /// <summary>
    /// objectFit
    /// </summary>
    [Description("@#objectFit")]
    public extern string ObjectFit { get; set; }

    /// <summary>
    /// objectPosition
    /// </summary>
    [Description("@#objectPosition")]
    public extern string ObjectPosition { get; set; }

    /// <summary>
    /// imageOrientation
    /// </summary>
    [Description("@#imageOrientation")]
    public extern string ImageOrientation { get; set; }

    /// <summary>
    /// imageRendering
    /// </summary>
    [Description("@#imageRendering")]
    public extern string ImageRendering { get; set; }

    /// <summary>
    /// dominantBaseline
    /// </summary>
    [Description("@#dominantBaseline")]
    public extern string DominantBaseline { get; set; }

    /// <summary>
    /// verticalAlign
    /// </summary>
    [Description("@#verticalAlign")]
    public extern string VerticalAlign { get; set; }

    /// <summary>
    /// baselineSource
    /// </summary>
    [Description("@#baselineSource")]
    public extern string BaselineSource { get; set; }

    /// <summary>
    /// alignmentBaseline
    /// </summary>
    [Description("@#alignmentBaseline")]
    public extern string AlignmentBaseline { get; set; }

    /// <summary>
    /// baselineShift
    /// </summary>
    [Description("@#baselineShift")]
    public extern string BaselineShift { get; set; }

    /// <summary>
    /// lineHeight
    /// </summary>
    [Description("@#lineHeight")]
    public extern string LineHeight { get; set; }

    /// <summary>
    /// lineFitEdge
    /// </summary>
    [Description("@#lineFitEdge")]
    public extern string LineFitEdge { get; set; }

    /// <summary>
    /// textBox
    /// </summary>
    [Description("@#textBox")]
    public extern string TextBox { get; set; }

    /// <summary>
    /// textBoxTrim
    /// </summary>
    [Description("@#textBoxTrim")]
    public extern string TextBoxTrim { get; set; }

    /// <summary>
    /// textBoxEdge
    /// </summary>
    [Description("@#textBoxEdge")]
    public extern string TextBoxEdge { get; set; }

    /// <summary>
    /// inlineSizing
    /// </summary>
    [Description("@#inlineSizing")]
    public extern string InlineSizing { get; set; }

    /// <summary>
    /// initialLetter
    /// </summary>
    [Description("@#initialLetter")]
    public extern string InitialLetter { get; set; }

    /// <summary>
    /// initialLetterAlign
    /// </summary>
    [Description("@#initialLetterAlign")]
    public extern string InitialLetterAlign { get; set; }

    /// <summary>
    /// initialLetterWrap
    /// </summary>
    [Description("@#initialLetterWrap")]
    public extern string InitialLetterWrap { get; set; }

    /// <summary>
    /// lineGrid
    /// </summary>
    [Description("@#lineGrid")]
    public extern string LineGrid { get; set; }

    /// <summary>
    /// lineSnap
    /// </summary>
    [Description("@#lineSnap")]
    public extern string LineSnap { get; set; }

    /// <summary>
    /// boxSnap
    /// </summary>
    [Description("@#boxSnap")]
    public extern string BoxSnap { get; set; }

    /// <summary>
    /// linkParameters
    /// </summary>
    [Description("@#linkParameters")]
    public extern string LinkParameters { get; set; }

    /// <summary>
    /// listStyleImage
    /// </summary>
    [Description("@#listStyleImage")]
    public extern string ListStyleImage { get; set; }

    /// <summary>
    /// listStyleType
    /// </summary>
    [Description("@#listStyleType")]
    public extern string ListStyleType { get; set; }

    /// <summary>
    /// listStylePosition
    /// </summary>
    [Description("@#listStylePosition")]
    public extern string ListStylePosition { get; set; }

    /// <summary>
    /// listStyle
    /// </summary>
    [Description("@#listStyle")]
    public extern string ListStyle { get; set; }

    /// <summary>
    /// markerSide
    /// </summary>
    [Description("@#markerSide")]
    public extern string MarkerSide { get; set; }

    /// <summary>
    /// counterReset
    /// </summary>
    [Description("@#counterReset")]
    public extern string CounterReset { get; set; }

    /// <summary>
    /// counterIncrement
    /// </summary>
    [Description("@#counterIncrement")]
    public extern string CounterIncrement { get; set; }

    /// <summary>
    /// counterSet
    /// </summary>
    [Description("@#counterSet")]
    public extern string CounterSet { get; set; }

    /// <summary>
    /// blockSize
    /// </summary>
    [Description("@#blockSize")]
    public extern string BlockSize { get; set; }

    /// <summary>
    /// inlineSize
    /// </summary>
    [Description("@#inlineSize")]
    public extern string InlineSize { get; set; }

    /// <summary>
    /// minBlockSize
    /// </summary>
    [Description("@#minBlockSize")]
    public extern string MinBlockSize { get; set; }

    /// <summary>
    /// minInlineSize
    /// </summary>
    [Description("@#minInlineSize")]
    public extern string MinInlineSize { get; set; }

    /// <summary>
    /// maxBlockSize
    /// </summary>
    [Description("@#maxBlockSize")]
    public extern string MaxBlockSize { get; set; }

    /// <summary>
    /// maxInlineSize
    /// </summary>
    [Description("@#maxInlineSize")]
    public extern string MaxInlineSize { get; set; }

    /// <summary>
    /// marginBlockStart
    /// </summary>
    [Description("@#marginBlockStart")]
    public extern string MarginBlockStart { get; set; }

    /// <summary>
    /// marginBlockEnd
    /// </summary>
    [Description("@#marginBlockEnd")]
    public extern string MarginBlockEnd { get; set; }

    /// <summary>
    /// marginInlineStart
    /// </summary>
    [Description("@#marginInlineStart")]
    public extern string MarginInlineStart { get; set; }

    /// <summary>
    /// marginInlineEnd
    /// </summary>
    [Description("@#marginInlineEnd")]
    public extern string MarginInlineEnd { get; set; }

    /// <summary>
    /// marginBlock
    /// </summary>
    [Description("@#marginBlock")]
    public extern string MarginBlock { get; set; }

    /// <summary>
    /// marginInline
    /// </summary>
    [Description("@#marginInline")]
    public extern string MarginInline { get; set; }

    /// <summary>
    /// paddingBlockStart
    /// </summary>
    [Description("@#paddingBlockStart")]
    public extern string PaddingBlockStart { get; set; }

    /// <summary>
    /// paddingBlockEnd
    /// </summary>
    [Description("@#paddingBlockEnd")]
    public extern string PaddingBlockEnd { get; set; }

    /// <summary>
    /// paddingInlineStart
    /// </summary>
    [Description("@#paddingInlineStart")]
    public extern string PaddingInlineStart { get; set; }

    /// <summary>
    /// paddingInlineEnd
    /// </summary>
    [Description("@#paddingInlineEnd")]
    public extern string PaddingInlineEnd { get; set; }

    /// <summary>
    /// paddingBlock
    /// </summary>
    [Description("@#paddingBlock")]
    public extern string PaddingBlock { get; set; }

    /// <summary>
    /// paddingInline
    /// </summary>
    [Description("@#paddingInline")]
    public extern string PaddingInline { get; set; }

    /// <summary>
    /// columnWidth
    /// </summary>
    [Description("@#columnWidth")]
    public extern string ColumnWidth { get; set; }

    /// <summary>
    /// columnHeight
    /// </summary>
    [Description("@#columnHeight")]
    public extern string ColumnHeight { get; set; }

    /// <summary>
    /// columnCount
    /// </summary>
    [Description("@#columnCount")]
    public extern string ColumnCount { get; set; }

    /// <summary>
    /// columnWrap
    /// </summary>
    [Description("@#columnWrap")]
    public extern string ColumnWrap { get; set; }

    /// <summary>
    /// columns
    /// </summary>
    [Description("@#columns")]
    public extern string Columns { get; set; }

    /// <summary>
    /// columnSpan
    /// </summary>
    [Description("@#columnSpan")]
    public extern string ColumnSpan { get; set; }

    /// <summary>
    /// columnFill
    /// </summary>
    [Description("@#columnFill")]
    public extern string ColumnFill { get; set; }

    /// <summary>
    /// overflowClipMarginTop
    /// </summary>
    [Description("@#overflowClipMarginTop")]
    public extern string OverflowClipMarginTop { get; set; }

    /// <summary>
    /// overflowClipMarginRight
    /// </summary>
    [Description("@#overflowClipMarginRight")]
    public extern string OverflowClipMarginRight { get; set; }

    /// <summary>
    /// overflowClipMarginBottom
    /// </summary>
    [Description("@#overflowClipMarginBottom")]
    public extern string OverflowClipMarginBottom { get; set; }

    /// <summary>
    /// overflowClipMarginLeft
    /// </summary>
    [Description("@#overflowClipMarginLeft")]
    public extern string OverflowClipMarginLeft { get; set; }

    /// <summary>
    /// overflowClipMarginBlockStart
    /// </summary>
    [Description("@#overflowClipMarginBlockStart")]
    public extern string OverflowClipMarginBlockStart { get; set; }

    /// <summary>
    /// overflowClipMarginInlineStart
    /// </summary>
    [Description("@#overflowClipMarginInlineStart")]
    public extern string OverflowClipMarginInlineStart { get; set; }

    /// <summary>
    /// overflowClipMarginBlockEnd
    /// </summary>
    [Description("@#overflowClipMarginBlockEnd")]
    public extern string OverflowClipMarginBlockEnd { get; set; }

    /// <summary>
    /// overflowClipMarginInlineEnd
    /// </summary>
    [Description("@#overflowClipMarginInlineEnd")]
    public extern string OverflowClipMarginInlineEnd { get; set; }

    /// <summary>
    /// overflowClipMargin
    /// </summary>
    [Description("@#overflowClipMargin")]
    public extern string OverflowClipMargin { get; set; }

    /// <summary>
    /// overflowClipMarginInline
    /// </summary>
    [Description("@#overflowClipMarginInline")]
    public extern string OverflowClipMarginInline { get; set; }

    /// <summary>
    /// overflowClipMarginBlock
    /// </summary>
    [Description("@#overflowClipMarginBlock")]
    public extern string OverflowClipMarginBlock { get; set; }

    /// <summary>
    /// textOverflow
    /// </summary>
    [Description("@#textOverflow")]
    public extern string TextOverflow { get; set; }

    /// <summary>
    /// blockEllipsis
    /// </summary>
    [Description("@#blockEllipsis")]
    public extern string BlockEllipsis { get; set; }

    /// <summary>
    /// lineClamp
    /// </summary>
    [Description("@#lineClamp")]
    public extern string LineClamp { get; set; }

    /// <summary>
    /// webkitLineClamp
    /// </summary>
    [Description("@#webkitLineClamp")]
    public extern string WebkitLineClamp { get; set; }

    /// <summary>
    /// continue
    /// </summary>
    [Description("@#continue")]
    public extern string Continue { get; set; }

    /// <summary>
    /// maxLines
    /// </summary>
    [Description("@#maxLines")]
    public extern string MaxLines { get; set; }

    /// <summary>
    /// scrollTargetGroup
    /// </summary>
    [Description("@#scrollTargetGroup")]
    public extern string ScrollTargetGroup { get; set; }

    /// <summary>
    /// scrollMarkerGroup
    /// </summary>
    [Description("@#scrollMarkerGroup")]
    public extern string ScrollMarkerGroup { get; set; }

    /// <summary>
    /// overflowX
    /// </summary>
    [Description("@#overflowX")]
    public extern string OverflowX { get; set; }

    /// <summary>
    /// overflowY
    /// </summary>
    [Description("@#overflowY")]
    public extern string OverflowY { get; set; }

    /// <summary>
    /// overflowBlock
    /// </summary>
    [Description("@#overflowBlock")]
    public extern string OverflowBlock { get; set; }

    /// <summary>
    /// overflowInline
    /// </summary>
    [Description("@#overflowInline")]
    public extern string OverflowInline { get; set; }

    /// <summary>
    /// overflow
    /// </summary>
    [Description("@#overflow")]
    public extern string Overflow { get; set; }

    /// <summary>
    /// scrollBehavior
    /// </summary>
    [Description("@#scrollBehavior")]
    public extern string ScrollBehavior { get; set; }

    /// <summary>
    /// scrollbarGutter
    /// </summary>
    [Description("@#scrollbarGutter")]
    public extern string ScrollbarGutter { get; set; }

    /// <summary>
    /// overscrollBehavior
    /// </summary>
    [Description("@#overscrollBehavior")]
    public extern string OverscrollBehavior { get; set; }

    /// <summary>
    /// overscrollBehaviorX
    /// </summary>
    [Description("@#overscrollBehaviorX")]
    public extern string OverscrollBehaviorX { get; set; }

    /// <summary>
    /// overscrollBehaviorY
    /// </summary>
    [Description("@#overscrollBehaviorY")]
    public extern string OverscrollBehaviorY { get; set; }

    /// <summary>
    /// overscrollBehaviorInline
    /// </summary>
    [Description("@#overscrollBehaviorInline")]
    public extern string OverscrollBehaviorInline { get; set; }

    /// <summary>
    /// overscrollBehaviorBlock
    /// </summary>
    [Description("@#overscrollBehaviorBlock")]
    public extern string OverscrollBehaviorBlock { get; set; }

    /// <summary>
    /// floatReference
    /// </summary>
    [Description("@#floatReference")]
    public extern string FloatReference { get; set; }

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern string Clear { get; set; }

    /// <summary>
    /// floatDefer
    /// </summary>
    [Description("@#floatDefer")]
    public extern string FloatDefer { get; set; }

    /// <summary>
    /// floatOffset
    /// </summary>
    [Description("@#floatOffset")]
    public extern string FloatOffset { get; set; }

    /// <summary>
    /// page
    /// </summary>
    [Description("@#page")]
    public extern string Page { get; set; }

    /// <summary>
    /// overlay
    /// </summary>
    [Description("@#overlay")]
    public extern string Overlay { get; set; }

    /// <summary>
    /// insetBlockStart
    /// </summary>
    [Description("@#insetBlockStart")]
    public extern string InsetBlockStart { get; set; }

    /// <summary>
    /// insetInlineStart
    /// </summary>
    [Description("@#insetInlineStart")]
    public extern string InsetInlineStart { get; set; }

    /// <summary>
    /// insetBlockEnd
    /// </summary>
    [Description("@#insetBlockEnd")]
    public extern string InsetBlockEnd { get; set; }

    /// <summary>
    /// insetInlineEnd
    /// </summary>
    [Description("@#insetInlineEnd")]
    public extern string InsetInlineEnd { get; set; }

    /// <summary>
    /// insetBlock
    /// </summary>
    [Description("@#insetBlock")]
    public extern string InsetBlock { get; set; }

    /// <summary>
    /// insetInline
    /// </summary>
    [Description("@#insetInline")]
    public extern string InsetInline { get; set; }

    /// <summary>
    /// inset
    /// </summary>
    [Description("@#inset")]
    public extern string Inset { get; set; }

    /// <summary>
    /// blockStepSize
    /// </summary>
    [Description("@#blockStepSize")]
    public extern string BlockStepSize { get; set; }

    /// <summary>
    /// blockStepInsert
    /// </summary>
    [Description("@#blockStepInsert")]
    public extern string BlockStepInsert { get; set; }

    /// <summary>
    /// blockStepAlign
    /// </summary>
    [Description("@#blockStepAlign")]
    public extern string BlockStepAlign { get; set; }

    /// <summary>
    /// blockStepRound
    /// </summary>
    [Description("@#blockStepRound")]
    public extern string BlockStepRound { get; set; }

    /// <summary>
    /// blockStep
    /// </summary>
    [Description("@#blockStep")]
    public extern string BlockStep { get; set; }

    /// <summary>
    /// lineHeightStep
    /// </summary>
    [Description("@#lineHeightStep")]
    public extern string LineHeightStep { get; set; }

    /// <summary>
    /// borderBoundary
    /// </summary>
    [Description("@#borderBoundary")]
    public extern string BorderBoundary { get; set; }

    /// <summary>
    /// rubyPosition
    /// </summary>
    [Description("@#rubyPosition")]
    public extern string RubyPosition { get; set; }

    /// <summary>
    /// rubyMerge
    /// </summary>
    [Description("@#rubyMerge")]
    public extern string RubyMerge { get; set; }

    /// <summary>
    /// rubyAlign
    /// </summary>
    [Description("@#rubyAlign")]
    public extern string RubyAlign { get; set; }

    /// <summary>
    /// rubyOverhang
    /// </summary>
    [Description("@#rubyOverhang")]
    public extern string RubyOverhang { get; set; }

    /// <summary>
    /// overflowAnchor
    /// </summary>
    [Description("@#overflowAnchor")]
    public extern string OverflowAnchor { get; set; }

    /// <summary>
    /// scrollSnapType
    /// </summary>
    [Description("@#scrollSnapType")]
    public extern string ScrollSnapType { get; set; }

    /// <summary>
    /// scrollPadding
    /// </summary>
    [Description("@#scrollPadding")]
    public extern string ScrollPadding { get; set; }

    /// <summary>
    /// scrollMargin
    /// </summary>
    [Description("@#scrollMargin")]
    public extern string ScrollMargin { get; set; }

    /// <summary>
    /// scrollSnapAlign
    /// </summary>
    [Description("@#scrollSnapAlign")]
    public extern string ScrollSnapAlign { get; set; }

    /// <summary>
    /// scrollSnapStop
    /// </summary>
    [Description("@#scrollSnapStop")]
    public extern string ScrollSnapStop { get; set; }

    /// <summary>
    /// scrollPaddingTop
    /// </summary>
    [Description("@#scrollPaddingTop")]
    public extern string ScrollPaddingTop { get; set; }

    /// <summary>
    /// scrollPaddingRight
    /// </summary>
    [Description("@#scrollPaddingRight")]
    public extern string ScrollPaddingRight { get; set; }

    /// <summary>
    /// scrollPaddingBottom
    /// </summary>
    [Description("@#scrollPaddingBottom")]
    public extern string ScrollPaddingBottom { get; set; }

    /// <summary>
    /// scrollPaddingLeft
    /// </summary>
    [Description("@#scrollPaddingLeft")]
    public extern string ScrollPaddingLeft { get; set; }

    /// <summary>
    /// scrollPaddingInlineStart
    /// </summary>
    [Description("@#scrollPaddingInlineStart")]
    public extern string ScrollPaddingInlineStart { get; set; }

    /// <summary>
    /// scrollPaddingBlockStart
    /// </summary>
    [Description("@#scrollPaddingBlockStart")]
    public extern string ScrollPaddingBlockStart { get; set; }

    /// <summary>
    /// scrollPaddingInlineEnd
    /// </summary>
    [Description("@#scrollPaddingInlineEnd")]
    public extern string ScrollPaddingInlineEnd { get; set; }

    /// <summary>
    /// scrollPaddingBlockEnd
    /// </summary>
    [Description("@#scrollPaddingBlockEnd")]
    public extern string ScrollPaddingBlockEnd { get; set; }

    /// <summary>
    /// scrollPaddingBlock
    /// </summary>
    [Description("@#scrollPaddingBlock")]
    public extern string ScrollPaddingBlock { get; set; }

    /// <summary>
    /// scrollPaddingInline
    /// </summary>
    [Description("@#scrollPaddingInline")]
    public extern string ScrollPaddingInline { get; set; }

    /// <summary>
    /// scrollMarginTop
    /// </summary>
    [Description("@#scrollMarginTop")]
    public extern string ScrollMarginTop { get; set; }

    /// <summary>
    /// scrollMarginRight
    /// </summary>
    [Description("@#scrollMarginRight")]
    public extern string ScrollMarginRight { get; set; }

    /// <summary>
    /// scrollMarginBottom
    /// </summary>
    [Description("@#scrollMarginBottom")]
    public extern string ScrollMarginBottom { get; set; }

    /// <summary>
    /// scrollMarginLeft
    /// </summary>
    [Description("@#scrollMarginLeft")]
    public extern string ScrollMarginLeft { get; set; }

    /// <summary>
    /// scrollMarginBlockStart
    /// </summary>
    [Description("@#scrollMarginBlockStart")]
    public extern string ScrollMarginBlockStart { get; set; }

    /// <summary>
    /// scrollMarginInlineStart
    /// </summary>
    [Description("@#scrollMarginInlineStart")]
    public extern string ScrollMarginInlineStart { get; set; }

    /// <summary>
    /// scrollMarginBlockEnd
    /// </summary>
    [Description("@#scrollMarginBlockEnd")]
    public extern string ScrollMarginBlockEnd { get; set; }

    /// <summary>
    /// scrollMarginInlineEnd
    /// </summary>
    [Description("@#scrollMarginInlineEnd")]
    public extern string ScrollMarginInlineEnd { get; set; }

    /// <summary>
    /// scrollMarginBlock
    /// </summary>
    [Description("@#scrollMarginBlock")]
    public extern string ScrollMarginBlock { get; set; }

    /// <summary>
    /// scrollMarginInline
    /// </summary>
    [Description("@#scrollMarginInline")]
    public extern string ScrollMarginInline { get; set; }

    /// <summary>
    /// scrollbarColor
    /// </summary>
    [Description("@#scrollbarColor")]
    public extern string ScrollbarColor { get; set; }

    /// <summary>
    /// scrollbarWidth
    /// </summary>
    [Description("@#scrollbarWidth")]
    public extern string ScrollbarWidth { get; set; }

    /// <summary>
    /// shapeInside
    /// </summary>
    [Description("@#shapeInside")]
    public extern string ShapeInside { get; set; }

    /// <summary>
    /// shapePadding
    /// </summary>
    [Description("@#shapePadding")]
    public extern string ShapePadding { get; set; }

    /// <summary>
    /// shapeOutside
    /// </summary>
    [Description("@#shapeOutside")]
    public extern string ShapeOutside { get; set; }

    /// <summary>
    /// shapeImageThreshold
    /// </summary>
    [Description("@#shapeImageThreshold")]
    public extern string ShapeImageThreshold { get; set; }

    /// <summary>
    /// shapeMargin
    /// </summary>
    [Description("@#shapeMargin")]
    public extern string ShapeMargin { get; set; }

    /// <summary>
    /// textSizeAdjust
    /// </summary>
    [Description("@#textSizeAdjust")]
    public extern string TextSizeAdjust { get; set; }

    /// <summary>
    /// aspectRatio
    /// </summary>
    [Description("@#aspectRatio")]
    public extern string AspectRatio { get; set; }

    /// <summary>
    /// containIntrinsicWidth
    /// </summary>
    [Description("@#containIntrinsicWidth")]
    public extern string ContainIntrinsicWidth { get; set; }

    /// <summary>
    /// containIntrinsicHeight
    /// </summary>
    [Description("@#containIntrinsicHeight")]
    public extern string ContainIntrinsicHeight { get; set; }

    /// <summary>
    /// containIntrinsicBlockSize
    /// </summary>
    [Description("@#containIntrinsicBlockSize")]
    public extern string ContainIntrinsicBlockSize { get; set; }

    /// <summary>
    /// containIntrinsicInlineSize
    /// </summary>
    [Description("@#containIntrinsicInlineSize")]
    public extern string ContainIntrinsicInlineSize { get; set; }

    /// <summary>
    /// containIntrinsicSize
    /// </summary>
    [Description("@#containIntrinsicSize")]
    public extern string ContainIntrinsicSize { get; set; }

    /// <summary>
    /// minIntrinsicSizing
    /// </summary>
    [Description("@#minIntrinsicSizing")]
    public extern string MinIntrinsicSizing { get; set; }

    /// <summary>
    /// boxSizing
    /// </summary>
    [Description("@#boxSizing")]
    public extern string BoxSizing { get; set; }

    /// <summary>
    /// voiceVolume
    /// </summary>
    [Description("@#voiceVolume")]
    public extern string VoiceVolume { get; set; }

    /// <summary>
    /// voiceBalance
    /// </summary>
    [Description("@#voiceBalance")]
    public extern string VoiceBalance { get; set; }

    /// <summary>
    /// speak
    /// </summary>
    [Description("@#speak")]
    public extern string Speak { get; set; }

    /// <summary>
    /// speakAs
    /// </summary>
    [Description("@#speakAs")]
    public extern string SpeakAs { get; set; }

    /// <summary>
    /// pauseBefore
    /// </summary>
    [Description("@#pauseBefore")]
    public extern string PauseBefore { get; set; }

    /// <summary>
    /// pauseAfter
    /// </summary>
    [Description("@#pauseAfter")]
    public extern string PauseAfter { get; set; }

    /// <summary>
    /// pause
    /// </summary>
    [Description("@#pause")]
    public extern string Pause { get; set; }

    /// <summary>
    /// restBefore
    /// </summary>
    [Description("@#restBefore")]
    public extern string RestBefore { get; set; }

    /// <summary>
    /// restAfter
    /// </summary>
    [Description("@#restAfter")]
    public extern string RestAfter { get; set; }

    /// <summary>
    /// rest
    /// </summary>
    [Description("@#rest")]
    public extern string Rest { get; set; }

    /// <summary>
    /// cueBefore
    /// </summary>
    [Description("@#cueBefore")]
    public extern string CueBefore { get; set; }

    /// <summary>
    /// cueAfter
    /// </summary>
    [Description("@#cueAfter")]
    public extern string CueAfter { get; set; }

    /// <summary>
    /// cue
    /// </summary>
    [Description("@#cue")]
    public extern string Cue { get; set; }

    /// <summary>
    /// voiceFamily
    /// </summary>
    [Description("@#voiceFamily")]
    public extern string VoiceFamily { get; set; }

    /// <summary>
    /// voiceRate
    /// </summary>
    [Description("@#voiceRate")]
    public extern string VoiceRate { get; set; }

    /// <summary>
    /// voicePitch
    /// </summary>
    [Description("@#voicePitch")]
    public extern string VoicePitch { get; set; }

    /// <summary>
    /// voiceRange
    /// </summary>
    [Description("@#voiceRange")]
    public extern string VoiceRange { get; set; }

    /// <summary>
    /// voiceStress
    /// </summary>
    [Description("@#voiceStress")]
    public extern string VoiceStress { get; set; }

    /// <summary>
    /// voiceDuration
    /// </summary>
    [Description("@#voiceDuration")]
    public extern string VoiceDuration { get; set; }

    /// <summary>
    /// tableLayout
    /// </summary>
    [Description("@#tableLayout")]
    public extern string TableLayout { get; set; }

    /// <summary>
    /// borderCollapse
    /// </summary>
    [Description("@#borderCollapse")]
    public extern string BorderCollapse { get; set; }

    /// <summary>
    /// borderSpacing
    /// </summary>
    [Description("@#borderSpacing")]
    public extern string BorderSpacing { get; set; }

    /// <summary>
    /// captionSide
    /// </summary>
    [Description("@#captionSide")]
    public extern string CaptionSide { get; set; }

    /// <summary>
    /// emptyCells
    /// </summary>
    [Description("@#emptyCells")]
    public extern string EmptyCells { get; set; }

    /// <summary>
    /// textTransform
    /// </summary>
    [Description("@#textTransform")]
    public extern string TextTransform { get; set; }

    /// <summary>
    /// wordSpaceTransform
    /// </summary>
    [Description("@#wordSpaceTransform")]
    public extern string WordSpaceTransform { get; set; }

    /// <summary>
    /// whiteSpace
    /// </summary>
    [Description("@#whiteSpace")]
    public extern string WhiteSpace { get; set; }

    /// <summary>
    /// whiteSpaceCollapse
    /// </summary>
    [Description("@#whiteSpaceCollapse")]
    public extern string WhiteSpaceCollapse { get; set; }

    /// <summary>
    /// whiteSpaceTrim
    /// </summary>
    [Description("@#whiteSpaceTrim")]
    public extern string WhiteSpaceTrim { get; set; }

    /// <summary>
    /// tabSize
    /// </summary>
    [Description("@#tabSize")]
    public extern string TabSize { get; set; }

    /// <summary>
    /// textWrapMode
    /// </summary>
    [Description("@#textWrapMode")]
    public extern string TextWrapMode { get; set; }

    /// <summary>
    /// wrapInside
    /// </summary>
    [Description("@#wrapInside")]
    public extern string WrapInside { get; set; }

    /// <summary>
    /// wrapBefore
    /// </summary>
    [Description("@#wrapBefore")]
    public extern string WrapBefore { get; set; }

    /// <summary>
    /// wrapAfter
    /// </summary>
    [Description("@#wrapAfter")]
    public extern string WrapAfter { get; set; }

    /// <summary>
    /// textWrapStyle
    /// </summary>
    [Description("@#textWrapStyle")]
    public extern string TextWrapStyle { get; set; }

    /// <summary>
    /// textWrap
    /// </summary>
    [Description("@#textWrap")]
    public extern string TextWrap { get; set; }

    /// <summary>
    /// wordBreak
    /// </summary>
    [Description("@#wordBreak")]
    public extern string WordBreak { get; set; }

    /// <summary>
    /// lineBreak
    /// </summary>
    [Description("@#lineBreak")]
    public extern string LineBreak { get; set; }

    /// <summary>
    /// hyphens
    /// </summary>
    [Description("@#hyphens")]
    public extern string Hyphens { get; set; }

    /// <summary>
    /// hyphenateCharacter
    /// </summary>
    [Description("@#hyphenateCharacter")]
    public extern string HyphenateCharacter { get; set; }

    /// <summary>
    /// hyphenateLimitZone
    /// </summary>
    [Description("@#hyphenateLimitZone")]
    public extern string HyphenateLimitZone { get; set; }

    /// <summary>
    /// hyphenateLimitChars
    /// </summary>
    [Description("@#hyphenateLimitChars")]
    public extern string HyphenateLimitChars { get; set; }

    /// <summary>
    /// hyphenateLimitLines
    /// </summary>
    [Description("@#hyphenateLimitLines")]
    public extern string HyphenateLimitLines { get; set; }

    /// <summary>
    /// hyphenateLimitLast
    /// </summary>
    [Description("@#hyphenateLimitLast")]
    public extern string HyphenateLimitLast { get; set; }

    /// <summary>
    /// overflowWrap
    /// </summary>
    [Description("@#overflowWrap")]
    public extern string OverflowWrap { get; set; }

    /// <summary>
    /// wordWrap
    /// </summary>
    [Description("@#wordWrap")]
    public extern string WordWrap { get; set; }

    /// <summary>
    /// textAlign
    /// </summary>
    [Description("@#textAlign")]
    public extern string TextAlign { get; set; }

    /// <summary>
    /// textAlignAll
    /// </summary>
    [Description("@#textAlignAll")]
    public extern string TextAlignAll { get; set; }

    /// <summary>
    /// textAlignLast
    /// </summary>
    [Description("@#textAlignLast")]
    public extern string TextAlignLast { get; set; }

    /// <summary>
    /// textJustify
    /// </summary>
    [Description("@#textJustify")]
    public extern string TextJustify { get; set; }

    /// <summary>
    /// textGroupAlign
    /// </summary>
    [Description("@#textGroupAlign")]
    public extern string TextGroupAlign { get; set; }

    /// <summary>
    /// wordSpacing
    /// </summary>
    [Description("@#wordSpacing")]
    public extern string WordSpacing { get; set; }

    /// <summary>
    /// letterSpacing
    /// </summary>
    [Description("@#letterSpacing")]
    public extern string LetterSpacing { get; set; }

    /// <summary>
    /// linePadding
    /// </summary>
    [Description("@#linePadding")]
    public extern string LinePadding { get; set; }

    /// <summary>
    /// textAutospace
    /// </summary>
    [Description("@#textAutospace")]
    public extern string TextAutospace { get; set; }

    /// <summary>
    /// textSpacingTrim
    /// </summary>
    [Description("@#textSpacingTrim")]
    public extern string TextSpacingTrim { get; set; }

    /// <summary>
    /// textSpacing
    /// </summary>
    [Description("@#textSpacing")]
    public extern string TextSpacing { get; set; }

    /// <summary>
    /// textIndent
    /// </summary>
    [Description("@#textIndent")]
    public extern string TextIndent { get; set; }

    /// <summary>
    /// hangingPunctuation
    /// </summary>
    [Description("@#hangingPunctuation")]
    public extern string HangingPunctuation { get; set; }

    /// <summary>
    /// textDecorationLine
    /// </summary>
    [Description("@#textDecorationLine")]
    public extern string TextDecorationLine { get; set; }

    /// <summary>
    /// textDecorationStyle
    /// </summary>
    [Description("@#textDecorationStyle")]
    public extern string TextDecorationStyle { get; set; }

    /// <summary>
    /// textDecorationColor
    /// </summary>
    [Description("@#textDecorationColor")]
    public extern string TextDecorationColor { get; set; }

    /// <summary>
    /// textDecorationThickness
    /// </summary>
    [Description("@#textDecorationThickness")]
    public extern string TextDecorationThickness { get; set; }

    /// <summary>
    /// textDecoration
    /// </summary>
    [Description("@#textDecoration")]
    public extern string TextDecoration { get; set; }

    /// <summary>
    /// textUnderlinePosition
    /// </summary>
    [Description("@#textUnderlinePosition")]
    public extern string TextUnderlinePosition { get; set; }

    /// <summary>
    /// textUnderlineOffset
    /// </summary>
    [Description("@#textUnderlineOffset")]
    public extern string TextUnderlineOffset { get; set; }

    /// <summary>
    /// textDecorationTrim
    /// </summary>
    [Description("@#textDecorationTrim")]
    public extern string TextDecorationTrim { get; set; }

    /// <summary>
    /// textDecorationSkip
    /// </summary>
    [Description("@#textDecorationSkip")]
    public extern string TextDecorationSkip { get; set; }

    /// <summary>
    /// textDecorationSkipSelf
    /// </summary>
    [Description("@#textDecorationSkipSelf")]
    public extern string TextDecorationSkipSelf { get; set; }

    /// <summary>
    /// textDecorationSkipBox
    /// </summary>
    [Description("@#textDecorationSkipBox")]
    public extern string TextDecorationSkipBox { get; set; }

    /// <summary>
    /// textDecorationSkipSpaces
    /// </summary>
    [Description("@#textDecorationSkipSpaces")]
    public extern string TextDecorationSkipSpaces { get; set; }

    /// <summary>
    /// textDecorationSkipInk
    /// </summary>
    [Description("@#textDecorationSkipInk")]
    public extern string TextDecorationSkipInk { get; set; }

    /// <summary>
    /// textEmphasisStyle
    /// </summary>
    [Description("@#textEmphasisStyle")]
    public extern string TextEmphasisStyle { get; set; }

    /// <summary>
    /// textEmphasisColor
    /// </summary>
    [Description("@#textEmphasisColor")]
    public extern string TextEmphasisColor { get; set; }

    /// <summary>
    /// textEmphasis
    /// </summary>
    [Description("@#textEmphasis")]
    public extern string TextEmphasis { get; set; }

    /// <summary>
    /// textEmphasisPosition
    /// </summary>
    [Description("@#textEmphasisPosition")]
    public extern string TextEmphasisPosition { get; set; }

    /// <summary>
    /// textEmphasisSkip
    /// </summary>
    [Description("@#textEmphasisSkip")]
    public extern string TextEmphasisSkip { get; set; }

    /// <summary>
    /// textShadow
    /// </summary>
    [Description("@#textShadow")]
    public extern string TextShadow { get; set; }

    /// <summary>
    /// translate
    /// </summary>
    [Description("@#translate")]
    public extern string Translate { get; set; }

    /// <summary>
    /// rotate
    /// </summary>
    [Description("@#rotate")]
    public extern string Rotate { get; set; }

    /// <summary>
    /// scale
    /// </summary>
    [Description("@#scale")]
    public extern string Scale { get; set; }

    /// <summary>
    /// transformStyle
    /// </summary>
    [Description("@#transformStyle")]
    public extern string TransformStyle { get; set; }

    /// <summary>
    /// perspective
    /// </summary>
    [Description("@#perspective")]
    public extern string Perspective { get; set; }

    /// <summary>
    /// perspectiveOrigin
    /// </summary>
    [Description("@#perspectiveOrigin")]
    public extern string PerspectiveOrigin { get; set; }

    /// <summary>
    /// backfaceVisibility
    /// </summary>
    [Description("@#backfaceVisibility")]
    public extern string BackfaceVisibility { get; set; }

    /// <summary>
    /// transform
    /// </summary>
    [Description("@#transform")]
    public extern string Transform { get; set; }

    /// <summary>
    /// transformOrigin
    /// </summary>
    [Description("@#transformOrigin")]
    public extern string TransformOrigin { get; set; }

    /// <summary>
    /// transformBox
    /// </summary>
    [Description("@#transformBox")]
    public extern string TransformBox { get; set; }

    /// <summary>
    /// outline
    /// </summary>
    [Description("@#outline")]
    public extern string Outline { get; set; }

    /// <summary>
    /// outlineWidth
    /// </summary>
    [Description("@#outlineWidth")]
    public extern string OutlineWidth { get; set; }

    /// <summary>
    /// outlineStyle
    /// </summary>
    [Description("@#outlineStyle")]
    public extern string OutlineStyle { get; set; }

    /// <summary>
    /// outlineColor
    /// </summary>
    [Description("@#outlineColor")]
    public extern string OutlineColor { get; set; }

    /// <summary>
    /// outlineOffset
    /// </summary>
    [Description("@#outlineOffset")]
    public extern string OutlineOffset { get; set; }

    /// <summary>
    /// resize
    /// </summary>
    [Description("@#resize")]
    public extern string Resize { get; set; }

    /// <summary>
    /// cursor
    /// </summary>
    [Description("@#cursor")]
    public extern string Cursor { get; set; }

    /// <summary>
    /// caretColor
    /// </summary>
    [Description("@#caretColor")]
    public extern string CaretColor { get; set; }

    /// <summary>
    /// caretAnimation
    /// </summary>
    [Description("@#caretAnimation")]
    public extern string CaretAnimation { get; set; }

    /// <summary>
    /// caretShape
    /// </summary>
    [Description("@#caretShape")]
    public extern string CaretShape { get; set; }

    /// <summary>
    /// caret
    /// </summary>
    [Description("@#caret")]
    public extern string Caret { get; set; }

    /// <summary>
    /// navUp
    /// </summary>
    [Description("@#navUp")]
    public extern string NavUp { get; set; }

    /// <summary>
    /// navRight
    /// </summary>
    [Description("@#navRight")]
    public extern string NavRight { get; set; }

    /// <summary>
    /// navDown
    /// </summary>
    [Description("@#navDown")]
    public extern string NavDown { get; set; }

    /// <summary>
    /// navLeft
    /// </summary>
    [Description("@#navLeft")]
    public extern string NavLeft { get; set; }

    /// <summary>
    /// userSelect
    /// </summary>
    [Description("@#userSelect")]
    public extern string UserSelect { get; set; }

    /// <summary>
    /// interactivity
    /// </summary>
    [Description("@#interactivity")]
    public extern string Interactivity { get; set; }

    /// <summary>
    /// accentColor
    /// </summary>
    [Description("@#accentColor")]
    public extern string AccentColor { get; set; }

    /// <summary>
    /// webkitAppearance
    /// </summary>
    [Description("@#webkitAppearance")]
    public extern string WebkitAppearance { get; set; }

    /// <summary>
    /// webkitUserSelect
    /// </summary>
    [Description("@#webkitUserSelect")]
    public extern string WebkitUserSelect { get; set; }

    /// <summary>
    /// interpolateSize
    /// </summary>
    [Description("@#interpolateSize")]
    public extern string InterpolateSize { get; set; }

    /// <summary>
    /// willChange
    /// </summary>
    [Description("@#willChange")]
    public extern string WillChange { get; set; }

    /// <summary>
    /// direction
    /// </summary>
    [Description("@#direction")]
    public extern string Direction { get; set; }

    /// <summary>
    /// unicodeBidi
    /// </summary>
    [Description("@#unicodeBidi")]
    public extern string UnicodeBidi { get; set; }

    /// <summary>
    /// writingMode
    /// </summary>
    [Description("@#writingMode")]
    public extern string WritingMode { get; set; }

    /// <summary>
    /// textOrientation
    /// </summary>
    [Description("@#textOrientation")]
    public extern string TextOrientation { get; set; }

    /// <summary>
    /// glyphOrientationVertical
    /// </summary>
    [Description("@#glyphOrientationVertical")]
    public extern string GlyphOrientationVertical { get; set; }

    /// <summary>
    /// textCombineUpright
    /// </summary>
    [Description("@#textCombineUpright")]
    public extern string TextCombineUpright { get; set; }

    /// <summary>
    /// zIndex
    /// </summary>
    [Description("@#zIndex")]
    public extern string ZIndex { get; set; }

    /// <summary>
    /// pageBreakBefore
    /// </summary>
    [Description("@#pageBreakBefore")]
    public extern string PageBreakBefore { get; set; }

    /// <summary>
    /// pageBreakAfter
    /// </summary>
    [Description("@#pageBreakAfter")]
    public extern string PageBreakAfter { get; set; }

    /// <summary>
    /// pageBreakInside
    /// </summary>
    [Description("@#pageBreakInside")]
    public extern string PageBreakInside { get; set; }

    /// <summary>
    /// fillRule
    /// </summary>
    [Description("@#fillRule")]
    public extern string FillRule { get; set; }

    /// <summary>
    /// fillBreak
    /// </summary>
    [Description("@#fillBreak")]
    public extern string FillBreak { get; set; }

    /// <summary>
    /// fillColor
    /// </summary>
    [Description("@#fillColor")]
    public extern string FillColor { get; set; }

    /// <summary>
    /// fillImage
    /// </summary>
    [Description("@#fillImage")]
    public extern string FillImage { get; set; }

    /// <summary>
    /// fillOrigin
    /// </summary>
    [Description("@#fillOrigin")]
    public extern string FillOrigin { get; set; }

    /// <summary>
    /// fillPosition
    /// </summary>
    [Description("@#fillPosition")]
    public extern string FillPosition { get; set; }

    /// <summary>
    /// fillSize
    /// </summary>
    [Description("@#fillSize")]
    public extern string FillSize { get; set; }

    /// <summary>
    /// fillRepeat
    /// </summary>
    [Description("@#fillRepeat")]
    public extern string FillRepeat { get; set; }

    /// <summary>
    /// fillOpacity
    /// </summary>
    [Description("@#fillOpacity")]
    public extern string FillOpacity { get; set; }

    /// <summary>
    /// strokeWidth
    /// </summary>
    [Description("@#strokeWidth")]
    public extern string StrokeWidth { get; set; }

    /// <summary>
    /// strokeAlign
    /// </summary>
    [Description("@#strokeAlign")]
    public extern string StrokeAlign { get; set; }

    /// <summary>
    /// strokeLinecap
    /// </summary>
    [Description("@#strokeLinecap")]
    public extern string StrokeLinecap { get; set; }

    /// <summary>
    /// strokeLinejoin
    /// </summary>
    [Description("@#strokeLinejoin")]
    public extern string StrokeLinejoin { get; set; }

    /// <summary>
    /// strokeMiterlimit
    /// </summary>
    [Description("@#strokeMiterlimit")]
    public extern string StrokeMiterlimit { get; set; }

    /// <summary>
    /// strokeBreak
    /// </summary>
    [Description("@#strokeBreak")]
    public extern string StrokeBreak { get; set; }

    /// <summary>
    /// strokeDasharray
    /// </summary>
    [Description("@#strokeDasharray")]
    public extern string StrokeDasharray { get; set; }

    /// <summary>
    /// strokeDashoffset
    /// </summary>
    [Description("@#strokeDashoffset")]
    public extern string StrokeDashoffset { get; set; }

    /// <summary>
    /// strokeDashCorner
    /// </summary>
    [Description("@#strokeDashCorner")]
    public extern string StrokeDashCorner { get; set; }

    /// <summary>
    /// strokeDashJustify
    /// </summary>
    [Description("@#strokeDashJustify")]
    public extern string StrokeDashJustify { get; set; }

    /// <summary>
    /// strokeColor
    /// </summary>
    [Description("@#strokeColor")]
    public extern string StrokeColor { get; set; }

    /// <summary>
    /// strokeImage
    /// </summary>
    [Description("@#strokeImage")]
    public extern string StrokeImage { get; set; }

    /// <summary>
    /// strokeOrigin
    /// </summary>
    [Description("@#strokeOrigin")]
    public extern string StrokeOrigin { get; set; }

    /// <summary>
    /// strokePosition
    /// </summary>
    [Description("@#strokePosition")]
    public extern string StrokePosition { get; set; }

    /// <summary>
    /// strokeSize
    /// </summary>
    [Description("@#strokeSize")]
    public extern string StrokeSize { get; set; }

    /// <summary>
    /// strokeRepeat
    /// </summary>
    [Description("@#strokeRepeat")]
    public extern string StrokeRepeat { get; set; }

    /// <summary>
    /// strokeOpacity
    /// </summary>
    [Description("@#strokeOpacity")]
    public extern string StrokeOpacity { get; set; }

    /// <summary>
    /// backdropFilter
    /// </summary>
    [Description("@#backdropFilter")]
    public extern string BackdropFilter { get; set; }

    /// <summary>
    /// offsetPath
    /// </summary>
    [Description("@#offsetPath")]
    public extern string OffsetPath { get; set; }

    /// <summary>
    /// offsetDistance
    /// </summary>
    [Description("@#offsetDistance")]
    public extern string OffsetDistance { get; set; }

    /// <summary>
    /// offsetPosition
    /// </summary>
    [Description("@#offsetPosition")]
    public extern string OffsetPosition { get; set; }

    /// <summary>
    /// offsetAnchor
    /// </summary>
    [Description("@#offsetAnchor")]
    public extern string OffsetAnchor { get; set; }

    /// <summary>
    /// offsetRotate
    /// </summary>
    [Description("@#offsetRotate")]
    public extern string OffsetRotate { get; set; }

    /// <summary>
    /// offset
    /// </summary>
    [Description("@#offset")]
    public extern string Offset { get; set; }

    /// <summary>
    /// strokeAlignment
    /// </summary>
    [Description("@#strokeAlignment")]
    public extern string StrokeAlignment { get; set; }

    /// <summary>
    /// strokeDashcorner
    /// </summary>
    [Description("@#strokeDashcorner")]
    public extern string StrokeDashcorner { get; set; }

    /// <summary>
    /// strokeDashadjust
    /// </summary>
    [Description("@#strokeDashadjust")]
    public extern string StrokeDashadjust { get; set; }
}