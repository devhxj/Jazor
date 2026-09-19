export function EnsureDisposableInstance(instance) {
  if (instance == null)
    throw new Error("NullReferenceException: instance is null.");
}
/*jazor:clr-member System.IDisposable.Dispose()*/
export function _6f97d94b6f2e4bc1(instance) {
  EnsureDisposableInstance(instance);
  if (Reflect.has(instance, "dispose")) {
    let dispose = Reflect.get(instance, "dispose");
    Reflect.apply(dispose, instance, []);
  }
}
