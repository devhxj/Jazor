import assert from "node:assert/strict";
import { disposePinia, getActivePinia } from "pinia";
import { createHmrBridge } from "./pinia.hmr-bridge.js";
import { createManagedPiniaRoot } from "./pinia.root-lifecycle.js";
import { ClearConfiguredActivePinia } from "host/app.mjs";
import {
  CreateFactoryTestingRoot,
  CreateTypedTestingRoot,
  CreateStrictTestingRoot,
  CreateTestingRoot
} from "tests/counter-testing.mjs";
import {
  UseCounterStore,
  UseProjectedCounterStore
} from "stores/counter-store.mjs";
import { CompareIsolatedRoots } from "components/counter-isolation.mjs";

async function withRuntimeCleanup(callback) {
  const ephemeralPiniaRoots = [];

  try {
    await callback(ephemeralPiniaRoots);
  } finally {
    ClearConfiguredActivePinia();

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
    const projectedStore = UseProjectedCounterStore(managed.pinia);

    assert.ok(managed.app);
    assert.strictEqual(managed.activePinia, managed.pinia);
    assert.strictEqual(getActivePinia(), managed.pinia);
    assert.ok(Object.keys(managed.pinia.state.value).includes("counterHydrationOptions"));
    assert.strictEqual(projectedStore.AuditTag, "counter:audited");
    assert.strictEqual(projectedStore.$state.PersistedAt, "plugin:counter");

    managed.dispose();

    assert.deepStrictEqual(managed.pinia.state.value, {});
    assert.strictEqual(getActivePinia(), undefined);
});

Deno.test("pinia runtime integration seams clears the active root through the generated host lifecycle helper", () => {
    const managed = createManagedPiniaRoot();

    assert.strictEqual(getActivePinia(), managed.pinia);

    const cleared = ClearConfiguredActivePinia();

    assert.strictEqual(cleared, undefined);
    assert.strictEqual(getActivePinia(), undefined);

    managed.dispose();
});

Deno.test("pinia runtime integration seams keeps the testing root fake app and plugin ordering observable at runtime", async () => {
  await withRuntimeCleanup(async (ephemeralPiniaRoots) => {
    const testingPinia = CreateTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(typeof testingPinia.app.use, "function");

    const projectedStore = UseProjectedCounterStore(testingPinia);

    assert.strictEqual(projectedStore.AuditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.PersistedAt, "testing:counter:typed");
  });
});

Deno.test("pinia runtime integration seams keeps the combined typed testing root observable at runtime", async () => {
  await withRuntimeCleanup(async (ephemeralPiniaRoots) => {
    const testingPinia = CreateTypedTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = UseProjectedCounterStore(testingPinia);
    const store = UseCounterStore(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(projectedStore.AuditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.PersistedAt, "testing:counter:typed");
    assert.strictEqual(store.Count, 12);

    store.Increment();
    assert.strictEqual(store.Count, 12);

    store.Decrement();
    assert.strictEqual(store.Count, 11);
    assert.strictEqual(store.Status, "decrement() updated the store.");
  });
});

Deno.test("pinia runtime integration seams keeps the combined typed factory testing root observable at runtime", async () => {
  await withRuntimeCleanup(async (ephemeralPiniaRoots) => {
    const testingPinia = CreateFactoryTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = UseProjectedCounterStore(testingPinia);
    const store = UseCounterStore(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(projectedStore.AuditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.PersistedAt, "testing:counter:typed");
    assert.strictEqual(store.Count, 18);

    store.Increment();
    assert.strictEqual(store.Count, 19);

    store.Decrement();
    assert.strictEqual(store.Count, 19);
    assert.strictEqual(store.Status, "increment() updated the store.");
  });
});

Deno.test("pinia runtime integration seams disposes an individual store scope without deleting the retained pinia state snapshot", () => {
    const managed = createManagedPiniaRoot();
    const store = UseCounterStore(managed.pinia);

    store.Increment();
    assert.strictEqual(store.Count, 3);

    store.$dispose();

    assert.strictEqual(managed.pinia.state.value.counter.Count, 3);
    assert.strictEqual(managed.pinia.state.value.counter.Status, "increment() updated the store.");

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
    const testingPinia = CreateStrictTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = UseProjectedCounterStore(testingPinia);
    const store = UseCounterStore(testingPinia);

    assert.ok(testingPinia.app);
    assert.strictEqual(projectedStore.AuditTag, "counter:testing");
    assert.strictEqual(projectedStore.$state.PersistedAt, "testing:counter:typed");
    assert.strictEqual(store.Count, 15);

    store.Increment();
    store.$patch({ Count: 200 });
    store.$reset();

    assert.strictEqual(store.Count, 15);
    assert.strictEqual(store.Status, "Seeded from strict createTestingPinia().");
    assert.strictEqual(store.$state.PersistedAt, "testing:counter:typed");
  });
});

Deno.test("pinia runtime integration seams keeps explicit multi-root store resolution isolated across two configured pinia roots", () => {
    const snapshot = CompareIsolatedRoots();

    assert.strictEqual(snapshot, "3|2|counter:audited|counter:audited|isolated:left:counter|plugin:counter");
});
