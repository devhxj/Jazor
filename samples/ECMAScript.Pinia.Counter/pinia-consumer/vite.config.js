import { defineConfig } from "vite";
import path from "node:path";
import { fileURLToPath, URL } from "node:url";

const sampleRoot = fileURLToPath(new URL("..", import.meta.url));
const defaultJazorRoot = fileURLToPath(new URL("../Pinia.Counter.Host/wwwroot/jazor/", import.meta.url));
const jazorRootFs = path.resolve(process.env.JAZOR_GENERATED_ROOT || defaultJazorRoot);
const jazorRoot = `${jazorRootFs.replace(/\\/g, "/").replace(/\/$/, "")}/`;
const vueRuntime = fileURLToPath(new URL("./node_modules/vue/dist/vue.runtime.esm-bundler.js", import.meta.url));
const piniaRuntime = fileURLToPath(new URL("./node_modules/pinia/dist/pinia.mjs", import.meta.url));
const piniaTestingRuntime = fileURLToPath(new URL("./node_modules/@pinia/testing/dist/index.mjs", import.meta.url));

export default defineConfig({
  resolve: {
    alias: [
      { find: "vue", replacement: vueRuntime },
      { find: "npm:vue@3", replacement: vueRuntime },
      { find: "pinia", replacement: piniaRuntime },
      { find: "@pinia/testing", replacement: piniaTestingRuntime },
      { find: /^components\//, replacement: `${jazorRoot}components/` },
      { find: /^host\//, replacement: `${jazorRoot}host/` },
      { find: /^stores\//, replacement: `${jazorRoot}stores/` },
      { find: /^tests\//, replacement: `${jazorRoot}tests/` },
      { find: /^System\//, replacement: `${jazorRoot}System/` }
    ]
  },
  server: {
    fs: {
      allow: [sampleRoot, jazorRootFs]
    }
  },
  test: {
    environment: "jsdom"
  }
});
