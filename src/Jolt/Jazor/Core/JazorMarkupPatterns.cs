using System.Text.RegularExpressions;

namespace Jazor.Vue;

internal static class JazorMarkupPatterns
{
    internal static readonly Regex ComponentTagPattern = new(
        @"<(?<name>[A-Z][A-Za-z0-9_]*)\b",
        RegexOptions.Compiled);
}
