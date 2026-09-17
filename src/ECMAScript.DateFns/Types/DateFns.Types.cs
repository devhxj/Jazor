using System;
using System.ComponentModel;
using ECMAScript.Contract;

namespace ECMAScript;

/// <summary>
/// 每周的起始日；数值与 date-fns 的 <c>Day</c> 0-6 定义一致，0 为星期日。
/// The day the week starts on; values mirror the date-fns <c>Day</c> 0-6 domain where 0 is Sunday.
/// </summary>
public enum DateFnsWeekStartsOn
{
    /// <summary>星期日。Sunday.</summary>
    Sunday = 0,

    /// <summary>星期一。Monday.</summary>
    Monday = 1,

    /// <summary>星期二。Tuesday.</summary>
    Tuesday = 2,

    /// <summary>星期三。Wednesday.</summary>
    Wednesday = 3,

    /// <summary>星期四。Thursday.</summary>
    Thursday = 4,

    /// <summary>星期五。Friday.</summary>
    Friday = 5,

    /// <summary>星期六。Saturday.</summary>
    Saturday = 6
}

/// <summary>
/// 年内第一周必须包含的日期；1 为 1 月 1 日所在周按周一计，4 为按周四计（ISO 8601）。
/// Which day the first week of the year must contain; 1 counts the week of January 1st by Monday, 4 by Thursday (ISO 8601).
/// </summary>
public enum DateFnsFirstWeekContainsDate
{
    /// <summary>1 月 1 日所在周按周一定为第一周。The week containing January 1st (by Monday) is the first week.</summary>
    FirstDay = 1,

    /// <summary>按周四判定第一周（ISO 8601）。The first week is determined by Thursday (ISO 8601).</summary>
    FourthDay = 4
}

/// <summary>
/// 数值取整方法，对应 date-fns 的 <c>RoundingMethod</c> 字符串值域。
/// Numeric rounding method matching the date-fns <c>RoundingMethod</c> string domain.
/// </summary>
[String]
public enum DateFnsRoundingMethod
{
    /// <summary>向上取整。Round towards positive infinity.</summary>
    [Description("@#ceil")]
    Ceil,

    /// <summary>向下取整。Round towards negative infinity.</summary>
    [Description("@#floor")]
    Floor,

    /// <summary>四舍五入。Round to the nearest integer.</summary>
    [Description("@#round")]
    Round,

    /// <summary>截断小数部分。Truncate the fractional part.</summary>
    [Description("@#trunc")]
    Trunc
}

/// <summary>
/// 严格距离格式化强制的单位，对应 <c>FormatDistanceStrictUnit</c> 字符串值域。
/// The unit forced by strict distance formatting, matching <c>FormatDistanceStrictUnit</c>.
/// </summary>
[String]
public enum DateFnsDistanceUnit
{
    /// <summary>秒。Seconds.</summary>
    [Description("@#second")]
    Second,

    /// <summary>分钟。Minutes.</summary>
    [Description("@#minute")]
    Minute,

    /// <summary>小时。Hours.</summary>
    [Description("@#hour")]
    Hour,

    /// <summary>天。Days.</summary>
    [Description("@#day")]
    Day,

    /// <summary>周。Weeks.</summary>
    [Description("@#week")]
    Week,

    /// <summary>月。Months.</summary>
    [Description("@#month")]
    Month,

    /// <summary>季度。Quarters.</summary>
    [Description("@#quarter")]
    Quarter,

    /// <summary>年。Years.</summary>
    [Description("@#year")]
    Year
}

/// <summary>
/// ISO 8601 格式的分隔风格。
/// The separator style of ISO 8601 formatting.
/// </summary>
[String]
public enum DateFnsISOFormat
{
    /// <summary>最少分隔符的基本格式。Basic format with the minimal number of separators.</summary>
    [Description("@#basic")]
    Basic,

    /// <summary>带分隔符的可读格式。Extended format with separators for readability.</summary>
    [Description("@#extended")]
    Extended
}

/// <summary>
/// ISO 8601 输出包含的日期/时间分量。
/// Which date/time components the ISO 8601 output includes.
/// </summary>
[String]
public enum DateFnsISORepresentation
{
    /// <summary>日期与时间。Both date and time.</summary>
    [Description("@#complete")]
    Complete,

    /// <summary>仅日期。Date only.</summary>
    [Description("@#date")]
    Date,

    /// <summary>仅时间。Time only.</summary>
    [Description("@#time")]
    Time
}

/// <summary>
/// parseISO 扩展年份格式的额外位数。
/// The additional digits of the extended year format accepted by <c>parseISO</c>.
/// </summary>
public enum DateFnsAdditionalDigits
{
    /// <summary>无额外位数。No additional digits.</summary>
    None = 0,

    /// <summary>1 位额外位数。One additional digit.</summary>
    One = 1,

    /// <summary>2 位额外位数。Two additional digits.</summary>
    Two = 2
}

/// <summary>
/// date-fns 时间段对象；各分量按日历语义独立叠加，字段可省略。
/// The date-fns duration object; components add up independently with calendar semantics and may be omitted.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsDuration
{
    /// <summary>年分量。The year component.</summary>
    [Description("@#years")]
    public Number? Years { get; init; }

    /// <summary>月分量。The month component.</summary>
    [Description("@#months")]
    public Number? Months { get; init; }

    /// <summary>周分量。The week component.</summary>
    [Description("@#weeks")]
    public Number? Weeks { get; init; }

    /// <summary>天分量。The day component.</summary>
    [Description("@#days")]
    public Number? Days { get; init; }

    /// <summary>小时分量。The hour component.</summary>
    [Description("@#hours")]
    public Number? Hours { get; init; }

    /// <summary>分钟分量。The minute component.</summary>
    [Description("@#minutes")]
    public Number? Minutes { get; init; }

    /// <summary>秒分量。The second component.</summary>
    [Description("@#seconds")]
    public Number? Seconds { get; init; }
}

/// <summary>
/// date-fns 区间对象；start/end 为区间端点。
/// The date-fns interval object; start/end are the interval endpoints.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsInterval
{
    /// <summary>区间起点。The start of the interval.</summary>
    [Description("@#start")]
    public Date Start { get; init; } = default!;

    /// <summary>区间终点。The end of the interval.</summary>
    [Description("@#end")]
    public Date End { get; init; } = default!;
}

/// <summary>
/// date-fns locale 对象的宿主投影；具体 locale 从 <see cref="DateFnsLocale"/> 获取。
/// Host projection of a date-fns locale object; obtain instances from <see cref="DateFnsLocale"/>.
/// </summary>
[ECMAScript]
[Description("@#")]
public sealed class DateFnsLocaleObject
{
    private DateFnsLocaleObject()
    {
    }
}

/// <summary>
/// <c>format</c> 的选项对象。
/// Options for <c>format</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsFormatOptions
{
    /// <summary>格式化使用的 locale。The locale used for formatting.</summary>
    [Description("@#locale")]
    public DateFnsLocaleObject? Locale { get; init; }

    /// <summary>每周的起始日。The day the week starts on.</summary>
    [Description("@#weekStartsOn")]
    public DateFnsWeekStartsOn? WeekStartsOn { get; init; }

    /// <summary>年内第一周必须包含的日期。Which day the first week of the year must contain.</summary>
    [Description("@#firstWeekContainsDate")]
    public DateFnsFirstWeekContainsDate? FirstWeekContainsDate { get; init; }

    /// <summary>允许周年份 token <c>YY</c>/<c>YYYY</c>。Enables the week-numbering year tokens <c>YY</c>/<c>YYYY</c>.</summary>
    [Description("@#useAdditionalWeekYearTokens")]
    public bool? UseAdditionalWeekYearTokens { get; init; }

    /// <summary>允许年积日 token <c>D</c>/<c>DD</c>。Enables the day of year tokens <c>D</c>/<c>DD</c>.</summary>
    [Description("@#useAdditionalDayOfYearTokens")]
    public bool? UseAdditionalDayOfYearTokens { get; init; }
}

/// <summary>
/// <c>parse</c> 的选项对象。
/// Options for <c>parse</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsParseOptions
{
    /// <summary>解析使用的 locale。The locale used for parsing.</summary>
    [Description("@#locale")]
    public DateFnsLocaleObject? Locale { get; init; }

    /// <summary>每周的起始日。The day the week starts on.</summary>
    [Description("@#weekStartsOn")]
    public DateFnsWeekStartsOn? WeekStartsOn { get; init; }

    /// <summary>年内第一周必须包含的日期。Which day the first week of the year must contain.</summary>
    [Description("@#firstWeekContainsDate")]
    public DateFnsFirstWeekContainsDate? FirstWeekContainsDate { get; init; }

    /// <summary>允许周年份 token <c>YY</c>/<c>YYYY</c>。Enables the week-numbering year tokens <c>YY</c>/<c>YYYY</c>.</summary>
    [Description("@#useAdditionalWeekYearTokens")]
    public bool? UseAdditionalWeekYearTokens { get; init; }

    /// <summary>允许年积日 token <c>D</c>/<c>DD</c>。Enables the day of year tokens <c>D</c>/<c>DD</c>.</summary>
    [Description("@#useAdditionalDayOfYearTokens")]
    public bool? UseAdditionalDayOfYearTokens { get; init; }
}

/// <summary>
/// <c>parseISO</c> 的选项对象。
/// Options for <c>parseISO</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsParseISOOptions
{
    /// <summary>扩展年份格式的额外位数。The additional digits of the extended year format.</summary>
    [Description("@#additionalDigits")]
    public DateFnsAdditionalDigits? AdditionalDigits { get; init; }
}

/// <summary>
/// <c>formatISO</c> 的选项对象。
/// Options for <c>formatISO</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsFormatISOOptions
{
    /// <summary>ISO 分隔风格。The ISO separator style.</summary>
    [Description("@#format")]
    public DateFnsISOFormat? Format { get; init; }

    /// <summary>输出包含的日期/时间分量。Which date/time components the output includes.</summary>
    [Description("@#representation")]
    public DateFnsISORepresentation? Representation { get; init; }
}

/// <summary>
/// <c>formatDistance</c> 与 <c>formatDistanceToNow</c> 的选项对象。
/// Options for <c>formatDistance</c> and <c>formatDistanceToNow</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsFormatDistanceOptions
{
    /// <summary>距离描述使用的 locale。The locale used for the distance description.</summary>
    [Description("@#locale")]
    public DateFnsLocaleObject? Locale { get; init; }

    /// <summary>一分钟以内的距离输出更细的秒级描述。Distances less than a minute become more detailed.</summary>
    [Description("@#includeSeconds")]
    public bool? IncludeSeconds { get; init; }

    /// <summary>附加“x 前/后”后缀。Appends an "x ago"/"in x" suffix.</summary>
    [Description("@#addSuffix")]
    public bool? AddSuffix { get; init; }
}

/// <summary>
/// <c>formatDistanceStrict</c> 与 <c>formatDistanceToNowStrict</c> 的选项对象。
/// Options for <c>formatDistanceStrict</c> and <c>formatDistanceToNowStrict</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsFormatDistanceStrictOptions
{
    /// <summary>距离描述使用的 locale。The locale used for the distance description.</summary>
    [Description("@#locale")]
    public DateFnsLocaleObject? Locale { get; init; }

    /// <summary>附加“x 前/后”后缀。Appends an "x ago"/"in x" suffix.</summary>
    [Description("@#addSuffix")]
    public bool? AddSuffix { get; init; }

    /// <summary>强制使用的单位。The unit forced on the output.</summary>
    [Description("@#unit")]
    public DateFnsDistanceUnit? Unit { get; init; }

    /// <summary>数值取整方法。The numeric rounding method.</summary>
    [Description("@#roundingMethod")]
    public DateFnsRoundingMethod? RoundingMethod { get; init; }
}

/// <summary>
/// <c>formatDuration</c> 的选项对象。
/// Options for <c>formatDuration</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsFormatDurationOptions
{
    /// <summary>时长描述使用的 locale。The locale used for the duration description.</summary>
    [Description("@#locale")]
    public DateFnsLocaleObject? Locale { get; init; }

    /// <summary>参与输出的单位列表。The units included in the output.</summary>
    [Description("@#format")]
    public DateFnsDistanceUnit[]? Format { get; init; }

    /// <summary>是否包含零值分量。Whether zero-valued components are included.</summary>
    [Description("@#zero")]
    public bool? Zero { get; init; }

    /// <summary>分量之间的分隔字符串。The delimiter between components.</summary>
    [Description("@#delimiter")]
    public string? Delimiter { get; init; }
}

/// <summary>
/// <c>formatRelative</c> 的选项对象。
/// Options for <c>formatRelative</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsFormatRelativeOptions
{
    /// <summary>相对描述使用的 locale。The locale used for the relative description.</summary>
    [Description("@#locale")]
    public DateFnsLocaleObject? Locale { get; init; }

    /// <summary>每周的起始日。The day the week starts on.</summary>
    [Description("@#weekStartsOn")]
    public DateFnsWeekStartsOn? WeekStartsOn { get; init; }
}

/// <summary>
/// <c>differenceInSeconds</c>/<c>differenceInMinutes</c>/<c>differenceInHours</c> 的选项对象。
/// Options for <c>differenceInSeconds</c>/<c>differenceInMinutes</c>/<c>differenceInHours</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsRoundingOptions
{
    /// <summary>数值取整方法。The numeric rounding method.</summary>
    [Description("@#roundingMethod")]
    public DateFnsRoundingMethod? RoundingMethod { get; init; }
}

/// <summary>
/// <c>eachDayOfInterval</c> 的选项对象。
/// Options for <c>eachDayOfInterval</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsEachDayOfIntervalOptions
{
    /// <summary>迭代步长。The step used when iterating days.</summary>
    [Description("@#step")]
    public Number? Step { get; init; }
}

/// <summary>
/// <c>areIntervalsOverlapping</c> 的选项对象。
/// Options for <c>areIntervalsOverlapping</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsAreIntervalsOverlappingOptions
{
    /// <summary>比较是否包含区间端点。Whether the comparison includes the interval endpoints.</summary>
    [Description("@#inclusive")]
    public bool? Inclusive { get; init; }
}

/// <summary>
/// date-fns 模块级默认选项；通过 <c>setDefaultOptions</c>/<c>getDefaultOptions</c> 读写。
/// The date-fns module-level default options, read and written via <c>setDefaultOptions</c>/<c>getDefaultOptions</c>.
/// </summary>
[ECMAScript]
[Description("@#")]
public record DateFnsDefaultOptions
{
    /// <summary>默认使用的 locale。The locale used by default.</summary>
    [Description("@#locale")]
    public DateFnsLocaleObject? Locale { get; init; }

    /// <summary>默认每周起始日。The default day the week starts on.</summary>
    [Description("@#weekStartsOn")]
    public DateFnsWeekStartsOn? WeekStartsOn { get; init; }

    /// <summary>默认年内第一周必须包含的日期。The default day the first week of the year must contain.</summary>
    [Description("@#firstWeekContainsDate")]
    public DateFnsFirstWeekContainsDate? FirstWeekContainsDate { get; init; }
}
