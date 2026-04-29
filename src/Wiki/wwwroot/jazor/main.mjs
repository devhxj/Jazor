import i$e2b55fd24ba846bc from "./components/wiki-home.mjs";
import { createApp } from "npm:vue@3";
import { createVuetify } from "npm:vuetify";
import { VBtn, VCard, VTextField } from "vuetify/components";
import { Ripple } from "vuetify/directives";
let VuetifyConfiguration = {
  components: {
    VBtn: VBtn,
    VCard: VCard,
    VTextField: VTextField
  },
  directives: { Ripple: Ripple },
  theme: { defaultTheme: "light" },
  display: { mobileBreakpoint: "md" }
};
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
  let __patin$07bf1ce29d54306855e0113c, input, __patin$91d60631e38ebcf57e9a1eb4, output;
  let app = createApp(i$e2b55fd24ba846bc);
  app.use(createVuetify(VuetifyConfiguration));
  app.mount("#app");
  if (!(__patin$07bf1ce29d54306855e0113c = document.getElementById("cs-input"), __patin$07bf1ce29d54306855e0113c instanceof HTMLTextAreaElement && (input = __patin$07bf1ce29d54306855e0113c, true)) || !(__patin$91d60631e38ebcf57e9a1eb4 = document.getElementById("js-output"), __patin$91d60631e38ebcf57e9a1eb4 instanceof HTMLElement && (output = __patin$91d60631e38ebcf57e9a1eb4, true)))
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
