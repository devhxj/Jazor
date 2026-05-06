import { computed, createApp, h, reactive } from "vue";
import { createVuetify } from "vuetify";
import "vuetify/styles";
import TodoApp from "../../Todo.Host/wwwroot/jazor/components/todo-app.vue";
import { razorVueHostRequirements } from "../../Todo.Host/wwwroot/jazor/__jazor/razorvue-host.mjs";

const state = reactive({
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

const root = {
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

const vuetify = createVuetify();
const app = createApp(root);

app.use(vuetify);
app.mount("#app");

console.info("RazorVue host requirements", razorVueHostRequirements);
