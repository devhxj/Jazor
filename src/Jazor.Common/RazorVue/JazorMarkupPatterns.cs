using System.Text.RegularExpressions;

namespace ECMAScript.Contract.RazorVue;

public static class JazorMarkupPatterns
{
    public static readonly Regex ComponentTagPattern = new(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);
}
