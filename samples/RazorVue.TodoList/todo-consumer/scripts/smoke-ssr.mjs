import { createServer } from "vite";

const server = await createServer({
  configFile: "./vite.config.js",
  appType: "custom",
  server: { middlewareMode: true }
});

try {
  const { runSmoke } = await server.ssrLoadModule("/scripts/smoke-ssr-entry.js");
  await runSmoke();
  console.log("RazorVue TodoList SSR smoke passed.");
} finally {
  await server.close();
}
