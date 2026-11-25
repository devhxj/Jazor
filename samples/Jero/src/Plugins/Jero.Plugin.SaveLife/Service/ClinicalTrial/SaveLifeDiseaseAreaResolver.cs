namespace Jero.Plugin.SaveLife;

public sealed class SaveLifeDiseaseAreaResolution
{
    public string? DiseaseArea { get; init; }

    public bool HasConflict { get; init; }

    public bool IsAmbiguous { get; init; }

    public bool IsHighConfidence { get; init; }

    public string? RuleName { get; init; }
}

public static class SaveLifeDiseaseAreaResolver
{
    private const string CirculatoryRuleName = "circulatory";
    private const string MetabolicRuleName = "metabolic";
    private const string DyslipidemiaKeyword = "血脂异常";
    private const string CardiovascularRiskFactorMarker = "心血管危险因素";

    private static readonly string[] NegativeEvidenceMarkers =
    [
        "尚未证明", "未证明", "尚未证实", "未证实", "无证据证明", "不能证明"
    ];

    private static readonly char[] ClauseSeparators =
    [
        '。', '；', ';', '，', ',', '\r', '\n', '！', '!', '？', '?'
    ];

    private static readonly DiseaseRule[] Rules =
    [
        new("circulatory", "循环系统疾病",
        [
            "冠心病", "冠状动脉", "急性冠脉", "心肌梗死", "心绞痛"
        ]),
        new("metabolic", "代谢系统与营养疾病",
        [
            "高胆固醇血症", "高脂血症", "血脂异常", "匹伐他汀"
        ]),
        new("reproductive", "生殖系统疾病",
        [
            "阴道炎", "阴道病", "外阴阴道"
        ]),
    ];

    public static SaveLifeDiseaseAreaResolution Resolve(
        string? sourceDiseaseArea,
        string? title,
        string? indication)
    {
        var source = Normalize(sourceDiseaseArea);
        var indicationMatches = MatchRules(indication);
        if (indicationMatches.Count > 1)
        {
            return new SaveLifeDiseaseAreaResolution
            {
                DiseaseArea = source,
                IsAmbiguous = true
            };
        }

        var match = indicationMatches.Count == 1
            ? indicationMatches[0]
            : ResolveEvidence(title);
        if (match == null)
        {
            return new SaveLifeDiseaseAreaResolution
            {
                DiseaseArea = source,
                IsAmbiguous = HasAmbiguousEvidence(title)
            };
        }

        return new SaveLifeDiseaseAreaResolution
        {
            DiseaseArea = match.DiseaseArea,
            HasConflict = source != null && !source.Equals(match.DiseaseArea, StringComparison.Ordinal),
            IsHighConfidence = true,
            RuleName = match.Name
        };
    }

    private static DiseaseRule? ResolveEvidence(string? value)
    {
        var matches = MatchRules(value);
        return matches.Count == 1 ? matches[0] : null;
    }

    private static bool HasAmbiguousEvidence(string? value)
    {
        return MatchRules(value).Count > 1;
    }

    private static List<DiseaseRule> MatchRules(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        return Rules
            .Where(rule => rule.Keywords.Any(keyword =>
                ContainsClassificationEvidence(value, rule.Name, keyword)))
            .ToList();
    }

    private static bool ContainsClassificationEvidence(string value, string ruleName, string keyword)
    {
        var searchStart = 0;
        while (searchStart < value.Length)
        {
            var matchIndex = value.IndexOf(keyword, searchStart, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
                return false;

            var isRiskFactorListItem = ruleName.Equals(MetabolicRuleName, StringComparison.Ordinal)
                && keyword.Equals(DyslipidemiaKeyword, StringComparison.Ordinal)
                && IsInsideCardiovascularRiskFactorList(value, matchIndex);
            var isNegativeOutcome = ruleName.Equals(CirculatoryRuleName, StringComparison.Ordinal)
                && IsInsideNegativeEvidenceClause(value, matchIndex);
            if (!isRiskFactorListItem && !isNegativeOutcome)
                return true;

            searchStart = matchIndex + keyword.Length;
        }

        return false;
    }

    private static bool IsInsideCardiovascularRiskFactorList(string value, int matchIndex)
    {
        var markerIndex = value.LastIndexOf(
            CardiovascularRiskFactorMarker,
            matchIndex,
            StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
            return false;

        var fullWidthOpenIndex = value.IndexOf('（', markerIndex);
        var asciiOpenIndex = value.IndexOf('(', markerIndex);
        var openIndex = FirstNonNegative(fullWidthOpenIndex, asciiOpenIndex);
        if (openIndex < 0 || openIndex >= matchIndex)
            return false;

        var fullWidthCloseIndex = value.IndexOf('）', openIndex + 1);
        var asciiCloseIndex = value.IndexOf(')', openIndex + 1);
        var closeIndex = FirstNonNegative(fullWidthCloseIndex, asciiCloseIndex);
        return closeIndex >= matchIndex;
    }

    private static bool IsInsideNegativeEvidenceClause(string value, int matchIndex)
    {
        var previousSeparatorIndex = value.LastIndexOfAny(ClauseSeparators, matchIndex);
        var clauseStart = previousSeparatorIndex < 0 ? 0 : previousSeparatorIndex + 1;
        var prefix = value[clauseStart..matchIndex];
        return NegativeEvidenceMarkers.Any(marker =>
            prefix.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static int FirstNonNegative(int first, int second)
    {
        if (first < 0)
            return second;
        if (second < 0)
            return first;
        return Math.Min(first, second);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record DiseaseRule(string Name, string DiseaseArea, string[] Keywords);
}
