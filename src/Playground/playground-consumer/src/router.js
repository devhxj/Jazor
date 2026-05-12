import { defineComponent, h, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { usePlaygroundStore } from "./stores/playground-store.js";
import { createCatalogViewModel, createDetailViewModel, deriveCatalogQuerySuffix } from "./view-models.js";

export function createPlaygroundRoutes() {
  return [
    {
      path: "/",
      name: "catalog",
      component: defineCatalogRouteComponent()
    },
    {
      path: "/examples/:id",
      name: "detail",
      component: defineDetailRouteComponent()
    }
  ];
}

function defineCatalogRouteComponent() {
  return defineComponent({
    name: "PlaygroundCatalogRoute",
    setup() {
      const route = useRoute();
      const router = useRouter();
      const store = usePlaygroundStore();

      onMounted(async () => {
        store.applyCatalogRoute(route.query);
        try {
          await store.ensureCatalogLoaded();
        } catch {
        }
      });

      watch(
        () => route.query,
        (query) => {
          store.applyCatalogRoute(query);
        },
        { deep: true }
      );

      return () => {
        const RootPage = store.catalogRootComponent;
        if (!RootPage) {
          return h("div", { class: "playground-route-state" }, "Catalog component is not available.");
        }

        return h(RootPage, {
          model: createCatalogViewModel(store),
          detailQuerySuffix: deriveCatalogQuerySuffix(store),
          onQueryChanged: (value) => {
            const query = typeof value === "string" ? value : "";
            store.setQuery(query);
            router.replace({ name: "catalog", query: store.createCatalogQueryObject() });
          },
          onCategorySelected: (value) => {
            store.setActiveCategory(typeof value === "string" ? value : "");
            router.replace({ name: "catalog", query: store.createCatalogQueryObject() });
          }
        });
      };
    }
  });
}

function defineDetailRouteComponent() {
  return defineComponent({
    name: "PlaygroundDetailRoute",
    setup() {
      const route = useRoute();
      const store = usePlaygroundStore();

      async function ensureLoaded(id) {
        if (typeof id !== "string" || id.length === 0) {
          return;
        }

        store.applyCatalogRoute(route.query);
        try {
          await store.ensureCatalogLoaded();
          await store.ensureDetailLoaded(id);
        } catch {
        }
      }

      onMounted(async () => {
        await ensureLoaded(route.params.id);
      });

      watch(
        () => route.params.id,
        async (id) => {
          await ensureLoaded(id);
        },
        { immediate: false }
      );

      return () => {
        if (!store.detailRootComponent) {
          return h("div", { class: "playground-route-state" }, "Detail component is not available.");
        }

        if (store.selectedDetail === null) {
          return h("div", { class: "playground-route-state" }, store.errorMessage || "Loading example...");
        }

        return h(store.detailRootComponent, {
          model: createDetailViewModel(store),
          onToggleFavorite: () => {
            store.toggleFavorite(store.selectedDetail.Id);
          }
        });
      };
    }
  });
}
