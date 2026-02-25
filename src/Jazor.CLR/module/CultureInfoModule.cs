namespace Jazor.CLR;

[ECMAScriptModule]
[Jazor(Op.Import, "System.Globalization.CultureInfo","System/Globalization/CultureInfoModule.js")]
public static class CultureInfoModule
{
	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name.</summary>
	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.CultureInfo(string)")]
	public extern static string _b7486264ae338f27(string name);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by name and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.CultureInfo(string, bool)")]
	public extern static string _df21a93fd9f84197(string name, bool useUserOverride);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier.</summary>
	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.CultureInfo(int)")]
	public extern static string _22aaac09e253b1f9(Number culture);

	///<summary>Initializes a new instance of the <see cref="T:System.Globalization.CultureInfo" /> class based on the culture specified by the culture identifier and on a value that specifies whether to use the user-selected culture settings from Windows.</summary>
	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.CultureInfo(int, bool)")]
	public extern static string _d0948ef9f698ec85(Number culture, bool useUserOverride);

	///<summary>Creates a <see cref="T:System.Globalization.CultureInfo" /> that represents the specific culture that is associated with the specified name.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.CreateSpecificCulture(string)")]
	public extern static String _a078d5ccbbf2345a(string name);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.CurrentCulture.get")]
	public extern static String _1a26e2e2e4e0ca1d(string instance);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.CurrentCulture.set")]
	public extern static void _82cfca57d721204e(string instance, String value);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.CurrentUICulture.get")]
	public extern static String _eca32c250ead7de9(string instance);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.CurrentUICulture.set")]
	public extern static void _7e355a1a63351619(string instance, String value);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.InstalledUICulture.get")]
	public extern static String _98e743867688a06d(string instance);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.get")]
	public extern static String? _3c1fdac9ccc43427(string instance);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.set")]
	public extern static void _96d14148886217cb(string instance, String? value);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.get")]
	public extern static String? _abdb5d2bfd934cfc(string instance);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.set")]
	public extern static void _12da8bfb928d7414(string instance, String? value);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.InvariantCulture.get")]
	public extern static String _e4c4d53d69e72382(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.Parent.get")]
	public extern static String _cd29576576563da3(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.LCID.get")]
	public extern static Number _9152aa33e0560712(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.KeyboardLayoutId.get")]
	public extern static Number _13b0607d8916da7b(string instance);

	///<summary>Gets the list of supported cultures filtered by the specified <see cref="T:System.Globalization.CultureTypes" /> parameter.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes)")]
	public extern static System.Globalization.CultureInfo[] _40087650ec4f5285(object types);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.Name.get")]
	public extern static string _822a986168c7c539(string instance);

	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.IetfLanguageTag.get")]
	public extern static string _9c9f6e469362911e(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.DisplayName.get")]
	public extern static string _59b041331098ad55(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.NativeName.get")]
	public extern static string _a4804f687bfc0013(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.EnglishName.get")]
	public extern static string _97ad9637d1f75e7c(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.TwoLetterISOLanguageName.get")]
	public extern static string _112fba1dc945fa1a(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.ThreeLetterISOLanguageName.get")]
	public extern static string _285ede13a469ce7b(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.ThreeLetterWindowsLanguageName.get")]
	public extern static string _1f981ccac713f3d9(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.CompareInfo.get")]
	public extern static System.Globalization.CompareInfo _90f3bc0ef0b5d452(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.TextInfo.get")]
	public extern static System.Globalization.TextInfo _e82427b8b3bb35c4(string instance);

	///<summary>Determines whether the specified object is the same culture as the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.CultureInfo.Equals(object)")]
	public extern static bool _dfe1a8cc1c9e5e52(string instance, object? value);

	///<summary>Serves as a hash function for the current <see cref="T:System.Globalization.CultureInfo" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.CultureInfo.GetHashCode()")]
	public extern static Number _b3aae6e43cf38d8a(string instance);

	///<summary>Returns a string containing the name of the current <see cref="T:System.Globalization.CultureInfo" /> in the format languagecode2-country/regioncode2.</summary>
	[Jazor(Op.Discard ,"override System.Globalization.CultureInfo.ToString()")]
	public extern static string _559b27327f84f1af(string instance);

	///<summary>Gets an object that defines how to format the specified type.</summary>
	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.GetFormat(System.Type)")]
	public extern static object? _f8c5b22a1e711ffe(string instance, object formatType);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.IsNeutralCulture.get")]
	public extern static bool _0bedb111138c14ed(string instance);

	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.CultureTypes.get")]
	public extern static System.Globalization.CultureTypes _7309acaa147028c6(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.NumberFormat.get")]
	public extern static System.Globalization.NumberFormatInfo _7472734ec9a97b33(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.NumberFormat.set")]
	public extern static void _5943bc5946aadc23(string instance, object value);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.DateTimeFormat.get")]
	public extern static System.Globalization.DateTimeFormatInfo _3084f61a73019848(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.DateTimeFormat.set")]
	public extern static void _a72ad1794743a630(string instance, object value);

	///<summary>Refreshes cached culture-related information.</summary>
	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.ClearCachedData()")]
	public extern static void _73e163fe0d6f4c41(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.Calendar.get")]
	public extern static GregorianCalendar _2ab4f6aaba1be337(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.OptionalCalendars.get")]
	public extern static System.Globalization.Calendar[] _5031598284c711b5(string instance);

	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.UseUserOverride.get")]
	public extern static bool _4b6ab04957c3b1d8(string instance);

	///<summary>Gets an alternate user interface culture suitable for console applications when the default graphic user interface culture is unsuitable.</summary>
	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.GetConsoleFallbackUICulture()")]
	public extern static String _e746a9049464da41(string instance);

	///<summary>Creates a copy of the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.Clone()")]
	public extern static object _52d3a5ff068445a1(string instance);

	///<summary>Returns a read-only wrapper around the specified <see cref="T:System.Globalization.CultureInfo" /> object.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.ReadOnly(System.Globalization.CultureInfo)")]
	public extern static String _f3218a923929edaf(String ci);

	[Jazor(Op.Discard ,"System.Globalization.CultureInfo.IsReadOnly.get")]
	public extern static bool _1a2fc3e83feec6fd(string instance);

	///<summary>Retrieves a cached, read-only instance of a culture by using the specified culture identifier.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.GetCultureInfo(int)")]
	public extern static String _be269d85f3085630(Number culture);

	///<summary>Retrieves a cached, read-only instance of a culture using the specified culture name.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.GetCultureInfo(string)")]
	public extern static String _a536c354b66082b9(string name);

	///<summary>Retrieves a cached, read-only instance of a culture. Parameters specify a culture that is initialized with the <see cref="T:System.Globalization.TextInfo" /> and <see cref="T:System.Globalization.CompareInfo" /> objects specified by another culture.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.GetCultureInfo(string, string)")]
	public extern static String _e17d240a4c1653be(string name, string altName);

	///<summary>Retrieves a cached, read-only instance of a culture.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.GetCultureInfo(string, bool)")]
	public extern static String _a43a2bb07ef29293(string name, bool predefinedOnly);

	///<summary>Deprecated. Retrieves a read-only <see cref="T:System.Globalization.CultureInfo" /> object having linguistic characteristics that are identified by the specified RFC 4646 language tag.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(string)")]
	public extern static String _1d57f4ce6dee8a81(string name);
}
