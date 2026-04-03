namespace ECMAScript;

[ECMAScript]
[Description("@#Intl")]
public static class Intl
{
	/// <summary>
	/// Common JavaScript object shape returned by Intl <c>formatToParts()</c> APIs.
	/// This is not a runtime global; it models the part records produced by Intl host objects.
	/// </summary>
	[Description("@#")]
	public sealed class FormatPart
	{
		[Description("@#type")]
		public extern string Type { get; }

		[Description("@#value")]
		public extern string Value { get; }
	}

	/// <summary>
	/// JavaScript object shape returned by <c>Intl.ListFormat.prototype.formatToParts()</c>.
	/// </summary>
	[Description("@#")]
	public sealed class ListFormatPart
	{
		[Description("@#type")]
		public extern string Type { get; }

		[Description("@#value")]
		public extern string Value { get; }
	}

	/// <summary>
	/// JavaScript object shape returned by Intl <c>formatRangeToParts()</c> APIs.
	/// In addition to <c>type</c> and <c>value</c>, the runtime exposes a <c>source</c> field telling whether a part came from the start range, end range, or a shared section.
	/// </summary>
	[Description("@#")]
	public sealed class RangeFormatPart
	{
		[Description("@#type")]
		public extern string Type { get; }

		[Description("@#value")]
		public extern string Value { get; }

		[Description("@#source")]
		public extern string Source { get; }
	}

	/// <summary>
	/// 指定比较器的用途。usage: "sort" | "search"
	/// </summary>
	[Description("@#")]
	public enum CollatorUsage
	{
		[Description("@#sort")]
		Sort,

		[Description("@#search")]
		Search
	}

	/// <summary>
	/// 选择本地化匹配算法。localeMatcher: "lookup" | "best fit"
	/// </summary>
	[Description("@#")]
	public enum LocaleMatcher
	{
		[Description("@#lookup")]
		Lookup,
		[Description("@#best fit")]
		BestFit
	}

	/// <summary>
	/// 控制大小写字母的排序顺序。caseFirst: "upper" | "lower" | "false"
	/// </summary>
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
	/// 指定比较的敏感度级别。sensitivity: "base" | "accent" | "case" | "variant"
	/// </summary>
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
	/// 指定特定语言或场景的排序规则。collation: "big5han" | "compat" | "dict" | "direct" | "ducet" | "emoji" | "eor" | "gb2312" | "phonebk" | "phonetic" | "pinyin" | "reformed" | "searchjl" | "stroke" | "trad" | "unihan" | "zhuyin"。未指定时保持省略状态。
	/// </summary>
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
	/// 定义了 Intl.Collator 构造函数的配置选项，用于控制字符串比较（排序或搜索）的行为。
	/// </summary>
	/// <param name="Usage">指定比较器的用途。<para>"sort"（默认）: 用于排序场景（区分大小写、重音等）。</para>"search": 用于搜索场景（可能更宽松，如忽略大小写）。 </param>
	/// <param name="LocaleMatcher">选择本地化匹配算法。<para>"lookup": 严格匹配，找不到完全匹配的 locale 时会回退。</para>"best fit"（默认）: 尝试找到最接近的 locale。 </param>
	/// <param name="Numeric">是否将数字字符串作为数值比较。<para>true: "10"大于 "2"（按数值）。</para>false（默认）: "10" 小于 "2"（按字典序）。 </param>
	/// <param name="CaseFirst">控制大小写字母的排序顺序。<para>"upper": 大写字母优先（如 A, a, B, b）。</para><para>​"lower": 小写字母优先（如 a, A, b, B）。</para>"false"（默认）: 由 locale 决定（通常不区分优先级）。 </param>
	/// <param name="Sensitivity">指定比较的敏感度级别。<para>"base": 忽略重音和大小写（a = á = A）。</para><para>"accent": 区分重音，忽略大小写（a ≠ á, a = A）。</para><para>"case": 区分大小写，忽略重音（a ≠ A, a = á）。</para>"variant"（默认）: 区分大小写和重音（a ≠ á ≠ A）。 </param>
	/// <param name="Collation">指定特定语言或场景的排序规则。例如：<para>"pinyin": 中文拼音排序。</para><para>"emoji": Emoji 符号排序。</para><para>"phonebk": 德语电话簿排序（如 ä = ae）。</para>...... </param>
	/// <param name="IgnorePunctuation">是否忽略标点符号。<para>true: 比较时忽略 !, - 等符号。</para>false（默认）: 标点符号参与比较。 </param>
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
	/// 
	/// </summary>
	/// <param name="locale"></param>
	/// <param name="usage"></param>
	/// <param name="sensitivity"></param>
	/// <param name="ignorePunctuation"></param>
	/// <param name="collation"></param>
	/// <param name="caseFirst"></param>
	/// <param name="numeric"></param>
	[Description("@#")]
	public record ResolvedCollatorOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#usage")] CollatorUsage usage,
		[property: Description("@#sensitivity")] Sensitivity sensitivity,
		[property: Description("@#ignorePunctuation")] bool ignorePunctuation,
		[property: Description("@#collation")] Collation collation,
		[property: Description("@#caseFirst")] CaseFirst caseFirst,
		[property: Description("@#numeric")] bool numeric);

	[Description("@#Collator")]
	public class Collator
	{
		public extern Collator();

		public extern Collator(Either<string, string[]> locales);

		public extern Collator(string locales);

		public extern Collator(string[] locales);

		public extern Collator(Either<string, string[]> locales, CollatorOptions options);

		public extern Collator(string locales, CollatorOptions options);

		public extern Collator(string[] locales, CollatorOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, CollatorOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, CollatorOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales, CollatorOptions options);

		[Description("@#compare")]
		public virtual extern Number Compare(string x, string y);

		[Description("@#resolvedOptions")]
		public virtual extern ResolvedCollatorOptions ResolvedOptions();
	}

	[Description("@#")]
	public enum NumberFormatOptionsStyle
	{
		[Description("@#decimal")]
		Decimal,  // 十进制格式（默认数字格式）
		[Description("@#percent")]
		Percent,  // 百分比格式（如 0.5 → "50%"）
		[Description("@#currency")]
		Currency  // 货币格式（如 1000 → "$1,000.00"）
	}

	[Description("@#")]
	public enum NumberFormatOptionsCurrencyDisplay
	{
		[Description("@#code")]
		Code,
		[Description("@#symbol")]
		Symbol,
		[Description("@#name")]
		Name
	}

	[Description("@#")]
	public record NumberFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher,
		[property: Description("@#style")] NumberFormatOptionsStyle? Style,
		[property: Description("@#currency")] string? Currency,
		[property: Description("@#currencyDisplay")] NumberFormatOptionsCurrencyDisplay? CurrencyDisplay,
		[property: Description("@#useGrouping")] bool? UseGrouping,
		[property: Description("@#minimumIntegerDigits")] Number? MinimumIntegerDigits,
		[property: Description("@#minimumFractionDigits")] Number? MinimumFractionDigits,
		[property: Description("@#maximumFractionDigits")] Number? MaximumFractionDigits,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits);

	[Description("@#")]
	public record ResolvedNumberFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#numberingSystem")] string NumberingSystem,
		[property: Description("@#style")] NumberFormatOptionsStyle Style,
		[property: Description("@#currency")] string Currency,
		[property: Description("@#currencyDisplay")] NumberFormatOptionsCurrencyDisplay CurrencyDisplay,
		[property: Description("@#minimumIntegerDigits")] Number MinimumIntegerDigits,
		[property: Description("@#minimumFractionDigits")] Number MinimumFractionDigits,
		[property: Description("@#maximumFractionDigits")] Number MaximumFractionDigits,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits,
		[property: Description("@#useGrouping")] bool UseGrouping);

	[Description("@#NumberFormat")]
    public sealed class NumberFormat : IFormatProvider
    {
        public extern NumberFormat();

        public extern NumberFormat(Either<string, string[]> locales);

        public extern NumberFormat(string locales);

        public extern NumberFormat(string[] locales);

        public extern NumberFormat(Either<string, string[]> locales, NumberFormatOptions options);

        public extern NumberFormat(string locales, NumberFormatOptions options);

        public extern NumberFormat(string[] locales, NumberFormatOptions options);

        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(string locales);

        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(string[] locales);

        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, NumberFormatOptions options);

        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(string locales, NumberFormatOptions options);

        [Description("@#supportedLocalesOf")]
        public static extern string[] SupportedLocalesOf(string[] locales, NumberFormatOptions options);

		[Description("@#format")]
		public extern string Format(Number value);

		/// <summary>
		/// Returns the localized number as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatToParts")]
		public extern Array<FormatPart> FormatToParts(Number value);

		/// <summary>
		/// Returns a localized string representing a numeric range.
		/// This stays on the <c>Intl.NumberFormat</c> host because JavaScript exposes it as an instance method there.
		/// </summary>
		[Description("@#formatRange")]
		public extern string FormatRange(Number start, Number end);

		/// <summary>
		/// Returns a localized numeric range as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatRangeToParts")]
		public extern Array<RangeFormatPart> FormatRangeToParts(Number start, Number end);

        [Description("@#resolvedOptions")]
        public extern ResolvedNumberFormatOptions ResolvedOptions();

        public extern object? GetFormat(Type? formatType);
    }

	[Description("@#Locale")]
	public sealed class Locale
	{
		public extern Locale(string tag);

		[Description("@#maximize")]
		public extern Locale Maximize();

		/// <summary>
		/// Returns a locale with likely-subtag information removed where possible.
		/// This stays on the <c>Intl.Locale</c> host to match the JavaScript runtime object directly.
		/// </summary>
		[Description("@#minimize")]
		public extern Locale Minimize();

		[Description("@#language")]
		public extern string Language { get; }

		[Description("@#script")]
		public extern string? Script { get; }

		[Description("@#region")]
		public extern string? Region { get; }

		[Description("@#baseName")]
		public extern string BaseName { get; }

		[Description("@#toString")]
		public extern override string ToString();
	}

    [Description("@#")]
	public enum FormatMatcher
	{
		[Description("@#best fit")]
		BestFit,
		[Description("@#basic")]
		Basic
	}

	[Description("@#")]
	public enum LongShortNarrow
	{
		[Description("@#long")]
		Long,
		[Description("@#short")]
		Short,
		[Description("@#narrow")]
		Narrow
	}

	[Description("@#")]
	public enum NumericTwoDigit
	{
		[Description("@#numeric")]
		Numeric,
		[Description("@#2-digit")]
		TwoDigit
	}

	[Description("@#")]
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

	[Description("@#")]
	public record DateTimeFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#weekday")] LongShortNarrow? Weekday = null,
		[property: Description("@#era")] LongShortNarrow? Era = null,
		[property: Description("@#year")] NumericTwoDigit? Year = null,
		[property: Description("@#month")] Either<NumericTwoDigit, LongShortNarrow>? Month = null,
		[property: Description("@#day")] NumericTwoDigit? Day = null,
		[property: Description("@#hour")] NumericTwoDigit? Hour = null,
		[property: Description("@#minute")] NumericTwoDigit? Minute = null,
		[property: Description("@#second")] NumericTwoDigit? Second = null,
		[property: Description("@#timeZoneName")] TimeZoneName? TimeZoneName = null,
		[property: Description("@#formatMatcher")] FormatMatcher? FormatMatcher = null,
		[property: Description("@#hour12")] bool? Hour12 = null,
		[property: Description("@#timeZone")] string? TimeZone = null);

	[Description("@#")]
	public record ResolvedDateTimeFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#calendar")] string Calendar,
		[property: Description("@#numberingSystem")] string NumberingSystem,
		[property: Description("@#timeZone")] string TimeZone,
		[property: Description("@#hour12")] bool? Hour12 = null,
		[property: Description("@#weekday")] string? Weekday = null,
		[property: Description("@#era")] string? Era = null,
		[property: Description("@#year")] string? Year = null,
		[property: Description("@#month")] string? Month = null,
		[property: Description("@#day")] string? Day = null,
		[property: Description("@#hour")] string? Hour = null,
		[property: Description("@#minute")] string? Minute = null,
		[property: Description("@#second")] string? Second = null,
		[property: Description("@#timeZoneName")] string? TimeZoneName = null);

	[Description("@#DateTimeFormat")]
	public class DateTimeFormat
	{
		public extern DateTimeFormat();

		public extern DateTimeFormat(Either<string, string[]> locales);

		public extern DateTimeFormat(string locales);

		public extern DateTimeFormat(string[] locales);

		public extern DateTimeFormat(Either<string, string[]> locales, DateTimeFormatOptions options);

		public extern DateTimeFormat(string locales, DateTimeFormatOptions options);

		public extern DateTimeFormat(string[] locales, DateTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, DateTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, DateTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales, DateTimeFormatOptions options);

		[Description("@#format")]
		public virtual extern string Format();

		[Description("@#format")]
		public virtual extern string Format(Either<Date, Number> date);

		[Description("@#format")]
		public virtual extern string Format(Date date);

		[Description("@#format")]
		public virtual extern string Format(Number date);

		/// <summary>
		/// Returns the formatted date as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts();

		/// <summary>
		/// Returns the formatted date as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts(Either<Date, Number> date);

		/// <summary>
		/// Returns the formatted date as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts(Date date);

		/// <summary>
		/// Returns the formatted date as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatToParts")]
		public virtual extern Array<FormatPart> FormatToParts(Number date);

		/// <summary>
		/// Returns a localized string representing a date/time range.
		/// </summary>
		[Description("@#formatRange")]
		public virtual extern string FormatRange(Either<Date, Number> startDate, Either<Date, Number> endDate);

		/// <summary>
		/// Returns a localized string representing a date/time range.
		/// </summary>
		[Description("@#formatRange")]
		public virtual extern string FormatRange(Date startDate, Date endDate);

		/// <summary>
		/// Returns a localized string representing a date/time range.
		/// </summary>
		[Description("@#formatRange")]
		public virtual extern string FormatRange(Number startDate, Number endDate);

		/// <summary>
		/// Returns a localized date/time range as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatRangeToParts")]
		public virtual extern Array<RangeFormatPart> FormatRangeToParts(Either<Date, Number> startDate, Either<Date, Number> endDate);

		/// <summary>
		/// Returns a localized date/time range as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatRangeToParts")]
		public virtual extern Array<RangeFormatPart> FormatRangeToParts(Date startDate, Date endDate);

		/// <summary>
		/// Returns a localized date/time range as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatRangeToParts")]
		public virtual extern Array<RangeFormatPart> FormatRangeToParts(Number startDate, Number endDate);

		[Description("@#resolvedOptions")]
		public virtual extern ResolvedDateTimeFormatOptions ResolvedOptions();
	}

	[Description("@#")]
	public enum RelativeTimeFormatStyle
	{
		[Description("@#long")]
		Long,
		[Description("@#short")]
		Short,
		[Description("@#narrow")]
		Narrow
	}

	[Description("@#")]
	public enum RelativeTimeFormatNumeric
	{
		[Description("@#always")]
		Always,
		[Description("@#auto")]
		Auto
	}

	[Description("@#")]
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
	/// </summary>
	[Description("@#")]
	public sealed class RelativeTimeFormatPart
	{
		[Description("@#type")]
		public extern string Type { get; }

		[Description("@#value")]
		public extern string Value { get; }

		[Description("@#unit")]
		public extern string? Unit { get; }
	}

	/// <summary>
	/// Configuration object for <c>Intl.RelativeTimeFormat</c>.
	/// It stays explicit so callers can see the JavaScript option names and value domains directly from C#.
	/// </summary>
	[Description("@#")]
	public record RelativeTimeFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#numeric")] RelativeTimeFormatNumeric? Numeric = null,
		[property: Description("@#style")] RelativeTimeFormatStyle? Style = null);

	/// <summary>
	/// JavaScript object shape returned by <c>Intl.RelativeTimeFormat.prototype.resolvedOptions()</c>.
	/// </summary>
	[Description("@#")]
	public record ResolvedRelativeTimeFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#style")] RelativeTimeFormatStyle Style,
		[property: Description("@#numeric")] RelativeTimeFormatNumeric Numeric,
		[property: Description("@#numberingSystem")] string NumberingSystem);

	/// <summary>
	/// Projection of JavaScript's <c>Intl.RelativeTimeFormat</c> constructor host.
	/// </summary>
	[Description("@#RelativeTimeFormat")]
	public sealed class RelativeTimeFormat
	{
		public extern RelativeTimeFormat();

		public extern RelativeTimeFormat(Either<string, string[]> locales);

		public extern RelativeTimeFormat(string locales);

		public extern RelativeTimeFormat(string[] locales);

		public extern RelativeTimeFormat(Either<string, string[]> locales, RelativeTimeFormatOptions options);

		public extern RelativeTimeFormat(string locales, RelativeTimeFormatOptions options);

		public extern RelativeTimeFormat(string[] locales, RelativeTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, RelativeTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, RelativeTimeFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales, RelativeTimeFormatOptions options);

		[Description("@#format")]
		public extern string Format(Number value, RelativeTimeUnit unit);

		[Description("@#formatToParts")]
		public extern Array<RelativeTimeFormatPart> FormatToParts(Number value, RelativeTimeUnit unit);

		[Description("@#resolvedOptions")]
		public extern ResolvedRelativeTimeFormatOptions ResolvedOptions();
	}

	[Description("@#")]
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
	/// </summary>
	[Description("@#")]
	public record ListFormatOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#type")] ListFormatType? Type = null,
		[property: Description("@#style")] LongShortNarrow? Style = null);

	/// <summary>
	/// JavaScript object shape returned by <c>Intl.ListFormat.prototype.resolvedOptions()</c>.
	/// </summary>
	[Description("@#")]
	public record ResolvedListFormatOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#type")] ListFormatType Type,
		[property: Description("@#style")] LongShortNarrow Style);

	/// <summary>
	/// Projection of JavaScript's <c>Intl.ListFormat</c> constructor host.
	/// </summary>
	[Description("@#ListFormat")]
	public sealed class ListFormat
	{
		public extern ListFormat();

		public extern ListFormat(Either<string, string[]> locales);

		public extern ListFormat(string locales);

		public extern ListFormat(string[] locales);

		public extern ListFormat(Either<string, string[]> locales, ListFormatOptions options);

		public extern ListFormat(string locales, ListFormatOptions options);

		public extern ListFormat(string[] locales, ListFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, ListFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, ListFormatOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales, ListFormatOptions options);

		/// <summary>
		/// Formats a JavaScript iterable of strings into a localized list.
		/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript iterables.
		/// </summary>
		[Description("@#format")]
		public extern string Format(IEnumerable<string> list);

		/// <summary>
		/// Compatibility overload that lets C# pass separate items while JavaScript itself takes a single iterable.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#format")]
		public extern string Format(params string[] list);

		/// <summary>
		/// Returns the localized list as JavaScript part records instead of a single concatenated string.
		/// </summary>
		[Description("@#formatToParts")]
		public extern Array<ListFormatPart> FormatToParts(IEnumerable<string> list);

		/// <summary>
		/// Compatibility overload that lets C# pass separate items while JavaScript itself takes a single iterable.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Description("@#formatToParts")]
		public extern Array<ListFormatPart> FormatToParts(params string[] list);

		[Description("@#resolvedOptions")]
		public extern ResolvedListFormatOptions ResolvedOptions();
	}

	[Description("@#")]
	public enum PluralRulesType
	{
		[Description("@#cardinal")]
		Cardinal,
		[Description("@#ordinal")]
		Ordinal
	}

	/// <summary>
	/// Configuration object for <c>Intl.PluralRules</c>.
	/// </summary>
	[Description("@#")]
	public record PluralRulesOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#type")] PluralRulesType? Type = null,
		[property: Description("@#minimumIntegerDigits")] Number? MinimumIntegerDigits = null,
		[property: Description("@#minimumFractionDigits")] Number? MinimumFractionDigits = null,
		[property: Description("@#maximumFractionDigits")] Number? MaximumFractionDigits = null,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits = null,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits = null);

	/// <summary>
	/// JavaScript object shape returned by <c>Intl.PluralRules.prototype.resolvedOptions()</c>.
	/// </summary>
	[Description("@#")]
	public record ResolvedPluralRulesOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#type")] PluralRulesType Type,
		[property: Description("@#pluralCategories")] string[] PluralCategories,
		[property: Description("@#minimumIntegerDigits")] Number MinimumIntegerDigits,
		[property: Description("@#minimumFractionDigits")] Number? MinimumFractionDigits = null,
		[property: Description("@#maximumFractionDigits")] Number? MaximumFractionDigits = null,
		[property: Description("@#minimumSignificantDigits")] Number? MinimumSignificantDigits = null,
		[property: Description("@#maximumSignificantDigits")] Number? MaximumSignificantDigits = null);

	/// <summary>
	/// Projection of JavaScript's <c>Intl.PluralRules</c> constructor host.
	/// </summary>
	[Description("@#PluralRules")]
	public sealed class PluralRules
	{
		public extern PluralRules();

		public extern PluralRules(Either<string, string[]> locales);

		public extern PluralRules(string locales);

		public extern PluralRules(string[] locales);

		public extern PluralRules(Either<string, string[]> locales, PluralRulesOptions options);

		public extern PluralRules(string locales, PluralRulesOptions options);

		public extern PluralRules(string[] locales, PluralRulesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, PluralRulesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, PluralRulesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales, PluralRulesOptions options);

		[Description("@#select")]
		public extern string Select(Number value);

		/// <summary>
		/// Returns the plural category for a numeric range.
		/// This stays explicit because JavaScript exposes it separately from <c>select</c>.
		/// </summary>
		[Description("@#selectRange")]
		public extern string SelectRange(Number start, Number end);

		[Description("@#resolvedOptions")]
		public extern ResolvedPluralRulesOptions ResolvedOptions();
	}

	[Description("@#")]
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

	[Description("@#")]
	public enum DisplayNamesFallback
	{
		[Description("@#code")]
		Code,
		[Description("@#none")]
		None
	}

	[Description("@#")]
	public enum DisplayNamesLanguageDisplay
	{
		[Description("@#dialect")]
		Dialect,
		[Description("@#standard")]
		Standard
	}

	/// <summary>
	/// Configuration object for <c>Intl.DisplayNames</c>.
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
	/// </summary>
	[Description("@#DisplayNames")]
	public sealed class DisplayNames
	{
		public extern DisplayNames(Either<string, string[]> locales, DisplayNamesOptions options);

		public extern DisplayNames(string locales, DisplayNamesOptions options);

		public extern DisplayNames(string[] locales, DisplayNamesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, DisplayNamesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, DisplayNamesOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales, DisplayNamesOptions options);

		/// <summary>
		/// Returns the localized display name for the supplied code.
		/// JavaScript may return <c>undefined</c> when no display name exists,
		/// so the C# projection stays nullable and maps that absence to <see langword="null" />.
		/// </summary>
		[Description("@#of")]
		public extern string? Of(string code);

		[Description("@#resolvedOptions")]
		public extern ResolvedDisplayNamesOptions ResolvedOptions();
	}

	[Description("@#")]
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
	/// </summary>
	[Description("@#")]
	public record SegmenterOptions(
		[property: Description("@#localeMatcher")] LocaleMatcher? LocaleMatcher = null,
		[property: Description("@#granularity")] SegmenterGranularity? Granularity = null);

	/// <summary>
	/// JavaScript object shape returned by <c>Intl.Segmenter.prototype.resolvedOptions()</c>.
	/// </summary>
	[Description("@#")]
	public record ResolvedSegmenterOptions(
		[property: Description("@#locale")] string Locale,
		[property: Description("@#granularity")] SegmenterGranularity Granularity);

	/// <summary>
	/// JavaScript object shape produced while iterating <c>Intl.Segmenter</c> results.
	/// </summary>
	[Description("@#")]
	public sealed class SegmentData
	{
		[Description("@#segment")]
		public extern string Segment { get; }

		[Description("@#index")]
		public extern Number Index { get; }

		[Description("@#input")]
		public extern string Input { get; }

		/// <summary>
		/// Only meaningful for word segmentation. JavaScript leaves this absent for other granularities, so the host projection stays nullable.
		/// </summary>
		[Description("@#isWordLike")]
		public extern bool? IsWordLike { get; }
	}

	/// <summary>
	/// JavaScript iterable object returned by <c>Intl.Segmenter.prototype.segment()</c>.
	/// This is not a global host; it models the runtime result object directly, including <c>containing()</c>.
	/// </summary>
	[Description("@#")]
	public sealed class Segments : IEnumerable<SegmentData>
	{
		/// <summary>
		/// Returns the segment containing the supplied code-unit index, or <see langword="null"/> when the index is outside the input.
		/// </summary>
		[Description("@#containing")]
		public extern SegmentData? Containing(Number index);

		extern IEnumerator<SegmentData> IEnumerable<SegmentData>.GetEnumerator();

		extern IEnumerator IEnumerable.GetEnumerator();
	}

	/// <summary>
	/// Projection of JavaScript's <c>Intl.Segmenter</c> constructor host.
	/// </summary>
	[Description("@#Segmenter")]
	public sealed class Segmenter
	{
		public extern Segmenter();

		public extern Segmenter(Either<string, string[]> locales);

		public extern Segmenter(string locales);

		public extern Segmenter(string[] locales);

		public extern Segmenter(Either<string, string[]> locales, SegmenterOptions options);

		public extern Segmenter(string locales, SegmenterOptions options);

		public extern Segmenter(string[] locales, SegmenterOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, SegmenterOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string locales, SegmenterOptions options);

		[Description("@#supportedLocalesOf")]
		public static extern string[] SupportedLocalesOf(string[] locales, SegmenterOptions options);

		/// <summary>
		/// Segments the input string and returns the JavaScript iterable result object.
		/// </summary>
		[Description("@#segment")]
		public extern Segments Segment(string input);

		[Description("@#resolvedOptions")]
		public extern ResolvedSegmenterOptions ResolvedOptions();
	}
}

