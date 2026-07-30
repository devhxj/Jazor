using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#RegExp")]
/// <summary>
/// JavaScript RegExp 构造器及正则实例 API 的 host binding。
/// </summary>
/// <remarks>
/// 正则语法、flags 和匹配结果遵循 JavaScript RegExp，而不是 System.Text.RegularExpressions；
/// C# 接口只描述可映射的 host surface。
/// </remarks>
public sealed class RegExp : IPattern, IMatchPattern, IMatchAllPattern, ISearchPattern, ISplitPattern
{
	/// <summary>
	/// JavaScript <c>RegExp.prototype</c> object.
	/// Keeping this on the constructor host avoids splitting the runtime host into an extra CLR wrapper.
	/// </summary>
	[Description("@#prototype")]
	public extern static RegExp Prototype { get; }

	/// <summary>
	/// Escapes regular-expression syntax characters in arbitrary text.
	/// This is the direct projection of JavaScript <c>RegExp.escape</c>.
	/// </summary>
	[Description("@#escape")]
	public extern static string Escape(string text);

	public extern RegExp(string pattern);

	public extern RegExp(string pattern, string flags);

	/// <summary>
	/// Recreates a regular expression from an existing JavaScript <see cref="RegExp"/> value.
	/// This overload stays on the constructor host because JavaScript allows <c>new RegExp(existingRegExp)</c>.
	/// </summary>
	public extern RegExp(RegExp pattern);

	/// <summary>
	/// Recreates a regular expression from an existing JavaScript <see cref="RegExp"/> value and replaces its flags.
	/// </summary>
	public extern RegExp(RegExp pattern, string flags);

	/// <summary>
	/// Executes a search on a string using a regular expression pattern, and returns an array containing the results of that search.
	/// </summary>
	/// <param name="s">The String object or string literal on which to perform the search.</param>
	/// <returns></returns>
	[Description("@#exec")]
	public extern RegExpResult? Exec(string s);

	/// <summary>
	/// Returns a Boolean value that indicates whether or not a pattern exists in a searched string.
	/// </summary>
	/// <param name="s">String on which to perform the search.</param>
	/// <returns></returns>
	[Description("@#test")]
	public extern bool Test(string s);

	/// <summary>
	/// Returns a copy of the text of the regular expression pattern. Read-only. The regExp argument is a Regular expression object. It can be a variable name or a literal.
	/// </summary>
	[Description("@#source")]
	public extern string Source { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the global flag (g) used with a regular expression. Default is false. Read-only.
	/// </summary>
	[Description("@#global")]
	public extern bool Global { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the ignoreCase flag (i) used with a regular expression. Default is false. Read-only.
	/// </summary>
	[Description("@#ignoreCase")]
	public extern bool IgnoreCase { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the multiline flag (m) used with a regular expression. Default is false. Read-only.
	/// </summary>
	[Description("@#multiline")]
	public extern bool Multiline { get; }

	/// <summary>
	/// Returns the flags of the regular expression as a string.
	/// </summary>
	[Description("@#flags")]
	public extern string Flags { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the dotAll flag (s) used with a regular expression.
	/// </summary>
	[Description("@#dotAll")]
	public extern bool DotAll { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the sticky flag (y) used with a regular expression.
	/// </summary>
	[Description("@#sticky")]
	public extern bool Sticky { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the unicode flag (u) used with a regular expression.
	/// </summary>
	[Description("@#unicode")]
	public extern bool Unicode { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the unicodeSets flag (v) used with a regular expression.
	/// </summary>
	[Description("@#unicodeSets")]
	public extern bool UnicodeSets { get; }

	/// <summary>
	/// Returns a Boolean value indicating the state of the hasIndices flag (d) used with a regular expression.
	/// </summary>
	[Description("@#hasIndices")]
	public extern bool HasIndices { get; }

	[Description("@#lastIndex")]
	public extern Number LastIndex { get; set; }

	/// <summary>
	/// @deprecated A legacy feature for browser compatibility
	/// </summary>
	/// <param name="pattern"></param>
	/// <param name="flags"></param>
	/// <returns></returns>
	[Description("@#compile")]
	public extern RegExp Compile(string pattern, string? flags);

	/// <summary>
	/// Returns the JavaScript source form of the regular expression, including delimiters and flags.
	/// This is the direct projection of <c>RegExp.prototype.toString()</c>.
	/// </summary>
	[Description("@#toString")]
	public extern override string ToString();

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@replace]</c>.
	/// The replacement argument stays as <see cref="object"/> because JavaScript accepts either a string or a callback function here.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@replace")]
	extern string IPattern.SymbolReplace(string value, object? replacement);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@match]</c>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@match")]
	extern Array<string?>? IMatchPattern.SymbolMatch(string value);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@matchAll]</c>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@matchAll")]
	extern IEnumerable<RegExpResult> IMatchAllPattern.SymbolMatchAll(string value);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@search]</c>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@search")]
	extern Number ISearchPattern.SymbolSearch(string value);

	/// <summary>
	/// Hidden protocol bridge for JavaScript <c>RegExp.prototype[@@split]</c>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Description("@#@@split")]
	extern Array<string> ISplitPattern.SymbolSplit(string value, Number? limit);
}
