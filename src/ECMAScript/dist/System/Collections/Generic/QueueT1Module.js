import { EqualsCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { JQueue } from "System/RuntimeModule.js";
function EnsureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
function CountCore(instance) {
  return instance.items.length - instance.head;
}
function DequeueCore(instance) {
  if (CountCore(instance) === 0)
    throw new Error("InvalidOperationException: Queue is empty.");
  let value = instance.items[instance.head];
  instance.head++;
  return value;
}
function PeekCore(instance) {
  if (CountCore(instance) === 0)
    throw new Error("InvalidOperationException: Queue is empty.");
  return instance.items[instance.head];
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Count.get*/
export function _874ffef6d586566e(instance) {
  EnsureInstance(instance);
  return CountCore(instance);
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Clear()*/
export function _c1380aa32ab3b19e(instance) {
  EnsureInstance(instance);
  instance.items.splice(0, instance.items.length);
  instance.head = 0;
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Enqueue(T)*/
export function _8a87022169c02c22(instance, item) {
  EnsureInstance(instance);
  instance.items.push(item);
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Dequeue()*/
export function _9828432fec9d535a(instance) {
  EnsureInstance(instance);
  return DequeueCore(instance);
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.TryDequeue(out T)*/
export function _96c6e0d13a99b6ff(instance) {
  EnsureInstance(instance);
  return CountCore(instance) === 0 ? [false, null] : [true, DequeueCore(instance)];
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Peek()*/
export function _e17f3e583930e78f(instance) {
  EnsureInstance(instance);
  return PeekCore(instance);
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.TryPeek(out T)*/
export function _35559a67cebb0fd9(instance) {
  EnsureInstance(instance);
  return CountCore(instance) === 0 ? [false, null] : [true, PeekCore(instance)];
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Contains(T)*/
export function _45549ae297d2d16d(instance, item) {
  EnsureInstance(instance);
  for (let index = instance.head; index < instance.items.length; index++) {
    if (EqualsCore(instance.items[index], item))
      return true;
  }
  return false;
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.ToArray()*/
export function _8cda2376e71ddbd2(instance) {
  EnsureInstance(instance);
  let result = new Array;
  for (let index = instance.head; index < instance.items.length; index++)
    result.push(instance.items[index]);
  return result;
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Queue()*/
export function _ea05a56d08fbd4f9() {
  return new JQueue("$ctor_83a6b5a077092c33");
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Queue(int)*/
export function _7fc2b76467c43db9(capacity) {
  return JQueue.WithCapacity(capacity);
}
/*jazor:clr-member System.Collections.Generic.Queue<T>.Queue(System.Collections.Generic.IEnumerable<T>)*/
export function _5eae085d83bbe242(collection) {
  return new JQueue("$ctor_a172437de92c387f", collection);
}
