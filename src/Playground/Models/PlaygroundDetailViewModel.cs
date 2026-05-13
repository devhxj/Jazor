namespace Playground.Models;

public sealed record PlaygroundDetailViewModel(
    PlaygroundExampleDetail Example,
    bool IsFavorite,
    string BackHref,
    string BackText,
    string FavoriteText);
