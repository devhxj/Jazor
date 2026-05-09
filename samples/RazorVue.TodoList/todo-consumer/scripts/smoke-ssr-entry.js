import { createSSRApp, h } from "vue";
import { renderToString } from "vue/server-renderer";
import { createVuetify } from "vuetify";
import TodoApp from "../../Todo.Host/wwwroot/jazor/components/todo-app.vue";
import { razorVueHostRequirements } from "../../Todo.Host/wwwroot/jazor/__jazor/razorvue-host.mjs";

export async function runSmoke() {
  const app = createSSRApp({
    render() {
      return h(TodoApp, {
        draftTitle: "Ship RazorVue",
        draftCategory: "Release",
        draftPinned: true,
        showCompleted: true,
        statusMessage: "SSR smoke loaded generated RazorVue SFC.",
        totalCount: 2,
        completedCount: 1,
        openCount: 1,
        pinnedCount: 1,
        visibleCount: 2,
        tasks: [
          { Id: 1, Title: "Validate generated DTO projections", Category: "Runtime", IsDone: false, IsPinned: true },
          { Id: 2, Title: "Bundle generated Vue SFC", Category: "Tooling", IsDone: true, IsPinned: false }
        ],
        onAddRequested: () => {}
      });
    }
  });

  app.use(createVuetify());

  const html = await renderToString(app);

  for (const expected of [
    "RazorVue Todo Workspace",
    "Validate generated DTO projections",
    "Runtime | Active",
    "Bundle generated Vue SFC",
    "Tooling | Completed",
    "Pinned"
  ]) {
    if (!html.includes(expected)) {
      throw new Error(`SSR smoke output did not contain expected text: ${expected}`);
    }
  }

  if (!razorVueHostRequirements.pluginRequirements.includes("vuetify")) {
    throw new Error("RazorVue host requirements must declare the Vuetify plugin.");
  }

  if (!razorVueHostRequirements.styles.includes("vuetify/styles")) {
    throw new Error("RazorVue host requirements must declare Vuetify styles.");
  }
}
