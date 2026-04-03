namespace ECMAScript;

[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPattern
{
	string SymbolReplace(string value);
}

/// <summary>
/// Bridge shape used by JavaScript tagged template helpers such as <c>String.raw</c>.
/// This stays hidden because it is a protocol object, not a standalone JavaScript runtime host.
/// </summary>
[ECMAScript]
[Description("@#")]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITemplateStringsArray : IArray<string>
{
	/// <summary>
	/// Raw template segments as exposed by JavaScript template literal objects.
	/// </summary>
	[Description("@#raw")]
	IArray<string> Raw { get; }
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
		/// <summary>
		/// Projection of JavaScript <c>String.prototype.includes</c> onto C# string extension members.
		/// The public surface stays close to JavaScript runtime shape and avoids introducing
		/// a separate CLR wrapper type for string hosts.
		/// </summary>
		[Description("@#includes")]
		public extern bool Includes(string? searchString);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.includes</c> with an explicit start position.
		/// </summary>
		[Description("@#includes")]
		public extern bool Includes(string? searchString, Number position);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.startsWith</c> onto C# string extension members.
		/// </summary>
		[Description("@#startsWith")]
		public extern bool StartsWith(string searchString, Number? position = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.endsWith</c> onto C# string extension members.
		/// </summary>
		[Description("@#endsWith")]
		public extern bool EndsWith(string searchString, Number? length = null);

		[Description("@#fromCodePoint")]
		public extern static string FromCodePoint(Number num);

		[Description("@#fromCodePoint")]
		public extern static string FromCodePoint(params Number[] nums);

		/// <summary>
		/// Projection of JavaScript <c>String.fromCharCode</c> onto the global string host.
		/// The static member is kept on the string host instead of inventing a CLR helper type.
		/// </summary>
		[Description("@#fromCharCode")]
		public extern static string FromCharCode(Number num);

		/// <summary>
		/// Projection of JavaScript <c>String.fromCharCode</c> for multiple code units.
		/// </summary>
		[Description("@#fromCharCode")]
		public extern static string FromCharCode(params Number[] nums);

		/// <summary>
		/// Projection of JavaScript <c>String.raw</c>.
		/// The first argument uses a hidden bridge interface because JavaScript supplies a template-literal protocol object rather than a distinct runtime host type.
		/// </summary>
		[Description("@#raw")]
		public extern static string Raw(ITemplateStringsArray template, params object?[] substitutions);

		[Description("@#replace")]
		public extern string Replace(string pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replace</c> with a callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// </summary>
		[Description("@#replace")]
		public extern string Replace(string pattern, Func<string, string> replacement);

		[Description("@#replace")]
		public extern string Replace(RegExp pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replace</c> with a regular-expression callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// </summary>
		[Description("@#replace")]
		public extern string Replace(RegExp pattern, Func<string, string> replacement);

		[Description("@#replace")]
		public extern string Replace(IPattern pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c>.
		/// This stays separate from <c>replace</c> because JavaScript treats them as distinct runtime members.
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(string pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> with a callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(string pattern, Func<string, string> replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> with a regular expression pattern.
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(RegExp pattern, string replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.replaceAll</c> with a regular-expression callback replacer.
		/// This overload models the common case where C# code only needs the matched substring even though JavaScript supplies additional callback arguments.
		/// </summary>
		[Description("@#replaceAll")]
		public extern string ReplaceAll(RegExp pattern, Func<string, string> replacement);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.split</c>.
		/// <see cref="Array{T}"/> is used because JavaScript returns a real array rather than an iterator.
		/// </summary>
		[Description("@#split")]
		public extern Array<string> Split(string? separator = null, Number? limit = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.split</c> with a regular expression separator.
		/// <see cref="Array{T}"/> is used because JavaScript returns a real array rather than an iterator.
		/// </summary>
		[Description("@#split")]
		public extern Array<string> Split(RegExp separator, Number? limit = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.substring</c>.
		/// </summary>
		[Description("@#substring")]
		public extern string Substring(Number start);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.substring</c> with an explicit end index.
		/// </summary>
		[Description("@#substring")]
		public extern string Substring(Number start, Number end);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.slice</c>.
		/// </summary>
		[Description("@#slice")]
		public extern string Slice(Number? start = null, Number? end = null);

		/// <summary>
		/// Legacy projection of JavaScript <c>String.prototype.substr</c>.
		/// This remains exposed for runtime compatibility even though newer code should generally prefer <see cref="Slice" /> or <see cref="Substring(Number)" />.
		/// </summary>
		[Description("@#substr")]
		public extern string Substr(Number start, Number? length = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.indexOf</c>.
		/// </summary>
		[Description("@#indexOf")]
		public extern Number IndexOf(string searchString, Number? position = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.lastIndexOf</c>.
		/// </summary>
		[Description("@#lastIndexOf")]
		public extern Number LastIndexOf(string searchString, Number? position = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.trim</c>.
		/// </summary>
		[Description("@#trim")]
		public extern string Trim();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.trimStart</c>.
		/// </summary>
		[Description("@#trimStart")]
		public extern string TrimStart();

		/// <summary>
		/// Legacy JavaScript alias for <see cref="TrimStart"/>.
		/// This remains exposed because many runtimes still provide <c>String.prototype.trimLeft</c> for web compatibility.
		/// </summary>
		[Description("@#trimLeft")]
		public extern string TrimLeft();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.trimEnd</c>.
		/// </summary>
		[Description("@#trimEnd")]
		public extern string TrimEnd();

		/// <summary>
		/// Legacy JavaScript alias for <see cref="TrimEnd"/>.
		/// This remains exposed because many runtimes still provide <c>String.prototype.trimRight</c> for web compatibility.
		/// </summary>
		[Description("@#trimRight")]
		public extern string TrimRight();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.padStart</c>.
		/// </summary>
		[Description("@#padStart")]
		public extern string PadStart(Number targetLength, string? padString = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.padEnd</c>.
		/// </summary>
		[Description("@#padEnd")]
		public extern string PadEnd(Number targetLength, string? padString = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.repeat</c>.
		/// </summary>
		[Description("@#repeat")]
		public extern string Repeat(Number count);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.charAt</c>.
		/// </summary>
		[Description("@#charAt")]
		public extern string CharAt(Number index);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.charCodeAt</c>.
		/// </summary>
		[Description("@#charCodeAt")]
		public extern Number CharCodeAt(Number index);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.codePointAt</c>.
		/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
		/// and the C# projection maps that absence to <see langword="null" />.
		/// </summary>
		[Description("@#codePointAt")]
		public extern Number? CodePointAt(Number index);

		/// <summary>
		/// C# host projection of JavaScript <c>String.prototype.at</c>.
		/// Nullable is used because JavaScript returns <c>undefined</c> for an out-of-range index,
		/// and the C# projection maps that absence to <see langword="null" />.
		/// </summary>
		[Description("@#at")]
		public extern string? At(Number index);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.match</c>.
		/// Nullable is used because JavaScript returns <c>null</c> when no match is found.
		/// The array elements are nullable because unmatched capture groups are <c>undefined</c> in JavaScript,
		/// and this projection maps that absence to <see langword="null" />.
		/// </summary>
		[Description("@#match")]
		public extern Array<string?>? Match(RegExp regexp);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.match</c> with a string pattern.
		/// JavaScript converts the string to a regular expression before matching.
		/// </summary>
		[Description("@#match")]
		public extern Array<string?>? Match(string pattern);

		/// <summary>
		/// Returns the JavaScript iterator produced by <c>String.prototype.matchAll()</c>.
		/// <see cref="IEnumerable{T}"/> is used as the common C# host surface for JavaScript iterables.
		/// </summary>
		[Description("@#matchAll")]
		public extern IEnumerable<RegExpResult> MatchAll(RegExp regexp);

		/// <summary>
		/// Returns the JavaScript iterator produced by <c>String.prototype.matchAll()</c> with a string pattern.
		/// JavaScript converts the string to a regular expression before matching.
		/// </summary>
		[Description("@#matchAll")]
		public extern IEnumerable<RegExpResult> MatchAll(string pattern);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.search</c>.
		/// </summary>
		[Description("@#search")]
		public extern Number Search(RegExp regexp);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.search</c> with a string pattern.
		/// JavaScript converts the string to a regular expression before searching.
		/// </summary>
		[Description("@#search")]
		public extern Number Search(string pattern);

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

		/// <summary>
		/// Returns the primitive string value carried by this host projection.
		/// This mirrors JavaScript <c>String.prototype.valueOf()</c> rather than a CLR conversion helper.
		/// </summary>
		[Description("@#valueOf")]
		public extern string ValueOf();

		/// <summary>
		/// Concatenates the string arguments to the end of the current string.
		/// This stays as a runtime host member because JavaScript exposes it as <c>String.prototype.concat</c>.
		/// </summary>
		[Description("@#concat")]
		public extern string Concat(params string[] strings);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toUpperCase</c>.
		/// </summary>
		[Description("@#toUpperCase")]
		public extern string ToUpperCase();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLowerCase</c>.
		/// </summary>
		[Description("@#toLowerCase")]
		public extern string ToLowerCase();

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleUpperCase</c>.
		/// </summary>
		[Description("@#toLocaleUpperCase")]
		public extern string ToLocaleUpperCase(string? locales = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleUpperCase</c> for locale lists.
		/// </summary>
		[Description("@#toLocaleUpperCase")]
		public extern string ToLocaleUpperCase(string[] locales);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleLowerCase</c>.
		/// </summary>
		[Description("@#toLocaleLowerCase")]
		public extern string ToLocaleLowerCase(string? locales = null);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.toLocaleLowerCase</c> for locale lists.
		/// </summary>
		[Description("@#toLocaleLowerCase")]
		public extern string ToLocaleLowerCase(string[] locales);

		/// <summary>
		/// Projection of JavaScript <c>String.prototype.normalize</c>.
		/// The normalization form is optional because JavaScript defaults it to NFC.
		/// </summary>
		[Description("@#normalize")]
		public extern string Normalize(string? form = null);

		/// <summary>
		/// Returns whether the string is a well-formed sequence of Unicode code points.
		/// This mirrors JavaScript <c>String.prototype.isWellFormed</c>.
		/// </summary>
		[Description("@#isWellFormed")]
		public extern bool IsWellFormed();

		/// <summary>
		/// Returns a copy of the string with lone surrogates replaced so the result is well-formed Unicode.
		/// This mirrors JavaScript <c>String.prototype.toWellFormed</c>.
		/// </summary>
		[Description("@#toWellFormed")]
		public extern string ToWellFormed();

		public extern static bool operator >(string x, string y);

		public extern static bool operator <(string x, string y);
	}
}
