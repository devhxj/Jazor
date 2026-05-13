export function createCatalogViewModel(store, detailRouteDefinition) {
  const visibleExamples = store.filteredExamples;
  const detailQuerySuffix = deriveCatalogQuerySuffix(store);
  return {
    Query: store.query,
    ActiveCategory: store.activeCategory,
    TotalExamples: store.examples.length,
    FeaturedCount: store.featuredCount,
    FavoritesCount: store.favoriteCount,
    VisibleCount: visibleExamples.length,
    Categories: store.categories,
    Examples: visibleExamples.map((example) => ({
      ...example,
      DetailHref: `${resolveRouteHref(detailRouteDefinition, { id: example.Id })}${detailQuerySuffix}`
    })),
    EmptyStateTitle: store.hasError ? "目录暂不可用" : "当前没有匹配项",
    EmptyStateBody: store.hasError
      ? store.errorMessage
      : "可以调整搜索关键词或分类筛选，扩大当前 Playground 目录范围。"
  };
}

export function createDetailViewModel(store, catalogRouteDefinition) {
  const detail = store.selectedDetail;
  const isFavorite = detail ? store.isFavorite(detail.Id) : false;
  return {
    Example: detail,
    IsFavorite: isFavorite,
    BackHref: deriveCatalogHref(catalogRouteDefinition, store),
    BackText: "返回目录",
    FavoriteText: isFavorite ? "取消收藏" : "加入收藏"
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

export function deriveCatalogHref(catalogRouteDefinition, store) {
  return `${catalogRouteDefinition.path}${deriveCatalogQuerySuffix(store)}`;
}

export function deriveDetailHref(detailRouteDefinition, exampleId, store) {
  const resolvedPath = resolveRouteHref(detailRouteDefinition, {
    id: exampleId
  });
  return `${resolvedPath}${deriveCatalogQuerySuffix(store)}`;
}

export function resolveRouteHref(routeDefinition, routeParameters = {}) {
  if (routeDefinition === null || typeof routeDefinition !== "object") {
    throw new Error("Playground route definition is required.");
  }

  if (typeof routeDefinition.path !== "string" || !routeDefinition.path.startsWith("/")) {
    throw new Error("Playground route definition path must be an absolute route path.");
  }

  const parameters = routeParameters ?? {};
  const segments = routeDefinition.path.split("/");
  const resolvedSegments = segments.map((segment) => resolveRouteSegment(segment, parameters, routeDefinition.alias));
  const resolvedPath = resolvedSegments.join("/");
  return resolvedPath.length === 0 ? "/" : resolvedPath;
}

function resolveRouteSegment(segment, parameters, alias) {
  if (!segment.startsWith(":")) {
    return segment;
  }

  const isOptional = segment.endsWith("?");
  const parameterName = segment.slice(1, isOptional ? -1 : undefined);
  if (parameterName.length === 0) {
    throw new Error(`Playground route '${alias}' contains an invalid parameter segment '${segment}'.`);
  }

  const value = parameters[parameterName];
  if (typeof value === "string" && value.length > 0) {
    return encodeURIComponent(value);
  }

  if (isOptional) {
    return "";
  }

  throw new Error(`Playground route '${alias}' requires route parameter '${parameterName}'.`);
}
