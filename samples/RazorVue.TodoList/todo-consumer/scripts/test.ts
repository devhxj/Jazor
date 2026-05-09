import { runBuild } from "./build.ts";
import { runBundleApiSmoke } from "./smoke-bundle-api.ts";
import { runSsrSmoke } from "./smoke-ssr.ts";

async function runAll(): Promise<void> {
  await runSsrSmoke();
  await runBundleApiSmoke();
  await runBuild();
  console.log("RazorVue TodoList pure Deno pipeline passed.");
}

if (import.meta.main) {
  await runAll();
}
