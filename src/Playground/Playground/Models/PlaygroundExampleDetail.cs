namespace Playground.Models;

public sealed record PlaygroundExampleDetail(
    string Id,
    string Title,
    string Category,
    string Difficulty,
    string Runtime,
    string Summary,
    string WhyItMatters,
    string[] Tags,
    string[] Highlights,
    string[] Steps,
    string[] Files,
    string UpdatedAtUtc,
    bool Featured,
    int EstimatedMinutes);
