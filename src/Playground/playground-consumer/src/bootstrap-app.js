import { computed, createApp, defineComponent, h } from "vue";
import { createPinia } from "pinia";
import { createRouter, createWebHistory, RouterView } from "vue-router";
import { createVuetify } from "vuetify";
import * as directives from "vuetify/directives";
import * as components from "vuetify/components";
import "vuetify/styles";
import { createPlaygroundRoutes } from "./router.js";
import { usePlaygroundStore } from "./stores/playground-store.js";
import { assertHostRequirements, installShellNavigationInterception } from "./runtime-common.js";

export function mountPlaygroundApp(CatalogPage, DetailPage, hostRequirements, selector = "#app") {
  assertHostRequirements(hostRequirements);
  const VApp = components.VApp;
  const VMain = components.VMain;

  const pinia = createPinia();
  const router = createRouter({
    history: createWebHistory(resolveBaseHref()),
    routes: createPlaygroundRoutes()
  });
  const vuetify = createVuetify({
    components,
    directives
  });

  const AppRoot = defineComponent({
    name: "PlaygroundAppRoot",
    setup() {
      const store = usePlaygroundStore(pinia);
      store.setRootComponents(CatalogPage, DetailPage);
      const appStateClass = computed(() => {
        if (store.isLoading) {
          return "playground-app--loading";
        }
        if (store.hasError) {
          return "playground-app--error";
        }

        return "playground-app--ready";
      });

      return () => h(
        VApp,
        {
          class: ["playground-app-shell", appStateClass.value]
        },
        {
          default: () => [
            h("div", { class: "playground-app-shell__backdrop" }),
            h("div", { class: "playground-app-shell__content" }, [
              h("header", { class: "playground-shell-topbar" }, [
                h("div", { class: "playground-shell-topbar__brand" }, "Playground"),
                h("div", { class: "playground-shell-topbar__meta" }, [
                  h("span", null, "RazorVue"),
                  h("span", null, "Vuetify"),
                  h("span", null, "Pinia"),
                  h("span", null, "Vue Router"),
                  h("span", null, "ASP.NET Core")
                ])
              ]),
              h(VMain, { class: "playground-shell-main" }, {
                default: () => [h(RouterView)]
              })
            ])
          ]
        }
      );
    }
  });

  const app = createApp(AppRoot);
  app.use(pinia);
  app.use(router);
  app.use(vuetify);
  installShellNavigationInterception(router);
  app.mount(selector);
  return { app, router, pinia, vuetify };
}

function resolveBaseHref() {
  const baseUri = document.baseURI ?? `${window.location.origin}/`;
  return new URL(baseUri).pathname || "/";
}
