import { assertEquals, assertThrows } from "jsr:@std/assert";
import {
  doesRouteMatchPath,
  resolveConsumerRoutes,
  resolveRequiredComponentExport
} from "./runtime-common.js";
import { createPlaygroundRoutes } from "./router.js";
import { resolveRouteHref } from "./view-models.js";

const guidRouteConstraintPattern = "[0-9A-Fa-f]{32}|[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}|\\{[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\\}|%7B[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}%7D|\\([0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}\\\\)|%28[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}%29|\\{0x[0-9A-Fa-f]{8},0x[0-9A-Fa-f]{4},0x[0-9A-Fa-f]{4},\\{0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2}\\}\\}|%7B0x[0-9A-Fa-f]{8},0x[0-9A-Fa-f]{4},0x[0-9A-Fa-f]{4},%7B0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2},0x[0-9A-Fa-f]{2}%7D%7D";

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
      defaultParameterValues: { id: "42" },
      elidableDefaultParameterNames: ["id", "id", "unknown", ""],
      parameterConstraints: {
        id: [
          { kind: "integerRange", min: "-2147483648", max: "2147483647" },
          { kind: "lengthRange", min: "1", max: "10" },
          { kind: "dateTimeParse" },
          { kind: "integerRange", min: "", max: "" },
          { kind: "unknown", min: "1" }
        ],
        unknown: [{ kind: "integerRange", min: "1" }]
      }
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
    },
    {
      name: "DetailPage__4_0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename.:ext",
      parameterNames: ["filename", "ext"]
    },
    {
      name: "DetailPage__4_1",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename",
      parameterNames: ["filename"]
    }
  ]);

  assertEquals(routes.length, 7);
  assertEquals(routes[0].routeTemplate, "/");
  assertEquals(routes[1].path, "/examples/:id");
  assertEquals(routes[1].parameterNames, ["id"]);
  assertEquals(routes[2].path, "/examples/post-:slug(\\d+)");
  assertEquals(routes[2].parameterNames, ["slug"]);
  assertEquals(routes[3].path, "/examples/:id?");
  assertEquals(routes[3].defaultParameterValues, { id: "42" });
  assertEquals(routes[3].elidableDefaultParameterNames, ["id"]);
  assertEquals(routes[3].parameterConstraints, {
    id: [
      { kind: "integerRange", min: "-2147483648", max: "2147483647" },
      { kind: "lengthRange", min: "1", max: "10" },
      { kind: "dateTimeParse" }
    ]
  });
  assertEquals(Object.isFrozen(routes[3].defaultParameterValues), true);
  assertEquals(Object.isFrozen(routes[3].elidableDefaultParameterNames), true);
  assertEquals(Object.isFrozen(routes[3].parameterConstraints), true);
  assertEquals(Object.isFrozen(routes[3].parameterConstraints.id), true);
  assertEquals(Object.isFrozen(routes[3].parameterConstraints.id[0]), true);
  assertEquals(routes[4].path, "/examples/:path(.*)*");
  assertEquals(routes[4].parameterNames, ["path"]);
  assertEquals(routes[5].path, "/files/:filename.:ext");
  assertEquals(routes[5].parameterNames, ["filename", "ext"]);
  assertEquals(routes[6].path, "/files/:filename");
  assertEquals(routes[6].parameterNames, ["filename"]);
  assertEquals(Object.isFrozen(routes), true);
  assertEquals(Object.isFrozen(routes[0]), true);
  assertEquals(Object.isFrozen(routes[1].parameterNames), true);
});

Deno.test("doesRouteMatchPath follows vue-router semantics for constrained, composite, optional-separator, and catch-all paths", () => {
  const routes = resolveConsumerRoutes([
    {
      name: "DetailPage__0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{id:int}",
      path: "/examples/:id([+-]?\\d+)",
      parameterNames: ["id"],
      parameterConstraints: {
        id: [{ kind: "integerRange", min: "-2147483648", max: "2147483647" }]
      }
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
      path: "/examples/:id([+-]?\\d+)?",
      parameterNames: ["id"],
      defaultParameterValues: { id: "42" },
      parameterConstraints: {
        id: [{ kind: "integerRange", min: "-2147483648", max: "2147483647" }]
      }
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
    },
    {
      name: "DetailPage__4",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{id:guid}",
      path: `/examples/:id(${guidRouteConstraintPattern})`,
      parameterNames: ["id"]
    },
    {
      name: "DetailPage__5",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/{flag:bool}",
      path: "/examples/:flag([Tt][Rr][Uu][Ee]|[Ff][Aa][Ll][Ss][Ee])",
      parameterNames: ["flag"]
    },
    {
      name: "DetailPage__6",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/bounded/{id:long:min(5):max(7)}",
      path: "/bounded/:id([+-]?\\d+)",
      parameterNames: ["id"],
      parameterConstraints: {
        id: [
          { kind: "integerRange", min: "-9223372036854775808", max: "9223372036854775807" },
          { kind: "integerRange", min: "5" },
          { kind: "integerRange", max: "7" }
        ]
      }
    },
    {
      name: "DetailPage__7",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/small/{id:range(-2,2)}",
      path: "/small/:id([+-]?\\d+)",
      parameterNames: ["id"],
      parameterConstraints: {
        id: [{ kind: "integerRange", min: "-2", max: "2" }]
      }
    },
    {
      name: "DetailPage__4_0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename.:ext",
      parameterNames: ["filename", "ext"]
    },
    {
      name: "DetailPage__4_1",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename",
      parameterNames: ["filename"]
    },
    {
      name: "DetailPage__10",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/fixed/{id:int:length(1,3)}",
      path: "/fixed/:id([+-]?\\d+)",
      parameterNames: ["id"],
      parameterConstraints: {
        id: [
          { kind: "integerRange", min: "-2147483648", max: "2147483647" },
          { kind: "lengthRange", min: "1", max: "3" }
        ]
      }
    },
    {
      name: "DetailPage__11",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/required/{id:int:required}",
      path: "/required/:id([+-]?\\d+)",
      parameterNames: ["id"],
      parameterConstraints: {
        id: [
          { kind: "integerRange", min: "-2147483648", max: "2147483647" },
          { kind: "lengthRange", min: "1" }
        ]
      }
    },
    {
      name: "DetailPage__12",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/numbers/{value:double}",
      path: "/numbers/:value([^/]+)",
      parameterNames: ["value"],
      parameterConstraints: {
        value: [{ kind: "numberParse", format: "double" }]
      }
    },
    {
      name: "DetailPage__13",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/floats/{value:float}",
      path: "/floats/:value([^/]+)",
      parameterNames: ["value"],
      parameterConstraints: {
        value: [{ kind: "numberParse", format: "float" }]
      }
    },
    {
      name: "DetailPage__14",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/decimals/{value:decimal}",
      path: "/decimals/:value([^/]+)",
      parameterNames: ["value"],
      parameterConstraints: {
        value: [{ kind: "numberParse", format: "decimal" }]
      }
    },
    {
      name: "DetailPage__15",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/events/{value:datetime}",
      path: "/events/:value([^/]+)",
      parameterNames: ["value"],
      parameterConstraints: {
        value: [{ kind: "dateTimeParse" }]
      }
    }
  ]);

  assertEquals(doesRouteMatchPath(routes[0], "/examples/42"), true);
  assertEquals(doesRouteMatchPath(routes[0], "/examples/+42"), true);
  assertEquals(doesRouteMatchPath(routes[0], "/examples/2147483648"), false);
  assertEquals(doesRouteMatchPath(routes[0], "/examples/abc"), false);
  assertEquals(doesRouteMatchPath(routes[1], "/examples/post-catalog-shell"), true);
  assertEquals(doesRouteMatchPath(routes[1], "/examples/post"), false);
  assertEquals(doesRouteMatchPath(routes[2], "/examples"), true);
  assertEquals(doesRouteMatchPath(routes[2], "/examples/42"), true);
  assertEquals(doesRouteMatchPath(routes[2], "/examples/2147483648"), false);
  assertEquals(doesRouteMatchPath(routes[2], "/examples/abc"), false);
  assertEquals(doesRouteMatchPath(routes[3], "/examples"), true);
  assertEquals(doesRouteMatchPath(routes[3], "/examples/a/b/c"), true);
  assertEquals(doesRouteMatchPath(routes[4], "/examples/00000000-0000-0000-0000-000000000000"), true);
  assertEquals(doesRouteMatchPath(routes[4], "/examples/00000000000000000000000000000000"), true);
  assertEquals(doesRouteMatchPath(routes[4], "/examples/%7B00000000-0000-0000-0000-000000000000%7D"), true);
  assertEquals(doesRouteMatchPath(routes[4], "/examples/%2800000000-0000-0000-0000-000000000000%29"), true);
  assertEquals(doesRouteMatchPath(routes[4], "/examples/%7B0x00000000,0x0000,0x0000,%7B0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00%7D%7D"), true);
  assertEquals(doesRouteMatchPath(routes[4], "/examples/not-a-guid"), false);
  assertEquals(doesRouteMatchPath(routes[5], "/examples/true"), true);
  assertEquals(doesRouteMatchPath(routes[5], "/examples/True"), true);
  assertEquals(doesRouteMatchPath(routes[5], "/examples/FALSE"), true);
  assertEquals(doesRouteMatchPath(routes[5], "/examples/yes"), false);
  assertEquals(doesRouteMatchPath(routes[6], "/bounded/4"), false);
  assertEquals(doesRouteMatchPath(routes[6], "/bounded/5"), true);
  assertEquals(doesRouteMatchPath(routes[6], "/bounded/7"), true);
  assertEquals(doesRouteMatchPath(routes[6], "/bounded/8"), false);
  assertEquals(doesRouteMatchPath(routes[7], "/small/-2"), true);
  assertEquals(doesRouteMatchPath(routes[7], "/small/2"), true);
  assertEquals(doesRouteMatchPath(routes[7], "/small/3"), false);
  assertEquals(doesRouteMatchPath(routes[8], "/files/readme.md"), true);
  assertEquals(doesRouteMatchPath(routes[8], "/files/readme"), false);
  assertEquals(doesRouteMatchPath(routes[9], "/files/readme"), true);
  assertEquals(doesRouteMatchPath(routes[10], "/fixed/1"), true);
  assertEquals(doesRouteMatchPath(routes[10], "/fixed/123"), true);
  assertEquals(doesRouteMatchPath(routes[10], "/fixed/1234"), false);
  assertEquals(doesRouteMatchPath(routes[11], "/required/1"), true);
  assertEquals(doesRouteMatchPath(routes[11], "/required/"), false);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/1.5"), true);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/.1e2"), true);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/1,,2"), true);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/NaN"), true);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/%2BInfinity"), true);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/Infinity"), true);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/1e"), false);
  assertEquals(doesRouteMatchPath(routes[12], "/numbers/%E2%88%9E"), false);
  assertEquals(doesRouteMatchPath(routes[13], "/floats/1e39"), true);
  assertEquals(doesRouteMatchPath(routes[13], "/floats/not-a-number"), false);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/1.5"), true);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/1-"), true);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/1,,2"), true);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/79228162514264337593543950335"), true);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/79228162514264337593543950335.1"), true);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/79228162514264337593543950335.5"), false);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/79228162514264337593543950336"), false);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/1e3"), false);
  assertEquals(doesRouteMatchPath(routes[14], "/decimals/NaN"), false);
  assertEquals(doesRouteMatchPath(routes[15], "/events/2026-05-25"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/2026-05"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/5%2F25%2F2026"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/5%2F25%2F26"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/5%2F25"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/May%2025"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/25%20May%202026"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/2026-05-25T13%3A45%3A30Z"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/Mon%2C%2025%20May%202026%2000%3A00%3A00%20GMT"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/13%3A45%3A30"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/1%20PM"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/1%20PM%20GMT"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/1%20PM%20%2B0800"), true);
  assertEquals(doesRouteMatchPath(routes[15], "/events/1%20PMZ"), false);
  assertEquals(doesRouteMatchPath(routes[15], "/events/1"), false);
  assertEquals(doesRouteMatchPath(routes[15], "/events/2026-02-29"), false);
  assertEquals(doesRouteMatchPath(routes[15], "/events/25%2F05%2F2026"), false);
  assertEquals(doesRouteMatchPath(routes[15], "/events/10000-01-01"), false);
  assertEquals(doesRouteMatchPath(routes[15], "/events/20260525"), false);
});

Deno.test("resolveRouteHref uses vue-router semantics for composite, default-valued, optional-separator, and catch-all paths", () => {
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
      defaultParameterValues: { id: "42" },
      elidableDefaultParameterNames: ["id"]
    }
  ])[0];
  const compositeDefaultRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__2",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/examples/post-{id=42}",
      path: "/examples/post-:id",
      parameterNames: ["id"],
      defaultParameterValues: { id: "42" },
      elidableDefaultParameterNames: []
    }
  ])[0];
  const catchAllRoute = resolveConsumerRoutes([
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
  ])[0];
  const fileRouteWithExtension = resolveConsumerRoutes([
    {
      name: "DetailPage__4_0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename.:ext",
      parameterNames: ["filename", "ext"]
    }
  ])[0];
  const fileRouteWithoutExtension = resolveConsumerRoutes([
    {
      name: "DetailPage__4_1",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename",
      parameterNames: ["filename"]
    }
  ])[0];
  const boundedRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__5",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/bounded/{id:range(5,7)}",
      path: "/bounded/:id([+-]?\\d+)",
      parameterNames: ["id"],
      parameterConstraints: {
        id: [{ kind: "integerRange", min: "5", max: "7" }]
      }
    }
  ])[0];
  const fixedLengthRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__6",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/fixed/{id:int:length(1,3)}",
      path: "/fixed/:id([+-]?\\d+)",
      parameterNames: ["id"],
      parameterConstraints: {
        id: [
          { kind: "integerRange", min: "-2147483648", max: "2147483647" },
          { kind: "lengthRange", min: "1", max: "3" }
        ]
      }
    }
  ])[0];
  const doubleRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__7",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/numbers/{value:double}",
      path: "/numbers/:value([^/]+)",
      parameterNames: ["value"],
      parameterConstraints: {
        value: [{ kind: "numberParse", format: "double" }]
      }
    }
  ])[0];
  const decimalRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__8",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/decimals/{value:decimal}",
      path: "/decimals/:value([^/]+)",
      parameterNames: ["value"],
      parameterConstraints: {
        value: [{ kind: "numberParse", format: "decimal" }]
      }
    }
  ])[0];
  const dateTimeRoute = resolveConsumerRoutes([
    {
      name: "DetailPage__9",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/events/{value:datetime}",
      path: "/events/:value([^/]+)",
      parameterNames: ["value"],
      parameterConstraints: {
        value: [{ kind: "dateTimeParse" }]
      }
    }
  ])[0];

  assertEquals(resolveRouteHref(compositeRoute, { slug: "catalog-shell" }), "/examples/post-catalog-shell");
  assertEquals(resolveRouteHref(defaultRoute, {}), "/examples");
  assertEquals(resolveRouteHref(defaultRoute, { id: "42" }), "/examples");
  assertEquals(resolveRouteHref(defaultRoute, { id: "108" }), "/examples/108");
  assertEquals(resolveRouteHref(compositeDefaultRoute, {}), "/examples/post-42");
  assertEquals(resolveRouteHref(compositeDefaultRoute, { id: "42" }), "/examples/post-42");
  assertEquals(resolveRouteHref(compositeDefaultRoute, { id: "108" }), "/examples/post-108");
  assertEquals(resolveRouteHref(catchAllRoute, { path: ["a", "b"] }), "/examples/a/b");
  assertEquals(resolveRouteHref(catchAllRoute, { path: "a/b" }), "/examples/a/b");
  assertEquals(resolveRouteHref(catchAllRoute, {}), "/examples");
  assertEquals(resolveRouteHref(fileRouteWithExtension, { filename: "readme", ext: "md" }), "/files/readme.md");
  assertEquals(resolveRouteHref(fileRouteWithoutExtension, { filename: "readme" }), "/files/readme");
  assertEquals(resolveRouteHref(boundedRoute, { id: "5" }), "/bounded/5");
  assertEquals(resolveRouteHref(fixedLengthRoute, { id: "123" }), "/fixed/123");
  assertEquals(resolveRouteHref(doubleRoute, { value: "NaN" }), "/numbers/NaN");
  assertEquals(resolveRouteHref(doubleRoute, { value: "1,,2" }), "/numbers/1,,2");
  assertEquals(resolveRouteHref(decimalRoute, { value: "1-" }), "/decimals/1-");
  assertEquals(resolveRouteHref(dateTimeRoute, { value: "2026-05-25T13:45:30Z" }), "/events/2026-05-25T13:45:30Z");
  assertThrows(
    () => resolveRouteHref(boundedRoute, { id: "8" }),
    Error,
    "parameters do not satisfy generated route constraints"
  );
  assertThrows(
    () => resolveRouteHref(fixedLengthRoute, { id: "1234" }),
    Error,
    "parameters do not satisfy generated route constraints"
  );
  assertThrows(
    () => resolveRouteHref(doubleRoute, { value: "not-a-number" }),
    Error,
    "parameters do not satisfy generated route constraints"
  );
  assertThrows(
    () => resolveRouteHref(decimalRoute, { value: "79228162514264337593543950336" }),
    Error,
    "parameters do not satisfy generated route constraints"
  );
  assertThrows(
    () => resolveRouteHref(dateTimeRoute, { value: "2026-02-29" }),
    Error,
    "parameters do not satisfy generated route constraints"
  );
});

Deno.test("createPlaygroundRoutes accepts generated detail route variants for one component", () => {
  const routeDefinitions = resolveConsumerRoutes([
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
      name: "DetailPage__0_0",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename.:ext",
      parameterNames: ["filename", "ext"]
    },
    {
      name: "DetailPage__0_1",
      alias: "DetailPage",
      componentId: "Playground.Pages.PlaygroundDetailPage",
      componentName: "PlaygroundDetailPage",
      componentModel: "sfc",
      routeTemplate: "/files/{filename}.{ext?}",
      path: "/files/:filename",
      parameterNames: ["filename"]
    }
  ]);

  const routes = createPlaygroundRoutes(routeDefinitions);
  assertEquals(routes.map((route) => route.path), ["/", "/files/:filename.:ext", "/files/:filename"]);
  assertEquals(routes[1].meta.routeDefinition.parameterNames, ["filename", "ext"]);
  assertEquals(routes[2].meta.routeDefinition.parameterNames, ["filename"]);
});

Deno.test("createPlaygroundRoutes rejects detail routes without a single-parameter href target", () => {
  const routeDefinitions = resolveConsumerRoutes([
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
      routeTemplate: "/files/{filename}.{ext}",
      path: "/files/:filename.:ext",
      parameterNames: ["filename", "ext"]
    }
  ]);

  assertThrows(
    () => createPlaygroundRoutes(routeDefinitions),
    Error,
    "Playground detail routes must include at least one single-parameter route for catalog href generation."
  );
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
