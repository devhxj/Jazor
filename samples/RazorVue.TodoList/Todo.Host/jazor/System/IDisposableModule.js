export function ensureDisposableInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
export function _6f97d94b6f2e4bc1(instance) {
  ensureDisposableInstance(instance);
  if (Reflect.has(instance, "dispose")) {
    let dispose = Reflect.get(instance, "dispose");
    Reflect.apply(dispose, instance, []);
  }
}
