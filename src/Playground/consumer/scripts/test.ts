import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { runBuild } from "./build.ts";
import { runBrowserSmoke } from "./smoke-browser.ts";
import { runSsrSmoke } from "./smoke-ssr.ts";

async function runUnitTests(): Promise<void> {
  const consumerRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
  const output = await new Deno.Command(Deno.execPath(), {
    cwd: consumerRoot,
    args: ["test", "--allow-env", "./src/runtime-common.test.js"],
    stdin: "null",
    stdout: "piped",
    stderr: "piped"
  }).output();

  if (!output.success) {
    throw new Error(new TextDecoder().decode(output.stderr).trim() || "Playground consumer unit tests failed.");
  }
}

async function runAll(): Promise<void> {
  await runUnitTests();
  await runSsrSmoke();
  await runBuild();
  await runBrowserSmoke();
  console.log("Playground pure Deno pipeline passed.");
}

if (import.meta.main) {
  await runAll();
}
