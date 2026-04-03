namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPattern
{
	string SymbolReplace(string value);
}

public static partial class Global
{
	/// <summary>
	/// Projection of JavaScript String built-ins onto C# string extension members.
	/// Any naming deviation here should be treated as a C# syntax escape hatch,
	/// not as a semantic difference from the JavaScript runtime.
	/// </summary>
	extension(string str)
	{
		[Description("@#includes")]
		public extern bool Includes(string? searchString);

		[Description("@#includes")]
		public extern bool Includes(string? searchString, Number position);

		[Description("@#fromCodePoint")]
		public extern static string FromCodePoint(Number num);

		[Description("@#fromCodePoint")]
		public extern static string FromCodePoint(params Number[] nums);

		[Description("@#replace")]
		public extern string Replace(string pattern, string replacement);

		[Description("@#replace")]
		public extern string Replace(RegExp pattern, string replacement);

		[Description("@#replace")]
		public extern string Replace(IPattern pattern, string replacement);

		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString);

		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, string? locales);

		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, string? locales, object options);

		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, string[] locales);

		[Description("@#localeCompare")]
		public extern Number LocaleCompare(string compareString, string[] locales, object options);

		public extern static bool operator >(string x, string y);

		public extern static bool operator <(string x, string y);
	}
}
