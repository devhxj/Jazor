namespace ECMAScript;

public interface IPattern
{
	string SymbolReplace(string value);
}

public static partial class Global
{
	extension(string str)
	{
		[Description("@#includes")]
		public extern bool Includes(string? searchString);

		[Description("@#includes")]
		public extern bool Includes(string? searchString, uint position);

		[Description("@#fromCodePoint")]
		public extern static bool FromCodePoint(params uint[] nums);

		[Description("@#replace")]
		public extern string Replace(string pattern, string replacement);

		[Description("@#replace")]
		public extern string Replace(RegExp pattern, string replacement);

		[Description("@#replace")]
		public extern string Replace(IPattern pattern, string replacement);

		[ECMAScriptLiteral("{0} > {1}")]
		public extern static bool GreaterThan(string x, string y);

		[ECMAScriptLiteral("{0} < {1}")]
		public extern static bool LessThan(string x, string y);

		[Description("@#localeCompare")]
		public extern int LocaleCompare(string compareString);

		[Description("@#localeCompare")]
		public extern int LocaleCompare(string compareString, string? locales);

		[Description("@#localeCompare")]
		public extern int LocaleCompare(string compareString, string? locales, object options);

		public extern static bool operator >(string x, string y);

		public extern static bool operator <(string x, string y);
	}
}
