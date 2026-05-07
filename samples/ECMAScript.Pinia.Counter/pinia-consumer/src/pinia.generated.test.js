import { beforeEach, describe, expect, it, vi } from "vitest";
import { setActivePinia } from "pinia";
import {
  createFactoryTestingRoot,
  createTypedTestingRoot,
  createStrictTestingRoot,
  createTestingRoot
} from "../../Pinia.Counter.Host/wwwroot/jazor/tests/counter-testing.mjs";
import { createConfiguredPinia } from "../../Pinia.Counter.Host/wwwroot/jazor/host/app.mjs";
import { useCounterStore, installAuditPlugin } from "../../Pinia.Counter.Host/wwwroot/jazor/stores/counter-store.mjs";
import {
  useHydrationOptionStore,
  useHydrationStore
} from "../../Pinia.Counter.Host/wwwroot/jazor/components/counter-hydration.mjs";

describe("generated pinia sample modules", () => {
  beforeEach(() => {
    setActivePinia(createTestingRoot());
  });

  it("seeds counter state through createTestingPinia()", () => {
    const store = useCounterStore();

    expect(store.count).toBe(9);
    expect(store.status).toBe("Seeded from createTestingPinia().");
    expect(store.$state.persistedAt).toBe("testing:counter:typed");
  });

  it("keeps increment live while decrement is stubbed by predicate", () => {
    const store = useCounterStore();

    store.increment();
    expect(store.count).toBe(10);
    expect(store.status).toBe("increment() updated the store.");

    store.decrement();
    expect(store.count).toBe(10);
    expect(store.status).toBe("increment() updated the store.");
  });

  it("allows real patch/reset because the testing root disabled those stubs", () => {
    const store = useCounterStore();

    store.$patch({
      count: 20,
      status: "patched from vitest"
    });
    expect(store.count).toBe(20);
    expect(store.status).toBe("patched from vitest");

    store.$reset();
    expect(store.count).toBe(2);
    expect(store.status).toBe("Store seeded through defineStore().");
  });

  it("supports combined typed testing options while keeping runtime shape unchanged", () => {
    setActivePinia(createTypedTestingRoot());

    const store = useCounterStore();

    expect(store.count).toBe(12);
    expect(store.status).toBe("Seeded from combined typed createTestingPinia().");
    expect(store.$state.persistedAt).toBe("testing:counter:typed");

    store.increment();
    expect(store.count).toBe(12);
    expect(store.status).toBe("Seeded from combined typed createTestingPinia().");

    store.decrement();
    expect(store.count).toBe(11);
    expect(store.status).toBe("decrement() updated the store.");

    store.$patch({
      count: 21,
      status: "typed root patch"
    });
    expect(store.count).toBe(21);
    expect(store.status).toBe("typed root patch");
  });

  it("supports combined typed testing options through the explicit union factory path", () => {
    setActivePinia(createFactoryTestingRoot());

    const store = useCounterStore();

    expect(store.count).toBe(18);
    expect(store.status).toBe("Seeded from combined typed factory createTestingPinia().");
    expect(store.$state.persistedAt).toBe("testing:counter:typed");

    store.increment();
    expect(store.count).toBe(19);
    expect(store.status).toBe("increment() updated the store.");

    store.decrement();
    expect(store.count).toBe(19);
    expect(store.status).toBe("increment() updated the store.");
  });

  it("can wrap the generated plugin callback with a JS spy", () => {
    const pluginContext = {
      store: {
        $id: "counter",
        $state: {
          persistedAt: ""
        }
      }
    };
    const spy = vi.fn(installAuditPlugin);

    const extension = spy(pluginContext);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(extension).toEqual({
      auditTag: "counter:audited"
    });
    expect(pluginContext.store.$state.persistedAt).toBe("plugin:counter");
  });

  it("keeps setup-store client-only refs skipped from hydration while option-store hydrate receives seeded state", () => {
    setActivePinia(createConfiguredPinia());

    const setupStore = useHydrationStore();
    const optionStore = useHydrationOptionStore();

    expect(setupStore.canHydrateClientOnlyNote()).toBe(false);
    expect(setupStore.clientOnlyNote).toBe("client-only note seeded in setup store");
    expect(optionStore.count).toBe(12);
    expect(optionStore.status).toBe("serialized SSR payload -> hydrate(storeState, initialState)");

    setupStore.refreshClientOnlyNote();
    expect(setupStore.clientOnlyNote).toBe("client note refreshed at serialized SSR payload -> hydrate(storeState, initialState)");
  });

  it("supports named stubActions plus stubbed patch/reset in a stricter testing root", () => {
    setActivePinia(createStrictTestingRoot());

    const store = useCounterStore();

    expect(store.count).toBe(15);
    expect(store.status).toBe("Seeded from strict createTestingPinia().");
    expect(store.$id).toBe("counter");
    expect(store.$state.persistedAt).toBe("testing:counter:typed");

    store.increment();
    store.decrement();
    expect(store.count).toBe(15);
    expect(store.status).toBe("Seeded from strict createTestingPinia().");

    store.$patch({
      count: 99,
      status: "should be blocked"
    });
    expect(store.count).toBe(15);
    expect(store.status).toBe("Seeded from strict createTestingPinia().");

    store.$reset();
    expect(store.count).toBe(15);
    expect(store.status).toBe("Seeded from strict createTestingPinia().");
    expect(store.$state.persistedAt).toBe("testing:counter:typed");
  });
});
