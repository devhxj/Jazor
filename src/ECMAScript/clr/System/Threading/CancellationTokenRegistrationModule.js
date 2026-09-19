import { UnregisterCancellationCallback } from "System/RuntimeModule.js";
/*jazor:clr-member System.Threading.CancellationTokenRegistration.Dispose()*/
export function dispose(instance) {
  UnregisterCancellationCallback(instance);
}
/*jazor:clr-member System.Threading.CancellationTokenRegistration.DisposeAsync()*/
export function disposeAsync(instance) {
  UnregisterCancellationCallback(instance);
  return Promise.resolve();
}
/*jazor:clr-member System.Threading.CancellationTokenRegistration.Unregister()*/
export function unregister(instance) {
  return UnregisterCancellationCallback(instance);
}
