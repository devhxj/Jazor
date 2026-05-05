import { JQueue } from "System/RuntimeModule.js";
export function _ea05a56d08fbd4f9() {
  return new JQueue;
}
export function _7fc2b76467c43db9(capacity) {
  return JQueue.withCapacity(capacity);
}
export function _5eae085d83bbe242(collection) {
  return new JQueue(collection);
}
export const QueueT1Module = {
  _ea05a56d08fbd4f9,
  _7fc2b76467c43db9,
  _5eae085d83bbe242
};
