using ECMAScript.Common;

namespace ECMAScript;

[ECMAScriptModule]
[WhiteList("System.Globalization.CultureInfo", WhiteListOp.Allowed, null,"System/Globalization/CultureInfoModule.js")]
public static class CultureInfoModule
{
	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name.</summary>
	[WhiteList("System.Globalization.CultureInfo.CultureInfo(string)", WhiteListOp.Discard)]
	public extern static String _b7486264ae338f27(object name);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
	[WhiteList("System.Globalization.CultureInfo.CultureInfo(string, bool)", WhiteListOp.Discard)]
	public extern static String _df21a93fd9f84197(object name, object useUserOverride);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier.</summary>
	[WhiteList("System.Globalization.CultureInfo.CultureInfo(int)", WhiteListOp.Discard)]
	public extern static String _22aaac09e253b1f9(Number culture);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
	[WhiteList("System.Globalization.CultureInfo.CultureInfo(int, bool)", WhiteListOp.Discard)]
	public extern static String _d0948ef9f698ec85(Number culture, object useUserOverride);

	///<summary>Creates a <see cref="T:System.Globalization.CultureInfo" /> that represents the specific culture that is associated with the specified name.</summary>
	[WhiteList("static System.Globalization.CultureInfo.CreateSpecificCulture(string)", WhiteListOp.Discard)]
	public extern static String _a078d5ccbbf2345a(object name);

	[WhiteList("static System.Globalization.CultureInfo.CurrentCulture.get", WhiteListOp.Discard)]
	public extern static String _1a26e2e2e4e0ca1d(String instance);

	[WhiteList("static System.Globalization.CultureInfo.CurrentCulture.set", WhiteListOp.Discard)]
	public extern static void _82cfca57d721204e(String instance, String value);

	[WhiteList("static System.Globalization.CultureInfo.CurrentUICulture.get", WhiteListOp.Discard)]
	public extern static String _eca32c250ead7de9(String instance);

	[WhiteList("static System.Globalization.CultureInfo.CurrentUICulture.set", WhiteListOp.Discard)]
	public extern static void _7e355a1a63351619(String instance, String value);

	[WhiteList("static System.Globalization.CultureInfo.InstalledUICulture.get", WhiteListOp.Discard)]
	public extern static String _98e743867688a06d(String instance);

	[WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.get", WhiteListOp.Discard)]
	public extern static String? _3c1fdac9ccc43427(String instance);

	[WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.set", WhiteListOp.Discard)]
	public extern static void _96d14148886217cb(String instance, String? value);

	[WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.get", WhiteListOp.Discard)]
	public extern static String? _abdb5d2bfd934cfc(String instance);

	[WhiteList("static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.set", WhiteListOp.Discard)]
	public extern static void _12da8bfb928d7414(String instance, String? value);

	[WhiteList("static System.Globalization.CultureInfo.InvariantCulture.get", WhiteListOp.Discard)]
	public extern static String _e4c4d53d69e72382(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.Parent.get", WhiteListOp.Discard)]
	public extern static String _cd29576576563da3(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.LCID.get", WhiteListOp.Discard)]
	public extern static Number _9152aa33e0560712(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.KeyboardLayoutId.get", WhiteListOp.Discard)]
	public extern static Number _13b0607d8916da7b(String instance);

	///<summary>Gets the list of supported cultures filtered by the specified <see cref="T:System.Globalization.CultureTypes" /> parameter.</summary>
	[WhiteList("static System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes)", WhiteListOp.Discard)]
	public extern static System.Globalization.CultureInfo[] _40087650ec4f5285(object types);

	[WhiteList("virtual System.Globalization.CultureInfo.Name.get", WhiteListOp.Discard)]
	public extern static string _822a986168c7c539(String instance);

	[WhiteList("System.Globalization.CultureInfo.IetfLanguageTag.get", WhiteListOp.Discard)]
	public extern static string _9c9f6e469362911e(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.DisplayName.get", WhiteListOp.Discard)]
	public extern static string _59b041331098ad55(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.NativeName.get", WhiteListOp.Discard)]
	public extern static string _a4804f687bfc0013(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.EnglishName.get", WhiteListOp.Discard)]
	public extern static string _97ad9637d1f75e7c(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.TwoLetterISOLanguageName.get", WhiteListOp.Discard)]
	public extern static string _112fba1dc945fa1a(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.ThreeLetterISOLanguageName.get", WhiteListOp.Discard)]
	public extern static string _285ede13a469ce7b(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.ThreeLetterWindowsLanguageName.get", WhiteListOp.Discard)]
	public extern static string _1f981ccac713f3d9(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.CompareInfo.get", WhiteListOp.Discard)]
	public extern static System.Globalization.CompareInfo _90f3bc0ef0b5d452(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.TextInfo.get", WhiteListOp.Discard)]
	public extern static System.Globalization.TextInfo _e82427b8b3bb35c4(String instance);

	///<summary>Determines whether the specified object is the same culture as the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
	[WhiteList("override System.Globalization.CultureInfo.Equals(object)", WhiteListOp.Discard)]
	public extern static bool _dfe1a8cc1c9e5e52(String instance, Object? value);

	///<summary>Serves as a hash function for the current <see cref="T:System.Globalization.CultureInfo" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
	[WhiteList("override System.Globalization.CultureInfo.GetHashCode()", WhiteListOp.Discard)]
	public extern static Number _b3aae6e43cf38d8a(String instance);

	///<summary>Returns a string containing the name of the current <see cref="T:System.Globalization.CultureInfo" /> in the format languagecode2-country/regioncode2.</summary>
	[WhiteList("override System.Globalization.CultureInfo.ToString()", WhiteListOp.Discard)]
	public extern static string _559b27327f84f1af(String instance);

	///<summary>Gets an object that defines how to format the specified type.</summary>
	[WhiteList("virtual System.Globalization.CultureInfo.GetFormat(System.Type)", WhiteListOp.Discard)]
	public extern static Object? _f8c5b22a1e711ffe(String instance, object formatType);

	[WhiteList("virtual System.Globalization.CultureInfo.IsNeutralCulture.get", WhiteListOp.Discard)]
	public extern static bool _0bedb111138c14ed(String instance);

	[WhiteList("System.Globalization.CultureInfo.CultureTypes.get", WhiteListOp.Discard)]
	public extern static System.Globalization.CultureTypes _7309acaa147028c6(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.NumberFormat.get", WhiteListOp.Discard)]
	public extern static System.Globalization.NumberFormatInfo _7472734ec9a97b33(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.NumberFormat.set", WhiteListOp.Discard)]
	public extern static void _5943bc5946aadc23(String instance, object value);

	[WhiteList("virtual System.Globalization.CultureInfo.DateTimeFormat.get", WhiteListOp.Discard)]
	public extern static System.Globalization.DateTimeFormatInfo _3084f61a73019848(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.DateTimeFormat.set", WhiteListOp.Discard)]
	public extern static void _a72ad1794743a630(String instance, object value);

	///<summary>Refreshes cached culture-related information.</summary>
	[WhiteList("System.Globalization.CultureInfo.ClearCachedData()", WhiteListOp.Discard)]
	public extern static void _73e163fe0d6f4c41(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.Calendar.get", WhiteListOp.Discard)]
	public extern static GregorianCalendar _2ab4f6aaba1be337(String instance);

	[WhiteList("virtual System.Globalization.CultureInfo.OptionalCalendars.get", WhiteListOp.Discard)]
	public extern static System.Globalization.Calendar[] _5031598284c711b5(String instance);

	[WhiteList("System.Globalization.CultureInfo.UseUserOverride.get", WhiteListOp.Discard)]
	public extern static bool _4b6ab04957c3b1d8(String instance);

	///<summary>Gets an alternate user interface culture suitable for console applications when the default graphic user interface culture is unsuitable.</summary>
	[WhiteList("System.Globalization.CultureInfo.GetConsoleFallbackUICulture()", WhiteListOp.Discard)]
	public extern static String _e746a9049464da41(String instance);

	///<summary>Creates a copy of the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
	[WhiteList("virtual System.Globalization.CultureInfo.Clone()", WhiteListOp.Discard)]
	public extern static Object _52d3a5ff068445a1(String instance);

	///<summary>Returns a read-only wrapper around the specified <see cref="T:System.Globalization.CultureInfo" /> object.</summary>
	[WhiteList("static System.Globalization.CultureInfo.ReadOnly(System.Globalization.CultureInfo)", WhiteListOp.Discard)]
	public extern static String _f3218a923929edaf(String ci);

	[WhiteList("System.Globalization.CultureInfo.IsReadOnly.get", WhiteListOp.Discard)]
	public extern static bool _1a2fc3e83feec6fd(String instance);

	///<summary>Retrieves a cached, read-only instance of a culture by using the specified culture identifier.</summary>
	[WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(int)", WhiteListOp.Discard)]
	public extern static String _be269d85f3085630(Number culture);

	///<summary>Retrieves a cached, read-only instance of a culture using the specified culture name.</summary>
	[WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(string)", WhiteListOp.Discard)]
	public extern static String _a536c354b66082b9(object name);

	///<summary>Retrieves a cached, read-only instance of a culture. Parameters specify a culture that is initialized with the <see cref="T:System.Globalization.TextInfo" /> and <see cref="T:System.Globalization.CompareInfo" /> objects specified by another culture.</summary>
	[WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(string, string)", WhiteListOp.Discard)]
	public extern static String _e17d240a4c1653be(object name, object altName);

	///<summary>Retrieves a cached, read-only instance of a culture.</summary>
	[WhiteList("static System.Globalization.CultureInfo.GetCultureInfo(string, bool)", WhiteListOp.Discard)]
	public extern static String _a43a2bb07ef29293(object name, object predefinedOnly);

	///<summary>Deprecated. Retrieves a read-only <see cref="T:System.Globalization.CultureInfo" /> object having linguistic characteristics that are identified by the specified RFC 4646 language tag.</summary>
	[WhiteList("static System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(string)", WhiteListOp.Discard)]
	public extern static String _1d57f4ce6dee8a81(object name);
}
