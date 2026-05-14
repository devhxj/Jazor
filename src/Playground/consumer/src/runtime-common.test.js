import { assertEquals, assertThrows } from "jsr:@std/assert";
import { resolveConsumerRoutes, resolveRequiredComponentExport } from "./runtime-common.js";

Deno.test("resolveConsumerRoutes rejects missing route definitions", () => {
  assertThrows(
    () => resolveConsumerRoutes(undefined),
    Error,
    "Playground consumer routes must be provided by the generated RazorVue consumer entry."
  );
});

Deno.test("resolveConsumerRoutes normalizes generated route metadata", () => {
  const routes = resolveConsumerRoutes([
    {
      name: "CatalogPage__0",
      alias: "CatalogPage",
      componentId: "Playground.Pages.PlaygroundCatalogPage",
      componentName: "PlaygroundCatalogPage",
      componentModel: "sfc",
      routeTemplate: "/",
      path: "/",
      parameterNames: []
    },
    {
      name: "DetailPage__0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{id}",
      path: "/examples/:id",
      parameterNames: ["id"]
    }
  ]);

  assertEquals(routes.length, 2);
  assertEquals(routes[0].routeTemplate, "/");
  assertEquals(routes[1].path, "/examples/:id");
  assertEquals(routes[1].parameterNames, ["id"]);
  assertEquals(Object.isFrozen(routes), true);
  assertEquals(Object.isFrozen(routes[0]), true);
  assertEquals(Object.isFrozen(routes[1].parameterNames), true);
});

Deno.test("resolveRequiredComponentExport rejects missing component object", () => {
  assertThrows(
    () => resolveRequiredComponentExport(undefined, "CatalogPage"),
    Error,
    "Playground consumer component exports must be provided as an object."
  );
});

Deno.test("resolveRequiredComponentExport rejects missing named export", () => {
  assertThrows(
    () => resolveRequiredComponentExport({}, "DetailPage"),
    Error,
    "Playground consumer expected a 'DetailPage' component export."
  );
});

Deno.test("resolveRequiredComponentExport returns valid component export", () => {
  const component = () => null;
  assertEquals(resolveRequiredComponentExport({ CatalogPage: component }, "CatalogPage"), component);
});
