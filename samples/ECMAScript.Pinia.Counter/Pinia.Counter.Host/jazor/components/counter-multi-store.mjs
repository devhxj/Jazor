import { mapStores, setMapStoreSuffix } from "pinia";
import { UseActivityStore } from "stores/activity-store.mjs";
import { UseCounterStore } from "stores/counter-store.mjs";
import { computed, defineComponent, h, ref } from "vue";
let OptionsApiSnapshot = ref("mapStores() snapshot will be captured after mount.");
let OptionsApiStoreIds = ref("store ids pending");
export let Component = defineComponent({
  name: "PiniaCounterMultiStore",
  computed: CreateComputed(),
  mounted: (__cb => function() {
    return __cb(this, ...arguments);
  })(CaptureMappedStores),
  setup: Setup
});
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
  return h("button", {
    type: "button",
    class: className,
    onClick: handler
  }, label);
}
//# sourceMappingURL=counter-multi-store.mjs.map
