import "./vue-feature-flags.mjs";
import TodoApp from "./generated-browser/components/todo-app.mjs";
import { razorVueHostRequirements } from "../../Todo.Host/wwwroot/jazor/__jazor/razorvue-host.mjs";
import { mountRootComponent } from "../src/runtime-client.js";

mountRootComponent(TodoApp, razorVueHostRequirements);
