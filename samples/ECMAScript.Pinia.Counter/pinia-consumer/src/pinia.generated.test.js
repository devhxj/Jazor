import assert from "node:assert/strict";
import { setActivePinia } from "pinia";
import {
  CreateFactoryTestingRoot,
  CreateTypedTestingRoot,
  CreateStrictTestingRoot,
  CreateTestingRoot
} from "tests/counter-testing.mjs";
import { CreateConfiguredPinia } from "host/app.mjs";
import { UseCounterStore, InstallAuditPlugin } from "stores/counter-store.mjs";
import {
  UseHydrationOptionStore,
  UseHydrationStore
} from "components/counter-hydration.mjs";

function activateTestingRoot() {
    setActivePinia(CreateTestingRoot());
}

Deno.test("generated pinia sample modules seeds counter state through createTestingPinia()", () => {
  activateTestingRoot();
    const store = UseCounterStore();

    assert.strictEqual(store.Count, 9);
    assert.strictEqual(store.Status, "Seeded from createTestingPinia().");
    assert.strictEqual(store.$state.PersistedAt, "testing:counter:typed");
});

Deno.test("generated pinia sample modules keeps increment live while decrement is stubbed by predicate", () => {
  activateTestingRoot();
    const store = UseCounterStore();

    store.Increment();
    assert.strictEqual(store.Count, 10);
    assert.strictEqual(store.Status, "increment() updated the store.");

    store.Decrement();
    assert.strictEqual(store.Count, 10);
    assert.strictEqual(store.Status, "increment() updated the store.");
});

Deno.test("generated pinia sample modules allows real patch/reset because the testing root disabled those stubs", () => {
  activateTestingRoot();
    const store = UseCounterStore();

    store.$patch({
      Count: 20,
      Status: "patched from deno"
    });
    assert.strictEqual(store.Count, 20);
    assert.strictEqual(store.Status, "patched from deno");

    store.$reset();
    assert.strictEqual(store.Count, 2);
    assert.strictEqual(store.Status, "Store seeded through defineStore().");
});

Deno.test("generated pinia sample modules supports combined typed testing options while keeping runtime shape unchanged", () => {
    setActivePinia(CreateTypedTestingRoot());

    const store = UseCounterStore();

    assert.strictEqual(store.Count, 12);
    assert.strictEqual(store.Status, "Seeded from combined typed createTestingPinia().");
    assert.strictEqual(store.$state.PersistedAt, "testing:counter:typed");

    store.Increment();
    assert.strictEqual(store.Count, 12);
    assert.strictEqual(store.Status, "Seeded from combined typed createTestingPinia().");

    store.Decrement();
    assert.strictEqual(store.Count, 11);
    assert.strictEqual(store.Status, "decrement() updated the store.");

    store.$patch({
      Count: 21,
      Status: "typed root patch"
    });
    assert.strictEqual(store.Count, 21);
    assert.strictEqual(store.Status, "typed root patch");
});

Deno.test("generated pinia sample modules supports combined typed testing options through the explicit union factory path", () => {
    setActivePinia(CreateFactoryTestingRoot());

    const store = UseCounterStore();

    assert.strictEqual(store.Count, 18);
    assert.strictEqual(store.Status, "Seeded from combined typed factory createTestingPinia().");
    assert.strictEqual(store.$state.PersistedAt, "testing:counter:typed");

    store.Increment();
    assert.strictEqual(store.Count, 19);
    assert.strictEqual(store.Status, "increment() updated the store.");

    store.Decrement();
    assert.strictEqual(store.Count, 19);
    assert.strictEqual(store.Status, "increment() updated the store.");
});

Deno.test("generated pinia sample modules can wrap the generated plugin callback with a JS spy", () => {
    const pluginContext = {
      store: {
        $id: "counter",
        $state: {
          PersistedAt: ""
        }
      }
    };
    let callCount = 0;
    const spy = (context) => {
      callCount += 1;
      return InstallAuditPlugin(context);
    };

    const extension = spy(pluginContext);

    assert.strictEqual(callCount, 1);
    assert.deepStrictEqual(extension, {
      AuditTag: "counter:audited"
    });
    assert.strictEqual(pluginContext.store.$state.PersistedAt, "plugin:counter");
});

Deno.test("generated pinia sample modules keeps setup-store client-only refs skipped from hydration while option-store hydrate receives seeded state", () => {
    setActivePinia(CreateConfiguredPinia());

    const setupStore = UseHydrationStore();
    const optionStore = UseHydrationOptionStore();

    assert.strictEqual(setupStore.CanHydrateClientOnlyNote(), false);
    assert.strictEqual(setupStore.ClientOnlyNote, "client-only note seeded in setup store");
    assert.strictEqual(optionStore.Count, 12);
    assert.strictEqual(optionStore.Status, "serialized SSR payload -> hydrate(storeState, initialState)");

    setupStore.RefreshClientOnlyNote();
    assert.strictEqual(
      setupStore.ClientOnlyNote,
      "client note refreshed at serialized SSR payload -> hydrate(storeState, initialState)"
    );
});

Deno.test("generated pinia sample modules supports named stubActions plus stubbed patch/reset in a stricter testing root", () => {
    setActivePinia(CreateStrictTestingRoot());

    const store = UseCounterStore();

    assert.strictEqual(store.Count, 15);
    assert.strictEqual(store.Status, "Seeded from strict createTestingPinia().");
    assert.strictEqual(store.$id, "counter");
    assert.strictEqual(store.$state.PersistedAt, "testing:counter:typed");

    store.Increment();
    store.Decrement();
    assert.strictEqual(store.Count, 15);
    assert.strictEqual(store.Status, "Seeded from strict createTestingPinia().");

    store.$patch({
      Count: 99,
      Status: "should be blocked"
    });
    assert.strictEqual(store.Count, 15);
    assert.strictEqual(store.Status, "Seeded from strict createTestingPinia().");

    store.$reset();
    assert.strictEqual(store.Count, 15);
    assert.strictEqual(store.Status, "Seeded from strict createTestingPinia().");
    assert.strictEqual(store.$state.PersistedAt, "testing:counter:typed");
});
