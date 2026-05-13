export function createCatalogViewModel(store) {
  const visibleExamples = store.filteredExamples;
  return {
    Query: store.query,
    ActiveCategory: store.activeCategory,
    TotalExamples: store.examples.length,
    FeaturedCount: store.featuredCount,
    FavoritesCount: store.favoriteCount,
    VisibleCount: visibleExamples.length,
    Categories: store.categories,
    Examples: visibleExamples,
    EmptyStateTitle: store.hasError ? "Catalog unavailable" : "No examples matched",
    EmptyStateBody: store.hasError
      ? store.errorMessage
      : "Adjust the active search text or category filter to broaden the Playground catalog."
  };
}

export function createDetailViewModel(store) {
  const detail = store.selectedDetail;
  const isFavorite = detail ? store.isFavorite(detail.Id) : false;
  return {
    Example: detail,
    IsFavorite: isFavorite,
    BackHref: `/${deriveCatalogQuerySuffix(store)}`,
    BackText: "Back to catalog",
    FavoriteText: isFavorite ? "Remove from saved" : "Save reference"
  };
}

export function deriveCatalogQuerySuffix(store) {
  const query = store.createCatalogQueryObject();
  const search = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    if (typeof value === "string" && value.length > 0) {
      search.set(key, value);
    }
  }

  const serialized = search.toString();
  return serialized.length === 0 ? "" : `?${serialized}`;
}
