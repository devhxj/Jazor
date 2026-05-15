import { computed, h, reactive } from "vue";

export function assertHostRequirements(hostRequirements) {
  if (hostRequirements === null || typeof hostRequirements !== "object") {
    throw new Error("RazorVue host requirements were not provided to the consumer runtime.");
  }

  if (!Array.isArray(hostRequirements.pluginRequirements)) {
    throw new Error("RazorVue host requirements must expose a pluginRequirements array.");
  }

  if (!Array.isArray(hostRequirements.styles)) {
    throw new Error("RazorVue host requirements must expose a styles array.");
  }

  if (!hostRequirements.pluginRequirements.includes("element-plus")) {
    throw new Error("RazorVue host requirements must declare the Element Plus plugin.");
  }

  if (!hostRequirements.styles.includes("element-plus/dist/index.css")) {
    throw new Error("RazorVue host requirements must declare Element Plus styles.");
  }
}

export function createVbenState() {
  return reactive({
    collapsed: false,
    selectedKey: "overview",
    expandedKeys: ["release", "runtime"]
  });
}

export function createVbenRootComponent(VbenDashboardApp, state = createVbenState()) {
  return {
    render() {
      return h(VbenDashboardApp, {
        collapsed: state.collapsed,
        "onUpdate:collapsed": (value) => {
          state.collapsed = !!value;
        },
        selectedKey: state.selectedKey,
        "onUpdate:selectedKey": (value) => {
          state.selectedKey = value ?? "overview";
        },
        expandedKeys: computed(() => state.expandedKeys).value,
        "onUpdate:expandedKeys": (value) => {
          state.expandedKeys = Array.isArray(value) ? value : ["release", "runtime"];
        }
      });
    }
  };
}
