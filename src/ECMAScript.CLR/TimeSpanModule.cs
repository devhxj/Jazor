using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.TimeSpan")]
public static class TimeSpanModule
{
	//System.TimeSpan.NanosecondsPerTick = 100;

	//System.TimeSpan.TicksPerMicrosecond = 10;

	//System.TimeSpan.TicksPerMillisecond = 10000;

	//System.TimeSpan.TicksPerSecond = 10000000;

	//System.TimeSpan.TicksPerMinute = 600000000;

	//System.TimeSpan.TicksPerHour = 36000000000;

	//System.TimeSpan.TicksPerDay = 864000000000;

	//System.TimeSpan.MicrosecondsPerMillisecond = 1000;

	//System.TimeSpan.MicrosecondsPerSecond = 1000000;

	//System.TimeSpan.MicrosecondsPerMinute = 60000000;

	//System.TimeSpan.MicrosecondsPerHour = 3600000000;

	//System.TimeSpan.MicrosecondsPerDay = 86400000000;

	//System.TimeSpan.MillisecondsPerSecond = 1000;

	//System.TimeSpan.MillisecondsPerMinute = 60000;

	//System.TimeSpan.MillisecondsPerHour = 3600000;

	//System.TimeSpan.MillisecondsPerDay = 86400000;

	//System.TimeSpan.SecondsPerMinute = 60;

	//System.TimeSpan.SecondsPerHour = 3600;

	//System.TimeSpan.SecondsPerDay = 86400;

	//System.TimeSpan.MinutesPerHour = 60;

	//System.TimeSpan.MinutesPerDay = 1440;

	//System.TimeSpan.HoursPerDay = 24;

    ///<summary>Represents the zero <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>    [WhiteList("static readonly System.TimeSpan.Zero")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanZero();

    ///<summary>Represents the maximum <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>    [WhiteList("static readonly System.TimeSpan.MaxValue")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanMaxValue();

    ///<summary>Represents the minimum <see cref="T:System.TimeSpan" /> value. This field is read-only.</summary>    [WhiteList("static readonly System.TimeSpan.MinValue")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanMinValue();

    [WhiteList("System.TimeSpan.TimeSpan()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanNew();

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to the specified number of ticks.</summary>
    ///<param name="ticks">A time period expressed in 100-nanosecond units.</param>    [WhiteList("System.TimeSpan.TimeSpan(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanNew2(BigInt ticks);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours, minutes, and seconds.</summary>
    ///<param name="hours">Number of hours.</param>
    ///<param name="minutes">Number of minutes.</param>
    ///<param name="seconds">Number of seconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>    [WhiteList("System.TimeSpan.TimeSpan(int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanNew3(Number hours, Number minutes, Number seconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, and seconds.</summary>
    ///<param name="days">Number of days.</param>
    ///<param name="hours">Number of hours.</param>
    ///<param name="minutes">Number of minutes.</param>
    ///<param name="seconds">Number of seconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>    [WhiteList("System.TimeSpan.TimeSpan(int, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanNew4(Number days, Number hours, Number minutes, Number seconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, and milliseconds.</summary>
    ///<param name="days">Number of days.</param>
    ///<param name="hours">Number of hours.</param>
    ///<param name="minutes">Number of minutes.</param>
    ///<param name="seconds">Number of seconds.</param>
    ///<param name="milliseconds">Number of milliseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>    [WhiteList("System.TimeSpan.TimeSpan(int, int, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanNew5(Number days, Number hours, Number minutes, Number seconds, Number milliseconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</summary>
    ///<param name="days">Number of days.</param>
    ///<param name="hours">Number of hours.</param>
    ///<param name="minutes">Number of minutes.</param>
    ///<param name="seconds">Number of seconds.</param>
    ///<param name="milliseconds">Number of milliseconds.</param>
    ///<param name="microseconds">Number of microseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>    [WhiteList("System.TimeSpan.TimeSpan(int, int, int, int, int, int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanNew6(Number days, Number hours, Number minutes, Number seconds, Number milliseconds, Number microseconds);

    [WhiteList("System.TimeSpan.Ticks.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanGetTicks(BigInt instance);

    [WhiteList("System.TimeSpan.Days.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetDays(BigInt instance);

    [WhiteList("System.TimeSpan.Hours.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetHours(BigInt instance);

    [WhiteList("System.TimeSpan.Milliseconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetMilliseconds(BigInt instance);

    [WhiteList("System.TimeSpan.Microseconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetMicroseconds(BigInt instance);

    [WhiteList("System.TimeSpan.Nanoseconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetNanoseconds(BigInt instance);

    [WhiteList("System.TimeSpan.Minutes.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetMinutes(BigInt instance);

    [WhiteList("System.TimeSpan.Seconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetSeconds(BigInt instance);

    [WhiteList("System.TimeSpan.TotalDays.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetTotalDays(BigInt instance);

    [WhiteList("System.TimeSpan.TotalHours.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetTotalHours(BigInt instance);

    [WhiteList("System.TimeSpan.TotalMilliseconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetTotalMilliseconds(BigInt instance);

    [WhiteList("System.TimeSpan.TotalMicroseconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetTotalMicroseconds(BigInt instance);

    [WhiteList("System.TimeSpan.TotalNanoseconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetTotalNanoseconds(BigInt instance);

    [WhiteList("System.TimeSpan.TotalMinutes.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetTotalMinutes(BigInt instance);

    [WhiteList("System.TimeSpan.TotalSeconds.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetTotalSeconds(BigInt instance);

    ///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the sum of the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
    ///<param name="ts">The time interval to add.</param>
    ///<exception cref="T:System.OverflowException">The resulting <see cref="T:System.TimeSpan" /> is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>
    ///<returns>A new object that represents the value of this instance plus the value of <paramref name="ts" />.</returns>    [WhiteList("System.TimeSpan.Add(System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanAdd(BigInt instance, BigInt ts);

    ///<summary>Compares two <see cref="T:System.TimeSpan" /> values and returns an integer that indicates whether the first value is shorter than, equal to, or longer than the second value.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>One of the following values. <list type="table"><listheader><term> Value</term><description> Description</description></listheader><item><term> -1</term><description><paramref name="t1" /> is shorter than <paramref name="t2" />.</description></item><item><term> 0</term><description><paramref name="t1" /> is equal to <paramref name="t2" />.</description></item><item><term> 1</term><description><paramref name="t1" /> is longer than <paramref name="t2" />.</description></item></list></returns>    [WhiteList("static System.TimeSpan.Compare(System.TimeSpan, System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanCompare(BigInt t1, BigInt t2);

    ///<summary>Compares this instance to a specified object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the specified object.</summary>
    ///<param name="value">An object to compare, or <see langword="null" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is not a <see cref="T:System.TimeSpan" />.</exception>
    ///<returns>One of the following values. <list type="table"><listheader><term> Value</term><description> Description</description></listheader><item><term> -1</term><description> This instance is shorter than <paramref name="value" />.</description></item><item><term> 0</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> 1</term><description> This instance is longer than <paramref name="value" />, or <paramref name="value" /> is <see langword="null" />.</description></item></list></returns>    [WhiteList("System.TimeSpan.CompareTo(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanCompareTo(BigInt instance, Object? value);

    ///<summary>Compares this instance to a specified <see cref="T:System.TimeSpan" /> object and returns an integer that indicates whether this instance is shorter than, equal to, or longer than the <see cref="T:System.TimeSpan" /> object.</summary>
    ///<param name="value">An object to compare to this instance.</param>
    ///<returns>A signed number indicating the relative values of this instance and <paramref name="value" />. <list type="table"><listheader><term> Value</term><description> Description</description></listheader><item><term> A negative integer</term><description> This instance is shorter than <paramref name="value" />.</description></item><item><term> Zero</term><description> This instance is equal to <paramref name="value" />.</description></item><item><term> A positive integer</term><description> This instance is longer than <paramref name="value" />.</description></item></list></returns>    [WhiteList("System.TimeSpan.CompareTo(System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanCompareTo2(BigInt instance, BigInt value);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of days, where the specification is accurate to the nearest millisecond.</summary>
    ///<param name="value">A number of days, accurate to the nearest millisecond.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />. -or- <paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />.</exception>
    ///<returns>An object that represents <paramref name="value" />.</returns>    [WhiteList("static System.TimeSpan.FromDays(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromDays(Number value);

    ///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the absolute value of the current <see cref="T:System.TimeSpan" /> object.</summary>
    ///<exception cref="T:System.OverflowException">The value of this instance is <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see>.</exception>
    ///<returns>A new object whose value is the absolute value of the current <see cref="T:System.TimeSpan" /> object.</returns>    [WhiteList("System.TimeSpan.Duration()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanDuration(BigInt instance);

    ///<summary>Returns a value indicating whether this instance is equal to a specified object.</summary>
    ///<param name="value">An object to compare with this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is a <see cref="T:System.TimeSpan" /> object that represents the same time interval as the current <see cref="T:System.TimeSpan" /> structure; otherwise, <see langword="false" />.</returns>    [WhiteList("override System.TimeSpan.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanEquals(BigInt instance, Object? value);

    ///<summary>Returns a value indicating whether this instance is equal to a specified <see cref="T:System.TimeSpan" /> object.</summary>
    ///<param name="obj">An object to compare with this instance.</param>
    ///<returns>  <see langword="true" /> if <paramref name="obj" /> represents the same time interval as this instance; otherwise, <see langword="false" />.</returns>    [WhiteList("System.TimeSpan.Equals(System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanEquals2(BigInt instance, BigInt obj);

    ///<summary>Returns a value that indicates whether two specified instances of <see cref="T:System.TimeSpan" /> are equal.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>  <see langword="true" /> if the values of <paramref name="t1" /> and <paramref name="t2" /> are equal; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.Equals(System.TimeSpan, System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanEquals3(BigInt t1, BigInt t2);

    ///<summary>Returns a hash code for this instance.</summary>
    ///<returns>A 32-bit signed integer hash code.</returns>    [WhiteList("override System.TimeSpan.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanGetHashCode(BigInt instance);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days.</summary>
    ///<param name="days">Number of days.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of days.</returns>    [WhiteList("static System.TimeSpan.FromDays(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromDays2(Number days);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</summary>
    ///<param name="days">Number of days.</param>
    ///<param name="hours">Number of hours.</param>
    ///<param name="minutes">Number of minutes.</param>
    ///<param name="seconds">Number of seconds.</param>
    ///<param name="milliseconds">Number of milliseconds.</param>
    ///<param name="microseconds">Number of microseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of days, hours, minutes, seconds, milliseconds, and microseconds.</returns>    [WhiteList("static System.TimeSpan.FromDays(int, int, long, long, long, long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromDays3(Number days, Number hours, BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours.</summary>
    ///<param name="hours">Number of hours.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of hours.</returns>    [WhiteList("static System.TimeSpan.FromHours(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromHours(Number hours);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of hours, minutes, seconds, milliseconds, and microseconds.</summary>
    ///<param name="hours">Number of hours.</param>
    ///<param name="minutes">Number of minutes.</param>
    ///<param name="seconds">Number of seconds.</param>
    ///<param name="milliseconds">Number of milliseconds.</param>
    ///<param name="microseconds">Number of microseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of hours, minutes, seconds, milliseconds, and microseconds.</returns>    [WhiteList("static System.TimeSpan.FromHours(int, long, long, long, long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromHours2(Number hours, BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of minutes.</summary>
    ///<param name="minutes">Number of minutes.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of minutes.</returns>    [WhiteList("static System.TimeSpan.FromMinutes(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMinutes(BigInt minutes);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of minutes, seconds, milliseconds, and microseconds.</summary>
    ///<param name="minutes">Number of minutes.</param>
    ///<param name="seconds">Number of seconds.</param>
    ///<param name="milliseconds">Number of milliseconds.</param>
    ///<param name="microseconds">Number of microseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of minutes, seconds, milliseconds, and microseconds.</returns>    [WhiteList("static System.TimeSpan.FromMinutes(long, long, long, long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMinutes2(BigInt minutes, BigInt seconds, BigInt milliseconds, BigInt microseconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of seconds.</summary>
    ///<param name="seconds">Number of seconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.</exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of seconds.</returns>    [WhiteList("static System.TimeSpan.FromSeconds(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromSeconds(BigInt seconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of seconds, milliseconds, and microseconds.</summary>
    ///<param name="seconds">Number of seconds.</param>
    ///<param name="milliseconds">Number of milliseconds.</param>
    ///<param name="microseconds">Number of microseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.</exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of seconds, milliseconds, and microseconds.</returns>    [WhiteList("static System.TimeSpan.FromSeconds(long, long, long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromSeconds2(BigInt seconds, BigInt milliseconds, BigInt microseconds);

    [WhiteList("static System.TimeSpan.FromMilliseconds(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMilliseconds(BigInt milliseconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of milliseconds, and microseconds.</summary>
    ///<param name="milliseconds">Number of milliseconds.</param>
    ///<param name="microseconds">Number of microseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of milliseconds, and microseconds.</returns>    [WhiteList("static System.TimeSpan.FromMilliseconds(long, long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMilliseconds2(BigInt milliseconds, BigInt microseconds);

    ///<summary>Initializes a new instance of the <see cref="T:System.TimeSpan" /> structure to a specified number of microseconds.</summary>
    ///<param name="microseconds">Number of microseconds.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">The parameters specify a <see cref="T:System.TimeSpan" /> value less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" /></exception>
    ///<returns>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of microseconds.</returns>    [WhiteList("static System.TimeSpan.FromMicroseconds(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMicroseconds(BigInt microseconds);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of hours, where the specification is accurate to the nearest millisecond.</summary>
    ///<param name="value">A number of hours accurate to the nearest millisecond.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />. -or- <paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />.</exception>
    ///<returns>An object that represents <paramref name="value" />.</returns>    [WhiteList("static System.TimeSpan.FromHours(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromHours3(Number value);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of milliseconds.</summary>
    ///<param name="value">A number of milliseconds.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />. -or- <paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />.</exception>
    ///<returns>An object that represents <paramref name="value" />.</returns>    [WhiteList("static System.TimeSpan.FromMilliseconds(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMilliseconds3(Number value);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of microseconds.</summary>
    ///<param name="value">A number of microseconds.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue" /> or greater than <see cref="F:System.TimeSpan.MaxValue" />.-or-<paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />-or-<paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" /></exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />.</exception>
    ///<returns>An object that represents <paramref name="value" />.</returns>    [WhiteList("static System.TimeSpan.FromMicroseconds(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMicroseconds2(Number value);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of minutes, where the specification is accurate to the nearest millisecond.</summary>
    ///<param name="value">A number of minutes, accurate to the nearest millisecond.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />. -or- <paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />.</exception>
    ///<returns>An object that represents <paramref name="value" />.</returns>    [WhiteList("static System.TimeSpan.FromMinutes(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromMinutes3(Number value);

    ///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the negated value of this instance.</summary>
    ///<exception cref="T:System.OverflowException">The negated value of this instance cannot be represented by a <see cref="T:System.TimeSpan" />; that is, the value of this instance is <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see>.</exception>
    ///<returns>A new object with the same numeric value as this instance, but with the opposite sign.</returns>    [WhiteList("System.TimeSpan.Negate()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanNegate(BigInt instance);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified number of seconds, where the specification is accurate to the nearest millisecond.</summary>
    ///<param name="value">A number of seconds, accurate to the nearest millisecond.</param>
    ///<exception cref="T:System.OverflowException">  <paramref name="value" /> is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- <paramref name="value" /> is <see cref="F:System.Double.PositiveInfinity" />. -or- <paramref name="value" /> is <see cref="F:System.Double.NegativeInfinity" />.</exception>
    ///<exception cref="T:System.ArgumentException">  <paramref name="value" /> is equal to <see cref="F:System.Double.NaN" />.</exception>
    ///<returns>An object that represents <paramref name="value" />.</returns>    [WhiteList("static System.TimeSpan.FromSeconds(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromSeconds3(Number value);

    ///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the difference between the specified <see cref="T:System.TimeSpan" /> object and this instance.</summary>
    ///<param name="ts">The time interval to be subtracted.</param>
    ///<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>
    ///<returns>A new time interval whose value is the result of the value of this instance minus the value of <paramref name="ts" />.</returns>    [WhiteList("System.TimeSpan.Subtract(System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanSubtract(BigInt instance, BigInt ts);

    ///<summary>Returns a new <see cref="T:System.TimeSpan" /> object which value is the result of multiplication of this instance and the specified <paramref name="factor" />.</summary>
    ///<param name="factor">The value to be multiplied by.</param>
    ///<returns>A new object that represents the value of this instance multiplied by the value of <paramref name="factor" />.</returns>    [WhiteList("System.TimeSpan.Multiply(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanMultiply(BigInt instance, Number factor);

    ///<summary>Returns a new <see cref="T:System.TimeSpan" /> object whose value is the result of dividing this instance by the specified <paramref name="divisor" />.</summary>
    ///<param name="divisor">The value to be divided by.</param>
    ///<returns>A new object that represents the value of this instance divided by the value of <paramref name="divisor" />.</returns>    [WhiteList("System.TimeSpan.Divide(double)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanDivide(BigInt instance, Number divisor);

    ///<summary>Returns a new <see cref="T:System.Double" /> value that's the result of dividing this instance by <paramref name="ts" />.</summary>
    ///<param name="ts">The value to be divided by.</param>
    ///<returns>A new value that represents result of dividing this instance by the value of <paramref name="ts" />.</returns>    [WhiteList("System.TimeSpan.Divide(System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number TimeSpanDivide2(BigInt instance, BigInt ts);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> that represents a specified time, where the specification is in units of ticks.</summary>
    ///<param name="value">A number of ticks that represent a time.</param>
    ///<returns>An object that represents <paramref name="value" />.</returns>    [WhiteList("static System.TimeSpan.FromTicks(long)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanFromTicks(BigInt value);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent.</summary>
    ///<param name="s">A string that specifies the time interval to convert.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="s" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="s" /> has an invalid format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="s" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- At least one of the days, hours, minutes, or seconds components is outside its valid range.</exception>
    ///<returns>A time interval that corresponds to <paramref name="s" />.</returns>    [WhiteList("static System.TimeSpan.Parse(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParse(string s);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific format information.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="input" /> has an invalid format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range.</exception>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formatProvider" />.</returns>    [WhiteList("static System.TimeSpan.Parse(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParse2(string input, Intl.NumberFormat? formatProvider);

    ///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific format information.</summary>
    ///<param name="input">A span containing the characters that represent the time interval to convert.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formatProvider" />.</returns>    [WhiteList("static System.TimeSpan.Parse(System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParse3(Uint32Array input, Intl.NumberFormat? formatProvider);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="input" /> has an invalid format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range.</exception>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>    [WhiteList("static System.TimeSpan.ParseExact(string, string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParseExact(string input, string format, Intl.NumberFormat? formatProvider);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified array of format strings and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="formats">An array of standard or custom format strings that defines the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="input" /> has an invalid format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range.</exception>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formats" /> and <paramref name="formatProvider" />.</returns>    [WhiteList("static System.TimeSpan.ParseExact(string, string[], System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParseExact2(string input, string[] formats, Intl.NumberFormat? formatProvider);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles. The format of the string representation must match the specified format exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input" />.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="styles" /> is an invalid <see cref="T:System.Globalization.TimeSpanStyles" /> value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="input" /> has an invalid format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range.</exception>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="format" />, <paramref name="formatProvider" />, and <paramref name="styles" />.</returns>    [WhiteList("static System.TimeSpan.ParseExact(string, string, System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParseExact3(string input, string format, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles);

    ///<summary>Converts the char span of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
    ///<param name="input">A span that specifies the time interval to convert.</param>
    ///<param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in <paramref name="input" />.</param>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>    [WhiteList("static System.TimeSpan.ParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParseExact4(Uint32Array input, Uint32Array format, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles. The format of the string representation must match one of the specified formats exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="formats">An array of standard or custom format strings that define the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in input.</param>
    ///<exception cref="T:System.ArgumentException">  <paramref name="styles" /> is an invalid <see cref="T:System.Globalization.TimeSpanStyles" /> value.</exception>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="input" /> is <see langword="null" />.</exception>
    ///<exception cref="T:System.FormatException">  <paramref name="input" /> has an invalid format.</exception>
    ///<exception cref="T:System.OverflowException">  <paramref name="input" /> represents a number that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>. -or- At least one of the days, hours, minutes, or seconds components in <paramref name="input" /> is outside its valid range.</exception>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formats" />, <paramref name="formatProvider" />, and <paramref name="styles" />.</returns>    [WhiteList("static System.TimeSpan.ParseExact(string, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParseExact5(string input, string[] formats, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information, and styles. The format of the string representation must match one of the specified formats exactly.</summary>
    ///<param name="input">A span that specifies the time interval to convert.</param>
    ///<param name="formats">An array of standard or custom format strings that define the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<param name="styles">A bitwise combination of enumeration values that defines the style elements that may be present in input.</param>
    ///<returns>A time interval that corresponds to <paramref name="input" />, as specified by <paramref name="formats" />, <paramref name="formatProvider" />, and <paramref name="styles" />.</returns>    [WhiteList("static System.TimeSpan.ParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static BigInt TimeSpanParseExact6(Uint32Array input, string[] formats, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A string that specifies the time interval to convert.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="s" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />. This operation returns <see langword="false" /> if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, has an invalid format, represents a time interval that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>, or has at least one days, hours, minutes, or seconds component outside its valid range.</returns>    [WhiteList("static System.TimeSpan.TryParse(string, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParse(string? s, OutValue<BigInt> result);

    ///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="s">A span containing the characters representing the time interval to convert.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="s" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="s" /> was converted successfully; otherwise, <see langword="false" />. This operation returns <see langword="false" /> if the <paramref name="s" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, has an invalid format, represents a time interval that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>, or has at least one days, hours, minutes, or seconds component outside its valid range.</returns>    [WhiteList("static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParse2(Uint32Array s, OutValue<BigInt> result);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific formatting information, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />. This operation returns <see langword="false" /> if the <paramref name="input" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, has an invalid format, represents a time interval that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>, or has at least one days, hours, minutes, or seconds component outside its valid range.</returns>    [WhiteList("static System.TimeSpan.TryParse(string, System.IFormatProvider, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParse3(string? input, Intl.NumberFormat? formatProvider, OutValue<BigInt> result);

    ///<summary>Converts the span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified culture-specific formatting information, and returns a value that indicates whether the conversion succeeded.</summary>
    ///<param name="input">A span containing the characters representing the time interval to convert.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />. This operation returns <see langword="false" /> if the <paramref name="input" /> parameter is <see langword="null" /> or <see cref="F:System.String.Empty" />, has an invalid format, represents a time interval that is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>, or has at least one days, hours, minutes, or seconds component outside its valid range.</returns>    [WhiteList("static System.TimeSpan.TryParse(System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParse4(Uint32Array input, Intl.NumberFormat? formatProvider, OutValue<BigInt> result);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(string, string, System.IFormatProvider, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact(string? input, string? format, Intl.NumberFormat? formatProvider, OutValue<BigInt> result);

    ///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format and culture-specific format information. The format of the string representation must match the specified format exactly.</summary>
    ///<param name="input">A span containing the characters that represent a time interval to convert.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact2(Uint32Array input, Uint32Array format, Intl.NumberFormat? formatProvider, OutValue<BigInt> result);

    ///<summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(string, string[], System.IFormatProvider, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact3(string? input, string?[]? formats, Intl.NumberFormat? formatProvider, OutValue<BigInt> result);

    ///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats and culture-specific format information. The format of the string representation must match one of the specified formats exactly.</summary>
    ///<param name="input">A span containing the characters that represent a time interval to convert.</param>
    ///<param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact4(Uint32Array input, string?[]? formats, Intl.NumberFormat? formatProvider, OutValue<BigInt> result);

    ///<summary>Converts the string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information and styles. The format of the string representation must match the specified format exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="format">A standard or custom format string that defines the required format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that provides culture-specific formatting information.</param>
    ///<param name="styles">One or more enumeration values that indicate the style of <paramref name="input" />.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(string, string, System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact5(string? input, string? format, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles, OutValue<BigInt> result);

    ///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified format, culture-specific format information, and styles, and returns a value that indicates whether the conversion succeeded. The format of the string representation must match the specified format exactly.</summary>
    ///<param name="input">A span containing the characters that represent a time interval to convert.</param>
    ///<param name="format">A span containing the charactes that represent a standard or custom format string that defines the acceptable format of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="styles">One or more enumeration values that indicate the style of <paramref name="input" />.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, System.ReadOnlySpan<char>, System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact6(Uint32Array input, Uint32Array format, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles, OutValue<BigInt> result);

    ///<summary>Converts the specified string representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information and styles. The format of the string representation must match one of the specified formats exactly.</summary>
    ///<param name="input">A string that specifies the time interval to convert.</param>
    ///<param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="styles">One or more enumeration values that indicate the style of <paramref name="input" />.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(string, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact7(string? input, string?[]? formats, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles, OutValue<BigInt> result);

    ///<summary>Converts the specified span representation of a time interval to its <see cref="T:System.TimeSpan" /> equivalent by using the specified formats, culture-specific format information and styles. The format of the string representation must match one of the specified formats exactly.</summary>
    ///<param name="input">A span containing the characters that represent a time interval to convert.</param>
    ///<param name="formats">An array of standard or custom format strings that define the acceptable formats of <paramref name="input" />.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<param name="styles">One or more enumeration values that indicate the style of <paramref name="input" />.</param>
    ///<param name="result">When this method returns, contains an object that represents the time interval specified by <paramref name="input" />, or <see cref="F:System.TimeSpan.Zero" /> if the conversion failed. This parameter is passed uninitialized.</param>
    ///<returns>  <see langword="true" /> if <paramref name="input" /> was converted successfully; otherwise, <see langword="false" />.</returns>    [WhiteList("static System.TimeSpan.TryParseExact(System.ReadOnlySpan<char>, string[], System.IFormatProvider, System.Globalization.TimeSpanStyles, out System.TimeSpan)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryParseExact8(Uint32Array input, string?[]? formats, Intl.NumberFormat? formatProvider, System.Globalization.TimeSpanStyles styles, OutValue<BigInt> result);

    ///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation.</summary>
    ///<returns>The string representation of the current <see cref="T:System.TimeSpan" /> value.</returns>    [WhiteList("override System.TimeSpan.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeSpanToString(BigInt instance);

    ///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format.</summary>
    ///<param name="format">A standard or custom <see cref="T:System.TimeSpan" /> format string.</param>
    ///<exception cref="T:System.FormatException">The <paramref name="format" /> parameter is not recognized or is not supported.</exception>
    ///<returns>The string representation of the current <see cref="T:System.TimeSpan" /> value in the format specified by the <paramref name="format" /> parameter.</returns>    [WhiteList("System.TimeSpan.ToString(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeSpanToString2(BigInt instance, string? format);

    ///<summary>Converts the value of the current <see cref="T:System.TimeSpan" /> object to its equivalent string representation by using the specified format and culture-specific formatting information.</summary>
    ///<param name="format">A standard or custom <see cref="T:System.TimeSpan" /> format string.</param>
    ///<param name="formatProvider">An object that supplies culture-specific formatting information.</param>
    ///<exception cref="T:System.FormatException">The <paramref name="format" /> parameter is not recognized or is not supported.</exception>
    ///<returns>The string representation of the current <see cref="T:System.TimeSpan" /> value, as specified by <paramref name="format" /> and <paramref name="formatProvider" />.</returns>    [WhiteList("System.TimeSpan.ToString(string, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string TimeSpanToString3(BigInt instance, string? format, Intl.NumberFormat? formatProvider);

    ///<summary>Tries to format the value of the current timespan number instance into the provided span of characters.</summary>
    ///<param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
    ///<param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
    ///<param name="format">A span containing the charactes that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
    ///<param name="formatProvider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
    ///<returns>  <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>    [WhiteList("System.TimeSpan.TryFormat(System.Span<char>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryFormat(BigInt instance, Uint32Array destination, OutValue<Number> charsWritten, Uint32Array format, Intl.NumberFormat? formatProvider);

    ///<summary>Tries to format the value of the current instance as UTF-8 into the provided span of bytes.</summary>
    ///<param name="utf8Destination">The span in which to write this instance's value formatted as a span of bytes.</param>
    ///<param name="bytesWritten">When this method returns, contains the number of bytes that were written in <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <code data-dev-comment-type="paramref">utf8Destination</code>.</param>
    ///<param name="formatProvider" />
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the formatting was successful; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("System.TimeSpan.TryFormat(System.Span<byte>, out int, System.ReadOnlySpan<char>, System.IFormatProvider)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool TimeSpanTryFormat2(BigInt instance, Uint8Array utf8Destination, OutValue<Number> bytesWritten, Uint32Array format, Intl.NumberFormat? formatProvider);

    ///<summary>Returns a <see cref="T:System.TimeSpan" /> whose value is the negated value of the specified instance.</summary>
    ///<param name="t">The time interval to be negated.</param>
    ///<exception cref="T:System.OverflowException">The negated value of this instance cannot be represented by a <see cref="T:System.TimeSpan" />; that is, the value of this instance is <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see>.</exception>
    ///<returns>An object that has the same numeric value as this instance, but the opposite sign.</returns>    [WhiteList("static System.TimeSpan.operator -(System.TimeSpan)")]
    [ECMAScriptLiteral("-{0}")]
	public extern static BigInt TimeSpanOpUnaryNegation(BigInt t);

    ///<summary>Subtracts a specified <see cref="T:System.TimeSpan" /> from another specified <see cref="T:System.TimeSpan" />.</summary>
    ///<param name="t1">The minuend.</param>
    ///<param name="t2">The subtrahend.</param>
    ///<exception cref="T:System.OverflowException">The return value is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>
    ///<returns>An object whose value is the result of the value of <paramref name="t1" /> minus the value of <paramref name="t2" />.</returns>    [WhiteList("static System.TimeSpan.operator -(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} - {1}")]
	public extern static BigInt TimeSpanOpSubtraction(BigInt t1, BigInt t2);

    ///<summary>Returns the specified instance of <see cref="T:System.TimeSpan" />.</summary>
    ///<param name="t">The time interval to return.</param>
    ///<returns>The time interval specified by <paramref name="t" />.</returns>    [WhiteList("static System.TimeSpan.operator +(System.TimeSpan)")]
    [ECMAScriptLiteral("+{0}")]
	public extern static BigInt TimeSpanOpUnaryPlus(BigInt t);

    ///<summary>Adds two specified <see cref="T:System.TimeSpan" /> instances.</summary>
    ///<param name="t1">The first time interval to add.</param>
    ///<param name="t2">The second time interval to add.</param>
    ///<exception cref="T:System.OverflowException">The resulting <see cref="T:System.TimeSpan" /> is less than <see cref="F:System.TimeSpan.MinValue">TimeSpan.MinValue</see> or greater than <see cref="F:System.TimeSpan.MaxValue">TimeSpan.MaxValue</see>.</exception>
    ///<returns>An object whose value is the sum of the values of <paramref name="t1" /> and <paramref name="t2" />.</returns>    [WhiteList("static System.TimeSpan.operator +(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} + {1}")]
	public extern static BigInt TimeSpanOpAddition(BigInt t1, BigInt t2);

    ///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of multiplying the specified <code data-dev-comment-type="paramref">timeSpan</code> instance and the specified <code data-dev-comment-type="paramref">factor</code>.</summary>
    ///<param name="timeSpan">The value to be multiplied.</param>
    ///<param name="factor">The value to be multiplied by.</param>
    ///<returns>A new object that represents the value of the specified <code data-dev-comment-type="paramref">timeSpan</code> instance multiplied by the value of the specified <code data-dev-comment-type="paramref">factor</code>.</returns>    [WhiteList("static System.TimeSpan.operator *(System.TimeSpan, double)")]
    [ECMAScriptLiteral("{0} * {1}")]
	public extern static BigInt TimeSpanOpMultiply(BigInt timeSpan, Number factor);

    ///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of multiplying the specified <code data-dev-comment-type="paramref">factor</code> and the specified <code data-dev-comment-type="paramref">timeSpan</code> instance.</summary>
    ///<param name="factor">The value to be multiplied by.</param>
    ///<param name="timeSpan">The value to be multiplied.</param>
    ///<returns>A new object that represents the value of the specified <code data-dev-comment-type="paramref">factor</code> multiplied by the value of the specified <code data-dev-comment-type="paramref">timeSpan</code> instance.</returns>    [WhiteList("static System.TimeSpan.operator *(double, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} * {1}")]
	public extern static BigInt TimeSpanOpMultiply2(Number factor, BigInt timeSpan);

    ///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> object whose value is the result of dividing the specified <code data-dev-comment-type="paramref">timeSpan</code> by the specified <code data-dev-comment-type="paramref">divisor</code>.</summary>
    ///<param name="timeSpan">Dividend or the value to be divided.</param>
    ///<param name="divisor">The value to be divided by.</param>
    ///<returns>A new object that represents the value of <code data-dev-comment-type="paramref">timeSpan</code> divided by the value of <code data-dev-comment-type="paramref">divisor</code>.</returns>    [WhiteList("static System.TimeSpan.operator /(System.TimeSpan, double)")]
    [ECMAScriptLiteral("{0} / {1}")]
	public extern static BigInt TimeSpanOpDivision(BigInt timeSpan, Number divisor);

    ///<summary>Returns a new <xref data-throw-if-not-resolved="true" uid="System.Double"></xref> value that's the result of dividing <code data-dev-comment-type="paramref">t1</code> by <code data-dev-comment-type="paramref">t2</code>.</summary>
    ///<param name="t1">The divident or the value to be divided.</param>
    ///<param name="t2">The value to be divided by.</param>
    ///<returns>A new value that represents result of dividing <code data-dev-comment-type="paramref">t1</code> by the value of <code data-dev-comment-type="paramref">t2</code>.</returns>    [WhiteList("static System.TimeSpan.operator /(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} / {1}")]
	public extern static Number TimeSpanOpDivision2(BigInt t1, BigInt t2);

    ///<summary>Indicates whether two <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> instances are equal.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the values of <code data-dev-comment-type="paramref">t1</code> and <code data-dev-comment-type="paramref">t2</code> are equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeSpan.operator ==(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} == {1}")]
	public extern static bool TimeSpanOpEquality(BigInt t1, BigInt t2);

    ///<summary>Indicates whether two <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> instances are not equal.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the values of <code data-dev-comment-type="paramref">t1</code> and <code data-dev-comment-type="paramref">t2</code> are not equal; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeSpan.operator !=(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} != {1}")]
	public extern static bool TimeSpanOpInequality(BigInt t1, BigInt t2);

    ///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is less than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the value of <code data-dev-comment-type="paramref">t1</code> is less than the value of <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeSpan.operator <(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} < {1}")]
	public extern static bool TimeSpanOpLessThan(BigInt t1, BigInt t2);

    ///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is less than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the value of <code data-dev-comment-type="paramref">t1</code> is less than or equal to the value of <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeSpan.operator <=(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} <= {1}")]
	public extern static bool TimeSpanOpLessThanOrEqual(BigInt t1, BigInt t2);

    ///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is greater than another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the value of <code data-dev-comment-type="paramref">t1</code> is greater than the value of <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeSpan.operator >(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} > {1}")]
	public extern static bool TimeSpanOpGreaterThan(BigInt t1, BigInt t2);

    ///<summary>Indicates whether a specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref> is greater than or equal to another specified <xref data-throw-if-not-resolved="true" uid="System.TimeSpan"></xref>.</summary>
    ///<param name="t1">The first time interval to compare.</param>
    ///<param name="t2">The second time interval to compare.</param>
    ///<returns>  <code data-dev-comment-type="langword">true</code> if the value of <code data-dev-comment-type="paramref">t1</code> is greater than or equal to the value of <code data-dev-comment-type="paramref">t2</code>; otherwise, <code data-dev-comment-type="langword">false</code>.</returns>    [WhiteList("static System.TimeSpan.operator >=(System.TimeSpan, System.TimeSpan)")]
    [ECMAScriptLiteral("{0} >= {1}")]
	public extern static bool TimeSpanOpGreaterThanOrEqual(BigInt t1, BigInt t2);
}
