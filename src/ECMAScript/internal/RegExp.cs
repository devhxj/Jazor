using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#RegExp")]
public sealed class RegExp
{
	public extern RegExp(string pattern);

	public extern RegExp(string pattern, string flags);

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

	[Description("@#lastIndex")]
	public extern Number LastIndex { get; }

	/// <summary>
	/// @deprecated A legacy feature for browser compatibility
	/// </summary>
	/// <param name="pattern"></param>
	/// <param name="flags"></param>
	/// <returns></returns>
	[Description("@#compile")]
	public extern RegExp Compile(string pattern, string? flags);
}
