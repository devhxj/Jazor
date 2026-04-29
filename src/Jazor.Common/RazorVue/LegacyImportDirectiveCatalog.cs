namespace Jazor.RazorVue;

public static class LegacyImportDirectiveCatalog
{
    public const string DiagnosticCode = "JAZORVUE020";
    public const string DiagnosticSource = "Jolt";

    public static IReadOnlyList<LegacyImportDirectiveOccurrence> FindOccurrences(string sourceText)
    {
        if (string.IsNullOrEmpty(sourceText))
        {
            return Array.Empty<LegacyImportDirectiveOccurrence>();
        }

        var occurrences = new List<LegacyImportDirectiveOccurrence>();
        foreach (var match in JazorImportDirectiveLocator.EnumerateLegacyDirectives(sourceText))
        {
            occurrences.Add(new LegacyImportDirectiveOccurrence(
                match.LegacyKind,
                match.DirectiveIndex,
                match.DirectiveLength));
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
