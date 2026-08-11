import assert from "node:assert/strict";
import { CreateConfiguredApp } from "host/app.mjs";
import { CreateRouterRuntime, NavigateScenario } from "router/memory-router.mjs";

Deno.test("vue route runtime integration seams expose a configured app with router installation", () => {
  const app = CreateConfiguredApp();

  assert.ok(app);
  assert.strictEqual(typeof app.mount, "function");
  assert.strictEqual(typeof app.unmount, "function");
});

Deno.test("vue route runtime integration seams execute the generated router scenario directly", async () => {
  const router = CreateRouterRuntime();
  const snapshot = await NavigateScenario(router);

  assert.strictEqual(snapshot.currentPath, "/query");
  assert.match(snapshot.globalGuard, /beforeResolve:/);
  assert.match(snapshot.afterEach, /afterEach:/);
  assert.strictEqual(snapshot.loadedPath, "/users/42");
});
