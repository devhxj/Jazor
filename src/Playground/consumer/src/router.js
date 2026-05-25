import { defineComponent, h, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { usePlaygroundStore } from "./stores/playground-store.js";
import { createCatalogViewModel, createDetailViewModel } from "./view-models.js";
import { applyRouteDefaultParameterValues, doRouteParametersSatisfyConstraints } from "./runtime-common.js";

export function createPlaygroundRoutes(routeDefinitions) {
  const catalogRoutes = resolveRoutes(routeDefinitions, "CatalogPage");
  const detailRoutes = resolveDetailRoutes(routeDefinitions);
  const primaryCatalogRoute = resolvePrimaryCatalogRoute(catalogRoutes);
  const primaryDetailRoute = resolvePrimaryDetailRoute(detailRoutes);
  const catalogComponent = defineCatalogRouteComponent(primaryCatalogRoute, primaryDetailRoute);
  const detailComponent = defineDetailRouteComponent(primaryCatalogRoute);

  return [
    ...catalogRoutes.map((route) => ({
      path: route.path,
      name: route.name,
      beforeEnter: createRouteConstraintGuard(route),
      component: catalogComponent
    })),
    ...detailRoutes.map((route) => ({
      path: route.path,
      name: route.name,
      beforeEnter: createRouteConstraintGuard(route),
      meta: {
        routeDefinition: route
      },
      component: detailComponent
    }))
  ];
}

function createRouteConstraintGuard(routeDefinition) {
  return (to) => doRouteParametersSatisfyConstraints(routeDefinition, to.params);
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

function defineDetailRouteComponent(catalogRoute) {
  return defineComponent({
    name: "PlaygroundDetailRoute",
    setup() {
      const route = useRoute();
      const store = usePlaygroundStore();

      async function ensureLoaded(routeParameters, routeDefinition) {
        const detailId = resolveSingleRouteParameter(routeParameters, routeDefinition);
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
        await ensureLoaded(route.params, route.meta.routeDefinition);
      });

      watch(
        () => [
          route.meta.routeDefinition,
          ...((route.meta.routeDefinition?.parameterNames ?? []).map((name) => route.params[name]))
        ],
        async () => {
          await ensureLoaded(route.params, route.meta.routeDefinition);
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
  const parameterName = routeDefinition?.parameterNames?.[0];
  if (typeof parameterName !== "string" || parameterName.length === 0) {
    throw new Error(`Playground route '${routeDefinition?.alias ?? ""}' does not declare a supported route parameter.`);
  }

  const normalizedRouteParameters = applyRouteDefaultParameterValues(routeDefinition, routeParameters);
  const value = normalizedRouteParameters?.[parameterName];
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

function resolveDetailRoutes(routeDefinitions) {
  const matches = resolveRoutes(routeDefinitions, "DetailPage");
  for (const route of matches) {
    if (route.parameterNames.length < 1) {
      throw new Error("Playground detail route must declare at least one route parameter.");
    }
  }

  return matches;
}

function resolvePrimaryDetailRoute(detailRoutes) {
  const singleParameterRoute = detailRoutes.find((route) => route.parameterNames.length === 1);
  if (!singleParameterRoute) {
    throw new Error("Playground detail routes must include at least one single-parameter route for catalog href generation.");
  }

  return singleParameterRoute;
}
