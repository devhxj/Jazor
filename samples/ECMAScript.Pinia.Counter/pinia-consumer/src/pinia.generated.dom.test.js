import assert from "node:assert/strict";
import { installDomEnvironment } from "./test-dom.js";

const disposeDomEnvironment = installDomEnvironment();
const { nextTick } = await import("vue");
const { createConfiguredApp } = await import("host/app.mjs");

addEventListener("unload", () => {
  disposeDomEnvironment();
});

function createHost() {
  const host = document.createElement("div");
  document.body.appendChild(host);
  return host;
}

function clickButton(root, label) {
  const button = Array.from(root.querySelectorAll("button")).find(candidate => candidate.textContent?.trim() === label);
  assert.ok(button);
  button.click();
}

async function flushDom() {
  await nextTick();
  await Promise.resolve();
  await nextTick();
}

async function withMountedApps(callback) {
  const mountedApps = [];

  try {
    await callback(mountedApps);
  } finally {
    while (mountedApps.length > 0) {
      mountedApps.pop().unmount();
    }

    document.body.innerHTML = "";
  }
}

Deno.test("generated pinia app DOM mounts the generated root with projected plugin, multi-store, subscription, and hmr cookbook cards", async () => {
  await withMountedApps(async (mountedApps) => {
    const host = createHost();
    const app = createConfiguredApp();

    app.mount(host);
    mountedApps.push(app);
    await flushDom();

    assert.notStrictEqual(host.querySelector(".counter-root"), null);
    assert.match(host.textContent ?? "", /Projected plugin cookbook/);
    assert.match(host.textContent ?? "", /Multi-store cookbook/);
    assert.match(host.textContent ?? "", /Subscription cookbook/);
    assert.match(host.textContent ?? "", /Hydration cookbook/);
    assert.match(host.textContent ?? "", /Root isolation cookbook/);
    assert.match(host.textContent ?? "", /HMR cookbook/);
    assert.match(host.textContent ?? "", /auditTag: counter:audited/);
    assert.match(host.textContent ?? "", /persistedAt: component:counter/);
    assert.match(host.textContent ?? "", /doubleCount: 4/);
    assert.match(host.textContent ?? "", /tripleCount: 6/);
    assert.match(host.textContent ?? "", /mapped component store ids: counter \+ activity/);
    assert.match(host.textContent ?? "", /should hydrate client-only note: false/);
    assert.match(host.textContent ?? "", /option-store status: serialized SSR payload -> hydrate\(storeState, initialState\)/);
    assert.match(host.textContent ?? "", /left persistedAt: plugin:counter/);
    assert.match(host.textContent ?? "", /right persistedAt: plugin:counter/);
  });
});

Deno.test("generated pinia app DOM reacts through the generated multi-store and subscription interactions", async () => {
  await withMountedApps(async (mountedApps) => {
    const host = createHost();
    const app = createConfiguredApp();

    app.mount(host);
    mountedApps.push(app);
    await flushDom();

    clickButton(host, "Increment + capture");
    await flushDom();

    assert.match(host.textContent ?? "", /capture\(increment\) updated the activity store\./);
    assert.match(host.textContent ?? "", /mutation kind: direct assignment/);
    assert.match(host.textContent ?? "", /count snapshot: 3/);
    assert.match(host.textContent ?? "", /status snapshot: increment\(\) updated the store\./);
    assert.match(host.textContent ?? "", /notifications seen: [1-9]\d*/);

    clickButton(host, "Object patch");
    await flushDom();

    assert.match(host.textContent ?? "", /mutation kind: \$patch\(\{ \.\.\. \}\) object merge/);
    assert.match(host.textContent ?? "", /payload summary: payload\.status = Object patch updated the counter store\./);
    assert.match(host.textContent ?? "", /doubleCount: 12/);
    assert.match(host.textContent ?? "", /tripleCount: 18/);
    assert.match(host.textContent ?? "", /notifications seen: [2-9]\d*/);

    clickButton(host, "Prime HMR snapshot");
    await flushDom();

    assert.match(host.textContent ?? "", /persistedAt: hmr:counter/);

    clickButton(host, "Refresh client note");
    await flushDom();

    assert.match(host.textContent ?? "", /client note refreshed at serialized SSR payload -> hydrate\(storeState, initialState\)/);

    clickButton(host, "Increment left root");
    await flushDom();

    assert.match(host.textContent ?? "", /snapshot: left=3, right=2, leftAudit=counter:audited, rightAudit=counter:audited/);
    assert.match(host.textContent ?? "", /left persistedAt: isolated:left:3/);
    assert.match(host.textContent ?? "", /right persistedAt: plugin:counter/);

    clickButton(host, "Increment right root");
    await flushDom();

    assert.match(host.textContent ?? "", /snapshot: left=3, right=3, leftAudit=counter:audited, rightAudit=counter:audited/);
    assert.match(host.textContent ?? "", /left persistedAt: isolated:left:3/);
    assert.match(host.textContent ?? "", /right persistedAt: isolated:right:3/);
  });
});

Deno.test("generated pinia app DOM can remount a fresh generated root after unmount cleanup", async () => {
  await withMountedApps(async (mountedApps) => {
    const firstHost = createHost();
    const firstApp = createConfiguredApp();
    const firstPinia = firstApp.config.globalProperties.$pinia;

    firstApp.mount(firstHost);
    mountedApps.push(firstApp);
    await flushDom();

    clickButton(firstHost, "Increment");
    await flushDom();
    assert.match(firstHost.textContent ?? "", /count3/);

    firstApp.unmount();
    await flushDom();
    assert.deepStrictEqual(firstPinia.state.value, {});
    mountedApps.pop();
    firstHost.remove();

    const secondHost = createHost();
    const secondApp = createConfiguredApp();
    mountedApps.push(secondApp);

    secondApp.mount(secondHost);
    await flushDom();

    assert.match(secondHost.textContent ?? "", /count2/);
    assert.match(secondHost.textContent ?? "", /No mutations observed yet\./);
    assert.match(secondHost.textContent ?? "", /persistedAt: component:counter/);
    assert.match(secondHost.textContent ?? "", /snapshot: left=2, right=2, leftAudit=counter:audited, rightAudit=counter:audited/);
    assert.match(secondHost.textContent ?? "", /left persistedAt: plugin:counter/);
    assert.match(secondHost.textContent ?? "", /right persistedAt: plugin:counter/);
  });
});
