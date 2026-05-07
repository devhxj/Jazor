import { afterEach, describe, expect, it } from "vitest";
import { nextTick } from "vue";
import { createConfiguredApp } from "host/app.mjs";

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
  await new Promise(resolve => setTimeout(resolve, 0));
  await flushDom();
}

function clickButton(root, label) {
  const button = Array.from(root.querySelectorAll("button")).find(candidate => candidate.textContent?.trim() === label);
  expect(button).toBeDefined();
  button.click();
}

describe("generated vue route app dom", () => {
  const mountedApps = [];

  afterEach(() => {
    while (mountedApps.length > 0) {
      mountedApps.pop().unmount();
    }

    document.body.innerHTML = "";
  });

  it("mounts the generated router root with RouterLink and RouterView", async () => {
    const host = createHost();
    const app = createConfiguredApp();
    mountedApps.push(app);

    app.mount(host);
    await flushRouteDom();

    expect(host.querySelector(".route-root")).not.toBeNull();
    expect(host.textContent).toContain("Typed Vue Router authoring");
    expect(host.textContent).toContain("Home");
    expect(host.textContent).toContain("Detail 5");
    expect(host.textContent).toContain("current name: home");
    expect(host.textContent).toContain("Go detail");
  });

  it("reacts through generated route navigation and component guards", async () => {
    const host = createHost();
    const app = createConfiguredApp();
    mountedApps.push(app);

    app.mount(host);
    await flushRouteDom();

    clickButton(host, "Go detail");
    await flushRouteDom();

    expect(host.textContent).toContain("Detail 7");
    expect(host.textContent).toContain("source: route-props");
    expect(host.textContent).toContain("query via: button");
    expect(host.textContent).toContain("useLink href:");
    expect(host.textContent).toContain("matched path: /users/:id");
    expect(host.textContent).toContain("injected route path: /users/7");

    clickButton(host, "Blocked target");
    await flushRouteDom();

    expect(host.textContent).toContain("global guard: beforeEach:blocked");

    clickButton(host, "Follow composed link");
    await flushRouteDom();

    expect(host.textContent).toContain("Query");
    expect(host.textContent).toContain("tab prop: 7-details");
  });
});
