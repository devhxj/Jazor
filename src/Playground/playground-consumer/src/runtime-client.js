import { mountPlaygroundApp } from "./bootstrap-app.js";
import "./style.css";

export { mountPlaygroundApp };
export function mountRootComponent(catalogComponent, detailComponent, hostRequirements, selector = "#app") {
  return mountPlaygroundApp(catalogComponent, detailComponent, hostRequirements, selector);
}
