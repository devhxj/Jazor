import { createSSRApp, h } from "vue";
import { renderToString } from "vue/server-renderer";
import { createVuetify } from "vuetify";
import { assertHostRequirements } from "./runtime-common.js";

const expectedTexts = [
  "RazorVue Todo Workspace",
  "Validate generated DTO projections",
  "Runtime | Active",
  "Bundle generated Vue SFC",
  "Tooling | Completed",
  "Pinned"
];

export async function runTodoConsumerSsr(components, hostRequirements) {
  assertHostRequirements(hostRequirements);

  const TodoApp = components?.TodoApp;
  if (typeof TodoApp !== "object" && typeof TodoApp !== "function") {
    throw new Error("RazorVue Todo consumer expected a TodoApp component export.");
  }

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
  for (const expectedText of expectedTexts) {
    if (!html.includes(expectedText)) {
      throw new Error(`SSR smoke output did not contain expected text: ${expectedText}`);
    }
  }

  return html;
}

export async function runSsrSmoke(TodoApp, hostRequirements) {
  return await runTodoConsumerSsr({ TodoApp }, hostRequirements);
}
