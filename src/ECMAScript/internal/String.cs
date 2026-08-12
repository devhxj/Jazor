namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>
/// Protocol bridge for JavaScript <c>@@replace</c> pattern objects.
/// JavaScript <c>@@replace</c> 模式对象的协议桥接；隐藏该接口是因为它表达协议能力，而不是独立运行时宿主。
/// </summary>
public interface IPattern
{
	/// <summary>
	/// Bridge for JavaScript <c>@@replace</c>.
	/// The replacement argument stays as <see cref="object"/> because JavaScript accepts either a string or a callback function here.
	/// JavaScript <c>@@replace</c> 的桥接；replacement 保持 <see cref="object"/>，因为 JavaScript 接受文本或回调函数。
	/// </summary>
	string SymbolReplace(string value, object? replacement);
}

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>Protocol bridge for JavaScript <c>@@match</c>. JavaScript <c>@@match</c> 的协议桥接。</summary>
public interface IMatchPattern
{
	/// <summary>
	/// Bridge for JavaScript <c>@@match</c>.
	/// JavaScript <c>@@match</c> 的桥接；结果可为 <see langword="null"/>，对应没有匹配项。
	/// </summary>
	Array<string?>? SymbolMatch(string value);
}

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>Protocol bridge for JavaScript <c>@@matchAll</c>. JavaScript <c>@@matchAll</c> 的协议桥接。</summary>
public interface IMatchAllPattern
{
	/// <summary>
	/// Bridge for JavaScript <c>@@matchAll</c>.
	/// JavaScript <c>@@matchAll</c> 的桥接，返回可枚举的匹配结果投影。
	/// </summary>
	IEnumerable<RegExpResult> SymbolMatchAll(string value);
}

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>Protocol bridge for JavaScript <c>@@search</c>. JavaScript <c>@@search</c> 的协议桥接。</summary>
public interface ISearchPattern
{
	/// <summary>
	/// Bridge for JavaScript <c>@@search</c>.
	/// JavaScript <c>@@search</c> 的桥接，未匹配时返回 JavaScript 约定的 <c>-1</c>。
	/// </summary>
	Number SymbolSearch(string value);
}

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
/// <summary>Protocol bridge for JavaScript <c>@@split</c>. JavaScript <c>@@split</c> 的协议桥接。</summary>
public interface ISplitPattern
{
	/// <summary>
	/// Bridge for JavaScript <c>@@split</c>.
	/// JavaScript <c>@@split</c> 的桥接，返回真正的 JavaScript 数组投影。
	/// </summary>
	Array<string> SymbolSplit(string value, Number? limit = null);
}

/// <summary>
/// Bridge shape used by JavaScript tagged template helpers such as <c>String.raw</c>.
/// This stays hidden because it is a protocol object, not a standalone JavaScript runtime host.
/// 供 <c>String.raw</c> 等 JavaScript tagged template 帮助器使用的桥接形状；它是协议对象而不是独立运行时宿主，因此保持隐藏。
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITemplateStringsArray : IArray<string>
{
	/// <summary>
	/// Raw template segments as exposed by JavaScript template literal objects.
	/// JavaScript 模板字面量对象公开的原始文本分段，不包含转义序列解析后的替换结果。
	/// </summary>
	[Description("@#raw")]
	IArray<string> Raw { get; }
}

public static partial class Global
{
	/// <summary>
	/// Projection of JavaScript String built-ins onto C# string extension members.
	/// Any naming deviation here should be treated as a C# syntax escape hatch,
	/// not as a semantic difference from the JavaScript runtime.
	/// 将 JavaScript String 内置成员投影为 C# string 扩展成员；任何命名偏差仅为 C# 语法适配，
	/// 不表示与 JavaScript 运行时语义不同。
	/// </summary>
	extension(string str)
	{
		/// <summary>
		/// Projection of JavaScript <c>String.prototype.includes</c> onto C# string extension members.
		/// The public surface stays close to JavaScript runtime shape and avoids introducing
		/// a separate CLR wrapper type for string hosts.
		/// 将 JavaScript <c>String.prototype.includes</c> 投影为 C# string 扩展成员，避免引入额外 CLR 包装类型。
		/// </summary>
		[Description("@#includes")]
		public extern bool Includes(string? searchString);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.includes</c> with an explicit start position.
		/// 带显式起始位置的 <c>String.prototype.includes</c> 投影；position 按 JavaScript UTF-16 代码单元索引解释。
		/// </summary>
		[Description("@#includes")]
		public extern bool Includes(string? searchString, Number position);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.startsWith</c> onto C# string extension members.
		/// JavaScript <c>String.prototype.startsWith</c> 的 C# 扩展投影；位置按 UTF-16 代码单元处理。
		/// </summary>
		[Description("@#startsWith")]
		public extern bool StartsWith(string searchString, Number? position = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.endsWith</c> onto C# string extension members.
		/// JavaScript <c>String.prototype.endsWith</c> 的 C# 扩展投影；可选 length 是 JavaScript 代码单元长度边界。
		/// </summary>
		[Description("@#endsWith")]
		public extern bool EndsWith(string searchString, Number? length = null);

		/// <summary>Creates text from one Unicode code point. 从一个 Unicode code point 创建文本。</summary>
		[Description("@#fromCodePoint")]
		public extern static string FromCodePoint(Number num);

		/// <summary>Creates text from Unicode code points. 从多个 Unicode code point 创建文本。</summary>
		[Description("@#fromCodePoint")]
		public extern static string FromCodePoint(params Number[] nums);

		/// <summary>
		/// Projection of JavaScript <c>String.fromCharCode</c> onto the global string host.
		/// The static member is kept on the string host instead of inventing a CLR helper type.
		/// JavaScript <c>String.fromCharCode</c> 的投影；静态成员保留在 string 宿主上，不另建 CLR 帮助器类型。参数为 UTF-16 代码单元，不一定是完整 Unicode code point。
		/// </summary>
		[Description("@#fromCharCode")]
		public extern static string FromCharCode(Number num);

		/// <summary>
		/// Projection of JavaScript <c>String.fromCharCode</c> for multiple code units.
		/// 从多个 UTF-16 代码单元创建文本；代理对处理遵循 JavaScript <c>String.fromCharCode</c>。
		/// </summary>
		[Description("@#fromCharCode")]
		public extern static string FromCharCode(params Number[] nums);

		/// <summary>
		/// Projection of JavaScript <c>String.raw</c>.
		/// The first argument uses a hidden bridge interface because JavaScript supplies a template-literal protocol object rather than a distinct runtime host type.
		/// JavaScript <c>String.raw</c> 投影；首个参数使用隐藏桥接接口，因为 JavaScript 提供的是模板字面量协议对象而非独立运行时类型。
		/// </summary>
		[Description("@#raw")]
		public extern static string Raw(ITemplateStringsArray template, params object?[] substitutions);

		/// <summary>Replaces the first string-pattern match. 替换第一个字符串模式匹配项。</summary>
		[Description("@#replace")]
		public extern string Replace(string pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replace</c> with a callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// 使用回调替换首个字符串模式匹配项；该重载仅公开最常见的 matched substring，JavaScript 仍会传入更多回调参数。
		/// </summary>
		/// <summary>Replaces matches using a JavaScript regular expression. 使用 JavaScript 正则表达式替换匹配项。</summary>
		[Description("@#replace")]
		public extern string Replace(string pattern, Func<string, string> replacement);

		[Description("@#replace")]
		public extern string Replace(RegExp pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replace</c> with a regular-expression callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// 使用正则回调替换匹配项；此 C# 表面仅公开常见的匹配文本参数。
		/// </summary>
		/// <summary>Delegates replacement to a JavaScript <c>@@replace</c> protocol object. 将替换委托给 JavaScript <c>@@replace</c> 协议对象。</summary>
		[Description("@#replace")]
		public extern string Replace(RegExp pattern, Func<string, string> replacement);

		[Description("@#replace")]
		public extern string Replace(IPattern pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replace</c> for objects that participate in the JavaScript <c>@@replace</c> protocol.
		/// The replacement callback remains available because JavaScript forwards it to the protocol method unchanged.
		/// 针对参与 JavaScript <c>@@replace</c> 协议的对象进行替换；回调会原样转交给协议方法。
		/// </summary>
		[Description("@#replace")]
		public extern string Replace(IPattern pattern, Func<string, string> replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c>.
		/// This stays separate from <c>replace</c> because JavaScript treats them as distinct runtime members.
		/// JavaScript <c>String.prototype.replaceAll</c> 投影；它与 <c>replace</c> 是不同的运行时成员，替换所有匹配项。
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(string pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> with a callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// 使用回调替换所有字符串模式匹配项；C# 仅公开常见的匹配文本参数。
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(string pattern, Func<string, string> replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> with a regular expression pattern.
		/// 使用正则表达式替换所有匹配项；是否要求 global flag 由 JavaScript 运行时规则决定。
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(RegExp pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> with a regular-expression callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// 使用正则回调替换所有匹配项；C# 仅公开常见的匹配文本参数。
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(RegExp pattern, Func<string, string> replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> for objects that participate in the JavaScript <c>@@replace</c> protocol.
		/// This stays on the string host so protocol-shaped JavaScript patterns can be consumed without introducing a CLR-specific wrapper API.
		/// 使用参与 <c>@@replace</c> 协议的对象替换所有匹配项；保留在 string 宿主上，避免引入 CLR 专用包装 API。
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(IPattern pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> for objects that participate in the JavaScript <c>@@replace</c> protocol.
		/// The replacement callback remains available because JavaScript forwards it to the protocol method unchanged.
		/// 协议对象的回调替换形式；JavaScript 会将回调原样转交给 <c>@@replace</c> 实现。
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(IPattern pattern, Func<string, string> replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.split</c>.
		/// <see cref="Array{T}"/> is used because JavaScript returns a real array rather than an iterator.
		/// JavaScript <c>String.prototype.split</c> 投影；返回真实数组而非迭代器，因此使用 <see cref="Array{T}"/>。
		/// </summary>
		[Description("@#split")]
		public extern Array<string> Split(string? separator = null, Number? limit = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.split</c> with a regular expression separator.
		/// <see cref="Array{T}"/> is used because JavaScript returns a real array rather than an iterator.
		/// 使用正则分隔文本；返回真实 JavaScript 数组而非迭代器。
		/// </summary>
		[Description("@#split")]
		public extern Array<string> Split(RegExp separator, Number? limit = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.split</c> for objects that participate in the JavaScript <c>@@split</c> protocol.
		/// 将分割委托给 JavaScript <c>@@split</c> 协议对象。
		/// </summary>
		[Description("@#split")]
		public extern Array<string> Split(ISplitPattern separator, Number? limit = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.substring</c>.
		/// JavaScript <c>String.prototype.substring</c> 投影；索引按 UTF-16 代码单元且负数按 JavaScript 规则归零。
		/// </summary>
		[Description("@#substring")]
		public extern string Substring(Number start);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.substring</c> with an explicit end index.
		/// 带结束索引的 <c>substring</c> 投影；若 start 大于 end，JavaScript 会交换二者。
		/// </summary>
		[Description("@#substring")]
		public extern string Substring(Number start, Number end);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.slice</c>.
		/// JavaScript <c>String.prototype.slice</c> 投影；负索引从字符串末尾计算。
		/// </summary>
		[Description("@#slice")]
		public extern string Slice(Number? start = null, Number? end = null);

		/// <summary>
		/// Legacy projection of JavaScript <c>String.prototype.substr</c>.
		/// This remains exposed for runtime compatibility even though newer code should generally prefer <see cref="Slice" /> or <see cref="Substring(Number)" />.
		/// 已废弃的 JavaScript <c>substr</c> 投影；为运行时兼容保留，新代码通常应使用 <see cref="Slice"/> 或 <see cref="Substring(Number)"/>。
		/// </summary>
		[Description("@#substr")]
		public extern string Substr(Number start, Number? length = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.indexOf</c>.
		/// JavaScript <c>String.prototype.indexOf</c> 投影；返回 UTF-16 代码单元索引，未找到时为 <c>-1</c>。
		/// </summary>
		[Description("@#indexOf")]
		public extern Number IndexOf(string searchString, Number? position = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.lastIndexOf</c>.
		/// JavaScript <c>String.prototype.lastIndexOf</c> 投影；从给定位置向前搜索，未找到时为 <c>-1</c>。
		/// </summary>
		[Description("@#lastIndexOf")]
		public extern Number LastIndexOf(string searchString, Number? position = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.trim</c>.
		/// JavaScript <c>String.prototype.trim</c> 投影；返回新字符串，移除两端 JavaScript whitespace。
		/// </summary>
		[Description("@#trim")]
		public extern string Trim();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.trimStart</c>.
		/// JavaScript <c>String.prototype.trimStart</c> 投影；返回移除起始 whitespace 的新字符串。
		/// </summary>
		[Description("@#trimStart")]
		public extern string TrimStart();

		/// <summary>
		/// Legacy JavaScript alias for <see cref="TrimStart"/>.
		/// This remains exposed because many runtimes still provide <c>String.prototype.trimLeft</c> for web compatibility.
		/// <see cref="TrimStart"/> 的遗留 JavaScript 别名；为 Web 兼容保留 <c>String.prototype.trimLeft</c>。
		/// </summary>
		[Description("@#trimLeft")]
		public extern string TrimLeft();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.trimEnd</c>.
		/// JavaScript <c>String.prototype.trimEnd</c> 投影；返回移除结尾 whitespace 的新字符串。
		/// </summary>
		[Description("@#trimEnd")]
		public extern string TrimEnd();

		/// <summary>
		/// Legacy JavaScript alias for <see cref="TrimEnd"/>.
		/// This remains exposed because many runtimes still provide <c>String.prototype.trimRight</c> for web compatibility.
		/// <see cref="TrimEnd"/> 的遗留 JavaScript 别名；为 Web 兼容保留 <c>String.prototype.trimRight</c>。
		/// </summary>
		[Description("@#trimRight")]
		public extern string TrimRight();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.padStart</c>.
		/// JavaScript <c>String.prototype.padStart</c> 投影；目标长度按 UTF-16 代码单元计数。
		/// </summary>
		[Description("@#padStart")]
		public extern string PadStart(Number targetLength, string? padString = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.padEnd</c>.
		/// JavaScript <c>String.prototype.padEnd</c> 投影；目标长度按 UTF-16 代码单元计数。
		/// </summary>
		[Description("@#padEnd")]
		public extern string PadEnd(Number targetLength, string? padString = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.repeat</c>.
		/// JavaScript <c>String.prototype.repeat</c> 投影；负数或无穷 count 的错误由 JavaScript 运行时处理。
		/// </summary>
		[Description("@#repeat")]
		public extern string Repeat(Number count);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.charAt</c>.
		/// JavaScript <c>String.prototype.charAt</c> 投影；返回单个 UTF-16 代码单元，越界时返回空字符串。
		/// </summary>
		[Description("@#charAt")]
		public extern string CharAt(Number index);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.charCodeAt</c>.
		/// JavaScript <c>String.prototype.charCodeAt</c> 投影；返回 UTF-16 代码单元数值，越界时为 <c>NaN</c>。
		/// </summary>
		[Description("@#charCodeAt")]
		public extern Number CharCodeAt(Number index);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.codePointAt</c>.
		/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
		/// and the C# projection maps that absence to <see langword="null" />.
		/// JavaScript <c>String.prototype.codePointAt</c> 投影；可读取完整 Unicode code point，越界 <c>undefined</c> 投影为 <see langword="null"/>。
		/// </summary>
		[Description("@#codePointAt")]
		public extern Number? CodePointAt(Number index);

		/// <summary>
		/// C# host projection of JavaScript <c>String.prototype.at</c>.
		/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
		/// and the C# projection maps that absence to <see langword="null" />.
		/// JavaScript <c>String.prototype.at</c> 的 C# 投影，支持负索引；越界 <c>undefined</c> 投影为 <see langword="null"/>。
		/// </summary>
		[Description("@#at")]
		public extern string? At(Number index);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.match</c>.
		/// Nullable is used because JavaScript returns <c>null</c> when no match is found.
		/// The array elements are nullable because unmatched capture groups are <c>undefined</c> in JavaScript,
		/// and this projection maps that absence to <see langword="null" />.
		/// JavaScript <c>String.prototype.match</c> 投影；无匹配时返回 <see langword="null"/>，未匹配捕获组的 <c>undefined</c> 也投影为 <see langword="null"/>。
		/// </summary>
		[Description("@#match")]
		public extern Array<string?>? Match(RegExp regexp);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.match</c> with a string pattern.
		/// JavaScript converts the string to a regular expression before matching.
		/// 使用字符串模式匹配时 JavaScript 会先构造正则表达式。
		/// </summary>
		[Description("@#match")]
		public extern Array<string?>? Match(string pattern);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.match</c> for objects that participate in the JavaScript <c>@@match</c> protocol.
		/// 将匹配委托给参与 JavaScript <c>@@match</c> 协议的对象。
		/// </summary>
		[Description("@#match")]
		public extern Array<string?>? Match(IMatchPattern pattern);

		/// <summary>
		/// Returns the JavaScript iterator produced by <c>String.prototype.matchAll()</c>.
		/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
		/// 返回 JavaScript <c>String.prototype.matchAll()</c> 产生的迭代器；<see cref="IEnumerable{T}"/> 仅是 C# 可迭代表面。
		/// </summary>
		[Description("@#matchAll")]
		public extern IEnumerable<RegExpResult> MatchAll(RegExp regexp);

		/// <summary>
		/// Returns the JavaScript iterator produced by <c>String.prototype.matchAll()</c> with a string pattern.
		/// JavaScript converts the string to a regular expression before matching.
		/// 字符串模式会先按 JavaScript 规则转换为正则表达式，再返回 matchAll 迭代器。
		/// </summary>
		[Description("@#matchAll")]
		public extern IEnumerable<RegExpResult> MatchAll(string pattern);

		/// <summary>
		/// Returns the JavaScript iterator produced by <c>String.prototype.matchAll()</c> for objects that participate in the JavaScript <c>@@matchAll</c> protocol.
		/// 将 matchAll 委托给参与 JavaScript <c>@@matchAll</c> 协议的对象。
		/// </summary>
		[Description("@#matchAll")]
		public extern IEnumerable<RegExpResult> MatchAll(IMatchAllPattern pattern);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.search</c>.
		/// JavaScript <c>String.prototype.search</c> 投影；返回首个匹配 UTF-16 代码单元索引，未匹配时为 <c>-1</c>。
		/// </summary>
		[Description("@#search")]
		public extern Number Search(RegExp regexp);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.search</c> with a string pattern.
		/// JavaScript converts the string to a regular expression before searching.
		/// 字符串模式会先按 JavaScript 规则转换为正则表达式再搜索。
		/// </summary>
		[Description("@#search")]
		public extern Number Search(string pattern);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.search</c> for objects that participate in the JavaScript <c>@@search</c> protocol.
		/// 将搜索委托给参与 JavaScript <c>@@search</c> 协议的对象。
		/// </summary>
		[Description("@#search")]
		public extern Number Search(ISearchPattern pattern);

		/// <summary>Compares text using the JavaScript runtime's default locale collation. 使用 JavaScript 运行时默认 locale 排序规则比较文本。</summary>
		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString);

		/// <summary>Compares text using a requested JavaScript locale. 使用指定 JavaScript locale 比较文本。</summary>
		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, string? locales);

		/// <summary>Compares text using a requested locale and collation options. 使用指定 locale 与排序选项比较文本。</summary>
		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, string? locales, Intl.CollatorOptions options);

		/// <summary>
		/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
		/// This exists because C# cannot naturally skip the leading locale argument in method calls.
		/// C# 便利重载，用于仅传 Collator 选项并省略前置 locales 参数。
		/// </summary>
		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, Intl.CollatorOptions options);

		/// <summary>
		/// Locale lists use <see cref="IEnumerable{T}"/> so arrays, lists, and other C# sequence shapes can map to the single JavaScript locale-list input.
		/// locale 列表使用 <see cref="IEnumerable{T}"/>，以便数组、列表等 C# 序列映射到单一 JavaScript locale-list 输入。
		/// </summary>
		/// <summary>Compares text using a locale list and collation options. 使用 locale 列表与排序选项比较文本。</summary>
		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, IEnumerable<string> locales);

		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, IEnumerable<string> locales, Intl.CollatorOptions options);

		/// <summary>
		/// Returns the primitive string value carried by this host projection.
		/// This mirrors JavaScript <c>String.prototype.valueOf()</c> rather than a CLR conversion helper.
		/// 返回此宿主投影承载的原始字符串值，镜像 JavaScript <c>String.prototype.valueOf()</c>，不是 CLR 转换帮助器。
		/// </summary>
		[Description("@#valueOf")]
		public extern string ValueOf();

		/// <summary>
		/// Concatenates the string arguments to the end of the current string.
		/// This stays as a runtime host member because JavaScript exposes it as <c>String.prototype.concat</c>.
		/// Arguments are nullable because JavaScript applies string coercion to arbitrary runtime values, including <see langword="null" />.
		/// 连接参数到当前字符串末尾；它是 JavaScript <c>String.prototype.concat</c> 运行时成员。参数可空，因为 JavaScript 会对任意值（包括 <see langword="null"/>）做字符串转换。
		/// </summary>
		[Description("@#concat")]
		public extern string Concat(params object?[] strings);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toUpperCase</c>.
		/// JavaScript <c>String.prototype.toUpperCase</c> 投影，使用默认 Unicode 大小写规则而非 CLR 当前文化规则。
		/// </summary>
		[Description("@#toUpperCase")]
		public extern string ToUpperCase();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLowerCase</c>.
		/// JavaScript <c>String.prototype.toLowerCase</c> 投影，使用默认 Unicode 大小写规则而非 CLR 当前文化规则。
		/// </summary>
		[Description("@#toLowerCase")]
		public extern string ToLowerCase();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleUpperCase</c>.
		/// JavaScript <c>String.prototype.toLocaleUpperCase</c> 投影，使用当前或指定 JavaScript locale。
		/// </summary>
		[Description("@#toLocaleUpperCase")]
		public extern string ToLocaleUpperCase(string? locales = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleUpperCase</c> for locale lists.
		/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
		/// 使用 JavaScript locale 列表执行大写转换；<see cref="IEnumerable{T}"/> 是通用 C# 输入表面。
		/// </summary>
		[Description("@#toLocaleUpperCase")]
		public extern string ToLocaleUpperCase(IEnumerable<string> locales);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleLowerCase</c>.
		/// JavaScript <c>String.prototype.toLocaleLowerCase</c> 投影，使用当前或指定 JavaScript locale。
		/// </summary>
		[Description("@#toLocaleLowerCase")]
		public extern string ToLocaleLowerCase(string? locales = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleLowerCase</c> for locale lists.
		/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
		/// 使用 JavaScript locale 列表执行小写转换；<see cref="IEnumerable{T}"/> 是通用 C# 输入表面。
		/// </summary>
		[Description("@#toLocaleLowerCase")]
		public extern string ToLocaleLowerCase(IEnumerable<string> locales);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.normalize</c>.
		/// The normalization form is optional because JavaScript defaults it to NFC.
		/// JavaScript <c>String.prototype.normalize</c> 投影；form 可省略，因为 JavaScript 默认 NFC。
		/// </summary>
		[Description("@#normalize")]
		public extern string Normalize(string? form = null);

		/// <summary>
		/// Returns whether the string is a well-formed sequence of Unicode code points.
		/// This mirrors JavaScript <c>String.prototype.isWellFormed</c>.
		/// 检查字符串是否为格式良好的 Unicode code point 序列，镜像 JavaScript <c>String.prototype.isWellFormed</c>。
		/// </summary>
		[Description("@#isWellFormed")]
		public extern bool IsWellFormed();

		/// <summary>
		/// Returns a copy of the string with lone surrogates replaced so the result is well-formed Unicode.
		/// This mirrors JavaScript <c>String.prototype.toWellFormed</c>.
		/// 返回将孤立 surrogate 替换为 replacement character 后的文本，镜像 JavaScript <c>String.prototype.toWellFormed</c>。
		/// </summary>
		[Description("@#toWellFormed")]
		public extern string ToWellFormed();

		/// <summary>Uses JavaScript relational comparison for two strings. 对两个字符串使用 JavaScript 关系比较。</summary>
		public extern static bool operator >(string x, string y);

		/// <summary>Uses JavaScript relational comparison for two strings. 对两个字符串使用 JavaScript 关系比较。</summary>
		public extern static bool operator <(string x, string y);
	}
}
