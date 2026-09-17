using System.ComponentModel;

namespace ECMAScript;

public static partial class DateFns
{
    // ---------- 解析 Parsing ----------

    /// <summary>
    /// 将参数转换为 Date 实例；数字按时间戳解释，其余类型返回 Invalid Date。
    /// Converts the argument to a Date instance; numbers are treated as timestamps and other types yield an Invalid Date.
    /// </summary>
    [Description("@#toDate")]
    public extern static Date ToDate(Date argument);

    /// <summary>
    /// 按格式字符串解析日期文本；token 规则遵循 Unicode TR35，与 Moment.js 不一致。
    /// Parses a date string against a format string; tokens follow Unicode TR35 and differ from Moment.js.
    /// </summary>
    [Description("@#parse")]
    public extern static Date Parse(string dateStr, string formatStr, Date referenceDate, DateFnsParseOptions? options = null);

    /// <summary>
    /// 解析 ISO 8601 日期文本。
    /// Parses an ISO 8601 date string.
    /// </summary>
    [Description("@#parseISO")]
    public extern static Date ParseISO(string argument, DateFnsParseISOOptions? options = null);

    /// <summary>
    /// 解析 <c>JSON.stringify(new Date(...))</c> 产生的日期文本。
    /// Parses the date string shape produced by <c>JSON.stringify(new Date(...))</c>.
    /// </summary>
    [Description("@#parseJSON")]
    public extern static Date ParseJSON(string dateStr);

    /// <summary>
    /// 将 Unix 秒级时间戳转换为 Date。
    /// Converts a Unix timestamp in seconds to a Date.
    /// </summary>
    [Description("@#fromUnixTime")]
    public extern static Date FromUnixTime(Number unixTime);

    // ---------- 格式化 Formatting ----------

    /// <summary>
    /// 按 Unicode TR35 token 格式化日期。
    /// Formats a date with Unicode TR35 tokens.
    /// </summary>
    [Description("@#format")]
    public extern static string Format(Date date, string formatStr, DateFnsFormatOptions? options = null);

    /// <summary>
    /// 按 ISO 8601 标准格式化日期。
    /// Formats a date according to the ISO 8601 standard.
    /// </summary>
    [Description("@#formatISO")]
    public extern static string FormatISO(Date date, DateFnsFormatISOOptions? options = null);

    /// <summary>
    /// 以自然语言返回两个日期之间的距离。
    /// Returns the distance between two dates in words.
    /// </summary>
    [Description("@#formatDistance")]
    public extern static string FormatDistance(Date laterDate, Date earlierDate, DateFnsFormatDistanceOptions? options = null);

    /// <summary>
    /// 以严格单位返回两个日期之间的距离。
    /// Returns the distance between two dates in words using strict units.
    /// </summary>
    [Description("@#formatDistanceStrict")]
    public extern static string FormatDistanceStrict(Date laterDate, Date earlierDate, DateFnsFormatDistanceStrictOptions? options = null);

    /// <summary>
    /// 以自然语言返回日期与当前时间的距离。
    /// Returns the distance between the date and now in words.
    /// </summary>
    [Description("@#formatDistanceToNow")]
    public extern static string FormatDistanceToNow(Date date, DateFnsFormatDistanceOptions? options = null);

    /// <summary>
    /// 以严格单位返回日期与当前时间的距离。
    /// Returns the distance between the date and now in words using strict units.
    /// </summary>
    [Description("@#formatDistanceToNowStrict")]
    public extern static string FormatDistanceToNowStrict(Date date, DateFnsFormatDistanceStrictOptions? options = null);

    /// <summary>
    /// 将时长对象格式化为自然语言。
    /// Formats a duration object in words.
    /// </summary>
    [Description("@#formatDuration")]
    public extern static string FormatDuration(DateFnsDuration duration, DateFnsFormatDurationOptions? options = null);

    /// <summary>
    /// 以相对基准日期的自然语言描述日期（如“昨天”、“上周”）。
    /// Represents the date in words relative to the base date (for example "yesterday", "last week").
    /// </summary>
    [Description("@#formatRelative")]
    public extern static string FormatRelative(Date date, Date baseDate, DateFnsFormatRelativeOptions? options = null);

    // ---------- 加减 Arithmetic ----------

    /// <summary>
    /// 按时长对象的各分量叠加日期。
    /// Adds the components of a duration object to the date.
    /// </summary>
    [Description("@#add")]
    public extern static Date Add(Date date, DateFnsDuration duration);

    /// <summary>
    /// 按时长对象的各分量扣减日期。
    /// Subtracts the components of a duration object from the date.
    /// </summary>
    [Description("@#sub")]
    public extern static Date Sub(Date date, DateFnsDuration duration);

    /// <summary>
    /// 增加指定天数。
    /// Adds the specified number of days.
    /// </summary>
    [Description("@#addDays")]
    public extern static Date AddDays(Date date, Number amount);

    /// <summary>
    /// 增加指定小时数。
    /// Adds the specified number of hours.
    /// </summary>
    [Description("@#addHours")]
    public extern static Date AddHours(Date date, Number amount);

    /// <summary>
    /// 增加指定毫秒数。
    /// Adds the specified number of milliseconds.
    /// </summary>
    [Description("@#addMilliseconds")]
    public extern static Date AddMilliseconds(Date date, Number amount);

    /// <summary>
    /// 增加指定分钟数。
    /// Adds the specified number of minutes.
    /// </summary>
    [Description("@#addMinutes")]
    public extern static Date AddMinutes(Date date, Number amount);

    /// <summary>
    /// 增加指定月数，月末日期按 JavaScript 规则溢出归一化。
    /// Adds the specified number of months; month-end overflow follows JavaScript normalization.
    /// </summary>
    [Description("@#addMonths")]
    public extern static Date AddMonths(Date date, Number amount);

    /// <summary>
    /// 增加指定季度数。
    /// Adds the specified number of quarters.
    /// </summary>
    [Description("@#addQuarters")]
    public extern static Date AddQuarters(Date date, Number amount);

    /// <summary>
    /// 增加指定秒数。
    /// Adds the specified number of seconds.
    /// </summary>
    [Description("@#addSeconds")]
    public extern static Date AddSeconds(Date date, Number amount);

    /// <summary>
    /// 增加指定周数。
    /// Adds the specified number of weeks.
    /// </summary>
    [Description("@#addWeeks")]
    public extern static Date AddWeeks(Date date, Number amount);

    /// <summary>
    /// 增加指定年数。
    /// Adds the specified number of years.
    /// </summary>
    [Description("@#addYears")]
    public extern static Date AddYears(Date date, Number amount);

    /// <summary>
    /// 增加指定工作日数，跳过周末。
    /// Adds the specified number of business days, skipping weekends.
    /// </summary>
    [Description("@#addBusinessDays")]
    public extern static Date AddBusinessDays(Date date, Number amount);

    /// <summary>
    /// 扣减指定天数。
    /// Subtracts the specified number of days.
    /// </summary>
    [Description("@#subDays")]
    public extern static Date SubDays(Date date, Number amount);

    /// <summary>
    /// 扣减指定小时数。
    /// Subtracts the specified number of hours.
    /// </summary>
    [Description("@#subHours")]
    public extern static Date SubHours(Date date, Number amount);

    /// <summary>
    /// 扣减指定毫秒数。
    /// Subtracts the specified number of milliseconds.
    /// </summary>
    [Description("@#subMilliseconds")]
    public extern static Date SubMilliseconds(Date date, Number amount);

    /// <summary>
    /// 扣减指定分钟数。
    /// Subtracts the specified number of minutes.
    /// </summary>
    [Description("@#subMinutes")]
    public extern static Date SubMinutes(Date date, Number amount);

    /// <summary>
    /// 扣减指定月数。
    /// Subtracts the specified number of months.
    /// </summary>
    [Description("@#subMonths")]
    public extern static Date SubMonths(Date date, Number amount);

    /// <summary>
    /// 扣减指定季度数。
    /// Subtracts the specified number of quarters.
    /// </summary>
    [Description("@#subQuarters")]
    public extern static Date SubQuarters(Date date, Number amount);

    /// <summary>
    /// 扣减指定秒数。
    /// Subtracts the specified number of seconds.
    /// </summary>
    [Description("@#subSeconds")]
    public extern static Date SubSeconds(Date date, Number amount);

    /// <summary>
    /// 扣减指定周数。
    /// Subtracts the specified number of weeks.
    /// </summary>
    [Description("@#subWeeks")]
    public extern static Date SubWeeks(Date date, Number amount);

    /// <summary>
    /// 扣减指定年数。
    /// Subtracts the specified number of years.
    /// </summary>
    [Description("@#subYears")]
    public extern static Date SubYears(Date date, Number amount);

    /// <summary>
    /// 扣减指定工作日数，跳过周末。
    /// Subtracts the specified number of business days, skipping weekends.
    /// </summary>
    [Description("@#subBusinessDays")]
    public extern static Date SubBusinessDays(Date date, Number amount);

    // ---------- 比较 Comparison ----------

    /// <summary>
    /// 判断日期是否早于比较日期。
    /// Determines whether the date is before the comparison date.
    /// </summary>
    [Description("@#isBefore")]
    public extern static bool IsBefore(Date date, Date dateToCompare);

    /// <summary>
    /// 判断日期是否晚于比较日期。
    /// Determines whether the date is after the comparison date.
    /// </summary>
    [Description("@#isAfter")]
    public extern static bool IsAfter(Date date, Date dateToCompare);

    /// <summary>
    /// 按时间值判断两个日期是否相等。
    /// Determines whether two dates are equal by their time values.
    /// </summary>
    [Description("@#isEqual")]
    public extern static bool IsEqual(Date leftDate, Date rightDate);

    /// <summary>
    /// 判断值是否为 Date 实例；参数域为任意 JavaScript 值，与 <see cref="Global.TypeOf"/> 的宿主约定一致。
    /// Determines whether the value is a Date instance; the domain is any JavaScript value, matching the <see cref="Global.TypeOf"/> host convention.
    /// </summary>
    [Description("@#isDate")]
    public extern static bool IsDate(object? value);

    /// <summary>
    /// 判断日期是否有效；Invalid Date 返回 false。
    /// Determines whether the date is valid; an Invalid Date returns false.
    /// </summary>
    [Description("@#isValid")]
    public extern static bool IsValid(Date date);

    /// <summary>
    /// 比较两个日期，早者返回 -1、相等返回 0、晚者返回 1。
    /// Compares two dates; returns -1 when earlier, 0 when equal, and 1 when later.
    /// </summary>
    [Description("@#compareAsc")]
    public extern static Number CompareAsc(Date dateLeft, Date dateRight);

    /// <summary>
    /// 按降序比较两个日期，晚者返回 -1、相等返回 0、早者返回 1。
    /// Compares two dates in descending order; returns -1 when later, 0 when equal, and 1 when earlier.
    /// </summary>
    [Description("@#compareDesc")]
    public extern static Number CompareDesc(Date dateLeft, Date dateRight);

    // ---------- 区间 Interval ----------

    /// <summary>
    /// 返回两个日期之间的毫秒差。
    /// Returns the difference between two dates in milliseconds.
    /// </summary>
    [Description("@#differenceInMilliseconds")]
    public extern static Number DifferenceInMilliseconds(Date laterDate, Date earlierDate);

    /// <summary>
    /// 返回两个日期之间的秒差。
    /// Returns the difference between two dates in seconds.
    /// </summary>
    [Description("@#differenceInSeconds")]
    public extern static Number DifferenceInSeconds(Date laterDate, Date earlierDate, DateFnsRoundingOptions? options = null);

    /// <summary>
    /// 返回两个日期之间的分钟差。
    /// Returns the difference between two dates in minutes.
    /// </summary>
    [Description("@#differenceInMinutes")]
    public extern static Number DifferenceInMinutes(Date laterDate, Date earlierDate, DateFnsRoundingOptions? options = null);

    /// <summary>
    /// 返回两个日期之间的小时差。
    /// Returns the difference between two dates in hours.
    /// </summary>
    [Description("@#differenceInHours")]
    public extern static Number DifferenceInHours(Date laterDate, Date earlierDate, DateFnsRoundingOptions? options = null);

    /// <summary>
    /// 返回两个日期之间的整天差。
    /// Returns the difference between two dates in full days.
    /// </summary>
    [Description("@#differenceInDays")]
    public extern static Number DifferenceInDays(Date laterDate, Date earlierDate);

    /// <summary>
    /// 返回两个日期之间的整周差。
    /// Returns the difference between two dates in full weeks.
    /// </summary>
    [Description("@#differenceInWeeks")]
    public extern static Number DifferenceInWeeks(Date laterDate, Date earlierDate);

    /// <summary>
    /// 返回两个日期之间的整月差。
    /// Returns the difference between two dates in full months.
    /// </summary>
    [Description("@#differenceInMonths")]
    public extern static Number DifferenceInMonths(Date laterDate, Date earlierDate);

    /// <summary>
    /// 返回两个日期之间的整年差。
    /// Returns the difference between two dates in full years.
    /// </summary>
    [Description("@#differenceInYears")]
    public extern static Number DifferenceInYears(Date laterDate, Date earlierDate);

    /// <summary>
    /// 将区间转换为单位化时长对象。
    /// Converts an interval into a unit-normalized duration object.
    /// </summary>
    [Description("@#intervalToDuration")]
    public extern static DateFnsDuration IntervalToDuration(DateFnsInterval interval);

    /// <summary>
    /// 判断日期是否落在区间内（含端点）。
    /// Determines whether the date falls within the interval, endpoints included.
    /// </summary>
    [Description("@#isWithinInterval")]
    public extern static bool IsWithinInterval(Date date, DateFnsInterval interval);

    /// <summary>
    /// 判断两个区间是否重叠。
    /// Determines whether two intervals overlap.
    /// </summary>
    [Description("@#areIntervalsOverlapping")]
    public extern static bool AreIntervalsOverlapping(DateFnsInterval intervalLeft, DateFnsInterval intervalRight, DateFnsAreIntervalsOverlappingOptions? options = null);

    /// <summary>
    /// 返回区间内按步长迭代的日期数组。
    /// Returns the array of dates inside the interval iterated by the configured step.
    /// </summary>
    [Description("@#eachDayOfInterval")]
    public extern static Date[] EachDayOfInterval(DateFnsInterval interval, DateFnsEachDayOfIntervalOptions? options = null);

    /// <summary>
    /// 返回日期数组中最早的有效日期。
    /// Returns the earliest valid date of the array.
    /// </summary>
    [Description("@#min")]
    public extern static Date Min(Date[] dates);

    /// <summary>
    /// 返回日期数组中最晚的有效日期。
    /// Returns the latest valid date of the array.
    /// </summary>
    [Description("@#max")]
    public extern static Date Max(Date[] dates);

    /// <summary>
    /// 将日期限制在区间端点之间。
    /// Clamps the date to the interval endpoints.
    /// </summary>
    [Description("@#clamp")]
    public extern static Date Clamp(Date date, DateFnsInterval interval);

    // ---------- 模块默认选项 Module defaults ----------

    /// <summary>
    /// 读取 date-fns 模块级默认选项。
    /// Reads the date-fns module-level default options.
    /// </summary>
    [Description("@#getDefaultOptions")]
    public extern static DateFnsDefaultOptions GetDefaultOptions();

    /// <summary>
    /// 设置 date-fns 模块级默认选项；影响后续未显式传参的函数调用。
    /// Sets the date-fns module-level default options, affecting subsequent calls that omit explicit options.
    /// </summary>
    [Description("@#setDefaultOptions")]
    public extern static void SetDefaultOptions(DateFnsDefaultOptions options);
}
