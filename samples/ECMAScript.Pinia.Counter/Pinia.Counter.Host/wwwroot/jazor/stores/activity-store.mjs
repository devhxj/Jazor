import { defineStore } from "pinia";
const activityStoreId = "activity";
export let useActivityStore = defineStore("activity", {
  state: createState,
  getters: { summary: (__cb => function() {
    return __cb(this, ...arguments);
  })(readSummary) },
  actions: { capture: (__cb => function() {
    return __cb(this, ...arguments);
  })(capture), queueReview: (__cb => function() {
    return __cb(this, ...arguments);
  })(queueReview) }
});
function createState() {
  return {
    completedActions: 1,
    pendingReviews: 2,
    highlight: "Waiting for the next workflow capture."
  };
}
function readSummary(self) {
  return self.highlight + " (done: " + self.completedActions + ", pending: " + self.pendingReviews + ")";
}
function capture(self, source) {
  self.completedActions += 1;
  if (self.pendingReviews > 0) {
    self.pendingReviews -= 1;
  }
  self.highlight = "capture(" + source + ") updated the activity store.";
}
function queueReview(self) {
  self.pendingReviews += 1;
  self.highlight = "queueReview() recorded another follow-up item.";
}
//# sourceMappingURL=activity-store.mjs.map
