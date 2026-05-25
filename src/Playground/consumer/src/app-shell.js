import { computed, defineComponent, h } from "vue";
import { RouterView } from "vue-router";
import { usePlaygroundStore } from "./stores/playground-store.js";
import { resolveRouteHrefForSingleParameter } from "./view-models.js";

export function createPlaygroundAppRoot({
  pinia,
  CatalogPage,
  DetailPage,
  routeDefinitions,
  VApp,
  VMain
}) {
  const shellRouteModel = createShellRouteModel(routeDefinitions);

  return defineComponent({
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
            h("div", { class: "playground-admin-shell" }, [
              h("aside", { class: "playground-admin-shell__barline" }, [
                h("div", { class: "playground-brand-mark" }, "J"),
                h("div", { class: "playground-barline__divider" }),
                h("div", { class: "playground-barline__badge" }, "控"),
                h("div", { class: "playground-barline__badge" }, "路"),
                h("div", { class: "playground-barline__badge" }, "例")
              ]),
              h("aside", { class: "playground-admin-shell__menuline" }, [
                h("div", { class: "playground-menuline__title" }, "运行导航"),
                h("nav", { class: "playground-menuline__nav" }, [
                  h("a", { href: shellRouteModel.catalogPrimaryHref }, "示例总览"),
                  h("a", { href: shellRouteModel.catalogAliasHref }, "目录别名"),
                  h("a", { href: shellRouteModel.detailSampleHref }, "示例详情")
                ]),
                h("div", { class: "playground-menuline__section" }, [
                  h("div", { class: "playground-menuline__label" }, "技术栈"),
                  h("div", { class: "playground-menuline__chips" }, [
                    h("span", null, "RazorVue"),
                    h("span", null, "Vuetify"),
                    h("span", null, "Pinia"),
                    h("span", null, "Vue Router"),
                    h("span", null, "ASP.NET Core")
                  ])
                ])
              ]),
              h("div", { class: "playground-admin-shell__workspace" }, [
                h("header", { class: "playground-headline" }, [
                  h("div", { class: "playground-headline__title" }, "中文后台式 Playground"),
                  h("div", { class: "playground-headline__subtitle" }, "基于 Razor @page、统一 jazor-manifest.json 与官方 host API 的真实宿主验证")
                ]),
                h("section", { class: "playground-urltree" }, [
                  h("div", { class: "playground-urltree__title" }, "URL Tree"),
                  h("div", { class: "playground-urltree__list" }, [
                    ...shellRouteModel.routeTemplates.map((routeTemplate) => h("code", null, routeTemplate))
                  ])
                ]),
                h(VMain, { class: "playground-body" }, {
                  default: () => [h(RouterView)]
                }),
                h("footer", { class: "playground-footer" }, [
                  h("span", null, "Jazor Playground"),
                  h("span", null, "版权所有 © 2026"),
                  h("span", null, "单 ASP.NET Core 宿主")
                ])
              ])
            ])
          ]
        }
      );
    }
  });
}

function createShellRouteModel(routeDefinitions) {
  const catalogRoutes = routeDefinitions.filter((route) => route.alias === "CatalogPage");
  const detailRoutes = routeDefinitions.filter((route) => route.alias === "DetailPage");
  const detailRoute = detailRoutes.find((route) => route.parameterNames.length === 1)
    ?? detailRoutes[0]
    ?? null;

  const catalogPrimaryRoute = catalogRoutes.find((route) => route.path === "/") ?? catalogRoutes[0] ?? null;
  const catalogAliasRoute = catalogRoutes.find((route) => route.path !== catalogPrimaryRoute?.path) ?? catalogPrimaryRoute;

  return {
    catalogPrimaryHref: catalogPrimaryRoute?.path ?? "/",
    catalogAliasHref: catalogAliasRoute?.path ?? catalogPrimaryRoute?.path ?? "/",
    detailSampleHref: detailRoute === null
      ? "/examples/catalog-shell"
      : resolveRouteHrefForSingleParameter(detailRoute, "catalog-shell"),
    routeTemplates: routeDefinitions.map((route) => route.routeTemplate ?? route.path)
  };
}
