import { afterEach, describe, expect, it, vi } from "vitest";
import { disposePinia, getActivePinia } from "pinia";
import { createHmrBridge } from "./pinia.hmr-bridge.js";
import { createManagedPiniaRoot } from "./pinia.root-lifecycle.js";
import { clearConfiguredActivePinia } from "../../Pinia.Counter.Host/wwwroot/jazor/host/app.mjs";
import {
  createFactoryTestingRoot,
  createTypedTestingRoot,
  createStrictTestingRoot,
  createTestingRoot
} from "../../Pinia.Counter.Host/wwwroot/jazor/tests/counter-testing.mjs";
import {
  useCounterStore,
  useProjectedCounterStore
} from "../../Pinia.Counter.Host/wwwroot/jazor/stores/counter-store.mjs";
import { compareIsolatedRoots } from "../../Pinia.Counter.Host/wwwroot/jazor/components/counter-isolation.mjs";

describe("pinia runtime integration seams", () => {
  const ephemeralPiniaRoots = [];

  afterEach(() => {
    clearConfiguredActivePinia();

    while (ephemeralPiniaRoots.length > 0) {
      disposePinia(ephemeralPiniaRoots.pop());
    }
  });

  it("registers both generated HMR accept handlers through the host bridge", () => {
    const accept = vi.fn();
    const bridge = createHmrBridge({ accept });

    expect(bridge.register()).toBe(true);
    expect(accept).toHaveBeenCalledTimes(2);
    expect(typeof accept.mock.calls[0][0]).toBe("function");
    expect(typeof accept.mock.calls[1][0]).toBe("function");
  });

  it("creates, activates, and disposes a configured pinia root", () => {
    const managed = createManagedPiniaRoot();
    const projectedStore = useProjectedCounterStore(managed.pinia);

    expect(managed.app).toBeDefined();
    expect(managed.activePinia).toBe(managed.pinia);
    expect(getActivePinia()).toBe(managed.pinia);
    expect(Object.keys(managed.pinia.state.value)).toContain("counterHydrationOptions");
    expect(projectedStore.auditTag).toBe("counter:audited");
    expect(projectedStore.$state.persistedAt).toBe("plugin:counter");

    managed.dispose();

    expect(managed.pinia.state.value).toEqual({});
    expect(getActivePinia()).toBeUndefined();
  });

  it("clears the active root through the generated host lifecycle helper", () => {
    const managed = createManagedPiniaRoot();

    expect(getActivePinia()).toBe(managed.pinia);

    const cleared = clearConfiguredActivePinia();

    expect(cleared).toBeUndefined();
    expect(getActivePinia()).toBeUndefined();

    managed.dispose();
  });

  it("keeps the testing root fake app and plugin ordering observable at runtime", () => {
    const testingPinia = createTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);

    expect(testingPinia.app).toBeDefined();
    expect(typeof testingPinia.app.use).toBe("function");

    const projectedStore = useProjectedCounterStore(testingPinia);

    expect(projectedStore.auditTag).toBe("counter:testing");
    expect(projectedStore.$state.persistedAt).toBe("testing:counter:typed");
  });

  it("keeps the combined typed testing root observable at runtime", () => {
    const testingPinia = createTypedTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = useProjectedCounterStore(testingPinia);
    const store = useCounterStore(testingPinia);

    expect(testingPinia.app).toBeDefined();
    expect(projectedStore.auditTag).toBe("counter:testing");
    expect(projectedStore.$state.persistedAt).toBe("testing:counter:typed");
    expect(store.count).toBe(12);

    store.increment();
    expect(store.count).toBe(12);

    store.decrement();
    expect(store.count).toBe(11);
    expect(store.status).toBe("decrement() updated the store.");
  });

  it("keeps the combined typed factory testing root observable at runtime", () => {
    const testingPinia = createFactoryTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = useProjectedCounterStore(testingPinia);
    const store = useCounterStore(testingPinia);

    expect(testingPinia.app).toBeDefined();
    expect(projectedStore.auditTag).toBe("counter:testing");
    expect(projectedStore.$state.persistedAt).toBe("testing:counter:typed");
    expect(store.count).toBe(18);

    store.increment();
    expect(store.count).toBe(19);

    store.decrement();
    expect(store.count).toBe(19);
    expect(store.status).toBe("increment() updated the store.");
  });

  it("disposes an individual store scope without deleting the retained pinia state snapshot", () => {
    const managed = createManagedPiniaRoot();
    const store = useCounterStore(managed.pinia);

    store.increment();
    expect(store.count).toBe(3);

    store.$dispose();

    expect(managed.pinia.state.value.counter.count).toBe(3);
    expect(managed.pinia.state.value.counter.status).toBe("increment() updated the store.");

    managed.dispose();
  });

  it("can recreate a fresh pinia root after disposing the previous one", () => {
    const first = createManagedPiniaRoot();
    first.dispose();

    const second = createManagedPiniaRoot();

    expect(second.pinia).not.toBe(first.pinia);
    expect(getActivePinia()).toBe(second.pinia);
    expect(Object.keys(second.pinia.state.value)).toContain("counterHydrationOptions");

    second.dispose();
  });

  it("restores the previous active root when managed roots are disposed in stack order", () => {
    const first = createManagedPiniaRoot();
    const second = createManagedPiniaRoot();

    expect(getActivePinia()).toBe(second.pinia);

    second.dispose();

    expect(getActivePinia()).toBe(first.pinia);

    first.dispose();

    expect(getActivePinia()).toBeUndefined();
  });

  it("never reactivates a previously disposed root when managed roots are torn down out of order", () => {
    const first = createManagedPiniaRoot();
    const second = createManagedPiniaRoot();

    first.dispose();

    expect(getActivePinia()).toBe(second.pinia);

    second.dispose();

    expect(getActivePinia()).toBeUndefined();
  });

  it("keeps named action stubs and patch/reset stubs active in the strict testing root", () => {
    const testingPinia = createStrictTestingRoot();
    ephemeralPiniaRoots.push(testingPinia);
    const projectedStore = useProjectedCounterStore(testingPinia);
    const store = useCounterStore(testingPinia);

    expect(testingPinia.app).toBeDefined();
    expect(projectedStore.auditTag).toBe("counter:testing");
    expect(projectedStore.$state.persistedAt).toBe("testing:counter:typed");
    expect(store.count).toBe(15);

    store.increment();
    store.$patch({ count: 200 });
    store.$reset();

    expect(store.count).toBe(15);
    expect(store.status).toBe("Seeded from strict createTestingPinia().");
    expect(store.$state.persistedAt).toBe("testing:counter:typed");
  });

  it("keeps explicit multi-root store resolution isolated across two configured pinia roots", () => {
    const snapshot = compareIsolatedRoots();

    expect(snapshot).toBe("3|2|counter:audited|counter:audited|isolated:left:counter|plugin:counter");
  });
});
