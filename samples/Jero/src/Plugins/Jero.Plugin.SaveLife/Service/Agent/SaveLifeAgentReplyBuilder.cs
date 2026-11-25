using System.Text.RegularExpressions;

namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeAgentReplyBuilder
{
    public static SaveLifeAgentPatientSignal ExtractSignal(IEnumerable<string> messages)
    {
        var text = string.Join(" ", messages.Where(u => !string.IsNullOrWhiteSpace(u)));
        return new SaveLifeAgentPatientSignal
        {
            Phone = ExtractPhone(text)
        };
    }

    public static string BuildEnrollmentNotes(
        string intention,
        IReadOnlyList<SaveLifeAgentMessageOutput> messages)
    {
        var transcript = string.Join("\n", messages
            .OrderBy(u => u.CreatedAt)
            .Select(u => $"{u.Role}: {u.Content}"));

        return $"小程序助手意向：{intention}\n\n咨询记录：\n{transcript}";
    }

    public static string MapIntentionStatus(string intention)
    {
        return intention?.Trim() switch
        {
            "明确有意向" => "pending",
            "犹豫考虑" => "draft",
            "暂不参与" => "closed",
            _ => throw new ArgumentOutOfRangeException(nameof(intention), "未知的结构化参与意向")
        };
    }

    private static string? ExtractPhone(string text)
    {
        var match = Regex.Match(text, @"(?<!\d)1[3-9]\d{9}(?!\d)");
        return match.Success ? match.Value : null;
    }

}
public sealed class SaveLifeAgentPatientSignal
{
    public SaveLifePatientProfileOutput? CurrentPatient { get; set; }

    public string? DiseaseKeyword { get; set; }

    public string? City { get; set; }

    public string? Phone { get; set; }

    public IReadOnlyList<string> SearchTerms { get; set; } = [];

    public IReadOnlyList<string> KnowledgeSearchTerms { get; set; } = [];

    public IReadOnlyList<string> TrialSearchTerms { get; set; } = [];

    public IReadOnlyList<string> MaterialSearchTerms { get; set; } = [];

    public bool UseSourceSearchTerms { get; set; }
}
