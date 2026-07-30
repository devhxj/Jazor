namespace ECMAScript;

/// <summary>
/// JavaScript <c>Date</c> runtime host.
/// Deliberately omits CLR comparison and formatting interfaces so the public surface stays close to JavaScript.
/// </summary>
[ECMAScript]
[Description("@#Date")]
/// <remarks>
/// Date 使用 JavaScript 的时间戳、时区和非法日期规则；它不是 System.DateTime 的透明替代品。
/// 需要 tick、DateOnly 或 DateTimeOffset 语义时，应使用相应 CLR runtime module 的 carrier。
/// </remarks>
public sealed class Date
{
	/// <summary>
	/// JavaScript <c>Date.prototype</c> object.
	/// Exposing this on the constructor host keeps the public surface aligned with the runtime host shape.
	/// </summary>
	[Description("@#prototype")]
	public extern static Date Prototype { get; }

	public extern Date();

	public extern Date(Number value);

	public extern Date(string dateString);

	public extern Date(Number year, Number month, Number? day = default, Number? hours = default, Number? minutes = default, Number? seconds = default, Number? milliseconds = default);

	/// <summary>
	/// Returns a string representation of a date. The format of the string depends on the locale.
	/// </summary>
	/// <returns></returns>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Returns a date as a string value.
	/// </summary>
	/// <returns></returns>
	[Description("@#toDateString")]
	public extern string ToDateString();

	/// <summary>
	/// Returns a time as a string value.
	/// </summary>
	/// <returns></returns>
	[Description("@#toTimeString")]
	public extern string ToTimeString();

	/// <summary>
	/// Returns a value as a string value appropriate to the host environment's current locale.
	/// </summary>
	/// <returns></returns>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString();

	/// <summary>
	/// Returns a date as a string value appropriate to the host environment's current locale.
	/// </summary>
	/// <returns></returns>
	[Description("@#toLocaleDateString")]
	public extern string ToLocaleDateString();

	/// <summary>
	/// Returns a time as a string value appropriate to the host environment's current locale.
	/// </summary>
	/// <returns></returns>
	[Description("@#toLocaleTimeString")]
	public extern string ToLocaleTimeString();

	/// <summary>
	/// Returns the stored time value in milliseconds since midnight, January 1, 1970 UTC.
	/// </summary>
	/// <returns></returns>
	[Description("@#valueOf")]
	public extern Number ValueOf();

	/// <summary>
	/// Returns the stored time value in milliseconds since midnight, January 1, 1970 UTC.
	/// </summary>
	/// <returns></returns>
	[Description("@#getTime")]
	public extern Number GetTime();

	/// <summary>
	/// Gets the year, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getFullYear")]
	public extern Number GetFullYear();

	/// <summary>
	/// Legacy JavaScript year getter.
	/// This remains exposed because many runtimes still provide <c>Date.prototype.getYear</c> for web compatibility.
	/// </summary>
	[Description("@#getYear")]
	public extern Number GetYear();

	/// <summary>
	/// Gets the year using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCFullYear")]
	public extern Number GetUTCFullYear();

	/// <summary>
	/// Gets the month, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getMonth")]
	public extern Number GetMonth();

	/// <summary>
	/// Gets the month of a Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCMonth")]
	public extern Number GetUTCMonth();

	/// <summary>
	/// Gets the day-of-the-month, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getDate")]
	public extern Number GetDate();

	/// <summary>
	/// Gets the day of the month using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCDate")]
	public extern Number GetUTCDate();

	/// <summary>
	/// Gets the day of the week, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getDay")]
	public extern Number GetDay();

	/// <summary>
	/// Gets the day of the week using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCDay")]
	public extern Number GetUTCDay();

	/// <summary>
	/// Gets the hours in a date, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getHours")]
	public extern Number GetHours();

	/// <summary>
	/// Gets the hours value in a Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCHours")]
	public extern Number GetUTCHours();

	/// <summary>
	/// Gets the minutes of a Date object, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getMinutes")]
	public extern Number GetMinutes();

	/// <summary>
	/// Gets the minutes of a Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCMinutes")]
	public extern Number GetUTCMinutes();

	/// <summary>
	/// Gets the seconds of a Date object, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getSeconds")]
	public extern Number GetSeconds();

	/// <summary>
	/// Gets the seconds of a Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCSeconds")]
	public extern Number GetUTCSeconds();

	/// <summary>
	/// Gets the milliseconds of a Date, using local time.
	/// </summary>
	/// <returns></returns>
	[Description("@#getMilliseconds")]
	public extern Number GetMilliseconds();

	/// <summary>
	/// Gets the milliseconds of a Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#getUTCMilliseconds")]
	public extern Number GetUTCMilliseconds();

	/// <summary>
	/// Gets the difference in minutes between Universal Coordinated Time (UTC) and the time on the local computer.
	/// </summary>
	/// <returns></returns>
	[Description("@#getTimezoneOffset")]
	public extern Number GetTimezoneOffset();

	/// <summary>
	/// Sets the date and time value in the Date object.
	/// </summary>
	/// <param name="time">A numeric value representing the number of elapsed milliseconds since midnight, January 1, 1970 GMT.</param>
	/// <returns></returns>
	[Description("@#setTime")]
	public extern Number SetTime(Number time);

	/// <summary>
	/// Sets the milliseconds value in the Date object using local time.
	/// </summary>
	/// <param name="ms">A numeric value equal to the millisecond value.</param>
	/// <returns></returns>
	[Description("@#setMilliseconds")]
	public extern Number SetMilliseconds(Number ms);

	/// <summary>
	/// Sets the milliseconds value in the Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <param name="ms">A numeric value equal to the millisecond value.</param>
	/// <returns></returns>
	[Description("@#setUTCMilliseconds")]
	public extern Number SetUTCMilliseconds(Number ms);

	/// <summary>
	/// Sets the seconds value in the Date object using local time.
	/// </summary>
	/// <param name="sec">A numeric value equal to the seconds value.</param>
	/// <param name="ms">A numeric value equal to the milliseconds value.</param>
	/// <returns></returns>
	[Description("@#setSeconds")]
	public extern Number SetSeconds(Number sec, Number? ms = default);

	/// <summary>
	/// Sets the seconds value in the Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <param name="sec">A numeric value equal to the seconds value.</param>
	/// <param name="ms">A numeric value equal to the milliseconds value.</param>
	/// <returns></returns>
	[Description("@#setUTCSeconds")]
	public extern Number SetUTCSeconds(Number sec, Number? ms = default);

	/// <summary>
	/// Sets the minutes value in the Date object using local time.
	/// </summary>
	/// <param name="min">A numeric value equal to the minutes value.</param>
	/// <param name="sec">A numeric value equal to the seconds value.</param>
	/// <param name="ms">A numeric value equal to the milliseconds value.</param>
	/// <returns></returns>
	[Description("@#setMinutes")]
	public extern Number SetMinutes(Number min, Number? sec = default, Number? ms = default);

	/// <summary>
	/// Sets the minutes value in the Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <param name="min">A numeric value equal to the minutes value.</param>
	/// <param name="sec">A numeric value equal to the seconds value.</param>
	/// <param name="ms">A numeric value equal to the milliseconds value.</param>
	/// <returns></returns>
	[Description("@#setUTCMinutes")]
	public extern Number SetUTCMinutes(Number min, Number? sec = default, Number? ms = default);

	/// <summary>
	/// Sets the hour value in the Date object using local time.
	/// </summary>
	/// <param name="hours">A numeric value equal to the hours value.</param>
	/// <param name="min">A numeric value equal to the minutes value.</param>
	/// <param name="sec">A numeric value equal to the seconds value.</param>
	/// <param name="ms">A numeric value equal to the milliseconds value.</param>
	/// <returns></returns>
	[Description("@#setHours")]
	public extern Number SetHours(Number hours, Number? min = default, Number? sec = default, Number? ms = default);

	/// <summary>
	/// Sets the hours value in the Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <param name="hours">A numeric value equal to the hours value.</param>
	/// <param name="min">A numeric value equal to the minutes value.</param>
	/// <param name="sec">A numeric value equal to the seconds value.</param>
	/// <param name="ms">A numeric value equal to the milliseconds value.</param>
	/// <returns></returns>
	[Description("@#setUTCHours")]
	public extern Number SetUTCHours(Number hours, Number? min = default, Number? sec = default, Number? ms = default);

	/// <summary>
	/// Sets the numeric day-of-the-month value of the Date object using local time.
	/// </summary>
	/// <param name="date">A numeric value equal to the day of the month.</param>
	/// <returns></returns>
	[Description("@#setDate")]
	public extern Number SetDate(Number date);

	/// <summary>
	/// Sets the numeric day of the month in the Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <param name="date">A numeric value equal to the day of the month.</param>
	/// <returns></returns>
	[Description("@#setUTCDate")]
	public extern Number SetUTCDate(Number date);

	/// <summary>
	/// Sets the month value in the Date object using local time.
	/// </summary>
	/// <param name="month">A numeric value equal to the month. The value for January is 0, and other month values follow consecutively.</param>
	/// <param name="date">A numeric value representing the day of the month. If this value is not supplied, the value from a call to the getDate method is used.</param>
	/// <returns></returns>
	[Description("@#setMonth")]
	public extern Number SetMonth(Number month, Number? date = default);

	/// <summary>
	/// Sets the month value in the Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <param name="month">A numeric value equal to the month. The value for January is 0, and other month values follow consecutively.</param>
	/// <param name="date">A numeric value representing the day of the month. If it is not supplied, the value from a call to the getUTCDate method is used.</param>
	/// <returns></returns>
	[Description("@#setUTCMonth")]
	public extern Number SetUTCMonth(Number month, Number? date = default);

	/// <summary>
	/// Sets the year of the Date object using local time.
	/// </summary>
	/// <param name="year">A numeric value for the year.</param>
	/// <param name="month">A zero-based numeric value for the month (0 for January, 11 for December). Must be specified if numDate is specified.</param>
	/// <param name="date">A numeric value equal for the day of the month.</param>
	/// <returns></returns>
	[Description("@#setFullYear")]
	public extern Number SetFullYear(Number year, Number? month = default, Number? date = default);

	/// <summary>
	/// Legacy JavaScript year setter.
	/// This remains exposed because many runtimes still provide <c>Date.prototype.setYear</c> for web compatibility.
	/// </summary>
	[Description("@#setYear")]
	public extern Number SetYear(Number year);

	/// <summary>
	/// Sets the year value in the Date object using Universal Coordinated Time (UTC).
	/// </summary>
	/// <param name="year">A numeric value equal to the year.</param>
	/// <param name="month">A numeric value equal to the month. The value for January is 0, and other month values follow consecutively. Must be supplied if numDate is supplied.</param>
	/// <param name="date">A numeric value equal to the day of the month.</param>
	/// <returns></returns>
	[Description("@#setUTCFullYear")]
	public extern Number SetUTCFullYear(Number year, Number? month = default, Number? date = default);

	/// <summary>
	/// Returns a date converted to a string using Universal Coordinated Time (UTC).
	/// </summary>
	/// <returns></returns>
	[Description("@#toUTCString")]
	public extern string ToUTCString();

	/// <summary>
	/// Legacy JavaScript alias for <see cref="ToUTCString"/>.
	/// This stays exposed because browsers and JavaScript runtimes still provide <c>Date.prototype.toGMTString</c>.
	/// </summary>
	[Description("@#toGMTString")]
	public extern string ToGMTString();

	/// <summary>
	/// Returns a date as a string value in ISO format.
	/// </summary>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	[Description("@#toISOString")]
	public extern string ToISOString();

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>Date.prototype[@@toPrimitive]</c>.
	/// The runtime may produce either a string or a number depending on the supplied hint, so the bridge stays union-shaped.
	/// </summary>
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
	[Description("@#@@toPrimitive")]
	public extern DatePrimitive ToPrimitive_(string? hint = null);

	/// <summary>
	/// Used by <c>JSON.stringify</c> to transform the date for JSON serialization.
	/// Nullable is used because JavaScript returns <c>null</c> when the date is invalid.
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	[Description("@#toJSON")]
	public extern string? ToJSON(object? key = null);

	/// <summary>
	/// Parses a string containing a date, and returns the number of milliseconds between that date and midnight, January 1, 1970.
	/// </summary>
	/// <param name="s">A date string</param>
	/// <returns></returns>
	[Description("@#parse")]
	public extern static Number Parse(string s);

	/// <summary>
	/// Returns the number of milliseconds between midnight, January 1, 1970 Universal Coordinated Time (UTC) (or GMT) and the specified date.
	/// </summary>
	/// <param name="year">The full year designation is required for cross-century date accuracy. If year is between 0 and 99 is used, then year is assumed to be 1900 + year.</param>
	/// <param name="monthIndex">The month as a number between 0 and 11 (January to December).</param>
	/// <param name="date">The date as a number between 1 and 31.</param>
	/// <param name="hours">Must be supplied if minutes is supplied. A number from 0 to 23 (midnight to 11pm) that specifies the hour.</param>
	/// <param name="minutes">Must be supplied if seconds is supplied. A number from 0 to 59 that specifies the minutes.</param>
	/// <param name="seconds">Must be supplied if milliseconds is supplied. A number from 0 to 59 that specifies the seconds.</param>
	/// <param name="milliseconds">A number from 0 to 999 that specifies the milliseconds.</param>
	/// <returns></returns>
	[Description("@#UTC")]
	public extern static Number UTC(Number year, Number monthIndex, Number? date = default, Number? hours = default, Number? minutes = default, Number? seconds = default, Number? milliseconds = default);

	/// <summary>
	/// Returns the number of milliseconds elapsed since midnight, January 1, 1970 UTC.
	/// This is the C# host projection of JavaScript <c>Date.now()</c>.
	/// </summary>
	[Description("@#now")]
	public extern static Number Now();

	public extern static implicit operator Number(Date date);

	public extern static implicit operator string(Date value);

	public extern static implicit operator Date(Number value);

	public extern static implicit operator Date(string value);

	public extern static bool operator ==(Date left, Date right);

	public extern static bool operator !=(Date left, Date right);

	public extern static bool operator <(Date left, Date right);

	public extern static bool operator >(Date left, Date right);

	public extern static bool operator <=(Date left, Date right);

	public extern static bool operator >=(Date left, Date right);

	// Kept only to satisfy C#'s operator contract for == and != on the host type.
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override bool Equals(object? obj);

	// Kept only to satisfy C#'s operator contract for == and != on the host type.
	[EditorBrowsable(EditorBrowsableState.Never)]
	public extern override int GetHashCode();

	/// <summary>
	/// Converts a date and time to a string by using the current or specified locale.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(string? locales = null, Intl.DateTimeFormatOptions? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// </summary>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(Intl.DateTimeFormatOptions options);

	/// <summary>
	/// Converts a date and time to a string by using the current or specified locale.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleString")]
	public extern string ToLocaleString(IEnumerable<string> locales, Intl.DateTimeFormatOptions? options = null);

	/// <summary>
	/// Converts a date to a string by using the current or specified locale.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleDateString")]
	public extern string ToLocaleDateString(string? locales = null, Intl.DateTimeFormatOptions? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// </summary>
	[Description("@#toLocaleDateString")]
	public extern string ToLocaleDateString(Intl.DateTimeFormatOptions options);

	/// <summary>
	/// Converts a date to a string by using the current or specified locale.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleDateString")]
	public extern string ToLocaleDateString(IEnumerable<string> locales, Intl.DateTimeFormatOptions? options = null);

	/// <summary>
	/// Converts a time to a string by using the current or specified locale.
	/// <see cref="IEnumerable{T}"/> is used as the common C# input surface for JavaScript locale lists.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleTimeString")]
	public extern string ToLocaleTimeString(string? locales = null, Intl.DateTimeFormatOptions? options = null);

	/// <summary>
	/// C# convenience overload for the JavaScript form that omits <c>locales</c> and only supplies options.
	/// This exists because C# cannot naturally skip the leading locale argument in method calls.
	/// </summary>
	[Description("@#toLocaleTimeString")]
	public extern string ToLocaleTimeString(Intl.DateTimeFormatOptions options);

	/// <summary>
	/// Converts a time to a string by using the current or specified locale.
	/// </summary>
	/// <param name="locales">A locale string or array of locale strings that contain one or more language or locale tags.If you include more than one locale string, list them in descending order of priority so that the first entry is the preferred locale.If you omit this parameter, the default locale of the JavaScript runtime is used.</param>
	/// <param name="options">An object that contains one or more properties that specify comparison options.</param>
	/// <returns></returns>
	[Description("@#toLocaleTimeString")]
	public extern string ToLocaleTimeString(IEnumerable<string> locales, Intl.DateTimeFormatOptions? options = null);
}
