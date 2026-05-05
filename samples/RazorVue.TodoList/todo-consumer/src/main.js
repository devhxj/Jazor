import { createApp } from "vue";
import { createVuetify } from "vuetify";
import "vuetify/styles";
import TodoApp from "../../Todo.Host/wwwroot/jazor/components/todo-app.vue";
import { razorVueHostRequirements } from "../../Todo.Host/wwwroot/jazor/__jazor/razorvue-host.mjs";

const vuetify = createVuetify();
const app = createApp(TodoApp);

app.use(vuetify);
app.mount("#app");

console.info("RazorVue host requirements", razorVueHostRequirements);
