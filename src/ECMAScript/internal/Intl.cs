namespace ECMAScript;

[ECMAScript]
[Description("@#Intl")]
public static class Intl
{
	/// <summary>
	/// 指定比较器的用途。usage: "sort" | "search"
	/// </summary>
	[Description("@#CollatorUsage")]
	public enum CollatorUsage
	{
		[Description("@#Sort")]
		Sort,

		[Description("@#Search")]
		Search
	}

	/// <summary>
	/// 选择本地化匹配算法。localeMatcher: "lookup" | "best fit"
	/// </summary>
	[Description("@#LocaleMatcher")]
	public enum LocaleMatcher
	{
		Lookup,
		BestFit
	}

	/// <summary>
	/// 控制大小写字母的排序顺序。caseFirst: "upper" | "lower" | "false"
	/// </summary>
	[Description("@#CaseFirst")]
	public enum CaseFirst { Upper, Lower, False }

	/// <summary>
	/// 指定比较的敏感度级别。sensitivity: "base" | "accent" | "case" | "variant"
	/// </summary>
	[Description("@#Sensitivity")]
	public enum Sensitivity { Base, Accent, Case, Variant }

	/// <summary>
	/// 指定特定语言或场景的排序规则。collation: "big5han" | "compat" | "dict" | "direct" | "ducet" | "emoji" | "eor" | "gb2312" | "phonebk" | "phonetic" | "pinyin" | "reformed" | "searchjl" | "stroke" | "trad" | "unihan" | "zhuyin" | undefined
	/// </summary>
	[Description("@#Collation")]
	public enum Collation
	{
		Big5han,
		Compat,
		Dict,
		Direct,
		Ducet,
		Emoji,
		Eor,
		Gb2312,
		Phonebk,
		Phonetic,
		Pinyin,
		Reformed,
		Searchjl,
		Stroke,
		Trad,
		Unihan,
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
	[Description("@#Collation")]
	public record CollatorOptions(CollatorUsage? Usage, LocaleMatcher? LocaleMatcher, bool? Numeric, CaseFirst? CaseFirst, Sensitivity? Sensitivity, Collation? Collation, bool? IgnorePunctuation);

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
	[Description("@#Collation")]
	public record ResolvedCollatorOptions(string Locale, CollatorUsage usage, Sensitivity sensitivity, bool ignorePunctuation, Collation collation, CaseFirst caseFirst, bool numeric);

	[Description("@#Collator")]
	public abstract class Collator
	{
		public extern Collator();

		public extern Collator(Either<string, string[]> locales);

		public extern Collator(string locales);

		public extern Collator(string[] locales);

		public extern Collator(Either<string, string[]> locales, CollatorOptions options);

		public extern Collator(string locales, CollatorOptions options);

		public extern Collator(string[] locales, CollatorOptions options);

		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		public static extern string[] SupportedLocalesOf(string locales);

		public static extern string[] SupportedLocalesOf(string[] locales);

		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, CollatorOptions options);

		public static extern string[] SupportedLocalesOf(string locales, CollatorOptions options);

		public static extern string[] SupportedLocalesOf(string[] locales, CollatorOptions options);

		public virtual extern Number Compare(string x, string y);

		public virtual extern ResolvedCollatorOptions ResolvedOptions();
	}

	[Description("@#NumberFormatOptionsStyle")]
	public enum NumberFormatOptionsStyle
	{
		Decimal,  // 十进制格式（默认数字格式）
		Percent,  // 百分比格式（如 0.5 → "50%"）
		Currency  // 货币格式（如 1000 → "$1,000.00"）
	}

	[Description("@#NumberFormatOptionsCurrencyDisplay")]
	public enum NumberFormatOptionsCurrencyDisplay
	{
		Code,
		Symbol,
		Name
	}

	[Description("@#NumberFormatOptions")]
	public record NumberFormatOptions(
		LocaleMatcher? LocaleMatcher,
		NumberFormatOptionsStyle? Style,
		string? Currency,
		NumberFormatOptionsCurrencyDisplay? CurrencyDisplay,
		bool? UseGrouping,
		Number? MinimumIntegerDigits,
		Number? MinimumFractionDigits,
		Number? MaximumFractionDigits,
		Number? MinimumSignificantDigits,
		Number? MaximumSignificantDigits);

	[Description("@#ResolvedNumberFormatOptions")]
	public record ResolvedNumberFormatOptions(
		string Locale,
		string NumberingSystem,
		NumberFormatOptionsStyle Style,
		string Currency,
		NumberFormatOptionsCurrencyDisplay CurrencyDisplay,
		Number MinimumIntegerDigits,
		Number MinimumFractionDigits,
		Number MaximumFractionDigits,
		Number? MinimumSignificantDigits,
		Number? MaximumSignificantDigits,
		bool UseGrouping);

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

        public static extern string[] supportedLocalesOf(Either<string, string[]> locales);

        public static extern string[] supportedLocalesOf(string locales);

        public static extern string[] supportedLocalesOf(string[] locales);

        public static extern string[] supportedLocalesOf(Either<string, string[]> locales, NumberFormatOptions options);

        public static extern string[] supportedLocalesOf(string locales, NumberFormatOptions options);

        public static extern string[] supportedLocalesOf(string[] locales, NumberFormatOptions options);

		[Description("@#format")]
		public extern string Format(Number value);

        public extern ResolvedNumberFormatOptions resolvedOptions();

        public extern object? GetFormat(Type? formatType);
    }

    [Description("@#FormatMatcher")]
	public enum FormatMatcher { BestFit, Basic }

	[Description("@#LongShortNarrow")]
	public enum LongShortNarrow { Long, Short, Narrow }

	[Description("@#NumericTwoDigit")]
	public enum NumericTwoDigit { Numeric, TwoDigit }

	[Description("@#TimeZoneName")]
	public enum TimeZoneName { Short, Long, ShortOffset, LongOffset, ShortGeneric, LongGeneric }

	[Description("@#DateTimeFormatOptions")]
	public record DateTimeFormatOptions(
		LocaleMatcher? LocaleMatcher = null,
		LongShortNarrow? Weekday = null,
		LongShortNarrow? Era = null,
		NumericTwoDigit? Year = null,
		Either<NumericTwoDigit, LongShortNarrow>? Month = null,
		NumericTwoDigit? Day = null,
		NumericTwoDigit? Hour = null,
		NumericTwoDigit? Minute = null,
		NumericTwoDigit? Second = null,
		TimeZoneName? TimeZoneName = null,
		FormatMatcher? FormatMatcher = null,
		bool? Hour12 = null,
		string? TimeZone = null);

	[Description("@#ResolvedDateTimeFormatOptions")]
	public record ResolvedDateTimeFormatOptions(
		string Locale,
		string Calendar,
		string NumberingSystem,
		string TimeZone,
		bool? Hour12 = null,
		string? Weekday = null,
		string? Era = null,
		string? Year = null,
		string? Month = null,
		string? Day = null,
		string? Hour = null,
		string? Minute = null,
		string? Second = null,
		string? TimeZoneName = null);

	[Description("@#DateTimeFormat")]
	public abstract class DateTimeFormat
	{
		public extern DateTimeFormat();

		public extern DateTimeFormat(Either<string, string[]> locales);

		public extern DateTimeFormat(string locales);

		public extern DateTimeFormat(string[] locales);

		public extern DateTimeFormat(Either<string, string[]> locales, DateTimeFormatOptions options);

		public extern DateTimeFormat(string locales, DateTimeFormatOptions options);

		public extern DateTimeFormat(string[] locales, DateTimeFormatOptions options);

		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales);

		public static extern string[] SupportedLocalesOf(string locales);

		public static extern string[] SupportedLocalesOf(string[] locales);

		public static extern string[] SupportedLocalesOf(Either<string, string[]> locales, DateTimeFormatOptions options);

		public static extern string[] SupportedLocalesOf(string locales, DateTimeFormatOptions options);

		public static extern string[] SupportedLocalesOf(string[] locales, DateTimeFormatOptions options);

		public virtual extern string Format();

		public virtual extern string Format(Either<Date, Number> date);

		public virtual extern string Format(Date date);

		public virtual extern string Format(Number date);

		public virtual extern ResolvedDateTimeFormatOptions ResolvedOptions();
	}
}

