import { defineConfig } from "vite";
import { fileURLToPath, URL } from "node:url";

const sampleRoot = fileURLToPath(new URL("..", import.meta.url));
const jazorRoot = fileURLToPath(new URL("../Pinia.Counter.Host/wwwroot/jazor/", import.meta.url));
const vueRuntime = fileURLToPath(new URL("./node_modules/vue/dist/vue.runtime.esm-bundler.js", import.meta.url));
const piniaRuntime = fileURLToPath(new URL("./node_modules/pinia/dist/pinia.mjs", import.meta.url));

export default defineConfig({
  resolve: {
    alias: [
      { find: "vue", replacement: vueRuntime },
      { find: "npm:vue@3", replacement: vueRuntime },
      { find: "pinia", replacement: piniaRuntime },
      { find: /^components\//, replacement: `${jazorRoot}components/` },
      { find: /^host\//, replacement: `${jazorRoot}host/` },
      { find: /^stores\//, replacement: `${jazorRoot}stores/` },
      { find: /^System\//, replacement: `${jazorRoot}System/` }
    ]
  },
  server: {
    fs: {
      allow: [sampleRoot]
    }
  }
});
