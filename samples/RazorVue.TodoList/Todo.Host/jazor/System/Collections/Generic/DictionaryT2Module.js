function ensureInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
export function _e73dbdff85c46ddc(instance, key) {
  ensureInstance(instance);
  if (!instance.has(key))
    throw new Error("KeyNotFoundException: The given key was not present in the dictionary.");
  return instance.get(key);
}
export function _39d6e632c4c102f9(instance, key, value) {
  ensureInstance(instance);
  if (instance.has(key))
    throw new Error("ArgumentException: An item with the same key has already been added.");
  instance.set(key, value);
}
export function _d6ac89338dff5e3b(instance, key) {
  ensureInstance(instance);
  if (instance.has(key)) {
    let value = instance.get(key);
    instance.delete(key);
    return [true, value];
  }
  return [false, null];
}
export function _7db4d9112b4ba3c4(instance, key) {
  ensureInstance(instance);
  if (instance.has(key))
    return [true, instance.get(key)];
  return [false, null];
}
export function _61b63b2c7b14f06a(instance, key, value) {
  ensureInstance(instance);
  if (instance.has(key))
    return false;
  instance.set(key, value);
  return true;
}
