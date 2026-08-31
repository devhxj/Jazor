import { defineStore, shouldHydrate, skipHydrate } from "pinia";
import { computed, defineComponent, h, ref } from "vue";
export const HydrationStoreId = "counterHydration";
export const HydrationOptionStoreId = "counterHydrationOptions";
export let UseHydrationStore = defineStore("counterHydration", SetupHydrationStore);
export let UseHydrationOptionStore = defineStore("counterHydrationOptions", { state: CreateHydrationState, hydrate: HydrateOptionStore });
export let Component = defineComponent({ name: "PiniaCounterHydrationCookbook", setup: Setup });
export function SeedInitialOptionStoreState(pinia) {
  pinia.state.value["counterHydrationOptions"] = { Count: 12, Status: "serialized SSR payload" };
}
function SetupHydrationStore(helpers) {
  let clientOnlyNote = skipHydrate(ref("client-only note seeded in setup store"));
  return {
    Count: 4,
    Status: "setup-store hydration boundary is ready",
    ClientOnlyNote: clientOnlyNote,
    CanHydrateClientOnlyNote: helpers.action(() => {
      return shouldHydrate(clientOnlyNote);
    }, "canHydrateClientOnlyNote"),
    RefreshClientOnlyNote: helpers.action(() => {
      clientOnlyNote.value = "client note refreshed at " + UseHydrationOptionStore().Status;
      return;
    }, "refreshClientOnlyNote")
  };
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
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-hydration.mjs.map
