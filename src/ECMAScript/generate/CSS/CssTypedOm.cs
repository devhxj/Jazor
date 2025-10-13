namespace ECMAScript.CSS;

/// <summary>
/// CSSNumericType
/// </summary>
[ECMAScript]
[Description("@#CSSNumericType")]
public record CSSNumericType(
    [property: Description("@#length")]int Length = default,
	[property: Description("@#angle")]int Angle = default,
	[property: Description("@#time")]int Time = default,
	[property: Description("@#frequency")]int Frequency = default,
	[property: Description("@#resolution")]int Resolution = default,
	[property: Description("@#flex")]int Flex = default,
	[property: Description("@#percent")]int Percent = default,
	[property: Description("@#percentHint")]CSSNumericBaseType? PercentHint = default);

/// <summary>
/// CSSMatrixComponentOptions
/// </summary>
[ECMAScript]
[Description("@#CSSMatrixComponentOptions")]
public record CSSMatrixComponentOptions(
    [property: Description("@#is2D")]bool Is2D = default);

/// <summary>
/// CSSStyleValue
/// </summary>
[ECMAScript]
[Description("@#CSSStyleValue")]
public class CSSStyleValue
{
    /// <summary>
    /// parse
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="cssText">cssText</param>
    [Description("@#parse")]
    public extern static CSSStyleValue Parse(string property, string cssText);

    /// <summary>
    /// parseAll
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="cssText">cssText</param>
    [Description("@#parseAll")]
    public extern static CSSStyleValue[] ParseAll(string property, string cssText);
}

/// <summary>
/// StylePropertyMapReadOnly
/// </summary>
[ECMAScript]
[Description("@#StylePropertyMapReadOnly")]
public class StylePropertyMapReadOnly : IEnumerable<(string, CSSStyleValue[])>
{
    extern IEnumerator<(string, CSSStyleValue[])> IEnumerable<(string, CSSStyleValue[])>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// get
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#get")]
    public extern Either<object, CSSStyleValue> Get(string property);

    /// <summary>
    /// getAll
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#getAll")]
    public extern CSSStyleValue[] GetAll(string property);

    /// <summary>
    /// has
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#has")]
    public extern bool Has(string property);

    /// <summary>
    /// size
    /// </summary>
    [Description("@#size")]
    public extern uint Size { get; }
}

/// <summary>
/// StylePropertyMap
/// </summary>
[ECMAScript]
[Description("@#StylePropertyMap")]
public class StylePropertyMap : StylePropertyMapReadOnly
{
    /// <summary>
    /// set
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="values">values</param>
    [Description("@#set")]
    public extern void Set(string property, Either<CSSStyleValue, string> values);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="property">property para</param>
    /// <param name="values">values</param>
    [Description("@#set")]
    public extern void Set(string property, CSSStyleValue values);

    /// <summary>
    /// set
    /// </summary>
    /// <param name="property">property para</param>
    /// <param name="values">values</param>
    [Description("@#set")]
    public extern void Set(string property, string values);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="values">values</param>
    [Description("@#append")]
    public extern void Append(string property, Either<CSSStyleValue, string> values);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="property">property para</param>
    /// <param name="values">values</param>
    [Description("@#append")]
    public extern void Append(string property, CSSStyleValue values);

    /// <summary>
    /// append
    /// </summary>
    /// <param name="property">property para</param>
    /// <param name="values">values</param>
    [Description("@#append")]
    public extern void Append(string property, string values);

    /// <summary>
    /// delete
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#delete")]
    public extern void Delete(string property);

    /// <summary>
    /// clear
    /// </summary>
    [Description("@#clear")]
    public extern void Clear();
}

/// <summary>
/// Element
/// </summary>
[ECMAScript]
[Description("@#Element")]
public partial class Element
{
    /// <summary>
    /// computedStyleMap
    /// </summary>
    [Description("@#computedStyleMap")]
    public extern StylePropertyMapReadOnly ComputedStyleMap();

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
/// CSSStyleRule
/// </summary>
[ECMAScript]
[Description("@#CSSStyleRule")]
public partial class CSSStyleRule : CSSGroupingRule
{
    /// <summary>
    /// styleMap
    /// </summary>
    [Description("@#styleMap")]
    public extern StylePropertyMap StyleMap { get; }

    /// <summary>
    /// selectorText
    /// </summary>
    [Description("@#selectorText")]
    public extern string SelectorText { get; set; }

    /// <summary>
    /// style
    /// </summary>
    [Description("@#style")]
    public extern CSSStyleProperties Style { get; }
}

/// <summary>
/// CSSUnparsedValue
/// </summary>
[ECMAScript]
[Description("@#CSSUnparsedValue")]
public class CSSUnparsedValue : CSSStyleValue, IEnumerable<CSSUnparsedSegment>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="members">members</param>
    public extern CSSUnparsedValue(CSSUnparsedSegment[] members);

    extern IEnumerator<CSSUnparsedSegment> IEnumerable<CSSUnparsedSegment>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern CSSUnparsedSegment this[uint index] { get; set; }
}

/// <summary>
/// CSSVariableReferenceValue
/// </summary>
[ECMAScript]
[Description("@#CSSVariableReferenceValue")]
public class CSSVariableReferenceValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="variable">variable</param>
    /// <param name="fallback">fallback</param>
    public extern CSSVariableReferenceValue(string variable, CSSUnparsedValue? fallback);

    /// <summary>
    /// variable
    /// </summary>
    [Description("@#variable")]
    public extern string Variable { get; set; }

    /// <summary>
    /// fallback
    /// </summary>
    [Description("@#fallback")]
    public extern CSSUnparsedValue? Fallback { get; }
}

/// <summary>
/// CSSKeywordValue
/// </summary>
[ECMAScript]
[Description("@#CSSKeywordValue")]
public class CSSKeywordValue : CSSStyleValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="value">value</param>
    public extern CSSKeywordValue(string value);

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern string Value { get; set; }
}

/// <summary>
/// CSSNumericValue
/// </summary>
[ECMAScript]
[Description("@#CSSNumericValue")]
public class CSSNumericValue : CSSStyleValue
{
    /// <summary>
    /// add
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#add")]
    public extern CSSNumericValue Add(CSSNumberish values);

    /// <summary>
    /// sub
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#sub")]
    public extern CSSNumericValue Sub(CSSNumberish values);

    /// <summary>
    /// mul
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#mul")]
    public extern CSSNumericValue Mul(CSSNumberish values);

    /// <summary>
    /// div
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#div")]
    public extern CSSNumericValue Div(CSSNumberish values);

    /// <summary>
    /// min
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#min")]
    public extern CSSNumericValue Min(CSSNumberish values);

    /// <summary>
    /// max
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#max")]
    public extern CSSNumericValue Max(CSSNumberish values);

    /// <summary>
    /// equals
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#equals")]
    public extern bool Equals(CSSNumberish value);

    /// <summary>
    /// to
    /// </summary>
    /// <param name="unit">unit</param>
    [Description("@#to")]
    public extern CSSUnitValue To(string unit);

    /// <summary>
    /// toSum
    /// </summary>
    /// <param name="units">units</param>
    [Description("@#toSum")]
    public extern CSSMathSum ToSum(string units);

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern CSSNumericType Type();

    /// <summary>
    /// parse
    /// </summary>
    /// <param name="cssText">cssText</param>
    [Description("@#parse")]
    public extern static CSSNumericValue Parse(string cssText);
}

/// <summary>
/// CSSUnitValue
/// </summary>
[ECMAScript]
[Description("@#CSSUnitValue")]
public class CSSUnitValue : CSSNumericValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="value">value</param>
    /// <param name="unit">unit</param>
    public extern CSSUnitValue(double value, string unit);

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern double Value { get; set; }

    /// <summary>
    /// unit
    /// </summary>
    [Description("@#unit")]
    public extern string Unit { get; }
}

/// <summary>
/// CSSMathValue
/// </summary>
[ECMAScript]
[Description("@#CSSMathValue")]
public class CSSMathValue : CSSNumericValue
{
    /// <summary>
    /// operator
    /// </summary>
    [Description("@#operator")]
    public extern CSSMathOperator Operator { get; }
}

/// <summary>
/// CSSMathSum
/// </summary>
[ECMAScript]
[Description("@#CSSMathSum")]
public class CSSMathSum : CSSMathValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="args">args</param>
    public extern CSSMathSum(CSSNumberish args);

    /// <summary>
    /// values
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// CSSMathProduct
/// </summary>
[ECMAScript]
[Description("@#CSSMathProduct")]
public class CSSMathProduct : CSSMathValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="args">args</param>
    public extern CSSMathProduct(CSSNumberish args);

    /// <summary>
    /// values
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// CSSMathNegate
/// </summary>
[ECMAScript]
[Description("@#CSSMathNegate")]
public class CSSMathNegate : CSSMathValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="arg">arg</param>
    public extern CSSMathNegate(CSSNumberish arg);

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }
}

/// <summary>
/// CSSMathInvert
/// </summary>
[ECMAScript]
[Description("@#CSSMathInvert")]
public class CSSMathInvert : CSSMathValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="arg">arg</param>
    public extern CSSMathInvert(CSSNumberish arg);

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }
}

/// <summary>
/// CSSMathMin
/// </summary>
[ECMAScript]
[Description("@#CSSMathMin")]
public class CSSMathMin : CSSMathValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="args">args</param>
    public extern CSSMathMin(CSSNumberish args);

    /// <summary>
    /// values
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// CSSMathMax
/// </summary>
[ECMAScript]
[Description("@#CSSMathMax")]
public class CSSMathMax : CSSMathValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="args">args</param>
    public extern CSSMathMax(CSSNumberish args);

    /// <summary>
    /// values
    /// </summary>
    [Description("@#values")]
    public extern CSSNumericArray Values { get; }
}

/// <summary>
/// CSSMathClamp
/// </summary>
[ECMAScript]
[Description("@#CSSMathClamp")]
public class CSSMathClamp : CSSMathValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="lower">lower</param>
    /// <param name="value">value</param>
    /// <param name="upper">upper</param>
    public extern CSSMathClamp(CSSNumberish lower, CSSNumberish value, CSSNumberish upper);

    /// <summary>
    /// lower
    /// </summary>
    [Description("@#lower")]
    public extern CSSNumericValue Lower { get; }

    /// <summary>
    /// value
    /// </summary>
    [Description("@#value")]
    public extern CSSNumericValue Value { get; }

    /// <summary>
    /// upper
    /// </summary>
    [Description("@#upper")]
    public extern CSSNumericValue Upper { get; }
}

/// <summary>
/// CSSNumericArray
/// </summary>
[ECMAScript]
[Description("@#CSSNumericArray")]
public class CSSNumericArray : IEnumerable<CSSNumericValue>
{
    extern IEnumerator<CSSNumericValue> IEnumerable<CSSNumericValue>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern CSSNumericValue this[uint index] { get; }
}

/// <summary>
/// CSSTransformValue
/// </summary>
[ECMAScript]
[Description("@#CSSTransformValue")]
public class CSSTransformValue : CSSStyleValue, IEnumerable<CSSTransformComponent>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="transforms">transforms</param>
    public extern CSSTransformValue(CSSTransformComponent[] transforms);

    extern IEnumerator<CSSTransformComponent> IEnumerable<CSSTransformComponent>.GetEnumerator();
    extern IEnumerator IEnumerable.GetEnumerator();

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern uint Length { get; }

    [Description("@#")]
    public extern CSSTransformComponent this[uint index] { get; set; }

    /// <summary>
    /// is2D
    /// </summary>
    [Description("@#is2D")]
    public extern bool Is2D { get; }

    /// <summary>
    /// toMatrix
    /// </summary>
    [Description("@#toMatrix")]
    public extern DOMMatrix ToMatrix();
}

/// <summary>
/// CSSTransformComponent
/// </summary>
[ECMAScript]
[Description("@#CSSTransformComponent")]
public class CSSTransformComponent
{
    /// <summary>
    /// is2D
    /// </summary>
    [Description("@#is2D")]
    public extern bool Is2D { get; set; }

    /// <summary>
    /// toMatrix
    /// </summary>
    [Description("@#toMatrix")]
    public extern DOMMatrix ToMatrix();
}

/// <summary>
/// CSSTranslate
/// </summary>
[ECMAScript]
[Description("@#CSSTranslate")]
public class CSSTranslate : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    public extern CSSTranslate(CSSNumericValue x, CSSNumericValue y, CSSNumericValue z);

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern CSSNumericValue X { get; set; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern CSSNumericValue Y { get; set; }

    /// <summary>
    /// z
    /// </summary>
    [Description("@#z")]
    public extern CSSNumericValue Z { get; set; }
}

/// <summary>
/// CSSRotate
/// </summary>
[ECMAScript]
[Description("@#CSSRotate")]
public class CSSRotate : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="angle">angle</param>
    public extern CSSRotate(CSSNumericValue angle);

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    /// <param name="angle">angle</param>
    public extern CSSRotate(CSSNumberish x, CSSNumberish y, CSSNumberish z, CSSNumericValue angle);

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern CSSNumberish X { get; set; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern CSSNumberish Y { get; set; }

    /// <summary>
    /// z
    /// </summary>
    [Description("@#z")]
    public extern CSSNumberish Z { get; set; }

    /// <summary>
    /// angle
    /// </summary>
    [Description("@#angle")]
    public extern CSSNumericValue Angle { get; set; }
}

/// <summary>
/// CSSScale
/// </summary>
[ECMAScript]
[Description("@#CSSScale")]
public class CSSScale : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="x">x</param>
    /// <param name="y">y</param>
    /// <param name="z">z</param>
    public extern CSSScale(CSSNumberish x, CSSNumberish y, CSSNumberish z);

    /// <summary>
    /// x
    /// </summary>
    [Description("@#x")]
    public extern CSSNumberish X { get; set; }

    /// <summary>
    /// y
    /// </summary>
    [Description("@#y")]
    public extern CSSNumberish Y { get; set; }

    /// <summary>
    /// z
    /// </summary>
    [Description("@#z")]
    public extern CSSNumberish Z { get; set; }
}

/// <summary>
/// CSSSkew
/// </summary>
[ECMAScript]
[Description("@#CSSSkew")]
public class CSSSkew : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="ax">ax</param>
    /// <param name="ay">ay</param>
    public extern CSSSkew(CSSNumericValue ax, CSSNumericValue ay);

    /// <summary>
    /// ax
    /// </summary>
    [Description("@#ax")]
    public extern CSSNumericValue Ax { get; set; }

    /// <summary>
    /// ay
    /// </summary>
    [Description("@#ay")]
    public extern CSSNumericValue Ay { get; set; }
}

/// <summary>
/// CSSSkewX
/// </summary>
[ECMAScript]
[Description("@#CSSSkewX")]
public class CSSSkewX : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="ax">ax</param>
    public extern CSSSkewX(CSSNumericValue ax);

    /// <summary>
    /// ax
    /// </summary>
    [Description("@#ax")]
    public extern CSSNumericValue Ax { get; set; }
}

/// <summary>
/// CSSSkewY
/// </summary>
[ECMAScript]
[Description("@#CSSSkewY")]
public class CSSSkewY : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="ay">ay</param>
    public extern CSSSkewY(CSSNumericValue ay);

    /// <summary>
    /// ay
    /// </summary>
    [Description("@#ay")]
    public extern CSSNumericValue Ay { get; set; }
}

/// <summary>
/// CSSPerspective
/// </summary>
[ECMAScript]
[Description("@#CSSPerspective")]
public class CSSPerspective : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="length">length</param>
    public extern CSSPerspective(CSSPerspectiveValue length);

    /// <summary>
    /// length
    /// </summary>
    [Description("@#length")]
    public extern CSSPerspectiveValue Length { get; set; }
}

/// <summary>
/// CSSMatrixComponent
/// </summary>
[ECMAScript]
[Description("@#CSSMatrixComponent")]
public class CSSMatrixComponent : CSSTransformComponent
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="matrix">matrix</param>
    /// <param name="options">options</param>
    public extern CSSMatrixComponent(DOMMatrixReadOnly matrix, CSSMatrixComponentOptions options);

    /// <summary>
    /// matrix
    /// </summary>
    [Description("@#matrix")]
    public extern DOMMatrix Matrix { get; set; }
}

/// <summary>
/// CSSImageValue
/// </summary>
[ECMAScript]
[Description("@#CSSImageValue")]
public class CSSImageValue : CSSStyleValue
{

}

/// <summary>
/// CSSColorValue
/// </summary>
[ECMAScript]
[Description("@#CSSColorValue")]
public class CSSColorValue : CSSStyleValue
{
    /// <summary>
    /// parse
    /// </summary>
    /// <param name="cssText">cssText</param>
    [Description("@#parse")]
    public extern static Either<CSSColorValue, CSSStyleValue> Parse(string cssText);
}

/// <summary>
/// CSSRGB
/// </summary>
[ECMAScript]
[Description("@#CSSRGB")]
public class CSSRGB : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="r">r</param>
    /// <param name="g">g</param>
    /// <param name="b">b</param>
    /// <param name="alpha">alpha</param>
    public extern CSSRGB(CSSColorRGBComp r, CSSColorRGBComp g, CSSColorRGBComp b, CSSColorPercent alpha);

    /// <summary>
    /// r
    /// </summary>
    [Description("@#r")]
    public extern CSSColorRGBComp R { get; set; }

    /// <summary>
    /// g
    /// </summary>
    [Description("@#g")]
    public extern CSSColorRGBComp G { get; set; }

    /// <summary>
    /// b
    /// </summary>
    [Description("@#b")]
    public extern CSSColorRGBComp B { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// CSSHSL
/// </summary>
[ECMAScript]
[Description("@#CSSHSL")]
public class CSSHSL : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="h">h</param>
    /// <param name="s">s</param>
    /// <param name="l">l</param>
    /// <param name="alpha">alpha</param>
    public extern CSSHSL(CSSColorAngle h, CSSColorPercent s, CSSColorPercent l, CSSColorPercent alpha);

    /// <summary>
    /// h
    /// </summary>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// s
    /// </summary>
    [Description("@#s")]
    public extern CSSColorPercent S { get; set; }

    /// <summary>
    /// l
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// CSSHWB
/// </summary>
[ECMAScript]
[Description("@#CSSHWB")]
public class CSSHWB : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="h">h</param>
    /// <param name="w">w</param>
    /// <param name="b">b</param>
    /// <param name="alpha">alpha</param>
    public extern CSSHWB(CSSNumericValue h, CSSNumberish w, CSSNumberish b, CSSNumberish alpha);

    /// <summary>
    /// h
    /// </summary>
    [Description("@#h")]
    public extern CSSNumericValue H { get; set; }

    /// <summary>
    /// w
    /// </summary>
    [Description("@#w")]
    public extern CSSNumberish W { get; set; }

    /// <summary>
    /// b
    /// </summary>
    [Description("@#b")]
    public extern CSSNumberish B { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSNumberish Alpha { get; set; }
}

/// <summary>
/// CSSLab
/// </summary>
[ECMAScript]
[Description("@#CSSLab")]
public class CSSLab : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="l">l</param>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="alpha">alpha</param>
    public extern CSSLab(CSSColorPercent l, CSSColorNumber a, CSSColorNumber b, CSSColorPercent alpha);

    /// <summary>
    /// l
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// a
    /// </summary>
    [Description("@#a")]
    public extern CSSColorNumber A { get; set; }

    /// <summary>
    /// b
    /// </summary>
    [Description("@#b")]
    public extern CSSColorNumber B { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// CSSLCH
/// </summary>
[ECMAScript]
[Description("@#CSSLCH")]
public class CSSLCH : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="l">l</param>
    /// <param name="c">c</param>
    /// <param name="h">h</param>
    /// <param name="alpha">alpha</param>
    public extern CSSLCH(CSSColorPercent l, CSSColorPercent c, CSSColorAngle h, CSSColorPercent alpha);

    /// <summary>
    /// l
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// c
    /// </summary>
    [Description("@#c")]
    public extern CSSColorPercent C { get; set; }

    /// <summary>
    /// h
    /// </summary>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// CSSOKLab
/// </summary>
[ECMAScript]
[Description("@#CSSOKLab")]
public class CSSOKLab : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="l">l</param>
    /// <param name="a">a</param>
    /// <param name="b">b</param>
    /// <param name="alpha">alpha</param>
    public extern CSSOKLab(CSSColorPercent l, CSSColorNumber a, CSSColorNumber b, CSSColorPercent alpha);

    /// <summary>
    /// l
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// a
    /// </summary>
    [Description("@#a")]
    public extern CSSColorNumber A { get; set; }

    /// <summary>
    /// b
    /// </summary>
    [Description("@#b")]
    public extern CSSColorNumber B { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// CSSOKLCH
/// </summary>
[ECMAScript]
[Description("@#CSSOKLCH")]
public class CSSOKLCH : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="l">l</param>
    /// <param name="c">c</param>
    /// <param name="h">h</param>
    /// <param name="alpha">alpha</param>
    public extern CSSOKLCH(CSSColorPercent l, CSSColorPercent c, CSSColorAngle h, CSSColorPercent alpha);

    /// <summary>
    /// l
    /// </summary>
    [Description("@#l")]
    public extern CSSColorPercent L { get; set; }

    /// <summary>
    /// c
    /// </summary>
    [Description("@#c")]
    public extern CSSColorPercent C { get; set; }

    /// <summary>
    /// h
    /// </summary>
    [Description("@#h")]
    public extern CSSColorAngle H { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSColorPercent Alpha { get; set; }
}

/// <summary>
/// CSSColor
/// </summary>
[ECMAScript]
[Description("@#CSSColor")]
public class CSSColor : CSSColorValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="colorSpace">colorSpace</param>
    /// <param name="channels">channels</param>
    /// <param name="alpha">alpha</param>
    public extern CSSColor(CSSKeywordish colorSpace, CSSColorPercent[] channels, CSSNumberish alpha);

    /// <summary>
    /// colorSpace
    /// </summary>
    [Description("@#colorSpace")]
    public extern CSSKeywordish ColorSpace { get; set; }

    /// <summary>
    /// channels
    /// </summary>
    [Description("@#channels")]
    public extern ObservableCollection<CSSColorPercent> Channels { get; set; }

    /// <summary>
    /// alpha
    /// </summary>
    [Description("@#alpha")]
    public extern CSSNumberish Alpha { get; set; }
}