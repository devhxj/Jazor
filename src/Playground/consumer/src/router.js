import { defineComponent, h, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { usePlaygroundStore } from "./stores/playground-store.js";
import { createCatalogViewModel, createDetailViewModel } from "./view-models.js";

export function createPlaygroundRoutes(routeDefinitions) {
  const catalogRoutes = resolveRoutes(routeDefinitions, "CatalogPage");
  const detailRoute = resolveDetailRoute(routeDefinitions);
  const primaryCatalogRoute = resolvePrimaryCatalogRoute(catalogRoutes);
  const catalogComponent = defineCatalogRouteComponent(primaryCatalogRoute, detailRoute);
  const detailComponent = defineDetailRouteComponent(primaryCatalogRoute, detailRoute);

  return [
    ...catalogRoutes.map((route) => ({
      path: route.path,
      name: route.name,
      component: catalogComponent
    })),
    {
      path: detailRoute.path,
      name: detailRoute.name,
      component: detailComponent
    }
  ];
}

function defineCatalogRouteComponent(primaryCatalogRoute, detailRoute) {
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
          return h("div", { class: "playground-route-state" }, "目录页面组件尚未加载。");
        }

        return h(RootPage, {
          model: createCatalogViewModel(store, detailRoute),
          onQueryChanged: (value) => {
            const query = typeof value === "string" ? value : "";
            store.setQuery(query);
            router.replace({
              name: route.name || primaryCatalogRoute.name,
              query: store.createCatalogQueryObject()
            });
          },
          onCategorySelected: (value) => {
            store.setActiveCategory(typeof value === "string" ? value : "");
            router.replace({
              name: route.name || primaryCatalogRoute.name,
              query: store.createCatalogQueryObject()
            });
          }
        });
      };
    }
  });
}

function defineDetailRouteComponent(catalogRoute, detailRoute) {
  return defineComponent({
    name: "PlaygroundDetailRoute",
    setup() {
      const route = useRoute();
      const store = usePlaygroundStore();

      async function ensureLoaded(routeParameters) {
        const detailId = resolveSingleRouteParameter(routeParameters, detailRoute);
        if (typeof detailId !== "string" || detailId.length === 0) {
          return;
        }

        store.applyCatalogRoute(route.query);
        try {
          await store.ensureCatalogLoaded();
          await store.ensureDetailLoaded(detailId);
        } catch {
        }
      }

      onMounted(async () => {
        await ensureLoaded(route.params);
      });

      watch(
        () => detailRoute.parameterNames.map((name) => route.params[name]),
        async () => {
          await ensureLoaded(route.params);
        },
        { immediate: false }
      );

      return () => {
        if (!store.detailRootComponent) {
          return h("div", { class: "playground-route-state" }, "详情页面组件尚未加载。");
        }

        if (store.selectedDetail === null) {
          return h("div", { class: "playground-route-state" }, store.errorMessage || "正在加载示例详情...");
        }

        return h(store.detailRootComponent, {
          model: createDetailViewModel(store, catalogRoute),
          onToggleFavorite: () => {
            store.toggleFavorite(store.selectedDetail.Id);
          }
        });
      };
    }
  });
}

function resolveSingleRouteParameter(routeParameters, routeDefinition) {
  const parameterName = routeDefinition.parameterNames[0];
  if (typeof parameterName !== "string" || parameterName.length === 0) {
    throw new Error(`Playground route '${routeDefinition.alias}' does not declare a supported route parameter.`);
  }

  const value = routeParameters?.[parameterName];
  if (Array.isArray(value)) {
    return typeof value[0] === "string" ? value[0] : "";
  }

  return typeof value === "string" ? value : "";
}

function resolveRoutes(routeDefinitions, alias) {
  const matches = routeDefinitions.filter((item) => item.alias === alias);
  if (matches.length === 0) {
    throw new Error(`Playground expected at least one route definition for alias '${alias}', but found none.`);
  }

  return matches;
}

function resolvePrimaryCatalogRoute(catalogRoutes) {
  const rootRoute = catalogRoutes.find((route) => route.path === "/");
  return rootRoute ?? catalogRoutes[0];
}

function resolveDetailRoute(routeDefinitions) {
  const matches = resolveRoutes(routeDefinitions, "DetailPage");
  if (matches.length !== 1) {
    throw new Error(`Playground expected exactly one detail route definition, but found ${matches.length}.`);
  }

  const route = matches[0];
  if (route.parameterNames.length !== 1) {
    throw new Error("Playground detail route must declare exactly one route parameter.");
  }

  return route;
}
