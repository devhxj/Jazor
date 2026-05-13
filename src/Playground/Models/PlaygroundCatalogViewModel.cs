using System.Collections.Generic;

namespace Playground.Models;

public sealed record PlaygroundCatalogViewModel(
    string Query,
    string ActiveCategory,
    int TotalExamples,
    int FeaturedCount,
    int FavoritesCount,
    int VisibleCount,
    IReadOnlyList<string> Categories,
    IReadOnlyList<PlaygroundExampleSummary> Examples,
    string EmptyStateTitle,
    string EmptyStateBody);
