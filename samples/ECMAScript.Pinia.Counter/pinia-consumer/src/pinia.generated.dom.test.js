import { afterEach, describe, expect, it } from "vitest";
import { nextTick } from "vue";
import { createConfiguredApp } from "../../Pinia.Counter.Host/wwwroot/jazor/host/app.mjs";

function createHost() {
  const host = document.createElement("div");
  document.body.appendChild(host);
  return host;
}

function clickButton(root, label) {
  const button = Array.from(root.querySelectorAll("button")).find(candidate => candidate.textContent?.trim() === label);
  expect(button).toBeDefined();
  button.click();
}

async function flushDom() {
  await nextTick();
  await Promise.resolve();
  await nextTick();
}

describe("generated pinia app DOM", () => {
  const mountedApps = [];

  afterEach(() => {
    while (mountedApps.length > 0) {
      mountedApps.pop().unmount();
    }

    document.body.innerHTML = "";
  });

  it("mounts the generated root with projected plugin, multi-store, subscription, and hmr cookbook cards", async () => {
    const host = createHost();
    const app = createConfiguredApp();
    mountedApps.push(app);

    app.mount(host);
    await flushDom();

    expect(host.querySelector(".counter-root")).not.toBeNull();
    expect(host.textContent).toContain("Projected plugin cookbook");
    expect(host.textContent).toContain("Multi-store cookbook");
    expect(host.textContent).toContain("Subscription cookbook");
    expect(host.textContent).toContain("Hydration cookbook");
    expect(host.textContent).toContain("Root isolation cookbook");
    expect(host.textContent).toContain("HMR cookbook");
    expect(host.textContent).toContain("auditTag: counter:audited");
    expect(host.textContent).toContain("persistedAt: component:counter");
    expect(host.textContent).toContain("doubleCount: 4");
    expect(host.textContent).toContain("tripleCount: 6");
    expect(host.textContent).toContain("mapped component store ids: counter + activity");
    expect(host.textContent).toContain("should hydrate client-only note: false");
    expect(host.textContent).toContain("option-store status: serialized SSR payload -> hydrate(storeState, initialState)");
    expect(host.textContent).toContain("left persistedAt: plugin:counter");
    expect(host.textContent).toContain("right persistedAt: plugin:counter");
  });

  it("reacts through the generated multi-store and subscription interactions", async () => {
    const host = createHost();
    const app = createConfiguredApp();
    mountedApps.push(app);

    app.mount(host);
    await flushDom();

    clickButton(host, "Increment + capture");
    await flushDom();

    expect(host.textContent).toContain("capture(increment) updated the activity store.");
    expect(host.textContent).toContain("mutation kind: direct assignment");
    expect(host.textContent).toContain("count snapshot: 3");
    expect(host.textContent).toContain("status snapshot: increment() updated the store.");
    expect(host.textContent).toMatch(/notifications seen: [1-9]\d*/);

    clickButton(host, "Object patch");
    await flushDom();

    expect(host.textContent).toContain("mutation kind: $patch({ ... }) object merge");
    expect(host.textContent).toContain("payload summary: payload.status = Object patch updated the counter store.");
    expect(host.textContent).toContain("doubleCount: 12");
    expect(host.textContent).toContain("tripleCount: 18");
    expect(host.textContent).toMatch(/notifications seen: [2-9]\d*/);

    clickButton(host, "Prime HMR snapshot");
    await flushDom();

    expect(host.textContent).toContain("persistedAt: hmr:counter");

    clickButton(host, "Refresh client note");
    await flushDom();

    expect(host.textContent).toContain("client note refreshed at serialized SSR payload -> hydrate(storeState, initialState)");

    clickButton(host, "Increment left root");
    await flushDom();

    expect(host.textContent).toContain("snapshot: left=3, right=2, leftAudit=counter:audited, rightAudit=counter:audited");
    expect(host.textContent).toContain("left persistedAt: isolated:left:3");
    expect(host.textContent).toContain("right persistedAt: plugin:counter");

    clickButton(host, "Increment right root");
    await flushDom();

    expect(host.textContent).toContain("snapshot: left=3, right=3, leftAudit=counter:audited, rightAudit=counter:audited");
    expect(host.textContent).toContain("left persistedAt: isolated:left:3");
    expect(host.textContent).toContain("right persistedAt: isolated:right:3");
  });

  it("can remount a fresh generated root after unmount cleanup", async () => {
    const firstHost = createHost();
    const firstApp = createConfiguredApp();
    const firstPinia = firstApp.config.globalProperties.$pinia;
    mountedApps.push(firstApp);

    firstApp.mount(firstHost);
    await flushDom();

    clickButton(firstHost, "Increment");
    await flushDom();
    expect(firstHost.textContent).toContain("count3");

    firstApp.unmount();
    await flushDom();
    expect(firstPinia.state.value).toEqual({});
    mountedApps.pop();
    firstHost.remove();

    const secondHost = createHost();
    const secondApp = createConfiguredApp();
    mountedApps.push(secondApp);

    secondApp.mount(secondHost);
    await flushDom();

    expect(secondHost.textContent).toContain("count2");
    expect(secondHost.textContent).toContain("No mutations observed yet.");
    expect(secondHost.textContent).toContain("persistedAt: component:counter");
    expect(secondHost.textContent).toContain("snapshot: left=2, right=2, leftAudit=counter:audited, rightAudit=counter:audited");
    expect(secondHost.textContent).toContain("left persistedAt: plugin:counter");
    expect(secondHost.textContent).toContain("right persistedAt: plugin:counter");
  });
});
