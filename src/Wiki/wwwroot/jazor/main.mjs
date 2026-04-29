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
let Initialized = Initialize();
function Initialize() {
  Boot();
  return true;
}
export function Boot() {
  let app = createApp(i$e2b55fd24ba846bc);
  app.use(createVuetify(VuetifyConfiguration));
  app.mount("#app");
}
//# sourceMappingURL=main.mjs.map
