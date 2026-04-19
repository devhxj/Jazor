using System.Text.RegularExpressions;

namespace Jazor.Vue;

public static class LegacyImportDirectiveCatalog
{
    public const string DiagnosticCode = "JAZORVUE020";
    public const string DiagnosticSource = "Jazor.VueHost";

    private static readonly Regex LegacyImportDirectivePattern = new(
        @"^\s*@(?<kind>import|jsimport|vueimport)\b",
        RegexOptions.Multiline | RegexOptions.Compiled);

    public static IReadOnlyList<LegacyImportDirectiveOccurrence> FindOccurrences(string sourceText)
    {
        if (string.IsNullOrEmpty(sourceText))
        {
            return Array.Empty<LegacyImportDirectiveOccurrence>();
        }

        var occurrences = new List<LegacyImportDirectiveOccurrence>();
        foreach (Match match in LegacyImportDirectivePattern.Matches(sourceText))
        {
            var kindGroup = match.Groups["kind"];
            if (!kindGroup.Success)
            {
                continue;
            }

            var directiveStart = Math.Max(0, kindGroup.Index - 1);
            var directiveLength = kindGroup.Length + 1;
            occurrences.Add(new LegacyImportDirectiveOccurrence(
                kindGroup.Value,
                directiveStart,
                directiveLength));
        }

        return occurrences;
    }

    public static string CreateDiagnosticMessage(string legacyKind)
        => $"@{legacyKind} is unsupported. Use @module instead.";
}

public readonly record struct LegacyImportDirectiveOccurrence(
    string Kind,
    int Start,
    int Length);
