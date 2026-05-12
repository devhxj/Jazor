import { runBuild } from "./build.ts";
import { runBrowserSmoke } from "./smoke-browser.ts";
import { runSsrSmoke } from "./smoke-ssr.ts";

async function runAll(): Promise<void> {
  await runSsrSmoke();
  await runBuild();
  await runBrowserSmoke();
  console.log("Playground pure Deno pipeline passed.");
}

if (import.meta.main) {
  await runAll();
}
