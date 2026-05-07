import { describe, expect, it } from "vitest";
import {
  createTestingRouter,
  navigateBlockedPath,
  navigateLegacyRedirect,
  runScenario
} from "tests/router-testing.mjs";

describe("vue route runtime smoke", () => {
  it("drives the generated memory-router navigation scenario", async () => {
    const snapshot = await runScenario();

    expect(snapshot.currentPath).toBe("/query");
    expect(snapshot.currentFullPath).toContain("/query");
    expect(snapshot.globalGuard).toContain("beforeResolve:");
    expect(snapshot.afterEach).toContain("afterEach:");
    expect(snapshot.loadedPath).toBe("/users/42");
  });

  it("resolves the generated redirect route through the typed route table", async () => {
    const redirected = await navigateLegacyRedirect();

    expect(redirected).toContain("/users/11");
    expect(redirected).toContain("via=legacy-redirect");
  });

  it("keeps the blocked route guard observable at runtime", async () => {
    const blocked = await navigateBlockedPath();

    expect(blocked).toContain("/");
    expect(blocked).toContain("beforeEach:blocked");
  });

  it("creates a standalone generated router instance", () => {
    const router = createTestingRouter();

    expect(router).toBeDefined();
    expect(typeof router.push).toBe("function");
    expect(typeof router.beforeEach).toBe("function");
  });
});
