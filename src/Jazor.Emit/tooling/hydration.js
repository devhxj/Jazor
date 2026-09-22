import { createSSRApp } from "vue";

export async function hydrate(modulePath, mountElementId = "app") {
  const mountElement = document.getElementById(mountElementId);
  if (!mountElement) throw new Error("Jazor SSR mount element was not found.");
  const stateElement = document.getElementById("__jazor_ssr_state");
  if (!stateElement) throw new Error("Jazor SSR state envelope element was not found.");
  const state = JSON.parse(stateElement.textContent);
  if (!state || state.schema !== "jazor-ssr-state" || state.version !== 1 ||
      !("props" in state) || !Array.isArray(state.providers) ||
      state.providers.some(provider => !provider || typeof provider.key !== "string" || provider.key.trim().length === 0) ||
      new Set(state.providers.map(provider => provider.key)).size !== state.providers.length) {
    throw new Error("Jazor SSR state envelope version or provider shape is not supported.");
  }
  if (mountElement.dataset.jazorSsrHydrated === "1" || mountElement.dataset.jazorSsrHydrating === "1" ||
      mountElement.dataset.jazorSsrHydrating === "failed") {
    throw new Error("Jazor SSR hydration was already executed for this mount element.");
  }
  // Claim before the first await: concurrent calls must not import or mount the root twice.
  mountElement.dataset.jazorSsrHydrating = "1";
  try {
    const load = componentLoaders[modulePath];
    if (!load) throw new Error(`Jazor SSR component '${modulePath}' is not a declared hydration root.`);
    const { default: component } = await load();
    const app = createSSRApp(component, state.props);
    let mountError;
    app.config.errorHandler = error => { mountError = error; };
    for (const provider of state.providers) app.provide(provider.key, provider.value);
    app.mount(mountElement);
    await Promise.resolve();
    if (mountError) throw mountError;
    mountElement.dataset.jazorSsrHydrated = "1";
    delete mountElement.dataset.jazorSsrHydrating;
  } catch (error) {
    mountElement.dataset.jazorSsrHydrating = "failed";
    throw error;
  }
}
