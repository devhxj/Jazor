import assert from "node:assert/strict";
import { disposePinia, getActivePinia } from "pinia";
import { createHmrBridge } from "./pinia.hmr-bridge.js";
import { createManagedPiniaRoot } from "./pinia.root-lifecycle.js";
import { clearConfiguredActivePinia } from "host/app.mjs";
import {
  createFactoryTestingRoot,
  createTypedTestingRoot,
  createStrictTestingRoot,
  createTestingRoot
} from "tests/counter-testing.mjs";
import {
  useCounterStore,
  useProjectedCounterStore
} from "stores/counter-store.mjs";
import { compareIsolatedRoots } from "components/counter-isolation.mjs";

async function withRuntimeCleanup(callback) {
  const ephemeralPiniaRoots = [];

  try {
    await callback(ephemeralPiniaRoots);
  } finally {
    clearConfiguredActivePinia();

    while (ephemeralPiniaRoots.length > 0) {
      disposePinia(ephemeralPiniaRoots.pop());
    }
  }
}

Deno.test("pinia runtime integration seams registers both generated HMR accept handlers through the host bridge", () => {
    const calls = [];
    const accept = (...args) => {
      calls.push(args);
    };
    const bridge = createHmrBridge({ accept });

    assert.strictEqual(bridge.register(), true);
    assert.strictEqual(calls.length, 2);
    assert.strictEqual(typeof calls[0][0], "function");
    assert.strictEqual(typeof calls[1][0], "function");
});

Deno.test("pinia runtime integration seams creates, activates, and disposes a configured pinia root", () => {
    const managed = createManagedPiniaRoot();
    const projectedStore = useProjectedCounterStore(managed.pinia);

    assert.ok(managed.app);
    assert.strictEqual(managed.activePinia, managed.pinia);
    assert.strictEqual(getActivePinia(), managed.pinia);
    assert.ok(Object.keys(managed.pinia.state.value).includes("counterHydrationOptions"));
    assert.strictEqual(projectedStore.auditTag, "counter:audited");
    assert.strictEqual(projectedStore.$state.persistedAt, "plugin:counter");

    managed.dispose();

    assert.deepStrictEqual(managed.pinia.state.value, {});
    assert.strictEqual(getActivePinia(), undefined);
});

Deno.test("pinia runtime integration seams clears the active root through the generated host lifecycle helper", () => {
    const managed = createManagedPiniaRoot();

    assert.strictEqual(getActivePinia(), managed.pinia);

    const cleared = clearConfiguredActivePinia();

    assert.strictEqual(cleared, undefined);
    assert.strictEqual(getActivePinia(), undefined);

    managed.dispose();
});

Deno.test("pinia runtime integration seams keeps the testing root fake app and plugin ordering observable at runtime", async () => {
  await withRuntimeCleanup(async (ephemeralPiniaRoots) => {
    const testingPinia = createTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(typeof testingPinia.app.use, "function");

    const projectedStore = useProjectedCounterStore(testingPinia);

    assert.strictEqual(projectedStore.auditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.persistedAt, "testing:counter:typed");
  });
});

Deno.test("pinia runtime integration seams keeps the combined typed testing root observable at runtime", async () => {
  await withRuntimeCleanup(async (ephemeralPiniaRoots) => {
    const testingPinia = createTypedTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = useProjectedCounterStore(testingPinia);
    const store = useCounterStore(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(projectedStore.auditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.persistedAt, "testing:counter:typed");
    assert.strictEqual(store.count, 12);

    store.increment();
    assert.strictEqual(store.count, 12);

    store.decrement();
    assert.strictEqual(store.count, 11);
    assert.strictEqual(store.status, "decrement() updated the store.");
  });
});

Deno.test("pinia runtime integration seams keeps the combined typed factory testing root observable at runtime", async () => {
  await withRuntimeCleanup(async (ephemeralPiniaRoots) => {
    const testingPinia = createFactoryTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = useProjectedCounterStore(testingPinia);
    const store = useCounterStore(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(projectedStore.auditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.persistedAt, "testing:counter:typed");
    assert.strictEqual(store.count, 18);

    store.increment();
    assert.strictEqual(store.count, 19);

    store.decrement();
    assert.strictEqual(store.count, 19);
    assert.strictEqual(store.status, "increment() updated the store.");
  });
});

Deno.test("pinia runtime integration seams disposes an individual store scope without deleting the retained pinia state snapshot", () => {
    const managed = createManagedPiniaRoot();
    const store = useCounterStore(managed.pinia);

    store.increment();
    assert.strictEqual(store.count, 3);

    store.$dispose();

    assert.strictEqual(managed.pinia.state.value.counter.count, 3);
    assert.strictEqual(managed.pinia.state.value.counter.status, "increment() updated the store.");

    managed.dispose();
});

Deno.test("pinia runtime integration seams can recreate a fresh pinia root after disposing the previous one", () => {
    const first = createManagedPiniaRoot();
    first.dispose();

    const second = createManagedPiniaRoot();

    assert.notStrictEqual(second.pinia, first.pinia);
    assert.strictEqual(getActivePinia(), second.pinia);
    assert.ok(Object.keys(second.pinia.state.value).includes("counterHydrationOptions"));

    second.dispose();
});

Deno.test("pinia runtime integration seams restores the previous active root when managed roots are disposed in stack order", () => {
    const first = createManagedPiniaRoot();
    const second = createManagedPiniaRoot();

    assert.strictEqual(getActivePinia(), second.pinia);

    second.dispose();

    assert.strictEqual(getActivePinia(), first.pinia);

    first.dispose();

    assert.strictEqual(getActivePinia(), undefined);
});

Deno.test("pinia runtime integration seams never reactivates a previously disposed root when managed roots are torn down out of order", () => {
    const first = createManagedPiniaRoot();
    const second = createManagedPiniaRoot();

    first.dispose();

    assert.strictEqual(getActivePinia(), second.pinia);

    second.dispose();

    assert.strictEqual(getActivePinia(), undefined);
});

Deno.test("pinia runtime integration seams keeps named action stubs and patch/reset stubs active in the strict testing root", async () => {
  await withRuntimeCleanup(async (ephemeralPiniaRoots) => {
    const testingPinia = createStrictTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = useProjectedCounterStore(testingPinia);
    const store = useCounterStore(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(projectedStore.auditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.persistedAt, "testing:counter:typed");
    assert.strictEqual(store.count, 15);

    store.increment();
    store.$patch({ count: 200 });
    store.$reset();

    assert.strictEqual(store.count, 15);
    assert.strictEqual(store.status, "Seeded from strict createTestingPinia().");
    assert.strictEqual(store.$state.persistedAt, "testing:counter:typed");
  });
});

Deno.test("pinia runtime integration seams keeps explicit multi-root store resolution isolated across two configured pinia roots", () => {
    const snapshot = compareIsolatedRoots();

    assert.strictEqual(snapshot, "3|2|counter:audited|counter:audited|isolated:left:counter|plugin:counter");
});
