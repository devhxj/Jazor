import { assertEquals, assertThrows } from "jsr:@std/assert";
import {
  doesRouteMatchPath,
  resolveConsumerRoutes,
  resolveRequiredComponentExport
} from "./runtime-common.js";
import { resolveRouteHref } from "./view-models.js";

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
    },
    {
      name: "DetailPage__1",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/post-{slug:int}",
      path: "/examples/post-:slug(\\d+)",
      parameterNames: ["slug"]
    },
    {
      name: "DetailPage__2",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{id=42}",
      path: "/examples/:id?",
      parameterNames: ["id"],
      defaultParameterValues: { id: "42" }
    },
    {
      name: "DetailPage__3",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{*path}",
      path: "/examples/:path(.*)*",
      parameterNames: ["path"]
    }
  ]);

  assertEquals(routes.length, 5);
  assertEquals(routes[0].routeTemplate, "/");
  assertEquals(routes[1].path, "/examples/:id");
  assertEquals(routes[1].parameterNames, ["id"]);
  assertEquals(routes[2].path, "/examples/post-:slug(\\d+)");
  assertEquals(routes[2].parameterNames, ["slug"]);
  assertEquals(routes[3].path, "/examples/:id?");
  assertEquals(routes[3].defaultParameterValues, { id: "42" });
  assertEquals(Object.isFrozen(routes[3].defaultParameterValues), true);
  assertEquals(routes[4].path, "/examples/:path(.*)*");
  assertEquals(routes[4].parameterNames, ["path"]);
  assertEquals(Object.isFrozen(routes), true);
  assertEquals(Object.isFrozen(routes[0]), true);
  assertEquals(Object.isFrozen(routes[1].parameterNames), true);
});

Deno.test("doesRouteMatchPath follows vue-router semantics for constrained, composite, and catch-all paths", () => {
  const routes = resolveConsumerRoutes([
    {
      name: "DetailPage__0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{id:int}",
      path: "/examples/:id(\\d+)",
      parameterNames: ["id"]
    },
    {
      name: "DetailPage__1",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/post-{slug}",
      path: "/examples/post-:slug",
      parameterNames: ["slug"]
    },
    {
      name: "DetailPage__2",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{id:int=42}",
      path: "/examples/:id(\\d+)?",
      parameterNames: ["id"],
      defaultParameterValues: { id: "42" }
    },
    {
      name: "DetailPage__3",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{*path}",
      path: "/examples/:path(.*)*",
      parameterNames: ["path"]
    }
  ]);

  assertEquals(doesRouteMatchPath(routes[0], "/examples/42"), true);
  assertEquals(doesRouteMatchPath(routes[0], "/examples/abc"), false);
  assertEquals(doesRouteMatchPath(routes[1], "/examples/post-catalog-shell"), true);
  assertEquals(doesRouteMatchPath(routes[1], "/examples/post"), false);
  assertEquals(doesRouteMatchPath(routes[2], "/examples"), true);
  assertEquals(doesRouteMatchPath(routes[2], "/examples/42"), true);
  assertEquals(doesRouteMatchPath(routes[2], "/examples/abc"), false);
  assertEquals(doesRouteMatchPath(routes[3], "/examples"), true);
  assertEquals(doesRouteMatchPath(routes[3], "/examples/a/b/c"), true);
});

Deno.test("resolveRouteHref uses vue-router semantics for composite, default-valued, and catch-all paths", () => {
  const compositeRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/post-{slug}",
      path: "/examples/post-:slug",
      parameterNames: ["slug"]
    }
  ])[0];
  const defaultRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__1",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{id=42}",
      path: "/examples/:id?",
      parameterNames: ["id"],
      defaultParameterValues: { id: "42" }
    }
  ])[0];
  const catchAllRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__2",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{*path}",
      path: "/examples/:path(.*)*",
      parameterNames: ["path"]
    }
  ])[0];

  assertEquals(resolveRouteHref(compositeRoute, { slug: "catalog-shell" }), "/examples/post-catalog-shell");
  assertEquals(resolveRouteHref(defaultRoute, {}), "/examples");
  assertEquals(resolveRouteHref(defaultRoute, { id: "42" }), "/examples");
  assertEquals(resolveRouteHref(defaultRoute, { id: "108" }), "/examples/108");
  assertEquals(resolveRouteHref(catchAllRoute, { path: ["a", "b"] }), "/examples/a/b");
  assertEquals(resolveRouteHref(catchAllRoute, { path: "a/b" }), "/examples/a/b");
  assertEquals(resolveRouteHref(catchAllRoute, {}), "/examples");
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
