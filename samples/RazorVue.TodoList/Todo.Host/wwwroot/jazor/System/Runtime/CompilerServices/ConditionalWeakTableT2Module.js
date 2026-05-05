export function _8360443cbe5b1f88(instance, key, value) {
  let typedKey = key;
  if (!instance.has(typedKey))
    return [false, null];
  return [true, instance.get(typedKey)];
}
export function _c013f77a250570ce(instance, key, value) {
  let typedKey = key;
  let typedValue = value;
  if (instance.has(typedKey))
    throw new Error("ArgumentException: An item with the same key has already been added.");
  instance.set(typedKey, typedValue);
}
export function _6a785a77d1b78937(instance, key, value) {
  let typedKey = key;
  if (instance.has(typedKey))
    return false;
  instance.set(typedKey, value);
  return true;
}
export function _3e5ae776a9edba7b(instance, key, value) {
  instance.set(key, value);
}
export function _0b5841f143b2e9e7(instance, key) {
  return instance.delete(key);
}
export function _14e40010b1fd2993(instance, key, value) {
  let typedKey = key;
  if (!instance.has(typedKey))
    return [false, null];
  let currentValue = instance.get(typedKey);
  instance.delete(typedKey);
  return [true, currentValue];
}
export function _8e3321f2e6fa2499(instance, key, value) {
  let typedKey = key;
  if (instance.has(typedKey))
    return instance.get(typedKey);
  let typedValue = value;
  instance.set(typedKey, typedValue);
  return typedValue;
}
export const ConditionalWeakTableT2Module = {
  _8360443cbe5b1f88,
  _c013f77a250570ce,
  _6a785a77d1b78937,
  _3e5ae776a9edba7b,
  _0b5841f143b2e9e7,
  _14e40010b1fd2993,
  _8e3321f2e6fa2499
};
