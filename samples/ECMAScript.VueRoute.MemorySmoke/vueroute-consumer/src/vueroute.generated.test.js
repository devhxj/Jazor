import assert from "node:assert/strict";
import {
  createTestingRouter,
  navigateBlockedPath,
  navigateLegacyRedirect,
  runScenario
} from "tests/router-testing.mjs";

Deno.test("generated vue route sample modules drives the generated memory-router navigation scenario", async () => {
  const snapshot = await runScenario();

  assert.strictEqual(snapshot.currentPath, "/query");
  assert.match(snapshot.currentFullPath, /\/query/);
  assert.match(snapshot.globalGuard, /beforeResolve:/);
  assert.match(snapshot.afterEach, /afterEach:/);
  assert.strictEqual(snapshot.loadedPath, "/users/42");
});

Deno.test("generated vue route sample modules resolves the generated redirect route through the typed route table", async () => {
  const redirected = await navigateLegacyRedirect();

  assert.match(redirected, /\/users\/11/);
  assert.match(redirected, /via=legacy-redirect/);
});

Deno.test("generated vue route sample modules keeps the blocked route guard observable at runtime", async () => {
  const blocked = await navigateBlockedPath();

  assert.match(blocked, /\//);
  assert.match(blocked, /beforeEach:blocked/);
});

Deno.test("generated vue route sample modules creates a standalone generated router instance", () => {
  const router = createTestingRouter();

  assert.ok(router);
  assert.strictEqual(typeof router.push, "function");
  assert.strictEqual(typeof router.beforeEach, "function");
});
