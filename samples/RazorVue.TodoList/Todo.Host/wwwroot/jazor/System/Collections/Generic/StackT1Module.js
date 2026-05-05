import { JStack } from "System/RuntimeModule.js";
export function _7d15fcc03d17599b() {
  return new JStack;
}
export function _f4ca5eb8de25d4a3(capacity) {
  return JStack.withCapacity(capacity);
}
export function _60d564060ac5fb0f(collection) {
  return new JStack(collection);
}
export const StackT1Module = {
  _7d15fcc03d17599b,
  _f4ca5eb8de25d4a3,
  _60d564060ac5fb0f
};
