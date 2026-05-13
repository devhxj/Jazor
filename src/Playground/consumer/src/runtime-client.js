import { mountPlaygroundApp } from "./bootstrap-app.js";
import "./style.css";

export { mountPlaygroundApp };
export function mountPlaygroundConsumer(components, hostRequirements, routesOrSelector = "#app", maybeSelector = "#app") {
  const hasExplicitRoutes = Array.isArray(routesOrSelector);
  const routeDefinitions = hasExplicitRoutes ? routesOrSelector : undefined;
  const selector = hasExplicitRoutes ? maybeSelector : routesOrSelector;

  return mountPlaygroundApp(
    components.CatalogPage,
    components.DetailPage,
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
