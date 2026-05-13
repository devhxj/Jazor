namespace Playground.Models;

public sealed record PlaygroundExampleSummary(
    string Id,
    string Title,
    string Category,
    string Difficulty,
    string Runtime,
    string Summary,
    bool Featured,
    int EstimatedMinutes,
    string[] Tags,
    bool IsFavorite);
