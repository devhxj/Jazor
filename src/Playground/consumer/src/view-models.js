import { createRouterMatcher } from "vue-router";
import { applyRouteDefaultParameterValues, doRouteParametersSatisfyConstraints } from "./runtime-common.js";

export function createCatalogViewModel(store, detailRouteDefinition) {
  const visibleExamples = store.filteredExamples;
  const detailQuerySuffix = deriveCatalogQuerySuffix(store);
  return {
    query: store.query,
    activeCategory: store.activeCategory,
    totalExamples: store.examples.length,
    featuredCount: store.featuredCount,
    favoritesCount: store.favoriteCount,
    visibleCount: visibleExamples.length,
    categories: store.categories,
    examples: visibleExamples.map((example) => ({
      id: example.Id,
      title: example.Title,
      category: example.Category,
      difficulty: example.Difficulty,
      runtime: example.Runtime,
      summary: example.Summary,
      featured: example.Featured,
      estimatedMinutes: example.EstimatedMinutes,
      tags: example.Tags ?? [],
      isFavorite: Boolean(example.IsFavorite),
      detailHref: `${resolveRouteHrefForSingleParameter(detailRouteDefinition, example.Id)}${detailQuerySuffix}`
    })),
    emptyStateTitle: store.hasError ? "目录暂不可用" : "当前没有匹配项",
    emptyStateBody: store.hasError
      ? store.errorMessage
      : "可以调整搜索关键词或分类筛选，扩大当前 Playground 目录范围。"
  };
}

export function createDetailViewModel(store, catalogRouteDefinition) {
  const detail = store.selectedDetail;
  const isFavorite = detail ? store.isFavorite(detail.Id) : false;
  return {
    example: detail === null
      ? null
      : {
        id: detail.Id,
        title: detail.Title,
        category: detail.Category,
        difficulty: detail.Difficulty,
        runtime: detail.Runtime,
        summary: detail.Summary,
        whyItMatters: detail.WhyItMatters,
        highlights: detail.Highlights ?? [],
        steps: detail.Steps ?? [],
        files: detail.Files ?? [],
        estimatedMinutes: detail.EstimatedMinutes,
        updatedAtUtc: detail.UpdatedAtUtc,
        featured: detail.Featured,
        tags: detail.Tags ?? []
      },
    isFavorite,
    backHref: deriveCatalogHref(catalogRouteDefinition, store),
    backText: "返回目录",
    favoriteText: isFavorite ? "取消收藏" : "加入收藏"
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
  const resolvedPath = resolveRouteHrefForSingleParameter(detailRouteDefinition, exampleId);
  return `${resolvedPath}${deriveCatalogQuerySuffix(store)}`;
}

export function resolveRouteHrefForSingleParameter(routeDefinition, value) {
  const parameterName = routeDefinition?.parameterNames?.[0];
  if (typeof parameterName !== "string" || parameterName.length === 0) {
    throw new Error("Playground route definition must declare at least one route parameter.");
  }

  return resolveRouteHref(routeDefinition, {
    [parameterName]: value
  });
}

export function resolveRouteHref(routeDefinition, routeParameters = {}) {
  if (routeDefinition === null || typeof routeDefinition !== "object") {
    throw new Error("Playground route definition is required.");
  }

  if (typeof routeDefinition.path !== "string" || !routeDefinition.path.startsWith("/")) {
    throw new Error("Playground route definition path must be an absolute route path.");
  }

  const matcher = getRouteHrefMatcher(routeDefinition);
  const normalizedParameters = normalizeRouteHrefParameters(routeDefinition, routeParameters);
  if (!doRouteParametersSatisfyConstraints(routeDefinition, normalizedParameters)) {
    throw new Error(`Playground route '${routeDefinition.name ?? ""}' parameters do not satisfy generated route constraints.`);
  }

  const resolved = matcher.resolve(
    {
      name: routeDefinition.name,
      params: normalizedParameters
    },
    createMatcherCurrentLocation()
  );
  return resolved.path || "/";
}

function normalizeRouteHrefParameters(routeDefinition, routeParameters) {
  const parametersWithDefaults = applyRouteDefaultParameterValues(routeDefinition, routeParameters);
  const normalized = { ...parametersWithDefaults };
  const defaultValues = routeDefinition?.defaultParameterValues;
  const elidableNames = routeDefinition?.elidableDefaultParameterNames;
  if (
    defaultValues === null ||
    typeof defaultValues !== "object" ||
    !Array.isArray(elidableNames)
  ) {
    return normalized;
  }

  for (const key of elidableNames) {
    const defaultValue = defaultValues[key];
    if (normalized[key] === defaultValue) {
      delete normalized[key];
    }
  }

  return normalized;
}

const routeHrefMatcherCache = new WeakMap();

function getRouteHrefMatcher(routeDefinition) {
  let matcher = routeHrefMatcherCache.get(routeDefinition);
  if (matcher) {
    return matcher;
  }

  matcher = createRouterMatcher(
    [
      {
        path: routeDefinition.path,
        name: routeDefinition.name,
        component: {}
      }
    ],
    {}
  );
  routeHrefMatcherCache.set(routeDefinition, matcher);
  return matcher;
}

function createMatcherCurrentLocation() {
  return {
    path: "/",
    fullPath: "/",
    params: {},
    query: {},
    hash: "",
    matched: [],
    meta: {}
  };
}
