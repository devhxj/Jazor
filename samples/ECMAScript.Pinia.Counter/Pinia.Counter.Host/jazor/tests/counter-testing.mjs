import { createTestingPinia } from "@pinia/testing";
export let TypedTestingAuditPlugin = InstallTypedTestingAuditPlugin;
export function CreateTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { Count: 9, Status: "Seeded from createTestingPinia()." } },
    stubActions: ShouldStubAction,
    stubPatch: false,
    stubReset: false,
    fakeApp: true,
    plugins: [TypedTestingAuditPlugin],
    createSpy: WrapSpy
  });
}
export function CreateTypedTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { Count: 12, Status: "Seeded from combined typed createTestingPinia()." } },
    stubActions: ShouldStubTypedAction,
    stubPatch: false,
    stubReset: false,
    fakeApp: true,
    plugins: [TypedTestingAuditPlugin],
    createSpy: WrapActionSpy
  });
}
export function CreateFactoryTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { Count: 18, Status: "Seeded from combined typed factory createTestingPinia()." } },
    stubActions: ShouldStubFactoryAction,
    stubPatch: false,
    stubReset: false,
    fakeApp: true,
    plugins: [TypedTestingAuditPlugin],
    createSpy: WrapActionSpy
  });
}
export function CreateStrictTestingRoot() {
  return createTestingPinia({
    initialState: { counter: { Count: 15, Status: "Seeded from strict createTestingPinia()." } },
    stubActions: ["Increment", "Decrement"],
    stubPatch: true,
    stubReset: true,
    fakeApp: true,
    plugins: [TypedTestingAuditPlugin],
    createSpy: WrapSpy
  });
}
function ShouldStubAction(actionName, store) {
  return actionName === "Decrement" && store.$id === "counter";
}
function ShouldStubTypedAction(actionName, store) {
  return actionName === "Increment" && store.$id === "counter" && store.Count >= 12;
}
function ShouldStubFactoryAction(actionName, store) {
  return actionName === "Decrement" && store.$id === "counter" && store.Count >= 18;
}
function WrapSpy(callback) {
  return callback ?? Noop;
}
function WrapActionSpy(callback) {
  return callback ?? Noop;
}
function Noop() { }
function InstallTypedTestingAuditPlugin(context) {
  let projectedStore = context.store;
  let customState = projectedStore.$state;
  let options = context.options;
  let increment = options.actions.Increment;
  customState.PersistedAt = "testing:" + projectedStore.$id + ":" + (increment === null ? "missing" : "typed");
  return { AuditTag: projectedStore.$id + ":testing" };
}
//# sourceMappingURL=counter-testing.mjs.map
