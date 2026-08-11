import * as _4cdfede9 from "./vendor/pinia/3.0.4/dist/pinia.mjs";
import * as _37f6f364 from "./vendor/vue3/3.5.13/dist/vue.runtime.esm-browser.prod.js";
import * as _7e461eb3 from "./vendor/pinia-testing/1.0.3/dist/index.mjs";
const __m = { 0: (module, exports, require) => {
  module.exports = _4cdfede9.__esModule ? _4cdfede9 : Object.assign({}, _4cdfede9.default, _4cdfede9);
}, 1: (module, exports, require) => {
  const { defineStore: defineStore, storeToRefs: storeToRefs } = require(0);
  const CounterStoreId = "counter";
  const SeedCount = 2;
  let UseCounterStore = defineStore("counter", { state: CreateState, getters: { DoubleCount: ((__cb) => (function () {
    return __cb(this, ...arguments);
  }))(ReadDoubleCount) }, actions: { Increment: ((__cb) => (function () {
    return __cb(this, ...arguments);
  }))(Increment), Decrement: ((__cb) => (function () {
    return __cb(this, ...arguments);
  }))(Decrement) } });
  exports.UseCounterStore = UseCounterStore;
  let UseProjectedCounterStore = UseCounterStore;
  exports.UseProjectedCounterStore = UseProjectedCounterStore;
  function UseCounterStoreRefs(store) {
    return storeToRefs(store);
  }
  exports.UseCounterStoreRefs = UseCounterStoreRefs;
  function UseProjectedCounterStoreRefs(store) {
    return storeToRefs(store);
  }
  exports.UseProjectedCounterStoreRefs = UseProjectedCounterStoreRefs;
  function InstallAuditPlugin(context) {
    if (context.store.$id !== "counter") {
      return null;
    }
    let projectedStore = context.store;
    let customState = projectedStore.$state;
    customState.PersistedAt = "plugin:" + context.store.$id;
    return { AuditTag: context.store.$id + ":audited" };
  }
  exports.InstallAuditPlugin = InstallAuditPlugin;
  function CreateState() {
    return { Count: 2, Status: "Store seeded through defineStore()." };
  }
  function ReadDoubleCount(self) {
    return self.Count * 2;
  }
  function Increment(self) {
    self.Count += 1;
    self.Status = "increment() updated the store.";
  }
  function Decrement(self) {
    if (self.Count > 0) {
      self.Count -= 1;
      self.Status = "decrement() updated the store.";
      return;
    }
    self.Status = "decrement() is clamped at zero.";
  }
}, 2: (module, exports, require) => {
  module.exports = _37f6f364.__esModule ? _37f6f364 : Object.assign({}, _37f6f364.default, _37f6f364);
}, 3: (module, exports, require) => {
  const { UseCounterStore: UseCounterStore, UseCounterStoreRefs: UseCounterStoreRefs } = require(1);
  const { defineComponent: defineComponent, h: h } = require(2);
  let Component = defineComponent({ name: "PiniaCounterApp", setup: Setup });
  exports.Component = Component;
  function Setup() {
    let store = UseCounterStore();
    let refs = UseCounterStoreRefs(store);
    let patchPlusFive = () => {
      store.$patch({ Count: store.Count + 5, Status: "Applied $patch({ ... }) from the component." });
      return;
    };
    let resetStore = store.$reset.bind(store);
    return () => {
      return h("section", { class: "counter-shell" }, [h("p", { class: "counter-kicker" }, "ECMAScript.Pinia sample"), h("h1", { class: "counter-title" }, "Typed Pinia store authored in C#"), h("p", { class: "counter-copy" }, "The store comes from defineStore(), is resolved through StoreDefinition.Use(), and is read via storeToRefs()."), h("div", { class: "counter-grid" }, [CreateMetricCard("count", refs.Count.value, "metric-card metric-card--primary"), CreateMetricCard("doubleCount", refs.DoubleCount.value, "metric-card metric-card--secondary")]), h("p", { class: "counter-status" }, refs.Status.value), h("div", { class: "counter-actions" }, [CreateActionButton("Increment", "action-button action-button--accent", store.Increment.bind(store)), CreateActionButton("Decrement", "action-button", store.Decrement.bind(store)), CreateActionButton("Patch +5", "action-button", patchPlusFive), CreateActionButton("Reset", "action-button action-button--ghost", resetStore)]), h("ul", { class: "counter-notes" }, [h("li", "createPinia() stays a normal external runtime import."), h("li", "StoreDefinition<TStore>.Use() keeps the callable store factory explicit in C#."), h("li", "storeToRefs() returns typed refs for both state and getters.")])]);
    };
  }
  function CreateMetricCard(label, value, className) {
    return h("article", { class: className }, [h("span", { class: "metric-label" }, label), h("strong", { class: "metric-value" }, value)]);
  }
  function CreateActionButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
}, 4: (module, exports, require) => {
  const { mapActions: mapActions, mapState: mapState } = require(0);
  const { UseProjectedCounterStore: UseProjectedCounterStore, UseProjectedCounterStoreRefs: UseProjectedCounterStoreRefs } = require(1);
  const { defineComponent: defineComponent, h: h } = require(2);
  let Component = defineComponent({ name: "PiniaCounterCookbook", computed: CreateComputed(), methods: CreateMethods(), setup: Setup });
  exports.Component = Component;
  function CreateComputed() {
    return mapState(UseProjectedCounterStore, { Count: "Count", Status: "Status", DoubleCount: "DoubleCount", TripleCount: ReadTripleCount, AuditTag: "AuditTag" });
  }
  function CreateMethods() {
    return mapActions(UseProjectedCounterStore, ["Increment", "Decrement"]);
  }
  function Setup() {
    let projectedStore = UseProjectedCounterStore();
    let refs = UseProjectedCounterStoreRefs(projectedStore);
    let baseStore = projectedStore;
    let customState = projectedStore.$state;
    customState.PersistedAt = "component:" + baseStore.$id;
    return () => {
      return h("section", { class: "counter-cookbook-shell" }, [h("h2", "Projected plugin cookbook"), h("p", "Projected store definitions flow through storeToRefs(), Options API helpers, and direct custom-property/custom-state projections without inventing a separate runtime object."), h("ul", [h("li", "auditTag: " + projectedStore.AuditTag), h("li", "persistedAt: " + projectedStore.$state.PersistedAt), h("li", "countRef: " + refs["Count"].value), h("li", "statusRef: " + refs["Status"].value), h("li", "doubleCount: " + projectedStore.DoubleCount), h("li", "tripleCount: " + ReadTripleCount(projectedStore))]), h("div", { class: "counter-actions" }, [CreateActionButton("Projected increment", "action-button action-button--accent", baseStore.Increment.bind(baseStore)), CreateActionButton("Projected decrement", "action-button", baseStore.Decrement.bind(baseStore))]), h("p", { class: "counter-status" }, "Options API helpers are configured through CreateComputed()/CreateMethods(); the live card shows the projected store + projected refs path.")]);
    };
  }
  function CreateActionButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
  function ReadTripleCount(store) {
    return store.Count * 3;
  }
}, 5: (module, exports, require) => {
  const { acceptHMRUpdate: acceptHMRUpdate } = require(0);
  const { UseCounterStore: UseCounterStore, UseProjectedCounterStore: UseProjectedCounterStore } = require(1);
  const { defineComponent: defineComponent, h: h } = require(2);
  let Component = defineComponent({ name: "PiniaCounterHmrCookbook", setup: Setup });
  exports.Component = Component;
  function CreateCounterHotHandler(hot) {
    return acceptHMRUpdate(UseCounterStore, hot);
  }
  exports.CreateCounterHotHandler = CreateCounterHotHandler;
  function CreateProjectedCounterHotHandler(hot) {
    return acceptHMRUpdate(UseProjectedCounterStore, hot);
  }
  exports.CreateProjectedCounterHotHandler = CreateProjectedCounterHotHandler;
  function ResolveCounterStore(pinia, hot) {
    return UseCounterStore(pinia, hot);
  }
  exports.ResolveCounterStore = ResolveCounterStore;
  function ResolveProjectedCounterStore(pinia, hot) {
    return UseProjectedCounterStore(pinia, hot);
  }
  exports.ResolveProjectedCounterStore = ResolveProjectedCounterStore;
  function Setup() {
    let store = UseCounterStore();
    let projectedStore = UseProjectedCounterStore();
    let customState = projectedStore.$state;
    let installHotSnapshot = () => {
      customState.PersistedAt = "hmr:" + projectedStore.$id;
      return;
    };
    return () => {
      return h("section", { class: "counter-hmr-shell" }, [h("h2", "HMR cookbook"), h("p", "acceptHMRUpdate(useStore, hot) and storeDefinition.Use(pinia, hot) stay explicit in C# so Vite/Jolt hot-module wiring can remain a host concern instead of hidden compiler magic."), h("ul", { class: "counter-notes" }, [h("li", "store id: " + store.$id), h("li", "auditTag: " + projectedStore.AuditTag), h("li", "persistedAt: " + customState.PersistedAt), h("li", "consumer bridge calls import.meta.hot.accept(createCounterHotHandler(import.meta.hot))")]), h("div", { class: "counter-actions" }, [CreateActionButton("Prime HMR snapshot", "action-button action-button--accent", installHotSnapshot)])]);
    };
  }
  function CreateActionButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
}, 6: (module, exports, require) => {
  const { defineStore: defineStore, shouldHydrate: shouldHydrate, skipHydrate: skipHydrate } = require(0);
  const { computed: computed, defineComponent: defineComponent, h: h, ref: ref } = require(2);
  const HydrationStoreId = "counterHydration";
  exports.HydrationStoreId = HydrationStoreId;
  const HydrationOptionStoreId = "counterHydrationOptions";
  exports.HydrationOptionStoreId = HydrationOptionStoreId;
  let UseHydrationStore = defineStore("counterHydration", SetupHydrationStore);
  exports.UseHydrationStore = UseHydrationStore;
  let UseHydrationOptionStore = defineStore("counterHydrationOptions", { state: CreateHydrationState, hydrate: HydrateOptionStore });
  exports.UseHydrationOptionStore = UseHydrationOptionStore;
  let Component = defineComponent({ name: "PiniaCounterHydrationCookbook", setup: Setup });
  exports.Component = Component;
  function SeedInitialOptionStoreState(pinia) {
    pinia.state.value["counterHydrationOptions"] = { Count: 12, Status: "serialized SSR payload" };
  }
  exports.SeedInitialOptionStoreState = SeedInitialOptionStoreState;
  function SetupHydrationStore(helpers) {
    let clientOnlyNote = skipHydrate(ref("client-only note seeded in setup store"));
    return { Count: 4, Status: "setup-store hydration boundary is ready", ClientOnlyNote: clientOnlyNote, CanHydrateClientOnlyNote: helpers.action(() => {
      return shouldHydrate(clientOnlyNote);
    }, "canHydrateClientOnlyNote"), RefreshClientOnlyNote: helpers.action(() => {
      clientOnlyNote.value = "client note refreshed at " + UseHydrationOptionStore().Status;
      return;
    }, "refreshClientOnlyNote") };
  }
  function CreateHydrationState() {
    return { Count: 8, Status: "option-store hydration hook waiting" };
  }
  function HydrateOptionStore(storeState, initialState) {
    storeState.Count = initialState.Count;
    storeState.Status = initialState.Status + " -> hydrate(storeState, initialState)";
  }
  function Setup() {
    let setupStore = UseHydrationStore();
    let optionStore = UseHydrationOptionStore();
    let clientHydrates = computed(setupStore.CanHydrateClientOnlyNote);
    let reapplyClientNote = setupStore.RefreshClientOnlyNote;
    let hydrateSnapshot = () => {
      optionStore.$patch({ Status: "hydration snapshot captured from client action" });
      return;
    };
    return () => {
      return h("section", { class: "counter-hydration-shell" }, [h("h2", "Hydration cookbook"), h("p", "skipHydrate()/shouldHydrate() remain explicit setup-store authoring tools, while option-store hydrate(storeState, initialState) stays available for SSR/client boundary repair without hiding runtime semantics."), h("ul", { class: "counter-notes" }, [h("li", "setup store id: " + "counterHydration"), h("li", "option store id: " + optionStore.$id), h("li", "client-only note: " + setupStore.ClientOnlyNote), h("li", "should hydrate client-only note: " + clientHydrates.value), h("li", "option-store status: " + optionStore.Status)]), h("div", { class: "counter-actions" }, [CreateActionButton("Refresh client note", "action-button action-button--accent", reapplyClientNote), CreateActionButton("Hydration snapshot", "action-button", hydrateSnapshot)])]);
    };
  }
  function CreateActionButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
}, 7: (module, exports, require) => {
  const { defineStore: defineStore } = require(0);
  const ActivityStoreId = "activity";
  let UseActivityStore = defineStore("activity", { state: CreateState, getters: { Summary: ((__cb) => (function () {
    return __cb(this, ...arguments);
  }))(ReadSummary) }, actions: { Capture: ((__cb) => (function () {
    return __cb(this, ...arguments);
  }))(Capture), QueueReview: ((__cb) => (function () {
    return __cb(this, ...arguments);
  }))(QueueReview) } });
  exports.UseActivityStore = UseActivityStore;
  function CreateState() {
    return { CompletedActions: 1, PendingReviews: 2, Highlight: "Waiting for the next workflow capture." };
  }
  function ReadSummary(self) {
    return self.Highlight + " (done: " + self.CompletedActions + ", pending: " + self.PendingReviews + ")";
  }
  function Capture(self, source) {
    self.CompletedActions += 1;
    if (self.PendingReviews > 0) {
      self.PendingReviews -= 1;
    }
    self.Highlight = "capture(" + source + ") updated the activity store.";
  }
  function QueueReview(self) {
    self.PendingReviews += 1;
    self.Highlight = "queueReview() recorded another follow-up item.";
  }
}, 8: (module, exports, require) => {
  const { mapStores: mapStores, setMapStoreSuffix: setMapStoreSuffix } = require(0);
  const { UseActivityStore: UseActivityStore } = require(7);
  const { UseCounterStore: UseCounterStore } = require(1);
  const { computed: computed, defineComponent: defineComponent, h: h, ref: ref } = require(2);
  let OptionsApiSnapshot = ref("mapStores() snapshot will be captured after mount.");
  let OptionsApiStoreIds = ref("store ids pending");
  let Component = defineComponent({ name: "PiniaCounterMultiStore", computed: CreateComputed(), mounted: ((__cb) => (function () {
    return __cb(this, ...arguments);
  }))(CaptureMappedStores), setup: Setup });
  exports.Component = Component;
  function CreateComputed() {
    setMapStoreSuffix("");
    return mapStores(UseCounterStore, UseActivityStore);
  }
  function Setup() {
    let counter = UseCounterStore();
    let activity = UseActivityStore();
    let incrementAndCapture = () => {
      counter.Increment();
      activity.Capture("increment");
      return;
    };
    let queueReview = activity.QueueReview.bind(activity);
    let liveSummary = computed(() => {
      return counter.Status + " | " + activity.Summary;
    });
    let combinedScore = computed(() => {
      return counter.Count + activity.CompletedActions + activity.PendingReviews;
    });
    return () => {
      return h("section", { class: "counter-multi-store-shell" }, [h("h2", "Multi-store cookbook"), h("p", "SetMapStoreSuffix(\"\") keeps the component-instance fields aligned with store ids while mapStores() projects both stores through one Options API entry point."), h("div", { class: "counter-summary-grid" }, [CreateMetricCard("counter.count", counter.Count, "metric-card metric-card--primary"), CreateMetricCard("activity.done", activity.CompletedActions, "metric-card metric-card--secondary"), CreateMetricCard("activity.pending", activity.PendingReviews, "metric-card metric-card--neutral"), CreateMetricCard("combined", combinedScore.value, "metric-card metric-card--accent")]), h("p", { class: "counter-status" }, liveSummary.value), h("div", { class: "counter-actions" }, [CreateActionButton("Increment + capture", "action-button action-button--accent", incrementAndCapture), CreateActionButton("Queue review", "action-button", queueReview)]), h("ul", { class: "counter-notes" }, [h("li", "mounted snapshot via mapStores(): " + OptionsApiSnapshot.value), h("li", "mapped component store ids: " + OptionsApiStoreIds.value), h("li", "direct setup render keeps the live surface readable while Options API captures the heterogeneous store projection.")])]);
    };
  }
  function CaptureMappedStores(self) {
    OptionsApiSnapshot.value = self.counter.Status + " | " + self.activity.Summary;
    OptionsApiStoreIds.value = self.counter.$id + " + " + self.activity.$id;
  }
  function CreateMetricCard(label, value, className) {
    return h("article", { class: className }, [h("span", { class: "metric-label" }, label), h("strong", { class: "metric-value" }, value)]);
  }
  function CreateActionButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
}, 9: (module, exports, require) => {
  const { UseCounterStore: UseCounterStore } = require(1);
  const { defineComponent: defineComponent, h: h, onMounted: onMounted, onUnmounted: onUnmounted, ref: ref } = require(2);
  let Component = defineComponent({ name: "PiniaCounterSubscriptionCookbook", setup: Setup });
  exports.Component = Component;
  function Setup() {
    let store = UseCounterStore();
    let mutationKind = ref("No mutations observed yet.");
    let storeId = ref(store.$id);
    let statusSnapshot = ref(store.Status);
    let countSnapshot = ref(store.Count);
    let payloadSnapshot = ref("payload appears only for $patch({ ... }) mutations.");
    let eventShape = ref("Debugger events are dev-only and may be unavailable.");
    let notificationCount = ref(0);
    let detach = null;
    let handleMutation = (mutation, state) => {
      notificationCount.value += 1;
      mutationKind.value = DescribeMutationType(mutation.type);
      storeId.value = mutation.storeId;
      countSnapshot.value = state.Count;
      statusSnapshot.value = state.Status;
      payloadSnapshot.value = ReadMutationSummary(mutation);
      eventShape.value = DescribeEvents(mutation.events);
      return;
    };
    let applyDirectMutation = () => {
      store.Count += 1;
      store.Status = "Direct assignment updated the counter store.";
      return;
    };
    let applyObjectPatch = () => {
      store.$patch({ Count: store.Count + 3, Status: "Object patch updated the counter store." });
      return;
    };
    let applyFunctionPatch = () => {
      store.$patch((state) => {
        state.Count += 2;
        state.Status = "Function patch updated the counter store.";
        return;
      });
      return;
    };
    onMounted(() => {
      detach = store.$subscribe(handleMutation, { detached: true, flush: "sync" });
      return;
    });
    onUnmounted(() => {
      if (detach !== null) {
        detach();
      }
      return;
    });
    return () => {
      return h("section", { class: "counter-subscription-shell" }, [h("h2", "Subscription cookbook"), h("p", "$subscribe() is registered with detached + sync options so the sample can inspect direct mutations, object patches, and function patches from one stable callback. Direct assignments may report multiple sync notifications when several fields change back-to-back."), h("div", { class: "counter-actions" }, [CreateActionButton("Direct +1", "action-button action-button--accent", applyDirectMutation), CreateActionButton("Object patch", "action-button", applyObjectPatch), CreateActionButton("Function patch", "action-button", applyFunctionPatch)]), h("ul", { class: "counter-notes" }, [h("li", "mutation kind: " + mutationKind.value), h("li", "store id: " + storeId.value), h("li", "count snapshot: " + countSnapshot.value), h("li", "status snapshot: " + statusSnapshot.value), h("li", "payload summary: " + payloadSnapshot.value), h("li", "events shape: " + eventShape.value), h("li", "notifications seen: " + notificationCount.value)])]);
    };
  }
  function DescribeMutationType(type) {
    return (() => {
      const __swexpr$96f5886a201b4b0350026376 = type;
      if (__swexpr$96f5886a201b4b0350026376 === "direct")
        return "direct assignment";
      if (__swexpr$96f5886a201b4b0350026376 === "patch object")
        return "$patch({ ... }) object merge";
      if (__swexpr$96f5886a201b4b0350026376 === "patch function")
        return "$patch((state) => ...) callback";
      return "unknown mutation";
    })();
  }
  function ReadMutationSummary(mutation) {
    if (mutation.type === "patch object") {
      let patchMutation = mutation;
      let payload = patchMutation.payload;
      if (payload.Status !== null) {
        return "payload.status = " + payload.Status;
      }
      if (payload.Count !== null) {
        return "payload.count = " + payload.Count;
      }
      return "object patch payload captured without known fields";
    }
    if (mutation.type === "patch function") {
      return "function patch metadata captured without a payload object";
    }
    return "direct assignments do not expose a payload object";
  }
  function DescribeEvents(events) {
    return events === null ? "not provided" : "reported";
  }
  function CreateActionButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
}, 11: (module, exports, require) => {
  const { Component: Component } = require(3);
  const { Component: i$171bc28d43220ee7 } = require(4);
  const { Component: i$fff0fa1c3f1985a1 } = require(5);
  const { Component: i$28ac66ff45fa890b, SeedInitialOptionStoreState: SeedInitialOptionStoreState } = require(6);
  const { Component: i$c82170163c5ef2dd } = require(10);
  const { Component: i$e8238f960a1622b8 } = require(8);
  const { Component: i$02b3d5fcbbfb3568 } = require(9);
  const { createPinia: createPinia, disposePinia: disposePinia, setActivePinia: setActivePinia } = require(0);
  const { InstallAuditPlugin: InstallAuditPlugin } = require(1);
  const { createApp: createApp, defineComponent: defineComponent, h: h } = require(2);
  function CreateConfiguredApp() {
    let app = createApp(defineComponent({ name: "PiniaCounterRoot", render: RenderRoot }));
    let pinia = CreateConfiguredPinia();
    app.use(pinia);
    app.onUnmount(() => {
      disposePinia(pinia);
      return;
    });
    return app;
  }
  exports.CreateConfiguredApp = CreateConfiguredApp;
  function CreateConfiguredPinia() {
    let pinia = createPinia().use(InstallAuditPlugin);
    SeedInitialOptionStoreState(pinia);
    return pinia;
  }
  exports.CreateConfiguredPinia = CreateConfiguredPinia;
  function CreatePiniaInstallationApp(pinia) {
    let app = createApp(defineComponent({ name: "PiniaConfiguredRootShell", render: RenderPiniaInstallationShell }));
    app.use(pinia);
    return app;
  }
  exports.CreatePiniaInstallationApp = CreatePiniaInstallationApp;
  function ClearConfiguredActivePinia() {
    return setActivePinia(undefined);
  }
  exports.ClearConfiguredActivePinia = ClearConfiguredActivePinia;
  function Boot(selector) {
    let app = CreateConfiguredApp();
    app.mount(selector);
  }
  exports.Boot = Boot;
  function RenderPiniaInstallationShell() {
    return h("div");
  }
  function RenderRoot() {
    return h("main", { class: "counter-root" }, [h("section", { class: "counter-hero" }, [h("p", { class: "counter-kicker" }, "ECMAScript.Pinia production sample"), h("h1", { class: "counter-title" }, "Typed Pinia stores, projected plugins, multi-store helpers, subscriptions, and testing"), h("p", { class: "counter-copy" }, "The sample keeps Pinia as a normal external runtime while exercising authoring paths that matter in production code: defineStore(), storeToRefs(), plugin projections, mapStores(), $subscribe(), acceptHMRUpdate(), and createTestingPinia().")]), h("div", { class: "counter-stack" }, [h(Component), h(i$171bc28d43220ee7), h(i$e8238f960a1622b8), h(i$02b3d5fcbbfb3568), h(i$28ac66ff45fa890b), h(i$c82170163c5ef2dd), h(i$fff0fa1c3f1985a1)])]);
  }
}, 10: (module, exports, require) => {
  const { CreateConfiguredPinia: CreateConfiguredPinia, CreatePiniaInstallationApp: CreatePiniaInstallationApp } = require(11);
  const { disposePinia: disposePinia } = require(0);
  const { UseCounterStore: UseCounterStore, UseProjectedCounterStore: UseProjectedCounterStore } = require(1);
  const { defineComponent: defineComponent, h: h, onUnmounted: onUnmounted, ref: ref } = require(2);
  let Component = defineComponent({ name: "PiniaCounterIsolationCookbook", setup: Setup });
  exports.Component = Component;
  function CompareIsolatedRoots() {
    let leftPinia = CreateInstalledConfiguredPinia();
    let rightPinia = CreateInstalledConfiguredPinia();
    try {
      let leftStore = UseCounterStore(leftPinia);
      let leftProjected = UseProjectedCounterStore(leftPinia);
      let rightStore = UseCounterStore(rightPinia);
      let rightProjected = UseProjectedCounterStore(rightPinia);
      leftStore.Increment();
      leftProjected.$state.PersistedAt = "isolated:left:" + leftStore.$id;
      return leftStore.Count + "|" + rightStore.Count + "|" + leftProjected.AuditTag + "|" + rightProjected.AuditTag + "|" + leftProjected.$state.PersistedAt + "|" + rightProjected.$state.PersistedAt;
    } finally {
      disposePinia(leftPinia);
      disposePinia(rightPinia);
    }
  }
  exports.CompareIsolatedRoots = CompareIsolatedRoots;
  function Setup() {
    let leftPinia = CreateInstalledConfiguredPinia();
    let rightPinia = CreateInstalledConfiguredPinia();
    let leftStore = UseCounterStore(leftPinia);
    let leftProjected = UseProjectedCounterStore(leftPinia);
    let rightStore = UseCounterStore(rightPinia);
    let rightProjected = UseProjectedCounterStore(rightPinia);
    let snapshot = ref(DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected));
    let incrementLeftOnly = () => {
      leftStore.Increment();
      leftProjected.$state.PersistedAt = "isolated:left:" + leftStore.Count;
      snapshot.value = DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
      return;
    };
    let incrementRightOnly = () => {
      rightStore.Increment();
      rightProjected.$state.PersistedAt = "isolated:right:" + rightStore.Count;
      snapshot.value = DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected);
      return;
    };
    onUnmounted(() => {
      disposePinia(leftPinia);
      disposePinia(rightPinia);
      return;
    });
    return () => {
      return h("section", { class: "counter-isolation-shell" }, [h("h2", "Root isolation cookbook"), h("p", "Explicit StoreDefinition.Use(pinia) resolution keeps multiple Pinia roots isolated even when they reuse the same generated store definition and plugin projection contract."), h("ul", { class: "counter-notes" }, [h("li", "snapshot: " + snapshot.value), h("li", "left persistedAt: " + leftProjected.$state.PersistedAt), h("li", "right persistedAt: " + rightProjected.$state.PersistedAt)]), h("div", { class: "counter-actions" }, [CreateActionButton("Increment left root", "action-button action-button--accent", incrementLeftOnly), CreateActionButton("Increment right root", "action-button", incrementRightOnly)])]);
    };
  }
  function CreateInstalledConfiguredPinia() {
    let pinia = CreateConfiguredPinia();
    CreatePiniaInstallationApp(pinia);
    return pinia;
  }
  function DescribeSnapshot(leftStore, rightStore, leftProjected, rightProjected) {
    return "left=" + leftStore.Count + ", right=" + rightStore.Count + ", leftAudit=" + leftProjected.AuditTag + ", rightAudit=" + rightProjected.AuditTag;
  }
  function CreateActionButton(label, className, handler) {
    return h("button", { type: "button", class: className, onClick: handler }, label);
  }
}, 12: (module, exports, require) => {
  module.exports = _7e461eb3.__esModule ? _7e461eb3 : Object.assign({}, _7e461eb3.default, _7e461eb3);
}, 13: (module, exports, require) => {
  const { createTestingPinia: createTestingPinia } = require(12);
  let TypedTestingAuditPlugin = InstallTypedTestingAuditPlugin;
  exports.TypedTestingAuditPlugin = TypedTestingAuditPlugin;
  function CreateTestingRoot() {
    return createTestingPinia({ initialState: { counter: { Count: 9, Status: "Seeded from createTestingPinia()." } }, stubActions: ShouldStubAction, writableComputed: true, stubPatch: false, stubReset: false, fakeApp: true, plugins: [TypedTestingAuditPlugin], createSpy: WrapSpy });
  }
  exports.CreateTestingRoot = CreateTestingRoot;
  function CreateTypedTestingRoot() {
    return createTestingPinia({ initialState: { counter: { Count: 12, Status: "Seeded from combined typed createTestingPinia()." } }, stubActions: ShouldStubTypedAction, writableComputed: true, stubPatch: false, stubReset: false, fakeApp: true, plugins: [TypedTestingAuditPlugin], createSpy: WrapActionSpy });
  }
  exports.CreateTypedTestingRoot = CreateTypedTestingRoot;
  function CreateFactoryTestingRoot() {
    return createTestingPinia({ initialState: { counter: { Count: 18, Status: "Seeded from combined typed factory createTestingPinia()." } }, stubActions: ShouldStubFactoryAction, writableComputed: true, stubPatch: false, stubReset: false, fakeApp: true, plugins: [TypedTestingAuditPlugin], createSpy: WrapActionSpy });
  }
  exports.CreateFactoryTestingRoot = CreateFactoryTestingRoot;
  function CreateStrictTestingRoot() {
    return createTestingPinia({ initialState: { counter: { Count: 15, Status: "Seeded from strict createTestingPinia()." } }, stubActions: ["Increment", "Decrement"], writableComputed: true, stubPatch: true, stubReset: true, fakeApp: true, plugins: [TypedTestingAuditPlugin], createSpy: WrapSpy });
  }
  exports.CreateStrictTestingRoot = CreateStrictTestingRoot;
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
  function Noop() {}
  function InstallTypedTestingAuditPlugin(context) {
    let projectedStore = context.store;
    let customState = projectedStore.$state;
    let options = context.options;
    let increment = options.actions.Increment;
    customState.PersistedAt = "testing:" + projectedStore.$id + ":" + (increment === null ? "missing" : "typed");
    return { AuditTag: projectedStore.$id + ":testing" };
  }
}, 14: (module, exports, require) => {
  Object.assign(exports, require(3));
  Object.assign(exports, require(4));
  Object.assign(exports, require(5));
  Object.assign(exports, require(6));
  Object.assign(exports, require(10));
  Object.assign(exports, require(8));
  Object.assign(exports, require(9));
  Object.assign(exports, require(11));
  Object.assign(exports, require(7));
  Object.assign(exports, require(1));
  Object.assign(exports, require(13));
} };
var __c = {};
function __r(id) {
  var mod = __c[id];
  if (mod)
    return mod.exports;
  mod = __c[id] = { exports: {} };
  __m[id](mod, mod.exports, __r);
  var e = mod.exports;
  if (e && (typeof e == "object" || typeof e == "function") && e.default === void 0)
    e.default = e;
  return e;
}
export default __r(14);
//# sourceMappingURL=bundle.js.map
