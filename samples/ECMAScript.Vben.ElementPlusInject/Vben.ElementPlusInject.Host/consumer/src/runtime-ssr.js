import { createSSRApp, h } from "vue";
import { renderToString } from "vue/server-renderer";
import ElementPlus from "element-plus";
import { assertHostRequirements } from "./runtime-common.js";

const expectedTexts = [
  "ECMAScript.Vben",
  "Element Plus injected shell",
  "Operations overview",
  "Build pipeline",
  "Container injection",
  "Consumer contract",
  "Create release",
  "Export report",
  "ops@prod"
];

export async function runVbenConsumerSsr(components, hostRequirements, razorVueConsumerRoutes) {
  assertHostRequirements(hostRequirements);
  void razorVueConsumerRoutes;

  const VbenDashboardApp = components?.VbenDashboardApp;
  if (typeof VbenDashboardApp !== "object" && typeof VbenDashboardApp !== "function") {
    throw new Error("RazorVue Vben consumer expected a VbenDashboardApp component export.");
  }

  const app = createSSRApp({
    render() {
      return h(VbenDashboardApp, {
        collapsed: false,
        selectedKey: "overview",
        expandedKeys: ["release", "runtime"],
        "onUpdate:collapsed": () => {},
        "onUpdate:selectedKey": () => {},
        "onUpdate:expandedKeys": () => {}
      });
    }
  });

  app.use(ElementPlus);

  const html = await renderToString(app);
  for (const expectedText of expectedTexts) {
    if (!html.includes(expectedText)) {
      throw new Error(`SSR smoke output did not contain expected text: ${expectedText}`);
    }
  }

  return html;
}

export async function runSsrSmoke(VbenDashboardApp, hostRequirements, razorVueConsumerRoutes) {
  return await runVbenConsumerSsr({ VbenDashboardApp }, hostRequirements, razorVueConsumerRoutes);
}
