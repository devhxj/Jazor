import { readdirSync } from "node:fs";
import { resolve } from "node:path";
import { defineConfig } from "vite";
import vue from "@vitejs/plugin-vue";

const sourceRoot = process.env.JAZOR_VUE_DATA_UI_SOURCE_ROOT;
const outputRoot = process.env.JAZOR_VUE_DATA_UI_OUTPUT_ROOT;

if (!sourceRoot || !outputRoot) {
  throw new Error("JAZOR_VUE_DATA_UI_SOURCE_ROOT and JAZOR_VUE_DATA_UI_OUTPUT_ROOT are required.");
}

const entriesRoot = resolve(sourceRoot, "src", "entries");
const entries = Object.fromEntries(
  readdirSync(entriesRoot)
    .filter((name) => name.startsWith("vue-ui-") && name.endsWith(".js"))
    .sort()
    .map((name) => [name.slice(0, -3), resolve(entriesRoot, name)]),
);

export default defineConfig({
  root: sourceRoot,
  plugins: [vue()],
  build: {
    outDir: outputRoot,
    emptyOutDir: true,
    copyPublicDir: false,
    target: "es2016",
    cssCodeSplit: true,
    manifest: true,
    minify: "oxc",
    rollupOptions: {
      external: ["vue", "jspdf"],
      // These files are public package subpaths. Vite's application defaults may turn a
      // multi-entry facade into a side-effect-only import, which would drop the named export
      // consumed by RazorVue. Keep every facade's export signature intact.
      preserveEntrySignatures: "strict",
      input: entries,
      output: {
        exports: "named",
        entryFileNames: "components/[name].js",
        chunkFileNames: "chunks/[name]-[hash].js",
        assetFileNames: "assets/[name]-[hash][extname]",
      },
    },
  },
});
