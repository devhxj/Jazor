import { computed, defineComponent, h, ref } from "npm:vue@3";
import { mapStores, setMapStoreSuffix } from "pinia";
import { useActivityStore } from "stores/activity-store.mjs";
import { useCounterStore } from "stores/counter-store.mjs";
let optionsApiSnapshot = ref("mapStores() snapshot will be captured after mount.");
let optionsApiStoreIds = ref("store ids pending");
export let component = defineComponent({
  name: "PiniaCounterMultiStore",
  computed: createComputed(),
  mounted: (__cb => function() {
    return __cb(this, ...arguments);
  })(captureMappedStores),
  setup: setup
});
function createComputed() {
  setMapStoreSuffix("");
  return mapStores(useCounterStore, useActivityStore);
}
function setup() {
  let counter = useCounterStore();
  let activity = useActivityStore();
  let incrementAndCapture = () => {
    counter.increment();
    activity.capture("increment");
    return;
  };
  let queueReview = activity.queueReview.bind(activity);
  let liveSummary = computed(() => {
    return counter.status + " | " + activity.summary;
  });
  let combinedScore = computed(() => {
    return counter.count + activity.completedActions + activity.pendingReviews;
  });
  return () => {
    return h("section", { class: "counter-multi-store-shell" }, [h("h2", "Multi-store cookbook"), h("p", "SetMapStoreSuffix(\"\") keeps the component-instance fields aligned with store ids while mapStores() projects both stores through one Options API entry point."), h("div", { class: "counter-summary-grid" }, [createMetricCard("counter.count", counter.count, "metric-card metric-card--primary"), createMetricCard("activity.done", activity.completedActions, "metric-card metric-card--secondary"), createMetricCard("activity.pending", activity.pendingReviews, "metric-card metric-card--neutral"), createMetricCard("combined", combinedScore.value, "metric-card metric-card--accent")]), h("p", { class: "counter-status" }, liveSummary.value), h("div", { class: "counter-actions" }, [createActionButton("Increment + capture", "action-button action-button--accent", incrementAndCapture), createActionButton("Queue review", "action-button", queueReview)]), h("ul", { class: "counter-notes" }, [h("li", "mounted snapshot via mapStores(): " + optionsApiSnapshot.value), h("li", "mapped component store ids: " + optionsApiStoreIds.value), h("li", "direct setup render keeps the live surface readable while Options API captures the heterogeneous store projection.")])]);
  };
}
function captureMappedStores(self) {
  optionsApiSnapshot.value = self.counter.status + " | " + self.activity.summary;
  optionsApiStoreIds.value = self.counter.$id + " + " + self.activity.$id;
}
function createMetricCard(label, value, className) {
  return h("article", { class: className }, [h("span", { class: "metric-label" }, label), h("strong", { class: "metric-value" }, value)]);
}
function createActionButton(label, className, handler) {
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-multi-store.mjs.map
