import { EqualsCore } from "System/Collections/Generic/EqualityComparerT1Module.js";
import { JStack } from "System/RuntimeModule.js";
function EnsureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
function CountCore(instance) {
  return instance.items.length;
}
function PeekCore(instance) {
  if (CountCore(instance) === 0)
    throw new Error("InvalidOperationException: Stack is empty.");
  return instance.items[instance.items.length - 1];
}
function PopCore(instance) {
  let value = PeekCore(instance);
  instance.items.splice(instance.items.length - 1, 1);
  return value;
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Count.get*/
export function _ec97cc120d8d804b(instance) {
  EnsureInstance(instance);
  return CountCore(instance);
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Clear()*/
export function _431a6c983678bc4d(instance) {
  EnsureInstance(instance);
  instance.items.splice(0, instance.items.length);
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Contains(T)*/
export function _f8679c85a69f0514(instance, item) {
  EnsureInstance(instance);
  for (let index = 0; index < instance.items.length; index++) {
    if (EqualsCore(instance.items[index], item))
      return true;
  }
  return false;
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Peek()*/
export function _c406861f59a5ccaf(instance) {
  EnsureInstance(instance);
  return PeekCore(instance);
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.TryPeek(out T)*/
export function _fa141b6d3bc0d25a(instance) {
  EnsureInstance(instance);
  return CountCore(instance) === 0 ? [false, null] : [true, PeekCore(instance)];
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Pop()*/
export function _26474a0aeb01f889(instance) {
  EnsureInstance(instance);
  return PopCore(instance);
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.TryPop(out T)*/
export function _247c56433f8b7216(instance) {
  EnsureInstance(instance);
  return CountCore(instance) === 0 ? [false, null] : [true, PopCore(instance)];
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Push(T)*/
export function _c18157d266fca530(instance, item) {
  EnsureInstance(instance);
  instance.items.push(item);
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.ToArray()*/
export function _e40d0cf595a7fe44(instance) {
  EnsureInstance(instance);
  let result = new Array;
  for (let index = instance.items.length; index > 0; index--)
    result.push(instance.items[index - 1]);
  return result;
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Stack()*/
export function _7d15fcc03d17599b() {
  return new JStack("$ctor_0be02918366cee67");
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Stack(int)*/
export function _f4ca5eb8de25d4a3(capacity) {
  return JStack.WithCapacity(capacity);
}
/*jazor:clr-member System.Collections.Generic.Stack<T>.Stack(System.Collections.Generic.IEnumerable<T>)*/
export function _60d564060ac5fb0f(collection) {
  return new JStack("$ctor_a657e829623938c5", collection);
}
