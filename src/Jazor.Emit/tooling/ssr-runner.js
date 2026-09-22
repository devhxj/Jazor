import { createSSRApp } from "vue";
import { renderToString } from "@vue/server-renderer";

const protocolPrefix = "__JAZOR_SSR__:";
const artifactRootUrl = new URL("./", import.meta.url);

function writeResponse(response) {
  console.log(protocolPrefix + JSON.stringify(response));
}

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

async function render(request) {
  const modulePath = normalizeModulePath(request.modulePath);
  const state = request.state;
  if (!state || state.schema !== "jazor-ssr-state" || state.version !== 1 ||
      !("props" in state) || !Array.isArray(state.providers)) {
    throw new Error("Jazor SSR state envelope is missing or has an unsupported schema/version.");
  }
  const moduleUrl = new URL(modulePath, artifactRootUrl);
  if (!moduleUrl.href.startsWith(artifactRootUrl.href)) {
    throw new Error("Jazor SSR request modulePath resolved outside the artifact root.");
  }

  const module = await import(moduleUrl.href);
  if (!("default" in module)) {
    throw new Error(`Jazor SSR root module '${modulePath}' must export a default component.`);
  }

  const app = createSSRApp(module.default, state.props);
  for (const provider of state.providers) {
    if (!provider || typeof provider.key !== "string" || provider.key.trim().length === 0) {
      throw new Error("Jazor SSR state envelope contains an invalid provider.");
    }
    app.provide(provider.key, provider.value);
  }
  // renderToString swallows render-hook errors and emits "<!---->" placeholders. Capture them
  // so a broken component fails the request explicitly instead of serving an empty page.
  // renderToString 会吞掉渲染期错误并输出 "<!---->"；捕获后显式失败，避免静默空页面。
  const renderErrors = [];
  app.config.errorHandler = (error) => {
    renderErrors.push(error instanceof Error ? error.stack ?? error.message : String(error));
  };
  const html = await renderToString(app);
  if (renderErrors.length > 0) {
    throw new Error(
      "Jazor SSR render failed while rendering '" + modulePath + "':\n" + renderErrors.join("\n"));
  }

  return html;
}

// A loopback HTTP endpoint survives Deno watch restarts without reusing an old stdin reader.
// The host owns worker concurrency; Deno owns module loading and development file watching.
Deno.serve({
  hostname: "127.0.0.1",
  port: 0,
  onListen: ({ port }) => writeResponse({ kind: "ready", url: `http://127.0.0.1:${port}/` })
}, async (request) => {
  try {
    const html = await render(await request.json());
    return Response.json({ html });
  } catch (error) {
    const message = error instanceof Error ? error.stack ?? error.message : String(error);
    return Response.json({ error: message }, { status: 500 });
  }
});
