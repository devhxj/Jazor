namespace ECMAScript;

[ECMAScript]
[Description("@#Intl")]
/// <summary>
/// JavaScript ECMA-402 <c>Intl</c> host and its locale-aware formatting constructors.
/// JavaScript ECMA-402 <c>Intl</c> 宿主及其本地化格式化构造器。
/// </summary>
public static class Intl
{
	/// <summary>
/// Common JavaScript object shape returned by Intl <c>formatToParts()</c> APIs.
/// This is not a runtime global; it models the part records produced by Intl host objects.
/// Intl <c>formatToParts()</c> API 返回的通用 JavaScript 对象形状；不是运行时全局对象，而是 Intl 宿主生成的 part 记录模型。
	/// </summary>
	[Description("@#")]
	public sealed class FormatPart
	{
		/// <summary>Gets the semantic part type, such as <c>integer</c> or <c>currency</c>. 获取语义 part 类型，例如 <c>integer</c> 或 <c>currency</c>。</summary>
		[Description("@#type")]
		public extern string Type { get; }

		/// <summary>Gets the localized text for this part. 获取此 part 的本地化文本。</summary>
		[Description("@#value")]
		public extern string Value { get; }
	}

	/// <summary>
/// JavaScript object shape returned by <c>Intl.ListFormat.prototype.formatToParts()</c>.
/// <c>Intl.ListFormat.prototype.formatToParts()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public sealed class ListFormatPart
	{
		/// <summary>Gets the list part type, such as <c>element</c> or <c>literal</c>. 获取列表 part 类型，例如 <c>element</c> 或 <c>literal</c>。</summary>
		[Description("@#type")]
		public extern string Type { get; }

		/// <summary>Gets the localized text for this part. 获取此 part 的本地化文本。</summary>
		[Description("@#value")]
		public extern string Value { get; }
	}

	/// <summary>
/// JavaScript object shape returned by Intl <c>formatRangeToParts()</c> APIs.
/// In addition to <c>type</c> and <c>value</c>, the runtime exposes a <c>source</c> field telling whether a part came from the start range, end range, or a shared section.
/// Intl <c>formatRangeToParts()</c> API 返回的 JavaScript 对象形状；除 <c>type</c>、<c>value</c> 外，运行时还通过 <c>source</c> 表示 part 来自起始、结束或共享区段。
	/// </summary>
	[Description("@#")]
	public sealed class RangeFormatPart
	{
		/// <summary>Gets the semantic part type. 获取语义 part 类型。</summary>
		[Description("@#type")]
		public extern string Type { get; }

		/// <summary>Gets the localized text for this part. 获取此 part 的本地化文本。</summary>
		[Description("@#value")]
		public extern string Value { get; }

		/// <summary>Gets whether this part comes from the start, end, or shared range section. 获取此 part 来自起始、结束或共享范围区段。</summary>
		[Description("@#source")]
		public extern string Source { get; }
	}

	/// <summary>
/// Canonicalizes a locale identifier using the JavaScript <c>Intl</c> host.
/// 使用 JavaScript <c>Intl</c> 宿主规范化 locale 标识符。
	/// </summary>
	[Description("@#getCanonicalLocales")]
	public static extern string[] GetCanonicalLocales(string locales);

	/// <summary>
/// Canonicalizes a locale list using the JavaScript <c>Intl</c> host.
/// Locale lists use <see cref="IEnumerable{T}"/> so C# sequence families map to JavaScript locale-list input.
/// 使用 JavaScript <c>Intl</c> 宿主规范化 locale 列表；<see cref="IEnumerable{T}"/> 使 C# 序列族可映射到 JavaScript locale-list 输入。
	/// </summary>
	[Description("@#getCanonicalLocales")]
	public static extern string[] GetCanonicalLocales(IEnumerable<string> locales);

	/// <summary>
/// Returns values currently supported by the runtime for an ECMA-402 key such as <c>calendar</c>, <c>collation</c>, or <c>timeZone</c>.
/// 返回运行时当前支持的 ECMA-402 键值，例如 <c>calendar</c>、<c>collation</c> 或 <c>timeZone</c>。
	/// </summary>
	[Description("@#supportedValuesOf")]
	public static extern string[] SupportedValuesOf(string key);

	/// <summary>
/// Specifies an <c>Intl.Collator</c> usage: <c>"sort"</c> or <c>"search"</c>.
/// 指定 <c>Intl.Collator</c> 用途：<c>"sort"</c> 或 <c>"search"</c>。
	/// </summary>
	[String]
	[Description("@#")]
	public enum CollatorUsage
	{
		[Description("@#sort")]
		Sort,

		[Description("@#search")]
		Search
	}

	/// <summary>
/// Selects locale matching: <c>"lookup"</c> or <c>"best fit"</c>.
/// 选择本地化匹配算法：<c>"lookup"</c> 或 <c>"best fit"</c>。
	/// </summary>
	[String]
	[Description("@#")]
	public enum LocaleMatcher
	{
		[Description("@#lookup")]
		Lookup,
		[Description("@#best fit")]
		BestFit
	}

	/// <summary>
/// Controls uppercase/lowercase collation ordering.
/// 控制大小写字母的排序顺序。
	/// </summary>
	[String]
	[Description("@#")]
	public enum CaseFirst
	{
		[Description("@#upper")]
		Upper,
		[Description("@#lower")]
		Lower,
		[Description("@#false")]
		False
	}

	/// <summary>
/// Specifies collation sensitivity: base, accent, case, or variant.
/// 指定比较敏感度级别：base、accent、case 或 variant。
	/// </summary>
	[String]
	[Description("@#")]
	public enum Sensitivity
	{
		[Description("@#base")]
		Base,
		[Description("@#accent")]
		Accent,
		[Description("@#case")]
		Case,
		[Description("@#variant")]
		Variant
	}

	/// <summary>
/// Specifies a locale-specific collation algorithm supported by the JavaScript runtime.
/// Omit the option to let locale defaults apply.
/// 指定 JavaScript 运行时支持的 locale 特定排序算法；省略时使用 locale 默认规则。
	/// </summary>
	[String]
	[Description("@#")]
	public enum Collation
	{
		[Description("@#big5han")]
		Big5han,
		[Description("@#compat")]
		Compat,
		[Description("@#dict")]
		Dict,
		[Description("@#direct")]
		Direct,
		[Description("@#ducet")]
		Ducet,
		[Description("@#emoji")]
		Emoji,
		[Description("@#eor")]
		Eor,
		[Description("@#gb2312")]
		Gb2312,
		[Description("@#phonebk")]
		Phonebk,
		[Description("@#phonetic")]
		Phonetic,
		[Description("@#pinyin")]
		Pinyin,
		[Description("@#reformed")]
		Reformed,
		[Description("@#searchjl")]
		Searchjl,
		[Description("@#stroke")]
		Stroke,
		[Description("@#trad")]
		Trad,
		[Description("@#unihan")]
		Unihan,
		[Description("@#zhuyin")]
		Zhuyin
	}

	/// <summary>
/// Configuration for <c>Intl.Collator</c> string comparison, sorting, or searching.
/// <c>Intl.Collator</c> 字符串比较、排序或搜索的配置。
	/// </summary>
	/// <param name="Usage">Sort or search intent. 排序或搜索用途。</param>
	/// <param name="LocaleMatcher">Locale selection algorithm. locale 选择算法。</param>
	/// <param name="Numeric">Whether digit runs compare numerically. 数字串是否按数值比较。</param>
	/// <param name="CaseFirst">Uppercase/lowercase ordering preference. 大小写排序优先级。</param>
	/// <param name="Sensitivity">Base, accent, case, or variant sensitivity. base、accent、case 或 variant 敏感度。</param>
	/// <param name="Collation">Locale-specific collation algorithm. locale 特定排序算法。</param>
	/// <param name="IgnorePunctuation">Whether punctuation participates in comparison. 标点是否参与比较。</param>
	[Description("@#")]
	public record CollatorOptions(
		[property: Description("@#usage")] CollatorUsage? Usage,
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher,
		[property: Description("@#numeric")] bool? Numeric,
		[property: Description("@#caseFirst")] CaseFirst? CaseFirst,
		[property: Description("@#sensitivity")] Sensitivity? Sensitivity,
		[property: Description("@#collation")] Collation? Collation,
		[property: Description("@#ignorePunctuation")] bool? IgnorePunctuation);

	/// <summary>
/// Resolved runtime options returned by <c>Intl.Collator.prototype.resolvedOptions()</c>.
/// <c>Intl.Collator.prototype.resolvedOptions()</c> 返回的运行时解析后选项。
	/// </summary>
	/// <param name="Locale">Resolved locale. 解析后的 locale。</param>
	/// <param name="usage">Resolved usage. 解析后的用途。</param>
	/// <param name="sensitivity">Resolved sensitivity. 解析后的敏感度。</param>
	/// <param name="ignorePunctuation">Resolved punctuation handling. 解析后的标点处理方式。</param>
	/// <param name="collation">Resolved collation. 解析后的排序规则。</param>
	/// <param name="caseFirst">Resolved case ordering. 解析后的大小写排序。</param>
	/// <param name="numeric">Resolved numeric comparison setting. 解析后的数值比较设置。</param>
	[Description("@#")]
	public record ResolvedCollatorOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#usage")] CollatorUsage usage,
		[property: Description("@#sensitivity")] Sensitivity sensitivity,
		[property: Description("@#ignorePunctuation")] bool ignorePunctuation,
		[property: Description("@#collation")] Collation collation,
		[property: Description("@#caseFirst")] CaseFirst caseFirst,
		[property: Description("@#numeric")] bool numeric);

	/// <summary>JavaScript <c>Intl.Collator</c> constructor host. JavaScript <c>Intl.Collator</c> 构造器宿主。</summary>
	[Description("@#Collator")]
	public class Collator
	{
		/// <summary>
/// Gets JavaScript <c>Intl.Collator.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.Collator.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static Collator Prototype { get; }

		/// <summary>Creates a collator using the runtime default locale and options. 使用运行时默认 locale 和选项创建比较器。</summary>
		public extern Collator();

		/// <summary>
/// Creates a collator for a locale identifier. 为 locale 标识符创建比较器。
		/// </summary>
		public extern Collator(string locales);

		/// <summary>Creates a collator for a locale list. 为 locale 列表创建比较器。</summary>
		public extern Collator(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法在构造调用中自然表达省略前导可选参数。
		/// </summary>
		public extern Collator(CollatorOptions options);

		/// <summary>Creates a collator for a locale and options. 为 locale 和 options 创建比较器。</summary>
		public extern Collator(string locales, CollatorOptions options);

		/// <summary>Creates a collator for a locale list and options. 为 locale 列表和 options 创建比较器。</summary>
		public extern Collator(IEnumerable<string> locales, CollatorOptions options);

		/// <summary>Returns supported locales from one locale identifier. 从单个 locale 标识符返回受支持 locale。</summary>
		[Description("@#supportedLocalesOf")]
		/// <summary>Returns supported locales for a locale identifier. 返回单个 locale 标识符受支持的 locale。</summary>
		public static extern string[] SupportedLocalesOf(string locales);

		/// <summary>Returns supported locales from a locale list. 从 locale 列表返回受支持 locale。</summary>
		[Description("@#supportedLocalesOf")]
		/// <summary>Returns supported locales for a locale list. 返回 locale 列表中受支持的 locale。</summary>
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		/// <summary>Returns locales supported under the supplied collation options. 返回给定比较选项下受支持的 locale。</summary>
		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, CollatorOptions options);

		/// <summary>Returns a locale-list subset supported under the supplied collation options. 返回给定比较选项下受支持的 locale 列表子集。</summary>
		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, CollatorOptions options);

		/// <summary>Compares two strings using resolved locale collation rules. 使用解析后的 locale 排序规则比较两个字符串。</summary>
		[Description("@#compare")]
		public virtual extern Number Compare(string x, string y);

		/// <summary>Returns the runtime-resolved options. 返回运行时解析后的选项。</summary>
		[Description("@#resolvedOptions")]
		public virtual extern ResolvedCollatorOptions ResolvedOptions();
	}

	[String]
	[Description("@#")]
	/// <summary>Output style for <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 的输出样式。</summary>
	public enum NumberFormatOptionsStyle
	{
		[Description("@#decimal")]
		Decimal,  // 十进制格式（默认数字格式）
		[Description("@#percent")]
		Percent,  // 百分比格式（如 0.5 → "50%"）
		[Description("@#currency")]
		Currency,  // 货币格式（如 1000 → "$1,000.00"）
		[Description("@#unit")]
		Unit
	}

	[String]
	[Description("@#")]
	/// <summary>Currency display form for <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 的货币显示形式。</summary>
	public enum NumberFormatOptionsCurrencyDisplay
	{
		[Description("@#code")]
		Code,
		[Description("@#symbol")]
		Symbol,
		[Description("@#narrowSymbol")]
		NarrowSymbol,
		[Description("@#name")]
		Name
	}

	[String]
	[Description("@#")]
	/// <summary>Currency-sign treatment for <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 的货币符号处理方式。</summary>
	public enum NumberFormatCurrencySign
	{
		[Description("@#standard")]
		Standard,
		[Description("@#accounting")]
		Accounting
	}

	[String]
	[Description("@#")]
	/// <summary>Number notation style for <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 的数值记法样式。</summary>
	public enum NumberFormatNotation
	{
		[Description("@#standard")]
		Standard,
		[Description("@#scientific")]
		Scientific,
		[Description("@#engineering")]
		Engineering,
		[Description("@#compact")]
		Compact
	}

	[String]
	[Description("@#")]
	/// <summary>Display width for compact notation. 紧凑记法的显示宽度。</summary>
	public enum CompactDisplay
	{
		[Description("@#short")]
		Short,
		[Description("@#long")]
		Long
	}

	[String]
	[Description("@#")]
	/// <summary>Sign display policy for <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 的符号显示策略。</summary>
	public enum NumberFormatSignDisplay
	{
		[Description("@#auto")]
		Auto,
		[Description("@#never")]
		Never,
		[Description("@#always")]
		Always,
		[Description("@#exceptZero")]
		ExceptZero,
		[Description("@#negative")]
		Negative
	}

	[String]
	[Description("@#")]
	/// <summary>Grouping mode for <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 的分组模式。</summary>
	public enum NumberFormatUseGrouping
	{
		[Description("@#auto")]
		Auto,
		[Description("@#always")]
		Always,
		[Description("@#min2")]
		Min2
	}

	[String]
	[Description("@#")]
	/// <summary>Rounding algorithm for ECMA-402 formatting. ECMA-402 格式化舍入算法。</summary>
	public enum RoundingMode
	{
		[Description("@#ceil")]
		Ceil,
		[Description("@#floor")]
		Floor,
		[Description("@#expand")]
		Expand,
		[Description("@#trunc")]
		Trunc,
		[Description("@#halfCeil")]
		HalfCeil,
		[Description("@#halfFloor")]
		HalfFloor,
		[Description("@#halfExpand")]
		HalfExpand,
		[Description("@#halfTrunc")]
		HalfTrunc,
		[Description("@#halfEven")]
		HalfEven
	}

	[String]
	[Description("@#")]
	/// <summary>Priority when fraction and significant-digit options coexist. 小数位和有效位选项共存时的优先级。</summary>
	public enum RoundingPriority
	{
		[Description("@#auto")]
		Auto,
		[Description("@#morePrecision")]
		MorePrecision,
		[Description("@#lessPrecision")]
		LessPrecision
	}

	[String]
	[Description("@#")]
	/// <summary>Trailing-zero display policy. 尾随零显示策略。</summary>
	public enum TrailingZeroDisplay
	{
		[Description("@#auto")]
		Auto,
		[Description("@#stripIfInteger")]
		StripIfInteger
	}

	/// <summary>Configuration object for <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 的配置对象。</summary>
	[Description("@#")]
	public record NumberFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#numberingSystem")] string? NumberingSystem = null,
		[property: Description("@#style")] NumberFormatOptionsStyle? Style = null,
		[property: Description("@#currency")] string? Currency = null,
		[property: Description("@#currencyDisplay")] NumberFormatOptionsCurrencyDisplay? CurrencyDisplay = null,
		[property: Description("@#currencySign")] NumberFormatCurrencySign? CurrencySign = null,
		[property: Description("@#unit")] string? Unit = null,
		[property: Description("@#unitDisplay")] LongShortNarrow? UnitDisplay = null,
		[property: Description("@#notation")] NumberFormatNotation? Notation = null,
		[property: Description("@#compactDisplay")] CompactDisplay? CompactDisplay = null,
		/// <summary>
/// JavaScript accepts Boolean and string modes here.
/// The union keeps the public host close to the runtime surface without forcing callers through CLR-only helper wrappers.
/// JavaScript 在此接受布尔值和字符串模式；联合类型使公开宿主接近运行时表面，无需调用方通过仅 CLR 的辅助包装器。
		/// </summary>
		[property: Description("@#useGrouping")] IntlUseGrouping? UseGrouping = null,
		[property: Description("@#signDisplay")] NumberFormatSignDisplay? SignDisplay = null,
		[property: Description("@#minimumIntegerDigits")] Number? MinimumIntegerDigits = null,
		[property: Description("@#minimumFractionDigits")] Number? MinimumFractionDigits = null,
		[property: Description("@#maximumFractionDigits")] Number? MaximumFractionDigits = null,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits = null,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits = null,
		[property: Description("@#roundingIncrement")] Number? RoundingIncrement = null,
		[property: Description("@#roundingMode")] RoundingMode? RoundingMode = null,
		[property: Description("@#roundingPriority")] RoundingPriority? RoundingPriority = null,
		[property: Description("@#trailingZeroDisplay")] TrailingZeroDisplay? TrailingZeroDisplay = null);

	/// <summary>Resolved options returned by <c>Intl.NumberFormat</c>. <c>Intl.NumberFormat</c> 返回的解析后选项。</summary>
	[Description("@#")]
	public record ResolvedNumberFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#numberingSystem")] string NumberingSystem,
		[property: Description("@#style")] NumberFormatOptionsStyle Style,
		[property: Description("@#currency")] string? Currency,
		[property: Description("@#currencyDisplay")] NumberFormatOptionsCurrencyDisplay? CurrencyDisplay,
		[property: Description("@#currencySign")] NumberFormatCurrencySign? CurrencySign,
		[property: Description("@#unit")] string? Unit,
		[property: Description("@#unitDisplay")] LongShortNarrow? UnitDisplay,
		[property: Description("@#minimumIntegerDigits")] Number MinimumIntegerDigits,
		[property: Description("@#minimumFractionDigits")] Number MinimumFractionDigits,
		[property: Description("@#maximumFractionDigits")] Number MaximumFractionDigits,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits,
		[property: Description("@#useGrouping")] IntlUseGrouping UseGrouping,
		[property: Description("@#notation")] NumberFormatNotation Notation,
		[property: Description("@#compactDisplay")] CompactDisplay? CompactDisplay,
		[property: Description("@#signDisplay")] NumberFormatSignDisplay SignDisplay,
		[property: Description("@#roundingIncrement")] Number RoundingIncrement,
		[property: Description("@#roundingMode")] RoundingMode RoundingMode,
		[property: Description("@#roundingPriority")] RoundingPriority RoundingPriority,
		[property: Description("@#trailingZeroDisplay")] TrailingZeroDisplay TrailingZeroDisplay);

	/// <summary>JavaScript <c>Intl.NumberFormat</c> constructor host. JavaScript <c>Intl.NumberFormat</c> 构造器宿主。</summary>
	[Description("@#NumberFormat")]
    public sealed class NumberFormat : IFormatProvider
    {
		/// <summary>
/// Gets JavaScript <c>Intl.NumberFormat.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.NumberFormat.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static NumberFormat Prototype { get; }

		/// <summary>Creates a number formatter using runtime defaults. 使用运行时默认设置创建数值格式化器。</summary>
        public extern NumberFormat();

        /// <summary>
/// Creates a number formatter for a locale identifier. 为 locale 标识符创建数值格式化器。
        /// </summary>
        public extern NumberFormat(string locales);

		/// <summary>Creates a number formatter for a locale list. 为 locale 列表创建数值格式化器。</summary>
        public extern NumberFormat(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern NumberFormat(NumberFormatOptions options);

		/// <summary>Creates a number formatter for a locale and options. 为 locale 和 options 创建数值格式化器。</summary>
        public extern NumberFormat(string locales, NumberFormatOptions options);

		/// <summary>Creates a number formatter for a locale list and options. 为 locale 列表和 options 创建数值格式化器。</summary>
        public extern NumberFormat(IEnumerable<string> locales, NumberFormatOptions options);

		/// <summary>Returns supported locales for a locale identifier. 返回单个 locale 标识符受支持的 locale。</summary>
        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(string locales);

		/// <summary>Returns supported locales for a locale list. 返回 locale 列表中受支持的 locale。</summary>
        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		/// <summary>Returns locales supported with number-format options. 返回数值格式选项下受支持的 locale。</summary>
        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(string locales, NumberFormatOptions options);

		/// <summary>Returns locale-list entries supported with number-format options. 返回数值格式选项下受支持的 locale 列表项。</summary>
        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, NumberFormatOptions options);

		/// <summary>Formats a JavaScript number as localized text. 将 JavaScript Number 格式化为本地化文本。</summary>
		[Description("@#format")]
		public extern string Format(Number value);

		/// <summary>
/// Formats a JavaScript bigint without forcing callers through a CLR number conversion that JavaScript itself does not require.
/// 格式化 JavaScript BigInt，无需调用方执行 JavaScript 本身不要求的 CLR 数值转换。
		/// </summary>
		[Description("@#format")]
		public extern string Format(BigInt value);

		/// <summary>
/// Formats any JavaScript mathematical value accepted by <c>Intl.NumberFormat</c>.
/// The union keeps the public host aligned with JavaScript's runtime coercion surface for numbers, bigints, and decimal strings.
/// 格式化 <c>Intl.NumberFormat</c> 接受的任意 JavaScript 数学值；联合类型与 JavaScript 对 Number、BigInt、十进制字符串的运行时转换表面保持一致。
		/// </summary>
		[Description("@#format")]
		public extern string Format(IntlNumberInput value);

		/// <summary>
/// Returns the localized number as JavaScript part records instead of a single concatenated string.
/// 将本地化 Number 返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public extern Array<FormatPart> FormatToParts(Number value);

		/// <summary>
/// Returns the localized bigint as JavaScript part records instead of a single concatenated string.
/// 将本地化 BigInt 返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public extern Array<FormatPart> FormatToParts(BigInt value);

		/// <summary>
/// Returns the localized mathematical value as JavaScript part records instead of a single concatenated string.
/// 将本地化数学值返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public extern Array<FormatPart> FormatToParts(IntlNumberInput value);

		/// <summary>
/// Returns a localized string representing a numeric range.
/// This stays on the <c>Intl.NumberFormat</c> host because JavaScript exposes it as an instance method there.
/// 返回表示 Number 范围的本地化字符串；保留在 <c>Intl.NumberFormat</c> 宿主上，因为 JavaScript 将其公开为实例方法。
		/// </summary>
		[Description("@#formatRange")]
		public extern string FormatRange(Number start, Number end);

		/// <summary>
/// Returns a localized string representing a bigint range.
/// 返回表示 BigInt 范围的本地化字符串。
		/// </summary>
		[Description("@#formatRange")]
		public extern string FormatRange(BigInt start, BigInt end);

		/// <summary>
/// Returns a localized string representing a JavaScript mathematical-value range.
/// The union keeps the public host aligned with JavaScript's runtime coercion surface for numbers, bigints, and decimal strings.
/// 返回表示 JavaScript 数学值范围的本地化字符串；联合类型与 JavaScript 对 Number、BigInt、十进制字符串的运行时转换表面保持一致。
		/// </summary>
		[Description("@#formatRange")]
		public extern string FormatRange(IntlNumberInput start, IntlNumberInput end);

		/// <summary>
/// Returns a localized numeric range as JavaScript part records instead of a single concatenated string.
/// 将本地化 Number 范围返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatRangeToParts")]
		public extern Array<RangeFormatPart> FormatRangeToParts(Number start, Number end);

		/// <summary>
/// Returns a localized bigint range as JavaScript part records instead of a single concatenated string.
/// 将本地化 BigInt 范围返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatRangeToParts")]
		public extern Array<RangeFormatPart> FormatRangeToParts(BigInt start, BigInt end);

		/// <summary>
/// Returns a localized mathematical-value range as JavaScript part records instead of a single concatenated string.
/// 将本地化数学值范围返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatRangeToParts")]
		public extern Array<RangeFormatPart> FormatRangeToParts(IntlNumberInput start, IntlNumberInput end);

		/// <summary>Returns the runtime-resolved number-format options. 返回运行时解析后的数值格式选项。</summary>
        [Description("@#resolvedOptions")]
        public extern ResolvedNumberFormatOptions ResolvedOptions();

		/// <summary>CLR <see cref="IFormatProvider"/> bridge; runtime behavior is JavaScript-host-defined. CLR <see cref="IFormatProvider"/> 桥接；运行时行为由 JavaScript 宿主定义。</summary>
        public extern object? GetFormat(Type? formatType);
    }

	/// <summary>JavaScript <c>Intl.Locale</c> constructor host. JavaScript <c>Intl.Locale</c> 构造器宿主。</summary>
	[Description("@#Locale")]
	public sealed class Locale
	{
		/// <summary>
/// Gets JavaScript <c>Intl.Locale.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.Locale.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static Locale Prototype { get; }

		/// <summary>Creates a locale from a BCP 47 language tag. 从 BCP 47 语言标记创建 locale。</summary>
		public extern Locale(string tag);

		/// <summary>
/// Creates a locale while overriding individual Unicode locale components through the JavaScript options bag.
/// 通过 JavaScript options bag 覆盖单独的 Unicode locale 组件并创建 locale。
		/// </summary>
		public extern Locale(string tag, LocaleOptions options);

		/// <summary>Returns a locale expanded with likely subtags. 返回补充 likely subtags 的 locale。</summary>
		[Description("@#maximize")]
		public extern Locale Maximize();

		/// <summary>
/// Returns a locale with likely-subtag information removed where possible.
/// This stays on the <c>Intl.Locale</c> host to match the JavaScript runtime object directly.
/// 返回尽可能移除 likely-subtag 信息的 locale；保留在 <c>Intl.Locale</c> 宿主上以直接对应 JavaScript 运行时对象。
		/// </summary>
		[Description("@#minimize")]
		public extern Locale Minimize();

		/// <summary>Gets the language subtag. 获取语言子标记。</summary>
		[Description("@#language")]
		public extern string Language { get; }

		/// <summary>Gets the Unicode calendar extension, when present. 获取存在时的 Unicode calendar 扩展。</summary>
		[Description("@#calendar")]
		public extern string? Calendar { get; }

		/// <summary>Gets the case-first collation extension, when present. 获取存在时的 case-first 排序扩展。</summary>
		[Description("@#caseFirst")]
		public extern CaseFirst? CaseFirst { get; }

		/// <summary>Gets the collation extension, when present. 获取存在时的排序扩展。</summary>
		[Description("@#collation")]
		public extern string? Collation { get; }

		/// <summary>
/// Gets the canonical Unicode first-day identifier such as <c>mon</c> or <c>sun</c>.
/// 获取规范 Unicode 首日标识符，例如 <c>mon</c> 或 <c>sun</c>。
		/// </summary>
		[Description("@#firstDayOfWeek")]
		public extern string? FirstDayOfWeek { get; }

		/// <summary>Gets the hour-cycle extension, when present. 获取存在时的小时制扩展。</summary>
		[Description("@#hourCycle")]
		public extern HourCycle? HourCycle { get; }

		/// <summary>Gets the script subtag, when present. 获取存在时的文字子标记。</summary>
		[Description("@#script")]
		public extern string? Script { get; }

		/// <summary>
/// Gets canonical variant subtags carried by this locale.
/// JavaScript exposes the canonicalized variant sequence as a single string and omits it when no variants exist, so the C# projection stays nullable.
/// 获取此 locale 携带的规范 variant 子标记；JavaScript 将规范化序列公开为单个字符串，无 variant 时省略，因此 C# 投影保持可空。
		/// </summary>
		[Description("@#variants")]
		public extern string? Variants { get; }

		/// <summary>Gets the numbering-system extension, when present. 获取存在时的编号系统扩展。</summary>
		[Description("@#numberingSystem")]
		public extern string? NumberingSystem { get; }

		/// <summary>Gets the numeric collation extension, when present. 获取存在时的数值排序扩展。</summary>
		[Description("@#numeric")]
		public extern bool? Numeric { get; }

		/// <summary>Gets the region subtag, when present. 获取存在时的地区子标记。</summary>
		[Description("@#region")]
		public extern string? Region { get; }

		/// <summary>Gets the locale base name without Unicode extension keywords. 获取不含 Unicode 扩展关键字的 locale 基本名称。</summary>
		[Description("@#baseName")]
		public extern string BaseName { get; }

		/// <summary>
/// Returns runtime-supported calendar identifiers for this locale.
/// 返回此 locale 运行时支持的 calendar 标识符。
		/// </summary>
		[Description("@#getCalendars")]
		public extern string[] GetCalendars();

		/// <summary>
/// Returns runtime-supported collation identifiers for this locale.
/// 返回此 locale 运行时支持的排序标识符。
		/// </summary>
		[Description("@#getCollations")]
		public extern string[] GetCollations();

		/// <summary>
/// Returns runtime-supported hour cycles for this locale.
/// 返回此 locale 运行时支持的小时制。
		/// </summary>
		[Description("@#getHourCycles")]
		public extern string[] GetHourCycles();

		/// <summary>
/// Returns runtime-supported numbering systems for this locale.
/// 返回此 locale 运行时支持的编号系统。
		/// </summary>
		[Description("@#getNumberingSystems")]
		public extern string[] GetNumberingSystems();

		/// <summary>
/// Returns runtime-supported time zones for this locale.
/// 返回此 locale 运行时支持的时区。
		/// </summary>
		[Description("@#getTimeZones")]
		public extern string[] GetTimeZones();

		/// <summary>
/// Returns the JavaScript text-info record for this locale.
/// 返回此 locale 的 JavaScript 文本信息记录。
		/// </summary>
		[Description("@#getTextInfo")]
		public extern LocaleTextInfo GetTextInfo();

		/// <summary>
/// Returns the JavaScript week-info record for this locale.
/// 返回此 locale 的 JavaScript 周信息记录。
		/// </summary>
		[Description("@#getWeekInfo")]
		public extern LocaleWeekInfo GetWeekInfo();

		/// <summary>Returns the canonical locale string. 返回规范 locale 字符串。</summary>
		[Description("@#toString")]
		public extern override string ToString();
	}

	[String]
	[Description("@#")]
	/// <summary>Unicode hour-cycle identifiers. Unicode 小时制标识符。</summary>
	public enum HourCycle
	{
		[Description("@#h11")]
		H11,
		[Description("@#h12")]
		H12,
		[Description("@#h23")]
		H23,
		[Description("@#h24")]
		H24
	}

	/// <summary>
/// JavaScript options bag for <c>Intl.Locale</c>.
/// JavaScript <c>Intl.Locale</c> 的 options bag。
	/// </summary>
	[Description("@#")]
	public record LocaleOptions(
		[property: Description("@#language")] string? Language = null,
		[property: Description("@#script")] string? Script = null,
		[property: Description("@#region")] string? Region = null,
		[property: Description("@#variants")] string? Variants = null,
		[property: Description("@#calendar")] string? Calendar = null,
		[property: Description("@#collation")] string? Collation = null,
		[property: Description("@#firstDayOfWeek")] string? FirstDayOfWeek = null,
		[property: Description("@#hourCycle")] HourCycle? HourCycle = null,
		[property: Description("@#caseFirst")] CaseFirst? CaseFirst = null,
		[property: Description("@#numeric")] bool? Numeric = null,
		[property: Description("@#numberingSystem")] string? NumberingSystem = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.Locale.prototype.getTextInfo()</c>.
/// <c>Intl.Locale.prototype.getTextInfo()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public sealed class LocaleTextInfo
	{
		/// <summary>Gets text direction, such as <c>ltr</c> or <c>rtl</c>. 获取文本方向，例如 <c>ltr</c> 或 <c>rtl</c>。</summary>
		[Description("@#direction")]
		public extern string Direction { get; }
	}

	/// <summary>
/// JavaScript object shape returned by <c>Intl.Locale.prototype.getWeekInfo()</c>.
/// <c>Intl.Locale.prototype.getWeekInfo()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public sealed class LocaleWeekInfo
	{
		/// <summary>Gets the locale first day of week. 获取 locale 一周的首日。</summary>
		[Description("@#firstDay")]
		public extern string FirstDay { get; }

		/// <summary>Gets locale weekend day identifiers. 获取 locale 周末日标识符。</summary>
		[Description("@#weekend")]
		public extern string[] Weekend { get; }
	}

	[String]
	[Description("@#")]
	/// <summary>Date/time format-matching algorithm. 日期时间格式匹配算法。</summary>
	public enum FormatMatcher
	{
		[Description("@#best fit")]
		BestFit,
		[Description("@#basic")]
		Basic
	}

	[String]
	[Description("@#")]
	/// <summary>Long, short, and narrow localized display widths. long、short 和 narrow 的本地化显示宽度。</summary>
	public enum LongShortNarrow
	{
		[Description("@#long")]
		Long,
		[Description("@#short")]
		Short,
		[Description("@#narrow")]
		Narrow
	}

	[String]
	[Description("@#")]
	/// <summary>Numeric or two-digit date/time field style. 数值或两位数字的日期时间字段样式。</summary>
	public enum NumericTwoDigit
	{
		[Description("@#numeric")]
		Numeric,
		[Description("@#2-digit")]
		TwoDigit
	}

	[String]
	[Description("@#")]
	/// <summary>Date/time preset style. 日期时间预设样式。</summary>
	public enum DateTimeStyle
	{
		[Description("@#full")]
		Full,
		[Description("@#long")]
		Long,
		[Description("@#medium")]
		Medium,
		[Description("@#short")]
		Short
	}

	[String]
	[Description("@#")]
	/// <summary>Time-zone name display style. 时区名称显示样式。</summary>
	public enum TimeZoneName
	{
		[Description("@#short")]
		Short,
		[Description("@#long")]
		Long,
		[Description("@#shortOffset")]
		ShortOffset,
		[Description("@#longOffset")]
		LongOffset,
		[Description("@#shortGeneric")]
		ShortGeneric,
		[Description("@#longGeneric")]
		LongGeneric
	}

	/// <summary>Fractional-second precision from one through three digits. 一至三位的小数秒精度。</summary>
	[Description("@#")]
	public enum FractionalSecondDigits
	{
		[Description("@#1")]
		One = 1,
		[Description("@#2")]
		Two = 2,
		[Description("@#3")]
		Three = 3
	}

	/// <summary>Configuration object for <c>Intl.DateTimeFormat</c>. <c>Intl.DateTimeFormat</c> 的配置对象。</summary>
	[Description("@#")]
	public record DateTimeFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#calendar")] string? Calendar = null,
		[property: Description("@#numberingSystem")] string? NumberingSystem = null,
		[property: Description("@#weekday")] LongShortNarrow? Weekday = null,
		[property: Description("@#era")] LongShortNarrow? Era = null,
		[property: Description("@#year")] NumericTwoDigit? Year = null,
		[property: Description("@#month")] IntlMonthStyle? Month = null,
		[property: Description("@#day")] NumericTwoDigit? Day = null,
		[property: Description("@#dayPeriod")] LongShortNarrow? DayPeriod = null,
		[property: Description("@#hour")] NumericTwoDigit? Hour = null,
		[property: Description("@#minute")] NumericTwoDigit? Minute = null,
		[property: Description("@#second")] NumericTwoDigit? Second = null,
		[property: Description("@#fractionalSecondDigits")] FractionalSecondDigits? FractionalSecondDigits = null,
		[property: Description("@#timeZoneName")] TimeZoneName? TimeZoneName = null,
		[property: Description("@#formatMatcher")] FormatMatcher? FormatMatcher = null,
		[property: Description("@#hour12")] bool? Hour12 = null,
		[property: Description("@#hourCycle")] HourCycle? HourCycle = null,
		[property: Description("@#dateStyle")] DateTimeStyle? DateStyle = null,
		[property: Description("@#timeStyle")] DateTimeStyle? TimeStyle = null,
		[property: Description("@#timeZone")] string? TimeZone = null);

	/// <summary>Resolved options returned by <c>Intl.DateTimeFormat</c>. <c>Intl.DateTimeFormat</c> 返回的解析后选项。</summary>
	[Description("@#")]
	public record ResolvedDateTimeFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#calendar")] string Calendar,
		[property: Description("@#numberingSystem")] string NumberingSystem,
		[property: Description("@#timeZone")] string TimeZone,
		[property: Description("@#hourCycle")] HourCycle? HourCycle = null,
		[property: Description("@#hour12")] bool? Hour12 = null,
		[property: Description("@#weekday")] string? Weekday = null,
		[property: Description("@#era")] string? Era = null,
		[property: Description("@#year")] string? Year = null,
		[property: Description("@#month")] string? Month = null,
		[property: Description("@#day")] string? Day = null,
		[property: Description("@#dayPeriod")] string? DayPeriod = null,
		[property: Description("@#hour")] string? Hour = null,
		[property: Description("@#minute")] string? Minute = null,
		[property: Description("@#second")] string? Second = null,
		[property: Description("@#fractionalSecondDigits")] Number? FractionalSecondDigits = null,
		[property: Description("@#timeZoneName")] string? TimeZoneName = null,
		[property: Description("@#dateStyle")] DateTimeStyle? DateStyle = null,
		[property: Description("@#timeStyle")] DateTimeStyle? TimeStyle = null);

	/// <summary>JavaScript <c>Intl.DateTimeFormat</c> constructor host. JavaScript <c>Intl.DateTimeFormat</c> 构造器宿主。</summary>
	[Description("@#DateTimeFormat")]
	public class DateTimeFormat
	{
		/// <summary>
/// Gets JavaScript <c>Intl.DateTimeFormat.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.DateTimeFormat.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static DateTimeFormat Prototype { get; }

		/// <summary>Creates a date/time formatter using runtime defaults. 使用运行时默认设置创建日期时间格式化器。</summary>
		public extern DateTimeFormat();

		/// <summary>
/// Creates a date/time formatter for a locale identifier. 为 locale 标识符创建日期时间格式化器。
		/// </summary>
		public extern DateTimeFormat(string locales);

		/// <summary>Creates a date/time formatter for a locale list. 为 locale 列表创建日期时间格式化器。</summary>
		public extern DateTimeFormat(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern DateTimeFormat(DateTimeFormatOptions options);

		/// <summary>Creates a date/time formatter for a locale and options. 为 locale 和 options 创建日期时间格式化器。</summary>
		public extern DateTimeFormat(string locales, DateTimeFormatOptions options);

		/// <summary>Creates a date/time formatter for a locale list and options. 为 locale 列表和 options 创建日期时间格式化器。</summary>
		public extern DateTimeFormat(IEnumerable<string> locales, DateTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		[Description("@#supportedLocalesOf")]
		/// <summary>Returns locales supported with date/time options. 返回日期时间选项下受支持的 locale。</summary>
		public static extern string[] SupportedLocalesOf(string locales, DateTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		/// <summary>Returns locale-list entries supported with date/time options. 返回日期时间选项下受支持的 locale 列表项。</summary>
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, DateTimeFormatOptions options);

		[Description("@#format")]
		/// <summary>Formats the current time using JavaScript runtime time. 使用 JavaScript 运行时时间格式化当前时刻。</summary>
		public virtual extern string Format();

		[Description("@#format")]
		/// <summary>Formats a Date-or-timestamp input. 格式化 Date 或时间戳输入。</summary>
		public virtual extern string Format(IntlDateTimeInput date);

		[Description("@#format")]
		/// <summary>Formats a JavaScript Date object. 格式化 JavaScript Date 对象。</summary>
		public virtual extern string Format(Date date);

		[Description("@#format")]
		/// <summary>Formats a Unix-epoch millisecond timestamp. 格式化 Unix epoch 毫秒时间戳。</summary>
		public virtual extern string Format(Number date);

		/// <summary>
/// Returns the current time as JavaScript part records instead of a single concatenated string.
/// 将当前时刻返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts();

		/// <summary>
/// Returns a Date-or-timestamp input as JavaScript part records instead of a single concatenated string.
/// 将 Date 或时间戳输入返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts(IntlDateTimeInput date);

		/// <summary>
/// Returns a JavaScript Date as JavaScript part records instead of a single concatenated string.
/// 将 JavaScript Date 返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts(Date date);

		/// <summary>
/// Returns a millisecond timestamp as JavaScript part records instead of a single concatenated string.
/// 将毫秒时间戳返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts(Number date);

		/// <summary>
/// Returns a localized string representing a Date-or-timestamp range.
/// 返回表示 Date 或时间戳范围的本地化字符串。
		/// </summary>
		[Description("@#formatRange")]
		public virtual extern string FormatRange(IntlDateTimeInput startDate, IntlDateTimeInput endDate);

		/// <summary>
/// Returns a localized string representing a JavaScript Date range.
/// 返回表示 JavaScript Date 范围的本地化字符串。
		/// </summary>
		[Description("@#formatRange")]
		public virtual extern string FormatRange(Date startDate, Date endDate);

		/// <summary>
/// Returns a localized string representing a millisecond timestamp range.
/// 返回表示毫秒时间戳范围的本地化字符串。
		/// </summary>
		[Description("@#formatRange")]
		public virtual extern string FormatRange(Number startDate, Number endDate);

		/// <summary>
/// Returns a localized Date-or-timestamp range as JavaScript part records instead of a single concatenated string.
/// 将本地化 Date 或时间戳范围返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatRangeToParts")]
		public virtual extern Array<RangeFormatPart> FormatRangeToParts(IntlDateTimeInput startDate, IntlDateTimeInput endDate);

		/// <summary>
/// Returns a localized JavaScript Date range as JavaScript part records instead of a single concatenated string.
/// 将本地化 JavaScript Date 范围返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatRangeToParts")]
		public virtual extern Array<RangeFormatPart> FormatRangeToParts(Date startDate, Date endDate);

		/// <summary>
/// Returns a localized millisecond timestamp range as JavaScript part records instead of a single concatenated string.
/// 将本地化毫秒时间戳范围返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatRangeToParts")]
		public virtual extern Array<RangeFormatPart> FormatRangeToParts(Number startDate, Number endDate);

		[Description("@#resolvedOptions")]
		/// <summary>Returns runtime-resolved date/time options. 返回运行时解析后的日期时间选项。</summary>
		public virtual extern ResolvedDateTimeFormatOptions ResolvedOptions();
	}

	[String]
	[Description("@#")]
	/// <summary>Relative-time display width. 相对时间显示宽度。</summary>
	public enum RelativeTimeFormatStyle
	{
		[Description("@#long")]
		Long,
		[Description("@#short")]
		Short,
		[Description("@#narrow")]
		Narrow
	}

	[String]
	[Description("@#")]
	/// <summary>Relative-time numeric wording policy. 相对时间数值措辞策略。</summary>
	public enum RelativeTimeFormatNumeric
	{
		[Description("@#always")]
		Always,
		[Description("@#auto")]
		Auto
	}

	[String]
	[Description("@#")]
	/// <summary>Unit accepted by <c>Intl.RelativeTimeFormat</c>. <c>Intl.RelativeTimeFormat</c> 接受的单位。</summary>
	public enum RelativeTimeUnit
	{
		[Description("@#year")]
		Year,
		[Description("@#quarter")]
		Quarter,
		[Description("@#month")]
		Month,
		[Description("@#week")]
		Week,
		[Description("@#day")]
		Day,
		[Description("@#hour")]
		Hour,
		[Description("@#minute")]
		Minute,
		[Description("@#second")]
		Second
	}

	/// <summary>
/// JavaScript object shape returned by <c>Intl.RelativeTimeFormat.prototype.formatToParts()</c>.
/// Relative time parts may also carry the formatted unit that produced the part.
/// <c>Intl.RelativeTimeFormat.prototype.formatToParts()</c> 返回的 JavaScript 对象形状；相对时间 part 还可能携带生成它的格式化单位。
	/// </summary>
	[Description("@#")]
	public sealed class RelativeTimeFormatPart
	{
		/// <summary>Gets the semantic part type. 获取语义 part 类型。</summary>
		[Description("@#type")]
		public extern string Type { get; }

		/// <summary>Gets localized text for this part. 获取此 part 的本地化文本。</summary>
		[Description("@#value")]
		public extern string Value { get; }

		/// <summary>Gets the source unit when the part has one. 获取存在时的源单位。</summary>
		[Description("@#unit")]
		public extern string? Unit { get; }
	}

	/// <summary>
/// Configuration object for <c>Intl.RelativeTimeFormat</c>.
/// It stays explicit so callers can see JavaScript option names and value domains directly from C#.
/// <c>Intl.RelativeTimeFormat</c> 的配置对象；保持显式使调用方可从 C# 直接了解 JavaScript 选项名称和值域。
	/// </summary>
	[Description("@#")]
	public record RelativeTimeFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#numeric")] RelativeTimeFormatNumeric? Numeric = null,
		[property: Description("@#style")] RelativeTimeFormatStyle? Style = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.RelativeTimeFormat.prototype.resolvedOptions()</c>.
/// <c>Intl.RelativeTimeFormat.prototype.resolvedOptions()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public record ResolvedRelativeTimeFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#style")] RelativeTimeFormatStyle Style,
		[property: Description("@#numeric")] RelativeTimeFormatNumeric Numeric,
		[property: Description("@#numberingSystem")] string NumberingSystem);

	/// <summary>
/// Projection of JavaScript's <c>Intl.RelativeTimeFormat</c> constructor host.
/// JavaScript <c>Intl.RelativeTimeFormat</c> 构造器宿主投影。
	/// </summary>
	[Description("@#RelativeTimeFormat")]
	public sealed class RelativeTimeFormat
	{
		/// <summary>
/// Gets JavaScript <c>Intl.RelativeTimeFormat.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.RelativeTimeFormat.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static RelativeTimeFormat Prototype { get; }

		/// <summary>Creates a relative-time formatter using runtime defaults. 使用运行时默认设置创建相对时间格式化器。</summary>
		public extern RelativeTimeFormat();

		/// <summary>
/// Creates a relative-time formatter for a locale identifier. 为 locale 标识符创建相对时间格式化器。
		/// </summary>
		public extern RelativeTimeFormat(string locales);

		/// <summary>Creates a relative-time formatter for a locale list. 为 locale 列表创建相对时间格式化器。</summary>
		public extern RelativeTimeFormat(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern RelativeTimeFormat(RelativeTimeFormatOptions options);

		/// <summary>Creates a formatter for a locale and options. 为 locale 和 options 创建格式化器。</summary>
		public extern RelativeTimeFormat(string locales, RelativeTimeFormatOptions options);

		/// <summary>Creates a formatter for a locale list and options. 为 locale 列表和 options 创建格式化器。</summary>
		public extern RelativeTimeFormat(IEnumerable<string> locales, RelativeTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, RelativeTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, RelativeTimeFormatOptions options);

		[Description("@#format")]
		/// <summary>Formats a signed value relative to the supplied unit. 按给定单位格式化带符号的相对值。</summary>
		public extern string Format(Number value, RelativeTimeUnit unit);

		[Description("@#formatToParts")]
		/// <summary>Formats a relative value as JavaScript part records. 将相对值格式化为 JavaScript part 记录。</summary>
		public extern Array<RelativeTimeFormatPart> FormatToParts(Number value, RelativeTimeUnit unit);

		/// <summary>Returns runtime-resolved relative-time options. 返回运行时解析后的相对时间选项。</summary>
		[Description("@#resolvedOptions")]
		public extern ResolvedRelativeTimeFormatOptions ResolvedOptions();
	}

	[String]
	[Description("@#")]
	/// <summary>Duration output style. Duration 输出样式。</summary>
	public enum DurationFormatStyle
	{
		[Description("@#long")]
		Long,
		[Description("@#short")]
		Short,
		[Description("@#narrow")]
		Narrow,
		[Description("@#digital")]
		Digital
	}

	[String]
	[Description("@#")]
	/// <summary>Whether a duration unit is automatically omitted or always shown. Duration 单位自动省略或始终显示的策略。</summary>
	public enum DurationDisplay
	{
		[Description("@#auto")]
		Auto,
		[Description("@#always")]
		Always
	}

	/// <summary>
/// Numeric-only duration-unit style used by sub-second units.
/// This stays separate because JavaScript allows <c>numeric</c> but not <c>2-digit</c> for these units.
/// 用于次秒单位的仅数值 Duration 样式；JavaScript 对这些单位允许 <c>numeric</c> 而不允许 <c>2-digit</c>，故单独建模。
	/// </summary>
	[String]
	[Description("@#")]
	public enum DurationNumericStyle
	{
		[Description("@#numeric")]
		Numeric
	}

	/// <summary>
/// JavaScript options bag for <c>Intl.DurationFormat</c>.
/// Per-unit style properties mirror runtime option names directly; the runtime still validates legal combinations for each unit.
/// JavaScript <c>Intl.DurationFormat</c> 的 options bag；按单位的样式属性直接映射运行时选项名称，合法组合仍由运行时验证。
	/// </summary>
	[Description("@#")]
	public record DurationFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#numberingSystem")] string? NumberingSystem = null,
		[property: Description("@#style")] DurationFormatStyle? Style = null,
		[property: Description("@#years")] LongShortNarrow? Years = null,
		[property: Description("@#yearsDisplay")] DurationDisplay? YearsDisplay = null,
		[property: Description("@#months")] LongShortNarrow? Months = null,
		[property: Description("@#monthsDisplay")] DurationDisplay? MonthsDisplay = null,
		[property: Description("@#weeks")] LongShortNarrow? Weeks = null,
		[property: Description("@#weeksDisplay")] DurationDisplay? WeeksDisplay = null,
		[property: Description("@#days")] LongShortNarrow? Days = null,
		[property: Description("@#daysDisplay")] DurationDisplay? DaysDisplay = null,
		[property: Description("@#hours")] IntlDurationTextStyle? Hours = null,
		[property: Description("@#hoursDisplay")] DurationDisplay? HoursDisplay = null,
		[property: Description("@#minutes")] IntlDurationTextStyle? Minutes = null,
		[property: Description("@#minutesDisplay")] DurationDisplay? MinutesDisplay = null,
		[property: Description("@#seconds")] IntlDurationTextStyle? Seconds = null,
		[property: Description("@#secondsDisplay")] DurationDisplay? SecondsDisplay = null,
		[property: Description("@#milliseconds")] IntlDurationFractionStyle? Milliseconds = null,
		[property: Description("@#millisecondsDisplay")] DurationDisplay? MillisecondsDisplay = null,
		[property: Description("@#microseconds")] IntlDurationFractionStyle? Microseconds = null,
		[property: Description("@#microsecondsDisplay")] DurationDisplay? MicrosecondsDisplay = null,
		[property: Description("@#nanoseconds")] IntlDurationFractionStyle? Nanoseconds = null,
		[property: Description("@#nanosecondsDisplay")] DurationDisplay? NanosecondsDisplay = null,
		[property: Description("@#fractionalDigits")] Number? FractionalDigits = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.DurationFormat.prototype.resolvedOptions()</c>.
/// <c>Intl.DurationFormat.prototype.resolvedOptions()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public record ResolvedDurationFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#numberingSystem")] string NumberingSystem,
		[property: Description("@#style")] DurationFormatStyle Style,
		[property: Description("@#years")] LongShortNarrow? Years = null,
		[property: Description("@#yearsDisplay")] DurationDisplay? YearsDisplay = null,
		[property: Description("@#months")] LongShortNarrow? Months = null,
		[property: Description("@#monthsDisplay")] DurationDisplay? MonthsDisplay = null,
		[property: Description("@#weeks")] LongShortNarrow? Weeks = null,
		[property: Description("@#weeksDisplay")] DurationDisplay? WeeksDisplay = null,
		[property: Description("@#days")] LongShortNarrow? Days = null,
		[property: Description("@#daysDisplay")] DurationDisplay? DaysDisplay = null,
		[property: Description("@#hours")] IntlDurationTextStyle? Hours = null,
		[property: Description("@#hoursDisplay")] DurationDisplay? HoursDisplay = null,
		[property: Description("@#minutes")] IntlDurationTextStyle? Minutes = null,
		[property: Description("@#minutesDisplay")] DurationDisplay? MinutesDisplay = null,
		[property: Description("@#seconds")] IntlDurationTextStyle? Seconds = null,
		[property: Description("@#secondsDisplay")] DurationDisplay? SecondsDisplay = null,
		[property: Description("@#milliseconds")] IntlDurationFractionStyle? Milliseconds = null,
		[property: Description("@#millisecondsDisplay")] DurationDisplay? MillisecondsDisplay = null,
		[property: Description("@#microseconds")] IntlDurationFractionStyle? Microseconds = null,
		[property: Description("@#microsecondsDisplay")] DurationDisplay? MicrosecondsDisplay = null,
		[property: Description("@#nanoseconds")] IntlDurationFractionStyle? Nanoseconds = null,
		[property: Description("@#nanosecondsDisplay")] DurationDisplay? NanosecondsDisplay = null,
		[property: Description("@#fractionalDigits")] Number? FractionalDigits = null);

	/// <summary>
/// JavaScript duration-like input consumed by <c>Intl.DurationFormat</c>.
/// This is not a global host; it models the object-shaped runtime input directly.
/// <c>Intl.DurationFormat</c> 使用的 JavaScript duration 类输入；不是全局宿主，直接建模对象形状的运行时输入。
	/// </summary>
	[Description("@#")]
	public record DurationInput(
		[property: Description("@#years")] Number? Years = null,
		[property: Description("@#months")] Number? Months = null,
		[property: Description("@#weeks")] Number? Weeks = null,
		[property: Description("@#days")] Number? Days = null,
		[property: Description("@#hours")] Number? Hours = null,
		[property: Description("@#minutes")] Number? Minutes = null,
		[property: Description("@#seconds")] Number? Seconds = null,
		[property: Description("@#milliseconds")] Number? Milliseconds = null,
		[property: Description("@#microseconds")] Number? Microseconds = null,
		[property: Description("@#nanoseconds")] Number? Nanoseconds = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.DurationFormat.prototype.formatToParts()</c>.
/// <c>Intl.DurationFormat.prototype.formatToParts()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public sealed class DurationFormatPart
	{
		/// <summary>Gets the semantic part type. 获取语义 part 类型。</summary>
		[Description("@#type")]
		public extern string Type { get; }

		/// <summary>Gets localized text for this part. 获取此 part 的本地化文本。</summary>
		[Description("@#value")]
		public extern string Value { get; }

		/// <summary>Gets the duration unit when the part has one. 获取存在时的 Duration 单位。</summary>
		[Description("@#unit")]
		public extern string? Unit { get; }
	}

	/// <summary>
/// Projection of JavaScript's <c>Intl.DurationFormat</c> constructor host.
/// JavaScript <c>Intl.DurationFormat</c> 构造器宿主投影。
	/// </summary>
	[Description("@#DurationFormat")]
	public sealed class DurationFormat
	{
		/// <summary>
/// Gets JavaScript <c>Intl.DurationFormat.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.DurationFormat.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static DurationFormat Prototype { get; }

		/// <summary>Creates a duration formatter using runtime defaults. 使用运行时默认设置创建 Duration 格式化器。</summary>
		public extern DurationFormat();

		/// <summary>
/// Creates a duration formatter for a locale identifier. 为 locale 标识符创建 Duration 格式化器。
		/// </summary>
		public extern DurationFormat(string locales);

		/// <summary>Creates a duration formatter for a locale list. 为 locale 列表创建 Duration 格式化器。</summary>
		public extern DurationFormat(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern DurationFormat(DurationFormatOptions options);

		/// <summary>Creates a duration formatter for a locale and options. 为 locale 和 options 创建 Duration 格式化器。</summary>
		public extern DurationFormat(string locales, DurationFormatOptions options);

		/// <summary>Creates a duration formatter for a locale list and options. 为 locale 列表和 options 创建 Duration 格式化器。</summary>
		public extern DurationFormat(IEnumerable<string> locales, DurationFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, DurationFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, DurationFormatOptions options);

		/// <summary>
/// Formats a JavaScript duration-like input object.
/// 格式化 JavaScript duration 类输入对象。
		/// </summary>
		[Description("@#format")]
		public extern string Format(DurationInput duration);

		/// <summary>
/// Returns the formatted duration as JavaScript part records instead of a single concatenated string.
/// 将格式化的 Duration 返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public extern Array<DurationFormatPart> FormatToParts(DurationInput duration);

		/// <summary>Returns runtime-resolved duration-format options. 返回运行时解析后的 Duration 格式选项。</summary>
		[Description("@#resolvedOptions")]
		public extern ResolvedDurationFormatOptions ResolvedOptions();
	}

	[String]
	[Description("@#")]
	/// <summary>List relationship type: conjunction, disjunction, or unit. 列表关系类型：合取、析取或单位。</summary>
	public enum ListFormatType
	{
		[Description("@#conjunction")]
		Conjunction,
		[Description("@#disjunction")]
		Disjunction,
		[Description("@#unit")]
		Unit
	}

	/// <summary>
/// Configuration object for <c>Intl.ListFormat</c>.
/// <c>Intl.ListFormat</c> 的配置对象。
	/// </summary>
	[Description("@#")]
	public record ListFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#type")] ListFormatType? Type = null,
		[property: Description("@#style")] LongShortNarrow? Style = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.ListFormat.prototype.resolvedOptions()</c>.
/// <c>Intl.ListFormat.prototype.resolvedOptions()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public record ResolvedListFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#type")] ListFormatType Type,
		[property: Description("@#style")] LongShortNarrow Style);

	/// <summary>
/// Projection of JavaScript's <c>Intl.ListFormat</c> constructor host.
/// JavaScript <c>Intl.ListFormat</c> 构造器宿主投影。
	/// </summary>
	[Description("@#ListFormat")]
	public sealed class ListFormat
	{
		/// <summary>
/// Gets JavaScript <c>Intl.ListFormat.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.ListFormat.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static ListFormat Prototype { get; }

		/// <summary>Creates a list formatter using runtime defaults. 使用运行时默认设置创建列表格式化器。</summary>
		public extern ListFormat();

		/// <summary>
/// Creates a list formatter for a locale identifier. 为 locale 标识符创建列表格式化器。
		/// </summary>
		public extern ListFormat(string locales);

		/// <summary>Creates a list formatter for a locale list. 为 locale 列表创建列表格式化器。</summary>
		public extern ListFormat(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern ListFormat(ListFormatOptions options);

		/// <summary>Creates a list formatter for a locale and options. 为 locale 和 options 创建列表格式化器。</summary>
		public extern ListFormat(string locales, ListFormatOptions options);

		/// <summary>Creates a list formatter for a locale list and options. 为 locale 列表和 options 创建列表格式化器。</summary>
		public extern ListFormat(IEnumerable<string> locales, ListFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, ListFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, ListFormatOptions options);

		/// <summary>
/// Formats a JavaScript iterable of strings into a localized list.
/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript iterables.
/// 将 JavaScript 字符串 iterable 格式化为本地化列表；<see cref="IEnumerable{T}"/> 作为 JavaScript iterable 的通用 C# 输入表面。
		/// </summary>
		[Description("@#format")]
		public extern string Format(IEnumerable<string> list);

		/// <summary>
/// Compatibility overload that lets C# pass separate items while JavaScript itself takes a single iterable.
/// 兼容重载允许 C# 传入分离项，而 JavaScript 本身接收单个 iterable。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#format")]
		public extern string Format(params string[] list);

		/// <summary>
/// Returns the localized list as JavaScript part records instead of a single concatenated string.
/// 将本地化列表返回为 JavaScript part 记录，而不是单个拼接字符串。
		/// </summary>
		[Description("@#formatToParts")]
		public extern Array<ListFormatPart> FormatToParts(IEnumerable<string> list);

		/// <summary>
/// Compatibility overload that lets C# pass separate items while JavaScript itself takes a single iterable.
/// 兼容重载允许 C# 传入分离项，而 JavaScript 本身接收单个 iterable。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#formatToParts")]
		public extern Array<ListFormatPart> FormatToParts(params string[] list);

		/// <summary>Returns runtime-resolved list-format options. 返回运行时解析后的列表格式选项。</summary>
		[Description("@#resolvedOptions")]
		public extern ResolvedListFormatOptions ResolvedOptions();
	}

	[String]
	[Description("@#")]
	/// <summary>Plural-rule category family. 复数规则类别族。</summary>
	public enum PluralRulesType
	{
		[Description("@#cardinal")]
		Cardinal,
		[Description("@#ordinal")]
		Ordinal
	}

	/// <summary>
/// Configuration object for <c>Intl.PluralRules</c>.
/// <c>Intl.PluralRules</c> 的配置对象。
	/// </summary>
	[Description("@#")]
	public record PluralRulesOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#type")] PluralRulesType? Type = null,
		[property: Description("@#notation")] NumberFormatNotation? Notation = null,
		[property: Description("@#compactDisplay")] CompactDisplay? CompactDisplay = null,
		[property: Description("@#minimumIntegerDigits")] Number? MinimumIntegerDigits = null,
		[property: Description("@#minimumFractionDigits")] Number? MinimumFractionDigits = null,
		[property: Description("@#maximumFractionDigits")] Number? MaximumFractionDigits = null,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits = null,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits = null,
		[property: Description("@#roundingIncrement")] Number? RoundingIncrement = null,
		[property: Description("@#roundingMode")] RoundingMode? RoundingMode = null,
		[property: Description("@#roundingPriority")] RoundingPriority? RoundingPriority = null,
		[property: Description("@#trailingZeroDisplay")] TrailingZeroDisplay? TrailingZeroDisplay = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.PluralRules.prototype.resolvedOptions()</c>.
/// <c>Intl.PluralRules.prototype.resolvedOptions()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public record ResolvedPluralRulesOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#type")] PluralRulesType Type,
		[property: Description("@#notation")] NumberFormatNotation Notation,
		[property: Description("@#compactDisplay")] CompactDisplay? CompactDisplay,
		[property: Description("@#pluralCategories")] string[] PluralCategories,
		[property: Description("@#minimumIntegerDigits")] Number MinimumIntegerDigits,
		[property: Description("@#minimumFractionDigits")] Number? MinimumFractionDigits,
		[property: Description("@#maximumFractionDigits")] Number? MaximumFractionDigits,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits,
		[property: Description("@#roundingIncrement")] Number RoundingIncrement,
		[property: Description("@#roundingMode")] RoundingMode RoundingMode,
		[property: Description("@#roundingPriority")] RoundingPriority RoundingPriority,
		[property: Description("@#trailingZeroDisplay")] TrailingZeroDisplay TrailingZeroDisplay);

	/// <summary>
/// Projection of JavaScript's <c>Intl.PluralRules</c> constructor host.
/// JavaScript <c>Intl.PluralRules</c> 构造器宿主投影。
	/// </summary>
	[Description("@#PluralRules")]
	public sealed class PluralRules
	{
		/// <summary>
/// Gets JavaScript <c>Intl.PluralRules.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.PluralRules.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static PluralRules Prototype { get; }

		/// <summary>Creates plural rules using runtime defaults. 使用运行时默认设置创建复数规则。</summary>
		public extern PluralRules();

		/// <summary>
/// Creates plural rules for a locale identifier. 为 locale 标识符创建复数规则。
		/// </summary>
		public extern PluralRules(string locales);

		/// <summary>Creates plural rules for a locale list. 为 locale 列表创建复数规则。</summary>
		public extern PluralRules(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern PluralRules(PluralRulesOptions options);

		/// <summary>Creates plural rules for a locale and options. 为 locale 和 options 创建复数规则。</summary>
		public extern PluralRules(string locales, PluralRulesOptions options);

		/// <summary>Creates plural rules for a locale list and options. 为 locale 列表和 options 创建复数规则。</summary>
		public extern PluralRules(IEnumerable<string> locales, PluralRulesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, PluralRulesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, PluralRulesOptions options);

		[Description("@#select")]
		/// <summary>Returns the plural category for a JavaScript number. 返回 JavaScript Number 的复数类别。</summary>
		public extern string Select(Number value);

		/// <summary>
/// Returns the plural category for any JavaScript mathematical value accepted by <c>Intl.PluralRules</c>.
/// The union keeps the public host aligned with JavaScript's runtime coercion surface for numbers, bigints, and decimal strings.
/// 返回 <c>Intl.PluralRules</c> 接受的任意 JavaScript 数学值的复数类别；联合类型与 JavaScript 对 Number、BigInt、十进制字符串的运行时转换表面保持一致。
		/// </summary>
		[Description("@#select")]
		public extern string Select(IntlNumberInput value);

		/// <summary>
/// Returns the plural category for a numeric range.
/// This stays explicit because JavaScript exposes it separately from <c>select</c>.
/// 返回 Number 范围的复数类别；明确提供是因为 JavaScript 将其与 <c>select</c> 分别公开。
		/// </summary>
		[Description("@#selectRange")]
		public extern string SelectRange(Number start, Number end);

		/// <summary>
/// Returns the plural category for any JavaScript mathematical-value range accepted by <c>Intl.PluralRules</c>.
/// 返回 <c>Intl.PluralRules</c> 接受的任意 JavaScript 数学值范围的复数类别。
		/// </summary>
		[Description("@#selectRange")]
		public extern string SelectRange(IntlNumberInput start, IntlNumberInput end);

		/// <summary>Returns runtime-resolved plural-rule options. 返回运行时解析后的复数规则选项。</summary>
		[Description("@#resolvedOptions")]
		public extern ResolvedPluralRulesOptions ResolvedOptions();
	}

	[String]
	[Description("@#")]
	/// <summary>Code domain accepted by <c>Intl.DisplayNames</c>. <c>Intl.DisplayNames</c> 接受的代码域。</summary>
	public enum DisplayNamesType
	{
		[Description("@#language")]
		Language,
		[Description("@#region")]
		Region,
		[Description("@#script")]
		Script,
		[Description("@#currency")]
		Currency,
		[Description("@#calendar")]
		Calendar,
		[Description("@#dateTimeField")]
		DateTimeField
	}

	[String]
	[Description("@#")]
	/// <summary>Fallback policy when a display name is unavailable. 显示名称不可用时的回退策略。</summary>
	public enum DisplayNamesFallback
	{
		[Description("@#code")]
		Code,
		[Description("@#none")]
		None
	}

	[String]
	[Description("@#")]
	/// <summary>Language-name display form. 语言名称显示形式。</summary>
	public enum DisplayNamesLanguageDisplay
	{
		[Description("@#dialect")]
		Dialect,
		[Description("@#standard")]
		Standard
	}

	/// <summary>
/// Configuration object for <c>Intl.DisplayNames</c>.
/// <c>Intl.DisplayNames</c> 的配置对象。
	/// </summary>
	[Description("@#")]
	public record DisplayNamesOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#style")] LongShortNarrow? Style = null,
		[property: Description("@#type")] DisplayNamesType? Type = null,
		[property: Description("@#fallback")] DisplayNamesFallback? Fallback = null,
		[property: Description("@#languageDisplay")] DisplayNamesLanguageDisplay? LanguageDisplay = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.DisplayNames.prototype.resolvedOptions()</c>.
/// <c>Intl.DisplayNames.prototype.resolvedOptions()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public record ResolvedDisplayNamesOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#style")] LongShortNarrow Style,
		[property: Description("@#type")] DisplayNamesType Type,
		[property: Description("@#fallback")] DisplayNamesFallback Fallback,
		[property: Description("@#languageDisplay")] DisplayNamesLanguageDisplay? LanguageDisplay = null);

	/// <summary>
/// Projection of JavaScript's <c>Intl.DisplayNames</c> constructor host.
/// JavaScript <c>Intl.DisplayNames</c> 构造器宿主投影。
	/// </summary>
	[Description("@#DisplayNames")]
	public sealed class DisplayNames
	{
		/// <summary>
/// Gets JavaScript <c>Intl.DisplayNames.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.DisplayNames.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static DisplayNames Prototype { get; }

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern DisplayNames(DisplayNamesOptions options);

		/// <summary>
/// Creates display names for a locale identifier and options. 为 locale 标识符和 options 创建显示名称格式化器。
		/// </summary>
		public extern DisplayNames(string locales, DisplayNamesOptions options);

		/// <summary>Creates display names for a locale list and options. 为 locale 列表和 options 创建显示名称格式化器。</summary>
		public extern DisplayNames(IEnumerable<string> locales, DisplayNamesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, DisplayNamesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, DisplayNamesOptions options);

		/// <summary>
/// Returns the localized display name for the supplied code.
/// JavaScript may return <c>undefined</c> when no display name exists, so the C# projection stays nullable and maps that absence to <see langword="null"/>.
/// 返回给定代码的本地化显示名称；JavaScript 无显示名称时可能返回 <c>undefined</c>，故 C# 投影保持可空并将缺失映射为 <see langword="null"/>。
		/// </summary>
		[Description("@#of")]
		public extern string? Of(string code);

		/// <summary>Returns runtime-resolved display-name options. 返回运行时解析后的显示名称选项。</summary>
		[Description("@#resolvedOptions")]
		public extern ResolvedDisplayNamesOptions ResolvedOptions();
	}

	[String]
	[Description("@#")]
	/// <summary>Text segmentation granularity. 文本分词粒度。</summary>
	public enum SegmenterGranularity
	{
		[Description("@#grapheme")]
		Grapheme,
		[Description("@#word")]
		Word,
		[Description("@#sentence")]
		Sentence
	}

	/// <summary>
/// Configuration object for <c>Intl.Segmenter</c>.
/// <c>Intl.Segmenter</c> 的配置对象。
	/// </summary>
	[Description("@#")]
	public record SegmenterOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#granularity")] SegmenterGranularity? Granularity = null);

	/// <summary>
/// JavaScript object shape returned by <c>Intl.Segmenter.prototype.resolvedOptions()</c>.
/// <c>Intl.Segmenter.prototype.resolvedOptions()</c> 返回的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public record ResolvedSegmenterOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#granularity")] SegmenterGranularity Granularity);

	/// <summary>
/// JavaScript object shape produced while iterating <c>Intl.Segmenter</c> results.
/// 迭代 <c>Intl.Segmenter</c> 结果时产生的 JavaScript 对象形状。
	/// </summary>
	[Description("@#")]
	public sealed class SegmentData
	{
		/// <summary>Gets the segmented text. 获取分段文本。</summary>
		[Description("@#segment")]
		public extern string Segment { get; }

		/// <summary>Gets the zero-based UTF-16 code-unit index in the input. 获取输入中的零基 UTF-16 代码单元索引。</summary>
		[Description("@#index")]
		public extern Number Index { get; }

		/// <summary>Gets the original input text. 获取原始输入文本。</summary>
		[Description("@#input")]
		public extern string Input { get; }

		/// <summary>
/// Gets whether the segment is word-like for word segmentation.
/// JavaScript leaves this absent for other granularities, so the host projection stays nullable.
/// 获取分词模式下该段是否为 word-like；JavaScript 在其他粒度下省略此字段，因此宿主投影保持可空。
		/// </summary>
		[Description("@#isWordLike")]
		public extern bool? IsWordLike { get; }
	}

	/// <summary>
/// JavaScript iterable object returned by <c>Intl.Segmenter.prototype.segment()</c>.
/// This is not a global host; it models the runtime result object directly, including <c>containing()</c>.
/// <c>Intl.Segmenter.prototype.segment()</c> 返回的 JavaScript iterable 对象；不是全局宿主，直接建模运行时结果对象（包括 <c>containing()</c>）。
	/// </summary>
	[Description("@#")]
	public sealed class Segments : IEnumerable<SegmentData>
	{
		/// <summary>
/// Returns the segment containing the supplied code-unit index, or <see langword="null"/> when the index is outside the input.
/// 返回包含给定代码单元索引的分段；索引超出输入时为 <see langword="null"/>。
		/// </summary>
		[Description("@#containing")]
		public extern SegmentData? Containing(Number index);

		extern IEnumerator<SegmentData> IEnumerable<SegmentData>.GetEnumerator();

		extern IEnumerator IEnumerable.GetEnumerator();
	}

	/// <summary>
/// Projection of JavaScript's <c>Intl.Segmenter</c> constructor host.
/// JavaScript <c>Intl.Segmenter</c> 构造器宿主投影。
	/// </summary>
	[Description("@#Segmenter")]
	public sealed class Segmenter
	{
		/// <summary>
/// Gets JavaScript <c>Intl.Segmenter.prototype</c> object.
/// Keeping this on the constructor host preserves the recognizable runtime host boundary.
/// 获取 JavaScript <c>Intl.Segmenter.prototype</c> 对象；保留在构造器宿主上可维持可辨识的运行时宿主边界。
		/// </summary>
		[Description("@#prototype")]
		public extern static Segmenter Prototype { get; }

		/// <summary>Creates a segmenter using runtime defaults. 使用运行时默认设置创建分词器。</summary>
		public extern Segmenter();

		/// <summary>
/// Creates a segmenter for a locale identifier. 为 locale 标识符创建分词器。
		/// </summary>
		public extern Segmenter(string locales);

		/// <summary>Creates a segmenter for a locale list. 为 locale 列表创建分词器。</summary>
		public extern Segmenter(IEnumerable<string> locales);

		/// <summary>
/// C# convenience overload for JavaScript form that omits <c>locales</c> and supplies options only.
/// This exists because C# cannot naturally express an omitted leading optional argument at construction sites.
/// JavaScript 形式中省略 <c>locales</c>、仅提供 options 的 C# 便利重载；C# 无法自然表达省略前导可选参数。
		/// </summary>
		public extern Segmenter(SegmenterOptions options);

		/// <summary>Creates a segmenter for a locale and options. 为 locale 和 options 创建分词器。</summary>
		public extern Segmenter(string locales, SegmenterOptions options);

		/// <summary>Creates a segmenter for a locale list and options. 为 locale 列表和 options 创建分词器。</summary>
		public extern Segmenter(IEnumerable<string> locales, SegmenterOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, SegmenterOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(IEnumerable<string> locales, SegmenterOptions options);
		
		/// <summary>
/// Segments the input string and returns the JavaScript iterable result object.
/// 对输入字符串分段并返回 JavaScript iterable 结果对象。
		/// </summary>
		[Description("@#segment")]
		public extern Segments Segment(string input);

		/// <summary>Returns runtime-resolved segmenter options. 返回运行时解析后的分词器选项。</summary>
		[Description("@#resolvedOptions")]
		public extern ResolvedSegmenterOptions ResolvedOptions();
	}
}
