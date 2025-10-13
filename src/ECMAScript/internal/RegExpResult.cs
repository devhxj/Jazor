using System.ComponentModel;

namespace ECMAScript;

[ECMAScript]
[Description("@#Exec")]
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

    ///<summary>
    ///Returns the character position where the next match begins in a searched Strings.
    ///</summary>
    [Description("@#lastIndex")]
    public extern Number LastIndex { get; }

    ///<summary>
    ///Returns the last matched characters from any regular expression search. Read-only.
    ///</summary>
    [Description("@#lastMatch")]
    public extern string LastMatch { get; }

    ///<summary>
    ///Returns the last parenthesized submatch from any regular expression search, if any. Read-only.
    ///</summary>
    [Description("@#lastParen")]
    public extern string LastParen { get; }

    ///<summary>
    ///Returns the characters from the beginning of a searched Strings up to the position before the beginning of the last match. Read-only.
    ///</summary>
    [Description("@#leftContext")]
    public extern string LeftContext { get; }

    ///<summary>
    ///Returns the characters from the position following the last match to the end of the searched Strings. Read-only.
    ///</summary>
    [Description("@#rightContext")]
    public extern string RightContext { get; }

    [Description("@#groups")]
    public extern IObject<string>? Groups { get; }

    public extern string this[uint index] { get; }

    public extern uint Length { get; }

    public extern IEnumerator GetEnumerator();
}