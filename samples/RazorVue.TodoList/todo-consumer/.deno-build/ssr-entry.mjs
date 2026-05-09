import TodoApp from "./generated-ssr/components/todo-app.mjs";
import { razorVueHostRequirements } from "../../Todo.Host/wwwroot/jazor/__jazor/razorvue-host.mjs";
import { runSsrSmoke } from "../src/runtime-ssr.js";

export { runSsrSmoke };
export async function executeSsrSmoke() {
  return await runSsrSmoke(TodoApp, razorVueHostRequirements);
}
