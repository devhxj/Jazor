import { UseCounterStore } from "stores/counter-store.mjs";
import { defineComponent, h, onMounted, onUnmounted, ref } from "vue";
export let Component = defineComponent({ name: "PiniaCounterSubscriptionCookbook", setup: Setup });
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
    store.$patch(state => {
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
    return h("section", { class: "counter-subscription-shell" }, [h("h2", "Subscription cookbook"), h("p", "$subscribe() is registered with detached + sync options so the sample can inspect direct mutations, object patches, and function patches from one stable callback. Direct assignments may report multiple sync notifications when several fields change back-to-back."), h("div", { class: "counter-actions" }, [CreateActionButton("Direct +1", "action-button action-button--accent", applyDirectMutation), CreateActionButton("Object patch", "action-button", applyObjectPatch), CreateActionButton("Function patch", "action-button", applyFunctionPatch)]), h("ul", { class: "counter-notes" }, [
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
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-subscription.mjs.map
