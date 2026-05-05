namespace ECMAScript.CSS;

/// <summary>
/// AnimationWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#AnimationWorkletGlobalScope")]
public class AnimationWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// registerAnimator
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="animatorCtor">animatorCtor</param>
    [Description("@#registerAnimator")]
    public extern void RegisterAnimator(string name, AnimatorInstanceConstructor animatorCtor);
}

/// <summary>
/// BreakToken
/// </summary>
[ECMAScript]
[Description("@#BreakToken")]
public class BreakToken
{
    /// <summary>
    /// childBreakTokens
    /// </summary>
    [Description("@#childBreakTokens")]
    public extern FrozenSet<ChildBreakToken> ChildBreakTokens { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }
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
    public static extern CSSColorValueParseResult Parse(string cssText);
}

/// <summary>
/// CSSConditionRule
/// </summary>
[ECMAScript]
[Description("@#CSSConditionRule")]
public class CSSConditionRule : CSSGroupingRule
{
    /// <summary>
    /// conditionText
    /// </summary>
    [Description("@#conditionText")]
    public extern string ConditionText { get; }
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
/// CSSImageValue
/// </summary>
[ECMAScript]
[Description("@#CSSImageValue")]
public class CSSImageValue : CSSStyleValue
{
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
/// CSSMediaRule
/// </summary>
[ECMAScript]
[Description("@#CSSMediaRule")]
public class CSSMediaRule : CSSConditionRule
{
    /// <summary>
    /// media
    /// </summary>
    [Description("@#media")]
    public extern MediaList Media { get; }
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
    public extern CSSNumericValue Add(params CSSNumberish[] values);

    /// <summary>
    /// sub
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#sub")]
    public extern CSSNumericValue Sub(params CSSNumberish[] values);

    /// <summary>
    /// mul
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#mul")]
    public extern CSSNumericValue Mul(params CSSNumberish[] values);

    /// <summary>
    /// div
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#div")]
    public extern CSSNumericValue Div(params CSSNumberish[] values);

    /// <summary>
    /// min
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#min")]
    public extern CSSNumericValue Min(params CSSNumberish[] values);

    /// <summary>
    /// max
    /// </summary>
    /// <param name="values">values</param>
    [Description("@#max")]
    public extern CSSNumericValue Max(params CSSNumberish[] values);

    /// <summary>
    /// equals
    /// </summary>
    /// <param name="value">value</param>
    [Description("@#equals")]
    public extern bool Equals(params CSSNumberish[] value);

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
    public extern CSSMathSum ToSum(params string[] units);

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
    public static extern CSSNumericValue Parse(string cssText);
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
/// CSSPageDescriptors
/// </summary>
[ECMAScript]
[Description("@#CSSPageDescriptors")]
public class CSSPageDescriptors : CSSStyleDeclaration
{
    /// <summary>
    /// margin
    /// </summary>
    [Description("@#margin")]
    public extern string Margin { get; set; }

    /// <summary>
    /// marginTop
    /// </summary>
    [Description("@#marginTop")]
    public extern string MarginTop { get; set; }

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
    /// marginLeft
    /// </summary>
    [Description("@#marginLeft")]
    public extern string MarginLeft { get; set; }

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
/// CSSParserAtRule
/// </summary>
[ECMAScript]
[Description("@#CSSParserAtRule")]
public class CSSParserAtRule : CSSParserRule
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="prelude">prelude</param>
    /// <param name="body">body</param>
    public extern CSSParserAtRule(string name, CSSToken[] prelude, CSSParserRule[]? body);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// prelude
    /// </summary>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule>? Body { get; }
}

/// <summary>
/// CSSParserBlock
/// </summary>
[ECMAScript]
[Description("@#CSSParserBlock")]
public class CSSParserBlock : CSSParserValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="body">body</param>
    public extern CSSParserBlock(string name, CSSParserValue[] body);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// CSSParserDeclaration
/// </summary>
[ECMAScript]
[Description("@#CSSParserDeclaration")]
public class CSSParserDeclaration : CSSParserRule
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="body">body</param>
    public extern CSSParserDeclaration(string name, CSSParserRule[] body);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserValue> Body { get; }
}

/// <summary>
/// CSSParserFunction
/// </summary>
[ECMAScript]
[Description("@#CSSParserFunction")]
public class CSSParserFunction : CSSParserValue
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="args">args</param>
    public extern CSSParserFunction(string name, CSSParserValue[][] args);

    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// args
    /// </summary>
    [Description("@#args")]
    public extern FrozenSet<FrozenSet<CSSParserValue>> Args { get; }
}

/// <summary>
/// CSSParserQualifiedRule
/// </summary>
[ECMAScript]
[Description("@#CSSParserQualifiedRule")]
public class CSSParserQualifiedRule : CSSParserRule
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="prelude">prelude</param>
    /// <param name="body">body</param>
    public extern CSSParserQualifiedRule(CSSToken[] prelude, CSSParserRule[]? body);

    /// <summary>
    /// prelude
    /// </summary>
    [Description("@#prelude")]
    public extern FrozenSet<CSSParserValue> Prelude { get; }

    /// <summary>
    /// body
    /// </summary>
    [Description("@#body")]
    public extern FrozenSet<CSSParserRule> Body { get; }
}

/// <summary>
/// CSSParserRule
/// </summary>
[ECMAScript]
[Description("@#CSSParserRule")]
public abstract class CSSParserRule
{
}

/// <summary>
/// CSSParserValue
/// </summary>
[ECMAScript]
[Description("@#CSSParserValue")]
public abstract class CSSParserValue
{
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
/// CSSPropertyRule
/// </summary>
[ECMAScript]
[Description("@#CSSPropertyRule")]
public class CSSPropertyRule : CSSRule
{
    /// <summary>
    /// name
    /// </summary>
    [Description("@#name")]
    public extern string Name { get; }

    /// <summary>
    /// syntax
    /// </summary>
    [Description("@#syntax")]
    public extern string Syntax { get; }

    /// <summary>
    /// inherits
    /// </summary>
    [Description("@#inherits")]
    public extern bool Inherits { get; }

    /// <summary>
    /// initialValue
    /// </summary>
    [Description("@#initialValue")]
    public extern string? InitialValue { get; }
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
/// CSSRule
/// </summary>
[ECMAScript]
[Description("@#CSSRule")]
public partial class CSSRule
{
    /// <summary>
    /// SUPPORTS_RULE
    /// </summary>
    [Description("@#SUPPORTS_RULE")]
    public const ushort SUPPORTS_RULE = 12;

    /// <summary>
    /// cssText
    /// </summary>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

    /// <summary>
    /// parentRule
    /// </summary>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }

    /// <summary>
    /// parentStyleSheet
    /// </summary>
    [Description("@#parentStyleSheet")]
    public extern CSSStyleSheet? ParentStyleSheet { get; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern ushort Type { get; }

    /// <summary>
    /// STYLE_RULE
    /// </summary>
    [Description("@#STYLE_RULE")]
    public const ushort STYLE_RULE = 1;

    /// <summary>
    /// CHARSET_RULE
    /// </summary>
    [Description("@#CHARSET_RULE")]
    public const ushort CHARSET_RULE = 2;

    /// <summary>
    /// IMPORT_RULE
    /// </summary>
    [Description("@#IMPORT_RULE")]
    public const ushort IMPORT_RULE = 3;

    /// <summary>
    /// MEDIA_RULE
    /// </summary>
    [Description("@#MEDIA_RULE")]
    public const ushort MEDIA_RULE = 4;

    /// <summary>
    /// FONT_FACE_RULE
    /// </summary>
    [Description("@#FONT_FACE_RULE")]
    public const ushort FONT_FACE_RULE = 5;

    /// <summary>
    /// PAGE_RULE
    /// </summary>
    [Description("@#PAGE_RULE")]
    public const ushort PAGE_RULE = 6;

    /// <summary>
    /// MARGIN_RULE
    /// </summary>
    [Description("@#MARGIN_RULE")]
    public const ushort MARGIN_RULE = 9;

    /// <summary>
    /// NAMESPACE_RULE
    /// </summary>
    [Description("@#NAMESPACE_RULE")]
    public const ushort NAMESPACE_RULE = 10;
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
/// CSSStyleDeclaration
/// </summary>
[ECMAScript]
[Description("@#CSSStyleDeclaration")]
public partial class CSSStyleDeclaration
{
    /// <summary>
    /// objectFit
    /// </summary>
    [Description("@#objectFit")]
    public extern string ObjectFit { get; set; }

    /// <summary>
    /// imageResolution
    /// </summary>
    [Description("@#imageResolution")]
    public extern string ImageResolution { get; set; }

    /// <summary>
    /// cssText
    /// </summary>
    [Description("@#cssText")]
    public extern string CssText { get; set; }

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
    public extern string GetItem(uint index);

    /// <summary>
    /// getPropertyValue
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#getPropertyValue")]
    public extern string GetPropertyValue(string property);

    /// <summary>
    /// getPropertyPriority
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#getPropertyPriority")]
    public extern string GetPropertyPriority(string property);

    /// <summary>
    /// setProperty
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="value">value</param>
    /// <param name="priority">priority</param>
    [Description("@#setProperty")]
    public extern void SetProperty(string property, string value, string priority = "");

    /// <summary>
    /// removeProperty
    /// </summary>
    /// <param name="property">property</param>
    [Description("@#removeProperty")]
    public extern string RemoveProperty(string property);

    /// <summary>
    /// parentRule
    /// </summary>
    [Description("@#parentRule")]
    public extern CSSRule? ParentRule { get; }
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
    public extern int AddRule(string selector = "undefined", string style = "undefined", uint? index = default);

    /// <summary>
    /// removeRule
    /// </summary>
    /// <param name="index">index</param>
    [Description("@#removeRule")]
    public extern void RemoveRule(uint index = 0);
}

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
    public static extern CSSStyleValue Parse(string property, string cssText);

    /// <summary>
    /// parseAll
    /// </summary>
    /// <param name="property">property</param>
    /// <param name="cssText">cssText</param>
    [Description("@#parseAll")]
    public static extern CSSStyleValue[] ParseAll(string property, string cssText);
}

/// <summary>
/// CSSSupportsRule
/// </summary>
[ECMAScript]
[Description("@#CSSSupportsRule")]
public class CSSSupportsRule : CSSConditionRule
{
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
/// ChildBreakToken
/// </summary>
[ECMAScript]
[Description("@#ChildBreakToken")]
public class ChildBreakToken
{
    /// <summary>
    /// breakType
    /// </summary>
    [Description("@#breakType")]
    public extern BreakType BreakType { get; }

    /// <summary>
    /// child
    /// </summary>
    [Description("@#child")]
    public extern LayoutChild Child { get; }
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
}

/// <summary>
/// FragmentResult
/// </summary>
[ECMAScript]
[Description("@#FragmentResult")]
public class FragmentResult
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="options">options</param>
    public extern FragmentResult(FragmentResultOptions options);

    /// <summary>
    /// inlineSize
    /// </summary>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// blockSize
    /// </summary>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }
}

/// <summary>
/// Highlight
/// </summary>
[ECMAScript]
[Description("@#Highlight")]
public class Highlight : ISet<AbstractRange>
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="initialRanges">initialRanges</param>
    public extern Highlight(AbstractRange initialRanges);

    #region Set
    extern int ICollection<AbstractRange>.Count { get; }
    extern bool ICollection<AbstractRange>.IsReadOnly { get; }
    extern bool ISet<AbstractRange>.Add(AbstractRange item);
    extern void ICollection<AbstractRange>.Clear();
    extern bool ICollection<AbstractRange>.Contains(AbstractRange item);
    extern void ICollection<AbstractRange>.CopyTo(AbstractRange[] array, int arrayIndex);
    extern void ISet<AbstractRange>.ExceptWith(IEnumerable<AbstractRange> other);
    extern IEnumerator<AbstractRange> IEnumerable<AbstractRange>.GetEnumerator();
    extern void ISet<AbstractRange>.IntersectWith(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsProperSubsetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsProperSupersetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsSubsetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.IsSupersetOf(IEnumerable<AbstractRange> other);
    extern bool ISet<AbstractRange>.Overlaps(IEnumerable<AbstractRange> other);
    extern bool ICollection<AbstractRange>.Remove(AbstractRange item);
    extern bool ISet<AbstractRange>.SetEquals(IEnumerable<AbstractRange> other);
    extern void ISet<AbstractRange>.SymmetricExceptWith(IEnumerable<AbstractRange> other);
    extern void ISet<AbstractRange>.UnionWith(IEnumerable<AbstractRange> other);
    extern void ICollection<AbstractRange>.Add(AbstractRange item);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion

    /// <summary>
    /// priority
    /// </summary>
    [Description("@#priority")]
    public extern int Priority { get; set; }

    /// <summary>
    /// type
    /// </summary>
    [Description("@#type")]
    public extern HighlightType Type { get; set; }
}

/// <summary>
/// HighlightRegistry
/// </summary>
[ECMAScript]
[Description("@#HighlightRegistry")]
public class HighlightRegistry : IDictionary<string, Highlight>
{
    #region Dictionary
    extern Highlight IDictionary<string, Highlight>.this[string key] { get; set; }
    extern ICollection<string> IDictionary<string, Highlight>.Keys { get; }
    extern ICollection<Highlight> IDictionary<string, Highlight>.Values { get; }
    extern int ICollection<KeyValuePair<string, Highlight>>.Count { get; }
    extern bool ICollection<KeyValuePair<string, Highlight>>.IsReadOnly { get; }
    extern void IDictionary<string, Highlight>.Add(string key, Highlight value);
    extern void ICollection<KeyValuePair<string, Highlight>>.Add(KeyValuePair<string, Highlight> item);
    extern void ICollection<KeyValuePair<string, Highlight>>.Clear();
    extern bool ICollection<KeyValuePair<string, Highlight>>.Contains(KeyValuePair<string, Highlight> item);
    extern bool IDictionary<string, Highlight>.ContainsKey(string key);
    extern void ICollection<KeyValuePair<string, Highlight>>.CopyTo(KeyValuePair<string, Highlight>[] array, int arrayIndex);
    extern IEnumerator<KeyValuePair<string, Highlight>> IEnumerable<KeyValuePair<string, Highlight>>.GetEnumerator();
    extern bool IDictionary<string, Highlight>.Remove(string key);
    extern bool ICollection<KeyValuePair<string, Highlight>>.Remove(KeyValuePair<string, Highlight> item);
    extern bool IDictionary<string, Highlight>.TryGetValue(string key, [MaybeNullWhen(false)] out Highlight value);
    extern IEnumerator IEnumerable.GetEnumerator();
    #endregion
}

/// <summary>
/// IntrinsicSizes
/// </summary>
[ECMAScript]
[Description("@#IntrinsicSizes")]
public class IntrinsicSizes
{
    /// <summary>
    /// minContentSize
    /// </summary>
    [Description("@#minContentSize")]
    public extern double MinContentSize { get; }

    /// <summary>
    /// maxContentSize
    /// </summary>
    [Description("@#maxContentSize")]
    public extern double MaxContentSize { get; }
}

/// <summary>
/// LayoutChild
/// </summary>
[ECMAScript]
[Description("@#LayoutChild")]
public class LayoutChild
{
    /// <summary>
    /// styleMap
    /// </summary>
    [Description("@#styleMap")]
    public extern StylePropertyMapReadOnly StyleMap { get; }

    /// <summary>
    /// intrinsicSizes
    /// </summary>
    [Description("@#intrinsicSizes")]
    public extern PromiseResult<IntrinsicSizes> IntrinsicSizes();

    /// <summary>
    /// layoutNextFragment
    /// </summary>
    /// <param name="constraints">constraints</param>
    /// <param name="breakToken">breakToken</param>
    [Description("@#layoutNextFragment")]
    public extern PromiseResult<LayoutFragment> LayoutNextFragment(LayoutConstraintsOptions constraints, ChildBreakToken breakToken);
}

/// <summary>
/// LayoutConstraints
/// </summary>
[ECMAScript]
[Description("@#LayoutConstraints")]
public class LayoutConstraints
{
    /// <summary>
    /// availableInlineSize
    /// </summary>
    [Description("@#availableInlineSize")]
    public extern double AvailableInlineSize { get; }

    /// <summary>
    /// availableBlockSize
    /// </summary>
    [Description("@#availableBlockSize")]
    public extern double AvailableBlockSize { get; }

    /// <summary>
    /// fixedInlineSize
    /// </summary>
    [Description("@#fixedInlineSize")]
    public extern double? FixedInlineSize { get; }

    /// <summary>
    /// fixedBlockSize
    /// </summary>
    [Description("@#fixedBlockSize")]
    public extern double? FixedBlockSize { get; }

    /// <summary>
    /// percentageInlineSize
    /// </summary>
    [Description("@#percentageInlineSize")]
    public extern double PercentageInlineSize { get; }

    /// <summary>
    /// percentageBlockSize
    /// </summary>
    [Description("@#percentageBlockSize")]
    public extern double PercentageBlockSize { get; }

    /// <summary>
    /// blockFragmentationOffset
    /// </summary>
    [Description("@#blockFragmentationOffset")]
    public extern double? BlockFragmentationOffset { get; }

    /// <summary>
    /// blockFragmentationType
    /// </summary>
    [Description("@#blockFragmentationType")]
    public extern BlockFragmentationType BlockFragmentationType { get; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }
}

/// <summary>
/// LayoutEdges
/// </summary>
[ECMAScript]
[Description("@#LayoutEdges")]
public class LayoutEdges
{
    /// <summary>
    /// inlineStart
    /// </summary>
    [Description("@#inlineStart")]
    public extern double InlineStart { get; }

    /// <summary>
    /// inlineEnd
    /// </summary>
    [Description("@#inlineEnd")]
    public extern double InlineEnd { get; }

    /// <summary>
    /// blockStart
    /// </summary>
    [Description("@#blockStart")]
    public extern double BlockStart { get; }

    /// <summary>
    /// blockEnd
    /// </summary>
    [Description("@#blockEnd")]
    public extern double BlockEnd { get; }

    /// <summary>
    /// inline
    /// </summary>
    [Description("@#inline")]
    public extern double Inline { get; }

    /// <summary>
    /// block
    /// </summary>
    [Description("@#block")]
    public extern double Block { get; }
}

/// <summary>
/// LayoutFragment
/// </summary>
[ECMAScript]
[Description("@#LayoutFragment")]
public class LayoutFragment
{
    /// <summary>
    /// inlineSize
    /// </summary>
    [Description("@#inlineSize")]
    public extern double InlineSize { get; }

    /// <summary>
    /// blockSize
    /// </summary>
    [Description("@#blockSize")]
    public extern double BlockSize { get; }

    /// <summary>
    /// inlineOffset
    /// </summary>
    [Description("@#inlineOffset")]
    public extern double InlineOffset { get; set; }

    /// <summary>
    /// blockOffset
    /// </summary>
    [Description("@#blockOffset")]
    public extern double BlockOffset { get; set; }

    /// <summary>
    /// data
    /// </summary>
    [Description("@#data")]
    public extern object Data { get; }

    /// <summary>
    /// breakToken
    /// </summary>
    [Description("@#breakToken")]
    public extern ChildBreakToken? BreakToken { get; }
}

/// <summary>
/// LayoutWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#LayoutWorkletGlobalScope")]
public class LayoutWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// registerLayout
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="layoutCtor">layoutCtor</param>
    [Description("@#registerLayout")]
    public extern void RegisterLayout(string name, Action layoutCtor);
}

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
/// PaintRenderingContext2D
/// </summary>
[ECMAScript]
[Description("@#PaintRenderingContext2D")]
public class PaintRenderingContext2D
{
}

/// <summary>
/// PaintSize
/// </summary>
[ECMAScript]
[Description("@#PaintSize")]
public class PaintSize
{
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
}

/// <summary>
/// PaintWorkletGlobalScope
/// </summary>
[ECMAScript]
[Description("@#PaintWorkletGlobalScope")]
public class PaintWorkletGlobalScope : WorkletGlobalScope
{
    /// <summary>
    /// registerPaint
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="paintCtor">paintCtor</param>
    [Description("@#registerPaint")]
    public extern void RegisterPaint(string name, Action paintCtor);

    /// <summary>
    /// devicePixelRatio
    /// </summary>
    [Description("@#devicePixelRatio")]
    public extern double DevicePixelRatio { get; }
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
    public extern void Set(string property, params StylePropertyMapSetValues[] values);
    
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
    public extern void Append(string property, params StylePropertyMapAppendValues[] values);
    
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
    public extern CSSStyleValue? Get(string property);

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
    public extern StyleSheetOwnerNode? OwnerNode { get; }

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
    public extern CSSStyleDeclaration GetComputedStyle(Element elt, string? pseudoElt = default);
}

/// <summary>
/// WorkletAnimation
/// </summary>
[ECMAScript]
[Description("@#WorkletAnimation")]
public class WorkletAnimation(AnimationEffect? effect, AnimationTimeline? timeline) : Animation(effect, timeline)
{
    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="animatorName">animatorName</param>
    /// <param name="effects">effects</param>
    /// <param name="timeline">timeline</param>
    /// <param name="options">options</param>
    public extern WorkletAnimation(string animatorName, WorkletAnimationEffects? effects, AnimationTimeline? timeline, object options);

    /// <summary>
    /// animatorName
    /// </summary>
    [Description("@#animatorName")]
    public extern string AnimatorName { get; }
}

/// <summary>
/// WorkletAnimationEffect
/// </summary>
[ECMAScript]
[Description("@#WorkletAnimationEffect")]
public class WorkletAnimationEffect
{
    /// <summary>
    /// getTiming
    /// </summary>
    [Description("@#getTiming")]
    public extern EffectTiming GetTiming();

    /// <summary>
    /// getComputedTiming
    /// </summary>
    [Description("@#getComputedTiming")]
    public extern ComputedEffectTiming GetComputedTiming();

    /// <summary>
    /// localTime
    /// </summary>
    [Description("@#localTime")]
    public extern double? LocalTime { get; set; }
}

/// <summary>
/// WorkletGroupEffect
/// </summary>
[ECMAScript]
[Description("@#WorkletGroupEffect")]
public class WorkletGroupEffect
{
    /// <summary>
    /// getChildren
    /// </summary>
    [Description("@#getChildren")]
    public extern WorkletAnimationEffect[] GetChildren();
}
