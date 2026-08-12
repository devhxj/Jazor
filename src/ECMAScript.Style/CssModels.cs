namespace ECMAScript.Style;

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents the declaration block shared by class rules, global rules, keyframe frames, and at-rules.
/// Generated properties provide the strongly typed CSS surface; <see cref="additional"/> is reserved for
/// intentional duplicate or currently unmodeled declarations.
/// 表示 class 规则、全局规则、关键帧和 at-rule 共用的声明块。生成属性提供强类型 CSS 表面；
/// <see cref="additional"/> 仅用于有意重复或当前尚未建模的声明。
/// </summary>
public partial record CssDeclarations
{
    /// <summary>
    /// Gets declarations emitted after generated properties, preserving their supplied order and allowing duplicates.
    /// Use this only when a typed generated property cannot express the required CSS declaration.
    /// 获取在生成属性之后按给定顺序输出的声明，并允许重复。仅当强类型生成属性无法表达所需 CSS 声明时使用。
    /// </summary>
    [Description("@#$additional")]
    public ICssDeclaration[]? additional { get; init; }

    /// <summary>
    /// Gets or sets a generated CSS property by its CSS property name.
    /// This preserves the <see cref="CssValue"/> union during lowering; it is not a general raw-string escape hatch.
    /// 按 CSS 属性名获取或设置生成属性。该索引器在 lowering 过程中保留 <see cref="CssValue"/> union；
    /// 它不是通用的原始字符串逃生口。
    /// </summary>
    public extern CssValue? this[string propertyName] { get; set; }
}

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents a CSS declaration block that may contain nested selectors and supported grouping at-rules.
/// The runtime emits the rule's own declarations first, then serializes <see cref="children"/> in authored order.
/// 表示可包含嵌套选择器和受支持分组 at-rule 的 CSS 声明块。运行时先输出规则自身声明，
/// 再按作者提供的顺序序列化 <see cref="children"/>。
/// </summary>
public sealed record CssRule : CssDeclarations
{
    /// <summary>
    /// Gets nested selectors or grouping at-rules associated with this rule.
    /// A selector child is combined with its parent selector; other child kinds wrap the parent rule in an at-rule.
    /// 获取与该规则关联的嵌套选择器或分组 at-rule。选择器子项会与父选择器组合；
    /// 其他子项类型会使用 at-rule 包裹父规则。
    /// </summary>
    [Description("@#$children")]
    public CssChild[]? children { get; init; }
}

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents a declaration appended after generated properties. This is the escape hatch for
/// intentional duplicate declarations, while normal properties remain strongly typed.
/// 表示追加在生成属性后的声明，用于有意保留重复声明；普通属性仍保持强类型。
/// </summary>
public interface ICssDeclaration
{
    /// <summary>
    /// Gets the CSS property name without a trailing colon.
    /// 获取不带末尾冒号的 CSS 属性名。
    /// </summary>
    [Description("@#name")]
    string Name { get; }

    /// <summary>
    /// Gets the strongly typed value emitted for <see cref="Name"/>.
    /// 获取为 <see cref="Name"/> 输出的强类型值。
    /// </summary>
    [Description("@#value")]
    CssValue Value { get; }

    /// <summary>
    /// Gets the declaration priority applied after <see cref="Value"/> has been serialized.
    /// 获取在 <see cref="Value"/> 序列化后应用的声明优先级。
    /// </summary>
    [Description("@#priority")]
    CssDeclarationPriority Priority { get; }
}

[String]
/// <summary>
/// Specifies whether an additional declaration is emitted normally or with the CSS <c>!important</c> priority.
/// This priority applies only to <see cref="ICssDeclaration"/> entries; use <c>important(value)</c> for typed properties.
/// 指定附加声明以普通优先级输出，还是携带 CSS <c>!important</c>。该优先级仅适用于
/// <see cref="ICssDeclaration"/>；强类型属性应使用 <c>important(value)</c>。
/// </summary>
public enum CssDeclarationPriority
{
    /// <summary>Emits the declaration without <c>!important</c>。不输出 <c>!important</c>。</summary>
    [Description("@#normal")]
    Normal,

    /// <summary>Emits the declaration with <c>!important</c>。输出带 <c>!important</c> 的声明。</summary>
    [Description("@#important")]
    Important
}

/// <summary>
/// Represents one explicitly ordered CSS declaration.
/// It is primarily intended for <see cref="CssDeclarations.additional"/> when duplicate declarations or an unmodeled property are required.
/// 表示一条显式排序的 CSS 声明。它主要用于 <see cref="CssDeclarations.additional"/>，
/// 以表达重复声明或尚未建模的属性。
/// </summary>
public sealed record CssDeclaration(
    [property: Description("@#name")] string Name,
    [property: Description("@#value")] CssValue Value,
    [property: Description("@#priority")] CssDeclarationPriority Priority = CssDeclarationPriority.Normal) : ICssDeclaration;

/// <summary>
/// Describes one structural CSS shadow. Optional parts are omitted instead of
/// serialized as defaults, so the authored C# mirrors the CSS grammar.
/// 描述一个结构化 CSS 阴影。可选部分会被省略而非填充默认值，使 C# 写法与 CSS 语法一致。
/// </summary>
public sealed record CssShadow(
    [property: Description("@#offsetX")] CssShadowLength OffsetX,
    [property: Description("@#offsetY")] CssShadowLength OffsetY,
    [property: Description("@#blur")] CssShadowLength? Blur = null,
    [property: Description("@#spread")] CssShadowLength? Spread = null,
    [property: Description("@#color")] CssShadowColor? Color = null,
    [property: Description("@#inset")] bool Inset = false);

/// <summary>
/// Describes one gradient color stop. <see cref="From"/> and <see cref="To"/> model the optional
/// one- or two-position form without admitting arbitrary CSS text.
/// 描述一个渐变色标；From/To 精确表达可选的单位置或双位置形式。
/// </summary>
public sealed record CssGradientStop(
    [property: Description("@#color")] CssColorValue Color,
    [property: Description("@#from")] CssLengthPercentageValue? From = null,
    [property: Description("@#to")] CssLengthPercentageValue? To = null);

[String]
/// <summary>
/// Classifies a nested <see cref="CssChild"/> as either a selector expansion or a supported grouping at-rule.
/// The surrounding <c>ECMAScript.Style</c> namespace already supplies the CSS domain, so the concise name avoids redundant qualification.
/// 将嵌套 <see cref="CssChild"/> 分类为选择器展开或受支持的分组 at-rule。外层
/// <c>ECMAScript.Style</c> 命名空间已表达 CSS 领域，因此使用简洁名称避免重复限定。
/// </summary>
public enum ChildKind
{
    /// <summary>Combines the prelude with the parent selector。将 Prelude 与父选择器组合。</summary>
    [Description("@#selector")]
    Selector,

    /// <summary>Wraps the child rule in <c>@media</c>。使用 <c>@media</c> 包裹子规则。</summary>
    [Description("@#media")]
    Media,

    /// <summary>Wraps the child rule in <c>@supports</c>。使用 <c>@supports</c> 包裹子规则。</summary>
    [Description("@#supports")]
    Supports,

    /// <summary>Wraps the child rule in <c>@container</c>。使用 <c>@container</c> 包裹子规则。</summary>
    [Description("@#container")]
    Container,

    /// <summary>Wraps the child rule in <c>@layer</c>。使用 <c>@layer</c> 包裹子规则。</summary>
    [Description("@#layer")]
    Layer,

    /// <summary>Wraps the child rule in <c>@scope</c>。使用 <c>@scope</c> 包裹子规则。</summary>
    [Description("@#scope")]
    Scope,

    /// <summary>Wraps the child rule in <c>@starting-style</c>, which has no prelude。使用无 Prelude 的 <c>@starting-style</c> 包裹子规则。</summary>
    [Description("@#starting-style")]
    StartingStyle
}

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents one nested rule entry within <see cref="CssRule.children"/>.
/// For <see cref="ChildKind.Selector"/>, <see cref="Prelude"/> is a non-empty nested selector; for grouping kinds it is the at-rule prelude.
/// 表示 <see cref="CssRule.children"/> 中的一条嵌套规则。对于 <see cref="ChildKind.Selector"/>，
/// <see cref="Prelude"/> 是非空嵌套选择器；对于分组类型，它是 at-rule 的 prelude。
/// </summary>
public sealed record CssChild(
    [property: Description("@#kind")] ChildKind Kind,
    [property: Description("@#prelude")] string? Prelude,
    [property: Description("@#rule")] CssRule Rule);

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents one keyframe selector and its declaration block.
/// Use selectors such as <c>from</c>, <c>to</c>, or a percentage; <c>keyframes(...)</c> requires at least one frame.
/// 表示一个关键帧选择器及其声明块。可使用 <c>from</c>、<c>to</c> 或百分比选择器；
/// <c>keyframes(...)</c> 至少需要一个帧。
/// </summary>
public sealed record CssFrame(
    [property: Description("@#selector")] string Selector,
    [property: Description("@#declarations")] CssDeclarations Declarations);

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents a free-form CSS at-rule with declarations and recursively nested at-rules.
/// Unlike <see cref="CssChild"/>, this model is intended for top-level registration through <c>at_rule(...)</c>.
/// 表示带声明和递归嵌套 at-rule 的自由形式 CSS at-rule。与 <see cref="CssChild"/> 不同，
/// 该模型用于通过 <c>at_rule(...)</c> 注册顶层规则。
/// </summary>
public sealed record CssAtRule(
    [property: Description("@#name")] string Name,
    [property: Description("@#declarations")] CssDeclarations Declarations,
    [property: Description("@#prelude")] string? Prelude = null,
    [property: Description("@#children")] CssAtRule[]? Children = null);

[ECMAScript]
[Description("@#")]
/// <summary>
/// Configures an isolated CSS registry or the process-wide default registry before its first registration.
/// A detached context cannot specify <see cref="Target"/> because it deliberately has no DOM ownership.
/// 配置隔离 CSS registry，或在首次注册前配置进程级默认 registry。分离上下文不能指定
/// <see cref="Target"/>，因为它刻意不拥有 DOM。
/// </summary>
public sealed record CssOptions
{
    /// <summary>
    /// Gets the id of the managed <c>style</c> element; blank values are rejected and null uses the default id.
    /// 获取受管理 <c>style</c> 元素的 id；空白值会被拒绝，null 使用默认 id。
    /// </summary>
    [Description("@#styleId")]
    public string? StyleId { get; init; }

    /// <summary>
    /// Gets the CSP nonce copied to a newly created or adopted style element.
    /// 获取复制到新建或接管的 style 元素上的 CSP nonce。
    /// </summary>
    [Description("@#nonce")]
    public string? Nonce { get; init; }

    /// <summary>
    /// Gets the document fragment that owns the managed style element, such as a shadow root.
    /// Null targets the main document head.
    /// 获取拥有受管理 style 元素的文档片段，例如 shadow root。null 表示目标为主文档 head。
    /// </summary>
    [Description("@#target")]
    public DocumentFragment? Target { get; init; }

    /// <summary>
    /// Gets whether the context records CSS only and never injects or adopts a DOM style element.
    /// 获取上下文是否只记录 CSS 而从不注入或接管 DOM style 元素。
    /// </summary>
    [Description("@#detached")]
    public bool Detached { get; init; }
}

[ECMAScript]
[Description("@#")]
/// <summary>
/// Captures the deterministic CSS and hydration payload of one context for SSR or later client adoption.
/// <see cref="HydrationText"/> preserves entry boundaries and ids, whereas <see cref="CssText"/> is the plain concatenated stylesheet.
/// 捕获一个上下文的确定性 CSS 与 hydration 载荷，供 SSR 或稍后的客户端接管使用。
/// <see cref="HydrationText"/> 保留条目边界和 id，<see cref="CssText"/> 则是普通拼接后的样式表。
/// </summary>
public sealed record CssSnapshot(
    [property: Description("@#styleId")] string StyleId,
    [property: Description("@#nonce")] string? Nonce,
    [property: Description("@#cssText")] string CssText,
    [property: Description("@#hydrationText")] string HydrationText);

[ECMAScript]
[Description("@#")]
/// <summary>
/// Represents an isolated CSS registry and, unless configured as detached, its style-element ownership state.
/// Obtain instances through <c>context(...)</c>; the internal maps are runtime implementation details and should not be exposed as API surface.
/// 表示隔离 CSS registry，以及在非分离模式下的 style 元素所有权状态。请通过 <c>context(...)</c> 获取实例；
/// 内部映射是运行时实现细节，不应作为 API 表面使用。
/// </summary>
public sealed record CssContext
{
    internal CssContext(bool initialized)
    {
    }

    [Description("@#$namesByCanonical")]
    internal Map<string, string> NamesByCanonical { get; init; } = null!;

    [Description("@#$canonicalByName")]
    internal Map<string, string> CanonicalByName { get; init; } = null!;

    [Description("@#$bodyById")]
    internal Map<string, string> BodyById { get; init; } = null!;

    [Description("@#$entryIds")]
    internal Array<string> EntryIds { get; set; } = null!;

    [Description("@#$entryBodies")]
    internal Array<string> EntryBodies { get; set; } = null!;

    [Description("@#$styleId")]
    internal string StyleId { get; set; } = null!;

    [Description("@#$nonce")]
    internal string? Nonce { get; set; }

    [Description("@#$target")]
    internal DocumentFragment? Target { get; set; }

    [Description("@#$detached")]
    internal bool Detached { get; set; }

    [Description("@#$hasRegistered")]
    internal bool HasRegistered { get; set; }

    [Description("@#$domStyle")]
    internal HTMLStyleElement? DomStyle { get; set; }

    [Description("@#$domDocument")]
    internal Document? DomDocument { get; set; }

    [Description("@#$domHydrated")]
    internal bool DomHydrated { get; set; }
}
