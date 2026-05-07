import { computed, defineComponent, h, ref } from "npm:vue@3";
import { defineStore, shouldHydrate, skipHydrate } from "pinia";
export const hydrationStoreId = "counterHydration";
export const hydrationOptionStoreId = "counterHydrationOptions";
export let useHydrationStore = defineStore("counterHydration", setupHydrationStore);
export let useHydrationOptionStore = defineStore("counterHydrationOptions", { state: createHydrationState, hydrate: hydrateOptionStore });
export let component = defineComponent({ name: "PiniaCounterHydrationCookbook", setup: setup });
export function seedInitialOptionStoreState(pinia) {
  pinia.state.value["counterHydrationOptions"] = { count: 12, status: "serialized SSR payload" };
}
function setupHydrationStore(helpers) {
  let clientOnlyNote = skipHydrate(ref("client-only note seeded in setup store"));
  return {
    count: 4,
    status: "setup-store hydration boundary is ready",
    clientOnlyNote: clientOnlyNote,
    canHydrateClientOnlyNote: helpers.action(() => {
      return shouldHydrate(clientOnlyNote);
    }, "canHydrateClientOnlyNote"),
    refreshClientOnlyNote: helpers.action(() => {
      clientOnlyNote.value = "client note refreshed at " + useHydrationOptionStore().status;
      return;
    }, "refreshClientOnlyNote")
  };
}
function createHydrationState() {
  return { count: 8, status: "option-store hydration hook waiting" };
}
function hydrateOptionStore(storeState, initialState) {
  storeState.count = initialState.count;
  storeState.status = initialState.status + " -> hydrate(storeState, initialState)";
}
function setup() {
  let setupStore = useHydrationStore();
  let optionStore = useHydrationOptionStore();
  let clientHydrates = computed(setupStore.canHydrateClientOnlyNote);
  let reapplyClientNote = setupStore.refreshClientOnlyNote;
  let hydrateSnapshot = () => {
    optionStore.$patch({ status: "hydration snapshot captured from client action" });
    return;
  };
  return () => {
    return h("section", { class: "counter-hydration-shell" }, [h("h2", "Hydration cookbook"), h("p", "skipHydrate()/shouldHydrate() remain explicit setup-store authoring tools, while option-store hydrate(storeState, initialState) stays available for SSR/client boundary repair without hiding runtime semantics."), h("ul", { class: "counter-notes" }, [h("li", "setup store id: " + "counterHydration"), h("li", "option store id: " + optionStore.$id), h("li", "client-only note: " + setupStore.clientOnlyNote), h("li", "should hydrate client-only note: " + clientHydrates.value), h("li", "option-store status: " + optionStore.status)]), h("div", { class: "counter-actions" }, [createActionButton("Refresh client note", "action-button action-button--accent", reapplyClientNote), createActionButton("Hydration snapshot", "action-button", hydrateSnapshot)])]);
  };
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-hydration.mjs.map
