namespace Jazor.CLR;

[ECMAScriptModule("System/Globalization/CultureInfoModule.js")]
[Jazor(Op.Alias, "System.Globalization.CultureInfo","String")]
public static class CultureInfoModule
{
	private const string InvariantCultureName = "";
	private const string InvariantCultureDisplayName = "Invariant Language (Invariant Country)";
	private const string InvariantIetfLanguageTag = "iv";
	private const string InvariantThreeLetterIsoLanguageName = "ivl";
	private const string InvariantThreeLetterWindowsLanguageName = "IVL";

	private static Number GetStringHashCode(string text)
	{
		var hash = 0;
		for (var i = 0; i < text.Length; i++)
			hash = ((hash << 5) - hash) + text[i];

		return hash | 0;
	}

	private static string GetParentCulture(string instance)
	{
		if (instance.Length == 0)
			return InvariantCultureName;

		var index = instance.LastIndexOf('-');
		if (index < 0)
			return InvariantCultureName;

		return instance.Substring(0, index);
	}

	private static string GetLanguagePart(string instance)
		=> instance.Length == 0 ? InvariantIetfLanguageTag : instance.Split('-')[0];

	private static string GetIetfLanguageTag(string instance)
		=> instance.Length == 0 ? InvariantIetfLanguageTag : instance;

	private static string GetDisplayName(string instance)
		=> instance.Length == 0 ? InvariantCultureDisplayName : instance;

	private static string CreateSpecificCulture(string name)
	{
		if (name.Length == 0)
			return InvariantCultureName;

		var supported = Intl.DateTimeFormat.SupportedLocalesOf(name);
		if (supported.Length == 0)
			throw new Error($"CultureNotFoundException: Culture '{name}' is not supported.");

		var resolved = supported[0]!;
		if (resolved.Contains("-"))
			return resolved;

		var worldLocale = resolved + "-001";
		var worldSupported = Intl.DateTimeFormat.SupportedLocalesOf(worldLocale);
		return worldSupported.Length != 0 ? worldLocale : resolved;
	}

	private static string CreateCultureInfo(string name)
	{
		if (name.Length == 0)
			return InvariantCultureName;

		var supported = Intl.DateTimeFormat.SupportedLocalesOf(name);
		if (supported.Length == 0)
			throw new Error($"CultureNotFoundException: Culture '{name}' is not supported.");

		return supported[0]!;
	}

	private static string CreateCultureInfo(Number culture)
		=> throw new Error($"NotSupportedException: LCID-based CultureInfo constructors are not supported: {culture}.");

	private static string GetCultureInfoByIetfLanguageTag(string name)
	{
		if (name == InvariantIetfLanguageTag)
			return InvariantCultureName;

		return CreateCultureInfo(name);
	}

	/// <summary>
	/// C#: new CultureInfo(name)
	/// JS: name (culture name as string)
	/// </summary>
	[Jazor(Op.Import, "System.Globalization.CultureInfo.CultureInfo(string)")]
	public static string _b7486264ae338f27(string name)
		=> CreateCultureInfo(name);

	/// <summary>
	/// C#: new CultureInfo(name, useUserOverride)
	/// JS: name (culture name as string, ignore useUserOverride in JS)
	/// </summary>
	[Jazor(Op.Import, "System.Globalization.CultureInfo.CultureInfo(string, bool)")]
	public static string _df21a93fd9f84197(string name, bool useUserOverride)
		=> CreateCultureInfo(name);

	/// <summary>
	/// C#: new CultureInfo(culture)
	/// JS: culture.toString() (culture ID as string)
	/// </summary>
	[Jazor(Op.Import, "System.Globalization.CultureInfo.CultureInfo(int)")]
	public static string _22aaac09e253b1f9(Number culture)
		=> CreateCultureInfo(culture);

	/// <summary>
	/// C#: new CultureInfo(culture, useUserOverride)
	/// JS: culture.toString()
	/// </summary>
	[Jazor(Op.Import, "System.Globalization.CultureInfo.CultureInfo(int, bool)")]
	public static string _d0948ef9f698ec85(Number culture, bool useUserOverride)
		=> CreateCultureInfo(culture);

	/// <summary>
	/// C#: CultureInfo.CreateSpecificCulture(name)
	/// JS: canonicalize supported locale; try to force a region when only a neutral locale is given
	/// </summary>
	[Jazor(Op.Import, "static System.Globalization.CultureInfo.CreateSpecificCulture(string)")]
	public static string _a078d5ccbbf2345a(string name)
		=> CreateSpecificCulture(name);

	/// <summary>
	/// C#: CultureInfo.CurrentCulture
	/// JS: Intl.DateTimeFormat().resolvedOptions().locale
	/// </summary>
	[Jazor(Op.Inline, "static System.Globalization.CultureInfo.CurrentCulture.get", "Intl.DateTimeFormat().resolvedOptions().locale")]
	public extern static String _1a26e2e2e4e0ca1d();

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.CurrentCulture.set")]
	public extern static void _82cfca57d721204e(String value);

	/// <summary>
	/// C#: CultureInfo.CurrentUICulture
	/// JS: navigator.language || 'en'
	/// </summary>
	[Jazor(Op.Inline, "static System.Globalization.CultureInfo.CurrentUICulture.get", "(typeof navigator !== 'undefined' ? navigator.language : 'en')")]
	public extern static String _eca32c250ead7de9();

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.CurrentUICulture.set")]
	public extern static void _7e355a1a63351619(String value);

	/// <summary>
	/// C#: CultureInfo.InstalledUICulture
	/// JS: 'en' (default)
	/// </summary>
	[Jazor(Op.Inline, "static System.Globalization.CultureInfo.InstalledUICulture.get", "'en'")]
	public extern static String _98e743867688a06d();

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.get")]
	public extern static String? _3c1fdac9ccc43427();

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentCulture.set")]
	public extern static void _96d14148886217cb(String? value);

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.get")]
	public extern static String? _abdb5d2bfd934cfc();

	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.DefaultThreadCurrentUICulture.set")]
	public extern static void _12da8bfb928d7414(String? value);

	/// <summary>
	/// C#: CultureInfo.InvariantCulture
	/// JS: '' (use empty string as invariant culture carrier)
	/// </summary>
	[Jazor(Op.Import, "static System.Globalization.CultureInfo.InvariantCulture.get")]
	public static string _e4c4d53d69e72382()
		=> InvariantCultureName;

	/// <summary>
	/// C#: instance.Parent
	/// JS: strip the last region/script segment; neutral cultures parent to invariant
	/// </summary>
	[Jazor(Op.Import, "virtual System.Globalization.CultureInfo.Parent.get")]
	public static string _cd29576576563da3(string instance)
		=> GetParentCulture(instance);

	/// <summary>
	/// C#: instance.LCID
	/// JS: 0 (locale ID not available in JS)
	/// </summary>
	[Jazor(Op.Inline, "virtual System.Globalization.CultureInfo.LCID.get", "0")]
	public extern static Number _9152aa33e0560712(string instance);

	/// <summary>
	/// C#: instance.KeyboardLayoutId
	/// JS: 0 (not available in JS)
	/// </summary>
	[Jazor(Op.Inline, "virtual System.Globalization.CultureInfo.KeyboardLayoutId.get", "0")]
	public extern static Number _13b0607d8916da7b(string instance);

	///<summary>Gets the list of supported cultures filtered by the specified <see cref="T:System.Globalization.CultureTypes" /> parameter.</summary>
	[Jazor(Op.Discard ,"static System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes)")]
	public extern static System.Globalization.CultureInfo[] _40087650ec4f5285(object types);

	/// <summary>
	/// C#: instance.Name
	/// JS: instance
	/// </summary>
	[Jazor(Op.Inline, "virtual System.Globalization.CultureInfo.Name.get", "__arg1")]
	public extern static string _822a986168c7c539(string instance);

	/// <summary>
	/// C#: instance.IetfLanguageTag
	/// JS: instance, except invariant culture maps to "iv"
	/// </summary>
	[Jazor(Op.Import, "System.Globalization.CultureInfo.IetfLanguageTag.get")]
	public static string _9c9f6e469362911e(string instance)
		=> GetIetfLanguageTag(instance);

	/// <summary>
	/// C#: instance.DisplayName
	/// JS: instance, except invariant culture uses a fixed display name
	/// </summary>
	[Jazor(Op.Import, "virtual System.Globalization.CultureInfo.DisplayName.get")]
	public static string _59b041331098ad55(string instance)
		=> GetDisplayName(instance);

	/// <summary>
	/// C#: instance.NativeName
	/// JS: instance, except invariant culture uses a fixed display name
	/// </summary>
	[Jazor(Op.Import, "virtual System.Globalization.CultureInfo.NativeName.get")]
	public static string _a4804f687bfc0013(string instance)
		=> GetDisplayName(instance);

	/// <summary>
	/// C#: instance.EnglishName
	/// JS: instance, except invariant culture uses a fixed display name
	/// </summary>
	[Jazor(Op.Import, "virtual System.Globalization.CultureInfo.EnglishName.get")]
	public static string _97ad9637d1f75e7c(string instance)
		=> GetDisplayName(instance);

	/// <summary>
	/// C#: instance.TwoLetterISOLanguageName
	/// JS: language part, except invariant culture maps to "iv"
	/// </summary>
	[Jazor(Op.Import, "virtual System.Globalization.CultureInfo.TwoLetterISOLanguageName.get")]
	public static string _112fba1dc945fa1a(string instance)
		=> GetLanguagePart(instance);

	/// <summary>
	/// C#: instance.ThreeLetterISOLanguageName
	/// JS: language part, except invariant culture maps to "ivl"
	/// </summary>
	[Jazor(Op.Import, "virtual System.Globalization.CultureInfo.ThreeLetterISOLanguageName.get")]
	public static string _285ede13a469ce7b(string instance)
		=> instance.Length == 0 ? InvariantThreeLetterIsoLanguageName : instance.Split('-')[0];

	/// <summary>
	/// C#: instance.ThreeLetterWindowsLanguageName
	/// JS: language part, except invariant culture maps to "IVL"
	/// </summary>
	[Jazor(Op.Import, "virtual System.Globalization.CultureInfo.ThreeLetterWindowsLanguageName.get")]
	public static string _1f981ccac713f3d9(string instance)
		=> instance.Length == 0 ? InvariantThreeLetterWindowsLanguageName : instance.Split('-')[0];

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.CompareInfo.get")]
	public extern static System.Globalization.CompareInfo _90f3bc0ef0b5d452(string instance);

	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.TextInfo.get")]
	public extern static System.Globalization.TextInfo _e82427b8b3bb35c4(string instance);

	///<summary>Determines whether the specified object is the same culture as the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
	[Jazor(Op.Import ,"override System.Globalization.CultureInfo.Equals(object)")]
	public static bool _dfe1a8cc1c9e5e52(string instance, object? value)
	{
		var other = value as string;
		return other != null && instance == other;
	}

	///<summary>Serves as a hash function for the current <see cref="T:System.Globalization.CultureInfo" />, suitable for hashing algorithms and data structures, such as a hash table.</summary>
	[Jazor(Op.Import ,"override System.Globalization.CultureInfo.GetHashCode()")]
	public static Number _b3aae6e43cf38d8a(string instance)
		=> GetStringHashCode(instance);

	///<summary>Returns a string containing the name of the current <see cref="T:System.Globalization.CultureInfo" /> in the format languagecode2-country/regioncode2.</summary>
	[Jazor(Op.Import ,"override System.Globalization.CultureInfo.ToString()")]
	public static string _559b27327f84f1af(string instance)
		=> instance;

	///<summary>Gets an object that defines how to format the specified type.</summary>
	[Jazor(Op.Discard ,"virtual System.Globalization.CultureInfo.GetFormat(System.Type)")]
	public extern static object? _f8c5b22a1e711ffe(string instance, object formatType);

	[Jazor(Op.Import ,"virtual System.Globalization.CultureInfo.IsNeutralCulture.get")]
	public static bool _0bedb111138c14ed(string instance)
		=> instance.Length != 0 && !instance.Contains("-");

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
	[Jazor(Op.Import ,"System.Globalization.CultureInfo.ClearCachedData()")]
	public static void _73e163fe0d6f4c41(string instance)
	{
	}

	[Jazor(Op.Import ,"virtual System.Globalization.CultureInfo.Calendar.get")]
	public static string _2ab4f6aaba1be337(string instance)
		=> GregorianCalendarModule._23b9e8d671b5210e();

	[Jazor(Op.Import ,"virtual System.Globalization.CultureInfo.OptionalCalendars.get")]
	public static string[] _5031598284c711b5(string instance)
		=> [GregorianCalendarModule._23b9e8d671b5210e()];

	[Jazor(Op.Import ,"System.Globalization.CultureInfo.UseUserOverride.get")]
	public static bool _4b6ab04957c3b1d8(string instance)
		=> false;

	///<summary>Gets an alternate user interface culture suitable for console applications when the default graphic user interface culture is unsuitable.</summary>
	[Jazor(Op.Import ,"System.Globalization.CultureInfo.GetConsoleFallbackUICulture()")]
	public static String _e746a9049464da41(string instance)
		=> instance;

	///<summary>Creates a copy of the current <see cref="T:System.Globalization.CultureInfo" />.</summary>
	[Jazor(Op.Import ,"virtual System.Globalization.CultureInfo.Clone()")]
	public static object _52d3a5ff068445a1(string instance)
		=> instance;

	///<summary>Returns a read-only wrapper around the specified <see cref="T:System.Globalization.CultureInfo" /> object.</summary>
	[Jazor(Op.Import ,"static System.Globalization.CultureInfo.ReadOnly(System.Globalization.CultureInfo)")]
	public static String _f3218a923929edaf(String ci)
		=> ci;

	[Jazor(Op.Import ,"System.Globalization.CultureInfo.IsReadOnly.get")]
	public static bool _1a2fc3e83feec6fd(string instance)
		=> true;

	///<summary>Retrieves a cached, read-only instance of a culture by using the specified culture identifier.</summary>
	[Jazor(Op.Import ,"static System.Globalization.CultureInfo.GetCultureInfo(int)")]
	public static String _be269d85f3085630(Number culture)
		=> CreateCultureInfo(culture);

	///<summary>Retrieves a cached, read-only instance of a culture using the specified culture name.</summary>
	[Jazor(Op.Import ,"static System.Globalization.CultureInfo.GetCultureInfo(string)")]
	public static String _a536c354b66082b9(string name)
		=> CreateCultureInfo(name);

	///<summary>Retrieves a cached, read-only instance of a culture. Parameters specify a culture that is initialized with the <see cref="T:System.Globalization.TextInfo" /> and <see cref="T:System.Globalization.CompareInfo" /> objects specified by another culture.</summary>
	[Jazor(Op.Import ,"static System.Globalization.CultureInfo.GetCultureInfo(string, string)")]
	public static String _e17d240a4c1653be(string name, string altName)
	{
		CreateCultureInfo(altName);
		return CreateCultureInfo(name);
	}

	///<summary>Retrieves a cached, read-only instance of a culture.</summary>
	[Jazor(Op.Import ,"static System.Globalization.CultureInfo.GetCultureInfo(string, bool)")]
	public static String _a43a2bb07ef29293(string name, bool predefinedOnly)
		=> CreateCultureInfo(name);

	///<summary>Deprecated. Retrieves a read-only <see cref="T:System.Globalization.CultureInfo" /> object having linguistic characteristics that are identified by the specified RFC 4646 language tag.</summary>
	[Jazor(Op.Import ,"static System.Globalization.CultureInfo.GetCultureInfoByIetfLanguageTag(string)")]
	public static String _1d57f4ce6dee8a81(string name)
		=> GetCultureInfoByIetfLanguageTag(name);
}
