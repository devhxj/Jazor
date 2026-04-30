import i$e2b55fd24ba846bc from "./components/wiki-home.mjs";
import { createApp } from "npm:vue@3";
import { createVuetify } from "npm:vuetify";
import { VBtn, VCard, VTextField } from "vuetify/components";
import { Ripple } from "vuetify/directives";
let vuetifyConfiguration = {
  components: {
    VBtn: VBtn,
    VCard: VCard,
    VTextField: VTextField
  },
  directives: { Ripple: Ripple },
  theme: { defaultTheme: "light" },
  display: { mobileBreakpoint: "md" }
};
let initialized = initialize();
function initialize() {
  boot();
  return true;
}
export function boot() {
  let app = createApp(i$e2b55fd24ba846bc);
  app.use(createVuetify(vuetifyConfiguration));
  app.mount("#app");
}
//# sourceMappingURL=main.mjs.map
