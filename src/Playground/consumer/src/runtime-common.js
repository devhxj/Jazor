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

export function installShellNavigationInterception(router) {
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

    if (!shouldHandleClientRoute(url.pathname)) {
      return;
    }

    event.preventDefault();
    await router.push(`${url.pathname}${url.search}${url.hash}`);
  });
}

function shouldHandleClientRoute(pathname) {
  return pathname === "/" || pathname.startsWith("/examples/");
}
