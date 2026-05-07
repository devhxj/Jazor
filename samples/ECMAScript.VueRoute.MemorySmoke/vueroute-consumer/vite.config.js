import { defineConfig } from "vite";
import path from "node:path";
import { fileURLToPath, URL } from "node:url";

const sampleRoot = fileURLToPath(new URL("..", import.meta.url));
const defaultJazorRoot = fileURLToPath(new URL("../VueRoute.MemorySmoke.Host/wwwroot/jazor/", import.meta.url));
const jazorRootFs = path.resolve(process.env.JAZOR_GENERATED_ROOT || defaultJazorRoot);
const jazorRoot = `${jazorRootFs.replace(/\\/g, "/").replace(/\/$/, "")}/`;
const vueRuntime = fileURLToPath(new URL("./node_modules/vue/dist/vue.runtime.esm-bundler.js", import.meta.url));
const vueRouterRuntime = fileURLToPath(new URL("./node_modules/vue-router/dist/vue-router.mjs", import.meta.url));

export default defineConfig({
  resolve: {
    alias: [
      { find: "vue", replacement: vueRuntime },
      { find: "npm:vue@3", replacement: vueRuntime },
      { find: "vue-router", replacement: vueRouterRuntime },
      { find: "npm:vue-router@4", replacement: vueRouterRuntime },
      { find: /^components\//, replacement: `${jazorRoot}components/` },
      { find: /^host\//, replacement: `${jazorRoot}host/` },
      { find: /^router\//, replacement: `${jazorRoot}router/` },
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
