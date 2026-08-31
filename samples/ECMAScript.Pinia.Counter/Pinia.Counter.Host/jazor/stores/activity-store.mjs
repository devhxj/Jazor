import { defineStore } from "pinia";
const ActivityStoreId = "activity";
export let UseActivityStore = defineStore("activity", {
  state: CreateState,
  getters: { Summary: (__cb => function() {
    return __cb(this, ...arguments);
  })(ReadSummary) },
  actions: { Capture: (__cb => function() {
    return __cb(this, ...arguments);
  })(Capture), QueueReview: (__cb => function() {
    return __cb(this, ...arguments);
  })(QueueReview) }
});
function CreateState() {
  return {
    CompletedActions: 1,
    PendingReviews: 2,
    Highlight: "Waiting for the next workflow capture."
  };
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
//# sourceMappingURL=activity-store.mjs.map
