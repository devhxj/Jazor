import assert from "node:assert/strict";
import { installDomEnvironment } from "./test-dom.js";

const disposeDomEnvironment = installDomEnvironment();
const { nextTick } = await import("vue");
const { CreateConfiguredApp } = await import("host/app.mjs");

addEventListener("unload", () => {
  disposeDomEnvironment();
});

function createHost() {
  const host = document.createElement("div");
  document.body.appendChild(host);
  return host;
}

async function flushDom() {
  await nextTick();
  await Promise.resolve();
  await nextTick();
}

async function flushRouteDom() {
  await flushDom();
  await new Promise((resolve) => setTimeout(resolve, 0));
  await flushDom();
}

function clickButton(root, label) {
  const button = Array.from(root.querySelectorAll("button")).find((candidate) => candidate.textContent?.trim() === label);
  assert.ok(button);
  button.click();
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

Deno.test("generated vue route app DOM mounts the generated router root with RouterLink and RouterView", async () => {
  await withMountedApps(async (mountedApps) => {
    const host = createHost();
    const app = CreateConfiguredApp();
    mountedApps.push(app);

    app.mount(host);
    await flushRouteDom();

    assert.notStrictEqual(host.querySelector(".route-root"), null);
    assert.match(host.textContent ?? "", /Typed Vue Router authoring/);
    assert.match(host.textContent ?? "", /Home/);
    assert.match(host.textContent ?? "", /Detail 5/);
    assert.match(host.textContent ?? "", /current name: home/);
    assert.match(host.textContent ?? "", /Go detail/);
  });
});

Deno.test("generated vue route app DOM reacts through generated route navigation and component guards", async () => {
  await withMountedApps(async (mountedApps) => {
    const host = createHost();
    const app = CreateConfiguredApp();
    mountedApps.push(app);

    app.mount(host);
    await flushRouteDom();

    clickButton(host, "Go detail");
    await flushRouteDom();

    assert.match(host.textContent ?? "", /Detail 7/);
    assert.match(host.textContent ?? "", /source: route-props/);
    assert.match(host.textContent ?? "", /query via: button/);
    assert.match(host.textContent ?? "", /useLink href:/);
    assert.match(host.textContent ?? "", /matched path: \/users\/:id/);
    assert.match(host.textContent ?? "", /injected route path: \/users\/7/);

    clickButton(host, "Blocked target");
    await flushRouteDom();

    assert.match(host.textContent ?? "", /global guard: beforeEach:blocked/);

    clickButton(host, "Follow composed link");
    await flushRouteDom();

    assert.match(host.textContent ?? "", /Query/);
    assert.match(host.textContent ?? "", /tab prop: 7-details/);
  });
});
