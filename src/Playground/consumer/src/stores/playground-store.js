import { computed, markRaw, ref } from "vue";
import { defineStore } from "pinia";
import { getCatalog, getExampleDetail } from "../api-client.js";

const favoritesStorageKey = "playground.favorites.v1";

export const usePlaygroundStore = defineStore("playground", () => {
  const catalogRootComponent = ref(null);
  const detailRootComponent = ref(null);
  const isLoading = ref(false);
  const hasLoadedCatalog = ref(false);
  const catalogResponse = ref(null);
  const detailById = ref(new Map());
  const selectedDetailId = ref("");
  const query = ref("");
  const activeCategory = ref("All");
  const errorMessage = ref("");
  const favorites = ref(readFavoriteSet());

  const examples = computed(() => catalogResponse.value?.Examples ?? []);
  const categories = computed(() => catalogResponse.value?.Categories ?? ["All"]);
  const featuredCount = computed(() => examples.value.filter((item) => item.Featured).length);
  const selectedDetail = computed(() => {
    if (!selectedDetailId.value) {
      return null;
    }

    return detailById.value.get(selectedDetailId.value) ?? null;
  });
  const filteredExamples = computed(() => {
    const normalizedQuery = query.value.trim().toLowerCase();
    return examples.value
      .filter((item) => {
        if (activeCategory.value && activeCategory.value !== "All" && item.Category !== activeCategory.value) {
          return false;
        }

        if (!normalizedQuery) {
          return true;
        }

        const haystack = [
          item.Title,
          item.Category,
          item.Difficulty,
          item.Runtime,
          item.Summary,
          ...(item.Tags ?? [])
        ]
          .join(" ")
          .toLowerCase();
        return haystack.includes(normalizedQuery);
      })
      .map((item) => ({
        ...item,
        IsFavorite: favorites.value.has(item.Id)
      }));
  });
  const favoriteCount = computed(() => favorites.value.size);
  const hasError = computed(() => errorMessage.value.length > 0);

  function setRootComponents(catalogComponent, detailComponent) {
    catalogRootComponent.value = catalogComponent === null ? null : markRaw(catalogComponent);
    detailRootComponent.value = detailComponent === null ? null : markRaw(detailComponent);
  }

  async function ensureCatalogLoaded() {
    if (hasLoadedCatalog.value) {
      return;
    }

    await runWithLoadGuard(async () => {
      catalogResponse.value = await getCatalog();
      hasLoadedCatalog.value = true;
      normalizeActiveCategory();
    });
  }

  async function ensureDetailLoaded(id) {
    selectedDetailId.value = id ?? "";
    if (!selectedDetailId.value) {
      return;
    }

    if (detailById.value.has(selectedDetailId.value)) {
      return;
    }

    await runWithLoadGuard(async () => {
      const detail = await getExampleDetail(selectedDetailId.value);
      detailById.value = new Map(detailById.value).set(selectedDetailId.value, detail);
    });
  }

  function hydrateCatalog(catalog) {
    catalogResponse.value = catalog;
    hasLoadedCatalog.value = true;
    normalizeActiveCategory();
  }

  function setQuery(value) {
    query.value = typeof value === "string" ? value : "";
  }

  function setActiveCategory(value) {
    const next = typeof value === "string" && value.length > 0 ? value : "All";
    activeCategory.value = next;
    normalizeActiveCategory();
  }

  function createCatalogQueryObject() {
    const routeQuery = {};
    if (query.value.trim().length > 0) {
      routeQuery.q = query.value.trim();
    }

    if (activeCategory.value !== "All") {
      routeQuery.category = activeCategory.value;
    }

    return routeQuery;
  }

  function applyCatalogRoute(routeQuery) {
    const nextQuery = typeof routeQuery?.q === "string" ? routeQuery.q : "";
    const nextCategory = typeof routeQuery?.category === "string" ? routeQuery.category : "All";
    query.value = nextQuery;
    activeCategory.value = nextCategory;
    normalizeActiveCategory();
  }

  function toggleFavorite(id) {
    if (typeof id !== "string" || id.length === 0) {
      return;
    }

    const next = new Set(favorites.value);
    if (next.has(id)) {
      next.delete(id);
    } else {
      next.add(id);
    }

    favorites.value = next;
    persistFavoriteSet(next);
  }

  function isFavorite(id) {
    return favorites.value.has(id);
  }

  async function runWithLoadGuard(action) {
    errorMessage.value = "";
    isLoading.value = true;
    try {
      await action();
    } catch (error) {
      errorMessage.value = error instanceof Error ? error.message : String(error);
      throw error;
    } finally {
      isLoading.value = false;
    }
  }

  function normalizeActiveCategory() {
    const categorySet = new Set(categories.value);
    if (!categorySet.has(activeCategory.value)) {
      activeCategory.value = "All";
    }
  }

  return {
    catalogRootComponent,
    detailRootComponent,
    isLoading,
    hasLoadedCatalog,
    query,
    activeCategory,
    errorMessage,
    examples,
    categories,
    featuredCount,
    selectedDetail,
    filteredExamples,
    favoriteCount,
    hasError,
    setRootComponents,
    ensureCatalogLoaded,
    ensureDetailLoaded,
    hydrateCatalog,
    setQuery,
    setActiveCategory,
    createCatalogQueryObject,
    applyCatalogRoute,
    toggleFavorite,
    isFavorite
  };
});

function readFavoriteSet() {
  if (typeof localStorage === "undefined") {
    return new Set();
  }

  try {
    const raw = localStorage.getItem(favoritesStorageKey);
    if (!raw) {
      return new Set();
    }

    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) {
      return new Set();
    }

    return new Set(parsed.filter((item) => typeof item === "string" && item.length > 0));
  } catch {
    return new Set();
  }
}

function persistFavoriteSet(value) {
  if (typeof localStorage === "undefined") {
    return;
  }

  localStorage.setItem(favoritesStorageKey, JSON.stringify([...value].sort((left, right) => left.localeCompare(right, "en"))));
}
