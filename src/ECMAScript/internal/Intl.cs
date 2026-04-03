namespace ECMAScript;

[ECMAScript]
[Description("@#Intl")]
public static class Intl
{
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
	/// 指定特定语言或场景的排序规则。collation: "big5han" | "compat" | "dict" | "direct" | "ducet" | "emoji" | "eor" | "gb2312" | "phonebk" | "phonetic" | "pinyin" | "reformed" | "searchjl" | "stroke" | "trad" | "unihan" | "zhuyin" | undefined
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

		[Description("@#resolvedOptions")]
		public virtual extern ResolvedDateTimeFormatOptions ResolvedOptions();
	}
}

