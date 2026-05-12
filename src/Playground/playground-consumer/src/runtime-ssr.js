import { createSSRApp, defineComponent, h } from "vue";
import { renderToString } from "vue/server-renderer";
import { createPinia, setActivePinia } from "pinia";
import { createRouter, createMemoryHistory, RouterView } from "vue-router";
import { createVuetify } from "vuetify";
import * as directives from "vuetify/directives";
import * as components from "vuetify/components";
import "vuetify/styles";
import { assertHostRequirements } from "./runtime-common.js";
import { createPlaygroundRoutes } from "./router.js";
import { usePlaygroundStore } from "./stores/playground-store.js";

const expectedTexts = [
  "RazorVue + Vuetify + Pinia + VueRoute on ASP.NET Core",
  "Catalog shell with API-backed discovery",
  "Pinia favorites and persisted operator preferences",
  "DenoHost consumer pipeline for generated SFCs"
];

export async function runSsrSmoke(CatalogPage, DetailPage, hostRequirements) {
  assertHostRequirements(hostRequirements);

  const pinia = createPinia();
  setActivePinia(pinia);
  const router = createRouter({
    history: createMemoryHistory("/"),
    routes: createPlaygroundRoutes()
  });
  const vuetify = createVuetify({
    components,
    directives
  });

  const store = usePlaygroundStore(pinia);
  store.setRootComponents(CatalogPage, DetailPage);
  store.hydrateCatalog({
    Examples: [
      {
        Id: "catalog-shell",
        Title: "Catalog shell with API-backed discovery",
        Category: "Architecture",
        Difficulty: "Intermediate",
        Runtime: "ASP.NET Core + RazorVue",
        Summary: "A real examples catalog with server-backed discovery, typed records, and a responsive RazorVue/Vuetify shell.",
        Featured: true,
        EstimatedMinutes: 18,
        Tags: ["catalog", "api", "razorvue", "vuetify"]
      },
      {
        Id: "pinia-favorites",
        Title: "Pinia favorites and persisted operator preferences",
        Category: "State",
        Difficulty: "Intermediate",
        Runtime: "Pinia",
        Summary: "Tracks saved examples, search text, and category filters through a typed client-side store with browser persistence.",
        Featured: true,
        EstimatedMinutes: 14,
        Tags: ["pinia", "state", "favorites", "persistence"]
      },
      {
        Id: "deno-pipeline",
        Title: "DenoHost consumer pipeline for generated SFCs",
        Category: "Tooling",
        Difficulty: "Advanced",
        Runtime: "DenoHost",
        Summary: "Compiles emitted RazorVue SFC artifacts into browser and SSR-ready modules through a pure Deno bundling pipeline.",
        Featured: false,
        EstimatedMinutes: 22,
        Tags: ["deno", "bundle", "ssr", "consumer"]
      }
    ],
    Categories: ["All", "Architecture", "State", "Tooling"]
  });

  const AppRoot = defineComponent({
    name: "PlaygroundSsrRoot",
    render() {
      return h("div", { class: "playground-app-shell" }, [h(RouterView)]);
    }
  });

  const app = createSSRApp(AppRoot);
  app.use(pinia);
  app.use(router);
  app.use(vuetify);
  await router.push("/");
  await router.isReady();

  const html = await renderToString(app);
  for (const expectedText of expectedTexts) {
    if (!html.includes(expectedText)) {
      throw new Error(`Playground SSR smoke output did not contain expected text: ${expectedText}`);
    }
  }

  return html;
}
