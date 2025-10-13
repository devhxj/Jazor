using System.Collections;
using static ECMAScript.CLRModule;
using static ECMAScript.Jazor;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Globalization.CultureInfo")]
public static class CultureInfoModule
{
    ///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name.</summary>
    ///<param name="name">A predefined <see cref="T:System.Globalization.CultureInfo" /> name, <see cref="P:System.Globalization.CultureInfo.Name" /> of an existing <see cref="T:System.Globalization.CultureInfo" />, or Windows-only culture name. <paramref name="name" /> is not case-sensitive.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> is not a valid culture name. For more information, see the Notes to Callers section.</exception>    [WhiteList("System.Globalization.CultureInfo.CultureInfo(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoNew(string name);

    ///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
    ///<param name="name">A predefined <see cref="T:System.Globalization.CultureInfo" /> name, <see cref="P:System.Globalization.CultureInfo.Name" /> of an existing <see cref="T:System.Globalization.CultureInfo" />, or Windows-only culture name. <paramref name="name" /> is not case-sensitive.</param>
    ///<param name="useUserOverride">  <see langword="true" /> to use the user-selected culture settings (Windows only); <see langword="false" /> to use the default culture settings.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> is not a valid culture name. See the Notes to Callers section for more information.</exception>    [WhiteList("System.Globalization.CultureInfo.CultureInfo(string, bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoNew2(string name, bool useUserOverride);

    ///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier.</summary>
    ///<param name="culture">A predefined <see cref="T:System.Globalization.CultureInfo" /> identifier, <see cref="P:System.Globalization.CultureInfo.LCID" /> property of an existing <see cref="T:System.Globalization.CultureInfo" /> object, or Windows-only culture identifier.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="culture" /> is less than zero.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="culture" /> is not a valid culture identifier. See the Notes to Callers section for more information.</exception>    [WhiteList("System.Globalization.CultureInfo.CultureInfo(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoNew3(Number culture);

    ///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
    ///<param name="culture">A predefined <see cref="T:System.Globalization.CultureInfo" /> identifier, <see cref="P:System.Globalization.CultureInfo.LCID" /> property of an existing <see cref="T:System.Globalization.CultureInfo" /> object, or Windows-only culture identifier.</param>
    ///<param name="useUserOverride">  <see langword="true" /> to use the user-selected culture settings (Windows only); <see langword="false" /> to use the default culture settings.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="culture" /> is less than zero.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="culture" /> is not a valid culture identifier. See the Notes to Callers section for more information.</exception>    [WhiteList("System.Globalization.CultureInfo.CultureInfo(int, bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoNew4(Number culture, bool useUserOverride);

    ///<summary>Creates a <see cref="T:System.Globalization.CultureInfo" /> that represents the specific culture that is associated with the specified name.</summary>
    ///<param name="name">A predefined <see cref="T:System.Globalization.CultureInfo" /> name or the name of an existing <see cref="T:System.Globalization.CultureInfo" /> object. <paramref name="name" /> is not case-sensitive.</param>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> is not a valid culture name. -or- The culture specified by <paramref name="name" /> does not have a specific culture associated with it.</exception>
    ///<exception cref="T:System.NullReferenceException">  <paramref name="name" /> is null.</exception>
    ///<returns>A <see cref="T:System.Globalization.CultureInfo" /> object that represents: The invariant culture, if <paramref name="name" /> is an empty string (""). -or- The specific culture associated with <paramref name="name" />, if <paramref name="name" /> is a neutral culture. -or- The culture specified by <paramref name="name" />, if <paramref name="name" /> is already a specific culture.</returns>    [WhiteList("static System.Globalization.CultureInfo.CreateSpecificCulture(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoCreateSpecificCulture(string name);

    [WhiteList("static System.Globalization.CultureInfo.CurrentCulture.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetCurrentCulture(String instance);

    [WhiteList("static System.Globalization.CultureInfo.CurrentCulture.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void CultureInfoSetCurrentCulture(String instance, String value);

    [WhiteList("static System.Globalization.CultureInfo.CurrentUICulture.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetCurrentUICulture(String instance);

    [WhiteList("static System.Globalization.CultureInfo.CurrentUICulture.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void CultureInfoSetCurrentUICulture(String instance, String value);

    [WhiteList("static System.Globalization.CultureInfo.InstalledUICulture.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetInstalledUICulture(String instance);

    [WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String? CultureInfoGetDefaultThreadCurrentCulture(String instance);

    [WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void CultureInfoSetDefaultThreadCurrentCulture(String instance, String? value);

    [WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String? CultureInfoGetDefaultThreadCurrentUICulture(String instance);

    [WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void CultureInfoSetDefaultThreadCurrentUICulture(String instance, String? value);

    [WhiteList("static System.Globalization.CultureInfo.InvariantCulture.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetInvariantCulture(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.Parent.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetParent(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.LCID.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CultureInfoGetLCID(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.KeyboardLayoutId.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CultureInfoGetKeyboardLayoutId(String instance);

    ///<summary>Gets the list of supported cultures filtered by the specified <see cref="T:System.Globalization.CultureTypes" /> parameter.</summary>
    ///<param name="types">A bitwise combination of the enumeration values that filter the cultures to retrieve.</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="types" /> specifies an invalid combination of <see cref="T:System.Globalization.CultureTypes" /> values.</exception>
    ///<returns>An array that contains the cultures specified by the <paramref name="types" /> parameter. The array of cultures is unsorted.</returns>    [WhiteList("static System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.CultureInfo[] CultureInfoGetCultures(System.Globalization.CultureTypes types);

    [WhiteList("virtual System.Globalization.CultureInfo.Name.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetName(String instance);

    [WhiteList("System.Globalization.CultureInfo.IetfLanguageTag.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetIetfLanguageTag(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.DisplayName.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetDisplayName(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.NativeName.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetNativeName(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.EnglishName.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetEnglishName(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.TwoLetterISOLanguageName.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetTwoLetterISOLanguageName(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.ThreeLetterISOLanguageName.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetThreeLetterISOLanguageName(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.ThreeLetterWindowsLanguageName.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoGetThreeLetterWindowsLanguageName(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.CompareInfo.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.CompareInfo CultureInfoGetCompareInfo(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.TextInfo.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.TextInfo CultureInfoGetTextInfo(String instance);

    ///<summary>Determines whether the specified object is the same culture as the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
    ///<param name="value">The object to compare with the current <see cref="T:System.Globalization.CultureInfo" />.</param>
    ///<returns>  <see langword="true" /> if <paramref name="value" /> is the same culture as the current <see cref="T:System.Globalization.CultureInfo" />; otherwise, <see langword="false" />.</returns>    [WhiteList("override System.Globalization.CultureInfo.Equals(object)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CultureInfoEquals(String instance, Object? value);

    ///<summary>Serves as a hash function for the current <see cref="T:System.Globalization.CultureInfo" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
    ///<returns>A hash code for the current <see cref="T:System.Globalization.CultureInfo" />.</returns>    [WhiteList("override System.Globalization.CultureInfo.GetHashCode()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Number CultureInfoGetHashCode(String instance);

    ///<summary>Returns a string containing the name of the current <see cref="T:System.Globalization.CultureInfo" /> in the format languagecode2-country/regioncode2.</summary>
    ///<returns>A string containing the name of the current <see cref="T:System.Globalization.CultureInfo" />.</returns>    [WhiteList("override System.Globalization.CultureInfo.ToString()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static string CultureInfoToString(String instance);

    ///<summary>Gets an object that defines how to format the specified type.</summary>
    ///<param name="formatType">The <see cref="T:System.Type" /> for which to get a formatting object. This method only supports the <see cref="T:System.Globalization.NumberFormatInfo" /> and <see cref="T:System.Globalization.DateTimeFormatInfo" /> types.</param>
    ///<returns>The value of the <see cref="P:System.Globalization.CultureInfo.NumberFormat" /> property, which is a <see cref="T:System.Globalization.NumberFormatInfo" /> containing the default number format information for the current <see cref="T:System.Globalization.CultureInfo" />, if <paramref name="formatType" /> is the <see cref="T:System.Type" /> object for the <see cref="T:System.Globalization.NumberFormatInfo" /> class. -or- The value of the <see cref="P:System.Globalization.CultureInfo.DateTimeFormat" /> property, which is a <see cref="T:System.Globalization.DateTimeFormatInfo" /> containing the default date and time format information for the current <see cref="T:System.Globalization.CultureInfo" />, if <paramref name="formatType" /> is the <see cref="T:System.Type" /> object for the <see cref="T:System.Globalization.DateTimeFormatInfo" /> class. -or- null, if <paramref name="formatType" /> is any other object.</returns>    [WhiteList("virtual System.Globalization.CultureInfo.GetFormat(System.Type)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Object? CultureInfoGetFormat(String instance, System.Type? formatType);

    [WhiteList("virtual System.Globalization.CultureInfo.IsNeutralCulture.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CultureInfoGetIsNeutralCulture(String instance);

    [WhiteList("System.Globalization.CultureInfo.CultureTypes.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.CultureTypes CultureInfoGetCultureTypes(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.NumberFormat.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.NumberFormatInfo CultureInfoGetNumberFormat(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.NumberFormat.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void CultureInfoSetNumberFormat(String instance, System.Globalization.NumberFormatInfo value);

    [WhiteList("virtual System.Globalization.CultureInfo.DateTimeFormat.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.DateTimeFormatInfo CultureInfoGetDateTimeFormat(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.DateTimeFormat.set")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void CultureInfoSetDateTimeFormat(String instance, System.Globalization.DateTimeFormatInfo value);

    ///<summary>Refreshes cached culture-related information.</summary>    [WhiteList("System.Globalization.CultureInfo.ClearCachedData()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static void CultureInfoClearCachedData(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.Calendar.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static GregorianCalendar CultureInfoGetCalendar(String instance);

    [WhiteList("virtual System.Globalization.CultureInfo.OptionalCalendars.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static System.Globalization.Calendar[] CultureInfoGetOptionalCalendars(String instance);

    [WhiteList("System.Globalization.CultureInfo.UseUserOverride.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CultureInfoGetUseUserOverride(String instance);

    ///<summary>Gets an alternate user interface culture suitable for console applications when the default graphic user interface culture is unsuitable.</summary>
    ///<returns>An alternate culture that is used to read and display text on the console.</returns>    [WhiteList("System.Globalization.CultureInfo.GetConsoleFallbackUICulture()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetConsoleFallbackUICulture(String instance);

    ///<summary>Creates a copy of the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
    ///<returns>A copy of the current <see cref="T:System.Globalization.CultureInfo" />.</returns>    [WhiteList("virtual System.Globalization.CultureInfo.Clone()")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static Object CultureInfoClone(String instance);

    ///<summary>Returns a read-only wrapper around the specified <see cref="T:System.Globalization.CultureInfo" /> object.</summary>
    ///<param name="ci">The <see cref="T:System.Globalization.CultureInfo" /> object to wrap.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="ci" /> is null.</exception>
    ///<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> wrapper around <paramref name="ci" />.</returns>    [WhiteList("static System.Globalization.CultureInfo.ReadOnly(System.Globalization.CultureInfo)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoReadOnly(String ci);

    [WhiteList("System.Globalization.CultureInfo.IsReadOnly.get")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static bool CultureInfoGetIsReadOnly(String instance);

    ///<summary>Retrieves a cached, read-only instance of a culture by using the specified culture identifier.</summary>
    ///<param name="culture">A locale identifier (LCID).</param>
    ///<exception cref="T:System.ArgumentOutOfRangeException">  <paramref name="culture" /> is less than zero.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="culture" /> specifies a culture that is not supported. See the Notes to Caller section for more information.</exception>
    ///<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>    [WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(int)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetCultureInfo(Number culture);

    ///<summary>Retrieves a cached, read-only instance of a culture using the specified culture name.</summary>
    ///<param name="name">The name of a culture. <paramref name="name" /> is not case-sensitive.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> specifies a culture that is not supported. See the Notes to Callers section for more information.</exception>
    ///<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>    [WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetCultureInfo2(string name);

    ///<summary>Retrieves a cached, read-only instance of a culture. Parameters specify a culture that is initialized with the <see cref="T:System.Globalization.TextInfo" /> and <see cref="T:System.Globalization.CompareInfo" /> objects specified by another culture.</summary>
    ///<param name="name">The name of a culture. <paramref name="name" /> is not case-sensitive.</param>
    ///<param name="altName">The name of a culture that supplies the <see cref="T:System.Globalization.TextInfo" /> and <see cref="T:System.Globalization.CompareInfo" /> objects used to initialize <paramref name="name" />. <paramref name="altName" /> is not case-sensitive.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> or <paramref name="altName" /> is null.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> or <paramref name="altName" /> specifies a culture that is not supported. See the Notes to Callers section for more information.</exception>
    ///<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>    [WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(string, string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetCultureInfo3(string name, string altName);

    ///<summary>Retrieves a cached, read-only instance of a culture.</summary>
    ///<param name="name">The name of a culture. It is not case-sensitive.</param>
    ///<param name="predefinedOnly">  <see langword="true" /> if requesting to create an instance of a culture that is known by the platform. <see langword="false" /> if it is ok to retreive a made-up culture even if the platform does not carry data for it.</param>
    ///<returns>A read-only instance of a culture.</returns>    [WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(string, bool)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetCultureInfo4(string name, bool predefinedOnly);

    ///<summary>Deprecated. Retrieves a read-only <see cref="T:System.Globalization.CultureInfo" /> object having linguistic characteristics that are identified by the specified RFC 4646 language tag.</summary>
    ///<param name="name">The name of a language as specified by the RFC 4646 standard.</param>
    ///<exception cref="T:System.ArgumentNullException">  <paramref name="name" /> is null.</exception>
    ///<exception cref="T:System.Globalization.CultureNotFoundException">  <paramref name="name" /> does not correspond to a supported culture.</exception>
    ///<returns>A read-only <see cref="T:System.Globalization.CultureInfo" /> object.</returns>    [WhiteList("static System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(string)")]
    [Obsolete("Not Support in Jazor",true)]
	public extern static String CultureInfoGetCultureInfoByIetfLanguageTag(string name);
}
