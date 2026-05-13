namespace Playground;

public sealed record PlaygroundExampleSummaryResponse(
    string Id,
    string Title,
    string Category,
    string Difficulty,
    string Runtime,
    string Summary,
    bool Featured,
    int EstimatedMinutes,
    string[] Tags);

public sealed record PlaygroundExampleDetailResponse(
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

public sealed record PlaygroundCatalogResponse(
    PlaygroundExampleSummaryResponse[] Examples,
    string[] Categories);
