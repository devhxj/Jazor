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

  if (!hostRequirements.pluginRequirements.includes("vuetify")) {
    throw new Error("RazorVue host requirements must declare the Vuetify plugin.");
  }

  if (!hostRequirements.styles.includes("vuetify/styles")) {
    throw new Error("RazorVue host requirements must declare Vuetify styles.");
  }
}

export function createTodoState() {
  return reactive({
    nextId: 4,
    draftTitle: "Document RazorVue SFC contract",
    draftCategory: "Architecture",
    draftPinned: false,
    showCompleted: true,
    statusMessage: "Library mode emits Vue SFC artifacts during design time.",
    tasks: [
      { id: 1, title: "Define per-component SFC topology", category: "Compiler", isDone: false, isPinned: true },
      { id: 2, title: "Wire host requirements into consumer bootstrap", category: "Host", isDone: true, isPinned: false },
      { id: 3, title: "Verify generated .vue imports stay stable", category: "Emit", isDone: false, isPinned: false }
    ]
  });
}

export function createTodoRootComponent(TodoApp, state = createTodoState()) {
  const totalCount = computed(() => state.tasks.length);
  const completedCount = computed(() => state.tasks.filter((task) => task.isDone).length);
  const openCount = computed(() => state.tasks.filter((task) => !task.isDone).length);
  const pinnedCount = computed(() => state.tasks.filter((task) => task.isPinned).length);
  const visibleCount = computed(() => state.tasks.filter((task) => state.showCompleted || !task.isDone).length);

  function addTask() {
    const title = state.draftTitle && state.draftTitle.trim() ? state.draftTitle.trim() : "Untitled task";
    const category = state.draftCategory && state.draftCategory.trim() ? state.draftCategory.trim() : "General";
    state.tasks.unshift({
      id: state.nextId++,
      title,
      category,
      isDone: false,
      isPinned: state.draftPinned
    });
    state.statusMessage = `Added "${title}" to the top of the workspace.`;
    state.draftTitle = "";
  }

  return {
    render() {
      return h(TodoApp, {
        draftTitle: state.draftTitle,
        "onUpdate:draftTitle": (value) => {
          state.draftTitle = value ?? "";
          state.statusMessage = "Draft title updated in consumer state.";
        },
        draftCategory: state.draftCategory,
        "onUpdate:draftCategory": (value) => {
          state.draftCategory = value ?? "";
          state.statusMessage = "Category focus updated in consumer state.";
        },
        draftPinned: state.draftPinned,
        "onUpdate:draftPinned": (value) => {
          state.draftPinned = !!value;
          state.statusMessage = state.draftPinned
            ? "New tasks will be pinned for focus."
            : "New tasks will be created without a pin.";
        },
        showCompleted: state.showCompleted,
        "onUpdate:showCompleted": (value) => {
          state.showCompleted = !!value;
          state.statusMessage = state.showCompleted
            ? "Showing the full backlog."
            : "Filtering to active work only.";
        },
        statusMessage: state.statusMessage,
        totalCount: totalCount.value,
        completedCount: completedCount.value,
        openCount: openCount.value,
        pinnedCount: pinnedCount.value,
        visibleCount: visibleCount.value,
        tasks: state.tasks,
        onAddRequested: addTask
      });
    }
  };
}
