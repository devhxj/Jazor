import * as Vue from "vue";
import "./designer.style.css";

// VForm 3.0.10 only ships UMD. Expose this ESM Vue namespace during evaluation only.
// 模块仅按需导入，临时 global 必须在成功/失败两条路径都精确恢复。
const hadVueGlobal = Object.prototype.hasOwnProperty.call(globalThis, "Vue");
const previousVueGlobal = globalThis.Vue;
globalThis.Vue = Vue;

let runtimeModule;
try {
  runtimeModule = await import("./designer.umd.js");
} finally {
  if (hadVueGlobal) {
    globalThis.Vue = previousVueGlobal;
  } else {
    delete globalThis.Vue;
  }
}

// Debug raw ESM uses the UMD global export; Release's Vite transform uses default.
// These are the two existing build projections of the same vendored runtime.
const plugin = runtimeModule?.default ?? globalThis.VFormDesigner;
if (!plugin || typeof plugin.install !== "function" || !plugin.VFormDesigner) {
  throw new Error("VForm 3 runtime did not expose a valid designer plugin.");
}

export const VFormDesigner = plugin.VFormDesigner;
export default plugin;
