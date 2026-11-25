namespace Jero.Core;

/// <summary>
/// 时间帮助类
/// </summary>
public class DateTimeUtil
{
    public readonly DateTime Date;

    private DateTimeUtil(TimeSpan timeSpan = default)
    {
        Date = DateTime.Now.AddTicks(timeSpan.Ticks);
    }

    private DateTimeUtil(DateTime time)
    {
        Date = time;
    }

    /// <summary>
    /// 实例化类
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static DateTimeUtil Init(TimeSpan timeSpan = default)
    {
        return new DateTimeUtil(timeSpan);
    }

    /// <summary>
    /// 实例化类
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTimeUtil Init(DateTime time)
    {
        return new DateTimeUtil(time);
    }

    /// <summary>
    /// 根据unix时间戳的长度自动判断是秒还是以毫秒为单位
    /// </summary>
    /// <param name="unixTime"></param>
    /// <returns></returns>
    public static DateTime ConvertUnixTime(long unixTime)
    {
        // 判断时间戳长度
        bool isMilliseconds = unixTime > 9999999999;

        if (isMilliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).ToLocalTime().DateTime;
        }
        else
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime().DateTime;
        }
    }

    /// <summary>
    /// 获取开始时间
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="days"></param>
    /// <returns></returns>
    public static DateTime GetBeginTime(DateTime? dateTime, int days = 0)
    {
        return dateTime == DateTime.MinValue || dateTime == null ? DateTime.Now.AddDays(days) : (DateTime)dateTime;
    }

    /// <summary>
    ///  时间戳转本地时间-时间戳精确到秒
    /// </summary>
    public static DateTime ToLocalTimeDateBySeconds(long unix)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unix).ToLocalTime().DateTime;
    }

    /// <summary>
    ///  时间转时间戳Unix-时间戳精确到秒
    /// </summary>
    public static long ToUnixTimestampBySeconds(DateTime dt)
    {
        return new DateTimeOffset(dt).ToUnixTimeSeconds();
    }

    /// <summary>
    ///  时间戳转本地时间-时间戳精确到毫秒
    /// </summary>
    public static DateTime ToLocalTimeDateByMilliseconds(long unix)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(unix).ToLocalTime().DateTime;
    }

    /// <summary>
    ///  时间转时间戳Unix-时间戳精确到毫秒
    /// </summary>
    public static long ToUnixTimestampByMilliseconds(DateTime dt)
    {
        return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// 毫秒转天时分秒
    /// </summary>
    /// <param name="ms">TotalMilliseconds</param>
    /// <param name="isSimplify">是否简化显示</param>
    /// <returns></returns>
    public static string FormatTime(long ms, bool isSimplify = false)
    {
        int ss = 1000;
        int mi = ss * 60;
        int hh = mi * 60;
        int dd = hh * 24;

        long day = ms / dd;
        long hour = (ms - day * dd) / hh;
        long minute = (ms - day * dd - hour * hh) / mi;
        long second = (ms - day * dd - hour * hh - minute * mi) / ss;
        //long milliSecond = ms - day * dd - hour * hh - minute * mi - second * ss;

        string sDay = day < 10 ? "0" + day : "" + day; //天
        string sHour = hour < 10 ? "0" + hour : "" + hour;//小时
        string sMinute = minute < 10 ? "0" + minute : "" + minute;//分钟
        string sSecond = second < 10 ? "0" + second : "" + second;//秒
        //string sMilliSecond = milliSecond < 10 ? "0" + milliSecond : "" + milliSecond;//毫秒
        //sMilliSecond = milliSecond < 100 ? "0" + sMilliSecond : "" + sMilliSecond;

        if (!isSimplify)
            return $"{sDay} 天 {sHour} 小时 {sMinute} 分 {sSecond} 秒";
        else
        {
            string result = string.Empty;
            if (day > 0)
                result = $"{sDay}天";
            if (hour > 0)
                result = $"{result}{sHour}小时";
            if (minute > 0)
                result = $"{result}{sMinute}分";
            if (!result.IsNullOrEmpty())
                result = $"{result}{sSecond}秒";
            else
                result = $"{sSecond}秒";
            return result;
        }
    }

    /// <summary>
    /// 获取unix时间戳
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static long GetUnixTimeStamp(DateTime dt)
    {
        return ((DateTimeOffset)dt).ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// 获取日期天的最小时间
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static DateTime GetDayMinDate(DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
    }

    /// <summary>
    /// 获取日期天的最大时间
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>

    public static DateTime GetDayMaxDate(DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
    }

    /// <summary>
    /// 根据日期是否在当前年份来格式化日期
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string FormatDateTime(DateTime? dt)
    {
        return dt == null ? string.Empty : dt.Value.ToString(dt.Value.Year == DateTime.Now.Year ? "MM-dd HH:mm" : "yyyy-MM-dd HH:mm");
    }

    /// <summary>
    /// 获取日期范围00:00:00 - 23:59:59
    /// </summary>
    /// <returns></returns>
    public static List<DateTime> GetTodayTimeList(DateTime time)
    {
        return new List<DateTime>
        {
            Convert.ToDateTime(time.ToString("D")),
            Convert.ToDateTime(time.AddDays(1).ToString("D")).AddSeconds(-1)
        };
    }

    /// <summary>
    /// 获取星期几
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static string GetWeekByDate(DateTime dt)
    {
        var day = new[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
        return day[Convert.ToInt32(dt.DayOfWeek.ToString("d"))];
    }

    /// <summary>
    /// 获取这个月的第几周
    /// </summary>
    /// <param name="daytime"></param>
    /// <returns></returns>
    public static int GetWeekNumInMonth(DateTime daytime)
    {
        int dayInMonth = daytime.Day;
        // 本月第一天
        DateTime firstDay = daytime.AddDays(1 - daytime.Day);
        // 本月第一天是周几
        int weekday = (int)firstDay.DayOfWeek == 0 ? 7 : (int)firstDay.DayOfWeek;
        // 本月第一周有几天
        int firstWeekEndDay = 7 - (weekday - 1);
        // 当前日期和第一周之差
        int diffday = dayInMonth - firstWeekEndDay;
        diffday = diffday > 0 ? diffday : 1;
        // 当前是第几周，若整除7就减一天
        return ((diffday % 7) == 0 ? (diffday / 7 - 1) : (diffday / 7)) + 1 + (dayInMonth > firstWeekEndDay ? 1 : 0);
    }

    /// <summary>
    /// 获取今天的时间范围
    /// </summary>
    /// <returns>返回包含开始时间和结束时间的元组</returns>
    public (DateTime Start, DateTime End) GetTodayRange()
    {
        var start = Date.Date; // 当天开始时间
        var end = start.AddDays(1).AddSeconds(-1); // 当天结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取本月的时间范围
    /// </summary>
    /// <returns>返回包含开始时间和结束时间的元组</returns>
    public (DateTime Start, DateTime End) GetMonthRange()
    {
        return (GetFirstDayOfMonth(), GetLastDayOfMonth());
    }

    /// <summary>
    /// 获取本月的第一天开始时间
    /// </summary>
    /// <returns>返回当月的第一天</returns>
    public DateTime GetFirstDayOfMonth()
    {
        return new DateTime(Date.Year, Date.Month, 1);
    }

    /// <summary>
    /// 获取本月的最后一天截至时间
    /// </summary>
    /// <returns>返回当月的最后一天</returns>
    public DateTime GetLastDayOfMonth()
    {
        var firstDayOfNextMonth = new DateTime(Date.Year, Date.Month, 1).AddMonths(1);
        return firstDayOfNextMonth.AddSeconds(-1);
    }

    /// <summary>
    /// 获取今年的时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetYearRange()
    {
        return (GetFirstDayOfYear(), GetLastDayOfYear());
    }

    /// <summary>
    /// 获取今年的第一天时间范围
    /// </summary>
    public DateTime GetFirstDayOfYear()
    {
        return new DateTime(Date.Year, 1, 1);
    }

    /// <summary>
    /// 获取今年的最后一天时间范围
    /// </summary>
    public DateTime GetLastDayOfYear()
    {
        return new DateTime(Date.Year, 12, 31, 23, 59, 59);
    }

    /// <summary>
    /// 获取前天时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetDayBeforeYesterdayRange()
    {
        var start = Date.Date.AddDays(-2); // 前天开始时间
        var end = start.AddDays(1).AddSeconds(-1); // 前天结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取昨天时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetYesterdayRange()
    {
        var start = Date.Date.AddDays(-1); // 昨天开始时间
        var end = start.AddDays(1).AddSeconds(-1); // 昨天结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取上一周时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetLastWeekRange()
    {
        // 计算上周的天数差
        var daysToSubtract = (int)Date.DayOfWeek + 7; // 确保周日也能正确计算
        var start = Date.Date.AddDays(-daysToSubtract); // 上周第一天
        var end = start.AddDays(7).AddSeconds(-1); // 上周最后一天
        return (start, end);
    }

    /// <summary>
    /// 获取本周时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetThisWeekRange()
    {
        // 计算本周的天数差
        var daysToSubtract = (int)Date.DayOfWeek;
        var start = Date.Date.AddDays(-daysToSubtract); // 本周第一天
        var end = start.AddDays(7).AddSeconds(-1); // 本周最后一天
        return (start, end);
    }

    /// <summary>
    /// 获取上月时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetLastMonthRange()
    {
        var firstDayOfLastMonth = new DateTime(Date.Year, Date.Month, 1).AddMonths(-1); // 上月第一天
        var lastDayOfLastMonth = firstDayOfLastMonth.AddMonths(1).AddSeconds(-1); // 上月最后一天
        return (firstDayOfLastMonth, lastDayOfLastMonth);
    }

    /// <summary>
    /// 获取近3天的时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetLast3DaysRange()
    {
        var start = Date.Date.AddDays(-2); // 3天前的开始时间
        var end = Date.Date.AddDays(1).AddSeconds(-1); // 当前日期的结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取近7天的时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetLast7DaysRange()
    {
        var start = Date.Date.AddDays(-6); // 7天前的开始时间
        var end = Date.Date.AddDays(1).AddSeconds(-1); // 当前日期的结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取近15天的时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetLast15DaysRange()
    {
        var start = Date.Date.AddDays(-14); // 15天前的开始时间
        var end = Date.Date.AddDays(1).AddSeconds(-1); // 当前日期的结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取近3个月的时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetLast3MonthsRange()
    {
        var start = Date.Date.AddMonths(-3); // 3个月前的开始时间
        var end = Date.Date.AddDays(1).AddSeconds(-1); // 当前日期的结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取上半年的时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetFirstHalfYearRange()
    {
        var start = new DateTime(Date.Year, 1, 1); // 上半年开始时间
        var end = new DateTime(Date.Year, 6, 30, 23, 59, 59); // 上半年结束时间
        return (start, end);
    }

    /// <summary>
    /// 获取下半年的时间范围
    /// </summary>
    public (DateTime Start, DateTime End) GetSecondHalfYearRange()
    {
        var start = new DateTime(Date.Year, 7, 1); // 下半年开始时间
        var end = new DateTime(Date.Year, 12, 31, 23, 59, 59); // 下半年结束时间
        return (start, end);
    }
}