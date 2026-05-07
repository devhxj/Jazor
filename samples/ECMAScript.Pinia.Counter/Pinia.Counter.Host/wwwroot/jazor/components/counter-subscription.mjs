import { defineComponent, h, onMounted, onUnmounted, ref } from "npm:vue@3";
import { useCounterStore } from "stores/counter-store.mjs";
export let component = defineComponent({ name: "PiniaCounterSubscriptionCookbook", setup: setup });
function setup() {
  let store = useCounterStore();
  let mutationKind = ref("No mutations observed yet.");
  let storeId = ref(store.$id);
  let statusSnapshot = ref(store.status);
  let countSnapshot = ref(store.count);
  let payloadSnapshot = ref("payload appears only for $patch({ ... }) mutations.");
  let eventShape = ref("Debugger events are dev-only and may be unavailable.");
  let notificationCount = ref(0);
  let detach = null;
  let handleMutation = (mutation, state) => {
    notificationCount.value += 1;
    mutationKind.value = describeMutationType(mutation.type);
    storeId.value = mutation.storeId;
    countSnapshot.value = state.count;
    statusSnapshot.value = state.status;
    payloadSnapshot.value = readMutationSummary(mutation);
    eventShape.value = describeEvents(mutation.events);
    return;
  };
  let applyDirectMutation = () => {
    store.count += 1;
    store.status = "Direct assignment updated the counter store.";
    return;
  };
  let applyObjectPatch = () => {
    store.$patch({ count: store.count + 3, status: "Object patch updated the counter store." });
    return;
  };
  let applyFunctionPatch = () => {
    store.$patch(state => {
      state.count += 2;
      state.status = "Function patch updated the counter store.";
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
    return h("section", { class: "counter-subscription-shell" }, [h("h2", "Subscription cookbook"), h("p", "$subscribe() is registered with detached + sync options so the sample can inspect direct mutations, object patches, and function patches from one stable callback. Direct assignments may report multiple sync notifications when several fields change back-to-back."), h("div", { class: "counter-actions" }, [createActionButton("Direct +1", "action-button action-button--accent", applyDirectMutation), createActionButton("Object patch", "action-button", applyObjectPatch), createActionButton("Function patch", "action-button", applyFunctionPatch)]), h("ul", { class: "counter-notes" }, [
      h("li", "mutation kind: " + mutationKind.value),
      h("li", "store id: " + storeId.value),
      h("li", "count snapshot: " + countSnapshot.value),
      h("li", "status snapshot: " + statusSnapshot.value),
      h("li", "payload summary: " + payloadSnapshot.value),
      h("li", "events shape: " + eventShape.value),
      h("li", "notifications seen: " + notificationCount.value)
    ])]);
  };
}
function describeMutationType(type) {
  return (() => {
    const __swexpr$59f6aa2a1ae075e733124215 = type;
    if (__swexpr$59f6aa2a1ae075e733124215 === "direct")
      return "direct assignment";
    if (__swexpr$59f6aa2a1ae075e733124215 === "patch object")
      return "$patch({ ... }) object merge";
    if (__swexpr$59f6aa2a1ae075e733124215 === "patch function")
      return "$patch((state) => ...) callback";
    return "unknown mutation";
  })();
}
function readMutationSummary(mutation) {
  if (mutation.type === "patch object") {
    let patchMutation = mutation;
    let payload = patchMutation.payload;
    if (payload.status !== null) {
      return "payload.status = " + payload.status;
    }
    if (payload.count !== null) {
      return "payload.count = " + payload.count;
    }
    return "object patch payload captured without known fields";
  }
  if (mutation.type === "patch function") {
    return "function patch metadata captured without a payload object";
  }
  return "direct assignments do not expose a payload object";
}
function describeEvents(events) {
  return events === null ? "not provided" : "reported";
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-subscription.mjs.map
