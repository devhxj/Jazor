import TodoApp from "./generated-browser/components/todo-app.mjs";
import { razorVueHostRequirements } from "../../Todo.Host/wwwroot/jazor/__jazor/razorvue-host.mjs";
import { mountTodoApp } from "../src/runtime-client.js";

mountTodoApp(TodoApp, razorVueHostRequirements);
