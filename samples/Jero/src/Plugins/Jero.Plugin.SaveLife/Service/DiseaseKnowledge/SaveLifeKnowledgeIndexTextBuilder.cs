namespace Jero.Plugin.SaveLife.Service;

public static class SaveLifeKnowledgeIndexTextBuilder
{
    public static string Build(SaveLifeDiseaseKnowledge knowledge)
    {
        return string.Join("\n", new[]
        {
            knowledge.Title,
            knowledge.DiseaseName,
            knowledge.Author,
            knowledge.Summary,
            SaveLifeKnowledgeContentSanitizer.ExtractPlainText(knowledge.Content),
            knowledge.Tags
        }.Where(u => !string.IsNullOrWhiteSpace(u)));
    }
}
