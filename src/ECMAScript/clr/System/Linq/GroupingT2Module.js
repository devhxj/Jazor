let Keys = new WeakMap;
export function Create(key) {
  let grouping = new Array;
  Keys.set(grouping, key);
  return grouping;
}
export function GetKey(grouping) {
  return Keys.get(grouping);
}
/*jazor:clr-member System.Linq.IGrouping<TKey, TElement>.Key.get*/
export function _44a1c9f2c4f246e9(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
  return GetKey(instance);
}
