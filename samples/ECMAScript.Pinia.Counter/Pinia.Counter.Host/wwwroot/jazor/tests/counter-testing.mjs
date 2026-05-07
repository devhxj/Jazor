import { createTestingPinia } from "@pinia/testing";
export let typedTestingAuditPlugin = installTypedTestingAuditPlugin;
export function createTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { count: 9, status: "Seeded from createTestingPinia()." } },
    stubActions: shouldStubAction,
    writableComputed: true,
    stubPatch: false,
    stubReset: false,
    fakeApp: true,
    plugins: [typedTestingAuditPlugin],
    createSpy: wrapSpy
  });
}
export function createTypedTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { count: 12, status: "Seeded from combined typed createTestingPinia()." } },
    stubActions: shouldStubTypedAction,
    writableComputed: true,
    stubPatch: false,
    stubReset: false,
    fakeApp: true,
    plugins: [typedTestingAuditPlugin],
    createSpy: wrapActionSpy
  });
}
export function createFactoryTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { count: 18, status: "Seeded from combined typed factory createTestingPinia()." } },
    stubActions: shouldStubFactoryAction,
    writableComputed: true,
    stubPatch: false,
    stubReset: false,
    fakeApp: true,
    plugins: [typedTestingAuditPlugin],
    createSpy: wrapActionSpy
  });
}
export function createStrictTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { count: 15, status: "Seeded from strict createTestingPinia()." } },
    stubActions: ["increment", "decrement"],
    writableComputed: true,
    stubPatch: true,
    stubReset: true,
    fakeApp: true,
    plugins: [typedTestingAuditPlugin],
    createSpy: wrapSpy
  });
}
function shouldStubAction(actionName, store) {
  return actionName === "decrement" && store.$id === "counter";
}
function shouldStubTypedAction(actionName, store) {
  return actionName === "increment" && store.$id === "counter" && store.count >= 12;
}
function shouldStubFactoryAction(actionName, store) {
  return actionName === "decrement" && store.$id === "counter" && store.count >= 18;
}
function wrapSpy(callback) {
  return callback ?? noop;
}
function wrapActionSpy(callback) {
  return callback ?? noop;
}
function noop() { }
function installTypedTestingAuditPlugin(context) {
  let projectedStore = context.store;
  let customState = projectedStore.$state;
  let options = context.options;
  let increment = options.actions.increment;
  customState.persistedAt = "testing:" + projectedStore.$id + ":" + (increment === null ? "missing" : "typed");
  return { auditTag: projectedStore.$id + ":testing" };
}
//# sourceMappingURL=counter-testing.mjs.map
