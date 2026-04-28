import i$e2b55fd24ba846bc from "./components/wiki-home.mjs";
import { createApp } from "vue";
let VisibilityPattern = RegExp("^\\s*(?:public|private|protected|internal)\\s+", "gm");
let StaticPattern = RegExp("^\\s*static\\s+", "gm");
let TypedLocalPattern = RegExp("^(\\s*)(?:var|bool|byte|sbyte|short|ushort|int|uint|long|ulong|float|double|decimal|string|char|object)\\s+([A-Za-z_][\\w]*)\\s*=", "gm");
let TypedMethodPattern = RegExp("^(\\s*)([A-Za-z_][\\w<>,\\[\\]\\?]*)\\s+([A-Za-z_][\\w]*)\\s*\\(([^)]*)\\)(\\s*\\{?)", "gm");
let EmptyLinePattern = RegExp("\\n{3,}", "g");
let _input;
let _output;
let Initialized = Initialize();
function Initialize() {
  Boot();
  return true;
}
export function Boot() {
  let __patin$df74ae785caa2099bc41e5bf, input, __patin$c7f279c9bde2e257dea34853, output;
  let app = createApp(i$e2b55fd24ba846bc);
  app.mount("#app");
  if (!(__patin$df74ae785caa2099bc41e5bf = document.getElementById("cs-input"), __patin$df74ae785caa2099bc41e5bf instanceof HTMLTextAreaElement && (input = __patin$df74ae785caa2099bc41e5bf, true)) || !(__patin$c7f279c9bde2e257dea34853 = document.getElementById("js-output"), __patin$c7f279c9bde2e257dea34853 instanceof HTMLElement && (output = __patin$c7f279c9bde2e257dea34853, true)))
    return;
  _input = input;
  _output = output;
  input.addEventListener("input", { handleEvent: OnInputChanged }, false);
  RenderPreview();
}
function OnInputChanged(event) {
  event;
  RenderPreview();
}
function RenderPreview() {
  if (_input === null || _output === null)
    return;
  let normalized = NormalizeInput(_input.value);
  if (normalized.length === 0) {
    _output.textContent = "// Input is empty.";
    return;
  }
  let body = ConvertPreviewSource(normalized);
  _output.textContent = "// jazor.wiki live preview\n" + "// This is a fast browser-side preview for authoring feedback.\n" + body;
}
function NormalizeInput(value) {
  return value.replaceAll("\r\n", "\n").replaceAll("\r", "\n").trimEnd();
}
function ConvertPreviewSource(source) {
  let text = source;
  text = text.replace(VisibilityPattern, "");
  text = text.replace(StaticPattern, "");
  text = text.replaceAll("Console.WriteLine", "console.log");
  text = text.replace(TypedLocalPattern, "$1let $2 =");
  text = text.replace(TypedMethodPattern, "$1function $3($4)$5");
  text = text.replace(EmptyLinePattern, "\n\n");
  return text;
}
//# sourceMappingURL=main.mjs.map
