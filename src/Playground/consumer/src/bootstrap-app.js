import { createApp } from "vue";
import { createPinia } from "pinia";
import { createRouter, createWebHistory } from "vue-router";
import { createVuetify } from "vuetify";
import * as directives from "vuetify/directives";
import * as components from "vuetify/components";
import "vuetify/styles";
import { createPlaygroundAppRoot } from "./app-shell.js";
import { createPlaygroundRoutes } from "./router.js";
import { assertHostRequirements, installShellNavigationInterception, resolveConsumerRoutes } from "./runtime-common.js";

export function mountPlaygroundApp(CatalogPage, DetailPage, hostRequirements, routeDefinitions, selector = "#app") {
  assertHostRequirements(hostRequirements);
  const consumerRoutes = resolveConsumerRoutes(routeDefinitions);

  const pinia = createPinia();
  const router = createRouter({
    history: createWebHistory(resolveBaseHref()),
    routes: createPlaygroundRoutes(consumerRoutes)
  });
  const vuetify = createVuetify({
    components,
    directives
  });
  const AppRoot = createPlaygroundAppRoot({
    pinia,
    CatalogPage,
    DetailPage,
    routeDefinitions: consumerRoutes,
    VApp: components.VApp,
    VMain: components.VMain
  });

  const app = createApp(AppRoot);
  app.use(pinia);
  app.use(router);
  app.use(vuetify);
  installShellNavigationInterception(router, consumerRoutes);
  app.mount(selector);
  return { app, router, pinia, vuetify };
}

function resolveBaseHref() {
  const baseUri = document.baseURI ?? `${window.location.origin}/`;
  return new URL(baseUri).pathname || "/";
}
