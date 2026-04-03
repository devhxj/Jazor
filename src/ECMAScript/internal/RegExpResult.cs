using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#")]
public sealed class RegExpResult : IArray<string>
{
    ///<summary>
    ///Returns the Strings against which a regular expression search was performed. Read-only.
    ///</summary>
    [Description("@#input")]
    public extern string Input { get; }

    ///<summary>
    ///Returns the character position where the first successful match begins in a searched Strings. Read-only.
    ///</summary>
    [Description("@#index")]
    public extern Number Index { get; }

    /// <summary>
    /// Named capture groups returned by <c>RegExp.prototype.exec</c>.
    /// This is exposed as <see cref="IObject"/> because the value is consumed through
    /// JavaScript-style key access rather than a strongly typed CLR dictionary contract.
    /// </summary>
    [Description("@#groups")]
    public extern IObject? Groups { get; }

    public extern string this[Number index] { get; }

    [Description("@#length")]
    public extern Number Length { get; }

    public extern IEnumerator GetEnumerator();
}
