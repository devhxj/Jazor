import { createSSRApp } from "vue";
import { renderToString } from "@vue/server-renderer";

function normalizeModulePath(value) {
  if (typeof value !== "string" || value.trim().length === 0) {
    throw new Error("Jazor SSR request modulePath must be a non-empty relative string.");
  }

  const segments = value.trim().replaceAll("\\", "/").split("/");
  if (segments.some((segment) => segment.length === 0 || segment === "." || segment === "..")) {
    throw new Error("Jazor SSR request modulePath cannot escape the artifact root.");
  }

  return segments.join("/");
}

const requestText = await new Response(Deno.stdin.readable).text();
const request = JSON.parse(requestText);
const modulePath = normalizeModulePath(request.modulePath);
const artifactRootUrl = new URL("../", import.meta.url);
const moduleUrl = new URL(modulePath, artifactRootUrl);
if (!moduleUrl.href.startsWith(artifactRootUrl.href)) {
  throw new Error("Jazor SSR request modulePath resolved outside the artifact root.");
}

const module = await import(moduleUrl.href);
if (!("default" in module)) {
  throw new Error(`Jazor SSR root module '${modulePath}' must export a default component.`);
}

const app = createSSRApp(module.default, request.props);
const html = await renderToString(app);
await Deno.stdout.write(new TextEncoder().encode(JSON.stringify({ html })));
