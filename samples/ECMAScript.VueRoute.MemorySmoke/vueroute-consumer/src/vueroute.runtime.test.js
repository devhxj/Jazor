import assert from "node:assert/strict";
import { createConfiguredApp } from "host/app.mjs";
import { createRouterRuntime, navigateScenario } from "router/memory-router.mjs";

Deno.test("vue route runtime integration seams expose a configured app with router installation", () => {
  const app = createConfiguredApp();

  assert.ok(app);
  assert.strictEqual(typeof app.mount, "function");
  assert.strictEqual(typeof app.unmount, "function");
});

Deno.test("vue route runtime integration seams execute the generated router scenario directly", async () => {
  const router = createRouterRuntime();
  const snapshot = await navigateScenario(router);

  assert.strictEqual(snapshot.currentPath, "/query");
  assert.match(snapshot.globalGuard, /beforeResolve:/);
  assert.match(snapshot.afterEach, /afterEach:/);
  assert.strictEqual(snapshot.loadedPath, "/users/42");
});
