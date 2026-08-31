export function EnsureDisposableInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
/*jazor:clr-member System.IAsyncDisposable.DisposeAsync()*/
export function _d17f7fbf9eb14eef(instance) {
  EnsureDisposableInstance(instance);
  if (Reflect.has(instance, "disposeAsync")) {
    let disposeAsync = Reflect.get(instance, "disposeAsync");
    let result = Reflect.apply(disposeAsync, instance, []);
    return Promise.resolve(result);
  }
  return Promise.resolve();
}
