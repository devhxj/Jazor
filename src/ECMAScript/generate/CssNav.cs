namespace ECMAScript;

/// <summary>
/// FocusableAreasOption
/// </summary>
[ECMAScript]
[Description("@#FocusableAreasOption")]
public record FocusableAreasOption(
    [property: Description("@#mode")]FocusableAreaSearchMode? Mode = default);

/// <summary>
/// SpatialNavigationSearchOptions
/// </summary>
[ECMAScript]
[Description("@#SpatialNavigationSearchOptions")]
public record SpatialNavigationSearchOptions(
    [property: Description("@#candidates")]Node[]? Candidates = default,
	[property: Description("@#container")]Node? Container = default);

/// <summary>
/// NavigationEventInit
/// </summary>
[ECMAScript]
[Description("@#NavigationEventInit")]
public record NavigationEventInit(
    [property: Description("@#dir")]SpatialNavigationDirection? Dir = default,
	[property: Description("@#relatedTarget")]EventTarget? RelatedTarget = null): UIEventInit;

/// <summary>
/// Element
/// </summary>
[ECMAScript]
[Description("@#Element")]
public partial class Element : Node
{
    /// <summary>
    /// getSpatialNavigationContainer
    /// </summary>
    [Description("@#getSpatialNavigationContainer")]
    public extern Node GetSpatialNavigationContainer();

    /// <summary>
    /// focusableAreas
    /// </summary>
    /// <param name="option">option</param>
    [Description("@#focusableAreas")]
    public extern Node[] FocusableAreas(FocusableAreasOption? option = default);

    /// <summary>
    /// spatialNavigationSearch
    /// </summary>
    /// <param name="dir">dir</param>
    /// <param name="options">options</param>
    [Description("@#spatialNavigationSearch")]
    public extern Node? SpatialNavigationSearch(SpatialNavigationDirection dir, SpatialNavigationSearchOptions? options = default);

    /// <summary>
    /// pseudo
    /// </summary>
    /// <param name="type">type</param>
    [Description("@#pseudo")]
    public extern CSSPseudoElement? Pseudo(string type);

    /// <summary>
    /// part
    /// </summary>
    [Description("@#part")]
    public extern List<string> Part { get; }

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
    /// checkVisibility
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#checkVisibility")]
    public extern bool CheckVisibility(CheckVisibilityOptions? options = default);

    /// <summary>
    /// scrollIntoView
    /// </summary>
    /// <param name="arg">arg</param>
    [Description("@#scrollIntoView")]
    public extern void ScrollIntoView(Either<bool, ScrollIntoViewOptions> arg);

    /// <summary>
    /// scrollIntoView
    /// </summary>
    /// <param name="arg">arg</param>
    [Description("@#scrollIntoView")]
    public extern void ScrollIntoView(bool arg);

    /// <summary>
    /// scrollIntoView
    /// </summary>
    /// <param name="arg">arg</param>
    [Description("@#scrollIntoView")]
    public extern void ScrollIntoView(ScrollIntoViewOptions? arg = default);

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
    /// scrollTop
    /// </summary>
    [Description("@#scrollTop")]
    public extern double ScrollTop { get; set; }

    /// <summary>
    /// scrollLeft
    /// </summary>
    [Description("@#scrollLeft")]
    public extern double ScrollLeft { get; set; }

    /// <summary>
    /// scrollWidth
    /// </summary>
    [Description("@#scrollWidth")]
    public extern int ScrollWidth { get; }

    /// <summary>
    /// scrollHeight
    /// </summary>
    [Description("@#scrollHeight")]
    public extern int ScrollHeight { get; }

    /// <summary>
    /// clientTop
    /// </summary>
    [Description("@#clientTop")]
    public extern int ClientTop { get; }

    /// <summary>
    /// clientLeft
    /// </summary>
    [Description("@#clientLeft")]
    public extern int ClientLeft { get; }

    /// <summary>
    /// clientWidth
    /// </summary>
    [Description("@#clientWidth")]
    public extern int ClientWidth { get; }

    /// <summary>
    /// clientHeight
    /// </summary>
    [Description("@#clientHeight")]
    public extern int ClientHeight { get; }

    /// <summary>
    /// currentCSSZoom
    /// </summary>
    [Description("@#currentCSSZoom")]
    public extern double CurrentCSSZoom { get; }

    /// <summary>
    /// namespaceURI
    /// </summary>
    [Description("@#namespaceURI")]
    public extern string? NamespaceURI { get; }

    /// <summary>
    /// prefix
    /// </summary>
    [Description("@#prefix")]
    public extern string? Prefix { get; }

    /// <summary>
    /// localName
    /// </summary>
    [Description("@#localName")]
    public extern string LocalName { get; }

    /// <summary>
    /// tagName
    /// </summary>
    [Description("@#tagName")]
    public extern string TagName { get; }

    /// <summary>
    /// id
    /// </summary>
    [Description("@#id")]
    public extern string Id { get; set; }

    /// <summary>
    /// className
    /// </summary>
    [Description("@#className")]
    public extern string ClassName { get; set; }

    /// <summary>
    /// classList
    /// </summary>
    [Description("@#classList")]
    public extern List<string> ClassList { get; }

    /// <summary>
    /// slot
    /// </summary>
    [Description("@#slot")]
    public extern string Slot { get; set; }

    /// <summary>
    /// hasAttributes
    /// </summary>
    [Description("@#hasAttributes")]
    public extern bool HasAttributes();

    /// <summary>
    /// attributes
    /// </summary>
    [Description("@#attributes")]
    public extern NamedNodeMap Attributes { get; }

    /// <summary>
    /// getAttributeNames
    /// </summary>
    [Description("@#getAttributeNames")]
    public extern string[] GetAttributeNames();

    /// <summary>
    /// getAttribute
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#getAttribute")]
    public extern string? GetAttribute(string qualifiedName);

    /// <summary>
    /// getAttributeNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#getAttributeNS")]
    public extern string? GetAttributeNS(string? @namespace, string localName);

    /// <summary>
    /// setAttribute
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    /// <param name="value">value</param>
    [Description("@#setAttribute")]
    public extern void SetAttribute(string qualifiedName, string value);

    /// <summary>
    /// setAttributeNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="qualifiedName">qualifiedName</param>
    /// <param name="value">value</param>
    [Description("@#setAttributeNS")]
    public extern void SetAttributeNS(string? @namespace, string qualifiedName, string value);

    /// <summary>
    /// removeAttribute
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#removeAttribute")]
    public extern void RemoveAttribute(string qualifiedName);

    /// <summary>
    /// removeAttributeNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#removeAttributeNS")]
    public extern void RemoveAttributeNS(string? @namespace, string localName);

    /// <summary>
    /// toggleAttribute
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    /// <param name="force">force</param>
    [Description("@#toggleAttribute")]
    public extern bool ToggleAttribute(string qualifiedName, bool force);

    /// <summary>
    /// hasAttribute
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#hasAttribute")]
    public extern bool HasAttribute(string qualifiedName);

    /// <summary>
    /// hasAttributeNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#hasAttributeNS")]
    public extern bool HasAttributeNS(string? @namespace, string localName);

    /// <summary>
    /// getAttributeNode
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#getAttributeNode")]
    public extern Attr? GetAttributeNode(string qualifiedName);

    /// <summary>
    /// getAttributeNodeNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#getAttributeNodeNS")]
    public extern Attr? GetAttributeNodeNS(string? @namespace, string localName);

    /// <summary>
    /// setAttributeNode
    /// </summary>
    /// <param name="attr">attr</param>
    [Description("@#setAttributeNode")]
    public extern Attr? SetAttributeNode(Attr attr);

    /// <summary>
    /// setAttributeNodeNS
    /// </summary>
    /// <param name="attr">attr</param>
    [Description("@#setAttributeNodeNS")]
    public extern Attr? SetAttributeNodeNS(Attr attr);

    /// <summary>
    /// removeAttributeNode
    /// </summary>
    /// <param name="attr">attr</param>
    [Description("@#removeAttributeNode")]
    public extern Attr RemoveAttributeNode(Attr attr);

    /// <summary>
    /// attachShadow
    /// </summary>
    /// <param name="init">init</param>
    [Description("@#attachShadow")]
    public extern ShadowRoot AttachShadow(ShadowRootInit init);

    /// <summary>
    /// shadowRoot
    /// </summary>
    [Description("@#shadowRoot")]
    public extern ShadowRoot? ShadowRoot { get; }

    /// <summary>
    /// customElementRegistry
    /// </summary>
    [Description("@#customElementRegistry")]
    public extern CustomElementRegistry? CustomElementRegistry { get; }

    /// <summary>
    /// closest
    /// </summary>
    /// <param name="selectors">selectors</param>
    [Description("@#closest")]
    public extern Element? Closest(string selectors);

    /// <summary>
    /// matches
    /// </summary>
    /// <param name="selectors">selectors</param>
    [Description("@#matches")]
    public extern bool Matches(string selectors);

    /// <summary>
    /// webkitMatchesSelector
    /// </summary>
    /// <param name="selectors">selectors</param>
    [Description("@#webkitMatchesSelector")]
    public extern bool WebkitMatchesSelector(string selectors);

    /// <summary>
    /// getElementsByTagName
    /// </summary>
    /// <param name="qualifiedName">qualifiedName</param>
    [Description("@#getElementsByTagName")]
    public extern HTMLCollection GetElementsByTagName(string qualifiedName);

    /// <summary>
    /// getElementsByTagNameNS
    /// </summary>
    /// <param name="namespace">namespace</param>
    /// <param name="localName">localName</param>
    [Description("@#getElementsByTagNameNS")]
    public extern HTMLCollection GetElementsByTagNameNS(string? @namespace, string localName);

    /// <summary>
    /// getElementsByClassName
    /// </summary>
    /// <param name="classNames">classNames</param>
    [Description("@#getElementsByClassName")]
    public extern HTMLCollection GetElementsByClassName(string classNames);

    /// <summary>
    /// insertAdjacentElement
    /// </summary>
    /// <param name="where">where</param>
    /// <param name="element">element</param>
    [Description("@#insertAdjacentElement")]
    public extern Element? InsertAdjacentElement(string where, Element element);

    /// <summary>
    /// insertAdjacentText
    /// </summary>
    /// <param name="where">where</param>
    /// <param name="data">data</param>
    [Description("@#insertAdjacentText")]
    public extern void InsertAdjacentText(string where, string data);

    /// <summary>
    /// elementTiming
    /// </summary>
    [Description("@#elementTiming")]
    public extern string ElementTiming { get; set; }

    /// <summary>
    /// requestFullscreen
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestFullscreen")]
    public extern PromiseResult<object> RequestFullscreen(FullscreenOptions? options = default);

    /// <summary>
    /// onfullscreenchange
    /// </summary>
    [Description("@#onfullscreenchange")]
    public extern EventHandler Onfullscreenchange { get; set; }

    /// <summary>
    /// onfullscreenerror
    /// </summary>
    [Description("@#onfullscreenerror")]
    public extern EventHandler Onfullscreenerror { get; set; }

    /// <summary>
    /// setHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#setHTMLUnsafe")]
    public extern void SetHTMLUnsafe(Either<TrustedHTML, string> html);

    /// <summary>
    /// setHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#setHTMLUnsafe")]
    public extern void SetHTMLUnsafe(TrustedHTML html);

    /// <summary>
    /// setHTMLUnsafe
    /// </summary>
    /// <param name="html">html</param>
    [Description("@#setHTMLUnsafe")]
    public extern void SetHTMLUnsafe(string html);

    /// <summary>
    /// getHTML
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getHTML")]
    public extern string GetHTML(GetHTMLOptions? options = default);

    /// <summary>
    /// innerHTML
    /// </summary>
    [Description("@#innerHTML")]
    public extern Either<TrustedHTML, string> InnerHTML { get; set; }

    /// <summary>
    /// outerHTML
    /// </summary>
    [Description("@#outerHTML")]
    public extern Either<TrustedHTML, string> OuterHTML { get; set; }

    /// <summary>
    /// insertAdjacentHTML
    /// </summary>
    /// <param name="position">position</param>
    /// <param name="string">string</param>
    [Description("@#insertAdjacentHTML")]
    public extern void InsertAdjacentHTML(string position, Either<TrustedHTML, string> @string);

    /// <summary>
    /// insertAdjacentHTML
    /// </summary>
    /// <param name="position">position para</param>
    /// <param name="string">string</param>
    [Description("@#insertAdjacentHTML")]
    public extern void InsertAdjacentHTML(string position, TrustedHTML @string);

    /// <summary>
    /// insertAdjacentHTML
    /// </summary>
    /// <param name="position">position para</param>
    /// <param name="string">string</param>
    [Description("@#insertAdjacentHTML")]
    public extern void InsertAdjacentHTML(string position, string @string);

    /// <summary>
    /// setPointerCapture
    /// </summary>
    /// <param name="pointerId">pointerId</param>
    [Description("@#setPointerCapture")]
    public extern void SetPointerCapture(int pointerId);

    /// <summary>
    /// releasePointerCapture
    /// </summary>
    /// <param name="pointerId">pointerId</param>
    [Description("@#releasePointerCapture")]
    public extern void ReleasePointerCapture(int pointerId);

    /// <summary>
    /// hasPointerCapture
    /// </summary>
    /// <param name="pointerId">pointerId</param>
    [Description("@#hasPointerCapture")]
    public extern bool HasPointerCapture(int pointerId);

    /// <summary>
    /// requestPointerLock
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#requestPointerLock")]
    public extern PromiseResult<object> RequestPointerLock(PointerLockOptions? options = default);

    #region mixin Region
    /// <summary>
    /// regionOverset
    /// </summary>
    [Description("@#regionOverset")]
    public extern string RegionOverset { get; }

    /// <summary>
    /// getRegionFlowRanges
    /// </summary>
    [Description("@#getRegionFlowRanges")]
    public extern Range[]? GetRegionFlowRanges();
    #endregion

    #region mixin GeometryUtils
    /// <summary>
    /// getBoxQuads
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getBoxQuads")]
    public extern DOMQuad[] GetBoxQuads(BoxQuadOptions? options = default);

    /// <summary>
    /// convertQuadFromNode
    /// </summary>
    /// <param name="quad">quad</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertQuadFromNode")]
    public extern DOMQuad ConvertQuadFromNode(DOMQuadInit quad, GeometryNode from, ConvertCoordinateOptions? options = default);

    /// <summary>
    /// convertRectFromNode
    /// </summary>
    /// <param name="rect">rect</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertRectFromNode")]
    public extern DOMQuad ConvertRectFromNode(DOMRectReadOnly rect, GeometryNode from, ConvertCoordinateOptions? options = default);

    /// <summary>
    /// convertPointFromNode
    /// </summary>
    /// <param name="point">point</param>
    /// <param name="from">from</param>
    /// <param name="options">options</param>
    [Description("@#convertPointFromNode")]
    public extern DOMPoint ConvertPointFromNode(DOMPointInit point, GeometryNode from, ConvertCoordinateOptions? options = default);
    #endregion

    #region mixin ParentNode
    /// <summary>
    /// children
    /// </summary>
    [Description("@#children")]
    public extern HTMLCollection Children { get; }

    /// <summary>
    /// firstElementChild
    /// </summary>
    [Description("@#firstElementChild")]
    public extern Element? FirstElementChild { get; }

    /// <summary>
    /// lastElementChild
    /// </summary>
    [Description("@#lastElementChild")]
    public extern Element? LastElementChild { get; }

    /// <summary>
    /// childElementCount
    /// </summary>
    [Description("@#childElementCount")]
    public extern uint ChildElementCount { get; }

    /// <summary>
    /// prepend
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#prepend")]
    public extern void Prepend(Either<Node, string> nodes);

    /// <summary>
    /// prepend
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#prepend")]
    public extern void Prepend(Node nodes);

    /// <summary>
    /// prepend
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#prepend")]
    public extern void Prepend(string nodes);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#append")]
    public extern void Append(Either<Node, string> nodes);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#append")]
    public extern void Append(Node nodes);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#append")]
    public extern void Append(string nodes);

    /// <summary>
    /// replaceChildren
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceChildren")]
    public extern void ReplaceChildren(Either<Node, string> nodes);

    /// <summary>
    /// replaceChildren
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceChildren")]
    public extern void ReplaceChildren(Node nodes);

    /// <summary>
    /// replaceChildren
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceChildren")]
    public extern void ReplaceChildren(string nodes);

    /// <summary>
    /// moveBefore
    /// </summary>
    /// <param name="node">node</param>
    /// <param name="child">child</param>
    [Description("@#moveBefore")]
    public extern void MoveBefore(Node node, Node? child);

    /// <summary>
    /// querySelector
    /// </summary>
    /// <param name="selectors">selectors</param>
    [Description("@#querySelector")]
    public extern Element? QuerySelector(string selectors);

    /// <summary>
    /// querySelectorAll
    /// </summary>
    /// <param name="selectors">selectors</param>
    [Description("@#querySelectorAll")]
    public extern NodeList QuerySelectorAll(string selectors);
    #endregion

    #region mixin NonDocumentTypeChildNode
    /// <summary>
    /// previousElementSibling
    /// </summary>
    [Description("@#previousElementSibling")]
    public extern Element? PreviousElementSibling { get; }

    /// <summary>
    /// nextElementSibling
    /// </summary>
    [Description("@#nextElementSibling")]
    public extern Element? NextElementSibling { get; }
    #endregion

    #region mixin ChildNode
    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(Either<Node, string> nodes);

    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(Node nodes);

    /// <summary>
    /// before
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#before")]
    public extern void Before(string nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(Either<Node, string> nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(Node nodes);

    /// <summary>
    /// after
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#after")]
    public extern void After(string nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(Either<Node, string> nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(Node nodes);

    /// <summary>
    /// replaceWith
    /// </summary>
    /// <param name="nodes">nodes</param>
    [Description("@#replaceWith")]
    public extern void ReplaceWith(string nodes);

    /// <summary>
    /// remove
    /// </summary>
    [Description("@#remove")]
    public extern void Remove();
    #endregion

    #region mixin Slottable
    /// <summary>
    /// assignedSlot
    /// </summary>
    [Description("@#assignedSlot")]
    public extern HTMLSlotElement? AssignedSlot { get; }
    #endregion

    #region mixin ARIAMixin
    /// <summary>
    /// role
    /// </summary>
    [Description("@#role")]
    public extern string? Role { get; set; }

    /// <summary>
    /// ariaActiveDescendantElement
    /// </summary>
    [Description("@#ariaActiveDescendantElement")]
    public extern Element? AriaActiveDescendantElement { get; set; }

    /// <summary>
    /// ariaAtomic
    /// </summary>
    [Description("@#ariaAtomic")]
    public extern string? AriaAtomic { get; set; }

    /// <summary>
    /// ariaAutoComplete
    /// </summary>
    [Description("@#ariaAutoComplete")]
    public extern string? AriaAutoComplete { get; set; }

    /// <summary>
    /// ariaBrailleLabel
    /// </summary>
    [Description("@#ariaBrailleLabel")]
    public extern string? AriaBrailleLabel { get; set; }

    /// <summary>
    /// ariaBrailleRoleDescription
    /// </summary>
    [Description("@#ariaBrailleRoleDescription")]
    public extern string? AriaBrailleRoleDescription { get; set; }

    /// <summary>
    /// ariaBusy
    /// </summary>
    [Description("@#ariaBusy")]
    public extern string? AriaBusy { get; set; }

    /// <summary>
    /// ariaChecked
    /// </summary>
    [Description("@#ariaChecked")]
    public extern string? AriaChecked { get; set; }

    /// <summary>
    /// ariaColCount
    /// </summary>
    [Description("@#ariaColCount")]
    public extern string? AriaColCount { get; set; }

    /// <summary>
    /// ariaColIndex
    /// </summary>
    [Description("@#ariaColIndex")]
    public extern string? AriaColIndex { get; set; }

    /// <summary>
    /// ariaColIndexText
    /// </summary>
    [Description("@#ariaColIndexText")]
    public extern string? AriaColIndexText { get; set; }

    /// <summary>
    /// ariaColSpan
    /// </summary>
    [Description("@#ariaColSpan")]
    public extern string? AriaColSpan { get; set; }

    /// <summary>
    /// ariaControlsElements
    /// </summary>
    [Description("@#ariaControlsElements")]
    public extern FrozenSet<Element>? AriaControlsElements { get; set; }

    /// <summary>
    /// ariaCurrent
    /// </summary>
    [Description("@#ariaCurrent")]
    public extern string? AriaCurrent { get; set; }

    /// <summary>
    /// ariaDescribedByElements
    /// </summary>
    [Description("@#ariaDescribedByElements")]
    public extern FrozenSet<Element>? AriaDescribedByElements { get; set; }

    /// <summary>
    /// ariaDescription
    /// </summary>
    [Description("@#ariaDescription")]
    public extern string? AriaDescription { get; set; }

    /// <summary>
    /// ariaDetailsElements
    /// </summary>
    [Description("@#ariaDetailsElements")]
    public extern FrozenSet<Element>? AriaDetailsElements { get; set; }

    /// <summary>
    /// ariaDisabled
    /// </summary>
    [Description("@#ariaDisabled")]
    public extern string? AriaDisabled { get; set; }

    /// <summary>
    /// ariaErrorMessageElements
    /// </summary>
    [Description("@#ariaErrorMessageElements")]
    public extern FrozenSet<Element>? AriaErrorMessageElements { get; set; }

    /// <summary>
    /// ariaExpanded
    /// </summary>
    [Description("@#ariaExpanded")]
    public extern string? AriaExpanded { get; set; }

    /// <summary>
    /// ariaFlowToElements
    /// </summary>
    [Description("@#ariaFlowToElements")]
    public extern FrozenSet<Element>? AriaFlowToElements { get; set; }

    /// <summary>
    /// ariaHasPopup
    /// </summary>
    [Description("@#ariaHasPopup")]
    public extern string? AriaHasPopup { get; set; }

    /// <summary>
    /// ariaHidden
    /// </summary>
    [Description("@#ariaHidden")]
    public extern string? AriaHidden { get; set; }

    /// <summary>
    /// ariaInvalid
    /// </summary>
    [Description("@#ariaInvalid")]
    public extern string? AriaInvalid { get; set; }

    /// <summary>
    /// ariaKeyShortcuts
    /// </summary>
    [Description("@#ariaKeyShortcuts")]
    public extern string? AriaKeyShortcuts { get; set; }

    /// <summary>
    /// ariaLabel
    /// </summary>
    [Description("@#ariaLabel")]
    public extern string? AriaLabel { get; set; }

    /// <summary>
    /// ariaLabelledByElements
    /// </summary>
    [Description("@#ariaLabelledByElements")]
    public extern FrozenSet<Element>? AriaLabelledByElements { get; set; }

    /// <summary>
    /// ariaLevel
    /// </summary>
    [Description("@#ariaLevel")]
    public extern string? AriaLevel { get; set; }

    /// <summary>
    /// ariaLive
    /// </summary>
    [Description("@#ariaLive")]
    public extern string? AriaLive { get; set; }

    /// <summary>
    /// ariaModal
    /// </summary>
    [Description("@#ariaModal")]
    public extern string? AriaModal { get; set; }

    /// <summary>
    /// ariaMultiLine
    /// </summary>
    [Description("@#ariaMultiLine")]
    public extern string? AriaMultiLine { get; set; }

    /// <summary>
    /// ariaMultiSelectable
    /// </summary>
    [Description("@#ariaMultiSelectable")]
    public extern string? AriaMultiSelectable { get; set; }

    /// <summary>
    /// ariaOrientation
    /// </summary>
    [Description("@#ariaOrientation")]
    public extern string? AriaOrientation { get; set; }

    /// <summary>
    /// ariaOwnsElements
    /// </summary>
    [Description("@#ariaOwnsElements")]
    public extern FrozenSet<Element>? AriaOwnsElements { get; set; }

    /// <summary>
    /// ariaPlaceholder
    /// </summary>
    [Description("@#ariaPlaceholder")]
    public extern string? AriaPlaceholder { get; set; }

    /// <summary>
    /// ariaPosInSet
    /// </summary>
    [Description("@#ariaPosInSet")]
    public extern string? AriaPosInSet { get; set; }

    /// <summary>
    /// ariaPressed
    /// </summary>
    [Description("@#ariaPressed")]
    public extern string? AriaPressed { get; set; }

    /// <summary>
    /// ariaReadOnly
    /// </summary>
    [Description("@#ariaReadOnly")]
    public extern string? AriaReadOnly { get; set; }

    /// <summary>
    /// ariaRelevant
    /// </summary>
    [Description("@#ariaRelevant")]
    public extern string? AriaRelevant { get; set; }

    /// <summary>
    /// ariaRequired
    /// </summary>
    [Description("@#ariaRequired")]
    public extern string? AriaRequired { get; set; }

    /// <summary>
    /// ariaRoleDescription
    /// </summary>
    [Description("@#ariaRoleDescription")]
    public extern string? AriaRoleDescription { get; set; }

    /// <summary>
    /// ariaRowCount
    /// </summary>
    [Description("@#ariaRowCount")]
    public extern string? AriaRowCount { get; set; }

    /// <summary>
    /// ariaRowIndex
    /// </summary>
    [Description("@#ariaRowIndex")]
    public extern string? AriaRowIndex { get; set; }

    /// <summary>
    /// ariaRowIndexText
    /// </summary>
    [Description("@#ariaRowIndexText")]
    public extern string? AriaRowIndexText { get; set; }

    /// <summary>
    /// ariaRowSpan
    /// </summary>
    [Description("@#ariaRowSpan")]
    public extern string? AriaRowSpan { get; set; }

    /// <summary>
    /// ariaSelected
    /// </summary>
    [Description("@#ariaSelected")]
    public extern string? AriaSelected { get; set; }

    /// <summary>
    /// ariaSetSize
    /// </summary>
    [Description("@#ariaSetSize")]
    public extern string? AriaSetSize { get; set; }

    /// <summary>
    /// ariaSort
    /// </summary>
    [Description("@#ariaSort")]
    public extern string? AriaSort { get; set; }

    /// <summary>
    /// ariaValueMax
    /// </summary>
    [Description("@#ariaValueMax")]
    public extern string? AriaValueMax { get; set; }

    /// <summary>
    /// ariaValueMin
    /// </summary>
    [Description("@#ariaValueMin")]
    public extern string? AriaValueMin { get; set; }

    /// <summary>
    /// ariaValueNow
    /// </summary>
    [Description("@#ariaValueNow")]
    public extern string? AriaValueNow { get; set; }

    /// <summary>
    /// ariaValueText
    /// </summary>
    [Description("@#ariaValueText")]
    public extern string? AriaValueText { get; set; }
    #endregion

    #region mixin Animatable
    /// <summary>
    /// animate
    /// </summary>
    /// <param name="keyframes">keyframes</param>
    /// <param name="options">options</param>
    [Description("@#animate")]
    public extern Animation Animate(object? keyframes, Either<double, KeyframeAnimationOptions> options);

    /// <summary>
    /// animate
    /// </summary>
    /// <param name="keyframes">keyframes para</param>
    /// <param name="options">options</param>
    [Description("@#animate")]
    public extern Animation Animate(object? keyframes, double options);

    /// <summary>
    /// animate
    /// </summary>
    /// <param name="keyframes">keyframes para</param>
    /// <param name="options">options</param>
    [Description("@#animate")]
    public extern Animation Animate(object? keyframes, KeyframeAnimationOptions? options = default);

    /// <summary>
    /// getAnimations
    /// </summary>
    /// <param name="options">options</param>
    [Description("@#getAnimations")]
    public extern Animation[] GetAnimations(GetAnimationsOptions? options = default);
    #endregion
}

/// <summary>
/// NavigationEvent
/// </summary>
[ECMAScript]
[Description("@#NavigationEvent")]
public class NavigationEvent(string type, UIEventInit eventInitDict) : UIEvent(type, eventInitDict)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="type">type</param>
    /// <param name="eventInitDict">eventInitDict</param>
    public extern NavigationEvent(string type, NavigationEventInit eventInitDict);

    /// <summary>
    /// dir
    /// </summary>
    [Description("@#dir")]
    public extern SpatialNavigationDirection Dir { get; }

    /// <summary>
    /// relatedTarget
    /// </summary>
    [Description("@#relatedTarget")]
    public extern EventTarget? RelatedTarget { get; }
}