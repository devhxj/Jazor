export function assertHostRequirements(hostRequirements) {
  if (hostRequirements === null || typeof hostRequirements !== "object") {
    throw new Error("RazorVue host requirements were not provided to the Playground consumer runtime.");
  }

  if (!Array.isArray(hostRequirements.pluginRequirements)) {
    throw new Error("RazorVue host requirements must expose a pluginRequirements array.");
  }

  if (!Array.isArray(hostRequirements.styles)) {
    throw new Error("RazorVue host requirements must expose a styles array.");
  }

  if (!hostRequirements.pluginRequirements.includes("vuetify")) {
    throw new Error("RazorVue host requirements must declare the Vuetify plugin.");
  }

  if (!hostRequirements.styles.includes("vuetify/styles")) {
    throw new Error("RazorVue host requirements must declare Vuetify styles.");
  }
}

export function resolveRequiredComponentExport(components, exportName) {
  if (components === null || typeof components !== "object") {
    throw new Error("Playground consumer component exports must be provided as an object.");
  }

  const component = components[exportName];
  if (typeof component !== "object" && typeof component !== "function") {
    throw new Error(`Playground consumer expected a '${exportName}' component export.`);
  }

  return component;
}

export function resolveConsumerRoutes(routeDefinitions) {
  if (routeDefinitions == null) {
    throw new Error("Playground consumer routes must be provided by the generated RazorVue consumer entry.");
  }

  if (!Array.isArray(routeDefinitions)) {
    throw new Error("Playground consumer routes must be provided as an array.");
  }

  const normalizedRoutes = routeDefinitions.map((route, index) => normalizeRoute(route, index));
  if (normalizedRoutes.length === 0) {
    throw new Error("Playground consumer routes must contain at least one route.");
  }

  return Object.freeze(normalizedRoutes);
}

export function installShellNavigationInterception(router, routeDefinitions) {
  if (typeof document === "undefined") {
    return;
  }

  document.addEventListener("click", async (event) => {
    if (event.defaultPrevented || event.button !== 0) {
      return;
    }

    const anchor = event.target instanceof Element
      ? event.target.closest("a[href]")
      : null;
    if (!(anchor instanceof HTMLAnchorElement)) {
      return;
    }

    if (anchor.target && anchor.target !== "_self") {
      return;
    }

    if (anchor.hasAttribute("download")) {
      return;
    }

    if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
      return;
    }

    const url = new URL(anchor.href, window.location.href);
    if (url.origin !== window.location.origin) {
      return;
    }

    if (!shouldHandleClientRoute(url.pathname, routeDefinitions)) {
      return;
    }

    event.preventDefault();
    await router.push(`${url.pathname}${url.search}${url.hash}`);
  });
}

function shouldHandleClientRoute(pathname, routeDefinitions) {
  return routeDefinitions.some((route) => matchRoutePath(route.path, pathname));
}

function normalizeRoute(route, index) {
  if (route === null || typeof route !== "object") {
    throw new Error(`Playground consumer route at index ${index} is invalid.`);
  }

  const name = typeof route.name === "string" && route.name.length > 0
    ? route.name
    : null;
  const alias = typeof route.alias === "string" && route.alias.length > 0
    ? route.alias
    : null;
  const path = typeof route.path === "string" && route.path.startsWith("/")
    ? route.path
    : null;
  const parameterNames = Array.isArray(route.parameterNames)
    ? route.parameterNames.filter((item) => typeof item === "string" && item.length > 0)
    : [];

  if (!name || !alias || !path) {
    throw new Error(`Playground consumer route at index ${index} is missing required metadata.`);
  }

  return Object.freeze({
    name,
    alias,
    componentId: typeof route.componentId === "string" ? route.componentId : "",
    componentName: typeof route.componentName === "string" ? route.componentName : alias,
    componentModel: typeof route.componentModel === "string" ? route.componentModel : "",
    routeTemplate: typeof route.routeTemplate === "string" ? route.routeTemplate : path,
    path,
    parameterNames: Object.freeze(parameterNames)
  });
}

function matchRoutePath(routePath, pathname) {
  if (routePath === "/") {
    return pathname === "/";
  }

  const routeSegments = splitPath(routePath);
  const pathSegments = splitPath(pathname);
  const requiredRouteSegmentCount = routeSegments.filter((segment) => !isOptionalRouteParameterSegment(segment)).length;
  if (pathSegments.length < requiredRouteSegmentCount || pathSegments.length > routeSegments.length) {
    return false;
  }

  for (let index = 0; index < routeSegments.length; index += 1) {
    const routeSegment = routeSegments[index];
    const pathSegment = pathSegments[index];
    if (routeSegment.startsWith(":")) {
      if (!pathSegment && !isOptionalRouteParameterSegment(routeSegment)) {
        return false;
      }

      continue;
    }

    if (routeSegment !== pathSegment) {
      return false;
    }
  }

  return true;
}

function splitPath(path) {
  return path
    .split("/")
    .filter((segment) => segment.length > 0);
}

function isOptionalRouteParameterSegment(segment) {
  return segment.startsWith(":") && segment.endsWith("?");
}
