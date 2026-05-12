import assert from "node:assert/strict";
import { setActivePinia } from "pinia";
import {
  createFactoryTestingRoot,
  createTypedTestingRoot,
  createStrictTestingRoot,
  createTestingRoot
} from "tests/counter-testing.mjs";
import { createConfiguredPinia } from "host/app.mjs";
import { useCounterStore, installAuditPlugin } from "stores/counter-store.mjs";
import {
  useHydrationOptionStore,
  useHydrationStore
} from "components/counter-hydration.mjs";

function activateTestingRoot() {
    setActivePinia(createTestingRoot());
}

Deno.test("generated pinia sample modules seeds counter state through createTestingPinia()", () => {
  activateTestingRoot();
    const store = useCounterStore();

    assert.strictEqual(store.count, 9);
    assert.strictEqual(store.status, "Seeded from createTestingPinia().");
    assert.strictEqual(store.$state.persistedAt, "testing:counter:typed");
});

Deno.test("generated pinia sample modules keeps increment live while decrement is stubbed by predicate", () => {
  activateTestingRoot();
    const store = useCounterStore();

    store.increment();
    assert.strictEqual(store.count, 10);
    assert.strictEqual(store.status, "increment() updated the store.");

    store.decrement();
    assert.strictEqual(store.count, 10);
    assert.strictEqual(store.status, "increment() updated the store.");
});

Deno.test("generated pinia sample modules allows real patch/reset because the testing root disabled those stubs", () => {
  activateTestingRoot();
    const store = useCounterStore();

    store.$patch({
      count: 20,
      status: "patched from deno"
    });
    assert.strictEqual(store.count, 20);
    assert.strictEqual(store.status, "patched from deno");

    store.$reset();
    assert.strictEqual(store.count, 2);
    assert.strictEqual(store.status, "Store seeded through defineStore().");
});

Deno.test("generated pinia sample modules supports combined typed testing options while keeping runtime shape unchanged", () => {
    setActivePinia(createTypedTestingRoot());

    const store = useCounterStore();

    assert.strictEqual(store.count, 12);
    assert.strictEqual(store.status, "Seeded from combined typed createTestingPinia().");
    assert.strictEqual(store.$state.persistedAt, "testing:counter:typed");

    store.increment();
    assert.strictEqual(store.count, 12);
    assert.strictEqual(store.status, "Seeded from combined typed createTestingPinia().");

    store.decrement();
    assert.strictEqual(store.count, 11);
    assert.strictEqual(store.status, "decrement() updated the store.");

    store.$patch({
      count: 21,
      status: "typed root patch"
    });
    assert.strictEqual(store.count, 21);
    assert.strictEqual(store.status, "typed root patch");
});

Deno.test("generated pinia sample modules supports combined typed testing options through the explicit union factory path", () => {
    setActivePinia(createFactoryTestingRoot());

    const store = useCounterStore();

    assert.strictEqual(store.count, 18);
    assert.strictEqual(store.status, "Seeded from combined typed factory createTestingPinia().");
    assert.strictEqual(store.$state.persistedAt, "testing:counter:typed");

    store.increment();
    assert.strictEqual(store.count, 19);
    assert.strictEqual(store.status, "increment() updated the store.");

    store.decrement();
    assert.strictEqual(store.count, 19);
    assert.strictEqual(store.status, "increment() updated the store.");
});

Deno.test("generated pinia sample modules can wrap the generated plugin callback with a JS spy", () => {
    const pluginContext = {
      store: {
        $id: "counter",
        $state: {
          persistedAt: ""
        }
      }
    };
    let callCount = 0;
    const spy = (context) => {
      callCount += 1;
      return installAuditPlugin(context);
    };

    const extension = spy(pluginContext);

    assert.strictEqual(callCount, 1);
    assert.deepStrictEqual(extension, {
      auditTag: "counter:audited"
    });
    assert.strictEqual(pluginContext.store.$state.persistedAt, "plugin:counter");
});

Deno.test("generated pinia sample modules keeps setup-store client-only refs skipped from hydration while option-store hydrate receives seeded state", () => {
    setActivePinia(createConfiguredPinia());

    const setupStore = useHydrationStore();
    const optionStore = useHydrationOptionStore();

    assert.strictEqual(setupStore.canHydrateClientOnlyNote(), false);
    assert.strictEqual(setupStore.clientOnlyNote, "client-only note seeded in setup store");
    assert.strictEqual(optionStore.count, 12);
    assert.strictEqual(optionStore.status, "serialized SSR payload -> hydrate(storeState, initialState)");

    setupStore.refreshClientOnlyNote();
    assert.strictEqual(
      setupStore.clientOnlyNote,
      "client note refreshed at serialized SSR payload -> hydrate(storeState, initialState)"
    );
});

Deno.test("generated pinia sample modules supports named stubActions plus stubbed patch/reset in a stricter testing root", () => {
    setActivePinia(createStrictTestingRoot());

    const store = useCounterStore();

    assert.strictEqual(store.count, 15);
    assert.strictEqual(store.status, "Seeded from strict createTestingPinia().");
    assert.strictEqual(store.$id, "counter");
    assert.strictEqual(store.$state.persistedAt, "testing:counter:typed");

    store.increment();
    store.decrement();
    assert.strictEqual(store.count, 15);
    assert.strictEqual(store.status, "Seeded from strict createTestingPinia().");

    store.$patch({
      count: 99,
      status: "should be blocked"
    });
    assert.strictEqual(store.count, 15);
    assert.strictEqual(store.status, "Seeded from strict createTestingPinia().");

    store.$reset();
    assert.strictEqual(store.count, 15);
    assert.strictEqual(store.status, "Seeded from strict createTestingPinia().");
    assert.strictEqual(store.$state.persistedAt, "testing:counter:typed");
});
