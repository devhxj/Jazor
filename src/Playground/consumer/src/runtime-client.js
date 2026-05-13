import { mountPlaygroundApp } from "./bootstrap-app.js";
import "./style.css";

export { mountPlaygroundApp };
export function mountPlaygroundConsumer(components, hostRequirements, selector = "#app") {
  return mountPlaygroundApp(
    components.CatalogPage,
    components.DetailPage,
    hostRequirements,
    selector);
}

export function mountRootComponent(catalogComponent, detailComponent, hostRequirements, selector = "#app") {
  return mountPlaygroundApp(catalogComponent, detailComponent, hostRequirements, selector);
}
