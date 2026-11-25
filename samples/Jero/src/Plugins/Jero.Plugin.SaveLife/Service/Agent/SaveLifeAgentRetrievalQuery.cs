using System.Text.RegularExpressions;

namespace Jero.Plugin.SaveLife.Service;

public sealed class SaveLifeAgentRetrievalQuery
{
    private SaveLifeAgentRetrievalQuery(
        string originalQuery,
        string? diseaseKeyword,
        string? city,
        List<string> keywords)
    {
        OriginalQuery = originalQuery;
        DiseaseKeyword = diseaseKeyword;
        City = city;
        Keywords = keywords;
        SearchTerms = keywords;
    }

    public string OriginalQuery { get; }

    public string? DiseaseKeyword { get; }

    public string? City { get; }

    public IReadOnlyList<string> Keywords { get; }

    public IReadOnlyList<string> SearchTerms { get; }

    public bool HasProjectDiscoveryCriteria =>
        SearchTerms.Count > 0 || !string.IsNullOrWhiteSpace(OriginalQuery);

    public static SaveLifeAgentRetrievalQuery Build(
        string? query,
        SaveLifeAgentPatientSignal? signal)
    {
        var keywords = new List<string>();
        var disease = NormalizeToken(signal?.DiseaseKeyword);
        var city = NormalizeToken(signal?.City);
        AddKeyword(keywords, disease);
        AddKeyword(keywords, city);
        foreach (var term in signal?.SearchTerms ?? [])
            AddKeyword(keywords, term);

        return new SaveLifeAgentRetrievalQuery(
            NormalizeText(query),
            disease,
            city,
            keywords);
    }

    public bool MatchesAny(params string?[] values)
    {
        var text = NormalizeText(string.Join("\n", values.Where(value => !string.IsNullOrWhiteSpace(value))));
        if (string.IsNullOrWhiteSpace(text))
            return false;

        return Keywords.Count == 0
            ? !string.IsNullOrWhiteSpace(OriginalQuery) &&
              text.Contains(OriginalQuery, StringComparison.OrdinalIgnoreCase)
            : Keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public double ScoreText(params string?[] values)
    {
        var text = NormalizeText(string.Join("\n", values.Where(value => !string.IsNullOrWhiteSpace(value))));
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        if (Keywords.Count == 0)
        {
            return !string.IsNullOrWhiteSpace(OriginalQuery) &&
                   text.Contains(OriginalQuery, StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;
        }

        var matchedWeight = 0d;
        var totalWeight = 0d;
        foreach (var keyword in Keywords)
        {
            var weight = string.Equals(keyword, DiseaseKeyword, StringComparison.OrdinalIgnoreCase)
                ? 1.5
                : string.Equals(keyword, City, StringComparison.OrdinalIgnoreCase)
                    ? 1.2
                    : 1.3;
            totalWeight += weight;
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                matchedWeight += weight;
        }

        return totalWeight <= 0 ? 0 : Math.Clamp(matchedWeight / totalWeight, 0, 1);
    }

    public string BuildEmbeddingText()
    {
        var parts = new List<string>();
        AddEmbeddingPart(parts, OriginalQuery);
        foreach (var keyword in Keywords)
            AddEmbeddingPart(parts, keyword);

        return string.Join("\n", parts);
    }

    private static void AddKeyword(List<string> keywords, string? keyword)
    {
        var normalized = NormalizeToken(keyword);
        if (string.IsNullOrWhiteSpace(normalized) ||
            keywords.Any(value => string.Equals(value, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        keywords.Add(normalized);
    }

    private static void AddEmbeddingPart(List<string> parts, string? value)
    {
        var normalized = NormalizeText(value);
        if (string.IsNullOrWhiteSpace(normalized) ||
            parts.Any(part => part.Contains(normalized, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        parts.Add(normalized);
    }

    private static string NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : Regex.Replace(value.Trim(), @"\s+", " ");
    }

    private static string? NormalizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = Regex.Replace(
            value.Trim(),
            @"[\s,，。.!！?？;；:：""'“”‘’（）()\[\]【】{}<>《》]+",
            string.Empty);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
