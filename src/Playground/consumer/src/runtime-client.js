import { mountPlaygroundApp } from "./bootstrap-app.js";
import "./style.css";
import { resolveRequiredComponentExport } from "./runtime-common.js";

export { mountPlaygroundApp };
export function mountPlaygroundConsumer(components, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
  const hasExplicitRoutes = Array.isArray(routesOrSelector);
  const routeDefinitions = hasExplicitRoutes ? routesOrSelector : undefined;
  const selector = hasExplicitRoutes ? maybeSelector : routesOrSelector;
  const CatalogPage = resolveRequiredComponentExport(components, "CatalogPage");
  const DetailPage = resolveRequiredComponentExport(components, "DetailPage");

  return mountPlaygroundApp(
    CatalogPage,
    DetailPage,
    hostRequirements,
    routeDefinitions,
    selector);
}

export function mountRootComponent(catalogComponent, detailComponent, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
  const hasExplicitRoutes = Array.isArray(routesOrSelector);
  const routeDefinitions = hasExplicitRoutes ? routesOrSelector : undefined;
  const selector = hasExplicitRoutes ? maybeSelector : routesOrSelector;

  return mountPlaygroundApp(catalogComponent, detailComponent, hostRequirements, routeDefinitions, selector);
}
