import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";
import vuetify from "vite-plugin-vuetify";
import { fileURLToPath, URL } from "node:url";

export default defineConfig({
  plugins: [
    vue(),
    vuetify({ autoImport: false })
  ],
  resolve: {
    alias: {
      "vuetify/components": fileURLToPath(new URL("./node_modules/vuetify/lib/components/index.js", import.meta.url))
    }
  }
});
