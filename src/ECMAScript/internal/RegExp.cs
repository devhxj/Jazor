using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#RegExp")]
/// <summary>
/// JavaScript <c>RegExp</c> constructor and regular-expression instance API host binding.
/// JavaScript <c>RegExp</c> 构造器及正则表达式实例 API 的宿主绑定。
/// </summary>
/// <remarks>
/// Regular-expression syntax, flags, and match results follow JavaScript <c>RegExp</c>, not
/// <c>System.Text.RegularExpressions</c>. The C# API only describes the mappable host surface.
/// 正则语法、标志和匹配结果遵循 JavaScript <c>RegExp</c>，而不是
/// <c>System.Text.RegularExpressions</c>；C# API 仅描述可映射的宿主表面。
/// </remarks>
public sealed class RegExp : IPattern, IMatchPattern, IMatchAllPattern, ISearchPattern, ISplitPattern
{
	/// <summary>
	/// Gets the JavaScript <c>RegExp.prototype</c> object.
	/// Keeping it on the constructor host avoids splitting the runtime host into an extra CLR wrapper.
	/// 获取 JavaScript <c>RegExp.prototype</c> 对象；保留在构造器宿主上可避免额外的 CLR 包装类型。
	/// </summary>
	[Description("@#prototype")]
	public extern static RegExp Prototype { get; }

	/// <summary>
	/// Escapes regular-expression syntax characters in arbitrary text for literal matching.
	/// This is the direct projection of JavaScript <c>RegExp.escape</c>.
	/// 转义任意文本中的正则语法字符，以便按字面量匹配；直接映射 JavaScript <c>RegExp.escape</c>。
	/// </summary>
	[Description("@#escape")]
	public extern static string Escape(string text);

	/// <summary>
	/// Creates a regular expression from a JavaScript pattern string without flags.
	/// 从 JavaScript 模式字符串创建不带标志的正则表达式。
	/// </summary>
	public extern RegExp(string pattern);

	/// <summary>
	/// Creates a regular expression from a pattern and JavaScript flag string such as <c>g</c>, <c>i</c>, or <c>u</c>.
	/// 从模式和 JavaScript 标志字符串创建正则表达式，例如 <c>g</c>、<c>i</c> 或 <c>u</c>。
	/// </summary>
	public extern RegExp(string pattern, string flags);

	/// <summary>
	/// Recreates a regular expression from an existing JavaScript <see cref="RegExp"/> value.
	/// This overload stays on the constructor host because JavaScript allows <c>new RegExp(existingRegExp)</c>.
	/// 根据已有 JavaScript <see cref="RegExp"/> 值重新创建正则表达式；JavaScript 支持该构造形式。
	/// </summary>
	public extern RegExp(RegExp pattern);

	/// <summary>
	/// Recreates a regular expression from an existing JavaScript <see cref="RegExp"/> value and replaces its flags.
	/// 根据已有 JavaScript <see cref="RegExp"/> 值重新创建正则表达式并替换其标志。
	/// </summary>
	public extern RegExp(RegExp pattern, string flags);

	/// <summary>
	/// Executes the expression against <paramref name="s"/> and returns the next match, or <see langword="null"/>.
	/// With global or sticky expressions, JavaScript reads and updates <see cref="LastIndex"/> as part of this call.
	/// 对 <paramref name="s"/> 执行表达式并返回下一次匹配；没有匹配时返回 <see langword="null"/>。
	/// 对全局或粘连表达式，JavaScript 会在调用中读取并更新 <see cref="LastIndex"/>。
	/// </summary>
	/// <param name="s">The string to search. 要搜索的字符串。</param>
	/// <returns>The match result, or <see langword="null"/> when no match exists. 匹配结果；无匹配时为 <see langword="null"/>。</returns>
	[Description("@#exec")]
	public extern RegExpResult? Exec(string s);

	/// <summary>
	/// Tests whether the expression matches <paramref name="s"/>.
	/// Global and sticky expressions also advance or reset <see cref="LastIndex"/> according to JavaScript semantics.
	/// 测试表达式是否匹配 <paramref name="s"/>；全局和粘连表达式同样会按 JavaScript 语义推进或重置 <see cref="LastIndex"/>。
	/// </summary>
	/// <param name="s">The string to test. 要测试的字符串。</param>
	/// <returns><see langword="true"/> when a match exists. 存在匹配时为 <see langword="true"/>。</returns>
	[Description("@#test")]
	public extern bool Test(string s);

	/// <summary>
	/// Gets the source text of the regular-expression pattern, without literal delimiters or flags.
	/// 获取正则表达式的源文本，不包含字面量分隔符和标志。
	/// </summary>
	[Description("@#source")]
	public extern string Source { get; }

	/// <summary>
	/// Gets whether the global <c>g</c> flag is enabled.
	/// When enabled, matching APIs use <see cref="LastIndex"/> to continue across calls.
	/// 获取是否启用全局 <c>g</c> 标志；启用后匹配 API 使用 <see cref="LastIndex"/> 跨调用继续匹配。
	/// </summary>
	[Description("@#global")]
	public extern bool Global { get; }

	/// <summary>
	/// Gets whether the case-insensitive <c>i</c> flag is enabled.
	/// 获取是否启用忽略大小写的 <c>i</c> 标志。
	/// </summary>
	[Description("@#ignoreCase")]
	public extern bool IgnoreCase { get; }

	/// <summary>
	/// Gets whether the multiline <c>m</c> flag is enabled, which changes the meaning of line anchors.
	/// 获取是否启用多行 <c>m</c> 标志；该标志会改变行锚点的匹配含义。
	/// </summary>
	[Description("@#multiline")]
	public extern bool Multiline { get; }

	/// <summary>
	/// Gets the canonical JavaScript flag string for this expression.
	/// 获取此表达式的规范 JavaScript 标志字符串。
	/// </summary>
	[Description("@#flags")]
	public extern string Flags { get; }

	/// <summary>
	/// Gets whether the dot-all <c>s</c> flag is enabled, allowing <c>.</c> to match line terminators.
	/// 获取是否启用 dot-all <c>s</c> 标志；启用后 <c>.</c> 可以匹配行终止符。
	/// </summary>
	[Description("@#dotAll")]
	public extern bool DotAll { get; }

	/// <summary>
	/// Gets whether the sticky <c>y</c> flag is enabled.
	/// Sticky matching must begin exactly at <see cref="LastIndex"/>.
	/// 获取是否启用粘连 <c>y</c> 标志；粘连匹配必须恰好从 <see cref="LastIndex"/> 开始。
	/// </summary>
	[Description("@#sticky")]
	public extern bool Sticky { get; }

	/// <summary>
	/// Gets whether the Unicode <c>u</c> flag is enabled.
	/// 获取是否启用 Unicode <c>u</c> 标志。
	/// </summary>
	[Description("@#unicode")]
	public extern bool Unicode { get; }

	/// <summary>
	/// Gets whether the Unicode sets <c>v</c> flag is enabled.
	/// 获取是否启用 Unicode 集合 <c>v</c> 标志。
	/// </summary>
	[Description("@#unicodeSets")]
	public extern bool UnicodeSets { get; }

	/// <summary>
	/// Gets whether the match-indices <c>d</c> flag is enabled.
	/// When enabled, match results expose index ranges through their JavaScript <c>indices</c> data.
	/// 获取是否启用匹配索引 <c>d</c> 标志；启用后匹配结果通过 JavaScript <c>indices</c> 数据公开索引范围。
	/// </summary>
	[Description("@#hasIndices")]
	public extern bool HasIndices { get; }

	/// <summary>
	/// Gets or sets the zero-based UTF-16 code-unit position from which global or sticky matching starts.
	/// <see cref="Exec"/> and <see cref="Test"/> can mutate this state, including resetting it after failure.
	/// 获取或设置全局或粘连匹配的零基 UTF-16 代码单元起始位置；<see cref="Exec"/> 和 <see cref="Test"/> 会改变该状态，包括失败后重置。
	/// </summary>
	[Description("@#lastIndex")]
	public extern Number LastIndex { get; set; }

	/// <summary>
	/// Recompiles this expression in place using JavaScript's legacy <c>RegExp.prototype.compile</c> API.
	/// This deprecated API exists only for browser compatibility; prefer constructing a new <see cref="RegExp"/>.
	/// 使用 JavaScript 遗留 <c>RegExp.prototype.compile</c> API 原地重新编译表达式。
	/// 此已弃用 API 仅为浏览器兼容而保留；应优先新建 <see cref="RegExp"/>。
	/// </summary>
	/// <param name="pattern">The replacement pattern source. 替换后的模式源。</param>
	/// <param name="flags">The replacement flags, or <see langword="null"/> to omit them. 替换后的标志；<see langword="null"/> 表示省略。</param>
	/// <returns>This recompiled expression. 重新编译后的当前表达式。</returns>
	[Description("@#compile")]
	public extern RegExp Compile(string pattern, string? flags);

	/// <summary>
	/// Returns the JavaScript source form of the expression, including delimiters and flags.
	/// This is the direct projection of <c>RegExp.prototype.toString()</c>.
	/// 返回包含分隔符和标志的 JavaScript 源形式；直接映射 <c>RegExp.prototype.toString()</c>。
	/// </summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@replace]</c>.
	/// The replacement argument stays as <see cref="object"/> because JavaScript accepts either a string or a callback function here.
	/// JavaScript <c>RegExp.prototype[@@replace]</c> 的隐藏协议桥接；替换参数保留为 <see cref="object"/>，因为 JavaScript 支持字符串或回调函数。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@replace")]
	extern string IPattern.SymbolReplace(string value, object? replacement);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@match]</c>.
	/// JavaScript <c>RegExp.prototype[@@match]</c> 的隐藏协议桥接。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@match")]
	extern Array<string?>? IMatchPattern.SymbolMatch(string value);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@matchAll]</c>.
	/// JavaScript <c>RegExp.prototype[@@matchAll]</c> 的隐藏协议桥接。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@matchAll")]
	extern IEnumerable<RegExpResult> IMatchAllPattern.SymbolMatchAll(string value);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@search]</c>.
	/// JavaScript <c>RegExp.prototype[@@search]</c> 的隐藏协议桥接。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@search")]
	extern Number ISearchPattern.SymbolSearch(string value);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@split]</c>.
	/// JavaScript <c>RegExp.prototype[@@split]</c> 的隐藏协议桥接。
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@split")]
	extern Array<string> ISplitPattern.SymbolSplit(string value, Number? limit);
}
